using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SqlServerCacheClient.Logging;


namespace SqlServerCacheClient
{
    public class CacheClient : ICacheClient
    {
        private static ILogger logger = LogManager.GetLogger(typeof (CacheClient));

        private const int CommandTimeout = 5;
        public const string DefaultSchemaName = "cache";
        public const int BlobMaxLength = 7980;
        public const int TextMaxLength = 3950;
        public readonly string CacheKeyPrefix;
        private readonly string connectionString;
        private readonly MetaData metaData;
        private readonly SHA256Managed hasher;
        public bool CompressBinaryIfNecessary { get; set; }
        public bool DontThrowOnValueOverflow { get; set; }
        public string SchemaName { get; }

        public CacheClient(string connectionString, string cacheKeyPrefix, string schemaName, MetaData metaData)
        {
            hasher = new SHA256Managed();
            SchemaName = schemaName;
            DontThrowOnValueOverflow = true;
            CacheKeyPrefix = cacheKeyPrefix;
            this.connectionString = connectionString;
            this.metaData = metaData;
            logger.DebugFormat("CacheClient instantiated: SchemaName={0} CacheKeyPrefix={1}  Metadata: {2} ", schemaName, cacheKeyPrefix, metaData);
        }

        public CacheClient(string connectionString, string cacheKeyPrefix, string schemaName)
            : this(connectionString, cacheKeyPrefix, schemaName, MetaDataManager.GetMetaData(connectionString, schemaName))
        {
        }

        public CacheClient(string connectionStringName, string cacheKeyPrefix) 
            : this(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString, cacheKeyPrefix, DefaultSchemaName)
        {
        }

        public async Task SetCounterAsync(string key, long count)
        {
            await SetCounterAsync(key, count, metaData.DefaultTimeToLive);
        }

