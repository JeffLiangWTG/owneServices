using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USACEFDAAddInfo))]
	public class USACEFDAAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetNewValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(fda.AddInfoValidation.GetType(), typeof(USACEFDAAddInfoInvoiceLineValidation));

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			AssertEquals(fdaOnProduct.AddInfoValidation.GetType(), typeof(USACEFDAAddInfoProductValidation));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var fda = Factory.New<ACEFDA>();
			var addInfo = new USACEFDAAddInfo(fda.B7_AddInfoDataInfo);
			return addInfo;
		}
	}
}
