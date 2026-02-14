# Development Guide

This guide walks you through developing PowerShell binary modules using this template.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Project Structure](#project-structure)
3. [Creating Cmdlets](#creating-cmdlets)
4. [Testing](#testing)
5. [Building](#building)
6. [Debugging](#debugging)
7. [Best Practices](#best-practices)

## Getting Started

### Prerequisites

Install the required tools:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PowerShell 7.4+](https://github.com/PowerShell/PowerShell/releases)
- [Visual Studio Code](https://code.visualstudio.com/) with C# Dev Kit extension
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for dev containers)

### Initial Setup

```powershell
# Clone the repository
git clone https://github.com/yourusername/PSBinaryModule.git
cd PSBinaryModule

# Install PowerShell dependencies
Install-PSResource -Name InvokeBuild, Pester -TrustRepository -AcceptLicense

# Restore .NET dependencies
dotnet restore

# Build the module
Invoke-Build

# Import and test
Import-Module ./build/out/PSBinaryModule/PSBinaryModule.psd1
# Returns a normalized culture name (for example en-US)
Get-SystemLocale
```

## Project Structure

```text
PSBinaryModule/
├── src/
│   ├── Commands/                      # Cmdlet implementations
│   │   └── GetSystemLocale.cs
│   ├── en-US/                         # Help files
│   │   └── about_PSBinaryModule.help.txt
│   ├── PSBinaryModule.csproj          # C# project file
│   ├── PSBinaryModule.psd1            # Module manifest
│   └── PSBinaryModule.Format.ps1xml   # Custom formatting
│
├── tests/
│   ├── PSBinaryModule.Tests/          # C# unit tests (xUnit)
│   │   ├── Commands/
│   │   │   └── GetSystemLocaleCommandTests.cs
│   │   └── PSBinaryModule.Tests.csproj
│   │
│   └── Integration/                   # PowerShell integration tests (Pester)
│       └── Module.Integration.Tests.ps1
│
├── .devcontainer/                     # Dev container configuration
├── .github/workflows/                 # CI/CD pipelines
├── .vscode/                           # VS Code settings
├── PSBinaryModule.sln                 # Visual Studio solution
├── PSBinaryModule.build.ps1           # Build script (InvokeBuild)
└── GitVersion.yml                     # Versioning configuration
```

## Creating Cmdlets

### Basic Cmdlet Structure

Every cmdlet should:

1. Inherit from `PSCmdlet`
2. Use the `[Cmdlet]` attribute with appropriate verb and noun
3. Implement `ProcessRecord()` for pipeline processing
4. Include XML documentation comments

### Step 1: Create the Cmdlet Class

Create a new file in `src/Commands/`:

```csharp
using System;
using System.Management.Automation;

namespace PSBinaryModule.Commands
{
    /// <summary>
    /// <para type="synopsis">Short description of what the cmdlet does.</para>
    /// <para type="description">Longer description with more details.</para>
    /// <example>
    ///   <code>Get-MyData -Name "Example"</code>
    ///   <para>Description of what this example does.</para>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "MyData")]
    [OutputType(typeof(MyDataObject))]
    public class GetMyDataCommand : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name parameter description.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Enter the name")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Optional switch parameter.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Detailed { get; set; }

        protected override void ProcessRecord()
        {
            var data = new MyDataObject
            {
                Name = Name,
                Timestamp = DateTime.Now
            };

            WriteObject(data);
        }
    }

    public class MyDataObject
    {
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
```

### Parameter Attributes

Common parameter attributes:

```csharp
// Mandatory parameter
[Parameter(Mandatory = true)]
public string RequiredParam { get; set; }

// Pipeline input
[Parameter(ValueFromPipeline = true)]
public string PipelineParam { get; set; }

// Pipeline by property name
[Parameter(ValueFromPipelineByPropertyName = true)]
public string NamedParam { get; set; }

// Position parameter
[Parameter(Position = 0)]
public string FirstParam { get; set; }

// Switch parameter
[Parameter()]
public SwitchParameter Force { get; set; }

// Validation
[ValidateNotNullOrEmpty]
[ValidateRange(1, 100)]
[ValidateSet("Option1", "Option2")]
[ValidatePattern(@"^\d{3}-\d{4}$")]
public string ValidatedParam { get; set; }
```

### Cmdlet Lifecycle Methods

```csharp
// Called once before processing
protected override void BeginProcessing()
{
    // Initialize resources
}

// Called for each pipeline object
protected override void ProcessRecord()
{
    // Main processing logic
}

// Called once after all processing
protected override void EndProcessing()
{
    // Cleanup, final output
}

// Called if Ctrl+C or Stop-Process
protected override void StopProcessing()
{
    // Cancel operations, cleanup
}
```

### Writing Output

```csharp
// Write object to pipeline
WriteObject(myObject);

// Write multiple objects
WriteObject(myCollection, enumerateCollection: true);

// Write verbose message (only shown with -Verbose)
WriteVerbose("Processing item...");

// Write debug message (only shown with -Debug)
WriteDebug($"Variable value: {value}");

// Write warning
WriteWarning("This operation may take a long time.");

// Write error (non-terminating)
WriteError(new ErrorRecord(
    exception,
    "ErrorId",
    ErrorCategory.InvalidOperation,
    targetObject));

// Throw terminating error
ThrowTerminatingError(new ErrorRecord(...));

// Write information (PowerShell 5.1+)
WriteInformation("Info message", new[] { "Tag1", "Tag2" });
```

### Step 2: Update Module Manifest

Add your cmdlet to `src/PSBinaryModule.psd1`:

```powershell
CmdletsToExport = @(
    'Get-SystemLocale',
    'Get-MyData'  # Add your new cmdlet
)
```

### Step 3: Add Unit Tests

Create `tests/PSBinaryModule.Tests/Commands/GetMyDataCommandTests.cs`:

```csharp
using System.Management.Automation;
using FluentAssertions;
using PSBinaryModule.Commands;

namespace PSBinaryModule.Tests.Commands;

public class GetMyDataCommandTests
{
    [Fact]
    public void ProcessRecord_WithValidName_ReturnsData()
    {
        // Arrange
        using var ps = PowerShell.Create();
        ps.AddCommand("Get-MyData")
          .AddParameter("Name", "Test");

        // Act
        var results = ps.Invoke();

        // Assert
        results.Should().HaveCount(1);
        var data = results[0].BaseObject as MyDataObject;
        data.Should().NotBeNull();
        data!.Name.Should().Be("Test");
    }

    [Fact]
    public void ProcessRecord_WithPipelineInput_ProcessesEachItem()
    {
        // Arrange
        using var ps = PowerShell.Create();
        ps.AddScript("'Test1', 'Test2' | Get-MyData");

        // Act
        var results = ps.Invoke();

        // Assert
        results.Should().HaveCount(2);
    }
}
```

### Step 4: Add Integration Tests

Add to `tests/Integration/Module.Integration.Tests.ps1`:

```powershell
Context 'Get-MyData' {
    It 'Should return data object' {
        $result = Get-MyData -Name 'Test'
        $result | Should -Not -BeNullOrEmpty
        $result.Name | Should -Be 'Test'
    }

    It 'Should accept pipeline input' {
        $result = 'Test' | Get-MyData
        $result.Name | Should -Be 'Test'
    }
}
```

## Testing

### Running C# Unit Tests

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect "XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~GetMyDataCommandTests"

# Run with detailed output
dotnet test --verbosity detailed
```

### Running PowerShell Integration Tests

```powershell
# Run all integration tests
Invoke-Pester -Path ./tests/Integration

# Run specific test
Invoke-Pester -Path ./tests/Integration -Tag Integration

# Run with code coverage
Invoke-Pester -Configuration @{
    Run = @{ Path = './tests/Integration' }
    CodeCoverage = @{ Enabled = $true }
}
```

### Test Coverage

Aim for:

- **80%+ code coverage** for C# code
- **100% cmdlet coverage** (all cmdlets have tests)
- **Integration tests** for all user-facing scenarios

## Building

### Using InvokeBuild

```powershell
# Build only
Invoke-Build -Task Build

# Build and test
Invoke-Build -Task Build, Test

# Build with specific version
Invoke-Build -Task Build -SemanticVersion "1.2.3"

# Clean build artifacts
Invoke-Build -Task Clean

# Full build pipeline
Invoke-Build -Task Clean, Build, Test
```

### Using dotnet CLI

```powershell
# Restore dependencies
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Clean
dotnet clean
```

### Build Output

After building, the module is available at:

```text
build/out/PSBinaryModule/
├── PSBinaryModule.dll           # Compiled assembly
├── PSBinaryModule.psd1          # Module manifest
├── PSBinaryModule.Format.ps1xml # Formatting
└── en-US/                       # Help files
```

## Debugging

### Method 1: Attach to PowerShell Process

1. Start PowerShell
2. Import your module
3. In VS Code, press F5 and select ".NET: Attach to Process"
4. Find and attach to `pwsh.exe` or `powershell.exe`
5. Set breakpoints in C# code
6. Run your cmdlet in PowerShell

### Method 2: Debug Launch Configuration

Use the pre-configured launch config in `.vscode/launch.json`:

1. Open `debug.ps1` in VS Code
2. Press F5 to start debugging
3. Set breakpoints in C# code
4. Step through cmdlet execution

### Method 3: PowerShell Debugging

For debugging the PowerShell usage:

```powershell
# Set a breakpoint
Set-PSBreakpoint -Command Get-MyData

# Run cmdlet (will break at command)
Get-MyData -Name "Test"

# Debug commands
# s - Step into
# v - Step over
# o - Step out
# c - Continue
# q - Quit
```

### Debugging Tips

1. **Use WriteDebug**: Add debug output in your code

   ```csharp
   WriteDebug($"Processing: {Name}");
   ```

2. **Check Pipeline Input**: Verify pipeline objects are correct

   ```csharp
   WriteVerbose($"Received pipeline input: {InputObject}");
   ```

3. **Test Error Paths**: Don't just test success scenarios

   ```csharp
   try {
       // Risky operation
   }
   catch (Exception ex) {
       WriteError(new ErrorRecord(ex, "OperationFailed",
           ErrorCategory.InvalidOperation, null));
   }
   ```

## Best Practices

### Code Organization

✅ **Do:**

- One cmdlet per file
- Group related cmdlets in the same namespace
- Use meaningful file and class names
- Keep cmdlets focused on a single responsibility

❌ **Don't:**

- Put multiple cmdlets in one file
- Mix business logic with cmdlet code
- Create God classes that do everything

### Parameter Design

✅ **Do:**

- Support pipeline input where appropriate
- Use standard parameter names (`Name`, `Path`, `Filter`, etc.)
- Add helpful parameter descriptions
- Validate parameters appropriately

❌ **Don't:**

- Require unnecessary mandatory parameters
- Use non-standard parameter names
- Skip validation

### Error Handling

✅ **Do:**

- Use `WriteError` for non-terminating errors
- Use `ThrowTerminatingError` for fatal errors
- Provide meaningful error messages
- Include error IDs for programmatic handling

❌ **Don't:**

- Swallow exceptions silently
- Use generic error messages
- Throw raw exceptions

### Performance

✅ **Do:**

- Process pipeline objects one at a time in `ProcessRecord()`
- Initialize expensive resources in `BeginProcessing()`
- Clean up resources in `EndProcessing()`
- Support `StopProcessing()` for long-running operations

❌ **Don't:**

- Buffer all pipeline input in memory
- Create resources in `ProcessRecord()`
- Leave resources uncleaned

### Documentation

✅ **Do:**

- Add XML comments to all public APIs
- Include `<example>` tags with code samples
- Write clear parameter descriptions
- Update help files when changing cmdlets

❌ **Don't:**

- Skip documentation
- Use vague descriptions
- Leave outdated examples

### Version Compatibility

✅ **Do:**

- Test on Windows PowerShell 5.1 and PowerShell 7+
- Test on Windows, Linux, and macOS
- Document minimum PowerShell version
- Use PowerShellStandard.Library for compatibility

❌ **Don't:**

- Assume PowerShell 7+ only
- Use platform-specific APIs without checks
- Ignore cross-platform testing

## Next Steps

1. Create your first cmdlet following the patterns above
2. Add comprehensive tests
3. Build and test locally
4. Push to GitHub and watch CI pipeline
5. Create a release when ready

For more information, see:

- [Binary vs Script Modules](./binary-vs-script-modules.md)
- [Contributing Guide](../CONTRIBUTING.md)
- [CI/CD Documentation](./ci-cd.md)
