fixusing UnityEngine;

namespace Scrips
{
    [RequireComponent(typeof(Collider2D))]
    public class BoxFilterWall : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private string playerTag = "Player";

        private Collider2D _wallCollider;

        private void Awake()
        {
            _wallCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            // Ignore collisions with any player already present in the scene
            GameObject player = GameObject.FindWithTag(playerTag);
            if (player != null)
            {
                IgnorePlayer(player);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Fallback for players spawned at runtime
            if (collision.gameObject.CompareTag(playerTag) || collision.gameObject.TryGetComponent<PlayerController2D>(out _))
            {
                Physics2D.IgnoreCollision(collision.collider, _wallCollider, true);
            }
        }

        private void IgnorePlayer(GameObject player)
        {
            Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>();
            foreach (var col in playerColliders)
            {
                Physics2D.IgnoreCollision(col, _wallCollider, true);
            }
        }
    }
}