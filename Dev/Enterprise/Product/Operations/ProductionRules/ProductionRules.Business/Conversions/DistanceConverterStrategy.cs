namespace Enterprise.ProductionRules.Business
{
	sealed class DistanceConverterStrategy : IUnitConverterStrategy
	{
		public decimal Convert(decimal value, string fromUnit, string toUnit) => Core.Constants.Distance.ConvertSafe(value, fromUnit, toUnit, applyDefaultRounding: false);
		public bool IsValidUnit(string unit) => Core.Constants.Distance.ContainsCode(unit);
	}
}
