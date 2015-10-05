using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Remove", "SqlCacheText", ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveSqlCacheText : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public override string Key { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Deleting text with key " + CacheKeyPrefix + Key);
            cacheClient.DeleteText(Key);
        }
    }
}