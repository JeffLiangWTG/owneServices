using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public class DailyIntegrationHelper : IDailyIntegrationHelper
	{
		readonly IMonthlyIntegrationFactory monthlyIntegrationFactory;
		readonly IHttpClientHelper httpClientHelper;
		readonly IDailyConfigProvider dailyConfigProvider;

		public DailyIntegrationHelper(IMonthlyIntegrationFactory monthlyIntegrationFactory, IDailyConfigProvider dailyConfigProvider, IHttpClientHelper httpClientHelper)
		{
			this.monthlyIntegrationFactory = monthlyIntegrationFactory;
			this.httpClientHelper = httpClientHelper;
			this.dailyConfigProvider = dailyConfigProvider;
		}

		public IMonthlyConfigProvider GetMonthlyConfigProvider()
		{
			return monthlyConfigProvider ?? (monthlyConfigProvider = monthlyIntegrationFactory.GetConfigProvider(dailyConfigProvider.MonthlyXMLDistributionUrl, string.Empty, dailyConfigProvider.Filter_GoodsNomenclatures, dailyConfigProvider.Filter_MeasureTypes, dailyConfigProvider.Filter_GeographicalAreaIds, dailyConfigProvider.MeasureTypeLink_alternative, dailyConfigProvider.MeasureLink_alternative, dailyConfigProvider.MeasureConditionCode_alternative, dailyConfigProvider.DeclarableGoodsNomenclatureLink_alternative, dailyConfigProvider.RegulationLink_alternative, dailyConfigProvider.PublicationTime_alternative, dailyConfigProvider.DownloadFolder_alternative));
		}
		IMonthlyConfigProvider monthlyConfigProvider;

		public IMonthlyDataProvider GetMonthlyDataProvider()
		{
			return monthlyDataProvider ?? (monthlyDataProvider = monthlyIntegrationFactory.GetDataProvider(GetMonthlyConfigProvider(), httpClientHelper));
		}
		IMonthlyDataProvider monthlyDataProvider;
	}
}
