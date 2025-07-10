namespace Enterprise.ProductionRules.Business
{
	public class WeightConverterStrategy : IUnitConverterStrategy
	{
		public decimal Convert(decimal value, string fromUnit, string toUnit) => Core.Constants.Weight.ConvertSafe(value, fromUnit, toUnit, applyDefaultRounding: false);
		public bool IsValidUnit(string unit) => Core.Constants.Weight.ContainsCode(unit);
	}
}
