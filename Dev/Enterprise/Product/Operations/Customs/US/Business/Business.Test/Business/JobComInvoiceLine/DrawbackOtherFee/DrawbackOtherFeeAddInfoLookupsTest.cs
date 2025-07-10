using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DrawbackOtherFeeAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOtherFeeTypes()
		{
			var drawbackInvoiceLine = Factory.New<JobComInvoiceLine>();
			var otherFee = drawbackInvoiceLine.DrawbackOtherFees.AddNew();
			var lookups = new DrawbackOtherFeeAddInfoLookups(new DrawbackOtherFeeAddInfo(otherFee.B7_AddInfoDataInfo));
			var otherFeeTypes = lookups.OtherFeeTypes;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "107, 053, 106, 056, 110, 675, 374, 055, 102, 108, 103, 674, 039, 054, 090, 369, 057, 105, 109, 079, 104", otherFeeTypes.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<DrawbackOtherFeeTypesList>(), otherFeeTypes);
			});
		}
	}
}
