using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEDrawbackRevenueTotalsTest : TestCaseWithFactory
	{
		public void TestRevenueTotals()
		{
			var revenueTotal = new ACEDrawbackRevenueTotals(DrawbackOtherFeeTypesList.Codes.DrawbackDuty, 100m);
			var revenue = (IACEDrawbackRevenueTotals)revenueTotal;
			AssertEquals(DrawbackOtherFeeTypesList.Codes.DrawbackDuty, revenue.AccountingClassCode);
			AssertEquals(100m, revenue.TotalAmount);
		}
	}
}
