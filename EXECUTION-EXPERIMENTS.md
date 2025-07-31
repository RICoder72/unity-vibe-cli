# Unity CLI Execution Experiments

This document describes the various approaches we tested for executing Unity commands while the Unity Editor is running with the project open.

## The Challenge

Unity enforces single-instance project locking - when a project is open in the Unity Editor, no other Unity process can access that project. This creates challenges for external CLI tools that want to automate Unity operations.

## Approaches Tested

### 1. Traditional Batch Mode ❌
**Command:** `Unity.exe -batchmode -quit -projectPath "..." -executeMethod ...`
**Result:** Failed with "It looks like another Unity instance is running with this project open"
**Reason:** Unity's project locking prevents multiple instances

### 2. No-Graphics Mode ❌  
**Command:** `Unity.exe -nographics -quit -projectPath "..." -executeMethod ...`
**Result:** Same failure - "project already open"
**Reason:** `-nographics` still attempts to open the project, triggering the same lock

### 3. Direct Execution (No Flags) ❌
**Command:** `Unity.exe -projectPath "..." -executeMethod ...`
**Result:** Still creates instance conflicts
**Reason:** Any attempt to open the project hits the lock

### 4. HTTP Server Approach ✅ (with caveats)
**Implementation:** Created `UnityVibeHttpServer.cs` that runs inside Unity
**Result:** Works! Can execute commands while Unity is running
**Caveats:** 
- Requires manual startup in Unity (Tools > Unity Vibe CLI > Start HTTP Server)
- Adds complexity for users
- Not automatic/seamless

### 5. File Watcher System ✅ (Current Solution)
**Implementation:** Unity watches `.vibe-commands/` directory for JSON files
**Result:** Works automatically and reliably
**Benefits:**
- No manual setup required
- Executes within the running Unity instance
- Preserves GameObject persistence
- Simple and reliable

## Key Findings

1. **Unity's Design Philosophy**: The single-instance lock is intentional for data integrity
2. **No Built-in Remote Control**: Unity Editor lacks built-in APIs for external control
3. **File System as IPC**: Using the file system for inter-process communication is reliable
4. **In-Process Execution**: Commands must execute within the already-running Unity process

## Future Possibilities

1. **Unity Remote API**: Unity may add official remote control APIs in future versions
2. **Unity Package Manager Extensions**: Potential hooks through UPM
3. **Custom Unity Modules**: Deep integration via custom C++ modules (complex)
4. **Unity as a Service**: Headless Unity server mode (not currently viable for Editor features)

## Recommendation

The File Watcher System remains the best solution because:
- It works automatically without user intervention
- It's reliable and has minimal overhead  
- It executes commands in the proper Unity context
- It maintains scene/GameObject persistence

The HTTP Server approach is kept as an alternative for future development when a more direct communication method is desired.