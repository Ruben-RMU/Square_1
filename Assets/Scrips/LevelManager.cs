using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void ResetLevel()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Unpause time if it was paused
        SceneManager.LoadScene("Main Menu");
    }
}