using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.DummyNet6Proj
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
		static string jsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.DummyNet6Proj.config.json";
		static IConfiguration config;

		public static string DeliveryServiceBaseUrl => Config[nameof(DeliveryServiceBaseUrl)];
		public static string TenantId => Config[nameof(TenantId)];
		public static string ClientId => Config[nameof(ClientId)];
		public static string DeliveryServiceClientId => Config[nameof(DeliveryServiceClientId)];
		public static string TestRetryUrl => Config[nameof(TestRetryUrl)];
		public static string ConfigFromRepoForTest => Config[nameof(ConfigFromRepoForTest)];
		public static string PrivateKeyFileName => Config[nameof(PrivateKeyFileName)];
		public static string CertificateFileName => Config[nameof(CertificateFileName)];
		public static int RefreshAdvanceInMinutes =>  int.Parse(Config[nameof(RefreshAdvanceInMinutes)], CultureInfo.InvariantCulture);
		public static string DummyPassword => Config[nameof(DummyPassword)];
		public static string DummyKeyFile => Config[nameof(DummyKeyFile)];

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
			config = null;
		}
#endif
	}
}
