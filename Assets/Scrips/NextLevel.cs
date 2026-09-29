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

        int activeLevel = currentLevelNumber > 0
            ? currentLevelNumber
            : currentSceneIndex;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel(activeLevel);
        }

        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadingScreen.nextSceneIndex = nextSceneIndex;

            int loadingSceneIndex = SceneManager.sceneCountInBuildSettings - 2;

            // Fade out using GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene(loadingSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(loadingSceneIndex);
            }
        }
        else
        {
            // Fade out using GameManager back to index 0
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene(0);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}