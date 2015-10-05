using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Get", "SqlCacheCounter", ConfirmImpact = ConfirmImpact.None)]
    public class GetSqlCacheCounter : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public override string Key { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Retrieving counter for key " + CacheKeyPrefix + Key);
            var result = cacheClient.RetrieveCounter(Key);
            WriteObject(result);
        }
    }
}