using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class CompanyInformationDTOTest : TestCaseWithFactory
	{
		public void TestCompanyInformationDTOConstructor()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.OH_FullName = "Test Organization";
			org.OH_IsConsignee = true;
			org.OH_IsConsignor = true;

			var mainAddress = org.MainAddress;
			mainAddress.City = "Test City";
			mainAddress.Postcode = ZString.Empty;
			mainAddress.Address1 = "123 Test Street";
			mainAddress.Address2 = "456";
			mainAddress.State = "Test State";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.Postcode = "252000";

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_ContactName = "Huang Fei Hong";
			orgContact.OC_Email = "123456@163.com";

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "1234567";

			Factory.Save();

			var boleroInvitationDetails = new BoleroInvitationDetails(org)
			{
				SelectedContactPK = orgContact.PK,
			};

			var companyInfo = new CompanyInformationDTO(boleroInvitationDetails);
			AssertEquals("Huang Fei", companyInfo.FirstName);
			AssertEquals("Hong", companyInfo.LastName);
			AssertEquals("123456@163.com", companyInfo.Email);
			AssertEquals("Test Organization", companyInfo.LegalCompanyName);
			AssertEquals("123 Test Street", companyInfo.AddressLine1);
			AssertEquals("456", companyInfo.AddressLine2);
			AssertEquals("Test City", companyInfo.City);
			AssertEquals("252000", companyInfo.PostCode);
			AssertEquals("Test State", companyInfo.State);
			AssertEquals("AU", companyInfo.CountryCode);
			AssertEquals("1234567", companyInfo.CompanyIdentifications["ABN"]);
			AssertEquals("CNE", companyInfo.EntityTypes[0]);
			AssertEquals("CNR", companyInfo.EntityTypes[1]);
			AssertEquals(2, companyInfo.EntityTypes.Count);
			AssertEquals(org.PK, companyInfo.CompanyOrgCode);

			org.MainAddress.Postcode = ZString.Empty;
			boleroInvitationDetails = new BoleroInvitationDetails(org)
			{
				SelectedContactPK = orgContact.PK,
			};

			companyInfo = new CompanyInformationDTO(boleroInvitationDetails);
			AssertEquals("If Postcode is empty, the default Postcode will be set to OOO", "OOO", companyInfo.PostCode);

			orgContact.OC_ContactName = "   Huang  Fei  Hong  ";
			companyInfo = new CompanyInformationDTO(boleroInvitationDetails);

			AssertEquals("Even if contactname has spaces, the first name should not contain spaces", "Huang Fei", companyInfo.FirstName);
			AssertEquals("Even if contactname has spaces, the last name should not contain spaces", "Hong", companyInfo.LastName);

			orgContact.OC_ContactName = "Simith";
			companyInfo = new CompanyInformationDTO(boleroInvitationDetails);
			AssertEquals("Simith", companyInfo.FirstName);
			AssertNullOrEmpty(companyInfo.LastName);
		}
	}
}
