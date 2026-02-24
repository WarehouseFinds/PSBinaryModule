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
            $commands.Name | Should -Contain 'Get-FileChecksum'
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
            $result | Should -BeOfType ([PSBinaryModule.SystemLocale])
            $result.Name | Should -Not -BeNullOrEmpty
        }

        It 'Should return locale in normalized format' {
            $result = Get-SystemLocale

            $result.Name | Should -Not -Match '_'
            { [System.Globalization.CultureInfo]::GetCultureInfo($result.Name) } | Should -Not -Throw

            $normalized = [System.Globalization.CultureInfo]::GetCultureInfo($result.Name).Name
            $result.Name | Should -Be $normalized
        }
    }

    Context 'Get-FileChecksum' {
        BeforeAll {
            # Create a temporary test file
            $script:tempFile = Join-Path $TestDrive 'testfile.txt'
            'Hello, World!' | Out-File -FilePath $script:tempFile -NoNewline -Encoding utf8
        }

        It 'Should calculate SHA256 hash by default' {
            $result = Get-FileChecksum -Path $script:tempFile
            
            $result | Should -Not -BeNullOrEmpty
            $result | Should -BeOfType ([PSBinaryModule.Commands.FileChecksumResult])
            $result.Algorithm | Should -Be 'SHA256'
            $result.Hash | Should -Not -BeNullOrEmpty
            $result.Hash.Length | Should -Be 64  # SHA256 produces 64 hex characters
        }

        It 'Should calculate MD5 hash when specified' {
            $result = Get-FileChecksum -Path $script:tempFile -Algorithm MD5
            
            $result.Algorithm | Should -Be 'MD5'
            $result.Hash | Should -Not -BeNullOrEmpty
            $result.Hash.Length | Should -Be 32  # MD5 produces 32 hex characters
        }

        It 'Should calculate SHA1 hash when specified' {
            $result = Get-FileChecksum -Path $script:tempFile -Algorithm SHA1
            
            $result.Algorithm | Should -Be 'SHA1'
            $result.Hash | Should -Not -BeNullOrEmpty
            $result.Hash.Length | Should -Be 40  # SHA1 produces 40 hex characters
        }

        It 'Should calculate SHA512 hash when specified' {
            $result = Get-FileChecksum -Path $script:tempFile -Algorithm SHA512
            
            $result.Algorithm | Should -Be 'SHA512'
            $result.Hash | Should -Not -BeNullOrEmpty
            $result.Hash.Length | Should -Be 128  # SHA512 produces 128 hex characters
        }

        It 'Should include file path in result' {
            $result = Get-FileChecksum -Path $script:tempFile
            
            $result.Path | Should -Not -BeNullOrEmpty
            $result.Path | Should -BeLike "*testfile.txt"
        }

        It 'Should include file size in result' {
            $result = Get-FileChecksum -Path $script:tempFile
            
            $result.FileSize | Should -BeGreaterThan 0
        }

        It 'Should produce consistent hash for same file' {
            $result1 = Get-FileChecksum -Path $script:tempFile
            $result2 = Get-FileChecksum -Path $script:tempFile
            
            $result1.Hash | Should -Be $result2.Hash
        }

        It 'Should error on non-existent file' {
            { Get-FileChecksum -Path 'C:\NonExistent\File.txt' -ErrorAction Stop } | Should -Throw
        }

        It 'Should accept pipeline input' {
            $result = Get-Item $script:tempFile | Get-FileChecksum
            
            $result | Should -Not -BeNullOrEmpty
            $result.Hash | Should -Not -BeNullOrEmpty
        }
    }
}

AfterAll {
    Remove-Module PSBinaryModule -Force -ErrorAction SilentlyContinue
}
