using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace VibeUnity.Editor
{
    /// <summary>
    /// GameObject operations and utilities for Vibe Unity
    /// </summary>
    public static class VibeUnityGameObjects
    {
        /// <summary>
        /// Finds a GameObject by name in the active scene
        /// </summary>
        public static GameObject FindInActiveScene(string name)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            
            foreach (GameObject rootObj in rootObjects)
            {
                GameObject found = FindRecursive(rootObj, name);
                if (found != null)
                {
                    return found;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Recursively searches for a GameObject by name
        /// </summary>
        public static GameObject FindRecursive(GameObject parent, string targetName)
        {
            if (parent.name == targetName)
            {
                return parent;
            }
            
            foreach (Transform child in parent.transform)
            {
                GameObject found = FindRecursive(child.gameObject, targetName);
                if (found != null)
                {
                    return found;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the full hierarchy path of a GameObject
        /// </summary>
        public static string GetPath(GameObject gameObject)
        {
            if (gameObject == null)
                return "null";
                
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
        /// Lists all available GameObjects in the active scene
        /// </summary>
        public static string ListAvailable()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
                return "No active scene";
                
            var names = new List<string>();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            
            foreach (GameObject rootObj in rootObjects)
            {
                CollectNames(rootObj, names, "");
            }
            
            return names.Count > 0 ? string.Join(", ", names) : "No GameObjects found";
        }
        
        /// <summary>
        /// Collects all GameObject names recursively
        /// </summary>
        private static void CollectNames(GameObject parent, List<string> names, string prefix)
        {
            string currentPath = string.IsNullOrEmpty(prefix) ? parent.name : prefix + "/" + parent.name;
            names.Add(currentPath);
            
            foreach (Transform child in parent.transform)
            {
                CollectNames(child.gameObject, names, currentPath);
            }
        }
        
        /// <summary>
        /// Adds a component to a GameObject by type name
        /// </summary>
        public static Component AddComponent(GameObject target, string componentTypeName)
        {
            try
            {
                // Handle common component type aliases
                string normalizedTypeName = NormalizeComponentTypeName(componentTypeName);
                
                // Try to find the type by name
                System.Type componentType = GetComponentTypeByName(normalizedTypeName);
                if (componentType == null)
                {
                    Debug.LogError($"[VibeUnity] Component type '{componentTypeName}' not found");
                    return null;
                }
                
                // Add the component
                Component component = target.AddComponent(componentType);
                if (component != null)
                {
                    Debug.Log($"[VibeUnity] Successfully added component '{componentTypeName}' to '{target.name}'");
                    return component;
                }
                else
                {
                    Debug.LogError($"[VibeUnity] Failed to add component '{componentTypeName}' to '{target.name}'");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VibeUnity] Exception adding component: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Normalizes component type names to handle common aliases
        /// </summary>
        private static string NormalizeComponentTypeName(string typeName)
        {
            // Remove common suffixes
            if (typeName.EndsWith("Component"))
                typeName = typeName.Substring(0, typeName.Length - 9);
                
            // Handle common aliases
            switch (typeName.ToLower())
            {
                case "rigidbody": return "Rigidbody";
                case "collider": return "BoxCollider";
                case "boxcollider": return "BoxCollider";
                case "spherecollider": return "SphereCollider";
                case "capsulecollider": return "CapsuleCollider";
                case "meshcollider": return "MeshCollider";
                case "camera": return "Camera";
                case "light": return "Light";
                case "audiosource": return "AudioSource";
                case "audiolistener": return "AudioListener";
                case "animator": return "Animator";
                case "rigidbody2d": return "Rigidbody2D";
                case "collider2d": return "BoxCollider2D";
                case "boxcollider2d": return "BoxCollider2D";
                case "circlecollider2d": return "CircleCollider2D";
                default: return typeName;
            }
        }
        
        /// <summary>
        /// Gets a component type by name, searching common Unity namespaces
        /// </summary>
        private static System.Type GetComponentTypeByName(string typeName)
        {
            // Try direct type lookup
            System.Type type = System.Type.GetType(typeName);
            if (type != null) return type;
            
            // Try with UnityEngine namespace
            type = System.Type.GetType($"UnityEngine.{typeName}, UnityEngine");
            if (type != null) return type;
            
            // Try with UnityEngine.UI namespace
            type = System.Type.GetType($"UnityEngine.UI.{typeName}, UnityEngine.UI");
            if (type != null) return type;
            
            // Search through all loaded assemblies
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null && type.IsSubclassOf(typeof(Component)))
                    return type;
                    
                // Try with UnityEngine prefix
                type = assembly.GetType($"UnityEngine.{typeName}");
                if (type != null && type.IsSubclassOf(typeof(Component)))
                    return type;
            }
            
            return null;
        }
        
        /// <summary>
        /// Sets component parameters from a dictionary
        /// </summary>
        public static bool SetComponentParameters(Component component, ComponentParameter[] parameters, System.Text.StringBuilder logCapture = null)
        {
            if (component == null || parameters == null || parameters.Length == 0)
                return true;
                
            bool allSuccess = true;
            
            foreach (var param in parameters)
            {
                if (!SetComponentParameter(component, param, logCapture))
                {
                    allSuccess = false;
                }
            }
            
            return allSuccess;
        }
        
        /// <summary>
        /// Sets a single component parameter
        /// </summary>
        private static bool SetComponentParameter(Component component, ComponentParameter param, System.Text.StringBuilder logCapture = null)
        {
            try
            {
                var type = component.GetType();
                var property = type.GetProperty(param.name);
                
                if (property != null && property.CanWrite)
                {
                    object value = ConvertParameterValue(param.value, param.type, property.PropertyType);
                    property.SetValue(component, value);
                    logCapture?.AppendLine($"   └─ Set {param.name} = {param.value}");
                    return true;
                }
                
                var field = type.GetField(param.name);
                if (field != null)
                {
                    object value = ConvertParameterValue(param.value, param.type, field.FieldType);
                    field.SetValue(component, value);
                    logCapture?.AppendLine($"   └─ Set {param.name} = {param.value}");
                    return true;
                }
                
                logCapture?.AppendLine($"   └─ Warning: Parameter '{param.name}' not found on component");
                return false;
            }
            catch (System.Exception e)
            {
                logCapture?.AppendLine($"   └─ Error setting parameter '{param.name}': {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Converts a parameter value from string to the target type
        /// </summary>
        private static object ConvertParameterValue(string value, string paramType, System.Type targetType)
        {
            try
            {
                // Handle null/empty values
                if (string.IsNullOrEmpty(value))
                {
                    return targetType.IsValueType ? System.Activator.CreateInstance(targetType) : null;
                }
                
                // Handle common types
                if (targetType == typeof(int))
                    return int.Parse(value);
                if (targetType == typeof(float))
                    return float.Parse(value);
                if (targetType == typeof(double))
                    return double.Parse(value);
                if (targetType == typeof(bool))
                    return bool.Parse(value);
                if (targetType == typeof(string))
                    return value;
                    
                // Handle Unity types
                if (targetType == typeof(Vector3))
                {
                    string[] parts = value.Split(',');
                    if (parts.Length >= 3)
                    {
                        return new Vector3(
                            float.Parse(parts[0].Trim()),
                            float.Parse(parts[1].Trim()),
                            float.Parse(parts[2].Trim())
                        );
                    }
                }
                
                if (targetType == typeof(Vector2))
                {
                    string[] parts = value.Split(',');
                    if (parts.Length >= 2)
                    {
                        return new Vector2(
                            float.Parse(parts[0].Trim()),
                            float.Parse(parts[1].Trim())
                        );
                    }
                }
                
                if (targetType == typeof(Color))
                {
                    Color color;
                    if (ColorUtility.TryParseHtmlString(value, out color))
                        return color;
                        
                    string[] parts = value.Split(',');
                    if (parts.Length >= 3)
                    {
                        return new Color(
                            float.Parse(parts[0].Trim()),
                            float.Parse(parts[1].Trim()),
                            float.Parse(parts[2].Trim()),
                            parts.Length > 3 ? float.Parse(parts[3].Trim()) : 1f
                        );
                    }
                }
                
                // Try generic conversion
                return System.Convert.ChangeType(value, targetType);
            }
            catch
            {
                // Return default value for type if conversion fails
                return targetType.IsValueType ? System.Activator.CreateInstance(targetType) : null;
            }
        }
    }
    
    /// <summary>
    /// Represents a component parameter for setting values
    /// </summary>
    [System.Serializable]
    public class ComponentParameter
    {
        public string name;
        public string value;
        public string type;
    }
}