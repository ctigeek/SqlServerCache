using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServerCacheClient;

namespace SqlServerCacheClientTests
{
    [TestClass]
    public class CacheClientTests
    {
        private string cacheName { get; set; }
        public const string ConnectionString = "Data Source=LOCALHOST;Initial Catalog=Cache;Integrated Security=SSPI;";
        private CacheClient cacheClient;
        private TimeSpan timeToLive = TimeSpan.FromSeconds(5);
        private Random random = new Random(DateTime.Now.Millisecond);
        private string randomKey;

        public CacheClientTests()
        {
            cacheName = "cache";
        }

        [TestInitialize]
        public void Setup()
        {
            randomKey = "key_" + random.Next().ToString();
            cacheClient = new CacheClient(ConnectionString, string.Empty, cacheName);
            cacheClient.ClearCache();
        }

        [TestMethod]
        public async Task SetCounterAsyncTest()
        {
            await cacheClient.SetCounterAsync(randomKey, 8, timeToLive);
            var counter = await cacheClient.RetrieveCounterAsync(randomKey);
            Assert.AreEqual(8, counter.Value);
        }

        [TestMethod]
        public void SetCounterTest()
        {
            cacheClient.SetCounter(randomKey, 7, timeToLive);
            var counter = cacheClient.RetrieveCounter(randomKey);
            Assert.AreEqual(7, counter.Value);
        }

        [TestMethod]
        public async Task DeleteCounterAsyncTest()
        {
            await cacheClient.SetCounterAsync(randomKey, 6, timeToLive);
            await cacheClient.DeleteCounterAsync(randomKey);
            var counter = await cacheClient.RetrieveCounterAsync(randomKey);
            Assert.IsNull(counter);
        }

        [TestMethod]
        public void DeleteCounterTest()
        {
            cacheClient.SetCounter(randomKey, 5, timeToLive);
            cacheClient.DeleteCounter(randomKey);
            var counter = cacheClient.RetrieveCounter(randomKey);
            Assert.IsNull(counter);
        }

        [TestMethod]
        public async Task RetrieveCounterAsyncTest()
        {
            await cacheClient.SetCounterAsync(randomKey, 11, timeToLive);
            var counter = await cacheClient.RetrieveCounterAsync(randomKey);
            Assert.AreEqual(11, counter.Value);
        }

        [TestMethod]
        public void RetrieveCounterTest()
        {
            cacheClient.SetCounter(randomKey, 12, timeToLive);
            var counter = cacheClient.RetrieveCounter(randomKey);
            Assert.AreEqual(12, counter.Value);
        }

        [TestMethod]
        public async Task IncrementCounterAsyncTest()
        {
            await cacheClient.SetCounterAsync(randomKey, 13, timeToLive);
            await cacheClient.IncrementCounterAsync(randomKey, timeToLive);
            var counter = await cacheClient.RetrieveCounterAsync(randomKey);
            Assert.AreEqual(14, counter.Value);
        }

        [TestMethod()]
        public void IncrementCounterTest()
        {
            cacheClient.SetCounter(randomKey, 13, timeToLive);
            cacheClient.IncrementCounter(randomKey, timeToLive);
            var counter = cacheClient.RetrieveCounter(randomKey);
            Assert.AreEqual(14, counter.Value);
        }

        [TestMethod()]
        public async Task DecrementCounterAsyncTest()
        {
            await cacheClient.SetCounterAsync(randomKey, 16, timeToLive);
            await cacheClient.DecrementCounterAsync(randomKey, timeToLive);
            var counter = await cacheClient.RetrieveCounterAsync(randomKey);
            Assert.AreEqual(15, counter.Value);
        }

        [TestMethod]
        public void DecrementCounterTest()
        {
            cacheClient.SetCounter(randomKey, 19, timeToLive);
            cacheClient.DecrementCounter(randomKey, timeToLive);
            var counter = cacheClient.RetrieveCounter(randomKey);
            Assert.AreEqual(18, counter.Value);
        }

        [TestMethod]
        public async Task SetTextAsyncTest()
        {
            await cacheClient.SetTextAsync(randomKey, "some text", timeToLive);
            var result = await cacheClient.RetrieveTextAsync(randomKey);
            Assert.AreEqual("some text", result);
        }

        [TestMethod]
        public void SetTextTest()
        {
            cacheClient.SetText(randomKey, "some text", timeToLive);
            var result = cacheClient.RetrieveText(randomKey);
            Assert.AreEqual("some text", result);
        }

        [TestMethod]
        public async Task DeleteTextAsyncTest()
        {
            await cacheClient.SetTextAsync(randomKey, "some text", timeToLive);
            var result = await cacheClient.RetrieveTextAsync(randomKey);
            Assert.AreEqual("some text", result);
            await cacheClient.DeleteTextAsync(randomKey);
            result = await cacheClient.RetrieveTextAsync(randomKey);
            Assert.IsNull(result);
        }

