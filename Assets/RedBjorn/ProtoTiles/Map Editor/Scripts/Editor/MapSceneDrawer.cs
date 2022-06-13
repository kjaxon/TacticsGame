using RedBjorn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    [Serializable]
    public class MapSceneDrawer
    {
        [Serializable]
        public struct SideInfo
        {
            public Vector3Int TilesPos;
            public int NeighbourIndex;

            public override int GetHashCode()
            {
                int hash = 13;
                hash = (7 * hash) + NeighbourIndex.GetHashCode();
                hash = (7 * hash) + TilesPos.GetHashCode();
                return hash;
            }
        }

        [Serializable] public class TilePresetDictionary : SerializableDictionary<Vector3Int, TileData> { } // Hack to serialize dictionary
        [Serializable] public class TileColorsDictionary : SerializableDictionary<string, Color> { } // Hack to serialize dictionary
        [Serializable] public class CursorTilesColorsDictionary : SerializableDictionary<string, Color> { } // Hack to serialize dictionary
        public string[] TileIds;
        public MapSettings Map;
        public MapWindowSettings WindowSettings;

        [SerializeField] TilePresetDictionary Tiles = new TilePresetDictionary();
        [SerializeField] TileColorsDictionary TileColors = new TileColorsDictionary();
        [SerializeField] CursorTilesColorsDictionary CursorTileColors = new CursorTilesColorsDictionary();

        HashSet<Vector3Int> TempTiles = new HashSet<Vector3Int>();
        HashSet<SideInfo> TempEdges = new HashSet<SideInfo>();

        public static Plane Ground = new Plane(Vector3.up, Vector3.zero);

        Color TileCursorErase => WindowSettings.TileCursorErase;
        Color EdgeColor => WindowSettings.EdgeColor;
        Color LabelColor => WindowSettings.LabelColor;
        Color EdgeCursorColor => WindowSettings.EdgeCursorColor;
        Color EdgeCursorPaint => WindowSettings.EdgeCursorPaint;
        Color EdgeCursorErase => WindowSettings.EdgeCursorErase;

        int CachedBrushType;
        public int BrushType
        {
            get
            {
                return CachedBrushType;
            }
            set
            {
                if (value != CachedBrushType)
                {
                    CachedBrushType = value;
                    OnBrushSwitched();
                }
                else
                {
                    CachedBrushType = value;
                }
            }
        }

        int CachedToolType;
        public int ToolType
        {
            get
            {
                return CachedToolType;
            }
            set
            {
                if (value != CachedToolType)
                {
                    CachedToolType = value;
                }
                else
                {
                    CachedToolType = value;
                }
            }
        }

        int CachedTileType;
        public int TileType
        {
            get
            {
                return CachedTileType;
            }
            set
            {
                CachedTileType = value;
            }
        }

        public event Action OnBeforeChanged;

        void OnBrushSwitched()
        {
            if (CachedBrushType == 0)
            {
                TempEdges.Clear();
            }
            else if (CachedBrushType == 1)
            {
                TempTiles.Clear();
            }
        }

        public void EditingUpdate()
        {
            Clear();
            TilesUpdate();
        }

        public void EditingFinish()
        {
            if (Tiles != null)
            {
                Map.Tiles = Tiles.Select(x => x.Value).ToList();
            }

            ClearTemp();
            EditorUtility.SetDirty(Map);
        }

        public void Redraw()
        {
            Clear();
            TilesUpdate();
        }

        public void Draw(bool isDrawing, bool showMap)
        {
            if (isDrawing)
            {
                Draw();
            }
            else if (showMap)
            {
                Observe();
            }
        }

        public void Clear()
        {
            Tiles.Clear();
            TileColors.Clear();
            CursorTileColors.Clear();
            ClearTemp();
        }

        void Draw()
        {
            if (CachedBrushType == 0)
            {
                TilesBrushDraw();
            }
            else if (CachedBrushType == 1)
            {
                EdgesBrushDraw();
            }
        }

        void ClearTemp()
        {
            TempTiles.Clear();
            TempEdges.Clear();
        }

        void Observe()
        {
            TilesDraw();
        }

        void TilesBrushDraw()
        {
            if (Map != null && TileIds.Length > 0)
            {
                var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                float enter;
                var groundPos = Vector3.zero;
                if (Ground.Raycast(ray, out enter))
                {
                    groundPos = ray.GetPoint(enter);
                }
                var tilePos = Map.WorldPosToTileFunc(groundPos, Map.Edge);

                var ev = Event.current;
                if (ev.type == EventType.MouseDown)
                {
                    if (ev.button == 0 && !ev.alt && !ev.shift)
                    {
                        TempTiles.Clear();
                        TempTiles.Add(tilePos);
                    }
                }
                else if (ev.type == EventType.MouseUp)
                {
                    if (ev.button == 0)
                    {
                        TilesTempApply();
                    }
                }

                Color cursorColor;
                Color tempTileColor;
                if (ToolType == 0)
                {
                    cursorColor = CursorTileColors.TryGetOrDefault(TileIds[CachedTileType]);
                    tempTileColor = TileColors.TryGetOrDefault(Map.Presets[CachedTileType].Id);
                }
                else
                {
                    cursorColor = WindowSettings.TileCursorErase;
                    tempTileColor = WindowSettings.TileCursorErase;
                }

                TilesDraw();
                TilesTempDraw(tempTileColor);
                if (TempTiles.Any())
                {
                    TempTiles.Add(tilePos);
                }

                var world = Map.TilePosToWorldFunc(tilePos, Map.Edge);
                TilesCursorShow(world, cursorColor, Map.Edge);

                if (Event.current.type == EventType.Layout)
                {
                    HandleUtility.AddDefaultControl(0);
                }
            }
        }

        void TilesUpdate()
        {
            if (Map != null)
            {
                foreach (var p in Map.Presets)
                {
                    TileColors.Add(p.Id, p.MapColor);
                    CursorTileColors.Add(p.Id, new Color(p.MapColor.r, p.MapColor.g, p.MapColor.b, Mathf.Clamp01(p.MapColor.a * 1.5f)));
                }

                foreach (var t in (Map.Tiles))
                {
                    Tiles.Add(t.TilePos, t);
                }
            }
        }

        void TilesDraw()
        {
            if (Map != null)
            {
                System.Action<Vector3, Color, float> drawTile = null;
                if (Map.Type == GridType.Hex)
                {
                    drawTile = GuiStyles.DrawHex;
                }
                else
                {
                    drawTile = GuiStyles.DrawSquare;
                }
                foreach (var t in Tiles)
                {
                    var pos = Map.TilePosToWorldFunc(t.Key, Map.Edge);
                    var color = TileColors.TryGetOrDefault(t.Value.Id);
                    drawTile(pos, color, Map.Edge);

                    for (int i = 0; i < t.Value.SideHeight.Length; i++)
                    {
                        if (t.Value.SideHeight[i] > 0f)
                        {
                            var edgePos = (pos + (pos + Map.TilePosToWorldFunc(Map.TileNeighbourAtIndex(i), Map.Edge))) / 2f;
                            var edgeRot = Map.TileSideRotation(i);
                            GuiStyles.DrawRect(edgePos, EdgeColor, 90f + edgeRot, Map.Edge, Map.Edge * 0.1f);
                        }
                    }

                    GuiStyles.DrawLabel(t.Value.MovableArea.ToString(), pos, LabelColor);
                }
            }
        }

        void TilesCursorShow(Vector3 point, Color color, float edge)
        {
            System.Action<Vector3, Color, float> drawTile = null;
            if (Map.Type == GridType.Hex)
            {
                drawTile = GuiStyles.DrawHex;
            }
            else
            {
                drawTile = GuiStyles.DrawSquare;
            }
            drawTile(point, color, edge);
        }

        void TilesTempApply()
        {
            OnBeforeChanged.SafeInvoke();
            foreach (var t in TempTiles)
            {
                var type = TileIds == null ? string.Empty : TileIds[CachedTileType];
                var preset = Map.Presets[CachedTileType];
                var existed = Tiles.TryGetOrDefault(t);
                if (ToolType == 0)
                {
                    if (existed == null)
                    {
                        Tiles.Add(t, new TileData { TilePos = t, Id = preset.Id });
                    }
                    else
                    {
                        existed.Id = preset.Id;
                    }
                }
                else
                {
                    if (existed != null)
                    {
                        Tiles.Remove(t);
                    }
                }
            }
            TempTiles.Clear();
            EditingFinish();
            if (WindowSettings.AreasAutoMark)
            {
                Map.MarkAreas();
            }
        }

        void TilesTempDraw(Color color)
        {
            System.Action<Vector3, Color, float> drawTile = null;
            if (Map.Type == GridType.Hex)
            {
                drawTile = GuiStyles.DrawHex;
            }
            else
            {
                drawTile = GuiStyles.DrawSquare;
            }

            foreach (var t in TempTiles)
            {
                drawTile(Map.TilePosToWorldFunc(t, Map.Edge), color, Map.Edge);
            }
        }

        void EdgesBrushDraw()
        {
            if (Map != null)
            {
                var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                float enter;
                var groundPos = Vector3.zero;
                if (Ground.Raycast(ray, out enter))
                {
                    groundPos = ray.GetPoint(enter);
                }

                var tilePos = Map.WorldPosToTileFunc(groundPos, Map.Edge);
                var worldTileCenter = Map.TileCenterWorld(groundPos);
                var neighbourIndex = Map.TileNeighbourIndexAtDirection(groundPos - worldTileCenter);

                var neighPos = worldTileCenter + Map.TilePosToWorldFunc(Map.TileNeighbourAtIndex(neighbourIndex), Map.Edge);
                var edgePos = (worldTileCenter + neighPos) / 2f;
                var edgeRot = Map.TileSideRotation(neighbourIndex);

                var ev = Event.current;
                if (ev.type == EventType.MouseDown)
                {
                    if (ev.button == 0 && !ev.alt && !ev.shift)
                    {
                        TempEdges.Clear();
                        TempEdges.Add(new SideInfo { TilesPos = tilePos, NeighbourIndex = neighbourIndex });
                    }
                }
                else if (ev.type == EventType.MouseUp)
                {
                    if (ev.button == 0)
                    {
                        EdgesTempApply();
                    }
                }
                if (TempEdges.Any())
                {
                    TempEdges.Add(new SideInfo { TilesPos = tilePos, NeighbourIndex = neighbourIndex });
                }

                TilesDraw();
                EdgesTempDraw();
                EdgesCursorShow(edgePos, EdgeCursorColor, edgeRot, Map.Edge);

                if (Event.current.type == EventType.Layout)
                {
                    HandleUtility.AddDefaultControl(0);
                }
            }
        }

        void EdgesCursorShow(Vector3 point, Color color, float angle, float edge)
        {
            GuiStyles.DrawRect(point, color, 90f + angle, edge, 0.1f * edge);
            Handles.Label(point, string.Format("Edges = {0}", TempEdges.Count.ToString()), GuiStyles.CenterAligment);
        }

        void EdgesTempApply()
        {
            OnBeforeChanged.SafeInvoke();
            var obstacleHeight = CachedToolType == 0 ? 1f : 0f;
            foreach (var t in TempEdges)
            {
                var existed = Tiles.TryGetOrDefault(t.TilesPos);
                if (existed != null)
                {
                    existed.SideHeight[t.NeighbourIndex] = obstacleHeight;
                }
                var neighbour = Tiles.TryGetOrDefault(t.TilesPos + Map.TileNeighbourAtIndex(t.NeighbourIndex));
                if (neighbour != null)
                {
                    neighbour.SideHeight[Map.TileNeighbourIndexOpposite(t.NeighbourIndex)] = obstacleHeight;
                }
            }
            TempEdges.Clear();
            EditingFinish();
            if (WindowSettings.AreasAutoMark)
            {
                Map.MarkAreas();
            }
        }

        void EdgesTempDraw()
        {
            var color = ToolType == 0 ? EdgeCursorPaint : EdgeCursorErase;
            foreach (var t in TempEdges)
            {
                var tile = Map.GetTile(t.TilesPos);
                if (tile != null)
                {
                    var obstacleHeight = tile.SideHeight[t.NeighbourIndex];
                    var worldPos = Map.TilePosToWorldFunc(t.TilesPos, Map.Edge);
                    var neighPos = worldPos + Map.TilePosToWorldFunc(Map.TileNeighbourAtIndex(t.NeighbourIndex), Map.Edge);
                    var edgePos = (worldPos + neighPos) / 2f;
                    GuiStyles.DrawRect(edgePos, color, Map.TileSideRotation(t.NeighbourIndex) + 90f, Map.Edge, 0.1f * Map.Edge);
                }
            }
        }
    }
}
