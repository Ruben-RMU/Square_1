using UnityEngine;

public class FireflyMovement : MonoBehaviour
{
    [HideInInspector] public Transform[] waypoints;
    [HideInInspector] public float speed = 1.5f;
    [HideInInspector] public Vector3 offset;
    [HideInInspector] public bool destroyAtEnd;

    private int currentPoint;
    private bool finished;

    public void SetStartingPoint(int point)
    {
        currentPoint = point;
    }

    void Update()
    {
        if (finished || waypoints == null || waypoints.Length < 2)
            return;

        Vector3 target = waypoints[currentPoint].position + offset;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            currentPoint++;

            // Reached the last point: stop (or remove)
            if (currentPoint >= waypoints.Length)
            {
                finished = true;
                if (destroyAtEnd)
                    Destroy(gameObject);
            }
        }
    }
}