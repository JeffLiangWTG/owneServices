using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACSDrawbackJobComInvoiceLineValidationTest : CommonDrawbackJobComInvoiceLineValidationTest
	{
		public void TestJI_Tariff()
		{
			InvoiceLine.US_DRWIsForImportSection = true;
			declaration.US_PetroleumClaimInd = true;
			InvoiceLine.JI_Tariff = "111111";
			AssertHasMessageErrorContaining(InvoiceLine.JI_TariffInfo, ACSDrawbackJobComInvoiceLineValidation.PetroliumTariffValidation);
			InvoiceLine.JI_Tariff = "11111111";
			AssertNoMessageErrorContaining(InvoiceLine.JI_TariffInfo, ACSDrawbackJobComInvoiceLineValidation.PetroliumTariffValidation);
			declaration.US_PetroleumClaimInd = false;
			InvoiceLine.JI_Tariff = "111111";
			AssertNoMessageErrorContaining(InvoiceLine.JI_TariffInfo, ACSDrawbackJobComInvoiceLineValidation.PetroliumTariffValidation);
			InvoiceLine.JI_Tariff = "4008111000";
			AssertNoMessageErrors(InvoiceLine.JI_TariffInfo);
			InvoiceLine.JI_Tariff = "0000000000";
			AssertHasMessageErrors(InvoiceLine.JI_TariffInfo);
			InvoiceLine.JI_Tariff = "";
			AssertHasMessageErrorContaining(InvoiceLine.JI_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(InvoiceLine.JI_TariffInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
