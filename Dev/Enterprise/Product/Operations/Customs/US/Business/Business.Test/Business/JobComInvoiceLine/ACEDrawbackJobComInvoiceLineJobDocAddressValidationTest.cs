using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEDrawbackJobComInvoiceLineJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckOrganisationPK()
		{
			var jobDocAddressValidation = new ACEDrawbackJobComInvoiceLineJobDocAddressValidation(exporterDocAddress, invoiceLine);
			jobDocAddressValidation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(exporterDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_DRWIsForExportSection = true;
			jobDocAddressValidation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(exporterDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(exporterDocAddress.OrganisationPKInfo, "company name is too long.");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			exporterDocAddress.OrganisationPK = orgHeader.PK;
			jobDocAddressValidation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(exporterDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(exporterDocAddress.OrganisationPKInfo, "company name is too long.");

			orgHeader.OH_FullName = "THIS IS A VERY LONG COMPANY NAME;THIS IS A VERY LONG COMPANY NAME;";
			exporterDocAddress.OrganisationPK = orgHeader.PK;
			jobDocAddressValidation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(exporterDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(exporterDocAddress.OrganisationPKInfo, "company name is too long.");
		}

		public void TestCheckE2_CompanyName()
		{
			var jobDocAddressValidation = new ACEDrawbackJobComInvoiceLineJobDocAddressValidation(exporterDocAddress, invoiceLine);
			exporterDocAddress.E2_AddressOverride = true;
			jobDocAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageErrorContaining(exporterDocAddress.E2_CompanyNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_DRWIsForExportSection = true;
			jobDocAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageErrorContaining(exporterDocAddress.E2_CompanyNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(exporterDocAddress.E2_CompanyNameInfo, "company name is too long.");

			exporterDocAddress.E2_CompanyName = "COMPANY";
			jobDocAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageErrorContaining(exporterDocAddress.E2_CompanyNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(exporterDocAddress.E2_CompanyNameInfo, "company name is too long.");

			exporterDocAddress.E2_CompanyName = "THIS IS A VERY LONG COMPANY NAME;THIS IS A VERY LONG COMPANY NAME;";
			jobDocAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageErrorContaining(exporterDocAddress.E2_CompanyNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(exporterDocAddress.E2_CompanyNameInfo, "company name is too long.");
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			exporterDocAddress = invoiceLine.ExporterOrDestroyer;
		}
		JobComInvoiceLine invoiceLine;
		JobDocAddress exporterDocAddress;
	}
}
