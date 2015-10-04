using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Add", "SqlCacheObject", ConfirmImpact = ConfirmImpact.Medium)]
    public class AddSqlCacheObject : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public object Value { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Adding object with key " + CacheKeyPrefix + Key);
            var psObject = Value as PSObject;
            cacheClient.SetBinary(Key, (psObject != null) ? psObject.ImmediateBaseObject : Value, TimeToLive);
        }
    }
}