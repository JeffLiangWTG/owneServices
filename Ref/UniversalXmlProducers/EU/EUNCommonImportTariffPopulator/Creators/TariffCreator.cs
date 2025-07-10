using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class TariffCreator : ITariffCreator
	{
		public RefCusTariff Create(declarableGoodsNomenclature declarable, goodsNomenclature[] goodsNomenclatures)
		{
			RefCusTariff result = null;
			if (!string.IsNullOrEmpty(declarable.goodsNomenclatureCode))
			{
				var goodsNomenclatureForDescriptions = Array.FindAll(goodsNomenclatures, x => x.goodsNomenclatureCode.TrimEnd("00") == declarable.goodsNomenclatureCode.TrimEnd("00"));
				if(goodsNomenclatureForDescriptions is null)
				{
					throw new NotSupportedException($"Description not found for this TariffCode: {declarable.goodsNomenclatureCode}");
				}
				result = new RefCusTariff
				{
					ZZ1_TariffCode = declarable.goodsNomenclatureCode,
					ZZ1_StartDate = declarable.dateStartSpecified ? declarable.dateStart : new DateTime(1900, 01, 01),
					ZZ1_EndDate = declarable.dateEndSpecified ? declarable.dateEnd : new DateTime(2079, 06, 06, 23, 59, 0),
					ZZ1_ZZI_NKTariffType = GetTariffType(declarable.type),
					ZZ1_Description = GetDescription(goodsNomenclatureForDescriptions)
				};
			}
			return result;
		}

		public IEnumerable<RefCusTariff> Create(measure measure, IEnumerable<declarableGoodsNomenclature> declarableGoodsNomenclatures, goodsNomenclature[] goodsNomenclatures)
		{
			if (!string.IsNullOrEmpty(measure.goodsNomenclatureCode))
			{
				var declarables = declarableGoodsNomenclatures.Where(x => x.goodsNomenclatureCode.StartsWith(measure.goodsNomenclatureCode, StringComparison.InvariantCultureIgnoreCase));
				foreach (var declarable in declarables)
				{
					yield return Create(declarable, goodsNomenclatures);
				}
			}
		}

		static string GetTariffType(string type)
		{
			switch (type)
			{
				case "I":
					return "IMP";
				case "E":
					return "EXP";
				default:
					throw new NotImplementedException($"Unknown declarable type:{type}");
			}
		}

		static string GetDescription(goodsNomenclature[] goodsNomenclature)
		{
			return goodsNomenclature.FirstOrDefault(x => x.productLineSuffix == "80")
				.goodsNomenclatureDescriptionPeriod.OrderByDescending(x => x.dateStart).FirstOrDefault()
				.goodsNomenclatureDescription.FirstOrDefault(x => string.Equals(x.languageId, "EN", StringComparison.OrdinalIgnoreCase))
				.description;
		}
	}
}
