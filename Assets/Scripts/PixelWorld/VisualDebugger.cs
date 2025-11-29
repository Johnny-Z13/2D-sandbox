using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace PixelWorld
{
    /// <summary>
    /// Visual debugging and information display for rendering system.
    /// Shows current settings, FPS, and provides quick toggles.
    /// </summary>
    [RequireComponent(typeof(RenderingPresetController))]
    public class VisualDebugger : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private Key toggleKey = Key.F12; // Changed from F1 to F12 (F1-F6 now used for graphics)

        private RenderingPresetController _presetController;
        private float _fpsTimer;
        private int _frameCount;
        private float _currentFPS;

        private void Start()
        {
            _presetController = GetComponent<RenderingPresetController>();
            
            if (infoText == null && showDebugInfo)
            {
                CreateDebugCanvas();
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                showDebugInfo = !showDebugInfo;
                if (infoText != null)
                {
                    infoText.gameObject.SetActive(showDebugInfo);
                }
            }

            if (showDebugInfo && infoText != null)
            {
                UpdateFPS();
                UpdateInfoDisplay();
            }
        }

        private void UpdateFPS()
        {
            _frameCount++;
            _fpsTimer += Time.deltaTime;

            if (_fpsTimer >= 0.5f)
            {
                _currentFPS = _frameCount / _fpsTimer;
                _frameCount = 0;
                _fpsTimer = 0f;
            }
        }

        private void UpdateInfoDisplay()
        {
            if (infoText == null) return;

            var worldManager = PixelWorldManager.Instance;
            if (worldManager == null) return;

            string info = $"<b>Pixel Sandbox - Rendering Showcase</b>\n";
            info += $"<color=#00FF00>FPS: {_currentFPS:F1}</color>\n";
            info += $"\n<b>World:</b>\n";
            info += $"  Resolution: {worldManager.Width}x{worldManager.Height}\n";
            info += $"  Total Pixels: {worldManager.Width * worldManager.Height:N0}\n";
            info += $"\n<b>Controls:</b>\n";
            info += $"  <color=#FFFF00>F1-F6</color> = Graphics Presets\n";
            info += $"  <color=#FFFF00>F12</color> = Toggle Info\n";
            info += $"  <color=#FFFF00>LMB</color> = Paint Sand\n";
            info += $"  <color=#FFFF00>RMB</color> = Paint Water\n";
            info += $"  <color=#FFFF00>MMB</color> = Erase\n";
            info += $"\n<b>Graphics Presets:</b>\n";
            info += $"  F1 = Default\n";
            info += $"  F2 = Desert Gold\n";
            info += $"  F3 = Subtle Realism\n";
            info += $"  F4 = Extreme Showcase\n";
            info += $"  F5 = Screenshot Mode\n";
            info += $"  F6 = Performance Mode\n";
            info += $"\n<b>Tips:</b>\n";
            info += $"  • Paint large sand areas to see glitter\n";
            info += $"  • Watch sand fall for shimmer effects\n";
            info += $"  • Compare presets to find your favorite\n";

            infoText.text = info;
        }

        private void CreateDebugCanvas()
        {
            // Create UI Canvas if info text is not assigned
            var canvasObj = new GameObject("DebugCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create text object
            var textObj = new GameObject("InfoText");
            textObj.transform.SetParent(canvasObj.transform, false);
            
            infoText = textObj.AddComponent<TextMeshProUGUI>();
            infoText.fontSize = 16;
            infoText.alignment = TextAlignmentOptions.TopLeft;
            infoText.color = Color.white;
            
            // Add shadow for readability
            var shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(2, -2);

            // Position in top-left
            var rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(10, -10);
            rectTransform.sizeDelta = new Vector2(400, 600);

            Debug.Log("VisualDebugger: Created debug canvas. Note: For best results, import TextMeshPro.");
        }

        private void OnGUI()
        {
            // Fallback GUI if TextMeshPro is not available
            if (showDebugInfo && infoText == null)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 14;
                style.normal.textColor = Color.white;
                style.padding = new RectOffset(10, 10, 10, 10);

                string info = $"Pixel Sandbox - Rendering Showcase\n";
                info += $"FPS: {_currentFPS:F1}\n\n";
                info += $"Press F12 to toggle info\n";
                info += $"Press F1-F6 for graphics presets\n\n";
                info += $"LMB = Sand | RMB = Water | MMB = Erase";

                GUI.Label(new Rect(10, 10, 400, 200), info, style);
            }
        }
    }
}
