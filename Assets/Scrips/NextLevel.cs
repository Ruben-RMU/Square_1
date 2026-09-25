using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";

    [Tooltip("Set to 0 to automatically use the current scene's build index.")]
    [SerializeField] private int currentLevelNumber = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Use manual level number if assigned > 0, otherwise use build index
        int activeLevel = currentLevelNumber > 0
            ? currentLevelNumber
            : currentSceneIndex;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel(activeLevel);
        }

        int nextSceneIndex = currentSceneIndex + 1;

        // Make sure there actually is another scene
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Remember which level we want to load
            LoadingScreen.nextSceneIndex = nextSceneIndex;

            // Second-to-last scene is the loading screen
            int loadingSceneIndex = SceneManager.sceneCountInBuildSettings - 2;

            SceneManager.LoadScene(loadingSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}