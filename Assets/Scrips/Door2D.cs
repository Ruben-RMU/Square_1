using UnityEngine;

public class Door2D : MonoBehaviour
{
    public enum OpenDirection
    {
        Up,
        Down,
        Left,
        Right,
        Custom
    }

    [Header("Movement Settings")]
    [SerializeField] private OpenDirection direction = OpenDirection.Up;
    [SerializeField] private Vector2 customDirection = Vector2.up;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float speed = 3f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;

    private void Start()
    {
        closedPos = transform.position;
        openPos = closedPos + (Vector3)GetDirectionVector() * moveDistance;
    }

    private void Update()
    {
        Vector3 targetPos = isOpen ? openPos : closedPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    private Vector2 GetDirectionVector()
    {
        switch (direction)
        {
            case OpenDirection.Up:    return Vector2.up;
            case OpenDirection.Down:  return Vector2.down;
            case OpenDirection.Left:  return Vector2.left;
            case OpenDirection.Right: return Vector2.right;
            case OpenDirection.Custom: return customDirection.normalized;
            default:                  return Vector2.up;
        }
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }

    // Shows the open position in the Scene view so you can see where the door will end up
    private void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? closedPos : transform.position;
        Vector3 end = start + (Vector3)GetDirectionVector() * moveDistance;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.15f);
    }
}