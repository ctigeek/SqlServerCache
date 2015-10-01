using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlServerCacheClient
{
    public interface ICacheClient
    {
        bool CompressBinaryIfNecessary { get; set; }
        bool DontThrowOnValueOverflow { get; set; }
        string SchemaName { get; set; }
        Task SetCounterAsync(string key, long count, TimeSpan timeToLive);
        void SetCounter(string key, long count, TimeSpan timeToLive);
        Task DeleteCounterAsync(string key);
        void DeleteCounter(string key);
        Task<long?> RetrieveCounterAsync(string key);
        long? RetrieveCounter(string key);
        Task<long> IncrementCounterAsync(string key, TimeSpan timeToLive);
        long IncrementCounter(string key, TimeSpan timeToLive);
        Task<long> DecrementCounterAsync(string key, TimeSpan timeToLive);
        long DecrementCounter(string key, TimeSpan timeToLive);
        Task SetTextAsync(string key, string value, TimeSpan timeToLive);
        void SetText(string key, string value, TimeSpan timeToLive);
        Task DeleteTextAsync(string key);
        void DeleteText(string key);
        Task<string> RetrieveTextAsync(string key);
        string RetrieveText(string key);
        Task SetBinaryAsync(string key, string value, TimeSpan timeToLive);
        Task SetBinaryAsync(string key, byte[] blob, TimeSpan timeToLive);
        Task SetBinaryAsync(string key, object value, TimeSpan timeToLive);
        void SetBinary(string key, string value, TimeSpan timeToLive);
        void SetBinary(string key, object value, TimeSpan timeToLive);
        void SetBinary(string key, byte[] blob, TimeSpan timeToLive);
        Task DeleteBinaryAsync(string key);
        void DeleteBinary(string key);
        Task<string> RetrieveBinaryStringAsync(string key);
        Task<byte[]> RetrieveBinaryAsync(string key);
        Task<object> RetrieveObjectAsync(string key);
        string RetrieveBinaryString(string key);
        byte[] RetrieveBinary(string key);
        object RetrieveObject(string key);
    }
}