using System.Management.Automation;
using System.Text;

namespace PSBinaryModule.Commands
{
    /// <summary>
    /// Encodes a string or file contents to Base64.
    /// </summary>
    [Cmdlet(VerbsData.ConvertTo, "Base64")]
    [OutputType(typeof(Base64ConversionResult))]
    public sealed class ConvertToBase64Command : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the string to encode.
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ParameterSetName = "InputString",
            HelpMessage = "String to encode to Base64")]
        [ValidateNotNullOrEmpty]
        public string InputString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file path to encode.
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0,
            ParameterSetName = "Path",
            HelpMessage = "Path to file to encode to Base64")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the encoding to use.
        /// </summary>
        [Parameter(
            Mandatory = false,
            HelpMessage = "Text encoding to use (UTF8, ASCII, Unicode). Default is UTF8")]
        [ValidateSet("UTF8", "ASCII", "Unicode")]
        public string Encoding { get; set; } = "UTF8";

        protected override void ProcessRecord()
        {
            try
            {
                // Get the data to encode based on parameter set
                string dataToEncode;
                if (ParameterSetName == "InputString")
                {
                    dataToEncode = InputString;
                }
                else
                {
                    // Resolve the file path
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

                    var resolvedPath = resolvedPaths[0].Path;

                    if (!File.Exists(resolvedPath))
                    {
                        WriteError(new ErrorRecord(
                            new FileNotFoundException($"Cannot find file '{resolvedPath}'."),
                            "FileNotFound",
                            ErrorCategory.ObjectNotFound,
                            resolvedPath));
                        return;
                    }

                    dataToEncode = File.ReadAllText(resolvedPath);
                }

                // Get encoding
                var enc = GetEncoding(Encoding);

                // Encode to Base64
                var bytes = enc.GetBytes(dataToEncode);
                var encoded = Convert.ToBase64String(bytes);

                // Create result
                var inputPreview = dataToEncode.Length > 100
                    ? dataToEncode.AsSpan(0, 100).ToString() + "..."
                    : dataToEncode;
                var result = Base64ConversionResult.FromEncoding(
                    inputPreview,
                    encoded,
                    Encoding);

                WriteObject(result);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "FileAccessError",
                    ErrorCategory.ReadError,
                    ParameterSetName == "Path" ? Path : null));
            }
        }

        private static Encoding GetEncoding(string encodingName)
        {
            return encodingName switch
            {
                "ASCII" => System.Text.Encoding.ASCII,
                "Unicode" => System.Text.Encoding.Unicode,
                _ => System.Text.Encoding.UTF8,
            };
        }
    }
}
