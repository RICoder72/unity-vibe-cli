# Vibe Unity Scripts

This directory contains scripts for enhanced Unity development workflow automation.

## claude-compile-check.sh

**Purpose**: Unity compilation validator specifically designed for claude-code integration.

### Features
- **Smart Window Management**: Focuses Unity window to trigger compilation, then returns focus to terminal
- **Project-Aware Window Detection**: Automatically detects current project name and finds matching Unity window
- **Multi-Unity Support**: Handles multiple Unity instances by finding the correct project window
- **Real-time Log Monitoring**: Watches Unity's Editor.log for compilation completion signals
- **Precise Error Parsing**: Extracts C# compilation errors with file:line locations
- **Structured Output**: Machine-readable format for automation
- **Warning Detection**: Optional warning reporting with `--include-warnings` flag
- **Smart Timeout Handling**: Checks compilation logs after timeout and retries if needed
- **Retry Logic**: Up to 2 retries if no compilation activity is detected
- **Nothing-to-Compile Detection**: Properly handles cases where all scripts are up-to-date

### Usage

#### Basic Compilation Check
```bash
./claude-compile-check.sh
```

#### Include Warnings
```bash
./claude-compile-check.sh --include-warnings
```

#### Help
```bash
./claude-compile-check.sh --help
```

### Output Format
```
STATUS: SUCCESS|ERRORS|TIMEOUT|ERROR
ERRORS: [count]
WARNINGS: [count]
DETAILS:
  [file:line] Error message
  [file:line] WARNING: Warning message
SCRIPT_VERSION: 1.3.1
```

### Exit Codes
- **0**: Success (no compilation errors)
- **1**: Compilation errors found
- **2**: Compilation timeout or Unity not accessible  
- **3**: Script execution error

### Example Output

#### Success Case
```
STATUS: SUCCESS
ERRORS: 0
WARNINGS: 0
SCRIPT_VERSION: 1.3.1
```

#### Error Case
```
STATUS: ERRORS
ERRORS: 2
WARNINGS: 1
DETAILS:
  [./Packages/com.ricoder.vibe-unity/Editor/MyScript.cs:25] 'GameObject' does not contain a definition for 'NonExistentMethod'
  [./Assets/Scripts/PlayerController.cs:42] Cannot implicitly convert type 'string' to 'int'
  [./Assets/Scripts/GameManager.cs:15] WARNING: Variable 'unusedVar' is assigned but its value is never used
SCRIPT_VERSION: 1.3.1
```

### Integration with Claude-Code

This script is designed to be called by claude-code after making changes to Unity C# scripts:

1. **Claude makes changes** to Unity scripts
2. **Claude runs** `./claude-compile-check.sh`
3. **Script triggers** Unity compilation and monitors completion
4. **Script returns** structured results with error details
5. **Claude analyzes** output and iterates on fixes if needed
6. **Claude proceeds** confidently when compilation succeeds

### Automatic Installation

The script is automatically copied to the project root when the Vibe Unity package is loaded. This ensures it's always available at `./claude-compile-check.sh` for claude-code to use.

### Requirements

- **Unity Editor**: Must be running with the target project loaded
- **WSL/Linux Environment**: Script designed for bash execution
- **Windows PowerShell**: Used for window management (via WSL interop)
- **Project Title**: Unity window title must contain the project name for detection

### Error Handling

The script includes comprehensive error handling:
- **Unity not found**: Clear error message if Unity isn't running
- **Compilation timeout**: Graceful handling if compilation hangs
- **Log file issues**: Validation of Unity Editor.log accessibility
- **Parsing errors**: Robust regex patterns for error extraction

### Development Notes

The script is based on the successful `test-workflow.sh` system but optimized specifically for compilation validation without test execution. It provides the fast feedback loop that claude-code needs for effective Unity development.