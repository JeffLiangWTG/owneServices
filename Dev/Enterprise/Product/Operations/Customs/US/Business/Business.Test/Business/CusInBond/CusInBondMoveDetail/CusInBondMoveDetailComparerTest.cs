using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusInBondMoveDetailComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			var comparer = new CusInBondMoveDetailComparer();
			var header = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			var bill1 = (Customs.Business.CusInBondBill)header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB2";
			var bill2 = (Customs.Business.CusInBondBill)header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MB1";
			var moveHeader = header.MovementHeader;
			var moveDetail1 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			var moveDetail2 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			moveDetail1.B9_SeqNo = "";
			moveDetail2.B9_SeqNo = "";
			AssertEquals(1, comparer.Compare(moveDetail1, moveDetail2));
			moveDetail1.B9_SeqNo = "1";
			moveDetail2.B9_SeqNo = "2";
			AssertEquals(-1, comparer.Compare(moveDetail1, moveDetail2));
			moveDetail2.B9_B0 = bill1.PK;
			moveDetail1.B9_SeqNo = "1";
			moveDetail2.B9_SeqNo = "1";
			AssertEquals(0, comparer.Compare(moveDetail1, moveDetail2));
		}
	}
}
