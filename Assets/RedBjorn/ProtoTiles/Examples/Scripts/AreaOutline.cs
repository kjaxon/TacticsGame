using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.ProtoTiles.Example
{
    public class AreaOutline : MonoBehaviour
    {
        public LineDrawer Line;
        public Color ActiveColor;
        public Color InactiveColor;

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
        }

        public void Show(List<Vector3> points)
        {
            Line.Show(points);
        }

        public void Hide()
        {
            Line.Hide();
        }
    }
}
