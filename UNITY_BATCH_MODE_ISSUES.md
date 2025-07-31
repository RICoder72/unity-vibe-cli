# Unity Batch Mode Session Isolation Issues

## Problem Summary

When using Unity's batch mode with the Vibe CLI, each command invocation creates a completely isolated Unity session. This causes issues when trying to perform sequential operations that depend on persistent state from previous commands.

## Root Cause Analysis

### Unity Batch Mode Behavior
- Each `Unity -batchmode -quit -executeMethod` call creates a fresh Unity process
- The process loads the project from scratch (Library, AssetDatabase, etc.)
- No state is maintained between separate batch mode invocations
- The "active scene" concept doesn't persist between commands

### Specific Issues Identified

#### 1. Scene Context Loss
**Problem:** After creating a scene with one CLI command, subsequent commands cannot automatically target that scene as the "current" scene.

**Example:**
```bash
# This works - creates scene
./vibe-unity create-scene MyScene Assets/Scenes --type DefaultGameObjects

# This FAILS - no "current scene" in new batch mode session  
./vibe-unity add-canvas MainCanvas
# Error: No valid scene available for canvas 'MainCanvas'
```

**Why it fails:**
- Scene creation command: Unity session starts → creates scene → saves → exits
- Canvas command: NEW Unity session starts → no active scene loaded → fails

#### 2. AssetDatabase Refresh Delays
**Problem:** Newly created assets may not be immediately visible to subsequent batch mode sessions due to AssetDatabase refresh timing.

**Example:**
```bash
./vibe-unity create-scene TestScene Assets/Scenes
./vibe-unity add-canvas TestCanvas --scene TestScene  # May fail if executed too quickly
```

**Why it happens:**
- Scene creation saves the .unity file
- Asset database refresh is asynchronous
- Next batch mode session might not see the new scene file immediately

#### 3. GameObject Persistence Failure
**Problem:** GameObjects created in one batch mode session are not visible in subsequent batch mode sessions, even when scenes are properly saved.

**Example:**
```bash
# This works - creates canvas
./vibe-unity create-scene GameScene Assets/Scenes --type DefaultGameObjects
./vibe-unity add-canvas MainCanvas --scene GameScene

# This FAILS - canvas not found in new batch session
./vibe-unity add-panel MenuPanel --scene GameScene --parent MainCanvas
# Error: Parent GameObject 'MainCanvas' not found in active scene
# Available GameObjects: Main Camera, Directional Light
```

**Why it fails:**
- Canvas creation: Unity session starts → loads scene → creates canvas → saves scene → exits
- Panel creation: NEW Unity session starts → loads scene → canvas not found in GameObject hierarchy → fails

**Root Cause:** Unity's batch mode scene loading doesn't properly reconstruct the GameObject hierarchy from saved scene files, or there's an issue with how GameObjects are persisted/loaded between batch sessions.

#### 4. Project State Inconsistencies
**Problem:** Unity's internal caches, temporary files, and project state may be inconsistent between batch sessions.

## Current Workarounds

### 1. Always Specify Target Scene Explicitly
**Status:** ✅ IMPLEMENTED

Instead of relying on "current scene", always specify the target scene name:

```bash
# ❌ BAD: Relies on current scene context
./vibe-unity add-canvas MainCanvas

# ✅ GOOD: Explicitly specifies target scene
./vibe-unity add-canvas MainCanvas --scene MyScene
```

**Implementation:** The CLI already supports `--scene` parameter for canvas operations.

### 2. Combined Operations in Single Command
**Status:** 🚧 PARTIAL - Could be expanded

Perform multiple operations in a single batch mode session:

```csharp
// Current: Each operation is separate
CLI.CreateScene("MyScene", "Assets/Scenes", "DefaultGameObjects");
CLI.AddCanvas("MainCanvas", "MyScene", "ScreenSpaceOverlay");

// Better: Combined operation
CLI.CreateSceneWithCanvas("MyScene", "Assets/Scenes", "DefaultGameObjects", "MainCanvas");
```

### 3. Session Management System  
**Status:** ✅ IMPLEMENTED

Implemented `.vibeunity` session file system to persist context between CLI calls:

```bash
# Start session with defaults
./vibe-unity start-session --scene GameScene --parent MainCanvas --resolution 1280x720

# Use session defaults (auto-fills scene, parent, resolution)
./vibe-unity add-panel MenuPanel --use-session
./vibe-unity add-button PlayButton --use-session
./vibe-unity set-parent MenuPanel  # Update parent for nested elements
./vibe-unity add-text HeaderText --use-session
```

