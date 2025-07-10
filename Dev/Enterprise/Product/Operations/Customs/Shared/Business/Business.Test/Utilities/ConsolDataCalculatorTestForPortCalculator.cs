using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ConsolDataCalculatorTestForPortCalculator : TestCaseWithFactory
	{
		public void TestIsPortInTheCountryDoesntExplode()
		{
			var calculatorForEmptyCountry = new ConsolDataCalculatorForPortTest(Factory.New<ForwardingConsol>());
			AssertEquals(false, calculatorForEmptyCountry.IsPortInTheCountryExposed(null));
		}
	}
}
