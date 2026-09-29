using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadePanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterFadePanel(canvasGroup);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterFadePanel(canvasGroup);
        }
    }
}