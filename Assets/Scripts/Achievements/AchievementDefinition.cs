using UnityEngine;

namespace Bobo.Achievements
{
    [CreateAssetMenu(menuName = "Bobo Simulator/Achievement Definition")]
    public class AchievementDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string medalName;
        [TextArea] public string description;
        public int daysRequired;
    }
}
