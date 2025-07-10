using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	internal class DeclarableLoader : IDeclarableLoader
	{
		public ITraderObjectLoader GetTraderObjectLoader(string link, string tmpPath)
		{
			return new TraderObjectLoader(link, tmpPath);
		}
	}
}
