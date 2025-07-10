using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEDrawbackJobComInvoiceLineValidationTest : CommonDrawbackJobComInvoiceLineValidationTest
	{
		public void TestJI_Description()
		{
			InvoiceLine.JI_Description = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(InvoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.US_DRWIsForExportSection = true;
			AssertHasMessageErrorContaining(InvoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.JI_Description = "THIS IS A VERY LONG DESCRIPTION FOR ACE DRAWBACK MESSAGE.";
			AssertNoMessageErrorContaining(InvoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(InvoiceLine.JI_DescriptionInfo, ACEDrawbackJobComInvoiceLineValidation.DescriptionIsTooLong);

			InvoiceLine.JI_Description = "AAAAA";
			AssertNoMessageErrorContaining(InvoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarning(InvoiceLine.JI_DescriptionInfo, ACEDrawbackJobComInvoiceLineValidation.DescriptionIsTooLong);
		}

		public void TestCheckUS_OA_DRWExporterOrDestroyer()
		{
			InvoiceLine.US_DRWIsForExportSection = true;
			InvoiceLine.US_OA_DRWExporterOrDestroyer = ZGuid.Empty;
			InvoiceLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(InvoiceLine.US_OA_DRWExporterOrDestroyerInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAAAAA";
			InvoiceLine.ExporterOrDestroyer.OrganisationPK = orgHeader.PK;
			InvoiceLine.Validation.ValidateAll();
			Assert(!InvoiceLine.US_OA_DRWExporterOrDestroyer.IsEmpty);
			AssertNoMessageErrorContaining(InvoiceLine.US_OA_DRWExporterOrDestroyerInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
		}
	}
}
