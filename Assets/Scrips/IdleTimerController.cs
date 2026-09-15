using UnityEngine;

public class IdleTimerController : MonoBehaviour
{
    private Animator animator;
    private float idleTimer = 0f;
    [SerializeField] private float idleThreshold = 5f; // Time in seconds

    private Rigidbody2D rb2d;
    private Vector3 lastPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
    }
    
    public void ResetLongIdle()
    {
        animator.SetBool("IsLongIdle", false);
        idleTimer = 0f;
    }

    void Update()
    {
        bool isMoving = false;

        // Check if using a Rigidbody2D component
        if (rb2d != null)
        {
            isMoving = rb2d.linearVelocity.sqrMagnitude > 0.01f;
        }
        else
        {
            // Fallback: Check if the character's position changed since the last frame
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            isMoving = distanceMoved > 0.001f;
            lastPosition = transform.position;
        }

        if (isMoving)
        {
            // Reset timer and animator state when moving
            idleTimer = 0f;
            animator.SetBool("IsLongIdle", false);
        }
        else
        {
            // Count up when standing still
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleThreshold)
            {
                animator.SetBool("IsLongIdle", true);
            }
        }
    }
}