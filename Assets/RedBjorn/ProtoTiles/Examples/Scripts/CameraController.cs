using UnityEngine;

namespace RedBjorn.ProtoTiles.Example
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] float Eps = 0.1f;

        Vector3? HoldPosition;
        Vector3? ClickPosition;

        public static bool IsMovingByPlayer;

        void LateUpdate()
        {
            if (MyInput.GetOnWorldDownFree)
            {
                HoldPosition = MyInput.GroundPositionCameraOffset;
                ClickPosition = transform.position;
            }
            else if (MyInput.GetOnWorldUpFree)
            {
                HoldPosition = null;
                ClickPosition = null;
            }
            UpdatePosition();
        }

        void OnDisable()
        {
            IsMovingByPlayer = false;
        }

        void UpdatePosition()
        {
            if (HoldPosition.HasValue)
            {
                var delta = HoldPosition.Value - MyInput.GroundPositionCameraOffset;
                transform.position += delta;
                transform.position = ClickPosition.Value + delta;
                if (!IsMovingByPlayer)
                {
                    IsMovingByPlayer = delta.sqrMagnitude > Eps;
                }
            }
            else
            {
                IsMovingByPlayer = false;
            }
        }
    }
}
