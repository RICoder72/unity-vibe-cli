# Changelog

All notable changes to Unity Vibe CLI will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.1] - 2025-08-01

### Fixed
- **Compilation Issues**: Fixed missing `using System;` directive in VibeUnitySystem.cs causing compilation errors
- **Script Compatibility**: Fixed Windows line endings (CRLF) in claude-compile-check.sh bash script for proper Linux/WSL execution
- **Asset Tracking**: Added missing Unity .meta files for proper asset tracking and version control

## [1.3.0] - 2025-08-01

### Added
- **Scene State System**: Complete scene export/import functionality using JSON state files
  - Export Unity scenes to comprehensive `.state.json` files stored alongside scene files
  - Import and rebuild scenes from JSON state with full component reconstruction
  - Scene state files serve as the authoritative source of truth for scene structure
- **Coverage Analysis & Gap Detection**: Advanced analysis of scene export/import capabilities
  - Real-time gap detection during scene export identifying unsupported components and features
  - Detailed coverage reports showing supported vs unsupported components with percentages
  - Structured logging of missing features with severity levels (Critical, Warning, Info)
  - Development roadmap generation based on most frequently missing components
- **Unity Editor Integration**: Comprehensive menu system for scene state management
  - `Tools > Vibe Unity > Scene State > Export Current Scene` - Manual scene state export
  - `Tools > Vibe Unity > Scene State > Import from State File...` - Scene import from JSON
  - `Tools > Vibe Unity > Scene State > Show Coverage Report` - View latest coverage analysis
  - `Tools > Vibe Unity > Scene State > Open Coverage Directory` - Browse analysis reports
- **Automatic State Generation**: Scene state files automatically generated on scene save and batch processing
  - Auto-export triggered by Unity's scene save events
  - Integrated into existing JSON batch processing workflow
  - Background processing to avoid blocking Unity Editor operations
- **Comprehensive Data Structures**: Complete scene representation with full component detail
  - Scene metadata, settings, and render pipeline information
  - GameObject hierarchy with parent-child relationships and transform data
  - Component introspection with property values and asset references
  - UI-specific data for Canvas, RectTransform, and UI components
  - Asset reference tracking for materials, textures, and fonts

### Changed
- Enhanced JSON batch processing to automatically generate scene state artifacts after successful operations
- Extended development workflow to include scene state validation and gap analysis
- Updated Unity Editor menu system with new Scene State submenu organization
- Improved component coverage tracking with support for 15+ component types including UI, 3D primitives, Camera, and Audio

### Technical Details
- Added `VibeUnitySceneState.cs` with comprehensive data structures for JSON serialization
- Implemented `VibeUnitySceneExporter.cs` with complete scene scanning and gap detection
- Created `VibeUnitySceneImporter.cs` for full scene reconstruction from JSON state
- Enhanced `VibeUnitySystem.cs` with automatic scene save hooks and state generation
- Coverage analysis generates reports in `.vibe-commands/coverage-analysis/` directory
- Import logs saved to `.vibe-commands/import-logs/` for debugging and validation
- Scene state files follow naming convention `[SceneName].state.json` alongside Unity scene files
- Full support for UI components (Canvas, Button, Text, ScrollView, Image, TextMeshPro)
- Component property introspection using reflection with type-safe value conversion

### For Claude-Code Integration
- Scene state JSON files provide human-readable scene structure for AI analysis
- Gap detection identifies exactly which components need implementation for full scene rebuilding
- Coverage reports guide package development priorities based on real scene usage
- State files enable version control of scene structure with meaningful diffs
- Import functionality allows claude-code to generate scenes by creating JSON state files

## [1.2.0] - 2025-08-01

### Added
- Complete development workflow automation with test-workflow.sh script
- Automatic Unity window focusing and compilation monitoring
- Smart compilation detection via Unity Editor.log parsing
- Integrated testing workflow that ensures compilation completion before testing

### Changed
- Re-enabled file watcher system by default (can be toggled via Tools menu)
- Improved development workflow to eliminate timing issues between code changes and testing
- Enhanced main thread dispatcher system for reliable Editor API execution

### Technical Details
- Added PowerShell-based window management for Unity/Ubuntu focus switching
- Implemented real-time Unity Editor.log monitoring for compilation completion detection
- Created comprehensive test workflow script with timeout protection and progress reporting
- Maintained thread-safe operation queuing from previous version

## [1.1.1] - 2025-08-01

### Fixed
- Fixed threading issue with FileSystemWatcher where Unity Editor API calls were being made from background threads
- Added main thread dispatcher to properly queue Editor API operations from file watcher events
- Resolved "GetAllRegisteredPackages can only be called from the main thread" error

### Technical Details
- Added ConcurrentQueue for thread-safe operation queuing
- Implemented EditorApplication.update handler for main thread processing
- Modified file watcher event handlers to use main thread dispatcher
- All Unity Editor API calls now properly execute on the main thread

## [1.1.0] - 2025-08-01

### Changed
- Version bump for package testing workflow
- Minor updates to package structure

## [1.0.0] - 2024-01-30

### Added
- Initial release of Unity Vibe CLI
- Scene creation functionality with multiple templates
- Canvas management with configurable parameters
- WSL integration with bash scripts
- Command-line interface for automated workflows
- Unity Package Manager support
- Cross-platform compatibility (Windows, macOS, Linux)
- Comprehensive documentation and examples

### Features
- **Scene Creation**: Create Unity scenes using CLI with various templates (Empty, DefaultGameObjects, 2D, 3D, URP, HDRP)
- **Canvas Management**: Add and configure UI canvases with different render modes and scaling options
- **CLI Commands**: 
  - `unity-create-scene` - Create new Unity scenes
  - `unity-add-canvas` - Add canvases to existing scenes
  - `unity-list-types` - List available scene types
  - `unity-cli-help` - Show help information
- **Unity Integration**: Menu items for debugging and testing CLI functionality
- **Build Settings**: Automatic integration with Unity's build settings
- **Error Handling**: Comprehensive validation and error reporting
- **Documentation**: Detailed README with examples and troubleshooting guide

### Technical Details
- Compatible with Unity 2022.3+
- Uses Unity's native APIs for maximum compatibility
- No external dependencies required
- Extensible architecture for adding new commands
- Support for Unity's scene template system
- WSL-compatible bash scripts for Windows users

### Package Structure
```
unity-vibe-cli/
├── Assets/
│   └── Editor/
│       └── UnityVibeCLI.cs          # Main CLI implementation
├── Scripts/                         # WSL/Bash integration scripts
│   ├── unity-create-scene
│   ├── unity-add-canvas
│   ├── unity-list-types
│   └── unity-cli-help
├── package.json                     # Unity Package Manager manifest
├── README.md                        # Comprehensive documentation
├── LICENSE                          # MIT License
├── CHANGELOG.md                     # This file
└── .gitignore                       # Git ignore rules
```

## [Unreleased]

### Planned Features
- Scene template creation and management
- Project structure analysis and export
- Custom build pipeline integration
- Plugin system for extending functionality
- GUI tool for non-CLI users
- Docker support for containerized builds
- GitHub Actions integration examples
- Advanced canvas animation presets
- Scene transition management
- Asset bundle preparation tools

---

For support and feature requests, please visit our [GitHub repository](https://github.com/unity-vibe/unity-vibe-cli).