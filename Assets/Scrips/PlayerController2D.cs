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
        private bool _isDead;
        private bool _inputLocked;

        [Header("Movement Tuning")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float fallMultiplier = 2.5f;

        [Header("Climbing Tuning")]
        [SerializeField] private float climbSpeed = 5f;
        [SerializeField] private float wallCheckDistance = 0.55f;
        [SerializeField] private LayerMask climbableLayer;

        [Header("Jump Assist")]
        [SerializeField] private float coyoteTime = 0.15f;
        private float _coyoteTimeCounter;
        private bool _jumpRequested;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckWidth = 0.4f;

        [Header("Touch UI References")]
        [SerializeField] private TouchButton leftButton;
        [SerializeField] private TouchButton rightButton;
        [SerializeField] private TouchButton jumpButton;

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private bool _isGrounded;
        private bool _isTouchingWall;
        private bool _isClimbing;
        private float _originalGravityScale;
        private bool _isKnockedBack;
        private Coroutine _knockbackCoroutine;

        public int CurrentLives => _currentLives;
        public System.Action<int> OnLivesChanged;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _currentLives = maxLives;
            _originalGravityScale = _rb.gravityScale;
        }

        private void Start()
        {
            OnLivesChanged?.Invoke(_currentLives);
        }

        public void SetInputLock(bool locked)
        {
            _inputLocked = locked;
            if (locked && _rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        private void Update()
        {
            if (_isKnockedBack || _isDead || _inputLocked) return;

            if (groundCheck != null)
            {
                _isGrounded = CheckGrounded();
            }

            _isTouchingWall = CheckTouchingWall();

            float horizontalInput = GetHorizontalInput();

            // CLIMBING STATE CONDITIONAL:
            // 1. Must be touching a climbable wall.
            // 2. If grounded and pressing Left (-1), release climb so player can walk away/down smoothly.
            // 3. Otherwise, engage climbing whenever pressing input or airborne against a wall.
            if (_isTouchingWall)
            {
                if (_isGrounded && horizontalInput < 0f)
                {
                    _isClimbing = false;
                }
                else if (horizontalInput != 0f || !_isGrounded)
                {
                    _isClimbing = true;
                }
            }
            else
            {
                _isClimbing = false;
            }

            // Coyote time active during ground touch OR climbing state
            if (_isGrounded || _isClimbing)
            {
                _coyoteTimeCounter = coyoteTime;
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }

            if (WasJumpPressed())
            {
                _jumpRequested = true;
            }
        }

        private void FixedUpdate()
        {
            if (_isKnockedBack || _isDead || _inputLocked) return;

            if (_isClimbing)
            {
                HandleClimbing();
            }
            else
            {
                _rb.gravityScale = _originalGravityScale;
                HandleHorizontalMovement();
                ApplyFallGravityScaling();
            }

            HandleJump();
        }

        private bool CheckGrounded()
        {
            Vector2 center = groundCheck.position;
            Vector2 right = center + Vector2.right * (groundCheckWidth * 0.5f);
            Vector2 left = center + Vector2.left * (groundCheckWidth * 0.5f);

            return RayHitsGround(center) || RayHitsGround(left) || RayHitsGround(right);
        }

        private bool RayHitsGround(Vector2 origin)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
            return hit.collider != null && !hit.collider.transform.IsChildOf(transform);
        }

        private bool CheckTouchingWall()
        {
            Vector2 position = transform.position;
            RaycastHit2D hitRight = Physics2D.Raycast(position, Vector2.right, wallCheckDistance, climbableLayer);
            RaycastHit2D hitLeft = Physics2D.Raycast(position, Vector2.left, wallCheckDistance, climbableLayer);

            return (hitRight.collider != null && !hitRight.collider.transform.IsChildOf(transform)) ||
                   (hitLeft.collider != null && !hitLeft.collider.transform.IsChildOf(transform));
        }

        private bool WasJumpPressed()
        {
            bool uiPressed = jumpButton != null && jumpButton.IsPressed;
            bool keyboardPressed = Keyboard.current != null && (
                Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.upArrowKey.wasPressedThisFrame
            );

            return uiPressed || keyboardPressed;
        }

        private void HandleJump()
        {
            if (_jumpRequested && _coyoteTimeCounter > 0f)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _coyoteTimeCounter = 0f;
                _jumpRequested = false;
                _isClimbing = false;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.IncrementJumps();
                }
            }

            if (_coyoteTimeCounter <= 0f)
            {
                _jumpRequested = false;
            }
        }

        private void HandleHorizontalMovement()
        {
            float direction = GetHorizontalInput();
            _rb.linearVelocity = new Vector2(direction * moveSpeed, _rb.linearVelocity.y);
        }

        private void HandleClimbing()
        {
            _rb.gravityScale = 0f;

            float inputDirection = GetHorizontalInput();
            float verticalVelocity = inputDirection * climbSpeed;

            // If moving down and about to hit the ground, apply a subtle outward push 
            // to disengage smoothly without getting clipped on wall corners
            float horizontalPush = 0f;
            if (_isGrounded && inputDirection < 0f)
            {
                horizontalPush = -0.1f;
            }

            _rb.linearVelocity = new Vector2(horizontalPush, verticalVelocity);
        }

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

        private void ApplyFallGravityScaling()
        {
            if (_rb.linearVelocity.y < 0)
            {
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
            }
        }

        public void ApplyKnockback(Vector2 force, float duration = 0.25f)
        {
            if (_isDead) return;

            if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = StartCoroutine(KnockbackRoutine(force, duration));
        }

        private IEnumerator KnockbackRoutine(Vector2 force, float duration)
        {
            _isKnockedBack = true;
            _isClimbing = false;

            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(force, ForceMode2D.Impulse);

            yield return new WaitForSeconds(duration);

            float recoveryDuration = 0.67f;
            float elapsed = 0f;

            Vector2 startVelocity = _rb.linearVelocity;

            while (elapsed < recoveryDuration && !_isDead)
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

        public void TakeDamage(int damageAmount = 1)
        {
            if (_currentLives <= 0 || _isInvincible || _isDead) return;

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
                while (elapsed < invincibilityDuration && !_isDead)
                {
                    _spriteRenderer.enabled = !_spriteRenderer.enabled;
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }

                if (!_isDead)
                {
                    _spriteRenderer.enabled = true;
                }
            }
            else
            {
                yield return new WaitForSeconds(invincibilityDuration);
            }

            _isInvincible = false;
        }

        public void Die()
        {
            if (_isDead) return;

            _isDead = true;
            _currentLives = 0;
            OnLivesChanged?.Invoke(_currentLives);

            StartCoroutine(DieRoutine());
        }

        private IEnumerator DieRoutine()
        {
            this.enabled = false;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.simulated = false;
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = false;
            }

            yield return new WaitForSeconds(1.0f);

            DeathScreenManager.RecordCurrentLevel();
            SceneManager.LoadScene("Death Screen");
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Vector3 center = groundCheck.position;
                Vector3 right = center + Vector3.right * (groundCheckWidth * 0.5f);
                Vector3 left = center + Vector3.left * (groundCheckWidth * 0.5f);

                Gizmos.DrawLine(center, center + Vector3.down * groundCheckDistance);
                Gizmos.DrawLine(left, left + Vector3.down * groundCheckDistance);
                Gizmos.DrawLine(right, right + Vector3.down * groundCheckDistance);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance);
        }
    }
}