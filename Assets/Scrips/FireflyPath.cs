using UnityEngine;

public class FireflyPath : MonoBehaviour
{
    public Transform[] waypoints;
    public GameObject fireflyPrefab;

    public int numberOfFireflies = 8;
    public float speed = 1.5f;

    void Start()
    {
        if (waypoints.Length < 2 || fireflyPrefab == null)
            return;

        for (int i = 0; i < numberOfFireflies; i++)
        {
            GameObject firefly = Instantiate(fireflyPrefab);

            FireflyMovement movement =
                firefly.GetComponent<FireflyMovement>();

            movement.waypoints = waypoints;
            movement.speed = speed;

            // Spread fireflies across the path
            int startingPoint =
                Mathf.FloorToInt(
                    (float)i / numberOfFireflies *
                    waypoints.Length
                );

            startingPoint =
                Mathf.Clamp(
                    startingPoint,
                    0,
                    waypoints.Length - 1
                );

            // Put firefly at its starting point
            firefly.transform.position =
                waypoints[startingPoint].position;

            // Tell the firefly where to continue from
            movement.SetStartingPoint(startingPoint + 1);

            if (startingPoint + 1 >= waypoints.Length)
            {
                movement.SetStartingPoint(0);
            }
        }
    }
}