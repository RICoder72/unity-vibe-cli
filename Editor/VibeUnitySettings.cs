#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace VibeUnity.Editor
{
    /// <summary>
    /// Settings manager for Vibe Unity package
    /// Handles keyboard shortcuts, preferences, and configuration
    /// </summary>
    public static class VibeUnitySettings
    {
        private const string SETTINGS_FILENAME = "vibe-unity-settings.json";
        private static string settingsPath;
        private static VibeUnitySettingsData cachedSettings;
        
        static VibeUnitySettings()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string vibeUnityDir = Path.Combine(projectRoot, ".vibe-unity");
            settingsPath = Path.Combine(vibeUnityDir, SETTINGS_FILENAME);
            
            Directory.CreateDirectory(vibeUnityDir);
        }
        
        /// <summary>
        /// Get current settings, loading from file if necessary
        /// </summary>
        public static VibeUnitySettingsData GetSettings()
        {
            if (cachedSettings == null)
            {
                LoadSettings();
            }
            return cachedSettings;
        }
        
        /// <summary>
        /// Save settings to file
        /// </summary>
        public static void SaveSettings(VibeUnitySettingsData settings)
        {
            try
            {
                string json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(settingsPath, json);
                cachedSettings = settings;
                
                Debug.Log($"[VibeUnity] Settings saved to: {settingsPath}");
                
                // Update menu items with new shortcuts
                VibeUnityCompilationController.UpdateShortcuts(settings.shortcuts);
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnity] Failed to save settings: {e.Message}");
            }
        }
        
        /// <summary>
        /// Load settings from file, or create defaults if file doesn't exist
        /// </summary>
        private static void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    cachedSettings = JsonUtility.FromJson<VibeUnitySettingsData>(json);
                    
                    // Validate and fill in any missing shortcuts with defaults
                    if (cachedSettings.shortcuts == null)
                    {
                        cachedSettings.shortcuts = CreateDefaultShortcuts();
                    }
                    else
                    {
                        ValidateShortcuts(cachedSettings.shortcuts);
                    }
                }
                else
                {
                    // Create default settings
                    cachedSettings = CreateDefaultSettings();
                    SaveSettings(cachedSettings);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnity] Failed to load settings: {e.Message}");
                cachedSettings = CreateDefaultSettings();
            }
        }
        
        /// <summary>
        /// Create default settings configuration
        /// </summary>
        private static VibeUnitySettingsData CreateDefaultSettings()
        {
            return new VibeUnitySettingsData
            {
                version = "1.0.0",
                shortcuts = CreateDefaultShortcuts(),
                enableDebugLogs = true,
                autoSaveSettings = true
            };
        }
        
        /// <summary>
        /// Create default keyboard shortcuts
        /// </summary>
        private static ShortcutSettings CreateDefaultShortcuts()
        {
            return new ShortcutSettings
            {
                checkStatus = "%#k",      // Ctrl+Shift+K
                forceRecompile = "%#u",   // Ctrl+Shift+U  
                clearCache = "%#l"        // Ctrl+Shift+L
            };
        }
        
        /// <summary>
        /// Validate shortcuts and fill in missing ones with defaults
        /// </summary>
        private static void ValidateShortcuts(ShortcutSettings shortcuts)
        {
            var defaults = CreateDefaultShortcuts();
            
            if (string.IsNullOrEmpty(shortcuts.checkStatus))
                shortcuts.checkStatus = defaults.checkStatus;
                
            if (string.IsNullOrEmpty(shortcuts.forceRecompile))
                shortcuts.forceRecompile = defaults.forceRecompile;
                
            if (string.IsNullOrEmpty(shortcuts.clearCache))
                shortcuts.clearCache = defaults.clearCache;
        }
        
        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        public static void ResetToDefaults()
        {
            var defaultSettings = CreateDefaultSettings();
            SaveSettings(defaultSettings);
        }
        
        /// <summary>
        /// Get the path to the settings file
        /// </summary>
        public static string GetSettingsPath() => settingsPath;
    }
    
    /// <summary>
    /// Main settings data structure
    /// </summary>
    [Serializable]
    public class VibeUnitySettingsData
    {
        public string version;
        public ShortcutSettings shortcuts;
        public bool enableDebugLogs;
        public bool autoSaveSettings;
    }
    
    /// <summary>
    /// Keyboard shortcut settings
    /// Uses Unity's MenuItemAttribute shortcut format:
    /// % = Ctrl/Cmd, # = Shift, & = Alt
    /// Example: "%#k" = Ctrl+Shift+K
    /// </summary>
    [Serializable]
    public class ShortcutSettings
    {
        [Tooltip("Shortcut for 'Check Compilation Status' (default: Ctrl+Shift+K)")]
        public string checkStatus;
        
        [Tooltip("Shortcut for 'Force Recompile' (default: Ctrl+Shift+U)")]
        public string forceRecompile;
        
        [Tooltip("Shortcut for 'Clear Cache' (default: Ctrl+Shift+L)")]
        public string clearCache;
        
        /// <summary>
        /// Get human-readable description of a shortcut
        /// </summary>
        public static string GetShortcutDescription(string shortcutCode)
        {
            if (string.IsNullOrEmpty(shortcutCode)) return "None";
            
            string result = "";
            
            if (shortcutCode.Contains("%"))
            {
                result += Application.platform == RuntimePlatform.OSXEditor ? "Cmd+" : "Ctrl+";
            }
            
            if (shortcutCode.Contains("#"))
            {
                result += "Shift+";
            }
            
            if (shortcutCode.Contains("&"))
            {
                result += "Alt+";
            }
            
            // Get the key (last character usually)
            char key = shortcutCode[shortcutCode.Length - 1];
            result += char.ToUpper(key);
            
            return result;
        }
    }
}
#endif