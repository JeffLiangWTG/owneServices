using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(WarehouseDetailCollection))]
	sealed class WarehouseDetailCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestIndexer_WarehouseNumber()
		{
			var moveDetail = MoveDetail;
			var container1 = moveDetail.WarehouseDetails.AddNew();
			container1.US_WarehouseNumber = "TURE1232122";
			var container2 = moveDetail.WarehouseDetails.AddNew();
			container2.US_WarehouseNumber = "ABDD1232122";
			var container3 = moveDetail.WarehouseDetails.AddNew();
			container3.US_WarehouseNumber = "KHJD1232122";
			AssertEquals(container3, moveDetail.WarehouseDetails["KHJD1232122"]);
			AssertEquals(container1, moveDetail.WarehouseDetails["TURE1232122"]);
			AssertEquals(container2, moveDetail.WarehouseDetails["ABDD1232122"]);
			AssertNull(moveDetail.WarehouseDetails["LJDS1232122"]);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new WarehouseDetailCollection(MoveDetail);

		CusInBondMoveDetail moveDetail;
		CusInBondMoveDetail MoveDetail
		{
			get
			{
				if (moveDetail == null)
				{
					var header = Factory.New<CusInBondHeader>();
					var bill = header.Bills.AddNew();
					var moveHeader = header.MovementHeaders.AddNew();
					moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				}
				return moveDetail;
			}
		}
	}
}
