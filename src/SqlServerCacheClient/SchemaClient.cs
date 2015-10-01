using System;

namespace SqlServerCacheClient
{
    public class SchemaClient
    {
        #region Format Strings

        public const string CreateSchemaName = "create schema [{0}]";

        public const string DropTextCacheTableFormatString = @"drop table [{0}].[TextCache];";
        public const string CreateTextCacheTableFormatString = @"CREATE TABLE [{0}].[TextCache] (
	[Key] [uniqueidentifier] NOT NULL,
	[Expires] [datetime2](7) NOT NULL,
	[TextBody] [nvarchar]({1}) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
CONSTRAINT [imPK_TextCache_Key] PRIMARY KEY NONCLUSTERED HASH (	[Key])WITH ( BUCKET_COUNT = 16384),
INDEX [IX_TextCache_Expires] NONCLUSTERED ( [Expires] ASC )
)WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_ONLY ); ";

        public const string DropBinaryCacheTableFormatString = @"drop table [{0}].[BinaryCache];";
        public const string CreateBinaryCacheTableFormatString = @"CREATE TABLE [{0}].[BinaryCache] (
	[Key] [uniqueidentifier] NOT NULL,
	[Expires] [datetime2](7) NOT NULL,
	[BinaryBlob] [varbinary]({1}) NOT NULL,
CONSTRAINT [imPK_BinaryCache_Key] PRIMARY KEY NONCLUSTERED HASH ([Key]) WITH ( BUCKET_COUNT = 16384),
INDEX [IX_BinaryCache_Expires] NONCLUSTERED ( [Expires] ASC ) )
WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_ONLY ); ";

        public const string DropTableCounterCache = @"drop table [{0}].[CounterCache];";
        public const string CreateTableCounterCache = @"CREATE TABLE [{0}].[CounterCache] (
	[Key] [uniqueidentifier] NOT NULL,
	[Expires] [datetime2](7) NOT NULL,
	[Count] [bigint] NOT NULL,
CONSTRAINT [imPK_CounterCache_Key] PRIMARY KEY NONCLUSTERED HASH ([Key]) WITH ( BUCKET_COUNT = 1000),
INDEX [IX_CounterCache_Expires] NONCLUSTERED ( [Expires] ASC)
)WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_ONLY ); ";

        public const string DropTableMeta = @"drop table [{0}].[Meta];";
        public const string CreateTableMeta = @"CREATE TABLE [{0}].[Meta] (
	[Key] int NOT NULL,
	[IsDebugSchema] bit NOT NULL,
	[CacheIsEnabled] bit NOT NULL,
	[DefaultTTLinSeconds] bigint NOT NULL,
	[MaxRowCountForAllTables] bigint NOT NULL,
	[MaxSizeForTextCache] bigint NOT NULL,
	[MaxSizeForBinaryCache] bigint NOT NULL,
    [LastRunDeleteExpiredCache] datetime2 NOT NULL,
	CONSTRAINT [imPK_Meta_Key] PRIMARY KEY NONCLUSTERED HASH ([Key])WITH ( BUCKET_COUNT = 1)
)WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_AND_DATA ); ";

        public const string DropStoredProcs = @"drop procedure [{0}].[DecrementCounter];
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
   select BinaryBlob from {0}.BinaryCache where [Key] = @uid and Expires > getdate();
END";
        public const string CreateSPSaveCacheBinary = @"create procedure [{0}].[SaveCacheBinary] @uid uniqueidentifier, @blob varbinary(7980), @expiration datetime2
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[BinaryCache] where [Key] = @uid;
   insert into [{0}].[BinaryCache] values (@uid, @expiration, @blob);
END";
        public const string CreateSPDeleteCacheBinary = @"create procedure [{0}].[DeleteCacheBinary] @uid uniqueidentifier
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[BinaryCache] where [Key] = @uid;
END";
        public const string CreateSPRetrieveCacheText = @"create procedure [{0}].[RetrieveCacheText] @uid uniqueidentifier
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   select TextBody from {0}.TextCache where [Key] = @uid and Expires > getdate();
END";
        public const string CreateSPSaveCacheText = @"create procedure [{0}].[SaveCacheText] @uid uniqueidentifier, @body nvarchar(3950), @expiration datetime2
WITH   NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[TextCache] where [Key] = @uid;
   insert into [{0}].[TextCache] values (@uid, @expiration, @body);
END";
        public const string CreateSPDeleteCacheText = @"create procedure [{0}].[DeleteCacheText] @uid uniqueidentifier
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[TextCache] where [Key] = @uid;
END";
        public const string CreateSPSetCounter = @"create procedure [{0}].[SetCounter] @uid uniqueidentifier, @count bigint, @expiration datetime2
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[CounterCache] where [Key] = @uid;
   insert into [{0}].[CounterCache] values (@uid, @expiration, @count);
END";
        public const string CreateSPRetrieveCounter = @"create procedure [{0}].[RetrieveCounter] @uid uniqueidentifier
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   select [Count] from {0}.CounterCache where [Key] = @uid and Expires > getdate();
END";
        public const string CreateSPDeleteCounter = @"create procedure [{0}].[DeleteCounter] @uid uniqueidentifier
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   delete [{0}].[CounterCache] where [Key] = @uid;
END";
        public const string CreateSPIncrementCounter = @"create procedure [{0}].[IncrementCounter] @uid uniqueidentifier, @newExpiration datetime2
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   update {0}.CounterCache set Count = Count+1, Expires = @newExpiration where [Key] = @uid and Expires > getdate();
   if (@@rowcount = 0) BEGIN
		delete {0}.CounterCache where [Key] = @uid;
		insert into [{0}].[CounterCache] values (@uid, @newExpiration, 1);
   END
   select [Count] from {0}.CounterCache where [Key] = @uid and Expires > getdate();
END";
        public const string CreateSPDecrementCounter = @"create procedure [{0}].[DecrementCounter] @uid uniqueidentifier, @newExpiration datetime2
WITH  NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER  AS
BEGIN ATOMIC
   WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE='us_english')
   update {0}.CounterCache set Count = Count+1, Expires = @newExpiration where [Key] = @uid and Expires > getdate();
   if (@@rowcount = 0) BEGIN
		delete {0}.CounterCache where [Key] = @uid;
		insert into [{0}].[CounterCache] values (@uid, @newExpiration, -1);
   END
   select [Count] from {0}.CounterCache where [Key] = @uid and Expires > getdate();
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
   delete {0}.CounterCache where Expires <= getdate();
   delete {0}.BinaryCache where Expires <= getdate();
   delete {0}.TextCache where Expires <= getdate();
END";

        //TODO create scheduled task to run DeleteExpiredCache, and populate Meta

        #endregion
        private readonly string connectionString;
        public SchemaClient(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void CreateTables(bool dropExistingTables)
        {
            
        }

        public void CreateStoredProcedures()
        {
            
        }
    }
}
