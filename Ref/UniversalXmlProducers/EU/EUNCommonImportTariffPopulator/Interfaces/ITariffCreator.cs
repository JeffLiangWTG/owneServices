using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface ITariffCreator
	{
		RefCusTariff Create(declarableGoodsNomenclature declarable, goodsNomenclature[] goodsNomenclatures);
		IEnumerable<RefCusTariff> Create(measure measure, IEnumerable<declarableGoodsNomenclature> declarableGoodsNomenclatures, goodsNomenclature[] goodsNomenclatures);
	}
}
