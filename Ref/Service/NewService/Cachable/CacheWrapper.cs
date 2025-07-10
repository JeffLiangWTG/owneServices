using System;
using System.Runtime.Caching;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.NewService
{
	public class CacheWrapper : ICacheWrapper
	{
		public CacheWrapper(MemoryCache memoryCache, CacheItemPolicy policy)
		{
			Argument.NotNull(policy, nameof(policy));
			cache = memoryCache;
			this.policy = policy;
		}

		public CacheWrapper()
			: this(MemoryCache.Default, new CacheItemPolicy())
		{
		}

		readonly MemoryCache cache;
		readonly CacheItemPolicy policy;

		public object Add(string key, object value)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			Argument.NotNull(value, nameof(value));
			return cache.Add(key, value, policy);
		}

		public object Remove(string key)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			return cache.Remove(key);
		}

		public object Get(string key)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			return cache.Get(key);
		}

		public bool Contains(string key)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			return cache.Contains(key);
		}

		public object Add(string key, object value, DateTimeOffset absoluteExpiration)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			Argument.NotNull(value, nameof(value));
			return cache.Add(key, value, new CacheItemPolicy() { AbsoluteExpiration = absoluteExpiration });
		}
	}
}
