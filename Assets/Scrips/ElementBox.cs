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
        [SerializeField] private GameObject anim1Prefab; // Spawned at merge point (Portal Effect)
        [SerializeField] private GameObject anim2Prefab; // Spawned at destination before player appears
        [SerializeField] private GameObject anim3Prefab; // Spawned at destination after player appears

        [Header("Dark Combination Delays & Timings")]
        [Tooltip("Max distance from the portal center to start sucking the player in. The portal stays open until player steps into range.")]
        [SerializeField] private float portalTriggerRadius = 3.5f;

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

        private static bool IsLightAndDarkPair(BoxType a, BoxType b)
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
            
            GameObject activePortalFX = null;
            ParticleSystem portalParticles = null;
            
            if (anim1Prefab != null)
            {
                activePortalFX = Instantiate(anim1Prefab, mergePoint, Quaternion.identity);
                portalParticles = activePortalFX.GetComponentInChildren<ParticleSystem>();
            }
            
            while (player != null && Vector3.Distance(player.transform.position, mergePoint) > portalTriggerRadius)
            {
                yield return null; 
            }
        
            if (player == null) 
            {
                if (activePortalFX != null) Destroy(activePortalFX);
                yield break;
            }
            
            float entrySpeed = 0f;
            Vector2 launchDirection = Vector2.up;
            PlayerController2D playerController = null;
            Rigidbody2D playerRb = null;
        
            player.TryGetComponent(out playerController);
            player.TryGetComponent(out playerRb);
        
            float originalGravityScale = 1f;
        
            if (playerRb != null)
            {
                originalGravityScale = playerRb.gravityScale;
                entrySpeed = playerRb.linearVelocity.magnitude;
                Vector2 travelDirection = ((Vector2)mergePoint - (Vector2)player.transform.position).normalized;
        
                if (entrySpeed > 0.1f)
                {
                    Vector2 rawVelocityDir = playerRb.linearVelocity.normalized;
                    launchDirection = (Vector2.Dot(rawVelocityDir, travelDirection) < 0f) ? -rawVelocityDir : rawVelocityDir;
                }
                else
                {
                    launchDirection = travelDirection != Vector2.zero ? travelDirection : Vector2.up;
                }
        
                playerRb.gravityScale = 0f;
                playerRb.linearVelocity = Vector2.zero;
            }
        
            if (playerController != null)
            {
                playerController.SetInputLock(true);
            }
        
            Vector3 startPos = player.transform.position;
            float distance = Vector3.Distance(startPos, mergePoint);
            float dynamicPullDuration = Mathf.Max(pullDuration, distance * 0.05f);
            float elapsed = 0f;
        
            while (elapsed < dynamicPullDuration)
            {
                if (player == null) 
                {
                    if (activePortalFX != null) Destroy(activePortalFX);
                    yield break;
                }
        
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / dynamicPullDuration);
                float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
                player.transform.position = Vector3.Lerp(startPos, mergePoint, easeProgress);
                yield return null;
            }
        
            player.transform.position = mergePoint;
            
            if (portalParticles != null)
            {
                portalParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(activePortalFX, 1.5f);
            }
            else if (activePortalFX != null)
            {
                Destroy(activePortalFX);
            }
            
            Vector3 targetPosition = GetTeleportTarget(mergePoint);
        
            SetPlayerState(player, visible: false);
        
            if (anim1StartDelay > 0f) yield return new WaitForSeconds(anim1StartDelay);
            yield return new WaitForSeconds(anim1Duration);
        
            if (anim2StartDelay > 0f) yield return new WaitForSeconds(anim2StartDelay);
            if (anim2Prefab != null) Instantiate(anim2Prefab, targetPosition, Quaternion.identity);
            yield return new WaitForSeconds(anim2Duration);
        
            if (anim3StartDelay > 0f) yield return new WaitForSeconds(anim3StartDelay);
            if (anim3Prefab != null) Instantiate(anim3Prefab, targetPosition, Quaternion.identity);
            
            player.transform.position = targetPosition;
        
            if (playerRb != null)
            {
                playerRb.gravityScale = originalGravityScale;
            }
        
            SetPlayerState(player, visible: true);
        
            if (playerController != null)
            {
                playerController.SetInputLock(false);
        
                float launchSpeed = Mathf.Max(entrySpeed, 8f);
                playerController.ApplyKnockback(launchDirection * 0.6f * launchSpeed, 0.3f);
            }
        
            if (anim3Duration > 0f) yield return new WaitForSeconds(anim3Duration);
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

        private void TriggerImplosion(Vector3 point)
        {
            float implosionRadius = 3.5f;
            int hitCount = Physics2D.OverlapCircleNonAlloc(point, implosionRadius, ExplosionResults);

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
            // Cyan ring for magnetic repulsion radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magneticRadius);

            // Magenta ring for dark portal player trigger radius
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, portalTriggerRadius);
        }
    }
}