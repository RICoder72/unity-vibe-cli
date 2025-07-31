#region File Documentation
/// <summary>
/// VIBEUNITYCLI.CS - Command Line Interface Tools for Unity Development
/// 
/// PURPOSE:
/// Provides CLI-based tools for Unity development workflow automation including scene creation,
/// canvas management, and project structure operations. Designed for rapid development workflows
/// and integration with external automation tools and scripts.
/// 
/// KEY FEATURES:
/// • CLI-based scene creation using Unity's built-in scene templates
/// • Canvas creation and management with configurable parameters
/// • Scene type discovery and listing functionality
/// • Project structure analysis and export capabilities
/// • Command-line driven workflow for automation and scripting
/// • Support for Unity's NewSceneSetup modes (Empty, DefaultGameObjects, etc.)
/// • Integration with Unity's scene template system for consistent setups
/// 
/// CLI USAGE DOCUMENTATION:
/// ========================
/// 
/// SCENE CREATION:
/// ---------------
/// Create scenes using Unity's built-in scene templates:
/// 
/// // Create empty scene
/// VibeUnity.CLI.CreateScene("MyScene", "Assets/Scenes", "Empty");
/// 
/// // Create scene with default GameObjects
/// VibeUnity.CLI.CreateScene("GameScene", "Assets/Scenes/Game", "DefaultGameObjects");
/// 
/// // Create scene with specific template
/// VibeUnity.CLI.CreateScene("UIScene", "Assets/Scenes/UI", "2D");
/// 
/// Available Scene Types (use --listtypes to see current install):
/// • Empty - Completely empty scene
/// • DefaultGameObjects - Scene with Camera and Light
/// • 2D - 2D optimized scene setup
/// • 3D - 3D optimized scene setup
/// • URP - Universal Render Pipeline scene
/// • HDRP - High Definition Render Pipeline scene
/// • VR - Virtual Reality scene setup
/// • AR - Augmented Reality scene setup
/// 
/// LIST SCENE TYPES:
/// -----------------
/// Get available scene types for current Unity installation:
/// 
/// VibeUnity.CLI.ListSceneTypes();
/// // Output: Available scene types: Empty, DefaultGameObjects, 2D, 3D, URP, HDRP
/// 
/// CANVAS CREATION:
/// ----------------
/// Add canvas to existing scene with configurable parameters:
/// 
/// // Basic canvas
/// VibeUnity.CLI.AddCanvas("MyCanvas", "ScreenSpaceOverlay");
/// 
/// // Canvas with custom settings
/// VibeUnity.CLI.AddCanvas("UICanvas", "ScreenSpaceOverlay", 1920, 1080, "ScaleWithScreenSize");
/// 
/// // World space canvas
/// VibeUnity.CLI.AddCanvas("WorldCanvas", "WorldSpace", 100, 100);
/// 
/// Canvas Parameters:
/// • canvasName: Name for the canvas GameObject
/// • renderMode: "ScreenSpaceOverlay", "ScreenSpaceCamera", "WorldSpace"
/// • referenceWidth: Reference resolution width (default: 1920)
/// • referenceHeight: Reference resolution height (default: 1080)
/// • scaleMode: "ConstantPixelSize", "ScaleWithScreenSize", "ConstantPhysicalSize"
/// • sortingOrder: Canvas sorting order (default: 0)
/// • worldPosition: Position for WorldSpace canvas (Vector3, default: Vector3.zero)
/// 
/// INTEGRATION EXAMPLES:
/// =====================
/// 
/// BATCH SCENE CREATION:
/// foreach(string sceneName in sceneList) {
///     VibeUnity.CLI.CreateScene(sceneName, basePath, "DefaultGameObjects");
/// }
/// 
/// AUTOMATED SETUP:
/// VibeUnity.CLI.CreateScene("MainMenu", "Assets/Scenes/UI", "2D");
/// VibeUnity.CLI.AddCanvas("MenuCanvas", "ScreenSpaceOverlay", 1920, 1080, "ScaleWithScreenSize");
/// 
/// EXTERNAL SCRIPT INTEGRATION:
/// // Call from external tools or build scripts
/// var cliType = System.Type.GetType("VibeUnity.Editor.CLI");
/// var createMethod = cliType.GetMethod("CreateScene");
/// createMethod.Invoke(null, new object[] { "TestScene", "Assets/Testing", "Empty" });
/// 
/// ERROR HANDLING:
/// ===============
/// All methods return boolean success indicators and log detailed error messages.
/// Check Unity Console for detailed operation logs and error information.
/// 
/// ARCHITECTURE NOTES:
/// • Static Methods: All functionality accessible without instantiation
/// • Editor-Only: #if UNITY_EDITOR compilation prevents inclusion in builds
/// • Unity Integration: Uses Unity's native APIs for maximum compatibility  
/// • Error Resilient: Comprehensive validation and error handling
/// • Logging: Detailed console output for operation tracking
/// • Extensible: Easy to add new CLI commands and functionality
/// </summary>
#endregion

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VibeUnity.Editor
{
    /// <summary>
    /// Data structure for batch command JSON files
    /// </summary>
    [System.Serializable]
    public class BatchCommandFile
    {
        public string version = "1.0";
        public string description = "";
        public SceneConfig scene;
        public BatchCommand[] commands;
    }
    
    /// <summary>
    /// Scene configuration for batch command files
    /// </summary>
    [System.Serializable]
    public class SceneConfig
    {
        public string name;              // Scene name (required)
        public bool create = false;      // Whether to create the scene if it doesn't exist
        public string path = "Assets/Scenes";  // Path where to create the scene
        public string type = "DefaultGameObjects";  // Scene type if creating
        public bool addToBuild = false;  // Whether to add to build settings if creating
    }
    
    /// <summary>
    /// Data structure for individual batch commands
    /// </summary>
    [System.Serializable]
    public class BatchCommand
    {
        public string action;
        public string name;
        
        // Scene creation fields
        public string path;
        public string type;
        public bool addToBuild;
        
        // Canvas fields
        public string scene;
        public string renderMode;
        public int referenceWidth;
        public int referenceHeight;
        public string scaleMode;
        public int sortingOrder;
        public float[] worldPosition;
        
        // UI element fields
        public string parent;
        public float width;
        public float height;
        public string anchor;
        public float[] position;
        
        // 3D GameObject fields
        public float[] rotation;
        public float[] scale;
        
        // ScrollView specific fields
        public bool horizontal = true;
        public bool vertical = true;
        public string scrollbarVisibility = "AutoHideAndExpandViewport";
        public float scrollSensitivity = 1.0f;
        
        // Text/Button specific fields
        public string text;
        public int fontSize;
        public string color;
    }

    /// <summary>
    /// Command Line Interface tools for Unity development workflow automation
    /// Provides scene creation, canvas management, and project analysis capabilities
    /// </summary>
    public static class CLI
    {
        #region Scene Creation Methods
        
        /// <summary>
        /// Creates a new Unity scene with specified name, path, and setup type
        /// </summary>
        /// <param name="sceneName">Name of the scene to create</param>
        /// <param name="scenePath">Directory path where scene will be created</param>
        /// <param name="sceneSetup">Unity scene setup type (Empty, DefaultGameObjects, etc.)</param>
        /// <param name="addToBuildSettings">Whether to add scene to build settings</param>
        /// <returns>True if scene was created successfully</returns>
        public static bool CreateScene(string sceneName, string scenePath, string sceneSetup = "Empty", bool addToBuildSettings = false)
        {
            if (!ValidateSceneCreation(sceneName, scenePath))
                return false;

            try
            {
                // Parse scene setup type
                NewSceneSetup setupType = ParseSceneSetup(sceneSetup);
                
                // Ensure directory exists
                if (!Directory.Exists(scenePath))
                {
                    Directory.CreateDirectory(scenePath);
                    AssetDatabase.Refresh();
                    Debug.Log($"[VibeUnityCLI] Created directory: {scenePath}");
                }

                // Create full scene path
                string fullScenePath = Path.Combine(scenePath, $"{sceneName}.unity");
                
                // Check if scene already exists
                if (File.Exists(fullScenePath))
                {
                    Debug.LogWarning($"[VibeUnityCLI] Scene already exists: {fullScenePath}");
                    return false;
                }

                // Create new scene
                Scene newScene = EditorSceneManager.NewScene(setupType, NewSceneMode.Single);
                
                // Save scene
                bool saved = EditorSceneManager.SaveScene(newScene, fullScenePath);
                
                if (saved)
                {
                    // Log detailed success information  
                Debug.Log($"[VibeUnityCLI] ✅ SUCCESS: Created scene '{sceneName}'");
                Debug.Log($"[VibeUnityCLI]    └─ Type: {sceneSetup}");
                Debug.Log($"[VibeUnityCLI]    └─ Path: {fullScenePath}");
                Debug.Log($"[VibeUnityCLI]    └─ Added to Build: {addToBuildSettings}");
                Debug.Log($"[VibeUnityCLI]    └─ Scene Objects: {newScene.rootCount} root GameObjects");
                    
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
                    Debug.LogError($"[VibeUnityCLI] Failed to save scene: {fullScenePath}");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] Exception creating scene: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Lists all available scene types for the current Unity installation
        /// </summary>
        public static void ListSceneTypes()
        {
            var availableTypes = GetAvailableSceneTypes();
            Debug.Log($"[VibeUnityCLI] Available scene types: {string.Join(", ", availableTypes)}");
            
            // Also log detailed descriptions
            Debug.Log("[VibeUnityCLI] Scene Type Descriptions:");
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
        
        #region Canvas Creation Methods
        
        /// <summary>
        /// Adds a canvas to the specified or currently active scene with specified parameters
        /// </summary>
        /// <param name="canvasName">Name for the canvas GameObject</param>
        /// <param name="sceneName">Name of scene to target (optional)</param>
        /// <param name="renderMode">Canvas render mode (ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace)</param>
        /// <param name="referenceWidth">Reference resolution width</param>
        /// <param name="referenceHeight">Reference resolution height</param>
        /// <param name="scaleMode">UI scale mode (ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize)</param>
        /// <param name="sortingOrder">Canvas sorting order</param>
        /// <param name="worldPosition">Position for WorldSpace canvas</param>
        /// <returns>True if canvas was created successfully</returns>
        public static bool AddCanvas(
            string canvasName,
            string sceneName = null,
            string renderMode = "ScreenSpaceOverlay", 
            int referenceWidth = 1920, 
            int referenceHeight = 1080,
            string scaleMode = "ScaleWithScreenSize",
            int sortingOrder = 0,
            Vector3? worldPosition = null)
        {
            try
            {
                // Load target scene if specified
                if (!LoadTargetScene(sceneName))
                {
                    return false;
                }
                
                // Validate active scene
                Scene activeScene = SceneManager.GetActiveScene();
                if (!activeScene.IsValid())
                {
                    Debug.LogError($"[VibeUnityCLI] ❌ ERROR: No valid scene available for canvas '{canvasName}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Target Scene: '{sceneName ?? "current"}'");
                    return false;
                }
                
                // Create canvas GameObject
                GameObject canvasGO = new GameObject(canvasName);
                Canvas canvas = canvasGO.AddComponent<Canvas>();
                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();
                
                // Set render mode
                canvas.renderMode = ParseRenderMode(renderMode);
                canvas.sortingOrder = sortingOrder;
                
                // Set vertex color always in gamma space for UI consistency
                canvas.vertexColorAlwaysGammaSpace = true;
                
                // Configure canvas scaler
                scaler.uiScaleMode = ParseScaleMode(scaleMode);
                scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                // Set world position for WorldSpace canvas
                if (canvas.renderMode == RenderMode.WorldSpace && worldPosition.HasValue)
                {
                    canvasGO.transform.position = worldPosition.Value;
                }
                
                // Create EventSystem if it doesn't exist
                if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    CreateEventSystem();
                }
                
                // Log detailed success information
                Debug.Log($"[VibeUnityCLI] ✅ SUCCESS: Created canvas '{canvasName}'");
                Debug.Log($"[VibeUnityCLI]    └─ Render Mode: {renderMode}");
                Debug.Log($"[VibeUnityCLI]    └─ Resolution: {referenceWidth}x{referenceHeight}");
                Debug.Log($"[VibeUnityCLI]    └─ Scale Mode: {scaleMode}");
                Debug.Log($"[VibeUnityCLI]    └─ Sorting Order: {sortingOrder}");
                Debug.Log($"[VibeUnityCLI]    └─ Vertex Color: Always in Gamma Space");
                Debug.Log($"[VibeUnityCLI]    └─ Components: Canvas, CanvasScaler, GraphicRaycaster");
                Debug.Log($"[VibeUnityCLI]    └─ Hierarchy: {GetGameObjectPath(canvasGO)}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] Exception creating canvas: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Creates EventSystem if none exists in the scene
        /// </summary>
        private static void CreateEventSystem()
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[VibeUnityCLI] Created EventSystem for UI interaction");
        }
        
        #endregion
        
        #region UI Element Creation Methods
        
        /// <summary>
        /// Creates a UI panel with optional parent GameObject
        /// </summary>
        /// <param name="panelName">Name for the panel GameObject</param>
        /// <param name="parentName">Name of parent GameObject (canvas or other UI element)</param>
        /// <param name="sceneName">Name of scene to target (optional)</param>
        /// <param name="width">Panel width (default: 200)</param>
        /// <param name="height">Panel height (default: 200)</param>
        /// <param name="anchorPreset">Anchor preset (default: MiddleCenter)</param>
        /// <returns>True if panel was created successfully</returns>
        public static bool AddPanel(
            string panelName,
            string parentName = null,
            string sceneName = null,
            float width = 200f,
            float height = 200f,
            string anchorPreset = "MiddleCenter")
        {
            try
            {
                // Load target scene if specified
                if (!LoadTargetScene(sceneName))
                {
                    return false;
                }
                
                // Find parent GameObject
                GameObject parent = FindUIParent(parentName);
                if (parent == null)
                {
                    Debug.LogError($"[VibeUnityCLI] ❌ ERROR: Parent lookup failed for panel '{panelName}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Requested Parent: '{parentName ?? "auto-detect"}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Target Scene: '{sceneName ?? "current"}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Available GameObjects: {ListAvailableGameObjects()}");
                    return false;
                }
                
                // Create panel GameObject
                GameObject panelGO = new GameObject(panelName);
                panelGO.transform.SetParent(parent.transform, false);
                
                // Add UI components
                Image panelImage = panelGO.AddComponent<Image>();
                panelImage.color = new Color(1f, 1f, 1f, 0.392f); // Default panel background
                
                // Setup RectTransform
                RectTransform rectTransform = panelGO.GetComponent<RectTransform>();
                SetupRectTransform(rectTransform, width, height, anchorPreset);
                
                // Log detailed success information
                Debug.Log($"[VibeUnityCLI] ✅ SUCCESS: Created panel '{panelName}'");
                Debug.Log($"[VibeUnityCLI]    └─ Parent: {parent.name} (Type: {parent.GetComponent<Canvas>()?.GetType().Name ?? parent.GetType().Name})");
                Debug.Log($"[VibeUnityCLI]    └─ Components: Image (background)");
                Debug.Log($"[VibeUnityCLI]    └─ Size: {width}x{height}");
                Debug.Log($"[VibeUnityCLI]    └─ Anchor: {anchorPreset}");
                Debug.Log($"[VibeUnityCLI]    └─ Hierarchy: {GetGameObjectPath(panelGO)}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] Exception creating panel: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Creates a UI button with optional parent GameObject
        /// </summary>
        /// <param name="buttonName">Name for the button GameObject</param>
        /// <param name="parentName">Name of parent GameObject</param>
        /// <param name="sceneName">Name of scene to target (optional)</param>
        /// <param name="buttonText">Text to display on button</param>
        /// <param name="width">Button width (default: 160)</param>
        /// <param name="height">Button height (default: 30)</param>
        /// <param name="anchorPreset">Anchor preset (default: MiddleCenter)</param>
        /// <returns>True if button was created successfully</returns>
        public static bool AddButton(
            string buttonName,
            string parentName = null,
            string sceneName = null,
            string buttonText = "Button",
            float width = 160f,
            float height = 30f,
            string anchorPreset = "MiddleCenter")
        {
            try
            {
                // Load target scene if specified
                if (!LoadTargetScene(sceneName))
                {
                    return false;
                }
                
                // Find parent GameObject
                GameObject parent = FindUIParent(parentName);
                if (parent == null)
                {
                    Debug.LogError($"[VibeUnityCLI] ❌ ERROR: Parent lookup failed for button '{buttonName}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Requested Parent: '{parentName ?? "auto-detect"}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Target Scene: '{sceneName ?? "current"}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Available GameObjects: {ListAvailableGameObjects()}");
                    return false;
                }
                
                // Create button GameObject
                GameObject buttonGO = new GameObject(buttonName);
                buttonGO.transform.SetParent(parent.transform, false);
                
                // Add button components
                Image buttonImage = buttonGO.AddComponent<Image>();
                Button button = buttonGO.AddComponent<Button>();
                
                // Setup RectTransform
                RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
                SetupRectTransform(rectTransform, width, height, anchorPreset);
                
                // Create text child
                if (!string.IsNullOrEmpty(buttonText))
                {
                    CreateButtonText(buttonGO, buttonText);
                }
                
                // Log detailed success information
                Debug.Log($"[VibeUnityCLI] ✅ SUCCESS: Created button '{buttonName}'");  
                Debug.Log($"[VibeUnityCLI]    └─ Parent: {parent.name} (Type: {parent.GetComponent<Canvas>()?.GetType().Name ?? parent.GetType().Name})");
                Debug.Log($"[VibeUnityCLI]    └─ Components: Image, Button");
                Debug.Log($"[VibeUnityCLI]    └─ Text: \"{buttonText}\"");
                Debug.Log($"[VibeUnityCLI]    └─ Size: {width}x{height}");
                Debug.Log($"[VibeUnityCLI]    └─ Anchor: {anchorPreset}");
                Debug.Log($"[VibeUnityCLI]    └─ Hierarchy: {GetGameObjectPath(buttonGO)}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] Exception creating button: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Creates a UI text element with optional parent GameObject
        /// </summary>
        /// <param name="textName">Name for the text GameObject</param>
        /// <param name="parentName">Name of parent GameObject</param>
        /// <param name="sceneName">Name of scene to target (optional)</param>
        /// <param name="textContent">Text content to display</param>
        /// <param name="fontSize">Font size (default: 14)</param>
        /// <param name="width">Text width (default: 200)</param>
        /// <param name="height">Text height (default: 50)</param>
        /// <param name="anchorPreset">Anchor preset (default: MiddleCenter)</param>
        /// <returns>True if text was created successfully</returns>
        public static bool AddText(
            string textName,
            string parentName = null,
            string sceneName = null,
            string textContent = "New Text",
            int fontSize = 14,
            float width = 200f,
            float height = 50f,
            string anchorPreset = "MiddleCenter")
        {
            try
            {
                // Load target scene if specified
                if (!LoadTargetScene(sceneName))
                {
                    return false;
                }
                
                // Find parent GameObject
                GameObject parent = FindUIParent(parentName);
                if (parent == null)
                {
                    Debug.LogError($"[VibeUnityCLI] ❌ ERROR: Parent lookup failed for text '{textName}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Requested Parent: '{parentName ?? "auto-detect"}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Target Scene: '{sceneName ?? "current"}'");
                    Debug.LogError($"[VibeUnityCLI]    └─ Available GameObjects: {ListAvailableGameObjects()}");
                    return false;
                }
                
                // Create text GameObject
                GameObject textGO = new GameObject(textName);
                textGO.transform.SetParent(parent.transform, false);
                
                // Add text component
                Text textComponent = textGO.AddComponent<Text>();
                textComponent.text = textContent;
                textComponent.fontSize = fontSize;
                textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                textComponent.alignment = TextAnchor.MiddleCenter;
                
                // Setup RectTransform
                RectTransform rectTransform = textGO.GetComponent<RectTransform>();
                SetupRectTransform(rectTransform, width, height, anchorPreset);
                
                // Log detailed success information
                Debug.Log($"[VibeUnityCLI] ✅ SUCCESS: Created text '{textName}'");
                Debug.Log($"[VibeUnityCLI]    └─ Parent: {parent.name} (Type: {parent.GetComponent<Canvas>()?.GetType().Name ?? parent.GetType().Name})");
                Debug.Log($"[VibeUnityCLI]    └─ Components: Text");
                Debug.Log($"[VibeUnityCLI]    └─ Content: \"{textContent}\"");
                Debug.Log($"[VibeUnityCLI]    └─ Font Size: {fontSize}px");
                Debug.Log($"[VibeUnityCLI]    └─ Size: {width}x{height}");
                Debug.Log($"[VibeUnityCLI]    └─ Anchor: {anchorPreset}");
                Debug.Log($"[VibeUnityCLI]    └─ Hierarchy: {GetGameObjectPath(textGO)}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] Exception creating text: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Adds a ScrollView UI element to a scene
        /// </summary>
        /// <param name="scrollViewName">Name for the ScrollView GameObject</param>
        /// <param name="parentName">Parent GameObject name (optional)</param>
        /// <param name="sceneName">Target scene name (optional)</param>
        /// <param name="width">ScrollView width in pixels</param>
        /// <param name="height">ScrollView height in pixels</param>
        /// <param name="anchorPreset">Anchor preset name</param>
        /// <param name="horizontal">Enable horizontal scrolling</param>
        /// <param name="vertical">Enable vertical scrolling</param>
        /// <param name="scrollbarVisibility">Scrollbar visibility mode</param>
        /// <param name="scrollSensitivity">Scroll sensitivity value</param>
        /// <returns>True if ScrollView was created successfully</returns>
        public static bool AddScrollView(
            string scrollViewName,
            string parentName = null,
            string sceneName = null,
            float width = 300f,
            float height = 200f,
            string anchorPreset = "MiddleCenter",
            bool horizontal = true,
            bool vertical = true,
            string scrollbarVisibility = "AutoHideAndExpandViewport",
            float scrollSensitivity = 1.0f)
        {
            try
            {
                // Load target scene if specified
                if (!LoadTargetScene(sceneName))
                {
                    return false;
                }
                
                // Find parent GameObject
                GameObject parent = FindUIParent(parentName);
                if (parent == null)
                {
                    Debug.LogError($"[VibeUnityCLI] Could not find suitable parent for ScrollView '{scrollViewName}'");
                    return false;
                }
                
                // Create ScrollView GameObject
                GameObject scrollViewGO = new GameObject(scrollViewName);
                scrollViewGO.transform.SetParent(parent.transform, false);
                
                // Add and configure RectTransform
                RectTransform scrollViewRect = scrollViewGO.AddComponent<RectTransform>();
                SetupRectTransform(scrollViewRect, width, height, anchorPreset);
                
                // Add Image component for background
                Image scrollViewImage = scrollViewGO.AddComponent<Image>();
                scrollViewImage.color = new Color(1f, 1f, 1f, 0.392f);
                
                // Add ScrollRect component
                ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
                scrollRect.horizontal = horizontal;
                scrollRect.vertical = vertical;
                scrollRect.scrollSensitivity = scrollSensitivity;
                
                // Parse scrollbar visibility
                switch (scrollbarVisibility.ToLower())
                {
                    case "autohideandexpandviewport":
                        scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                        break;
                    case "autohide":
                        scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                        break;
                    case "permanent":
                        scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
                        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
                        break;
                }
                
                // Create Viewport
                GameObject viewport = new GameObject("Viewport");
                viewport.transform.SetParent(scrollViewGO.transform, false);
                
                RectTransform viewportRect = viewport.AddComponent<RectTransform>();
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.sizeDelta = Vector2.zero;
                viewportRect.offsetMin = Vector2.zero;
                viewportRect.offsetMax = Vector2.zero;
                
                Image viewportImage = viewport.AddComponent<Image>();
                viewportImage.color = new Color(1f, 1f, 1f, 0.392f);
                
                Mask viewportMask = viewport.AddComponent<Mask>();
                viewportMask.showMaskGraphic = false;
                
                // Create Content
                GameObject content = new GameObject("Content");
                content.transform.SetParent(viewport.transform, false);
                
                RectTransform contentRect = content.AddComponent<RectTransform>();
                contentRect.anchorMin = Vector2.up;
                contentRect.anchorMax = Vector2.one;
                contentRect.sizeDelta = new Vector2(0, 300);
                contentRect.pivot = Vector2.up;
                
                // Assign references to ScrollRect
                scrollRect.viewport = viewportRect;
                scrollRect.content = contentRect;
                
                Debug.Log($"[VibeUnityCLI] ✅ ScrollView created: {scrollViewName}");
                Debug.Log($"[VibeUnityCLI]    └─ Parent: {parent.name}");
                Debug.Log($"[VibeUnityCLI]    └─ Size: {width}x{height}");
                Debug.Log($"[VibeUnityCLI]    └─ Horizontal: {horizontal}");
                Debug.Log($"[VibeUnityCLI]    └─ Vertical: {vertical}");
                Debug.Log($"[VibeUnityCLI]    └─ Anchor: {anchorPreset}");
                Debug.Log($"[VibeUnityCLI]    └─ Hierarchy: {GetGameObjectPath(scrollViewGO)}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] Exception creating scrollview: {e.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Parses string scene setup type to Unity enum
        /// </summary>
        private static NewSceneSetup ParseSceneSetup(string sceneSetup)
        {
            switch (sceneSetup.ToLower())
            {
                case "empty":
                    return NewSceneSetup.EmptyScene;
                case "defaultgameobjects":
                case "default":
                case "3d":
                    return NewSceneSetup.DefaultGameObjects;
                case "2d":
                    // For 2D, we'll use empty and set up 2D specific settings
                    return NewSceneSetup.EmptyScene;
                default:
                    Debug.LogWarning($"[VibeUnityCLI] Unknown scene setup '{sceneSetup}', using Empty");
                    return NewSceneSetup.EmptyScene;
            }
        }
        
        /// <summary>
        /// Parses string render mode to Unity enum
        /// </summary>
        private static RenderMode ParseRenderMode(string renderMode)
        {
            switch (renderMode.ToLower())
            {
                case "screenspaceoverlay":
                case "overlay":
                    return RenderMode.ScreenSpaceOverlay;
                case "screenspacecamera":
                case "camera":
                    return RenderMode.ScreenSpaceCamera;
                case "worldspace":
                case "world":
                    return RenderMode.WorldSpace;
                default:
                    Debug.LogWarning($"[VibeUnityCLI] Unknown render mode '{renderMode}', using ScreenSpaceOverlay");
                    return RenderMode.ScreenSpaceOverlay;
            }
        }
        
        /// <summary>
        /// Parses string scale mode to Unity enum
        /// </summary>
        private static CanvasScaler.ScaleMode ParseScaleMode(string scaleMode)
        {
            switch (scaleMode.ToLower())
            {
                case "constantpixelsize":
                case "constant":
                    return CanvasScaler.ScaleMode.ConstantPixelSize;
                case "scalewithscreensize":
                case "scale":
                    return CanvasScaler.ScaleMode.ScaleWithScreenSize;
                case "constantphysicalsize":
                case "physical":
                    return CanvasScaler.ScaleMode.ConstantPhysicalSize;
                default:
                    Debug.LogWarning($"[VibeUnityCLI] Unknown scale mode '{scaleMode}', using ScaleWithScreenSize");
                    return CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }
        }
        
        /// <summary>
        /// Finds a UI parent GameObject by name in the active scene
        /// </summary>
        private static GameObject FindUIParent(string parentName)
        {
            if (string.IsNullOrEmpty(parentName))
            {
                // If no parent specified, try to find the first canvas
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    return canvas.gameObject;
                }
                Debug.LogError("[VibeUnityCLI] No parent specified and no canvas found in scene");
                return null;
            }
            
            // Search for GameObject by name in active scene
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            
            foreach (GameObject rootObj in rootObjects)
            {
                GameObject found = FindGameObjectRecursive(rootObj, parentName);
                if (found != null)
                {
                    return found;
                }
            }
            
            Debug.LogError($"[VibeUnityCLI] Parent GameObject '{parentName}' not found in active scene");
            return null;
        }
        
        /// <summary>
        /// Recursively searches for a GameObject by name
        /// </summary>
        private static GameObject FindGameObjectRecursive(GameObject parent, string targetName)
        {
            if (parent.name == targetName)
                return parent;
                
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                GameObject found = FindGameObjectRecursive(parent.transform.GetChild(i).gameObject, targetName);
                if (found != null)
                    return found;
            }
            
            return null;
        }
        
        /// <summary>
        /// Sets up RectTransform properties for UI elements
        /// </summary>
        private static void SetupRectTransform(RectTransform rectTransform, float width, float height, string anchorPreset)
        {
            // Set anchor preset
            switch (anchorPreset.ToLower())
            {
                case "topleft":
                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(0f, 1f);
                    rectTransform.pivot = new Vector2(0f, 1f);
                    break;
                case "topcenter":
                    rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                    break;
                case "topright":
                    rectTransform.anchorMin = new Vector2(1f, 1f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                    rectTransform.pivot = new Vector2(1f, 1f);
                    break;
                case "middleleft":
                    rectTransform.anchorMin = new Vector2(0f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0f, 0.5f);
                    rectTransform.pivot = new Vector2(0f, 0.5f);
                    break;
                case "middlecenter":
                default:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case "middleright":
                    rectTransform.anchorMin = new Vector2(1f, 0.5f);
                    rectTransform.anchorMax = new Vector2(1f, 0.5f);
                    rectTransform.pivot = new Vector2(1f, 0.5f);
                    break;
                case "bottomleft":
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0f, 0f);
                    break;
                case "bottomcenter":
                    rectTransform.anchorMin = new Vector2(0.5f, 0f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                    break;
                case "bottomright":
                    rectTransform.anchorMin = new Vector2(1f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0f);
                    rectTransform.pivot = new Vector2(1f, 0f);
                    break;
            }
            
            // Set size
            rectTransform.sizeDelta = new Vector2(width, height);
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        /// <summary>
        /// Creates text child for button GameObject
        /// </summary>
        private static void CreateButtonText(GameObject buttonParent, string textContent)
        {
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonParent.transform, false);
            
            Text textComponent = textGO.AddComponent<Text>();
            textComponent.text = textContent;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 14;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.black;
            
            // Setup RectTransform to fill button
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
        }
        
        /// <summary>
        /// Gets the full hierarchy path of a GameObject
        /// </summary>
        private static string GetGameObjectPath(GameObject gameObject)
        {
            string path = gameObject.name;
            Transform parent = gameObject.transform.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }
        
        /// <summary>
        /// Lists available GameObjects in the scene for error reporting
        /// </summary>
        private static string ListAvailableGameObjects()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            var gameObjectNames = new List<string>();
            
            foreach (GameObject rootObj in rootObjects)
            {
                CollectGameObjectNames(rootObj, gameObjectNames, "");
            }
            
            return string.Join(", ", gameObjectNames.Take(10)) + (gameObjectNames.Count > 10 ? "..." : "");
        }
        
        /// <summary>
        /// Recursively collects GameObject names for error reporting
        /// </summary>
        private static void CollectGameObjectNames(GameObject parent, List<string> names, string prefix)
        {
            names.Add(prefix + parent.name);
            
            for (int i = 0; i < parent.transform.childCount && names.Count < 15; i++)
            {
                CollectGameObjectNames(parent.transform.GetChild(i).gameObject, names, prefix + "  ");
            }
        }
        
        /// <summary>
        /// Loads the target scene if specified, otherwise uses current scene or smart detection
        /// </summary>
        private static bool LoadTargetScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                // Try current active scene first
                Scene currentScene = SceneManager.GetActiveScene();
                if (currentScene.IsValid() && !string.IsNullOrEmpty(currentScene.path))
                {
                    Debug.Log($"[VibeUnityCLI] Using current active scene: {currentScene.name}");
                    return true;
                }
                
                // Fallback: Smart scene detection - use most recently modified scene
                string smartScene = GetMostRecentScene();
                if (!string.IsNullOrEmpty(smartScene))
                {
                    Debug.Log($"[VibeUnityCLI] ⚡ Smart Detection: Using most recent scene: {System.IO.Path.GetFileNameWithoutExtension(smartScene)}");
                    try
                    {
                        Scene targetScene = EditorSceneManager.OpenScene(smartScene);
                        if (targetScene.IsValid())
                        {
                            Debug.Log($"[VibeUnityCLI] ✅ Loaded detected scene: {targetScene.name}");
                            return true;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[VibeUnityCLI] Failed to load detected scene: {e.Message}");
                    }
                }
                
                Debug.LogError($"[VibeUnityCLI] ❌ ERROR: No valid scene available");
                Debug.LogError($"[VibeUnityCLI]    └─ No active scene and no scenes found in project");
                Debug.LogError($"[VibeUnityCLI]    └─ Available scenes: {ListAvailableScenes()}");
                return false;
            }
            
            // Try to find scene by name
            string scenePath = FindSceneAsset(sceneName);
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError($"[VibeUnityCLI] ❌ ERROR: Scene '{sceneName}' not found");
                Debug.LogError($"[VibeUnityCLI]    └─ Available scenes: {ListAvailableScenes()}");
                return false;
            }
            
            try
            {
                Scene targetScene = EditorSceneManager.OpenScene(scenePath);
                if (targetScene.IsValid())
                {
                    Debug.Log($"[VibeUnityCLI] ✅ Loaded target scene: {sceneName} ({scenePath})");
                    return true;
                }
                else
                {
                    Debug.LogError($"[VibeUnityCLI] ❌ ERROR: Failed to load scene '{sceneName}'");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] ❌ ERROR: Exception loading scene '{sceneName}': {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Finds scene asset path by name
        /// </summary>
        private static string FindSceneAsset(string sceneName)
        {
            // Add .unity extension if not present
            if (!sceneName.EndsWith(".unity"))
            {
                sceneName += ".unity";
            }
            
            // Search in common scene directories
            string[] searchPaths = {
                $"Assets/Scenes/{sceneName}",
                $"Assets/{sceneName}",
                $"Assets/Scenes/Game/{sceneName}",
                $"Assets/Scenes/UI/{sceneName}",
                $"Assets/Game/Scenes/{sceneName}"
            };
            
            foreach (string path in searchPaths)
            {
                if (System.IO.File.Exists(path))
                {
                    return path;
                }
            }
            
            // Try AssetDatabase search as fallback
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName.Replace(".unity", "")}");
            if (guids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }
            
            return null;
        }
        
        /// <summary>
        /// Lists available scenes for error reporting
        /// </summary>
        private static string ListAvailableScenes()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            var sceneNames = new List<string>();
            
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                sceneNames.Add(sceneName);
            }
            
            return string.Join(", ", sceneNames.Take(10)) + (sceneNames.Count > 10 ? "..." : "");
        }
        
        /// <summary>
        /// Gets the most recently modified scene in the project (for smart scene detection)
        /// </summary>
        private static string GetMostRecentScene()
        {
            try
            {
                string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
                if (sceneGuids.Length == 0)
                {
                    Debug.Log("[VibeUnityCLI] Smart Detection: No scenes found in project");
                    return null;
                }
                
                string mostRecentPath = null;
                System.DateTime mostRecentTime = System.DateTime.MinValue;
                
                foreach (string guid in sceneGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                    {
                        System.DateTime fileTime = System.IO.File.GetLastWriteTime(path);
                        if (fileTime > mostRecentTime)
                        {
                            mostRecentTime = fileTime;
                            mostRecentPath = path;
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(mostRecentPath))
                {
                    Debug.Log($"[VibeUnityCLI] Smart Detection: Found most recent scene: {mostRecentPath} (modified: {mostRecentTime})");
                }
                else
                {
                    Debug.Log("[VibeUnityCLI] Smart Detection: No valid scene files found");
                }
                
                return mostRecentPath;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VibeUnityCLI] Error in smart scene detection: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets list of available scene types
        /// </summary>
        private static List<string> GetAvailableSceneTypes()
        {
            var types = new List<string>
            {
                "Empty",
                "DefaultGameObjects",
                "2D",
                "3D"
            };
            
            // Check for render pipeline specific types
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
            
            // Check for VR/AR support (simplified check)
            #if UNITY_XR_MANAGEMENT
            types.Add("VR");
            #endif
            
            #if UNITY_AR_FOUNDATION
            types.Add("AR");
            #endif
            
            return types;
        }
        
        /// <summary>
        /// Validates scene creation parameters
        /// </summary>
        private static bool ValidateSceneCreation(string sceneName, string scenePath)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[VibeUnityCLI] Scene name cannot be empty");
                return false;
            }
            
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("[VibeUnityCLI] Scene path cannot be empty");
                return false;
            }
            
            // Check for invalid characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (sceneName.IndexOfAny(invalidChars) >= 0)
            {
                Debug.LogError("[VibeUnityCLI] Scene name contains invalid characters");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Adds scene to build settings
        /// </summary>
        private static void AddSceneToBuildSettings(string scenePath)
        {
            try
            {
                // Get current build settings
                EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
                
                // Check if scene already exists in build settings
                foreach (var scene in originalScenes)
                {
                    if (scene.path == scenePath)
                    {
                        Debug.Log($"[VibeUnityCLI] Scene already in build settings: {scenePath}");
                        return;
                    }
                }
                
                // Add scene to build settings
                EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[originalScenes.Length + 1];
                System.Array.Copy(originalScenes, newScenes, originalScenes.Length);
                newScenes[originalScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
                
                EditorBuildSettings.scenes = newScenes;
                Debug.Log($"[VibeUnityCLI] Added scene to build settings: {scenePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VibeUnityCLI] Failed to add scene to build settings: {e.Message}");
            }
        }
        
        #endregion
        
        #region Command Line Interface Entry Points
        
        /// <summary>
        /// Command line entry point for scene creation - DISABLED
        /// Usage: Unity -batchmode -quit -executeMethod VibeUnity.Editor.CLI.CreateSceneFromCommandLine -projectPath "path/to/project" scene_name scene_path scene_type
        /// </summary>
        public static void CreateSceneFromCommandLine()
        {
            // CLI commands disabled - functionality commented out
            Debug.LogWarning("[VibeUnityCLI] CLI commands are disabled. Use internal API methods instead.");
            return;
            
            /*
            string[] args = System.Environment.GetCommandLineArgs();
            
            // Find our arguments after -executeMethod
            int executeMethodIndex = System.Array.FindIndex(args, arg => arg == "-executeMethod");
            if (executeMethodIndex == -1 || executeMethodIndex + 4 >= args.Length)
            {
                Debug.LogError("[VibeUnityCLI] Invalid arguments. Usage: scene_name scene_path [scene_type] [add_to_build]");
                Debug.LogError("[VibeUnityCLI] Available scene types: " + string.Join(", ", GetAvailableSceneTypes()));
                return;
            }
            
            string sceneName = args[executeMethodIndex + 2];
            string scenePath = args[executeMethodIndex + 3];
            string sceneType = args.Length > executeMethodIndex + 4 ? args[executeMethodIndex + 4] : "DefaultGameObjects";
            bool addToBuild = args.Length > executeMethodIndex + 5 ? bool.Parse(args[executeMethodIndex + 5]) : false;
            
            Debug.Log($"[VibeUnityCLI] Creating scene: {sceneName} at {scenePath} (type: {sceneType})");
            
            bool success = CreateScene(sceneName, scenePath, sceneType, addToBuild);
            
            if (success)
            {
                Debug.Log($"[VibeUnityCLI] ✅ Successfully created scene: {scenePath}/{sceneName}.unity");
            }
            else
            {
                Debug.LogError($"[VibeUnityCLI] ❌ Failed to create scene");
                UnityEditor.EditorApplication.Exit(1);
            }
            */
        }
        
        /// <summary>
        /// Command line entry point for canvas creation - DISABLED
        /// Usage: Unity -batchmode -quit -executeMethod VibeUnity.Editor.CLI.AddCanvasFromCommandLine canvas_name [scene_name] render_mode [width] [height] [scale_mode]
        /// </summary>
        public static void AddCanvasFromCommandLine()
        {
            // CLI commands disabled - functionality commented out
            Debug.LogWarning("[VibeUnityCLI] CLI commands are disabled. Use internal API methods instead.");
            return;
            
            /*
            string[] args = System.Environment.GetCommandLineArgs();
            
            int executeMethodIndex = System.Array.FindIndex(args, arg => arg == "-executeMethod");
            if (executeMethodIndex == -1 || executeMethodIndex + 2 >= args.Length)
            {
                Debug.LogError("[VibeUnityCLI] Invalid arguments. Usage: canvas_name [scene_name] render_mode [width] [height] [scale_mode]");
                Debug.LogError("[VibeUnityCLI] Render modes: ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace");
                return;
            }
            
            string canvasName = args[executeMethodIndex + 2];
            string sceneName = args.Length > executeMethodIndex + 3 ? args[executeMethodIndex + 3] : null;
            string renderMode = args.Length > executeMethodIndex + 4 ? args[executeMethodIndex + 4] : "ScreenSpaceOverlay";
            int width = args.Length > executeMethodIndex + 5 ? int.Parse(args[executeMethodIndex + 5]) : 1920;
            int height = args.Length > executeMethodIndex + 6 ? int.Parse(args[executeMethodIndex + 6]) : 1080;
            string scaleMode = args.Length > executeMethodIndex + 7 ? args[executeMethodIndex + 7] : "ScaleWithScreenSize";
            
            // Handle empty string as null for optional parameters
            if (string.IsNullOrEmpty(sceneName)) sceneName = null;
            
            Debug.Log($"[VibeUnityCLI] Adding canvas: {canvasName} in scene {sceneName ?? "current"} ({renderMode}, {width}x{height})");
            
            bool success = AddCanvas(canvasName, sceneName, renderMode, width, height, scaleMode);
            
            if (success)
            {
                Debug.Log($"[VibeUnityCLI] ✅ Successfully added canvas: {canvasName}");
                
                // Save the scene
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            }
            else
            {
                Debug.LogError($"[VibeUnityCLI] ❌ Failed to add canvas");
                UnityEditor.EditorApplication.Exit(1);
            }
            */
        }
        
        /// <summary>
        /// Command line entry point for listing scene types - DISABLED
        /// Usage: Unity -batchmode -quit -executeMethod VibeUnity.Editor.CLI.ListSceneTypesFromCommandLine
        /// </summary>
        public static void ListSceneTypesFromCommandLine()
        {
            // CLI commands disabled - functionality commented out
            Debug.LogWarning("[VibeUnityCLI] CLI commands are disabled. Use internal API methods instead.");
            return;
            
            /*
            Debug.Log("[VibeUnityCLI] === Available Scene Types ===");
            ListSceneTypes();
            Debug.Log("[VibeUnityCLI] ===========================");
            */
        }
        
        /// <summary>
        /// Command line entry point for adding UI panels - DISABLED
        /// Usage: Unity -batchmode -quit -executeMethod VibeUnity.Editor.CLI.AddPanelFromCommandLine panel_name [parent_name] [scene_name] [width] [height] [anchor_preset]
        /// </summary>
        public static void AddPanelFromCommandLine()
        {
            // CLI commands disabled - functionality commented out
            Debug.LogWarning("[VibeUnityCLI] CLI commands are disabled. Use internal API methods instead.");
            return;
        }
        
        /// <summary>
        /// Command line entry point for adding UI buttons - DISABLED
        /// Usage: Unity -batchmode -quit -executeMethod VibeUnity.Editor.CLI.AddButtonFromCommandLine button_name [parent_name] [scene_name] [button_text] [width] [height] [anchor_preset]
        /// </summary>
        public static void AddButtonFromCommandLine()
        {
            // CLI commands disabled - functionality commented out
            Debug.LogWarning("[VibeUnityCLI] CLI commands are disabled. Use internal API methods instead.");
            return;
        }
        
        /// <summary>
        /// Command line entry point for adding UI text - DISABLED
        /// Usage: Unity -batchmode -quit -executeMethod VibeUnity.Editor.CLI.AddTextFromCommandLine text_name [parent_name] [scene_name] [text_content] [font_size] [width] [height] [anchor_preset]
        /// </summary>
        public static void AddTextFromCommandLine()
        {
            // CLI commands disabled - functionality commented out
            Debug.LogWarning("[VibeUnityCLI] CLI commands are disabled. Use internal API methods instead.");
            return;
        }
        
        /// <summary>
        /// Command line entry point for batch file execution - DISABLED
        /// Usage: Unity -batchmode -quit -executeMethod VibeUnity.Editor.CLI.ExecuteBatchFromCommandLine json_file_path
        /// </summary>
        public static void ExecuteBatchFromCommandLine()
        {
            // CLI commands disabled - functionality commented out
            Debug.LogWarning("[VibeUnityCLI] CLI commands are disabled. Use internal API methods instead.");
            return;
        }
        
        /// <summary>
        /// Executes commands from a JSON batch file
        /// </summary>
        /// <param name="jsonFilePath">Path to the JSON batch file</param>
        /// <returns>True if all commands executed successfully</returns>
        public static bool ExecuteBatchFile(string jsonFilePath)
        {
            try
            {
                if (!System.IO.File.Exists(jsonFilePath))
                {
                    Debug.LogError($"[VibeUnityCLI] Batch file not found: {jsonFilePath}");
                    return false;
                }
                
                string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                
                // Parse JSON using Unity's built-in JSON utility
                BatchCommandFile batchFile;
                try
                {
                    batchFile = JsonUtility.FromJson<BatchCommandFile>(jsonContent);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[VibeUnityCLI] Failed to parse JSON: {e.Message}");
                    return false;
                }
                
                if (batchFile == null || batchFile.commands == null || batchFile.commands.Length == 0)
                {
                    Debug.LogError("[VibeUnityCLI] No commands found in batch file");
                    return false;
                }
                
                Debug.Log($"[VibeUnityCLI] Batch file loaded: {batchFile.commands.Length} commands");
                if (!string.IsNullOrEmpty(batchFile.description))
                {
                    Debug.Log($"[VibeUnityCLI] Description: {batchFile.description}");
                }
                
                // Execute commands sequentially
                for (int i = 0; i < batchFile.commands.Length; i++)
                {
                    var command = batchFile.commands[i];
                    Debug.Log($"[VibeUnityCLI] Executing command {i + 1}/{batchFile.commands.Length}: {command.action}");
                    
                    bool commandSuccess = ExecuteBatchCommand(command);
                    if (!commandSuccess)
                    {
                        Debug.LogError($"[VibeUnityCLI] Command {i + 1} failed: {command.action}");
                        return false;
                    }
                }
                
                // Save all scenes after batch execution
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                AssetDatabase.Refresh();
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] Exception executing batch file: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes commands from a JSON batch file with detailed logging
        /// </summary>
        /// <param name="jsonFilePath">Path to the JSON batch file</param>
        /// <param name="logCapture">StringBuilder to capture log output</param>
        /// <returns>True if all commands executed successfully</returns>
        private static bool ExecuteBatchFileWithLogging(string jsonFilePath, System.Text.StringBuilder logCapture)
        {
            try
            {
                if (!System.IO.File.Exists(jsonFilePath))
                {
                    string error = $"Batch file not found: {jsonFilePath}";
                    logCapture.AppendLine($"ERROR: {error}");
                    Debug.LogError($"[VibeUnityCLI] {error}");
                    return false;
                }
                
                string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                logCapture.AppendLine("JSON Content:");
                logCapture.AppendLine(jsonContent);
                logCapture.AppendLine();
                
                // Parse JSON using Unity's built-in JSON utility
                BatchCommandFile batchFile;
                try
                {
                    batchFile = JsonUtility.FromJson<BatchCommandFile>(jsonContent);
                    logCapture.AppendLine("✅ JSON parsing successful");
                }
                catch (System.Exception e)
                {
                    string error = $"Failed to parse JSON: {e.Message}";
                    logCapture.AppendLine($"❌ ERROR: {error}");
                    logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                    Debug.LogError($"[VibeUnityCLI] {error}");
                    return false;
                }
                
                if (batchFile == null || batchFile.commands == null || batchFile.commands.Length == 0)
                {
                    string error = "No commands found in batch file";
                    logCapture.AppendLine($"❌ ERROR: {error}");
                    Debug.LogError($"[VibeUnityCLI] {error}");
                    return false;
                }
                
                logCapture.AppendLine($"✅ Batch file loaded: {batchFile.commands.Length} commands");
                if (!string.IsNullOrEmpty(batchFile.description))
                {
                    logCapture.AppendLine($"Description: {batchFile.description}");
                }
                logCapture.AppendLine();
                
                // Validate scene configuration
                logCapture.AppendLine("=== Scene Configuration Validation ===");
                if (batchFile.scene == null || string.IsNullOrEmpty(batchFile.scene.name))
                {
                    string error = "Batch file missing required 'scene' configuration with 'name' field";
                    logCapture.AppendLine($"❌ ERROR: {error}");
                    Debug.LogError($"[VibeUnityCLI] {error}");
                    return false;
                }
                
                var sceneConfig = batchFile.scene;
                logCapture.AppendLine($"✅ Scene Name: '{sceneConfig.name}'");
                logCapture.AppendLine($"   └─ Create if missing: {sceneConfig.create}");
                if (sceneConfig.create)
                {
                    logCapture.AppendLine($"   └─ Creation Path: {sceneConfig.path}");
                    logCapture.AppendLine($"   └─ Scene Type: {sceneConfig.type}");
                    logCapture.AppendLine($"   └─ Add to Build: {sceneConfig.addToBuild}");
                }
                logCapture.AppendLine();
                
                // Handle scene loading/creation
                logCapture.AppendLine("=== Scene Processing ===");
                if (!EnsureSceneLoadedWithSceneConfig(sceneConfig, logCapture))
                {
                    logCapture.AppendLine($"❌ Failed to load/create scene: {sceneConfig.name}");
                    Debug.LogError($"[VibeUnityCLI] Failed to load/create scene: {sceneConfig.name}");
                    return false;
                }
                
                // Execute all commands in the loaded scene
                logCapture.AppendLine($"=== Command Execution ({batchFile.commands.Length} commands) ===");
                for (int i = 0; i < batchFile.commands.Length; i++)
                {
                    var command = batchFile.commands[i];
                    logCapture.AppendLine($"--- Command {i + 1}/{batchFile.commands.Length}: {command.action} ---");
                    logCapture.AppendLine($"Command details: {JsonUtility.ToJson(command, true)}");
                    
                    bool commandSuccess = ExecuteBatchCommandWithLogging(command, logCapture);
                    if (!commandSuccess)
                    {
                        logCapture.AppendLine($"❌ Command {i + 1} failed: {command.action}");
                        Debug.LogError($"[VibeUnityCLI] Command {i + 1} failed: {command.action}");
                        return false;
                    }
                    else
                    {
                        logCapture.AppendLine($"✅ Command {i + 1} succeeded: {command.action}");
                    }
                    logCapture.AppendLine();
                }
                
                // Save all scenes after batch execution
                logCapture.AppendLine("Saving scenes and refreshing asset database...");
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                AssetDatabase.Refresh();
                logCapture.AppendLine("✅ Scenes saved and asset database refreshed");
                
                return true;
            }
            catch (System.Exception e)
            {
                string error = $"Exception executing batch file: {e.Message}";
                logCapture.AppendLine($"❌ FATAL ERROR: {error}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                Debug.LogError($"[VibeUnityCLI] {error}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes a single batch command
        /// </summary>
        private static bool ExecuteBatchCommand(BatchCommand command)
        {
            switch (command.action.ToLower())
            {
                case "create-scene":
                    return ExecuteCreateSceneCommand(command);
                case "add-canvas":
                    return ExecuteAddCanvasCommand(command);
                case "add-panel":
                    return ExecuteAddPanelCommand(command);
                case "add-button":
                    return ExecuteAddButtonCommand(command);
                case "add-text":
                    return ExecuteAddTextCommand(command);
                case "add-scrollview":
                    return ExecuteAddScrollViewCommand(command);
                case "add-cube":
                    return ExecuteAddCubeCommand(command);
                case "add-sphere":
                    return ExecuteAddSphereCommand(command);
                case "add-plane":
                    return ExecuteAddPlaneCommand(command);
                case "add-cylinder":
                    return ExecuteAddCylinderCommand(command);
                case "add-capsule":
                    return ExecuteAddCapsuleCommand(command);
                default:
                    Debug.LogError($"[VibeUnityCLI] Unknown batch command: {command.action}");
                    return false;
            }
        }
        
        /// <summary>
        /// Executes a single batch command with detailed logging
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="logCapture">StringBuilder to capture log output</param>
        /// <returns>True if command executed successfully</returns>
        private static bool ExecuteBatchCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            try
            {
                bool result = false;
                
                switch (command.action.ToLower())
                {
                    case "create-scene":
                        logCapture.AppendLine("Executing create-scene command...");
                        result = ExecuteCreateSceneCommandWithLogging(command, logCapture);
                        break;
                    case "add-canvas":
                        logCapture.AppendLine("Executing add-canvas command...");
                        result = ExecuteAddCanvasCommandWithLogging(command, logCapture);
                        break;
                    case "add-panel":
                        logCapture.AppendLine("Executing add-panel command...");
                        result = ExecuteAddPanelCommandWithLogging(command, logCapture);
                        break;
                    case "add-button":
                        logCapture.AppendLine("Executing add-button command...");
                        result = ExecuteAddButtonCommandWithLogging(command, logCapture);
                        break;
                    case "add-text":
                        logCapture.AppendLine("Executing add-text command...");
                        result = ExecuteAddTextCommandWithLogging(command, logCapture);
                        break;
                    case "add-scrollview":
                        logCapture.AppendLine("Executing add-scrollview command...");
                        result = ExecuteAddScrollViewCommandWithLogging(command, logCapture);
                        break;
                    case "add-cube":
                        logCapture.AppendLine("Executing add-cube command...");
                        result = ExecuteAddCubeCommandWithLogging(command, logCapture);
                        break;
                    case "add-sphere":
                        logCapture.AppendLine("Executing add-sphere command...");
                        result = ExecuteAddSphereCommandWithLogging(command, logCapture);
                        break;
                    case "add-plane":
                        logCapture.AppendLine("Executing add-plane command...");
                        result = ExecuteAddPlaneCommandWithLogging(command, logCapture);
                        break;
                    case "add-cylinder":
                        logCapture.AppendLine("Executing add-cylinder command...");
                        result = ExecuteAddCylinderCommandWithLogging(command, logCapture);
                        break;
                    case "add-capsule":
                        logCapture.AppendLine("Executing add-capsule command...");
                        result = ExecuteAddCapsuleCommandWithLogging(command, logCapture);
                        break;
                    default:
                        string error = $"Unknown batch command: {command.action}";
                        logCapture.AppendLine($"❌ ERROR: {error}");
                        Debug.LogError($"[VibeUnityCLI] {error}");
                        return false;
                }
                
                return result;
            }
            catch (System.Exception e)
            {
                string error = $"Exception executing command {command.action}: {e.Message}";
                logCapture.AppendLine($"❌ ERROR: {error}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                Debug.LogError($"[VibeUnityCLI] {error}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes create-scene batch command
        /// </summary>
        private static bool ExecuteCreateSceneCommand(BatchCommand command)
        {
            string sceneName = command.name;
            string scenePath = !string.IsNullOrEmpty(command.path) ? command.path : "Assets/Scenes";
            string sceneType = !string.IsNullOrEmpty(command.type) ? command.type : "DefaultGameObjects";
            bool addToBuild = command.addToBuild;
            
            return CreateScene(sceneName, scenePath, sceneType, addToBuild);
        }
        
        /// <summary>
        /// Executes add-canvas batch command
        /// </summary>
        private static bool ExecuteAddCanvasCommand(BatchCommand command)
        {
            string canvasName = command.name;
            string sceneName = command.scene;
            string renderMode = !string.IsNullOrEmpty(command.renderMode) ? command.renderMode : "ScreenSpaceOverlay";
            int width = command.referenceWidth > 0 ? command.referenceWidth : 1920;
            int height = command.referenceHeight > 0 ? command.referenceHeight : 1080;
            string scaleMode = !string.IsNullOrEmpty(command.scaleMode) ? command.scaleMode : "ScaleWithScreenSize";
            int sortingOrder = command.sortingOrder;
            
            Vector3? worldPosition = null;
            if (command.worldPosition != null && command.worldPosition.Length == 3)
            {
                worldPosition = new Vector3(command.worldPosition[0], command.worldPosition[1], command.worldPosition[2]);
            }
            
            return AddCanvas(canvasName, sceneName, renderMode, width, height, scaleMode, sortingOrder, worldPosition);
        }
        
        /// <summary>
        /// Executes add-panel batch command
        /// </summary>
        private static bool ExecuteAddPanelCommand(BatchCommand command)
        {
            string panelName = command.name;
            string parentName = command.parent;
            string sceneName = command.scene;
            float width = command.width > 0 ? command.width : 200f;
            float height = command.height > 0 ? command.height : 200f;
            string anchor = !string.IsNullOrEmpty(command.anchor) ? command.anchor : "MiddleCenter";
            
            bool success = AddPanel(panelName, parentName, sceneName, width, height, anchor);
            
            // Apply position offset if specified
            if (success && command.position != null && command.position.Length >= 2)
            {
                GameObject panelGO = FindGameObjectInActiveScene(panelName);
                if (panelGO != null)
                {
                    RectTransform rectTransform = panelGO.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector2 offset = new Vector2(command.position[0], command.position[1]);
                        rectTransform.anchoredPosition = offset;
                    }
                }
            }
            
            return success;
        }
        
        /// <summary>
        /// Executes add-button batch command
        /// </summary>
        private static bool ExecuteAddButtonCommand(BatchCommand command)
        {
            string buttonName = command.name;
            string parentName = command.parent;
            string sceneName = command.scene;
            string buttonText = !string.IsNullOrEmpty(command.text) ? command.text : "Button";
            float width = command.width > 0 ? command.width : 160f;
            float height = command.height > 0 ? command.height : 30f;
            string anchor = !string.IsNullOrEmpty(command.anchor) ? command.anchor : "MiddleCenter";
            
            bool success = AddButton(buttonName, parentName, sceneName, buttonText, width, height, anchor);
            
            // Apply position offset if specified
            if (success && command.position != null && command.position.Length >= 2)
            {
                GameObject buttonGO = FindGameObjectInActiveScene(buttonName);
                if (buttonGO != null)
                {
                    RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector2 offset = new Vector2(command.position[0], command.position[1]);
                        rectTransform.anchoredPosition = offset;
                    }
                }
            }
            
            return success;
        }
        
        /// <summary>
        /// Executes add-text batch command
        /// </summary>
        private static bool ExecuteAddTextCommand(BatchCommand command)
        {
            string textName = command.name;
            string parentName = command.parent;
            string sceneName = command.scene;
            string textContent = !string.IsNullOrEmpty(command.text) ? command.text : "New Text";
            int fontSize = command.fontSize > 0 ? command.fontSize : 14;
            float width = command.width > 0 ? command.width : 200f;
            float height = command.height > 0 ? command.height : 50f;
            string anchor = !string.IsNullOrEmpty(command.anchor) ? command.anchor : "MiddleCenter";
            
            bool success = AddText(textName, parentName, sceneName, textContent, fontSize, width, height, anchor);
            
            // Apply position offset if specified
            if (success && command.position != null && command.position.Length >= 2)
            {
                GameObject textGO = FindGameObjectInActiveScene(textName);
                if (textGO != null)
                {
                    RectTransform rectTransform = textGO.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector2 offset = new Vector2(command.position[0], command.position[1]);
                        rectTransform.anchoredPosition = offset;
                    }
                    
                    // Apply color if specified
                    if (!string.IsNullOrEmpty(command.color))
                    {
                        Text textComponent = textGO.GetComponent<Text>();
                        if (textComponent != null && ColorUtility.TryParseHtmlString(command.color, out Color color))
                        {
                            textComponent.color = color;
                        }
                    }
                }
            }
            
            return success;
        }
        
        /// <summary>
        /// Executes add-scrollview batch command
        /// </summary>
        private static bool ExecuteAddScrollViewCommand(BatchCommand command)
        {
            string scrollViewName = command.name;
            string parentName = command.parent;
            string sceneName = command.scene;
            float width = command.width > 0 ? command.width : 300f;
            float height = command.height > 0 ? command.height : 200f;
            string anchor = !string.IsNullOrEmpty(command.anchor) ? command.anchor : "MiddleCenter";
            bool horizontal = command.horizontal;
            bool vertical = command.vertical;
            string scrollbarVisibility = !string.IsNullOrEmpty(command.scrollbarVisibility) ? command.scrollbarVisibility : "AutoHideAndExpandViewport";
            float scrollSensitivity = command.scrollSensitivity > 0 ? command.scrollSensitivity : 1.0f;
            
            bool success = AddScrollView(scrollViewName, parentName, sceneName, width, height, anchor, horizontal, vertical, scrollbarVisibility, scrollSensitivity);
            
            // Apply position offset if specified
            if (success && command.position != null && command.position.Length >= 2)
            {
                GameObject scrollViewGO = FindGameObjectInActiveScene(scrollViewName);
                if (scrollViewGO != null)
                {
                    RectTransform rectTransform = scrollViewGO.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector2 offset = new Vector2(command.position[0], command.position[1]);
                        rectTransform.anchoredPosition = offset;
                    }
                }
            }
            
            return success;
        }
        
        /// <summary>
        /// Executes add-cube batch command
        /// </summary>
        private static bool ExecuteAddCubeCommand(BatchCommand command)
        {
            return ExecutePrimitiveCommand(command, PrimitiveType.Cube);
        }
        
        /// <summary>
        /// Executes add-sphere batch command
        /// </summary>
        private static bool ExecuteAddSphereCommand(BatchCommand command)
        {
            return ExecutePrimitiveCommand(command, PrimitiveType.Sphere);
        }
        
        /// <summary>
        /// Executes add-plane batch command
        /// </summary>
        private static bool ExecuteAddPlaneCommand(BatchCommand command)
        {
            return ExecutePrimitiveCommand(command, PrimitiveType.Plane);
        }
        
        /// <summary>
        /// Executes add-cylinder batch command
        /// </summary>
        private static bool ExecuteAddCylinderCommand(BatchCommand command)
        {
            return ExecutePrimitiveCommand(command, PrimitiveType.Cylinder);
        }
        
        /// <summary>
        /// Executes add-capsule batch command
        /// </summary>
        private static bool ExecuteAddCapsuleCommand(BatchCommand command)
        {
            return ExecutePrimitiveCommand(command, PrimitiveType.Capsule);
        }
        
        /// <summary>
        /// Generic method to execute primitive GameObject creation
        /// </summary>
        private static bool ExecutePrimitiveCommand(BatchCommand command, PrimitiveType primitiveType)
        {
            try
            {
                string objectName = command.name;
                Vector3 position = Vector3.zero;
                Vector3 rotation = Vector3.zero;
                Vector3 scale = Vector3.one;
                
                // Parse position if specified
                if (command.position != null && command.position.Length >= 3)
                {
                    position = new Vector3(command.position[0], command.position[1], command.position[2]);
                }
                
                // Parse rotation if specified
                if (command.rotation != null && command.rotation.Length >= 3)
                {
                    rotation = new Vector3(command.rotation[0], command.rotation[1], command.rotation[2]);
                }
                
                // Parse scale if specified
                if (command.scale != null && command.scale.Length >= 3)
                {
                    scale = new Vector3(command.scale[0], command.scale[1], command.scale[2]);
                }
                
                // Create the primitive GameObject
                GameObject primitiveObject = GameObject.CreatePrimitive(primitiveType);
                primitiveObject.name = objectName;
                primitiveObject.transform.position = position;
                primitiveObject.transform.eulerAngles = rotation;
                primitiveObject.transform.localScale = scale;
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] Failed to create {primitiveType}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Finds a GameObject by name in the active scene
        /// </summary>
        private static GameObject FindGameObjectInActiveScene(string name)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            
            foreach (GameObject rootObj in rootObjects)
            {
                GameObject found = FindGameObjectRecursive(rootObj, name);
                if (found != null)
                {
                    return found;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Command line entry point for help - DISABLED
        /// Usage: Unity -batchmode -quit -executeMethod VibeUnity.Editor.CLI.ShowHelpFromCommandLine
        /// </summary>
        public static void ShowHelpFromCommandLine()
        {
            // CLI commands disabled - functionality commented out
            Debug.LogWarning("[VibeUnityCLI] CLI commands are disabled. Use internal API methods instead.");
            return;
            
            /*
            Debug.Log("[VibeUnityCLI] === Vibe Unity Help ===");
            Debug.Log("[VibeUnityCLI] ");
            Debug.Log("[VibeUnityCLI] SCENE CREATION:");
            Debug.Log("[VibeUnityCLI]   unity-create-scene <scene_name> <scene_path> [scene_type] [add_to_build]");
            Debug.Log("[VibeUnityCLI]   Example: unity-create-scene MyScene Assets/Scenes DefaultGameObjects false");
            Debug.Log("[VibeUnityCLI] ");
            Debug.Log("[VibeUnityCLI] ADD CANVAS:");
            Debug.Log("[VibeUnityCLI]   unity-add-canvas <canvas_name> <render_mode> [width] [height] [scale_mode]");
            Debug.Log("[VibeUnityCLI]   Example: unity-add-canvas UICanvas ScreenSpaceOverlay 1920 1080 ScaleWithScreenSize");
            Debug.Log("[VibeUnityCLI] ");
            Debug.Log("[VibeUnityCLI] BATCH FILE:");
            Debug.Log("[VibeUnityCLI]   unity-batch-file <json_file_path>");
            Debug.Log("[VibeUnityCLI]   Example: unity-batch-file ui-setup.json");
            Debug.Log("[VibeUnityCLI] ");
            Debug.Log("[VibeUnityCLI] LIST SCENE TYPES:");
            Debug.Log("[VibeUnityCLI]   unity-list-types");
            Debug.Log("[VibeUnityCLI] ");
            Debug.Log("[VibeUnityCLI] HELP:");
            Debug.Log("[VibeUnityCLI]   unity-cli-help");
            Debug.Log("[VibeUnityCLI] ");
            Debug.Log("[VibeUnityCLI] Available Scene Types: " + string.Join(", ", GetAvailableSceneTypes()));
            Debug.Log("[VibeUnityCLI] Available Render Modes: ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace");
            Debug.Log("[VibeUnityCLI] Available Scale Modes: ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize");
            Debug.Log("[VibeUnityCLI] ===============================");
            */
        }
        
        #endregion
        
        #region Logging Command Execution Wrappers
        
        /// <summary>
        /// Executes create-scene batch command with logging
        /// </summary>
        private static bool ExecuteCreateSceneCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            try
            {
                string sceneName = command.name;
                string scenePath = !string.IsNullOrEmpty(command.path) ? command.path : "Assets/Scenes";
                string sceneType = !string.IsNullOrEmpty(command.type) ? command.type : "DefaultGameObjects";
                bool addToBuild = command.addToBuild;
                
                logCapture.AppendLine($"Scene Name: {sceneName}");
                logCapture.AppendLine($"Scene Path: {scenePath}");
                logCapture.AppendLine($"Scene Type: {sceneType}");
                logCapture.AppendLine($"Add to Build: {addToBuild}");
                
                bool result = CreateScene(sceneName, scenePath, sceneType, addToBuild);
                
                if (result)
                {
                    logCapture.AppendLine($"✅ Scene created successfully: {scenePath}/{sceneName}.unity");
                }
                else
                {
                    logCapture.AppendLine($"❌ Failed to create scene: {scenePath}/{sceneName}.unity");
                }
                
                return result;
            }
            catch (System.Exception e)
            {
                logCapture.AppendLine($"❌ Exception in create-scene: {e.Message}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes add-canvas batch command with logging
        /// </summary>
        private static bool ExecuteAddCanvasCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            try
            {
                string canvasName = command.name;
                string renderMode = !string.IsNullOrEmpty(command.renderMode) ? command.renderMode : "ScreenSpaceOverlay";
                int width = command.referenceWidth > 0 ? command.referenceWidth : 1920;
                int height = command.referenceHeight > 0 ? command.referenceHeight : 1080;
                string scaleMode = !string.IsNullOrEmpty(command.scaleMode) ? command.scaleMode : "ScaleWithScreenSize";
                int sortingOrder = command.sortingOrder;
                
                Vector3? worldPosition = null;
                if (command.worldPosition != null && command.worldPosition.Length == 3)
                {
                    worldPosition = new Vector3(command.worldPosition[0], command.worldPosition[1], command.worldPosition[2]);
                }
                
                logCapture.AppendLine($"Canvas Name: {canvasName}");
                logCapture.AppendLine($"Render Mode: {renderMode}");
                logCapture.AppendLine($"Resolution: {width}x{height}");
                logCapture.AppendLine($"Scale Mode: {scaleMode}");
                logCapture.AppendLine($"Sorting Order: {sortingOrder}");
                if (worldPosition.HasValue)
                    logCapture.AppendLine($"World Position: {worldPosition.Value}");
                
                // Scene is already loaded at batch level, pass null to use current scene
                bool result = AddCanvas(canvasName, null, renderMode, width, height, scaleMode, sortingOrder, worldPosition);
                
                if (result)
                {
                    logCapture.AppendLine($"✅ Canvas created successfully: {canvasName}");
                }
                else
                {
                    logCapture.AppendLine($"❌ Failed to create canvas: {canvasName}");
                }
                
                return result;
            }
            catch (System.Exception e)
            {
                logCapture.AppendLine($"❌ Exception in add-canvas: {e.Message}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes add-panel batch command with logging
        /// </summary>
        private static bool ExecuteAddPanelCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            try
            {
                string panelName = command.name;
                string parentName = command.parent;
                string sceneName = command.scene;
                float width = command.width > 0 ? command.width : 200f;
                float height = command.height > 0 ? command.height : 200f;
                string anchor = !string.IsNullOrEmpty(command.anchor) ? command.anchor : "MiddleCenter";
                
                logCapture.AppendLine($"Panel Name: {panelName}");
                logCapture.AppendLine($"Parent: {parentName ?? "auto-detect"}");
                logCapture.AppendLine($"Size: {width}x{height}");
                logCapture.AppendLine($"Anchor: {anchor}");
                
                // Scene is already loaded at batch level, pass null to use current scene
                bool result = AddPanel(panelName, parentName, null, width, height, anchor);
                
                // Apply position offset if specified
                if (result && command.position != null && command.position.Length >= 2)
                {
                    GameObject panelGO = FindGameObjectInActiveScene(panelName);
                    if (panelGO != null)
                    {
                        RectTransform rectTransform = panelGO.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            Vector2 offset = new Vector2(command.position[0], command.position[1]);
                            rectTransform.anchoredPosition = offset;
                            logCapture.AppendLine($"Applied position offset: {offset}");
                        }
                    }
                }
                
                if (result)
                {
                    logCapture.AppendLine($"✅ Panel created successfully: {panelName}");
                }
                else
                {
                    logCapture.AppendLine($"❌ Failed to create panel: {panelName}");
                }
                
                return result;
            }
            catch (System.Exception e)
            {
                logCapture.AppendLine($"❌ Exception in add-panel: {e.Message}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes add-button batch command with logging
        /// </summary>
        private static bool ExecuteAddButtonCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            try
            {
                string buttonName = command.name;
                string parentName = command.parent;
                string sceneName = command.scene;
                string buttonText = !string.IsNullOrEmpty(command.text) ? command.text : "Button";
                float width = command.width > 0 ? command.width : 160f;
                float height = command.height > 0 ? command.height : 30f;
                string anchor = !string.IsNullOrEmpty(command.anchor) ? command.anchor : "MiddleCenter";
                
                logCapture.AppendLine($"Button Name: {buttonName}");
                logCapture.AppendLine($"Parent: {parentName ?? "auto-detect"}");
                logCapture.AppendLine($"Text: {buttonText}");
                logCapture.AppendLine($"Size: {width}x{height}");
                logCapture.AppendLine($"Anchor: {anchor}");
                
                // Scene is already loaded at batch level, pass null to use current scene
                bool result = AddButton(buttonName, parentName, null, buttonText, width, height, anchor);
                
                // Apply position offset if specified
                if (result && command.position != null && command.position.Length >= 2)
                {
                    GameObject buttonGO = FindGameObjectInActiveScene(buttonName);
                    if (buttonGO != null)
                    {
                        RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            Vector2 offset = new Vector2(command.position[0], command.position[1]);
                            rectTransform.anchoredPosition = offset;
                            logCapture.AppendLine($"Applied position offset: {offset}");
                        }
                    }
                }
                
                if (result)
                {
                    logCapture.AppendLine($"✅ Button created successfully: {buttonName}");
                }
                else
                {
                    logCapture.AppendLine($"❌ Failed to create button: {buttonName}");
                }
                
                return result;
            }
            catch (System.Exception e)
            {
                logCapture.AppendLine($"❌ Exception in add-button: {e.Message}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes add-text batch command with logging
        /// </summary>
        private static bool ExecuteAddTextCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            try
            {
                string textName = command.name;
                string parentName = command.parent;
                string sceneName = command.scene;
                string textContent = !string.IsNullOrEmpty(command.text) ? command.text : "New Text";
                int fontSize = command.fontSize > 0 ? command.fontSize : 14;
                float width = command.width > 0 ? command.width : 200f;
                float height = command.height > 0 ? command.height : 50f;
                string anchor = !string.IsNullOrEmpty(command.anchor) ? command.anchor : "MiddleCenter";
                
                logCapture.AppendLine($"Text Name: {textName}");
                logCapture.AppendLine($"Parent: {parentName ?? "auto-detect"}");
                logCapture.AppendLine($"Content: {textContent}");
                logCapture.AppendLine($"Font Size: {fontSize}");
                logCapture.AppendLine($"Size: {width}x{height}");
                logCapture.AppendLine($"Anchor: {anchor}");
                
                // Scene is already loaded at batch level, pass null to use current scene
                bool result = AddText(textName, parentName, null, textContent, fontSize, width, height, anchor);
                
                // Apply position offset and color if specified
                if (result && (command.position != null || !string.IsNullOrEmpty(command.color)))
                {
                    GameObject textGO = FindGameObjectInActiveScene(textName);
                    if (textGO != null)
                    {
                        RectTransform rectTransform = textGO.GetComponent<RectTransform>();
                        if (rectTransform != null && command.position != null && command.position.Length >= 2)
                        {
                            Vector2 offset = new Vector2(command.position[0], command.position[1]);
                            rectTransform.anchoredPosition = offset;
                            logCapture.AppendLine($"Applied position offset: {offset}");
                        }
                        
                        // Apply color if specified
                        if (!string.IsNullOrEmpty(command.color))
                        {
                            Text textComponent = textGO.GetComponent<Text>();
                            if (textComponent != null && ColorUtility.TryParseHtmlString(command.color, out Color color))
                            {
                                textComponent.color = color;
                                logCapture.AppendLine($"Applied color: {command.color}");
                            }
                            else
                            {
                                logCapture.AppendLine($"⚠️ Warning: Could not parse color: {command.color}");
                            }
                        }
                    }
                }
                
                if (result)
                {
                    logCapture.AppendLine($"✅ Text created successfully: {textName}");
                }
                else
                {
                    logCapture.AppendLine($"❌ Failed to create text: {textName}");
                }
                
                return result;
            }
            catch (System.Exception e)
            {
                logCapture.AppendLine($"❌ Exception in add-text: {e.Message}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes add-scrollview batch command with logging
        /// </summary>
        private static bool ExecuteAddScrollViewCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            try
            {
                string scrollViewName = command.name;
                string parentName = command.parent;
                string sceneName = command.scene;
                float width = command.width > 0 ? command.width : 300f;
                float height = command.height > 0 ? command.height : 200f;
                string anchor = !string.IsNullOrEmpty(command.anchor) ? command.anchor : "MiddleCenter";
                bool horizontal = command.horizontal;
                bool vertical = command.vertical;
                string scrollbarVisibility = !string.IsNullOrEmpty(command.scrollbarVisibility) ? command.scrollbarVisibility : "AutoHideAndExpandViewport";
                float scrollSensitivity = command.scrollSensitivity > 0 ? command.scrollSensitivity : 1.0f;
                
                logCapture.AppendLine($"ScrollView Name: {scrollViewName}");
                logCapture.AppendLine($"Parent: {parentName ?? "auto-detect"}");
                logCapture.AppendLine($"Size: {width}x{height}");
                logCapture.AppendLine($"Anchor: {anchor}");
                logCapture.AppendLine($"Horizontal: {horizontal}");
                logCapture.AppendLine($"Vertical: {vertical}");
                logCapture.AppendLine($"Scrollbar Visibility: {scrollbarVisibility}");
                logCapture.AppendLine($"Scroll Sensitivity: {scrollSensitivity}");
                
                bool result = AddScrollView(scrollViewName, parentName, sceneName, width, height, anchor, horizontal, vertical, scrollbarVisibility, scrollSensitivity);
                
                if (result)
                {
                    GameObject scrollViewGO = FindGameObjectInActiveScene(scrollViewName);
                    if (scrollViewGO != null)
                    {
                        RectTransform rectTransform = scrollViewGO.GetComponent<RectTransform>();
                        if (rectTransform != null && command.position != null && command.position.Length >= 2)
                        {
                            Vector2 offset = new Vector2(command.position[0], command.position[1]);
                            rectTransform.anchoredPosition = offset;
                            logCapture.AppendLine($"Applied position offset: {offset}");
                        }
                    }
                }
                
                if (result)
                {
                    logCapture.AppendLine($"✅ ScrollView created successfully: {scrollViewName}");
                }
                else
                {
                    logCapture.AppendLine($"❌ Failed to create scrollview: {scrollViewName}");
                }
                
                return result;
            }
            catch (System.Exception e)
            {
                logCapture.AppendLine($"❌ Exception in add-scrollview: {e.Message}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes add-cube batch command with logging
        /// </summary>
        private static bool ExecuteAddCubeCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            return ExecutePrimitiveCommandWithLogging(command, logCapture, PrimitiveType.Cube, "Cube");
        }
        
        /// <summary>
        /// Executes add-sphere batch command with logging
        /// </summary>
        private static bool ExecuteAddSphereCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            return ExecutePrimitiveCommandWithLogging(command, logCapture, PrimitiveType.Sphere, "Sphere");
        }
        
        /// <summary>
        /// Executes add-plane batch command with logging
        /// </summary>
        private static bool ExecuteAddPlaneCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            return ExecutePrimitiveCommandWithLogging(command, logCapture, PrimitiveType.Plane, "Plane");
        }
        
        /// <summary>
        /// Executes add-cylinder batch command with logging
        /// </summary>
        private static bool ExecuteAddCylinderCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            return ExecutePrimitiveCommandWithLogging(command, logCapture, PrimitiveType.Cylinder, "Cylinder");
        }
        
        /// <summary>
        /// Executes add-capsule batch command with logging
        /// </summary>
        private static bool ExecuteAddCapsuleCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture)
        {
            return ExecutePrimitiveCommandWithLogging(command, logCapture, PrimitiveType.Capsule, "Capsule");
        }
        
        /// <summary>
        /// Generic method to execute primitive GameObject creation with logging
        /// </summary>
        private static bool ExecutePrimitiveCommandWithLogging(BatchCommand command, System.Text.StringBuilder logCapture, PrimitiveType primitiveType, string typeName)
        {
            try
            {
                string objectName = command.name;
                Vector3 position = Vector3.zero;
                Vector3 rotation = Vector3.zero;
                Vector3 scale = Vector3.one;
                
                // Parse position if specified
                if (command.position != null && command.position.Length >= 3)
                {
                    position = new Vector3(command.position[0], command.position[1], command.position[2]);
                }
                
                logCapture.AppendLine($"{typeName} Name: {objectName}");
                logCapture.AppendLine($"Position: {position}");
                logCapture.AppendLine($"Scale: {scale}");
                
                // Create the primitive GameObject
                GameObject primitiveObject = GameObject.CreatePrimitive(primitiveType);
                primitiveObject.name = objectName;
                primitiveObject.transform.position = position;
                primitiveObject.transform.rotation = Quaternion.Euler(rotation);
                primitiveObject.transform.localScale = scale;
                
                logCapture.AppendLine($"✅ {typeName} created successfully: {objectName}");
                logCapture.AppendLine($"   └─ Position: {primitiveObject.transform.position}");
                logCapture.AppendLine($"   └─ Has Collider: {primitiveObject.GetComponent<Collider>() != null}");
                logCapture.AppendLine($"   └─ Has Renderer: {primitiveObject.GetComponent<Renderer>() != null}");
                
                return true;
            }
            catch (System.Exception e)
            {
                logCapture.AppendLine($"❌ Exception in add-{typeName.ToLower()}: {e.Message}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Ensures a scene is loaded based on scene configuration, with detailed logging
        /// </summary>
        /// <param name="sceneConfig">Scene configuration specifying how to handle the scene</param>
        /// <param name="logCapture">StringBuilder to capture log output</param>
        /// <returns>True if scene was successfully loaded or created</returns>
        private static bool EnsureSceneLoadedWithSceneConfig(SceneConfig sceneConfig, System.Text.StringBuilder logCapture)
        {
            try
            {
                string sceneName = sceneConfig.name;
                logCapture.AppendLine($"Processing scene: '{sceneName}'");
                
                // First, try to find the scene file
                string sceneAssetPath = FindSceneAsset(sceneName);
                
                if (!string.IsNullOrEmpty(sceneAssetPath))
                {
                    // Scene exists, try to load it
                    logCapture.AppendLine($"✅ Scene file found: {sceneAssetPath}");
                    
                    try
                    {
                        Scene targetScene = EditorSceneManager.OpenScene(sceneAssetPath);
                        if (targetScene.IsValid())
                        {
                            logCapture.AppendLine($"✅ Scene loaded successfully: {sceneName}");
                            logCapture.AppendLine($"   └─ Root GameObjects: {targetScene.rootCount}");
                            logCapture.AppendLine($"   └─ Scene Path: {targetScene.path}");
                            return true;
                        }
                        else
                        {
                            logCapture.AppendLine($"❌ Scene loaded but is invalid: {sceneName}");
                            return false;
                        }
                    }
                    catch (System.Exception e)
                    {
                        logCapture.AppendLine($"❌ Failed to load scene: {e.Message}");
                        return false;
                    }
                }
                else
                {
                    // Scene doesn't exist
                    if (sceneConfig.create)
                    {
                        // Create the scene as requested
                        logCapture.AppendLine($"⚠️ Scene not found, creating as requested: {sceneName}");
                        logCapture.AppendLine($"   └─ Path: {sceneConfig.path}");
                        logCapture.AppendLine($"   └─ Type: {sceneConfig.type}");
                        logCapture.AppendLine($"   └─ Add to Build: {sceneConfig.addToBuild}");
                        
                        // Create the scene with specified parameters
                        bool sceneCreated = CreateScene(sceneName, sceneConfig.path, sceneConfig.type, sceneConfig.addToBuild);
                        
                        if (sceneCreated)
                        {
                            logCapture.AppendLine($"✅ Scene created and loaded: {sceneName}");
                            logCapture.AppendLine($"   └─ Full Path: {sceneConfig.path}/{sceneName}.unity");
                            return true;
                        }
                        else
                        {
                            logCapture.AppendLine($"❌ Failed to create scene: {sceneName}");
                            return false;
                        }
                    }
                    else
                    {
                        // Scene doesn't exist and creation not requested
                        logCapture.AppendLine($"❌ Scene '{sceneName}' not found and 'create' is false");
                        logCapture.AppendLine($"   └─ Set scene.create = true to auto-create missing scenes");
                        return false;
                    }
                }
            }
            catch (System.Exception e)
            {
                logCapture.AppendLine($"❌ Exception in EnsureSceneLoadedWithSceneConfig: {e.Message}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Ensures a scene is loaded, creating it if necessary, with detailed logging (Legacy method)
        /// </summary>
        /// <param name="sceneName">Name of the scene (without .unity extension)</param>
        /// <param name="logCapture">StringBuilder to capture log output</param>
        /// <returns>True if scene was successfully loaded or created</returns>
        private static bool EnsureSceneLoadedWithLogging(string sceneName, System.Text.StringBuilder logCapture)
        {
            try
            {
                logCapture.AppendLine($"Checking scene: '{sceneName}'");
                
                // First, try to find the scene file
                string sceneAssetPath = FindSceneAsset(sceneName);
                
                if (!string.IsNullOrEmpty(sceneAssetPath))
                {
                    // Scene exists, try to load it
                    logCapture.AppendLine($"✅ Scene file found: {sceneAssetPath}");
                    
                    try
                    {
                        Scene targetScene = EditorSceneManager.OpenScene(sceneAssetPath);
                        if (targetScene.IsValid())
                        {
                            logCapture.AppendLine($"✅ Scene loaded successfully: {sceneName}");
                            logCapture.AppendLine($"   └─ Root GameObjects: {targetScene.rootCount}");
                            logCapture.AppendLine($"   └─ Scene Path: {targetScene.path}");
                            return true;
                        }
                        else
                        {
                            logCapture.AppendLine($"❌ Scene loaded but is invalid: {sceneName}");
                            return false;
                        }
                    }
                    catch (System.Exception e)
                    {
                        logCapture.AppendLine($"❌ Failed to load scene: {e.Message}");
                        return false;
                    }
                }
                else
                {
                    // Scene doesn't exist, create it
                    logCapture.AppendLine($"⚠️ Scene not found, creating new scene: {sceneName}");
                    
                    // Use default path and settings for new scenes
                    string defaultPath = "Assets/Scenes";
                    string sceneType = "DefaultGameObjects";
                    bool addToBuild = false;
                    
                    logCapture.AppendLine($"   └─ Path: {defaultPath}");
                    logCapture.AppendLine($"   └─ Type: {sceneType}");
                    logCapture.AppendLine($"   └─ Add to Build: {addToBuild}");
                    
                    // Create the scene
                    bool sceneCreated = CreateScene(sceneName, defaultPath, sceneType, addToBuild);
                    
                    if (sceneCreated)
                    {
                        logCapture.AppendLine($"✅ Scene created and loaded: {sceneName}");
                        logCapture.AppendLine($"   └─ Full Path: {defaultPath}/{sceneName}.unity");
                        return true;
                    }
                    else
                    {
                        logCapture.AppendLine($"❌ Failed to create scene: {sceneName}");
                        return false;
                    }
                }
            }
            catch (System.Exception e)
            {
                logCapture.AppendLine($"❌ Exception in EnsureSceneLoaded: {e.Message}");
                logCapture.AppendLine($"Stack Trace: {e.StackTrace}");
                return false;
            }
        }
        
        #endregion
        
        #region File Watcher System
        
        /// <summary>
        /// Directory where CLI drops command files for Unity to pick up
        /// </summary>
        private static readonly string COMMAND_QUEUE_DIR = Path.Combine(Application.dataPath, "..", ".vibe-commands");
        
        /// <summary>
        /// Initialize the file watcher system
        /// </summary>
        [InitializeOnLoadMethod]
        private static void InitializeFileWatcher()
        {
            // Ensure command queue directory exists
            if (!Directory.Exists(COMMAND_QUEUE_DIR))
            {
                Directory.CreateDirectory(COMMAND_QUEUE_DIR);
            }
            
            // Start watching for command files if enabled
            if (VibeUnityMenu.IsFileWatcherEnabled)
            {
                EditorApplication.update += CheckForCommandFiles;
                Debug.Log("[VibeUnityCLI] File watcher initialized. Watching: " + COMMAND_QUEUE_DIR);
            }
        }
        
        /// <summary>
        /// Check for new command files and execute them
        /// </summary>
        private static void CheckForCommandFiles()
        {
            try
            {
                if (!Directory.Exists(COMMAND_QUEUE_DIR))
                    return;
                
                string[] commandFiles = Directory.GetFiles(COMMAND_QUEUE_DIR, "*.json");
                
                foreach (string filePath in commandFiles)
                {
                    // Skip if file is being written (check if locked)
                    if (IsFileLocked(filePath))
                        continue;
                    
                    string fileName = Path.GetFileName(filePath);
                    Debug.Log($"[VibeUnityCLI] Found command file: {fileName}");
                    
                    // Start log capture
                    var logCapture = new System.Text.StringBuilder();
                    logCapture.AppendLine($"=== Processing {fileName} ===");
                    logCapture.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    logCapture.AppendLine($"File Path: {filePath}");
                    logCapture.AppendLine();
                    
                    bool success = false;
                    try
                    {
                        // Execute the batch file with log capture
                        success = ExecuteBatchFileWithLogging(filePath, logCapture);
                    }
                    catch (Exception ex)
                    {
                        logCapture.AppendLine($"FATAL ERROR during execution: {ex.Message}");
                        logCapture.AppendLine($"Stack Trace: {ex.StackTrace}");
                        success = false;
                    }
                    
                    // Create processed directory
                    string processedDir = Path.Combine(COMMAND_QUEUE_DIR, "processed");
                    if (!Directory.Exists(processedDir))
                        Directory.CreateDirectory(processedDir);
                    
                    string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string processedJsonPath = Path.Combine(processedDir, $"{timestamp}-{fileName}");
                    string logFilePath = Path.Combine(processedDir, $"{timestamp}-{fileNameWithoutExt}.log");
                    
                    // Save the log file
                    logCapture.AppendLine();
                    logCapture.AppendLine($"=== Processing Complete ===");
                    logCapture.AppendLine($"Result: {(success ? "SUCCESS" : "FAILED")}");
                    logCapture.AppendLine($"End Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    
                    try
                    {
                        File.WriteAllText(logFilePath, logCapture.ToString());
                        Debug.Log($"[VibeUnityCLI] Log saved to: {logFilePath}");
                    }
                    catch (Exception logEx)
                    {
                        Debug.LogError($"[VibeUnityCLI] Failed to save log file: {logEx.Message}");
                    }
                    
                    // Move the original JSON file
                    try
                    {
                        File.Move(filePath, processedJsonPath);
                        Debug.Log($"[VibeUnityCLI] Command file processed and moved to: {processedJsonPath}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[VibeUnityCLI] Could not move processed file: {e.Message}");
                        // Try to delete instead
                        try
                        {
                            File.Delete(filePath);
                            Debug.Log($"[VibeUnityCLI] Command file deleted: {fileName}");
                        }
                        catch
                        {
                            Debug.LogError($"[VibeUnityCLI] Could not delete command file: {filePath}");
                        }
                    }
                    
                    if (success)
                    {
                        Debug.Log($"[VibeUnityCLI] ✅ File watcher command executed successfully - see {logFilePath}");
                    }
                    else
                    {
                        Debug.LogError($"[VibeUnityCLI] ❌ File watcher command execution failed - see {logFilePath}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnityCLI] Error in file watcher: {e.Message}");
            }
        }
        
        /// <summary>
        /// Check if a file is currently locked (being written to)
        /// </summary>
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // File watcher toggle moved to VibeUnityMenu.cs
        
        /// <summary>
        /// Enable the file watcher
        /// </summary>
        public static void EnableFileWatcher()
        {
            EditorApplication.update -= CheckForCommandFiles;
            EditorApplication.update += CheckForCommandFiles;
            Debug.Log("[VibeUnityCLI] File watcher enabled");
        }
        
        /// <summary>
        /// Disable the file watcher
        /// </summary>
        public static void DisableFileWatcher()
        {
            EditorApplication.update -= CheckForCommandFiles;
            Debug.Log("[VibeUnityCLI] File watcher disabled");
        }
        
        #endregion       
    }
}
#endif