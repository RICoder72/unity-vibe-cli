using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace VibeUnity.Editor
{
    /// <summary>
    /// Scene management functionality for Vibe Unity
    /// </summary>
    public static class VibeUnityScenes
    {
        #region Scene Creation
        
        /// <summary>
        /// Creates a new Unity scene with specified name, path, and setup type
        /// </summary>
        public static bool CreateScene(string sceneName, string scenePath, string sceneSetup = "Empty", bool addToBuildSettings = false)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(sceneName))
                {
                    Debug.LogError("[VibeUnity] Scene name cannot be empty");
                    return false;
                }
                
                // Ensure scene path exists
                string fullScenePath = Path.Combine(scenePath, sceneName + ".unity");
                string directory = Path.GetDirectoryName(fullScenePath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    AssetDatabase.Refresh();
                }
                
                // Check if scene already exists
                if (File.Exists(fullScenePath))
                {
                    Debug.LogWarning($"[VibeUnity] Scene already exists: {fullScenePath}");
                    return false;
                }

                // Create new scene
                NewSceneSetup setupType = ParseSceneSetup(sceneSetup);
                Scene newScene = EditorSceneManager.NewScene(setupType, NewSceneMode.Single);
                
                // Save scene
                bool saved = EditorSceneManager.SaveScene(newScene, fullScenePath);
                
                if (saved)
                {
                    Debug.Log($"[VibeUnity] ✅ SUCCESS: Created scene '{sceneName}'");
                    Debug.Log($"[VibeUnity]    └─ Type: {sceneSetup}");
                    Debug.Log($"[VibeUnity]    └─ Path: {fullScenePath}");
                    Debug.Log($"[VibeUnity]    └─ Added to Build: {addToBuildSettings}");
                    Debug.Log($"[VibeUnity]    └─ Scene Objects: {newScene.rootCount} root GameObjects");
                    
                    // Add to build settings if requested
                    if (addToBuildSettings)
                    {
                        AddSceneToBuildSettings(fullScenePath);
                    }
                    
                    // Refresh asset database
                    AssetDatabase.Refresh();
                    return true;
                }
                else
                {
                    Debug.LogError($"[VibeUnity] Failed to save scene: {fullScenePath}");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnity] Exception creating scene: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Lists all available scene types for the current Unity installation
        /// </summary>
        public static void ListSceneTypes()
        {
            var availableTypes = GetAvailableSceneTypes();
            Debug.Log($"[VibeUnity] Available scene types: {string.Join(", ", availableTypes)}");
            
            // Also log detailed descriptions
            Debug.Log("[VibeUnity] Scene Type Descriptions:");
            Debug.Log("  Empty - Completely empty scene");
            Debug.Log("  DefaultGameObjects - Scene with Main Camera and Directional Light");
            Debug.Log("  2D - 2D optimized scene setup");
            Debug.Log("  3D - 3D optimized scene setup with skybox");
            
            // Check for render pipeline specific types
            if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
            {
                string rpName = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.GetType().Name;
                if (rpName.Contains("Universal"))
                {
                    Debug.Log("  URP - Universal Render Pipeline optimized scene");
                }
                else if (rpName.Contains("HDRP") || rpName.Contains("HighDefinition"))
                {
                    Debug.Log("  HDRP - High Definition Render Pipeline optimized scene");
                }
            }
        }
        
        #endregion
        
        #region Scene Loading
        
        /// <summary>
        /// Loads a target scene if specified, otherwise uses current scene
        /// </summary>
        public static bool LoadTargetScene(string sceneName)
        {
            // If no scene name provided, use current scene
            if (string.IsNullOrEmpty(sceneName))
            {
                return SceneManager.GetActiveScene().IsValid();
            }
            
            // Try to find and load the scene
            string sceneAssetPath = FindSceneAsset(sceneName);
            
            if (string.IsNullOrEmpty(sceneAssetPath))
            {
                Debug.LogError($"[VibeUnity] Scene '{sceneName}' not found in project");
                return false;
            }
            
            try
            {
                Scene targetScene = EditorSceneManager.OpenScene(sceneAssetPath);
                if (targetScene.IsValid())
                {
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnity] Failed to load scene '{sceneName}': {e.Message}");
            }
            
            return false;
        }
        
        /// <summary>
        /// Finds a scene asset by name in the project
        /// </summary>
        public static string FindSceneAsset(string sceneName)
        {
            // Clean up scene name
            if (sceneName.EndsWith(".unity"))
            {
                sceneName = sceneName.Substring(0, sceneName.Length - 6);
            }
            
            // Search for scene assets
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string assetName = Path.GetFileNameWithoutExtension(path);
                
                if (assetName.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }
            
            // If exact match not found, try partial match
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string assetName = Path.GetFileNameWithoutExtension(path);
                
                if (assetName.Contains(sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }
            
            return null;
        }
        
        #endregion
        
        #region Scene Management
        
        /// <summary>
        /// Saves the currently active scene and refreshes the asset database
        /// </summary>
        public static void SaveActiveScene(bool suppressLog = false, bool forceSave = false)
        {
            try
            {
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.Refresh();
                if (!suppressLog)
                    Debug.Log("[VibeUnity] Scene saved successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VibeUnity] Failed to save scene: {e.Message}");
            }
        }
        
        /// <summary>
        /// Marks the currently active scene as dirty to ensure changes are saved
        /// </summary>
        public static void MarkActiveSceneDirty()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
        }
        
        /// <summary>
        /// Force saves the currently active scene immediately
        /// </summary>
        public static void ForceSaveActiveScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && !string.IsNullOrEmpty(activeScene.path))
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene, activeScene.path);
                AssetDatabase.Refresh();
            }
        }
        
        #endregion
        
        #region Build Settings
        
        /// <summary>
        /// Adds a scene to the build settings
        /// </summary>
        public static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            
            // Check if scene already exists in build settings
            bool exists = scenes.Any(s => s.path == scenePath);
            
            if (!exists)
            {
                var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
                scenes.CopyTo(newScenes, 0);
                newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = newScenes;
                
                Debug.Log($"[VibeUnity] Added scene to build settings: {scenePath}");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Parses scene setup type from string
        /// </summary>
        private static NewSceneSetup ParseSceneSetup(string sceneSetup)
        {
            switch (sceneSetup.ToLower())
            {
                case "empty":
                    return NewSceneSetup.EmptyScene;
                case "defaultgameobjects":
                case "default":
                    return NewSceneSetup.DefaultGameObjects;
                default:
                    Debug.LogWarning($"[VibeUnity] Unknown scene setup '{sceneSetup}', using DefaultGameObjects");
                    return NewSceneSetup.DefaultGameObjects;
            }
        }
        
        /// <summary>
        /// Gets all available scene types for the current Unity installation
        /// </summary>
        public static List<string> GetAvailableSceneTypes()
        {
            var types = new List<string> { "Empty", "DefaultGameObjects" };
            
            // Check for 2D and 3D templates
            types.Add("2D");
            types.Add("3D");
            
            // Check for render pipeline specific templates
            if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
            {
                string rpName = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.GetType().Name;
                if (rpName.Contains("Universal"))
                {
                    types.Add("URP");
                }
                else if (rpName.Contains("HDRP") || rpName.Contains("HighDefinition"))
                {
                    types.Add("HDRP");
                }
            }
            
            return types;
        }
        
        /// <summary>
        /// Lists all available scenes in the project
        /// </summary>
        public static string ListAvailableScenes()
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            var scenes = new List<string>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string sceneName = Path.GetFileNameWithoutExtension(path);
                scenes.Add($"{sceneName} ({path})");
            }
            
            return scenes.Count > 0 ? string.Join(", ", scenes) : "No scenes found";
        }
        
        #endregion
    }
}