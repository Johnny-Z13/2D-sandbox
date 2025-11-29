# Project Structure

Directory organization and file conventions for the 2D Pixel Sandbox.

---

## 📂 Root Directory

```
Assets/
├── Art/                       # Sprites and textures
├── Audio/                     # Sound effects and music
├── CodeMonkey/                # Third-party UI tools
├── DeadRevolver/              # Player character assets
├── Docs/                      # Documentation (in-editor view)
├── Editor/                    # Unity Editor scripts
├── Fonts/                     # TextMeshPro fonts
├── ID InfiniteDesign/         # Robot/Enemy assets
├── Materials/                 # Physics and Render materials
├── Prefabs/                   # Game object prefabs
├── Scenes/                    # Unity scenes
├── Scripts/                   # C# Source code
├── Settings/                  # Render pipeline settings
├── Shaders/                   # Compute and Render shaders
└── ...
```

---

## 📂 Scripts (`Assets/Scripts/`)

### `PixelWorld/` - Core Systems
The heart of the simulation.

- **`PixelWorldManager.cs`**: Main entry point. Manages the Compute Shader, RenderTextures, and simulation loop.
- **`PixelCollisionSystem.cs`**: Handles AsyncGPUReadback to provide CPU-side collision data.
- **`PlayerController2D.cs`**: Player movement, input handling, and interaction with the pixel world.
- **`CameraFollow.cs`**: Smooth camera tracking with world boundary clamping.
- **`RenderingPresetController.cs`**: Manages visual settings (bloom, shader params) and presets.
- **`WorldRendererScaler.cs`**: Ensures the world quad matches the simulation dimensions.
- **`WorldGenerationDebugger.cs`**: Editor tools for analyzing terrain generation.
- **`WorldSystemValidator.cs`**: Health check tool for scene setup.

### `Items/` - Gameplay Objects
- **`Bomb.cs`**: Physics-based explosive that destroys terrain and pushes entities.

### `UI/` - User Interface
- **`HotbarController.cs`**: Manages selected tools (Dig, Sand, Water, Bomb).

### `Editor/` - Editor Tools
- **`FixLighting.cs`**: Utility to fix 2D lighting issues.
- **`CheckRenderPipeline.cs`**: Diagnostic tool for URP setup.

---

## 📂 Shaders (`Assets/Shaders/`)

### `Compute/`
- **`PixelWorld.compute`**: The physics engine. Contains `CSInit` (generation) and `CSMain` (simulation) kernels.

### `Materials/`
- **`PixelWorldRender.shader`**: The rendering engine. Handles coloring, lighting, and visual effects for the pixel grid.

---

## 📂 Settings (`Assets/Settings/`)

Contains Universal Render Pipeline (URP) configuration.

- **`UniversalRP.asset`**: Main pipeline asset.
- **`Renderer2D.asset`**: 2D Renderer configuration (Lighting, Post-processing).

---

## 📂 Prefabs (`Assets/Prefabs/`)

- **`Player-01.prefab`**: The player character with Animator, Controller, and Input components.
- **`Bomb.prefab`**: Explosive item prefab.

---

## 📝 File Conventions

### Scripts
- **Namespace**: `PixelWorld`
- **Class Names**: PascalCase (e.g., `PixelWorldManager`)
- **Private Fields**: `_camelCase` (e.g., `_worldWidth`)
- **Public Fields**: `camelCase` (e.g., `worldWidth`)
- **Serialized Fields**: `camelCase` with `[SerializeField]` attribute

### Shaders
- **Properties**: `_PascalCase` (e.g., `_SandColor`)
- **Macros**: `UPPER_CASE` (e.g., `MAT_SAND`)

### Scene Hierarchy
- **Managers**: Root level, no parent (e.g., `PixelWorldManager`)
- **UI**: Under `Canvas` or `HUD_Root`
- **World**: `PixelWorldScreen` (Quad)

---

## 🔄 Key Dependencies

- **Unity.InputSystem**: Used for all input handling.
- **Unity.RenderPipelines.Universal**: Used for rendering and 2D lighting.
- **Unity.Mathematics**: Used in some physics calculations.

---

**Note:** This structure is subject to change as the project evolves. Always check the actual project files for the most up-to-date organization.
