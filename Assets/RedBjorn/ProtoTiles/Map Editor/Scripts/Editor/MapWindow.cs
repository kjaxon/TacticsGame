using RedBjorn.Utils;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    public class MapWindow : EditorWindow
    {
        [SerializeField] int GridType;
        [SerializeField] bool ShowMap;
        [SerializeField] int TilePresetCurrent;
        [SerializeField] int ToolCurrent;
        Rect Header;
        Rect Common;
        Rect WorkArea;
        Vector2 ScrollPos;
        bool[] ToolToogle = new bool[4];
        bool[] ToolTooglePrevious = new bool[4];
        string[] TileIds;
        MapWindowSettings Settings;

        string ButtonPlacePrefabs = "Place Prefabs";

        static readonly string[] TileToolNames = new string[]
        {
            "Brush",
            "Eraser"
        };

        static readonly string[] EdgeToolNames = new string[]
        {
            "Brush",
            "Eraser"
        };

        static readonly string[] Grids = System.Enum.GetNames(typeof(GridType));

        [SerializeField] MapSceneDrawer CachedSceneDrawer;
        MapSceneDrawer SceneDrawer
        {
            get
            {
                if (CachedSceneDrawer == null)
                {
                    CachedSceneDrawer = new MapSceneDrawer() { WindowSettings = Settings };
                }
                return CachedSceneDrawer;
            }
        }

        bool Editable { get { return Map != null; } }
        bool IsSceneEditing { get { return Editable && ShowMap && ToolToogle.Any(t => t); } }
        bool IsSceneEditingPrevious { get { return Editable && ShowMap && ToolTooglePrevious.Any(t => t); } }

        float HeaderHeight => Settings.HeaderHeight;
        float CommonHeight => Settings.CommonHeight;
        float Border => Settings.Border;
        float ToolLabelWidth => Settings.ToolLabelWidth;
        float TileLabelWidth => Settings.TileLabelWidth;
        Color Separator => Settings.Separator;
        bool ShouldDrawHeader => Settings.DrawHeader;
        Texture2D HeaderBackground => Settings.HeaderBackground;
        Texture2D CommonBackground => Settings.CommonBackground;
        Texture2D WorkAreaBackground => Settings.WorkAreaBackground;
        GUISkin Skin => Settings.Skin;

        [SerializeField] MapSettings CachedMap;
        MapSettings Map
        {
            get
            {
                return CachedMap;
            }
            set
            {
                if (CachedMap != value)
                {
                    CachedMap = value;
                    OnChangedMap();
                }
            }
        }

        [MenuItem("Tools/Red Bjorn/Editors/Map")]
        static void Init()
        {
            var window = (MapWindow)EditorWindow.GetWindow(typeof(MapWindow));
            window.minSize = MapWindowSettings.WindowMinSize;
            window.titleContent = new GUIContent("Map Editor");
            window.ShowMap = true;
            window.Show();
        }

        void OnEnable()
        {
            InitResources();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        void OnGUI()
        {
            Undo.RecordObject(this, "Map");
            DrawAreaHeader();
            DrawAreaCommon();
            GUI.enabled = Editable;
            DrawAreaWork();
        }

        void OnFocus()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
            if (SceneDrawer != null)
            {
                SceneDrawer.OnBeforeChanged += UndoRecord;
            }
        }

        void OnLostFocus()
        {
            if (SceneDrawer != null)
            {
                SceneDrawer.OnBeforeChanged -= UndoRecord;
            }
        }

        void OnChangedMap()
        {
            if (Map)
            {
                GridType = (int)Map.Type;
            }
            SceneDrawer.Map = Map;
            SceneDrawer.Redraw();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            SceneDrawer.Draw(IsSceneEditing, ShowMap);
            SceneView.RepaintAll();
        }

        void InitResources()
        {
            Settings = MapWindowSettings.Instance;
        }

        void OnUndoRedoPerformed()
        {
            Repaint();
        }

        void UndoRecord()
        {
            Undo.RegisterCompleteObjectUndo(this, "Map");
        }

        void DrawAreaHeader()
        {
            if (ShouldDrawHeader)
            {
                Header.x = 0;
                Header.y = 0;
                Header.width = Screen.width;
                Header.height = HeaderHeight;
                GUI.DrawTexture(Header, HeaderBackground, ScaleMode.StretchToFill);

                GUILayout.BeginArea(Header);
                GUILayout.Label("MAP EDITOR");
                GUILayout.EndArea();
            }
        }

        void DrawAreaCommon()
        {
            Common.x = 2 * Border;
            Common.y = 2 * Border;
            if (ShouldDrawHeader)
            {
                Common.y += HeaderHeight;
            }
            Common.width = Screen.width - 4 * Border;
            Common.height = CommonHeight - 2 * Border;
            GUI.DrawTexture(Common, CommonBackground, ScaleMode.StretchToFill);

            GUILayout.BeginArea(Common);
            Map = EditorGUILayout.ObjectField("Map Asset", Map, typeof(MapSettings), allowSceneObjects: false) as MapSettings;
            GuiStyles.DrawHorizontal(Separator);

            EditorGUILayout.LabelField("Map Asset Creator", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Grid", GUILayout.Width(30));
            GridType = GUILayout.SelectionGrid(GridType, Grids, 2, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Create", GUILayout.MaxWidth(MapWindowSettings.WindowMinSize.x - 6 * Border)))
            {
                MapCreateAsset();
            }
            GuiStyles.DrawHorizontal(Separator);
            GUILayout.EndArea();

        }

        void DrawAreaWork()
        {
            WorkArea.x = 2 * Border;
            WorkArea.y = CommonHeight;
            if (ShouldDrawHeader)
            {
                WorkArea.y += HeaderHeight;
            }
            WorkArea.width = Screen.width - 4 * Border;
            WorkArea.height = Screen.height - CommonHeight - 10 * Border;
            if (ShouldDrawHeader)
            {
                WorkArea.height -= HeaderHeight;
            }
            GUI.DrawTexture(WorkArea, WorkAreaBackground, ScaleMode.StretchToFill);

            GUILayout.BeginArea(WorkArea);
            DrawTools();
            DrawPresets();
            GUILayout.EndArea();
        }

        void DrawTools()
        {
            var presetsDetermined = "Not all prefabs at Presets\nare determined.\nContinue?";
            var mapViewChilds = "MapView gameObject has childs.\nThey will be replaced.\nContinue?";
            EditorGUILayout.LabelField("Map", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            ShowMap = EditorGUILayout.Toggle("Show", ShowMap);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Clear", GUILayout.MaxWidth(MapWindowSettings.WindowMinSize.x - 6 * Border)))
            {
                Action yes = () =>
                {
                    Map.Clear();
                    SceneDrawer.Clear();
                };
                ConfirmEditorWindow.Init("Map data and scene\ndata will be cleared.\nContinue?", yesAction: yes);
            }
            if (GUILayout.Button(ButtonPlacePrefabs, GUILayout.MaxWidth(MapWindowSettings.WindowMinSize.x - 6 * Border)))
            {
                var incorrectPrefabs = Map.Presets.Any(p => !p.Prefab);
                Action placeAction = () =>
                {
                    Map.PlacePrefabs();
                };
                if (Map.IsEmpty)
                {
                    if (incorrectPrefabs)
                    {
                        ConfirmEditorWindow.Init(presetsDetermined, yesAction: placeAction);
                    }
                    else
                    {
                        Map.PlacePrefabs();
                    }
                }
                else
                {
                    if (incorrectPrefabs)
                    {
                        Action yesAction = () =>
                        {
                            ConfirmEditorWindow.Init(mapViewChilds, yesAction: placeAction);
                        };
                        ConfirmEditorWindow.Init(presetsDetermined, yesAction: yesAction);
                    }
                    else
                    {
                        ConfirmEditorWindow.Init(mapViewChilds, yesAction: placeAction);
                    }
                }
            }
            GuiStyles.DrawHorizontal(Separator);

            var guiEnabled = GUI.enabled;
            GUI.enabled = Editable && ShowMap;
            if (!IsSceneEditingPrevious && IsSceneEditing)
            {
                SceneEditingStart();
            }
            else if (IsSceneEditingPrevious && !IsSceneEditing)
            {
                SceneEditingFinish();
            }

            ToolToogle.CopyTo(ToolTooglePrevious, 0);
            DrawToolTiles();
            if (Settings.DrawSideTool)
            {
                DrawToolEdges();
            }
            GUI.enabled = guiEnabled;

            for (int i = 0; i < ToolToogle.Length; i++)
            {
                if (ToolToogle[i] && ToolToogle[i] != ToolTooglePrevious[i])
                {
                    if (!ToolValidate())
                    {
                        for (int j = 0; j < ToolToogle.Length; j++)
                        {
                            ToolToogle[j] = false;
                        }
                        ConfirmEditorWindow.Init("Presets list is empty.\nPlease, add at least one preset", noText: "OK");
                        break;
                    }
                    ToolCurrent = i;
                    for (int j = 0; j < ToolToogle.Length; j++)
                    {
                        if (j != i)
                        {
                            ToolToogle[j] = false;
                        }
                    }
                    break;
                }
            }
            SceneDrawer.ToolType = ToolCurrent == 0 || ToolCurrent == 2 ? 0 : 1;
            SceneDrawer.BrushType = ToolCurrent < 2 ? 0 : 1;
        }

        void DrawToolTiles()
        {
            var toolStyleNormal = Skin.customStyles[7];
            var toolStyleSelected = Skin.customStyles[8];
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Tile", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (ButtonTwoStyle(Settings.BrushIcon, TileToolNames[0], toolStyleNormal, toolStyleSelected, ToolToogle[0]))
            {
                ToolToogle[0] = !ToolToogle[0];
            }

            if (ButtonTwoStyle(Settings.EraseIcon, TileToolNames[1], toolStyleNormal, toolStyleSelected, ToolToogle[1]))
            {
                ToolToogle[1] = !ToolToogle[1];
            }
            EditorGUILayout.EndHorizontal();
            GuiStyles.DrawHorizontal(Separator);
            EditorGUILayout.EndVertical();
        }

        void DrawToolEdges()
        {
            var toolStyleNormal = Skin.customStyles[7];
            var toolStyleSelected = Skin.customStyles[8];
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Edge", EditorStyles.boldLabel);
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = ToolLabelWidth;
            EditorGUILayout.BeginHorizontal();
            if (ButtonTwoStyle(Settings.BrushIcon, EdgeToolNames[0], toolStyleNormal, toolStyleSelected, ToolToogle[2]))
            {
                ToolToogle[2] = !ToolToogle[2];
            }
            if (ButtonTwoStyle(Settings.EraseIcon, EdgeToolNames[1], toolStyleNormal, toolStyleSelected, ToolToogle[3]))
            {
                ToolToogle[3] = !ToolToogle[3];
            }
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = labelWidth;
            GuiStyles.DrawHorizontal(Separator);
            EditorGUILayout.EndVertical();
        }

        void DrawPresets()
        {
            if (Map)
            {
                var buttonAddStyle = Skin.customStyles[4];
                var buttonRemoveStyle = Skin.customStyles[5];
                var buttonTypeNormal = Skin.customStyles[7];
                var buttonTypeSelected = Skin.customStyles[8];
                EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
                ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = TileLabelWidth;

                var serializedMap = new SerializedObject(Map);
                var presetsProperty = serializedMap.FindProperty(nameof(Map.Presets));
                for (int i = 0; i < presetsProperty.arraySize; i++)
                {
                    var presetProperty = presetsProperty.GetArrayElementAtIndex(i);
                    var tagsProperty = presetProperty.FindPropertyRelative("Tags");
                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical(GUILayout.Width(30f), GUILayout.MinHeight(100f));

                    if (ButtonTwoStyle(buttonTypeNormal, buttonTypeSelected, TilePresetCurrent == i))
                    {
                        TilePresetCurrent = i;
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();

                    EditorGUILayout.LabelField(string.Format("Type {0}", i));
                    EditorGUILayout.PropertyField(presetProperty.FindPropertyRelative("Type"), new GUIContent("Name"), true);
                    EditorGUILayout.PropertyField(presetProperty.FindPropertyRelative("MapColor"), new GUIContent("Editor Color"), true);
                    EditorGUILayout.PropertyField(presetProperty.FindPropertyRelative("Prefab"), new GUIContent("Prefab"), true);
                    EditorGUILayout.PropertyField(presetProperty.FindPropertyRelative("GridOffset"), new GUIContent("Grid Offset"), true);
                    EditorGUILayout.LabelField("Tags");

                    var previouslabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 22f;
                    for (int j = 0; j < tagsProperty.arraySize; j++)
                    {
                        GUILayout.BeginHorizontal(GUILayout.Height(20f));
                        var tagProperty = tagsProperty.GetArrayElementAtIndex(j);

                        EditorGUILayout.PropertyField(tagProperty, new GUIContent(" " + j + ":"), true);
                        if (GUILayout.Button("-", buttonRemoveStyle, GUILayout.Width(20f)))
                        {
                            if (tagProperty.objectReferenceValue)
                            {
                                tagProperty.DeleteCommand();
                            }
                            tagProperty.DeleteCommand();
                            GUILayout.EndHorizontal();
                            break;
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUIUtility.labelWidth = previouslabelWidth;
                    if (GUILayout.Button("+", buttonAddStyle, GUILayout.Height(20f)))
                    {
                        tagsProperty.arraySize++;
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUILayout.Width(30f), GUILayout.MinHeight(100f));
                    if (GUILayout.Button("-", buttonRemoveStyle))
                    {
                        Map.PresetRemove(i);
                        SceneDrawer.Redraw();
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                        break;
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }

                if (GUILayout.Button("+", buttonAddStyle, GUILayout.Height(20)))
                {
                    Map.PresetAddDefault();
                    if (IsSceneEditing)
                    {
                        SceneDrawer.EditingUpdate();
                    }
                }
                serializedMap.ApplyModifiedProperties();

                TilePresetCurrent = Mathf.Clamp(TilePresetCurrent, 0, Map.Presets.Count - 1);
                EditorGUIUtility.labelWidth = labelWidth;

                TileIds = Map.Presets.Select(t => t.Id).ToArray();
                SceneDrawer.TileIds = TileIds;
                SceneDrawer.TileType = TilePresetCurrent;

                EditorGUILayout.EndScrollView();
            }
        }

        void SceneEditingStart()
        {
            SceneDrawer.Map = Map;
            SceneDrawer.EditingUpdate();
        }

        void SceneEditingFinish()
        {
            SceneDrawer.EditingFinish();
            Map.MarkAreas();
        }

        void MapCreateAsset()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var filename = System.IO.Path.GetFileNameWithoutExtension(scene.path);
            var extension = ".asset";
            var directory = System.IO.Path.GetDirectoryName(scene.path);
            var mapPath = System.IO.Path.Combine(directory, filename + "_Map" + Grids[GridType] + extension);
            mapPath = AssetDatabase.GenerateUniqueAssetPath(mapPath);
            var mapInstance = ScriptableObject.CreateInstance<MapSettings>();
            mapInstance.Init((GridType)GridType, Settings.Rules, Settings.CellBorder);
            AssetDatabase.CreateAsset(mapInstance, mapPath);
            AssetDatabase.SaveAssets();
            Map = mapInstance;
        }

        bool ToolValidate()
        {
            return Map.Presets != null && Map.Presets.Count > 0;
        }

        bool ButtonTwoStyle(Texture2D icon, string tooltip, GUIStyle normal, GUIStyle selected, bool state)
        {
            var toolPressed = false;
            if (state)
            {
                toolPressed = GUILayout.Button(new GUIContent(icon, tooltip), selected, GUILayout.Width(32f), GUILayout.Height(32f));
            }
            else
            {
                toolPressed = GUILayout.Button(new GUIContent(icon, tooltip), normal, GUILayout.Width(32f), GUILayout.Height(32f));
            }
            return toolPressed;
        }

        bool ButtonTwoStyle(GUIStyle normal, GUIStyle selected, bool state)
        {
            var toolPressed = false;
            if (state)
            {
                toolPressed = GUILayout.Button("+", selected, GUILayout.Width(24f));
            }
            else
            {
                toolPressed = GUILayout.Button(" ", normal, GUILayout.Width(24f));
            }
            return toolPressed;
        }
    }
}
