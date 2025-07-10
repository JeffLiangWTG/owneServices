using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFBillCollection))]
	sealed class CusISFBillCollectionTest : ActiveBusinessObjectCollectionTestCase<CusISFBillCollection>
	{
		public void TestIndexer_BillTypeAndBillNumber()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBillCollection referenceDatas = header.ReferenceDatas;
			CusISFBill bill1 = referenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "1234567890";
			CusISFBill bill2 = referenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill2.BB_BillNum = "1234567890";
			AssertEquals(bill1, referenceDatas[BillTypeList.Codes.HouseBillOfLading, "1234567890"]);
			AssertEquals(bill2, referenceDatas[BillTypeList.Codes.MasterBillOfLading, "1234567890"]);
			AssertNull(referenceDatas[BillTypeList.Codes.OceanBillOfLading, "1234567890"]);
			AssertNull(referenceDatas[BillTypeList.Codes.HouseBillOfLading, "1234564894"]);
		}

		public void TestGetFirstMatchingType()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBillCollection referenceDatas = header.ReferenceDatas;
			CusISFBill bill1 = referenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "1234567890";
			CusISFBill bill2 = referenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill2.BB_BillNum = "1234567890";
			CusISFBill bill3 = referenceDatas.AddNew();
			bill3.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill3.BB_BillNum = "1234567890";
			CusISFBill bill4 = referenceDatas.AddNew();
			bill4.BB_BillType = BillTypeList.Codes.BondReferenceNumber;
			bill4.BB_BillNum = "1234567890";
			AssertEquals(bill1, referenceDatas.GetFirstMatchingType(BillTypeList.Codes.HouseBillOfLading));
			AssertEquals(bill2, referenceDatas.GetFirstMatchingType(BillTypeList.Codes.MasterBillOfLading));
			AssertEquals(bill4, referenceDatas.GetFirstMatchingType(BillTypeList.Codes.BondReferenceNumber));
		}

		public void TestUpdateBill()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill bill1 = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "1234567890";
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill2.BB_BillNum = "1234567890";
			CusISFBill bill3 = header.ReferenceDatas.AddNew();
			bill3.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill3.BB_BillNum = "1234567890";
			CusISFBill bill4 = header.ReferenceDatas.AddNew();
			bill4.BB_BillType = BillTypeList.Codes.SuretyCode;
			bill4.BB_BillNum = "1234567890";
			header.ReferenceDatas.UpdateBill("1234567890", "ZZ", ZDateTime.BrettsBirthday);
			AssertEquals("ZZ", bill1.BB_CustomsStatus);
			AssertEquals(ZDateTime.Empty, bill1.BB_MatchDate);
			AssertEquals(ZString.Empty, bill2.BB_CustomsStatus);
			AssertEquals(ZDateTime.Empty, bill2.BB_MatchDate);
			AssertEquals("ZZ", bill3.BB_CustomsStatus);
			AssertEquals(ZDateTime.Empty, bill3.BB_MatchDate);
			AssertEquals(ZString.Empty, bill4.BB_CustomsStatus);
			AssertEquals(ZDateTime.Empty, bill4.BB_MatchDate);
			header.ReferenceDatas.UpdateBill("1234567890", DispositionCodeList.Codes.S1, ZDateTime.BrettsBirthday);
			AssertEquals(DispositionCodeList.Codes.S1, bill1.BB_CustomsStatus);
			AssertEquals(ZDateTime.BrettsBirthday, bill1.BB_MatchDate);
			AssertEquals(ZString.Empty, bill2.BB_CustomsStatus);
			AssertEquals(ZDateTime.Empty, bill2.BB_MatchDate);
			AssertEquals(DispositionCodeList.Codes.S1, bill3.BB_CustomsStatus);
			AssertEquals(ZDateTime.BrettsBirthday, bill3.BB_MatchDate);
			AssertEquals(ZString.Empty, bill4.BB_CustomsStatus);
			AssertEquals(ZDateTime.Empty, bill4.BB_MatchDate);
		}

		protected override CusISFBillCollection GetCollectionToTest() => new CusISFBillCollection(Factory.New<CusISFHeader>());
	}
}
