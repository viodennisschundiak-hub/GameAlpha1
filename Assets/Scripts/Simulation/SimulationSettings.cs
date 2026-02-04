using UnityEngine;

namespace Bobo.Simulation
{
    [CreateAssetMenu(menuName = "Bobo Simulator/Simulation Settings")]
    public class SimulationSettings : ScriptableObject
    {
        [Header("World")]
        public int chunkSize = 256;
        public int worldSize = 100000;

        [Header("Simulation")]
        public float gainFree = 0.12f;
        public float costAlive = 0.2f;
        public float costCrowd = 0.05f;
        public float reproduceThreshold = 1.2f;
        public int freeThreshold = 3;
        public float newbornEnergy = 1.5f;
        public int moveStressThreshold = 5;
        public bool enableMovement = true;
    }
}
