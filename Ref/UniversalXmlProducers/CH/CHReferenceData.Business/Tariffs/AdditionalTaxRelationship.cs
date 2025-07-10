using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	internal class AdditionalTaxRelationship
	{
		internal string commodityCode;
		internal int statisticalCode;
		internal bool isOptional;
		internal int groupControl;
		internal int tradeGroup;
		internal List<int> excludedTradeGroups = new List<int>();

		internal AdditionalTaxRelationship()
		{
		}

		internal AdditionalTaxRelationship(AdditionalTaxRelationship source)
		{
			commodityCode = source.commodityCode;
			statisticalCode = source.statisticalCode;
			isOptional = source.isOptional;
			groupControl = source.groupControl;
			tradeGroup = source.tradeGroup;
			excludedTradeGroups.AddRange(source.excludedTradeGroups);
		}
	}
}
