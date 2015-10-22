using System;
using System.Data.SqlClient;
using SqlServerCacheClient.Logging;

namespace SqlServerCacheClient
{
    public class SchemaClient
    {
        #region Format Strings

        public const string CreateSchemaNameFormatString = "create schema [{0}];";
        public const string DropSchemaNameFormatString = "drop schema [{0}];";

        public const string DropTextCacheTableFormatString = @"drop table [{0}].[TextCache];";
        public const string CreateTextCacheTableFormatString = @"CREATE TABLE [{0}].[TextCache] (
	[Pk] [uniqueidentifier] NOT NULL,
	[Expires] [datetime2](7) NOT NULL,
	[TextBody] [nvarchar]({1}) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
CONSTRAINT [imPK_TextCache_Pk] PRIMARY KEY NONCLUSTERED HASH (	[Pk])WITH ( BUCKET_COUNT = 16384),
INDEX [IX_TextCache_Expires] NONCLUSTERED ( [Expires] ASC )
)WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_ONLY ); ";

        public const string DropBinaryCacheTableFormatString = @"drop table [{0}].[BinaryCache];";
        public const string CreateBinaryCacheTableFormatString = @"CREATE TABLE [{0}].[BinaryCache] (
	[Pk] [uniqueidentifier] NOT NULL,
	[Expires] [datetime2](7) NOT NULL,
	[BinaryBlob] [varbinary]({1}) NOT NULL,
CONSTRAINT [imPK_BinaryCache_Pk] PRIMARY KEY NONCLUSTERED HASH ([Pk]) WITH ( BUCKET_COUNT = 16384),
INDEX [IX_BinaryCache_Expires] NONCLUSTERED ( [Expires] ASC ) )
WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_ONLY ); ";

        public const string DropCounterCacheTableFormatString = @"drop table [{0}].[CounterCache];";
        public const string CreateCounterCacheTableFormatString = @"CREATE TABLE [{0}].[CounterCache] (
	[Pk] [uniqueidentifier] NOT NULL,
	[Expires] [datetime2](7) NOT NULL,
	[Count] [bigint] NOT NULL,
CONSTRAINT [imPK_CounterCache_Pk] PRIMARY KEY NONCLUSTERED HASH ([Pk]) WITH ( BUCKET_COUNT = 1000),
INDEX [IX_CounterCache_Expires] NONCLUSTERED ( [Expires] ASC)
)WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_ONLY ); ";

        public const string DropMetaTableFormatString = @"drop table [{0}].[Meta];";
        public const string CreateMetaTableFormatString = @"CREATE TABLE [{0}].[Meta] (
	[Pk] int NOT NULL,
	[IsDebugSchema] bit NOT NULL,
	[CacheIsEnabled] bit NOT NULL,
	[DefaultTTLinSeconds] bigint NOT NULL,
	[MaxRowCountForAllTables] bigint NOT NULL,
	[MaxSizeForTextCache] bigint NOT NULL,
	[MaxSizeForBinaryCache] bigint NOT NULL,
    [LastRunDeleteExpiredCache] datetime2 NOT NULL,
	CONSTRAINT [imPK_Meta_Pk] PRIMARY KEY NONCLUSTERED HASH ([Pk])WITH ( BUCKET_COUNT = 1)
)WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_AND_DATA ); ";

        public const string InsertRowMetaTableFormatString = @"Insert into [{0}].Meta (Pk, IsDebugSchema, CacheIsEnabled, DefaultTTLinSeconds, MaxRowCountForAllTables, MaxSizeForTextCache, MaxSizeForBinaryCache, LastRunDeleteExpiredCache) 
    values (@Pk,@IsDebugSchema, @CacheIsEnabled, @DefaultTTLinSeconds, @MaxRowCountForAllTables, @MaxSizeForTextCache, @MaxSizeForBinaryCache, @LastRunDeleteExpiredCache); ";

        public const string DropStoredProcsFormatString = @"drop procedure [{0}].[DecrementCounter];
