using System;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PSBinaryModule.Tests;

internal static class TestModuleLoader
{
    internal static void ImportModule(PowerShell ps)
    {
        if (ps is null)
        {
            throw new ArgumentNullException(nameof(ps));
        }

        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var releasePath = Path.Combine(repoRoot, "src", "bin", "Release", "PSBinaryModule.dll");
        var debugPath = Path.Combine(repoRoot, "src", "bin", "Debug", "PSBinaryModule.dll");
        var modulePath = File.Exists(releasePath) ? releasePath : debugPath;

        if (!File.Exists(modulePath))
        {
            throw new FileNotFoundException("Compiled module not found. Build the module before running tests.", modulePath);
        }

        ps.AddCommand("Import-Module")
          .AddParameter("Name", modulePath)
          .AddParameter("Force", true);

        ps.Invoke();
        ps.Commands.Clear();

        if (ps.HadErrors)
        {
            var error = ps.Streams.Error.FirstOrDefault()?.ToString();
            throw new InvalidOperationException($"Failed to import module: {error}");
        }
    }
}
