using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestsSubclassesOf(typeof(ICusInBondMoveDetailCollection))]
	public abstract class CusInBondMoveDetailCollectionTest<T> : ActiveBusinessObjectCollectionTestCase<T> where T : ICusInBondMoveDetailCollection
	{
		public void TestSetAllCustomsStatus()
		{
			var moveDetail1 = moveDetail;
			var moveDetail2 = CreateNewCusInBondMoveDetail(moveHeader);
			moveHeader.MovementDetails.SetAllCustomsStatus("ZZZ");
			AssertEquals("ZZZ", moveDetail1.B9_CustomsStatus);
			AssertEquals("ZZZ", moveDetail2.B9_CustomsStatus);
		}

		protected Customs.Business.CusInBondHeader header;
		protected Customs.Business.CusInBondBill bill;
		protected CusInBondMoveHeader moveHeader;
		protected CusInBondMoveDetail moveDetail;
		protected override void SetUp()
		{
			base.SetUp();
			header = CreateNewCusInBondHeader();
			bill = CreateNewCusInBondBill(header);
			moveHeader = CreateNewCusInBondMoveHeader(header);
			moveDetail = CreateNewCusInBondMoveDetail(moveHeader);
			moveDetail.B9_B0 = bill.PK;
		}

		protected abstract CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header);

		protected abstract Customs.Business.CusInBondHeader CreateNewCusInBondHeader();

		protected abstract Customs.Business.CusInBondBill CreateNewCusInBondBill(Customs.Business.CusInBondHeader header);

		protected abstract CusInBondMoveDetail CreateNewCusInBondMoveDetail(CusInBondMoveHeader moveHeader);
	}
}
