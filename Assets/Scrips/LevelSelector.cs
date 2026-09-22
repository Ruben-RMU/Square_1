using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [Header("Level Settings")]
    public int level;

    [Header("Background Sprites")]
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite currentLevelSprite;
    [SerializeField] private Sprite lockedSprite;

    [Header("UI Overlay Elements")]
    [SerializeField] private GameObject lockIcon;        // Drag your Lock Icon child GameObject here
    [SerializeField] private GameObject levelTextObject; // Drag your Level Text child GameObject here

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

        // Enable or disable button interactions
        if (button != null)
        {
            button.interactable = isUnlocked;
        }

        // Show lock icon when locked, hide when unlocked
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        // Hide level text when locked, show when unlocked
        if (levelTextObject != null)
        {
            levelTextObject.SetActive(isUnlocked);
        }

        // Update the background sprite based on progress
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

        SceneManager.LoadScene("Level " + level.ToString());
    }
}