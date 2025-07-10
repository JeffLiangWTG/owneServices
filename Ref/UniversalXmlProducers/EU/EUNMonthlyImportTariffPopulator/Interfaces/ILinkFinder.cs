using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public interface ILinkFinder
	{
		string GetMeasureTypeLink();
		string GetMeasureLink();
		string GetGoodsNomenclatureLink();
		string GetDeclarableGoodsNomenclatureLink();
		string GetRegulationLink();
		string GetMeasureConditionCodeLink();
		DateTime GetPublicationTime();
	}
}
