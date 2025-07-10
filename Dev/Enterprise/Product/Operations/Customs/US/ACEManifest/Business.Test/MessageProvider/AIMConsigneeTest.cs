using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMConsigneeTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.OH_FullName = "FullName";
			org.OA_Address1 = "Address1";
			org.OA_Address2 = "Address2";
			org.OA_City = "SIN";
			org.OA_State = "STATE";
			org.OA_PostCode = "0001";
			org.OA_Phone = "+00123456888";
			org.OA_RN_NKCountryCode = "SG";
			bill.ABL_OA_Consignee = org.PK;
			var aimConsignee = new AIMConsignee(bill);
			AssertEquals("FullName", aimConsignee.Name);
			AssertEquals("Address1", aimConsignee.StreetAddress);
			AssertEquals("SIN", aimConsignee.CityCountyTownship);
			AssertEquals("STATE", aimConsignee.StateOrProvince);
			AssertEquals("0001", aimConsignee.PostalCode);
			AssertEquals("00123456888", aimConsignee.TelephoneNumber);
			bill.ABL_ConsigneePhone = "+001 2345 6999";
			var aimConsignee2 = new AIMConsignee(bill);
			AssertEquals("00123456999", aimConsignee2.TelephoneNumber);
		}
	}
}
