using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private string animationTriggerName = "Play";
    [SerializeField] private float animationDuration = 1.0f;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDelay = 0.5f;
    [SerializeField] private float fadeDuration = 1.0f;

    public void StartGame()
    {
        StartCoroutine(PlayAnimationThenFadeAndLoad());
    }

    private IEnumerator PlayAnimationThenFadeAndLoad()
    {
        // Prevent multiple clicks during sequence
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
        }

        // 1. Trigger your Menu Animation
        if (menuAnimator != null)
        {
            menuAnimator.SetTrigger(animationTriggerName);
        }

        // Wait for the menu animation to finish
        if (animationDuration > 0f)
        {
            yield return new WaitForSeconds(animationDuration);
        }

        // Wait half a second before fading
        if (fadeDelay > 0f)
        {
            yield return new WaitForSeconds(fadeDelay);
        }

        // 2. Fade screen to black
        if (fadeCanvasGroup != null && fadeDuration > 0f)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        // 3. Load scene
        SceneManager.LoadSceneAsync(1);
    }
}