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
        // Use manual level number if assigned > 0, otherwise fallback to buildIndex
        int activeLevel = currentLevelNumber > 0 ? currentLevelNumber : currentSceneIndex;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel(activeLevel);
        }

        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0); 
        }
    }
}