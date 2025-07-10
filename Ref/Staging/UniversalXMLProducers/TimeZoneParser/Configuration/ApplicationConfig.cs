using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser
{
	public static class ApplicationConfig
	{
		public const string TimeZoneDownloadUrl = @"https://nodatime.org/tzdb/";
		public const string TimeZoneDataSource = "Timezone Data";

		public static string LatestTimeZoneDbLink => Config["LatestTimeZoneDB"];
		public static string LocalDownloadFolder => Config["DownloadsFolder"];
		public static string TimeZoneXMLFile => Config["TimeZoneXMLFile"];

		static IConfiguration Config =>
			new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser.config.json").Build();
	}
}
