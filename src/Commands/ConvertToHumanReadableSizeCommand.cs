using System;
using System.Management.Automation;

namespace PSBinaryModule.Commands
{
    /// <summary>
    /// <para type="synopsis">Converts bytes to a human-readable size format.</para>
    /// <para type="description">The ConvertTo-HumanReadableSize cmdlet converts a number of bytes into a human-readable format (KB, MB, GB, etc.).</para>
    /// <example>
    ///   <code>ConvertTo-HumanReadableSize -Bytes 1048576</code>
    ///   <para>Converts 1048576 bytes to "1.00 MB".</para>
    /// </example>
    /// <example>
    ///   <code>1073741824 | ConvertTo-HumanReadableSize -Precision 3</code>
    ///   <para>Converts 1GB to "1.000 GB" with 3 decimal places.</para>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsData.ConvertTo, "HumanReadableSize")]
    [OutputType(typeof(string))]
    public class ConvertToHumanReadableSizeCommand : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The number of bytes to convert.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [ValidateRange(0, long.MaxValue)]
        public long Bytes { get; set; }

        /// <summary>
        /// <para type="description">Number of decimal places in the output. Default is 2.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateRange(0, 10)]
        public int Precision { get; set; } = 2;

        /// <summary>
        /// ProcessRecord method - main cmdlet logic
        /// </summary>
        protected override void ProcessRecord()
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            double size = Bytes;
            int order = 0;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            string result = $"{size.ToString($"F{Precision}")} {sizes[order]}";
            WriteObject(result);
        }
    }
}
