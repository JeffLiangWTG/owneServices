using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondMoveLineItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUnitList()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var line = moveDetail.CBP7512Lines.AddNew();
			AssertContainsExactElementsInAnyOrder(new WeightUnitList(), line.Lookups.WeightUnitList);
		}
	}
}
