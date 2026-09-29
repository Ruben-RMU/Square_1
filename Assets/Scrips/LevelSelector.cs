using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    [Header("Level Settings")]
    public int level;

    [Header("Background Sprites")]
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite currentLevelSprite;
    [SerializeField] private Sprite lockedSprite;

    [Header("UI Overlay Elements")]
    [SerializeField] private GameObject lockIcon;        
    [SerializeField] private GameObject levelTextObject; 

    private Button button;
    private Image backgroundImage;

    private void Start()
    {
        button = GetComponent<Button>();
        backgroundImage = GetComponent<Image>();

        UpdateLevelVisuals();
    }

    private void UpdateLevelVisuals()
    {
        if (GameManager.Instance == null || GameManager.Instance.Data == null) return;

        int highestUnlocked = GameManager.Instance.Data.highestLevelUnlocked;
        bool isUnlocked = level <= highestUnlocked;

        if (button != null)
        {
            button.interactable = isUnlocked;
        }

        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        if (levelTextObject != null)
        {
            levelTextObject.SetActive(isUnlocked);
        }

        if (backgroundImage != null)
        {
            if (level < highestUnlocked)
            {
                if (unlockedSprite != null) backgroundImage.sprite = unlockedSprite;
            }
            else if (level == highestUnlocked)
            {
                if (currentLevelSprite != null) backgroundImage.sprite = currentLevelSprite;
            }
            else
            {
                if (lockedSprite != null) backgroundImage.sprite = lockedSprite;
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

        string sceneName = "Level " + level.ToString();

        // Call GameManager so it fades out before loading
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(sceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}