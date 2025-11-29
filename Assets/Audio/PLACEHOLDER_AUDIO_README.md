# Audio Files - Placeholder Guide

## 📁 Folder Structure

```
Assets/Audio/
├── Music/
│   └── ambient_loop.wav           (Looping background ambience)
│
├── SFX/
│   ├── Player/
│   │   ├── jump.wav               (Player jumps)
│   │   ├── land.wav               (Player lands on ground)
│   │   ├── dig.wav                (Digging action)
│   │   ├── walk.wav               (Footstep sound)
│   │   └── crouch.wav             (Crouch/stand sound)
│   │
│   ├── World/
│   │   ├── paint_sand.wav         (Painting sand material)
│   │   ├── paint_water.wav        (Painting water material)
│   │   ├── erase.wav              (Erasing terrain)
│   │   ├── bomb_place.wav         (Placing bomb)
│   │   ├── bomb_explode.wav       (Bomb explosion)
│   │   ├── sand_fall.wav          (Sand falling/shifting)
│   │   ├── water_splash.wav       (Water splash/flow)
│   │   ├── rock_break.wav         (Breaking rock)
│   │   └── dirt_dig.wav           (Digging dirt)
│   │
│   └── UI/
│       ├── ui_click.wav           (Button click)
│       ├── hotbar_switch.wav      (Switching hotbar items)
│       └── preset_change.wav      (Changing visual presets)
```

## 🔊 Audio Specifications

### Music
- **Format:** WAV (uncompressed)
- **Sample Rate:** 44.1kHz or 48kHz
- **Bit Depth:** 16-bit or 24-bit
- **Channels:** Stereo
- **Loop:** YES (set in Unity Inspector)
- **Recommended Length:** 2-5 minutes for seamless looping

### Sound Effects
- **Format:** WAV (uncompressed)
- **Sample Rate:** 44.1kHz
- **Bit Depth:** 16-bit
- **Channels:** Mono or Stereo
- **Length:** Short (0.1s - 2s typically)

## 📋 Placeholder Audio TODO List

### Music (1 file)
- [ ] `ambient_loop.wav` - Cave/underground atmosphere

### Player Sounds (5 files)
- [ ] `jump.wav` - Jumping sound
- [ ] `land.wav` - Landing thud
- [ ] `dig.wav` - Digging/mining
- [ ] `walk.wav` - Footstep
- [ ] `crouch.wav` - Crouch toggle

### World Interaction (9 files)
- [ ] `paint_sand.wav` - Sand placement
- [ ] `paint_water.wav` - Water pour
- [ ] `erase.wav` - Terrain removal
- [ ] `bomb_place.wav` - Bomb placement
- [ ] `bomb_explode.wav` - Explosion
- [ ] `sand_fall.wav` - Sand physics
- [ ] `water_splash.wav` - Water splash
- [ ] `rock_break.wav` - Rock destruction
- [ ] `dirt_dig.wav` - Dirt digging

### UI Sounds (3 files)
- [ ] `ui_click.wav` - UI interaction
- [ ] `hotbar_switch.wav` - Item selection
- [ ] `preset_change.wav` - Preset switch

## 🎨 Unity Import Settings

### For Music Files:
1. Select audio file in Project window
2. Inspector settings:
   - **Load Type:** Streaming
   - **Preload Audio Data:** OFF
   - **Compression Format:** Vorbis (quality: 100%)
   - **Sample Rate Setting:** Preserve Sample Rate

### For Sound Effects:
1. Select audio file in Project window
2. Inspector settings:
   - **Load Type:** Decompress On Load (for short SFX)
   - **Preload Audio Data:** ON
   - **Compression Format:** ADPCM or Vorbis (quality: 70-100%)
   - **Sample Rate Setting:** Optimize Sample Rate

## 🔗 Integration

All audio files are automatically linked in the **AudioManager** component.

### Setup Instructions:
1. Create `AudioManager` GameObject in scene
2. Add `AudioManager.cs` component
3. Drag audio clips from Project window to corresponding fields in Inspector
4. Adjust volume levels as needed

## 🎵 Placeholder Resources

### Free Audio Resources (for placeholders):
- **Freesound.org** - Community sound library
- **OpenGameArt.org** - Game audio assets
- **ZapSplat** - Free SFX library
- **BBC Sound Effects** - Public domain sounds

### Quick Placeholder Generation:
Use tools like **Audacity**, **BFXR**, or **ChipTone** to generate simple placeholder sounds quickly.

## 📝 Notes for Audio Replacement

When replacing placeholder audio:
1. Match the naming convention exactly
2. Keep similar duration/characteristics
3. Test in-game to ensure proper volume levels
4. Update Unity Inspector references if needed
5. Consider fade-in/fade-out for music loops

## ⚙️ AudioManager Features

- **Automatic pooling** - 10 audio sources for overlapping sounds
- **Cooldown system** - Prevents sound spam
- **Volume control** - Separate SFX and Music volume
- **Pitch variation** - Randomized pitch for variety
- **Singleton pattern** - Easy access from any script

## 🚀 Quick Start

1. Place all WAV files in appropriate folders
2. Select `AudioManager` GameObject
3. Drag clips to Inspector fields
4. Press Play - audio should work automatically!

---

**Note:** All placeholder audio will be replaced with final assets later in development.

