using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServerCacheClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerCacheClient.Tests
{
    [TestClass]
    public class CacheClientTests
    {
        public const string ConnectionString = "Data Source=LOCALHOST;Initial Catalog=cache;Integrated Security=SSPI;";
        private CacheClient cacheClient;

        [TestInitialize]
        public void Setup()
        {
            cacheClient = new CacheClient(ConnectionString, string.Empty, CacheClient.DefaultSchemaName);
            
        }

        [TestMethod()]
        public async Task SetCounterAsyncTest()
        {

        }

        [TestMethod()]
        public void SetCounterTest()
        {

        }

        [TestMethod()]
        public void DeleteCounterAsyncTest()
        {

        }

        [TestMethod()]
        public void DeleteCounterTest()
        {

        }

        [TestMethod()]
        public void RetrieveCounterAsyncTest()
        {

        }

        [TestMethod()]
        public void RetrieveCounterTest()
        {

        }

        [TestMethod()]
        public void IncrementCounterAsyncTest()
        {

        }

        [TestMethod()]
        public void IncrementCounterTest()
        {

        }

        [TestMethod()]
        public void DecrementCounterAsyncTest()
        {

        }

        [TestMethod()]
        public void DecrementCounterTest()
        {

        }

        [TestMethod()]
        public void SetTextAsyncTest()
        {

        }

        [TestMethod()]
        public void SetTextTest()
        {

        }

        [TestMethod()]
        public void DeleteTextAsyncTest()
        {

        }

        [TestMethod()]
        public void DeleteTextTest()
        {

        }

        [TestMethod()]
        public void RetrieveTextAsyncTest()
        {

        }

        [TestMethod()]
        public void RetrieveTextTest()
        {

        }

        [TestMethod()]
        public void SetBinaryAsyncTest()
        {

        }

        [TestMethod()]
        public void SetBinaryAsyncTest1()
        {

        }

        [TestMethod()]
        public void SetBinaryAsyncTest2()
        {

        }

        [TestMethod()]
        public void SetBinaryTest()
        {

        }

        [TestMethod()]
        public void SetBinaryTest1()
        {

        }

        [TestMethod()]
        public void SetBinaryTest2()
        {

        }

        [TestMethod()]
        public void DeleteBinaryAsyncTest()
        {

        }

        [TestMethod()]
        public void DeleteBinaryTest()
        {

        }

        [TestMethod()]
        public void RetrieveBinaryStringAsyncTest()
        {

        }

        [TestMethod()]
        public void RetrieveBinaryAsyncTest()
        {

        }

        [TestMethod()]
        public void RetrieveObjectAsyncTest()
        {

        }

        [TestMethod()]
        public void RetrieveBinaryStringTest()
        {

        }

        [TestMethod()]
        public void RetrieveBinaryTest()
        {

        }

        [TestMethod()]
        public void RetrieveObjectTest()
        {

        }
    }
}