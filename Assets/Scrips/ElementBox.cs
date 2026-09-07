using UnityEngine;

namespace Scrips
{
    public enum BoxType { Light, Dark }

    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public class PushableBox : MonoBehaviour
    {
        [Header("Box Identity")]
        public BoxType boxType;

        [Header("Combination Prefabs")]
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private GameObject platformPrefab;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Check if we collided with another box
            if (collision.gameObject.TryGetComponent<PushableBox>(out var otherBox))
            {
                // Prevent double execution: only the box with the lower Instance ID handles the collision
                if (GetInstanceID() < otherBox.GetInstanceID())
                {
                    Vector3 contactPoint = collision.GetContact(0).point;
                    CombineBoxes(this.boxType, otherBox.boxType, contactPoint);

                    // Destroy both physical boxes upon merging
                    Destroy(otherBox.gameObject);
                    Destroy(gameObject);
                }
            }
        }

        private void CombineBoxes(BoxType typeA, BoxType typeB, Vector3 point)
        {
            // 1. Light + Light = Explosion
            if (typeA == BoxType.Light && typeB == BoxType.Light)
            {
                TriggerExplosion(point);
            }
            // 2. Dark + Dark = Teleport Player
            else if (typeA == BoxType.Dark && typeB == BoxType.Dark)
            {
                TriggerTeleport(point);
            }
            // 3. Light + Dark = New Platform
            else
            {
                SpawnPlatform(point);
            }
        }

        private void TriggerExplosion(Vector3 point)
        {
            // 1. Spawn explosion visuals/particles if assigned
            if (explosionEffectPrefab) 
                Instantiate(explosionEffectPrefab, point, Quaternion.identity);

            // 2. Define explosion radius
            float explosionRadius = 4f;

            // Find all colliders within the explosion area
            Collider2D[] affected = Physics2D.OverlapCircleAll(point, explosionRadius);

            foreach (var col in affected)
            {
                // Check if the affected object is a Breakable Wall
                if (col.TryGetComponent<BreakableWall>(out var wall))
                {
                    wall.Break();
                }

                // Apply knockback force to rigidbodies (Player, other boxes, physics props)
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

            // Find all active teleport targets in the level
            GameObject[] targets = GameObject.FindGameObjectsWithTag("TeleportTarget");

            if (targets.Length > 0)
            {
                GameObject nearestTarget = null;
                float shortestDistance = float.MaxValue;

                // Find the target closest to the collision point
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
                // Fallback: Teleport to the collision point if no targets are found
                player.transform.position = point;
            }
        }

        private void SpawnPlatform(Vector3 point)
        {
            if (platformPrefab)
            {
                Instantiate(platformPrefab, point, Quaternion.identity);
            }
        }
    }
}
