using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Install", "SqlCache", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class InstallSqlCache : SqlCmdletBase
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
            if (ShouldProcess( DataSource + "." + Database, "Create schema `" + SchemaName + "`"))
            {
                schemaClient.CreateSchema(WriteVerbose);
                schemaClient.CreateTables(WriteVerbose);
                schemaClient.CreateStoredProcedures(WriteVerbose);
                WriteVerbose("The cache using schema name " + SchemaName + " has been created.");
            }
        }
    }
}