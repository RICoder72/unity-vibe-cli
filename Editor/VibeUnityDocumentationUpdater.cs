#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System;

namespace VibeUnity.Editor
{
    /// <summary>
    /// Automatically updates CLAUDE.md with current Vibe Unity integration information
    /// </summary>
    public static class VibeUnityDocumentationUpdater
    {
        private const string SECTION_START_MARKER = "# Vibe Unity Integration Guide";
        private const string SECTION_END_MARKER = "*This section is automatically maintained by Vibe Unity";
        private const string VERSION_PATTERN = @"v(\d+\.\d+\.\d+)";
        
        /// <summary>
        /// Initialize documentation updater on Unity startup
        /// </summary>
        [InitializeOnLoadMethod]
        public static void InitializeDocumentationUpdater()
        {
            // Run after a short delay to ensure other systems are initialized
            EditorApplication.delayCall += () =>
            {
                try
                {
                    UpdateClaudeMdDocumentation();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[VibeUnity] Failed to update CLAUDE.md documentation: {e.Message}");
                }
            };
        }
        
        /// <summary>
        /// Updates the CLAUDE.md file with current Vibe Unity integration information
        /// </summary>
        public static bool UpdateClaudeMdDocumentation()
        {
            try
            {
                // Find CLAUDE.md file in project root
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string claudeMdPath = Path.Combine(projectRoot, "CLAUDE.md");
                
                if (!File.Exists(claudeMdPath))
                {
                    Debug.Log("[VibeUnity] CLAUDE.md not found - skipping documentation update");
                    return false;
                }
                
                // Get current package version
                string packageVersion = GetPackageVersion();
                if (string.IsNullOrEmpty(packageVersion))
                {
                    Debug.LogWarning("[VibeUnity] Could not determine package version - skipping documentation update");
                    return false;
                }
                
                // Read existing content
                string existingContent = File.ReadAllText(claudeMdPath);
                
                // Check if update is needed
                if (!NeedsUpdate(existingContent, packageVersion))
                {
                    Debug.Log($"[VibeUnity] CLAUDE.md documentation is up to date (v{packageVersion})");
                    return true;
                }
                
                // Generate new documentation section
                string newSection = GenerateDocumentationSection(packageVersion);
                
                // Update the file
                string updatedContent = UpdateDocumentationSection(existingContent, newSection);
                
                // Write back to file
                File.WriteAllText(claudeMdPath, updatedContent);
                
                Debug.Log($"[VibeUnity] ✅ Updated CLAUDE.md with Vibe Unity integration guide (v{packageVersion})");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnity] Error updating CLAUDE.md: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets the current package version from package.json
        /// </summary>
        private static string GetPackageVersion()
        {
            try
            {
                string packageJsonPath = Path.Combine(Application.dataPath, "..", "Packages", "com.ricoder.vibe-unity", "package.json");
                if (!File.Exists(packageJsonPath))
                    return null;
                    
                string packageJson = File.ReadAllText(packageJsonPath);
                var match = Regex.Match(packageJson, @"""version"":\s*""([^""]+)""");
                
                return match.Success ? match.Groups[1].Value : null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Checks if the documentation section needs updating
        /// </summary>
        private static bool NeedsUpdate(string existingContent, string currentVersion)
        {
            // If section doesn't exist, we need to add it
            if (!existingContent.Contains(SECTION_START_MARKER))
                return true;
            
            // Check if version has changed
            var versionMatch = Regex.Match(existingContent, $@"{SECTION_START_MARKER}.*?{VERSION_PATTERN}", RegexOptions.Singleline);
            if (!versionMatch.Success)
                return true;
                
            string existingVersion = versionMatch.Groups[1].Value;
            return existingVersion != currentVersion;
        }
        
        /// <summary>
        /// Generates the complete documentation section
        /// </summary>
        private static string GenerateDocumentationSection(string version)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"# Vibe Unity Integration Guide (Auto-generated - v{version})");
            sb.AppendLine();
            sb.AppendLine("## Quick Reference for Claude-Code");
            sb.AppendLine();
            sb.AppendLine("### Scene State System (Primary Integration Point)");
            sb.AppendLine("- **Scene Files**: `.state.json` alongside Unity scenes in `Assets/Scenes/`");
            sb.AppendLine("- **Coverage Reports**: `.vibe-commands/coverage-analysis/` - shows what components are supported");
            sb.AppendLine("- **Auto-generation**: State files created automatically on scene save and batch processing");
            sb.AppendLine();
            sb.AppendLine("### Essential Commands for Claude-Code");
            sb.AppendLine("```bash");
            sb.AppendLine("# Compilation validation (for claude-code script changes)");
            sb.AppendLine("./claude-compile-check.sh                # Check compilation, return errors if found");
            sb.AppendLine("./claude-compile-check.sh --include-warnings  # Include warning details");
            sb.AppendLine("");
            sb.AppendLine("# Manual testing workflow (after compilation passes)");
            sb.AppendLine("Tools > Vibe Unity > Force Recompile    # Ensure code changes compiled");
            sb.AppendLine("Tools > Vibe Unity > Run Test File       # Process test-scene-creation.json");
            sb.AppendLine("Tools > Vibe Unity > Scene State > Export Current Scene");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("### Batch Processing (JSON-Driven Scene Creation)");
            sb.AppendLine("- **Drop JSON files** in `.vibe-commands/` for automatic processing");
            sb.AppendLine("- **Test file**: `.vibe-commands/test-scene-creation.json`");
            sb.AppendLine("- **Supported actions**: create-scene, add-canvas, add-button, add-text, add-scrollview, add-cube, etc.");
            sb.AppendLine();
            sb.AppendLine($"### Current Component Support (v{version})");
            sb.AppendLine("- ✅ **UI**: Canvas, Button, Text, Image, ScrollView, TextMeshPro");
            sb.AppendLine("- ✅ **3D**: Cube, Sphere, Plane, Cylinder, Capsule, Camera, Light");
            sb.AppendLine("- ⚠️ **Partial**: Rigidbody, Colliders");
            sb.AppendLine("- ❌ **Missing**: ParticleSystem, custom scripts, animations");
            sb.AppendLine();
            sb.AppendLine("### Compilation Validation for Claude-Code");
            sb.AppendLine("- **Script**: `./claude-compile-check.sh` (auto-installed with package)");
            sb.AppendLine("- **Purpose**: Validate Unity script changes without running tests");
            sb.AppendLine("- **Output**: Structured error/warning reports with file:line locations");
            sb.AppendLine("- **Exit Codes**: 0=success, 1=errors, 2=timeout, 3=script error");
            sb.AppendLine("- **Usage**: Run after making C# changes to verify compilation");
            sb.AppendLine();
            sb.AppendLine("### Development Workflow Status");
            sb.AppendLine("- **File Watcher**: ✅ ENABLED (automatic JSON processing)");
            sb.AppendLine("- **HTTP Server**: DISABLED");
            sb.AppendLine("- **CLI Commands**: DISABLED");
            sb.AppendLine("- **Manual Testing**: ✅ ACTIVE (Use Unity menu items)");
            sb.AppendLine("- **Claude Compile Check**: ✅ INSTALLED (automatic deployment)");
            sb.AppendLine();
            sb.AppendLine("## For Detailed Usage");
            sb.AppendLine("- **Full Documentation**: [Package README](./Packages/com.ricoder.vibe-unity/README.md)");
            sb.AppendLine("- **JSON Schema Examples**: [Package Test Files](./Packages/com.ricoder.vibe-unity/.vibe-commands/)");
            sb.AppendLine("- **Coverage Analysis**: Check latest report in `.vibe-commands/coverage-analysis/`");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine($"*This section is automatically maintained by Vibe Unity v{version}*");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Updates or adds the documentation section in the existing content
        /// </summary>
        private static string UpdateDocumentationSection(string existingContent, string newSection)
        {
            // Find existing section boundaries
            int sectionStart = existingContent.IndexOf(SECTION_START_MARKER);
            
            if (sectionStart >= 0)
            {
                // Find the end of the existing section
                int sectionEndSearch = existingContent.IndexOf(SECTION_END_MARKER, sectionStart);
                if (sectionEndSearch >= 0)
                {
                    // Find the end of the line containing the end marker
                    int sectionEnd = existingContent.IndexOf('\n', sectionEndSearch);
                    if (sectionEnd >= 0)
                        sectionEnd++; // Include the newline
                    else
                        sectionEnd = existingContent.Length;
                    
                    // Replace the existing section
                    return existingContent.Substring(0, sectionStart) + 
                           newSection + 
                           (sectionEnd < existingContent.Length ? "\n" + existingContent.Substring(sectionEnd) : "");
                }
                else
                {
                    // Section start found but no end marker - replace from start marker to end
                    return existingContent.Substring(0, sectionStart) + newSection;
                }
            }
            else
            {
                // No existing section - add at the end
                return existingContent.TrimEnd() + "\n\n" + newSection + "\n";
            }
        }
        
        /// <summary>
        /// Manual menu item to force documentation update
        /// </summary>
        [MenuItem("Tools/Vibe Unity/Update CLAUDE.md Documentation", priority = 300)]
        public static void ForceUpdateDocumentation()
        {
            if (UpdateClaudeMdDocumentation())
            {
                EditorUtility.DisplayDialog("Documentation Update", 
                    "CLAUDE.md has been updated with current Vibe Unity integration information.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Documentation Update Failed", 
                    "Failed to update CLAUDE.md. Check the console for error details.", 
                    "OK");
            }
        }
    }
}
#endif