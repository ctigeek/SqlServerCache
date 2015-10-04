using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Get", "SqlCacheObject", ConfirmImpact = ConfirmImpact.Low)]
    public class GetSqlCacheObject : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public override string Key { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Retrieving object for key " + CacheKeyPrefix + Key);
            var result = cacheClient.RetrieveObject<object>(Key);
            WriteObject(result);
        }
    }
}
