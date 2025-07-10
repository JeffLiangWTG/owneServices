namespace Enterprise.ProductionRules.Business
{
	interface IUnitConverterStrategy
	{
		decimal Convert(decimal value, string fromUnit, string toUnit);
		bool IsValidUnit(string unit);
	}
}
