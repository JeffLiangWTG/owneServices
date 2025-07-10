using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public interface IMonthlyIntegrationFactory
	{
		IMonthlyConfigProvider GetConfigProvider(string xmlDistributionUrl,
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
			string downloadFolder_alternative);

		IMonthlyDataProvider GetDataProvider(IMonthlyConfigProvider configProvider, IHttpClientHelper httpClientHelper);
	}
}
