using System.Management.Automation;
using FluentAssertions;
using PSBinaryModule.Commands;

namespace PSBinaryModule.Tests.Commands;

public class GetBinaryModuleMetadataCommandTests
{
    [Fact]
    public void ProcessRecord_WithoutDetailed_ReturnsBasicMetadata()
    {
        // Arrange
        using var ps = PowerShell.Create();
        ps.AddCommand("Get-BinaryModuleMetadata");

        // Act
        var results = ps.Invoke();

        // Assert
        results.Should().HaveCount(1);
        var metadata = results[0].BaseObject as ModuleMetadata;
        metadata.Should().NotBeNull();
        metadata!.Name.Should().Be("PSBinaryModule");
        metadata.Version.Should().NotBeEmpty();
        metadata.Author.Should().NotBeEmpty();
        metadata.PowerShellVersion.Should().BeNull();
        metadata.DotNetVersion.Should().BeNull();
        metadata.CLRVersion.Should().BeNull();
    }

    [Fact]
    public void ProcessRecord_WithDetailed_ReturnsDetailedMetadata()
    {
        // Arrange
        using var ps = PowerShell.Create();
        ps.AddCommand("Get-BinaryModuleMetadata")
          .AddParameter("Detailed");

        // Act
        var results = ps.Invoke();

        // Assert
        results.Should().HaveCount(1);
        var metadata = results[0].BaseObject as ModuleMetadata;
        metadata.Should().NotBeNull();
        metadata!.Name.Should().Be("PSBinaryModule");
        metadata.PowerShellVersion.Should().NotBeNull();
        metadata.DotNetVersion.Should().NotBeNull();
        metadata.CLRVersion.Should().NotBeNull();
    }

    [Fact]
    public void ProcessRecord_ReturnsCorrectType()
    {
        // Arrange
        using var ps = PowerShell.Create();
        ps.AddCommand("Get-BinaryModuleMetadata");

        // Act
        var results = ps.Invoke();

        // Assert
        results[0].BaseObject.Should().BeOfType<ModuleMetadata>();
    }
}
