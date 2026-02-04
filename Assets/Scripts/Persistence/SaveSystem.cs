using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bobo.Achievements;
using Bobo.Data;
using Bobo.Simulation;
using UnityEngine;

namespace Bobo.Persistence
{
    public sealed class SaveSystem : MonoBehaviour
    {
        [SerializeField] private SimulationSettings settings;
        [SerializeField] private AchievementManager achievementManager;

        private SimulationCore _simulation;
        private string _currentSaveName = "autosave";

        public void Initialize(SimulationCore simulation)
        {
            _simulation = simulation;
        }

        public void SaveCurrent()
        {
            SaveAs(_currentSaveName);
        }

        public void SaveAs(string newName)
        {
            if (_simulation == null)
            {
                return;
            }

            _currentSaveName = string.IsNullOrWhiteSpace(newName) ? "autosave" : newName;
            string directory = GetSaveDirectory();
            Directory.CreateDirectory(directory);
            string jsonPath = Path.Combine(directory, $"{_currentSaveName}.json");
            string binPath = Path.Combine(directory, $"{_currentSaveName}.bin");

            SaveMetadata metadata = new SaveMetadata
            {
                worldName = WorldCreationData.WorldName,
                seed = WorldCreationData.Seed,
                tick = _simulation.TickCount,
                population = _simulation.Population,
                saveDate = System.DateTime.UtcNow.ToString("u"),
                settings = new SimulationSettingsData
                {
                    gainFree = settings.gainFree,
                    costAlive = settings.costAlive,
                    costCrowd = settings.costCrowd,
                    reproduceThreshold = settings.reproduceThreshold,
                    freeThreshold = settings.freeThreshold,
                    newbornEnergy = settings.newbornEnergy,
                    moveStressThreshold = settings.moveStressThreshold,
                    enableMovement = settings.enableMovement
                },
                unlockedAchievements = achievementManager != null ? achievementManager.GetUnlockedIds() : new List<string>(),
                chunkDataFile = Path.GetFileName(binPath)
            };

            string json = JsonUtility.ToJson(metadata, true);
            File.WriteAllText(jsonPath, json, Encoding.UTF8);
            SaveChunks(binPath);
        }

        public void OpenLoadMenu()
        {
            List<SaveSlotInfo> saves = ListSaves();
            Debug.Log($"Found {saves.Count} saves.");
        }

        public void Load(string saveName)
        {
            string directory = GetSaveDirectory();
            string jsonPath = Path.Combine(directory, $"{saveName}.json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogWarning("Save not found");
                return;
            }

            string json = File.ReadAllText(jsonPath, Encoding.UTF8);
            SaveMetadata metadata = JsonUtility.FromJson<SaveMetadata>(json);
            if (metadata.saveVersion != 1)
            {
                Debug.LogError("Incompatible save version");
                return;
            }

            ApplySettings(metadata);
            LoadChunks(Path.Combine(directory, metadata.chunkDataFile));
            if (achievementManager != null)
            {
                achievementManager.LoadUnlocked(metadata.unlockedAchievements);
            }
        }

        public List<SaveSlotInfo> ListSaves()
        {
            string directory = GetSaveDirectory();
            if (!Directory.Exists(directory))
            {
                return new List<SaveSlotInfo>();
            }

            return Directory.GetFiles(directory, "*.json")
                .Select(path =>
                {
                    string json = File.ReadAllText(path, Encoding.UTF8);
                    SaveMetadata metadata = JsonUtility.FromJson<SaveMetadata>(json);
                    return new SaveSlotInfo
                    {
                        worldName = metadata.worldName,
                        fileName = Path.GetFileNameWithoutExtension(path),
                        saveDate = metadata.saveDate,
                        tick = metadata.tick,
                        population = metadata.population
                    };
                })
                .ToList();
        }

        private void ApplySettings(SaveMetadata metadata)
        {
            settings.gainFree = metadata.settings.gainFree;
            settings.costAlive = metadata.settings.costAlive;
            settings.costCrowd = metadata.settings.costCrowd;
            settings.reproduceThreshold = metadata.settings.reproduceThreshold;
            settings.freeThreshold = metadata.settings.freeThreshold;
            settings.newbornEnergy = metadata.settings.newbornEnergy;
            settings.moveStressThreshold = metadata.settings.moveStressThreshold;
            settings.enableMovement = metadata.settings.enableMovement;
        }

        private void SaveChunks(string binPath)
        {
            SparseWorld world = _simulation.World;
            using FileStream stream = new FileStream(binPath, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(world.Chunks.Count);
            foreach (KeyValuePair<ChunkCoord, Chunk> pair in world.Chunks)
            {
                ChunkCoord coord = pair.Key;
                Chunk chunk = pair.Value;
                writer.Write(coord.X);
                writer.Write(coord.Y);

                List<int> liveIndices = new List<int>();
                for (int i = 0; i < chunk.Alive.Length; i++)
                {
                    if (chunk.Alive[i] == 1)
                    {
                        liveIndices.Add(i);
                    }
                }

                writer.Write(liveIndices.Count);
                foreach (int index in liveIndices)
                {
                    writer.Write(index);
                    writer.Write(chunk.Energy[index]);
                    writer.Write(chunk.Age[index]);
                }
            }
        }

        private void LoadChunks(string binPath)
        {
            if (_simulation == null)
            {
                return;
            }

            _simulation.Reset();
            if (!File.Exists(binPath))
            {
                return;
            }

            using FileStream stream = new FileStream(binPath, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new BinaryReader(stream);

            int chunkCount = reader.ReadInt32();
            for (int c = 0; c < chunkCount; c++)
            {
                int chunkX = reader.ReadInt32();
                int chunkY = reader.ReadInt32();
                int liveCount = reader.ReadInt32();

                for (int i = 0; i < liveCount; i++)
                {
                    int index = reader.ReadInt32();
                    float energy = reader.ReadSingle();
                    int age = reader.ReadInt32();

                    int localX = index % settings.chunkSize;
                    int localY = index / settings.chunkSize;
                    int worldX = chunkX * settings.chunkSize + localX;
                    int worldY = chunkY * settings.chunkSize + localY;
                    _simulation.SetCellAlive(worldX, worldY, energy, age);
                }
            }
        }

        private static string GetSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, "BoboSaves");
        }
    }
}
