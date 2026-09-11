using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrips
{
    public class EndScreenUI : MonoBehaviour
    {
        public void GoToMainMenu()
        {
            SceneManager.LoadScene("Main Menu");
        }
    
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}