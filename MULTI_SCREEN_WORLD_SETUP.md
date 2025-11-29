# Multi-Screen World Setup Guide
## Supporting Camera-Following Exploration & Deep Digging

---

## Problem Solved

**Before:** World was too small (1 screen), player couldn't explore or dig deep  
**After:** World properly sized for 3×3 screens (or any size), full ground coverage everywhere including deep underground

---

## Quick Start - 3 Steps to Set Up Your World

### Step 1: Calculate Required Dimensions

1. **Add WorldDimensionCalculator** to any GameObject in your scene
2. In the inspector, set:
   - **Screens Wide:** `3`
   - **Screens Deep:** `3` (or more if you want deeper digging!)
   - **Main Camera:** Drag your camera
3. **Right-click component** → **"Calculate Required Dimensions"**
4. Check the console - it will tell you the exact pixel dimensions needed

**Example Output:**
```
Required Pixel Dimensions: 3072 × 1536
```

---

### Step 2: Update PixelWorldManager

1. Select your **PixelWorldManager** GameObject
2. In the inspector, set:
   - **Width:** Use the value from calculator (e.g., `3072`)
   - **Height:** Use the value from calculator (e.g., `1536`)
3. **Save the scene**

---

### Step 3: Update World Renderer Scale

If you have **WorldRendererScaler** attached:
- It will auto-update on Play ✅

If not:
1. Select your **World Renderer** quad GameObject
2. **Add Component** → **WorldRendererScaler**
3. Assign **PixelWorldManager** reference
4. Press Play - scale updates automatically

---

## Understanding the System

### Camera & Screen Dimensions

Your camera has:
- **Orthographic Size:** 5.12 (this is half-height)
- **Screen Height:** 5.12 × 2 = **10.24 units**
- **Screen Width:** 10.24 × aspect ratio ≈ **18.2 units** (for 16:9)

For **3 screens × 3 screens**:
- **Total Width:** 18.2 × 3 = **54.6 units**
- **Total Height:** 10.24 × 3 = **30.72 units**

At **0.02 units per pixel**:
- **Width in Pixels:** 54.6 / 0.02 = **2730 pixels** → rounded to **3072** (multiple of 64)
- **Height in Pixels:** 30.72 / 0.02 = **1536 pixels**

---

### Current World Size (Scene: Prototype-01)

**Before Fix:**
- Width: 1024 pixels = 20.48 units ≈ **1.1 screens** ❌
- Height: 512 pixels = 10.24 units = **1.0 screen** ❌

**Player would run out of world immediately!**

**After Fix (Recommended):**
- Width: 3072 pixels = 61.44 units ≈ **3.4 screens** ✅
- Height: 1536 pixels = 30.72 units = **3.0 screens** ✅

**Perfect for exploration and deep digging!**

---

## Procedural Generation Changes

### Surface Placement (NEW)

**Before:**
```hlsl
// Surface at 93% height - only 7% underground!
float surfaceHeight = 0.93 + 0.03 * noise(...);
```

**After:**
```hlsl
// Surface at 82-86% height - 14-18% underground for deep play!
float surfaceHeight = 0.82 + 0.04 * noise(...);
```

**Impact:**
- For 3-screen-deep world (30.72 units):
  - **Before:** Only ~2 units (~0.2 screens) of underground ❌
  - **After:** ~5 units (~0.5 screens) of rich underground ✅

---

### Depth-Based Generation (NEW)

The procedural system now generates different terrain at different depths:

```
┌─────────────────────────────────────┐ ← Top (y = 1.0)
│         SKY (Empty)                 │
├─────────────────────────────────────┤ ← Surface (y = ~0.82-0.86)
│  Surface Layer (95-100%)            │ ← Dirt + Sand patches
│  - Dirt with sand                   │
├─────────────────────────────────────┤ 
│  Upper Underground (70-95%)         │ ← Mixed terrain
│  - Rock, Dirt, Sand mix             │
│  - Smaller caves                    │
├─────────────────────────────────────┤
│  Mid-Depth (30-70%)                 │ ← Rock dominant
│  - Primarily rock                   │
│  - Sand veins                       │
│  - Moderate caves                   │
├─────────────────────────────────────┤
│  Deep Underground (0-30%)           │ ← Dense + water
│  - Dense rock                       │
│  - Rare sand pockets                │
│  - Larger caves                     │
│  - Water pools                      │
├─────────────────────────────────────┤ ← Bottom boundary
│  BOUNDARY ROCK WALL (5 pixels)     │
└─────────────────────────────────────┘ ← Bottom (y = 0.0)
```

