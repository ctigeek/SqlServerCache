using System;
using System.Management.Automation;
using System.Text;
using Microsoft.Win32;

namespace SqlServerCacheClient.Powershell
{
    public abstract class SqlCmdletBase : PSCmdlet
    {
        [Parameter]
        [Credential]
        public virtual PSCredential SqlCredential { get; set; }

        [Parameter]
        public virtual string ConnectionString { get; set; }

        [Parameter]
        public virtual string DataSource { get; set; }

        [Parameter]
        public virtual string Database { get; set; }

        [Parameter]
        public virtual string SchemaName { get; set; }

        protected override void BeginProcessing()
        {
            if (SqlCredential == null)
                SqlCredential = PSCredential.Empty;

            if (string.IsNullOrEmpty(Database))
            {
                WriteVerbose("No database specified. Using default of `Cache`.");
                Database = "Cache";
            }
            if (string.IsNullOrEmpty(SchemaName))
            {
                WriteVerbose("No schema name specified. Using default of `cache`.");
                SchemaName = "cache";
            }
            ConfigureServerProperty();
            ConfigureConnectionString();
        }

        protected void ConfigureServerProperty()
        {
            if (string.IsNullOrEmpty(DataSource))
            {
                var localInstanceName = FindLocalSqlInstance();
                if (localInstanceName == null)
                {
                    throw new ArgumentNullException("DataSource", "DataSource name not specified and no local instance of Sql Server was found.");
                }
                if (localInstanceName == string.Empty)
                {
                    DataSource = "localhost";
                }
                else
                {
                    DataSource = "localhost\\" + localInstanceName;
                }
                WriteVerbose("No Data Source specified. Using local server & instance: `" + DataSource + "`.");
            }
        }

        protected virtual void ConfigureConnectionString()
        {
            if (!string.IsNullOrEmpty(ConnectionString)) return;

            var connString = new StringBuilder();
            connString.AppendFormat("Data Source={0};", DataSource);
            if (!string.IsNullOrEmpty(Database))
            {
                connString.AppendFormat("Initial Catalog={0};", Database);
            }
            if (SqlCredential != PSCredential.Empty)
            {
                connString.AppendFormat("User ID={0};Password={1};", SqlCredential.UserName, SqlCredential.Password.ConvertToUnsecureString());
            }
            else
            {
                WriteVerbose("No database credentials supplied. Using Windows authentication.");
                connString.Append("Integrated Security=SSPI;");
            }
            ConnectionString = connString.ToString();
        }

        private string FindLocalSqlInstance()
        {
            string localInstanceName = null;
            var key = GetRegistryKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL");
            using (key)
            {
                if (key.ValueCount > 0)
                {
                    localInstanceName = key.GetValueNames()[0];
                    if (localInstanceName.ToLower() == "default") localInstanceName = string.Empty;
                }
            }
            return (localInstanceName == "MSSQLSERVER") ? String.Empty : localInstanceName;
        }

        private static RegistryKey GetRegistryKey(string keyPath)
        {
            RegistryKey localMachineRegistry
                = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    Environment.Is64BitOperatingSystem
                        ? RegistryView.Registry64
                        : RegistryView.Registry32);

            return string.IsNullOrEmpty(keyPath)
                ? localMachineRegistry
                : localMachineRegistry.OpenSubKey(keyPath);
        }
    }
}