        [TestMethod()]
        public void DeleteTextTest()
        {
            cacheClient.SetText(randomKey, "some text", timeToLive);
            var result = cacheClient.RetrieveText(randomKey);
            Assert.AreEqual("some text", result);
            cacheClient.DeleteText(randomKey);
            result = cacheClient.RetrieveText(randomKey);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RetrieveTextAsyncTest()
        {
            await cacheClient.SetTextAsync(randomKey, "some text", timeToLive);
            var result = await cacheClient.RetrieveTextAsync(randomKey);
            Assert.AreEqual("some text", result);
        }

        [TestMethod]
        public void RetrieveTextTest()
        {
            cacheClient.SetText(randomKey, "some text", timeToLive);
            var result = cacheClient.RetrieveText(randomKey);
            Assert.AreEqual("some text", result);
        }

        [TestMethod]
        public async Task SetBinaryStringAsyncTest()
        {
            await cacheClient.SetBinaryAsync(randomKey, "some text", timeToLive);
            var result = await cacheClient.RetrieveBinaryStringAsync(randomKey);
            Assert.AreEqual("some text", result);
        }

        [TestMethod]
        public async Task SetBinaryBlobAsyncTest()
        {
            var bytes = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            await cacheClient.SetBinaryAsync(randomKey, bytes, timeToLive);
            var result = await cacheClient.RetrieveBinaryAsync(randomKey);
            CollectionAssert.AreEqual(bytes, result);
        }

        [TestMethod]
        public async Task SetBinaryObjectAsyncTest()
        {
            var kst = KitchenSinkTest.BuildKitchenSinkTest();
            await cacheClient.SetBinaryAsync(randomKey, kst, timeToLive);
            var result = await cacheClient.RetrieveObjectAsync<KitchenSinkTest>(randomKey);
            Assert.AreEqual(0, result.CompareTo(kst));
        }

        [TestMethod]
        public void SetBinaryObjectTest()
        {
            var kst = KitchenSinkTest.BuildKitchenSinkTest();
            cacheClient.SetBinary(randomKey, kst, timeToLive);
            var result = cacheClient.RetrieveObject<KitchenSinkTest>(randomKey);
            Assert.AreEqual(0, result.CompareTo(kst));
        }

        [TestMethod]
        public void SetBinaryBlobTest()
        {
            var bytes = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            cacheClient.SetBinary(randomKey, bytes, timeToLive);
            var result = cacheClient.RetrieveBinary(randomKey);
            CollectionAssert.AreEqual(bytes, result);
        }

        [TestMethod]
        public void SetBinaryStringTest()
        {
            cacheClient.SetBinary(randomKey, "some text", timeToLive);
            var result = cacheClient.RetrieveBinaryString(randomKey);
            Assert.AreEqual("some text", result);
        }

        [TestMethod]
        public async Task DeleteBinaryAsyncTest()
        {
            await cacheClient.SetBinaryAsync(randomKey, "some text", timeToLive);
            var result = await cacheClient.RetrieveBinaryStringAsync(randomKey);
            Assert.AreEqual("some text", result);
            await cacheClient.DeleteBinaryAsync(randomKey);
            result = await cacheClient.RetrieveBinaryStringAsync(randomKey);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void DeleteBinaryTest()
        {
            cacheClient.SetBinary(randomKey, "some text", timeToLive);
            var result = cacheClient.RetrieveBinaryString(randomKey);
            Assert.AreEqual("some text", result);
            cacheClient.DeleteBinary(randomKey);
            result = cacheClient.RetrieveBinaryString(randomKey);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RetrieveBinaryStringAsyncTest()
        {
            await cacheClient.SetBinaryAsync(randomKey, "some text", timeToLive);
            var result = await cacheClient.RetrieveBinaryStringAsync(randomKey);
            Assert.AreEqual("some text", result);
        }

        [TestMethod]
        public async Task RetrieveBinaryAsyncTest()
        {
            var bytes = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            await cacheClient.SetBinaryAsync(randomKey, bytes, timeToLive);
            var result = await cacheClient.RetrieveBinaryAsync(randomKey);
            CollectionAssert.AreEqual(bytes, result);
        }

        [TestMethod]
        public async Task RetrieveObjectAsyncTest()
        {
            var kst = KitchenSinkTest.BuildKitchenSinkTest();
            await cacheClient.SetBinaryAsync(randomKey, kst, timeToLive);
            var result = (await cacheClient.RetrieveObjectAsync<KitchenSinkTest>(randomKey));
            Assert.AreEqual(0, result.CompareTo(kst));
        }

        [TestMethod()]
        public void RetrieveBinaryStringTest()
        {
            cacheClient.SetBinary(randomKey, "some text", timeToLive);
            var result = cacheClient.RetrieveBinaryString(randomKey);
            Assert.AreEqual("some text", result);
        }

        [TestMethod()]
        public void RetrieveBinaryTest()
        {
            var bytes = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            cacheClient.SetBinary(randomKey, bytes, timeToLive);
            var result = cacheClient.RetrieveBinary(randomKey);
            CollectionAssert.AreEqual(bytes, result);
        }

        [TestMethod]
        public void RetrieveObjectTest()
        {
            var kst = KitchenSinkTest.BuildKitchenSinkTest();
            cacheClient.SetBinary(randomKey, kst, timeToLive);
            var result = cacheClient.RetrieveObject<KitchenSinkTest>(randomKey);
            Assert.AreEqual(0, result.CompareTo(kst));
        }

        [TestMethod]
        public void TimeToLiveForObjectTest()
        {
            var kst = KitchenSinkTest.BuildKitchenSinkTest();
            cacheClient.SetBinary(randomKey, kst, TimeSpan.FromSeconds(3));
            var result = cacheClient.RetrieveObject<KitchenSinkTest>(randomKey);
            Assert.AreEqual(0, result.CompareTo(kst));
            Thread.Sleep(5000);
            result = cacheClient.RetrieveObject<KitchenSinkTest>(randomKey);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TimeToLiveForCounterTest()
        {
            cacheClient.SetCounter(randomKey, 7, TimeSpan.FromSeconds(3));
            var counter = cacheClient.RetrieveCounter(randomKey);
            Assert.AreEqual(7, counter.Value);
            Thread.Sleep(5000);
            counter = cacheClient.RetrieveCounter(randomKey);
            Assert.IsNull(counter);
        }

        [TestMethod]
        public void TimeToLiveForTextTest()
        {
            cacheClient.SetText(randomKey, "some text", TimeSpan.FromSeconds(3));
            var result = cacheClient.RetrieveText(randomKey);
            Assert.AreEqual("some text", result);
            Thread.Sleep(5000);
            result = cacheClient.RetrieveText(randomKey);
            Assert.IsNull(result);
        }
    }

    [Serializable]
    public class KitchenSinkTest : IComparable
    {
        public static KitchenSinkTest BuildKitchenSinkTest()
        {
            var kst = new KitchenSinkTest
            {
                aString = "blah blah blah",
                anInt = 1231,
                aLong = 9999999999999999,
                ATimeSpan = TimeSpan.FromDays(123),
                ADateTime = DateTime.Now,
                ByteArray = new byte[] {9, 8, 7, 6, 5, 4, 3, 2, 1}
            };

            var kst2 = new KitchenSinkTest
            {
                aString = "blah2 blah2 blah2",
                anInt = 1222,
                aLong = 9922922222299999,
                ATimeSpan = TimeSpan.FromDays(222),
                ADateTime = DateTime.Now,
                ByteArray = new byte[] { 2, 2, 2, 2, 5, 4, 3, 2, 1 }
            };

            var kst3 = new KitchenSinkTest
            {
                aString = "blah3 blah3 blah3",
                anInt = 1333,
                aLong = 9933333333339999,
                ATimeSpan = TimeSpan.FromDays(333),
                ADateTime = DateTime.Now,
                ByteArray = new byte[] { 3, 3, 3, 3, 5, 4, 3, 2, 1 }
            };
            kst.ChildObject1 = kst2;
            kst.ChildObject2 = kst3;
            return kst;
        }

        public int CompareTo(object obj)
        {
            var kst = (KitchenSinkTest) obj;
            var result = this.aString.CompareTo(kst.aString);
            if (result != 0) return result;
            result = this.anInt.CompareTo(kst.anInt);
            if (result != 0) return result;
            result = this.aLong.CompareTo(kst.aLong);
            if (result != 0) return result;
            result = this.ATimeSpan.CompareTo(kst.ATimeSpan);
            if (result != 0) return result;
            result = this.ADateTime.CompareTo(kst.ADateTime);
            if (result != 0) return result;
            if (!ArraysEqual<byte>(this.ByteArray, kst.ByteArray)) return -1;
            if (ChildObject1 != null)
            {
                result = this.ChildObject1.CompareTo(kst.ChildObject1);
                if (result != 0) return result;
            }
            if (ChildObject2 != null)
            {
                result = this.ChildObject2.CompareTo(kst.ChildObject2);
                if (result != 0) return result;
            }
            return 0;
        }

        public string aString { get; set; }
        public int anInt { get; set; }
        public long aLong { get; set; }
        public TimeSpan ATimeSpan { get; set; }
        public DateTime ADateTime { get; set; }
        public byte[] ByteArray { get; set; }
        public KitchenSinkTest ChildObject1 { get; set; }
        public KitchenSinkTest ChildObject2 { get; set; }

        static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }
    }
}