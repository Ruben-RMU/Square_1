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

        [Header("Dark Combination Prefabs")]
        [SerializeField] private GameObject anim1Prefab; // Spawned at merge point after player disappears
        [SerializeField] private GameObject anim2Prefab; // Spawned at destination before player appears
        [SerializeField] private GameObject anim3Prefab; // Spawned at destination after player appears

        [Header("Dark Combination Delays & Timings")]
        [Tooltip("Suck-in force and pull duration moving player to center before disappearing.")]
        [SerializeField] private float pullDuration = 0.3f;

        [Tooltip("Delay after player disappears before Anim 1 plays.")]
        [SerializeField] private float anim1StartDelay = 0.0f;

        [Tooltip("Duration to wait while Anim 1 plays.")]
        [SerializeField] private float anim1Duration = 0.4f;

        [Tooltip("Delay before Anim 2 plays at destination.")]
        [SerializeField] private float anim2StartDelay = 0.0f;

        [Tooltip("Duration to wait while Anim 2 plays before player appears.")]
        [SerializeField] private float anim2Duration = 0.4f;

        [Tooltip("Delay after player appears before Anim 3 plays.")]
        [SerializeField] private float anim3StartDelay = 0.0f;

        [Tooltip("Duration to wait for Anim 3 to finish before destroying boxes.")]
        [SerializeField] private float anim3Duration = 0.2f;

        [Header("Magnetic Repulsion")]
        [SerializeField] private float magneticRadius = 3f;
        [SerializeField] private float maxRepelForce = 15f;

        private Rigidbody2D _rb;
        private bool _isCombining;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_isCombining) return;
            ApplyMagneticRepulsion();
        }

        private void ApplyMagneticRepulsion()
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, magneticRadius);

            foreach (var col in nearbyColliders)
            {
                if (col.gameObject == gameObject) continue;

                if (col.TryGetComponent<ElementBox>(out var otherBox))
                {
                    if (otherBox._isCombining) continue;

                    if (IsLightAndDarkPair(this.boxType, otherBox.boxType))
                    {
                        Vector2 directionAway = transform.position - col.transform.position;
                        float distance = directionAway.magnitude;

                        if (distance > 0)
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
            if (_isCombining) return;

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

            Vector3 mergePoint = (otherBox != null)
                ? (transform.position + otherBox.transform.position) * 0.5f
                : transform.position;

            yield return StartCoroutine(CombineBoxesRoutine(
                this.boxType,
                (otherBox != null ? otherBox.boxType : this.boxType),
                mergePoint,
                otherBox
            ));

            if (otherBox != null)
            {
                Destroy(otherBox.gameObject);
            }
            Destroy(gameObject);
        }

        private bool IsLightAndDarkPair(BoxType a, BoxType b)
        {
            return (a == BoxType.Light && b == BoxType.Dark) || (a == BoxType.Dark && b == BoxType.Light);
        }

        private IEnumerator CombineBoxesRoutine(BoxType typeA, BoxType typeB, Vector3 point, ElementBox otherBox)
        {
            HideAndDisableBox(this);
            if (otherBox != null) HideAndDisableBox(otherBox);

            if (typeA == BoxType.Light && typeB == BoxType.Light)
            {
                TriggerExplosion(point);
            }
            else if (typeA == BoxType.Dark && typeB == BoxType.Dark)
            {
                yield return StartCoroutine(DarkTeleportSequenceRoutine(point));
            }
        }

        private IEnumerator DarkTeleportSequenceRoutine(Vector3 mergePoint)
        {
            GameObject player = GameObject.FindWithTag("Player");

            // 1. Get sucked in: Smoothly pull the player directly to the center of the merge point
            TriggerImplosion(mergePoint);

            if (player != null)
            {
                if (player.TryGetComponent<Rigidbody2D>(out var playerRb))
                {
                    playerRb.linearVelocity = Vector2.zero;
                }

                Vector3 startPos = player.transform.position;
                float elapsed = 0f;

                while (elapsed < pullDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = (pullDuration > 0f) ? Mathf.Clamp01(elapsed / pullDuration) : 1f;
                    player.transform.position = Vector3.Lerp(startPos, mergePoint, progress);
                    yield return null;
                }

                player.transform.position = mergePoint;
            }
            else
            {
                yield return new WaitForSeconds(pullDuration);
            }

            if (player == null) yield break;

            Vector3 targetPosition = GetTeleportTarget(mergePoint);

            // 2. Player disappears at merge point center
            SetPlayerState(player, visible: false);

            // 3. First animation (plays at merge point center)
            if (anim1StartDelay > 0f) yield return new WaitForSeconds(anim1StartDelay);
            if (anim1Prefab != null) Instantiate(anim1Prefab, mergePoint, Quaternion.identity);
            yield return new WaitForSeconds(anim1Duration);

            // 4. Second animation (plays at destination point)
            if (anim2StartDelay > 0f) yield return new WaitForSeconds(anim2StartDelay);
            if (anim2Prefab != null) Instantiate(anim2Prefab, targetPosition, Quaternion.identity);
            yield return new WaitForSeconds(anim2Duration);

            // 5. Third animation starts at destination point
            if (anim3StartDelay > 0f) yield return new WaitForSeconds(anim3StartDelay);
            if (anim3Prefab != null) Instantiate(anim3Prefab, targetPosition, Quaternion.identity);

            

            // Player appears in the middle of Anim 3
            player.transform.position = targetPosition;
            SetPlayerState(player, visible: true);

            
        }

        private void SetPlayerState(GameObject player, bool visible)
        {
            foreach (var r in player.GetComponentsInChildren<Renderer>())
                r.enabled = visible;

            foreach (var c in player.GetComponentsInChildren<Collider2D>())
                c.enabled = visible;

            if (player.TryGetComponent<Rigidbody2D>(out var playerRb))
            {
                playerRb.simulated = visible;
                if (!visible)
                {
                    playerRb.linearVelocity = Vector2.zero;
                }
            }
        }

        private Vector3 GetTeleportTarget(Vector3 originPoint)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag("TeleportTarget");
            if (targets.Length == 0) return originPoint;

            GameObject nearestTarget = null;
            float shortestDistance = float.MaxValue;

            foreach (GameObject target in targets)
            {
                float distance = Vector2.Distance(originPoint, target.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = target;
                }
            }

            return nearestTarget != null ? nearestTarget.transform.position : originPoint;
        }

        private void HideAndDisableBox(ElementBox box)
        {
            if (box == null) return;

            foreach (var r in box.GetComponentsInChildren<Renderer>())
                r.enabled = false;

            foreach (var c in box.GetComponentsInChildren<Collider2D>())
                c.enabled = false;

            if (box.TryGetComponent<Rigidbody2D>(out var rb))
                rb.simulated = false;
        }

        private void TriggerExplosion(Vector3 point)
        {
            if (explosionEffectPrefab)
                Instantiate(explosionEffectPrefab, point, Quaternion.identity);

            float explosionRadius = 3.5f;
            Collider2D[] affectedColliders = Physics2D.OverlapCircleAll(point, explosionRadius);

            foreach (var col in affectedColliders)
            {
                BreakableWall wall = col.GetComponentInParent<BreakableWall>();
                if (wall != null)
                {
                    wall.Break();
                    continue;
                }

                if (col.TryGetComponent<PlayerController2D>(out var player))
                {
                    Vector2 forceDir = ((Vector2)col.transform.position - (Vector2)point).normalized;
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

        private void TriggerImplosion(Vector3 point)
        {
            float implosionRadius = 3.5f;
            Collider2D[] affectedColliders = Physics2D.OverlapCircleAll(point, implosionRadius);

            foreach (var col in affectedColliders)
            {
                BreakableWall wall = col.GetComponentInParent<BreakableWall>();
                if (wall != null)
                {
                    wall.Break();
                    continue;
                }

                Vector2 pullDir = ((Vector2)point - (Vector2)col.transform.position).normalized;

                if (col.TryGetComponent<PlayerController2D>(out var player))
                {
                    player.ApplyKnockback(pullDir * 15f, 0.3f);
                }
                else if (col.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.AddForce(pullDir * 15f, ForceMode2D.Impulse);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magneticRadius);
        }
    }
}