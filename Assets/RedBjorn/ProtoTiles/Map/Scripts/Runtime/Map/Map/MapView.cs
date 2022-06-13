using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    public class MapView : MonoBehaviour
    {
        GameObject Grid;

        public void Init(MapEntity map)
        {
            Grid = new GameObject("Grid");
            Grid.transform.SetParent(transform);
            Grid.transform.localPosition = Vector3.zero;
            map.CreateGrid(Grid.transform);
        }

        public void GridEnable(bool enable)
        {
            if (Grid)
            {
                Grid.SetActive(enable);
            }
            else
            {
                Debug.LogErrorFormat("Can't enable Grid state: {0}. It wasn't created", enable);
            }
        }

        public void GridToggle()
        {
            if (Grid)
            {
                Grid.SetActive(!Grid.activeSelf);
            }
            else
            {
                Debug.LogError("Can't toggle Grid state. It wasn't created");
            }
        }
    }
}