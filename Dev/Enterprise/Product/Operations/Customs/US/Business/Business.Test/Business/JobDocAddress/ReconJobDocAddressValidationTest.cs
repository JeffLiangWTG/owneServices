using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckE2_Address1()
		{
			string addressDescriptionWarning = "Address 1 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			reconDec.SummaryDocRecipientAddress.E2_AddressOverride = true;
			reconDec.SummaryDocRecipientAddress.E2_Address1 = "address 1";
			AssertHasMessageErrorContaining(reconDec.SummaryDocRecipientAddress.E2_Address1Info, ReconJobDocAddressValidation.SummaryDocRecipientAddressOnlyValidInFTA);
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconDec.SummaryDocRecipientAddress.RunPreSaveValidation();
			AssertNoMessageErrorContaining(reconDec.SummaryDocRecipientAddress.E2_Address1Info, ReconJobDocAddressValidation.SummaryDocRecipientAddressOnlyValidInFTA);
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			reconDec.ClaimantAddress.E2_AddressOverride = true;
			reconDec.ClaimantAddress.E2_Address1 = "address 1";
			AssertHasMessageErrorContaining(reconDec.ClaimantAddress.E2_Address1Info, ReconJobDocAddressValidation.ClaimantAddressOnlyValidInFTA);
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconDec.ClaimantAddress.RunPreSaveValidation();
			AssertNoMessageErrorContaining(reconDec.ClaimantAddress.E2_Address1Info, ReconJobDocAddressValidation.ClaimantAddressOnlyValidInFTA);
			var origEntry1 = reconDec.OriginalEntries.AddNew();
			var origEntry2 = reconDec.OriginalEntries.AddNew();
			origEntry1.US_ProtestStat = true;
			origEntry2.US_ProtestStat = false;
			reconDec.ClaimantAddress.E2_Address1 = "AAA";
			AssertNoMessageErrorContaining(reconDec.ClaimantAddress.E2_Address1Info, ReconJobDocAddressValidation.AtLeastOneProtestFiledShouldBeTicked);
			origEntry1.US_ProtestStat = false;
			reconDec.ClaimantAddress.RunPreSaveValidation();
			AssertHasMessageErrorContaining(reconDec.ClaimantAddress.E2_Address1Info, ReconJobDocAddressValidation.AtLeastOneProtestFiledShouldBeTicked);
			reconDec.ClaimantAddress.E2_Address1 = "测试地址";
			AssertHasWarning(reconDec.ClaimantAddress.E2_Address1Info, addressDescriptionWarning);
		}

		public void TestCheckCheckE2_OA_Address()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			var party = Factory.New<OrgHeader>();
			var address = party.Addresses.AddNew();
			address.OA_City = "KYIV";
			address.OA_Address1 = "éééÄöß";
			address.OA_Address2 = "Address2Äöß";
			address.OA_Code = "öß";
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.ImporterAddress.E2_OA_Address = address.PK;
			AssertHasWarning(reconDec.ImporterAddress.E2_OA_AddressInfo, addressCodeWarning);
			AssertHasWarning(reconDec.ImporterAddress.E2_OA_AddressInfo, addressDescriptionWarning);
		}

		public void TestCheckOrganisationPK()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			reconDec.SummaryDocRecipientAddress.OrganisationPK = orgHeader.PK;
			AssertHasMessageErrorContaining(reconDec.SummaryDocRecipientAddress.OrganisationPKInfo, ReconJobDocAddressValidation.SummaryDocRecipientAddressOnlyValidInFTA);
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconDec.SummaryDocRecipientAddress.RunPreSaveValidation();
			AssertNoMessageErrorContaining(reconDec.SummaryDocRecipientAddress.OrganisationPKInfo, ReconJobDocAddressValidation.SummaryDocRecipientAddressOnlyValidInFTA);
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			reconDec.ClaimantAddress.OrganisationPK = orgHeader.PK;
			AssertHasMessageErrorContaining(reconDec.ClaimantAddress.OrganisationPKInfo, ReconJobDocAddressValidation.ClaimantAddressOnlyValidInFTA);
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconDec.ClaimantAddress.RunPreSaveValidation();
			AssertNoMessageErrorContaining(reconDec.ClaimantAddress.OrganisationPKInfo, ReconJobDocAddressValidation.ClaimantAddressOnlyValidInFTA);
		}
	}
}
