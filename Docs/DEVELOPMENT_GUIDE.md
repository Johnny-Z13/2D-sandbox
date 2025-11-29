# Development Guide

Guide for adding new features and maintaining code quality in the 2D Pixel Sandbox project.

---

## Getting Started

### Prerequisites
- Unity 2022.3 LTS or newer
- Basic understanding of C# and Unity
- Familiarity with compute shaders (for advanced work)
- Read `WORLD_SYSTEMS.md` to understand core architecture

### Project Setup
1. Open project in Unity
2. Open scene: `Assets/Scenes/Prototype-01.unity`
3. Familiarize yourself with the script structure in `Assets/Scripts/PixelWorld/`

---

## Code Organization

### Folder Structure

```
Assets/Scripts/PixelWorld/
├── Core Systems (Production)
│   ├── PixelWorldManager.cs       - World simulation orchestration
│   ├── PixelCollisionSystem.cs    - Collision detection
│   └── PlayerController2D.cs      - Player movement & input
│
├── Utilities
│   ├── CameraFollow.cs            - Camera system
│   └── WorldRendererScaler.cs    - Auto-scale renderer
│
├── Features
│   ├── RenderingPresetController.cs - Visual presets
│   └── VisualDebugger.cs          - Runtime debug UI
│
└── Debug Tools (Editor/Development)
    ├── QuickDiagnostic.cs         - Runtime diagnostics
    ├── WorldGenerationDebugger.cs - World validation
    ├── WorldSystemValidator.cs    - System checks
    ├── WorldBoundsDebugger.cs     - Scene gizmos
    └── WorldDimensionCalculator.cs - World sizing tool
```

### File Naming Conventions
- **Classes:** PascalCase (e.g., `PixelWorldManager`)
- **Private fields:** `_camelCase` with underscore prefix
- **Public properties:** PascalCase
- **Methods:** PascalCase
- **Constants:** UPPER_SNAKE_CASE or PascalCase for readonly

---

## Adding New Features

### 1. New Material Type

**Example: Adding Lava**

#### Step 1: Define Material ID (Compute Shader)
```hlsl
// In Assets/Shaders/Compute/PixelWorld.compute
#define MAT_LAVA 5
```

#### Step 2: Add Generation Logic (CSInit kernel)
```hlsl
// In CSInit kernel:
else if (uv.y < 0.1f && caveNoise > 0.7) {
    mat = MAT_LAVA; // Lava pools deep underground
}
```

#### Step 3: Add Physics Logic (CSMain kernel)
```hlsl
// In CSMain kernel:
else if (self == MAT_LAVA) {
    // Lava spreads horizontally, destroys water
    if (up == MAT_WATER) result = MAT_WATER; // Water extinguishes lava
    else if (down == MAT_EMPTY) result = MAT_EMPTY; // Flow down slowly
    else if (left == MAT_EMPTY || right == MAT_EMPTY) {
        if ((uint)(_Time * 10.0) % 3u == 0u) result = MAT_EMPTY; // Slow horizontal spread
    }
}
```

#### Step 4: Add Rendering (Pixel Shader)
```hlsl
// In Assets/Shaders/Materials/PixelWorldRender.shader
else if (mat == MAT_LAVA) {
    // Pulsing orange glow
    float pulse = 0.8 + 0.2 * sin(_Time * 5.0);
    return fixed4(1.0, 0.4 * pulse, 0.0, 1.0);
}
```

#### Step 5: Update Collision System
```csharp
// In Assets/Scripts/PixelWorld/PixelCollisionSystem.cs
public bool IsSolid(Vector2 worldPos) {
    // ...
    return mat == MAT_ROCK || mat == MAT_DIRT || mat == MAT_SAND || mat == MAT_LAVA;
}

public bool IsDeadly(Vector2 worldPos) {
    // New method for hazards
    int mat = GetMaterialAt(worldPos);
    return mat == MAT_LAVA;
}
```

#### Step 6: Add to World Manager API (Optional)
```csharp
// In Assets/Scripts/PixelWorld/PixelWorldManager.cs
public void PlaceLava(Vector2 worldPos, float radius) {
    ModifyWorld(worldPos, radius, 5); // MAT_LAVA = 5
}
```

### 2. New Player Ability

**Example: Adding Double Jump**

#### Step 1: Add State Variables
```csharp
// In PlayerController2D.cs
[Header("Abilities")]
[SerializeField] private bool canDoubleJump = true;
[SerializeField] private float doubleJumpForce = 6f;

private int _jumpsRemaining;
```

