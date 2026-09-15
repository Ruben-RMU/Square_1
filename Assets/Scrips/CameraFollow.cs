using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    [Header("Level Bounds (X Axis)")]
    public float minX;
    public float maxX;

    [Header("Vertical Settings")]
    public bool lockY = true;
    private float fixedY;

    [Header("Debug")]
    public bool showBounds = true; // Toggle to show/hide lines

    void Start()
    {
        if (lockY)
        {
            fixedY = transform.position.y;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = Mathf.Clamp(target.position.x + offset.x, minX, maxX);
        float targetY = lockY ? fixedY : target.position.y + offset.y;

        Vector3 desiredPosition = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }

    void OnDrawGizmos()
    {
        // If the toggle is turned off, do not draw the lines
        if (!showBounds) return;

        Gizmos.color = Color.red;
        
        // Draw vertical lines in the Scene view for minX and maxX boundaries
        Gizmos.DrawLine(new Vector3(minX, -50f, 0f), new Vector3(minX, 50f, 0f));
        Gizmos.DrawLine(new Vector3(maxX, -50f, 0f), new Vector3(maxX, 50f, 0f));
    }
}