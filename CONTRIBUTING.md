# Contributing to Unity Vibe CLI

Thank you for your interest in contributing to Unity Vibe CLI! This guide will help you get started with contributing to the project.

## 🚀 Getting Started

### Prerequisites

- Unity 2022.3 or later
- Git
- Basic knowledge of C# and Unity Editor scripting
- (Optional) WSL or Linux environment for testing bash scripts

### Development Setup

1. **Fork the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/unity-vibe-cli.git
   cd unity-vibe-cli
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Open the project directory
   - Unity will automatically import the package

3. **Test the installation**
   - Go to `Tools > Unity Vibe CLI > Test Unity CLI`
   - Verify all functionality works as expected

## 🤝 How to Contribute

### Reporting Issues

Before creating a new issue, please:
1. Check if the issue already exists
2. Provide detailed information about the problem
3. Include Unity version, OS, and reproduction steps
4. Add relevant logs or error messages

### Suggesting Features

We welcome feature suggestions! Please:
1. Check existing feature requests
2. Describe the use case and expected behavior
3. Consider if it fits the project's scope and goals

### Code Contributions

#### Branch Naming Convention
- `feature/description-of-feature`
- `bugfix/description-of-fix`
- `docs/description-of-documentation-update`
- `refactor/description-of-refactor`

#### Pull Request Process

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow the coding standards (see below)
   - Add or update tests as needed
   - Update documentation if necessary

3. **Test your changes**
   - Test all CLI commands
   - Verify bash scripts work on different platforms
   - Run Unity's built-in tests if applicable

4. **Commit your changes**
   ```bash
   git add .
   git commit -m "feat: add description of your feature"
   ```

5. **Push and create PR**
   ```bash
   git push origin feature/your-feature-name
   ```
   Then create a pull request on GitHub.

## 📝 Coding Standards

### C# Code Style

- Follow Unity's C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public methods
- Keep methods focused and small
- Use proper error handling and logging

### Example Method Structure

```csharp
/// <summary>
/// Creates a new Unity scene with specified parameters
/// </summary>
/// <param name="sceneName">Name of the scene to create</param>
/// <param name="scenePath">Directory path where scene will be created</param>
/// <param name="sceneSetup">Unity scene setup type</param>
/// <returns>True if scene was created successfully</returns>
public static bool CreateScene(string sceneName, string scenePath, string sceneSetup = "Empty")
{
    if (!ValidateParameters(sceneName, scenePath))
        return false;

    try
    {
        // Implementation here
        Debug.Log($"[UnityCLI] Successfully created scene: {sceneName}");
        return true;
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[UnityCLI] Failed to create scene: {e.Message}");
        return false;
    }
}
```

### Bash Script Standards

- Use proper error handling with exit codes
- Include usage information and examples
- Make scripts portable across different environments
- Add comments for complex operations

### Example Bash Script Structure

```bash
#!/bin/bash

# Unity Vibe CLI - Script Description
# Brief description of what the script does
# Usage: script-name <required> [optional]

# Check arguments
if [ $# -lt 1 ]; then
    echo "Usage: script-name <required> [optional]"
    echo "Description of what the script does"
    exit 1
fi

# Main logic here

# Check exit code and provide feedback
if [ $? -eq 0 ]; then
    echo "✅ Operation completed successfully"
else
    echo "❌ Operation failed"
    exit 1
fi
```

## 🧪 Testing

### Manual Testing Checklist

Before submitting a PR, please test:

- [ ] Scene creation with different types
- [ ] Canvas creation with various configurations  
- [ ] CLI commands work from bash scripts
- [ ] Unity menu items function correctly
- [ ] Error handling works as expected
- [ ] Cross-platform compatibility (if possible)

### Adding New Tests

When adding new functionality:
1. Create test scenarios for different use cases
2. Test edge cases and error conditions
3. Verify integration with Unity systems
4. Document test procedures

## 📚 Documentation

### Documentation Standards

- Update README.md for new features
- Add inline code comments for complex logic
- Include usage examples
- Update CHANGELOG.md with changes

### Documentation Structure

When documenting new features:
1. Brief description
2. Usage examples
3. Parameter explanations
4. Common use cases
5. Troubleshooting tips

## 🏗️ Project Structure

```
unity-vibe-cli/
├── Assets/
│   └── Editor/
│       ├── UnityVibeCLI.cs         # Main CLI implementation
│       └── UnityVibeCLI.cs.meta    # Unity meta file
├── Scripts/                        # Bash scripts for CLI access
│   ├── unity-create-scene
│   ├── unity-add-canvas
│   ├── unity-list-types
│   └── unity-cli-help
├── Documentation/                  # Additional documentation
├── package.json                    # Unity Package Manager manifest
├── README.md                       # Main documentation
├── LICENSE                         # MIT License
├── CHANGELOG.md                    # Version history
├── CONTRIBUTING.md                 # This file
└── .gitignore                      # Git ignore rules
```

## 🔍 Adding New CLI Commands

To add a new CLI command:

1. **Add method to CLI class**
   ```csharp
   public static bool YourNewCommand(string parameter)
   {
       // Implementation
       return true;
   }
   ```

2. **Add command line entry point**
   ```csharp
   public static void YourNewCommandFromCommandLine()
   {
       // Parse command line arguments
       // Call your method
   }
   ```

3. **Create bash script**
   ```bash
   #!/bin/bash
   # Script implementation
   ```

4. **Update documentation**
   - Add to README.md
   - Update help command
   - Add usage examples

5. **Test thoroughly**
   - Test all parameter combinations
   - Verify error handling
   - Test bash script on different platforms

## 🎯 Code Review Process

### What We Look For

- **Functionality**: Does the code work as intended?
- **Code Quality**: Is the code clean, readable, and maintainable?
- **Testing**: Are there adequate tests and has manual testing been done?
- **Documentation**: Is the feature properly documented?
- **Compatibility**: Does it work across different Unity versions and platforms?

### Review Timeline

- Initial review: Within 3-5 business days
- Follow-up reviews: Within 1-2 business days
- Merge: After approval from at least one maintainer

## 🏆 Recognition

Contributors will be:
- Listed in the project's contributors section
- Mentioned in release notes for significant contributions
- Given credit in the package documentation

## 📞 Getting Help

- **Questions**: Open a discussion on GitHub
- **Issues**: Create an issue with detailed information
- **Direct Contact**: Email contact@unityvibe.com

## 📋 Commit Message Guidelines

We follow conventional commits:

- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, etc.)
- `refactor:` - Code refactoring
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks

Example:
```
feat: add support for custom scene templates

- Add method to create scenes from custom templates
- Update CLI interface to accept template paths
- Add validation for template files
- Update documentation with examples
```

Thank you for contributing to Unity Vibe CLI! 🎉