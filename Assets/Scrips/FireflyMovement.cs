using UnityEngine;

public class FireflyMovement : MonoBehaviour
{
    [HideInInspector]
    public Transform[] waypoints;

    [HideInInspector]
    public float speed = 1.5f;

    private int currentPoint;

    public void SetStartingPoint(int point)
    {
        currentPoint = point;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        // Move smoothly toward the next point
        transform.position = Vector3.MoveTowards(
            transform.position,
            waypoints[currentPoint].position,
            speed * Time.deltaTime
        );

        // Check if we reached the point
        if (Vector3.Distance(
            transform.position,
            waypoints[currentPoint].position
        ) < 0.01f)
        {
            currentPoint++;

            // Start again when reaching the end
            if (currentPoint >= waypoints.Length)
            {
                currentPoint = 0;
            }
        }
    }
}