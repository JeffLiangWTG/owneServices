using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Web;

namespace CargoWise.eHub.DataAccess.Cache
{
	delegate object SqlValueDelegate();

	internal static class DataCache
	{
		private const string DATA_CACHE_EXPIRY_KEY = "DataCacheExpiry";

		public static object Retrieve(string key, SqlValueDelegate sqlValueDelegate)
		{
			var result = HttpRuntime.Cache[key];
			if (result != null) return result;

			CacheData(key, sqlValueDelegate);

			return HttpRuntime.Cache[key];
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		static void CacheData(string key, SqlValueDelegate sqlValueDelegate)
		{
			if (HttpRuntime.Cache[key] == null)
			{
				var value = sqlValueDelegate();

				if (value != null)
				{
					HttpRuntime.Cache.Add(key,
						value,
						null,
						System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationManager.AppSettings[DATA_CACHE_EXPIRY_KEY])), System.Web.Caching.CacheItemPriority.Normal, null);
				}
			}
		}
	}
}
