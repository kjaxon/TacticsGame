using RedBjorn.Utils;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    [CreateAssetMenu(menuName = "Red Bjorn/Settings/Map Window")]
    public class MapWindowSettings : ScriptableObjectExtended
    {
        public MapRules Rules;

        public float HeaderHeight = 50;
        public float CommonHeight = 140;
        public float Border = 4;
        public float ToolLabelWidth = 40f;
        public float TileLabelWidth = 80f;

        public Color Separator = new Color(0.7607f, 0.7607f, 0.7607f, 1f);
        public Color LabelColor = Color.black;
        public Color EdgeColor = Color.red;
        public Color EdgeCursorColor = Color.yellow;
        public Color EdgeCursorPaint = Color.green;
        public Color EdgeCursorErase = Color.red;
        public Color TileCursorErase = new Color(1f, 1f, 1f, 0.7f);

        public Texture2D HeaderBackground;
        public Texture2D CommonBackground;
        public Texture2D WorkAreaBackground;
        public Texture2D BrushIcon;
        public Texture2D EraseIcon;
        public GUISkin Skin;

        [HideInInspector] public bool DrawHeader;
        [HideInInspector] public bool DrawSideTool;
        public bool AreasAutoMark;
        public Material CellBorder;

        public const string DefaultPathFull = "Assets/RedBjorn/" + DefaultPathRelative;
        public const string DefaultPathRelative = "ProtoTiles/Map Editor/Editor Resources/MapWindowSettings.asset";
        public static Vector2 WindowMinSize = new Vector2(270f, 480f);

        public static MapWindowSettings Instance
        {
            get
            {
                var instance = AssetDatabase.LoadAssetAtPath<MapWindowSettings>(DefaultPathFull);
                if (!instance)
                {
                    var instances = Resources.FindObjectsOfTypeAll<MapWindowSettings>()
                                             .OrderBy(r => AssetDatabase.GetAssetPath(r));
                    instance = instances.FirstOrDefault(i => AssetDatabase.GetAssetPath(i).Contains(DefaultPathRelative));
                    if (!instance)
                    {
                        instance = instances.FirstOrDefault();
                    }
                }
                return instance;
            }
        }
    }
}
