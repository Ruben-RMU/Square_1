using UnityEngine;

namespace Scrips
{
    public class DeathBorder : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool instantKill = true;
        [SerializeField] private int damageAmount = 1;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerController2D player = collision.GetComponentInParent<PlayerController2D>();

                if (player != null)
                {
                    if (instantKill)
                    {
                        player.TakeDamage(player.CurrentLives);
                    }
                    else
                    {
                        player.TakeDamage(damageAmount);
                    }
                }
                return;
            }
            
            Destroy(collision.gameObject);
        }
    }
}