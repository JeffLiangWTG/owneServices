using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.Rule
{
	public static class ApplicationConfig
	{
		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new ConfigurationBuilder().AddJsonFile(JsonConfigFile).Build();
				}
				return config;
			}
		}

		const string JsonConfigFile = "CargoWise.RefDbRepo.Staging.RuleRunner.config.json";
		static IConfiguration config;

		public static string SafeUpdateServiceUri => Config[nameof(SafeUpdateServiceUri)];
		public static string TenantId => Config[nameof(TenantId)];
		public static string ClientId => Config[nameof(ClientId)];
		public static string ServiceId => Config[nameof(ServiceId)];
		public static string PrivateKeyFileName => Config[nameof(PrivateKeyFileName)];
		public static string CertificateFileName => Config[nameof(CertificateFileName)];
		public static int RefreshAdvanceInMinutes =>  int.Parse(Config[nameof(RefreshAdvanceInMinutes)], CultureInfo.InvariantCulture);
	}
}
