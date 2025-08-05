#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

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
                VibeUnitySystem.EnableFileWatcher();
            }
            else
            {
                VibeUnitySystem.DisableFileWatcher();
            }
        }
        
        #endregion
        
        #region Scene State Menu Items
        
        [MenuItem("Tools/Vibe Unity/Scene State/Export Current Scene", priority = 150)]
        private static void ExportCurrentSceneState()
        {
            if (VibeUnitySceneExporter.ExportActiveSceneState())
            {
                EditorUtility.DisplayDialog("Scene State Export", 
                    "Scene state exported successfully!\n\nCheck the console for details and coverage analysis.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Scene State Export Failed", 
                    "Failed to export scene state. Check the console for error details.", 
                    "OK");
            }
        }
        
        [MenuItem("Tools/Vibe Unity/Scene State/Export Current Scene", validate = true, priority = 150)]
        private static bool ValidateExportCurrentSceneState()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().IsValid();
        }
        
        [MenuItem("Tools/Vibe Unity/Scene State/Import from State File...", priority = 151)]
        private static void ImportFromStateFile()
        {
            string stateFilePath = EditorUtility.OpenFilePanel("Select Scene State File", 
                "Assets", "json");
            
            if (!string.IsNullOrEmpty(stateFilePath) && stateFilePath.EndsWith(".state.json"))
            {
                if (EditorUtility.DisplayDialog("Import Scene from State", 
                    $"This will create a new scene from the state file:\n\n{System.IO.Path.GetFileName(stateFilePath)}\n\nAny unsaved changes in the current scene will be lost. Continue?", 
                    "Import", "Cancel"))
                {
                    if (VibeUnitySceneImporter.ImportSceneFromState(stateFilePath))
                    {
                        EditorUtility.DisplayDialog("Scene Import Success", 
                            "Scene imported successfully from state file!\n\nCheck the console for import details.", 
                            "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Scene Import Failed", 
                            "Failed to import scene from state file. Check the console for error details.", 
                            "OK");
                    }
                }
            }
            else if (!string.IsNullOrEmpty(stateFilePath))
            {
                EditorUtility.DisplayDialog("Invalid File", 
                    "Please select a valid .state.json file.", 
                    "OK");
            }
        }
        
        [MenuItem("Tools/Vibe Unity/Scene State/Show Coverage Report", priority = 152)]
        private static void ShowCoverageReport()
        {
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            string coverageDir = System.IO.Path.Combine(projectRoot, ".vibe-unity", "commands", "coverage-analysis");
            
            if (System.IO.Directory.Exists(coverageDir))
            {
                var files = System.IO.Directory.GetFiles(coverageDir, "*.log")
                    .OrderByDescending(System.IO.File.GetLastWriteTime)
                    .ToArray();
                
                if (files.Length > 0)
                {
                    string latestReport = files[0];
                    string fileName = System.IO.Path.GetFileName(latestReport);
                    
                    if (EditorUtility.DisplayDialog("Coverage Report", 
                        $"Open latest coverage report?\n\n{fileName}", 
                        "Open", "Cancel"))
                    {
                        Application.OpenURL($"file://{latestReport}");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("No Reports Found", 
                        "No coverage reports found. Export a scene state first to generate a coverage report.", 
                        "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("No Reports Found", 
                    "Coverage analysis directory not found. Export a scene state first to generate reports.", 
                    "OK");
            }
        }
        
        [MenuItem("Tools/Vibe Unity/Scene State/Open Coverage Directory", priority = 153)]
        private static void OpenCoverageDirectory()
        {
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            string coverageDir = System.IO.Path.Combine(projectRoot, ".vibe-unity", "commands", "coverage-analysis");
            
            if (System.IO.Directory.Exists(coverageDir))
            {
                EditorUtility.RevealInFinder(coverageDir);
            }
            else
            {
                EditorUtility.DisplayDialog("Directory Not Found", 
                    "Coverage analysis directory not found. Export a scene state first to create it.", 
                    "OK");
            }
        }
        
        [MenuItem("Tools/Vibe Unity/Scene State/Clean Up Log Files", priority = 154)]
        private static void CleanUpLogFiles()
        {
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            string vibeCommandsDir = System.IO.Path.Combine(projectRoot, ".vibe-unity", "commands");
            
            if (!System.IO.Directory.Exists(vibeCommandsDir))
            {
                EditorUtility.DisplayDialog("Nothing to Clean", 
                    ".vibe-unity/commands directory not found. No files to clean up.", 
                    "OK");
                return;
            }
            
            try
            {
                int totalFilesDeleted = 0;
                var directories = new[]
                {
                    System.IO.Path.Combine(vibeCommandsDir, "processed"),
                    System.IO.Path.Combine(vibeCommandsDir, "coverage-analysis"),
                    System.IO.Path.Combine(vibeCommandsDir, "import-logs")
                };
                
                var fileCounts = new System.Collections.Generic.Dictionary<string, int>();
                
                foreach (string dir in directories)
                {
                    if (System.IO.Directory.Exists(dir))
                    {
                        var files = System.IO.Directory.GetFiles(dir, "*.*");
                        fileCounts[System.IO.Path.GetFileName(dir)] = files.Length;
                        
                        foreach (string file in files)
                        {
                            System.IO.File.Delete(file);
                            totalFilesDeleted++;
                        }
                    }
                    else
                    {
                        fileCounts[System.IO.Path.GetFileName(dir)] = 0;
                    }
                }
                
                var message = new System.Text.StringBuilder();
                message.AppendLine("Log files cleaned up successfully!");
                message.AppendLine();
                message.AppendLine("Files deleted:");
                foreach (var kvp in fileCounts)
                {
                    message.AppendLine($"• {kvp.Key}: {kvp.Value} files");
                }
                message.AppendLine();
                message.AppendLine($"Total: {totalFilesDeleted} files deleted");
                
                EditorUtility.DisplayDialog("Cleanup Complete", message.ToString(), "OK");
                
                Debug.Log($"[VibeUnity] ✅ Cleaned up {totalFilesDeleted} log files from .vibe-unity/commands directories");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Cleanup Failed", 
                    $"Failed to clean up log files:\n\n{e.Message}", 
                    "OK");
                Debug.LogError($"[VibeUnity] Failed to clean up log files: {e.Message}");
            }
        }
        
        #endregion
        
        #region Configuration Menu
        
        [MenuItem("Tools/Vibe Unity/Update CLAUDE.md Documentation", priority = 195)]
        private static void UpdateClaudeDocumentation()
        {
            // Force update the stored version first
            string currentVersion = VibeUnityDocumentationUpdater.GetPackageVersion();
            EditorPrefs.SetString("VibeUnity_LastDocumentedVersion", currentVersion);
            
            // Call the documentation updater
            VibeUnityDocumentationUpdater.ForceUpdateDocumentation();
        }
        
        [MenuItem("Tools/Vibe Unity/Configuration", priority = 200)]
        private static void OpenConfiguration()
        {
            // For now, show a dialog with current settings
            string message = $"Vibe Unity Configuration\n\n" +
                           $"HTTP Server: DISABLED\n" +
                           $"CLI Commands: DISABLED\n\n" +
                           $"File Watcher: {(isFileWatcherEnabled ? "Enabled" : "Disabled")}\n" +
                           $"Watch Directory: .vibe-unity/commands\n\n" +
                           $"Scene State System: ENABLED\n" +
                           $"- Export/Import scene state JSON files\n" +
                           $"- Comprehensive coverage analysis\n" +
                           $"- Gap detection for missing features\n\n" +
                           $"Note: HTTP Server and CLI commands have been disabled.\n" +
                           $"File watching system and scene state functionality remain active.\n" +
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