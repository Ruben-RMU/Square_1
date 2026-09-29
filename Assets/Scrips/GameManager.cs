using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameData Data { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [Tooltip("Short pause before fading in, so the scene-load hitch happens while the screen is still black.")]
    [SerializeField] private float fadeInDelay = 0.15f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CanvasGroup currentFadeCanvasGroup;
    private Coroutine fadeInRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Data = SaveSystem.Load();
    }

    // Called automatically by each scene's FadePanel script on Awake
    public void RegisterFadePanel(CanvasGroup canvasGroup)
    {
        currentFadeCanvasGroup = canvasGroup;

        // Make sure we start fully black, regardless of the panel's editor alpha
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (fadeInRoutine != null) StopCoroutine(fadeInRoutine);
        fadeInRoutine = StartCoroutine(FadeInAfterDelay());
    }

    // Called automatically when the scene unloads
    public void UnregisterFadePanel(CanvasGroup canvasGroup)
    {
        if (currentFadeCanvasGroup == canvasGroup)
        {
            currentFadeCanvasGroup = null;
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionToScene(sceneName));
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(TransitionToScene(sceneIndex));
    }

    private IEnumerator FadeInAfterDelay()
    {
        // Let the first heavy frames of the new scene pass while the screen is black
        yield return null;
        yield return null;
        if (fadeInDelay > 0f) yield return new WaitForSecondsRealtime(fadeInDelay);

        yield return Fade(1f, 0f);
        fadeInRoutine = null;
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        if (fadeInRoutine != null) StopCoroutine(fadeInRoutine);
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator TransitionToScene(int sceneIndex)
    {
        if (fadeInRoutine != null) StopCoroutine(fadeInRoutine);
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(sceneIndex);
    }

    private IEnumerator Fade(float startAlpha, float targetAlpha)
    {
        if (currentFadeCanvasGroup == null) yield break;

        currentFadeCanvasGroup.blocksRaycasts = true;
        currentFadeCanvasGroup.alpha = startAlpha;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            // Unscaled + clamped so hitches and timeScale = 0 don't break the fade
            timer += Mathf.Min(Time.unscaledDeltaTime, 0.033f);

            if (currentFadeCanvasGroup == null) yield break; // Guard in case panel is destroyed mid-fade

            float t = fadeCurve.Evaluate(Mathf.Clamp01(timer / fadeDuration));
            currentFadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        if (currentFadeCanvasGroup != null)
        {
            currentFadeCanvasGroup.alpha = targetAlpha;
            currentFadeCanvasGroup.blocksRaycasts = targetAlpha > 0.99f;
        }
    }

    public void IncrementJumps() => Data.totalJumps++;

    public void OnPlayerDeath()
    {
        Data.totalDeaths++;
        SaveSystem.Save(Data);
    }

    public void CompleteLevel(int levelNumber)
    {
        if (levelNumber >= Data.highestLevelUnlocked)
        {
            Data.highestLevelUnlocked = levelNumber + 1;
        }
        SaveSystem.Save(Data);
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save(Data);
    }
}