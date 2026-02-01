# PSBinaryModule

[![CI](https://github.com/yourusername/PSBinaryModule/actions/workflows/ci.yml/badge.svg)](https://github.com/yourusername/PSBinaryModule/actions/workflows/ci.yml)
[![Release](https://github.com/yourusername/PSBinaryModule/actions/workflows/release.yml/badge.svg)](https://github.com/yourusername/PSBinaryModule/actions/workflows/release.yml)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/PSBinaryModule.svg)](https://www.powershellgallery.com/packages/PSBinaryModule)
[![License](https://img.shields.io/github/license/yourusername/PSBinaryModule.svg)](LICENSE)

A production-ready PowerShell binary module template with complete CI/CD pipeline, unit testing, and development container support.

## Features

- ✅ **PowerShell Binary Module** - Built with C# for high performance
- ✅ **Complete CI/CD Pipeline** - GitHub Actions workflows for CI and release
- ✅ **Unit Testing** - xUnit tests for C# code with code coverage
- ✅ **Integration Testing** - Pester tests for PowerShell module functionality
- ✅ **Cross-Platform** - Works on Windows, Linux, and macOS
- ✅ **Development Container** - Ready-to-use dev container configuration
- ✅ **Automated Versioning** - GitVersion for semantic versioning
- ✅ **Code Quality** - EditorConfig for consistent code style
- ✅ **Documentation** - XML documentation and help files

## Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [PowerShell 7.4](https://github.com/PowerShell/PowerShell/releases) or later (or Windows PowerShell 5.1+)

### Installation

```powershell
# Install from PowerShell Gallery
Install-Module -Name PSBinaryModule -Scope CurrentUser

# Or install from source
git clone https://github.com/yourusername/PSBinaryModule.git
cd PSBinaryModule
pwsh -Command "Invoke-Build"
```

### Building

```powershell
# Build the module
Invoke-Build

# Build with specific version
Invoke-Build -SemanticVersion "1.2.3"

# Run tests
Invoke-Build -Task Test

# Clean build artifacts
Invoke-Build -Task Clean
```

## Usage

### Get Module Metadata

```powershell
# Get basic metadata
Get-BinaryModuleMetadata

# Get detailed metadata
Get-BinaryModuleMetadata -Detailed
```

### Convert Bytes to Human-Readable Size

```powershell
# Convert bytes to MB
ConvertTo-HumanReadableSize -Bytes 1048576
# Output: 1.00 MB

# Use pipeline
2048 | ConvertTo-HumanReadableSize
# Output: 2.00 KB

# Custom precision
ConvertTo-HumanReadableSize -Bytes 1536 -Precision 1
# Output: 1.5 KB
```

## Development

### Development Container

This project includes a development container configuration for Visual Studio Code. To use it:

1. Install [Docker](https://www.docker.com/products/docker-desktop) and [Visual Studio Code](https://code.visualstudio.com/)
2. Install the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
3. Open the project in VS Code
4. Click "Reopen in Container" when prompted

### Project Structure

```
PSBinaryModule/
├── .devcontainer/          # Development container configuration
├── .github/
│   └── workflows/          # GitHub Actions workflows
├── .vscode/                # VS Code settings and tasks
├── src/
│   ├── Commands/           # Cmdlet implementations
│   ├── en-US/              # Help files
│   ├── PSBinaryModule.csproj
│   ├── PSBinaryModule.psd1 # Module manifest
│   └── PSBinaryModule.Format.ps1xml
├── tests/
│   ├── Integration/        # PowerShell integration tests
│   └── PSBinaryModule.Tests/  # C# unit tests
├── PSBinaryModule.build.ps1   # Build script
├── PSBinaryModule.sln      # Visual Studio solution
└── GitVersion.yml          # Version configuration
```

### Adding New Cmdlets

1. Create a new C# file in `src/Commands/`
2. Inherit from `PSCmdlet` and implement your cmdlet
3. Add XML documentation comments
4. Build and test

Example:

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

### Running Tests

```powershell
# Run all tests
Invoke-Build -Task Test

# Run C# unit tests only
dotnet test

# Run PowerShell integration tests only
Invoke-Pester -Path ./tests/Integration
```

## CI/CD Pipeline

This template includes two GitHub Actions workflows:

### CI Workflow

Runs on every push and pull request:
- Builds the module on Windows, Linux, and macOS
- Runs C# unit tests with code coverage
- Runs PowerShell integration tests
- Uploads test results and artifacts

### Release Workflow

Runs on pushes to main branch:
- Determines semantic version using GitVersion
- Builds and tests the module
- Creates a GitHub release
- Publishes to PowerShell Gallery (if configured)

## Configuration

### PowerShell Gallery Publishing

To enable automatic publishing to PowerShell Gallery:

1. Get an API key from [PowerShell Gallery](https://www.powershellgallery.com/)
2. Add it as a repository secret named `PSGALLERY_API_KEY`
3. Update the module manifest GUID and metadata

### GitVersion

Versioning is controlled by `GitVersion.yml`. The default configuration:
- `main` branch: Minor version increment
- `release/*` branches: Patch version with beta tag
- `feature/*` branches: Minor version with alpha tag
- `hotfix/*` branches: Patch version with hotfix tag

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [PowerShellStandard.Library](https://www.nuget.org/packages/PowerShellStandard.Library/)
- Testing with [xUnit](https://xunit.net/) and [Pester](https://pester.dev/)
- CI/CD powered by [GitHub Actions](https://github.com/features/actions)
- Versioning with [GitVersion](https://gitversion.net/)
