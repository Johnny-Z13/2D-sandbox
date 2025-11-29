# Audio System Documentation

## Overview

The 2D Pixel Sandbox includes a comprehensive audio system with sound effects for all major game events and ambient background music.

---

## Architecture

### Components

1. **AudioManager.cs** - Central audio management system
   - Manages audio sources and pooling
   - Handles volume controls
   - Plays ambient music loop
   - Provides public API for sound effects

2. **AudioEventTriggers.cs** - Static event triggers
   - Simplified API for triggering sounds
   - Called from gameplay scripts
   - Null-safe with automatic fallbacks

3. **PixelWorldAudioIntegration.cs** - Integration with world systems
   - Connects audio to existing gameplay events
   - Already integrated with player, bombs, and UI

---

## Audio Files

### Location
All audio files should be placed in: `Assets/Audio/Placeholders/`

### Required Files (15 total)

#### Ambient Music
- **level_ambience.wav** - Looping background music for levels
  - Format: WAV, 44.1kHz, Stereo
  - Import Settings: Streaming, Vorbis compression
  - Should loop seamlessly

#### Player Sounds (4)
- **player_jump.wav** - Jump sound effect
- **player_land.wav** - Landing after fall
- **player_footstep.wav** - Walking/running footsteps
- **player_dig.wav** - Digging through terrain

#### World Interaction (4)
- **paint_sand.wav** - Painting sand with mouse
- **paint_water.wav** - Painting water with mouse
- **erase.wav** - Erasing terrain
- **sand_fall.wav** - Sand falling/flowing (optional)

#### Combat (3)
- **bomb_place.wav** - Placing a bomb
- **bomb_explosion.wav** - Bomb exploding
- **player_hit.wav** - Player taking damage (future use)

#### UI Sounds (3)
- **ui_click.wav** - Generic UI click
- **hotbar_switch.wav** - Switching hotbar items
- **preset_change.wav** - Changing graphics preset

---

## Generating Placeholder Files

### Using Unity Editor Tool

1. Open Unity Editor
2. Go to menu: **Tools > Audio > Generate Placeholder WAVs**
3. This creates silent 1-second WAV files for all required sounds
4. Replace these with your actual audio assets

### Manual Creation

If you prefer to create files manually:
- Format: WAV, 16-bit, 44.1kHz
- Keep SFX files short (< 2 seconds)
- Ambience should be seamlessly loopable
- Normalize volume levels across all files

---

## Setup Instructions

### 1. Create AudioManager GameObject

```csharp
// In your main scene (Prototype-01.unity):
1. Create empty GameObject: "AudioManager"
2. Add component: AudioManager.cs
3. Configure in Inspector:
   - Assign all audio clips (from Assets/Audio/Placeholders/)
   - Set volume levels
   - Adjust settings as needed
```

### 2. Audio Import Settings

#### For Ambient Music (level_ambience.wav):
```
Load Type: Streaming
Compression Format: Vorbis
Quality: 70-100%
Preload Audio Data: No
```

#### For SFX Files:
```
Load Type: Decompress On Load
Compression Format: PCM or ADPCM
Preload Audio Data: Yes
Force To Mono: Yes (saves memory)
```

### 3. Volume Configuration

Default volumes (adjustable in Inspector):
- **Master Volume:** 1.0 (100%)
- **SFX Volume:** 0.8 (80%)
- **Ambience Volume:** 0.6 (60%)

---

## Usage in Code

### Triggering Sound Effects

Use the static `AudioEventTriggers` class:

```csharp
// Player actions
AudioEventTriggers.OnPlayerJump();
AudioEventTriggers.OnPlayerLand();
AudioEventTriggers.OnDig();

// World interaction
AudioEventTriggers.OnPaintSand();
AudioEventTriggers.OnPaintWater();
AudioEventTriggers.OnErase();

// Combat
AudioEventTriggers.OnBombPlaced();
AudioEventTriggers.OnBombExplosion();

// UI
AudioEventTriggers.OnUIClick();
AudioEventTriggers.OnHotbarSwitch();
AudioEventTriggers.OnPresetChange();
```

### Direct AudioManager Access

For advanced control:

```csharp
if (AudioManager.Instance != null)
{
    // Volume controls
    AudioManager.Instance.SetMasterVolume(0.8f);
    AudioManager.Instance.SetSFXVolume(0.7f);
    AudioManager.Instance.SetAmbienceVolume(0.5f);
    
    // Ambient music control
    AudioManager.Instance.PlayAmbience();
    AudioManager.Instance.StopAmbience();
}
```

---

## Current Integrations

### Already Integrated
✅ **Player Movement** - Jump, land, footsteps  
✅ **Player Actions** - Dig, crouch  
✅ **Bombs** - Place and explosion sounds  
✅ **Hotbar** - Item switching  
✅ **Graphics Presets** - Preset change notification  

### Ready for Integration
⚪ **Mouse Painting** - Sand, water, erase (hooks ready)  
⚪ **Sand Physics** - Falling sand sound (optional)  
⚪ **Player Damage** - Hit sound (future health system)  

