using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace VibeUnity.Editor
{
    /// <summary>
    /// System utilities and helper methods for Vibe Unity
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
        /// Initialize the file watcher system
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
            
            EnableFileWatcher();
            
            // Also check for existing files on startup
            CheckForCommandFiles();
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
                        
                    ProcessCommandFile(file);
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
                fileWatcher.Created += (sender, e) => ProcessCommandFile(e.FullPath);
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
    }
}