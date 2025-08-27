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
                
                string runtimePath = Path.Combine(packagePath, "Runtime");
                string scriptsPath = Path.Combine(packagePath, "Scripts");
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                
                // Copy claude-compile-check.sh script with LF line endings preserved
                string sourceCompileCheck = Path.Combine(scriptsPath, "claude-compile-check.sh");
                string targetCompileCheck = Path.Combine(projectRoot, "claude-compile-check.sh");
                
                if (File.Exists(sourceCompileCheck))
                {
                    // Always overwrite the compile check script to ensure latest version
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
                    
                    Debug.Log($"[Vibe Unity] Updated compilation check script to latest version: {targetCompileCheck}");
                    
                    // Add to .gitignore to prevent line ending issues
                    AddToGitIgnore(projectRoot, "claude-compile-check.sh");
                }
                
                // Copy vibe-unity script from Runtime folder
                string sourceScript = Path.Combine(runtimePath, "vibe-unity");
                string targetScript = Path.Combine(projectRoot, "Scripts", "vibe-unity");
                
                if (File.Exists(sourceScript))
                {
                    Directory.CreateDirectory(Path.Combine(projectRoot, "Scripts"));
                    
                    // Read with preserved line endings and write to preserve LF
                    string content = File.ReadAllText(sourceScript);
                    // Ensure Unix line endings (LF only)
                    content = content.Replace("\r\n", "\n").Replace("\r", "\n");
                    File.WriteAllText(targetScript, content);
                    
                    // Make executable on Unix systems
                    if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                    {
                        System.Diagnostics.Process.Start("chmod", $"+x \"{targetScript}\"");
                    }
                    
                    Debug.Log($"[Vibe Unity] Copied CLI script to: {targetScript}");
                    
                    // Add to .gitignore to prevent line ending issues
                    AddToGitIgnore(projectRoot, "vibe-unity");
                }
                
                // Copy install script from Runtime folder
                string sourceInstaller = Path.Combine(runtimePath, "install-vibe-unity");
                string targetInstaller = Path.Combine(projectRoot, "Scripts", "install-vibe-unity");
                
                if (File.Exists(sourceInstaller))
                {
                    
                    // Read with preserved line endings and write to preserve LF
                    string content = File.ReadAllText(sourceInstaller);
                    // Ensure Unix line endings (LF only)
                    content = content.Replace("\r\n", "\n").Replace("\r", "\n");
                    File.WriteAllText(targetInstaller, content);
                    
                    // Make executable on Unix systems
                    if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                    {
                        System.Diagnostics.Process.Start("chmod", $"+x \"{targetInstaller}\"");
                    }
                    
                    Debug.Log($"[Vibe Unity] Copied installer to: {targetInstaller}");
                    
                    // Add to .gitignore to prevent line ending issues
                    AddToGitIgnore(projectRoot, "Scripts/install-vibe-unity");
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
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            
            string[] searchPaths = {
                // Git URL packages are cached in Library/PackageCache with various naming patterns
                Path.Combine(projectRoot, "Library", "PackageCache", "com.vibe.unity@*"),
                Path.Combine(projectRoot, "Library", "PackageCache", "com.ricoder.vibe-unity@*"),
                // Local package in Packages folder
                Path.Combine(projectRoot, "Packages", "com.vibe.unity"),
                Path.Combine(projectRoot, "Packages", "com.ricoder.vibe-unity"),
                // Embedded package
                Path.Combine(Application.dataPath, "VibeUnity")
            };
            
            foreach (string searchPattern in searchPaths)
            {
                if (searchPattern.Contains("@*"))
                {
                    // Handle wildcard patterns for package cache
                    string directory = Path.GetDirectoryName(searchPattern);
                    if (Directory.Exists(directory))
                    {
                        string pattern = Path.GetFileName(searchPattern);
                        string[] matchingDirs = Directory.GetDirectories(directory, pattern.Replace("@*", "@*"));
                        if (matchingDirs.Length > 0)
                        {
                            // Return the first (and typically only) match
                            return matchingDirs[0];
                        }
                    }
                }
                else if (Directory.Exists(searchPattern))
                {
                    return searchPattern;
                }
            }
            
            // Fallback: try to use Unity's PackageManager API
            try
            {
                var listRequest = UnityEditor.PackageManager.Client.List();
                while (!listRequest.IsCompleted)
                {
                    System.Threading.Thread.Sleep(10);
                }
                
                if (listRequest.Status == UnityEditor.PackageManager.StatusCode.Success)
                {
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name.Contains("vibe-unity") || package.name.Contains("com.vibe.unity"))
                        {
                            return package.resolvedPath;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Vibe Unity] Could not use PackageManager API: {e.Message}");
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
        
        /// <summary>
        /// Force re-run setup (called from menu)
        /// </summary>
        public static void ForceRunSetup()
        {
            RunSetup();
        }
    }
}
#endif