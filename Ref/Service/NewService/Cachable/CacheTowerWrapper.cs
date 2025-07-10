using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.NewService;

public class CacheTowerWrapper : ICacheTowerWrapper
{
	public CacheTowerWrapper(ILogHelper logHelper, IUserService userService, ICacheTowerProvider cacheTowerProvider)
	{
		Argument.NotNull(logHelper, nameof(logHelper));
		Argument.NotNull(userService, nameof(userService));
		this.logHelper = logHelper;
		this.userService = userService;
		cacheProvider = cacheTowerProvider;
	}
	readonly ILogHelper logHelper;
	readonly IUserService userService;
	readonly ICacheTowerProvider cacheProvider;

	public async Task Add(string key, object value, string regionName = null)
	{
		Argument.NotNullOrEmpty(key, nameof(key));
		logHelper.LogInfo(userService.GetUserId(), $"Adding {regionName ?? "data"} cache. Key: {key}");
		await cacheProvider.Add(GetActualKey(key, regionName), value);
	}

	public async Task<T> Get<T>(string key, string regionName = null)
	{
		Argument.NotNullOrEmpty(key, nameof(key));
		logHelper.LogInfo(userService.GetUserId(), $"Getting {regionName ?? "data"} cache. Key: {key}");
		return await cacheProvider.Get<T>(GetActualKey(key, regionName));
	}

	public async Task Remove(string key, string regionName = null)
	{
		Argument.NotNullOrEmpty(key, nameof(key));
		logHelper.LogInfo(userService.GetUserId(), $"Removing {regionName ?? "data"} cache. Key: {key}");
		await cacheProvider.Remove(GetActualKey(key, regionName));
	}

	static string GetActualKey(string key, string regionName) => $"{key}{regionName}";
}
