using UnityEngine;
using UnityEngine.UI;

namespace Bobo.Achievements
{
    public sealed class AchievementPopup : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text detailLabel;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip popupSound;

        public void Show(AchievementDefinition achievement, int days)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            if (titleLabel != null)
            {
                titleLabel.text = "Herzlichen Glückwunsch!";
            }

            if (detailLabel != null)
            {
                detailLabel.text = $"{achievement.medalName} – Tag {days}";
            }

            if (audioSource != null && popupSound != null)
            {
                audioSource.PlayOneShot(popupSound);
            }
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
