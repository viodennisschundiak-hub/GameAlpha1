using Bobo.Simulation;
using UnityEngine;

namespace Bobo.Rendering
{
    [RequireComponent(typeof(CameraController))]
    public sealed class WorldRenderer : MonoBehaviour
    {
        [SerializeField] private SimulationSettings settings;
        [SerializeField] private int viewportWidth = 1080;
        [SerializeField] private int viewportHeight = 720;
        [SerializeField] private Color aliveColor = new Color(0.2f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color deadColor = new Color(0.02f, 0.02f, 0.04f, 1f);
        [SerializeField] private Renderer targetRenderer;

        private Texture2D _texture;
        private SimulationCore _simulation;
        private CameraController _cameraController;
        private Color[] _pixels;

        public void Initialize(SimulationCore simulation)
        {
            _simulation = simulation;
            _cameraController = GetComponent<CameraController>();
            _texture = new Texture2D(viewportWidth, viewportHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            _pixels = new Color[viewportWidth * viewportHeight];
            if (targetRenderer != null)
            {
                targetRenderer.material.mainTexture = _texture;
            }

            _simulation.OnWorldChanged += Render;
            Render();
        }

        private void OnDestroy()
        {
            if (_simulation != null)
            {
                _simulation.OnWorldChanged -= Render;
            }
        }

        public void Render()
        {
            if (_simulation == null || _pixels == null)
            {
                return;
            }

            float zoom = _cameraController != null ? _cameraController.Zoom : 8f;
            Vector2 center = _cameraController != null ? _cameraController.WorldCenter : Vector2.zero;
            float cellsPerPixel = Mathf.Max(1f, zoom);
            float halfWidth = viewportWidth * 0.5f * cellsPerPixel;
            float halfHeight = viewportHeight * 0.5f * cellsPerPixel;
            int startX = Mathf.FloorToInt(center.x - halfWidth);
            int startY = Mathf.FloorToInt(center.y - halfHeight);

            for (int y = 0; y < viewportHeight; y++)
            {
                for (int x = 0; x < viewportWidth; x++)
                {
                    int worldX = startX + Mathf.FloorToInt(x * cellsPerPixel);
                    int worldY = startY + Mathf.FloorToInt(y * cellsPerPixel);
                    float intensity = SampleDensity(worldX, worldY, cellsPerPixel);
                    _pixels[x + y * viewportWidth] = Color.Lerp(deadColor, aliveColor, intensity);
                }
            }

            _texture.SetPixels(_pixels);
            _texture.Apply();
        }

        private float SampleDensity(int worldX, int worldY, float cellsPerPixel)
        {
            if (cellsPerPixel <= 1f)
            {
                return _simulation.IsAlive(worldX, worldY) ? 1f : 0f;
            }

            int sampleSize = Mathf.Clamp(Mathf.RoundToInt(cellsPerPixel), 1, 32);
            int liveCount = 0;
            int total = sampleSize * sampleSize;
            for (int y = 0; y < sampleSize; y++)
            {
                for (int x = 0; x < sampleSize; x++)
                {
                    if (_simulation.IsAlive(worldX + x, worldY + y))
                    {
                        liveCount++;
                    }
                }
            }

            return liveCount / (float)total;
        }
    }
}
