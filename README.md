# 2D Pixel Sandbox - Destructible Terrain Game

A Unity 2D sandbox game with fully destructible pixel-based terrain, inspired by Noita. Features procedural world generation, dynamic physics simulation, and visual effects.

---

## 🚀 Quick Start

### First Time Setup
1. Open project in Unity 2022.3 LTS or newer
2. Open scene: `Assets/Scenes/Prototype-01.unity`
3. Press Play ▶️
4. Use WASD to move, Space to jump, E to dig

### Controls
- **WASD / Arrow Keys** - Move
- **Space** - Jump
- **E** - Dig/Interact (Hold to dig continuously)
- **C** - Crouch
- **Left Mouse** - Paint Sand
- **Right Mouse** - Paint Water
- **Middle Mouse** - Erase
- **1, 2** - Cycle Hotbar (Dig, Sand, Water, Bomb)
- **F1-F6 Keys** - Graphics Presets
- **F12** - Toggle Debug Info

---

## 📚 Documentation

### Getting Started
- **[Controls & Input](Docs/CONTROLS.md)** - Complete control reference
- **[Audio Setup Guide](AUDIO_SETUP_GUIDE.md)** - 5-minute audio setup

### Core Systems
- **[API Reference](Docs/API_REFERENCE.md)** - **Complete API documentation**
- **[World Systems](Docs/WORLD_SYSTEMS.md)** - Physics, collision, simulation
- **[Audio System](Docs/AUDIO_SYSTEM.md)** - Audio implementation details
- **[Project Structure](Docs/PROJECT_STRUCTURE.md)** - File organization

### Development
- **[Development Guide](Docs/DEVELOPMENT_GUIDE.md)** - Adding new features
- **[Contributing Guide](CONTRIBUTING.md)** - **Collaboration guidelines**
- **[Codebase Review](CODEBASE_REVIEW_REPORT.md)** - Technical review

### Reference
- **[Product Requirements](Assets/2D-sandbox-PRD)** - Design goals
- **[Noita Technical Reference](Assets/Noita_Destructible_Environment_Tech.md)** - Inspiration
- **[Changelog](CHANGELOG.md)** - Version history
- **[Audio System](Docs/AUDIO_SYSTEM.md)** - Sound implementation

---

## ✨ Key Features

### Destructible Terrain
- Fully dynamic pixel-based world
- GPU-accelerated simulation (compute shaders)
- Real-time physics for sand, water, rock, dirt
- 4096×1536 pixel world (scalable)
- **New:** Floating pixel cleanup (stability threshold)
- **New:** Improved collision tolerance for falling objects

### Procedural Generation
- Multi-layer cave systems with FBM noise
- Configurable cave density and complexity
- Dynamic water pools in caves
- Sand deposits and mineral veins
- 5 presets: Default, Cave Explorer, Dense Solid, Underwater, Desert

### Visual Effects
- **Universal Render Pipeline (URP)** with 2D Lighting
- Multi-layer sand glitter & shimmer
- Water caustics and depth shading
- Procedural rock textures with mineral veins
- 6 visual presets for different looks
- Post-processing with bloom and color grading

### Player System
- Smooth 2D platformer movement
- Pixel-perfect collision with terrain
- Dig/mine terrain in real-time (Rock, Dirt, Sand)
- **New:** Bomb item with knockback and context-sensitive debris
- Camera follow with world bounds
- Animation system with multiple states

### Audio System
- Comprehensive sound effect coverage (15+ sounds)
- Looping ambient music
- Audio pooling for efficient playback
- Volume controls (master, SFX, ambience)
- Easy-to-use event trigger API
- **New:** Audio system with SFX for all actions

---

## 🎮 Gameplay Features

### Current
- Explore procedural cave systems
- Dig through terrain (rock, dirt, sand)
- Paint new materials (sand, water)
- Use Bombs to clear large areas
- Camera follows player with smooth zoom
- Multiple animation states (idle, run, jump, crouch, dig)

### Planned
- Replace placeholder audio with final sound effects
- Inventory and hotbar UI (In Progress)
- More materials (lava, gas, plant matter)
- Enemy AI
- Puzzle mechanics
- Save/load system
- Additional audio (replacing placeholders)

---

## 🛠️ Technical Stack

### Core Technologies
- **Unity 2022.3 LTS** (URP - Universal Render Pipeline)
- **Compute Shaders** - GPU-accelerated physics simulation
- **Unity Input System** - Modern input handling
- **TextMeshPro** - UI text rendering

### Architecture
- **RenderTexture (RInt format)** - Pixel world data storage
- **Double-buffered simulation** - Efficient GPU state updates
- **Cellular automata** - Sand and water physics
- **AsyncGPUReadback** - Collision detection on CPU
- **Fractal Brownian Motion** - Procedural cave generation

### Performance
- 60 FPS target on mid-range hardware
- ~6 million pixels simulated per frame
- GPU-accelerated for excellent performance
- Configurable quality settings via presets

---

## 📁 Project Structure

