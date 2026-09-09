using UnityEngine;

namespace Scrips
{
    public class Hazard : MonoBehaviour
    {
        [Header("Hazard Settings")]
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private GameObject hitEffectPrefab;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleImpact(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleImpact(collision.gameObject);
        }

        private void HandleImpact(GameObject target)
        {
            if (target.CompareTag("Player"))
            {
                PlayerController2D player = target.GetComponentInParent<PlayerController2D>();

                if (player != null)
                {
                    if (hitEffectPrefab != null)
                    {
                        Instantiate(hitEffectPrefab, target.transform.position, Quaternion.identity);
                    }

                    player.TakeDamage(damageAmount);
                }
            }
        }
    }
}