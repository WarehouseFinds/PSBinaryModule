# Quick Start Guide

Get started with PSBinaryModule development in 5 minutes!

## Prerequisites

Install these tools first:

```powershell
# Install .NET 8.0 SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0

# Install PowerShell 7 (if not already installed)
# Download from: https://github.com/PowerShell/PowerShell/releases

# Verify installations
dotnet --version    # Should show 8.0.x
pwsh --version      # Should show 7.4.x or higher
```

## Option 1: Using Dev Container (Recommended)

If you have Docker and VS Code:

1. **Open in VS Code**

   ```bash
   code .
   ```

2. **Reopen in Container**
   - Click "Reopen in Container" when prompted
   - Or: Press `F1` → "Dev Containers: Reopen in Container"

3. **Wait for setup** (first time only, takes ~2 minutes)

4. **You're ready!** The container includes:
   - .NET 8.0 SDK
   - PowerShell 7
   - All required extensions
   - All PowerShell modules

## Option 2: Local Development

1. **Clone the repository**

   ```bash
   git clone https://github.com/yourusername/PSBinaryModule.git
   cd PSBinaryModule
   ```

2. **Install PowerShell dependencies**

   ```powershell
   Install-PSResource -Name InvokeBuild, Pester -TrustRepository -AcceptLicense
   ```

3. **Restore .NET dependencies**

   ```powershell
   dotnet restore
   ```

## Your First Build

```powershell
# Build the module
Invoke-Build

# The module is now at: build/out/PSBinaryModule/
```

## Test the Module

```powershell
# Import the built module
Import-Module ./build/out/PSBinaryModule/PSBinaryModule.psd1

# Try the cmdlets
# Returns a normalized culture name (for example en-US)
Get-SystemLocale

# Remove module when done
Remove-Module PSBinaryModule
```

## Run Tests

```powershell
# Run all tests (C# + PowerShell)
Invoke-Build -Task Test

# Or run C# tests only
dotnet test

# Or run PowerShell tests only
Invoke-Pester -Path ./tests/Integration
```

## Create Your First Cmdlet

1. **Create a new cmdlet file**: `src/Commands/GetExampleCommand.cs`

   ```csharp
   using System.Management.Automation;

   namespace PSBinaryModule.Commands
   {
       [Cmdlet(VerbsCommon.Get, "Example")]
       [OutputType(typeof(string))]
       public class GetExampleCommand : PSCmdlet
       {
           [Parameter(Mandatory = true)]
           public string Name { get; set; }

           protected override void ProcessRecord()
           {
               WriteObject($"Hello, {Name}!");
           }
       }
   }
   ```

2. **Update the module manifest**: `src/PSBinaryModule.psd1`

   ```powershell
   CmdletsToExport = @(
         'Get-SystemLocale',
       'Get-Example'  # Add this line
   )
   ```

3. **Build and test**

   ```powershell
   # Build
   Invoke-Build

   # Import and test
   Import-Module ./build/out/PSBinaryModule/PSBinaryModule.psd1 -Force
   Get-Example -Name "World"
   # Output: Hello, World!
   ```

## Debug Your Cmdlet

### Using VS Code

1. Open `debug.ps1` in VS Code
2. Set a breakpoint in your cmdlet code (click left of line number)
3. Press `F5` to start debugging
4. Your breakpoint will be hit when the cmdlet runs

### Using PowerShell Debugger

```powershell
# Set a breakpoint on your cmdlet
Set-PSBreakpoint -Command Get-Example

# Run the cmdlet - it will break at entry
Get-Example -Name "Test"

# Debug commands:
# s = Step into
# v = Step over
# c = Continue
# q = Quit
```

## Common Tasks

### Clean build artifacts

```powershell
Invoke-Build -Task Clean
```

### Build with specific version

```powershell
Invoke-Build -SemanticVersion "1.2.3"
```

### Run specific test

```powershell
# C# test
dotnet test --filter "FullyQualifiedName~GetExampleCommandTests"

# PowerShell test
Invoke-Pester -Path ./tests/Integration/Module.Integration.Tests.ps1
```

### View available build tasks

```powershell
Invoke-Build ?
```

## VS Code Shortcuts

| Shortcut | Action |
| ---------- | -------- |
| `F5` | Start debugging |
| `Ctrl+Shift+B` | Build module |
| `Ctrl+Shift+T` | Run tests |
| `F12` | Go to definition |
| `Shift+F12` | Find all references |
| `Ctrl+.` | Quick fix / refactor |

## Useful Commands

```powershell
# See all module commands
Get-Command -Module PSBinaryModule

# Get help for a cmdlet
Get-Help Get-SystemLocale -Full

# View cmdlet source (in VS Code)
# Ctrl+Click on cmdlet name

# Format code
# Save file (formats automatically if configured)

# Run InvokeBuild tasks
Invoke-Build -Task Build      # Build only
Invoke-Build -Task Test       # Test only
Invoke-Build -Task Clean      # Clean only
Invoke-Build                  # Default task (Clean + Build)
```

## Next Steps

Now you're ready to develop! Check out:

- [Development Guide](docs/development.md) - In-depth development documentation
- [Binary vs Script Modules](docs/binary-vs-script-modules.md) - Understanding the differences
- [CI/CD Guide](docs/ci-cd.md) - Setting up continuous integration
- [Contributing Guide](CONTRIBUTING.md) - How to contribute

## Troubleshooting

### "Module not found" after build

- Make sure build succeeded without errors
- Check that `build/out/PSBinaryModule/PSBinaryModule.psd1` exists
- Try running `Invoke-Build -Task Clean` then rebuild

### "dotnet: command not found"

- Install .NET 8.0 SDK from <https://dotnet.microsoft.com/download>
- Restart terminal after installation
- Verify with `dotnet --version`

### "Invoke-Build: command not found"

- Run: `Install-PSResource -Name InvokeBuild -TrustRepository`
- Or use full path: `& (Get-PSResource InvokeBuild).InstalledLocation\Invoke-Build.ps1`

### "Cannot find type [PSCmdlet]"

- You need PowerShellStandard.Library NuGet package
- Run: `dotnet restore` in project directory

### Tests fail with "Module not loaded"

- Build the module first: `Invoke-Build`
- Make sure you're running integration tests after building

## Getting Help

- Read the [docs](docs/) folder
- Open an [issue](https://github.com/yourusername/PSBinaryModule/issues)
- Check [existing issues](https://github.com/yourusername/PSBinaryModule/issues)

Happy coding! 🚀
