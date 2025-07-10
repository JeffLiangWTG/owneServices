using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEDrawbackRevenueClaimedTest : TestCaseWithFactory
	{
		public void TestRevenueClaim()
		{
			var revenueClaim = new ACEDrawbackRevenueClaimed(DrawbackOtherFeeTypesList.Codes.DrawbackDuty, 100m, 0.99m, 0.88m, "01");
			var revenue = (IACEDrawbackRevenueClaimed)revenueClaim;
			AssertEquals(DrawbackOtherFeeTypesList.Codes.DrawbackDuty, revenue.AccountingClassCode);
			AssertEquals(100m, revenue.ClaimAmount);
			AssertEquals(0.99m, revenue.CalculatedAmount);
			AssertEquals(0.88m, revenue.AdjustedClaimAmount);
			AssertEquals("01", revenue.QualifierIndicator);
		}
	}
}
