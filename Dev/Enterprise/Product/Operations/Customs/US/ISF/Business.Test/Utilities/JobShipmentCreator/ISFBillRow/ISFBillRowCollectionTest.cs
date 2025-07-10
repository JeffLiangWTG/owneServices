using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFBillRowCollection))]
	sealed class ISFBillRowCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ISFBillRowCollection>
	{
		public void TestIndexer_BillPK()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill bill1 = header.ReferenceDatas.AddNew();
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRowCollection collection = new ISFBillRowCollection(headerRow);
			ISFBillRow billRow1 = collection.AddNew();
			billRow1.BillPK = bill2.PK;
			ISFBillRow billRow2 = collection.AddNew();
			billRow2.BillPK = bill1.PK;
			AssertNull(collection[ZGuid.Empty]);
			AssertNull(collection[ZGuid.Invalid]);
			AssertEquals(billRow1, collection[bill2.PK]);
			AssertEquals(billRow2, collection[bill1.PK]);
		}

		public void TestAllowFeature()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill masterBill = header.ReferenceDatas.AddNew();
			masterBill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			CusISFBill houseBill = header.ReferenceDatas.AddNew();
			houseBill.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			CusISFBill oceanBill = header.ReferenceDatas.AddNew();
			oceanBill.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			ISFBillRowCollection collection = headerRow.Bills;
			foreach (ZGuid masterBillPK in new ZGuid[] { ZGuid.Empty, masterBill.PK, houseBill.PK, ZGuid.Invalid })
			{
				headerRow.MasterBillPK = masterBillPK;
				AssertEquals(true, collection.AllowNew);
				AssertEquals(true, collection.AllowRemove);
			}

			headerRow.MasterBillPK = oceanBill.PK;
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);
		}

		protected override ISFBillRowCollection GetCollectionToTest() => new ISFBillRowCollection(new ISFHeaderRow(Factory.New<CusISFHeader>()));

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ISFBillRow(new ISFHeaderRow(Factory.New<CusISFHeader>()));
	}
}
