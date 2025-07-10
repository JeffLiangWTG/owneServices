using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USACEFDAAddInfoProductValidationTest : USACEFDAAddInfoValidationTest
	{
		public void TestContainerValidationForProduct()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			var pivot = product.PivotsForBinding.AddNew();

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000400000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";

			pivot.CI_TariffNum = importTariff.UE_Tariff;

			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			fdaOnProduct.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fdaOnProduct.US_FDAForcePN = true;
			AssertNoExceptionThrown(delegate
			{ fdaOnProduct.AddInfoValidation.ValidateAll(); });
		}

		public void TestCheckQuantity()
		{
			FDA.US_UQ1 = "KG";
			FDA.US_Qty1 = 10m;
			FDA.US_UQ2 = "CA";
			FDA.US_Qty2 = 5m;
			FDA.US_UQ3 = "CS";
			FDA.US_Qty3 = 0m;
			FDA.AddInfoValidation.ValidateAll();
			AssertNoWarning(FDA.US_Qty3Info, "Quantity should not be entered here as it will be calculated when the invoice line is entered.");

			FDA.US_Qty3 = 10m;
			FDA.AddInfoValidation.ValidateAll();
			AssertHasWarning(FDA.US_Qty3Info, "Quantity should not be entered here as it will be calculated when the invoice line is entered.");
		}

		protected override ACEFDA FDA
		{
			get
			{
				if (fda == null)
				{
					var importer = Factory.NewWithValidTestData<OrgHeader>();
					var product = Factory.New<OrgSupplierPart>();
					product.OP_PartNum = "Test";
					var relOrg = product.RelatedOrganisations.AddNew();
					relOrg.OU_OH = importer.PK;
					relOrg.OU_Relationship = "OWN";
					var pivot = product.PivotsForBinding.AddNew();
					pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;

					fda = pivot.ACEFDAs.AddNew();
				}
				return fda;
			}
		}
		ACEFDA fda;
	}
}
