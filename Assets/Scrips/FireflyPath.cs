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

    [Header("Effects")]
    [Tooltip("The second effect, triggered at the designated checkpoint")]
    public GameObject secondEffectPrefab;

    [Tooltip("The third effect, triggered after the pause until reaching the path end")]
    public GameObject thirdEffectPrefab;

    [Tooltip("Waypoint index where the second effect triggers (1 = second waypoint)")]
    public int effectPointIndex = 1;

    [Tooltip("Duration (seconds) the second effect plays")]
    public float secondEffectDuration = 2f;

    [Tooltip("Pause duration (seconds) with no extra effects before third effect starts and movement resumes")]
    public float quietTime = 2f;

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2 || fireflyPrefab == null)
        {
            Debug.LogWarning("FireflyPath: Assign at least 2 waypoints and a firefly prefab.", this);
            return;
        }

        int effectIndex = Mathf.Clamp(effectPointIndex, 1, waypoints.Length - 1);

        for (int i = 0; i < numberOfFireflies; i++)
        {
            GameObject firefly = Instantiate(fireflyPrefab);

            FireflyMovement movement = firefly.GetComponentInChildren<FireflyMovement>();
            if (movement == null)
            {
                Debug.LogError("FireflyPrefab is missing the FireflyMovement script!", fireflyPrefab);
                Destroy(firefly);
                return;
            }

            Vector2 rand = Random.insideUnitCircle * groupRadius;
            Vector3 offset = new Vector3(rand.x, rand.y, 0f);

            movement.waypoints = waypoints;
            movement.speed = speed * Random.Range(0.9f, 1.1f);
            movement.offset = offset;
            movement.destroyAtEnd = destroyAtEnd;

            movement.effectPointIndex = effectIndex;
            movement.secondEffectPrefab = secondEffectPrefab;
            movement.thirdEffectPrefab = thirdEffectPrefab;
            movement.secondEffectDuration = secondEffectDuration;
            movement.quietTime = quietTime;

            movement.transform.position = waypoints[0].position + offset;
            movement.SpawnFirstEffect();
        }
    }
}