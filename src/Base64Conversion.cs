namespace PSBinaryModule
{
    public sealed record Base64ConversionResult(string Input, string Output, string Encoding)
    {
        internal static Base64ConversionResult FromEncoding(string input, string encodedData, string encodingName) =>
            new(input, encodedData, encodingName);
    }
}
