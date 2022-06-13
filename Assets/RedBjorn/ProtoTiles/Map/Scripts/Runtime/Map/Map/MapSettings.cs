using RedBjorn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    [CreateAssetMenu(menuName = "Red Bjorn/Settings/Map")]
    public class MapSettings : ScriptableObjectExtended
    {
        public GridType Type;
        [HideInInspector] public float Edge = 1f;
        [HideInInspector] public LayerMask MapMask;
        [Range(0f, 1f)] public float BorderSize = 0.025f;
        public Material CellMaterial;
        public MapRules Rules;
        public List<TilePreset> Presets = new List<TilePreset>();
        public List<TileData> Tiles = new List<TileData>();

#pragma warning disable 0414
        string DefaultTileTypeName = "Ground";
#pragma warning restore 0414

        /// <summary>
        /// Map View at Scene View is empty?
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                var holder = GetMapHolder();
                return holder == null || holder.transform.childCount == 0;
            }
        }

        /// <summary>
        /// Method converting position in world space coordinates to tile coordinates
        /// </summary>
        public Func<Vector3, float, Vector3Int> WorldPosToTileFunc
        {
            get
            {
                switch (Type)
                {
                    case GridType.Hex: return Hex.WorldToHexCube;
                    case GridType.Square: return Square.WorldToSquare;
                }
                return null;
            }
        }

        /// <summary>
        /// Method converting position in tile coordinates to world space coordinates
        /// </summary>
        public Func<Vector3Int, float, Vector3> TilePosToWorldFunc
        {
            get
            {
                switch (Type)
                {
                    case GridType.Hex: return Hex.HexCubeToPoint;
                    case GridType.Square: return Square.SquareToPoint;
                }
                return null;
            }
        }

        /// <summary>
        /// Method calculating distance between two tiles with corresponding positions with side size = 1
        /// </summary>
        Func<Vector3Int, Vector3Int, float> DistanceFunc
        {
            get
            {
                switch (Type)
                {
                    case GridType.Hex: return Hex.HexCubeDistance;
                    case GridType.Square: return Square.Distance;
                }
                return null;
            }
        }

        /// <summary>
        /// Neighbour directions in integer coordinates with side size = 1
        /// </summary>
        Vector3Int[] NeighbourDirections
        {
            get
            {
                switch (Type)
                {
                    case GridType.Hex: return Hex.HexCubeNeighbour;
                    case GridType.Square: return Square.Neighbour;
                }
                return null;
            }
        }

        /// <summary>
        /// Init map settings asset with new data
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="rules"></param>
        public void Init(GridType grid, MapRules rules, Material material)
        {
            Type = grid;
            Edge = 1f;
            Rules = rules;
            CellMaterial = material;
            Presets.Clear();
            Tiles.Clear();
        }

        /// <summary>
        /// Add default preset for map settings
        /// </summary>
        /// <returns></returns>
        public TilePreset PresetAddDefault()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Add Tile Preset");
            var tile = new TilePreset
            {
                Id = GUID.Generate().ToString(),
                MapColor = new Color(1f, 1f, 1f, 0.5f),
                Type = DefaultTileTypeName
            };
            Presets.Add(tile);
            return tile;
#else
            return null;
