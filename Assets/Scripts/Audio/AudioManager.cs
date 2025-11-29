using UnityEngine;
using System.Collections.Generic;

namespace PixelWorld
{
    /// <summary>
    /// Centralized audio management system for the pixel sandbox.
    /// Handles sound effects, ambient music, and audio pooling.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource sfxSourcePrefab;
        
        [Header("Music & Ambience")]
        [SerializeField] private AudioClip levelAmbience;
        [SerializeField] private float ambienceVolume = 0.6f;
        
        [Header("Player SFX")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip landSound;
        [SerializeField] private AudioClip footstepSound;
        [SerializeField] private AudioClip digSound;
        
        [Header("World Interaction SFX")]
        [SerializeField] private AudioClip sandPaintSound;
        [SerializeField] private AudioClip waterPaintSound;
        [SerializeField] private AudioClip eraseSound;
        [SerializeField] private AudioClip sandFallSound;
        
        [Header("Combat SFX")]
        [SerializeField] private AudioClip bombPlaceSound;
        [SerializeField] private AudioClip bombExplosionSound;
        [SerializeField] private AudioClip playerHitSound;
        
        [Header("UI SFX")]
        [SerializeField] private AudioClip uiClickSound;
        [SerializeField] private AudioClip hotbarSwitchSound;
        [SerializeField] private AudioClip presetChangeSound;
        
        [Header("Settings")]
        [SerializeField] private float masterVolume = 1.0f;
        [SerializeField] private float sfxVolume = 0.8f;
        [SerializeField] private int maxSFXSources = 10;
        [SerializeField] private bool enableFootsteps = true;
        [SerializeField] private float footstepInterval = 0.4f;
        
        // Audio source pooling
        private List<AudioSource> _sfxPool = new List<AudioSource>();
        private int _currentPoolIndex = 0;
        
        // Footstep timing
        private float _lastFootstepTime;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAudioSources();
            CreateSFXPool();
        }

        private void Start()
        {
            PlayAmbience();
        }

        /// <summary>
        /// Initialize music and ambience audio sources if not assigned
        /// </summary>
        private void InitializeAudioSources()
        {
            // Create music source if needed
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = 0.5f;
            }
            
