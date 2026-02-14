using System.Globalization;
using System.Management.Automation;

namespace PSBinaryModule.Commands
{
    [Cmdlet(VerbsCommon.Get, "SystemLocale")]
    [OutputType(typeof(string))]
    public sealed class GetSystemLocaleCommand : PSCmdlet
    {
        protected override void ProcessRecord() => WriteObject(GetNormalizedSystemLocale());

        internal static string GetNormalizedSystemLocale()
        {
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
                return CultureInfo.GetCultureInfo(sanitized).Name;
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }
    }
}
