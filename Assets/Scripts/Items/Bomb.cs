using UnityEngine;
using System.Collections;

namespace PixelWorld
{
    public class Bomb : MonoBehaviour
    {
        [SerializeField] private float fuseTime = 1.5f;
        [SerializeField] private float explosionRadius = 200f; // Large crater
        [SerializeField] private float knockbackForce = 15f;
        [SerializeField] private GameObject explosionEffect;
        [SerializeField] private int splashCount = 30; 

        [Header("Physics")]
        [SerializeField] private float gravity = 50f;
        private Vector2 _velocity;
        private bool _isGrounded;
        private int _framesSubmerged = 0;

        private SpriteRenderer _sr;
        private Vector3 _originalScale;

        private void Update()
        {
            // Track submersion to prevent chain reactions from stray droplets
            if (PixelCollisionSystem.Instance != null && PixelCollisionSystem.Instance.HasData)
            {
                // Check center for water
                if (PixelCollisionSystem.Instance.GetMaterialAt(transform.position) == 4)
                    _framesSubmerged++;
                else
                    _framesSubmerged = 0;
            }

            if (_isGrounded) return;

            // Apply Gravity
            _velocity.y -= gravity * Time.deltaTime;

            // Move
            Vector3 nextPos = transform.position + (Vector3)_velocity * Time.deltaTime;

            // Check Collision
            if (PixelCollisionSystem.Instance != null && PixelCollisionSystem.Instance.HasData)
            {
                // Check bottom of the bomb (approximate radius 0.15 based on 0.3 scale)
                Vector2 checkPos = (Vector2)nextPos + Vector2.down * 0.15f;
                
                // Check material at position
                int matID = PixelCollisionSystem.Instance.GetMaterialAt(checkPos);
                
                // Water (ID 4) logic: Sink slowly
                if (matID == 4)
                {
                    // Apply drag/buoyancy in water
                    _velocity.y *= 0.9f; // Drag
                    _velocity.y = Mathf.Max(_velocity.y, -2f); // Terminal velocity in water
                    
                    // Continue moving (sinking)
                    transform.position = nextPos;
                }
                // Solid Ground (Rock=1, Dirt=2, Sand=3)
                else if (PixelCollisionSystem.Instance.IsSolid(checkPos))
                {
                    _isGrounded = true;
                    _velocity = Vector2.zero;
                }
                else
                {
                    // Air / Empty
                    transform.position = nextPos;
                }
            }
            else
            {
                transform.position = nextPos;
            }
        }

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            _originalScale = transform.localScale;
            
            // Play bomb placement sound
            AudioEventTriggers.OnBombPlaced();
            
            // Load the bomb sprite if not set (fallback)
            if (_sr != null && _sr.sprite == null)
            {
                // Try to load from Resources or just use what's there
                // Since we can't easily load non-Resources assets at runtime without Addressables/AssetBundles,
                // we rely on the Prefab having the sprite assigned.
            }

            StartCoroutine(ExplodeRoutine());
        }

        private IEnumerator ExplodeRoutine()
        {
            // Pulse 3 times
            float pulseDuration = fuseTime / 3f;
            
            for (int i = 0; i < 3; i++)
            {
                float timer = 0;
                while (timer < pulseDuration)
                {
                    timer += Time.deltaTime;
                    float t = timer / pulseDuration;
                    
                    // Scale up and down
                    float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                    transform.localScale = _originalScale * scale;
                    
                    // Color flash red at peak
                    if (_sr != null)
                    {
                        _sr.color = Color.Lerp(Color.white, Color.red, Mathf.Sin(t * Mathf.PI));
                    }

                    yield return null;
                }
            }

            yield return StartCoroutine(ExplodeSequence());
        }

        private IEnumerator ExplodeSequence()
        {
            // Play explosion sound
            AudioEventTriggers.OnBombExplode();
            
            if (PixelWorldManager.Instance != null)
            {
                // Determine debris type based on surroundings
                int debrisMatID = 3; // Default to Sand
                bool spawnDebris = true;

                if (PixelCollisionSystem.Instance != null)
                {
                    // Use tracked submersion to decide if this is a water explosion
                    // This prevents chain reactions where a bomb gets splashed by a drop 
                    // and then explodes as if it were underwater, creating more water.
                    if (_framesSubmerged > 5)
                    {
                        debrisMatID = 4; // Water splash
                    }
                    else
                    {
                        // Check if we are in air (for no debris)
                        int center = PixelCollisionSystem.Instance.GetMaterialAt(transform.position);
                        int down = PixelCollisionSystem.Instance.GetMaterialAt(transform.position + Vector3.down * 0.2f);
                        
                        if (center == 0 && down == 0)
                        {
                            spawnDebris = false; // Air explosion, no debris
                        }
                    }
                }

                // 1. Create Crater (Empty = 0)
                PixelWorldManager.Instance.ModifyWorld(transform.position, explosionRadius, 0);
                
                // 2. Apply Knockback to Player
                float worldRadius = explosionRadius * PixelWorldManager.Instance.CellSize;
                PlayerController2D player = FindObjectOfType<PlayerController2D>();
                
                if (player != null)
                {
                    Vector2 dir = player.transform.position - transform.position;
                    float dist = dir.magnitude;
                    
                    // Check if player is within explosion range (plus a little buffer)
                    if (dist < worldRadius * 1.5f)
                    {
                        // Normalize direction
                        dir.Normalize();
                        
                        // Ensure we have some horizontal component if perfectly vertical
                        if (Mathf.Abs(dir.x) < 0.1f)
                        {
                            dir.x = Random.Range(-0.5f, 0.5f);
                            dir.Normalize();
                        }

                        // Calculate force (stronger when closer)
                        // Linear falloff: 1 at center, 0 at max range
                        float falloff = 1f - Mathf.Clamp01(dist / (worldRadius * 1.5f));
                        
                        // Apply force
                        Vector2 knockback = dir * knockbackForce * falloff;
                        
                        // Add a bit of guaranteed upward force for better feel
                        knockback.y += knockbackForce * 0.2f * falloff;
                        
                        player.ApplyVelocity(knockback);
                    }
                }

                yield return null;

                // 3. Splash Debris (Displacement)
                
                if (spawnDebris)
                {
                    // Adjust splash settings based on material
                    int count = splashCount;
                    float minRadius = 30f;
                    float maxRadius = 50f;

                    if (debrisMatID == 4) // Water
                    {
                        // Water splashes should be smaller droplets and less volume overall
                        // to avoid flooding the world.
                        // Drastically reduced to prevent "infinite water" exploits
                        count = 15; 
                        minRadius = 3f; 
                        maxRadius = 6f;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        Vector2 randomDir = Random.insideUnitCircle;
                        randomDir.y = Mathf.Abs(randomDir.y); // Upward splash
                        
                        Vector3 offset = randomDir * worldRadius * 1.5f;
                        Vector3 splashPos = transform.position + offset;
                        
                        float blobRadius = Random.Range(minRadius, maxRadius); 
                        
                        PixelWorldManager.Instance.ModifyWorld(splashPos, blobRadius, debrisMatID);
                        yield return null;
                    }
                }
            }

            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
