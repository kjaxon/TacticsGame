using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    public class Square
    {
        /// <summary>
        /// Epsilon
        /// </summary>
        public static readonly Vector3 Eps = new Vector3(1e-6f, 1e-6f, 1e-6f);

        /// <summary>
        /// Neighbour directions with side size = 1
        /// </summary>
        public static readonly Vector3Int[] Neighbour = new Vector3Int[]
        {
            new Vector3Int( 0, 0, 1),
            new Vector3Int( 1, 0, 0),
            new Vector3Int( 0, 0,-1),
            new Vector3Int(-1, 0, 0),
        };

        /// <summary>
        /// Vertices of the single square with side size = 1
        /// </summary>
        public static readonly Vector3[] Vertices = new Vector3[]
        {
            new Vector3( 0.5f, 0f, 0.5f),
            new Vector3( 0.5f, 0f,-0.5f),
            new Vector3(-0.5f, 0f,-0.5f),
            new Vector3(-0.5f, 0f, 0.5f)
        };

        /// <summary>
        /// Points of square side centers with side size = 1
        /// </summary>
        public static readonly Vector3[] SideCenter = new Vector3[]
        {
            (Vertices[0] + Vertices[3]) / 2f,
            (Vertices[1] + Vertices[0]) / 2f,
            (Vertices[2] + Vertices[1]) / 2f,
            (Vertices[3] + Vertices[2]) / 2f,
        };

        public static readonly float[] SideRotation = new float[] {
             90f,
              0f,
             90f,
              0f,
        };

        /// <summary>
        /// Distance between two squares with side size = 1
        /// </summary>
        public static readonly float DistanceBetweenCenters = 1f;

        /// <summary>
        /// Index of neighbour direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>Index starting from 4th vertice</returns>
        public static int NeighbourTileIndexAtDirection(Vector3 direction)
        {
            var v1 = Vertices[3];
            var v2 = new Vector3(direction.x, 0f, direction.z);
            var angle = Vector3.Angle(v1, v2);
            angle = Mathf.Sign(Vector3.Cross(v1, v2).y) < 0 ? 360 - angle : angle;
            return Mathf.FloorToInt(angle / 90f);
        }

        /// <summary>
        /// Distance between two squares with corresponding positions with side size = 1
        /// </summary>
        /// <param name="coord1"></param>
        /// <param name="coord2"></param>
        /// <returns></returns>
        public static float Distance(Vector3Int coord1, Vector3Int coord2)
        {
            return Vector3Int.Distance(coord1, coord2);
        }

        /// <summary>
        /// Convert position in integer coordinates to world space coordinates
        /// </summary>
        /// <param name="square"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector3 SquareToPoint(Vector3Int square, float size = 1f)
        {
            return new Vector3(square.x * size, 0f, square.z * size);
        }

        /// <summary>
        /// Convert position in world space coordinates to integer coordinates
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector3Int WorldToSquare(Vector3 point, float size = 1f)
        {
            return new Vector3Int(Mathf.RoundToInt(point.x / size), 0, Mathf.RoundToInt(point.z / size));
        }

        /// <summary>
        /// Center of nearest square
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector3 Center(Vector3 position, float size = 1f)
        {
            return SquareToPoint(WorldToSquare(position, size));
        }

        /// <summary>
        /// Positions of squares in area around origin at max range with side size = 1
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="range"></param>
        /// <returns>integer coordinates</returns>
        public static List<Vector3Int> Area(Vector3Int origin, float range)
        {
            if(range <= 0f)
            {
                return new List<Vector3Int> { origin };
            }
            var rangeSqr = range * range;
            var rangeBorder = Mathf.CeilToInt(range);
            var area = new List<Vector3Int>();
            var bottomX = origin.x - rangeBorder;
            var topX = origin.x + rangeBorder;
            var bottomZ = origin.z - rangeBorder;
            var topZ = origin.z + rangeBorder;
            for (int x = bottomX; x <= topX; x++)
            {
                for (int z = bottomZ; z <= topZ; z++)
                {
                    var v = origin + new Vector3Int(x, 0, z);
                    if(v.sqrMagnitude <= rangeSqr)
                    {
                        area.Add(v);
                    }
                }
            }
            return area;
        }
    }
}