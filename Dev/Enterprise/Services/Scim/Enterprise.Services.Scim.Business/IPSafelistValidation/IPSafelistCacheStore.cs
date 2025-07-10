using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
#if NETFRAMEWORK
using System.Runtime.Caching;
#endif
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
#if NETCOREAPP
using Microsoft.Extensions.Caching.Memory;
#endif

namespace Enterprise.Services.Scim.Business
{
	public class IPSafelistCacheStore : IIPSafelistCacheStore
	{
		readonly MemoryCache cache;

		public IPSafelistCacheStore()
		{
#if NETFRAMEWORK
			cache = MemoryCache.Default;
#else
			cache = new MemoryCache(new MemoryCacheOptions());
#endif
		}

		public List<IPNetwork2> GetSafelistedIpsFromCache()
		{
#if NETFRAMEWORK
			var safelistedIps = (string[])cache.Get(IPSafelistHelper.SAFELIST_CACHE_KEY, null);
#else
			var safelistedIps = (string[])cache.Get(IPSafelistHelper.SAFELIST_CACHE_KEY);
#endif
			if (safelistedIps == null || safelistedIps.Length == 0)
			{
				UpdateSafelistToCache();
#if NETFRAMEWORK
				safelistedIps = (string[])cache.Get(IPSafelistHelper.SAFELIST_CACHE_KEY, null);
#else
				safelistedIps = (string[])cache.Get(IPSafelistHelper.SAFELIST_CACHE_KEY);
#endif
			}

			if (safelistedIps == null || safelistedIps.Length == 0)
			{
				throw new InvalidOperationException("No IP safelist networks configured");
			}

			var ipNetworks = new List<IPNetwork2>();
			foreach (var ip in safelistedIps)
			{
				if (IPNetwork2.TryParse(ip, out var ipnetwork))
				{
					ipNetworks.Add(ipnetwork);
				}
			}

			return ipNetworks;
		}

		void UpdateSafelistToCache()
		{
			try
			{
				var factory = new BusinessObjectFactory();
				var stmDataRecord = factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, Config.Constants.ScimIpSafelistConstant)).FirstOrDefault();

				if (stmDataRecord == null || stmDataRecord.SD_BinaryValue == null)
				{
					throw new InvalidOperationException("No IP safelist found in the database");
				}

				var ipJson = System.Text.Encoding.UTF8.GetString(stmDataRecord.SD_BinaryValue);
				var safelistedIps = JsonConvert.DeserializeObject<string[]>(ipJson);

				if (safelistedIps == null || safelistedIps.Length == 0)
				{
					throw new InvalidOperationException("No IP safelist networks configured");
				}

#if NETFRAMEWORK
				var policy = new CacheItemPolicy
				{
					AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(5) // Refresh the cache every 5 days - Azure IP ranges are updated weekly
				};
#else
				var policy = new MemoryCacheEntryOptions
				{
					AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(5) // Refresh the cache every 5 days - Azure IP ranges are updated weekly
				};
#endif

				cache.Set(IPSafelistHelper.SAFELIST_CACHE_KEY, safelistedIps, policy);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Failed to update IP safelist in cache", ex);
			}
		}
	}
}
