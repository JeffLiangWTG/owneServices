using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public interface IDailyIntegrationHelper
	{
		IMonthlyDataProvider GetMonthlyDataProvider();
		IMonthlyConfigProvider GetMonthlyConfigProvider();
	}
}
