using System;
using System.Management.Automation;

namespace PSBinaryModule.Commands
{
    /// <summary>
    /// <para type="synopsis">Gets metadata information about the PSBinaryModule.</para>
    /// <para type="description">The Get-BinaryModuleMetadata cmdlet retrieves metadata information about the PSBinaryModule, including version, author, and other details.</para>
    /// <example>
    ///   <code>Get-BinaryModuleMetadata</code>
    ///   <para>Gets the metadata for PSBinaryModule.</para>
    /// </example>
    /// <example>
    ///   <code>Get-BinaryModuleMetadata -Detailed</code>
    ///   <para>Gets detailed metadata including all properties.</para>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "BinaryModuleMetadata")]
    [OutputType(typeof(ModuleMetadata))]
    public class GetBinaryModuleMetadataCommand : PSCmdlet
    {
        /// <summary>
        /// <para type="description">If specified, returns detailed metadata information.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Detailed { get; set; }

        /// <summary>
        /// ProcessRecord method - main cmdlet logic
        /// </summary>
        protected override void ProcessRecord()
        {
            var metadata = new ModuleMetadata
            {
                Name = "PSBinaryModule",
                Version = "1.0.0",
                Author = "Your Name",
                Description = "A PowerShell binary module template with CI/CD support",
                CompanyName = "Your Company",
                Copyright = $"(c) {DateTime.Now.Year} Your Name. All rights reserved."
            };

            if (Detailed.IsPresent)
            {
                metadata.PowerShellVersion = "5.1";
                metadata.DotNetVersion = "8.0";
                metadata.CLRVersion = Environment.Version.ToString();
            }

            WriteObject(metadata);
        }
    }

    /// <summary>
    /// Represents module metadata
    /// </summary>
    public class ModuleMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Copyright { get; set; } = string.Empty;
        public string? PowerShellVersion { get; set; }
        public string? DotNetVersion { get; set; }
        public string? CLRVersion { get; set; }
    }
}
