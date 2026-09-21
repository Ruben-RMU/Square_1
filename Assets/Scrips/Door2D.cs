using UnityEngine;

public class Door2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float speed = 3f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;

    private void Start()
    {
        closedPos = transform.position;
        openPos = closedPos + Vector3.up * moveDistance;
    }

    private void Update()
    {
        Vector3 targetPos = isOpen ? openPos : closedPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }
}