# Quick Reference Guide
## 2D Pixel Sandbox - At-a-Glance Reference

Quick lookup for common tasks, APIs, and information.

---

## 🎮 Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD / Arrows | Left Stick |
| Jump | Space | A / Cross |
| Crouch | C | B / Circle |
| Dig/Use | E | X / Square |
| Bomb (slot 3) | E | X / Square |
| Hotbar Previous | 1 | DPad Left |
| Hotbar Next | 2 | DPad Right |
| Paint Sand | Left Mouse | - |
| Paint Water | Right Mouse | - |
| Erase | Middle Mouse | - |
| Graphics Preset | F1-F6 | - |
| Toggle Debug | F12 | - |

---

## 🔧 Common APIs

### World Manipulation

```csharp
// Dig/erase (10 pixel radius)
PixelWorldManager.Instance.ModifyWorld(position, 10f, 0);

// Place sand
PixelWorldManager.Instance.ModifyWorld(position, 20f, 3);

// Place water
PixelWorldManager.Instance.ModifyWorld(position, 15f, 4);

// Regenerate world
PixelWorldManager.Instance.RegenerateWorld();
```

### Collision Detection

```csharp
// Check if position is solid
bool solid = PixelCollisionSystem.Instance.IsSolid(worldPos);

// Check ground with threshold (ignore dust)
bool grounded = PixelCollisionSystem.Instance.IsSolidDown(feetPos, 2);

// Get material at position
int matID = PixelCollisionSystem.Instance.GetMaterialAt(worldPos);
```

### Audio Triggers

```csharp
// Player sounds
AudioEventTriggers.OnPlayerJump();
AudioEventTriggers.OnPlayerLand();
AudioEventTriggers.OnPlayerFootstep();
AudioEventTriggers.OnDig();

// World interaction
AudioEventTriggers.OnPaintSand();
AudioEventTriggers.OnPaintWater();
AudioEventTriggers.OnEraseTerrain();

// Combat
AudioEventTriggers.OnBombPlaced();
AudioEventTriggers.OnBombExplode();

// UI
AudioEventTriggers.OnHotbarSwitch();
AudioEventTriggers.OnPresetChange();
```

### Player Interaction

```csharp
// Get player reference
PlayerController2D player = FindObjectOfType<PlayerController2D>();

// Apply knockback
player.ApplyVelocity(new Vector2(forceX, forceY));

// Check player state
bool grounded = player.IsGrounded;
bool crouching = player.IsCrouching;
Vector2 velocity = player.Velocity;
```

### Camera Control

```csharp
// Camera auto-follows player target
// Zoom with mouse wheel (user controlled)
// Bounds auto-calculated from world size
```

### Hotbar

```csharp
// Get selected slot
int slot = HotbarController.Instance.SelectedIndex; // 0-3

// Select slot
HotbarController.Instance.SelectSlot(2);

// Cycle slots
HotbarController.Instance.CycleSlot(1);  // Next
HotbarController.Instance.CycleSlot(-1); // Previous
```

---

## 📋 Material IDs

```csharp
0 = MAT_EMPTY  // Air/empty space
1 = MAT_ROCK   // Solid rock (unbreakable at edges)
2 = MAT_DIRT   // Dirt (diggable)
3 = MAT_SAND   // Sand (falls, diggable)
4 = MAT_WATER  // Water (flows)
```

---

## 🎨 Visual Presets

```csharp
// Keyboard: F1-F6
// Code:
RenderingPresetController.Instance.ApplyPreset(VisualPreset.Default);

// Available presets:
VisualPreset.Default          // F1 - Balanced
VisualPreset.DesertGold       // F2 - Warm & sparkly
VisualPreset.SubtleRealism    // F3 - Understated
VisualPreset.ExtremeShowcase  // F4 - Maximum impact
VisualPreset.ScreenshotMode   // F5 - Marketing ready
VisualPreset.PerformanceMode  // F6 - Speed optimized
```

---

## 🎵 Audio System

### Volume Control

```csharp
AudioManager.Instance.SetMasterVolume(0.8f);    // 0.0 - 1.0
AudioManager.Instance.SetSFXVolume(0.7f);       // 0.0 - 1.0
AudioManager.Instance.SetAmbienceVolume(0.5f);  // 0.0 - 1.0
```

### Music Control

```csharp
AudioManager.Instance.PlayAmbience();   // Start looping music
AudioManager.Instance.StopAmbience();   // Stop music
```

---

## 🔍 Debug Tools

### Context Menu Actions

Right-click components in Inspector:

**PixelWorldManager:**
- Regenerate World
- Load Preset: [Name]
- World Size: [Size]

**AudioManager:**
- Test All Sounds
- Log Audio Status

**WorldSystemValidator:**
- Find All References
- 🔍 Validate All Systems

**WorldGenerationDebugger:**
- 🔍 Check World Generation
- 🔍 Detailed Material Breakdown

**WorldDimensionCalculator:**
- Calculate Required Dimensions

**PixelCollisionSystem:**
- Validate Collision System

### Unity Menu

**Tools > Audio:**
- Generate Placeholder WAVs
- Open Placeholder Folder

---

## 📊 World Dimensions

### Common Configurations

```csharp
// 1×6 screens (Narrow & Deep)
width = 1024, height = 3072

// 3×3 screens (Balanced) - Default
width = 3072, height = 1536

// 6×6 screens (Huge)
width = 6144, height = 3072
```

### Coordinate Conversion

