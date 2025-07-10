using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public interface IMonthlyDataProvider : IDataProvider
	{
		IEnumerable<measureType1> GetMeasureTypes(DateTime publicationDate);
		string[] GetRegulationIds(DateTime publicationDate);
		IEnumerable<SEReferenceData.Services.measureConditionCode> GetMeasureConditionCodes(DateTime publicationDate);
		SEReferenceData.Services.goodsNomenclature[] GetGoodsNomenclatures(DateTime publicationDate);
		IEnumerable<declarableGoodsNomenclature> GetDeclarableGoodsNomenclature(DateTime publicationDate);
		void SetRelatedDataForImportTariffs(IEnumerable<RefCusTariff> importTariffs, DateTime publicationDate);
	}
}
