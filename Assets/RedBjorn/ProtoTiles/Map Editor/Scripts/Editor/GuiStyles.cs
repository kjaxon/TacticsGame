using UnityEditor;
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    public class GuiStyles
    {
        public readonly static GUIStyle HorizontalLine = new GUIStyle
        {
            normal = new GUIStyleState() { background = EditorGUIUtility.whiteTexture },
            fixedHeight = 4f,
            margin = new RectOffset(0, 0, 8, 8)
        };

        public readonly static GUIStyle CenterAligment = new GUIStyle
        {
            alignment = TextAnchor.UpperCenter
        };

        public static void DrawHorizontal(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, HorizontalLine);
            GUI.color = c;
        }

        public static void DrawHex(Vector3 center, Color color, float edge)
        {
            var c = Handles.color;
            Handles.color = color;
            var v1 = edge * Hex.Vertices[0] + center;
            var v2 = edge * Hex.Vertices[1] + center;
            var v3 = edge * Hex.Vertices[2] + center;
            var v4 = edge * Hex.Vertices[3] + center;
            var v5 = edge * Hex.Vertices[4] + center;
            var v6 = edge * Hex.Vertices[5] + center;
            Handles.DrawAAConvexPolygon(v1, v2, v3, v4, v5, v6);
            Handles.color = c;
        }

        public static void DrawSquare(Vector3 center, Color color, float edge)
        {
            var c = Handles.color;
            Handles.color = color;
            var v1 = edge * Square.Vertices[0] + center;
            var v2 = edge * Square.Vertices[1] + center;
            var v3 = edge * Square.Vertices[2] + center;
            var v4 = edge * Square.Vertices[3] + center;
            Handles.DrawAAConvexPolygon(v1, v2, v3, v4);
            Handles.color = c;
        }

        public static void DrawX(Vector3 center, Color color, float edge)
        {
            var c = Handles.color;
            Handles.color = color;
            var half = edge / 2f;
            var v1 = edge * new Vector3(-half, 0f, half) + center;
            var v2 = edge * new Vector3(half, 0f, -half) + center;
            var v3 = edge * new Vector3(half, 0f, half) + center;
            var v4 = edge * new Vector3(-half, 0f, -half) + center;
            Handles.DrawLine(v1, v2);
            Handles.DrawLine(v3, v4);
            Handles.color = c;
        }

        public static void DrawCircle(Vector3 center, Color color, float radius)
        {
            var c = Handles.color;
            Handles.color = color;
            Handles.DrawSolidArc(center, Vector3.up, Vector3.forward, 360f, radius);
            Handles.color = c;
        }

        public static void DrawRect(Vector3 center, Color color, float angle, float width, float height)
        {
            var c = Handles.color;
            Handles.color = color;
            var v1 = Quaternion.Euler(0f, angle, 0f) * new Vector3(-width / 2f, 0f, height / 2f) + center;
            var v2 = Quaternion.Euler(0f, angle, 0f) * new Vector3(width / 2f, 0f, height / 2f) + center;
            var v3 = Quaternion.Euler(0f, angle, 0f) * new Vector3(width / 2f, 0f, -height / 2f) + center;
            var v4 = Quaternion.Euler(0f, angle, 0f) * new Vector3(-width / 2f, 0f, -height / 2f) + center;
            Handles.DrawAAConvexPolygon(v1, v2, v3, v4);
            Handles.color = c;
        }

        public static void DrawLabel(string text, Vector3 center, Color color)
        {
            var c = Handles.color;
            Handles.color = color;
            Handles.Label(center, text);
            Handles.color = c;
        }
    }
}