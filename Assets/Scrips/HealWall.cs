using UnityEngine;

namespace Scrips
{
    public class HealWall : MonoBehaviour
    {
        [Header("Healing")]
        [SerializeField] private int healAmount = 1;

        [Tooltip("If true, the wall works once and then can't heal anymore.")]
        [SerializeField] private bool singleUse = true;

        [Tooltip("Seconds before the wall can heal again (ignored if Single Use is on).")]
        [SerializeField] private float cooldown = 3f;

        [Tooltip("Optional: tint the wall to this color once it's used up.")]
        [SerializeField] private Color usedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        private float _nextHealTime;
        private bool _used;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryHeal(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHeal(other.gameObject);
        }

        private void TryHeal(GameObject other)
        {
            if (_used || Time.time < _nextHealTime) return;

            PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
            if (player == null) return;

            // Heal returns false if the player is already at full health,
            // so the wall isn't wasted.
            if (!player.Heal(healAmount)) return;

            if (singleUse)
            {
                _used = true;
                if (_spriteRenderer != null) _spriteRenderer.color = usedColor;
            }
            else
            {
                _nextHealTime = Time.time + cooldown;
            }
        }
    }
}