using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using WTG.IdentitySecurity;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.Tests")]

namespace CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.CmdLine
{
    public static class AppConfig
    {
		internal static void OverrideConfig(IConfiguration newConfig)
		{
			config = newConfig;
		}

		public const string JsonConfigFile = "CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.CmdLine.config.json";
		static IConfiguration config;
		public static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					config = configurationBuilder.AddJsonFile(JsonConfigFile).Build();
				}
				return config;
			}
		}
		public static string OutputPath => Config["OutputPath"];
		public static string TokenEndpoint => $"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";
		public static string TenantId => Config["TenantId"];
		public static string Azp => Config["Azp"];
		public static string Aud => Config["Aud"];
		public static string LMSSiteUrl => Config["LMSSiteUrl"];
		public static X509Certificate2 Certificate => new(Config["CertificatePath"]);
		public static RSA PrivateKey => RSAKeyProvider.ImportPrivateKey(File.ReadAllText(Config["PrivateKeyPath"]));
	}
}
