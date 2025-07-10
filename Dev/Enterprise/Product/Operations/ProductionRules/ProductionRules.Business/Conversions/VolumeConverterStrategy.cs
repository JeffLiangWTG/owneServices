namespace Enterprise.ProductionRules.Business
{
	public class VolumeConverterStrategy : IUnitConverterStrategy
	{
		public decimal Convert(decimal value, string fromUnit, string toUnit) => Core.Constants.Volume.ConvertSafe(value, fromUnit, toUnit, applyDefaultRounding: false);
		public bool IsValidUnit(string unit) => Core.Constants.Volume.ContainsCode(unit);
	}
}
