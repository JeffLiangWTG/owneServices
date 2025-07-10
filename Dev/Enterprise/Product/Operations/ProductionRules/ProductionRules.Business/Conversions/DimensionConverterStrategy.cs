namespace Enterprise.ProductionRules.Business
{
	sealed class DimensionConverterStrategy : IUnitConverterStrategy
	{
		public decimal Convert(decimal value, string fromUnit, string toUnit) => Core.Constants.Dimension.ConvertSafe(value, fromUnit, toUnit, applyDefaultRounding: false);
		public bool IsValidUnit(string unit) => Core.Constants.Dimension.ContainsCode(unit);
	}
}
