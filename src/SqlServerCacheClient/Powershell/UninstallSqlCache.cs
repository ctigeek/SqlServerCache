using System;
using System.Management.Automation;

namespace SqlServerCacheClient.Powershell
{
    [Cmdlet("Uninstall", "SqlCache", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
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
                try
                {
                    schemaClient.DropStoredProcedures(WriteVerbose);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "Stored Procedures", ErrorCategory.InvalidOperation, null));
                }
                try
                {
                    schemaClient.DropTables(WriteVerbose);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "Tables", ErrorCategory.InvalidOperation, null));
                }
                try
                {
                    schemaClient.DropSchema(WriteVerbose);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "Schema", ErrorCategory.InvalidOperation, null));
                }

                WriteVerbose("The cache using schema name " + SchemaName + " has been dropped from the database.");
            }
        }
    }
}