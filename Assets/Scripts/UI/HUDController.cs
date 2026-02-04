using Bobo.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Bobo.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private GameController gameController;
        [SerializeField] private Text tickLabel;
        [SerializeField] private Text populationLabel;
        [SerializeField] private Text timeLabel;
        [SerializeField] private Text cameraLabel;
        [SerializeField] private Bobo.Rendering.CameraController cameraController;

        private SimulationCore _simulation;

        private void Start()
        {
            if (gameController != null)
            {
                _simulation = gameController.Simulation;
            }
        }

        private void Update()
        {
            if (_simulation == null)
            {
                return;
            }

            int ticks = _simulation.TickCount;
            if (tickLabel != null)
            {
                tickLabel.text = $"Ticks: {ticks}";
            }

            if (populationLabel != null)
            {
                populationLabel.text = $"Population: {_simulation.Population}";
            }

            if (timeLabel != null)
            {
                int days = ticks;
                int weeks = days / 7;
                int months = days / 30;
                int years = days / 365;
                timeLabel.text = $"Tag {days} (Woche {weeks}, Monat {months}, Jahr {years})";
            }

            if (cameraLabel != null && cameraController != null)
            {
                cameraLabel.text = $"Zoom {cameraController.Zoom:F1} | Cam {cameraController.WorldCenter.x:F1},{cameraController.WorldCenter.y:F1}";
            }
        }
    }
}
