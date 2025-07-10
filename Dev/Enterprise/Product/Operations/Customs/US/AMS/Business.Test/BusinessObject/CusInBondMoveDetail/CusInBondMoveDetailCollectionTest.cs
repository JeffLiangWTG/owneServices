using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetailCollection))]
	sealed class CusInBondMoveDetailCollectionTest : US.Business.Testing.CusInBondMoveDetailCollectionTest<CusInBondMoveDetailCollection>
	{
		public void TestFindBillOfLading()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "OTT1";
			bill1.B0_MasterBillNumber = "MB1";
			var moveDetail1 = moveHeader.MovementDetails[0];

			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "OTT1";
			bill2.B0_MasterBillNumber = "MB2";
			var moveDetail2 = moveHeader.MovementDetails[1];

			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "OTT2";
			bill3.B0_MasterBillNumber = "MB2";
			var moveDetail3 = moveHeader.MovementDetails[2];
			Factory.Save();

			var collection = moveHeader.MovementDetails;
			AssertEquals(moveDetail2, collection.FindBillOfLading("OTT1", "MB2"));
			AssertEquals(moveDetail1, collection.FindBillOfLading("OTT1", "MB1"));
			AssertEquals(moveDetail3, collection.FindBillOfLading("OTT2", "MB2"));
			AssertEquals(moveDetail1, collection.FindBillOfLading("", "MB1"));
			AssertNull(collection.FindBillOfLading("OTT1", "MB3"));
			AssertNull(collection.FindBillOfLading("OTT1", ""));
		}

		public void TestAddNew_BillPK()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			AssertEquals(bill.PK, moveDetail.B9_B0);
		}

		protected override CusInBondMoveDetailCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			return new CusInBondMoveDetailCollection(moveHeader);
		}

		protected override US.Business.CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header)
		{
			var moveHeader = header.Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			return moveHeader;
		}

		protected override Customs.Business.CusInBondHeader CreateNewCusInBondHeader()
		{
			return Factory.New<CusInBondHeader>();
		}

		protected override Customs.Business.CusInBondBill CreateNewCusInBondBill(Customs.Business.CusInBondHeader header)
		{
			return ((CusInBondHeader)header).Bills.AddNew();
		}

		protected override US.Business.CusInBondMoveDetail CreateNewCusInBondMoveDetail(US.Business.CusInBondMoveHeader moveHeader)
		{
			return ((CusInBondMoveHeader)moveHeader).MovementDetails.AddNew();
		}
	}
}
