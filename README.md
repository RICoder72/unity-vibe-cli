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

## 🐧 WSL Setup (Windows Users)

After installing the Unity package, set up the CLI for WSL:

### One-Time Setup Per Project
```bash
# Navigate to your Unity project root
cd /mnt/c/path/to/your/unity-project

# Run the installation script (creates the Scripts directory if needed)
./Scripts/install-vibe-unity

# Restart your terminal or reload shell configuration
source ~/.bashrc
```

### Verify Installation
```bash
# Test the CLI is working
vibe-unity --version
vibe-unity --help

# Test Unity detection
vibe-unity list-types
```

**Requirements:**
- Unity Hub installed in standard Windows location
- WSL with bash or zsh shell
- Unity project with Unity Vibe CLI package installed

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

#### Unified CLI Interface (Recommended)
```bash
# Install CLI for WSL (run once per project)
./Scripts/install-vibe-unity

# Create a new scene
vibe-unity create-scene MyScene Assets/Scenes
vibe-unity create-scene GameScene Assets/Scenes/Game --type 3D --build

# Add a canvas
vibe-unity add-canvas MainCanvas --mode ScreenSpaceOverlay --width 1920 --height 1080

# List available scene types
vibe-unity list-types

# Show help
vibe-unity --help
vibe-unity help create-scene
```

#### Direct Script Usage
```bash
# Run from project root without installation
./Scripts/vibe-unity create-scene MyScene Assets/Scenes
./Scripts/vibe-unity list-types
./Scripts/vibe-unity --help
```

#### Legacy Unity Command Line
```bash
# Direct Unity batch mode (not recommended)
Unity -batchmode -quit -projectPath "path/to/project" -executeMethod UnityVibe.Editor.CLI.CreateSceneFromCommandLine MyScene Assets/Scenes DefaultGameObjects true
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

#### vibe-unity create-scene
```bash
vibe-unity create-scene <SCENE_NAME> <SCENE_PATH> [OPTIONS]
```

**Options:**
- `-t, --type <TYPE>` - Scene type (default: DefaultGameObjects)
- `-b, --build` - Add scene to build settings
- `-h, --help` - Show help for this command

**Examples:**
```bash
vibe-unity create-scene MyScene Assets/Scenes
vibe-unity create-scene GameScene Assets/Scenes/Game --type 3D --build
vibe-unity create-scene UIScene Assets/Scenes/UI -t 2D
```

#### vibe-unity add-canvas
```bash
vibe-unity add-canvas <CANVAS_NAME> [OPTIONS]
```

**Options:**
- `-m, --mode <MODE>` - Render mode (default: ScreenSpaceOverlay)
- `-w, --width <WIDTH>` - Reference width (default: 1920)
- `--height <HEIGHT>` - Reference height (default: 1080)
- `-s, --scale <SCALE>` - Scale mode (default: ScaleWithScreenSize)
- `-h, --help` - Show help for this command

**Examples:**
```bash
vibe-unity add-canvas MainCanvas
vibe-unity add-canvas UICanvas --mode ScreenSpaceOverlay --width 1920 --height 1080
vibe-unity add-canvas WorldCanvas -m WorldSpace
```

#### vibe-unity list-types
```bash
vibe-unity list-types
```

Lists all available scene types for your Unity installation.

#### vibe-unity help
```bash
vibe-unity --help                    # General help
vibe-unity help <COMMAND>            # Command-specific help
vibe-unity <COMMAND> --help          # Alternative command help
```

Shows detailed help information for commands.

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
vibe-unity create-scene MainMenu Assets/Scenes/Menus --type DefaultGameObjects --build
vibe-unity create-scene Level1 Assets/Scenes/Levels --type DefaultGameObjects --build
vibe-unity create-scene Settings Assets/Scenes/UI --type 2D --build

# Add UI canvases
vibe-unity add-canvas MainMenuCanvas --mode ScreenSpaceOverlay --width 1920 --height 1080
vibe-unity add-canvas SettingsCanvas --mode ScreenSpaceOverlay --width 1920 --height 1080

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
# Install CLI (run once per project)
./Scripts/install-vibe-unity

# Create a new scene (Unity Editor must be closed)
vibe-unity create-scene MyScene Assets/Scenes/Test --type DefaultGameObjects

# List available scene types  
vibe-unity list-types

# Show help documentation
vibe-unity --help
vibe-unity help create-scene

# Add canvas (requires Unity UI package - currently disabled)
vibe-unity add-canvas MainCanvas --mode ScreenSpaceOverlay --width 1920 --height 1080
```

**Expected Output:**
```bash
$ vibe-unity create-scene MyNewScene Assets/Scenes/Test --type 3D --build
Unity Vibe CLI - Creating Scene
================================
Scene Name: MyNewScene
Scene Path: Assets/Scenes/Test  
Scene Type: 3D
Add to Build: true
Project Path: C:/repos/unity-vibe-cli

✅ Scene created successfully: Assets/Scenes/Test/MyNewScene.unity

$ vibe-unity list-types
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