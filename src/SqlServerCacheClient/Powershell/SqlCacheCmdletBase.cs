using System;
using System.Management.Automation;
using System.Text;
using Microsoft.Win32;

namespace SqlServerCacheClient.Powershell
{
    public class SqlCacheCmdletBase : PSCmdlet
    {
        public SqlCacheCmdletBase()
        {
            TimeToLive = TimeSpan.FromDays(999);
        }

        [Parameter(Mandatory = true)]
        public virtual string Key { get; set; }

        [Parameter]
        [Credential]
        public PSCredential SqlCredential { get; set; }

        [Parameter]
        public string ConnectionString { get; set; }

        [Parameter]
        public string DataSource { get; set; }

        [Parameter]
        public string Database { get; set; }

        [Parameter]
        public string SchemaName { get; set; }

        [Parameter]
        public string CacheKeyPrefix { get; set; }

        [Parameter]
        public TimeSpan TimeToLive { get; set; }

        protected CacheClient cacheClient;

        protected override void BeginProcessing()
        {
            if (SqlCredential == null)
                SqlCredential = PSCredential.Empty;
            if (CacheKeyPrefix == null)
                CacheKeyPrefix = string.Empty;
            if (string.IsNullOrEmpty(Database))
                Database = "Cache";
            if (string.IsNullOrEmpty(SchemaName))
                SchemaName = "cache";
            ConfigureServerProperty();
            ConfigureConnectionString();
            cacheClient = new CacheClient(ConnectionString, CacheKeyPrefix, SchemaName);
            cacheClient.DefaultTimeToLive = TimeToLive;
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
                WriteVerbose("Server set to " + DataSource);
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