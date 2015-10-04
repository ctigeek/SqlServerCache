using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Uninstall", "SqlCache", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class UninstallSqlCache : SqlCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public override string SchemaName { get; set; }

        private SchemaClient schemaClient;

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(SchemaName)) throw new PSArgumentException("SchemaName is required.");
            base.BeginProcessing();
            schemaClient = new SchemaClient(ConnectionString, SchemaName);
        }

        protected override void ProcessRecord()
        {
            if (ShouldProcess(DataSource + "." + Database, "Drop schema `" + SchemaName + "`"))
            {
                schemaClient.DropStoredProcedures(WriteVerbose);
                schemaClient.DropTables(WriteVerbose);
                schemaClient.DropSchema(WriteVerbose);
                WriteVerbose("The cache using schema name " + SchemaName + " has been dropped from the database.");
            }
        }
    }
}