using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace VibeUnity.Editor
{
    /// <summary>
    /// System utilities and helper methods for Vibe Unity - v1.2.0
    /// </summary>
    public static class VibeUnitySystem
    {
        #region File Watcher System
        
        /// <summary>
        /// Directory where CLI drops command files for Unity to pick up
        /// </summary>
        private static readonly string COMMAND_QUEUE_DIR = Path.Combine(Application.dataPath, "..", ".vibe-commands");
        private static FileSystemWatcher fileWatcher;
        private static readonly object lockObject = new object();
        
        /// <summary>
        /// Queue for main thread operations to avoid threading issues with Unity Editor APIs
        /// </summary>
        private static readonly ConcurrentQueue<System.Action> mainThreadQueue = new ConcurrentQueue<System.Action>();
        
        /// <summary>
        /// Initialize the file watcher system and scene state hooks
        /// </summary>
        [InitializeOnLoadMethod]
        public static void InitializeFileWatcher()
        {
            // Ensure command queue directory exists
            if (!Directory.Exists(COMMAND_QUEUE_DIR))
            {
                Directory.CreateDirectory(COMMAND_QUEUE_DIR);
                Debug.Log($"[VibeUnity] Created command queue directory: {COMMAND_QUEUE_DIR}");
            }
            
            // Set up claude-compile-check script symlink
            SetupClaudeCompileCheckScript();
            
            // Enable file watcher by default - can be toggled via menu
            EnableFileWatcher();
            
            // Set up the main thread dispatcher for thread-safe operations
            EditorApplication.update += ProcessMainThreadQueue;
            
            // Set up scene state auto-generation hooks
            InitializeSceneStateHooks();
            
            // Also check for existing files on startup
            CheckForCommandFiles();
        }
        
        /// <summary>
        /// Processes queued main thread operations
        /// </summary>
        private static void ProcessMainThreadQueue()
        {
            while (mainThreadQueue.TryDequeue(out System.Action action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[VibeUnity] Error processing main thread operation: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Queues an action to be executed on the main thread
        /// </summary>
        public static void QueueMainThreadAction(System.Action action)
        {
            if (action != null)
            {
                mainThreadQueue.Enqueue(action);
            }
        }
        
        
        /// <summary>
        /// Checks for existing command files and processes them
        /// </summary>
        private static void CheckForCommandFiles()
        {
            try
            {
                if (!Directory.Exists(COMMAND_QUEUE_DIR))
                    return;
                    
                string[] jsonFiles = Directory.GetFiles(COMMAND_QUEUE_DIR, "*.json");
                
                foreach (string file in jsonFiles)
                {
                    // Skip if file is locked (still being written)
                    if (IsFileLocked(file))
                        continue;
                        
                    // Queue the file processing on the main thread
                    string filePath = file; // Capture for closure
                    QueueMainThreadAction(() => ProcessCommandFile(filePath));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnity] Error checking for command files: {e.Message}");
            }
        }
        
        /// <summary>
        /// Processes a command file using the JSON processor
        /// </summary>
        private static void ProcessCommandFile(string filePath)
        {
            lock (lockObject)
            {
                try
                {
                    Debug.Log($"[VibeUnity] Processing command file: {Path.GetFileName(filePath)}");
                    
                    // Use the JSON processor to handle the file
                    bool success = VibeUnityJSONProcessor.ProcessBatchFileWithLogging(filePath);
                    
                    // Move processed file to processed directory
                    string processedDir = Path.Combine(COMMAND_QUEUE_DIR, "processed");
                    if (!Directory.Exists(processedDir))
                    {
                        Directory.CreateDirectory(processedDir);
                    }
                    
                    string fileName = Path.GetFileName(filePath);
                    string timestamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string processedJsonPath = Path.Combine(processedDir, $"{timestamp}-{fileName}");
                    
                    File.Move(filePath, processedJsonPath);
                    Debug.Log($"[VibeUnity] Moved processed file to: {processedJsonPath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[VibeUnity] Error processing command file {filePath}: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Checks if a file is locked (being written to)
        /// </summary>
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }
        
        /// <summary>
        /// Enables the file watcher
        /// </summary>
        public static void EnableFileWatcher()
        {
            if (fileWatcher == null && Directory.Exists(COMMAND_QUEUE_DIR))
            {
                fileWatcher = new FileSystemWatcher(COMMAND_QUEUE_DIR, "*.json");
                fileWatcher.Created += (sender, e) => {
                    // Queue the file processing on the main thread to avoid threading issues
                    string filePath = e.FullPath; // Capture for closure
                    QueueMainThreadAction(() => ProcessCommandFile(filePath));
                };
                fileWatcher.EnableRaisingEvents = true;
                Debug.Log("[VibeUnity] File watcher enabled");
            }
        }
        
        /// <summary>
        /// Disables the file watcher
        /// </summary>
        public static void DisableFileWatcher()
        {
            if (fileWatcher != null)
            {
                fileWatcher.EnableRaisingEvents = false;
                fileWatcher.Dispose();
                fileWatcher = null;
                Debug.Log("[VibeUnity] File watcher disabled");
            }
            
            // Remove the main thread dispatcher
            EditorApplication.update -= ProcessMainThreadQueue;
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates scene creation parameters
        /// </summary>
        public static bool ValidateSceneCreation(string sceneName, string scenePath)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[VibeUnity] Scene name cannot be empty");
                return false;
            }
            
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("[VibeUnity] Scene path cannot be empty");
                return false;
            }
            
            // Check for valid characters in scene name
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (sceneName.IndexOfAny(invalidChars) >= 0)
            {
                Debug.LogError($"[VibeUnity] Scene name contains invalid characters: {sceneName}");
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Debug and Information
        
        /// <summary>
        /// Shows help information for CLI commands
        /// </summary>
        public static void ShowHelp()
        {
            Debug.Log("[VibeUnity] === Vibe Unity Help ===");
            Debug.Log("[VibeUnity] ");
            Debug.Log("[VibeUnity] SCENE CREATION:");
            Debug.Log("[VibeUnity]   CreateScene(sceneName, scenePath, sceneType, addToBuild)");
            Debug.Log("[VibeUnity]   Example: CreateScene(\"MyScene\", \"Assets/Scenes\", \"DefaultGameObjects\", false)");
            Debug.Log("[VibeUnity] ");
            Debug.Log("[VibeUnity] ADD CANVAS:");
            Debug.Log("[VibeUnity]   AddCanvas(canvasName, sceneName, renderMode, width, height, scaleMode)");
            Debug.Log("[VibeUnity]   Example: AddCanvas(\"UICanvas\", null, \"ScreenSpaceOverlay\", 1920, 1080, \"ScaleWithScreenSize\")");
            Debug.Log("[VibeUnity] ");
            Debug.Log("[VibeUnity] JSON BATCH FILES:");
            Debug.Log("[VibeUnity]   Drop JSON files in .vibe-commands/ directory for automatic processing");
            Debug.Log("[VibeUnity] ");
            Debug.Log("[VibeUnity] Available Scene Types: " + string.Join(", ", VibeUnityScenes.GetAvailableSceneTypes()));
            Debug.Log("[VibeUnity] Available Render Modes: ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace");
            Debug.Log("[VibeUnity] Available Scale Modes: ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize");
            Debug.Log("[VibeUnity] ===============================");
        }
        
        /// <summary>
        /// Shows debug information about the current Unity configuration
        /// </summary>
        public static void ShowDebugInfo()
        {
            Debug.Log("[VibeUnity] === Debug Information ===");
            Debug.Log($"[VibeUnity] Unity Version: {Application.unityVersion}");
            Debug.Log($"[VibeUnity] Project Path: {Application.dataPath}");
            Debug.Log($"[VibeUnity] Command Queue: {COMMAND_QUEUE_DIR}");
            Debug.Log($"[VibeUnity] File Watcher: {(fileWatcher?.EnableRaisingEvents == true ? "Enabled" : "Disabled")}");
            Debug.Log($"[VibeUnity] Available Scene Types: {string.Join(", ", VibeUnityScenes.GetAvailableSceneTypes())}");
            Debug.Log($"[VibeUnity] Available Scenes: {VibeUnityScenes.ListAvailableScenes()}");
            
            // Check render pipeline
            if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
            {
                string rpName = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.GetType().Name;
                Debug.Log($"[VibeUnity] Render Pipeline: {rpName}");
            }
            else
            {
                Debug.Log("[VibeUnity] Render Pipeline: Built-in");
            }
            
            Debug.Log("[VibeUnity] ========================");
        }
        
        /// <summary>
        /// Tests CLI functionality
        /// </summary>
        public static bool TestFunctionality()
        {
            Debug.Log("[VibeUnity] === Testing Functionality ===");
            
            try
            {
                // Test scene types
                var sceneTypes = VibeUnityScenes.GetAvailableSceneTypes();
                Debug.Log($"[VibeUnity] ✅ Scene types available: {sceneTypes.Count}");
                
                // Test current scene
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                Debug.Log($"[VibeUnity] ✅ Active scene: {activeScene.name} ({activeScene.path})");
                
                // Test command queue directory
                bool queueExists = Directory.Exists(COMMAND_QUEUE_DIR);
                Debug.Log($"[VibeUnity] ✅ Command queue directory: {(queueExists ? "Exists" : "Missing")}");
                
                // Test file watcher
                bool watcherActive = fileWatcher?.EnableRaisingEvents == true;
                Debug.Log($"[VibeUnity] ✅ File watcher: {(watcherActive ? "Active" : "Inactive")}");
                
                Debug.Log("[VibeUnity] ✅ All tests passed");
                Debug.Log("[VibeUnity] ==========================");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnity] ❌ Test failed: {e.Message}");
                Debug.Log("[VibeUnity] ==========================");
                return false;
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Gets the most recently modified scene in the project
        /// </summary>
        public static string GetMostRecentScene()
        {
            try
            {
                string[] guids = AssetDatabase.FindAssets("t:Scene");
                
                if (guids.Length == 0)
                    return null;
                
                var sceneInfo = guids
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => new { 
                        Path = path, 
                        Name = Path.GetFileNameWithoutExtension(path),
                        LastWrite = File.GetLastWriteTime(path)
                    })
                    .OrderByDescending(info => info.LastWrite)
                    .First();
                
                return sceneInfo.Name;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Refreshes the Unity asset database
        /// </summary>
        public static void RefreshAssets()
        {
            AssetDatabase.Refresh();
            Debug.Log("[VibeUnity] Asset database refreshed");
        }
        
        /// <summary>
        /// Cleans up temporary files and directories
        /// </summary>
        public static void Cleanup()
        {
            try
            {
                // Clean up old processed files (older than 7 days)
                string processedDir = Path.Combine(COMMAND_QUEUE_DIR, "processed");
                if (Directory.Exists(processedDir))
                {
                    var oldFiles = Directory.GetFiles(processedDir)
                        .Where(file => File.GetLastWriteTime(file) < System.DateTime.Now.AddDays(-7))
                        .ToArray();
                        
                    foreach (string file in oldFiles)
                    {
                        File.Delete(file);
                    }
                    
                    if (oldFiles.Length > 0)
                    {
                        Debug.Log($"[VibeUnity] Cleaned up {oldFiles.Length} old processed files");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VibeUnity] Error during cleanup: {e.Message}");
            }
        }
        
        #endregion
        
        #region Scene State Auto-Generation Hooks
        
        private static bool sceneStateHooksInitialized = false;
        
        /// <summary>
        /// Initialize scene state auto-generation hooks
        /// </summary>
        private static void InitializeSceneStateHooks()
        {
            if (sceneStateHooksInitialized)
                return;
                
            // Hook into scene save events to automatically generate state files
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
            
            sceneStateHooksInitialized = true;
            Debug.Log("[VibeUnity] Scene state auto-generation hooks initialized");
        }
        
        /// <summary>
        /// Called when a scene is saved - automatically generates scene state file
        /// </summary>
        private static void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            try
            {
                // Only generate state files for scenes that are actually saved to disk
                if (string.IsNullOrEmpty(scene.path))
                    return;
                    
                Debug.Log($"[VibeUnity] Auto-generating scene state for saved scene: {scene.name}");
                
                // Generate state file in a background operation to avoid blocking save
                mainThreadQueue.Enqueue(() =>
                {
                    try
                    {
                        if (VibeUnitySceneExporter.ExportActiveSceneState())
                        {
                            Debug.Log($"[VibeUnity] ✅ Auto-generated scene state file: {scene.name}.state.json");
                        }
                        else
                        {
                            Debug.LogWarning($"[VibeUnity] ⚠️ Failed to auto-generate scene state for: {scene.name}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[VibeUnity] ⚠️ Exception during auto scene state generation: {e.Message}");
                    }
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnity] Error in scene save hook: {e.Message}");
            }
        }
        
        /// <summary>
        /// Cleanup scene state hooks when shutting down
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            // Re-initialize hooks after script reload
            InitializeSceneStateHooks();
        }
        
        #endregion
        
        #region Claude Compile Check Script Setup
        
        /// <summary>
        /// Sets up the claude-compile-check.sh script symlink for claude-code integration
        /// </summary>
        private static void SetupClaudeCompileCheckScript()
        {
            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string packageScriptPath = Path.Combine(Application.dataPath, "..", "Packages", "com.ricoder.vibe-unity", "Scripts", "claude-compile-check.sh");
                string projectScriptPath = Path.Combine(projectRoot, "claude-compile-check.sh");
                
                // Check if package script exists
                if (!File.Exists(packageScriptPath))
                {
                    Debug.LogWarning($"[VibeUnity] Claude compile check script not found at: {packageScriptPath}");
                    return;
                }
                
                // Check if symlink already exists and points to correct location
                if (File.Exists(projectScriptPath))
                {
                    try
                    {
                        // On Windows/WSL, we'll use a copy instead of symlink for compatibility
                        var existingContent = File.ReadAllText(projectScriptPath);
                        var packageContent = File.ReadAllText(packageScriptPath);
                        
                        if (existingContent == packageContent)
                        {
                            // Script is up to date
                            return;
                        }
                        else
                        {
                            // Update the script
                            File.Copy(packageScriptPath, projectScriptPath, true);
                            Debug.Log("[VibeUnity] Updated claude-compile-check.sh script in project root");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[VibeUnity] Failed to update claude-compile-check.sh: {e.Message}");
                        return;
                    }
                }
                else
                {
                    // Copy script to project root
                    try
                    {
                        File.Copy(packageScriptPath, projectScriptPath);
                        
                        // Make it executable if on Unix-like system
                        if (Environment.OSVersion.Platform == PlatformID.Unix || 
                            Environment.OSVersion.Platform == PlatformID.MacOSX)
                        {
                            var process = new System.Diagnostics.Process
                            {
                                StartInfo = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "chmod",
                                    Arguments = "+x " + projectScriptPath,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };
                            process.Start();
                            process.WaitForExit();
                        }
                        
                        Debug.Log("[VibeUnity] ✅ Installed claude-compile-check.sh script for claude-code integration");
                        Debug.Log($"[VibeUnity] Script location: {projectScriptPath}");
                        Debug.Log("[VibeUnity] Usage: ./claude-compile-check.sh [--include-warnings]");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[VibeUnity] Failed to install claude-compile-check.sh script: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnity] Error setting up claude-compile-check script: {e.Message}");
            }
        }
        
        #endregion
    }
}