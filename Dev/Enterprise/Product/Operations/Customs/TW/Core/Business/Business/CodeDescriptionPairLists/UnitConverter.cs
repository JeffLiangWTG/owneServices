using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TW.Business.Constants;

namespace Enterprise.Customs.TW.Business.CodeDescriptionPairLists
{
	static class UnitConverter
	{
		static decimal GetBaseUnit(ZString unit)
		{
			if (unit == UnitOfQuantityCodes.Kilograms || unit == UnitOfQuantityCodes.LTR)
			{
				return 1m;
			}
			else if (unit == UnitOfQuantityCodes.Tonnes || unit == UnitOfQuantityCodes.KLT)
			{
				return 1000m;
			}
			return 0m;
		}

		public static (decimal value, ZString unit) TryConvertToInterchangableUnit(string sourceUnit, decimal sourceValue)
		{
			var targetUnit = GetInterchangeableUnit(sourceUnit);
			if (!targetUnit.IsEmpty)
			{
				decimal sourceBaseValue = GetBaseUnit(sourceUnit);
				decimal targetBaseValue = GetBaseUnit(targetUnit);
				decimal result = 0m;
				if (targetBaseValue != 0m)
				{
					result = Utilities.Round(sourceValue / targetBaseValue * sourceBaseValue, 3);
				}
				return (result, targetUnit);
			}
			else
			{
				return (0m, targetUnit);
			}
		}

		public static ZString GetInterchangeableUnit(ZString unit)
		{
			if (!unit.IsEmpty)
			{
				if (unit == UnitOfQuantityCodes.Tonnes)
				{
					return UnitOfQuantityCodes.Kilograms;
				}
				else if (unit == UnitOfQuantityCodes.Kilograms)
				{
					return UnitOfQuantityCodes.Tonnes;
				}
				else if (unit == UnitOfQuantityCodes.KLT)
				{
					return UnitOfQuantityCodes.LTR;
				}
				else if (unit == UnitOfQuantityCodes.LTR)
				{
					return UnitOfQuantityCodes.KLT;
				}
			}
			return ZString.Empty;
		}
	}
}
