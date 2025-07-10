using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Caching;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.NewService
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public class FileCacheProvider : IFileCacheProvider
	{
		[ActivatorUtilitiesConstructor]
		public FileCacheProvider(ILogHelper logHelper) : this(logHelper, ApplicationConfig.CacheFolder)
		{
		}

		public FileCacheProvider(ILogHelper logHelper, string cacheFolder)
		{
			_logHelper = logHelper;

			cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cacheFolder);
			cleanInterval = TimeSpan.FromHours(int.Parse(ApplicationConfig.CacheCleanIntervalInHours, CultureInfo.InvariantCulture));
			syncInterval = TimeSpan.FromMinutes(int.Parse(ApplicationConfig.CacheSizeSyncIntervalInMinutes, CultureInfo.InvariantCulture));
			syncStartPercentage = double.Parse(ApplicationConfig.CacheSizeSyncBeginPercentage, CultureInfo.InvariantCulture) / 100;
			expirationInterval = TimeSpan.FromHours(int.Parse(ApplicationConfig.CacheExpirationInHours, CultureInfo.InvariantCulture));
			maxCacheSize = long.TryParse(ApplicationConfig.CacheMaxSize, out var maxSize) ? maxSize : long.MaxValue;

			cache = new FileCache(cacheDir, true, TimeSpan.FromDays(7));
			cacheManager = cache.CacheManager;
			FormattedHostName = Dns.GetHostName() + ": ";

			_logHelper.LogInfo(null, $"{FormattedHostName}FileCache is created at UtcTime {DateTime.UtcNow}. CurrentCacheSize is {cache.CurrentCacheSize}.");
			if (ShouldClean())
			{
				CleanExpiredCacheAsync();
			}
		}

		readonly ILogHelper _logHelper;
		readonly FileCache cache;
		readonly FileCacheManager cacheManager;
		readonly TimeSpan cleanInterval;
		readonly TimeSpan syncInterval;
		readonly double syncStartPercentage;
		readonly TimeSpan expirationInterval;
		readonly long maxCacheSize;
		readonly string cacheDir;
		readonly string FormattedHostName;
		const string SemaphoreFile = "cache.sem";
		const string LastCleanedDateFile = "cache.lcd";
		const string LastCacheSizeSyncFile = "cache.lss";

		public void Add(string key, object value, string regionName = null)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			Argument.NotNull(value, nameof(value));
			cache.Set(key, value, DateTimeOffset.Now.Add(expirationInterval), regionName);
		}

#if DEBUG
		public long CurrentCacheSize
		{
			get
			{
				return _currentCacheSizeOverride == 0 ? cache.CurrentCacheSize : _currentCacheSizeOverride;
			}
			set
			{
				_currentCacheSizeOverride = value;
			}
		}	
		long _currentCacheSizeOverride ;
		public void Add(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			Argument.NotNull(value, nameof(value));
			Argument.NotNull(absoluteExpiration, nameof(absoluteExpiration));
			cache.Set(key, value, absoluteExpiration, regionName);
		}
		public void WriteLastCacheSizeSyncFile(DateTime dateTime)
		{
			cacheManager.WriteSysValue(LastCacheSizeSyncFile, dateTime);
		}
#endif


		public bool Contains(string key, string regionName = null)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			return cache.Contains(key, regionName);
		}

		public object Get(string key, string regionName = null)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			return cache.Get(key, regionName);
		}

		public object Remove(string key, string regionName = null)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			return cache.Remove(key, regionName);
		}

		public string GetCachePath(string key)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			var cacheFilePath = cacheManager.GetCachePath(key);
			return cacheFilePath;
		}

		public CacheItemPolicy GetPolicy(string key, string regionName = null)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			return cache.GetPolicy(key, regionName);
		}

		public void Clear()
		{
			cache.Clear();
		}

#if DEBUG
		public
