using System.Runtime.InteropServices;

namespace PSBinaryModule
{
    public sealed record EnvironmentInfo(
        string MachineName,
        string UserName,
        string PowerShellVersion,
        string DotNetVersion,
        string OSPlatform,
        bool IsAdmin)
    {
        internal static EnvironmentInfo GetCurrent()
        {
            // Get PowerShell version from the actual PSVersionTable
            var psVersionFull = $"{Environment.Version}";
            var dotnetVersion = RuntimeInformation.FrameworkDescription;

            // Get OS platform
            var osPlatform = RuntimeInformation.OSDescription;

            // Check if running as admin
            var isAdmin = false;
            if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                try
                {
                    isAdmin = new System.Security.Principal.WindowsPrincipal(
                        System.Security.Principal.WindowsIdentity.GetCurrent())
                        .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
                catch
                {
                    // Permission denied or other error, assume not admin
                    isAdmin = false;
                }
            }

            return new EnvironmentInfo(
                MachineName: Environment.MachineName,
                UserName: Environment.UserName,
                PowerShellVersion: psVersionFull,
                DotNetVersion: dotnetVersion,
                OSPlatform: osPlatform,
                IsAdmin: isAdmin);
        }
    }
}
