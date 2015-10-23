using System;

namespace SqlServerCacheClient
{
    public class MetaData
    {
        public readonly string SchemaName;
        public readonly string ConnectionString;

        public MetaData(string connectionString, string schemaName)
        {
            SchemaName = schemaName;
            ConnectionString = connectionString;
        }

        public MetaData(bool isDebugSchema, bool cacheIsEnabled, TimeSpan defaultTimeToLive, long maxRowCountForAllTables)
        {
            SchemaName = CacheClient.DefaultSchemaName;
            ConnectionString = string.Empty;
            IsDebugSchema = isDebugSchema;
            CacheIsEnabled = cacheIsEnabled;
            DefaultTimeToLive = defaultTimeToLive;
            MaxRowCountForAllTables = maxRowCountForAllTables;
            MaxSizeForBinaryCache = SchemaClient.BlobMaxLength;
            MaxSizeForTextCache = SchemaClient.TextMaxLength;
        }

        public bool IsDebugSchema { get; internal set; }

        public bool CacheIsEnabled { get; internal set; }

        public TimeSpan DefaultTimeToLive { get; internal set; }

        public long MaxRowCountForAllTables { get; internal set; }

        public long MaxSizeForTextCache { get; internal set; }

        public long MaxSizeForBinaryCache { get; internal set; }

        public DateTime LastRunDeleteExpiredCache { get; internal set; }

        public override string ToString()
        {
            return $"IsDebugSchema={IsDebugSchema}, CacheIsEnabled={CacheIsEnabled}, DefaultTimeToLive={DefaultTimeToLive}, MaxRowCountForAllTables={MaxRowCountForAllTables}, MaxSizeForBinaryCache={MaxSizeForBinaryCache}, MaxSizeForTextCache={MaxSizeForTextCache}";
        }
    }
}
