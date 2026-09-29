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
        // 1. Keep the loading screen visible for 1.5 seconds 
        yield return new WaitForSeconds(1.5f);

        // 2. Trigger GameManager to fade out to black before switching to the next scene
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}