using System;
using System.Collections.Generic;

namespace Bobo.Persistence
{
    [Serializable]
    public class SaveMetadata
    {
        public int saveVersion = 1;
        public string worldName;
        public int seed;
        public int tick;
        public int population;
        public string saveDate;
        public SimulationSettingsData settings;
        public List<string> unlockedAchievements = new();
        public string chunkDataFile;
    }

    [Serializable]
    public class SimulationSettingsData
    {
        public float gainFree;
        public float costAlive;
        public float costCrowd;
        public float reproduceThreshold;
        public int freeThreshold;
        public float newbornEnergy;
        public int moveStressThreshold;
        public bool enableMovement;
    }

    [Serializable]
    public class SaveSlotInfo
    {
        public string worldName;
        public string fileName;
        public string saveDate;
        public int tick;
        public int population;
    }
}
