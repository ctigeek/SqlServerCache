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

namespace SqlServerCacheClient
{
    public class CacheClient : ICacheClient
    {
        private const int CommandTimeout = 5;
        public const string DefaultSchemaName = "cache";
        public const int BlobMaxLength = 7980;
        public const int TextMaxLength = 3950;
        public readonly string CacheKeyPrefix;
        private readonly string connectionString;
        private readonly SHA256Managed hasher;
        public bool CompressBinaryIfNecessary { get; set; }
        public bool DontThrowOnValueOverflow { get; set; }
        public string SchemaName { get; set; }
        public TimeSpan DefaultTimeToLive { get; set; }

        public CacheClient(string connectionString, string cacheKeyPrefix, string schemaName)
        {
            hasher = new SHA256Managed();
            SchemaName = schemaName;
            DontThrowOnValueOverflow = true;
            CacheKeyPrefix = cacheKeyPrefix;
            this.connectionString = connectionString;
        }

        public CacheClient(string connectionStringName, string cacheKeyPrefix) : this(string.Empty, cacheKeyPrefix, DefaultSchemaName)
        {
            if (ConfigurationManager.ConnectionStrings[connectionStringName] == null)
                throw new ArgumentNullException("There is no connection string with the name of " + connectionStringName);
            connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
        }

        public async Task SetCounterAsync(string key, long count)
        {
            await SetCounterAsync(key, count, DefaultTimeToLive);
        }

        public async Task SetCounterAsync(string key, long count, TimeSpan timeToLive)
        {
            var comm = BuildSetCounterCommand(key, count, timeToLive);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void SetCounter(string key, long count, TimeSpan timeToLive)
        {
            var comm = BuildSetCounterCommand(key, count, timeToLive);
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
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void DeleteCounter(string key)
        {
            var comm = BuildDeleteCounterCommand(key);
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
            if (result == null || result == DBNull.Value) return null;
            return (long?) result;
        }

        public long? RetrieveCounter(string key)
        {
            var comm = BuildRetrieveCounterCommand(key);
            var result = ExecuteScalarCommand(comm);
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

        public async Task<long> IncrementCounterAsync(string key, TimeSpan timeToLive)
        {
            var comm = BuildIncrementCounterCommand(key, timeToLive);
            var result = await ExecuteScalarCommandAsync(comm);
            return (long) result;
        }

        public long IncrementCounter(string key, TimeSpan timeToLive)
        {
            var comm = BuildIncrementCounterCommand(key, timeToLive);
            var result = ExecuteScalarCommand(comm);
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

        public async Task<long> DecrementCounterAsync(string key, TimeSpan timeToLive)
        {
            var comm = BuildDecrementCounterCommand(key, timeToLive);
            var result = await ExecuteScalarCommandAsync(comm);
            return (long) result;
        }

        public long DecrementCounter(string key, TimeSpan timeToLive)
        {
            var comm = BuildDecrementCounterCommand(key, timeToLive);
            var result = ExecuteScalarCommand(comm);
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

        public async Task SetTextAsync(string key, string value, TimeSpan timeToLive)
        {
            if (value.Length > TextMaxLength) throw new ArgumentOutOfRangeException("value", value.Length, "The maximum text size that can be saved is " + TextMaxLength.ToString());
            var comm = BuildSaveCacheTextCommand(key, value, timeToLive);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void SetText(string key, string value, TimeSpan timeToLive)
        {
            if (value.Length > TextMaxLength) throw new ArgumentOutOfRangeException("value", value.Length, "The maximum text size that can be saved is " + TextMaxLength.ToString());
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
            var comm = BuildDeleteCacheTextCommand(key);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void DeleteText(string key)
        {
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
            if (result == null || result == DBNull.Value) return null;
            return (string) result;
        }

        public string RetrieveText(string key)
        {
            var comm = BuildRetrieveCacheTextCommand(key);
            var result = ExecuteScalarCommand(comm);
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

        public async Task SetBinaryAsync(string key, string value, TimeSpan timeToLive)
        {
            var body = GetBytesFromString(value);
            await SetBinaryAsync(key, body, timeToLive);
        }

        public async Task SetBinaryAsync(string key, byte[] blob, TimeSpan timeToLive)
        {
            var compressedBlob = CompressBytes(blob);
            if (compressedBlob.Length > BlobMaxLength)
                throw new ArgumentOutOfRangeException("blob", compressedBlob.Length,
                    "The binary blob is too big, (even when compressed if enabled.) Maximum size binary blob that can be saved is " + BlobMaxLength.ToString());
            var comm = BuildSaveCacheBinaryCommand(key, compressedBlob, timeToLive);
            await ExecuteNonQueryCommandAsync(comm);
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

        public void SetBinary(string key, string value, TimeSpan timeToLive)
        {
            var body = GetBytesFromString(value);
            SetBinary(key, body, timeToLive);
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

        public void SetBinary(string key, byte[] blob, TimeSpan timeToLive)
        {
            var compressedBlob = CompressBytes(blob);
            if (compressedBlob.Length > BlobMaxLength)
                throw new ArgumentOutOfRangeException("blob", compressedBlob.Length,
                    "The binary blob is too big, (even when compressed if enabled.) Maximum size binary blob that can be saved is " + BlobMaxLength.ToString());
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
            var comm = BuildDeleteCacheBinaryCommand(key);
            await ExecuteNonQueryCommandAsync(comm);
        }

        public void DeleteBinary(string key)
        {
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

        public async Task<string> RetrieveBinaryStringAsync(string key)
        {
            var result = await RetrieveBinaryAsync(key);
            return GetStringFromBytes(result);
        }

        public async Task<byte[]> RetrieveBinaryAsync(string key)
        {
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

        public string RetrieveBinaryString(string key)
        {
            var result = RetrieveBinary(key);
            return GetStringFromBytes(result);
        }

        public byte[] RetrieveBinary(string key)
        {
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
            var comm = BuildClearCacheCommand();
            ExecuteNonQueryCommand(comm);
        }

        public async Task ClearCacheAsync()
        {
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

        private byte[] GetBytesFromString(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        private string GetStringFromBytes(byte[] bytes)
        {
            if (bytes == null) return null;
            return Encoding.UTF8.GetString(bytes);
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