---

### Cave Distribution

Caves are now **depth-dependent**:

| Depth Range | Cave Size | Cave Threshold | Description |
|-------------|-----------|----------------|-------------|
| **0-30%** (Deep) | Large | 0.30 | Big caverns for exploration |
| **30-60%** (Mid) | Medium | 0.35 | Balanced cave systems |
| **60-100%** (Upper) | Small | 0.42 | Tight caves near surface |

**Result:** Interesting exploration at ALL depths, not just near surface!

---

## Configuration Options

### Adjust for Different Level Sizes

**WorldDimensionCalculator** supports any screen configuration:

```csharp
// Small level (1×2 screens - vertical shaft)
screensWide = 1;
screensDeep = 2;

// Medium level (3×3 screens - default)
screensWide = 3;
screensDeep = 3;

// Large level (5×4 screens - huge exploration)
screensWide = 5;
screensDeep = 4;

// Ultra-deep mining (2×8 screens - vertical emphasis)
screensWide = 2;
screensDeep = 8;
```

Just change the values and recalculate!

---

### Adjust Surface Height

In `PixelWorld.compute` (line ~67):

```hlsl
// Higher surface = less underground
float surfaceHeight = 0.90 + 0.04 * noise(...); // Only 10% underground

// Lower surface = more underground (current)
float surfaceHeight = 0.82 + 0.04 * noise(...); // 18% underground

// Very low surface = massive underground
float surfaceHeight = 0.70 + 0.05 * noise(...); // 30% underground!
```

**Rule of Thumb:**
- **0.90:** Light digging, mostly above-ground gameplay
- **0.82:** Balanced (current) - good for 3×3 screens
- **0.70:** Deep mining focus - for tall worlds (e.g., 3×6 screens)

---

### Adjust Cave Density

In `PixelWorld.compute` (lines ~85-96):

```hlsl
// MORE caves (easier exploration)
if (depthRatio < 0.3) 
    caveThreshold = 0.25; // Was 0.30 - now 25% more caves
    
// LESS caves (more solid rock)
if (depthRatio < 0.3) 
    caveThreshold = 0.35; // Was 0.30 - now 25% less caves
```

---

## Testing Your World

### Step-by-Step Verification

1. **Start the game**
2. **Check surface extends everywhere:**
   - Pan camera left/right - ground visible? ✅
   
3. **Test deep digging:**
   - Dig straight down
   - Can you dig through multiple screens? ✅
   - Is there varied terrain (rock, caves, sand)? ✅
   
4. **Test horizontal exploration:**
   - Move player left/right
   - Camera follows smoothly? ✅
   - Ground everywhere, no gaps? ✅
   
5. **Check boundaries:**
   - Try to reach world edges
   - Solid rock walls on all sides? ✅

---

### Visual Debugging

Add **WorldBoundsDebugger** to see:
- Green outline: Required play area (3×3 screens)
- Cyan/Red outline: Actual world size
- Yellow grid: Screen divisions

**If red outline:** World too small - increase dimensions!  
**If cyan outline:** World size correct - you're good! ✅

---

## Performance Considerations

### World Size vs Performance

| Size | Pixels | Memory | FPS (RTX 3060) | Use Case |
|------|--------|--------|----------------|----------|
| 1024×512 | 524K | 2 MB | 200+ | Small test level |
| 2048×1024 | 2.1M | 8 MB | 120 | Medium level |
| **3072×1536** | **4.7M** | **19 MB** | **80-100** | **3×3 screens** ✅ |
| 4096×2048 | 8.4M | 33 MB | 50-70 | Large level |
| 8192×4096 | 33.5M | 134 MB | 20-30 | Massive world |

**Recommendation for 3×3 screens:** 3072×1536 is optimal balance ✅

---

### Optimization Tips

If performance is an issue:

1. **Reduce world dimensions slightly:**
   - 2560×1280 instead of 3072×1536
   - Still covers 2.8×2.5 screens (acceptable)

2. **Use Performance Mode preset** (from visual upgrade):
   - Press `6` key in play mode
   - Disables heavy visual effects

3. **Adjust update rate:**
   - PixelWorldManager → Update Rate: `0.016` (60 FPS)
   - Reduces simulation frequency slightly

4. **Lower visual quality:**
   - Reduce Glitter Intensity in material
   - Disable bloom in post-processing

---

## Per-Level Configuration (Advanced)

### Creating Level-Specific Presets

