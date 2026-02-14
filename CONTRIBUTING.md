# Contributing to PSBinaryModule

Thank you for your interest in contributing to PSBinaryModule! This document provides guidelines and instructions for contributing.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for all contributors.

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- PowerShell 7.4 or later (or Windows PowerShell 5.1+)
- Git
- Visual Studio Code (recommended) or Visual Studio

### Development Setup

1. **Fork and Clone**

   ```bash
   git clone https://github.com/yourusername/PSBinaryModule.git
   cd PSBinaryModule
   ```

2. **Open in Dev Container** (recommended)
   - Open the project in VS Code
   - Click "Reopen in Container" when prompted
   - Wait for the container to build and dependencies to install

3. **Or Setup Locally**

   ```powershell
   # Install build dependencies
   Install-PSResource -Name InvokeBuild, Pester -TrustRepository -AcceptLicense

   # Restore .NET dependencies
   dotnet restore
   ```

## Development Workflow

### Building

```powershell
# Build the module
Invoke-Build

# Build and run tests
Invoke-Build -Task Build, Test

# Clean build artifacts
Invoke-Build -Task Clean
```

### Testing

#### C# Unit Tests

```powershell
# Run all C# tests
dotnet test

# Run tests with coverage
dotnet test --collect "XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~GetBinaryModuleMetadataCommandTests"
```

#### PowerShell Integration Tests

```powershell
# Run all integration tests
Invoke-Pester -Path ./tests/Integration

# Run specific test
Invoke-Pester -Path ./tests/Integration -Tag Integration
```

### Code Style

- Follow the EditorConfig settings
- Use meaningful variable and method names
- Add XML documentation to all public APIs
- Keep methods focused and single-purpose
- Maintain test coverage above 80%

### C# Conventions

```csharp
// Good
[Cmdlet(VerbsCommon.Get, "Example")]
[OutputType(typeof(string))]
public class GetExampleCommand : PSCmdlet
{
    /// <summary>
    /// <para type="description">The name parameter description.</para>
    /// </summary>
    [Parameter(Mandatory = true)]
    public string Name { get; set; }

    protected override void ProcessRecord()
    {
        WriteObject($"Hello, {Name}!");
    }
}
```

### PowerShell Conventions

```powershell
# Good
Describe 'Get-Example' {
    It 'Should return greeting' {
        $result = Get-Example -Name 'World'
        $result | Should -Be 'Hello, World!'
    }
}
```

## Adding New Features

### Adding a New Cmdlet

1. **Create the Cmdlet Class**

   ```csharp
   // src/Commands/YourCommand.cs
   using System.Management.Automation;

   namespace PSBinaryModule.Commands
   {
       [Cmdlet(VerbsCommon.Get, "YourFeature")]
       public class GetYourFeatureCommand : PSCmdlet
       {
           // Implementation
       }
   }
   ```

2. **Add Unit Tests**

   ```csharp
   // tests/PSBinaryModule.Tests/Commands/YourCommandTests.cs
   public class GetYourFeatureCommandTests
   {
       [Fact]
       public void TestYourFeature()
       {
           // Test implementation
       }
   }
   ```

3. **Update Module Manifest**

   ```powershell
   # src/PSBinaryModule.psd1
   CmdletsToExport = @(
         'Get-SystemLocale',
       'Get-YourFeature'  # Add your cmdlet
   )
   ```

4. **Add Integration Tests**

   ```powershell
   # tests/Integration/Module.Integration.Tests.ps1
   Context 'Get-YourFeature' {
       It 'Should work correctly' {
           # Test implementation
       }
   }
   ```

## Submitting Changes

### Branch Naming

- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `hotfix/description` - Critical fixes
- `docs/description` - Documentation updates

### Commit Messages

Use conventional commit format:

```text
type(scope): subject

body

footer
```

Types:

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `test`: Adding or updating tests
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `chore`: Maintenance tasks

Example:

```text
feat(cmdlets): add Get-Example cmdlet

Added new cmdlet to retrieve example data with support for
pipeline input and custom formatting.

Closes #123
```

### Pull Request Process

1. **Create a Branch**

   ```bash
   git checkout -b feature/my-new-feature
   ```

2. **Make Changes**
   - Write code
   - Add tests
   - Update documentation

3. **Run Tests**

   ```powershell
   Invoke-Build -Task Test
   ```

4. **Commit Changes**

   ```bash
   git add .
   git commit -m "feat: add new feature"
   ```

5. **Push to Fork**

   ```bash
   git push origin feature/my-new-feature
   ```

6. **Create Pull Request**
   - Go to GitHub
   - Click "New Pull Request"
   - Fill out the template
   - Wait for CI checks to pass
   - Request review

### Pull Request Guidelines

- Fill out the PR template completely
- Ensure all CI checks pass
- Maintain or improve test coverage
- Update documentation as needed
- Keep PRs focused and reasonably sized
- Respond to review feedback promptly

## Release Process

Releases are automated via GitHub Actions:

1. Merge PR to `main` branch
2. GitVersion determines the version
3. CI builds and tests the module
4. GitHub release is created
5. Module is published to PowerShell Gallery

## Getting Help

- Open an [issue](https://github.com/yourusername/PSBinaryModule/issues) for bugs or feature requests
- Start a [discussion](https://github.com/yourusername/PSBinaryModule/discussions) for questions
- Review existing issues and PRs before creating new ones

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
