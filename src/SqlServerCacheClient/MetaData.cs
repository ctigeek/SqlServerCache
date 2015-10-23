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

        public MetaData(bool isDebugSchema, bool cacheIsEnabled, TimeSpan defaultTimeToLive, 
            long maxRowCountCounterCache, long maxRowCountTextCache, long maxRowCountBinaryCache)
        {
            SchemaName = CacheClient.DefaultSchemaName;
            ConnectionString = string.Empty;
            IsDebugSchema = isDebugSchema;
            CacheIsEnabled = cacheIsEnabled;
            DefaultTimeToLive = defaultTimeToLive;
            MaxRowCountCounterCache = maxRowCountCounterCache;
            MaxRowCountBinaryCache = maxRowCountBinaryCache;
            MaxRowCountTextCache = maxRowCountTextCache;
            MaxPayloadSizeForBinaryCache = SchemaClient.BlobMaxLength;
            MaxPayloadSizeForTextCache = SchemaClient.TextMaxLength;
        }

        public bool IsDebugSchema { get; internal set; }

        public bool CacheIsEnabled { get; internal set; }

        public TimeSpan DefaultTimeToLive { get; internal set; }

        public long MaxRowCountCounterCache { get; internal set; }

        public long MaxRowCountBinaryCache { get; internal set; }

        public long MaxRowCountTextCache { get; internal set; }

        public long MaxPayloadSizeForTextCache { get; internal set; }

        public long MaxPayloadSizeForBinaryCache { get; internal set; }

        public DateTime LastRunDeleteExpiredCache { get; internal set; }

        public override string ToString()
        {
            return $"IsDebugSchema={IsDebugSchema}, CacheIsEnabled={CacheIsEnabled}, DefaultTimeToLive={DefaultTimeToLive}, MaxRowCountCounterCache={MaxRowCountCounterCache}, MaxRowCountBinaryCache={MaxRowCountBinaryCache}, MaxRowCountTextCache={MaxRowCountTextCache}, MaxPayloadSizeForBinaryCache={MaxPayloadSizeForBinaryCache}, MaxPayloadSizeForTextCache={MaxPayloadSizeForTextCache}";
        }
    }
}
