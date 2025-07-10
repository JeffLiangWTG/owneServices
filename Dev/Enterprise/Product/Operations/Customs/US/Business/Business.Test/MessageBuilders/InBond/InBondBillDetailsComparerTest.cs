using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InBondBillDetailsComparerTest : TestCaseWithFactory
	{
		public void TestComparer()
		{
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

			var list = new List<IInBondBillDetails>(moveHeader.MovementDetails.Cast<IInBondBillDetails>());
			list.Sort(new InBondBillDetailsComparer());

			AssertEquals(moveDetail2, list[0]);
			AssertEquals(moveDetail1, list[1]);
		}
	}
}
