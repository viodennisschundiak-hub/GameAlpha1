using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bobo.Achievements
{
    [Serializable]
    public class AchievementCatalog
    {
        public List<AchievementCatalogEntry> achievements = new();
    }

    [Serializable]
    public class AchievementCatalogEntry
    {
        public string id;
        public string displayName;
        public string medalName;
        public string description;
        public int daysRequired;
    }

    public static class AchievementCatalogLoader
    {
        public static List<AchievementDefinition> LoadFromResources(string resourcePath)
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                return new List<AchievementDefinition>();
            }

            AchievementCatalog catalog = JsonUtility.FromJson<AchievementCatalog>(asset.text);
            List<AchievementDefinition> definitions = new List<AchievementDefinition>();
            foreach (AchievementCatalogEntry entry in catalog.achievements)
            {
                AchievementDefinition achievement = ScriptableObject.CreateInstance<AchievementDefinition>();
                achievement.id = entry.id;
                achievement.displayName = entry.displayName;
                achievement.medalName = entry.medalName;
                achievement.description = entry.description;
                achievement.daysRequired = entry.daysRequired;
                definitions.Add(achievement);
            }

            return definitions;
        }
    }
}
