using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US.ISF;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBillTypes()
		{
			var header = Factory.New<CusISFHeader>();
			var bill = header.ReferenceDatas.AddNew();
			var list = bill.Lookups.BillTypes;
			AssertEquals(10, list.Count);
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.BondReferenceNumber));
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber));
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.HouseBillOfLading));
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.ISFBondNumber));
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.MasterBillOfLading));
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.OceanBillOfLading));
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.SuretyCode));
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.USCBPEntryNumber));
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.UserDefinedReferenceNumber));
			AssertEquals(true, list.ContainsCode(BillTypeList.Codes.FullNameOfISFImporter));
		}

		public void TestReferenceBillTypes()
		{
			var iSFBill = Factory.New<CusISFBill>();
			AssertEquals("reference bill types should have 3 codes", iSFBill.Lookups.ReferenceBillTypes.Count, 3);
			Assert(iSFBill.Lookups.ReferenceBillTypes.ContainsCode(BillTypeList.Codes.HouseBillOfLading));
			Assert(iSFBill.Lookups.ReferenceBillTypes.ContainsCode(BillTypeList.Codes.MasterBillOfLading));
			Assert(iSFBill.Lookups.ReferenceBillTypes.ContainsCode(BillTypeList.Codes.OceanBillOfLading));
		}
	}
}
