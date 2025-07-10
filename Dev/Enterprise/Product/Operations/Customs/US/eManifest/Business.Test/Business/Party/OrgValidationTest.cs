using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class OrgValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateOrganization()
		{
			const string messageError = @"Consignee is invalid and has following errors:
You have not entered a name;
You have not entered an address line 1;
You have not entered a city;
You have not entered a state/province;
You have not entered a postal/zip code;
You have not entered a country.";
			const string warning = @"Consignee has the following warnings:
The length of data entered into name (41) exceeds the maximum allowed. Only the first 40 characters will be transmitted to Customs;
The length of data entered into state/province (5) exceeds the maximum allowed. Only the first 3 characters will be transmitted to Customs.";
			var party = Factory.New<Trip>().Shipments.AddNew().Parties.AddNew();
			party.E2_AddressType = PartyTypes.Codes.Consignee;
			using (party.SuspendValidationTesting())
			using (party.GetValidationSuspender())
			{
				party.E2_AddressOverride = true;
				OrgValidation.ValidateOrganization(party);
				AssertNoMessageError(party.OrganisationPKInfo, messageError);
				var org = Factory.New<OrgHeader>();
				org.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
				party.E2_AddressOverride = false;
				party.OrganisationPK = org.PK;
				party.OrganisationPKInfo.ClearAllNotifications();
				OrgValidation.ValidateOrganization(party);
				AssertHasMessageError(party.OrganisationPKInfo, messageError);
				org.MainAddress.OA_CompanyNameOverride = "Long Company Name More Than 40 Characters";
				org.MainAddress.OA_Address1 = "Address";
				org.MainAddress.OA_City = "City";
				org.MainAddress.OA_State = "State";
				org.MainAddress.OA_PostCode = "012345";
				org.MainAddress.OA_RL_NKRelatedPortCode = "US";
				party.OrganisationPKInfo.ClearAllNotifications();
				OrgValidation.ValidateOrganization(party);
				AssertHasWarning(party.OrganisationPKInfo, warning);
				org.MainAddress.OA_CompanyNameOverride = "Company";
				org.MainAddress.OA_State = "CA";
				party.OrganisationPKInfo.ClearAllNotifications();
				OrgValidation.ValidateOrganization(party);
				AssertNoNotifications(party.OrganisationPKInfo);
			}
		}

		public void TestValidateAddress()
		{
			const string messageError = @"Location is invalid and has following errors:
You have not entered an address line 1;
You have not entered a city;
You have not entered a state/province;
You have not entered a postal/zip code;
You have not entered a country.";
			const string warning = @"Location has the following warnings:
The length of data entered into state/province (5) exceeds the maximum allowed. Only the first 3 characters will be transmitted to Customs.";
			var address = Factory.New<JobDocAddress>();
			address.DocAddressType = DocAddressType.Location;
			using (address.SuspendValidationTesting())
			using (address.GetValidationSuspender())
			{
				address.E2_AddressOverride = true;
				OrgValidation.ValidateAddress(address);
				AssertNoMessageError(address.OrganisationPKInfo, messageError);
				var org = Factory.New<OrgHeader>();
				org.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
				address.E2_AddressOverride = false;
				address.OrganisationPK = org.PK;
				address.OrganisationPKInfo.ClearAllNotifications();
				OrgValidation.ValidateAddress(address);
				AssertHasMessageError(address.OrganisationPKInfo, messageError);
				org.MainAddress.OA_Address1 = "Address";
				org.MainAddress.OA_City = "City";
				org.MainAddress.OA_State = "State";
				org.MainAddress.OA_PostCode = "012345";
				org.MainAddress.OA_RL_NKRelatedPortCode = "US";
				address.OrganisationPKInfo.ClearAllNotifications();
				OrgValidation.ValidateAddress(address);
				AssertHasWarning(address.OrganisationPKInfo, warning);
				org.MainAddress.OA_State = "CA";
				address.OrganisationPKInfo.ClearAllNotifications();
				OrgValidation.ValidateAddress(address);
				AssertNoNotifications(address.OrganisationPKInfo);
			}
		}

		public void TestValidateEuropeanLanguageCharacters()
		{
			var engHeader = Factory.New<OrgHeader>();
			engHeader.OH_Code = "ENGORG";
			engHeader.OH_FullName = "TEST ENG COMPANY";
			engHeader.MainAddress.OA_Address1 = "TEST ENG ADDRESS 1";
			engHeader.MainAddress.OA_Address2 = "TEST ENG ADDRESS 2";
			engHeader.MainAddress.OA_City = "CHICAGO";
			engHeader.MainAddress.OA_State = "IL";
			engHeader.MainAddress.OA_PostCode = "11111111";
			engHeader.MainAddress.OA_RL_NKRelatedPortCode = "US";
			var frnHeader = Factory.New<OrgHeader>();
			frnHeader.OH_Code = "FREORG";
			frnHeader.OH_FullName = "TEST FRÉ COMPÄNY";
			frnHeader.MainAddress.OA_Address1 = "TêST FRÈ àDDRESS 1";
			frnHeader.MainAddress.OA_Address2 = "TëST FRE ÀDDRESS 2";
			frnHeader.MainAddress.OA_City = "TEST CÏTY";
			frnHeader.MainAddress.OA_State = "IL";
			frnHeader.MainAddress.OA_PostCode = "12345Ä";
			frnHeader.MainAddress.OA_RL_NKRelatedPortCode = "US";
			var chsHeader = Factory.New<OrgHeader>();
			chsHeader.OH_Code = "ENGORG2";
			chsHeader.OH_FullName = "测试公司名称";
			chsHeader.MainAddress.OA_Address1 = "测试地址1";
			chsHeader.MainAddress.OA_Address2 = "测试地址2";
			chsHeader.MainAddress.OA_City = "芝加哥";
			chsHeader.MainAddress.OA_State = "IL";
			chsHeader.MainAddress.OA_PostCode = "111111啊";
			chsHeader.MainAddress.OA_RL_NKRelatedPortCode = "US";
			var party = Factory.New<Trip>().Shipments.AddNew().Parties.AddNew();
			party.E2_AddressType = PartyTypes.Codes.Shipper;
			party.OrganisationPK = engHeader.PK;
			AssertNoMessageErrors(party.OrganisationPKInfo);
			party.OrganisationPK = frnHeader.PK;
			AssertNoMessageErrors(party.OrganisationPKInfo);
			party.OrganisationPK = chsHeader.PK;
			AssertHasMessageErrorContaining(party.OrganisationPKInfo, "Original shipper is invalid and has following errors:\r\n" +
				"Original shipper: Company Name only accepts English language characters;\r\n" +
				"Original shipper: Address Line 1 only accepts English language characters;\r\n" +
				"Original shipper: Address Line 2 only accepts English language characters;\r\n" +
				"Original shipper: City only accepts English language characters;\r\n" +
				"Original shipper: Postcode only accepts English language characters.");
			party.OrganisationPK = ZGuid.Empty;
			party.E2_AddressOverride = true;
			party.E2_CompanyName = "ABCDEF";
			AssertNoMessageErrors(party.E2_CompanyNameInfo);
			party.E2_CompanyName = "ÄBCDEF";
			AssertNoMessageErrors(party.E2_CompanyNameInfo);
			party.E2_CompanyName = "测试";
			AssertHasMessageErrorContaining(party.E2_CompanyNameInfo, party.E2_CompanyNameInfo.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
			party.E2_Address1 = "ABCDEF";
			AssertNoMessageErrors(party.E2_Address1Info);
			party.E2_Address1 = "ÄBCDEF";
			AssertNoMessageErrors(party.E2_Address1Info);
			party.E2_Address1 = "测试";
			AssertHasMessageErrorContaining(party.E2_Address1Info, party.E2_Address1Info.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
			party.E2_Address2 = "ABCDEF";
			AssertNoMessageErrors(party.E2_Address2Info);
			party.E2_Address2 = "ÄBCDEF";
			AssertNoMessageErrors(party.E2_Address2Info);
			party.E2_Address2 = "测试";
			AssertHasMessageErrorContaining(party.E2_Address2Info, party.E2_Address2Info.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
			party.E2_City = "ABCDEF";
			AssertNoMessageErrors(party.E2_CityInfo);
			party.E2_City = "ÄBCDEF";
			AssertNoMessageErrors(party.E2_CityInfo);
			party.E2_City = "测试";
			AssertHasMessageErrorContaining(party.E2_CityInfo, party.E2_CityInfo.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
			party.E2_Postcode = "11111";
			AssertNoMessageErrors(party.E2_PostcodeInfo);
			party.E2_Postcode = "11111Ä";
			AssertNoMessageErrors(party.E2_PostcodeInfo);
			party.E2_Postcode = "测试";
			AssertHasMessageErrorContaining(party.E2_PostcodeInfo, party.E2_PostcodeInfo.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
		}

		internal static void AssertJobDocAddressUsesOrgValidationIfOrgSpecified(ZPropertyInfo orgPkInfo, bool validatingCompanyName = true)
		{
			const string messageError = "is invalid and has following errors:";
			orgPkInfo.Value = ZGuid.Empty;
			AssertNoMessageErrorContaining(orgPkInfo, messageError);
			var org = orgPkInfo.BizObj.Factory.New<OrgHeader>();
			org.MainAddress.OA_RL_NKRelatedPortCode = "US";
			orgPkInfo.Value = org.PK;
			AssertHasMessageErrorContaining(orgPkInfo, messageError);
			org = orgPkInfo.BizObj.Factory.New<OrgHeader>();
			org.MainAddress.OA_Address1 = "Address";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_State = "CA";
			org.MainAddress.OA_PostCode = "012345";
			org.MainAddress.OA_RL_NKRelatedPortCode = "US";
			if (validatingCompanyName)
			{
				org.MainAddress.OA_CompanyNameOverride = "Company";
			}

			orgPkInfo.Value = org.PK;
			AssertNoMessageErrorContaining(orgPkInfo, messageError);
		}
	}
}
