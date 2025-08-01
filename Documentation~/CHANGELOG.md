# Changelog

All notable changes to Unity Vibe CLI will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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