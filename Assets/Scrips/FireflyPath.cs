using UnityEngine;

public class FireflyPath : MonoBehaviour
{
    public Transform[] waypoints;
    public GameObject fireflyPrefab;

    public int numberOfFireflies = 8;
    public float speed = 1.5f;

    [Tooltip("How spread out the group is around the path")]
    public float groupRadius = 0.5f;

    [Tooltip("Remove fireflies when they reach the last point")]
    public bool destroyAtEnd = false;

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2 || fireflyPrefab == null)
        {
            Debug.LogWarning("FireflyPath: assign at least 2 waypoints and a prefab.", this);
            return;
        }

        foreach (Transform w in waypoints)
        {
            if (w == null)
            {
                Debug.LogError("FireflyPath: a waypoint slot is empty.", this);
                return;
            }
        }

        for (int i = 0; i < numberOfFireflies; i++)
        {
            GameObject firefly = Instantiate(fireflyPrefab);

            FireflyMovement movement = firefly.GetComponentInChildren<FireflyMovement>();
            if (movement == null)
            {
                Debug.LogError("Firefly prefab has no FireflyMovement script!", fireflyPrefab);
                Destroy(firefly);
                return;
            }

            // Each firefly keeps its own small offset so the group stays clustered
            Vector2 rand = Random.insideUnitCircle * groupRadius;
            Vector3 offset = new Vector3(rand.x, rand.y, 0f);

            movement.waypoints = waypoints;
            movement.speed = speed * Random.Range(0.9f, 1.1f); // slight variation looks more natural
            movement.offset = offset;
            movement.destroyAtEnd = destroyAtEnd;

            movement.transform.position = waypoints[0].position + offset;
            movement.SetStartingPoint(1);
        }
    }
}