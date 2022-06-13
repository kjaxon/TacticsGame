using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    [Serializable]
    public partial class MapEntity : IMapNode, IMapDirectional
    {
        float TileSize = 1f;
        TileDictionary Tiles = new TileDictionary();
        Vector3 Eps;
        Vector3[] Vertices;
        MapSettings Settings;
        MapView View;

        Func<Vector3, float, Vector3Int> WorldPosToTile;
        Func<Vector3Int, float, Vector3> TilePosToWorld;
        Func<Vector3Int, Vector3Int, float> DistanceFunc;
        Func<Vector3Int, float, List<Vector3Int>> AreaFunc;

        Action<Vector3Int, TileDictionary, float> Reset;

        public float TileDistance { get; private set; }
        public MapRules Rules { get { return Settings.Rules; } }

        MapEntity() { }

        public MapEntity(MapSettings settings,
                         MapView view,
                         float tileSize,
                         float tileDistance,
                         TileDictionary tiles,
                         Func<Vector3Int, Vector3Int, float> distanceFunc,
                         Func<Vector3Int, float, Vector3> tilePosToWorld,
                         Func<Vector3, float, Vector3Int> worldPosToTile,
                         Func<Vector3Int, float, List<Vector3Int>> areaFunc,
                         Action<Vector3Int, TileDictionary, float> reset,
                         Vector3Int[] neighboursDirection,
                         Vector3[] vertices,
                         Vector3 eps)
        {
            Settings = settings;
            View = view;
            TileSize = tileSize;
            TileDistance = TileSize * tileDistance;
            Tiles = tiles;
            DistanceFunc = distanceFunc;
            TilePosToWorld = tilePosToWorld;
            WorldPosToTile = worldPosToTile;
            NeighboursDirection = neighboursDirection;
            AreaFunc = areaFunc;
            Vertices = vertices;
            Reset = reset;
            Eps = eps;
        }

        /// <summary>
        /// Get tile entity by world space position
        /// </summary>
        /// <param name="worldPos">world space position </param>
        /// <returns></returns>
        public TileEntity Tile(Vector3 worldPos)
        {
            return Tiles.TryGetOrDefault(WorldPosToTile(worldPos, TileSize));
        }

        /// <summary>
        /// Get tile entity by position in tile coordinates
        /// </summary>
        /// <param name="tilePos"></param>
        /// <returns></returns>
        public TileEntity Tile(Vector3Int tilePos)
        {
            return Tiles.TryGetOrDefault(tilePos);
        }

        public TileEntity LineCast(Vector3 worldPosStart, Vector3 worldPosFinish, float range)
        {
            worldPosStart += Eps;
            worldPosFinish += Eps;
            var distance = Distance(worldPosStart, worldPosFinish);
            var step = (worldPosFinish - worldPosStart) / distance;
            TileEntity possible = null;
            for (int i = 0; i <= distance; i++)
            {
                var tile = Tile(worldPosStart + step * i);
                if (tile != null && Distance(worldPosStart, WorldPosition(tile)) > range)
                {
                    break;
                }
                possible = tile;
            }
            return possible;
        }

        public List<TileEntity> LineCast(Vector3 worldPosStart, Vector3 worldPosFinish, float range, Func<TileEntity, bool> condition, Func<TileEntity, bool> includeLast)
        {
            var result = new List<TileEntity>();
            worldPosStart = TileCenter(worldPosStart + Eps);
            worldPosFinish = TileCenter(worldPosFinish + Eps);
            var distance = Distance(worldPosStart, worldPosFinish);
            var step = (worldPosFinish - worldPosStart) / distance;
            distance = Mathf.Max(distance, range);
            for (int i = 0; i <= distance; i++)
            {
                var tile = Tile(worldPosStart + step * i);
                if (tile != null)
                {
                    if (Distance(worldPosStart, WorldPosition(tile)) > range)
                    {
                        break;
                    }
                    if (!condition(tile))
                    {
                        if (includeLast(tile))
                        {
                            result.Add(tile);
                        }
                        break;
                    }
                    result.Add(tile);
                }
            }
            return result;
        }

        public void LineCastNonAlloc(List<TileEntity> result, Vector3 worldPosStart, Vector3 worldPosFinish, float range, Func<TileEntity, bool> condition, bool includeLast = true)
        {
            result.Clear();
            worldPosStart = TileCenter(worldPosStart + Eps);
            worldPosFinish = TileCenter(worldPosFinish + Eps);
            var distance = Distance(worldPosStart, worldPosFinish);
            var step = distance < 0.001f ? Vector3.zero : (worldPosFinish - worldPosStart) / distance;
            distance = Mathf.Max(distance, range);
            for (int i = 0; i <= distance; i++)
            {
                var tile = Tile(worldPosStart + step * i);
                if (tile != null)
                {
                    if (Distance(worldPosStart, WorldPosition(tile)) > range)
                    {
                        return;
                    }
                    if (!condition(tile))
                    {
                        if (includeLast)
                        {
                            result.Add(tile);
                        }
                        return;
                    }
                    result.Add(tile);
                }
            }
        }

        /// <summary>
        /// Get world space position of center of tile entity
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public Vector3 WorldPosition(TileEntity tile)
        {
            return tile == null ? Vector3.zero : TilePosToWorld(tile.Position, TileSize);
        }

        /// <summary>
        /// Get world space position of center of tile bt it's coordinates
        /// </summary>
        /// <param name="tilePos"></param>
        /// <returns></returns>
        public Vector3 WorldPosition(Vector3Int tilePos)
        {
            return TilePosToWorld(tilePos, TileSize);
        }

        /// <summary>
        /// Get world space position of center of tile which is located at world space position
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public Vector3 TileCenter(Vector3 worldPos)
        {
            return TilePosToWorld(WorldPosToTile(worldPos, TileSize), TileSize);
        }

        /// <summary>
        /// Normalize vector scaled to tile distance
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 Normalize(Vector3 direction)
        {
            return direction.normalized * TileDistance;
        }

        /// <summary>
        /// Distance between two tiles
        /// </summary>
        /// <param name="tileA"></param>
        /// <param name="tileB"></param>
        /// <returns></returns>
        public float Distance(TileEntity tileA, TileEntity tileB)
        {
            return tileA == null || tileB == null ? float.MaxValue : DistanceFunc(tileA.Position, tileB.Position);
        }

        /// <summary>
        /// Distance between two tiles located at corresponding world space positions
        /// </summary>
        /// <param name="worldPosA"></param>
        /// <param name="worldPosB"></param>
        /// <returns></returns>
        public float Distance(Vector3 worldPosA, Vector3 worldPosB)
        {
            return DistanceFunc(WorldPosToTile(worldPosA, TileSize), WorldPosToTile(worldPosB, TileSize));
        }

        /// <summary>
        /// Get walkable tiles around origin at range maximum
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public HashSet<TileEntity> WalkableTiles(Vector3Int origin, float range)
        {
            var nodes = NodePathFinder.WalkableArea(this, Tile(origin), range);
            var tiles = new HashSet<TileEntity>();
            foreach (var n in nodes)
            {
                tiles.Add(n as TileEntity);
            }
            return tiles;
        }

        /// <summary>
        /// Check if two positions are inside same tile
        /// </summary>
        /// <param name="position1"></param>
        /// <param name="position2"></param>
        /// <returns></returns>
        public bool IsSameTile(Vector3 position1, Vector3 position2)
        {
            var tile1 = Tile(position1);
            var tile2 = Tile(position2);
            return tile1 != null && tile2 != null && tile1 == tile2;
        }

        /// <summary>
        /// Get first neighbour tile to position which is met defined condtion
        /// </summary>
        /// <param name="position"></param>
        /// <param name="nearest"></param>
        /// <param name="condition"></param>
        /// <param name="orderBy">order of neighbours</param>
        /// <returns></returns>
        public bool NearestPosition(Vector3Int position, out Vector3Int nearest, Func<TileEntity, bool> condition, Func<Vector3Int, float> orderBy)
        {
            nearest = Vector3Int.zero;
            foreach (var n in NeighboursDirection.OrderBy(orderBy))
            {
                var pos = n + position;
                var tile = Tile(pos);
                if (tile != null && condition(tile))
                {
                    nearest = tile.Position;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get path that consist of tile entities
        /// </summary>
        /// <param name="from">start position</param>
        /// <param name="to">finish position</param>
        /// <param name="range">maximum range</param>
        /// <returns></returns>
        public List<TileEntity> PathTiles(Vector3 from, Vector3 to, float range)
        {
            var nodes = NodePathFinder.Path(this, Tile(from), Tile(to), range);
            var path = new List<TileEntity>();
            if (nodes != null)
            {
                foreach (var n in nodes)
                {
                    path.Add(n as TileEntity);
                }
            }
            return path;
        }

        /// <summary>
        /// Get path that consist of world space positions
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public List<Vector3> PathPoints(Vector3 from, Vector3 to, float range)
        {
            var nodes = NodePathFinder.Path(this, Tile(from), Tile(to), range);
            var path = new List<Vector3>();
            if (nodes != null)
            {
                foreach (var n in nodes)
                {
                    path.Add(WorldPosition(n.Position));
                }
            }
            return path;
        }

        /// <summary>
        /// Get tile positions which cross the line between two world space postions
        /// </summary>
        /// <param name="worldPosA"></param>
        /// <param name="worldPosB"></param>
        /// <returns>in tile coordinates</returns>
        public HashSet<Vector3Int> LineTilePositions(Vector3 worldPosA, Vector3 worldPosB)
        {
            worldPosA += Eps;
            worldPosB += Eps;
            var distance = Distance(worldPosA, worldPosB);
            var step = (worldPosB - worldPosA) / distance;
            var result = new HashSet<Vector3Int>();
            for (int i = 0; i <= distance; i++)
            {
                var possible = Tile(worldPosA + step * i);
                if (possible != null)
                {
                    result.Add(possible.Position);
                }
            }
            return result;
        }

        /// <summary>
        /// Get positions of border of walkable area
        /// </summary>
        /// <param name="tilePosition"></param>
        /// <param name="range"></param>
        /// <returns>in world space coordinates</returns>
        public List<Vector3> WalkableBorder(Vector3Int tilePosition, float range)
        {
            var origin = Tile(tilePosition);
            return WalkableBorder(origin, range);
        }

        /// <summary>
        /// Get positions of border of walkable area
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public List<Vector3> WalkableBorder(Vector3 worldPosition, float range)
        {
            var origin = Tile(worldPosition);
            return WalkableBorder(origin, range);
        }

        /// <summary>
        /// Get positions of border of walkable area
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public List<Vector3> WalkableBorder(TileEntity origin, float range)
        {
            var borderPoints = new List<Vector3>();
            var walkable = NodePathFinder.WalkableAreaPositions(this, origin, range);
            var borderedAreas = MapBorder.FindBorderPositions(this, walkable);
            foreach (var point in borderedAreas)
            {
                borderPoints.Add(WorldPosition(point.TilePos) + Vertices[point.VerticeIndex] * TileSize);
            }
            return borderPoints;
        }

        /// <summary>
        /// Get area positions around origin at max range
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="range"></param>
        /// <returns>in world space positions</returns>
        public List<Vector3> Area(Vector3 origin, float range)
        {
            var tile = WorldPosToTile(origin, TileSize);
            var areaTiles = AreaFunc(tile, range);
            return areaTiles.Select(a => WorldPosition(a)).ToList();
        }

        /// <summary>
        /// Create grid with standard border size
        /// </summary>
        /// <param name="parent"></param>
        public void CreateGrid(Transform parent)
        {
            var cellPrefab = CreateCell(false, true);
            cellPrefab.transform.SetParent(parent);
            cellPrefab.transform.localPosition = Vector3.zero;
            foreach (var tile in Tiles)
            {
                var worldPos = WorldPosition(tile.Key);
                worldPos.y = tile.Value.Preset.GridOffset;
                var go = GameObject.Instantiate(cellPrefab, worldPos, Quaternion.identity, parent);
                go.name = $"Cell {tile.Key}";
            }
            cellPrefab.SetActive(false);
        }

        /// <summary>
        /// Create single cell
        /// </summary>
        /// <param name="showInner">show inside part of cell</param>
        /// <param name="showBorder">show border part of cell</param>
        /// <param name="borderSize">border width</param>
        /// <param name="inner">material of inside part</param>
        /// <param name="border">material of border part</param>
        /// <returns></returns>
        public GameObject CreateCell(bool showInner, bool showBorder, float borderSize, Material inner = null, Material border = null)
        {
            return Settings.CreateCell(showInner, inner, showBorder, borderSize, border);
        }

        /// <summary>
        /// Create single cell with standard border size
        /// </summary>
        /// <param name="showInner">show inside part of cell</param>
        /// <param name="showBorder">show border part of cell</param>
        /// <param name="inner">material of inside part</param>
        /// <param name="border">material of border part</param>
        /// <returns></returns>
        public GameObject CreateCell(bool showInner, bool showBorder, Material inner = null, Material border = null)
        {
            return Settings.CreateCell(showInner, inner, showBorder, border);
        }

        /// <summary>
        /// Set state of gameobject which contains map grid
        /// </summary>
        /// <param name="enable"></param>
        public void GridEnable(bool enable)
        {
            if (View)
            {
                View.GridEnable(enable);
            }
            else
            {
                Debug.LogErrorFormat("Can't enable Grid state: {0}. MapView was not set inside MapEntity", enable);
            }
        }

        /// <summary>
        /// Toggle gameobject which contains map grid
        /// </summary>
        public void GridToggle()
        {
            if (View)
            {
                View.GridToggle();
            }
            else
            {
                Debug.LogError("Can't toggle Grid state. MapView was not set inside MapEntity");
            }
        }

        #region IMapNode
        public Vector3Int[] NeighboursDirection { get; private set; }

        float IMapNode.Distance(INode x, INode y)
        {
            return x == null || y == null ? float.MaxValue : DistanceFunc(x.Position, y.Position);
        }

        IEnumerable<INode> IMapNode.Neighbours(INode node)
        {
            for (int i = 0; i < NeighboursDirection.Length; i++)
            {
                var n = NeighboursDirection[i];
                yield return Tiles.TryGetOrDefault(n + node.Position);
            }
        }

        IEnumerable<INode> IMapNode.NeighborsMovable(INode node)
        {
            var nodeEntity = Tiles.TryGetOrDefault(node.Position);
            for (int i = 0; i < NeighboursDirection.Length; i++)
            {
                if (nodeEntity.NeighbourMovable[i] <= 0f)
                {
                    var n = NeighboursDirection[i];
                    var neigh = Tiles.TryGetOrDefault(n + node.Position);
                    if (neigh != null)
                    {
                        yield return neigh;
                    }
                }
            }
        }

        void IMapNode.Reset()
        {
            foreach (var tile in Tiles)
            {
                tile.Value.Depth = float.MaxValue;
                tile.Value.Visited = false;
                tile.Value.Considered = false;
            }
        }

        void IMapNode.Reset(float range, INode startNode)
        {
            if (startNode != null)
            {
                var startPos = startNode.Position;
                Reset(startPos, Tiles, range);
            }
        }
        #endregion // IMapNode

        #region IMapDirectional
        Vector3Int IMapDirectional.TurnLeft(Vector3Int fromDirection)
        {
            var ind = Array.IndexOf(NeighboursDirection, fromDirection * -1);
            return NeighboursDirection[(ind + 1) % NeighboursDirection.Length];
        }

        Vector3Int IMapDirectional.TurnRight(Vector3Int fromDirection)
        {
            var ind = Array.IndexOf(NeighboursDirection, fromDirection * -1);
            if (ind == 0)
            {
                return NeighboursDirection[NeighboursDirection.Length - 1];
            }
            return NeighboursDirection[ind - 1];
        }
        #endregion // IMapDirectional
    }
}
