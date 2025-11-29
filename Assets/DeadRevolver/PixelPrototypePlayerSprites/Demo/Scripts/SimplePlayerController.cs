using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadRevolver.PixelPrototypePlayer {
    public class SimplePlayerController : MonoBehaviour {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float jumpForce = 10f;

        [Header("Animation States")]
        public string idleState = "Idle";
        public string runState = "Run";
        public string jumpState = "Jump";
        public string fallState = "JumpFall";

        private Rigidbody2D _rb;
        private Animator _anim;
        private SpriteRenderer _sr;
        private bool _isGrounded;
        private string _currentState;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();

            if (_rb == null) {
                _rb = gameObject.AddComponent<Rigidbody2D>();
                _rb.freezeRotation = true;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            
            // Ensure we have a collider
            if (GetComponent<Collider2D>() == null) {
                var col = gameObject.AddComponent<CapsuleCollider2D>();
                col.size = new Vector2(0.5f, 1.5f);
                col.offset = new Vector2(0, 0.75f);
            }
        }

        private void Update() {
            if (Keyboard.current == null) return;

            // Movement
            float moveInput = 0f;
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) {
                moveInput = -1f;
            } else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) {
                moveInput = 1f;
            }

            _rb.linearVelocity = new Vector2(moveInput * moveSpeed, _rb.linearVelocity.y);

            // Flip Sprite
            if (moveInput != 0) {
                _sr.flipX = moveInput < 0;
            }

            // Jump
            if ((Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) && _isGrounded) {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _isGrounded = false;
                ChangeAnimation(jumpState);
            }

            // Animation Logic
            if (_isGrounded) {
                if (moveInput != 0) {
                    ChangeAnimation(runState);
                } else {
                    ChangeAnimation(idleState);
                }
            } else {
                // Simple air logic
                if (_rb.linearVelocity.y < 0) {
                    ChangeAnimation(fallState);
                } else {
                    ChangeAnimation(jumpState);
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            // Simple ground check
            foreach (ContactPoint2D contact in collision.contacts) {
                if (contact.normal.y > 0.7f) {
                    _isGrounded = true;
                    break;
                }
            }
        }

        private void ChangeAnimation(string newState) {
            if (_currentState == newState) return;

            _anim.Play(newState);
            _currentState = newState;
        }
    }
}
