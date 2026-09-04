using UnityEngine;
using UnityEngine.InputSystem; // Required for New Input System

namespace Scrips
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement Tuning")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Touch UI References")]
        [SerializeField] private TouchButton leftButton;
        [SerializeField] private TouchButton rightButton;

        private Rigidbody2D _rb;
        private bool _isGrounded;
        private bool _jumpRequested;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Ground detection
            if (groundCheck != null)
            {
                _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }

            // Keyboard testing fallback for Unity Editor (New Input System)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || 
                    Keyboard.current.wKey.wasPressedThisFrame || 
                    Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    OnJumpPressed();
                }
            }
        }

        private void FixedUpdate()
        {
            HandleHorizontalMovement();
            HandleJump();
            ApplyJumpGravityScaling();
        }

        private void HandleHorizontalMovement()
        {
            float direction = 0f;

            // UI Touch Inputs
            if (leftButton && leftButton.IsPressed) direction -= 1f;
            if (rightButton && rightButton.IsPressed) direction += 1f;

            // Editor Keyboard Input Override (New Input System)
            if (direction == 0f && Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direction -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direction += 1f;
            }

            // Direct velocity assignment
            _rb.linearVelocity = new Vector2(direction * moveSpeed, _rb.linearVelocity.y);
        }

        // Must be PUBLIC so the UI Button component can trigger it
        public void OnJumpPressed()
        {
            if (_isGrounded)
            {
                _jumpRequested = true;
            }
        }

        private void HandleJump()
        {
            if (_jumpRequested)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _jumpRequested = false;
            }
        }

        private void ApplyJumpGravityScaling()
        {
            bool jumpHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

            // Snappy falling physics
            if (_rb.linearVelocity.y < 0)
            {
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
            }
            else if (_rb.linearVelocity.y > 0 && !jumpHeld)
            {
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime);
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