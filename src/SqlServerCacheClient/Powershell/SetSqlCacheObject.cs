using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Set", "SqlCacheObject", ConfirmImpact = ConfirmImpact.Low)]
    public class SetSqlCacheObject : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public object Value { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Setting object with key " + CacheKeyPrefix + Key);
            var psObject = Value as PSObject;
            cacheClient.SetBinary(Key, (psObject != null) ? psObject.ImmediateBaseObject : Value, TimeToLive);
        }
    }
}