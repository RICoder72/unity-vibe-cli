#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace UnityVibe.Editor
{
    /// <summary>
    /// Handles automatic setup of Unity Vibe CLI when package is imported
    /// </summary>
    [InitializeOnLoad]
    public static class UnityVibeSetup
    {
        private const string SETUP_COMPLETE_KEY = "UnityVibeCLI_SetupComplete";
        
        static UnityVibeSetup()
        {
            // Only run setup once per project
            if (!EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
            {
                EditorApplication.delayCall += RunSetup;
            }
        }
        
        private static void RunSetup()
        {
            Debug.Log("[Unity Vibe CLI] Setting up CLI tools...");
            
            // Copy bash scripts to project root for WSL users
            CopyBashScriptsToProjectRoot();
            
            // Mark setup as complete
            EditorPrefs.SetBool(SETUP_COMPLETE_KEY, true);
            
            // Show welcome message
            ShowWelcomeDialog();
        }
        
        private static void CopyBashScriptsToProjectRoot()
        {
            try
            {
                string packagePath = GetPackagePath();
                if (string.IsNullOrEmpty(packagePath)) return;
                
                string scriptsPath = Path.Combine(packagePath, "Scripts");
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                
                // Copy vibe-unity script
                string sourceScript = Path.Combine(scriptsPath, "vibe-unity");
                string targetScript = Path.Combine(projectRoot, "vibe-unity");
                
                if (File.Exists(sourceScript))
                {
                    File.Copy(sourceScript, targetScript, true);
                    Debug.Log($"[Unity Vibe CLI] Copied CLI script to: {targetScript}");
                }
                
                // Copy install script
                string sourceInstaller = Path.Combine(scriptsPath, "install-vibe-unity");
                string targetInstaller = Path.Combine(projectRoot, "Scripts", "install-vibe-unity");
                
                if (File.Exists(sourceInstaller))
                {
                    Directory.CreateDirectory(Path.Combine(projectRoot, "Scripts"));
                    File.Copy(sourceInstaller, targetInstaller, true);
                    Debug.Log($"[Unity Vibe CLI] Copied installer to: {targetInstaller}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Unity Vibe CLI] Could not copy bash scripts: {e.Message}");
            }
        }
        
        private static string GetPackagePath()
        {
            // Try to find the package in various locations
            string[] searchPaths = {
                Path.Combine(Application.dataPath, "..", "Packages", "com.unityvibe.cli"),
                Path.Combine(Application.dataPath, "..", "Library", "PackageCache", "com.unityvibe.cli@1.0.0"),
                Path.Combine(Application.dataPath, "UnityVibeCLI") // Local package
            };
            
            foreach (string path in searchPaths)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
            }
            
            return null;
        }
        
        private static void ShowWelcomeDialog()
        {
            EditorApplication.delayCall += () =>
            {
                bool showDialog = EditorUtility.DisplayDialog(
                    "Unity Vibe CLI",
                    "Unity Vibe CLI has been installed!\n\n" +
                    "You can now:\n" +
                    "• Use the C# API: CLI.CreateScene(), CLI.AddCanvas()\n" +
                    "• Access Tools > Unity Vibe CLI menu\n" +
                    "• WSL users: Run ./vibe-unity from project root\n\n" +
                    "Would you like to see the documentation?",
                    "Open Documentation",
                    "Close"
                );
                
                if (showDialog)
                {
                    Application.OpenURL("https://github.com/unity-vibe/unity-vibe-cli#readme");
                }
            };
        }
        
        [MenuItem("Tools/Unity Vibe CLI/Reset Setup", priority = 500)]
        private static void ResetSetup()
        {
            EditorPrefs.DeleteKey(SETUP_COMPLETE_KEY);
            Debug.Log("[Unity Vibe CLI] Setup reset. Restart Unity to run setup again.");
        }
    }
}
#endif