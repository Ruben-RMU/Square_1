using UnityEngine;

namespace Scrips
{
    public class BreakableWall : MonoBehaviour
    {
        [Header("Break Effects (Optional)")]
        [SerializeField] private GameObject breakEffectPrefab;

        public void Break()
        {
            // Spawn debris or particles if assigned
            if (breakEffectPrefab != null)
            {
                Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            }

            // Destroy the wall GameObject
            Destroy(gameObject);
        }
    }
}