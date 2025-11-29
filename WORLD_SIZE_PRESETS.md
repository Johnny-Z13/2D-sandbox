# World Size Presets

Quick-load world dimensions for different gameplay styles.

---

## 🎯 3 World Size Presets

### 1×6 - Narrow & Deep ⬇️
**Dimensions:** 1024 × 3072 pixels (20.48 × 61.44 units)
- **1 screen wide** × **6 screens deep**
- Vertical shaft exploration
- Focus on digging deep
- Claustrophobic, challenging
- **Memory:** ~3.1 MB
- **Best for:** Vertical mining, tight spaces, focus on depth

### 3×3 - Balanced ⚖️
**Dimensions:** 3072 × 1536 pixels (61.44 × 30.72 units)
- **3 screens wide** × **3 screens deep**
- Balanced exploration & digging
- Good for general gameplay
- Recommended default
- **Memory:** ~4.7 MB
- **Best for:** General gameplay, balanced experience

### 6×6 - Huge 🌍
**Dimensions:** 6144 × 3072 pixels (122.88 × 61.44 units)
- **6 screens wide** × **6 screens deep**
- Massive open world
- Maximum exploration
- Performance intensive
- **Memory:** ~18.9 MB
- **Best for:** Open-world exploration, epic scale

---

## 🚀 How to Use

### Method 1: Context Menu (Instant)
1. Select `PixelWorldManager` in Hierarchy
2. Right-click component in Inspector
3. Choose one:
   - **⚡ World Size: 1×6 (Narrow & Deep)**
   - **⚡ World Size: 3×3 (Balanced)**
   - **⚡ World Size: 6×6 (Huge)**
4. World regenerates instantly!

### Method 2: Code
```csharp
// Load a preset
PixelWorldManager.Instance.LoadWorldSizePreset(WorldSizePreset.Balanced3x3);

// Or manually set dimensions
var manager = PixelWorldManager.Instance;
manager.width = 3072;
manager.height = 1536;
manager.InitializeWorld();
```

---

## 📊 Comparison Table

| Preset | Pixels (W×H) | Units (W×H) | Screens | Memory | Performance |
|--------|--------------|-------------|---------|--------|-------------|
| **1×6** | 1024 × 3072 | 20.5 × 61.4 | 1×6 | 3.1 MB | ⚡⚡⚡⚡⚡ Excellent |
| **3×3** | 3072 × 1536 | 61.4 × 30.7 | 3×3 | 4.7 MB | ⚡⚡⚡⚡ Great |
| **6×6** | 6144 × 3072 | 122.9 × 61.4 | 6×6 | 18.9 MB | ⚡⚡⚡ Good |

*Memory = RenderTexture size (RInt format, 4 bytes per pixel)*

---

## 🎮 Gameplay Impact

### 1×6 - Narrow & Deep
**Pros:**
- Deep underground exploration (6 screens down!)
- Challenging navigation
- Fast performance
- Focus on vertical gameplay

**Cons:**
- Limited horizontal space
- Can feel constrained
- Less room for exploration

**Use Cases:**
- Mining-focused levels
- Vertical platforming challenges
- Resource gathering runs
- Performance-constrained platforms

---

### 3×3 - Balanced
**Pros:**
- Good mix of width and depth
- Room to explore horizontally
- Deep enough for mining (3 screens)
- Balanced camera movement

**Cons:**
- Not as deep as 1×6
- Not as wide as 6×6
- Middle-of-the-road

**Use Cases:**
- General gameplay (recommended default)
- First levels / tutorials
- Balanced exploration & mining
- Good for most situations

---

### 6×6 - Huge
**Pros:**
- Massive world to explore
- Epic scale
- Lots of content space
- Players can get lost (in a good way)

**Cons:**
- Higher memory usage (18.9 MB)
- More GPU processing
- May be overwhelming
- Harder to navigate

**Use Cases:**
- End-game content
- Open-world exploration
- Epic boss arenas
- Showcase levels

---

## 🔧 Technical Details

### Screen Calculation
```
1 Screen = Camera Orthographic Size × 2 × Aspect Ratio
         = 5.12 × 2 × ~1.78 (16:9)
         = 10.24 × 18.2 units
         ≈ 1024 × 512 pixels (at 0.02 units/pixel)
```

### Pixel Dimensions Formula
```
Width (pixels)  = Screens Wide × 1024
Height (pixels) = Screens Deep × 512

Examples:
1×6 = 1×1024 × 6×512 = 1024 × 3072
3×3 = 3×1024 × 3×512 = 3072 × 1536
6×6 = 6×1024 × 6×512 = 6144 × 3072
```

