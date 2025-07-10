namespace Enterprise.Rating.Business.Testing
{
	internal class WarehousePackageLineCartageCalculatorTest : BaseWarehousePackageLineCombinedCalculatorTest<CartageCalculator>
	{
		protected override string CalculatorCode => CartageCalculator.Code;
	}
}
