# Vibe Unity Changelog

All notable changes to the Vibe Unity package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.5.2] - 2025-08-05

### Added
- Automatic creation of logs/latest.log for easy access to processing results
- Smart timeout handling in claude-compile-check.sh to prevent infinite loops
- Improved error detection in test scripts with better log parsing

### Fixed
- Directory structure moved from .vibe-commands to .vibe-unity/commands for better organization
- GetPackageVersion accessibility issue in menu system
- Scene creation JSON format corrected in documentation and examples
- Compile check script now properly detects when Unity is already compiled
- Log directories are now created automatically during command processing

### Changed
- Updated all references from .vibe-commands to .vibe-unity/commands throughout codebase
- Enhanced test procedures with better error reporting and verification
- Improved documentation with correct JSON format examples

## [1.5.1] - 2025-08-05

### Added
- Automatic CLAUDE.md documentation updates when package version changes
- Version tracking for claude-compile-check.sh script with smart update detection
- Menu item to manually update CLAUDE.md documentation (Tools > Vibe Unity > Update CLAUDE.md Documentation)

### Fixed
- Line ending preservation in SetupClaudeCompileCheckScript to ensure Unix LF endings on all platforms
- Script version comparison to only update when content actually changes
- Documentation now only regenerates when package version changes, reducing unnecessary writes

### Changed
- Improved compile check script installation with better version tracking and logging
- Documentation updater now tracks last documented version to avoid redundant updates

## [1.5.0] - 2025-01-05

### Added
- Unity-based compilation tracking using `EditorApplication.isCompiling` API for more reliable detection
- File locking mechanism in `.vibe-unity/status/compilation.json` for atomic compilation state management
- Real-time compilation monitoring without complex external progress detection

### Changed
- **BREAKING**: claude-compile-check.sh now uses Unity's internal compilation status instead of external Windows API detection
- Simplified compilation detection by removing complex PowerShell window enumeration
- Improved reliability and cross-platform compatibility for compilation checking

### Fixed
- Eliminated false timeouts during compilation by using Unity's authoritative internal state
- Reduced CPU overhead by removing Windows API calls for progress detection
- More accurate compilation completion detection with precise start/end timestamps

## [1.4.4] - 2025-01-05

### Fixed
- Enhanced installer to automatically add claude-compile-check.sh to .gitignore to prevent line ending corruption
- Installer now preserves Unix LF line endings when copying claude-compile-check.sh to project root
- Prevents Git from modifying line endings in the compilation check script
- Ensures script remains executable on Unix systems after installation

## [1.4.3] - 2025-01-04

### Fixed
- Resolved Windows line ending issues in claude-compile-check.sh causing bash syntax errors
- Added .gitattributes to enforce LF line endings for shell scripts across platforms
- Prevents future line ending corruption during git operations

## [1.4.2] - 2025-01-02

### Fixed
- Fixed Windows line endings in bash scripts (already had Unix line endings)
- Enhanced Unity window detection to find the correct project window when multiple Unity instances are open
- Improved timeout handling with log checking and retry logic (up to 2 retries)
- Added proper "nothing to compile" detection for up-to-date scripts

### Added
- Project-aware Unity window detection using current directory name
- Smart retry mechanism when no compilation activity is detected
- Better compilation log analysis after timeout
- Enhanced documentation for new features

### Changed
- Updated claude-compile-check.sh to version 1.3.1
- Improved error messages and user feedback
- Enhanced PowerShell Unity window detection logic

## [1.4.1] - 2025-01-02

### Fixed
- Fixed missing claude-compile-check.sh script in package installation
- Ensured all package files are properly deployed from vibe-unity source
- Added proper Scripts directory with README documentation

### Added
- Scripts/README.md documentation for claude-compile-check.sh

## [1.4.0] - 2025-01-02

### Added
- Ultra-efficient manual JSON scene state export functionality
- Improved scene creation capabilities
- Enhanced error reporting for better debugging

### Fixed
- Critical bugs in scene creation workflow
- Various error reporting issues
- Deprecated FindObjectsOfType replaced with FindObjectsByType
- Missing System using directive
- Bash script line endings issues

## Previous versions
- See git history for changes prior to 1.4.0