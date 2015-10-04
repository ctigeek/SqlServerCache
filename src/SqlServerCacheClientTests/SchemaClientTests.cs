using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServerCacheClient;
using System.Data.SqlClient;

namespace SqlServerCacheClientTests
{
    [TestClass]
    public class SchemaClientTests
    {
        private SchemaClient schemaClient;
        private Random random = new Random(DateTime.Now.Millisecond);
        public const string ConnectionString = "Data Source=LOCALHOST;Initial Catalog=Cache;Integrated Security=SSPI;";
        private string schemaName;
        private CacheClient cacheClient;

        [TestInitialize]
        public void Setup()
        {
            schemaName = "cache" + random.Next().ToString();
            schemaClient = new SchemaClient(ConnectionString, schemaName);
            cacheClient = new CacheClient(ConnectionString, string.Empty, schemaName);
        }

        [TestCleanup]
        public void Teardown()
        {
            try
            {
                schemaClient.DropStoredProcedures(null);
            }
            catch
            {
            }
            try
            {
                schemaClient.DropTables(null);
            }
            catch
            {
            }
            try
            {
                schemaClient.DropSchema(null);
            }
            catch
            {
            }
        }

        [TestMethod]
        public void CreateSchemaTest()
        {
            schemaClient.CreateSchema(null);
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                var comm = new SqlCommand("select name from sys.schemas where name = @schemaName;", conn);
                comm.Parameters.AddWithValue("schemaName", schemaName);
                var result = comm.ExecuteScalar();
                if (result == null || result == DBNull.Value) Assert.Fail("Schema was not created.");
                Assert.AreEqual(schemaName, result.ToString());
            }
        }

        [TestMethod]
        public void CreateTablesTest()
        {
            schemaClient.CreateSchema(null);
            schemaClient.CreateTables(null);
            string[] tables = new[] {"BinaryCache", "CounterCache", "Meta", "TextCache"};
            int index = 0;
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                var comm = new SqlCommand("select tables.name from sys.tables inner join sys.schemas on (tables.schema_id = schemas.schema_id) where schemas.name = @schemaName order by name;", conn);
                comm.Parameters.AddWithValue("schemaName", schemaName);
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(tables[index], reader["name"].ToString());
                        index++;
                    }
                }
            }
        }

        [TestMethod]
        public void CreateStoredProceduresTest()
        {
            schemaClient.CreateSchema(null);
            schemaClient.CreateTables(null);
            schemaClient.CreateStoredProcedures(null);
            var storedProcName = new[]
            {"ClearCache","DecrementCounter","DeleteCacheBinary","DeleteCacheText",
                "DeleteCounter","DeleteExpiredCache","IncrementCounter",
                "RetrieveCacheBinary", "RetrieveCacheText","RetrieveCounter",
                "SaveCacheBinary", "SaveCacheText", "SetCounter"
            };
            int index = 0;
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                var comm = new SqlCommand("select procedures.name from sys.procedures inner join sys.schemas on (procedures.schema_id = schemas.schema_id) where schemas.name = @schemaName order by procedures.name;", conn);
                comm.Parameters.AddWithValue("schemaName", schemaName);
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(storedProcName[index], reader["name"].ToString());
                        index++;
                    }
                }
            }
        }
    }
}
