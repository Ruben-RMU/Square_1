using System.Collections;
using UnityEngine;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Scrips
{
    public class CheatCodeManager : MonoBehaviour
    {
        [Header("UI References")] [SerializeField]
        private GameObject cheatPanel;

        [SerializeField] private TMP_InputField cheatInputField;

        [Header("Settings")] [SerializeField] private int totalLevels = 10;
        [SerializeField] private string unlockAllCode = "unlockall";

        private void Start()
        {
            if (cheatPanel != null)
            {
                cheatPanel.SetActive(false);
            }
            else
            {
                Debug.LogError("[CheatCodeManager] Cheat Panel is NOT assigned in Inspector!");
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            // Check keypress via New Input System or Legacy Input System
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rightShiftKey.wasPressedThisFrame)
            {
                Debug.Log("[CheatCodeManager] 'C' key detected via New Input System.");
                ToggleCheatPanel();
            }
#else
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("[CheatCodeManager] 'C' key detected via Legacy Input.");
                ToggleCheatPanel();
            }
#endif
#endif

            // Mobile 3-Finger Tap Detection (Legacy Touch)
            if (Input.touchCount == 3 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
            {
                Debug.Log("[CheatCodeManager] 3-finger touch detected.");
                ToggleCheatPanel();
            }
        }

        public void ToggleCheatPanel()
        {
            if (cheatPanel == null) return;

            bool isActive = !cheatPanel.activeSelf;
            cheatPanel.SetActive(isActive);

            if (isActive && cheatInputField != null)
            {
                cheatInputField.text = "";
                StartCoroutine(FocusInputFieldNextFrame());
            }
        }

        private IEnumerator FocusInputFieldNextFrame()
        {
            yield return null;
            cheatInputField.Select();
            cheatInputField.ActivateInputField();
        }

        public void SubmitCheatCode(string code)
        {
            Debug.Log($"[CheatCodeManager] SubmitCheatCode called with input: '{code}'");

            string formattedCode = code.Trim().ToLower();

            if (formattedCode == unlockAllCode.ToLower())
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UnlockAllLevels(totalLevels);
                    Debug.Log($"[Cheat Success] Unlocked all {totalLevels} levels!");
                }
                else
                {
                    Debug.LogError("[CheatCodeManager] GameManager.Instance is NULL!");
                }

                cheatPanel.SetActive(false);
            }
            else if (!string.IsNullOrEmpty(formattedCode))
            {
                Debug.LogWarning($"[Cheat Failed] Code '{formattedCode}' did not match '{unlockAllCode.ToLower()}'.");
                cheatInputField.text = "";
            }
        }
    }
}