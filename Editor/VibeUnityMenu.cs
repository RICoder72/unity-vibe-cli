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
        
        #region Development Tools Menu
        
        [MenuItem("Tools/Vibe Unity/Development/Force Asset Refresh", priority = 180)]
        private static void ForceAssetRefresh()
        {
            Debug.Log("[Vibe Unity] Forcing asset database refresh...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            Debug.Log("[Vibe Unity] ✅ Asset database refresh completed");
            
            // Show a brief dialog to confirm the action
            EditorUtility.DisplayDialog("Asset Refresh", 
                "Asset database has been refreshed and Unity will recompile if needed.", 
                "OK");
        }
        
        [MenuItem("Tools/Vibe Unity/Development/Force Recompile", priority = 181)]
        private static void ForceRecompile()
        {
            Debug.Log("[Vibe Unity] Forcing script recompilation via CompilationController...");
            
            // Use the new CompilationController for better monitoring
            VibeUnityCompilationController.TriggerCompilation();
            
            string message = $"Compilation triggered for project {VibeUnityCompilationController.GetProjectHash()}.\n\n";
            
            if (VibeUnityCompilationController.IsCompiling())
            {
                message += "Unity is currently compiling - check console for progress.";
            }
            else
            {
                message += "Compilation requested - Unity will recompile if needed.";
            }
            
            EditorUtility.DisplayDialog("Force Recompile", message, "OK");
        }
        
        [MenuItem("Tools/Vibe Unity/Development/Run Test File", priority = 182)]
        private static void RunTestFile()
        {
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            string testFilePath = System.IO.Path.Combine(projectRoot, ".vibe-unity", "commands", "test-scene-creation.json");
            
            if (System.IO.File.Exists(testFilePath))
            {
                Debug.Log("[Vibe Unity] Running test file: test-scene-creation.json");
                
                // Process the test file directly
                if (IsFileWatcherEnabled)
                {
                    // File watcher will pick it up, just touch the file to trigger processing
                    System.IO.File.SetLastWriteTime(testFilePath, System.DateTime.Now);
                    Debug.Log("[Vibe Unity] ✅ Test file touched - file watcher will process it");
                }
                else
                {
                    // Process directly since file watcher is disabled
                    try
                    {
                        bool success = VibeUnityJSONProcessor.ProcessBatchFileWithLogging(testFilePath);
                        if (success)
                        {
                            Debug.Log("[Vibe Unity] ✅ Test file processed directly");
                        }
                        else
                        {
                            Debug.LogWarning("[Vibe Unity] ⚠ Test file processing completed with issues");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[Vibe Unity] Failed to process test file: {e.Message}");
                    }
                }
                
                EditorUtility.DisplayDialog("Test File", 
                    "Test file processing initiated.\n\nCheck Unity Console for results.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Test File Not Found", 
                    $"Test file not found at:\n{testFilePath}\n\nPlease ensure the test file exists.", 
                    "OK");
            }
        }
        
        [MenuItem("Tools/Vibe Unity/Development/Compilation Status", priority = 183)]
        private static void ShowCompilationStatus()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("Compilation Status");
            info.AppendLine("==================");
            info.AppendLine();
            info.AppendLine($"Project Hash: {VibeUnityCompilationController.GetProjectHash()}");
            info.AppendLine($"Currently Compiling: {(VibeUnityCompilationController.IsCompiling() ? "YES" : "NO")}");
            info.AppendLine();
            
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            string compileCommandsDir = System.IO.Path.Combine(projectRoot, ".vibe-unity", "compile-commands");
            string compileStatusDir = System.IO.Path.Combine(projectRoot, ".vibe-unity", "compile-status");
            
            info.AppendLine($"Commands Directory: {(System.IO.Directory.Exists(compileCommandsDir) ? "Exists" : "Missing")}");
            if (System.IO.Directory.Exists(compileCommandsDir))
            {
                var commandFiles = System.IO.Directory.GetFiles(compileCommandsDir, "*.json");
                info.AppendLine($"Pending Commands: {commandFiles.Length}");
            }
            
            info.AppendLine($"Status Directory: {(System.IO.Directory.Exists(compileStatusDir) ? "Exists" : "Missing")}");
            if (System.IO.Directory.Exists(compileStatusDir))
            {
                var statusFiles = System.IO.Directory.GetFiles(compileStatusDir, "*.json");
                info.AppendLine($"Recent Status Files: {statusFiles.Length}");
                
                if (statusFiles.Length > 0)
                {
                    var latestStatus = statusFiles.OrderByDescending(System.IO.File.GetLastWriteTime).First();
                    var fileName = System.IO.Path.GetFileName(latestStatus);
                    var lastModified = System.IO.File.GetLastWriteTime(latestStatus);
                    info.AppendLine($"Latest: {fileName} ({lastModified:HH:mm:ss})");
                }
            }
            
            info.AppendLine();
            info.AppendLine("Use 'Force Recompile' to test compilation system.");
            
            EditorUtility.DisplayDialog("Compilation Status", info.ToString(), "OK");
            Debug.Log($"[Vibe Unity] Compilation Status:\n{info.ToString()}");
        }

        [MenuItem("Tools/Vibe Unity/Development/Show Debug Info", priority = 184)]
        private static void ShowDebugInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("Vibe Unity Debug Information");
            info.AppendLine("============================");
            info.AppendLine();
            info.AppendLine($"Unity Version: {Application.unityVersion}");
            info.AppendLine($"Project: {Application.productName}");
            info.AppendLine($"Platform: {Application.platform}");
            info.AppendLine();
            info.AppendLine($"File Watcher: {(isFileWatcherEnabled ? "Enabled" : "Disabled")}");
            info.AppendLine($"HTTP Server: DISABLED");
            info.AppendLine();
            info.AppendLine($"Currently Compiling: {EditorApplication.isCompiling}");
            info.AppendLine($"Play Mode: {EditorApplication.isPlaying}");
            info.AppendLine($"Pause Mode: {EditorApplication.isPaused}");
            info.AppendLine();
            
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            string commandsDir = System.IO.Path.Combine(projectRoot, ".vibe-unity", "commands");
            info.AppendLine($"Commands Directory: {(System.IO.Directory.Exists(commandsDir) ? "Exists" : "Missing")}");
            
            if (System.IO.Directory.Exists(commandsDir))
            {
                var jsonFiles = System.IO.Directory.GetFiles(commandsDir, "*.json");
                info.AppendLine($"JSON Files in Commands Dir: {jsonFiles.Length}");
            }
            
            EditorUtility.DisplayDialog("Debug Information", info.ToString(), "OK");
            Debug.Log($"[Vibe Unity] Debug Info:\n{info.ToString()}");
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