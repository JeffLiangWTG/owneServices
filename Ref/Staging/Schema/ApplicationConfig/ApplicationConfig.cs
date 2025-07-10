using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.ApplicationConfig
{
	public static class ApplicationConfig
	{
		static IConfiguration Config
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
		static string jsonConfigFile = "CargoWise.RefDbRepo.Staging.ApplicationConfig.config.json";
		static IConfiguration config;

		public static string CipherPublicKeyName => Config[nameof(CipherPublicKeyName)];
		public static string CipherPrivateKeyName => Config[nameof(CipherPrivateKeyName)];
		public static string AesKeyName => Config[nameof(AesKeyName)];

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
			config = null;
		}
#endif
	}
}
