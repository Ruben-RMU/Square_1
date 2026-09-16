using System.Collections;
using UnityEngine;

namespace Scrips
{
    public enum BoxType { Light, Dark }

    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public class ElementBox : MonoBehaviour
    {
        [Header("Box Identity")]
        public BoxType boxType;

        [Header("Combination Settings")]
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private float lightCombineDelay = 0.2f; 
        [SerializeField] private float darkCombineDelay = 0.5f;  

        [Header("Magnetic Repulsion")]
        [SerializeField] private float magneticRadius = 3f;  
        [SerializeField] private float maxRepelForce = 15f; 

        private Rigidbody2D _rb;
        private Transform _transform;
        private bool _isCombining;

        private static readonly Collider2D[] RepulsionResults = new Collider2D[16];
        private static readonly Collider2D[] ExplosionResults = new Collider2D[32];

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _transform = transform;
        }

        private void FixedUpdate()
        {
            if (_isCombining) return;

            ApplyMagneticRepulsion();
        }

        private void ApplyMagneticRepulsion()
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(_transform.position, magneticRadius, RepulsionResults);

            for (int i = 0; i < hitCount; i++)
            {
                var col = RepulsionResults[i];
                if (col == null || col.gameObject == gameObject) continue;

                if (col.TryGetComponent<ElementBox>(out var otherBox))
                {
                    if (otherBox._isCombining) continue;

                    if (IsLightAndDarkPair(this.boxType, otherBox.boxType))
                    {
                        Vector2 directionAway = (Vector2)_transform.position - (Vector2)col.transform.position;
                        float distance = directionAway.magnitude;

                        if (distance > 0f)
                        {
                            float proximityFactor = 4f - Mathf.Clamp01(distance / magneticRadius);
                            float forceMagnitude = maxRepelForce * proximityFactor;

                            _rb.AddForce(directionAway.normalized * forceMagnitude, ForceMode2D.Force);
                        }
                    }
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Always deal exactly 1 damage if the box is currently combining/exploding
            if (_isCombining)
            {
                if (collision.gameObject.TryGetComponent<PlayerController2D>(out var player))
                {
                    Vector2 forceDir = ((Vector2)collision.transform.position - (Vector2)_transform.position).normalized;
                    player.TakeDamage(1);
                    player.ApplyKnockback(forceDir * 15f, 0.3f);
                }
                return;
            }

            if (collision.gameObject.TryGetComponent<ElementBox>(out var otherBox))
            {
                if (otherBox._isCombining) return;
                
                if (this.boxType == otherBox.boxType)
                {
                    if (GetInstanceID() < otherBox.GetInstanceID())
                    {
                        _isCombining = true;
                        otherBox._isCombining = true;

                        Vector3 contactPoint = collision.GetContact(0).point;
                        float targetDelay = (this.boxType == BoxType.Light) ? lightCombineDelay : darkCombineDelay;

                        StartCoroutine(DelayedCombineRoutine(otherBox, contactPoint, targetDelay));
                    }
                }
            }
        }

        private IEnumerator DelayedCombineRoutine(ElementBox otherBox, Vector3 contactPoint, float delay)
        {
            yield return new WaitForSeconds(delay);

            CombineBoxes(this.boxType, (otherBox != null ? otherBox.boxType : this.boxType), contactPoint);

            if (otherBox != null)
            {
                Destroy(otherBox.gameObject);
            }
            Destroy(gameObject);
        }

        private static bool IsLightAndDarkPair(BoxType a, BoxType b)
        {
            return (a == BoxType.Light && b == BoxType.Dark) || (a == BoxType.Dark && b == BoxType.Light);
        }

        private void CombineBoxes(BoxType typeA, BoxType typeB, Vector3 point)
        {
            if (typeA == BoxType.Light && typeB == BoxType.Light)
            {
                TriggerExplosion(point);
            }
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
            int hitCount = Physics2D.OverlapCircleNonAlloc(point, explosionRadius, ExplosionResults);

            for (int i = 0; i < hitCount; i++)
            {
                var col = ExplosionResults[i];
                if (col == null) continue;

                BreakableWall wall = col.GetComponentInParent<BreakableWall>();
                if (wall != null)
                {
                    wall.Break();
                    continue;
                }
                
                if (col.TryGetComponent<PlayerController2D>(out var player))
                {
                    Vector2 forceDir = ((Vector2)col.transform.position - (Vector2)point).normalized;
                    player.TakeDamage(1);
                    player.ApplyKnockback(forceDir * 15f, 0.3f); 
                }
                else if (col.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 forceDir = ((Vector2)col.transform.position - (Vector2)point).normalized;
                    rb.linearVelocity = Vector2.zero;
                    rb.AddForce(forceDir * 15f, ForceMode2D.Impulse);
                }
            }
        }

        private void TriggerTeleport(Vector3 point)
        {
            PlayerController2D player = Object.FindFirstObjectByType<PlayerController2D>();
            if (player == null) return;

            GameObject[] targets = GameObject.FindGameObjectsWithTag("TeleportTarget");

            if (targets.Length > 0)
            {
                GameObject nearestTarget = null;
                float shortestDistance = float.MaxValue;

                foreach (GameObject target in targets)
                {
                    if (target == null) continue;
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magneticRadius);
        }
    }
}