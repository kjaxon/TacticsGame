using UnityEngine;

namespace RedBjorn.ProtoTiles.Example
{
    public class ExampleStart : MonoBehaviour
    {
        public MapSettings Map;
        public KeyCode GridToggle = KeyCode.G;
        public MapView MapView;
        public UnitMove Unit;
        
        MapEntity MapEntity;

        void Start()
        {
            if (!MapView)
            {
                MapView = GameObject.FindObjectOfType<MapView>();
            }
            MapEntity = Map.CreateEntity(MapView);
            if (MapView)
            {
                MapView.Init(MapEntity);
            }
            else
            {
                Debug.LogError("Can't find MapView. Random errors can occur");
            }

            if (!Unit)
            {
                Unit = GameObject.FindObjectOfType<UnitMove>();
            }
            if (Unit)
            {
                Unit.Init(MapEntity);
            }
            else
            {
                Debug.LogError("Can't find any Unit. Example level start incorrect");
            }
        }

        void Update()
        {
            if (Input.GetKeyUp(GridToggle))
            {
                MapEntity.GridToggle();
            }
        }
    }
}
