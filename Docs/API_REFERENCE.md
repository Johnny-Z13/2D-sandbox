# API Reference
## 2D Pixel Sandbox - Complete API Documentation

This document provides a complete reference for all public APIs in the project.

---

## Table of Contents

1. [Core Systems](#core-systems)
   - [PixelWorldManager](#pixelworldmanager)
   - [PixelCollisionSystem](#pixelcollisionsystem)
   - [PlayerController2D](#playercontroller2d)
2. [Audio System](#audio-system)
   - [AudioManager](#audiomanager)
   - [AudioEventTriggers](#audioeventtriggers)
3. [Camera & Rendering](#camera--rendering)
   - [CameraFollow](#camerafollow)
   - [RenderingPresetController](#renderingpresetcontroller)
4. [UI Systems](#ui-systems)
   - [HotbarController](#hotbarcontroller)
   - [VisualDebugger](#visualdebugger)
5. [Items & Gameplay](#items--gameplay)
   - [Bomb](#bomb)
6. [Debug Tools](#debug-tools)

---

## Core Systems

### PixelWorldManager

**Location:** `Assets/Scripts/PixelWorld/PixelWorldManager.cs`  
**Namespace:** `PixelWorld`

Central manager for the pixel world simulation. Handles GPU compute shader execution, world generation, and user input.

#### Singleton Access

```csharp
PixelWorldManager manager = PixelWorldManager.Instance;
```

#### Public Properties

```csharp
// World dimensions (read-only)
int Width { get; }          // World width in pixels (e.g., 4096)
int Height { get; }         // World height in pixels (e.g., 1536)
float CellSize { get; }     // Size of each pixel in Unity units (default: 0.02)
```

#### Public Methods

##### ModifyWorld
```csharp
public void ModifyWorld(Vector2 worldPos, float radius, int matID)
```
Modifies the pixel world at the specified position.

**Parameters:**
- `worldPos` - Position in world space (Unity units)
- `radius` - Radius of effect in pixels
- `matID` - Material ID to place (0=Empty, 1=Rock, 2=Dirt, 3=Sand, 4=Water)

**Example:**
```csharp
// Dig a hole (10 pixel radius)
PixelWorldManager.Instance.ModifyWorld(transform.position, 10f, 0);

// Place sand (20 pixel radius)
PixelWorldManager.Instance.ModifyWorld(mousePos, 20f, 3);
```

##### RegenerateWorld
```csharp
public void RegenerateWorld()
```
Regenerates the entire world with current settings.

**Example:**
```csharp
PixelWorldManager.Instance.RegenerateWorld();
```

##### GetCurrentTexture
```csharp
public RenderTexture GetCurrentTexture()
```
Returns the current world state texture (for collision system).

**Returns:** `RenderTexture` - Current world state (RInt format)

#### Material IDs

```csharp
// Defined in PixelWorld.compute shader
const int MAT_EMPTY = 0;   // Air/empty space
const int MAT_ROCK = 1;    // Solid rock (unbreakable in edges)
const int MAT_DIRT = 2;    // Dirt (diggable)
const int MAT_SAND = 3;    // Sand (falls, diggable)
const int MAT_WATER = 4;   // Water (flows)
```

#### Serialized Fields (Inspector)

```csharp
[Header("World Dimensions")]
[SerializeField] private int width = 4096;
[SerializeField] private int height = 1536;
[SerializeField] private float cellSize = 0.02f;

[Header("Cave Generation")]
[SerializeField] private float caveThreshold = 0.35f;     // 0.2-0.6
[SerializeField] private float caveFrequencyX = 8.0f;     // 4-16
[SerializeField] private float caveFrequencyY = 16.0f;    // 8-32
[SerializeField] private float caveLayerBlend = 0.5f;     // 0-1

[Header("Water Generation")]
[SerializeField] private float waterPoolChance = 0.3f;    // 0-1
[SerializeField] private float waterDepthThreshold = 0.25f; // 0.05-0.6
[SerializeField] private float waterNoiseThreshold = 0.55f; // 0.3-0.7

[Header("Material Variety")]
[SerializeField] private float sandFrequency = 20.0f;     // 10-40
[SerializeField] private float sandThresholdShallow = 0.6f; // 0.4-0.9
[SerializeField] private float sandThresholdDeep = 0.7f;   // 0.5-0.95

[Header("Physics Settings")]
[SerializeField] private int stabilityThreshold = 2;      // 0-3
```

#### Context Menu Actions (Inspector)

- **"Regenerate World"** - Regenerates terrain with current settings
- **"Load Preset: [Name]"** - Loads cave generation preset
- **"World Size: [Size]"** - Loads world size preset

---

### PixelCollisionSystem

**Location:** `Assets/Scripts/PixelWorld/PixelCollisionSystem.cs`  
**Namespace:** `PixelWorld`

Handles GPU-to-CPU data transfer for collision detection. Uses AsyncGPUReadback for efficient performance.

#### Singleton Access

```csharp
PixelCollisionSystem collision = PixelCollisionSystem.Instance;
```

#### Public Properties

```csharp
bool HasData { get; }  // True when collision data is available (1-2 frames after start)
```

#### Public Methods

##### IsSolid
```csharp
public bool IsSolid(Vector2 worldPos)
```
Checks if a single pixel at world position is solid.

**Parameters:**
- `worldPos` - Position in world space (Unity units)

**Returns:** `bool` - True if solid (Rock, Dirt, or Sand)

**Example:**
```csharp
if (PixelCollisionSystem.Instance.IsSolid(transform.position))
{
    Debug.Log("Standing on solid ground");
}
```

##### IsSolidDown
```csharp
public bool IsSolidDown(Vector2 worldPos, int threshold)
```
Checks for solid ground below with a volume threshold. Used for falling collision to ignore small dust particles.

**Parameters:**
- `worldPos` - Position in world space (Unity units)
- `threshold` - Minimum solid pixels required (typically 2-3)

**Returns:** `bool` - True if enough solid pixels below position

**Example:**
```csharp
// Check for landing (ignore small dust)
if (PixelCollisionSystem.Instance.IsSolidDown(feetPosition, 2))
{
    _isGrounded = true;
}
```

##### GetMaterialAt
```csharp
public int GetMaterialAt(Vector2 worldPos)
```
Gets the material ID at a specific world position.

**Parameters:**
- `worldPos` - Position in world space (Unity units)

**Returns:** `int` - Material ID (0-4)

**Example:**
```csharp
int mat = PixelCollisionSystem.Instance.GetMaterialAt(checkPos);
if (mat == 4) // Water
{
    ApplyWaterPhysics();
}
```

#### Serialized Fields (Inspector)

```csharp
[Header("Settings")]
[SerializeField] private float readbackInterval = 0.02f; // 50Hz update rate
[SerializeField] private bool debugDraw = false;         // Draw bounds gizmo

[Header("References")]
[SerializeField] private PixelWorldManager worldManager;
```

#### Context Menu Actions (Inspector)

- **"Validate Collision System"** - Checks system configuration

---

### PlayerController2D

**Location:** `Assets/Scripts/PixelWorld/PlayerController2D.cs`  
**Namespace:** `PixelWorld`

Handles player movement, input, and interaction with the pixel world.

#### Public Properties

```csharp
// Read-only state (for debugging/other systems)
bool IsGrounded { get; }
bool IsCrouching { get; }
Vector2 Velocity { get; }
float PlayerWidth { get; }
float PlayerHeight { get; }
```

#### Public Methods

##### ApplyVelocity
```csharp
public void ApplyVelocity(Vector2 velocity)
```
Applies external velocity (e.g., from explosions).

**Parameters:**
- `velocity` - Velocity to add (x for knockback, y for upward force)

**Example:**
```csharp
// Apply explosion knockback
Vector2 knockback = explosionDir * force;
player.ApplyVelocity(knockback);
```

#### Input Actions (Unity Input System)

These methods are automatically called by Unity's Input System:

```csharp
public void OnMove(InputValue value)     // WASD/Arrows/Left Stick
public void OnJump(InputValue value)     // Space/A button
public void OnCrouch(InputValue value)   // C/B button
public void OnInteract(InputValue value) // E/X button (dig/bomb)
public void OnAttack(InputValue value)   // Enter/Y button
public void OnPrevious(InputValue value) // 1/DPad Left
public void OnNext(InputValue value)     // 2/DPad Right
```

#### Serialized Fields (Inspector)

```csharp
[Header("Movement")]
[SerializeField] private float moveSpeed = 5f;
[SerializeField] private float jumpForce = 8f;
[SerializeField] private float gravity = 20f;

[Header("Collision")]
[SerializeField] private float playerHeight = 1.0f;
[SerializeField] private float playerWidth = 0.5f;
[SerializeField] private int verticalRays = 3;
[SerializeField] private int groundCollisionThreshold = 2;
[SerializeField] private GameObject bombPrefab;

[Header("Knockback")]
[SerializeField] private float knockbackDecay = 5f;
```

---

## Audio System

### AudioManager

**Location:** `Assets/Scripts/Audio/AudioManager.cs`  
**Namespace:** `PixelWorld`

Central audio management system with pooling, volume controls, and ambient music.

#### Singleton Access

```csharp
AudioManager audio = AudioManager.Instance;
```

#### Public Methods - Sound Effects

##### Player Sounds
```csharp
public void PlayJump()      // Jump sound
public void PlayLand()      // Landing sound
public void PlayFootstep()  // Footstep sound (throttled)
public void PlayDig()       // Digging sound
```

##### World Interaction
```csharp
public void PlayPaintSand()   // Painting sand
public void PlayPaintWater()  // Painting water
public void PlayErase()       // Erasing terrain
public void PlaySandFall()    // Sand falling (optional)
```

##### Combat
```csharp
public void PlayBombPlace()     // Placing bomb
public void PlayBombExplosion() // Bomb exploding
public void PlayPlayerHit()     // Player taking damage
```

##### UI
```csharp
public void PlayUIClick()       // Generic UI click
public void PlayHotbarSwitch()  // Switching items
public void PlayPresetChange()  // Changing graphics preset
```

#### Public Methods - Music Control

```csharp
public void PlayAmbience()  // Start ambient music loop
public void StopAmbience()  // Stop ambient music
```

#### Public Methods - Volume Control

```csharp
public void SetMasterVolume(float volume)    // 0.0 - 1.0
public void SetSFXVolume(float volume)       // 0.0 - 1.0
public void SetAmbienceVolume(float volume)  // 0.0 - 1.0
```

**Example:**
```csharp
// Adjust volumes
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetSFXVolume(0.7f);
AudioManager.Instance.SetAmbienceVolume(0.5f);
```

#### Serialized Fields (Inspector)

```csharp
[Header("Audio Sources")]
[SerializeField] private AudioSource musicSource;
[SerializeField] private AudioSource ambienceSource;
[SerializeField] private AudioSource sfxSourcePrefab;

[Header("Music & Ambience")]
[SerializeField] private AudioClip levelAmbience;
[SerializeField] private float ambienceVolume = 0.6f;

[Header("Player SFX")]
[SerializeField] private AudioClip jumpSound;
[SerializeField] private AudioClip landSound;
[SerializeField] private AudioClip footstepSound;
[SerializeField] private AudioClip digSound;

[Header("World Interaction SFX")]
[SerializeField] private AudioClip sandPaintSound;
[SerializeField] private AudioClip waterPaintSound;
[SerializeField] private AudioClip eraseSound;
[SerializeField] private AudioClip sandFallSound;

[Header("Combat SFX")]
[SerializeField] private AudioClip bombPlaceSound;
[SerializeField] private AudioClip bombExplosionSound;
[SerializeField] private AudioClip playerHitSound;

[Header("UI SFX")]
[SerializeField] private AudioClip uiClickSound;
[SerializeField] private AudioClip hotbarSwitchSound;
[SerializeField] private AudioClip presetChangeSound;

[Header("Settings")]
[SerializeField] private float masterVolume = 1.0f;
[SerializeField] private float sfxVolume = 0.8f;
[SerializeField] private int maxSFXSources = 10;
[SerializeField] private bool enableFootsteps = true;
[SerializeField] private float footstepInterval = 0.4f;
```

#### Context Menu Actions (Inspector)

- **"Test All Sounds"** - Plays each sound effect in sequence
- **"Log Audio Status"** - Displays configuration and missing clips

---

### AudioEventTriggers

**Location:** `Assets/Scripts/Audio/AudioEventTriggers.cs`  
**Namespace:** `PixelWorld`

Static helper class for triggering audio events. Recommended way to play sounds.

#### Static Methods

##### Player Events
```csharp
public static void OnPlayerJump()
public static void OnPlayerLand()
public static void OnPlayerFootstep()
public static void OnPlayerCrouch()  // Placeholder
```

##### World Interaction
```csharp
public static void OnDig()
public static void OnPaintSand()
public static void OnPaintWater()
public static void OnEraseTerrain()
```

##### Combat
```csharp
public static void OnBombPlaced()
public static void OnBombExplode()
```

##### UI
```csharp
public static void OnUIClick()
public static void OnHotbarSwitch()
public static void OnPresetChange()
```

**Example Usage:**
```csharp
// Simple, clean syntax - use anywhere
AudioEventTriggers.OnPlayerJump();
AudioEventTriggers.OnBombExplode();
AudioEventTriggers.OnHotbarSwitch();
```

**Note:** All methods are null-safe and check for AudioManager.Instance.

---

## Camera & Rendering

### CameraFollow

**Location:** `Assets/Scripts/PixelWorld/CameraFollow.cs`  
**Namespace:** `PixelWorld`

Smooth camera following system with world bounds clamping and zoom control.

#### Serialized Fields (Inspector)

```csharp
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
[SerializeField] private float zoomSpeed = 2f;
```

**Auto-Configuration:**
- Automatically finds player if target not assigned
- Auto-calculates bounds based on PixelWorldManager world size
- Snaps to target immediately on start (no drift)

**Zoom Controls:**
- Mouse wheel up = Zoom in (decrease ortho size)
- Mouse wheel down = Zoom out (increase ortho size)

---

### RenderingPresetController

**Location:** `Assets/Scripts/PixelWorld/RenderingPresetController.cs`  
**Namespace:** `PixelWorld`

Manages visual rendering presets for quick style switching.

#### Public Enums

```csharp
public enum VisualPreset
{
    Default,          // Balanced look
    DesertGold,       // Warm, sparkly sand
    SubtleRealism,    // Understated effects
    ExtremeShowcase,  // Maximum visual impact
    ScreenshotMode,   // Marketing-ready
    PerformanceMode   // Speed optimized
}
```

#### Public Methods

```csharp
public void ApplyPreset(VisualPreset preset)
```
Applies a visual preset.

**Example:**
```csharp
RenderingPresetController controller = GetComponent<RenderingPresetController>();
controller.ApplyPreset(VisualPreset.ExtremeShowcase);
```

```csharp
public void SetPreset(int presetIndex)
```
Sets preset by index (0-5).

```csharp
public void SetPresetByName(string presetName)
```
Sets preset by name string.

**Example:**
```csharp
controller.SetPresetByName("DesertGold");
```

#### Keyboard Shortcuts

- **F1** - Default
- **F2** - Desert Gold
- **F3** - Subtle Realism
- **F4** - Extreme Showcase
- **F5** - Screenshot Mode
- **F6** - Performance Mode

#### Serialized Fields (Inspector)

```csharp
[Header("References")]
[SerializeField] private Renderer worldRenderer;
[SerializeField] private Volume postProcessVolume;

[Header("Preset Selection")]
[SerializeField] private VisualPreset currentPreset = VisualPreset.Default;

[Header("On-Screen Notification")]
[SerializeField] private bool showPresetNotification = true;
[SerializeField] private float notificationDuration = 3f;

[Header("Manual Override")]
[SerializeField] private bool useManualSettings = false;
[SerializeField] private SandSettings manualSandSettings;
[SerializeField] private WaterSettings manualWaterSettings;
[SerializeField] private BloomSettings manualBloomSettings;
```

---

## UI Systems

### HotbarController

**Location:** `Assets/Scripts/UI/HotbarController.cs`  
**Namespace:** `PixelWorld`

Manages hotbar selection for tools/items.

#### Singleton Access

```csharp
HotbarController hotbar = HotbarController.Instance;
```

#### Public Properties

```csharp
int SelectedIndex { get; }  // Current selected slot (0-3)
```

#### Public Methods

```csharp
public void SelectSlot(int index)
```
Selects a hotbar slot (0-3).

**Example:**
```csharp
HotbarController.Instance.SelectSlot(2); // Select slot 3
```

```csharp
public void CycleSlot(int direction)
```
Cycles through slots.

**Parameters:**
- `direction` - 1 for next, -1 for previous

**Example:**
```csharp
HotbarController.Instance.CycleSlot(1);  // Next slot
HotbarController.Instance.CycleSlot(-1); // Previous slot
```

#### Hotbar Slots

```
Slot 0: Dig/Mine tool
Slot 1: Sand brush
Slot 2: Water brush
Slot 3: Bomb
```

#### Keyboard Controls

- **1 Key** - Previous slot
- **2 Key** - Next slot
- **Digit keys 1-4** - Direct selection

#### Serialized Fields (Inspector)

```csharp
[Header("UI References")]
[SerializeField] private Transform slotContainer;
[SerializeField] private Color selectedColor = Color.white;
[SerializeField] private Color normalColor = new Color(0.5f, 0.5f, 0.5f, 1f);
```

---

### VisualDebugger

**Location:** `Assets/Scripts/PixelWorld/VisualDebugger.cs`  
**Namespace:** `PixelWorld`

On-screen debug information display with FPS counter and controls reference.

#### Serialized Fields (Inspector)

```csharp
[Header("UI References")]
[SerializeField] private TextMeshProUGUI infoText;
[SerializeField] private bool showDebugInfo = true;
[SerializeField] private Key toggleKey = Key.F12;
```

**Toggle Key:**
- **F12** - Toggle debug info on/off

**Displays:**
- FPS counter
- World resolution
- Total pixels
- Control reference
- Graphics preset keys
- Tips

---

## Items & Gameplay

### Bomb

**Location:** `Assets/Scripts/Items/Bomb.cs`  
**Namespace:** `PixelWorld`

Physics-based explosive item with context-aware debris.

#### Serialized Fields (Inspector)

```csharp
[SerializeField] private float fuseTime = 1.5f;        // Seconds until explosion
[SerializeField] private float explosionRadius = 200f; // Crater size in pixels
[SerializeField] private float knockbackForce = 15f;   // Force applied to player
[SerializeField] private GameObject explosionEffect;   // Particle effect (optional)
[SerializeField] private int splashCount = 30;         // Debris particles

[Header("Physics")]
[SerializeField] private float gravity = 50f;          // Fall speed
```

**Behavior:**
- Falls with gravity until hitting solid ground or water
- Pulses 3 times during fuse countdown
- Explodes after `fuseTime` seconds
- Creates crater of `explosionRadius` pixels
- Applies knockback to nearby players
- Context-aware debris:
  - In water: Water splash
  - In air: No debris
  - On ground: Sand debris

**Spawning:**
```csharp
// Spawned by PlayerController2D when bomb selected (slot 3) and E pressed
Instantiate(bombPrefab, spawnPos, Quaternion.identity);
```

---

## Debug Tools

### WorldSystemValidator

**Location:** `Assets/Scripts/PixelWorld/WorldSystemValidator.cs`  
**Namespace:** `PixelWorld`

Validates system configuration and reports issues.

#### Context Menu Actions (Inspector)

- **"Find All References"** - Auto-finds components in scene
- **"🔍 Validate All Systems"** - Comprehensive validation check

**Validates:**
- PixelWorldManager configuration
- Collision system setup
- Renderer scale matching world size
- Camera bounds
- Player spawn position

---

### WorldGenerationDebugger

**Location:** `Assets/Scripts/PixelWorld/WorldGenerationDebugger.cs`  
**Namespace:** `PixelWorld`

Analyzes generated terrain and reports statistics.

#### Context Menu Actions (Inspector)

- **"🔍 Check World Generation"** - Analyzes world data
- **"🔍 Detailed Material Breakdown"** - Detailed stats

**Reports:**
- Material distribution percentages
- Empty space ratio
- Cave density
- Water coverage
- Sand distribution

---

### WorldBoundsDebugger

**Location:** `Assets/Scripts/PixelWorld/WorldBoundsDebugger.cs`  
**Namespace:** `PixelWorld`

Visualizes world bounds in Scene view.

**Gizmos Display:**
- World bounds rectangle
- Boundary walls (5 pixels)
- Screen grid overlay (optional)
- Dimension labels

---

### WorldDimensionCalculator

**Location:** `Assets/Scripts/PixelWorld/WorldDimensionCalculator.cs`  
**Namespace:** `PixelWorld`

Calculates required world dimensions for N×M screen coverage.

#### Serialized Fields (Inspector)

```csharp
[SerializeField] private int screensWide = 3;
[SerializeField] private int screensDeep = 3;
[SerializeField] private Camera mainCamera;
```

#### Context Menu Actions (Inspector)

- **"Calculate Required Dimensions"** - Calculates and logs dimensions

**Example Output:**
```
Required Pixel Dimensions: 3072 × 1536
World Size: 61.44 × 30.72 units
```

---

### QuickDiagnostic

**Location:** `Assets/Scripts/PixelWorld/QuickDiagnostic.cs`  
**Namespace:** `PixelWorld`

Runtime diagnostic display in Scene view.

**Displays (Scene View):**
- Player position
- Player velocity
- Grounded state
- Input values
- Collision status

---

## Coordinate Systems

### World Space to Pixel Space Conversion

The pixel world uses two coordinate systems:

#### World Space (Unity Units)
- Origin at center (0, 0)
- Measured in Unity units
- Used by GameObjects

#### Pixel Space (Array Indices)
- Origin at bottom-left (0, 0)
- Measured in pixel indices
- Used by GPU/collision

**Conversion Formula:**
```csharp
// World to Pixel
float pixelX = (worldPos.x + halfWorldWidth) / cellSize;
float pixelY = (worldPos.y + halfWorldHeight) / cellSize;

// Where:
// halfWorldWidth = (width * cellSize) / 2f
// halfWorldHeight = (height * cellSize) / 2f
// cellSize = 0.02f (default)
```

**Example:**
```csharp
// For 4096×1536 world with cellSize=0.02:
// World space: -40.96 to +40.96 (X), -15.36 to +15.36 (Y)
// Pixel space: 0 to 4095 (X), 0 to 1535 (Y)

Vector2 worldPos = new Vector2(0, 0);      // Center in world
Vector2 pixelPos = new Vector2(2048, 768); // Center in pixels
```

---

## Common Patterns

### Singleton Access Pattern

Most manager classes use the Singleton pattern:

```csharp
if (PixelWorldManager.Instance != null)
{
    PixelWorldManager.Instance.ModifyWorld(pos, radius, matID);
}
```

### Audio Trigger Pattern

Always use AudioEventTriggers for sound effects:

```csharp
// ✅ GOOD: Clean, null-safe
AudioEventTriggers.OnPlayerJump();

// ❌ BAD: Direct access, need null check
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayJump();
```

### Collision Check Pattern

Always check HasData before using collision:

```csharp
// ✅ GOOD: Safe collision check
if (PixelCollisionSystem.Instance != null && 
    PixelCollisionSystem.Instance.HasData)
{
    bool solid = PixelCollisionSystem.Instance.IsSolid(pos);
}

// ❌ BAD: No HasData check
if (PixelCollisionSystem.Instance.IsSolid(pos)) // May return stale data!
```

### Context Menu Pattern

Use `[ContextMenu]` for inspector debugging:

```csharp
[ContextMenu("Test Functionality")]
private void TestFunctionality()
{
    Debug.Log("Test executed!");
}
```

Right-click component in Inspector to access.

---

## Performance Considerations

### GPU Operations
- World simulation runs on GPU (compute shader)
- Collision data transfer: 1-2 frame latency (acceptable)
- Async readback: Non-blocking, efficient

### Memory Usage
- World textures: ~19 MB (4096×1536, double-buffered)
- Collision data: ~6 MB (CPU copy)
- Audio system: ~5 MB
- Total: ~30 MB

### CPU Performance
- AudioManager: O(1) audio pooling
- PlayerController: Efficient raycasting (3-5 rays)
- No GC allocations in hot paths

---

## Error Handling

### Common Issues

**"PixelWorldManager: Compute Shader not assigned!"**
- Assign compute shader in PixelWorldManager Inspector

**"PixelCollisionSystem: World texture is null!"**
- Ensure PixelWorldManager initializes before collision system
- Check execution order

**"No collision data yet"**
- Wait 1-2 frames for AsyncGPUReadback
- Check `HasData` property before using collision

**"AudioManager: Level ambience clip not assigned"**
- Assign audio clips in AudioManager Inspector
- Generate placeholders: Tools > Audio > Generate Placeholder WAVs

---

## Version Information

**API Version:** 1.0  
**Last Updated:** November 29, 2025  
**Unity Version:** 2022.3 LTS or newer  
**Render Pipeline:** Universal Render Pipeline (URP) 17.2.0

---

## Additional Resources

### Documentation
- `README.md` - Project overview
- `Docs/DEVELOPMENT_GUIDE.md` - Development guide
- `Docs/WORLD_SYSTEMS.md` - Technical architecture
- `Docs/AUDIO_SYSTEM.md` - Audio system details
- `Docs/CONTROLS.md` - Input reference
- `Docs/PROJECT_STRUCTURE.md` - File organization

### Guides
- `AUDIO_SETUP_GUIDE.md` - Audio setup instructions
- `CAVE_GENERATION_GUIDE.md` - Procedural generation
- `MULTI_SCREEN_WORLD_SETUP.md` - World sizing

### Reference
- `Assets/2D-sandbox-PRD` - Product requirements
- `CHANGELOG.md` - Version history
- `CODEBASE_REVIEW_REPORT.md` - Technical review

---

**For questions or contributions, see the project README.**

