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
    /// Exports Unity scenes to comprehensive JSON state files with gap detection
    /// </summary>
    public static class VibeUnitySceneExporter
    {
        private static readonly HashSet<string> SupportedComponents = new HashSet<string>
        {
            // Transform and basic components
            "Transform", "RectTransform",
            
            // UI Components (currently supported by VibeUnity)
            "Canvas", "CanvasScaler", "GraphicRaycaster",
            "Image", "Text", "Button", "ScrollRect", "Scrollbar",
            "TextMeshPro", "TextMeshProUGUI",
            
            // Basic 3D components
            "MeshRenderer", "MeshFilter",
            
            // Camera and Lighting
            "Camera", "Light",
            
            // Audio
            "AudioSource", "AudioListener"
        };
        
        private static readonly HashSet<string> PartiallySupported = new HashSet<string>
        {
            // Components we support but might be missing some properties
            "Rigidbody", "Collider", "BoxCollider", "SphereCollider"
        };
        
        /// <summary>
        /// Exports the active scene to a JSON state file
        /// </summary>
        /// <param name="scenePath">Optional specific scene path to export</param>
        /// <returns>True if export was successful</returns>
        public static bool ExportActiveSceneState(string scenePath = null)
        {
            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (!activeScene.IsValid())
                {
                    Debug.LogError("[VibeUnitySceneExporter] No active scene to export");
                    return false;
                }
                
                string outputPath;
                if (string.IsNullOrEmpty(scenePath))
                {
                    // Generate output path next to scene file
                    string sceneAssetPath = activeScene.path;
                    if (string.IsNullOrEmpty(sceneAssetPath))
                    {
                        Debug.LogError("[VibeUnitySceneExporter] Scene has no saved path. Save the scene first.");
                        return false;
                    }
                    
                    string directory = Path.GetDirectoryName(sceneAssetPath);
                    string sceneName = Path.GetFileNameWithoutExtension(sceneAssetPath);
                    outputPath = Path.Combine(directory, $"{sceneName}.state.json");
                }
                else
                {
                    outputPath = scenePath;
                }
                
                // Export the scene state
                SceneState sceneState = ExportSceneToState(activeScene);
                
                // Serialize to JSON
                string jsonContent = JsonUtility.ToJson(sceneState, true);
                
                // Write to file
                File.WriteAllText(outputPath, jsonContent);
                
                // Generate gap analysis report
                GenerateGapAnalysisReport(sceneState, outputPath);
                
                Debug.Log($"[VibeUnitySceneExporter] ✅ Scene state exported: {outputPath}");
                Debug.Log($"[VibeUnitySceneExporter] Coverage: {sceneState.coverageReport.summary.coveragePercentage:F1}% " +
                         $"({sceneState.coverageReport.summary.supportedComponents}/{sceneState.coverageReport.summary.totalComponents} components)");
                
                // Refresh asset database to show new file
                AssetDatabase.Refresh();
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnitySceneExporter] Failed to export scene state: {e.Message}");
                Debug.LogError($"[VibeUnitySceneExporter] Stack trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Exports a scene to a SceneState object
        /// </summary>
        private static SceneState ExportSceneToState(Scene scene)
        {
            var sceneState = new SceneState();
            sceneState.exportTime = DateTime.Now;
            
            // Export metadata
            sceneState.metadata = ExportSceneMetadata(scene);
            
            // Export scene settings
            sceneState.settings = ExportSceneSettings();
            
            // Export all GameObjects
            var gameObjectsList = new List<GameObjectInfo>();
            var assetReferences = new HashSet<string>();
            var coverageData = new CoverageAnalysisData();
            
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                ExportGameObjectRecursive(rootObj, "", gameObjectsList, assetReferences, coverageData);
            }
            
            sceneState.gameObjects = gameObjectsList.ToArray();
            sceneState.assetReferences = assetReferences.ToArray();
            
            // Generate coverage report
            sceneState.coverageReport = GenerateCoverageReport(coverageData, gameObjectsList.Count);
            
            return sceneState;
        }
        
        /// <summary>
        /// Exports scene metadata
        /// </summary>
        private static SceneMetadata ExportSceneMetadata(Scene scene)
        {
            return new SceneMetadata
            {
                sceneName = scene.name,
                scenePath = scene.path,
                unityVersion = Application.unityVersion,
                exportTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                totalGameObjects = scene.rootCount,
                renderPipelines = GetRenderPipelineInfo()
            };
        }
        
        /// <summary>
        /// Gets render pipeline information
        /// </summary>
        private static string[] GetRenderPipelineInfo()
        {
            var pipelines = new List<string>();
            
            if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
            {
                string rpName = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.GetType().Name;
                if (rpName.Contains("Universal"))
                    pipelines.Add("URP");
                else if (rpName.Contains("HDRP") || rpName.Contains("HighDefinition"))
                    pipelines.Add("HDRP");
                else
                    pipelines.Add("Custom");
            }
            else
            {
                pipelines.Add("Built-in");
            }
            
            return pipelines.ToArray();
        }
        
        /// <summary>
        /// Exports scene-level settings
        /// </summary>
        private static SceneSettings ExportSceneSettings()
        {
            return new SceneSettings
            {
                lighting = new LightingSettings
                {
                    realtimeGI = Lightmapping.realtimeGI,
                    bakedGI = Lightmapping.bakedGI,
                    ambientColor = ColorToFloatArray(UnityEngine.RenderSettings.ambientLight),
                    ambientMode = UnityEngine.RenderSettings.ambientMode.ToString(),
                    ambientIntensity = UnityEngine.RenderSettings.ambientIntensity
                },
                render = new VibeUnity.Editor.RenderSettings
                {
                    fog = UnityEngine.RenderSettings.fog,
                    fogColor = ColorToFloatArray(UnityEngine.RenderSettings.fogColor),
                    fogMode = UnityEngine.RenderSettings.fogMode.ToString(),
                    fogDensity = UnityEngine.RenderSettings.fogDensity,
                    fogStartDistance = UnityEngine.RenderSettings.fogStartDistance,
                    fogEndDistance = UnityEngine.RenderSettings.fogEndDistance,
                    skyboxMaterial = AssetDatabase.GetAssetPath(UnityEngine.RenderSettings.skybox)
                },
                physics = new PhysicsSettings
                {
                    gravity = Vector3ToFloatArray(Physics.gravity),
                    bounceThreshold = Physics.bounceThreshold,
                    defaultSolverIterations = Physics.defaultSolverIterations,
                    defaultSolverVelocityIterations = Physics.defaultSolverVelocityIterations
                },
                audio = new AudioSettings
                {
                    volume = AudioListener.volume,
                    dopplerFactor = 1.0f, // Default Doppler factor
                    speedOfSound = 343.0f // Unity's default speed of sound
                }
            };
        }
        
        /// <summary>
        /// Recursively exports a GameObject and its children
        /// </summary>
        private static void ExportGameObjectRecursive(GameObject gameObject, string parentPath, 
            List<GameObjectInfo> gameObjectsList, HashSet<string> assetReferences, CoverageAnalysisData coverageData)
        {
            string currentPath = string.IsNullOrEmpty(parentPath) ? gameObject.name : $"{parentPath}/{gameObject.name}";
            
            var gameObjectInfo = new GameObjectInfo
            {
                name = gameObject.name,
                hierarchyPath = currentPath,
                instanceId = gameObject.GetInstanceID(),
                isActive = gameObject.activeInHierarchy,
                tag = gameObject.tag,
                layer = gameObject.layer,
                isStatic = gameObject.isStatic,
                parentPath = parentPath,
                siblingIndex = gameObject.transform.GetSiblingIndex()
            };
            
            // Export transform
            gameObjectInfo.transform = ExportTransform(gameObject.transform);
            
            // Export children paths
            var childrenPaths = new List<string>();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform child = gameObject.transform.GetChild(i);
                childrenPaths.Add($"{currentPath}/{child.name}");
            }
            gameObjectInfo.childrenPaths = childrenPaths.ToArray();
            
            // Export all components
            Component[] components = gameObject.GetComponents<Component>();
            var componentInfos = new List<ComponentInfo>();
            
            foreach (Component component in components)
            {
                if (component != null) // Null check for missing script components
                {
                    var componentInfo = ExportComponent(component, assetReferences, coverageData);
                    componentInfos.Add(componentInfo);
                }
                else
                {
                    // Handle missing script components
                    coverageData.AddGap("Component", "Critical", "MissingScript", 
                        "Missing script component detected", 1, new[] { currentPath },
                        "Restore missing script or remove component reference");
                }
            }
            
            gameObjectInfo.components = componentInfos.ToArray();
            
            // Export UI-specific information if this is a UI element
            if (gameObject.GetComponent<RectTransform>() != null)
            {
                gameObjectInfo.uiInfo = ExportUIElementInfo(gameObject, assetReferences);
            }
            
            gameObjectsList.Add(gameObjectInfo);
            
            // Recursively export children
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform child = gameObject.transform.GetChild(i);
                ExportGameObjectRecursive(child.gameObject, currentPath, gameObjectsList, assetReferences, coverageData);
            }
        }
        
        /// <summary>
        /// Exports transform information
        /// </summary>
        private static TransformInfo ExportTransform(Transform transform)
        {
            return new TransformInfo
            {
                localPosition = Vector3ToFloatArray(transform.localPosition),
                localRotation = QuaternionToFloatArray(transform.localRotation),
                localScale = Vector3ToFloatArray(transform.localScale),
                worldPosition = Vector3ToFloatArray(transform.position),
                worldRotation = QuaternionToFloatArray(transform.rotation),
                worldScale = Vector3ToFloatArray(transform.lossyScale)
            };
        }
        
        /// <summary>
        /// Exports a component with coverage analysis
        /// </summary>
        private static ComponentInfo ExportComponent(Component component, HashSet<string> assetReferences, CoverageAnalysisData coverageData)
        {
            Type componentType = component.GetType();
            string typeName = componentType.Name;
            string fullTypeName = componentType.FullName;
            
            var componentInfo = new ComponentInfo
            {
                typeName = typeName,
                fullTypeName = fullTypeName,
                enabled = GetComponentEnabledState(component)
            };
            
            // Check if component is supported
            bool isSupported = SupportedComponents.Contains(typeName);
            bool isPartiallySupported = PartiallySupported.Contains(typeName);
            
            componentInfo.isSupported = isSupported;
            
            // Track coverage
            coverageData.TrackComponent(typeName, isSupported, isPartiallySupported);
            
            // Export component properties
            var properties = new List<ComponentProperty>();
            var missingFeatures = new List<string>();
            
            ExportComponentProperties(component, properties, assetReferences, missingFeatures);
            
            componentInfo.properties = properties.ToArray();
            componentInfo.missingFeatures = missingFeatures.ToArray();
            
            // Log gaps for unsupported components
            if (!isSupported && !isPartiallySupported)
            {
                coverageData.AddGap("Component", "Warning", typeName,
                    $"Component type '{typeName}' is not supported for scene rebuilding",
                    1, new[] { GetGameObjectPath(component.gameObject) },
                    $"Add support for {typeName} component in VibeUnity package");
            }
            else if (isPartiallySupported && missingFeatures.Count > 0)
            {
                foreach (string feature in missingFeatures)
                {
                    coverageData.AddGap("Property", "Info", typeName,
                        $"Property '{feature}' not fully supported on {typeName}",
                        1, new[] { GetGameObjectPath(component.gameObject) },
                        $"Add property support for {typeName}.{feature}");
                }
            }
            
            return componentInfo;
        }
        
        /// <summary>
        /// Gets the enabled state of a component
        /// </summary>
        private static bool GetComponentEnabledState(Component component)
        {
            // Handle different component types that have enabled properties
            if (component is Behaviour behaviour)
                return behaviour.enabled;
            if (component is Collider collider)
                return collider.enabled;
            if (component is Renderer renderer)
                return renderer.enabled;
                
            return true; // Default for components without enabled state
        }
        
        /// <summary>
        /// Exports component properties using reflection
        /// </summary>
        private static void ExportComponentProperties(Component component, List<ComponentProperty> properties, 
            HashSet<string> assetReferences, List<string> missingFeatures)
        {
            Type componentType = component.GetType();
            
            // Get all serializable fields and properties
            FieldInfo[] fields = componentType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] props = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // Export fields
            foreach (FieldInfo field in fields)
            {
                if (ShouldExportField(field))
                {
                    try
                    {
                        object value = field.GetValue(component);
                        var property = CreateComponentProperty(field.Name, value, field.FieldType, assetReferences);
                        properties.Add(property);
                    }
                    catch (Exception e)
                    {
                        missingFeatures.Add($"Field {field.Name}: {e.Message}");
                    }
                }
            }
            
            // Export properties (read-only for inspection)
            foreach (PropertyInfo prop in props)
            {
                if (ShouldExportProperty(prop))
                {
                    try
                    {
                        object value = prop.GetValue(component);
                        var property = CreateComponentProperty(prop.Name, value, prop.PropertyType, assetReferences);
                        properties.Add(property);
                    }
                    catch (Exception e)
                    {
                        missingFeatures.Add($"Property {prop.Name}: {e.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Determines if a field should be exported
        /// </summary>
        private static bool ShouldExportField(FieldInfo field)
        {
            // Skip Unity internal fields
            if (field.Name.StartsWith("m_"))
                return false;
                
            // Skip non-serializable fields
            if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
                return false;
                
            return true;
        }
        
        /// <summary>
        /// Determines if a property should be exported
        /// </summary>
        private static bool ShouldExportProperty(PropertyInfo property)
        {
            // Only export readable properties
            if (!property.CanRead)
                return false;
                
            // Skip indexer properties
            if (property.GetIndexParameters().Length > 0)
                return false;
                
            // Skip Unity internal properties
            if (property.Name.StartsWith("m_"))
                return false;
                
            return true;
        }
        
        /// <summary>
        /// Creates a ComponentProperty from a field/property value
        /// </summary>
        private static ComponentProperty CreateComponentProperty(string name, object value, Type valueType, HashSet<string> assetReferences)
        {
            var property = new ComponentProperty
            {
                name = name,
                typeName = valueType.Name,
                isSerializable = IsSerializableType(valueType)
            };
            
            if (value == null)
            {
                property.value = "null";
            }
            else if (value is UnityEngine.Object unityObj)
            {
                // Handle Unity object references
                property.isAssetReference = true;
                property.assetPath = AssetDatabase.GetAssetPath(unityObj);
                property.value = unityObj.name;
                
                if (!string.IsNullOrEmpty(property.assetPath))
                {
                    assetReferences.Add(property.assetPath);
                }
            }
            else
            {
                // Convert value to string representation
                property.value = ConvertValueToString(value);
            }
            
            return property;
        }
        
        /// <summary>
        /// Checks if a type is serializable by Unity
        /// </summary>
        private static bool IsSerializableType(Type type)
        {
            if (type.IsPrimitive || type == typeof(string))
                return true;
                
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                return true;
                
            if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) ||
                type == typeof(Quaternion) || type == typeof(Color) || type == typeof(Rect))
                return true;
                
            return type.GetCustomAttribute<SerializableAttribute>() != null;
        }
        
        /// <summary>
        /// Converts various value types to string representations
        /// </summary>
        private static string ConvertValueToString(object value)
        {
            switch (value)
            {
                case Vector2 v2:
                    return $"{v2.x},{v2.y}";
                case Vector3 v3:
                    return $"{v3.x},{v3.y},{v3.z}";
                case Vector4 v4:
                    return $"{v4.x},{v4.y},{v4.z},{v4.w}";
                case Quaternion q:
                    return $"{q.x},{q.y},{q.z},{q.w}";
                case Color c:
                    return $"{c.r},{c.g},{c.b},{c.a}";
                case Rect r:
                    return $"{r.x},{r.y},{r.width},{r.height}";
                default:
                    return value.ToString();
            }
        }
        
        /// <summary>
        /// Exports UI-specific element information
        /// </summary>
        private static UIElementInfo ExportUIElementInfo(GameObject gameObject, HashSet<string> assetReferences)
        {
            var uiInfo = new UIElementInfo();
            
            // Export RectTransform
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                uiInfo.rectTransform = new RectTransformInfo
                {
                    anchoredPosition = Vector2ToFloatArray(rectTransform.anchoredPosition),
                    anchorMin = Vector2ToFloatArray(rectTransform.anchorMin),
                    anchorMax = Vector2ToFloatArray(rectTransform.anchorMax),
                    offsetMin = Vector2ToFloatArray(rectTransform.offsetMin),
                    offsetMax = Vector2ToFloatArray(rectTransform.offsetMax),
                    pivot = Vector2ToFloatArray(rectTransform.pivot),
                    sizeDelta = Vector2ToFloatArray(rectTransform.sizeDelta)
                };
            }
            
            // Export Canvas information
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                uiInfo.canvas = new CanvasInfo
                {
                    renderMode = canvas.renderMode.ToString(),
                    planeDistance = canvas.planeDistance,
                    sortingOrder = canvas.sortingOrder,
                    sortingLayerName = canvas.sortingLayerName,
                    overrideSorting = canvas.overrideSorting
                };
                
                CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    uiInfo.canvas.scaleFactor = scaler.scaleFactor;
                    uiInfo.canvas.referenceResolution = Vector2ToFloatArray(scaler.referenceResolution);
                    uiInfo.canvas.scaleMode = scaler.uiScaleMode.ToString();
                    uiInfo.canvas.matchWidthOrHeight = scaler.matchWidthOrHeight;
                }
            }
            
            // Export Graphic information (Image, Text, etc.)
            Graphic graphic = gameObject.GetComponent<Graphic>();
            if (graphic != null)
            {
                uiInfo.graphic = new GraphicInfo
                {
                    graphicType = graphic.GetType().Name,
                    color = ColorToFloatArray(graphic.color),
                    raycastTarget = graphic.raycastTarget
                };
                
                if (graphic.material != null)
                {
                    uiInfo.graphic.material = AssetDatabase.GetAssetPath(graphic.material);
                    assetReferences.Add(uiInfo.graphic.material);
                }
                
                // Handle specific graphic types
                if (graphic is Image image)
                {
                    uiInfo.graphic.imageType = image.type.ToString();
                    uiInfo.graphic.preserveAspect = image.preserveAspect;
                    if (image.sprite != null)
                    {
                        uiInfo.graphic.sprite = AssetDatabase.GetAssetPath(image.sprite);
                        assetReferences.Add(uiInfo.graphic.sprite);
                    }
                }
                else if (graphic is Text text)
                {
                    uiInfo.graphic.text = text.text;
                    uiInfo.graphic.fontSize = text.fontSize;
                    uiInfo.graphic.fontStyle = text.fontStyle.ToString();
                    uiInfo.graphic.alignment = text.alignment.ToString();
                    uiInfo.graphic.richText = text.supportRichText;
                    if (text.font != null)
                    {
                        uiInfo.graphic.font = AssetDatabase.GetAssetPath(text.font);
                        assetReferences.Add(uiInfo.graphic.font);
                    }
                }
                else if (graphic is TextMeshProUGUI tmpText)
                {
                    uiInfo.graphic.text = tmpText.text;
                    uiInfo.graphic.fontSize = (int)tmpText.fontSize;
                    uiInfo.graphic.fontStyle = tmpText.fontStyle.ToString();
                    uiInfo.graphic.alignment = tmpText.alignment.ToString();
                    uiInfo.graphic.richText = tmpText.richText;
                    if (tmpText.font != null)
                    {
                        uiInfo.graphic.font = AssetDatabase.GetAssetPath(tmpText.font);
                        assetReferences.Add(uiInfo.graphic.font);
                    }
                }
            }
            
            return uiInfo;
        }
        
        /// <summary>
        /// Generates a comprehensive coverage report
        /// </summary>
        private static CoverageReport GenerateCoverageReport(CoverageAnalysisData coverageData, int totalGameObjects)
        {
            var report = new CoverageReport();
            
            // Generate summary
            int totalComponents = coverageData.GetTotalComponents();
            int supportedComponents = coverageData.GetSupportedComponents();
            int partiallySupported = coverageData.GetPartiallySupported();
            
            report.summary = new CoverageSummary
            {
                totalGameObjects = totalGameObjects,
                totalComponents = totalComponents,
                supportedComponents = supportedComponents,
                partiallySupported = partiallySupported,
                unsupportedComponents = totalComponents - supportedComponents - partiallySupported,
                coveragePercentage = totalComponents > 0 ? (float)(supportedComponents + partiallySupported) / totalComponents * 100f : 100f,
                canFullyRebuild = coverageData.GetCriticalGaps() == 0
            };
            
            // Generate component coverage details
            report.componentCoverage = coverageData.GetComponentCoverage();
            
            // Generate gaps list
            report.gaps = coverageData.GetGaps();
            
            // Generate recommendations
            report.recommendations = GenerateRecommendations(coverageData);
            
            return report;
        }
        
        /// <summary>
        /// Generates recommendations based on coverage analysis
        /// </summary>
        private static string[] GenerateRecommendations(CoverageAnalysisData coverageData)
        {
            var recommendations = new List<string>();
            
            var gaps = coverageData.GetGaps();
            var componentGaps = gaps.Where(g => g.category == "Component" && g.severity == "Warning").ToArray();
            
            if (componentGaps.Length > 0)
            {
                var topMissing = componentGaps.OrderByDescending(g => g.affectedCount).Take(3);
                recommendations.Add($"Implement support for most common missing components: {string.Join(", ", topMissing.Select(g => g.componentType))}");
            }
            
            int criticalGaps = gaps.Count(g => g.severity == "Critical");
            if (criticalGaps > 0)
            {
                recommendations.Add($"Fix {criticalGaps} critical issues that prevent scene rebuilding");
            }
            
            float coverage = coverageData.GetCoveragePercentage();
            if (coverage < 80f)
            {
                recommendations.Add($"Current coverage is {coverage:F1}%. Focus on most frequently used components to improve coverage.");
            }
            
            return recommendations.ToArray();
        }
        
        /// <summary>
        /// Generates a detailed gap analysis report file
        /// </summary>
        private static void GenerateGapAnalysisReport(SceneState sceneState, string sceneStateFilePath)
        {
            try
            {
                // Create coverage analysis directory
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                string coverageDir = Path.Combine(projectRoot, ".vibe-commands", "coverage-analysis");
                if (!Directory.Exists(coverageDir))
                {
                    Directory.CreateDirectory(coverageDir);
                }
                
                // Generate report file name
                string sceneName = sceneState.metadata.sceneName;
                string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string reportPath = Path.Combine(coverageDir, $"{timestamp}-{sceneName}-gaps.log");
                
                // Generate report content
                var report = new StringBuilder();
                report.AppendLine($"=== Scene Coverage Analysis Report ===");
                report.AppendLine($"Scene: {sceneState.metadata.sceneName}");
                report.AppendLine($"Generated: {sceneState.metadata.exportTimestamp}");
                report.AppendLine($"Unity Version: {sceneState.metadata.unityVersion}");
                report.AppendLine($"State File: {sceneStateFilePath}");
                report.AppendLine();
                
                // Summary
                var summary = sceneState.coverageReport.summary;
                report.AppendLine($"=== COVERAGE SUMMARY ===");
                report.AppendLine($"Total GameObjects: {summary.totalGameObjects}");
                report.AppendLine($"Total Components: {summary.totalComponents}");
                report.AppendLine($"Supported: {summary.supportedComponents} ({(float)summary.supportedComponents/summary.totalComponents*100:F1}%)");
                report.AppendLine($"Partially Supported: {summary.partiallySupported} ({(float)summary.partiallySupported/summary.totalComponents*100:F1}%)");
                report.AppendLine($"Unsupported: {summary.unsupportedComponents} ({(float)summary.unsupportedComponents/summary.totalComponents*100:F1}%)");
                report.AppendLine($"Overall Coverage: {summary.coveragePercentage:F1}%");
                report.AppendLine($"Can Fully Rebuild: {(summary.canFullyRebuild ? "YES" : "NO")}");
                report.AppendLine();
                
                // Component breakdown
                report.AppendLine($"=== COMPONENT BREAKDOWN ===");
                foreach (var componentCov in sceneState.coverageReport.componentCoverage)
                {
                    string status = componentCov.isSupported ? "✅ SUPPORTED" : 
                                   componentCov.supportLevel == "Partial" ? "⚠️ PARTIAL" : "❌ MISSING";
                    report.AppendLine($"{status} {componentCov.componentType} ({componentCov.instanceCount} instances)");
                    
                    if (componentCov.missingProperties?.Length > 0)
                    {
                        report.AppendLine($"   Missing Properties: {string.Join(", ", componentCov.missingProperties)}");
                    }
                    if (componentCov.missingFeatures?.Length > 0)
                    {
                        report.AppendLine($"   Missing Features: {string.Join(", ", componentCov.missingFeatures)}");
                    }
                }
                report.AppendLine();
                
                // Detailed gaps
                report.AppendLine($"=== DETAILED GAPS ===");
                var gapsByCategory = sceneState.coverageReport.gaps.GroupBy(g => g.category);
                foreach (var categoryGroup in gapsByCategory)
                {
                    report.AppendLine($"--- {categoryGroup.Key.ToUpper()} GAPS ---");
                    foreach (var gap in categoryGroup.OrderByDescending(g => g.affectedCount))
                    {
                        string severity = gap.severity == "Critical" ? "🔴 CRITICAL" :
                                         gap.severity == "Warning" ? "🟡 WARNING" : "🔵 INFO";
                        report.AppendLine($"{severity}: {gap.missingItem}");
                        report.AppendLine($"   Description: {gap.description}");
                        report.AppendLine($"   Affected Count: {gap.affectedCount}");
                        report.AppendLine($"   Recommendation: {gap.recommendation}");
                        if (gap.examplePaths?.Length > 0)
                        {
                            report.AppendLine($"   Example Paths: {string.Join(", ", gap.examplePaths.Take(3))}");
                        }
                        report.AppendLine();
                    }
                }
                
                // Recommendations
                if (sceneState.coverageReport.recommendations?.Length > 0)
                {
                    report.AppendLine($"=== RECOMMENDATIONS ===");
                    for (int i = 0; i < sceneState.coverageReport.recommendations.Length; i++)
                    {
                        report.AppendLine($"{i + 1}. {sceneState.coverageReport.recommendations[i]}");
                    }
                    report.AppendLine();
                }
                
                report.AppendLine($"=== END REPORT ===");
                
                // Write report file
                File.WriteAllText(reportPath, report.ToString());
                Debug.Log($"[VibeUnitySceneExporter] Gap analysis report saved: {reportPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnitySceneExporter] Failed to generate gap analysis report: {e.Message}");
            }
        }
        
        #region Helper Methods
        
        private static float[] ColorToFloatArray(Color color)
        {
            return new float[] { color.r, color.g, color.b, color.a };
        }
        
        private static float[] Vector2ToFloatArray(Vector2 vector)
        {
            return new float[] { vector.x, vector.y };
        }
        
        private static float[] Vector3ToFloatArray(Vector3 vector)
        {
            return new float[] { vector.x, vector.y, vector.z };
        }
        
        private static float[] QuaternionToFloatArray(Quaternion quaternion)
        {
            return new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        }
        
        private static string GetGameObjectPath(GameObject gameObject)
        {
            return VibeUnityGameObjects.GetPath(gameObject);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Internal class for tracking coverage analysis during export
    /// </summary>
    internal class CoverageAnalysisData
    {
        private Dictionary<string, ComponentCoverageInfo> componentCoverage = new Dictionary<string, ComponentCoverageInfo>();
        private List<Gap> gaps = new List<Gap>();
        
        public void TrackComponent(string componentType, bool isSupported, bool isPartiallySupported)
        {
            if (!componentCoverage.ContainsKey(componentType))
            {
                componentCoverage[componentType] = new ComponentCoverageInfo
                {
                    componentType = componentType,
                    isSupported = isSupported,
                    supportLevel = isSupported ? "Full" : isPartiallySupported ? "Partial" : "None"
                };
            }
            
            componentCoverage[componentType].instanceCount++;
        }
        
        public void AddGap(string category, string severity, string componentType, string description, 
            int affectedCount, string[] examplePaths, string recommendation)
        {
            gaps.Add(new Gap
            {
                category = category,
                severity = severity,
                componentType = componentType,
                missingItem = componentType,
                description = description,
                affectedCount = affectedCount,
                examplePaths = examplePaths,
                recommendation = recommendation
            });
        }
        
        public int GetTotalComponents()
        {
            return componentCoverage.Values.Sum(c => c.instanceCount);
        }
        
        public int GetSupportedComponents()
        {
            return componentCoverage.Values.Where(c => c.supportLevel == "Full").Sum(c => c.instanceCount);
        }
        
        public int GetPartiallySupported()
        {
            return componentCoverage.Values.Where(c => c.supportLevel == "Partial").Sum(c => c.instanceCount);
        }
        
        public int GetCriticalGaps()
        {
            return gaps.Count(g => g.severity == "Critical");
        }
        
        public float GetCoveragePercentage()
        {
            int total = GetTotalComponents();
            if (total == 0) return 100f;
            return (float)(GetSupportedComponents() + GetPartiallySupported()) / total * 100f;
        }
        
        public ComponentCoverage[] GetComponentCoverage()
        {
            return componentCoverage.Values.Select(c => new ComponentCoverage
            {
                componentType = c.componentType,
                instanceCount = c.instanceCount,
                isSupported = c.isSupported,
                supportLevel = c.supportLevel
            }).ToArray();
        }
        
        public Gap[] GetGaps()
        {
            return gaps.ToArray();
        }
    }
    
    internal class ComponentCoverageInfo
    {
        public string componentType;
        public int instanceCount;
        public bool isSupported;
        public string supportLevel;
    }
}
#endif