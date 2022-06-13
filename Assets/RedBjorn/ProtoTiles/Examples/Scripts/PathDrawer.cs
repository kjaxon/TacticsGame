using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.ProtoTiles.Example
{
    public class PathDrawer : MonoBehaviour
    {
        public LineDrawer Line;
        public SpriteRenderer Tail;
        public Color ActiveColor;
        public Color InactiveColor;

        public bool IsEnabled { get; set; }

        public void ActiveState()
        {
            SetColor(ActiveColor);
        }

        public void InactiveState()
        {
            SetColor(InactiveColor);
        }

        void SetColor(Color color)
        {
            Line.Line.material.color = color;
            Tail.color = color;
        }

        public void Show(List<Vector3> points)
        {
            if (points == null || points.Count == 0)
            {
                Hide();
            }
            else
            {
                var tailPos = points[points.Count - 1];
                Tail.transform.localPosition = tailPos;
                Tail.gameObject.SetActive(true);
                if (points.Count > 1)
                {
                    points[points.Count - 1] = (points[points.Count - 1] + points[points.Count - 2]) / 2f;
                    Line.Show(points);
                }
            }
        }

        public void Hide()
        {
            Line.Hide();
            Tail.gameObject.SetActive(false);
        }
    }
}