**Pros:**
- Eliminates repetitive parameter specification
- Maintains workflow context between commands
- Supports dynamic parent updates during UI building
- Simple key-value file format

**Cons:**
- Still subject to GameObject persistence issues
- Session state can become stale if scene is modified outside CLI

### 4. Asset Database Forced Refresh
**Status:** ✅ IMPLEMENTED

The CLI already calls `AssetDatabase.Refresh()` after scene creation to ensure immediate visibility.

## Potential Solutions

### Solution 1: Force Scene Save and Reload
**Complexity:** Medium
**Impact:** High

Force explicit scene saving and reloading to ensure GameObject persistence:

```csharp
// In Unity C# CLI methods
public static bool AddCanvas(string canvasName, string sceneName, ...)
{
    // ... create canvas ...
    
    // Force save scene immediately
    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    
    // Force asset database refresh
    AssetDatabase.Refresh();
    AssetDatabase.SaveAssets();
    
    return true;
}

public static bool AddPanel(string panelName, string parentName, string sceneName, ...)
{
    // Force reload scene to get latest GameObject hierarchy
    if (!string.IsNullOrEmpty(sceneName))
    {
        string scenePath = FindSceneAsset(sceneName);
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }
    
    // ... rest of method ...
}
```

**Pros:**
- Works with existing CLI architecture
- Ensures GameObject persistence between sessions
- Low implementation complexity

**Cons:**
- Performance overhead from extra save/load operations
- May not solve all Unity batch mode persistence issues

### Solution 2: Single-Session CLI Mode
**Complexity:** High
**Impact:** High

Create a persistent Unity batch mode session that can accept multiple commands:

```bash
# Start persistent session
./vibe-unity start-session

# Execute commands in same session
./vibe-unity exec create-scene MyScene Assets/Scenes
./vibe-unity exec add-canvas MainCanvas

# End session
./vibe-unity end-session
```

**Implementation approach:**
- Use Unity's `-batchmode` without `-quit`
- Implement IPC (named pipes/sockets) for command communication
- Keep Unity process alive between commands

**Pros:**
- Maintains scene context between commands
- Better performance (no startup overhead)
- True "current scene" concept

**Cons:**
- Complex IPC implementation
- Process management complexity
- Potential memory leaks in long sessions
- Harder debugging when session hangs

### Solution 2: Smart Scene Detection
**Complexity:** Medium  
**Impact:** Medium

Automatically detect the most recently created/modified scene when no scene is specified:

```csharp
private static string GetMostRecentScene()
{
    var sceneGuids = AssetDatabase.FindAssets("t:Scene");
    var mostRecent = sceneGuids
        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
        .Select(path => new { Path = path, Time = File.GetLastWriteTime(path) })
        .OrderByDescending(x => x.Time)
        .FirstOrDefault();
    
    return mostRecent?.Path;
}
```

**Pros:**
- Works with existing CLI architecture
- Intuitive for common workflows
- Low complexity

**Cons:**
- Heuristic-based (not always correct)
- Could target wrong scene in complex scenarios

### Solution 3: Command Chaining/Batching
**Complexity:** Low
**Impact:** High

Allow multiple commands in a single CLI invocation:

```bash
# Chain commands with &&
./vibe-unity create-scene MyScene Assets/Scenes && add-canvas MainCanvas --scene MyScene

# Or explicit batch syntax
./vibe-unity batch "create-scene MyScene Assets/Scenes; add-canvas MainCanvas --scene MyScene"
```

**Implementation:**
- Parse multiple commands from CLI arguments
- Execute all commands in single Unity batch session
- Maintain scene context within the session

**Pros:**
- Simple to implement
- Maintains context within batch
- Familiar CLI pattern

**Cons:**
- More complex CLI argument parsing
- All-or-nothing execution (one failure kills all)

### Solution 4: Scene State Persistence
**Complexity:** Medium
**Impact:** Low

Store "current scene" information in a persistent file:

```bash
# Create scene and remember it
./vibe-unity create-scene MyScene Assets/Scenes --set-current

# Use remembered scene
./vibe-unity add-canvas MainCanvas  # Uses stored current scene
```

**Implementation:**
- Store current scene in `.vibe-unity-state` file
- Read state file when no scene specified
- Update state on scene operations

**Pros:**
- Simple to implement
- Works with existing architecture
- Familiar stateful CLI pattern

**Cons:**
- State can become stale/incorrect
- Additional file management
- Doesn't solve fundamental isolation issue

## Recommended Approach

**Primary:** **Solution 3 (Command Chaining/Batching)** + **Solution 2 (Smart Scene Detection)**

