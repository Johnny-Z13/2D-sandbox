using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelWorld
{
    /// <summary>
    /// Quick diagnostic to check what's wrong with player and mouse input
    /// </summary>
    public class QuickDiagnostic : MonoBehaviour
    {
        private PixelWorldManager worldManager;
        private PixelCollisionSystem collisionSystem;
        private Camera mainCam;

        private void Start()
        {
            worldManager = FindObjectOfType<PixelWorldManager>();
            collisionSystem = FindObjectOfType<PixelCollisionSystem>();
            mainCam = Camera.main;
            
            Debug.Log("<b>=== QUICK DIAGNOSTIC START ===</b>");
            
            // Check systems
            Debug.Log($"PixelWorldManager: {(worldManager != null ? "✅ Found" : "❌ MISSING")}");
            Debug.Log($"PixelCollisionSystem: {(collisionSystem != null ? "✅ Found" : "❌ MISSING")}");
            Debug.Log($"Main Camera: {(mainCam != null ? "✅ Found" : "❌ MISSING")}");
            
            if (worldManager != null)
            {
                Debug.Log($"World Dimensions: {worldManager.Width}×{worldManager.Height}px");
            }
            
            // Check player
            var players = FindObjectsOfType<PlayerController2D>();
            Debug.Log($"PlayerController2D instances: {players.Length}");
            foreach (var p in players)
            {
                Debug.Log($"  Player '{p.name}' at position {p.transform.position}");
                var playerInput = p.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    Debug.Log($"    PlayerInput: ✅ (ActionMap: {playerInput.currentActionMap?.name ?? "null"})");
                }
                else
                {
                    Debug.Log($"    PlayerInput: ❌ MISSING - Player won't respond to input!");
                }
            }
            
            Debug.Log("<b>=== Press Space 3 times to test mouse painting ===</b>");
        }

        private int spaceCount = 0;
        
        private void Update()
        {
            // Test mouse painting when user presses space 3 times
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                spaceCount++;
                if (spaceCount >= 3)
                {
                    TestMousePainting();
                    spaceCount = 0;
                }
            }
            
            // Show mouse position in world
            if (Mouse.current != null && mainCam != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
                
                // Draw cursor position
                Debug.DrawLine(worldPos + Vector3.left * 0.5f, worldPos + Vector3.right * 0.5f, Color.yellow);
                Debug.DrawLine(worldPos + Vector3.up * 0.5f, worldPos + Vector3.down * 0.5f, Color.yellow);
                
                // Test mouse clicks
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Debug.Log($"<color=yellow>LEFT CLICK at screen {mousePos} → world {worldPos}</color>");
                    TestPaintingAtPosition(worldPos, 3, "SAND");
                }
                
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    Debug.Log($"<color=cyan>RIGHT CLICK at screen {mousePos} → world {worldPos}</color>");
                    TestPaintingAtPosition(worldPos, 4, "WATER");
                }
            }
            
            // Test collision at player position
            var player = FindObjectOfType<PlayerController2D>();
            if (player != null && collisionSystem != null && collisionSystem.HasData)
            {
                Vector2 playerPos = player.transform.position;
                Vector2 feetPos = playerPos + Vector2.down * 0.5f;
                
                bool groundSolid = collisionSystem.IsSolid(feetPos);
                
                // Draw debug
                Color color = groundSolid ? Color.green : Color.red;
                Debug.DrawLine(feetPos + Vector2.left * 0.3f, feetPos + Vector2.right * 0.3f, color);
                
                if (!groundSolid && Mathf.Abs(player.transform.position.y) > 0.1f)
                {
                    // Player is floating!
                    Debug.DrawLine(playerPos, feetPos, Color.red, 0.1f);
                }
            }
        }

        private void TestMousePainting()
        {
            Debug.Log("<b>=== TESTING MOUSE PAINTING ===</b>");
            
            if (worldManager == null)
            {
                Debug.LogError("❌ Cannot test: PixelWorldManager is null!");
                return;
            }
            
            if (mainCam == null)
            {
                Debug.LogError("❌ Cannot test: Main Camera is null!");
                return;
            }
            
            // Test painting at center of screen
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector3 worldCenter = mainCam.ScreenToWorldPoint(screenCenter);
            
            Debug.Log($"Testing paint at screen center {screenCenter} → world {worldCenter}");
            
            // Try to paint sand
            worldManager.ModifyWorld(worldCenter, 20, 3); // Radius 20, Sand
            
            Debug.Log($"✅ Called ModifyWorld - Check if sand appears at center of screen!");
            Debug.Log($"If nothing appears, the world might not be initialized yet.");
        }
        
        private void TestPaintingAtPosition(Vector3 worldPos, int matID, string matName)
        {
            if (worldManager != null)
            {
                worldManager.ModifyWorld(worldPos, 15, matID);
                Debug.Log($"  → Called ModifyWorld({worldPos}, 15, {matID}) for {matName}");
            }
            else
            {
                Debug.LogError($"  → Cannot paint: PixelWorldManager is null!");
            }
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 12;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleLeft;
            style.padding = new RectOffset(10, 10, 5, 5);
            
            string status = "<b>Quick Diagnostic Active</b>\n";
            
            if (collisionSystem != null)
            {
                status += collisionSystem.HasData ? "Collision: <color=green>✅ Ready</color>\n" : "Collision: <color=yellow>⏳ Loading...</color>\n";
            }
            else
            {
                status += "Collision: <color=red>❌ Missing</color>\n";
            }
            
            var player = FindObjectOfType<PlayerController2D>();
            if (player != null)
            {
                status += $"Player Y: {player.transform.position.y:F2}\n";
                
                if (collisionSystem != null && collisionSystem.HasData)
                {
                    Vector2 feetPos = (Vector2)player.transform.position + Vector2.down * 0.5f;
                    bool hasGround = collisionSystem.IsSolid(feetPos);
                    status += hasGround ? "<color=green>Ground: ✅ Solid</color>\n" : "<color=red>Ground: ❌ Empty (falling!)</color>\n";
                }
            }
            
            status += "\n<color=yellow>Left/Right Click to test painting</color>";
            
            GUI.Box(new Rect(10, Screen.height - 120, 300, 100), status, style);
        }
    }
}

