using System.IO;
using System.Runtime.Caching;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.NewService
{
	public class FileCacheWrapper : IFileCacheWrapper
	{
		public FileCacheWrapper(ILogHelper logHelper, IUserService userService, IFileCacheProvider fileCacheProvider)
		{
			Argument.NotNull(logHelper, nameof(logHelper));
			Argument.NotNull(userService, nameof(userService));
			this.logHelper = logHelper;
			this.userService = userService;
			cacheProvider = fileCacheProvider;
		}
		readonly ILogHelper logHelper;
		readonly IUserService userService;
		readonly IFileCacheProvider cacheProvider;

		public string GetCachePath(string key)
		{
			var cacheFilePath = cacheProvider.GetCachePath(key);
			if (!string.IsNullOrEmpty(cacheFilePath) && File.Exists(cacheFilePath))
			{
				var fileSize = new FileInfo(cacheFilePath).Length;
				logHelper.LogInfo(userService.GetUserId(), $"Cache file exists, key: {key}, path: {cacheFilePath}, size: {fileSize} bytes");
			}
			else
			{
				logHelper.LogInfo(userService.GetUserId(), $"Cache file does not exist, key: {key}.");
			}
			return cacheFilePath;
		}

		public void Add(string key, object value, string regionName = null)
		{
			logHelper.LogInfo(userService.GetUserId(), $"Adding {regionName ?? "data"} cache. Key: {key}");
			cacheProvider.Add(key, value, regionName);
		}

		public bool Contains(string key, string regionName = null)
		{
			return cacheProvider.Contains(key, regionName);
		}

		public object Get(string key, string regionName = null)
		{
			logHelper.LogInfo(userService.GetUserId(), $"Getting {regionName ?? "data"} cache. Key: {key}");
			return cacheProvider.Get(key, regionName);
		}

		public object Remove(string key, string regionName = null)
		{
			logHelper.LogInfo(userService.GetUserId(), $"Removing {regionName ?? "data"} cache. Key: {key}");
			return cacheProvider.Remove(key, regionName);
		}

		public void ShrinkCacheSize()
		{
			cacheProvider.ShrinkCacheSizeAsync();
		}

#if DEBUG
		public CacheItemPolicy GetCacheItemPolicy(string key, string regionName = null)
		{
			return cacheProvider.GetPolicy(key, regionName);
		}
#endif
	}
}
