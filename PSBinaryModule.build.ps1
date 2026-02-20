#requires -modules InvokeBuild

<#
.SYNOPSIS
    Build script for the 'PSBinaryModule' PowerShell binary module

.DESCRIPTION
    This script contains the tasks for building the 'PSBinaryModule' PowerShell binary module
#>
[CmdletBinding()]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSReviewUnusedParameter', '',
    Justification = 'Suppress false positives in Invoke-Build tasks')]
param (
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $SemanticVersion,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $NugetApiKey,

    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [String]
    $Configuration = 'Release'
)

# Synopsis: Default task
task . Clean, Build

# Setup build environment
Enter-Build {
    $script:moduleName = 'PSBinaryModule'
    $script:srcPath = Join-Path -Path $BuildRoot -ChildPath 'src'
    $script:testPath = Join-Path -Path $BuildRoot -ChildPath 'tests'
    $script:testOutputPath = Join-Path -Path $BuildRoot -ChildPath 'test-results'
    $script:buildPath = Join-Path -Path $BuildRoot -ChildPath 'build'
    $script:outputPath = Join-Path -Path $buildPath -ChildPath "out/$moduleName"
    $script:solutionFile = Join-Path -Path $BuildRoot -ChildPath "$moduleName.sln"
}

# Synopsis: Restore NuGet packages
task Restore {
    exec { dotnet restore $solutionFile }
}

# Synopsis: Build the C# project
task Compile Restore, {
    $buildArgs = @(
        'build'
        $solutionFile
        '--configuration', $Configuration
        '--no-restore'
        '--verbosity', 'minimal'
    )

    if ($SemanticVersion) {
        $buildArgs += '/p:Version={0}' -f $SemanticVersion
    }

    exec { dotnet @buildArgs }
}

# Synopsis: Run C# unit tests with code coverage
task UnitTests Compile, {
    if (-not (Test-Path $testOutputPath)) {
        [void] (New-Item -Path $testOutputPath -ItemType Directory)
    }

    $testArgs = @(
        'test'
        $solutionFile
        '--configuration', $Configuration
        '--no-build'
        '--verbosity', 'normal'
        '--logger', "trx;LogFileName=$testOutputPath/unit-tests.trx"
        '--collect', 'XPlat Code Coverage'
        '--results-directory', $testOutputPath
    )

    exec { dotnet @testArgs }

    # Move coverage file to expected location
    $coverageFile = Get-ChildItem -Path $testOutputPath -Recurse -Filter 'coverage.cobertura.xml' | Select-Object -First 1
    if ($coverageFile) {
        $coverageDir = Join-Path -Path $testOutputPath -ChildPath 'codecoverage'
        if (-not (Test-Path $coverageDir)) {
            [void] (New-Item -Path $coverageDir -ItemType Directory)
        }
        Copy-Item -Path $coverageFile.FullName -Destination (Join-Path -Path $coverageDir -ChildPath 'code-coverage.xml') -Force
    }
}

# Synopsis: Run PowerShell integration tests
task IntegrationTests {
    if (-not (Test-Path $testOutputPath)) {
        [void] (New-Item -Path $testOutputPath -ItemType Directory)
    }

    $integrationTestPath = Join-Path -Path $testPath -ChildPath 'Integration'
    if (-not (Test-Path $integrationTestPath)) {
        Write-Warning "No integration tests found at '$integrationTestPath'"
        return
    }

    $config = New-PesterConfiguration @{
        Run        = @{
            Path     = $integrationTestPath
            PassThru = $true
            Exit     = $true
        }
        TestResult = @{
            Enabled      = $true
            OutputFormat = 'NUnitXml'
            OutputPath   = "$testOutputPath/integration-tests.xml"
        }
        Filter     = @{
            Tag = 'Integration'
        }
        Output     = @{
            Verbosity = 'Detailed'
        }
    }

    Invoke-Pester -Configuration $config
}

# Synopsis: Run all tests
task Test UnitTests

