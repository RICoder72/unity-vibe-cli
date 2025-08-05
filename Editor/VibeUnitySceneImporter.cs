#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;

namespace VibeUnity.Editor
{
    /// <summary>
    /// Imports and rebuilds Unity scenes from JSON state files
    /// </summary>
    public static class VibeUnitySceneImporter
    {
        /// <summary>
        /// Imports a scene from a JSON state file
        /// </summary>
        /// <param name="stateFilePath">Path to the .state.json file</param>
        /// <param name="targetScenePath">Optional target scene path (if different from state file location)</param>
        /// <returns>True if import was successful</returns>
        public static bool ImportSceneFromState(string stateFilePath, string targetScenePath = null)
        {
            try
            {
                if (!File.Exists(stateFilePath))
                {
                    Debug.LogError($"[VibeUnitySceneImporter] State file not found: {stateFilePath}");
                    return false;
                }
                
                // Read and parse JSON
                string jsonContent = File.ReadAllText(stateFilePath);
                SceneState sceneState;
                
                try
                {
                    sceneState = JsonUtility.FromJson<SceneState>(jsonContent);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[VibeUnitySceneImporter] Failed to parse state file: {e.Message}");
                    return false;
                }
                
                // Determine target scene path
                if (string.IsNullOrEmpty(targetScenePath))
                {
                    string directory = Path.GetDirectoryName(stateFilePath);
                    string baseName = Path.GetFileNameWithoutExtension(stateFilePath);
                    if (baseName.EndsWith(".state"))
                    {
                        baseName = baseName.Substring(0, baseName.Length - 6); // Remove ".state"
                    }
                    targetScenePath = Path.Combine(directory, $"{baseName}.unity");
                }
                
                // Create new scene
                Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                
                // Set scene path for saving
                if (!string.IsNullOrEmpty(targetScenePath))
                {
                    EditorSceneManager.SaveScene(newScene, targetScenePath);
                    newScene = EditorSceneManager.OpenScene(targetScenePath);
                }
                
                // Import scene settings
                ImportSceneSettings(sceneState.settings);
                
                // Track import results
                var importLog = new StringBuilder();
                importLog.AppendLine($"=== Scene Import Log ===");
                importLog.AppendLine($"Source State File: {stateFilePath}");
                importLog.AppendLine($"Target Scene: {targetScenePath}");
                importLog.AppendLine($"Import Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                importLog.AppendLine();
                
                // Import GameObjects
                var gameObjectMap = new Dictionary<string, GameObject>();
                int importedCount = 0;
                int skippedCount = 0;
                
                // First pass: Create all GameObjects without parenting
                foreach (var gameObjectInfo in sceneState.gameObjects)
                {
                    try
                    {
                        GameObject gameObject = ImportGameObject(gameObjectInfo, importLog);
                        if (gameObject != null)
                        {
                            gameObjectMap[gameObjectInfo.hierarchyPath] = gameObject;
                            importedCount++;
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }
                    catch (Exception e)
                    {
                        importLog.AppendLine($"❌ Failed to import GameObject '{gameObjectInfo.name}': {e.Message}");
                        skippedCount++;
                    }
                }
                
                // Second pass: Set up hierarchy relationships
                foreach (var gameObjectInfo in sceneState.gameObjects)
                {
                    if (gameObjectMap.ContainsKey(gameObjectInfo.hierarchyPath) && 
                        !string.IsNullOrEmpty(gameObjectInfo.parentPath) &&
                        gameObjectMap.ContainsKey(gameObjectInfo.parentPath))
                    {
                        GameObject child = gameObjectMap[gameObjectInfo.hierarchyPath];
                        GameObject parent = gameObjectMap[gameObjectInfo.parentPath];
                        
                        child.transform.SetParent(parent.transform);
                        child.transform.SetSiblingIndex(gameObjectInfo.siblingIndex);
                    }
                }
                
                // Third pass: Apply transforms after hierarchy is established
                foreach (var gameObjectInfo in sceneState.gameObjects)
                {
                    if (gameObjectMap.ContainsKey(gameObjectInfo.hierarchyPath))
                    {
                        GameObject gameObject = gameObjectMap[gameObjectInfo.hierarchyPath];
                        ApplyTransform(gameObject.transform, gameObjectInfo.transform);
                    }
                }
                
                // Generate import report
                importLog.AppendLine($"=== Import Results ===");
                importLog.AppendLine($"Total GameObjects in State: {sceneState.gameObjects.Length}");
                importLog.AppendLine($"Successfully Imported: {importedCount}");
                importLog.AppendLine($"Skipped/Failed: {skippedCount}");
                importLog.AppendLine($"Success Rate: {(float)importedCount / sceneState.gameObjects.Length * 100:F1}%");
                
                // Save import log
                SaveImportLog(stateFilePath, importLog);
                
                // Mark scene as dirty and save
                EditorSceneManager.MarkSceneDirty(newScene);
                EditorSceneManager.SaveScene(newScene);
                
                Debug.Log($"[VibeUnitySceneImporter] ✅ Scene imported successfully: {targetScenePath}");
                Debug.Log($"[VibeUnitySceneImporter] Import rate: {(float)importedCount / sceneState.gameObjects.Length * 100:F1}% " +
                         $"({importedCount}/{sceneState.gameObjects.Length} GameObjects)");
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnitySceneImporter] Failed to import scene: {e.Message}");
                Debug.LogError($"[VibeUnitySceneImporter] Stack trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Imports scene-level settings
        /// </summary>
        private static void ImportSceneSettings(SceneSettings settings)
        {
            if (settings == null) return;
            
            try
            {
                // Import lighting settings
                if (settings.lighting != null)
                {
                    Lightmapping.realtimeGI = settings.lighting.realtimeGI;
                    Lightmapping.bakedGI = settings.lighting.bakedGI;
                    
                    if (settings.lighting.ambientColor != null && settings.lighting.ambientColor.Length >= 4)
                    {
                        UnityEngine.RenderSettings.ambientLight = new Color(
                            settings.lighting.ambientColor[0],
                            settings.lighting.ambientColor[1],
                            settings.lighting.ambientColor[2],
                            settings.lighting.ambientColor[3]);
                    }
                    
                    if (Enum.TryParse<UnityEngine.Rendering.AmbientMode>(settings.lighting.ambientMode, out var ambientMode))
                    {
                        UnityEngine.RenderSettings.ambientMode = ambientMode;
                    }
                    
                    UnityEngine.RenderSettings.ambientIntensity = settings.lighting.ambientIntensity;
                }
                
                // Import render settings
                if (settings.render != null)
                {
                    UnityEngine.RenderSettings.fog = settings.render.fog;
                    
                    if (settings.render.fogColor != null && settings.render.fogColor.Length >= 4)
                    {
                        UnityEngine.RenderSettings.fogColor = new Color(
                            settings.render.fogColor[0],
                            settings.render.fogColor[1],
                            settings.render.fogColor[2],
                            settings.render.fogColor[3]);
                    }
                    
                    if (Enum.TryParse<FogMode>(settings.render.fogMode, out var fogMode))
                    {
                        UnityEngine.RenderSettings.fogMode = fogMode;
                    }
                    
                    UnityEngine.RenderSettings.fogDensity = settings.render.fogDensity;
                    UnityEngine.RenderSettings.fogStartDistance = settings.render.fogStartDistance;
                    UnityEngine.RenderSettings.fogEndDistance = settings.render.fogEndDistance;
                    
                    // Load skybox material if specified
                    if (!string.IsNullOrEmpty(settings.render.skyboxMaterial))
                    {
                        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(settings.render.skyboxMaterial);
                        if (skybox != null)
                        {
                            UnityEngine.RenderSettings.skybox = skybox;
                        }
                    }
                }
                
                // Import physics settings
                if (settings.physics != null)
                {
                    if (settings.physics.gravity != null && settings.physics.gravity.Length >= 3)
                    {
                        Physics.gravity = new Vector3(
                            settings.physics.gravity[0],
                            settings.physics.gravity[1],
                            settings.physics.gravity[2]);
                    }
                    
                    Physics.bounceThreshold = settings.physics.bounceThreshold;
                    Physics.defaultSolverIterations = settings.physics.defaultSolverIterations;
                    Physics.defaultSolverVelocityIterations = settings.physics.defaultSolverVelocityIterations;
                }
                
                // Import audio settings
                if (settings.audio != null)
                {
                    AudioListener.volume = settings.audio.volume;
                    // Note: Doppler factor is set per AudioSource, not globally on AudioListener
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[VibeUnitySceneImporter] Failed to import some scene settings: {e.Message}");
            }
        }
        
        /// <summary>
        /// Imports a single GameObject from GameObjectInfo
        /// </summary>
        private static GameObject ImportGameObject(GameObjectInfo gameObjectInfo, StringBuilder importLog)
        {
            try
            {
                // Create GameObject
                GameObject gameObject = new GameObject(gameObjectInfo.name);
                
                // Set basic properties
                gameObject.SetActive(gameObjectInfo.isActive);
                gameObject.tag = gameObjectInfo.tag;
                gameObject.layer = gameObjectInfo.layer;
                gameObject.isStatic = gameObjectInfo.isStatic;
                
                importLog.AppendLine($"✅ Created GameObject: {gameObjectInfo.name}");
                
                // Import components
                int componentCount = 0;
                int skippedComponents = 0;
                
                foreach (var componentInfo in gameObjectInfo.components)
                {
                    try
                    {
                        if (ImportComponent(gameObject, componentInfo, importLog))
                        {
                            componentCount++;
                        }
                        else
                        {
                            skippedComponents++;
                        }
                    }
                    catch (Exception e)
                    {
                        importLog.AppendLine($"   ❌ Failed to import component {componentInfo.typeName}: {e.Message}");
                        skippedComponents++;
                    }
                }
                
                // Apply UI-specific settings if present
                if (gameObjectInfo.uiInfo != null)
                {
                    ApplyUIElementInfo(gameObject, gameObjectInfo.uiInfo, importLog);
                }
                
                importLog.AppendLine($"   └─ Components: {componentCount} imported, {skippedComponents} skipped");
                
                return gameObject;
            }
            catch (Exception e)
            {
                importLog.AppendLine($"❌ Failed to create GameObject '{gameObjectInfo.name}': {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Imports a component and applies its properties
        /// </summary>
        private static bool ImportComponent(GameObject gameObject, ComponentInfo componentInfo, StringBuilder importLog)
        {
            try
            {
                // Skip Transform component as it's automatically added
                if (componentInfo.typeName == "Transform")
                {
                    return true;
                }
                
                // Handle RectTransform specially
                if (componentInfo.typeName == "RectTransform")
                {
                    // Remove existing Transform and add RectTransform
                    if (gameObject.GetComponent<Transform>() != null && !(gameObject.GetComponent<Transform>() is RectTransform))
                    {
                        UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<Transform>());
                    }
                    
                    if (gameObject.GetComponent<RectTransform>() == null)
                    {
                        gameObject.AddComponent<RectTransform>();
                    }
                    
                    return true;
                }
                
                // Try to add the component using the existing VibeUnity system
                Component component = VibeUnityGameObjects.AddComponent(gameObject, componentInfo.typeName);
                
                if (component == null)
                {
                    importLog.AppendLine($"   ⚠️ Component {componentInfo.typeName} not supported - skipping");
                    return false;
                }
                
                // Set component enabled state
                SetComponentEnabledState(component, componentInfo.enabled);
                
                // Apply component properties
                if (componentInfo.properties != null && componentInfo.properties.Length > 0)
                {
                    int appliedProperties = 0;
                    foreach (var property in componentInfo.properties)
                    {
                        if (ApplyComponentProperty(component, property, importLog))
                        {
                            appliedProperties++;
                        }
                    }
                    
                    if (appliedProperties > 0)
                    {
                        importLog.AppendLine($"   └─ {componentInfo.typeName}: {appliedProperties}/{componentInfo.properties.Length} properties applied");
                    }
                }
                
                return true;
            }
            catch (Exception e)
            {
                importLog.AppendLine($"   ❌ Exception importing component {componentInfo.typeName}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Sets the enabled state of a component
        /// </summary>
        private static void SetComponentEnabledState(Component component, bool enabled)
        {
            try
            {
                if (component is Behaviour behaviour)
                    behaviour.enabled = enabled;
                else if (component is Collider collider)
                    collider.enabled = enabled;
                else if (component is Renderer renderer)
                    renderer.enabled = enabled;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[VibeUnitySceneImporter] Failed to set enabled state for {component.GetType().Name}: {e.Message}");
            }
        }
        
        /// <summary>
        /// Applies a single component property
        /// </summary>
        private static bool ApplyComponentProperty(Component component, ComponentProperty property, StringBuilder importLog)
        {
            try
            {
                Type componentType = component.GetType();
                
                // Try to find field first
                FieldInfo field = componentType.GetField(property.name, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    object value = ConvertStringToValue(property.value, field.FieldType, property);
                    if (value != null)
                    {
                        field.SetValue(component, value);
                        return true;
                    }
                }
                
                // Try to find property
                PropertyInfo prop = componentType.GetProperty(property.name, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    object value = ConvertStringToValue(property.value, prop.PropertyType, property);
                    if (value != null)
                    {
                        prop.SetValue(component, value);
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception e)
            {
                importLog.AppendLine($"      ⚠️ Failed to apply property {property.name}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Converts string values back to their original types
        /// </summary>
        private static object ConvertStringToValue(string stringValue, Type targetType, ComponentProperty property)
        {
            try
            {
                if (string.IsNullOrEmpty(stringValue) || stringValue == "null")
                {
                    return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                }
                
                // Handle Unity Object references (assets)
                if (property.isAssetReference && !string.IsNullOrEmpty(property.assetPath))
                {
                    if (targetType.IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        return AssetDatabase.LoadAssetAtPath(property.assetPath, targetType);
                    }
                }
                
                // Handle primitive types
                if (targetType == typeof(int))
                    return int.Parse(stringValue);
                if (targetType == typeof(float))
                    return float.Parse(stringValue);
                if (targetType == typeof(double))
                    return double.Parse(stringValue);
                if (targetType == typeof(bool))
                    return bool.Parse(stringValue);
                if (targetType == typeof(string))
                    return stringValue;
                    
                // Handle Unity vector types
                if (targetType == typeof(Vector2))
                    return ParseVector2(stringValue);
                if (targetType == typeof(Vector3))
                    return ParseVector3(stringValue);
                if (targetType == typeof(Vector4))
                    return ParseVector4(stringValue);
                if (targetType == typeof(Quaternion))
                    return ParseQuaternion(stringValue);
                if (targetType == typeof(Color))
                    return ParseColor(stringValue);
                if (targetType == typeof(Rect))
                    return ParseRect(stringValue);
                    
                // Handle enums
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, stringValue);
                }
                
                // Fallback: try direct conversion
                return Convert.ChangeType(stringValue, targetType);
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        /// <summary>
        /// Applies transform information to a transform component
        /// </summary>
        private static void ApplyTransform(Transform transform, TransformInfo transformInfo)
        {
            if (transformInfo == null) return;
            
            try
            {
                if (transformInfo.localPosition != null && transformInfo.localPosition.Length >= 3)
                {
                    transform.localPosition = new Vector3(
                        transformInfo.localPosition[0],
                        transformInfo.localPosition[1],
                        transformInfo.localPosition[2]);
                }
                
                if (transformInfo.localRotation != null && transformInfo.localRotation.Length >= 4)
                {
                    transform.localRotation = new Quaternion(
                        transformInfo.localRotation[0],
                        transformInfo.localRotation[1],
                        transformInfo.localRotation[2],
                        transformInfo.localRotation[3]);
                }
                
                if (transformInfo.localScale != null && transformInfo.localScale.Length >= 3)
                {
                    transform.localScale = new Vector3(
                        transformInfo.localScale[0],
                        transformInfo.localScale[1],
                        transformInfo.localScale[2]);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[VibeUnitySceneImporter] Failed to apply transform: {e.Message}");
            }
        }
        
        /// <summary>
        /// Applies UI-specific element information
        /// </summary>
        private static void ApplyUIElementInfo(GameObject gameObject, UIElementInfo uiInfo, StringBuilder importLog)
        {
            try
            {
                // Apply RectTransform settings
                if (uiInfo.rectTransform != null)
                {
                    RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        if (uiInfo.rectTransform.anchoredPosition != null && uiInfo.rectTransform.anchoredPosition.Length >= 2)
                        {
                            rectTransform.anchoredPosition = new Vector2(
                                uiInfo.rectTransform.anchoredPosition[0],
                                uiInfo.rectTransform.anchoredPosition[1]);
                        }
                        
                        if (uiInfo.rectTransform.anchorMin != null && uiInfo.rectTransform.anchorMin.Length >= 2)
                        {
                            rectTransform.anchorMin = new Vector2(
                                uiInfo.rectTransform.anchorMin[0],
                                uiInfo.rectTransform.anchorMin[1]);
                        }
                        
                        if (uiInfo.rectTransform.anchorMax != null && uiInfo.rectTransform.anchorMax.Length >= 2)
                        {
                            rectTransform.anchorMax = new Vector2(
                                uiInfo.rectTransform.anchorMax[0],
                                uiInfo.rectTransform.anchorMax[1]);
                        }
                        
                        if (uiInfo.rectTransform.pivot != null && uiInfo.rectTransform.pivot.Length >= 2)
                        {
                            rectTransform.pivot = new Vector2(
                                uiInfo.rectTransform.pivot[0],
                                uiInfo.rectTransform.pivot[1]);
                        }
                        
                        if (uiInfo.rectTransform.sizeDelta != null && uiInfo.rectTransform.sizeDelta.Length >= 2)
                        {
                            rectTransform.sizeDelta = new Vector2(
                                uiInfo.rectTransform.sizeDelta[0],
                                uiInfo.rectTransform.sizeDelta[1]);
                        }
                        
                        importLog.AppendLine($"   └─ Applied RectTransform settings");
                    }
                }
                
                // Apply Canvas settings
                if (uiInfo.canvas != null)
                {
                    Canvas canvas = gameObject.GetComponent<Canvas>();
                    if (canvas != null)
                    {
                        if (Enum.TryParse<RenderMode>(uiInfo.canvas.renderMode, out var renderMode))
                        {
                            canvas.renderMode = renderMode;
                        }
                        
                        canvas.planeDistance = uiInfo.canvas.planeDistance;
                        canvas.sortingOrder = uiInfo.canvas.sortingOrder;
                        canvas.sortingLayerName = uiInfo.canvas.sortingLayerName;
                        canvas.overrideSorting = uiInfo.canvas.overrideSorting;
                        
                        importLog.AppendLine($"   └─ Applied Canvas settings");
                    }
                    
                    // Apply CanvasScaler settings if present
                    if (uiInfo.canvas.referenceResolution != null)
                    {
                        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
                        if (scaler != null)
                        {
                            scaler.scaleFactor = uiInfo.canvas.scaleFactor;
                            
                            if (uiInfo.canvas.referenceResolution.Length >= 2)
                            {
                                scaler.referenceResolution = new Vector2(
                                    uiInfo.canvas.referenceResolution[0],
                                    uiInfo.canvas.referenceResolution[1]);
                            }
                            
                            if (Enum.TryParse<CanvasScaler.ScaleMode>(uiInfo.canvas.scaleMode, out var scaleMode))
                            {
                                scaler.uiScaleMode = scaleMode;
                            }
                            
                            scaler.matchWidthOrHeight = uiInfo.canvas.matchWidthOrHeight;
                            
                            importLog.AppendLine($"   └─ Applied CanvasScaler settings");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                importLog.AppendLine($"   ⚠️ Failed to apply UI settings: {e.Message}");
            }
        }
        
        /// <summary>
        /// Saves the import log to a file
        /// </summary>
        private static void SaveImportLog(string stateFilePath, StringBuilder importLog)
        {
            try
            {
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                string importDir = Path.Combine(projectRoot, ".vibe-unity", "commands", "import-logs");
                if (!Directory.Exists(importDir))
                {
                    Directory.CreateDirectory(importDir);
                }
                
                string fileName = Path.GetFileNameWithoutExtension(stateFilePath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string logPath = Path.Combine(importDir, $"{timestamp}-{fileName}-import.log");
                
                importLog.AppendLine();
                importLog.AppendLine($"=== Import Complete ===");
                importLog.AppendLine($"Log saved: {logPath}");
                
                File.WriteAllText(logPath, importLog.ToString());
                Debug.Log($"[VibeUnitySceneImporter] Import log saved: {logPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnitySceneImporter] Failed to save import log: {e.Message}");
            }
        }
        
        #region Parsing Helper Methods
        
        private static Vector2 ParseVector2(string value)
        {
            string[] parts = value.Split(',');
            if (parts.Length >= 2)
            {
                return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }
            return Vector2.zero;
        }
        
        private static Vector3 ParseVector3(string value)
        {
            string[] parts = value.Split(',');
            if (parts.Length >= 3)
            {
                return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }
            return Vector3.zero;
        }
        
        private static Vector4 ParseVector4(string value)
        {
            string[] parts = value.Split(',');
            if (parts.Length >= 4)
            {
                return new Vector4(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            }
            return Vector4.zero;
        }
        
        private static Quaternion ParseQuaternion(string value)
        {
            string[] parts = value.Split(',');
            if (parts.Length >= 4)
            {
                return new Quaternion(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            }
            return Quaternion.identity;
        }
        
        private static Color ParseColor(string value)
        {
            string[] parts = value.Split(',');
            if (parts.Length >= 4)
            {
                return new Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            }
            return Color.white;
        }
        
        private static Rect ParseRect(string value)
        {
            string[] parts = value.Split(',');
            if (parts.Length >= 4)
            {
                return new Rect(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            }
            return new Rect();
        }
        
        #endregion
    }
}
#endif