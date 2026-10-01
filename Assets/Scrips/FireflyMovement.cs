using UnityEngine;

public class FireflyMovement : MonoBehaviour
{
    [HideInInspector] public Transform[] waypoints;
    [HideInInspector] public float speed = 1.5f;
    [HideInInspector] public Vector3 offset;
    [HideInInspector] public bool destroyAtEnd;
    [HideInInspector] public GameObject rootToDestroy;

    [HideInInspector] public int effectPointIndex = 1;
    [HideInInspector] public GameObject firstEffectPrefab;
    [HideInInspector] public GameObject secondEffectPrefab;
    [HideInInspector] public GameObject thirdEffectPrefab;
    [HideInInspector] public float secondEffectDuration = 2f;
    [HideInInspector] public float quietTime = 2f;

    private enum State
    {
        Moving,
        SecondEffect,
        Quiet,
        Finished
    }

    private State state = State.Moving;

    private int currentPoint = 1;
    private bool secondEffectTriggered;
    private float timer;

    private GameObject firstEffectInstance;
    private GameObject secondEffectInstance;
    private Renderer[] baseRenderers;

    private void Awake()
    {
        // Cache original renderers on the firefly prefab before any effect prefabs are spawned
        baseRenderers = GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// Spawns the initial firefly effect.
    /// </summary>
    public void SpawnFirstEffect()
    {
        if (firstEffectPrefab != null)
        {
            firstEffectInstance = Instantiate(firstEffectPrefab, transform);
            firstEffectInstance.transform.localPosition = Vector3.zero;
        }
    }

    void Update()
    {
        if (state == State.Finished || waypoints == null || waypoints.Length < 2)
            return;

        switch (state)
        {
            case State.Moving:
                Move();
                break;

            case State.SecondEffect:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    if (secondEffectInstance != null)
                        Destroy(secondEffectInstance);

                    timer = quietTime;
                    state = State.Quiet;
                }

                break;

            case State.Quiet:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    // Re-enable base firefly visuals and spawn third effect to finish the route
                    SetBaseVisuals(true);

                    if (thirdEffectPrefab != null)
                    {
                        GameObject thirdFx = Instantiate(thirdEffectPrefab, transform);
                        thirdFx.transform.localPosition = Vector3.zero;
                    }

                    currentPoint++;
                    state = State.Moving;
                    CheckFinished();
                }

                break;
        }
    }

    private void Move()
    {
        Vector3 target = waypoints[currentPoint].position + offset;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            if (!secondEffectTriggered && currentPoint == effectPointIndex)
            {
                secondEffectTriggered = true;
                StartSecondEffect();
                return;
            }

            currentPoint++;
            CheckFinished();
        }
    }

    private void StartSecondEffect()
    {
        // 1. Remove the first effect instance
        if (firstEffectInstance != null)
        {
            Destroy(firstEffectInstance);
            firstEffectInstance = null;
        }

        // 2. Hide the base firefly mesh/sprite graphics
        SetBaseVisuals(false);

        // 3. Instantiate the second effect
        if (secondEffectPrefab != null)
        {
            secondEffectInstance = Instantiate(secondEffectPrefab, transform);
            secondEffectInstance.transform.localPosition = Vector3.zero;
        }

        timer = secondEffectDuration;
        state = State.SecondEffect;
    }

    private void SetBaseVisuals(bool visible)
    {
        if (baseRenderers == null) return;

        foreach (Renderer r in baseRenderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    private void CheckFinished()
    {
        if (currentPoint >= waypoints.Length)
        {
            state = State.Finished;
            if (destroyAtEnd)
                Destroy(rootToDestroy != null ? rootToDestroy : gameObject);
        }
    }
}