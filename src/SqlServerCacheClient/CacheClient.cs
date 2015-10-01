using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerCacheClient
{
    public class CacheClient
    {
        public const int BlobMaxLength = 7950;
        public const int TextMaxLength = 4950;
        public readonly string ConnectionStringName;
        public readonly string CacheKeyPrefix;
        private readonly string connectionString;
        private readonly SHA256Managed hasher;
        public bool CompressBinaryIfNecessary { get; set; }
        public string SchemaName { get; set; }

        public CacheClient(string connectionStringName, string cacheKeyPrefix)
        {
            SchemaName = "cache";
            this.CacheKeyPrefix = cacheKeyPrefix;
            this.ConnectionStringName = connectionStringName;
            connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
            hasher = new SHA256Managed();
        }

        public async Task SetCounterAsync(string key, long count, TimeSpan timeToLive)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildSetCounterCommand(conn, key, count, timeToLive);
                await ExecuteNonQueryCommandAsync(comm);
            }
        }

        public void SetCounter(string key, long count, TimeSpan timeToLive)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildSetCounterCommand(conn, key, count, timeToLive);
                ExecuteNonQueryCommand(comm);
            }
        }

        private SqlCommand BuildSetCounterCommand(SqlConnection conn, string key, long count, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".SetCounter", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("count", count);
            comm.Parameters.AddWithValue("expiration", DateTime.Now.Add(timeToLive));
            return comm;
        }

        public async Task DeleteCounterAsync(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildDeleteCounterCommand(conn, key);
                await ExecuteNonQueryCommandAsync(comm);
            }
        }

        public void DeleteCounter(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildDeleteCounterCommand(conn, key);
                ExecuteNonQueryCommand(comm);
            }
        }

        private SqlCommand BuildDeleteCounterCommand(SqlConnection conn, string key)
        {
            var comm = new SqlCommand(SchemaName + ".DeleteCounter", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        public async Task<long?> RetrieveCounterAsync(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildRetrieveCounterCommand(conn, key);
                var result = await ExecuteScalarCommandAsync(comm);
                if (result == null || result == DBNull.Value) return null;
                return (long)result;
            }
        }

        public long? RetrieveCounter(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildRetrieveCounterCommand(conn, key);
                var result = ExecuteScalarCommand(comm);
                if (result == null || result == DBNull.Value) return null;
                return (long)result;
            }
        }

        private SqlCommand BuildRetrieveCounterCommand(SqlConnection conn, string key)
        {
            var comm = new SqlCommand(SchemaName + ".RetrieveCounter", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        public async Task<long> IncrementCounterAsync(string key, TimeSpan timeToLive)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildIncrementCounterCommand(conn, key, timeToLive);
                var result = await ExecuteScalarCommandAsync(comm);
                return (long)result;
            }
        }

        public long IncrementCounter(string key, TimeSpan timeToLive)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildIncrementCounterCommand(conn, key, timeToLive);
                var result = ExecuteScalarCommand(comm);
                return (long)result;
            }
        }

        private SqlCommand BuildIncrementCounterCommand(SqlConnection conn, string key, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".IncrementCounter", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("newExpiration", DateTime.Now.Add(timeToLive));
            return comm;
        }

        public async Task<long> DeccrementCounterAsync(string key, TimeSpan timeToLive)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildDecrementCounterCommand(conn, key, timeToLive);
                var result = await ExecuteScalarCommandAsync(comm);
                return (long)result;
            }
        }

        public long DecrementCounter(string key, TimeSpan timeToLive)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildDecrementCounterCommand(conn, key, timeToLive);
                var result = ExecuteScalarCommand(comm);
                return (long)result;
            }
        }

        private SqlCommand BuildDecrementCounterCommand(SqlConnection conn, string key, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".DeccrementCounter", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("newExpiration", DateTime.Now.Add(timeToLive));
            return comm;
        }

        public async Task SetTextAsync(string key, string value, TimeSpan timeToLive)
        {
            if (value.Length > TextMaxLength) throw new ArgumentOutOfRangeException("value", value.Length, "The maximum text size that can be saved is " + TextMaxLength.ToString());
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildSaveCacheTextCommand(conn, key, value, timeToLive);
                await ExecuteNonQueryCommandAsync(comm);
            }
        }

        public void SetText(string key, string value, TimeSpan timeToLive)
        {
            if (value.Length > TextMaxLength) throw new ArgumentOutOfRangeException("value", value.Length, "The maximum text size that can be saved is " + TextMaxLength.ToString());
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildSaveCacheTextCommand(conn, key, value, timeToLive);
                ExecuteNonQueryCommand(comm);
            }
        }

        private SqlCommand BuildSaveCacheTextCommand(SqlConnection conn, string key, string value, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".SaveCacheText", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("body", value);
            comm.Parameters.AddWithValue("expiration", DateTime.Now.Add(timeToLive));
            return comm;
        }

        public async Task DeleteTextAsync(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildDeleteCacheTextCommand(conn, key);
                await ExecuteNonQueryCommandAsync(comm);
            }
        }

        public void DeleteText(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildDeleteCacheTextCommand(conn, key);
                ExecuteNonQueryCommand(comm);
            }
        }

        private SqlCommand BuildDeleteCacheTextCommand(SqlConnection conn, string key)
        {
            var comm = new SqlCommand(SchemaName + ".DeleteCacheText", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        public async Task<string> RetrieveTextAsync(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildRetrieveCacheTextCommand(conn, key);
                var result = await ExecuteScalarCommandAsync(comm);
                if (result == null || result == DBNull.Value) return null;
                return (string)result;
            }
        }

        public string RetrieveText(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildRetrieveCacheTextCommand(conn, key);
                var result = ExecuteScalarCommand(comm);
                if (result == null || result == DBNull.Value) return null;
                return (string)result;
            }
        }

        private SqlCommand BuildRetrieveCacheTextCommand(SqlConnection conn, string key)
        {
            var comm = new SqlCommand(SchemaName + ".RetrieveCacheText", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
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
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildSaveCacheBinaryCommand(conn, key, compressedBlob, timeToLive);
                await ExecuteNonQueryCommandAsync(comm);
            }
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
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildSaveCacheBinaryCommand(conn, key, compressedBlob, timeToLive);
                ExecuteNonQueryCommand(comm);
            }
        }

        public SqlCommand BuildSaveCacheBinaryCommand(SqlConnection conn, string key, byte[] compressedBlob, TimeSpan timeToLive)
        {
            var comm = new SqlCommand(SchemaName + ".SaveCacheBinary", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            comm.Parameters.AddWithValue("blob", compressedBlob);
            comm.Parameters.AddWithValue("expiration", DateTime.Now.Add(timeToLive));
            return comm;
        }

        public async Task DeleteBinaryAsync(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildDeleteCacheBinaryCommand(conn, key);
                await ExecuteNonQueryCommandAsync(comm);
            }
        }

        public void DeleteBinary(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildDeleteCacheBinaryCommand(conn, key);
                ExecuteNonQueryCommand(comm);
            }
        }

        private SqlCommand BuildDeleteCacheBinaryCommand(SqlConnection conn, string key)
        {
            var comm = new SqlCommand(SchemaName + ".DeleteCacheBinary", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
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
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var comm = BuildRetrieveCacheBinaryCommand(conn, key);
                var result = await ExecuteScalarCommandAsync(comm);
                if (result == null || result == DBNull.Value) return null;
                return DecompressBytes((byte[])result);
            }
        }

        public async Task<object> RetrieveObjectAsync(string key)
        {
            var bytes = await RetrieveBinaryAsync(key);
            if (bytes == null) return null;
            using (var memStream = new MemoryStream(bytes))
            {
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = new BinaryFormatter().Deserialize(memStream);
                return obj;
            }
        }

        public string RetrieveBinaryString(string key)
        {
            var result = RetrieveBinary(key);
            return GetStringFromBytes(result);
        }

        public byte[] RetrieveBinary(string key)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = BuildRetrieveCacheBinaryCommand(conn, key);
                var result = ExecuteScalarCommand(comm);
                if (result == null || result == DBNull.Value) return null;
                return DecompressBytes((byte[])result);
            }
        }

        public object RetrieveObject(string key)
        {
            var bytes = RetrieveBinary(key);
            if (bytes == null) return null;
            using (var memStream = new MemoryStream(bytes))
            {
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = new BinaryFormatter().Deserialize(memStream);
                return obj;
            }
        }

        private SqlCommand BuildRetrieveCacheBinaryCommand(SqlConnection conn, string key)
        {
            var comm = new SqlCommand(SchemaName + ".RetrieveCacheBinary", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandTimeout = 3;
            comm.Parameters.AddWithValue("uid", GetUidKey(key));
            return comm;
        }

        private void ExecuteNonQueryCommand(SqlCommand command)
        {
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        private async Task ExecuteNonQueryCommandAsync(SqlCommand command)
        {
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
            }
        }

        private object ExecuteScalarCommand(SqlCommand command)
        {
            try
            {
                var result = command.ExecuteScalar();
                return result;
            }
            catch
            {
                return null;
            }
        }

        private async Task<object> ExecuteScalarCommandAsync(SqlCommand command)
        {
            try
            {
                var result = await command.ExecuteScalarAsync();
                return result;
            }
            catch
            {
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
