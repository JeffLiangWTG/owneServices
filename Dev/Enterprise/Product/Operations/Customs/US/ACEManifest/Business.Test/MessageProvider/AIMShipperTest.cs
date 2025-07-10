using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMShipperTest : TestCaseWithFactory
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
			bill.ABL_OA_Shipper = org.PK;
			var aimShipper = new AIMShipper(bill);
			AssertEquals("FullName", aimShipper.Name);
			AssertEquals("Address1", aimShipper.StreetAddress);
			AssertEquals("SIN", aimShipper.CityCountyTownship);
			AssertEquals("STATE", aimShipper.StateOrProvince);
			AssertEquals("0001", aimShipper.PostalCode);
			AssertEquals("+00123456888", aimShipper.TelephoneNumber);
		}
	}
}
