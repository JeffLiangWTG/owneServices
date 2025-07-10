using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInbondBillAddRefCollection))]
	sealed class CusInbondBillAddRefCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInbondBillAddRefCollection>
	{
		public void TestIndexer_Qualifier()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var collection = bill.ShipmentReferenceDetails;
			var reference1 = collection.AddNew(BillReferenceList.Codes.BN, "BNSDFS234");
			var reference2 = collection.AddNew(BillReferenceList.Codes.OB, "OBSDFS234");
			AssertNull(collection[BillReferenceList.Codes.MB]);
			AssertEquals(reference1, collection[BillReferenceList.Codes.BN]);
			AssertEquals(reference2, collection[BillReferenceList.Codes.OB]);
			reference1.Delete();
			AssertNull(collection[BillReferenceList.Codes.BN]);
		}

		public void TestAddNewWithQualifierAndReferenceNum()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var reference = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.BN, "BNSDFS234");
			AssertEquals(BillReferenceList.Codes.BN, reference.BR_Qualifier);
			AssertEquals("BNSDFS234", reference.BR_ReferenceNum);
		}

		protected override CusInbondBillAddRefCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			return new CusInbondBillAddRefCollection(bill);
		}
	}
}
