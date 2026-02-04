using Bobo.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bobo.UI
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private SaveSystem saveSystem;

        private bool _isOpen;

        public void Toggle()
        {
            _isOpen = !_isOpen;
            if (panel != null)
            {
                panel.SetActive(_isOpen);
            }

            Time.timeScale = _isOpen ? 0f : 1f;
        }

        public void Save()
        {
            saveSystem?.SaveCurrent();
        }

        public void SaveAs(string newName)
        {
            saveSystem?.SaveAs(newName);
        }

        public void Load()
        {
            saveSystem?.OpenLoadMenu();
        }

        public void BackToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void Options()
        {
            SceneManager.LoadScene("Options");
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
