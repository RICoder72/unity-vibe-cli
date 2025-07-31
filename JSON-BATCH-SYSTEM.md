# Unity Vibe CLI - JSON Batch System

## Overview

The JSON Batch System allows you to programmatically create complex Unity scenes and UI hierarchies using structured JSON files. This system solves the GameObject persistence issues inherent in Unity's batch mode by executing all commands within a single Unity session.

## ✅ Confirmed Working Features

### Scene Creation
- Create scenes with various types (Empty, DefaultGameObjects, 2D, 3D, URP, HDRP)
- Automatically add scenes to build settings
- Support for custom scene paths

### UI Hierarchy Creation
- Canvas creation with configurable render modes and scaling
- Panel creation with custom dimensions and anchor points
- Button creation with custom text and positioning
- Text elements with configurable fonts, sizes, and colors
- **Complex parent-child relationships that persist properly**

### Advanced Features
- Multi-level nesting (tested up to 3 levels deep)
- Custom positioning with anchor points and offsets
- Multiple panels and UI sections in single scene
- Color customization for text elements
- Multi-line text support

## Execution Methods

### 1. Unity Menu (Recommended - ✅ WORKING)
```
Tools > Unity Vibe CLI > Execute Batch File
```
- Select JSON file via file picker
- Executes within Unity editor session
- Full GameObject persistence
- Real-time feedback in Unity Console

### 2. CLI with Unity Open (⚠️ Detection Issues)
```bash
./vibe-unity batch-file ui-setup.json
```
- Detects Unity process and uses editor mode
- May have timing/detection issues
- Falls back to batch mode if Unity not detected

### 3. CLI with Unity Closed
```bash
./vibe-unity batch-file ui-setup.json
```
- Uses batch mode (`-batchmode -quit`)
- Subject to GameObject persistence limitations
- Suitable for scene creation only

## JSON Schema

### Basic Structure
```json
{
  "version": "1.0",
  "description": "Optional description",
  "commands": [
    {
      "action": "create-scene",
      "name": "SceneName",
      "path": "Assets/Scenes",
      "type": "DefaultGameObjects",
      "addToBuild": true
    }
  ]
}
```

### Available Actions

#### create-scene
```json
{
  "action": "create-scene",
  "name": "GameScene",
  "path": "Assets/Scenes", 
  "type": "DefaultGameObjects",
  "addToBuild": true
}
```

#### add-canvas
```json
{
  "action": "add-canvas",
  "name": "MainCanvas",
  "scene": "GameScene",
  "renderMode": "ScreenSpaceOverlay",
  "referenceWidth": 1920,
  "referenceHeight": 1080,
  "scaleMode": "ScaleWithScreenSize"
}
```

#### add-panel
```json
{
  "action": "add-panel",
  "name": "MenuPanel",
  "parent": "MainCanvas",
  "width": 400,
  "height": 300,
  "anchor": "MiddleCenter",
  "position": [0, 50]
}
```

#### add-button
```json
{
  "action": "add-button", 
  "name": "PlayButton",
  "parent": "MenuPanel",
  "text": "PLAY GAME",
  "width": 200,
  "height": 50,
  "anchor": "MiddleCenter",
  "position": [0, 25]
}
```

#### add-text
```json
{
  "action": "add-text",
  "name": "TitleText", 
  "parent": "HeaderPanel",
  "text": "Game Title",
  "fontSize": 24,
  "width": 300,
  "height": 60,
  "anchor": "TopCenter",
  "color": "#FFFFFF"
}
```

## CLI Options

### Validation
```bash
./vibe-unity batch-file ui-setup.json --validate-only
```

### Dry Run Preview
```bash
./vibe-unity batch-file ui-setup.json --dry-run
```

### Help
```bash
./vibe-unity batch-file --help
```

## Tested Complex Example

Successfully created a complete game menu with:

**Structure:**
```
GameMenuScene
└── MainUICanvas (1920x1080)
    ├── HeaderPanel
    │   └── GameTitle ("AWESOME GAME")
    ├── MainMenuPanel  
    │   ├── PlayButton ("PLAY GAME")
    │   ├── OptionsButton ("OPTIONS")
    │   ├── CreditsButton ("CREDITS") 
    │   └── ExitButton ("EXIT")
    ├── SideInfoPanel
    │   ├── InfoTitle ("Game Info")
    │   ├── VersionText ("Version: 1.0.0")
    │   └── PlayerStatsText (multi-line)
    └── FooterPanel
        └── CopyrightText (©2025...)
```

**Results:**
- ✅ 15 UI elements created successfully
- ✅ Perfect parent-child relationships maintained
- ✅ All positioning and styling applied correctly
- ✅ Multi-level nesting (3 levels deep) working
- ✅ Scene added to build settings automatically

## Key Benefits

1. **GameObject Persistence Solved**: All commands execute in single Unity session
2. **Complex Hierarchies**: Deep nesting with proper parent-child relationships
3. **Reusable Templates**: JSON files can be version controlled and shared
4. **No Command Length Limits**: Unlimited complexity in JSON files
5. **Structured Workflow**: Readable, maintainable UI definitions
6. **Rapid Prototyping**: Quickly generate complex UI layouts

## Troubleshooting

### Menu Method Not Working
- Ensure Unity is open with the project loaded
- Check Unity Console for error messages
- Verify `UnityVibeCLI.cs` is compiled without errors

### CLI Detection Issues  
- Use `./vibe-unity check-unity` to verify Unity process detection
- Try closing and reopening Unity if detection fails
- Fall back to Unity menu method

### JSON Validation Errors
- Install `jq` for better validation: `sudo apt install jq` (WSL/Linux)
- Use `--validate-only` flag to check JSON syntax
- Refer to `batch-schema.json` for complete schema

## Files

- `batch-schema.json` - Complete JSON schema definition
- `complex-ui-test.json` - Working complex example
- `example-batch.json` - Additional example template
- `Assets/Editor/UnityVibeCLI.cs` - C# implementation
- `vibe-unity` - CLI script with batch support

## Next Steps

The JSON Batch System provides a solid foundation for:
- Automated UI generation workflows
- Template-based scene creation
- Integration with external tools and scripts
- Batch processing of multiple UI layouts
- CI/CD pipeline integration for automated scene generation

This system successfully addresses the Unity batch mode GameObject persistence limitations and enables programmatic creation of complex Unity scenes and UI hierarchies.