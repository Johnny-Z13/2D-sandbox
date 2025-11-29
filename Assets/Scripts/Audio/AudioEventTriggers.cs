using UnityEngine;

namespace PixelWorld
{
    /// <summary>
    /// Static event triggers for audio system.
    /// Call these methods from anywhere to trigger audio events.
    /// </summary>
    public static class AudioEventTriggers
    {
        // ============================================
        // PLAYER EVENTS
        // ============================================
        
        public static void OnPlayerJump()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJump();
            }
        }
        
        public static void OnPlayerLand()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLand();
            }
        }
        
        public static void OnPlayerFootstep()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayFootstep();
            }
        }
        
        public static void OnPlayerCrouch()
        {
            // Optional: Add crouch sound later
        }
        
        // ============================================
        // DIGGING & WORLD INTERACTION
        // ============================================
        
        public static void OnDig()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDig();
            }
        }
        
        public static void OnPaintSand()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPaintSand();
            }
        }
        
        public static void OnPaintWater()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPaintWater();
            }
        }
        
        public static void OnEraseTerrain()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayErase();
            }
        }
        
        // ============================================
        // BOMB EVENTS
        // ============================================
        
        public static void OnBombPlaced()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBombPlace();
            }
        }
        
        public static void OnBombExplode()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBombExplosion();
            }
        }
        
        // ============================================
        // UI EVENTS
        // ============================================
        
        public static void OnUIClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }
        }
        
        public static void OnHotbarSwitch()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHotbarSwitch();
            }
        }
        
        public static void OnPresetChange()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPresetChange();
            }
        }
    }
}
