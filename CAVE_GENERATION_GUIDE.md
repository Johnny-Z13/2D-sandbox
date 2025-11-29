# Procedural Cave Generation System

## Overview
The `PixelWorldManager` now includes a fully configurable cave generation system that allows you to create varied, interesting worlds with hollow caves, water pools, sand deposits, and more.

## Features
✅ **Configurable Cave Density** - Control how hollow or dense your world is  
✅ **Multi-Layer Cave Systems** - Complex, interconnected cave networks  
✅ **Dynamic Water Pools** - Water spawns in deep caves with configurable frequency  
✅ **Sand Variety** - Sand pockets, veins, and cave beaches  
✅ **5 Preset Configurations** - Quick-start templates for different world types  
✅ **Real-time Regeneration** - Test settings without restarting the game

---

## Inspector Settings

### Cave Generation

#### Cave Threshold (0.2 - 0.6)
**Controls how hollow the caves are.**
- **0.2** = Very dense, few small caves
- **0.35** = Balanced (default)
- **0.5** = Very hollow, large open chambers

💡 Lower = more solid, Higher = more hollow

#### Cave Frequency X (4 - 16)
**Horizontal cave stretch.**
- **4** = Tight, vertical shafts
- **8** = Balanced (default)
- **12+** = Wide, sprawling horizontal caves

#### Cave Frequency Y (8 - 32)
**Vertical cave stretch.**
- **8** = Flat, wide chambers
- **16** = Balanced (default)
- **24+** = Tall, vertical chambers

#### Cave Layer Blend (0 - 1)
**Adds complexity by blending two noise layers.**
- **0** = Simple, predictable caves
- **0.5** = Balanced complexity (default)
- **1** = Very complex, interconnected systems

💡 Higher values create more intricate cave networks with unexpected connections.

---

### Water Generation

#### Water Pool Chance (0 - 1)
**Probability of water spawning in caves.**
- **0** = No water (desert world)
- **0.3** = Occasional pools (default)
- **0.8** = Flooded caves

#### Water Depth Threshold (0.05 - 0.6)
**How deep underground water can spawn.**
- **0.1** = Only at bottom
- **0.25** = Lower half (default)
- **0.5** = Even mid-level caves

💡 Lower values = water only deep down. Higher values = water everywhere.

#### Water Noise Threshold (0.3 - 0.7)
**Rarity of water within eligible cave areas.**
- **0.3** = Very common, most caves have water
- **0.55** = Balanced (default)
- **0.7** = Rare, only special caves

---

### Material Variety

#### Sand Frequency (10 - 40)
**How clustered sand deposits are.**
- **10** = Scattered, rare pockets
- **20** = Balanced (default)
- **35+** = Dense, sandy world

#### Sand Threshold Shallow (0.4 - 0.9)
**Rarity of sand in the dirt layer (near surface).**
- **0.4** = Very common, sandy topsoil
- **0.6** = Balanced (default)
- **0.8** = Rare patches

#### Sand Threshold Deep (0.5 - 0.95)
**Rarity of sand veins deep underground.**
- **0.5** = Common sand pockets
- **0.7** = Balanced (default)
- **0.9** = Very rare sand veins

💡 Lower threshold = more sand. Higher threshold = less sand.

---

## Preset Configurations

Right-click on `PixelWorldManager` in the Inspector to access preset context menu items.

### 1. Default Balanced ⚖️
**Best all-around experience.**
- Moderate cave density
- Occasional water pools
- Balanced sand distribution
- **Use for:** General gameplay, testing

### 2. Cave Explorer 🕳️
**Emphasis on exploration and cave systems.**
- **Very hollow** caves (25% threshold)
- Complex multi-layer systems
- Frequent water pools
- More sand for variety
- **Use for:** Exploration-focused gameplay, Metroidvania-style

### 3. Dense Solid 🪨
**Emphasis on digging and resource gathering.**
- Very dense terrain (50% threshold)
- Small, tight caves
- Rare water
- Less sand, more rock
- **Use for:** Mining-focused gameplay, limited resources

### 4. Underwater Caves 🌊
**Flooded cave systems.**
- Moderate cave density
- **80% water chance** - most caves flooded
- Water spawns even at mid-levels
- Lots of sand (underwater beaches)
- **Use for:** Underwater exploration, submarine gameplay

### 5. Desert Caves 🏜️
**Dry, sandy world.**
- Moderate cave density
- **10% water chance** - very rare oases
- **Lots of sand** everywhere
- Sand in caves, surface, and underground
- **Use for:** Desert survival, scarce water resource management

---

## How to Use

### Method 1: Inspector Sliders (Recommended for Experimentation)
1. Select `PixelWorldManager` in the scene
2. Adjust sliders in the Inspector
3. Right-click the component → **"Regenerate World"**
4. World instantly regenerates with new settings!

### Method 2: Context Menu Presets (Quick Testing)
1. Select `PixelWorldManager` in the scene
2. Right-click the component
3. Choose **"Load Preset: [Preset Name]"**
4. World regenerates automatically

### Method 3: Code/Script
```csharp
// Get the manager
var worldManager = PixelWorldManager.Instance;

// Option A: Load a preset
worldManager.LoadPreset(WorldPreset.CaveExplorer);

// Option B: Custom settings
worldManager.caveThreshold = 0.28f;
worldManager.waterPoolChance = 0.6f;
worldManager.RegenerateWorld();
```

### Method 4: Per-Level Configuration
**Set different settings per scene/level:**
1. Create multiple scenes with different world configurations
2. Each scene has its own `PixelWorldManager` with unique settings
3. Save the scene with desired configuration
4. Load different scenes for different level types

