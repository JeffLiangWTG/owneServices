using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDocAddressWrapperTest : TestCaseWithFactory
	{
		public void TestIAddressWithoutOverride()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Company Name";
			orgHeader.MainAddress.OA_Address1 = "OH Address1";
			orgHeader.MainAddress.OA_Address2 = "OH Address2";
			orgHeader.MainAddress.OA_City = "OH City";
			orgHeader.MainAddress.OA_State = "OH State";
			orgHeader.MainAddress.OA_PostCode = "OH Post";
			orgHeader.MainAddress.OA_Phone = "OH Phone";
			orgHeader.MainAddress.OA_Fax = "OH Fax";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "OA Address1";
			orgAddress.OA_Address2 = "OA Address2";
			orgAddress.OA_City = "OA City";
			orgAddress.OA_State = "OA State";
			orgAddress.OA_PostCode = "OA Post";
			orgAddress.OA_Phone = "OA Phone";
			orgAddress.OA_Fax = "OA Fax";
			orgAddress.OA_RN_NKCountryCode = "US";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = orgAddress.PK;
			IAddress address = JobDocAddressWrapper.New(docAddress);

			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", "Company Name", address.CompanyName);
				AssertEquals("Address1", "OA Address1", address.Address1);
				AssertEquals("Address2", "OA Address2", address.Address2);
				AssertEquals("Address3", "OA City OA State", address.Address3);
				AssertEquals("City", "OA City", address.City);
				AssertEquals("State", "OA State", address.State);
				AssertEquals("PostCode", "OA Post", address.PostCode);
				AssertEquals("CountryCode", "US", address.CountryCode);
				AssertEquals("Phone", "OA Phone", address.Phone);
				AssertEquals("Fax", "OA Fax", address.Fax);
			});
		}

		public void TestIAddressWithOverride()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "Company Name";
			docAddress.E2_Address1 = "Address 1";
			docAddress.E2_Address2 = "Address 2";
			docAddress.E2_RN_NKCountryCode = "AU";
			docAddress.E2_Fax = "1212";
			docAddress.E2_Phone = "2323";
			docAddress.E2_Postcode = "3434";
			docAddress.E2_State = "State";
			docAddress.E2_City = "City";

			IAddress address = JobDocAddressWrapper.New(docAddress);

			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", "Company Name", address.CompanyName);
				AssertEquals("Address1", "Address 1", address.Address1);
				AssertEquals("Address2", "Address 2", address.Address2);
				AssertEquals("Address3", "City State", address.Address3);
				AssertEquals("City", "City", address.City);
				AssertEquals("State", "State", address.State);
				AssertEquals("PostCode", "3434", address.PostCode);
				AssertEquals("CountryCode", "AU", address.CountryCode);
				AssertEquals("Phone", "2323", address.Phone);
				AssertEquals("Fax", "1212", address.Fax);
			});
		}
	}
}
