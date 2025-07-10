using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class DbConnectionStringManager
	{
		public static string StagingConnectionString => Config.GetConnectionString(nameof(ConnectionStringOption.RefDbRepoStagingConnString));
		public static string SafeConnectionString => Config.GetConnectionString(nameof(ConnectionStringOption.RefDbRepoSafeConnString));
		public static string SafeReadOnlyConnectionString => Config.GetConnectionString(nameof(ConnectionStringOption.RefDbRepoReadOnlySafeConnString));

		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					LoadConfiguration();
				}
				return config;
			}
		}

		static void LoadConfiguration()
		{
			IConfigurationBuilder builder = new ConfigurationBuilder();
			builder = builder.AddJsonFile(jsonConfigFile);
			config = builder.Build();
		}

		static string jsonConfigFile = "ConnectionStrings.config.json";
		static IConfiguration config;

		enum ConnectionStringOption
		{
			RefDbRepoSafeConnString,
			RefDbRepoStagingConnString,
			RefDbRepoReadOnlySafeConnString
		}

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
			config = null;
		}
#endif
	}
}
