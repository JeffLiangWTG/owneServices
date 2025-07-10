using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public static class ApplicationConfig
	{
		static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonConfigFileName)
						.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		const string JsonConfigFileName = "CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator.config.json";

		public static string DailyXMLDistributionUrl => Configuration[nameof(DailyXMLDistributionUrl)];

		public static string DailyImportTariffUXmlFile => Configuration[nameof(DailyImportTariffUXmlFile)];

		public static string DailyPublicationTime_alternative => Configuration[nameof(DailyPublicationTime_alternative)];
		public static string DownloadFolder_alternative => Configuration[nameof(DownloadFolder_alternative)];

		public static string MonthlyXMLDistributionUrl => Configuration[nameof(MonthlyXMLDistributionUrl)];
		public static string Filter_GoodsNomenclatures => Configuration[nameof(Filter_GoodsNomenclatures)];
		public static string Filter_MeasureTypes => Configuration[nameof(Filter_MeasureTypes)];
		public static string Filter_GeographicalAreaIds => Configuration[nameof(Filter_GeographicalAreaIds)];
		public static string MeasureTypeLink_alternative => Configuration[nameof(MeasureTypeLink_alternative)];

		public static string MeasureLink_alternative => Configuration[nameof(MeasureLink_alternative)];

		public static string MeasureConditionCode_alternative => Configuration[nameof(MeasureConditionCode_alternative)];
		public static string DeclarableGoodsNomenclatureLink_alternative => Configuration[nameof(DeclarableGoodsNomenclatureLink_alternative)];
		public static string RegulationLink_alternative => Configuration[nameof(RegulationLink_alternative)];
		public static string PublicationTime_alternative => Configuration[nameof(PublicationTime_alternative)];
	}
}
