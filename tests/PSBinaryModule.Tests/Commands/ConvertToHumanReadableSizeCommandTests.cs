using System.Management.Automation;
using FluentAssertions;

namespace PSBinaryModule.Tests.Commands;

public class ConvertToHumanReadableSizeCommandTests
{
    [Theory]
    [InlineData(0, "0.00 B")]
    [InlineData(1, "1.00 B")]
    [InlineData(1023, "1023.00 B")]
    [InlineData(1024, "1.00 KB")]
    [InlineData(1048576, "1.00 MB")]
    [InlineData(1073741824, "1.00 GB")]
    [InlineData(1099511627776, "1.00 TB")]
    public void ProcessRecord_WithDefaultPrecision_ReturnsCorrectFormat(long bytes, string expected)
    {
        // Arrange
        using var ps = PowerShell.Create();
        TestModuleLoader.ImportModule(ps);
        ps.AddCommand("ConvertTo-HumanReadableSize")
          .AddParameter("Bytes", bytes);

        // Act
        var results = ps.Invoke();

        // Assert
        results.Should().HaveCount(1);
        results[0].BaseObject.Should().Be(expected);
    }

    [Theory]
    [InlineData(1536, 0, "1 KB")]
    [InlineData(1536, 1, "1.5 KB")]
    [InlineData(1536, 2, "1.50 KB")]
    [InlineData(1536, 3, "1.500 KB")]
    public void ProcessRecord_WithCustomPrecision_ReturnsCorrectFormat(long bytes, int precision, string expected)
    {
        // Arrange
        using var ps = PowerShell.Create();
        TestModuleLoader.ImportModule(ps);
        ps.AddCommand("ConvertTo-HumanReadableSize")
          .AddParameter("Bytes", bytes)
          .AddParameter("Precision", precision);

        // Act
        var results = ps.Invoke();

        // Assert
        results.Should().HaveCount(1);
        results[0].BaseObject.Should().Be(expected);
    }

    [Fact]
    public void ProcessRecord_WithPipelineInput_ReturnsCorrectFormat()
    {
        // Arrange
        using var ps = PowerShell.Create();
        TestModuleLoader.ImportModule(ps);
        ps.AddScript("1048576 | ConvertTo-HumanReadableSize");

        // Act
        var results = ps.Invoke();

        // Assert
        results.Should().HaveCount(1);
        results[0].BaseObject.Should().Be("1.00 MB");
    }

    [Fact]
    public void ProcessRecord_WithMultiplePipelineInputs_ReturnsMultipleResults()
    {
        // Arrange
        using var ps = PowerShell.Create();
        TestModuleLoader.ImportModule(ps);
        ps.AddScript("1024, 2048, 3072 | ConvertTo-HumanReadableSize");

        // Act
        var results = ps.Invoke();

        // Assert
        results.Should().HaveCount(3);
        results[0].BaseObject.Should().Be("1.00 KB");
        results[1].BaseObject.Should().Be("2.00 KB");
        results[2].BaseObject.Should().Be("3.00 KB");
    }
}
