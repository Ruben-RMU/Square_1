using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler,
    IPointerExitHandler
{
    public bool IsPressed { get; private set; }

    private RectTransform _rectTransform;
    private Canvas _parentCanvas;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerPress != null || Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            IsPressed = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPressed = false;
    }

    private void Update()
    {
        if (IsPressed && !IsPointerOverButton())
        {
            IsPressed = false;
        }
    }

    private bool IsPointerOverButton()
    {
        Camera cam = (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? _parentCanvas.worldCamera
            : null;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, touch.position, cam))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        if (Input.GetMouseButton(0))
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition, cam);
        }

        return false;
    }

    private void OnDisable()
    {
        IsPressed = false;
    }
}