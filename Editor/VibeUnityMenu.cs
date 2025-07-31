#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VibeUnity.Editor
{
    /// <summary>
    /// Central menu system for Vibe Unity
    /// </summary>
    public static class VibeUnityMenu
    {
        private const string HTTP_SERVER_ENABLED_KEY = "VibeUnity_HTTPServerEnabled";
        private const string FILE_WATCHER_ENABLED_KEY = "VibeUnity_FileWatcherEnabled";
        
        private static bool isHttpServerEnabled;
        private static bool isFileWatcherEnabled;
        
        static VibeUnityMenu()
        {
            // Load preferences
            isHttpServerEnabled = EditorPrefs.GetBool(HTTP_SERVER_ENABLED_KEY, false); // Default to false since disabled
            isFileWatcherEnabled = EditorPrefs.GetBool(FILE_WATCHER_ENABLED_KEY, true);
            
            // Apply initial state
            EditorApplication.delayCall += () =>
            {
                // HTTP Server disabled - don't apply state
                // ApplyHttpServerState();
                ApplyFileWatcherState();
            };
        }
        
        #region HTTP Server Menu Items - DISABLED
        
        // HTTP Server menu options disabled
        /*
        [MenuItem("Tools/Vibe Unity/HTTP Server Enabled", priority = 100)]
        private static void ToggleHttpServer()
        {
            isHttpServerEnabled = !isHttpServerEnabled;
            EditorPrefs.SetBool(HTTP_SERVER_ENABLED_KEY, isHttpServerEnabled);
            ApplyHttpServerState();
            
            Debug.Log($"[Vibe Unity] HTTP Server {(isHttpServerEnabled ? "enabled" : "disabled")}");
        }
        
        [MenuItem("Tools/Vibe Unity/HTTP Server Enabled", validate = true, priority = 100)]
        private static bool ValidateToggleHttpServer()
        {
            Menu.SetChecked("Tools/Vibe Unity/HTTP Server Enabled", isHttpServerEnabled);
            return true;
        }
        */
        
        private static void ApplyHttpServerState()
        {
            // HTTP Server disabled - functionality commented out
            /*
            if (isHttpServerEnabled)
            {
                // Start the server if not already running
                if (!VibeUnityHttpServer.IsRunning)
                {
                    VibeUnityHttpServer.StartServerInternal();
                }
            }
            else
            {
                // Stop the server if running
                if (VibeUnityHttpServer.IsRunning)
                {
                    VibeUnityHttpServer.StopServerInternal();
                }
            }
            */
        }
        
        #endregion
        
        #region File Watcher Menu Items
        
        [MenuItem("Tools/Vibe Unity/File Watcher Enabled", priority = 101)]
        private static void ToggleFileWatcher()
        {
            isFileWatcherEnabled = !isFileWatcherEnabled;
            EditorPrefs.SetBool(FILE_WATCHER_ENABLED_KEY, isFileWatcherEnabled);
            ApplyFileWatcherState();
            
            Debug.Log($"[Vibe Unity] File Watcher {(isFileWatcherEnabled ? "enabled" : "disabled")}");
        }
        
        [MenuItem("Tools/Vibe Unity/File Watcher Enabled", validate = true, priority = 101)]
        private static bool ValidateToggleFileWatcher()
        {
            Menu.SetChecked("Tools/Vibe Unity/File Watcher Enabled", isFileWatcherEnabled);
            return true;
        }
        
        private static void ApplyFileWatcherState()
        {
            if (isFileWatcherEnabled)
            {
                CLI.EnableFileWatcher();
            }
            else
            {
                CLI.DisableFileWatcher();
            }
        }
        
        #endregion
        
        #region Configuration Menu
        
        [MenuItem("Tools/Vibe Unity/Configuration", priority = 200)]
        private static void OpenConfiguration()
        {
            // For now, show a dialog with current settings
            string message = $"Vibe Unity Configuration\n\n" +
                           $"HTTP Server: DISABLED\n" +
                           $"CLI Commands: DISABLED\n\n" +
                           $"File Watcher: {(isFileWatcherEnabled ? "Enabled" : "Disabled")}\n" +
                           $"Watch Directory: .vibe-commands\n\n" +
                           $"Note: HTTP Server and CLI commands have been disabled.\n" +
                           $"File watching system remains functional.\n" +
                           $"Settings are saved in Unity EditorPrefs.";
            
            EditorUtility.DisplayDialog("Vibe Unity Configuration", message, "OK");
        }
        
        #endregion
        
        #region Utility Methods
        
        public static bool IsHttpServerEnabled => isHttpServerEnabled;
        public static bool IsFileWatcherEnabled => isFileWatcherEnabled;
        
        #endregion
    }
}
#endif