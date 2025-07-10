using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondMoveLineItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackageTypeList()
		{
			AssertContainsExactElementsInAnyOrder(RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory), line.Lookups.PackageTypeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			line = moveDetail.CusInBondMoveLineItemCollection.AddNew();
		}

		CusInBondMoveLineItem line;
	}
}
