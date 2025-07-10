using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	class CombinedCalculatorControlTest : BaseCombinedCalculatorControlTest<CombinedControl>
	{
		public override string CalculatorCode => CombinedCalculator.Code;
	}
}
