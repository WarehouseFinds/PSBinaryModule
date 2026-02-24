using System.Management.Automation;
using System.Security.Cryptography;

namespace PSBinaryModule.Commands
{
    /// <summary>
    /// Calculates the hash checksum of a file.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "FileChecksum")]
    [OutputType(typeof(FileChecksumResult))]
    public sealed class GetFileChecksumCommand : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the file to calculate the checksum for.
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Path to the file to calculate checksum for")]
        [ValidateNotNullOrEmpty]
        [Alias("FilePath", "FullName")]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hash algorithm to use.
        /// </summary>
        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = "Hash algorithm to use (MD5, SHA1, SHA256, SHA512)")]
        [ValidateSet("MD5", "SHA1", "SHA256", "SHA512")]
        public string Algorithm { get; set; } = "SHA256";

        protected override void ProcessRecord()
        {
            // Resolve the path to handle relative paths and wildcards
            string resolvedPath;
            try
            {
                var resolvedPaths = SessionState.Path.GetResolvedPSPathFromPSPath(Path);

                if (resolvedPaths.Count == 0)
                {
                    WriteError(new ErrorRecord(
                        new FileNotFoundException($"Cannot find path '{Path}' because it does not exist."),
                        "PathNotFound",
                        ErrorCategory.ObjectNotFound,
                        Path));
                    return;
                }

                if (resolvedPaths.Count > 1)
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException($"Path '{Path}' resolves to multiple items. Please specify a single file."),
                        "MultiplePathsResolved",
                        ErrorCategory.InvalidArgument,
                        Path));
                    return;
                }

                resolvedPath = resolvedPaths[0].Path;
            }
            catch (Exception ex) when (ex is ItemNotFoundException or ArgumentException)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "PathResolutionError",
                    ErrorCategory.ObjectNotFound,
                    Path));
                return;
            }

            // Verify the path points to a file
            if (!File.Exists(resolvedPath))
            {
                WriteError(new ErrorRecord(
                    new FileNotFoundException($"Cannot find file '{resolvedPath}'."),
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    resolvedPath));
                return;
            }

            // Check if the path is a directory
            if (Directory.Exists(resolvedPath))
            {
                WriteError(new ErrorRecord(
                    new ArgumentException($"Path '{resolvedPath}' is a directory. Please specify a file."),
                    "PathIsDirectory",
                    ErrorCategory.InvalidArgument,
                    resolvedPath));
                return;
            }

            // Calculate the hash
            try
            {
                using var stream = File.OpenRead(resolvedPath);
                var hashBytes = ComputeHash(stream, Algorithm);
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                var result = new FileChecksumResult
                {
                    Path = resolvedPath,
                    Algorithm = Algorithm,
                    Hash = hashString,
                    FileSize = new FileInfo(resolvedPath).Length
                };

                WriteObject(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "UnauthorizedAccess",
                    ErrorCategory.PermissionDenied,
                    resolvedPath));
            }
            catch (IOException ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "IOError",
                    ErrorCategory.ReadError,
                    resolvedPath));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "UnexpectedError",
                    ErrorCategory.NotSpecified,
                    resolvedPath));
            }
        }

        internal static byte[] ComputeHash(Stream stream, string algorithm)
        {
            using var hashAlgorithm = CreateHashAlgorithm(algorithm);
            return hashAlgorithm.ComputeHash(stream);
        }

        internal static HashAlgorithm CreateHashAlgorithm(string algorithm)
        {
#pragma warning disable CA5351 // MD5 is supported as user option for compatibility
#pragma warning disable CA5350 // SHA1 is supported as user option for compatibility
            return algorithm.ToUpperInvariant() switch
            {
                "MD5" => MD5.Create(),
                "SHA1" => SHA1.Create(),
                "SHA256" => SHA256.Create(),
                "SHA512" => SHA512.Create(),
                _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}", nameof(algorithm))
            };
#pragma warning restore CA5350
#pragma warning restore CA5351
        }
    }

    /// <summary>
    /// Represents the result of a file checksum calculation.
    /// </summary>
    public sealed class FileChecksumResult
    {
        /// <summary>
        /// Gets or sets the path to the file.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hash algorithm used.
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the calculated hash value.
        /// </summary>
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public long FileSize { get; set; }
    }
}