#### Step 2: Update Jump Logic
```csharp
public void OnJump(InputValue value) {
    if (!value.isPressed) return;
    
    // Regular jump
    if (_isGrounded && _jumpsRemaining == 0) {
        _velocity.y = jumpForce;
        _jumpsRemaining = canDoubleJump ? 1 : 0;
    }
    // Double jump
    else if (_jumpsRemaining > 0) {
        _velocity.y = doubleJumpForce;
        _jumpsRemaining--;
    }
}

private void Move() {
    // ... collision detection ...
    
    if (_isGrounded) {
        _jumpsRemaining = canDoubleJump ? 1 : 0; // Reset on landing
    }
}
```

#### Step 3: Add Animation (Optional)
```csharp
// Update animation state
_animator.SetBool("IsDoubleJumping", !_isGrounded && _jumpsRemaining == 0);
```

### 3. New Debug Tool

**Example: FPS Counter**

Create `Assets/Scripts/PixelWorld/FPSCounter.cs`:

```csharp
using UnityEngine;
using TMPro;

namespace PixelWorld
{
    /// <summary>
    /// Simple FPS counter for performance monitoring
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private float updateInterval = 0.5f;
        
        private float _timer;
        private int _frames;
        private float _fps;
        
        private void Update() {
            _frames++;
            _timer += Time.deltaTime;
            
            if (_timer >= updateInterval) {
                _fps = _frames / _timer;
                _frames = 0;
                _timer = 0f;
                
                if (fpsText != null) {
                    fpsText.text = $"FPS: {_fps:F1}";
                }
            }
        }
    }
}
```

---

## Code Style Guidelines

### Unity-Specific Conventions

```csharp
// ✅ GOOD: SerializeField for inspector exposure
[SerializeField] private float moveSpeed = 5f;

// ❌ BAD: Public fields (use properties instead)
public float moveSpeed = 5f;

// ✅ GOOD: Cached component references
private Rigidbody2D _rb;
void Start() { _rb = GetComponent<Rigidbody2D>(); }

// ❌ BAD: GetComponent every frame
void Update() { GetComponent<Rigidbody2D>().velocity = ...; }
```

### Header Organization

Use `[Header]` to group related fields:

```csharp
[Header("Movement")]
[SerializeField] private float moveSpeed = 5f;
[SerializeField] private float jumpForce = 8f;

[Header("Collision")]
[SerializeField] private float playerHeight = 1.0f;
[SerializeField] private int verticalRays = 3;

[Header("References")]
[SerializeField] private Animator animator;
```

### Tooltips for Complex Parameters

```csharp
[Tooltip("How hollow the caves are (0.2 = dense, 0.5 = hollow)")]
[Range(0.2f, 0.6f)]
[SerializeField] private float caveThreshold = 0.35f;
```

### XML Documentation for Public APIs

```csharp
/// <summary>
/// Modifies the world at the specified position.
/// </summary>
/// <param name="worldPos">Position in world space (Unity units)</param>
/// <param name="radius">Radius of effect in pixels</param>
/// <param name="matID">Material ID to place (0=empty, 1=rock, etc.)</param>
public void ModifyWorld(Vector2 worldPos, float radius, int matID) {
    // ...
}
```

---

## Performance Best Practices

### 1. Minimize Allocations

```csharp
// ❌ BAD: Creates garbage every frame
void Update() {
    var position = new Vector2(x, y); // Allocates!
}

// ✅ GOOD: Reuse variables
private Vector2 _position;
void Update() {
    _position.x = x;
    _position.y = y;
}
```

### 2. Cache Component References

```csharp
// ❌ BAD: Slow lookup every frame
void Update() {
    GetComponent<Animator>().SetBool("IsMoving", true);
}

// ✅ GOOD: Cached in Start/Awake
private Animator _animator;
void Start() { _animator = GetComponent<Animator>(); }
void Update() { _animator.SetBool("IsMoving", true); }
```

### 3. Use Fixed Update for Physics

```csharp
// ✅ Physics calculations in FixedUpdate
void FixedUpdate() {
    ApplyGravity();
    CheckCollision();
}

// ✅ Input and rendering in Update
void Update() {
    HandleInput();
    UpdateAnimation();
}
```

### 4. Avoid FindObjectOfType in Update

```csharp
// ❌ BAD: Very slow search
void Update() {
    var manager = FindObjectOfType<PixelWorldManager>();
}

// ✅ GOOD: Find once, cache reference
private PixelWorldManager _manager;
void Start() { _manager = FindObjectOfType<PixelWorldManager>(); }
```

---

## Testing

### Manual Testing Checklist

When adding new features, test:
- ✅ Works with keyboard/mouse
- ✅ Works with gamepad
- ✅ No console errors
- ✅ Performance is acceptable (60 FPS)
- ✅ Doesn't break existing features
- ✅ Edge cases handled (e.g., world boundaries)

### Debug Tools

Use built-in debug tools:
```csharp
// WorldSystemValidator - Check all systems
[ContextMenu("🔍 Validate All Systems")]

// WorldGenerationDebugger - Check terrain generation
[ContextMenu("🔍 Check World Generation")]

// QuickDiagnostic - Runtime player/input diagnostics
// Add component to scene for real-time debugging
```

