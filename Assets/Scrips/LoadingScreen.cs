using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public static int nextSceneIndex;

    private void Start()
    {
        StartCoroutine(LoadNextLevel());
    }

    private IEnumerator LoadNextLevel()
    {
        // Keep the loading screen visible for 1.5 seconds
        yield return new WaitForSeconds(1.5f);

        // Load the level that NextLevel told us to load
        SceneManager.LoadScene(nextSceneIndex);
    }
}