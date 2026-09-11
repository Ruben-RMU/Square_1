using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Scrips
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Lives & Health")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float invincibilityDuration = 1.5f;
        private int _currentLives;
        private bool _isInvincible;

        [Header("Movement Tuning")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float fallMultiplier = 2.5f;

        [Header("Jump Assist")]
        [SerializeField] private float coyoteTime = 0.15f;
        private float _coyoteTimeCounter;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Touch UI References")]
        [SerializeField] private TouchButton leftButton;
        [SerializeField] private TouchButton rightButton;
        [SerializeField] private TouchButton jumpButton;

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private bool _isGrounded;
        private bool _isKnockedBack; // Knockback state flag
        private Coroutine _knockbackCoroutine;

        public int CurrentLives => _currentLives;
        public System.Action<int> OnLivesChanged;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _currentLives = maxLives;
        }

        private void Start()
        {
            OnLivesChanged?.Invoke(_currentLives);
        }

        private void Update()
        {
            if (_isKnockedBack) return; // Skip input processing during knockback

            // Ground detection via downward Raycast
            if (groundCheck != null)
            {
                RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
                _isGrounded = hit.collider != null && hit.collider.gameObject != gameObject;
            }

            // Coyote time calculation
            if (_isGrounded)
            {
                _coyoteTimeCounter = coyoteTime;
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (_isKnockedBack) return; // Stop movement code from overriding physics during knockback

            HandleHorizontalMovement();
            HandleJump();
            ApplyFallGravityScaling();
        }

        // Public method called by ElementBox.cs
        public void ApplyKnockback(Vector2 force, float duration = 0.25f)
        {
            if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = StartCoroutine(KnockbackRoutine(force, duration));
        }

        private IEnumerator KnockbackRoutine(Vector2 force, float duration)
        {
            _isKnockedBack = true;

            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(force, ForceMode2D.Impulse);

            yield return new WaitForSeconds(duration);

            float recoveryDuration = 0.67f;
            float elapsed = 0f;

            Vector2 startVelocity = _rb.linearVelocity;

            while (elapsed < recoveryDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / recoveryDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                float moveInput = GetHorizontalInput();
                Vector2 targetInputVelocity = new Vector2(moveInput * moveSpeed, _rb.linearVelocity.y);

                _rb.linearVelocity = Vector2.Lerp(startVelocity, targetInputVelocity, smoothT);

                yield return null;
            }

            _isKnockedBack = false;
        }

// Helper method to read movement direction safely during recovery
        private float GetHorizontalInput()
        {
            float direction = 0f;
            if (leftButton && leftButton.IsPressed) direction -= 1f;
            if (rightButton && rightButton.IsPressed) direction += 1f;

            if (direction == 0f && Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direction -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direction += 1f;
            }
            return direction;
        }

        private void Die()
        {
            Debug.Log("You Died");
            StartCoroutine(RestartSceneRoutine());
        }

        private IEnumerator RestartSceneRoutine()
        {
            this.enabled = false;

            if (_rb != null)
            {
                _rb.simulated = false;
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = false;
            }

            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene("Death Screen");
        }

        public void TakeDamage(int damageAmount = 1)
        {
            if (_currentLives <= 0 || _isInvincible) return;

            _currentLives -= damageAmount;
            OnLivesChanged?.Invoke(_currentLives);

            if (_currentLives <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }

        private IEnumerator InvincibilityRoutine()
        {
            _isInvincible = true;
            
            if (_spriteRenderer != null)
            {
                float elapsed = 0f;
                while (elapsed < invincibilityDuration)
                {
                    _spriteRenderer.enabled = !_spriteRenderer.enabled;
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }
                _spriteRenderer.enabled = true;
            }
            else
            {
                yield return new WaitForSeconds(invincibilityDuration);
            }

            _isInvincible = false;
        }

        private void HandleHorizontalMovement()
        {
            float direction = 0f;

            // UI Touch Inputs
            if (leftButton && leftButton.IsPressed) direction -= 1f;
            if (rightButton && rightButton.IsPressed) direction += 1f;

            // Editor Keyboard Input Override
            if (direction == 0f && Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direction -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direction += 1f;
            }

            _rb.linearVelocity = new Vector2(direction * moveSpeed, _rb.linearVelocity.y);
        }

        private bool IsJumpHeld()
        {
            bool UIHeld = jumpButton != null && jumpButton.IsPressed;
            bool keyboardHeld = Keyboard.current != null && (
                Keyboard.current.spaceKey.isPressed ||
                Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed
            );

            return UIHeld || keyboardHeld;
        }

        private void HandleJump()
        {
            if (IsJumpHeld() && _coyoteTimeCounter > 0f)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _coyoteTimeCounter = 0f;
            }
        }

        private void ApplyFallGravityScaling()
        {
            if (_rb.linearVelocity.y < 0)
            {
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
            }
        }
    }
}