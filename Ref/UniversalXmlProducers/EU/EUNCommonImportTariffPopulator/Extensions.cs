using System;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public static class Extensions
	{
		public static bool IsApplicable(this measure measure, string declarableCode)
		{
			return declarableCode.StartsWith(measure.goodsNomenclatureCode, StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool IsBelongTo(this declarableGoodsNomenclature declarable, string goodsNomenclature)
		{
			return declarable.goodsNomenclatureCode.StartsWith(goodsNomenclature, StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool IsBelongTo(this goodsNomenclature goodsNomenclatureRecord, string goodsNomenclature)
		{
			return goodsNomenclatureRecord.goodsNomenclatureCode.StartsWith(goodsNomenclature, StringComparison.InvariantCultureIgnoreCase);
		}

		public static string TrimEnd(this string goodsNomenclatureCode, string subString)
		{
			var result = goodsNomenclatureCode;
			while (result.EndsWith(subString, StringComparison.InvariantCultureIgnoreCase))
			{
				result = result.Substring(0, result.Length - subString.Length);
			}
			return result;
		}
	}
}