            // Create ambience source if needed
            if (ambienceSource == null)
            {
                GameObject ambienceObj = new GameObject("AmbienceSource");
                ambienceObj.transform.SetParent(transform);
                ambienceSource = ambienceObj.AddComponent<AudioSource>();
                ambienceSource.loop = true;
                ambienceSource.playOnAwake = false;
                ambienceSource.volume = ambienceVolume;
            }
        }

        /// <summary>
        /// Create a pool of audio sources for sound effects
        /// </summary>
        private void CreateSFXPool()
        {
            for (int i = 0; i < maxSFXSources; i++)
            {
                GameObject sfxObj = new GameObject($"SFX_Source_{i}");
                sfxObj.transform.SetParent(transform);
                AudioSource source = sfxObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume * masterVolume;
                _sfxPool.Add(source);
            }
            
            Debug.Log($"AudioManager: Created SFX pool with {maxSFXSources} sources");
        }

        /// <summary>
        /// Play ambient music loop
        /// </summary>
        public void PlayAmbience()
        {
            if (levelAmbience != null && ambienceSource != null)
            {
                ambienceSource.clip = levelAmbience;
                ambienceSource.volume = ambienceVolume * masterVolume;
                ambienceSource.Play();
                Debug.Log("AudioManager: Playing level ambience");
            }
            else
            {
                Debug.LogWarning("AudioManager: Level ambience clip not assigned");
            }
        }

        /// <summary>
        /// Stop ambient music
        /// </summary>
        public void StopAmbience()
        {
            if (ambienceSource != null)
            {
                ambienceSource.Stop();
            }
        }

        /// <summary>
        /// Play a sound effect using the audio pool
        /// </summary>
        private void PlaySFX(AudioClip clip, float volumeScale = 1.0f, float pitch = 1.0f)
        {
            if (clip == null) return;
            
            // Get next available source from pool
            AudioSource source = _sfxPool[_currentPoolIndex];
            _currentPoolIndex = (_currentPoolIndex + 1) % _sfxPool.Count;
            
            // Configure and play
            source.clip = clip;
            source.volume = sfxVolume * masterVolume * volumeScale;
            source.pitch = pitch;
            source.Play();
        }

        // ============================================
        // PUBLIC API - PLAYER SOUNDS
        // ============================================

        public void PlayJump()
        {
            PlaySFX(jumpSound, 0.7f, Random.Range(0.95f, 1.05f));
        }

        public void PlayLand()
        {
            PlaySFX(landSound, 0.8f, Random.Range(0.9f, 1.1f));
        }

        public void PlayFootstep()
        {
            if (!enableFootsteps) return;
            
            // Throttle footsteps
            if (Time.time - _lastFootstepTime < footstepInterval) return;
            
            PlaySFX(footstepSound, 0.4f, Random.Range(0.95f, 1.05f));
            _lastFootstepTime = Time.time;
        }

        public void PlayDig()
        {
            PlaySFX(digSound, 0.6f, Random.Range(0.9f, 1.1f));
        }

        // ============================================
        // PUBLIC API - WORLD INTERACTION SOUNDS
        // ============================================

        public void PlayPaintSand()
        {
            PlaySFX(sandPaintSound, 0.5f, Random.Range(0.95f, 1.05f));
        }

        public void PlayPaintWater()
        {
            PlaySFX(waterPaintSound, 0.5f, Random.Range(0.95f, 1.05f));
        }

        public void PlayErase()
        {
            PlaySFX(eraseSound, 0.5f, Random.Range(0.9f, 1.1f));
        }

        public void PlaySandFall()
        {
            PlaySFX(sandFallSound, 0.3f, Random.Range(0.95f, 1.05f));
        }

        // ============================================
        // PUBLIC API - COMBAT SOUNDS
        // ============================================

        public void PlayBombPlace()
        {
            PlaySFX(bombPlaceSound, 0.7f);
        }

        public void PlayBombExplosion()
        {
            PlaySFX(bombExplosionSound, 1.0f, Random.Range(0.9f, 1.1f));
        }

        public void PlayPlayerHit()
        {
            PlaySFX(playerHitSound, 0.8f, Random.Range(0.95f, 1.05f));
        }

        // ============================================
        // PUBLIC API - UI SOUNDS
        // ============================================

        public void PlayUIClick()
        {
            PlaySFX(uiClickSound, 0.6f);
        }

        public void PlayHotbarSwitch()
        {
            PlaySFX(hotbarSwitchSound, 0.5f);
        }

        public void PlayPresetChange()
        {
            PlaySFX(presetChangeSound, 0.6f);
        }

        // ============================================
        // VOLUME CONTROLS
        // ============================================

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetAmbienceVolume(float volume)
        {
            ambienceVolume = Mathf.Clamp01(volume);
            if (ambienceSource != null)
            {
                ambienceSource.volume = ambienceVolume * masterVolume;
            }
        }

        private void UpdateVolumes()
        {
            // Update ambience
            if (ambienceSource != null)
            {
                ambienceSource.volume = ambienceVolume * masterVolume;
            }
            
            // Update SFX pool
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    source.volume = sfxVolume * masterVolume;
                }
            }
        }

        // ============================================
        // DEBUG & TESTING
        // ============================================

        [ContextMenu("Test All Sounds")]
        private void TestAllSounds()
        {
            Debug.Log("Testing all audio clips...");
            
            // Test each sound with a delay
            StartCoroutine(TestSoundsSequence());
        }

        private System.Collections.IEnumerator TestSoundsSequence()
        {
            AudioClip[] allClips = {
                jumpSound, landSound, footstepSound, digSound,
                sandPaintSound, waterPaintSound, eraseSound, sandFallSound,
                bombPlaceSound, bombExplosionSound, playerHitSound,
                uiClickSound, hotbarSwitchSound, presetChangeSound
            };

            string[] clipNames = {
                "Jump", "Land", "Footstep", "Dig",
                "Paint Sand", "Paint Water", "Erase", "Sand Fall",
                "Bomb Place", "Bomb Explosion", "Player Hit",
                "UI Click", "Hotbar Switch", "Preset Change"
            };

            for (int i = 0; i < allClips.Length; i++)
            {
                if (allClips[i] != null)
                {
                    Debug.Log($"Playing: {clipNames[i]}");
                    PlaySFX(allClips[i]);
                    yield return new UnityEngine.WaitForSeconds(0.5f);
                }
                else
                {
                    Debug.LogWarning($"Missing: {clipNames[i]}");
                }
            }
        }

        [ContextMenu("Log Audio Status")]
        private void LogAudioStatus()
        {
            Debug.Log("=== AUDIO MANAGER STATUS ===");
            Debug.Log($"Master Volume: {masterVolume}");
            Debug.Log($"SFX Volume: {sfxVolume}");
            Debug.Log($"Ambience Volume: {ambienceVolume}");
            Debug.Log($"Active SFX Sources: {_sfxPool.Count}");
            
            int playingCount = 0;
            foreach (var source in _sfxPool)
            {
                if (source.isPlaying) playingCount++;
            }
            Debug.Log($"Currently Playing: {playingCount}/{_sfxPool.Count}");
            
            // Check missing clips
            int missingClips = 0;
            if (levelAmbience == null) { Debug.LogWarning("Missing: Level Ambience"); missingClips++; }
            if (jumpSound == null) { Debug.LogWarning("Missing: Jump Sound"); missingClips++; }
            if (landSound == null) { Debug.LogWarning("Missing: Land Sound"); missingClips++; }
            if (digSound == null) { Debug.LogWarning("Missing: Dig Sound"); missingClips++; }
            if (bombExplosionSound == null) { Debug.LogWarning("Missing: Bomb Explosion"); missingClips++; }
            
            Debug.Log($"Missing Clips: {missingClips}/15");
        }
    }
}
