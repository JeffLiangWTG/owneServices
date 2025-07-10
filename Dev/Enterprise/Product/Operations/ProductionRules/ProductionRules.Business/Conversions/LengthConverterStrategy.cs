namespace Enterprise.ProductionRules.Business
{
	sealed class LengthConverterStrategy : IUnitConverterStrategy
	{
		public decimal Convert(decimal value, string fromUnit, string toUnit) => Core.Constants.Length.ConvertSafe(value, fromUnit, toUnit, applyDefaultRounding: false);
		public bool IsValidUnit(string unit) => Core.Constants.Length.ContainsCode(unit);
	}
}
