using System;
using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    public abstract class SqlCacheClientCmdletBase : SqlCmdletBase
    {
        [Parameter(Mandatory = true)]
        public virtual string Key { get; set; }

        [Parameter]
        public virtual string CacheKeyPrefix { get; set; }

        protected CacheClient cacheClient;

        protected override void BeginProcessing()
        {
            if (CacheKeyPrefix == null)
                CacheKeyPrefix = string.Empty;
            base.BeginProcessing();

            cacheClient = new CacheClient(ConnectionString, CacheKeyPrefix, SchemaName);
        }
    }
}