using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrips
{
    public class DeathScreenManager : MonoBehaviour
    {
        [Header("Scene Names or Indexes")]
        [SerializeField] private string mainMenuSceneName = "Main Menu";

        public static string lastLevelName;

        // Ensure this method is marked public static
        public static void RecordCurrentLevel()
        {
            lastLevelName = SceneManager.GetActiveScene().name;
        }

        public void ReplayLevel()
        {
            if (!string.IsNullOrEmpty(lastLevelName))
            {
                SceneManager.LoadScene(lastLevelName);
            }
            else
            {
                Debug.LogWarning("No last level saved!");
            }
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}