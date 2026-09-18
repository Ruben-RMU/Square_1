using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public int level;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        if (GameManager.Instance != null && GameManager.Instance.Data != null)
        {
            bool isUnlocked = level <= GameManager.Instance.Data.highestLevelUnlocked;

            // Gray out and disable UI button if locked
            if (button != null)
            {
                button.interactable = isUnlocked;
            }
        }
    }

    public void OpenScene()
    {
        // Guard check to prevent loading locked levels
        if (GameManager.Instance != null && GameManager.Instance.Data != null)
        {
            if (level > GameManager.Instance.Data.highestLevelUnlocked)
            {
                Debug.LogWarning($"Level {level} is locked!");
                return;
            }
        }

        SceneManager.LoadScene("Level " + level.ToString());
    }
}