```
Assets/
├── Scenes/                    # Unity scenes
│   └── Prototype-01.unity     # Main scene
├── Scripts/
│   └── PixelWorld/            # Core game systems
│       ├── PixelWorldManager.cs       # World simulation
│       ├── PixelCollisionSystem.cs    # Collision detection
│       ├── PlayerController2D.cs      # Player movement
│       ├── CameraFollow.cs            # Camera system
│       ├── WorldRendererScaler.cs     # Renderer setup
│       ├── RenderingPresetController.cs # Visual presets
│       ├── WorldGenerationDebugger.cs # Debug tools
│       └── ...
├── Shaders/
│   ├── Compute/
│   │   └── PixelWorld.compute # Physics simulation
│   └── Materials/
│       └── PixelWorldRender.shader # Rendering
├── Materials/
│   └── Mat_PixelWorld.mat     # World material
├── Prefabs/
│   └── Player-01.prefab       # Player character
├── InputSystem_Actions.inputactions # Input mappings
└── DefaultVolumeProfile.asset # Post-processing

Docs/
├── CONTROLS.md                # Control reference
├── WORLD_SYSTEMS.md           # Systems guide
├── PROJECT_STRUCTURE.md       # File organization
├── DEVELOPMENT_GUIDE.md       # Dev guide
└── API/                       # Code documentation
```

---

## 🎨 Graphics Presets

Press **F1-F6** keys in play mode (requires RenderingPresetController):

- **F1** - Default (Balanced look)
- **F2** - Desert Gold (Warm, sparkly sand)
- **F3** - Subtle Realism (Understated effects)
- **F4** - Extreme Showcase (Maximum visual impact)
- **F5** - Screenshot Mode (Marketing-ready)
- **F6** - Performance Mode (Speed optimized)

*On-screen notification shows current preset for 3 seconds*

---

## 🐛 Debug Tools

### In-Game
- **F12** - Toggle debug info (when VisualDebugger active)
- **Context Menus** - Right-click components in Inspector for diagnostics

### Inspector Tools
- `WorldGenerationDebugger` - Verify world generation
- `WorldSystemValidator` - Check system integrity
- `WorldBoundsDebugger` - Visualize world boundaries (Scene view)
- `QuickDiagnostic` - Runtime diagnostics

---

## 📝 Recent Changes

### Latest Updates
- ✅ **URP Integration:** Switched to Universal Render Pipeline for better 2D lighting.
- ✅ **Lighting Fixes:** Resolved black sprite issues with Global Light 2D configuration.
- ✅ **Physics Improvements:** Added stability threshold to clean up floating pixels.
- ✅ **Collision Tuning:** Improved falling collision detection to ignore small dust particles.
- ✅ **Bomb Mechanics:** Added knockback force and context-sensitive debris (water splash vs sand).
- ✅ **Sand Digging:** Enabled digging through sand material.

See `CHANGELOG.md` for full history.

---

## 🔧 Development

### Requirements
- Unity 2022.3 LTS or newer
- Windows/Mac/Linux
- GPU with compute shader support

### Getting Started (Developers)
1. Clone/download project
2. Open in Unity
3. Read `Docs/DEVELOPMENT_GUIDE.md`
4. Check `Docs/PROJECT_STRUCTURE.md` for organization
5. Review API docs in `Docs/API/`

### Adding New Features
See `Docs/DEVELOPMENT_GUIDE.md` for:
- Code style guidelines
- Where to add new scripts
- How to use debug tools
- Performance best practices

---

## 📖 Learning Resources

### Understanding the Tech
- **Noita Reference** - `Assets/Noita_Destructible_Environment_Tech.md`
- **Product Requirements** - `Assets/2D-sandbox-PRD`
- **Compute Shaders** - `Assets/Shaders/Compute/PixelWorld.compute` (well-commented)

### Unity Concepts Used
- Compute Shaders & GPU programming
- Unity Input System
- Universal Render Pipeline (URP)
- Post-Processing Stack
- Async GPU Readback
- 2D Animation & State Machines

---

## 🎯 Next Steps

### For Players
1. Try different cave generation presets
2. Experiment with visual presets
3. Dig deep and explore!

### For Developers
1. **Read the docs:** Start with `QUICK_REFERENCE.md` for quick overview
2. **API Reference:** See `Docs/API_REFERENCE.md` for complete API documentation
3. **Set up audio:** Follow `AUDIO_SETUP_GUIDE.md` (5 minutes)
4. **Contributing:** Read `CONTRIBUTING.md` before making changes
5. **Understand core systems:** `Docs/WORLD_SYSTEMS.md`
6. Start experimenting!

---

## 📜 License

This project is provided as-is for educational and development purposes.

---

## 🙏 Credits

- **Pixel Art Assets**: Dead Revolver (Pixel Prototype Player Sprites)
- **UI Assets**: Code Monkey Toolkit
- **Robot Assets**: ID Infinite Design
- **Technical Inspiration**: Noita by Nolla Games

---

## 📞 Support

Copyright © 2025 Z13 Labs
Contact: johnny@z13labs.com

---

**Built with ❤️ using Unity**

*Last Updated: November 29, 2025*
