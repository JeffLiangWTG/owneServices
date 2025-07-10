using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class PSMonthCalculatorTest : TestCaseWithFactory
	{
		public void TestGetMonth()
		{
			var calculator = new PSMonthCalculator();
			AssertEquals("01", calculator.GetMonth(new ZDate(2011, 12, 31)));
			AssertEquals("10", calculator.GetMonth(new ZDate(2011, 9, 30)));
			AssertEquals(ZString.Empty, calculator.GetMonth(ZDate.Empty));
			AssertEquals(ZString.Empty, calculator.GetMonth(ZDate.Invalid));
		}
	}
}
