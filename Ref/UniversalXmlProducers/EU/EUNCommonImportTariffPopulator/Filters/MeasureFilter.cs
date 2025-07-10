using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class MeasureFilter : IXmlFilter<measure>
	{
		public MeasureFilter(string[] filter_GoodsNomenclatures, string[] filter_MeasureTypes, string[] filter_GeographicalAreaIds, IEnumerable<string> regulationIds)
		{
			this.filter_GoodsNomenclatures = filter_GoodsNomenclatures;
			this.filter_MeasureTypes = filter_MeasureTypes;
			this.filter_GeographicalAreaIds = filter_GeographicalAreaIds;
			this.regulationIds = regulationIds;
		}
		readonly IEnumerable<string> regulationIds;
		readonly string[] filter_GoodsNomenclatures;
		readonly string[] filter_MeasureTypes;
		readonly string[] filter_GeographicalAreaIds;

		public measure GetValidValue(measure value)
		{
			value.goodsNomenclatureCode = value.goodsNomenclatureCode.TrimEnd("00");
			value.measureComponent = (value.measureComponent ?? Array.Empty<measureComponent>()).Where(x => x.nationalSpecified && x.national == 0L).ToArray();
			value.measureCondition = (value.measureCondition ?? Array.Empty<measureCondition>()).Where(x => x.nationalSpecified && x.national == 0L).ToArray();
			foreach (var cond in value.measureCondition)
			{
				cond.measureConditionComponent = (cond.measureConditionComponent ?? Array.Empty<measureConditionComponent>())
					.Where(x => x.nationalSpecified && x.national == 0L).ToArray();
			}
			value.measureExcludedGeographicalArea = (value.measureExcludedGeographicalArea ?? Array.Empty<measureExcludedGeographicalArea>())
				.Where(x => x.nationalSpecified && x.national == 0L).ToArray();
			return value;
		}

		public bool IsValid(measure value, DateTime publicationDate)
		{
			var result = value.nationalSpecified && value.national == 0L && (!value.dateEndSpecified || value.dateEnd >= publicationDate)
				&& !string.IsNullOrEmpty(value.goodsNomenclatureCode) && regulationIds.Any(x => x == value.regulationId);
			if (result && filter_GoodsNomenclatures.Length > 0)
			{
				result = filter_GoodsNomenclatures.Any(x => value.goodsNomenclatureCode.StartsWith(x, StringComparison.InvariantCultureIgnoreCase) || x.StartsWith(value.goodsNomenclatureCode.TrimEnd("00"), StringComparison.InvariantCultureIgnoreCase));
			}
			if (result && filter_MeasureTypes.Length > 0)
			{
				result = filter_MeasureTypes.Any(x => value.measureType.Equals(x, StringComparison.OrdinalIgnoreCase));
			}
			if (result && filter_GeographicalAreaIds.Length > 0)
			{
				result = filter_GeographicalAreaIds.Any(x => value.geographicalAreaId.Equals(x, StringComparison.OrdinalIgnoreCase));
			}
			return result;
		}
	}
}
