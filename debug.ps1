# Debug script for testing the module during development

[CmdletBinding()]
param()

# Build the module first
Write-Host "Building module..." -ForegroundColor Cyan
Invoke-Build -Task Build

# Import the built module
$modulePath = Join-Path $PSScriptRoot 'build/out/PSBinaryModule/PSBinaryModule.psd1'

if (Test-Path $modulePath) {
    Write-Host "Importing module from: $modulePath" -ForegroundColor Cyan
    Import-Module $modulePath -Force -Verbose

    # Display available commands
    Write-Host "`nAvailable Commands:" -ForegroundColor Green
    Get-Command -Module PSBinaryModule | Format-Table -AutoSize

    Write-Host "`nModule is ready for testing!" -ForegroundColor Green
    Write-Host "Try running:" -ForegroundColor Yellow
    Write-Host "  Get-BinaryModuleMetadata" -ForegroundColor White
    Write-Host "  ConvertTo-HumanReadableSize -Bytes 1048576" -ForegroundColor White
    Write-Host "  1024 | ConvertTo-HumanReadableSize" -ForegroundColor White
}
else {
    Write-Error "Module not found at $modulePath. Build may have failed."
}
