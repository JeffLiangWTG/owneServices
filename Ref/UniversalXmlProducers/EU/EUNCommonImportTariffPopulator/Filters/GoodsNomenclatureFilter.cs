using System;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class GoodsNomenclatureFilter : IXmlFilter<goodsNomenclature>
	{
		public GoodsNomenclatureFilter(string[] filter_GoodsNomenclatures)
		{
			this.filter_GoodsNomenclatures = filter_GoodsNomenclatures;
		}
		readonly string[] filter_GoodsNomenclatures;

		public goodsNomenclature GetValidValue(goodsNomenclature value)
		{
			return value;
		}

		public bool IsValid(goodsNomenclature value, DateTime publicationDate)
		{
			var result = !value.dateEndSpecified || value.dateEnd >= publicationDate;
			if (result && filter_GoodsNomenclatures.Length > 0)
			{
				result = filter_GoodsNomenclatures.Any(x => value.IsBelongTo(x));
			}
			return result;
		}
	}
}
