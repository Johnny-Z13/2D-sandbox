PLACEHOLDER AUDIO FILES
=======================

This folder contains placeholder WAV files for the audio system.
These are temporary files and should be replaced with final audio assets.

File List:
----------
1. level_ambience.wav - Looping ambient background music for levels
2. player_jump.wav - Player jump sound
3. player_land.wav - Player landing sound
4. player_footstep.wav - Player footstep (walking/running)
5. player_dig.wav - Digging through terrain
6. paint_sand.wav - Painting sand with mouse
7. paint_water.wav - Painting water with mouse
8. erase.wav - Erasing terrain
9. sand_fall.wav - Sand falling/flowing sound
10. bomb_place.wav - Placing a bomb
11. bomb_explosion.wav - Bomb exploding
12. player_hit.wav - Player taking damage
13. ui_click.wav - Generic UI click
14. hotbar_switch.wav - Switching hotbar items
15. preset_change.wav - Changing graphics preset

Instructions for Replacement:
------------------------------
1. Keep the same filenames when replacing
2. Use WAV format (16-bit, 44.1kHz recommended)
3. Level ambience should be seamlessly loopable
4. Keep SFX files short (< 2 seconds)
5. Normalize volume levels across all files
6. After replacing, reimport in Unity to update

Unity Import Settings:
---------------------
- Level Ambience: 
  * Load Type: Streaming
  * Compression Format: Vorbis
  * Quality: 70-100%
  
- SFX Files:
  * Load Type: Decompress On Load
  * Compression Format: PCM or ADPCM
  * Preload Audio Data: Yes

Notes:
------
- All current files are silent 1-second WAVs as placeholders
- AudioManager.cs will automatically use these when assigned
- No code changes needed when replacing files

