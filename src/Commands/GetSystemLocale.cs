using System.Globalization;
using PSBinaryModule;
using System.Management.Automation;

namespace PSBinaryModule.Commands
{
    [Cmdlet(VerbsCommon.Get, "SystemLocale")]
    [OutputType(typeof(SystemLocale))]
    public sealed class GetSystemLocaleCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var locale = GetNormalizedSystemLocale(out var usedFallback);
            if (usedFallback)
            {
                WriteWarning("No valid system culture detected. Falling back to en-US.");
            }

            var culture = CultureInfo.GetCultureInfo(locale);
            WriteObject(SystemLocale.FromCulture(culture));
        }

        internal static string GetNormalizedSystemLocale()
        {
            return GetNormalizedSystemLocale(out _);
        }

        internal static string GetNormalizedSystemLocale(out bool usedFallback)
        {
            usedFallback = false;
            var candidates = new[]
            {
                CultureInfo.CurrentCulture.Name,
                CultureInfo.CurrentUICulture.Name,
                CultureInfo.InstalledUICulture.Name,
                CultureInfo.DefaultThreadCurrentCulture?.Name,
                CultureInfo.DefaultThreadCurrentUICulture?.Name
            };

            foreach (var candidate in candidates)
            {
                var normalized = NormalizeLocale(candidate);
                if (!string.IsNullOrEmpty(normalized))
                {
                    return normalized;
                }
            }

            usedFallback = true;
            return "en-US";
        }

        internal static string? NormalizeLocale(string? locale)
        {
            if (string.IsNullOrWhiteSpace(locale))
            {
                return null;
            }

            var sanitized = locale.Trim().Replace('_', '-');

            try
            {
                var normalized = CultureInfo.GetCultureInfo(sanitized).Name;
                var isKnown = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Any(culture => string.Equals(culture.Name, normalized, StringComparison.OrdinalIgnoreCase));
                return isKnown ? normalized : null;
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }
    }
}
