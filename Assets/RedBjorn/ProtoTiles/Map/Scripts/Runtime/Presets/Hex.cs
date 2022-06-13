using RedBjorn.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    public class Hex
    {
        #region Cube Coordinates
        /// <summary>
        /// Epsilon
        /// </summary>
        public static readonly Vector3 Eps = new Vector3(1e-6f, 2e-6f, -3e-6f);

        /// <summary>
        /// Neighbour directions in hex cube integer coordinates with side size = 1
        /// </summary>
        public static readonly Vector3Int[] HexCubeNeighbour = new Vector3Int[]
        {
            new Vector3Int( 0, 1,-1),
            new Vector3Int( 1, 0,-1),
            new Vector3Int( 1,-1, 0),
            new Vector3Int( 0,-1, 1),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(-1, 1, 0)
        };

        /// <summary>
        /// Vertices of the single hex with side size = 1
        /// </summary>
        public static readonly Vector3[] Vertices = new Vector3[]
        {
            new Vector3( Constants.Cos60, 0f,  Constants.Cos30),
            new Vector3( 1f,              0f,  0f),
            new Vector3( Constants.Cos60, 0f, -Constants.Cos30),
            new Vector3(-Constants.Cos60, 0f, -Constants.Cos30),
            new Vector3(-1,               0f,  0f),
            new Vector3(-Constants.Cos60, 0f,  Constants.Cos30)
        };

        /// <summary>
        /// Points of hex side centers with side size = 1
        /// </summary>
        public static readonly Vector3[] SideCenter = new Vector3[]
        {
            (Vertices[0] + Vertices[5]) / 2f,
            (Vertices[1] + Vertices[0]) / 2f,
            (Vertices[2] + Vertices[1]) / 2f,
            (Vertices[3] + Vertices[2]) / 2f,
            (Vertices[4] + Vertices[3]) / 2f,
            (Vertices[5] + Vertices[4]) / 2f
        };

        public static readonly float[] SideRotation = new float[]
        {
             90f,
            -30f,
             30f,
             90f,
            -30f,
             30f
        };

        /// <summary>
        /// Distance between two hexes with side size = 1
        /// </summary>
        public static readonly float DistanceBetweenCenters = Constants.Sqrt3;

        /// <summary>
        /// Index of neighbour direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>Index starting from 6th vertice</returns>
        public static int NeighbourTileIndexAtDirection(Vector3 direction)
        {
            var v1 = Vertices[5];
            var v2 = new Vector3(direction.x, 0f, direction.z);
            var angle = Vector3.Angle(v1, v2); ;
            angle = Mathf.Sign(Vector3.Cross(v1, v2).y) < 0 ? 360 - angle : angle;
            return Mathf.FloorToInt(angle / 60f);
        }

        /// <summary>
        /// Nearest hex cube integer coordinates to position in world space coordinates with side size = 1
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3Int HexCubeNearest(Vector3 point)
        {
            var roundX = Mathf.RoundToInt(point.x);
            var roundY = Mathf.RoundToInt(point.y);
            var roundZ = Mathf.RoundToInt(point.z);

            var diffX = Mathf.Abs(roundX - point.x);
            var diffY = Mathf.Abs(roundY - point.y);
            var diffZ = Mathf.Abs(roundZ - point.z);

            if (diffX > diffY && diffX > diffZ)
            {
                roundX = -roundZ - roundY;
            }
            else if (diffY > diffX)
            {
                roundY = -roundX - roundZ;
            }
            else
            {
                roundZ = -roundX - roundY;
            }
            return new Vector3Int(roundX, roundY, roundZ);
        }

        /// <summary>
        /// Distance between two hexes with corresponding positions with side size = 1
        /// </summary>
        /// <param name="coord1"></param>
        /// <param name="coord2"></param>
        /// <returns></returns>
        public static float HexCubeDistance(Vector3Int coord1, Vector3Int coord2)
        {
            return Mathf.Max(Math.Abs(coord1.x - coord2.x), Math.Abs(coord1.y - coord2.y), Math.Abs(coord1.z - coord2.z));
        }

        /// <summary>
        /// Convert position in hex cube integer coordinates to world space coordinates
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector3 HexCubeToPoint(Vector3Int hex, float size = 1f)
        {
            var x = (Matrix.Cube.HexToWorld.x11 * hex.x + Matrix.Cube.HexToWorld.x12 * hex.y) * size;
            var z = (Matrix.Cube.HexToWorld.x21 * hex.x + Matrix.Cube.HexToWorld.x22 * hex.y) * size;
            return new Vector3(x, 0f, z);
        }

        /// <summary>
        /// Convert position in world space coordinates to hex cube coordinates
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector3 HexCubeFractionalToPoint(Vector3 point, float size = 1f)
        {
            var x = (Matrix.Cube.Fractional.x11 * point.x + Matrix.Cube.Fractional.x12 * point.z) / size;
            var y = (Matrix.Cube.Fractional.x21 * point.x + Matrix.Cube.Fractional.x22 * point.z) / size;
            return new Vector3(x, y, -x - y);
        }

        /// <summary>
        /// Convert position in world space coordinates to hex cube integer coordinates
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector3Int WorldToHexCube(Vector3 point, float size = 1f)
        {
            return HexCubeNearest(HexCubeFractionalToPoint(point, size));
        }

        /// <summary>
        /// Center of nearest hex
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector3 Center(Vector3 position, float size = 1f)
        {
            return HexCubeToPoint(WorldToHexCube(position, size), size);
        }

        /// <summary>
        /// Positions of hexes in area around origin at max range with side size = 1
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="range"></param>
        /// <returns>hex cube integer coordinates</returns>
        public static List<Vector3Int> Area(Vector3Int origin, float range)
        {
            var area = new List<Vector3Int>();
            var rangeBorder = Mathf.FloorToInt(range);
            for (int x = -rangeBorder; x <= range; x++)
            {
                var yMin = Mathf.Max(-rangeBorder, -x - rangeBorder);
                var yMax = Mathf.Min(rangeBorder, -x + rangeBorder);
                for (int y = yMin; y <= yMax; y++)
                {
                    area.Add(new Vector3Int(x, y, -x - y));
                }
            }
            return area;
        }

        #endregion //Cube Coordinates
    }
}

