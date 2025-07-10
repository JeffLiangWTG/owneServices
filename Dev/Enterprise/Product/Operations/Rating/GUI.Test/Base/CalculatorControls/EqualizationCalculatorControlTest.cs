using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.GUI.Testing
{
	class EqualizationCalculatorControlTest : TestCaseWithFactory
	{
		public void TestMaximumTwoRows()
		{
			using (var control = new EqualizationCalculatorControl())
			{
				AssertEquals("Maximum 2 rows are allowed", 2, control.RateLineItemsGrid.MaximumRows);
			}
		}
	}
}
