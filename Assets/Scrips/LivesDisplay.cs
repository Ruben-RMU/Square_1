using UnityEngine;
using TMPro;

namespace Scrips
{
    public class LivesDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController2D player;
        [SerializeField] private TextMeshProUGUI livesText;

        private void OnEnable()
        {
            if (player != null)
            {
                player.OnLivesChanged += UpdateDisplay;
                UpdateDisplay(player.CurrentLives); // Set initial text on start
            }
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.OnLivesChanged -= UpdateDisplay;
            }
        }

        private void UpdateDisplay(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {lives}";
            }
        }
    }
}