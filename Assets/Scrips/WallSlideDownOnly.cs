using UnityEngine;

public class WallSlideDownOnly : MonoBehaviour
{
    [Header("Wall Detection")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;

    [Header("Slide Settings")]
    [SerializeField] private float maxSlideSpeed = 2.5f;
    [SerializeField] private bool stopUpwardMomentum = true;

    private Rigidbody2D rb;
    private bool isTouchingWall;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Check if touching a wall on the Wall Layer
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        if (isTouchingWall)
        {
            // 1. Prevent sliding UP: Cancel upward vertical velocity on wall contact
            if (stopUpwardMomentum && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }

            // 2. Controlled sliding DOWN: Clamp downward velocity to maxSlideSpeed
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x, 
                    Mathf.Max(rb.linearVelocity.y, -maxSlideSpeed)
                );
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}