using System.Globalization;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public static class ConfigurationProvider
	{
		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configurationBuilder.AddJsonFile(JsonConfigFile);
					configuration = configurationBuilder.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static string OIDCAuthority => Configuration[nameof(OIDCAuthority)];
		public static string OIDCAudience => Configuration[nameof(OIDCAudience)];
		public static string SafeUpdateServiceUri => Configuration[nameof(SafeUpdateServiceUri)];
		public static string TenantId => Configuration[nameof(TenantId)];
		public static string ClientId => Configuration[nameof(ClientId)];
		public static string ServiceId => Configuration[nameof(ServiceId)];
		public static string PrivateKeyFileName => Configuration[nameof(PrivateKeyFileName)];
		public static string CertificateFileName => Configuration[nameof(CertificateFileName)];
		public static int RefreshAdvanceInMinutes => int.Parse(Configuration[nameof(RefreshAdvanceInMinutes)], CultureInfo.InvariantCulture);
		public static string StagingConnectionString => DbConnectionStringManager.StagingConnectionString;
		public static string CipherPublicKeyName => Configuration[nameof(CipherPublicKeyName)];
		public static string CipherPrivateKeyName => Configuration[nameof(CipherPrivateKeyName)];
		public static string AesKeyName => Configuration[nameof(AesKeyName)];
		public static IConfigurationSection MemoryUsageLogging => Configuration.GetSection("MemoryUsageLogging");

		const string JsonConfigFile = "CargoWise.RefDbRepo.Staging.NewService.config.json";
	}
}
