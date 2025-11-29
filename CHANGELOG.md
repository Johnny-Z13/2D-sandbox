# Changelog

All notable changes to the 2D Pixel Sandbox project.

## [Unreleased] - 2025-11-29

### Added
- **Audio System:**
  - Complete AudioManager with pooling and cooldown system
  - 17+ sound effects for player actions, world interaction, and UI
  - Looping ambient music support
  - AudioEventTriggers helper class for easy integration
  - Audio integrated into PlayerController2D, Bomb, HotbarController, RenderingPresetController
  - Placeholder audio structure ready for final assets
  - Comprehensive audio documentation in `Docs/AUDIO_SYSTEM.md`
  - Automatic volume control and pitch variation

- **Professional Codebase Review:**
  - Created comprehensive `.gitignore` file following Unity best practices
  - Added CodeMonkey toolkit folder to `.gitignore` for cleaner version control
  - Validated all C# scripts - no linter errors found
  - Comprehensive documentation consolidation

### Removed
- Legacy `_Recovery` folder and outdated scene files
- Duplicate water physics documentation (consolidated into `Docs/WATER_PHYSICS_IMPROVEMENTS.md`)
- Redundant visual showcase documentation files
- Unused `SampleScene.unity` (project uses `Prototype-01.unity`)
- Temporary fix documentation files (information integrated into main docs)

### Changed
- Documentation structure improved and streamlined
- `START_HERE.md` updated with current file references
- Better organization of markdown files

---

## [Previous Updates] - 2025-11-29

### Added
- **Bomb Mechanics:**
  - Added `Bomb.cs` with physics-based movement.
  - Implemented explosion knockback on player (horizontal and vertical).
  - Added context-sensitive debris: bombs exploding in water splash water, bombs in air create no debris, bombs on ground create sand.
- **Physics Improvements:**
  - Added `Stability Threshold` to `PixelWorldManager` and Compute Shader. Floating pixels (dust) now crumble into sand if unsupported.
  - Added `Ground Collision Threshold` to `PlayerController2D`. Player now ignores small isolated pixels when falling, preventing "standing on dust".
- **Sand Digging:**
  - Updated `PixelWorld.compute` to allow digging through sand material.

### Changed
- **Documentation:**
  - Created comprehensive `Docs/API_REFERENCE.md` with complete API documentation
  - Added `CONTRIBUTING.md` with collaboration guidelines
  - Added `QUICK_REFERENCE.md` for at-a-glance information
  - Updated `README.md` with audio system info and reorganized documentation links
  - All documentation reviewed and updated for clarity
  
- **Rendering System:**
  - Migrated to **Universal Render Pipeline (URP)**.
  - Replaced `MKGlow` with URP Post-Processing (Bloom).
  - Updated `RenderingPresetController` to work with URP Volume framework.
  
- **Lighting:**
  - Fixed `Global Light 2D` configuration to ensure correct scene illumination.
  - Forced Global Light to affect all sorting layers to fix black sprite issues.

### Fixed
- Fixed compilation errors caused by missing MKGlow scripts.
- Fixed "Renderer2D is missing RendererFeatures" error in settings.
- Fixed player and bomb sprites appearing black due to lighting configuration.
- Fixed player getting stuck on single floating pixels.

---

## [Visual Upgrade] - 2025-11-28

### Added
- **Visual Presets:**
  - Added `RenderingPresetController` with 6 presets (Default, Desert Gold, Subtle, Extreme, Screenshot, Performance).
  - Added F1-F6 hotkeys for switching presets.
- **Shader Effects:**
  - Implemented multi-layer sand glitter and shimmer.
  - Added water caustics and depth-based shading.
  - Added procedural rock textures with mineral veins.

### Changed
- Optimized bloom settings for better performance.
- Updated `PixelWorldRender.shader` for enhanced visuals.

---

## [Cave Generation Update] - 2025-11-27

### Added
- **Procedural Generation:**
  - Implemented multi-layer FBM noise for cave generation.
  - Added configurable parameters for cave density, frequency, and complexity.
  - Added dynamic water pools and sand deposits.
- **World Presets:**
  - Added 5 generation presets: Default, Cave Explorer, Dense Solid, Underwater, Desert.

### Fixed
- Fixed shader warnings related to float precision.
- Resolved input system conflicts.

---

## [Initial Release] - 2025-11-20

### Added
- Basic pixel simulation (sand, water, rock).
- Player movement and collision.
- Digging and painting mechanics.
- Camera follow system.
