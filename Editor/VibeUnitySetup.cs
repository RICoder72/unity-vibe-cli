#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace VibeUnity.Editor
{
    /// <summary>
    /// Handles automatic setup of Vibe Unity when package is imported
    /// </summary>
    [InitializeOnLoad]
    public static class VibeUnitySetup
    {
        private const string SETUP_COMPLETE_KEY = "VibeUnity_SetupComplete";
        
        static VibeUnitySetup()
        {
            // Only run setup once per project
            if (!EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
            {
                EditorApplication.delayCall += RunSetup;
            }
        }
        
        private static void RunSetup()
        {
            Debug.Log("[Vibe Unity] Setting up CLI tools...");
            
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
                
                // Copy claude-compile-check.sh script with LF line endings preserved
                string sourceCompileCheck = Path.Combine(scriptsPath, "claude-compile-check.sh");
                string targetCompileCheck = Path.Combine(projectRoot, "claude-compile-check.sh");
                
                if (File.Exists(sourceCompileCheck))
                {
                    // Read with preserved line endings and write to preserve LF
                    string content = File.ReadAllText(sourceCompileCheck);
                    // Ensure Unix line endings (LF only)
                    content = content.Replace("\r\n", "\n").Replace("\r", "\n");
                    File.WriteAllText(targetCompileCheck, content);
                    
                    // Make executable on Unix systems
                    if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                    {
                        System.Diagnostics.Process.Start("chmod", $"+x \"{targetCompileCheck}\"");
                    }
                    
                    Debug.Log($"[Vibe Unity] Copied compilation check script to: {targetCompileCheck}");
                    
                    // Add to .gitignore to prevent line ending issues
                    AddToGitIgnore(projectRoot, "claude-compile-check.sh");
                }
                
                // Copy vibe-unity script
                string sourceScript = Path.Combine(scriptsPath, "vibe-unity");
                string targetScript = Path.Combine(projectRoot, "vibe-unity");
                
                if (File.Exists(sourceScript))
                {
                    File.Copy(sourceScript, targetScript, true);
                    Debug.Log($"[Vibe Unity] Copied CLI script to: {targetScript}");
                }
                
                // Copy install script
                string sourceInstaller = Path.Combine(scriptsPath, "install-vibe-unity");
                string targetInstaller = Path.Combine(projectRoot, "Scripts", "install-vibe-unity");
                
                if (File.Exists(sourceInstaller))
                {
                    Directory.CreateDirectory(Path.Combine(projectRoot, "Scripts"));
                    File.Copy(sourceInstaller, targetInstaller, true);
                    Debug.Log($"[Vibe Unity] Copied installer to: {targetInstaller}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Vibe Unity] Could not copy bash scripts: {e.Message}");
            }
        }
        
        public static void AddToGitIgnore(string projectRoot, string fileName)
        {
            try
            {
                string gitIgnorePath = Path.Combine(projectRoot, ".gitignore");
                string entryToAdd = fileName;
                
                // Check if .gitignore exists
                if (File.Exists(gitIgnorePath))
                {
                    string existingContent = File.ReadAllText(gitIgnorePath);
                    
                    // Check if the entry already exists
                    if (!existingContent.Contains(entryToAdd))
                    {
                        // Add the entry with a comment
                        string newEntry = $"\n# Vibe Unity - auto-generated script with Unix line endings\n{entryToAdd}\n";
                        File.AppendAllText(gitIgnorePath, newEntry);
                        Debug.Log($"[Vibe Unity] Added {fileName} to .gitignore to preserve line endings");
                    }
                }
                else
                {
                    // Create .gitignore with the entry
                    string content = $"# Vibe Unity - auto-generated script with Unix line endings\n{entryToAdd}\n";
                    File.WriteAllText(gitIgnorePath, content);
                    Debug.Log($"[Vibe Unity] Created .gitignore and added {fileName}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Vibe Unity] Could not update .gitignore: {e.Message}");
            }
        }
        
        private static string GetPackagePath()
        {
            // Try to find the package in various locations
            string[] searchPaths = {
                Path.Combine(Application.dataPath, "..", "Packages", "com.vibe.unity"),
                Path.Combine(Application.dataPath, "..", "Library", "PackageCache", "com.vibe.unity@1.0.0"),
                Path.Combine(Application.dataPath, "VibeUnity") // Local package
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
                    "Vibe Unity",
                    "Unity Vibe CLI has been installed!\n\n" +
                    "You can now:\n" +
                    "• Use the C# API: CLI.CreateScene(), CLI.AddCanvas()\n" +
                    "• Access Tools > Vibe Unity menu\n" +
                    "• WSL users: Run ./vibe-unity from project root\n\n" +
                    "Would you like to see the documentation?",
                    "Open Documentation",
                    "Close"
                );
                
                if (showDialog)
                {
                    Application.OpenURL("https://github.com/RICoder72/vibe-unity#readme");
                }
            };
        }
        
        // Reset setup functionality removed - menu consolidated in VibeUnityMenu.cs
    }
}
#endif