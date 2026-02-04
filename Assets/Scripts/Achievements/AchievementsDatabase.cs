using System.Collections.Generic;
using UnityEngine;

namespace Bobo.Achievements
{
    [CreateAssetMenu(menuName = "Bobo Simulator/Achievements Database")]
    public class AchievementsDatabase : ScriptableObject
    {
        public List<AchievementDefinition> achievements = new();
    }
}
