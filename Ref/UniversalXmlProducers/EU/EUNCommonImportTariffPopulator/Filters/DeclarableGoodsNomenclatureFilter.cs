using System;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class DeclarableGoodsNomenclatureFilter : IXmlFilter<declarableGoodsNomenclature>
	{
		public DeclarableGoodsNomenclatureFilter(string[] filter_GoodsNomenclatures)
		{
			this.filter_GoodsNomenclatures = filter_GoodsNomenclatures;
		}
		readonly string[] filter_GoodsNomenclatures;

		public declarableGoodsNomenclature GetValidValue(declarableGoodsNomenclature value)
		{
			return value;
		}

		public bool IsValid(declarableGoodsNomenclature value, DateTime publicationDate)
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
