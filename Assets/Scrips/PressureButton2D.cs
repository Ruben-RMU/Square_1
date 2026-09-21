using Unity.VisualScripting;
using UnityEngine;

public class PressureButton2D : MonoBehaviour
{
    [SerializeField] private Door2D doorToControl;
    [SerializeField] private Vector3 pressedOffset = new Vector3(0, -0.1f, 0);
    [SerializeField] private float pressSpeed = 5f;

    private Vector3 unpressedPos;
    private Vector3 pressedPos;
    private int playerTouchCount = 0;

    private void Start()
    {
        unpressedPos = transform.position;
        pressedPos = unpressedPos + pressedOffset;
    }

    private void Update()
    {
        // Visual button press animation
        Vector3 targetPos = (playerTouchCount > 0) ? pressedPos : unpressedPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, pressSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("ElementBox"))
        {
            playerTouchCount++;
            if (playerTouchCount == 1)
            {
                doorToControl.OpenDoor();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("ElementBox"))
        {
            playerTouchCount = Mathf.Max(0, playerTouchCount - 1);
            if (playerTouchCount == 0)
            {
                doorToControl.CloseDoor();
            }
        }
    }
}   