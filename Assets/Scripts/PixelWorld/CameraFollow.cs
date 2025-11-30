using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelWorld
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

        [Header("Settings")]
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-40, -20);
        [SerializeField] private Vector2 maxBounds = new Vector2(40, 20);

        [Header("Zoom")]
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 15f;
        [SerializeField] private float zoomSpeed = 10f;

        private Vector3 _currentVelocity;
        private Camera _cam;
        private float _targetZoom;
        
        // Cache world size to auto-set bounds if needed
        private PixelWorldManager _manager;

        private void Start()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null)
            {
                Debug.LogError("CameraFollow: No Camera component found!");
                return;
            }
            
            _targetZoom = _cam.orthographicSize;

            // Auto-find player if target not assigned
            if (target == null)
            {
                var player = GameObject.Find("Player-01");
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log("CameraFollow: Auto-found player target: " + player.name);
                }
                else
                {
                    Debug.LogWarning("CameraFollow: No target assigned and couldn't find 'Player-01'. Camera won't follow.");
                }
            }

            // Snap to target immediately to avoid initial drift
            if (target != null)
            {
                transform.position = target.position + offset;
            }

            _manager = FindObjectOfType<PixelWorldManager>();
            if (_manager != null)
            {
                // Auto-calculate bounds based on World Size
                // World is centered at 0,0.
                // Width = pixels * CellSize
                float w = _manager.Width * _manager.CellSize;
                float h = _manager.Height * _manager.CellSize;
                
                // Half dimensions
                float halfW = w / 2f;
                float halfH = h / 2f;

                // Camera size (ortho size is half-height)
                float camHeight = _cam.orthographicSize;
                float camWidth = camHeight * _cam.aspect;

                minBounds = new Vector2(-halfW + camWidth, -halfH + camHeight);
                maxBounds = new Vector2(halfW - camWidth, halfH - camHeight);
                
                Debug.Log($"CameraFollow: Bounds set to ({minBounds.x:F1}, {minBounds.y:F1}) → ({maxBounds.x:F1}, {maxBounds.y:F1})");
            }
        }

        private void Update()
        {
            HandleZoom();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                // Try to find player again if lost
                var player = GameObject.Find("Player-01");
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log("CameraFollow: Re-found player target");
                }
                return;
            }

            Vector3 desiredPos = target.position + offset;
            
            // Smooth Damp
            Vector3 smoothPos = Vector3.SmoothDamp(transform.position, desiredPos, ref _currentVelocity, smoothTime);

            // Clamp to World Bounds
            if (useBounds)
            {
                float clampedX = Mathf.Clamp(smoothPos.x, minBounds.x, maxBounds.x);
                float clampedY = Mathf.Clamp(smoothPos.y, minBounds.y, maxBounds.y);
                transform.position = new Vector3(clampedX, clampedY, smoothPos.z);
            }
            else
            {
                transform.position = new Vector3(smoothPos.x, smoothPos.y, smoothPos.z);
            }
        }

        private void HandleZoom()
        {
            if (_cam == null) return;
            
            if (Mouse.current != null)
            {
                // Mouse wheel scroll for zoom (middle mouse wheel)
                Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
                float scroll = scrollDelta.y;
                
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    // Scroll Up (positive) = Zoom In (decrease ortho size)
                    // Scroll Down (negative) = Zoom Out (increase ortho size)
                    _targetZoom -= scroll * zoomSpeed * 0.01f;
                    _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
                }
            }

            // Smoothly interpolate to target zoom
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, Time.deltaTime * 10f);
        }
    }
}
