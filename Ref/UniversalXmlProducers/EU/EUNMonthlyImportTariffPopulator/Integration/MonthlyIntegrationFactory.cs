using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public class MonthlyIntegrationFactory : IMonthlyIntegrationFactory
	{
		public IMonthlyConfigProvider GetConfigProvider(string xmlDistributionUrl,
			string importTariffUXmlFile,
			string[] filter_GoodsNomenclatures,
			string[] filter_MeasureTypes,
			string[] filter_GeographicalAreaIds,
			string measureTypeLink_alternative,
			string measureLink_alternative,
			string measureConditionCode_alternative,
			string declarableGoodsNomenclatureLink_alternative,
			string regulationLink_alternative,
			string publicationTime_alternative,
			string downloadFolder_alternative)
		{
			return new ConfigProvider(xmlDistributionUrl, importTariffUXmlFile, filter_GoodsNomenclatures, filter_MeasureTypes, filter_GeographicalAreaIds, measureTypeLink_alternative, measureLink_alternative, measureConditionCode_alternative, declarableGoodsNomenclatureLink_alternative, regulationLink_alternative, publicationTime_alternative, downloadFolder_alternative);
		}

		public IMonthlyDataProvider GetDataProvider(IMonthlyConfigProvider configProvider, IHttpClientHelper httpClientHelper)
		{
			return monthlyDataProvider ?? (monthlyDataProvider = new MonthlyDataProvider(configProvider, httpClientHelper, configProvider.DownloadFolder_alternative));
		}
		IMonthlyDataProvider monthlyDataProvider;
	}
}
