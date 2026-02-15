using System.Globalization;

namespace PSBinaryModule
{
    public sealed record SystemLocale(string Name, int Lcid, string DisplayName)
    {
        internal static SystemLocale FromCulture(CultureInfo culture)
        {
            return new SystemLocale(culture.Name, culture.LCID, culture.DisplayName);
        }
    }
}
