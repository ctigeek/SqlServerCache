# Sql Server Cache

### This is still BETA software.  There WILL be changes before 1.0. You have been warned.

Sql Server Cache is a .NET library that allows you to utilize your Sql Server instance as a fast in-memory data store, similar to memcached. 

```csharp
	var sscClient = new CacheClient(myConnectionStringName);
    var myObject = new MyObjectType();
    
    //save object
    sscClient.SetBinary(someString, myObject);
    
    //retrieve object
    var myObject2 = sscClient.RetrieveObject<MyObjectType>(someString);
    
    //delete object.  Cache items also have a Time-to-Live.
    sssClient.DeleteBinary(someString);
```

## Features
 * Very performant. [Comparable to memcached in speed](http://ctigeek.net/cache-comparison-memcached-vs-sql-server-wait-what/).
 * Simple to use in your code.
 * All functionality supports syncronous as well as Async Task.
 * Generic methods support strongly-typed return values.
 * Mockable.
 * Compatible with IOC containers.
 * Available as a Nuget package.
 * Included in the same library are a set of Powershell commands for managing and using it.

## Limitations
 * Keys are hashed so there's no limit on key-length. Because they're hashed they are also case-sensitive.
 * In-memory tables have a hard limit of 8 kb per row. If you use the SetBinary method to save data, it will compress the serialized data if it goes over the 8kb limit.  So using compression, the largest serialized object you can store is probably about 14 kb.
 * Limited to the memory of your active Sql Instance. (i.e. no sharding.) Sql Server is memory heavy to begin with, so be careful. Taking memory away from the Sql Server engine could impact database performance in other areas.  But hey, memory is super cheap so just order some more.
 
 
### How to set it up:
 1. Update your Sql Server database to [support in-memory tables](https://msdn.microsoft.com/en-us/library/dn133079.aspx). For this example, the database is called 'Cache'.
```sql
ALTER DATABASE Cache ADD FILEGROUP cache_mod CONTAINS MEMORY_OPTIMIZED_DATA 
-- Important! In the path below, you specify a folder, not a file.
ALTER DATABASE Cache ADD FILE (name='cache_mod', filename='c:\Program Files\Microsoft Sql Server\MSSQL12.MSSQLSERVER\MSSQL\DATA') TO FILEGROUP cache_mod 
ALTER DATABASE Cache SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT=ON
GO

```
 2. [Download the Nuget package](https://github.com/ctigeek/SqlServerCache/releases) and unzip it so you can access the DLL.
 3. Open a powershell window and import the DLL:
```powershell
Import-Module .\SqlServerCacheClient.dll
```
 4. Create the schema, tables, and stored procedures by running this powershell command:
```powershell
## Default time to live of 1 hour.
Install-SqlCache -DataSource "MyServer\MyInstance" -Database "Cache" -SchemaName "cache" -DefaultTimeToLive "0.01:00:00"
```
 5. Create a Sql Agent job to run the `cache.DeleteExpiredCache` stored procedure every minute or so.

You can repeat steps 4-5 with different SchemaName values if you want to create more than one cache repository in the same database. (i.e. you don't have to create multiple databases.)

And you're done! You can now use the cache from your code, or by using the powershell commands.

### More info about how it works
 * In memory tables is a new feature of Sql Server 2012.
 * There are four tables:
   * BinaryCache - used to store serialized objects.
   * TextCache - used to store strings.
   * CounterCache - used to store counters. Counters are always 64-bit integers.
   * Meta - contains a single ros that holds configuration data. This table is persisted to disk.
 * The code never accesses the tables directly. It uses pre-compiled stored procedures which are extremely fast.
 * All caches have a default Time-To-Live (which is stored in the Meta table). Unless you specify a TTL when setting a cache item, it will have the default TTL.
 * A cache item is considered dead (or expired) if the current UTC is greater than the UTC stored in the expires column.
 * A cache item that is still in the database, but is expired, will not be returned by the stored procedures called to retrieve it.
 * The stored procedure `DeleteExpiredCache` will delete all expired cache items. 
 * The stored procedure `DeleteExpiredCache` will also trim the tables down to the specified number of rows in the Meta table, deleting the oldest cache items first, to prevent memory bloat. You should adjust those values in the Meta table to match your memory available for caching.  I have **NOT** done any profiling to see how many rows is equivalent to how much memory, so good luck with that.

## Code examples

#### Instantiating
```csharp
	var sscClient = new CacheClient(myConnectionStringName);
```
 * myConnectionStringName - the *name* of your connection string in the app/web config file (not the connection string itself.)
 * This constructor will give schemaName the default value of 'cache', and cacheKeyPrefix the default value of string.Empty.

```csharp
   var sscClient = new CacheClient(myConnectionStringName, schemaName);
```
 * schemaName - this is the name of the schema that was used to create the cache.
 * This constructor will give cacheKeyPrefix the default value of string.Empty.

```csharp
   var sscClient = new CacheClient(myConnectionStringName, schemaName, cacheKeyPrefix);
```
* cacheKeyPrefix - a string to prefix to every key used by this client. This is useful when your cache is used for multiple purposes and there could be an overlapping key space. I don't recommend using this unless it is necessary. (Just create a new cache in the same database with a different schema name.) **There's a good chance this will be removed in 1.0.**

```csharp
   var sscClient = new CacheClient(myConnectionString, schemaName, cacheKeyPrefix, metaData);
```
* This constructor was included only for unit testing! **You should not use this constructor. I will probably make it internal for 1.0.**



#### Using Counters

```csharp
  string myKey = "counterKey1";
  
  // set a counter
  sscClient.SetCounter(myKey, 1);
  
  // increment the counter...
  sscClient.IncrementCounter(myKey);
  
  // decrement the counter...
  sscClient.DecrementCounter(myKey);
  
  // retrieve the counter...
  var theCount = sscClient.RetrieveCounter(myKey);  //returns null if counter doesn't exist.
  if (theCount.HasValue) WriteLine(theCount);
  
  // remove the counter from the cache...
  sscClient.DeleteCounter(myKey);
  
```
 * RetrieveCounter returns `long?`, so if a counter doesn't exist it will return null.
 * IncrementCounter will create a counter if it doesn't exist and set it to 1.
 * DecrementCounter will create a counter if it doesn't exist and set it to -1.
 * You do not need to explicitly create a counter. Just call Increment or Decrement and it will be created for you.
 * The time-to-live (TTL) on all cache items is set to the default unless you specifically set it.

```csharp
   //set a longer TTL for this one...
   sscClient.SetCounter(myKey, 1, TimeSpan.FromDays(999));
```

Doing the same thing in powershell:
```powershell
   ## All powershell commands assume the database is localhost (or localhost\SQLEXPRESS) unless otherwise specified.
   ## All powershell commands assume the database is called 'Cache' unless otherwise specified.
   ## All powershell commands assume the schema is called 'cache' unless otherwise specified.
   $myKey = "counterKey1"
  
  ## set a counter
  Set-SqlCacheCounter -Key $myKey -Count 1
  
  ## increment the counter...
  Set-SqlCacheCounter -Key $myKey -Increment
  
  ## decrement the counter...
  Set-SqlCacheCounter -Key $myKey -Decrement
  
  ## retrieve the counter...
  $counterVal = Get-SqlCacheCounter -Key
  if ($counterVal) Write-Host $counterVal
  
  ## remove the counter from the cache...
  Remove-SqlCacheCounter -Key $myKey
```


### Saving Objects

```csharp
  string myKey = "counterKey1";
  var myObject = new MyObjectType();
  
  // save object
  sscClient.SetBinary(myKey, myObject);
  
  // retrieve object
  var myObject2 = sscClient.RetrieveObject<MyObjectType>(myKey);
  
  // delete object
  sscClient.DeleteBinary(myKey);

```
Doing the same thing in Powershell:
```powershell
$myKey = "counterKey1"
$myObject = New-Object -TypeName "MyObjectType"

## save the object to cache...
Set-SqlCacheObject -Key $myKey -Value $myObject

## retrieve the object
$myObject2 = Get-SqlCacheObject -Key $myKey

## delete from cache
Remove-SqlCacheObject -Key $myKey

```


### Saving Text

You should only use the Text functions if you need to "see" the text in the tables. i.e. if you are going to be using the cache from other places outside of this client code and want to see the value.  These tables can only hold about 4 kb of text. You are better off using the SetBinary & RetrieveObject functions, which can save far greater sizes.


TBD









