using System;
using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Install", "SqlCache", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class InstallSqlCache : SqlCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public override string SchemaName { get; set; }

        [Parameter]
        public TimeSpan? DefaultTimeToLive { get; set; }

        private SchemaClient schemaClient;

        protected override void BeginProcessing()
        {
            if (!DefaultTimeToLive.HasValue) DefaultTimeToLive = TimeSpan.FromDays(999);
            if (DefaultTimeToLive <= TimeSpan.Zero) throw new PSArgumentException("DefaultTimeToLive must be a positive value. Try using [TimeSpan]::FromMinutes(123) or similar.");
            if (string.IsNullOrEmpty(SchemaName)) throw new PSArgumentException("SchemaName is required.");
            base.BeginProcessing();
            schemaClient = new SchemaClient(ConnectionString, SchemaName);
        }

        protected override void ProcessRecord()
        {
            if (ShouldProcess( DataSource + "." + Database, "Create schema `" + SchemaName + "`"))
            {
                schemaClient.CreateSchema(WriteVerbose);
                schemaClient.CreateTables(WriteVerbose, DefaultTimeToLive.Value);
                schemaClient.CreateStoredProcedures(WriteVerbose);
                WriteVerbose("The cache using schema name " + SchemaName + " has been created. You should create a scheduled task to run DeleteExpiredCache every few minutes.");
            }
        }
    }
}