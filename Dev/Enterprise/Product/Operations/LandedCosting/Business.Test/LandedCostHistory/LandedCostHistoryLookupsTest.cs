using CargoWise.EntityFramework.Testing;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LandedCostHistoryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLineTypeList()
		{
			var header = Factory.New<DummyLandedCostHeader>();
			header.SupportsNoCostApportionmentItem = true;
			var lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.DefaultFromHost(header);

			var landedCostHistory = lcHeader.Histories.AddNew();
			AssertEquals("ACT, NCA", landedCostHistory.Lookups.LineTypeList.CodesAsString);

			landedCostHistory = Factory.New<LandedCostHistory>();
			AssertEquals("ACT, EST", landedCostHistory.Lookups.LineTypeList.CodesAsString);
		}
	}
}
