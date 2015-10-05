using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Set", "SqlCacheText", ConfirmImpact = ConfirmImpact.Low)]
    public class SetSqlCacheText : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public string Text { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Saving text with key " + CacheKeyPrefix + Key);
            cacheClient.SetText(Key, Text);
        }
    }
}