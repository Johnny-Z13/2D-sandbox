#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace PixelWorld
{
    /// <summary>
    /// Editor utility to generate placeholder audio files.
    /// Run from menu: Tools > Audio > Generate Placeholder WAVs
    /// </summary>
    public class GeneratePlaceholderAudio : EditorWindow
    {
        private const string FOLDER_PATH = "Assets/Audio/Placeholders/";
        
        [MenuItem("Tools/Audio/Generate Placeholder WAVs")]
        public static void GeneratePlaceholders()
        {
            // Ensure directory exists
            if (!Directory.Exists(FOLDER_PATH))
            {
                Directory.CreateDirectory(FOLDER_PATH);
            }

            string[] fileNames = new string[]
            {
                "level_ambience.wav",
                "player_jump.wav",
                "player_land.wav",
                "player_footstep.wav",
                "player_dig.wav",
                "paint_sand.wav",
                "paint_water.wav",
                "erase.wav",
                "sand_fall.wav",
                "bomb_place.wav",
                "bomb_explosion.wav",
                "player_hit.wav",
                "ui_click.wav",
                "hotbar_switch.wav",
                "preset_change.wav"
            };

            int successCount = 0;
            foreach (string fileName in fileNames)
            {
                string fullPath = Path.Combine(FOLDER_PATH, fileName);
                
                // Check if file already exists
                if (File.Exists(fullPath))
                {
                    Debug.Log($"Skipping {fileName} (already exists)");
                    continue;
                }
                
                // Generate silent WAV (1 second, 44.1kHz, mono, 16-bit)
                if (GenerateSilentWav(fullPath, 1.0f, 44100, 1, 16))
                {
                    successCount++;
                    Debug.Log($"Created: {fileName}");
                }
                else
                {
                    Debug.LogError($"Failed to create: {fileName}");
                }
            }

            AssetDatabase.Refresh();
            
            Debug.Log($"=== Placeholder Audio Generation Complete ===");
            Debug.Log($"Created {successCount} placeholder WAV files in {FOLDER_PATH}");
            Debug.Log($"NOTE: These are silent 1-second files. Replace them with actual audio.");
            
            EditorUtility.DisplayDialog(
                "Placeholder Audio Generated",
                $"Created {successCount} placeholder WAV files.\n\n" +
                $"Location: {FOLDER_PATH}\n\n" +
                "These are silent files. Replace them with your actual audio assets.",
                "OK"
            );
        }

        /// <summary>
        /// Generate a silent WAV file
        /// </summary>
        private static bool GenerateSilentWav(string path, float duration, int sampleRate, int channels, int bitsPerSample)
        {
            try
            {
                int byteRate = sampleRate * channels * (bitsPerSample / 8);
                int samplesCount = (int)(sampleRate * duration) * channels;
                int dataSize = samplesCount * (bitsPerSample / 8);
                
                using (FileStream fs = new FileStream(path, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    // RIFF header
                    writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                    writer.Write(36 + dataSize); // File size - 8
                    writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                    
                    // fmt chunk
                    writer.Write(new char[] { 'f', 'm', 't', ' ' });
                    writer.Write(16); // Subchunk1Size (16 for PCM)
                    writer.Write((ushort)1); // AudioFormat (1 = PCM)
                    writer.Write((ushort)channels); // NumChannels
                    writer.Write(sampleRate); // SampleRate
                    writer.Write(byteRate); // ByteRate
                    writer.Write((ushort)(channels * (bitsPerSample / 8))); // BlockAlign
                    writer.Write((ushort)bitsPerSample); // BitsPerSample
                    
                    // data chunk
                    writer.Write(new char[] { 'd', 'a', 't', 'a' });
                    writer.Write(dataSize);
                    
                    // Write silent samples (all zeros)
                    byte[] silence = new byte[dataSize];
                    writer.Write(silence);
                }
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error generating WAV file: {e.Message}");
                return false;
            }
        }

        [MenuItem("Tools/Audio/Open Placeholder Folder")]
        public static void OpenPlaceholderFolder()
        {
            string fullPath = Path.GetFullPath(FOLDER_PATH);
            
            if (Directory.Exists(fullPath))
            {
                EditorUtility.RevealInFinder(fullPath);
            }
            else
            {
                Debug.LogWarning($"Folder does not exist: {fullPath}");
            }
        }
    }
}
#endif