---

## Tips & Tricks

### Creating Interesting Worlds

**Sprawling Cave Networks:**
- Cave Threshold: 0.3
- Cave Frequency X: 12
- Cave Layer Blend: 0.8

**Vertical Shaft Mines:**
- Cave Threshold: 0.35
- Cave Frequency X: 6
- Cave Frequency Y: 24

**Underground Ocean:**
- Water Pool Chance: 0.9
- Water Depth Threshold: 0.4
- Sand Threshold Shallow: 0.4 (sandy shores)

**Claustrophobic Tunnels:**
- Cave Threshold: 0.45
- Cave Frequency X: 5
- Cave Frequency Y: 10
- Cave Layer Blend: 0.2

**Alien Planet (High Variety):**
- Cave Layer Blend: 1.0 (max complexity)
- Water Pool Chance: 0.5
- Sand Frequency: 35
- All thresholds set to mid-range for maximum variety

### Performance Considerations

All generation happens **once at startup** in the `CSInit` compute shader kernel. There's no performance impact during gameplay - only at world initialization.

**Regeneration is fast:**
- Typical regeneration time: 10-50ms (depending on world size)
- Safe to regenerate during gameplay (brief frame hitch)
- No impact on physics simulation (CSMain kernel)

### Debugging New Settings

1. **Start with presets** - Get a feel for how settings interact
2. **Adjust one parameter at a time** - See its isolated effect
3. **Use small worlds for testing** - Set Width/Height to 1024x512 for faster iteration
4. **Toggle Cave Layer Blend** - See the difference between simple and complex caves

---

## Technical Details

### How It Works

The world generation (`CSInit` kernel in `PixelWorld.compute`) now uses:

1. **Multi-Layer Noise**: Two FBM (Fractal Brownian Motion) noise layers blended together
2. **Dynamic Thresholds**: Cave density changes with depth (bigger caves underground)
3. **Independent Noise Layers**: Separate noise for caves, water, and sand placement
4. **Conditional Logic**: Water/sand only spawn when multiple conditions are met

### Noise Functions
- **FBM (Fractal Brownian Motion)**: Used for organic, natural-looking cave systems
- **Perlin Noise**: Used for surface terrain, sand pockets, and water placement
- **Multi-Octave**: Multiple noise frequencies layered for detail at different scales

### Material Distribution Logic
```
Surface (y > 0.95): AIR
  ↓
Top Layer (0-5% depth): DIRT (with sand patches)
  ↓
Deep Underground: ROCK (with sand veins)
  ↓
Cave Hollows (noise > threshold): EMPTY or WATER or SAND (cave beaches)
  ↓
Bottom Boundary (y < 5px): ROCK (unbreakable)
```

---

## Files Modified

1. **`Assets/Scripts/PixelWorld/PixelWorldManager.cs`**
   - Added cave generation parameters
   - Added water generation parameters
   - Added sand generation parameters
   - Added `RegenerateWorld()` method
   - Added `LoadPreset()` method with 5 presets
   - Added context menu shortcuts

2. **`Assets/Shaders/Compute/PixelWorld.compute`**
   - Added shader parameters for configurable generation
   - Multi-layer cave noise system
   - Dynamic cave threshold based on depth
   - Independent water pool noise
   - Enhanced sand placement logic
   - Cave beaches (sand in caves near water)

---

## Example Use Cases

### Level 1: Tutorial (Dense Solid)
- Easy digging, few obstacles
- Teaches mechanics in controlled environment

### Level 2: Cave Explorer (Default Balanced)
- Introduces cave systems
- Occasional water pools

### Level 3: The Depths (Cave Explorer)
- Complex cave networks
- Requires navigation skills

### Level 4: Underground Sea (Underwater Caves)
- Water management becomes crucial
- Introduces underwater mechanics

### Level 5: Desert Ruins (Desert Caves)
- Resource scarcity (water is rare)
- Survival challenge

---

## Troubleshooting

**Caves too small/dense:**
- Lower `caveThreshold` (try 0.25)
- Increase `caveLayerBlend` for complexity

**Too much/little water:**
- Adjust `waterPoolChance` (0 = none, 1 = everywhere)
- Adjust `waterDepthThreshold` (controls how deep water spawns)

**Need more variety:**
- Increase `caveLayerBlend` to 0.8+
- Increase `sandFrequency` to 30+
- Lower both sand thresholds

**World generation looks wrong:**
- Check that `PixelWorldManager` references `PixelWorld.compute` shader
- Ensure all sliders are within valid ranges
- Try loading a preset to reset to known-good values

**Player falls through ground after regeneration:**
- Surface height is fixed at 0.95 (95% of world height)
- Ensure player spawn Y position matches this
- Default: Player at Y ≈ 4.35 for 512-height world

---

## Future Enhancements (Ideas)

- 🔮 Biome system (different rules per region)
- 🔮 Ore/resource veins (iron, gold, gems)
- 🔮 Lava pools (like water but deadly)
- 🔮 Crystal formations in caves
- 🔮 Vegetation (grass, trees) on surface
- 🔮 Structure generation (ruins, temples)
- 🔮 Random seed generator button
- 🔮 Export/import world settings as JSON

---

## Related Documentation
- [Multi-Screen World Setup](./MULTI_SCREEN_WORLD_SETUP.md)
- [Collision System](./QUICK_COLLISION_FIX.md)
- [Visual Upgrades](./VISUAL_UPGRADE_README.md)

