using System;
using UnityEngine;

namespace PixelWorld
{
    [Serializable]
    public class WorldSettingsData
    {
        public int seed;
        public int worldStyle; // 0=Organic, 1=Geometric
        public float caveThreshold;
        public float caveFrequencyX;
        public float caveFrequencyY;
        public float caveLayerBlend;
        public float waterPoolChance;
        public float waterDepthThreshold;
        public float waterNoiseThreshold;
        public float sandFrequency;
        public float sandThresholdShallow;
        public float sandThresholdDeep;
        public int stabilityThreshold;
        
        // Also save dimensions? The user said "combo of seed, cave, water etc gen settings".
        // Dimensions are usually separate from "generation style", but let's include them just in case,
        // or maybe keep them separate as they are handled by WorldSizePreset.
        // Let's stick to generation settings for now as that's what "randomize" usually targets.
    }
}