#endif
        }

        /// <summary>
        /// Remove preset at index
        /// </summary>
        /// <param name="index"></param>
        public void PresetRemove(int index)
        {
            if (index < 0 || index >= Presets.Count)
            {
                Debug.LogErrorFormat("Can't remove tyle preset {0}, count: {1}", index, Presets.Count);
                return;
            }
#if UNITY_EDITOR
            Undo.RecordObject(this, "Remove Tile Preset");
#endif
            var currentPreset = Presets[index];
            Presets.RemoveAt(index);
            Tiles.RemoveAll(p => p.Id == currentPreset.Id);
        }

        /// <summary>
        /// Tile side rotation
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float TileSideRotation(int index)
        {
            switch (Type)
            {
                case GridType.Hex: return Hex.SideRotation[index];
                case GridType.Square: return Square.SideRotation[index];
            }
            return 0f;
        }

        /// <summary>
        /// Find tile data by integer position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public TileData GetTile(Vector3Int position)
        {
            return Tiles.FirstOrDefault(t => t.TilePos == position);
        }

        /// <summary>
        /// Tile center in world space coordinates
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 TileCenterWorld(Vector3 position)
        {
            switch (Type)
            {
                case GridType.Hex: return Hex.Center(position, Edge);
                case GridType.Square: return Square.Center(position, Edge);
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Index of tile neighbour located at direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public int TileNeighbourIndexAtDirection(Vector3 direction)
        {
            switch (Type)
            {
                case GridType.Hex: return Hex.NeighbourTileIndexAtDirection(direction);
                case GridType.Square: return Square.NeighbourTileIndexAtDirection(direction);
            }
            return 0;
        }

        /// <summary>
        /// Neighbour index of opposite tile neighbour
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int TileNeighbourIndexOpposite(int index)
        {
            switch (Type)
            {
                case GridType.Hex: return (index + 3) % Hex.HexCubeNeighbour.Length;
                case GridType.Square: return (index + 2) % Square.Neighbour.Length;
            }
            return 0;
        }

        /// <summary>
        /// Neighbour directions in tile coordinates with side size = 1
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3Int TileNeighbourAtIndex(int index)
        {
            switch (Type)
            {
                case GridType.Hex:
                    if (index >= Hex.HexCubeNeighbour.Length)
                    {
                        Debug.LogErrorFormat("Index = {0}", index);
                        return Vector3Int.zero;
                    }
                    return Hex.HexCubeNeighbour[index];
                case GridType.Square:
                    if (index >= Square.Neighbour.Length)
                    {
                        Debug.LogErrorFormat("Index = {0}", index);
                        return Vector3Int.zero;
                    }
                    return Square.Neighbour[index];
            }
            return Vector3Int.zero;
        }

        /// <summary>
        /// Create entities of tile presets
        /// </summary>
        /// <returns></returns>
        public TileDictionary CreateTiles()
        {
            var tiles = new TileDictionary();
            for (int i = 0; i < Tiles.Count; i++)
            {
                var tilePreset = Tiles[i];
                var type = Presets.FirstOrDefault(t => t.Id == tilePreset.Id);
                tiles[tilePreset.TilePos] = new TileEntity(tilePreset, type, Rules);
            }
            return tiles;
        }

        /// <summary>
        /// Create entity of map settings
        /// </summary>
        /// <returns></returns>
        public MapEntity CreateEntity(MapView view)
        {
            MapEntity map = null;
            switch (Type)
            {
                case GridType.Hex: map = CreateHexMap(view); break;
                case GridType.Square: map = CreateSquareMap(view); break;
            }
            return map;
        }

        MapEntity CreateHexMap(MapView view)
        {
            var map = new MapEntity(this, view, Edge, Hex.DistanceBetweenCenters, CreateTiles(), Hex.HexCubeDistance, Hex.HexCubeToPoint, Hex.WorldToHexCube, Hex.Area, ResetHex, Hex.HexCubeNeighbour, Hex.Vertices, Hex.Eps);
            return map;
        }

        MapEntity CreateSquareMap(MapView view)
        {
            var map = new MapEntity(this, view, Edge, Edge, CreateTiles(), Square.Distance, Square.SquareToPoint, Square.WorldToSquare, Square.Area, ResetSquare, Square.Neighbour, Square.Vertices, Square.Eps);
            return map;
        }

        /// <summary>
        /// Place tile presets prefabs to scene
        /// </summary>
        public void PlacePrefabs()
        {
            switch (Type)
            {
                case GridType.Hex: PlacePrefabsHex(); break;
                case GridType.Square: PlacePrefabsSquare(); break;
            }
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }

        void PlacePrefabsHex()
        {
            var holder = GetMapHolder(true);
            var parent = new GameObject("Tiles");
            parent.transform.SetParent(holder.transform);
            parent.transform.localPosition = Vector3.zero;

            foreach (var t in Tiles)
            {
                var preset = Presets.FirstOrDefault(p => p.Id == t.Id);
                if (preset != null)
                {
                    var prefab = Presets.FirstOrDefault(p => p.Id == t.Id).Prefab;
                    if (prefab != null)
                    {
#if UNITY_EDITOR
                        var tileGo = PrefabUtility.InstantiatePrefab(prefab, parent.transform) as GameObject;
                        tileGo.transform.localRotation = Quaternion.Inverse(parent.transform.rotation);
                        tileGo.transform.position = Hex.HexCubeToPoint(t.TilePos, Edge);
                        Undo.RegisterCreatedObjectUndo(tileGo, "Create MapView");
#endif
                    }
                }
            }
        }

        void PlacePrefabsSquare()
        {
            var holder = GetMapHolder(true);
            var parent = new GameObject("Tiles");
            parent.transform.SetParent(holder.transform);
            parent.transform.localPosition = Vector3.zero;
            foreach (var t in Tiles)
            {
                var preset = Presets.FirstOrDefault(p => p.Id == t.Id);
                if (preset != null)
                {
                    var prefab = Presets.FirstOrDefault(p => p.Id == t.Id).Prefab;
                    if (prefab != null)
                    {
#if UNITY_EDITOR
                        var tileGo = PrefabUtility.InstantiatePrefab(prefab, parent.transform) as GameObject;
                        tileGo.transform.localRotation = Quaternion.Inverse(parent.transform.rotation);
                        tileGo.transform.position = Square.SquareToPoint(t.TilePos, Edge);
                        Undo.RegisterCreatedObjectUndo(tileGo, "Create MapView");
#endif
                    }
                }
            }
        }

        /// <summary>
        /// Clear Map View at Scene View and clear tile data
        /// </summary>
        public void Clear()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Clear");
            Tiles.Clear();
            var holder = FindObjectOfType<MapView>();
            if (holder != null)
            {
                for (int i = holder.transform.childCount - 1; i >= 0; i--)
                {
                    Undo.DestroyObjectImmediate(holder.transform.GetChild(i).gameObject);
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }

        MapView GetMapHolder(bool doClean = false)
        {
#if UNITY_EDITOR
            var holder = FindObjectOfType<MapView>();
            if (holder == null)
            {
                var holderGo = new GameObject();
                holder = holderGo.AddComponent<MapView>();
                holderGo.name = "MapView";
                holderGo.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                Undo.RegisterCreatedObjectUndo(holderGo, "Create Map View");
            }

            if (doClean)
            {
                for (int i = holder.transform.childCount - 1; i >= 0; i--)
                {
                    Undo.DestroyObjectImmediate(holder.transform.GetChild(i).gameObject);
                }
            }
            return holder;
#else
            return null;
#endif
        }

        public void MapCreate(int rows, int columns)
        {
            switch (Type)
            {
                case GridType.Hex: MapCreateHex(rows, columns); break;
                case GridType.Square: MapCreateSquare(rows, columns); break;
            }
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }

        void MapCreateHex(int rows, int columns)
        {
            var preset = Presets.FirstOrDefault();
            if (preset == null)
            {
                preset = PresetAddDefault();
            }

            Tiles.Clear();
            var shift3 = rows / 2;
            var shift4 = columns / 4 + (columns % 4 == 3 ? 1 : 0);
            var shift5 = columns / 4;
            var bottomLeftHex = Hex.HexCubeNeighbour[3] * shift3 + Hex.HexCubeNeighbour[4] * shift4 + Hex.HexCubeNeighbour[5] * shift5;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    var tilePos = bottomLeftHex + Hex.HexCubeNeighbour[1] * ((j + 1) / 2) + Hex.HexCubeNeighbour[2] * (j / 2);
                    var tile = new TileData() { TilePos = tilePos, Id = preset.Id };
                    tile.SideHeight = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
                    Tiles.Add(tile);
                }
                bottomLeftHex += Hex.HexCubeNeighbour[0];
            }
        }

        void MapCreateSquare(int rows, int columns)
        {
            var preset = Presets.FirstOrDefault();
            if (preset == null)
            {
                preset = PresetAddDefault();
            }
            if (preset != null)
            {
                Tiles.Clear();
                var leftColumn = Mathf.FloorToInt(columns / 2f);
                var bottomRow = Mathf.FloorToInt(rows / 2f);
                var bottomLeftSquare = new Vector3Int(-leftColumn, 0, -bottomRow);

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        var tilePos = bottomLeftSquare + Square.Neighbour[1] * j;
                        var tile = new TileData() { TilePos = tilePos, Id = preset.Id };
                        tile.SideHeight = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
                        Tiles.Add(tile);
                    }
                    bottomLeftSquare += Square.Neighbour[0];
                }
            }
        }

        public void MapAnalyze()
        {
            switch (Type)
            {
                case GridType.Hex: MapAnalyzeHex(); break;
                case GridType.Square: MapAnalyzeSquare(); break;
            }
        }

        void MapAnalyzeHex(int rows = 0, int colummns = 0, bool symmetric = true)
        {
            Tiles.Clear();
            var shift3 = rows / 2;
            var shift4 = colummns / 4 + (colummns % 4 == 3 ? 1 : 0);
            var shift5 = colummns / 4;
            var bottomLeftHex = Hex.HexCubeNeighbour[3] * shift3 + Hex.HexCubeNeighbour[4] * shift4 + Hex.HexCubeNeighbour[5] * shift5;

            if (symmetric)
            {
                var start = bottomLeftHex + Hex.HexCubeNeighbour[2];
                for (int j = 1; j < colummns; j++)
                {
                    if (j % 2 == 1)
                    {
                        var tilePos = start + (Hex.HexCubeNeighbour[1] + Hex.HexCubeNeighbour[2]) * (j / 2);
                        var ray = new Ray(Hex.HexCubeToPoint(tilePos, Edge) + Vector3.up * 10f, Vector3.down);
                        var movable = !Physics.SphereCast(ray, 0.7f * Edge * Constants.Cos30, 10f - Edge - 0.01f, MapMask);
                        var tile = new TileData() { TilePos = tilePos };
                        tile.SideHeight = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
                        for (int k = 0; k < Hex.HexCubeNeighbour.Length; k++)
                        {
                            var neighDir = Hex.HexCubeNeighbour[k];
                            var neight = tilePos + neighDir;
                            var betweenPos = (Hex.HexCubeToPoint(neight) + Hex.HexCubeToPoint(tilePos)) / 2f;
                            if (Physics.Raycast(betweenPos + Vector3.up * 10f, Vector3.down, 9.95f, MapMask))
                            {
                                tile.SideHeight[k] = 0.5f;
                            }
                        }
                        Tiles.Add(tile);
                    }
                }
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < colummns; j++)
                {
                    var tilePos = bottomLeftHex + Hex.HexCubeNeighbour[1] * ((j + 1) / 2) + Hex.HexCubeNeighbour[2] * (j / 2);
                    var ray = new Ray(Hex.HexCubeToPoint(tilePos, Edge) + Vector3.up * 10f, Vector3.down);
                    var movable = !Physics.SphereCast(ray, 0.7f * Edge * Constants.Cos30, 10f - Edge - 0.01f, MapMask);
                    var tile = new TileData() { TilePos = tilePos };
                    tile.SideHeight = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
                    for (int k = 0; k < Hex.HexCubeNeighbour.Length; k++)
                    {
                        var neighDir = Hex.HexCubeNeighbour[k];
                        var neight = tilePos + neighDir;
                        var betweenPos = (Hex.HexCubeToPoint(neight) + Hex.HexCubeToPoint(tilePos)) / 2f;
                        if (Physics.Raycast(betweenPos + Vector3.up * 10f, Vector3.down, 9.95f, MapMask))
                        {
                            tile.SideHeight[k] = 0.5f;
                        }
                    }
                    Tiles.Add(tile);
                }
                bottomLeftHex += Hex.HexCubeNeighbour[0];
            }
        }

        void MapAnalyzeSquare()
        {

        }

        /// <summary>
        /// Calculate tile area ownership
        /// </summary>
        public void MarkAreas()
        {
            switch (Type)
            {
                case GridType.Hex: MarkAreas(Hex.HexCubeDistance, Hex.HexCubeNeighbour); break;
                case GridType.Square: MarkAreas(Square.Distance, Square.Neighbour); break;
            }
        }

        void MarkAreas(Func<Vector3Int, Vector3Int, float> DistanceFunc, Vector3Int[] NeighboursDirection)
        {
            var map = new MapEntityMock();
            map.DistanceFunc = DistanceFunc;
            map.NeighboursDirection = NeighboursDirection;
            for (int i = 0; i < Tiles.Count; i++)
            {
                var tilePreset = Tiles[i];
                var type = Presets.FirstOrDefault(t => t.Id == tilePreset.Id);
                map.Tiles[Tiles[i].TilePos] = new TileEntity(tilePreset, type, Rules);
            }
            int movableArea = 1;
            var marked = new HashSet<INode>();
            var walkableCount = map.Tiles.Count(t => t.Value.Vacant);
            while (marked.Count < walkableCount)
            {
                var walkable = map.Tiles.FirstOrDefault(t => t.Value.Vacant && !marked.Any(m => m.Position == t.Value.Position));
                if (walkable.Value != null)
                {
                    var accessible = NodePathFinder.AccessibleArea(map, walkable.Value);
                    foreach (var a in accessible)
                    {
                        a.ChangeMovableAreaPreset(movableArea);
                        marked.Add(a);
                    }
                }
                else
                {
                    break;
                }
                movableArea++;
            }

            foreach (var t in map.Tiles.Where(t => !t.Value.Vacant))
            {
                t.Value.Data.MovableArea = 0;
            }
        }

        void ResetHex(Vector3Int startPos, TileDictionary Tiles, float range)
        {
            var rangeBorder = Mathf.CeilToInt(range);
            for (int x = -rangeBorder; x <= rangeBorder; x++)
            {
                var start = Mathf.Max(-rangeBorder, -x - rangeBorder);
                var finish = Mathf.Min(rangeBorder, -x + rangeBorder);
                for (int y = start; y <= finish; y++)
                {
                    var tile = Tiles.TryGetOrDefault(new Vector3Int(x, y, -x - y) + startPos);
                    if (tile != null)
                    {
                        tile.Depth = float.MaxValue;
                        tile.Visited = false;
                        tile.Considered = false;
                    }
                }
            }
        }

        void ResetSquare(Vector3Int startPos, TileDictionary Tiles, float range)
        {
            var rangeBorder = Mathf.CeilToInt(range);
            for (int x = -rangeBorder; x <= rangeBorder; x++)
            {
                for (int z = -rangeBorder; z <= rangeBorder; z++)
                {
                    var tile = Tiles.TryGetOrDefault(new Vector3Int(x, 0, z) + startPos);
                    if (tile != null)
                    {
                        tile.Depth = float.MaxValue;
                        tile.Visited = false;
                        tile.Considered = false;
                    }
                }
            }
        }

        public GameObject CreateCell(bool showInner, Material inner, bool showBorder, Material border)
        {
            return CreateCell(showInner, inner, showBorder, BorderSize, border);
        }

        public GameObject CreateCell(bool showInner, Material inner, bool showBorder, float borderSize, Material border)
        {
            switch (Type)
            {
                case GridType.Hex: return CreateCellHex(Edge, showInner, inner, showBorder, borderSize, border ?? CellMaterial);
                case GridType.Square: return CreateCellSquare(Edge, showInner, inner, showBorder, borderSize, border ?? CellMaterial);
            }
            return null;
        }

        GameObject CreateCellHex(float size, bool showInner, Material innerMaterial, bool showBorder, float border, Material borderMaterial)
        {
            var go = new GameObject("Cell");
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            border = Mathf.Clamp01(border);
            vertices.Add(Hex.Vertices[0] * size);
            vertices.Add(Hex.Vertices[1] * size);
            vertices.Add(Hex.Vertices[2] * size);
            vertices.Add(Hex.Vertices[3] * size);
            vertices.Add(Hex.Vertices[4] * size);
            vertices.Add(Hex.Vertices[5] * size);
            vertices.Add(Hex.Vertices[0] * (1 - border) * size);
            vertices.Add(Hex.Vertices[1] * (1 - border) * size);
            vertices.Add(Hex.Vertices[2] * (1 - border) * size);
            vertices.Add(Hex.Vertices[3] * (1 - border) * size);
            vertices.Add(Hex.Vertices[4] * (1 - border) * size);
            vertices.Add(Hex.Vertices[5] * (1 - border) * size);
            mesh.SetVertices(vertices);

            int submesh = 0;
            var triangles = new List<int>();
            var materials = new List<Material>();
            if (showBorder)
            {
                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(6);
                triangles.Add(1);
                triangles.Add(7);
                triangles.Add(6);
                triangles.Add(1);
                triangles.Add(2);
                triangles.Add(7);
                triangles.Add(2);
                triangles.Add(8);
                triangles.Add(7);
                triangles.Add(2);
                triangles.Add(3);
                triangles.Add(8);
                triangles.Add(3);
                triangles.Add(9);
                triangles.Add(8);
                triangles.Add(3);
                triangles.Add(4);
                triangles.Add(9);
                triangles.Add(4);
                triangles.Add(10);
                triangles.Add(9);
                triangles.Add(4);
                triangles.Add(5);
                triangles.Add(10);
                triangles.Add(5);
                triangles.Add(11);
                triangles.Add(10);
                triangles.Add(0);
                triangles.Add(11);
                triangles.Add(5);
                triangles.Add(0);
                triangles.Add(6);
                triangles.Add(11);
                mesh.SetTriangles(triangles, submesh);
                materials.Add(borderMaterial);
                submesh++;
            }

            if (showInner)
            {
                mesh.subMeshCount = submesh + 1;
                triangles.Clear();
                triangles.Add(6);
                triangles.Add(7);
                triangles.Add(11);
                triangles.Add(7);
                triangles.Add(8);
                triangles.Add(11);
                triangles.Add(8);
                triangles.Add(9);
                triangles.Add(11);
                triangles.Add(9);
                triangles.Add(10);
                triangles.Add(11);
                mesh.SetTriangles(triangles, submesh);
                materials.Add(innerMaterial);
                submesh++;
            }

            var filter = go.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterials = materials.ToArray();
            return go;
        }

        GameObject CreateCellSquare(float size, bool showInner, Material innerMaterial, bool showBorder, float border, Material borderMaterial)
        {
            var go = new GameObject("Cell");
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            border = Mathf.Clamp01(border);
            var borderHalf = border / 2f;
            vertices.Add(Square.Vertices[0] * size);
            vertices.Add(Square.Vertices[1] * size);
            vertices.Add(Square.Vertices[2] * size);
            vertices.Add(Square.Vertices[3] * size);
            vertices.Add((Square.Vertices[0] + new Vector3(-borderHalf, 0f, -borderHalf)) * size);
            vertices.Add((Square.Vertices[1] + new Vector3(-borderHalf, 0f, borderHalf)) * size);
            vertices.Add((Square.Vertices[2] + new Vector3(borderHalf, 0f, borderHalf)) * size);
            vertices.Add((Square.Vertices[3] + new Vector3(borderHalf, 0f, -borderHalf)) * size);
            mesh.SetVertices(vertices);

            int submesh = 0;
            var triangles = new List<int>();
            var materials = new List<Material>();
            if (showBorder)
            {
                triangles.Add(1);
                triangles.Add(4);
                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(5);
                triangles.Add(4);
                triangles.Add(1);
                triangles.Add(2);
                triangles.Add(5);
                triangles.Add(2);
                triangles.Add(6);
                triangles.Add(5);
                triangles.Add(2);
                triangles.Add(3);
                triangles.Add(6);
                triangles.Add(3);
                triangles.Add(7);
                triangles.Add(6);
                triangles.Add(0);
                triangles.Add(7);
                triangles.Add(3);
                triangles.Add(0);
                triangles.Add(4);
                triangles.Add(7);
                mesh.SetTriangles(triangles, submesh);
                materials.Add(borderMaterial);
                submesh++;
            }
            
            if (showInner)
            {
                mesh.subMeshCount = submesh + 1;
                triangles.Clear();
                triangles.Add(4);
                triangles.Add(5);
                triangles.Add(7);

                triangles.Add(5);
                triangles.Add(6);
                triangles.Add(7);
                mesh.SetTriangles(triangles, submesh);
                materials.Add(innerMaterial);
            }

            var filter = go.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterials = materials.ToArray();
            return go;
        }
    }
}


