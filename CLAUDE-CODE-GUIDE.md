# Unity Vibe CLI - Claude Code Integration Guide

This guide is specifically for Claude Code users who want to create Unity scenes, UI elements, and other Unity content through command-line automation.

## Quick Start for Claude Code

### Installation
1. Import the Unity Vibe CLI package into your Unity project
2. The package will automatically set up the file watcher system
3. No additional configuration required!

### Basic Usage

When Unity is **closed**, run commands directly:
```bash
./Scripts/vibe-unity create-scene MyScene Assets/Scenes --type DefaultGameObjects
./Scripts/vibe-unity add-canvas MainCanvas --mode ScreenSpaceOverlay
```

When Unity is **running** (recommended), the CLI automatically uses the file watcher system:
```bash
# Same commands work - the CLI detects Unity is running and adapts
./Scripts/vibe-unity create-scene MyScene Assets/Scenes --type DefaultGameObjects
./Scripts/vibe-unity add-canvas MainCanvas --mode ScreenSpaceOverlay
```

## How It Works

### Automatic Detection and Adaptation
1. The CLI detects if Unity is running with your project open
2. If Unity is running, it generates JSON command files in `.vibe-commands/`
3. Unity's file watcher immediately processes these commands
4. Results are reported back to the CLI

### Why This Approach?
Unity prevents multiple instances from opening the same project. Our file watcher system works *within* the already-running Unity instance, avoiding conflicts entirely.

## Command Reference

### Scene Creation
```bash
vibe-unity create-scene <name> <path> [options]

Options:
  --type, -t     Scene type (Empty, DefaultGameObjects, 2D, 3D)
  --build, -b    Add scene to build settings

Example:
  vibe-unity create-scene MainMenu Assets/Scenes/UI --type 2D --build
```

### UI Creation
```bash
# Add Canvas
vibe-unity add-canvas <name> [options]
  --mode, -m     Render mode (ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace)
  --width        Reference width (default: 1920)
  --height       Reference height (default: 1080)

# Add Panel
vibe-unity add-panel <name> <parent> [options]
  --width, -w    Panel width (default: 200)
  --height, -h   Panel height (default: 150)
  --anchor, -a   Anchor preset (e.g., MiddleCenter, TopLeft)

# Add Button
vibe-unity add-button <name> <parent> [options]
  --text, -t     Button text
  --width, -w    Button width
  --height, -h   Button height
  --anchor, -a   Anchor preset

# Add Text
vibe-unity add-text <name> <parent> [options]
  --text, -t     Display text
  --size, -s     Font size
  --color, -c    Text color (e.g., white, #FF0000)
```

### Batch Operations
```bash
vibe-unity batch <json-file>

# Example batch file:
{
  "version": "1.0",
  "commands": [
    {"action": "create-scene", "name": "TestScene", "path": "Assets/Scenes"},
    {"action": "add-canvas", "name": "UI", "renderMode": "ScreenSpaceOverlay"},
    {"action": "add-panel", "name": "Menu", "parent": "UI", "width": 400}
  ]
}
```

## Best Practices for Claude Code

### 1. Keep Unity Running
For the best experience, keep Unity open with your project. The file watcher system provides the most reliable execution.

### 2. Check Unity Console
Always check the Unity Console for detailed execution logs. The CLI shows summary information, but Unity Console has full details.

### 3. Use Batch Files for Complex Operations
For complex UI hierarchies, create JSON batch files:
```json
{
  "version": "1.0",
  "description": "Create complete main menu UI",
  "commands": [
    {"action": "create-scene", "name": "MainMenu", "path": "Assets/Scenes"},
    {"action": "add-canvas", "name": "MenuCanvas"},
    {"action": "add-panel", "name": "MenuPanel", "parent": "MenuCanvas", "width": 600, "height": 400},
    {"action": "add-button", "name": "PlayButton", "parent": "MenuPanel", "text": "Play"},
    {"action": "add-button", "name": "OptionsButton", "parent": "MenuPanel", "text": "Options"},
    {"action": "add-button", "name": "QuitButton", "parent": "MenuPanel", "text": "Quit"}
  ]
}
```

### 4. Scene Context
Commands that create UI elements operate on the currently active scene. The system automatically detects the most recently modified scene if no scene is explicitly active.

## Troubleshooting

### "Unity instance already running" Error
This is expected when Unity is open. The CLI will automatically use the file watcher system.

### Commands Not Executing
1. Check if Unity is actually running with your project
2. Look for JSON files in `.vibe-commands/processed/` - this confirms they were processed
3. Check Unity Console for any error messages

### UI Elements Not Appearing
1. Ensure you're viewing the correct scene in Unity
2. Check the Hierarchy window - elements might be created but not visible in Game view
3. Verify parent-child relationships in your commands

## Architecture Notes

### File Watcher System
- Located in `Assets/Editor/UnityVibeCLI.cs`
- Uses `[InitializeOnLoadMethod]` to start automatically
- Monitors `.vibe-commands/` directory
- Processes JSON files immediately upon detection
- Moves processed files to `.vibe-commands/processed/`

### CLI Script
- Bash script at `Scripts/vibe-unity`
- Detects Unity process status
- Automatically chooses execution method
- Provides unified interface regardless of Unity state

### Future Enhancements
- HTTP server for direct communication (experimental)
- WebSocket support for real-time feedback
- Extended command vocabulary
- Visual script generation

## Example Workflow for Claude Code

```bash
# 1. Create a new scene
vibe-unity create-scene GameMenu Assets/Scenes/UI --type 2D

# 2. Add UI canvas
vibe-unity add-canvas MainCanvas --mode ScreenSpaceOverlay

# 3. Create menu structure
vibe-unity add-panel MenuBackground MainCanvas --width 800 --height 600 --anchor MiddleCenter
vibe-unity add-text MenuTitle MenuBackground --text "Game Menu" --size 48 --anchor TopCenter
vibe-unity add-panel ButtonPanel MenuBackground --width 600 --height 400 --anchor MiddleCenter

# 4. Add menu buttons
vibe-unity add-button NewGameBtn ButtonPanel --text "New Game" --width 200 --height 50
vibe-unity add-button ContinueBtn ButtonPanel --text "Continue" --width 200 --height 50
vibe-unity add-button SettingsBtn ButtonPanel --text "Settings" --width 200 --height 50
vibe-unity add-button ExitBtn ButtonPanel --text "Exit" --width 200 --height 50
```

This creates a complete menu system with proper hierarchy and layout!