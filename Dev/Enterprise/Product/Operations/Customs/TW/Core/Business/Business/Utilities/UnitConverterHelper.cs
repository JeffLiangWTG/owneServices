using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public static class UnitConverterHelper
	{
		readonly static Dictionary<string, string> CustomsUnitToWeightUnitDict = new Dictionary<string, string>
		{
			{ Constants.UnitOfQuantityCodes.Kilograms, Core.Constants.Weight.Kilograms },
			{ Constants.UnitOfQuantityCodes.Tonnes, Core.Constants.Weight.Tonnes },
			{ Constants.UnitOfQuantityCodes.Gram, Core.Constants.Weight.Grams },
			{ Constants.UnitOfQuantityCodes.Pound, Core.Constants.Weight.Pounds }
		};

		public static string ConvertToCW1StandardWeightUnit(string customsUnit)
		{
			return CustomsUnitToWeightUnitDict.TryGetValue(customsUnit, out var mappedWeightCode) ? mappedWeightCode : customsUnit;
		}

		public static bool CanConvertFromNetWeightToCustomsUnit(ZDecimal netWeight, ZString netWeightUQ, ZString customsUnit)
		{
			return netWeight > 0m && Core.Constants.Weight.ContainsCode(netWeightUQ) && Core.Constants.Weight.ContainsCode(ConvertToCW1StandardWeightUnit(customsUnit));
		}
	}
}
