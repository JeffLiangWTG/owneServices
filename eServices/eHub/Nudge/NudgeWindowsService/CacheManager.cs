using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.eHub.Nudge
{
	public class CacheManager<KeyType, ValueType>
	{
		public ValueType GetItemFromCache(KeyType key)
		{
			ValueType value;
			cache.TryGetValue(key, out value);
			return value;
		}

		public ValueType GetItemFromCacheOrCreateWhenNotExist(KeyType key, Func<ValueType> createItemFunc)
		{
			ValueType value;
			cache.TryGetValue(key, out value);
			if (value == null)
			{
				value = createItemFunc();
				cache[key] = value;
			}
			return value;
		}

		public bool PutInCache(KeyType key, ValueType value)
		{
			ValueType valueInCache;
			if (cache.TryGetValue(key, out valueInCache))
			{
				return false;
			}
			else
			{
				cache[key] = value;
				return true;
			}
		}

		public void RemoveItemFromCacheByKey(KeyType key)
		{
			cache.Remove(key);
		}

		public ValueType[] GetAllItems()
		{
			return cache.Values.ToArray();
		}

		public int Count
		{
			get
			{
				return cache.Count;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return cache.Count == 0;
			}
		}

		public void Clear()
		{
			cache.Clear();
		}

		Dictionary<KeyType, ValueType> cache = new Dictionary<KeyType, ValueType>();
	}

	public class CacheManager
	{
		public static CacheManager<string, EHubClientSystem> SystemInfoCache = new CacheManager<string, EHubClientSystem>();
		public static CacheManager<string, NudgeRequestManager> NudgeRequestControlCache = new CacheManager<string, NudgeRequestManager>();
	}
}
