using System.Management.Automation;
using System.Text;

namespace PSBinaryModule.Commands
{
    /// <summary>
    /// Decodes a Base64 string back to its original format.
    /// </summary>
    [Cmdlet(VerbsData.ConvertFrom, "Base64")]
    [OutputType(typeof(Base64ConversionResult))]
    public sealed class ConvertFromBase64Command : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the Base64 string to decode.
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            HelpMessage = "Base64 string to decode")]
        [ValidateNotNullOrEmpty]
        public string InputString { get; set; } = string.Empty;

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
                // Get encoding
                var enc = GetEncoding(Encoding);

                // Decode from Base64
                var bytes = Convert.FromBase64String(InputString);
                var decoded = enc.GetString(bytes);

                // Create result
                var inputPreview = InputString.Length > 100
                    ? InputString.AsSpan(0, 100).ToString() + "..."
                    : InputString;
                var result = Base64ConversionResult.FromEncoding(
                    inputPreview,
                    decoded,
                    Encoding);

                WriteObject(result);
            }
            catch (FormatException ex)
            {
                WriteError(new ErrorRecord(
                    new FormatException($"The input string is not a valid Base64 string: {ex.Message}", ex),
                    "InvalidBase64Format",
                    ErrorCategory.InvalidData,
                    InputString));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "DecodingError",
                    ErrorCategory.InvalidOperation,
                    InputString));
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
