using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using CacheTower;
using CacheTower.Providers.FileSystem;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.NewService;

public class CacheTowerProvider : ICacheTowerProvider
{
	public CacheTowerProvider(ILogHelper logHelper, FileCacheLayer fileCacheLayer)
	{
		Argument.NotNull(logHelper, nameof(logHelper));
		Argument.NotNull(fileCacheLayer, nameof(fileCacheLayer));
		this.logHelper = logHelper;
		this.fileCacheLayer = fileCacheLayer;

		expirationInterval =
			TimeSpan.FromHours(int.Parse(ApplicationConfig.CacheExpirationInHours, CultureInfo.InvariantCulture));

		cache = new CacheStack(null, new CacheStackOptions(this.fileCacheLayer));

		var formattedHostName = Dns.GetHostName() + ": ";
		logHelper.LogInfo(null, $"{formattedHostName}FileCache is created at UtcTime {DateTime.UtcNow}.");
		CleanExpiredCache();
	}

	readonly ILogHelper logHelper;
	readonly ICacheStack cache;
	readonly FileCacheLayer fileCacheLayer;
	readonly TimeSpan expirationInterval;

	public async Task Add(string key, object value)
	{
		await cache.SetAsync(key, value, expirationInterval);
		await fileCacheLayer.SaveManifestAsync();
	}

	public async Task<T> Get<T>(string key)
	{
		var result = await cache.GetAsync<T>(key);
		return result == null ? default : result.Value;
	}

	public async Task Remove(string key)
	{
		await cache.EvictAsync(key);
	}

	public void CleanExpiredCache()
	{
		Task.Run(() => cache.CleanupAsync());
	}

	public async Task InitializeManifestAsync()
	{
		// will remove it after cacheTower lib update to prerelease version.
		await cache.SetAsync("PersistentKey",
			"it's used to make sure manifest content should not be 'null'",
			TimeSpan.FromDays(365));
		await fileCacheLayer.SaveManifestAsync();
	}
}
