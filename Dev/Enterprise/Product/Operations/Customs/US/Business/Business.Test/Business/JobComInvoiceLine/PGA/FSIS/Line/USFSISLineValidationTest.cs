using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class USFSISLineValidationTest : TestCaseWithFactory
	{
		public void ValidatePGAContact()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_CertifyCargoRelease = true;

			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(org2, "TIM", "VAN", "034 34343", "1111111111111111111111111111111111111111111111111111@fda.com", null);
			var org3 = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(org2, "TIM", "VAN", "034 34343", "111111111@fda.com", null);

			var invoiceHeader = dec.Invoices.AddNew();

			foreach (var propertyInfo in new[] { dec.IOROrgPKInfo, invoiceHeader.JZ_OH_BuyerInfo })
			{
				propertyInfo.Value = org1.PK;
				AssertHasMessageError(propertyInfo, string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Name"));
				AssertHasMessageError(propertyInfo, string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Work Phone"));
				AssertHasMessageError(propertyInfo, string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Email/Fax"));

				propertyInfo.Value = org2.PK;
				AssertNoMessageError(propertyInfo, string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Name"));
				AssertNoMessageError(propertyInfo, string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Work Phone"));
				AssertNoMessageError(propertyInfo, string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Email/Fax"));

				propertyInfo.Value = ZGuid.Empty;
			}

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "X1";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "X2";
			staff1.GS_FullName = "TIM VAN";
			staff1.GS_WorkPhone = "02323 2323";
			staff1.GS_EmailAddress = "1111111111111111111111111111111111111111111111111111@fda.com";
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "X3";
			staff3.GS_FullName = "TIM VAN";
			staff3.GS_WorkPhone = "02323 2323";
			staff3.GS_EmailAddress = "1111111111111@fda.com";

			dec.JE_GS_NKCusAgent = "X1";
			AssertHasMessageError(dec.JE_GS_NKCusAgentInfo, string.Format(OrganisationValidation.StaffInformationIsMissing, "Name"));
			AssertHasMessageError(dec.JE_GS_NKCusAgentInfo, string.Format(OrganisationValidation.StaffInformationIsMissing, "Work Phone"));
			AssertHasMessageError(dec.JE_GS_NKCusAgentInfo, string.Format(OrganisationValidation.StaffInformationIsMissing, "Email/Fax"));

			dec.JE_GS_NKCusAgent = "X2";
			AssertNoMessageError(dec.JE_GS_NKCusAgentInfo, string.Format(OrganisationValidation.StaffInformationIsMissing, "Name"));
			AssertNoMessageError(dec.JE_GS_NKCusAgentInfo, string.Format(OrganisationValidation.StaffInformationIsMissing, "Work Phone"));
			AssertNoMessageError(dec.JE_GS_NKCusAgentInfo, string.Format(OrganisationValidation.StaffInformationIsMissing, "Email/Fax"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew();
		}
	}
}
