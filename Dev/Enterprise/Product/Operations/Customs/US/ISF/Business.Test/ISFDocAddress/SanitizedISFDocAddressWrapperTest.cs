using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class SanitizedISFDocAddressWrapperTest : TestCaseWithFactory
	{
		public void TestISanitizedISFDocAddressMembers()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "INBOM";
			var orgaddress = organisation.MainAddress;
			var contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			contact.OC_Birthday = ZDate.BrettsBirthday;
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Consignee.ToString();
			document.OD_DefaultContact = true;
			organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.SocialSecurityNumber, "986-45-8976", Core.Constants.CountryCodes.UnitedStates);
			var header = Factory.New<CusISFHeader>();
			var docAddress = header.DocAddresses.FindOrCreateWithRequirement(header.ISFDocAddressRequirementProvider.ConsigneeDocAddressRequirement);
			var iAddress = new SanitizedISFDocAddressWrapper(docAddress);
			AssertEquals("IISFDocAddress.E2_AddressOverride.", false, iAddress.E2_AddressOverride);
			AssertEquals("IISFDocAddress.E2_Contact", "", iAddress.E2_Contact);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumber", "", iAddress.E2_SocialSecurityNumber);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumberDateOfBirth", ZDateTime.Empty, iAddress.E2_SocialSecurityNumberDateOfBirth);
			docAddress.E2_OA_Address = orgaddress.PK;
			AssertEquals("IISFDocAddress.E2_Contact", "BOB THE BUILDER", iAddress.E2_Contact);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumber", "986-45-8976", iAddress.E2_SocialSecurityNumber);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumberDateOfBirth", ZDateTime.BrettsBirthday, iAddress.E2_SocialSecurityNumberDateOfBirth);
			docAddress.E2_Contact = "BOB THE BUILDER";
			docAddress.E2_AddressOverride = true;
			AssertEquals("IISFDocAddress.E2_AddressOverride.", true, iAddress.E2_AddressOverride);
			AssertEquals("IISFDocAddress.E2_Contact", "BOB THE BUILDER", iAddress.E2_Contact);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumber", "986-45-8976", iAddress.E2_SocialSecurityNumber);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumberDateOfBirth", ZDateTime.BrettsBirthday, iAddress.E2_SocialSecurityNumberDateOfBirth);
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			AssertEquals("IISFDocAddress.E2_Contact", "BOB THE BUILDER", iAddress.E2_Contact);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumber", "", iAddress.E2_SocialSecurityNumber);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumberDateOfBirth", ZDateTime.Empty, iAddress.E2_SocialSecurityNumberDateOfBirth);
			docAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			docAddress.E2_GovRegNum = "986-45-897618SEP1971";
			AssertEquals("IISFDocAddress.E2_Contact", "BOB THE BUILDER", iAddress.E2_Contact);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumber", "986-45-8976", iAddress.E2_SocialSecurityNumber);
			AssertEquals("IISFDocAddress.E2_SocialSecurityNumberDateOfBirth", ZDateTime.BrettsBirthday, iAddress.E2_SocialSecurityNumberDateOfBirth);
		}
	}
}