```csharp
// World space: Unity units, origin at center
// Pixel space: Array indices, origin at bottom-left

// Example for 4096×1536 world:
// World: -40.96 to +40.96 (X), -15.36 to +15.36 (Y)
// Pixel: 0 to 4095 (X), 0 to 1535 (Y)

// Conversion (done internally by systems)
pixelX = (worldX + halfWorldWidth) / cellSize;
pixelY = (worldY + halfWorldHeight) / cellSize;
```

---

## 🎯 Common Patterns

### Singleton Access (Null-Safe)

```csharp
if (PixelWorldManager.Instance != null)
{
    PixelWorldManager.Instance.ModifyWorld(pos, 10f, 0);
}
```

### Collision Check Pattern

```csharp
if (PixelCollisionSystem.Instance != null && 
    PixelCollisionSystem.Instance.HasData)
{
    bool solid = PixelCollisionSystem.Instance.IsSolid(pos);
}
```

### Audio Trigger Pattern

```csharp
// Always use AudioEventTriggers (null-safe, clean)
AudioEventTriggers.OnPlayerJump();
```

### Component Caching

```csharp
// ✅ GOOD: Cache in Start
private Rigidbody2D _rb;
void Start() { _rb = GetComponent<Rigidbody2D>(); }

// ❌ BAD: Every frame
void Update() { GetComponent<Rigidbody2D>().velocity = ...; }
```

---

## 📁 Key Files

### Core Scripts

```
Assets/Scripts/PixelWorld/
├── PixelWorldManager.cs       - World simulation
├── PixelCollisionSystem.cs    - Collision detection
├── PlayerController2D.cs      - Player movement
├── CameraFollow.cs             - Camera system
└── RenderingPresetController.cs - Visual presets

Assets/Scripts/Audio/
├── AudioManager.cs             - Audio system
└── AudioEventTriggers.cs       - Audio API

Assets/Scripts/Items/
└── Bomb.cs                     - Bomb item

Assets/Scripts/UI/
└── HotbarController.cs         - Hotbar UI
```

### Shaders

```
Assets/Shaders/Compute/
└── PixelWorld.compute          - GPU physics simulation

Assets/Shaders/Materials/
└── PixelWorldRender.shader     - Rendering
```

### Documentation

```
README.md                       - Project overview
Docs/API_REFERENCE.md           - Complete API docs
Docs/DEVELOPMENT_GUIDE.md       - Dev guide
Docs/WORLD_SYSTEMS.md           - Architecture
CONTRIBUTING.md                 - Collaboration guide
AUDIO_SETUP_GUIDE.md            - Audio setup
QUICK_REFERENCE.md              - This file
```

---

## ⚙️ Inspector Settings Quick Reference

### PixelWorldManager

```
World Dimensions:
  width: 4096 (pixels)
  height: 1536 (pixels)
  cellSize: 0.02 (Unity units)

Cave Generation:
  caveThreshold: 0.35 (0.2-0.6, higher = more hollow)
  caveFrequencyX: 8.0 (4-16, horizontal stretch)
  caveFrequencyY: 16.0 (8-32, vertical stretch)
  caveLayerBlend: 0.5 (0-1, complexity)

Water Generation:
  waterPoolChance: 0.3 (0-1, frequency)
  waterDepthThreshold: 0.25 (0.05-0.6, depth)
  waterNoiseThreshold: 0.55 (0.3-0.7, rarity)

Material Variety:
  sandFrequency: 20.0 (10-40, clustering)
  sandThresholdShallow: 0.6 (0.4-0.9, surface sand)
  sandThresholdDeep: 0.7 (0.5-0.95, deep sand)

Physics:
  stabilityThreshold: 2 (0-3, floating pixel cleanup)
```

### AudioManager

```
Settings:
  masterVolume: 1.0
  sfxVolume: 0.8
  ambienceVolume: 0.6
  maxSFXSources: 10
  enableFootsteps: true
  footstepInterval: 0.4 (seconds)
```

### PlayerController2D

```
Movement:
  moveSpeed: 5.0
  jumpForce: 8.0
  gravity: 20.0

Collision:
  playerHeight: 1.0
  playerWidth: 0.5
  verticalRays: 3
  groundCollisionThreshold: 2
```

---

## 🚀 Performance Tips

- **World Size:** Smaller = faster (3072×1536 is balanced)
- **Visual Presets:** Use Performance Mode (F6) if needed
- **Audio Pool:** Increase if > 10 simultaneous sounds needed
- **Collision Readback:** Default 50Hz is efficient
- **Cache References:** Always cache GetComponent calls

---

## 🐛 Common Issues

**No collision:**
- Wait 1-2 frames for HasData
- Check PixelCollisionSystem.Instance not null

**No sound:**
- Check AudioManager exists in scene
- Verify clips assigned in Inspector
- Check Master Volume > 0

**Black sprites:**
- Check Global Light 2D exists
- Light affects all sorting layers
- Sprites use Lit shader

**Player falls through ground:**
- Check surface height (0.95 default)
- Verify collision system working
- Check groundCollisionThreshold (2 default)

---

## 📞 Getting Help

**Documentation:**
- `Docs/API_REFERENCE.md` - Complete API
- `Docs/DEVELOPMENT_GUIDE.md` - Patterns
- `CONTRIBUTING.md` - Guidelines

**Tools:**
- Context menus on components
- Unity Console for errors
- Tools > Audio menu

**Contact:**
- johnny@z13labs.com

---

**Last Updated:** November 29, 2025  
**Quick Reference Version:** 1.0

