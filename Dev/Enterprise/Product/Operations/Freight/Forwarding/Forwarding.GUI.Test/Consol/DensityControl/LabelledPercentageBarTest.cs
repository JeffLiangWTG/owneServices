using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class LabelledPercentageBarTest : TestCaseWithFactory
	{
		public void TestLargeDecimalPercentage()
		{
			using (var percentageBar = new LabelledPercentageBar())
			{
				AssertNoExceptionThrown("Large values are handled appropriately", () => percentageBar.SetValue(3000000000));
			}
		}

		public void TestLargeDecimalPercentage_Negative()
		{
			using (var percentageBar = new LabelledPercentageBar())
			{
				AssertNoExceptionThrown("Large negative values are handled appropriately", () => percentageBar.SetValue(-3000000000));
			}
		}

		public void TestNormalDecimalPErcentage()
		{
			using (var percentageBar = new LabelledPercentageBar())
			{
				AssertNoExceptionThrown("Ordinary use case", () => percentageBar.SetValue(54));
			}
		}
	}
}