---

## Audio Pooling System

The AudioManager uses object pooling for efficient SFX playback:

- **Pool Size:** 10 audio sources (configurable)
- **Allocation:** Round-robin assignment
- **Advantage:** No runtime allocation, no GC pressure
- **Overlap:** Can play up to 10 sounds simultaneously

---

## Performance Considerations

### Memory Usage
- **Ambience (streaming):** ~1-2 MB in memory
- **SFX (decompressed):** ~2-3 MB total (15 clips)
- **Audio Sources:** Minimal overhead
- **Total:** ~5 MB memory footprint

### CPU Usage
- Audio processing handled by Unity's native audio engine
- Minimal CPU overhead from AudioManager
- No GC allocations during gameplay

---

## Debugging & Testing

### Inspector Tools

**AudioManager Context Menus:**
- **Test All Sounds** - Plays each sound effect in sequence
- **Log Audio Status** - Displays current configuration and missing clips

### Console Commands

The AudioManager logs helpful information:
```
AudioManager: Created SFX pool with 10 sources
AudioManager: Playing level ambience
AudioManager: Missing clips: 0/15
```

### Troubleshooting

**No sound playing?**
1. Check Master Volume in AudioManager Inspector
2. Verify audio clips are assigned
3. Check if AudioManager.Instance exists
4. Look for console warnings

**Sound cuts off?**
- Increase `maxSFXSources` in AudioManager (default: 10)

**Ambience not looping?**
- Ensure "Loop" is enabled on ambienceSource
- Check that level_ambience.wav loops seamlessly

---

## Adding New Sounds

### 1. Add AudioClip Field

In `AudioManager.cs`:
```csharp
[Header("New Category")]
[SerializeField] private AudioClip newSound;
```

### 2. Create Playback Method

```csharp
public void PlayNewSound()
{
    PlaySFX(newSound, 0.7f, Random.Range(0.95f, 1.05f));
}
```

### 3. Add Event Trigger

In `AudioEventTriggers.cs`:
```csharp
public static void OnNewEvent()
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayNewSound();
    }
}
```

### 4. Call from Gameplay

```csharp
AudioEventTriggers.OnNewEvent();
```

---

## Audio Design Guidelines

### Volume Levels
- **Ambient Music:** 40-60% (background, not distracting)
- **Player Actions:** 60-80% (clear feedback)
- **World Events:** 50-70% (informative, not overpowering)
- **Explosions:** 80-100% (high impact)
- **UI Sounds:** 50-60% (subtle confirmation)

### Pitch Variation
Most sounds use slight random pitch variation:
```csharp
Random.Range(0.95f, 1.05f)  // ±5% variation
```

This prevents repetitive sounds from feeling artificial.

### Sound Design Tips
- **Footsteps:** Use interval timing (default: 0.4s) to prevent spam
- **Digging:** Short, punchy sound
- **Explosions:** Layered sounds (impact + debris + rumble)
- **Water:** Soft, flowing sound
- **Sand:** Grainy, subtle texture

---

## Future Enhancements

### Planned Features
- ⚪ **Audio Zones** - Different ambience per area
- ⚪ **Dynamic Music** - Intensity changes based on gameplay
- ⚪ **3D Spatial Audio** - Directional sound for bombs/explosions
- ⚪ **Audio Mixer** - Professional mixing with Unity Audio Mixer
- ⚪ **Sound Variations** - Multiple clips per event for variety

### Optional Additions
- Underwater filter effect when submerged
- Reverb in caves
- Material-specific footstep sounds (rock vs. sand)
- Particle system audio (water splash, sand pour)

---

## Best Practices

1. **Always null-check AudioManager.Instance**
2. **Use AudioEventTriggers for cleaner code**
3. **Keep SFX files small** (< 2 seconds, < 100KB each)
4. **Test all audio levels together** (no single sound dominates)
5. **Provide volume controls** (master, sfx, music)
6. **Optimize import settings** (compression for music, uncompressed for SFX)

---

## Related Files

### Scripts
- `Assets/Scripts/Audio/AudioManager.cs`
- `Assets/Scripts/Audio/AudioEventTriggers.cs`
- `Assets/Scripts/Audio/PixelWorldAudioIntegration.cs`
- `Assets/Scripts/Audio/GeneratePlaceholderAudio.cs` (Editor tool)

### Audio Assets
- `Assets/Audio/Placeholders/*.wav`
- `Assets/Audio/Placeholders/README.txt`

### Documentation
- `Docs/AUDIO_SYSTEM.md` (this file)
- `README.md` - Updated with audio system info

---

## Summary

The audio system is **production-ready** with:
- ✅ Comprehensive sound effect coverage
- ✅ Efficient pooling system
- ✅ Easy-to-use API
- ✅ Professional architecture
- ✅ Full integration with gameplay
- ✅ Editor tools for setup

Simply add your audio files and assign them in the AudioManager Inspector to bring the game to life!

---

**Last Updated:** November 29, 2025  
**Audio System Version:** 1.0  
**Status:** Ready for audio asset integration
