using System.Collections.Generic;
using Bobo.Simulation;
using UnityEngine;

namespace Bobo.Achievements
{
    public sealed class AchievementManager : MonoBehaviour
    {
        [SerializeField] private AchievementsDatabase database;
        [SerializeField] private AchievementPopup popup;

        private SimulationCore _simulation;
        private readonly HashSet<string> _unlocked = new();

        public IReadOnlyCollection<string> Unlocked => _unlocked;

        public void Initialize(SimulationCore simulation)
        {
            _simulation = simulation;
            EnsureDatabase();
            if (_simulation != null)
            {
                _simulation.OnWorldChanged += CheckAchievements;
            }
        }

        private void OnDestroy()
        {
            if (_simulation != null)
            {
                _simulation.OnWorldChanged -= CheckAchievements;
            }
        }

        public void LoadUnlocked(IEnumerable<string> ids)
        {
            _unlocked.Clear();
            foreach (string id in ids)
            {
                _unlocked.Add(id);
            }
        }

        public List<string> GetUnlockedIds() => new List<string>(_unlocked);

        private void CheckAchievements()
        {
            if (_simulation == null || database == null)
            {
                return;
            }

            int days = _simulation.TickCount;
            foreach (AchievementDefinition achievement in database.achievements)
            {
                if (_unlocked.Contains(achievement.id))
                {
                    continue;
                }

                if (days >= achievement.daysRequired)
                {
                    _unlocked.Add(achievement.id);
                    popup?.Show(achievement, days);
                }
            }
        }

        private void EnsureDatabase()
        {
            if (database != null && database.achievements.Count > 0)
            {
                return;
            }

            database = ScriptableObject.CreateInstance<AchievementsDatabase>();
            List<AchievementDefinition> loaded = AchievementCatalogLoader.LoadFromResources(\"achievements\");
            if (loaded.Count == 0)
            {
                loaded.AddRange(AchievementDefaults.CreateDefaults());
            }

            database.achievements.AddRange(loaded);
        }
    }
}
