using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Set", "SqlCacheCounter", ConfirmImpact = ConfirmImpact.Low)]
    public class SetSqlCacheCounter : SqlCacheClientCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public override string Key { get; set; }

        [Parameter]
        public long? Count { get; set; }

        [Parameter]
        public SwitchParameter Increment { get; set; }

        [Parameter]
        public SwitchParameter Decrement { get; set; }

        protected override void BeginProcessing()
        {
            if (!Count.HasValue && !(Increment || Decrement))
                throw new PSArgumentException("You have to either include a value, or include the increment or decrement flag.");
            if (Count.HasValue && (Increment || Decrement))
                throw new PSArgumentException("You can either include a value, or include the increment or decrement flag, but not both.");
            if (Increment && Decrement)
                throw new PSArgumentException("You can't specify both increment and decrement.");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose("Setting counter for key " + CacheKeyPrefix + Key);
            if (Increment)
            {
                cacheClient.IncrementCounter(Key);
            }
            else if (Decrement)
            {
                cacheClient.DecrementCounter(Key);
            }
            else
            {
                cacheClient.SetCounter(Key, Count.Value, TimeToLive);
            }
        }
    }
}