using UnityEngine;

namespace Bobo.Rendering
{
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private float panSpeed = 50f;
        [SerializeField] private float zoomSpeed = 4f;
        [SerializeField] private float minZoom = 1f;
        [SerializeField] private float maxZoom = 50f;

        public Vector2 WorldCenter { get; private set; }
        public float Zoom { get; private set; } = 8f;

        public void SetCenter(Vector2 center)
        {
            WorldCenter = center;
        }

        public void SetZoom(float zoom)
        {
            Zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }

        private void Update()
        {
            Vector2 move = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
            {
                move.y += 1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                move.y -= 1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                move.x -= 1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                move.x += 1f;
            }

            if (move != Vector2.zero)
            {
                WorldCenter += move.normalized * panSpeed * Time.deltaTime / Zoom;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                SetZoom(Zoom + scroll * zoomSpeed);
            }
        }
    }
}
