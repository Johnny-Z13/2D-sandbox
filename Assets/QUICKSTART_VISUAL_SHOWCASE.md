# Quick Start - Visual Showcase

## 🚀 Get Amazing Graphics in 3 Steps

### Step 1: Ensure Your Scene is Set Up
Your scene should already have:
- ✅ PixelWorldManager component
- ✅ World Renderer with Mat_PixelWorld material
- ✅ Global Volume with DefaultVolumeProfile

If not, check the main scene file.

### Step 2: Add the Preset Controller (Optional)
1. Create an empty GameObject (name it "RenderingController")
2. Add component: `RenderingPresetController`
3. Assign:
   - World Renderer → Your pixel world renderer
   - Post Process Volume → Your Global Volume
4. (Optional) Add `VisualDebugger` component for on-screen info

### Step 3: Play and Cycle Presets
Press **Play** and use number keys **1-6** to see different visual styles!

---

## 🎨 Creating a Stunning Demo

### Best Showcase Workflow

1. **Paint a large sand formation:**
   - Hold Left Mouse Button
   - Create dunes, mountains, or flowing patterns
   - The more sand, the more sparkles!

2. **Add water pools:**
   - Hold Right Mouse Button
   - Create lakes or waterfalls
   - Watch the caustics shimmer

3. **Press 4 for Extreme Showcase:**
   - Maximum glitter
   - Intense bloom
   - Perfect for demos

4. **Take screenshots:**
   - Press 5 for Screenshot Mode
   - Disable UI (F1 to toggle info)
   - Use Game view at max resolution
   - Zoom in on sand details

---

## 🎯 Preset Guide

### When to Use Each Preset

**1 - Default**
- Balanced, pleasant
- Good for gameplay
- 60+ FPS

**2 - Desert Gold**
- Warm, sparkly
- Great for sand-heavy scenes
- Desert/beach vibes

**3 - Subtle Realism**
- Understated effects
- Professional look
- Best for serious projects

**4 - Extreme Showcase**
- Maximum visual impact
- Demo mode
- May drop to 50 FPS

**5 - Screenshot Mode**
- Balanced for captures
- Slightly enhanced
- Great for marketing

**6 - Performance Mode**
- Optimized for speed
- Low-end hardware
- 90+ FPS

---

## 💡 Pro Tips

### Maximizing Visual Impact

**Composition:**
- Large sand surfaces show glitter best
- Contrast sand against dark backgrounds
- Create flowing forms (dunes, slopes)
- Add water nearby for variety

**Lighting:**
- The shader generates its own highlights
- Dark "empty" space makes sand pop
- Create caverns with sand waterfalls

**Camera Angles:**
- Zoom in to see individual sparkles
- Mid-range shows overall shimmer
- Moving camera enhances animation

**Performance:**
- Start with Default preset
- Only use Extreme for screenshots
- If FPS drops, try Performance Mode
- Lower world resolution helps too

---

## 🎬 Recording Video

### Settings for Best Results

1. **Use Screenshot Mode preset (5)**
2. **Set Time.timeScale = 0.5f** for slow-motion:
   ```csharp
   Time.timeScale = 0.5f;
   ```
3. **Increase render resolution** in Game view
4. **Use screen recorder** (OBS, ShadowPlay, etc.)
5. **Capture sand falling** - looks amazing in slow-mo

### Suggested Scenes to Record

- Sand pouring into a cavern
- Water mixing with sand
- Digging through sand layers
- Sand avalanche/collapse
- Player walking on sand surface

---

## 🔧 Fine-Tuning

### If You Want to Customize

**Open Material** (`Mat_PixelWorld`):
- Adjust Glitter Intensity (0-5)
- Change Glitter Scale (size of sparkles)
- Modify Glitter Speed (animation rate)

**Adjust Post-Processing:**
1. Select Global Volume
2. Find Bloom override
3. Tweak Threshold and Intensity
4. Adjust Color Adjustments

**Custom Preset:**
1. In RenderingPresetController
2. Enable "Use Manual Settings"
3. Adjust sliders to taste
4. Save as your own preset

---

## 🐛 Common Issues

**Problem: Sand looks flat**
- Solution: Make sure game is playing (not paused)
- Solution: Check Glitter Intensity > 0
- Solution: Paint more sand (need large areas)

**Problem: No bloom glow**
- Solution: Verify HDR enabled on camera
- Solution: Check Global Volume is active
- Solution: Increase Bloom Intensity

**Problem: Too sparkly/distracting**
- Solution: Use Subtle Realism preset (3)
- Solution: Lower Glitter Intensity to 1.0
- Solution: Increase Glitter Scale to 35

**Problem: Performance issues**
- Solution: Press 6 for Performance Mode
- Solution: Reduce world resolution (2048x1024)
- Solution: Lower Bloom Max Iterations to 4

---

## 📸 Screenshot Checklist

Before taking your best shot:

- [ ] Switch to Screenshot Mode preset (5)
- [ ] Hide UI (press F1)
- [ ] Set Game view to highest resolution
- [ ] Frame your shot (interesting composition)
- [ ] Ensure sand is visible and prominent
- [ ] Consider adding water for contrast
- [ ] Take multiple angles
- [ ] Use Unity's screenshot tool or F12 (if set up)

---

## 🎉 Show Off Your Results!

You now have a graphically stunning pixel sandbox!

**Share your creations:**
- Post screenshots on social media
- Create time-lapse videos
- Compare before/after with friends
- Experiment with different scenes

**Next Steps:**
- Read full documentation: `RENDERING_SHOWCASE.md`
- Check upgrade summary: `RENDERING_UPGRADE_SUMMARY.md`
- Explore shader code: `PixelWorldRender.shader`

---

**Have fun creating beautiful pixel worlds! ✨🏖️**