        public async Task SetCounterAsync(string key, long count, TimeSpan timeToLive)
        {
            var comm = BuildSetCounterCommand(key, count, timeToLive);
            logger.DebugFormat("Cache {0}: Set Counter {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, count);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void SetCounter(string key, long count)
        {
            SetCounter(key, count, metaData.DefaultTimeToLive);
        }

        public void SetCounter(string key, long count, TimeSpan timeToLive)
        {
            var comm = BuildSetCounterCommand(key, count, timeToLive);
            logger.DebugFormat("Cache {0}: Set Counter {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, count);
            ExecuteNonQueryCommand(comm);
        }

        private SqlCommand BuildSetCounterCommand(string key, long count, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".SetCounter");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("count", count);
            comm.Parameters.AddWithValue("expiration", DateTime.UtcNow.Add(timeToLive));
            return comm;
        }

        public async Task DeleteCounterAsync(string key)
        {
            var comm = BuildDeleteCounterCommand(key);
            logger.DebugFormat("Cache {0}: Delete Counter {1}{2}", SchemaName, CacheKeyPrefix, key);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void DeleteCounter(string key)
        {
            var comm = BuildDeleteCounterCommand(key);
            logger.DebugFormat("Cache {0}: Delete Counter {1}{2}", SchemaName, CacheKeyPrefix, key);
            ExecuteNonQueryCommand(comm);
        }

        private SqlCommand BuildDeleteCounterCommand(string key)
        {
            var comm = new SqlCommand(SchemaName + ".DeleteCounter");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        public async Task<long?> RetrieveCounterAsync(string key)
        {
            var comm = BuildRetrieveCounterCommand(key);
            var result = await ExecuteScalarCommandAsync(comm);
            logger.DebugFormat("Cache {0}: Retrieve Counter {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, result);
            if (result == null || result == DBNull.Value) return null;
            return (long?) result;
        }

        public long? RetrieveCounter(string key)
        {
            var comm = BuildRetrieveCounterCommand(key);
            var result = ExecuteScalarCommand(comm);
            logger.DebugFormat("Cache {0}: Retrieve Counter {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, result);
            if (result == null || result == DBNull.Value) return null;
            return (long) result;
        }

        private SqlCommand BuildRetrieveCounterCommand(string key)
        {
            var comm = new SqlCommand(SchemaName + ".RetrieveCounter");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        public async Task<long> IncrementCounterAsync(string key)
        {
            var comm = BuildIncrementCounterCommand(key, metaData.DefaultTimeToLive);
            var result = await ExecuteScalarCommandAsync(comm);
            logger.DebugFormat("Cache {0}: Increment Counter {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, result);
            return (long) result;
        }

        public long IncrementCounter(string key)
        {
            var comm = BuildIncrementCounterCommand(key, metaData.DefaultTimeToLive);
            var result = ExecuteScalarCommand(comm);
            logger.DebugFormat("Cache {0}: Increment Counter {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, result);
            return (long) result;
        }

        private SqlCommand BuildIncrementCounterCommand(string key, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".IncrementCounter");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("newExpiration", DateTime.UtcNow.Add(timeToLive));
            return comm;
        }

        public async Task<long> DecrementCounterAsync(string key)
        {
            var comm = BuildDecrementCounterCommand(key, metaData.DefaultTimeToLive);
            var result = await ExecuteScalarCommandAsync(comm);
            logger.DebugFormat("Cache {0}: Decrement Counter {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, result);
            return (long) result;
        }

        public long DecrementCounter(string key)
        {
            var comm = BuildDecrementCounterCommand(key, metaData.DefaultTimeToLive);
            var result = ExecuteScalarCommand(comm);
            logger.DebugFormat("Cache {0}: Decrement Counter {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, result);
            return (long) result;
        }

        private SqlCommand BuildDecrementCounterCommand(string key, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".DecrementCounter");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("newExpiration", DateTime.UtcNow.Add(timeToLive));
            return comm;
        }

        public async Task SetTextAsync(string key, string value)
        {
            await SetTextAsync(key, value, metaData.DefaultTimeToLive);
        }

        public async Task SetTextAsync(string key, string value, TimeSpan timeToLive)
        {
            logger.DebugFormat("Cache {0}: Set Text {1}{2} = {3}, TTL={4}", SchemaName, CacheKeyPrefix, key, value, timeToLive);
            if (value.Length > TextMaxLength)
            {
                if (DontThrowOnValueOverflow) return;
                throw new ArgumentOutOfRangeException(nameof(value), value.Length, "The maximum text size that can be saved is " + TextMaxLength.ToString());
            }
            var comm = BuildSaveCacheTextCommand(key, value, timeToLive);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void SetText(string key, string value)
        {
            SetText(key, value, metaData.DefaultTimeToLive);
        }

        public void SetText(string key, string value, TimeSpan timeToLive)
        {
            logger.DebugFormat("Cache {0}: Set Text {1}{2} = {3}, TTL={4}", SchemaName, CacheKeyPrefix, key, value, timeToLive);
            if (value.Length > TextMaxLength)
            {
                if (DontThrowOnValueOverflow) return;
                throw new ArgumentOutOfRangeException(nameof(value), value.Length, "The maximum text size that can be saved is " + TextMaxLength.ToString());
            }
            var comm = BuildSaveCacheTextCommand(key, value, timeToLive);
            ExecuteNonQueryCommand(comm);
        }

        private SqlCommand BuildSaveCacheTextCommand(string key, string value, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".SaveCacheText");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("body", value);
            comm.Parameters.AddWithValue("expiration", DateTime.UtcNow.Add(timeToLive));
            return comm;
        }

        public async Task DeleteTextAsync(string key)
        {
            logger.DebugFormat("Cache {0}: Delete Text {1}{2}", SchemaName, CacheKeyPrefix, key);
            var comm = BuildDeleteCacheTextCommand(key);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void DeleteText(string key)
        {
            logger.DebugFormat("Cache {0}: Delete Text {1}{2}", SchemaName, CacheKeyPrefix, key);
            var comm = BuildDeleteCacheTextCommand(key);
            ExecuteNonQueryCommand(comm);
        }

        private SqlCommand BuildDeleteCacheTextCommand(string key)
        {
            var comm = new SqlCommand(SchemaName + ".DeleteCacheText");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        public async Task<string> RetrieveTextAsync(string key)
        {
            var comm = BuildRetrieveCacheTextCommand(key);
            var result = await ExecuteScalarCommandAsync(comm);
            logger.DebugFormat("Cache {0}: Retrieve Text {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, result);
            if (result == null || result == DBNull.Value) return null;
            return (string) result;
        }

        public string RetrieveText(string key)
        {
            var comm = BuildRetrieveCacheTextCommand(key);
            var result = ExecuteScalarCommand(comm);
            logger.DebugFormat("Cache {0}: Retrieve Text {1}{2} = {3}", SchemaName, CacheKeyPrefix, key, result);
            if (result == null || result == DBNull.Value) return null;
            return (string) result;
        }

        private SqlCommand BuildRetrieveCacheTextCommand(string key)
        {
            var comm = new SqlCommand(SchemaName + ".RetrieveCacheText");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        public async Task SetBinaryAsync(string key, byte[] blob)
        {
            await SetBinaryAsync(key, blob, metaData.DefaultTimeToLive);
        }

        public async Task SetBinaryAsync(string key, byte[] blob, TimeSpan timeToLive)
        {
            logger.DebugFormat("Cache {0}: Set Binary {1}{2}  TTL={3}", SchemaName, CacheKeyPrefix, key, timeToLive);
            var compressedBlob = CompressBytes(blob);
            if (compressedBlob.Length > BlobMaxLength)
            {
                if (DontThrowOnValueOverflow) return;
                throw new ArgumentOutOfRangeException(nameof(blob), compressedBlob.Length,
                    "The binary blob is too big, (even when compressed if enabled.) Maximum size binary blob that can be saved is " + BlobMaxLength.ToString());
            }
            var comm = BuildSaveCacheBinaryCommand(key, compressedBlob, timeToLive);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public async Task SetBinaryAsync(string key, object value)
        {
            await SetBinaryAsync(key, value, metaData.DefaultTimeToLive);
        }

        public async Task SetBinaryAsync(string key, object value, TimeSpan timeToLive)
        {
            using (var memStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memStream, value);
                memStream.Dispose();
                await SetBinaryAsync(key, memStream.ToArray(), timeToLive);
            }
        }

        public void SetBinary(string key, object value)
        {
            SetBinary(key, value, metaData.DefaultTimeToLive);
        }

        public void SetBinary(string key, object value, TimeSpan timeToLive)
        {
            using (var memStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memStream, value);
                memStream.Dispose();
                SetBinary(key, memStream.ToArray(), timeToLive);
            }
        }

        public void SetBinary(string key, byte[] blob)
        {
            SetBinary(key, blob, metaData.DefaultTimeToLive);
        }

        public void SetBinary(string key, byte[] blob, TimeSpan timeToLive)
        {
            logger.DebugFormat("Cache {0}: Set Binary {1}{2}  TTL={3}", SchemaName, CacheKeyPrefix, key, timeToLive);
            var compressedBlob = CompressBytes(blob);
            if (compressedBlob.Length > BlobMaxLength)
            {
                if (DontThrowOnValueOverflow) return;
                throw new ArgumentOutOfRangeException(nameof(blob), compressedBlob.Length,
                    "The binary blob is too big, (even when compressed if enabled.) Maximum size binary blob that can be saved is " + BlobMaxLength.ToString());
            }
            var comm = BuildSaveCacheBinaryCommand(key, compressedBlob, timeToLive);
            ExecuteNonQueryCommand(comm);
        }

        private SqlCommand BuildSaveCacheBinaryCommand(string key, byte[] compressedBlob, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".SaveCacheBinary");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("blob", compressedBlob);
            comm.Parameters.AddWithValue("expiration", DateTime.UtcNow.Add(timeToLive));
            return comm;
        }

        public async Task DeleteBinaryAsync(string key)
        {
            logger.DebugFormat("Cache {0}: Delete Binary {1}{2} ", SchemaName, CacheKeyPrefix, key);
            var comm = BuildDeleteCacheBinaryCommand(key);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void DeleteBinary(string key)
        {
            logger.DebugFormat("Cache {0}: Delete Binary {1}{2} ", SchemaName, CacheKeyPrefix, key);
            var comm = BuildDeleteCacheBinaryCommand(key);
            ExecuteNonQueryCommand(comm);
        }

        private SqlCommand BuildDeleteCacheBinaryCommand(string key)
        {
            var comm = new SqlCommand(SchemaName + ".DeleteCacheBinary");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        public async Task<byte[]> RetrieveBinaryAsync(string key)
        {
            logger.DebugFormat("Cache {0}: Retrieve Binary {1}{2} ", SchemaName, CacheKeyPrefix, key);
            var comm = BuildRetrieveCacheBinaryCommand(key);
            var result = await ExecuteScalarCommandAsync(comm);
            if (result == null || result == DBNull.Value) return null;
            return DecompressBytes((byte[]) result);
        }

        public async Task<T> RetrieveObjectAsync<T>(string key) where T : class
        {
            var bytes = await RetrieveBinaryAsync(key);
            if (bytes == null) return (T)null;
            using (var memStream = new MemoryStream(bytes))
            {
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = new BinaryFormatter().Deserialize(memStream);
                return (T)obj;
            }
        }

        public byte[] RetrieveBinary(string key)
        {
            logger.DebugFormat("Cache {0}: Retrieve Binary {1}{2} ", SchemaName, CacheKeyPrefix, key);
            var comm = BuildRetrieveCacheBinaryCommand(key);
            var result = ExecuteScalarCommand(comm);
            if (result == null || result == DBNull.Value) return null;
            return DecompressBytes((byte[]) result);
        }

        public T RetrieveObject<T>(string key) where T : class
        {
            var bytes = RetrieveBinary(key);
            if (bytes == null) return (T) null;
            using (var memStream = new MemoryStream(bytes))
            {
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = new BinaryFormatter().Deserialize(memStream);
                return (T) obj;
            }
        }

        private SqlCommand BuildRetrieveCacheBinaryCommand(string key)
        {
            var comm = new SqlCommand(SchemaName + ".RetrieveCacheBinary");
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        public void ClearCache()
        {
            logger.DebugFormat("Cache {0}: Clear Cache.", SchemaName);
            var comm = BuildClearCacheCommand();
            ExecuteNonQueryCommand(comm);
        }

        public async Task ClearCacheAsync()
        {
            logger.DebugFormat("Cache {0}: Clear Cache.", SchemaName);
            var comm = BuildClearCacheCommand();
            await ExecuteNonQueryCommandAsync(comm);
        }

        private SqlCommand BuildClearCacheCommand()
        {
            var comm = new SqlCommand(SchemaName + ".ClearCache");
            comm.CommandType = CommandType.StoredProcedure;
            return comm;
        }

        private void ExecuteNonQueryCommand(SqlCommand command)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    command.CommandTimeout = CommandTimeout;
                    command.Connection = conn;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task ExecuteNonQueryCommandAsync(SqlCommand command)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    command.CommandTimeout = CommandTimeout;
                    command.Connection = conn;
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private object ExecuteScalarCommand(SqlCommand command)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    command.CommandTimeout = CommandTimeout;
                    command.Connection = conn;
                    var result = command.ExecuteScalar();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        private async Task<object> ExecuteScalarCommandAsync(SqlCommand command)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    command.CommandTimeout = CommandTimeout;
                    command.Connection = conn;
                    var result = await command.ExecuteScalarAsync();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        private byte[] DecompressBytes(byte[] bytes)
        {
            if (bytes[0] == 0x1f && bytes[1] == 0x8b && bytes[2] == 8 && bytes[8] == 4 && (bytes[3] & bytes[4] & bytes[5] & bytes[6] & bytes[7]) == 0)
            {
                var outputStream = new MemoryStream();
                using (var memStream = new MemoryStream(bytes, false))
                {
                    using (outputStream)
                    {
                        using (var zip = new GZipStream(memStream, CompressionMode.Decompress))
                        {
                            zip.CopyTo(outputStream);

                        }
                    }
                }
                return outputStream.ToArray();
            }
            return bytes;
        }

        private byte[] CompressBytes(byte[] bytes)
        {
            if (bytes.Length <= BlobMaxLength || !CompressBinaryIfNecessary)
            {
                return bytes;
            }
            var outputMemStream = new MemoryStream();
            using (var memStream = new MemoryStream(bytes, false))
            {
                using (outputMemStream)
                {
                    memStream.Seek(0, SeekOrigin.Begin);
                    using (var zip = new GZipStream(outputMemStream, CompressionLevel.Fastest))
                    {
                        memStream.CopyTo(zip);
                    }
                }
            }
            return outputMemStream.ToArray();
        }

        private Guid GetUidKey(string key)
        {
            var bytes = Encoding.UTF8.GetBytes(CacheKeyPrefix + key);
            var hash = hasher.ComputeHash(bytes);
            var guid = new Guid(hash.Take(16).ToArray());
            return guid;
        }
    }
}
