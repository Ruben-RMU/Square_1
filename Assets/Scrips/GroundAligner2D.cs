using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerGroundAligner2D : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float rayDistance = 1.2f;
    [SerializeField] private float rayOffset = 0.45f;
    
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float airborneResetSpeed = 5f;

    public bool IsGrounded { get; private set; }

    private void FixedUpdate()
    {
        AlignToGroundTwoRays();
    }

    private void AlignToGroundTwoRays()
    {
        Vector2 originLeft = (Vector2)transform.position - ((Vector2)transform.right * rayOffset);
        Vector2 originRight = (Vector2)transform.position + ((Vector2)transform.right * rayOffset);

        RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, Vector2.down, rayDistance, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(originRight, Vector2.down, rayDistance, groundLayer);

        Vector2 targetNormal;

        if (hitLeft.collider != null && hitRight.collider != null)
        {
            IsGrounded = true;
            targetNormal = (hitLeft.normal + hitRight.normal).normalized;
        }
        else if (hitLeft.collider != null)
        {
            IsGrounded = true;
            targetNormal = hitLeft.normal;
        }
        else if (hitRight.collider != null)
        {
            IsGrounded = true;
            targetNormal = hitRight.normal;
        }
        else
        {
            IsGrounded = false;
            targetNormal = Vector2.up;
        }

        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, targetNormal);

        float currentSpeed = IsGrounded ? rotationSpeed : airborneResetSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentSpeed * Time.fixedDeltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;

        Vector2 originLeft = (Vector2)transform.position - ((Vector2)transform.right * rayOffset);
        Vector2 originRight = (Vector2)transform.position + ((Vector2)transform.right * rayOffset);

        Gizmos.DrawRay(originLeft, Vector2.down * rayDistance);
        Gizmos.DrawRay(originRight, Vector2.down * rayDistance);
    }
}