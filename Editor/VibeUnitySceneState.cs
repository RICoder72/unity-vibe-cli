using UnityEngine;
using System;
using System.Collections.Generic;

namespace VibeUnity.Editor
{
    /// <summary>
    /// Complete scene state representation for JSON serialization
    /// Serves as the authoritative source of truth for Unity scenes
    /// </summary>
    [Serializable]
    public class SceneState
    {
        public string version = "1.0";
        public SceneMetadata metadata;
        public SceneSettings settings;
        public GameObjectInfo[] gameObjects;
        public string[] assetReferences;
        public CoverageReport coverageReport;
        
        [NonSerialized]
        public DateTime exportTime;
    }
    
    /// <summary>
    /// Scene metadata and identification
    /// </summary>
    [Serializable]
    public class SceneMetadata
    {
        public string sceneName;
        public string scenePath;
        public string unityVersion;
        public string exportedBy = "VibeUnity";
        public string exportTimestamp;
        public int totalGameObjects;
        public int totalComponents;
        public string[] renderPipelines;
    }
    
    /// <summary>
    /// Scene-level settings and configuration
    /// </summary>
    [Serializable]
    public class SceneSettings
    {
        public LightingSettings lighting;
        public RenderSettings render;
        public PhysicsSettings physics;
        public AudioSettings audio;
    }
    
    /// <summary>
    /// Lighting configuration
    /// </summary>
    [Serializable]
    public class LightingSettings
    {
        public bool realtimeGI;
        public bool bakedGI;
        public float[] ambientColor;
        public string ambientMode;
        public float[] skyboxTint;
        public float ambientIntensity;
    }
    
    /// <summary>
    /// Render settings
    /// </summary>
    [Serializable]
    public class RenderSettings
    {
        public bool fog;
        public float[] fogColor;
        public string fogMode;
        public float fogDensity;
        public float fogStartDistance;
        public float fogEndDistance;
        public string skyboxMaterial;
    }
    
    /// <summary>
    /// Physics settings
    /// </summary>
    [Serializable]
    public class PhysicsSettings
    {
        public float[] gravity;
        public string defaultMaterial;
        public float bounceThreshold;
        public int defaultSolverIterations;
        public int defaultSolverVelocityIterations;
    }
    
    /// <summary>
    /// Audio settings
    /// </summary>
    [Serializable]
    public class AudioSettings
    {
        public float volume;
        public float dopplerFactor;
        public float speedOfSound;
    }
    
    /// <summary>
    /// Complete GameObject representation with hierarchy and components
    /// </summary>
    [Serializable]
    public class GameObjectInfo
    {
        public string name;
        public string hierarchyPath;
        public int instanceId;
        public bool isActive;
        public string tag;
        public int layer;
        public bool isStatic;
        
        // Hierarchy information
        public string parentPath;
        public string[] childrenPaths;
        public int siblingIndex;
        
        // Transform data
        public TransformInfo transform;
        
        // All components on this GameObject
        public ComponentInfo[] components;
        
        // Special UI data if this is a UI element
        public UIElementInfo uiInfo;
    }
    
    /// <summary>
    /// Transform information with full precision
    /// </summary>
    [Serializable]
    public class TransformInfo
    {
        public float[] localPosition;
        public float[] localRotation; // Quaternion (x,y,z,w)
        public float[] localScale;
        public float[] worldPosition;
        public float[] worldRotation;
        public float[] worldScale;
    }
    
    /// <summary>
    /// Generic component information
    /// </summary>
    [Serializable]
    public class ComponentInfo
    {
        public string typeName;
        public string fullTypeName;
        public bool enabled;
        public ComponentProperty[] properties;
        public bool isSupported;
        public string[] missingFeatures;
    }
    
    /// <summary>
    /// Component property with type information
    /// </summary>
    [Serializable]
    public class ComponentProperty
    {
        public string name;
        public string value;
        public string typeName;
        public bool isSerializable;
        public bool isAssetReference;
        public string assetPath;
    }
    
    /// <summary>
    /// Specialized UI element information
    /// </summary>
    [Serializable]
    public class UIElementInfo
    {
        public RectTransformInfo rectTransform;
        public CanvasInfo canvas;
        public string canvasGroup;
        public LayoutElementInfo layoutElement;
        public GraphicInfo graphic;
    }
    
    /// <summary>
    /// RectTransform-specific data
    /// </summary>
    [Serializable]
    public class RectTransformInfo
    {
        public float[] anchoredPosition;
        public float[] anchorMin;
        public float[] anchorMax;
        public float[] offsetMin;
        public float[] offsetMax;
        public float[] pivot;
        public float[] sizeDelta;
    }
    
    /// <summary>
    /// Canvas component information
    /// </summary>
    [Serializable]
    public class CanvasInfo
    {
        public string renderMode;
        public float[] worldCamera;
        public float planeDistance;
        public int sortingOrder;
        public string sortingLayerName;
        public bool overrideSorting;
        public float scaleFactor;
        public float[] referenceResolution;
        public string scaleMode;
        public float matchWidthOrHeight;
    }
    
    /// <summary>
    /// Layout element information
    /// </summary>
    [Serializable]
    public class LayoutElementInfo
    {
        public bool ignoreLayout;
        public float minWidth;
        public float minHeight;
        public float preferredWidth;
        public float preferredHeight;
        public float flexibleWidth;
        public float flexibleHeight;
        public int layoutPriority;
    }
    
    /// <summary>
    /// Graphic component information (Image, Text, etc.)
    /// </summary>
    [Serializable]
    public class GraphicInfo
    {
        public string graphicType;
        public float[] color;
        public string material;
        public bool raycastTarget;
        
        // Image-specific
        public string sprite;
        public string imageType;
        public bool preserveAspect;
        
        // Text-specific
        public string text;
        public string font;
        public int fontSize;
        public string fontStyle;
        public string alignment;
        public bool richText;
    }
    
    /// <summary>
    /// Coverage analysis report for gap detection
    /// </summary>
    [Serializable]
    public class CoverageReport
    {
        public CoverageSummary summary;
        public ComponentCoverage[] componentCoverage;
        public Gap[] gaps;
        public string[] recommendations;
    }
    
    /// <summary>
    /// Overall coverage summary
    /// </summary>
    [Serializable]
    public class CoverageSummary
    {
        public int totalGameObjects;
        public int totalComponents;
        public int supportedComponents;
        public int partiallySupported;
        public int unsupportedComponents;
        public float coveragePercentage;
        public bool canFullyRebuild;
    }
    
    /// <summary>
    /// Per-component type coverage information
    /// </summary>
    [Serializable]
    public class ComponentCoverage
    {
        public string componentType;
        public int instanceCount;
        public bool isSupported;
        public string supportLevel; // "Full", "Partial", "None"
        public string[] supportedProperties;
        public string[] missingProperties;
        public string[] missingFeatures;
    }
    
    /// <summary>
    /// Individual gap or missing feature
    /// </summary>
    [Serializable]
    public class Gap
    {
        public string category; // "Component", "Property", "Feature", "Asset"
        public string severity; // "Critical", "Warning", "Info"
        public string componentType;
        public string missingItem;
        public string description;
        public int affectedCount;
        public string[] examplePaths;
        public string recommendation;
    }
    
    /// <summary>
    /// Asset reference tracking
    /// </summary>
    [Serializable]
    public class AssetReference
    {
        public string assetPath;
        public string assetType;
        public string guid;
        public bool exists;
        public string[] referencedBy;
    }
}