using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrips
{
    public class Hazard : MonoBehaviour
    {
        [Header("Hazard Settings")]
        [SerializeField] private bool reloadSceneOnTouch = true;
        [SerializeField] private GameObject deathEffectPrefab;

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
                // Optional: Spawn blood/sparks effect on death
                if (deathEffectPrefab != null)
                {
                    Instantiate(deathEffectPrefab, target.transform.position, Quaternion.identity);
                }

                if (reloadSceneOnTouch)
                {
                    // Instant restart on touching hazard
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                else
                {
                    Destroy(target);
                }
            }
        }
    }
}