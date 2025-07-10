using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public interface IDailyHelper
	{
		IEnumerable<measure> GetMeasures(IDailyConfigProvider dailyConfigProvider, ITraderObjectLoader dailyMeasureLoader, string[] regulationIds, DateTime publicationDate);
		string[] GetRegulations(IEnumerable<ITraderObjectLoader> measureLoaders, string[] regulationsIds, DateTime publicationDate);
		IEnumerable<RefCusTariff> GetTariffsFromMeasureLoaders(IEnumerable<ITraderObjectLoader> measureLoaders, IEnumerable<EUNCommonImportTariffPopulator.measureType1> measureTypes, goodsNomenclature[] goodsNomenclatures, IEnumerable<EUNCommonImportTariffPopulator.declarableGoodsNomenclature> monthlyDeclarableGoodsNomenclature, IDailyConfigProvider dailyConfigProvider, string[] regulationIds, DateTime publicationDate, List<RefCusTariff> dailyTariffList);
		IEnumerable<RefCusTariff> GetTariffsFromDeclarableGoodsNomenclatureLinks(IEnumerable<string> declarableGoodsNomenclatureLinks, goodsNomenclature[] goodsNomenclatures, IDeclarableLoader declarableLoader, IDailyConfigProvider dailyConfigProvider, DateTime publicationDate, string temporaryDownloadPath, List<RefCusTariff> dailyTariffList);
		IEnumerable<goodsNomenclature> GetGoodsNomenclatures(IEnumerable<ITraderObjectLoader> measureLoaders, IDailyConfigProvider dailyConfigProvider, goodsNomenclature[] monthlyGoodsNomenclatures, DateTime publicationDate);
	}
}
