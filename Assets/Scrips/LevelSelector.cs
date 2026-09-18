using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public int level;
    
    [Header("UI Element References")]
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject levelText;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        // Auto-assign references if not set in Inspector
        if (lockIcon == null && transform.Find("lock") != null)
        {
            lockIcon = transform.Find("lock").gameObject;
        }

        if (levelText == null && transform.Find("Text (TMP)") != null)
        {
            levelText = transform.Find("Text (TMP)").gameObject;
        }

        if (GameManager.Instance != null && GameManager.Instance.Data != null)
        {
            bool isUnlocked = level <= GameManager.Instance.Data.highestLevelUnlocked;

            // Disable button interaction if locked
            if (button != null)
            {
                button.interactable = isUnlocked;
            }

            // Show text and hide lock if unlocked; hide text and show lock if locked
            if (lockIcon != null)
            {
                lockIcon.SetActive(!isUnlocked);
            }

            if (levelText != null)
            {
                levelText.SetActive(isUnlocked);
            }
        }
    }

    public void OpenScene()
    {
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