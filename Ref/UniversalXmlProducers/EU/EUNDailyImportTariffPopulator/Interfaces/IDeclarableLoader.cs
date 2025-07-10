using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public interface IDeclarableLoader
	{
		ITraderObjectLoader GetTraderObjectLoader(string link, string tmpPath);
	}
}
