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
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Touch UI References")]
        [SerializeField] private TouchButton leftButton;
        [SerializeField] private TouchButton rightButton;
        [SerializeField] private TouchButton jumpButton;

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private bool _isGrounded;
        
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
            // Ground detection
            if (groundCheck != null)
            {
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
                _isGrounded = false;

                foreach (var col in hitColliders)
                {
                    if (col.gameObject != gameObject)
                    {
                        _isGrounded = true;
                        break;
                    }
                }
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
            HandleHorizontalMovement();
            HandleJump();
            ApplyFallGravityScaling();
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
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}