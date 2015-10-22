using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SqlServerCacheClient.Logging;

namespace SqlServerCacheClient
{
    internal static class MetaDataManager
    {
        private static readonly ILogger logger = LogManager.GetLogger(typeof(MetaDataManager));
        private static readonly List<MetaData> metaDataList;
        private static Timer timer;
        private readonly static object lockObject = new object();

        static MetaDataManager()
        {
            metaDataList = new List<MetaData>();
            timer = new Timer(CheckCacheData, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        public static MetaData GetMetaData(string connectionString, string schemaName)
        {
            lock (lockObject)
            {
                var metaData = metaDataList.FirstOrDefault(mdl => mdl.ConnectionString == connectionString && mdl.SchemaName == schemaName);
                if (metaData == null)
                {
                    metaData = LoadCacheData(connectionString, schemaName);
                    metaDataList.Add(metaData);
                }
                return metaData;
            }
        }

        private static void CheckCacheData(object o)
        {
            lock (lockObject)
            {
                try
                {
                    foreach (var metaData in metaDataList.ToArray())
                    {
                        var newMetaData = LoadCacheData(metaData.ConnectionString, metaData.SchemaName);
                        metaDataList.Add(newMetaData);
                        metaDataList.Remove(metaData);
                    }
                }
                catch(Exception ex)
                {
                    logger.ErrorFormat("Error reloading metadata. {0}", ex);
                }
            }
        }

        private static MetaData LoadCacheData(string connectionString, string schemaName)
        {
            try
            {
                var metaData = new MetaData(connectionString, schemaName);
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var comm = new SqlCommand($"select * from [{schemaName}].Meta where Pk=@Pk;", conn);
                    comm.Parameters.AddWithValue("Pk", 1);
                    using (var reader = comm.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            metaData.DefaultTimeToLive = TimeSpan.FromSeconds((long)reader["DefaultTTLinSeconds"]);
                            metaData.CacheIsEnabled = (bool) reader["CacheIsEnabled"];
                            metaData.IsDebugSchema = (bool) reader["IsDebugSchema"];
                            metaData.LastRunDeleteExpiredCache = (DateTime) reader["LastRunDeleteExpiredCache"];
                            metaData.MaxRowCountForAllTables = (long) reader["MaxRowCountForAllTables"];
                            metaData.MaxSizeForTextCache = (long) reader["MaxSizeForTextCache"];
                            metaData.MaxSizeForBinaryCache = (long)reader["MaxSizeForBinaryCache"];
                        }
                        else
                        {
                            throw new ApplicationException("Could not find configuration in Meta table.");
                        }
                    }
                }
                return metaData;
            }
            catch (Exception ex)
            {
                //TODO: is there's a problem reading the meta table, we should probably mark the cache as disabled.
                logger.ErrorFormat("Error while loading MetaData for cache {0}. {1}", schemaName, ex);
                throw;
            }
        }
    }
}