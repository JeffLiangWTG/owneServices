using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUtcOffsetUpdater
{
	public static class ApplicationConfig
	{
		public const string DataSourceName = "UnlocoUtcOffset";
		public const string OutputFilename = "UnlocoUtcOffset.xml";
		public const string TimeZoneDownloadUrl = @"https://nodatime.org/tzdb/";

		public static string RequiredOffsetPeriodInMonths => Config["RequiredOffsetPeriodInMonths"];
		public static string SafeDataUpdateUri => Config["SafeDataUpdateUri"];
		public static string OutputFilePath => Config["OutputFilePath"];
		public static string LatestTimeZoneDbLink => Config["LatestTimeZoneDB"];
		public static string LocalDownloadFolder => Config["DownloadsFolder"];

		static IConfiguration Config =>
			new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUtcOffsetUpdater.config.json").Build();
	}
}
