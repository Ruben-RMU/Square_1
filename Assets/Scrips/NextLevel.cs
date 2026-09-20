using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float delaySeconds = 5f;

    // Static variable persists across scenes to remember the destination level
    private static int nextLevelIndex = 0;

    private void Start()
    {
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int loadingSceneIndex = totalScenes - 2;

        // If we are currently on the Loading Screen, wait 5 seconds then load the stored next level
        if (totalScenes >= 2 && currentSceneIndex == loadingSceneIndex)
        {
            StartCoroutine(WaitAndLoadNextLevel());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            GoToLoadingScreen();
        }
    }

    private void GoToLoadingScreen()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        int loadingSceneIndex = totalScenes - 2;

        // Calculate the next level after the current scene
        nextLevelIndex = currentSceneIndex + 1;

        // Loop back to index 0 if the next level reaches the loading screen or goes out of bounds
        if (nextLevelIndex >= loadingSceneIndex)
        {
            nextLevelIndex = 0;
        }

        // Load the Loading Screen scene (second-to-last in Build Settings)
        SceneManager.LoadScene(loadingSceneIndex);
    }

    private IEnumerator WaitAndLoadNextLevel()
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(nextLevelIndex);
    }
}