#endif
		long CleanExpiredCache(string regionName = null)
		{
			long removed = 0;
			_logHelper.LogInfo(null, $"{FormattedHostName}Start CleanExpiredCache");
			//lock down other threads from trying to shrink or clean
			using (FileStream cLock = GetCleaningLock())
			{
				if (cLock == null)
				{
					_logHelper.LogInfo(null, $"{FormattedHostName}Stop cleaning as other thread is trying to clean or shrink");
					return 0;
				}

				IEnumerable<string> regions =
					string.IsNullOrEmpty(regionName)
						? cacheManager.GetRegions()
						: new List<string>(1) { regionName };

				foreach (var region in regions)
				{
					_logHelper.LogInfo(null, $"{FormattedHostName}Start to clean expired {region ?? "file"} cache, expiration time: {DateTime.Now}.");
					var removedKeyList = new List<string>();
					foreach (string key in cache.GetKeys(region))
					{
						CacheItemPolicy policy = GetPolicy(key, region);
						if (policy.AbsoluteExpiration < DateTime.Now)
						{
							try
							{
								string cachePath = cacheManager.GetCachePath(key, region);
								string policyPath = cacheManager.GetPolicyPath(key, region);
								CacheItemReference ci = new CacheItemReference(key, region, cachePath, policyPath);
								Remove(key, region); // CT note: Remove will update CurrentCacheSize
								removed += ci.Length;
								removedKeyList.Add(key);
							}
							catch (Exception ex) // skip if the file cannot be accessed
							{
								_logHelper.LogInfo(null, $"{FormattedHostName}Failed to remove expired {region ?? "file"} cache, key: {key}, error: {ex.Message}");
							}
						}
						else
						{
							_logHelper.LogInfo(null, $"{FormattedHostName}Not clean as {policy.AbsoluteExpiration} >= {DateTime.Now}");
						}
					}
					_logHelper.LogInfo(null, $"{FormattedHostName}Removed expired {region ?? "file"} cache, keys: {string.Join(", ", removedKeyList)}");
				}

				// mark that we've cleaned the cache
				cacheManager.WriteSysValue(LastCleanedDateFile, DateTime.Now);

				// unlock
				cLock.Close();
			}
			_logHelper.LogInfo(null, $"{FormattedHostName}Clean expired cache finished, total removed size: {removed} bytes.");
			return removed;
		}

		FileStream GetCleaningLock()
		{
			_logHelper.LogInfo(null, $"{FormattedHostName}Start GetCleaningLock");
			try
			{
				return File.Open(Path.Combine(cacheDir, SemaphoreFile), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			}
			catch (Exception ex)
			{
				_logHelper.LogInfo(null, $"{FormattedHostName}Failed to get cleaning lock for cache file, error message: {ex.Message}");
				return null;
			}
		}

		public bool ShouldClean()
		{
			try
			{
				DateTime lastClean;
				if (!cacheManager.ReadSysValue(LastCleanedDateFile, out lastClean))
				{
					_logHelper.LogInfo(null, $"{FormattedHostName}FileCache lastCleanTime cannot be read from {LastCleanedDateFile}.");
					return true;
				}

				var shouldClean = DateTime.Now - lastClean >= cleanInterval;
				_logHelper.LogInfo(null, $"{FormattedHostName}FileCache lastCleanTime is {lastClean}, should clean: {shouldClean}");
				return shouldClean;
			}
			catch (Exception ex)
			{
				_logHelper.LogInfo(null, $"{FormattedHostName}Failed to check if should clean cache, error: {ex.Message}");
				return true;
			}
		}

		public void CleanExpiredCacheAsync()
		{
			Task.Factory.StartNew(() =>
			{
				CleanExpiredCache();
			});
		}

		public void ShrinkCacheSizeAsync()
		{
			DateTime lastSync;
			var cacheSize = cache.CurrentCacheSize;
			_logHelper.LogInfo(null, $"{FormattedHostName}Sync start percentage is: {syncStartPercentage}, CurrentCacheSize is {cacheSize}");
#if DEBUG  //This is for testing, becaseu the CurrentCacheSize in PROD is not accessible and not accurate
			cacheSize = CurrentCacheSize;
#endif
			if (cacheSize >= maxCacheSize)
			{
				Task.Factory.StartNew(() =>
				{
					var newSize = ShrinkCacheToSize((long)(maxCacheSize * 0.75));
				});
			}
			else if (cacheSize > syncStartPercentage * maxCacheSize)
			{
				if (!cacheManager.ReadSysValue(LastCacheSizeSyncFile, out lastSync)
					|| (DateTime.Now - lastSync >= syncInterval))
				{
					_logHelper.LogInfo(null, $"{FormattedHostName}Start to sync cache size. Previous cache size: {cacheSize}");
					cache.UpdateCacheSizeAsync();
					cacheManager.WriteSysValue(LastCacheSizeSyncFile, DateTime.Now);
				}
			}
		}

#if DEBUG
		public
#endif
		long ShrinkCacheToSize(long newSize, string regionName = null)
		{
			long originalSize = 0, amount = 0, removed = 0;

			//lock down other threads from trying to shrink or clean
			using (FileStream cLock = GetCleaningLock())
			{
				if (cLock == null)
				{
					_logHelper.LogInfo(null, $"{FormattedHostName}Stop shrinking as other thread is trying to clean or shrink");
					return -1;
				}

				// if we're shrinking the whole cache, we can use the stored
				// size if it's available. If it's not available we calculate it and store
				// it for next time.
				if (regionName == null)
				{
					originalSize = cache.CurrentCacheSize;
				}
				else
				{
					originalSize = cache.GetCacheSize(regionName);
				}

				_logHelper.LogInfo(null, $"{FormattedHostName}Start to shrink cache size, current size: {originalSize} bytes, target size: {newSize} bytes.");
				// Find out how much we need to get rid of
				amount = originalSize - newSize;
				// CT note: This will update CurrentCacheSize
				removed = DeleteOldestFiles(amount, regionName);
				// unlock the semaphore for others
				cLock.Close();
			}

			// return the final size of the cache (or region)
			var newCacheSize = originalSize - removed;
			_logHelper.LogInfo(null, $"{FormattedHostName}Shrink cache finished, new cache size: {newCacheSize} bytes.");
			return newCacheSize;
		}

		long DeleteOldestFiles(long amount, string regionName = null)
		{
			// Verify that we actually need to shrink
			if (amount <= 0)
			{
				return 0;
			}

			//Heap of all CacheReferences
			PriortyQueue<CacheItemReference> cacheReferences = new PriortyQueue<CacheItemReference>();

			IEnumerable<string> regions =
				string.IsNullOrEmpty(regionName)
					? cacheManager.GetRegions()
					: new List<string>(1) { regionName };

			foreach (var region in regions)
			{
				//build a heap of all files in cache region
				foreach (string key in cacheManager.GetKeys(region))
				{
					try
					{
						//build item reference
						string cachePath = cacheManager.GetCachePath(key, region);
						string policyPath = cacheManager.GetPolicyPath(key, region);
						CacheItemReference ci = new CacheItemReference(key, region, cachePath, policyPath);
						cacheReferences.Enqueue(ci);
					}
					catch (Exception ex)
					{
						_logHelper.LogInfo(null, $"{FormattedHostName}Failed to get {region ?? "file"} cache file info, key: {key}, error: {ex.Message}");
					}
				}
			}

			//remove cache items until size requirement is met
			long removedBytes = 0;
			var removedKeyList = new List<string>();
			while (removedBytes < amount && cacheReferences.GetSize() > 0)
			{
				//remove oldest item
				CacheItemReference oldest = cacheReferences.Dequeue();
				try
				{
					removedBytes += oldest.Length;
					Remove(oldest.Key, oldest.Region);
					var cachePath = string.IsNullOrEmpty(oldest.Region) ? "" : oldest.Region + "\\";
					removedKeyList.Add(cachePath + oldest.Key);
				}
				catch (Exception ex)
				{
					_logHelper.LogInfo(null, $"{FormattedHostName}Failed to remove {oldest.Region ?? "file"} cache, key: {oldest.Key}, error: {ex.Message}");
				}
			}

			_logHelper.LogInfo(null, $"{FormattedHostName}Removed file cache, keys: {string.Join(", ", removedKeyList)}, total size: {removedBytes} bytes.");
			return removedBytes;
		}

		private class CacheItemReference : IComparable<CacheItemReference>
		{
			public readonly DateTime LastAccessTime;
			public readonly long Length;
			public readonly string Key;
			public readonly string Region;

			public CacheItemReference(string key, string region, string cachePath, string policyPath)
			{
				Key = key;
				Region = region;
				FileInfo cfi = new FileInfo(cachePath);
				FileInfo pfi = new FileInfo(policyPath);
				cfi.Refresh();
				LastAccessTime = cfi.LastAccessTime;
				Length = cfi.Length + pfi.Length;
			}

			public int CompareTo(CacheItemReference other)
			{
				int i = LastAccessTime.CompareTo(other.LastAccessTime);

				// It's possible, although rare, that two different items will have
				// the same LastAccessTime. So in that case, we need to check to see
				// if they're actually the same.
				if (i == 0)
				{
					// second order should be length (but from smallest to largest,
					// that way we delete smaller files first)
					i = -1 * Length.CompareTo(other.Length);
					if (i == 0)
					{
						i = string.Compare(Region, other.Region, StringComparison.Ordinal);
						if (i == 0)
						{
							i = string.Compare(Key, other.Key, StringComparison.Ordinal);
						}
					}
				}

				return i;
			}

			public static bool operator >(CacheItemReference lhs, CacheItemReference rhs)
			{
				if (lhs.CompareTo(rhs) > 0)
				{
					return true;
				}
				return false;
			}

			public static bool operator <(CacheItemReference lhs, CacheItemReference rhs)
			{
				if (lhs.CompareTo(rhs) < 0)
				{
					return true;
				}
				return false;
			}
		}
	}
}