drop procedure [{0}].[DeleteCacheBinary];
drop procedure [{0}].[DeleteCacheText];
drop procedure [{0}].[DeleteCounter];
drop procedure [{0}].[IncrementCounter];
drop procedure [{0}].[RetrieveCacheBinary];
drop procedure [{0}].[RetrieveCounter];
drop procedure [{0}].[RetrieveCacheText];
drop procedure [{0}].[SaveCacheBinary];
drop procedure [{0}].[SaveCacheText];
drop procedure [{0}].[SetCounter];
drop procedure [{0}].[ClearCache];
drop procedure [{0}].[DeleteExpiredCache];";

        public const string CreateSPRetrieveCacheBinary = @"create procedure [{0}].[RetrieveCacheBinary] @uid uniqueidentifier
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   select BinaryBlob from [{0}].BinaryCache where [Pk] = @uid and Expires > GETUTCDATE();
END";
        public const string CreateSPSaveCacheBinary = @"create procedure [{0}].[SaveCacheBinary] @uid uniqueidentifier, @blob varbinary(7980), @expiration datetime2
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[BinaryCache] where [Pk] = @uid;
   insert into [{0}].[BinaryCache] values (@uid, @expiration, @blob);
END";
        public const string CreateSPDeleteCacheBinary = @"create procedure [{0}].[DeleteCacheBinary] @uid uniqueidentifier
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[BinaryCache] where [Pk] = @uid;
END";
        public const string CreateSPRetrieveCacheText = @"create procedure [{0}].[RetrieveCacheText] @uid uniqueidentifier
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   select TextBody from [{0}].TextCache where [Pk] = @uid and Expires > GETUTCDATE();
END";
        public const string CreateSPSaveCacheText = @"create procedure [{0}].[SaveCacheText] @uid uniqueidentifier, @body nvarchar(3950), @expiration datetime2
WITH   NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[TextCache] where [Pk] = @uid;
   insert into [{0}].[TextCache] values (@uid, @expiration, @body);
END";
        public const string CreateSPDeleteCacheText = @"create procedure [{0}].[DeleteCacheText] @uid uniqueidentifier
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[TextCache] where [Pk] = @uid;
END";
        public const string CreateSPSetCounter = @"create procedure [{0}].[SetCounter] @uid uniqueidentifier, @count bigint, @expiration datetime2
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[CounterCache] where [Pk] = @uid;
   insert into [{0}].[CounterCache] values (@uid, @expiration, @count);
END";
        public const string CreateSPRetrieveCounter = @"create procedure [{0}].[RetrieveCounter] @uid uniqueidentifier
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   select [Count] from [{0}].CounterCache where [Pk] = @uid and Expires > GETUTCDATE();
END";
        public const string CreateSPDeleteCounter = @"create procedure [{0}].[DeleteCounter] @uid uniqueidentifier
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[CounterCache] where [Pk] = @uid;
END";
        public const string CreateSPIncrementCounter = @"create procedure [{0}].[IncrementCounter] @uid uniqueidentifier, @newExpiration datetime2
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   update [{0}].CounterCache set Count = Count+1, Expires = @newExpiration where [Pk] = @uid and Expires > GETUTCDATE();
   if (@@rowcount = 0) BEGIN
		delete [{0}].CounterCache where [Pk] = @uid;
		insert into [{0}].[CounterCache] values (@uid, @newExpiration, 1);
   END
   select [Count] from [{0}].CounterCache where [Pk] = @uid and Expires > GETUTCDATE();
END";
        public const string CreateSPDecrementCounter = @"create procedure [{0}].[DecrementCounter] @uid uniqueidentifier, @newExpiration datetime2
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   update [{0}].CounterCache set Count = Count-1, Expires = @newExpiration where [Pk] = @uid and Expires > GETUTCDATE();
   if (@@rowcount = 0) BEGIN
		delete [{0}].CounterCache where [Pk] = @uid;
		insert into [{0}].[CounterCache] values (@uid, @newExpiration, -1);
   END
   select [Count] from [{0}].CounterCache where [Pk] = @uid and Expires > GETUTCDATE();
END";

        public const string CreateSPClearCache = @"create procedure [{0}].[ClearCache]
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[BinaryCache];
   delete [{0}].[TextCache];
   delete [{0}].[CounterCache];
END";

        public const string CreateSPDeleteExpiredCache = @"create procedure [{0}].[DeleteExpiredCache]
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].CounterCache where Expires <= GETUTCDATE();
   delete [{0}].BinaryCache where Expires <= GETUTCDATE();
   delete [{0}].TextCache where Expires <= GETUTCDATE();
