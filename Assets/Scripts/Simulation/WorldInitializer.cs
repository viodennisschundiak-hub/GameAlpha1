using Bobo.Data;
using UnityEngine;

namespace Bobo.Simulation
{
    public sealed class WorldInitializer : MonoBehaviour
    {
        [SerializeField] private Bobo.UI.GameController gameController;
        [SerializeField] private int randomSpread = 64;

        private void Start()
        {
            if (gameController == null)
            {
                return;
            }

            SimulationCore simulation = gameController.Simulation;
            if (simulation == null)
            {
                return;
            }

            string preset = WorldCreationData.Preset;
            if (preset == "Glider")
            {
                foreach (Vector2Int offset in Presets.Glider)
                {
                    simulation.SetCellAlive(offset.x, offset.y, WorldCreationData.Settings != null ? WorldCreationData.Settings.newbornEnergy : 1.5f, 0);
                }
            }
            else if (preset == "Blinker")
            {
                foreach (Vector2Int offset in Presets.Blinker)
                {
                    simulation.SetCellAlive(offset.x, offset.y, WorldCreationData.Settings != null ? WorldCreationData.Settings.newbornEnergy : 1.5f, 0);
                }
            }
            else if (preset == "Random")
            {
                int count = Mathf.RoundToInt(1000 * WorldCreationData.StartDensity);
                foreach (Vector2Int offset in Presets.Random(WorldCreationData.Seed, count, randomSpread))
                {
                    simulation.SetCellAlive(offset.x, offset.y, WorldCreationData.Settings != null ? WorldCreationData.Settings.newbornEnergy : 1.5f, 0);
                }
            }
        }
    }
}
