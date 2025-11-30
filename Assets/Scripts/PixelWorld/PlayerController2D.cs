using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace PixelWorld
{
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = 20f;
        
        [Header("Collision")]
        [SerializeField] private float playerHeight = 1.0f;
        [SerializeField] private float playerWidth = 0.5f;
        [SerializeField] private int verticalRays = 3;
        [Tooltip("Minimum number of solid pixels required to register a ground collision when falling.")]
        [SerializeField] private int groundCollisionThreshold = 2;
        [SerializeField] private GameObject bombPrefab;

        private Vector2 _velocity;
        private Vector2 _moveInput;
        private bool _isGrounded;
        private bool _isCrouching;
        private bool _isAttacking;
        private bool _isDigging;
        private float _attackTimer;
        private int _weaponType = 0; // 0=Unarmed, 1=Sword, 2=Gun
        
        private PixelCollisionSystem _collision;
        private Animator _animator;
        private SpriteRenderer _visualRenderer;
        private PlayerInput _playerInput;

        // Public getters for debugging and other systems
        public bool IsGrounded => _isGrounded;
        public bool IsCrouching => _isCrouching;
        public Vector2 Velocity => _velocity;
        public float PlayerWidth => playerWidth;
        public float PlayerHeight => playerHeight;

        private float _knockbackX;
        [SerializeField] private float knockbackDecay = 5f;

        public void ApplyVelocity(Vector2 velocity)
        {
            _knockbackX += velocity.x;
            _velocity.y += velocity.y;
        }

        private void Start()
        {
            _collision = PixelCollisionSystem.Instance;
            _animator = GetComponentInChildren<Animator>();
            _visualRenderer = GetComponentInChildren<SpriteRenderer>();
            _playerInput = GetComponent<PlayerInput>();

            if (bombPrefab == null)
            {
                Debug.LogError("PlayerController2D: bombPrefab is NULL in Start()!");
            }
            else
            {
                Debug.Log($"PlayerController2D: bombPrefab is assigned: {bombPrefab.name}");
            }

            if (_playerInput == null)
            {
                Debug.LogError("PlayerController2D: No PlayerInput component found!");
            }
            else
            {
                Debug.Log($"PlayerController2D: PlayerInput found. Action Map: {_playerInput.defaultActionMap}, Behavior: {_playerInput.notificationBehavior}");
            }
        }

        private void Update()
        {
            if (_collision == null)
            {
                _collision = PixelCollisionSystem.Instance;
                if (_collision == null)
                {
                    return;
                }
            }

            if (!_collision.HasData)
            {
                _velocity = Vector2.zero;
                return;
            }

            // Digging logic moved to OnInteract for single-press action

            if (_isAttacking)
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0)
                {
                    _isAttacking = false;
                }
                else
                {
                    _velocity.x = 0;
                    ApplyGravity();
                    Move(); 
                    return;
                }
            }

            ProcessMovement();
            ApplyGravity();
            Move();
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (_animator != null)
            {
                float speed = Mathf.Abs(_velocity.x);
                
                _animator.SetFloat("Speed", speed);
                _animator.SetBool("IsGrounded", _isGrounded);
                _animator.SetFloat("VerticalVelocity", _velocity.y);
                _animator.SetBool("IsCrouching", _isCrouching);
                // _animator.SetBool("IsDigging", _isDigging); // Parameter missing in Animator
                _animator.SetInteger("WeaponType", _weaponType);
            }

            if (_visualRenderer != null && Mathf.Abs(_velocity.x) > 0.1f)
            {
                _visualRenderer.flipX = _velocity.x < 0;
            }
        }

        // Input System Messages
        
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed && _isGrounded)
            {
                _velocity.y = jumpForce;
                AudioEventTriggers.OnPlayerJump();
            }
        }

        public void OnCrouch(InputValue value)
        {
            bool wasCrouching = _isCrouching;
            _isCrouching = value.isPressed;
            
            if (_isCrouching && !wasCrouching)
            {
                AudioEventTriggers.OnPlayerCrouch();
            }
        }

        public void OnAttack(InputValue value)
        {
            // Attack logic if needed (e.g. sword swing)
            if (value.isPressed && _animator != null) 
            {
                _animator.SetTrigger("Attack");
            }
        }

        public void OnInteract(InputValue value)
        {
            int selectedTool = HotbarController.Instance != null ? HotbarController.Instance.SelectedIndex : 0;
            Debug.Log($"OnInteract: Selected Tool Index: {selectedTool}, IsPressed: {value.isPressed}");

            if (selectedTool == 3) // Bomb
            {
                if (value.isPressed)
                {
                    Debug.Log("Attempting to spawn bomb...");
                    // Place bomb (One-shot)
                    if (bombPrefab != null)
                    {
                        // Calculate spawn position based on facing direction
                        float facingDir = 1f;
                        if (_visualRenderer != null && _visualRenderer.flipX)
                        {
                            facingDir = -1f;
                        }
                        else if (_moveInput.x != 0)
                        {
                            facingDir = Mathf.Sign(_moveInput.x);
                        }

                        Vector3 spawnOffset = new Vector3(facingDir * 1.5f, 0, 0); // 1.5 units in front
                        Vector3 spawnPos = transform.position + spawnOffset;

                        Instantiate(bombPrefab, spawnPos, Quaternion.identity);
                        AudioEventTriggers.OnBombPlaced();
                        Debug.Log($"Bomb spawned at {spawnPos} (Offset: {spawnOffset})");
                    }
                    else
                    {
                        Debug.LogError("Bomb Prefab is null!");
                    }
                }
                _isDigging = false;
            }
            else
            {
                // Dig / Sand / Water (Single Press)
                _isDigging = value.isPressed;
                
                if (value.isPressed)
                {
                    PerformToolAction(selectedTool);
                }
            }
        }

        private void PerformToolAction(int selectedTool)
        {
            Vector3 digDirection = Vector3.down; // Default to down
            
            // If moving, dig in that direction
            if (_moveInput.sqrMagnitude > 0.1f)
            {
                digDirection = _moveInput.normalized;
            }
            else if (_visualRenderer != null && _visualRenderer.flipX)
            {
                // If not moving but facing left, dig left? 
                // Or keep default down? 
                // Usually digging is directional. Let's assume down if idle, or maybe forward?
                // The original code defaulted to down.
                // Let's check if we want to dig forward when idle.
                // For now, keeping original logic: Default to down.
            }
            
            // Offset the dig position based on direction
            Vector3 digPos = transform.position + digDirection * 0.5f;
            
            int matID = 0; // Dig
            if (selectedTool == 1) matID = 3; // Sand
            if (selectedTool == 2) matID = 4; // Water

            if (matID == 3) // Sand
            {
                // Spawn sand cloud effect
                StartCoroutine(SpawnSandCloud(digPos));
                AudioEventTriggers.OnPaintSand();
            }
            else
            {
                // Radius 19 (approx 10% bigger than 17)
                // Reduced water radius by 50% (from 19 to 9.5)
                float radius = (matID == 4) ? 9.5f : 19f;
                PixelWorldManager.Instance.ModifyWorld(digPos, radius, matID);

                if (matID == 4) // Water
                {
                    AudioEventTriggers.OnPaintWater();
                }
                else if (matID == 0) // Dig
                {
                    AudioEventTriggers.OnDig();
                }
            }
        }

        private IEnumerator SpawnSandCloud(Vector3 centerPos)
        {
            // Increased particle count by 70% (from 12 to ~20)
            int particleCount = 20;
            float duration = 0.15f;
            
            for (int i = 0; i < particleCount; i++)
            {
                // Random offset for cloud effect
                Vector2 randomOffset = Random.insideUnitCircle * 0.8f;
                Vector3 spawnPos = centerPos + (Vector3)randomOffset;
                
                // Smaller radius for individual particles
                float particleRadius = Random.Range(3f, 6f);
                
                PixelWorldManager.Instance.ModifyWorld(spawnPos, particleRadius, 3); // 3 = Sand
                
                // Stagger the spawning slightly
                if (i % 3 == 0) yield return new WaitForSeconds(duration / particleCount);
            }
        }

        public void OnNext(InputValue value)
        {
            if (value.isPressed && HotbarController.Instance != null)
            {
                HotbarController.Instance.CycleSlot(1);
            }
        }

        public void OnPrevious(InputValue value)
        {
            if (value.isPressed && HotbarController.Instance != null)
            {
                HotbarController.Instance.CycleSlot(-1);
            }
        }

        private void ProcessMovement()
        {
            // Decay knockback
            _knockbackX = Mathf.Lerp(_knockbackX, 0, Time.deltaTime * knockbackDecay);

            if (_isCrouching)
            {
                _velocity.x = _knockbackX;
            }
            else
            {
                _velocity.x = (_moveInput.x * moveSpeed) + _knockbackX;
            }
        }

        private void PerformAttack()
        {
            if (_animator != null) _animator.SetTrigger("Attack");
            _isAttacking = true;
            _attackTimer = 0.4f; 
            _velocity.x = 0;
        }

        private void ApplyGravity()
        {
            _velocity.y -= gravity * Time.deltaTime;
        }

        private void Move()
        {
            Vector2 pos = transform.position;
            Vector2 delta = _velocity * Time.deltaTime;

            // Horizontal Collision
            float dirX = Mathf.Sign(delta.x);
            if (delta.x != 0)
            {
                Vector2 sideCheckPos = pos + new Vector2(dirX * (playerWidth * 0.5f), 0); 
                if (_collision.IsSolid(sideCheckPos + new Vector2(delta.x, 0)))
                {
                    _velocity.x = 0;
                    delta.x = 0;
                }
            }
            
            pos.x += delta.x;

            // Vertical Collision (Ground Check)
            bool wasGrounded = _isGrounded;
            _isGrounded = false;
            if (delta.y < 0) // Falling
            {
                bool hitGround = false;
                float feetY = pos.y - (playerHeight * 0.5f);
                
                for (int i = 0; i < verticalRays; i++)
                {
                    float t = (float)i / (verticalRays - 1);
                    float rayX = Mathf.Lerp(pos.x - (playerWidth * 0.4f), pos.x + (playerWidth * 0.4f), t);
                    
                    // Use IsSolidDown with threshold to ignore tiny floating pixels
                    if (_collision.IsSolidDown(new Vector2(rayX, feetY + delta.y), groundCollisionThreshold))
                    {
                        hitGround = true;
                        break;
                    }
                }

                if (hitGround)
                {
                    _isGrounded = true;
                    _velocity.y = 0;
                    
                    // Play land sound if just landed with decent velocity
                    if (!wasGrounded && Mathf.Abs(_velocity.y) > 2f)
                    {
                        AudioEventTriggers.OnPlayerLand();
                    }
                }
                else
                {
                    pos.y += delta.y;
                }
            }
            else if (delta.y > 0) // Jumping Up
            {
                float headY = pos.y + (playerHeight * 0.5f);
                 if (_collision.IsSolid(new Vector2(pos.x, headY + delta.y)))
                 {
                     _velocity.y = 0; 
                 }
                 else
                 {
                     pos.y += delta.y;
                 }
            }

            transform.position = pos;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(playerWidth, playerHeight, 0));
        }
    }
}
