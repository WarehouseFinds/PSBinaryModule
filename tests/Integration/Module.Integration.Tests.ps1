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
            $commands.Name | Should -Contain 'Get-SystemLocale'
        }

        It 'Should have correct module version' {
            $module = Get-Module -Name PSBinaryModule
            $module.Version | Should -Not -BeNullOrEmpty
        }
    }

    Context 'Get-SystemLocale' {
        It 'Should return a locale string' {
            $result = Get-SystemLocale
            $result | Should -Not -BeNullOrEmpty
            $result | Should -BeOfType ([string])
        }

        It 'Should return locale in normalized format' {
            $result = Get-SystemLocale

            $result | Should -Not -Match '_'
            { [System.Globalization.CultureInfo]::GetCultureInfo($result) } | Should -Not -Throw

            $normalized = [System.Globalization.CultureInfo]::GetCultureInfo($result).Name
            $result | Should -Be $normalized
        }
    }
}

AfterAll {
    Remove-Module PSBinaryModule -Force -ErrorAction SilentlyContinue
}
