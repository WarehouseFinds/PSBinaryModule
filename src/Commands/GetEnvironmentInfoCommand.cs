using System.Management.Automation;

namespace PSBinaryModule.Commands
{
    /// <summary>
    /// Gets environment metadata information.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "EnvironmentInfo")]
    [OutputType(typeof(EnvironmentInfo))]
    public sealed class GetEnvironmentInfoCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var environmentInfo = EnvironmentInfo.GetCurrent();
            WriteObject(environmentInfo);
        }
    }
}
