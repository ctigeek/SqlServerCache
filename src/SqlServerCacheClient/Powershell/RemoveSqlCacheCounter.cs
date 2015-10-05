using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Remove", "SqlCacheCounter", ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveSqlCacheCounter : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public override string Key { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Deleting counter for key " + CacheKeyPrefix + Key);
            cacheClient.DeleteCounter(Key);
        }
    }
}