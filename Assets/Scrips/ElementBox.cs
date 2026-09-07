using UnityEngine;

namespace Scrips
{
    public enum BoxType { Light, Dark }

    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public class PushableBox : MonoBehaviour
    {
        [Header("Box Identity")]
        public BoxType boxType;

        [Header("Combination Settings")]
        [SerializeField] private GameObject explosionEffectPrefab;

        [Header("Magnetic Repulsion")]
        [SerializeField] private float magneticRadius = 3f;  // Distance where magnetic force starts
        [SerializeField] private float maxRepelForce = 15f; // Push strength at point-blank range

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            // Continuously scan for opposite-type boxes nearby
            ApplyMagneticRepulsion();
        }

        private void ApplyMagneticRepulsion()
        {
            // Find all colliders within the magnetic radius
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, magneticRadius);

            foreach (var col in nearbyColliders)
            {
                // Skip checking ourselves
                if (col.gameObject == gameObject) continue;

                if (col.TryGetComponent<PushableBox>(out var otherBox))
                {
                    // Only apply repulsion between Light and Dark
                    if (IsLightAndDarkPair(this.boxType, otherBox.boxType))
                    {
                        Vector2 directionAway = transform.position - col.transform.position;
                        float distance = directionAway.magnitude;

                        if (distance > 0)
                        {
                            // Force gets stronger the closer they get (inverse linear falloff)
                            float proximityFactor = 4f - Mathf.Clamp01(distance / magneticRadius);
                            float forceMagnitude = maxRepelForce * proximityFactor;

                            // Apply continuous smooth magnetic force
                            _rb.AddForce(directionAway.normalized * forceMagnitude, ForceMode2D.Force);
                        }
                    }
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<PushableBox>(out var otherBox))
            {
                // Light + Light or Dark + Dark combination logic
                if (this.boxType == otherBox.boxType)
                {
                    if (GetInstanceID() < otherBox.GetInstanceID())
                    {
                        Vector3 contactPoint = collision.GetContact(0).point;
                        CombineBoxes(this.boxType, otherBox.boxType, contactPoint);

                        Destroy(otherBox.gameObject);
                        Destroy(gameObject);
                    }
                }
            }
        }

        private bool IsLightAndDarkPair(BoxType a, BoxType b)
        {
            return (a == BoxType.Light && b == BoxType.Dark) || (a == BoxType.Dark && b == BoxType.Light);
        }

        private void CombineBoxes(BoxType typeA, BoxType typeB, Vector3 point)
        {
            // 1. Light + Light = Explosion
            if (typeA == BoxType.Light && typeB == BoxType.Light)
            {
                TriggerExplosion(point);
            }
            // 2. Dark + Dark = Teleport Player to nearest target
            else if (typeA == BoxType.Dark && typeB == BoxType.Dark)
            {
                TriggerTeleport(point);
            }
        }

        private void TriggerExplosion(Vector3 point)
        {
            if (explosionEffectPrefab) 
                Instantiate(explosionEffectPrefab, point, Quaternion.identity);

            float explosionRadius = 3.5f;
            Collider2D[] affectedColliders = Physics2D.OverlapCircleAll(point, explosionRadius);

            foreach (var col in affectedColliders)
            {
                if (col.TryGetComponent<BreakableWall>(out var wall))
                {
                    Vector2 direction = col.transform.position - point;
                    RaycastHit2D hit = Physics2D.Raycast(point, direction.normalized, direction.magnitude);

                    if (hit.collider != null && hit.collider.gameObject == col.gameObject)
                    {
                        wall.Break();
                    }
                }

                if (col.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 forceDir = (col.transform.position - point).normalized;
                    rb.AddForce(forceDir * 15f, ForceMode2D.Impulse);
                }
            }
        }

        private void TriggerTeleport(Vector3 point)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            GameObject[] targets = GameObject.FindGameObjectsWithTag("TeleportTarget");

            if (targets.Length > 0)
            {
                GameObject nearestTarget = null;
                float shortestDistance = float.MaxValue;

                foreach (GameObject target in targets)
                {
                    float distance = Vector2.Distance(point, target.transform.position);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestTarget = target;
                    }
                }

                if (nearestTarget != null)
                {
                    player.transform.position = nearestTarget.transform.position;
                }
            }
            else
            {
                player.transform.position = point;
            }
        }

        // Draw magnetic field in Scene View for easy tuning
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magneticRadius);
        }
    }
}