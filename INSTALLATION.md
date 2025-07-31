# Vibe Unity Installation Guide

Vibe Unity is a Unity Package Manager (UPM) compatible package that provides CLI tools for Unity development workflow automation.

## Installation Methods

### Method 1: Install via Git URL (Recommended)

1. Open Unity Package Manager:
   - Window > Package Manager
   
2. Click the `+` button in the top-left corner

3. Select "Add package from git URL..."

4. Enter the repository URL:
   ```
   https://github.com/RICoder72/vibe-unity.git
   ```

5. Click "Add" and Unity will download and import the package

### Method 2: Install from Local Path

1. Clone the repository to your local machine:
   ```bash
   git clone https://github.com/RICoder72/vibe-unity.git
   ```

2. In Unity Package Manager:
   - Click `+` > "Add package from disk..."
   - Navigate to the cloned repository
   - Select the `package.json` file
   - Click "Open"

### Method 3: Manual Installation

1. Download the repository as a ZIP file from GitHub

2. Extract the ZIP to your Unity project's `Packages` folder:
   ```
   YourUnityProject/
   └── Packages/
       └── com.vibe.unity/
           ├── Assets/
           ├── Scripts/
           ├── package.json
           └── ...
   ```

3. Unity will automatically detect and import the package

## Post-Installation Setup

After installation, a setup dialog will appear automatically. If it doesn't:

1. The HTTP server and file watcher will start automatically (can be toggled via menu)
2. Access the menu at: **Tools > Vibe Unity**
3. Configure settings via: **Tools > Vibe Unity > Configuration**

### For WSL Users

The package includes bash scripts for WSL integration:
- `./vibe-unity` - Main CLI executable
- `Scripts/install-vibe-unity` - Installation helper

These scripts are automatically copied to your project root during setup.

## Requirements

- Unity 2022.3 or later
- .NET Framework 4.7.1 or later
- Windows, macOS, or Linux
- WSL (optional, for enhanced CLI features on Windows)

## Features

- **HTTP Server**: Allows external command execution (port 9876)
- **File Watcher**: Monitors `.vibe-commands` directory for JSON command files
- **CLI Integration**: Scene creation, canvas management, and UI automation
- **Claude Code Integration**: Works seamlessly with Claude Code AI assistant

## Verification

To verify the installation:

1. Check the menu: **Tools > Vibe Unity** should be visible
2. Look for console message: `[VibeUnityHTTP] Server started on http://localhost:9876/`
3. The `.vibe-commands` directory should be created in your project root

## Troubleshooting

### Package Not Showing in Unity

- Ensure you're using Unity 2022.3 or later
- Check Console for import errors
- Try reimporting: Right-click in Project window > Reimport All

### HTTP Server Not Starting

- Check if port 9876 is already in use
- Toggle via: **Tools > Vibe Unity > HTTP Server Enabled**
- Check console for error messages

### File Watcher Issues

- Verify `.vibe-commands` directory exists
- Check file permissions
- Toggle via: **Tools > Vibe Unity > File Watcher Enabled**

## Uninstallation

To remove the package:

1. Open Package Manager
2. Select "In Project" from the dropdown
3. Find "Vibe Unity"
4. Click "Remove"

Or manually delete from `Packages/manifest.json`:
```json
"com.vibe.unity": "https://github.com/RICoder72/vibe-unity.git"
```