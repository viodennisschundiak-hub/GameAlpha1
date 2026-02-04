using System.Collections.Generic;
using UnityEngine;

namespace Bobo.Achievements
{
    public static class AchievementDefaults
    {
        public static IEnumerable<AchievementDefinition> CreateDefaults()
        {
            return new List<AchievementDefinition>
            {
                Create("day_1", "Bergkristall-Medaille", 1),
                Create("week_1", "Edelaragonit-Medaille", 7),
                Create("month_1", "Achat-Medaille", 30),
                Create("year_1", "Chrysokoll-Medaille", 365),
                Create("year_2", "Fluoridgrün-Medaille", 730),
                Create("year_3", "Fluoridblau-Medaille", 1095),
                Create("year_4", "Bronzemedaille", 1460),
                Create("year_8", "Silbermedaille", 2920),
                Create("year_10", "Goldmedaille", 3650)
            };
        }

        private static AchievementDefinition Create(string id, string medalName, int days)
        {
            AchievementDefinition achievement = ScriptableObject.CreateInstance<AchievementDefinition>();
            achievement.id = id;
            achievement.displayName = medalName;
            achievement.medalName = medalName;
            achievement.description = $"Erreiche {days} Tage.";
            achievement.daysRequired = days;
            return achievement;
        }
    }
}
