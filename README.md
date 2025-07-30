# Unity Vibe CLI

A powerful Command Line Interface tool for Unity development workflow automation. Streamline your Unity development with CLI-based scene creation, canvas management, and project structure operations.

![Unity Version](https://img.shields.io/badge/Unity-2022.3+-blue.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey.svg)

## 🚀 Features

- **CLI-based Scene Creation**: Create Unity scenes using command line with various templates
- **Canvas Management**: Add and configure UI canvases with customizable parameters
- **WSL Integration**: Native Windows Subsystem for Linux support with bash scripts
- **Scene Template Support**: Works with Unity's built-in scene templates
- **Build Settings Integration**: Automatically add created scenes to build settings
- **Extensible Architecture**: Easy to extend with new CLI commands
- **Cross-Platform**: Works on Windows, macOS, and Linux
- **No External Dependencies**: Uses only Unity's native APIs

## 📦 Installation

### Method 1: Unity Package Manager (Recommended)

1. Open Unity Package Manager
2. Click the `+` button and select "Add package from git URL"
3. Enter: `https://github.com/unity-vibe/unity-vibe-cli.git`

### Method 2: Manual Installation

1. Download or clone this repository
2. Copy the contents to your Unity project's `Packages` folder
3. Unity will automatically import the package

### Method 3: Unity Package

1. Download the latest `.unitypackage` from [Releases](https://github.com/unity-vibe/unity-vibe-cli/releases)
2. Import into your Unity project via `Assets > Import Package > Custom Package`

## 🛠️ Quick Start

### Basic Usage (C# API)

```csharp
using UnityVibe.Editor;

// Create a new scene
CLI.CreateScene("MyScene", "Assets/Scenes", "DefaultGameObjects", true);

// Add a canvas to current scene
CLI.AddCanvas("MainCanvas", "ScreenSpaceOverlay", 1920, 1080, "ScaleWithScreenSize");

// List available scene types
CLI.ListSceneTypes();
```

### Command Line Usage

#### Windows/WSL
```bash
# Add Scripts directory to your PATH or run from project root

# Create a new scene
./Scripts/unity-create-scene MyScene Assets/Scenes DefaultGameObjects true

# Add a canvas
./Scripts/unity-add-canvas MainCanvas ScreenSpaceOverlay 1920 1080

# List available scene types
./Scripts/unity-list-types

# Show help
./Scripts/unity-cli-help
```

#### Direct Unity Command Line
```bash
# Create scene
Unity -batchmode -quit -projectPath "path/to/project" -executeMethod UnityVibe.Editor.CLI.CreateSceneFromCommandLine MyScene Assets/Scenes DefaultGameObjects true

# Add canvas
Unity -batchmode -quit -projectPath "path/to/project" -executeMethod UnityVibe.Editor.CLI.AddCanvasFromCommandLine MainCanvas ScreenSpaceOverlay 1920 1080
```

## 📚 Documentation

### Scene Creation

Create Unity scenes with various templates and configurations:

```csharp
// Basic scene creation
CLI.CreateScene("GameScene", "Assets/Scenes/Game");

// Scene with specific template
CLI.CreateScene("UIScene", "Assets/Scenes/UI", "2D");

// Scene added to build settings
CLI.CreateScene("MainMenu", "Assets/Scenes/Menus", "DefaultGameObjects", true);
```

#### Available Scene Types
- `Empty` - Completely empty scene
- `DefaultGameObjects` - Scene with Main Camera and Directional Light
- `2D` - 2D optimized scene setup
- `3D` - 3D optimized scene setup with skybox
- `URP` - Universal Render Pipeline optimized (if URP is installed)
- `HDRP` - High Definition Render Pipeline optimized (if HDRP is installed)

### Canvas Management

Add and configure UI canvases with various render modes:

```csharp
// Basic overlay canvas
CLI.AddCanvas("UICanvas", "ScreenSpaceOverlay");

// Canvas with custom resolution
CLI.AddCanvas("GameUI", "ScreenSpaceOverlay", 1920, 1080, "ScaleWithScreenSize");

// World space canvas
CLI.AddCanvas("WorldUI", "WorldSpace", 100, 100);
```

#### Canvas Parameters
- **Canvas Name**: Name for the canvas GameObject
- **Render Mode**: `ScreenSpaceOverlay`, `ScreenSpaceCamera`, `WorldSpace`
- **Reference Width/Height**: Resolution for scaling (default: 1920x1080)
- **Scale Mode**: `ConstantPixelSize`, `ScaleWithScreenSize`, `ConstantPhysicalSize`
- **Sorting Order**: Canvas sorting order (default: 0)
- **World Position**: Position for WorldSpace canvas (Vector3, default: Vector3.zero)

### CLI Commands

#### unity-create-scene
```bash
unity-create-scene <scene_name> <scene_path> [scene_type] [add_to_build]
```

**Examples:**
```bash
unity-create-scene MyScene Assets/Scenes
unity-create-scene GameScene Assets/Scenes/Game DefaultGameObjects true
unity-create-scene UIScene Assets/Scenes/UI 2D false
```

#### unity-add-canvas
```bash
unity-add-canvas <canvas_name> <render_mode> [width] [height] [scale_mode]
```

**Examples:**
```bash
unity-add-canvas MainCanvas ScreenSpaceOverlay
unity-add-canvas UICanvas ScreenSpaceOverlay 1920 1080 ScaleWithScreenSize
unity-add-canvas WorldCanvas WorldSpace 100 100
```

#### unity-list-types
```bash
unity-list-types
```

Lists all available scene types for your Unity installation.

#### unity-cli-help
```bash
unity-cli-help
```

Shows detailed help information for all available commands.

## 🔧 Advanced Usage

### Batch Scene Creation

```csharp
string[] sceneNames = { "Level1", "Level2", "Level3", "MainMenu", "Settings" };
string basePath = "Assets/Scenes";

foreach (string sceneName in sceneNames)
{
    CLI.CreateScene(sceneName, basePath, "DefaultGameObjects", true);
}
```

### Automated UI Setup

```csharp
// Create UI scene
CLI.CreateScene("MainMenu", "Assets/Scenes/UI", "2D");

// Add main canvas
CLI.AddCanvas("MenuCanvas", "ScreenSpaceOverlay", 1920, 1080, "ScaleWithScreenSize");

// Add overlay canvas for popups
CLI.AddCanvas("PopupCanvas", "ScreenSpaceOverlay", 1920, 1080, "ScaleWithScreenSize");
```

### Integration with Build Tools

```bash
#!/bin/bash
# build-setup.sh - Automated project setup

echo "Setting up Unity project structure..."

# Create scene directories
unity-create-scene MainMenu Assets/Scenes/Menus DefaultGameObjects true
unity-create-scene Level1 Assets/Scenes/Levels DefaultGameObjects true
unity-create-scene Settings Assets/Scenes/UI 2D true

# Add UI canvases
unity-add-canvas MainMenuCanvas ScreenSpaceOverlay 1920 1080
unity-add-canvas SettingsCanvas ScreenSpaceOverlay 1920 1080

echo "Project setup complete!"
```

## 🎯 Unity Menu Integration

Access Unity Vibe CLI features directly from Unity's menu:

- **Tools > Unity Vibe CLI > Debug Unity CLI** - Show current configuration
- **Tools > Unity Vibe CLI > Test Unity CLI** - Test CLI functionality

## 🔍 Troubleshooting

### WSL (Windows Subsystem for Linux) Configuration

**Important for WSL Users:**
- **Unity Editor must be closed** before running CLI commands in batch mode
- Scripts automatically convert WSL paths (`/mnt/c/*`) to Windows paths (`C:/*`)
- Unity installation path is auto-detected from common locations
- All bash scripts are executable and ready to use

**Current Working CLI Commands:**
```bash
# Create a new scene (Unity Editor must be closed)
./Scripts/unity-create-scene MyScene Assets/Scenes/Test DefaultGameObjects false

# List available scene types  
./Scripts/unity-list-types

# Show help documentation
./Scripts/unity-cli-help

# Add canvas (requires Unity UI package - currently disabled)
./Scripts/unity-add-canvas MainCanvas ScreenSpaceOverlay 1920 1080
```

**Expected Output:**
```bash
$ ./Scripts/unity-create-scene MyNewScene Assets/Scenes/Test DefaultGameObjects false
Unity Vibe CLI - Creating Scene
================================
Scene Name: MyNewScene
Scene Path: Assets/Scenes/Test  
Scene Type: DefaultGameObjects
Add to Build: false
Project Path: C:/repos/unity-vibe-cli

✅ Scene created successfully: Assets/Scenes/Test/MyNewScene.unity

$ ./Scripts/unity-list-types
Unity Vibe CLI - Available Scene Types
======================================
Project Path: C:/repos/unity-vibe-cli

[UnityCLI] === Available Scene Types ===
[UnityCLI] Available scene types: Empty, DefaultGameObjects, 2D, 3D, URP
[UnityCLI] Scene Type Descriptions:
[UnityCLI]   Empty - Completely empty scene
[UnityCLI]   DefaultGameObjects - Scene with Main Camera and Directional Light
[UnityCLI]   2D - 2D optimized scene setup
[UnityCLI]   3D - 3D optimized scene setup with skybox
[UnityCLI]   URP - Universal Render Pipeline optimized scene
[UnityCLI] ===========================

✅ Scene types listed successfully
```

### Common Issues

**"Multiple Unity instances cannot open the same project"**
- **Solution**: Close Unity Editor completely before running CLI commands
- This is a Unity limitation with batch mode operations
- Scripts work perfectly once Unity Editor is closed

**"Unity Editor not found"**
- Ensure Unity is installed and accessible in PATH
- For WSL: Scripts auto-detect Unity from `/mnt/c/Program Files/Unity/Hub/Editor/*/Editor/Unity.exe`
- If detection fails, manually update `UNITY_PATH` in scripts

**"Couldn't set project path"** 
- This was fixed by implementing WSL path conversion in scripts
- Paths are automatically converted from `/mnt/c/*` to `C:/*` format
- No manual configuration needed

**"No active scene to add canvas to"**
- Open a scene in Unity Editor before adding canvas  
- Create a new scene first using `unity-create-scene`
- Canvas functionality requires Unity UI package (currently commented out)

**"Scene already exists"**
- Check if scene file already exists at the specified path
- Use a different scene name or path
- CLI will warn you with: `[UnityCLI] Scene already exists: path/to/scene.unity`

### Debug Information

Enable detailed logging by running:
```csharp
CLI.DebugUnityCLI(); // Shows configuration and available options
CLI.TestUnityCLI();  // Tests CLI functionality
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Open in Unity 2022.3 or later
3. Make your changes
4. Test with the included test scenes
5. Submit a pull request

### Adding New Commands

1. Add method to `UnityVibe.Editor.CLI` class
2. Create corresponding bash script in `Scripts/` directory
3. Update documentation
4. Add tests

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Unity Technologies for the excellent Unity Editor APIs
- The Unity community for inspiration and feedback
- Contributors who help improve this tool

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/unity-vibe/unity-vibe-cli/issues)
- **Discussions**: [GitHub Discussions](https://github.com/unity-vibe/unity-vibe-cli/discussions)
- **Email**: contact@unityvibe.com

---

**Made with ❤️ by the Unity Vibe CLI Team**