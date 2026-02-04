using Bobo.Simulation;
using UnityEngine;

namespace Bobo.UI
{
    public sealed class BrushPainter : MonoBehaviour
    {
        [SerializeField] private GameController gameController;
        [SerializeField] private Bobo.Rendering.CameraController cameraController;
        [SerializeField] private float brushRadius = 2f;

        private void Update()
        {
            if (gameController == null || cameraController == null)
            {
                return;
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 worldPos = ScreenToWorld(Input.mousePosition);
                Paint(worldPos, true);
            }
            else if (Input.GetMouseButton(1))
            {
                Vector2 worldPos = ScreenToWorld(Input.mousePosition);
                Paint(worldPos, false);
            }
        }

        private Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            Vector2 viewport = new Vector2(Screen.width, Screen.height);
            Vector2 normalized = screenPosition / viewport;
            float cellsPerPixel = Mathf.Max(1f, cameraController.Zoom);
            Vector2 center = cameraController.WorldCenter;
            Vector2 offset = (normalized - new Vector2(0.5f, 0.5f)) * new Vector2(Screen.width, Screen.height) * cellsPerPixel;
            return center + offset;
        }

        private void Paint(Vector2 worldPosition, bool alive)
        {
            SimulationCore simulation = gameController.Simulation;
            int radius = Mathf.RoundToInt(brushRadius);
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y > radius * radius)
                    {
                        continue;
                    }

                    int worldX = Mathf.RoundToInt(worldPosition.x) + x;
                    int worldY = Mathf.RoundToInt(worldPosition.y) + y;
                    if (alive)
                    {
                        simulation.SetCellAlive(worldX, worldY, 1.5f, 0);
                    }
                    else
                    {
                        simulation.SetCellDead(worldX, worldY);
                    }
                }
            }
        }
    }
}
