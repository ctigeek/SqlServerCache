using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Remove", "SqlCacheObject", ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveSqlCacheObject : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public override string Key { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Deleting object with key " + CacheKeyPrefix + Key);
            cacheClient.DeleteBinary(Key);
        }
    }
}