# Synopsis: Build the module
task Build Compile, {
    # Create output directory
    if (-not (Test-Path $outputPath)) {
        [void] (New-Item -Path $outputPath -ItemType Directory -Force)
    }

    # Copy compiled DLL
    $dllPath = Join-Path -Path $srcPath -ChildPath "bin/$Configuration/$moduleName.dll"
    Copy-Item -Path $dllPath -Destination $outputPath -Force

    # Copy module manifest
    $manifestPath = Join-Path -Path $srcPath -ChildPath "$moduleName.psd1"
    $manifestContent = Get-Content -Path $manifestPath -Raw

    # Update version in manifest if SemanticVersion is provided
    if ($SemanticVersion) {
        $manifestContent = $manifestContent -replace "ModuleVersion\s*=\s*'[^']*'", "ModuleVersion = '$SemanticVersion'"
    }

    $outputManifest = Join-Path -Path $outputPath -ChildPath "$moduleName.psd1"
    Set-Content -Path $outputManifest -Value $manifestContent -Force

    # Copy format file
    $formatPath = Join-Path -Path $srcPath -ChildPath "$moduleName.Format.ps1xml"
    if (Test-Path $formatPath) {
        Copy-Item -Path $formatPath -Destination $outputPath -Force
    }

    # Copy help files
    $helpPath = Join-Path -Path $srcPath -ChildPath 'en-US'
    if (Test-Path $helpPath) {
        $outputHelpPath = Join-Path -Path $outputPath -ChildPath 'en-US'
        if (-not (Test-Path $outputHelpPath)) {
            [void] (New-Item -Path $outputHelpPath -ItemType Directory -Force)
        }
        Copy-Item -Path "$helpPath/*" -Destination $outputHelpPath -Recurse -Force
    }

    # Copy dependencies
    $binPath = Join-Path -Path $srcPath -ChildPath "bin/$Configuration"
    Get-ChildItem -Path $binPath -Filter '*.dll' | Where-Object {
        $_.Name -ne "$moduleName.dll" -and
        $_.Name -ne 'PowerShellStandard.Library.dll'
    } | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $outputPath -Force
    }

    # Copy framework assemblies from .NET runtime for self-contained distribution
    $dotnetRoot = $env:DOTNET_ROOT
    if (-not $dotnetRoot) {
        $dotnetInfo = & dotnet --info
        $sdkPath = $dotnetInfo | Select-String 'Base Path:' | ForEach-Object { $_.Line -replace '.*Base Path:\s*', '' }
        if ($sdkPath) {
            $dotnetRoot = Split-Path -Parent (Split-Path -Parent $sdkPath)
        }
    }

    if ($dotnetRoot) {
        $runtimeDirs = Get-ChildItem -Path "$dotnetRoot/shared/Microsoft.NETCore.App" -Directory | Sort-Object -Property Name -Descending
        if ($runtimeDirs) {
            $frameworkDir = $runtimeDirs[0].FullName
            # Copy all System.* framework assemblies for self-contained module
            Get-ChildItem -Path $frameworkDir -Filter 'System.*.dll' | ForEach-Object {
                Copy-Item -Path $_.FullName -Destination $outputPath -Force
            }
        }
    }

    # Copy the deps.json file which helps with assembly resolution
    $depsFile = Join-Path -Path $binPath -ChildPath "$moduleName.deps.json"
    if (Test-Path $depsFile) {
        Copy-Item -Path $depsFile -Destination $outputPath -Force
    }

    Write-Build Green "Module built successfully at: $outputPath"
}

# Synopsis: Create a NuGet package for the module
task Package {
    $packageOutputPath = Join-Path -Path $buildPath -ChildPath 'package'
    if (!(Test-Path $packageOutputPath)) {
        [void] (New-Item -Path $packageOutputPath -ItemType Directory -Force)
    }

    $requestParam = @{
        Name               = "$($moduleName)_local_feed"
        SourceLocation     = $packageOutputPath
        PublishLocation    = $packageOutputPath
        InstallationPolicy = 'Trusted'
        ErrorAction        = 'Stop'
    }
    [void] (Register-PSRepository @requestParam)

    $requestParam = @{
        Path        = (Join-Path -Path $buildPath -ChildPath "out/$moduleName")
        Repository  = "$($moduleName)_local_feed"
        NuGetApiKey = 'ABC123'
        ErrorAction = 'Stop'
    }
    [void] (Publish-Module @requestParam)

    [void] (Unregister-PSRepository -Name "$($moduleName)_local_feed")

}

# Synopsis: Publish the module to PSGallery
task Publish -If ($NugetApiKey) {
    $requestParam = @{
        Path        = $outputPath
        NuGetApiKey = $NugetApiKey
        ErrorAction = 'Stop'
    }
    [void] (Publish-Module @requestParam)
}

# Synopsis: Clean up the build directory
task Clean {
    if (Test-Path $buildPath) {
        Write-Warning "Removing build output folder at '$buildPath'"
        $requestParam = @{
            Path    = $buildPath
            Recurse = $true
            Force   = $true
        }
        [void] (Remove-Item @requestParam)
    }

    # Clean dotnet build artifacts
    $binDirs = Get-ChildItem -Path $BuildRoot -Directory -Recurse -Filter 'bin'
    $objDirs = Get-ChildItem -Path $BuildRoot -Directory -Recurse -Filter 'obj'

    foreach ($dir in ($binDirs + $objDirs)) {
        Write-Warning "Removing $($dir.FullName)"
        Remove-Item -Path $dir.FullName -Recurse -Force
    }
}
