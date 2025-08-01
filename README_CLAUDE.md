# Vibe Unity CLI - Claude Documentation

## Overview
File-watching system that processes JSON batch commands to create Unity scenes and UI elements. JSON files placed in `.vibe-commands/` are automatically processed with detailed logging.

## Quick Start
1. Place JSON files in project root `.vibe-commands/` directory
2. Files are processed automatically when Unity detects changes
3. Results logged to `.vibe-commands/processed/filename.log`

## JSON Structure
```json
{
  "version": "1.0",
  "description": "Brief description",
  "scene": {
    "name": "SceneName",
    "create": true,
    "path": "Assets/Scenes",
    "type": "DefaultGameObjects",
    "addToBuild": false
  },
  "commands": [
    { /* command objects */ }
  ]
}
```

## Available Commands

### Scene Management
```json
{ "action": "create-scene", "name": "MyScene", "path": "Assets/Scenes" }
```

### UI Elements
```json
// Canvas
{ "action": "add-canvas", "name": "MainCanvas", "renderMode": "ScreenSpaceOverlay" }

// Panel
{ "action": "add-panel", "name": "Panel", "parent": "Canvas", "width": 400, "height": 300, "color": "#2C3E50" }

// Button
{ "action": "add-button", "name": "PlayButton", "parent": "Panel", "text": "PLAY", "width": 200, "height": 50, "color": "#27AE60" }

// Text
{ "action": "add-text", "name": "Title", "parent": "Panel", "text": "Game Title", "fontSize": 24, "color": "#FFFFFF" }

// ScrollView
{ "action": "add-scrollview", "name": "ScrollView", "parent": "Canvas", "width": 400, "height": 300, "vertical": true, "horizontal": false }
```

### 3D Objects
```json
// Primitives (cube, sphere, plane, cylinder, capsule)
{ "action": "add-cube", "name": "RedCube", "position": [0, 0, 0], "rotation": [0, 45, 0], "scale": [1, 1, 1] }
```

### Component Management
```json
// Add component to existing GameObject
{ "action": "add-component", "name": "GameController", "componentType": "Rigidbody" }

// Add component with parameters
{
  "action": "add-component", 
  "name": "GameController", 
  "componentType": "Rigidbody",
  "parameters": [
    { "name": "mass", "value": "2.5", "type": "float" },
    { "name": "useGravity", "value": "true", "type": "bool" },
    { "name": "drag", "value": "0.1", "type": "float" }
  ]
}

// Add AudioSource with configuration
{
  "action": "add-component",
  "name": "SoundObject", 
  "componentType": "AudioSource",
  "parameters": [
    { "name": "volume", "value": "0.8", "type": "float" },
    { "name": "playOnAwake", "value": "false", "type": "bool" }
  ]
}

// Add custom component with GameObject reference
{
  "action": "add-component",
  "name": "Player",
  "componentType": "SceneController", 
  "parameters": [
    { "name": "Camera", "value": "Main Camera", "type": "GameObject" },
    { "name": "speed", "value": "10", "type": "float" }
  ]
}
```

## Common Properties

### UI Elements
- `name` - GameObject name (required)
- `parent` - Parent GameObject name
- `width`, `height` - Size in pixels
- `position` - [x, y] offset from anchor
- `anchor` - "TopLeft", "MiddleCenter", "BottomRight", etc.
- `color` - Hex color "#RRGGBB"

### 3D Objects  
- `name` - GameObject name (required)
- `position` - [x, y, z] world position
- `rotation` - [x, y, z] euler angles
- `scale` - [x, y, z] scale factors

### Canvas Specific
- `renderMode` - "ScreenSpaceOverlay", "ScreenSpaceCamera", "WorldSpace"
- `referenceWidth`, `referenceHeight` - Canvas scaler reference resolution
- `scaleMode` - "ScaleWithScreenSize", "ConstantPixelSize"

### ScrollView Specific
- `horizontal`, `vertical` - Enable scrolling directions (boolean)
- `scrollbarVisibility` - "AutoHideAndExpandViewport", "AutoHide", "Permanent"
- `scrollSensitivity` - Scroll speed multiplier (float)

### Component Management
- `componentType` - Name of component class to add (string)
- `parameters` - Array of parameter objects to set on component
  - `name` - Field or property name on the component
  - `value` - String representation of the value
  - `type` - Value type: "string", "int", "float", "bool", "GameObject", "Component"

### Supported Component Types
- **Physics**: `Rigidbody`, `BoxCollider`, `SphereCollider`, `MeshCollider`
- **Audio**: `AudioSource`, `AudioListener`
- **Rendering**: `Camera`, `Light`, `MeshRenderer`
- **Custom Scripts**: Any MonoBehaviour-derived class by name
- **Aliases**: `rigidbody` → `Rigidbody`, `collider` → `BoxCollider`

## Example: Complete Menu System
```json
{
  "version": "1.0",
  "description": "Main menu with scrollable content",
  "scene": {
    "name": "MainMenu",
    "create": true,
    "path": "Assets/Scenes"
  },
  "commands": [
    {
      "action": "add-canvas",
      "name": "MenuCanvas",
      "renderMode": "ScreenSpaceOverlay",
      "referenceWidth": 1920,
      "referenceHeight": 1080
    },
    {
      "action": "add-scrollview",
      "name": "OptionsScroll",
      "parent": "MenuCanvas",
      "width": 600,
      "height": 400,
      "anchor": "MiddleCenter",
      "vertical": true,
      "horizontal": false
    },
    {
      "action": "add-button",
      "name": "PlayButton",
      "parent": "Content",
      "text": "PLAY GAME",
      "width": 300,
      "height": 60,
      "color": "#27AE60",
      "position": [0, -50]
    }
  ]
}
```

## Scene Persistence
- **Individual Commands**: Scenes are automatically saved after each successful command
- **Batch Operations**: Scene saving is optimized - only saved once at the end of the batch
- **Component Modifications**: All component additions and parameter changes are automatically persisted
- **Error Handling**: Failed operations don't trigger scene saving to maintain integrity

## Recent Updates (v1.2.0)

### ✨ New Features
- **TextMeshPro Integration**: All UI text components now use TextMeshProUGUI for better rendering
- **Component Attachment System**: Add any Unity component to GameObjects via JSON
- **Parameter Assignment**: Set component properties and fields using reflection
- **Smart Scene Saving**: Automatic scene persistence with batch optimization
- **Enhanced Error Handling**: Comprehensive logging and rollback on failures

### 🧪 Test Results (All Passed ✅)
- **TextMeshPro UI Test**: UI elements with TMP components - SUCCESS
- **Component Attachment Test**: Rigidbody, Colliders with parameters - SUCCESS  
- **Scene Saving Test**: Persistence verification - SUCCESS
- **Comprehensive Integration Test**: All features combined - SUCCESS

## Status
- ✅ HTTP Server: Disabled
- ✅ CLI Commands: Disabled  
- ✅ File Watcher: Active
- ✅ Logging: Enabled (.log files)
- ✅ Auto Scene Saving: Enabled
- ✅ TextMeshPro Integration: Active
- ✅ Component System: Fully Functional