using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	class CartageControlTest : BaseCombinedCalculatorControlTest<CartageControl>
	{
		public override string CalculatorCode => CartageCalculator.Code;
	}
}
