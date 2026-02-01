# Binary Module vs Script Module Development Guide

This document explains the key differences between PowerShell binary modules (like this template) and PowerShell script modules, and when to use each.

## What is a Binary Module?

A binary module is a PowerShell module where cmdlets are implemented in a compiled .NET language (C#, F#, or VB.NET) and distributed as a DLL assembly.

## What is a Script Module?

A script module is a PowerShell module where functions are implemented directly in PowerShell script (.ps1, .psm1 files).

## When to Use Binary Modules

Choose binary modules when you need:

### Performance
- **CPU-intensive operations**: Binary modules execute much faster than PowerShell script
- **Large-scale data processing**: Compiled code handles large datasets more efficiently
- **Frequent execution**: Cmdlets that run many times benefit from compilation

### Advanced .NET Features
- **Low-level API access**: Direct access to Windows APIs, .NET Framework internals
- **Complex algorithms**: Easier to implement complex data structures and algorithms in C#
- **Third-party libraries**: Seamless integration with NuGet packages and .NET libraries
- **Multi-threading**: Better control over parallel execution and threading

### Strong Typing and Compile-Time Checking
- **Type safety**: Catch errors at compile time instead of runtime
- **IntelliSense**: Better IDE support and autocomplete for cmdlet development
- **Refactoring**: Safer refactoring with compiler guarantees

### Professional Distribution
- **Intellectual property**: Compiled code is harder to reverse-engineer
- **Code protection**: Hide implementation details from end users
- **Enterprise requirements**: Some organizations prefer compiled modules

## When to Use Script Modules

Choose script modules when you need:

### Rapid Development
- **Quick prototyping**: Faster iteration cycle without compilation
- **Simple automation**: Straightforward tasks that don't require complex logic
- **Frequent updates**: Easy to modify and redistribute scripts

### PowerShell-Specific Features
- **Pipeline-centric**: Natural PowerShell pipeline operations
- **Dynamic typing**: Flexibility with PowerShell's dynamic nature
- **PowerShell idioms**: Easier to use PowerShell-specific features like splatting, automatic variables

### Accessibility
- **Easy debugging**: Direct script debugging in PowerShell ISE or VS Code
- **Transparency**: Users can view and learn from source code
- **Community contribution**: Lower barrier for contributions

### Minimal Dependencies
- **No compilation required**: Works without .NET SDK
- **Cross-version compatibility**: Easier to support multiple PowerShell versions

## Key Development Differences

### Project Structure

**Binary Module:**
```
PSBinaryModule/
├── src/
│   ├── Commands/           # C# cmdlet classes
│   ├── PSBinaryModule.csproj
│   └── PSBinaryModule.psd1
├── tests/
│   ├── PSBinaryModule.Tests/  # xUnit C# tests
│   └── Integration/           # Pester integration tests
├── PSBinaryModule.sln
└── PSBinaryModule.build.ps1
```

**Script Module:**
```
PSScriptModule/
├── src/
│   ├── Public/            # Public functions (.ps1)
│   ├── Private/           # Private functions (.ps1)
│   └── PSScriptModule.psd1
├── tests/
│   ├── Unit/              # Pester unit tests
│   └── Integration/       # Pester integration tests
└── PSScriptModule.build.ps1
```

### Development Workflow

**Binary Module:**
1. Write C# code in Visual Studio/VS Code
2. Build with `dotnet build` or `Invoke-Build`
3. Test with xUnit (C#) and Pester (PowerShell)
4. Debug with C# debugger or PowerShell debugger
5. Distribute compiled DLL

**Script Module:**
1. Write PowerShell functions in VS Code
2. Import module or dot-source functions
3. Test with Pester
4. Debug with PowerShell debugger
5. Distribute .ps1/.psm1 files

### Cmdlet Implementation

**Binary Module (C#):**
```csharp
[Cmdlet(VerbsCommon.Get, "Example")]
[OutputType(typeof(string))]
public class GetExampleCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty]
    public string Name { get; set; }

    protected override void ProcessRecord()
    {
        WriteObject($"Hello, {Name}!");
    }
}
```

**Script Module (PowerShell):**
```powershell
function Get-Example {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [string]$Name
    )

    process {
        Write-Output "Hello, $Name!"
    }
}
```

### Testing Approach

**Binary Module:**
- **C# Unit Tests**: xUnit, NUnit, or MSTest for cmdlet logic
- **PowerShell Integration Tests**: Pester for end-to-end scenarios
- **Code Coverage**: Built-in .NET coverage tools

**Script Module:**
- **Pester Tests**: All tests written in PowerShell
- **Code Coverage**: Pester's built-in coverage
- **Mock Functions**: PowerShell's flexible mocking

### Build Process

**Binary Module:**
```powershell
# Requires .NET SDK
dotnet restore
dotnet build --configuration Release
dotnet test

# Or use InvokeBuild
Invoke-Build -Task Build, Test
```

**Script Module:**
```powershell
# No compilation needed
# Just use ModuleBuilder to combine files
Invoke-Build -Task Build, Test
```

### Debugging

**Binary Module:**
- Attach C# debugger to PowerShell process
- Use Visual Studio or VS Code debugger
- Set breakpoints in C# code
- Also supports PowerShell debugging of usage

**Script Module:**
- Use PowerShell ISE or VS Code
- Set breakpoints directly in .ps1 files
- Use `Set-PSBreakpoint` cmdlet
- Built-in PowerShell debugging experience

### Performance Comparison

**Example: Processing 100,000 items**

Binary Module (C#):
```csharp
for (int i = 0; i < items.Count; i++)
{
    // Process item
}
// Execution time: ~50ms
```

Script Module (PowerShell):
```powershell
foreach ($item in $items) {
    # Process item
}
# Execution time: ~500ms
```

## Hybrid Approach

You can combine both approaches:

1. **Core cmdlets in C#**: Performance-critical operations
2. **Helper functions in PowerShell**: Simple wrappers and utilities
3. **Best of both worlds**: Performance where needed, flexibility elsewhere

Example:
```powershell
# PSBinaryModule with PowerShell helpers
# Import compiled cmdlets
Import-Module PSBinaryModule

# Add PowerShell helper functions
function Get-LargeFiles {
    [CmdletBinding()]
    param([string]$Path, [long]$MinSize = 1MB)
    
    Get-ChildItem -Path $Path -Recurse -File |
        Where-Object { $_.Length -gt $MinSize } |
        ForEach-Object {
            [PSCustomObject]@{
                Name = $_.Name
                Size = ConvertTo-HumanReadableSize -Bytes $_.Length  # Binary cmdlet
                Path = $_.FullName
            }
        }
}
```

## Migration Path

### From Script to Binary Module

1. Identify performance-critical functions
2. Implement them in C# as cmdlets
3. Keep simple functions in PowerShell
4. Test thoroughly with integration tests
5. Update documentation and examples

### From Binary to Script Module

1. Replace simple cmdlets with PowerShell functions
2. Keep performance-critical code in C#
3. Update tests to use Pester exclusively
4. Simplify build process

## Recommendation

**Start with a script module** if:
- You're prototyping or learning
- Performance isn't critical
- You want maximum flexibility
- You're building simple automation

**Use a binary module** when:
- Performance is critical
- You need advanced .NET features
- You're building professional products
- You have C# expertise on the team
- You need strong typing and compile-time checking

## Best Practices for Binary Modules

1. **Keep cmdlets focused**: One responsibility per cmdlet
2. **Add comprehensive XML documentation**: Users can't read your C# code
3. **Implement proper parameter validation**: Use attributes effectively
4. **Follow PowerShell naming conventions**: Verb-Noun format
5. **Support pipeline input**: Make cmdlets work well in pipelines
6. **Write integration tests**: Test PowerShell user experience
7. **Provide examples**: Show real-world usage scenarios
8. **Handle errors properly**: Use `WriteError`, `WriteWarning`, etc.
9. **Support ShouldProcess**: For cmdlets that make changes
10. **Version carefully**: Binary modules are harder to hot-swap

## Resources

- [PowerShell SDK Documentation](https://docs.microsoft.com/powershell/scripting/developer/module/writing-a-windows-powershell-module)
- [Writing a PowerShell Binary Module](https://docs.microsoft.com/powershell/scripting/developer/module/writing-a-windows-powershell-module)
- [PowerShellStandard.Library](https://www.nuget.org/packages/PowerShellStandard.Library/)
- [xUnit Testing](https://xunit.net/)
- [Pester Testing](https://pester.dev/)
