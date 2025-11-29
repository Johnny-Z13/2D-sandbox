# 🎵 Audio System Setup Guide

## Quick Setup (5 Minutes)

### Step 1: Generate Placeholder Audio Files
1. Open Unity Editor
2. Go to menu: **Tools > Audio > Generate Placeholder WAVs**
3. Click "OK" when the dialog appears
4. Files created in `Assets/Audio/Placeholders/`

### Step 2: Create AudioManager GameObject
1. In Hierarchy, create new Empty GameObject
2. Name it: **"AudioManager"**
3. Add component: **AudioManager** (Scripts/Audio/)
4. **Important:** Drag AudioManager to top of Hierarchy (loads first)

### Step 3: Assign Audio Clips
In AudioManager Inspector, drag and drop:

**Music & Ambience:**
- Level Ambience → `level_ambience.wav`

**Player SFX:**
- Jump Sound → `player_jump.wav`
- Land Sound → `player_land.wav`
- Footstep Sound → `player_footstep.wav`
- Dig Sound → `player_dig.wav`

**World Interaction SFX:**
- Sand Paint Sound → `paint_sand.wav`
- Water Paint Sound → `paint_water.wav`
- Erase Sound → `erase.wav`
- Sand Fall Sound → `sand_fall.wav` (optional)

**Combat SFX:**
- Bomb Place Sound → `bomb_place.wav`
- Bomb Explosion Sound → `bomb_explosion.wav`
- Player Hit Sound → `player_hit.wav`

**UI SFX:**
- UI Click Sound → `ui_click.wav`
- Hotbar Switch Sound → `hotbar_switch.wav`
- Preset Change Sound → `preset_change.wav`

### Step 4: Test
1. Press Play ▶️
2. Jump (Space) - Should hear jump sound
3. Walk (WASD) - Should hear footsteps
4. Place bomb (E with bomb selected) - Should hear place + explosion

---

## Replacing Placeholder Audio

### Option 1: Replace Files Directly
1. Export your audio as WAV files (16-bit, 44.1kHz)
2. Use **same filenames** as placeholders
3. Copy/paste into `Assets/Audio/Placeholders/`
4. Unity auto-reimports
5. Done! No reassignment needed

### Option 2: Use Custom Organization
1. Create new folder: `Assets/Audio/Final/`
2. Import your audio files
3. Configure import settings:
   - **Ambience:** Load Type = Streaming, Compression = Vorbis
   - **SFX:** Load Type = Decompress On Load, Force Mono = Yes
4. Reassign clips in AudioManager Inspector

---

## Audio Import Settings

### For Background Music (level_ambience.wav)
```
Load Type: Streaming
Compression Format: Vorbis
Quality: 70-100%
Preload Audio Data: No
Load In Background: Yes
```

### For Sound Effects (all other files)
```
Load Type: Decompress On Load
Compression Format: PCM (best quality) or ADPCM (smaller)
Preload Audio Data: Yes
Force To Mono: Yes (saves 50% memory)
```

---

## Recommended Audio Specifications

### Level Ambience
- **Format:** WAV or OGG
- **Length:** 1-3 minutes (loops seamlessly)
- **Sample Rate:** 44.1kHz
- **Bit Depth:** 16-bit
- **Channels:** Stereo
- **File Size:** 5-15 MB before compression

### Sound Effects
- **Format:** WAV
- **Length:** 0.1-2 seconds
- **Sample Rate:** 44.1kHz
- **Bit Depth:** 16-bit
- **Channels:** Mono (Unity converts)
- **File Size:** 10-100 KB each

---

## Volume Recommendations

Suggested volume levels in AudioManager Inspector:

```
Master Volume: 1.0 (100%)
SFX Volume: 0.8 (80%)
Ambience Volume: 0.6 (60%)

Individual Sound Volumes (adjust as needed):
- Jump: 70%
- Land: 80%
- Footsteps: 40%
- Dig: 60%
- Explosions: 100%
- UI Sounds: 50-60%
```

---

## Troubleshooting

### No sound playing?
1. Check AudioManager exists in scene
2. Verify clips are assigned in Inspector
3. Check Master Volume > 0
4. Look for console errors

### Ambience not looping?
1. Select ambience audio clip
2. In Inspector, check "Loop" is enabled
3. Ensure audio starts/ends at same volume (for seamless loop)

### Footsteps too fast/slow?
Adjust `Footstep Interval` in AudioManager Inspector:
- **Default:** 0.4 seconds
- **Slower:** 0.5-0.6 seconds
- **Faster:** 0.2-0.3 seconds

### Too many sounds playing?
Increase `Max SFX Sources` in AudioManager Inspector:
- **Default:** 10 sources
- **More sounds:** 15-20 sources
- **Note:** Higher = more memory usage

---

## Testing Checklist

After setup, test these scenarios:

- [ ] Ambient music plays on game start
- [ ] Jump makes sound
- [ ] Landing after fall makes sound
- [ ] Walking plays footsteps
- [ ] Digging makes sound
- [ ] Bomb placement makes sound
- [ ] Bomb explosion makes sound
- [ ] Hotbar switching makes sound (keys 1-4)
- [ ] Graphics preset change makes sound (F1-F6)
- [ ] No console errors related to audio
- [ ] Volume sliders work correctly

---

## Pro Tips

1. **Loop Points:** Use audio editing software (Audacity/Adobe Audition) to create perfect loops
2. **Normalization:** Normalize all SFX to same peak level (-3dB recommended)
3. **Fade In/Out:** Add short fades to prevent clicks
4. **Variations:** Create 2-3 variations per sound for variety
5. **Test Together:** Play game with all sounds active to check balance

---

## Advanced: Audio Mixer Setup (Optional)

For professional audio mixing:

1. Create Audio Mixer asset
2. Add groups: Master, Music, SFX, UI
3. Assign AudioManager sources to groups
4. Add effects (EQ, compression, reverb)
5. Expose parameters for runtime control

See Unity documentation for Audio Mixer details.

---

## File Checklist

Required files in `Assets/Audio/Placeholders/`:

- [ ] level_ambience.wav
- [ ] player_jump.wav
- [ ] player_land.wav
- [ ] player_footstep.wav
- [ ] player_dig.wav
- [ ] paint_sand.wav
- [ ] paint_water.wav
- [ ] erase.wav
- [ ] sand_fall.wav
- [ ] bomb_place.wav
- [ ] bomb_explosion.wav
- [ ] player_hit.wav
- [ ] ui_click.wav
- [ ] hotbar_switch.wav
- [ ] preset_change.wav

**Total: 15 files**

---

## Need Help?

1. Check `Docs/AUDIO_SYSTEM.md` for technical details
2. Use **Tools > Audio > Open Placeholder Folder** to quickly access files
3. Use **AudioManager Context Menu > Test All Sounds** to verify setup
4. Use **AudioManager Context Menu > Log Audio Status** to check configuration

---

**Your audio system is ready to go! Just add your sound files and enjoy! 🎵**

