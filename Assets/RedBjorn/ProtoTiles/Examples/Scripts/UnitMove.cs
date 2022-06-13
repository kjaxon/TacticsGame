using RedBjorn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.ProtoTiles.Example
{
    public class UnitMove : MonoBehaviour
    {
        public float Speed = 5;
        public float Range = 10f;
        public Transform RotationNode;
        public AreaOutline AreaPrefab;
        public PathDrawer PathPrefab;

        MapEntity Map;
        AreaOutline Area;
        PathDrawer Path;
        Coroutine MovingCoroutine;

        void Update()
        {
            if (MyInput.GetOnWorldUp)
            {
                HandleWorldClick();
            }
            PathUpdate();
        }

        public void Init(MapEntity map)
        {
            Map = map;
            Area = Spawner.Spawn(AreaPrefab, Vector3.zero, Quaternion.identity);
            AreaShow();
            PathCreate();
        }

        void HandleWorldClick()
        {
            var clickPos = MyInput.GroundPosition;
            var tile = Map.Tile(clickPos);
            if (tile != null && tile.Vacant)
            {
                AreaHide();
                Path.IsEnabled = false;
                PathHide();
                var path = Map.PathTiles(transform.position, clickPos, Range);
                Move(path, () =>
                {
                    Path.IsEnabled = true;
                    AreaShow();
                });
            }
        }

        public void Move(List<TileEntity> path, Action onCompleted)
        {
            if (path != null)
            {
                if (MovingCoroutine != null)
                {
                    StopCoroutine(MovingCoroutine);
                }
                MovingCoroutine = StartCoroutine(Moving(path, onCompleted));
            }
            else
            {
                onCompleted.SafeInvoke();
            }
        }

        IEnumerator Moving(List<TileEntity> path, Action onCompleted)
        {
            var nextIndex = 0;
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

            while (nextIndex < path.Count)
            {
                var targetPoint = Map.WorldPosition(path[nextIndex]);
                var stepDir = (targetPoint - transform.position) * Speed;
                var reached = stepDir.sqrMagnitude < 0.01f;
                while (!reached)
                {
                    RotationNode.rotation = Quaternion.LookRotation(stepDir, Vector3.up);
                    transform.position += stepDir * Time.deltaTime;
                    reached = Vector3.Dot(stepDir, (targetPoint - transform.position)) < 0f;
                    yield return null;
                }
                transform.position = targetPoint;
                nextIndex++;
            }
            onCompleted.SafeInvoke();
        }

        void AreaShow()
        {
            AreaHide();
            Area.Show(Map.WalkableBorder(transform.position, Range));
        }

        void AreaHide()
        {
            Area.Hide();
        }

        void PathCreate()
        {
            if (!Path)
            {
                Path = Spawner.Spawn(PathPrefab, Vector3.zero, Quaternion.identity);
                Path.Show(new List<Vector3>() { });
                Path.InactiveState();
                Path.IsEnabled = true;
            }
        }

        void PathHide()
        {
            if (Path)
            {
                Path.Hide();
            }
        }

        void PathUpdate()
        {
            if (Path && Path.IsEnabled)
            {
                var tile = Map.Tile(MyInput.GroundPosition);
                if (tile != null && tile.Vacant)
                {
                    var path = Map.PathPoints(transform.position, Map.WorldPosition(tile.Position), Range);
                    Path.Show(path);
                    Path.ActiveState();
                    Area.ActiveState();
                }
                else
                {
                    Path.InactiveState();
                    Area.InactiveState();
                }
            }
        }
    }
}
