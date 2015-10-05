using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Get", "SqlCacheText", ConfirmImpact = ConfirmImpact.None)]
    public class GetSqlCacheText : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public override string Key { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Retrieving text with key " + CacheKeyPrefix + Key);
            WriteObject(cacheClient.RetrieveText(Key));
        }
    }
}