using System.Collections;
using UnityEngine;

namespace Scrips
{
    public class BoxRespawner : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("Prefab to spawn. Must have an ElementBox component.")]
        [SerializeField] private ElementBox boxPrefab;

        [Tooltip("Optional: a box already placed in the scene that this spawner should watch. " +
                 "Leave empty to spawn one on Start.")]
        [SerializeField] private ElementBox existingBox;

        [Tooltip("Where to spawn. Uses this object's position if left empty.")]
        [SerializeField] private Transform spawnPoint;

        [Header("Timing")]
        [SerializeField] private float respawnDelay = 2f;

        [Header("Spawn Safety (optional)")]
        [Tooltip("Wait until nothing on 'blockingMask' overlaps the spawn point (e.g. the player). Set radius to 0 to disable.")]
        [SerializeField] private float clearRadius = 0.6f;
        [SerializeField] private LayerMask blockingMask;

        private ElementBox _current;

        private Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

        private void Start()
        {
            _current = existingBox;
            StartCoroutine(WatchRoutine());
        }

        private IEnumerator WatchRoutine()
        {
            while (true)
            {
                // Nothing to watch yet -> spawn straight away. Otherwise wait for the box to be destroyed.
                if (_current != null)
                {
                    // Unity overloads == so a destroyed object compares equal to null.
                    yield return new WaitUntil(() => _current == null);
                    yield return new WaitForSeconds(respawnDelay);
                }

                // Don't spawn on top of the player or other blockers.
                if (clearRadius > 0f)
                {
                    yield return new WaitUntil(IsSpawnPointClear);
                }

                _current = Instantiate(boxPrefab, SpawnPosition, Quaternion.identity);
            }
        }

        private bool IsSpawnPointClear()
        {
            return Physics2D.OverlapCircle(SpawnPosition, clearRadius, blockingMask) == null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(SpawnPosition, clearRadius > 0f ? clearRadius : 0.25f);
        }
    }
}