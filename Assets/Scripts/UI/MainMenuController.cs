using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bobo.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        public void NewWorld()
        {
            SceneManager.LoadScene("WorldCreator");
        }

        public void LoadWorld()
        {
            SceneManager.LoadScene("WorldLoad");
        }

        public void Options()
        {
            SceneManager.LoadScene("Options");
        }

        public void HowToPlay()
        {
            SceneManager.LoadScene("HowToPlay");
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
