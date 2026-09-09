using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrips
{
    public class DeathScreenManager : MonoBehaviour
    {
        [Header("Scene Names or Indexes")]
        [SerializeField] private string mainMenuSceneName = "Main Menu";
        
        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
