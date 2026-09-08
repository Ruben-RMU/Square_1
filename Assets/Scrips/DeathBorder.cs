using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrips
{
    public class DeathBorder : MonoBehaviour
    {
        [SerializeField] private bool reloadSceneOnPlayerDeath = true;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 1. If Player falls out, reload current scene
            if (collision.CompareTag("Player"))
            {
                if (reloadSceneOnPlayerDeath)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                else
                {
                    Destroy(collision.gameObject);
                }
                return;
            }

            // 2. Destroy any falling boxes or debris to keep the scene clean
            Destroy(collision.gameObject);
        }
    }
}