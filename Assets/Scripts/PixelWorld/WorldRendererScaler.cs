using UnityEngine;

namespace PixelWorld
{
    /// <summary>
    /// Automatically scales the world renderer quad to match the PixelWorldManager dimensions.
    /// Ensures the entire play area is visible, including off-screen regions.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Transform))]
    public class WorldRendererScaler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PixelWorldManager worldManager;
        
        [Header("Settings")]
        [Tooltip("World units per pixel (should match PixelWorldManager's cell size)")]
        [SerializeField] private float cellSize = 0.02f;
        
        [Header("Info (Read-Only)")]
        [SerializeField] private Vector2 currentWorldSize;
        [SerializeField] private Vector2 currentScale;

        private void Start()
        {
            UpdateScale();
        }

        private void Update()
        {
            // Auto-update in editor for convenience
            if (Application.isEditor && !Application.isPlaying)
            {
                UpdateScale();
            }
        }

        /// <summary>
        /// Updates the transform scale to match the world dimensions
        /// </summary>
        public void UpdateScale()
        {
            if (worldManager == null)
            {
                worldManager = FindObjectOfType<PixelWorldManager>();
                if (worldManager == null)
                {
                    Debug.LogWarning("WorldRendererScaler: No PixelWorldManager found in scene!");
                    return;
                }
            }

            // Calculate world size in Unity units
            float worldWidth = worldManager.Width * cellSize;
            float worldHeight = worldManager.Height * cellSize;
            
            currentWorldSize = new Vector2(worldWidth, worldHeight);
            
            // Unity's default quad is 1x1, so scale directly to world size
            Vector3 newScale = new Vector3(worldWidth, worldHeight, 1f);
            transform.localScale = newScale;
            
            currentScale = new Vector2(newScale.x, newScale.y);
            
            Debug.Log($"WorldRendererScaler: Updated scale to {currentScale} (World: {worldManager.Width}x{worldManager.Height} pixels)");
        }

        /// <summary>
        /// Call this from the PixelWorldManager when dimensions change
        /// </summary>
        public void OnWorldDimensionsChanged()
        {
            UpdateScale();
        }
    }
}

