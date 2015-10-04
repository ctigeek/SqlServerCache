using System;
using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    public abstract class SqlCacheClientCmdletBase : SqlCmdletBase
    {
        protected SqlCacheClientCmdletBase()
        {
            TimeToLive = TimeSpan.FromDays(999);
        }

        [Parameter(Mandatory = true)]
        public virtual string Key { get; set; }

        [Parameter]
        public virtual string CacheKeyPrefix { get; set; }

        [Parameter]
        public virtual TimeSpan TimeToLive { get; set; }

        protected CacheClient cacheClient;

        protected override void BeginProcessing()
        {
            if (CacheKeyPrefix == null)
                CacheKeyPrefix = string.Empty;
            base.BeginProcessing();

            cacheClient = new CacheClient(ConnectionString, CacheKeyPrefix, SchemaName);
            cacheClient.DefaultTimeToLive = TimeToLive;
        }
    }
}