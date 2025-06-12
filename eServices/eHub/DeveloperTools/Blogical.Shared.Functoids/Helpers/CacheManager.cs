using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data.Common;
using System.Data;

namespace Blogical.Shared.Functoids
{
    /// <summary>
    /// This class gives access to the HttpRuntimes Cache class and ensures it is running 
    /// and ready to use for non-web applications.
    /// </summary>
    public class CacheManager
    {
        /// <summary>
        /// This object holds a cache-object that we utilize for result-persistance.
        /// </summary>
        private static HttpRuntime _httpRuntime;

        /// <summary>
        /// Gives us a safe and easy way to access the cache.
        /// </summary>
        /// <value>The cache contained in the HttpRuntime-class</value>
        public static System.Web.Caching.Cache Cache
        {
            get
            {
                EnsureHttpRuntime();
                return HttpRuntime.Cache;
            }
        }

        /// <summary>
        /// Creates an instance of HttpRuntime if it doesn't exist.
        /// </summary>
        private static void EnsureHttpRuntime()
        {
            if (null == _httpRuntime)
            {
                // Create an Http Content to give us access to the cache.
                _httpRuntime = new HttpRuntime();
            }
        }

        /// <summary>
        /// This assembles and returns a unique key to use as cache key from the specified parameters.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string BuildCacheKey(string query, DbParameterCollection parameters)
        {
            string key = query;

            foreach (DbParameter p in parameters)
            {
                key += "_" + p.ParameterName + (p.Direction == ParameterDirection.Output ? "out" : p.Value) + "_";
            }

            return key;
        }
    }
}
