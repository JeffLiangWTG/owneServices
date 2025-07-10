using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.MXReferenceData.Services
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
					configurationBuilder.AddJsonFile("CargoWise.RefDbRepo.MXReferenceData.CmdLine.config.json");
					configuration = configurationBuilder.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static string OutputFolder => ConfigurationProvider.Configuration.GetSection("OutputFolder").Value;

		public static class ExchangeRate
		{
			public static string Host => ConfigurationProvider.Configuration.GetSection("EXCHANGE_RATE_FTP_HOST").Value;

			public static string UserName => ConfigurationProvider.Configuration.GetSection("EXCHANGE_RATE_FTP_USER").Value;

			public static string Password => ConfigurationProvider.Configuration.GetSection("EXCHANGE_RATE_FTP_PASWORD").Value;

			public static string DownloadPath => ConfigurationProvider.Configuration.GetSection("EXCHANGE_RATE_FTP_DOWNLOAD_PATH").Value;

			public static IEnumerable<string> FileNamesToDownload => ConfigurationProvider.Configuration.GetSection("EXCHANGE_RATE_FTP_FILES_TO_DOWNLOAD").Value.Split(',');

			public static IEnumerable<string> ListFilesToDownload => FileNamesToDownload.Select(fileName => DownloadPath + fileName);
		}

		public static class TariffRate
		{
			public static string Host => ConfigurationProvider.Configuration.GetSection("TARIFF_RATE_FTP_HOST").Value;

			public static string UserName => ConfigurationProvider.Configuration.GetSection("TARIFF_RATE_FTP_USER").Value;

			public static string Password => ConfigurationProvider.Configuration.GetSection("TARIFF_RATE_FTP_PASWORD").Value;

			public static string DownloadPath => ConfigurationProvider.Configuration.GetSection("TARIFF_RATE_FTP_DOWNLOAD_PATH").Value;

			public static IEnumerable<string> FileNamesToDownload => ConfigurationProvider.Configuration.GetSection("TARIFF_RATE_FTP_FILES_TO_DOWNLOAD").Value.Split(',');

			public static IEnumerable<string> ListFilesToDownload => FileNamesToDownload.Select(fileName => DownloadPath + fileName);
		}

		public static class CustomsFacilities
		{
			public static string DownloadPath => ConfigurationProvider.configuration.GetSection("CUSTOMS_SECTION_URL").Value;
		}
	}
}
