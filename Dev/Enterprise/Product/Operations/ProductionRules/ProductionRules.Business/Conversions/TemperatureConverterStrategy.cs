using System.Linq;

namespace Enterprise.ProductionRules.Business
{
	class TemperatureConverterStrategy : IUnitConverterStrategy
	{
		public decimal Convert(decimal value, string fromUnit, string toUnit)
		{
			var convertedValue = 0m;

			if (IsValidUnit(fromUnit) && IsValidUnit(toUnit))
			{
				convertedValue = Core.Constants.Temperature.Convert(value, fromUnit, toUnit);
			}

			return convertedValue;
		}

		public bool IsValidUnit(string unit) => Core.Constants.Temperature.Codes.Contains(unit);
	}
}