### World Units Formula
```
Width (units)  = Pixels × 0.02
Height (units) = Pixels × 0.02

Examples:
1024 pixels = 1024 × 0.02 = 20.48 units
3072 pixels = 3072 × 0.02 = 61.44 units
```

---

## 🎯 Performance Considerations

### GPU Simulation Cost
Proportional to **total pixels simulated per frame**:

| Preset | Pixels/Frame | Relative Cost |
|--------|--------------|---------------|
| **1×6** | 3,145,728 | 1.0× (baseline) |
| **3×3** | 4,718,592 | 1.5× |
| **6×6** | 18,874,368 | 6.0× |

**Impact:**
- **1×6:** 60+ FPS on most hardware
- **3×3:** 60 FPS on mid-range hardware (current default)
- **6×6:** May drop to 40-50 FPS on older GPUs

### Memory Usage
Each RenderTexture (RInt format, 4 bytes/pixel):

| Preset | Memory per Texture | Total (Double-Buffered) |
|--------|-------------------|------------------------|
| **1×6** | 1.6 MB | 3.1 MB |
| **3×3** | 2.4 MB | 4.7 MB |
| **6×6** | 9.4 MB | 18.9 MB |

**Plus:**
- Collision data (CPU copy): Same as texture size
- **Total memory per preset:**
  - 1×6: ~6.2 MB
  - 3×3: ~9.4 MB
  - 6×6: ~37.8 MB

---

## 🐛 Troubleshooting

### Problem: World doesn't regenerate after selecting preset

**Solution:**
- Ensure you're in Play Mode OR
- Exit and re-enter Play Mode after selection

### Problem: Camera doesn't adjust to new bounds

**Solution:**
- Camera bounds auto-calculate on Start()
- Exit and re-enter Play Mode to recalculate

### Problem: Performance is slow on 6×6

**Expected:**
- 6×6 is 6× more expensive than 1×6
- Reduce quality settings or use smaller preset

**Optimizations:**
- Use Performance Mode visual preset (press 6)
- Reduce world update rate in PixelWorldManager
- Skip simulation frames (set updateRate > 0)

### Problem: Player falls through ground after resizing

**Cause:**
- Surface generation happens at fixed % of height
- Different heights = different surface Y position

**Solution:**
- Respawn player after world resize
- Or adjust player Y position manually

---

## 💡 Tips & Tricks

### Combining with Cave Generation Presets

You can combine world size + cave generation for variety:

```csharp
// Huge underwater world
manager.LoadWorldSizePreset(WorldSizePreset.Huge6x6);
manager.LoadPreset(WorldPreset.UnderwaterCaves);

// Narrow deep mining shaft
manager.LoadWorldSizePreset(WorldSizePreset.Narrow1x6);
manager.LoadPreset(WorldPreset.DenseSolid);

// Balanced cave exploration
manager.LoadWorldSizePreset(WorldSizePreset.Balanced3x3);
manager.LoadPreset(WorldPreset.CaveExplorer);
```

### Per-Level Configuration

Different levels can have different world sizes:

**Level 1 (Tutorial):** 1×6 - Teach vertical mining  
**Level 2 (Exploration):** 3×3 - Introduce horizontal exploration  
**Level 3 (Boss Arena):** 6×6 - Epic scale final battle

### Custom Sizes

Not limited to presets! Manually set any size:

```csharp
// 2×8 (narrow and very deep)
manager.width = 2048;
manager.height = 4096;
manager.InitializeWorld();

// 10×2 (very wide and shallow)
manager.width = 10240;
manager.height = 1024;
manager.InitializeWorld();
```

**Recommendations:**
- Keep dimensions multiples of 64 (for GPU performance)
- Keep aspect ratios reasonable (avoid 100×1)
- Test performance on target hardware

---

## 📚 Related Documentation

- **[MULTI_SCREEN_WORLD_SETUP.md](./MULTI_SCREEN_WORLD_SETUP.md)** - Detailed setup guide
- **[CAVE_GENERATION_GUIDE.md](./CAVE_GENERATION_GUIDE.md)** - Cave generation presets
- **[Docs/WORLD_SYSTEMS.md](./Docs/WORLD_SYSTEMS.md)** - Technical architecture

---

## 🎉 Quick Reference

**Narrow & Deep (1×6):**
```
Right-click PixelWorldManager → ⚡ World Size: 1×6 (Narrow & Deep)
```

**Balanced (3×3):**
```
Right-click PixelWorldManager → ⚡ World Size: 3×3 (Balanced)
```

**Huge (6×6):**
```
Right-click PixelWorldManager → ⚡ World Size: 6×6 (Huge)
```

---

**Choose your world size and start exploring!** 🌍✨

