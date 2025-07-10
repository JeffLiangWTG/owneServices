using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.MasterData.GUI
{
	sealed class DedupePanelAdvancedFilterHelper
	{
		DedupePanelAdvancedFilterHelper()
		{
		}

		static readonly ConcurrentBag<string> PersonCachedList = new ConcurrentBag<string>();
		static readonly ConcurrentBag<string> PersonFilterNamesToBeCachedPersistently = new ConcurrentBag<string>();

		public const string RegionName = "DedupePanelAdvancedFilterCache_";

		public static void SetCache(string name, object model, bool isPersistenceRequired, FilterType filterType = FilterType.Person)
		{
			var key = GetCacheKey(name);
			if (PersonCachedList.All(u => u != key))
			{
				PersonCachedList.Add(key);
			}

			if (isPersistenceRequired && filterType == FilterType.Person && PersonFilterNamesToBeCachedPersistently.All(u => u != key))
			{
				PersonFilterNamesToBeCachedPersistently.Add(key);
			}

			MemoryCache.Default.Set(key, model, new CacheItemPolicy());
		}

		public static object GetCache(string name)
		{
			return MemoryCache.Default.Get(GetCacheKey(name));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
		public static void ClearCache()
		{
			while (PersonCachedList.TryTake(out var key))
			{
				if (!string.IsNullOrEmpty(key) && MemoryCache.Default.Contains(key))
				{
					MemoryCache.Default.Remove(key);
				}
			}

			while (!PersonFilterNamesToBeCachedPersistently.IsEmpty)
			{
				PersonFilterNamesToBeCachedPersistently.TryTake(out _);
			}
		}

		public static void LoadPersonPersistenceFiltersValueToCache()
		{
			// TODO: Implement in the next WI, make sure when set the cache, not set to the NeedPersistencePersonFilterCache
		}

		public static void PersistencePersonFiltersValue(BusinessObjectFactory factory)
		{
			var needPersistenceFilters = new List<object>();

			while (PersonFilterNamesToBeCachedPersistently.TryTake(out var key))
			{
				if (!string.IsNullOrEmpty(key) && MemoryCache.Default.Contains(key))
				{
					var filterValueCache = MemoryCache.Default.Get(key);

					if (filterValueCache != null)
					{
						needPersistenceFilters.Add(filterValueCache);
					}
				}
			}

			if (needPersistenceFilters.Count > 0)
			{
				var stmData = LoadOrCreateNewStmData(factory, MDMPersonDeduplicationPanelFiltersValueKey);
				stmData.SD_BinaryValue = SerializeTheFiltersValue(needPersistenceFilters);
				factory.Save();
			}
		}

		internal const string MDMPersonDeduplicationPanelFiltersValueKey = "MDMPersonDeduplicationPanelFiltersValue";

		static StmData LoadOrCreateNewStmData(BusinessObjectFactory factory, string key)
		{
			var findQuery = new ZQuery(StmDataSchema.SD_Owner, Env.CurrentUser.PK);
			findQuery.AddToFilter(StmDataSchema.SD_Name, key);
			var stmData = factory.LoadTop1<StmData>(findQuery);

			if (stmData == null)
			{
				stmData = factory.New<StmData>();
				stmData.SD_Owner = Env.CurrentUser.PK;
				stmData.SD_Name = key;
				stmData.SD_Type = RegistryDataTypes.Codes.Binary;
			}

			return stmData;
		}

		static ZBlob SerializeTheFiltersValue(List<object> filterValues)
		{
			var jsonStr = JsonConvert.SerializeObject(filterValues, Formatting.None, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			});

			return ZBlob.FromUTF8(jsonStr);
		}

		static string GetCacheKey(string name)
		{
			return RegionName + name;
		}
	}

	public enum FilterType
	{
		Person,
		Organization
	}
}