END";

        //TODO create scheduled task to run DeleteExpiredCache, and populate Meta

        #endregion

        private static ILogger logger = LogManager.GetLogger(typeof(SchemaClient));
        private readonly string connectionString;
        private readonly string schemaName;
        public SchemaClient(string connectionString, string schemaName)
        {
            this.connectionString = connectionString;
            this.schemaName = schemaName;
        }

        public void CreateSchema(Action<string> updateStatus)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = new SqlCommand(string.Format(CreateSchemaNameFormatString, schemaName), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Schema `" + schemaName + "` has been created.");
            }
        }

        public void DropSchema(Action<string> updateStatus)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = new SqlCommand(string.Format(DropSchemaNameFormatString, schemaName), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Schema `" + schemaName + "` has been dropped.");
            }
        }

        public void CreateTables(Action<string> updateStatus, TimeSpan defaultTimeToLive)
        {
            if (defaultTimeToLive <= TimeSpan.Zero) throw new ArgumentException("You must specify a positive TimeSpan for parameter defaultTimeToLive. You specified " + defaultTimeToLive.ToString());
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                updateStatus?.Invoke("Creating TextCache table.");
                var comm = new SqlCommand(string.Format(CreateTextCacheTableFormatString, schemaName, CacheClient.TextMaxLength), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Creating BinaryCache table.");
                comm = new SqlCommand(string.Format(CreateBinaryCacheTableFormatString, schemaName, CacheClient.BlobMaxLength), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Creating CounterCache table.");
                comm = new SqlCommand(string.Format(CreateCounterCacheTableFormatString, schemaName), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Creating Meta table.");
                comm = new SqlCommand(string.Format(CreateMetaTableFormatString, schemaName), conn);
                comm.ExecuteNonQuery();

                updateStatus?.Invoke("Inserting row into meta table.");
                comm = new SqlCommand(string.Format(InsertRowMetaTableFormatString, schemaName), conn);
                comm.Parameters.AddWithValue("Pk", 1);
                comm.Parameters.AddWithValue("IsDebugSchema", false);
                comm.Parameters.AddWithValue("CacheIsEnabled", true);
                comm.Parameters.AddWithValue("DefaultTTLinSeconds", (long) defaultTimeToLive.TotalSeconds);
                comm.Parameters.AddWithValue("MaxRowCountForAllTables", 10000);
                comm.Parameters.AddWithValue("MaxSizeForTextCache", CacheClient.TextMaxLength);
                comm.Parameters.AddWithValue("MaxSizeForBinaryCache", CacheClient.BlobMaxLength);
                comm.Parameters.AddWithValue("LastRunDeleteExpiredCache", DateTime.UtcNow);
                comm.ExecuteNonQuery();

                updateStatus?.Invoke("Done creating tables.");
            }
        }

        public void DropTables(Action<string> updateStatus)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                updateStatus?.Invoke("Dropping TextCache table.");
                var comm = new SqlCommand(string.Format(DropTextCacheTableFormatString, schemaName), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Dropping BinaryCache table.");
                comm = new SqlCommand(string.Format(DropBinaryCacheTableFormatString, schemaName), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Dropping CounterCache table.");
                comm = new SqlCommand(string.Format(DropCounterCacheTableFormatString, schemaName), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Dropping Meta table.");
                comm = new SqlCommand(string.Format(DropMetaTableFormatString, schemaName), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Done dropping tables.");
            }
        }

        public void DropStoredProcedures(Action<string> updateStatus)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var comm = new SqlCommand(string.Format(DropStoredProcsFormatString, schemaName), conn);
                comm.ExecuteNonQuery();
                updateStatus?.Invoke("Stored procedures for schema `" + schemaName + "` have been dropped.");
            }
        }

        public void CreateStoredProcedures(Action<string> updateStatus)
        {
            var storedProcName = new []
            {
                "RetrieveCacheBinary", "SaveCacheBinary", "DeleteCacheBinary",
                "RetrieveCacheText", "SaveCacheText", "DeleteCacheText",
                "SetCounter", "RetrieveCounter", "DeleteCounter",
                "IncrementCounter", "DecrementCounter", "ClearCache","DeleteExpiredCache"
            };
            var formatStrings = new []
            {
                CreateSPRetrieveCacheBinary, CreateSPSaveCacheBinary, CreateSPDeleteCacheBinary,
                CreateSPRetrieveCacheText, CreateSPSaveCacheText, CreateSPDeleteCacheText,
                CreateSPSetCounter, CreateSPRetrieveCounter, CreateSPDeleteCounter,
                CreateSPIncrementCounter, CreateSPDecrementCounter, CreateSPClearCache,
                CreateSPDeleteExpiredCache
            };
            int index = 0;
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (var formatString in formatStrings)
                {
                    updateStatus?.Invoke("Creating stored procedure " + storedProcName[index]);
                    index++;
                    var comm = new SqlCommand(string.Format(formatString, schemaName), conn);
                    comm.ExecuteNonQuery();
                }
            }
        }
    }
}