### Gizmos for Visualization

```csharp
private void OnDrawGizmos() {
    // Visualize collision rays
    Gizmos.color = Color.red;
    for (int i = 0; i < verticalRays; i++) {
        Vector2 rayStart = GetRayPosition(i);
        Gizmos.DrawLine(rayStart, rayStart + Vector2.down * 0.5f);
    }
}
```

---

## Common Pitfalls

### 1. Coordinate System Confusion

```csharp
// World space (Unity units): (-40, -15) to (40, 15)
// Pixel space (array index): (0, 0) to (4095, 1535)

// ✅ Always use WorldToPixel() for conversions
Vector2 pixelPos = WorldToPixel(worldPos);

// ❌ Don't manually calculate
// Vector2 pixelPos = worldPos * 50; // WRONG!
```

### 2. Forgetting Async GPU Readback Latency

```csharp
// ❌ BAD: Assumes immediate collision data
WorldManager.ModifyWorld(pos, 10, 0);
if (CollisionSystem.IsSolid(pos)) { ... } // May be stale!

// ✅ GOOD: Wait for collision data
if (!CollisionSystem.HasData) return;
if (CollisionSystem.IsSolid(pos)) { ... }
```

### 3. Not Clamping Shader Parameters

```csharp
// ✅ GOOD: Clamp before sending to GPU
pixelSimShader.SetFloat("_CaveThreshold", 
    Mathf.Clamp(caveThreshold, 0.2f, 0.6f));

// ❌ BAD: Invalid values crash shader
pixelSimShader.SetFloat("_CaveThreshold", caveThreshold); // Could be negative!
```

### 4. Modifying Compute Shader Without Reimport

After editing `.compute` files:
1. Save the file
2. Return to Unity
3. Right-click compute shader → **Reimport**
4. Otherwise Unity uses old compiled version!

---

## Debugging Techniques

### 1. Console Logging

```csharp
// Use rich text for readability
Debug.Log("<color=green>✅ System initialized</color>");
Debug.LogWarning("<color=yellow>⚠️ Performance slow</color>");
Debug.LogError("<color=red>❌ Critical failure</color>");

// Conditional logging
if (debugMode) {
    Debug.Log($"Player position: {transform.position}");
}
```

### 2. Scene Gizmos

```csharp
private void OnDrawGizmos() {
    // Draw player bounds
    Gizmos.color = Color.green;
    Gizmos.DrawWireCube(transform.position, 
        new Vector3(playerWidth, playerHeight, 0));
}
```

### 3. Inspector Debugging

```csharp
[Header("Debug Info (Read-Only)")]
[SerializeField] private bool isGrounded;
[SerializeField] private Vector2 velocity;

// Update in Update() so you can see values change in Inspector
```

### 4. Context Menus for Quick Tests

```csharp
[ContextMenu("Test Jump")]
private void TestJump() {
    _velocity.y = jumpForce;
    Debug.Log("Jump tested!");
}
```

---

## Git Workflow (Recommended)

### Commit Messages
```
feat(player): add double jump ability
fix(collision): resolve wall-climbing bug
docs(readme): update setup instructions
refactor(world): extract cave generation parameters
perf(shader): optimize sand physics loop
```

### Branching Strategy
```
main - Stable, tested code
develop - Integration branch
feature/double-jump - New features
fix/collision-bug - Bug fixes
```

### What to Commit
- ✅ All `.cs` scripts
- ✅ `.compute` and `.shader` files
- ✅ `.unity` scene files
- ✅ `.prefab` files
- ✅ `.inputactions` files
- ✅ Documentation `.md` files
- ❌ Library/ folder
- ❌ Temp/ folder
- ❌ Logs/ folder
- ❌ `.vs/` or `.vscode/` folders

---

## Resources

### Unity Documentation
- [Compute Shaders](https://docs.unity3d.com/Manual/class-ComputeShader.html)
- [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)

### Project Documentation
- `WORLD_SYSTEMS.md` - Core technical systems
- `CAVE_GENERATION_GUIDE.md` - Procedural generation
- `VISUAL_UPGRADE_README.md` - Rendering & effects
- `Assets/2D-sandbox-PRD` - Product requirements

### Learning Resources
- Noita GDC Talk (Google: "Noita GDC falling sand")
- Cellular Automata tutorials
- GPU programming basics

---

## Next Steps

1. Read `WORLD_SYSTEMS.md` to understand architecture
2. Explore existing code in `Assets/Scripts/PixelWorld/`
3. Try adding a simple feature (e.g., new material)
4. Use debug tools to validate your changes
5. Test thoroughly before committing

---

**Questions?** Check documentation or add comments in code for future reference.

**Found a bug?** Add it to your issue tracker or fix it following the patterns above.

**Want to contribute?** Follow the code style and testing guidelines in this document.

