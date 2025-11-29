using UnityEngine;

namespace PixelWorld
{
    /// <summary>
    /// Visualizes the world bounds in the Scene view to help verify coverage.
    /// Shows the complete play area boundaries including off-screen regions.
    /// </summary>
    public class WorldBoundsDebugger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PixelWorldManager worldManager;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showBounds = true;
        [SerializeField] private bool showGrid = false;
        [SerializeField] private Color boundsColor = Color.yellow;
        [SerializeField] private Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private float cellSize = 0.02f;
        
        [Header("Boundary Settings")]
        [SerializeField] private int boundaryThickness = 5; // Must match compute shader
        [SerializeField] private Color boundaryZoneColor = new Color(1f, 0f, 0f, 0.3f);

        private void OnDrawGizmos()
        {
            if (worldManager == null)
            {
                worldManager = FindObjectOfType<PixelWorldManager>();
                if (worldManager == null) return;
            }

            if (!showBounds && !showGrid) return;

            // Calculate world dimensions
            float worldWidth = worldManager.Width * cellSize;
            float worldHeight = worldManager.Height * cellSize;
            float halfWidth = worldWidth * 0.5f;
            float halfHeight = worldHeight * 0.5f;

            // Draw outer bounds
            if (showBounds)
            {
                Gizmos.color = boundsColor;
                
                // Draw box outline
                Vector3 bottomLeft = transform.position + new Vector3(-halfWidth, -halfHeight, 0);
                Vector3 bottomRight = transform.position + new Vector3(halfWidth, -halfHeight, 0);
                Vector3 topLeft = transform.position + new Vector3(-halfWidth, halfHeight, 0);
                Vector3 topRight = transform.position + new Vector3(halfWidth, halfHeight, 0);
                
                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);
                
                // Draw boundary zones (5-pixel thick rock walls)
                Gizmos.color = boundaryZoneColor;
                float boundaryWidth = boundaryThickness * cellSize;
                
                // Left boundary
                DrawBoundaryZone(
                    transform.position + new Vector3(-halfWidth, 0, 0),
                    new Vector3(boundaryWidth, worldHeight, 0.01f)
                );
                
                // Right boundary
                DrawBoundaryZone(
                    transform.position + new Vector3(halfWidth, 0, 0),
                    new Vector3(boundaryWidth, worldHeight, 0.01f)
                );
                
                // Bottom boundary
                DrawBoundaryZone(
                    transform.position + new Vector3(0, -halfHeight, 0),
                    new Vector3(worldWidth, boundaryWidth, 0.01f)
                );
                
                // Top boundary (THE FIX!)
                DrawBoundaryZone(
                    transform.position + new Vector3(0, halfHeight, 0),
                    new Vector3(worldWidth, boundaryWidth, 0.01f)
                );
            }

            // Draw grid
            if (showGrid)
            {
                Gizmos.color = gridColor;
                
                // Vertical lines (every 10 world units)
                for (float x = -halfWidth; x <= halfWidth; x += 10f)
                {
                    Vector3 start = transform.position + new Vector3(x, -halfHeight, 0);
                    Vector3 end = transform.position + new Vector3(x, halfHeight, 0);
                    Gizmos.DrawLine(start, end);
                }
                
                // Horizontal lines (every 10 world units)
                for (float y = -halfHeight; y <= halfHeight; y += 10f)
                {
                    Vector3 start = transform.position + new Vector3(-halfWidth, y, 0);
                    Vector3 end = transform.position + new Vector3(halfWidth, y, 0);
                    Gizmos.DrawLine(start, end);
                }
            }
        }

        private void DrawBoundaryZone(Vector3 center, Vector3 size)
        {
            // Draw wireframe cube for boundary zone
            Gizmos.DrawWireCube(center, size);
            
            // Draw semi-transparent fill
            Color fillColor = Gizmos.color;
            fillColor.a *= 0.2f;
            Gizmos.color = fillColor;
            Gizmos.DrawCube(center, size);
        }

        // Draw labels in Scene view
        private void OnDrawGizmosSelected()
        {
            if (worldManager == null) return;

            float worldWidth = worldManager.Width * cellSize;
            float worldHeight = worldManager.Height * cellSize;
            float halfWidth = worldWidth * 0.5f;
            float halfHeight = worldHeight * 0.5f;

            // Draw dimension labels
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            
            // Width label
            Vector3 bottomCenter = transform.position + new Vector3(0, -halfHeight - 1f, 0);
            UnityEditor.Handles.Label(bottomCenter, 
                $"Width: {worldManager.Width}px ({worldWidth:F2} units)");
            
            // Height label
            Vector3 leftCenter = transform.position + new Vector3(-halfWidth - 2f, 0, 0);
            UnityEditor.Handles.Label(leftCenter, 
                $"Height: {worldManager.Height}px ({worldHeight:F2} units)");
            
            // Boundary info
            Vector3 topCenter = transform.position + new Vector3(0, halfHeight + 1f, 0);
            UnityEditor.Handles.Label(topCenter, 
                $"Boundary: {boundaryThickness}px ({boundaryThickness * cellSize:F3} units) - ALL EDGES SEALED ✓");
            #endif
        }
    }
}

