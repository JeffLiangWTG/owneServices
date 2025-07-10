using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USCPSCRuleAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCPSCRuleValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			var party2 = Factory.New<OrgHeader>();
			var orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			CPSCRule.US_OA_SafetyTestLocationAddress = orgAddress1.PK;
			AssertHasWarning(CPSCRule.US_OA_SafetyTestLocationAddressInfo, addressDescriptionWarning);
			AssertHasWarning(CPSCRule.US_OA_SafetyTestLocationAddressInfo, addressCodeWarning);
			CPSCRule.US_OA_SafetyTestLocationAddress = orgAddress2.PK;
			AssertNoWarning(CPSCRule.US_OA_SafetyTestLocationAddressInfo, addressDescriptionWarning);
			AssertNoWarning(CPSCRule.US_OA_SafetyTestLocationAddressInfo, addressCodeWarning);
		}

		public void TestCheckUS_OA_SafetyTestLocationAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			CPSCRule.US_OA_SafetyTestLocationAddress = orgHeader.MainAddress.PK;
			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact.";
			AssertHasMessageErrorContaining(CPSCRule.US_OA_SafetyTestLocationAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, "AAAA", "BBBB", null, null, null);

			CPSCRule.AddInfoValidation.ValidateUS_OA_SafetyTestLocationAddress();
			AssertHasMessageErrorContaining(CPSCRule.US_OA_SafetyTestLocationAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, "1234567", null, null);
			CPSCRule.AddInfoValidation.ValidateUS_OA_SafetyTestLocationAddress();
			AssertHasMessageErrorContaining(CPSCRule.US_OA_SafetyTestLocationAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, null, null, "23456");
			CPSCRule.AddInfoValidation.ValidateUS_OA_SafetyTestLocationAddress();
			AssertNoMessageErrorContaining(CPSCRule.US_OA_SafetyTestLocationAddressInfo, errorMsg);

			CPSCRule.US_OA_SafetyTestLocationAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(CPSCRule.US_OA_SafetyTestLocationAddressInfo, MandatoryValidation.YouHaveNotEntered);

			CPSCRule.US_OA_SafetyTestLocationAddress = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(CPSCRule.US_OA_SafetyTestLocationAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CPSCAccreditedLabID()
		{
			CPSCRule.US_CPSCAccreditedLabID = ZString.Empty;
			AssertHasMessageErrorContaining(CPSCRule.US_CPSCAccreditedLabIDInfo, MandatoryValidation.YouHaveNotEntered);

			CPSCRule.US_CPSCAccreditedLabID = "123";
			AssertNoMessageErrorContaining(CPSCRule.US_CPSCAccreditedLabIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PreviousInspectionDate()
		{
			CPSCRule.US_PreviousInspectionDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(CPSCRule.US_PreviousInspectionDateInfo, MandatoryValidation.YouHaveNotEntered);

			CPSCRule.US_PreviousInspectionDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(CPSCRule.US_PreviousInspectionDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		CPSCRule CPSCRule
		{
			get
			{
				if (rule == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
					rule = cpscHeader.RuleAndLabs.AddNew();
				}
				return rule;
			}
		}
		CPSCRule rule;
	}
}
