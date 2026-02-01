BeforeAll {
    # Import the built module
    $modulePath = Join-Path $PSScriptRoot '../../build/out/PSBinaryModule/PSBinaryModule.psd1'
    if (Test-Path $modulePath) {
        Import-Module $modulePath -Force
    }
    else {
        throw "Module not found at $modulePath. Build the module first."
    }
}

Describe 'PSBinaryModule Integration Tests' -Tag 'Integration' {
    
    Context 'Module Import' {
        It 'Should import the module successfully' {
            $module = Get-Module -Name PSBinaryModule
            $module | Should -Not -BeNullOrEmpty
            $module.Name | Should -Be 'PSBinaryModule'
        }

        It 'Should export the expected cmdlets' {
            $commands = Get-Command -Module PSBinaryModule
            $commands.Name | Should -Contain 'Get-BinaryModuleMetadata'
            $commands.Name | Should -Contain 'ConvertTo-HumanReadableSize'
        }

        It 'Should have correct module version' {
            $module = Get-Module -Name PSBinaryModule
            $module.Version | Should -Not -BeNullOrEmpty
        }
    }

    Context 'Get-BinaryModuleMetadata' {
        It 'Should return metadata object' {
            $result = Get-BinaryModuleMetadata
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be 'PSBinaryModule'
        }

        It 'Should return basic metadata by default' {
            $result = Get-BinaryModuleMetadata
            $result.Name | Should -Not -BeNullOrEmpty
            $result.Version | Should -Not -BeNullOrEmpty
            $result.Author | Should -Not -BeNullOrEmpty
            $result.PowerShellVersion | Should -BeNullOrEmpty
        }

        It 'Should return detailed metadata when requested' {
            $result = Get-BinaryModuleMetadata -Detailed
            $result.PowerShellVersion | Should -Not -BeNullOrEmpty
            $result.DotNetVersion | Should -Not -BeNullOrEmpty
            $result.CLRVersion | Should -Not -BeNullOrEmpty
        }
    }

    Context 'ConvertTo-HumanReadableSize' {
        It 'Should convert bytes to KB' {
            $result = ConvertTo-HumanReadableSize -Bytes 1024
            $result | Should -Be '1.00 KB'
        }

        It 'Should convert bytes to MB' {
            $result = ConvertTo-HumanReadableSize -Bytes 1048576
            $result | Should -Be '1.00 MB'
        }

        It 'Should handle pipeline input' {
            $result = 2048 | ConvertTo-HumanReadableSize
            $result | Should -Be '2.00 KB'
        }

        It 'Should accept custom precision' {
            $result = ConvertTo-HumanReadableSize -Bytes 1536 -Precision 1
            $result | Should -Be '1.5 KB'
        }

        It 'Should handle zero bytes' {
            $result = ConvertTo-HumanReadableSize -Bytes 0
            $result | Should -Be '0.00 B'
        }

        It 'Should handle large values' {
            $result = ConvertTo-HumanReadableSize -Bytes 1099511627776
            $result | Should -Be '1.00 TB'
        }
    }

    Context 'Error Handling' {
        It 'Should handle negative bytes gracefully' {
            { ConvertTo-HumanReadableSize -Bytes -1 } | Should -Throw
        }

        It 'Should handle invalid precision gracefully' {
            { ConvertTo-HumanReadableSize -Bytes 1024 -Precision -1 } | Should -Throw
        }

        It 'Should handle precision greater than 10' {
            { ConvertTo-HumanReadableSize -Bytes 1024 -Precision 11 } | Should -Throw
        }
    }
}

AfterAll {
    Remove-Module PSBinaryModule -Force -ErrorAction SilentlyContinue
}
