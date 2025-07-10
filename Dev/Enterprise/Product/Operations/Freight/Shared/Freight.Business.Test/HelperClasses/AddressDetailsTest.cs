using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class AddressDetailsTest : TestCaseWithFactory
	{
		public void TestPopulateAddressDetailsFromIDocAddress()
		{
			var addressDetails = AddressDetails.Get(address, true);
			AssertNotNull("Precondition - addressDetails is not null.", addressDetails);
			AssertEquals("CompanyName should come from OA_CompanyNameOverride.", "Nintendo USA", addressDetails.CompanyName);
			AssertEquals("Address1 should come from OA_Address1.", "1 Epping Road", addressDetails.Address1);
			AssertEquals("Address2 should come from OA_Address2.", "2 Epping Road", addressDetails.Address2);
			AssertEquals("City should come from OA_City.", "Tokyo", addressDetails.City);
			AssertEquals("State should come from OA_State.", "Tokyo", addressDetails.State);
			AssertEquals("Postcode should come from OA_PostCode.", "157-863", addressDetails.Postcode);

			address.OA_CompanyNameOverride = ZString.Empty;
			addressDetails = AddressDetails.Get(address, true);
			AssertEquals("CompanyName should come from OrgHeader.", "Nintendo", addressDetails.CompanyName);
		}

		public void TestPopulateAddressDetailsFromOrgTranslatedAddress()
		{
			address.OA_Language = "ZH-CN";
			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.FillWithValidTestData();
			translatedAddress.OTA_Language = "EN";
			translatedAddress.OTA_CompanyName = "Nintendo China";
			translatedAddress.OTA_Address1 = "WESTINGHOUSE ROAD";
			translatedAddress.OTA_Address2 = "TRAFFORD PARK";
			translatedAddress.OTA_City = "NEWARK";
			translatedAddress.OTA_State = "NJ";
			translatedAddress.OTA_PostCode = "07105";

			var addressDetails = AddressDetails.Get(address, true);
			AssertNotNull("Precondition - addressDetails is not null.", addressDetails);
			AssertEquals("CompanyName should come from OTA_CompanyName.", "Nintendo China", addressDetails.CompanyName);
			AssertEquals("Address1 should come from OTA_Address1.", "WESTINGHOUSE ROAD", addressDetails.Address1);
			AssertEquals("Address2 should come from OTA_Address2.", "TRAFFORD PARK", addressDetails.Address2);
			AssertEquals("City should come from OTA_City.", "NEWARK", addressDetails.City);
			AssertEquals("State should come from OTA_State.", "NJ", addressDetails.State);
			AssertEquals("Postcode should come from OTA_PostCode.", "07105", addressDetails.Postcode);

			translatedAddress.OTA_CompanyName = ZString.Empty;
			addressDetails = AddressDetails.Get(address, true);
			AssertEquals("CompanyName should come from OrgHeader.", "Nintendo USA", addressDetails.CompanyName);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var company = Factory.NewWithValidTestData<OrgHeader>();
			company.OH_FullName = "Nintendo";

			address = company.Addresses.AddNew();
			address.FillWithValidTestData();
			address.OA_Language = "EN";
			address.OA_CompanyNameOverride = "Nintendo USA";
			address.OA_Address1 = "1 Epping Road";
			address.OA_Address2 = "2 Epping Road";
			address.OA_City = "Tokyo";
			address.OA_State = "Tokyo";
			address.OA_PostCode = "157-863";
		}

		OrgAddress address;
	}
}