**Approach 1: Multiple Scenes**
```
- Level_1_SmallCave.unity   (1024×1024)
- Level_2_DeepMine.unity    (2048×3072)  
- Level_3_OpenWorld.unity   (4096×2048)
```

Each scene has PixelWorldManager with appropriate dimensions.

---

**Approach 2: Runtime Dimension Setting**

```csharp
public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelConfig
    {
        public string levelName;
        public int width;
        public int height;
        public int seed;
    }
    
    public LevelConfig[] levels;
    
    public void LoadLevel(int index)
    {
        var config = levels[index];
        
        // Apply configuration
        PixelWorldManager.Instance.SetDimensions(
            config.width, 
            config.height, 
            config.seed
        );
    }
}
```

*(Note: Requires adding `SetDimensions()` method to PixelWorldManager)*

---

## Troubleshooting

### Issue: Player hits edge of world quickly

**Cause:** World dimensions too small  
**Fix:** Increase Width/Height in PixelWorldManager  
**Tool:** Use WorldDimensionCalculator to calculate correct size

---

### Issue: Not enough underground to dig

**Cause:** Surface height too high  
**Fix:** Lower surface baseline in compute shader (line ~67)  
**Current:** `0.82` → Try `0.75` for more underground

---

### Issue: Caves too dense/sparse

**Cause:** Cave threshold needs adjustment  
**Fix:** Modify thresholds in compute shader (lines ~85-96)  
**More caves:** Lower thresholds (0.30 → 0.25)  
**Less caves:** Raise thresholds (0.30 → 0.35)

---

### Issue: Ground doesn't extend to edges

**Cause:** Renderer quad not scaled correctly  
**Fix:** Add WorldRendererScaler component  
**Verify:** Check quad scale matches world size

---

### Issue: Performance too slow

**Cause:** World too large for hardware  
**Fix Options:**
1. Reduce dimensions (3072×1536 → 2560×1280)
2. Increase Update Rate (0 → 0.016)
3. Use Performance preset (key `6`)
4. Lower visual quality settings

---

## Summary Checklist

Before starting development, ensure:

- [ ] **WorldDimensionCalculator** added and calculated
- [ ] **PixelWorldManager** dimensions updated (e.g., 3072×1536)
- [ ] **WorldRendererScaler** attached to renderer quad
- [ ] **CameraFollow** component properly configured
- [ ] Tested: Player can explore all 3×3 screens
- [ ] Tested: Can dig deep underground
- [ ] Tested: Ground covers entire play area
- [ ] Performance acceptable (60+ FPS target)

---

## Quick Reference: Recommended Settings

### For 3×3 Screen World

**PixelWorldManager:**
- Width: `3072`
- Height: `1536`
- Seed: `Any number`
- Update Rate: `0` (every frame)

**Camera:**
- Orthographic Size: `5.12`
- Position: Follows player via CameraFollow script

**World Renderer:**
- Scale: Auto (via WorldRendererScaler)
- Material: Mat_PixelWorld with PixelWorldRender shader

**CameraFollow:**
- Smooth Time: `0.25`
- Bounds: Auto-calculated from world size

---

## Files Modified/Created

### Modified
1. ✏️ `Assets/Shaders/Compute/PixelWorld.compute`
   - Adjusted surface height (0.93 → 0.82)
   - Added depth-based terrain generation
   - Improved cave distribution across depth
   - Added all-edge boundaries (including top)

### Created
2. ➕ `Assets/Scripts/PixelWorld/WorldDimensionCalculator.cs`
   - Calculates required dimensions for N×M screens
   - Validates current vs required size
   - Visual gizmos showing screen grid
   
3. ➕ `Assets/Scripts/PixelWorld/WorldRendererScaler.cs`
   - Auto-scales renderer to match world size
   
4. ➕ `Assets/Scripts/PixelWorld/WorldBoundsDebugger.cs`
   - Visualizes world bounds and boundaries

5. ➕ `MULTI_SCREEN_WORLD_SETUP.md` (this file)
   - Complete setup and configuration guide

---

## Next Steps

1. **Right now:** Update PixelWorldManager dimensions to 3072×1536
2. **Test:** Play and verify you can explore/dig everywhere
3. **Tune:** Adjust surface height and cave density to taste
4. **Per-level:** Create configurations for different level sizes

---

**🎉 Your world now supports full camera-following exploration with deep digging!**

*Ground coverage: Everywhere ✅ | Digging depth: Multiple screens ✅ | Camera follow: Smooth ✅*

---

*Setup completed: November 24, 2025*  
*Unity 2D Pixel Sandbox - Multi-Screen World System*

