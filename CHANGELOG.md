# Vibe Unity Changelog

All notable changes to the Vibe Unity package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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