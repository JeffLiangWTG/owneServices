using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.NewService
{
	public static class ApplicationConfig
	{
		public static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new ConfigurationBuilder().AddJsonFile(jsonConfigFile).Build();
				}
				return config;
			}
		}

		static string jsonConfigFile = "CargoWise.RefDbRepo.NewService.config.json";
		static IConfiguration config;

		public static string S2SAuthority => Config[nameof(S2SAuthority)];
		public static string S2SAudience => Config[nameof(S2SAudience)];
		public static string PortalUri => Config[nameof(PortalUri)];
		public static bool DisableAuthentication
		{
			get
			{
#if DEBUG
				if (bool.TryParse(Config["DEBUG_DisableAuthentication"], out var disableAuthentication))
				{
					return disableAuthentication;
				}
#endif
				return false;
			}
		}
		public static string GetMergeTimeout(string key) => Config[$"{key}_MergeTimeout"];
		public static string CommandTimeout => Config[nameof(CommandTimeout)];
		public static string RawResponseMaxSize => Config[nameof(RawResponseMaxSize)];
		public static string EnableRawResponse => Config[nameof(EnableRawResponse)];
		public static IConfigurationSection MemoryUsageLogging => Config.GetSection("MemoryUsageLogging");

		#region cache

		public static string CacheFolder => Config["Cache.Folder"];
		public static string CacheMaxSize => Config["Cache.MaxSize"];
		public static string CacheExpirationInHours => Config["Cache.ExpirationInHours"];
		public static string CacheCleanIntervalInHours => Config["Cache.CleanIntervalInHours"];
		public static string CacheSizeSyncIntervalInMinutes => Config["Cache.CacheSizeSyncIntervalInMinutes"];
		public static string CacheSizeSyncBeginPercentage => Config["Cache.CacheSizeSyncBeginPercentage"];

		#endregion

		#region connection string

		public static string ZZDbServerEntities => DbConnectionStringManager.SafeConnectionString;
		public static string ZZReadOnlyDbServerEntities => DbConnectionStringManager.SafeReadOnlyConnectionString;
		public static string StagingConnectionString => DbConnectionStringManager.StagingConnectionString;

		#endregion

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
			config = null;
		}
#endif
	}
}