1. **Implement command batching** for common workflows:
   ```bash
   ./vibe-unity batch "create-scene MyScene Assets/Scenes --type 3D; add-canvas MainCanvas; add-panel MenuPanel --parent MainCanvas"
   ```

2. **Add smart scene detection** as fallback when no scene specified:
   - Use most recently modified scene in project
   - Provide clear feedback about which scene was auto-selected

3. **Keep explicit scene specification** as the most reliable option

This approach provides:
- ✅ Backwards compatibility
- ✅ Intuitive workflow for simple cases
- ✅ Explicit control for complex cases
- ✅ Reasonable implementation complexity

## Implementation Priority

1. **High Priority:** Solution 3 (Command Batching) - Solves the core workflow issue
2. **Medium Priority:** Solution 2 (Smart Scene Detection) - Improves UX for simple cases  
3. **Low Priority:** Solution 1 (Persistent Session) - Complex but most powerful

## Testing Strategy

1. **Reproduce Scene Context Issues:**
   ```bash
   ./vibe-unity create-scene Test1 Assets/Scenes
   ./vibe-unity add-canvas Canvas1  # Should fail - no scene context
   
   ./vibe-unity create-scene Test2 Assets/Scenes  
   sleep 1  # Test timing issues
   ./vibe-unity add-canvas Canvas2 --scene Test2  # Should work - explicit scene
   ```

2. **Reproduce GameObject Persistence Issues:**
   ```bash
   # Create scene and canvas
   ./vibe-unity create-scene Test3 Assets/Scenes --type DefaultGameObjects
   ./vibe-unity add-canvas MainCanvas --scene Test3
   
   # Try to add child to canvas - FAILS due to GameObject persistence issue
   ./vibe-unity add-panel MenuPanel --scene Test3 --parent MainCanvas
   # Expected error: Parent GameObject 'MainCanvas' not found in active scene
   # Available GameObjects: Main Camera, Directional Light
   ```

3. **Test Session Management:**
   ```bash
   # Test session workflow
   ./vibe-unity start-session --scene SessionTest --parent TestCanvas
   ./vibe-unity create-scene SessionTest Assets/Scenes
   ./vibe-unity add-canvas TestCanvas --use-session
   ./vibe-unity show-session
   ./vibe-unity set-parent TestCanvas
   ./vibe-unity add-panel TestPanel --use-session  # Will fail due to persistence issue
   ```

4. **Test Workarounds:**
   ```bash
   # Test explicit scene specification
   ./vibe-unity create-scene Test4 Assets/Scenes
   ./vibe-unity add-canvas Canvas4 --scene Test4
   
   # Test batching (when implemented)
   ./vibe-unity batch "create-scene Test5 Assets/Scenes; add-canvas Canvas5"
   ```

5. **Performance Testing:**
   - Measure time for separate commands vs batched commands
   - Test with various project sizes
   - Monitor memory usage in persistent sessions

## For Claude Code Integration

**Current Limitations:**
Due to GameObject persistence issues, Claude Code workflows are currently limited to:

```bash
# ✅ WORKS: Scene creation and canvas addition
./vibe-unity create-scene GameScene Assets/Scenes --type DefaultGameObjects
./vibe-unity add-canvas UICanvas --scene GameScene --mode ScreenSpaceOverlay

# ❌ FAILS: Cannot add UI elements to previously created GameObjects
./vibe-unity add-panel MenuPanel --parent UICanvas --scene GameScene
# Error: Parent GameObject 'UICanvas' not found in active scene
```

**Current Workaround:**
Use session management for parameter persistence, but avoid parent relationships:

```bash
# Set up session context
./vibe-unity start-session --scene GameScene --resolution 1280x720

# Create scene structure (each command works independently)
./vibe-unity create-scene GameScene Assets/Scenes --type DefaultGameObjects
./vibe-unity add-canvas UICanvas --use-session
./vibe-unity add-canvas PopupCanvas --use-session  # Separate canvas for each UI section
```

**Future Enhancement:**
When GameObject persistence is resolved, Claude Code can generate hierarchical UI workflows:

```bash
# Ideal future workflow (when persistence works)
./vibe-unity start-session --scene GameScene --parent UICanvas
./vibe-unity create-scene GameScene Assets/Scenes --type DefaultGameObjects
./vibe-unity add-canvas UICanvas --use-session
./vibe-unity add-panel MenuPanel --use-session
./vibe-unity set-parent MenuPanel
./vibe-unity add-button PlayButton --use-session --text "Start Game"
./vibe-unity add-button SettingsButton --use-session --text "Settings"
```

**Recommended Approach for Now:**
Focus on scene and canvas creation, avoid complex GameObject hierarchies until persistence issues are resolved.