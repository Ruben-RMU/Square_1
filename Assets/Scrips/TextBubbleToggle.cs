using UnityEngine;

public class TextBubbleToggle : MonoBehaviour
{
    [SerializeField] private GameObject textBubble;

    private void Start()
    {
        if (textBubble != null)
            textBubble.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            textBubble.SetActive(!textBubble.activeSelf);
        }
    }
}