using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class InvoiceHeaderValidationTest : TestCaseWithFactory
	{
		public void TestValidateExchangeRateEvenWhenAmountIsEmpty()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "NEW";
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency for ");
		}

		public void TestValidateJZ_IncoTerm()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			InvoiceHeaderValidatorClass testValidator = new InvoiceHeaderValidatorClass(invoice);
			testValidator.ValidateJZ_IncoTerm();
			AssertEquals("Has an error", true, invoice.JZ_IncoTermInfo.HasErrors());

			invoice.JZ_IncoTerm = "DAF";
			AssertNoErrors(invoice.JZ_IncoTermInfo);
			AssertHasMessageError(invoice.JZ_IncoTermInfo, "The code you have selected is not in the list.");
			invoice.JZ_IncoTerm = "DES";
			AssertHasMessageError(invoice.JZ_IncoTermInfo, "The code you have selected is not in the list.");
			invoice.JZ_IncoTerm = "DEQ";
			AssertHasMessageError(invoice.JZ_IncoTermInfo, "The code you have selected is not in the list.");
			invoice.JZ_IncoTerm = "DDU";
			AssertHasMessageError(invoice.JZ_IncoTermInfo, "The code you have selected is not in the list.");
			invoice.JZ_IncoTerm = "DDP";
			AssertNoMessageErrors(invoice.JZ_IncoTermInfo);
			invoice.JZ_IncoTerm = "";
			AssertHasMessageErrorContaining(invoice.JZ_IncoTermInfo, MandatoryValidation.MustBeEnteredMessage(invoice.JZ_IncoTermInfo.Description));
		}

		public void TestCheckJZ_NoOfPacks()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_NoOfPacks = -1m;
			AssertHasErrors(invoice.JZ_NoOfPacksInfo);

			invoice.JZ_NoOfPacks = 10m;
			AssertNoErrors(invoice.JZ_NoOfPacksInfo);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MiscServ = Factory.New<OrgMiscServ>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.Importer.MiscServ.OM_IMBalanceInvoicePackage = true;
			invoice.JZ_NoOfPacks = 0m;
			AssertHasMessageError(invoice.JZ_NoOfPacksInfo, "No. of Packages is mandatory.");
			invoice.JZ_NoOfPacks = 1m;
			AssertNoMessageError(invoice.JZ_NoOfPacksInfo, "No. of Packages is mandatory.");

			declaration.Importer.MiscServ.OM_IMBalanceInvoicePackage = false;
			invoice.JZ_NoOfPacks = 0m;
			AssertNoMessageError(invoice.JZ_NoOfPacksInfo, "No. of Packages is mandatory.");
			invoice.JZ_NoOfPacks = 1m;
			AssertNoMessageError(invoice.JZ_NoOfPacksInfo, "No. of Packages is mandatory.");
		}

		public void TestCheckJZ_NetWeight()
		{
			string warning = "not be greater than";
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 0m;
			invoice.JZ_WeightUQ = ZString.Empty;
			invoice.JZ_NetWeight = 0m;
			invoice.JZ_NetWeightUQ = ZString.Empty;
			AssertNoWarning(invoice.JZ_NetWeightInfo, warning);

			invoice.JZ_Weight = 10m;
			invoice.JZ_NetWeight = 11m;
			AssertNoWarning(invoice.JZ_NetWeightInfo, warning);

			invoice.JZ_WeightUQ = "XX";
			invoice.JZ_NetWeightUQ = "XX";
			AssertNoWarning(invoice.JZ_NetWeightInfo, warning);

			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoWarning(invoice.JZ_NetWeightInfo, warning);
			invoice.JZ_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertHasWarningContaining(invoice.JZ_NetWeightInfo, warning);
			invoice.JZ_NetWeight = 10m;
			AssertNoWarning(invoice.JZ_NetWeightInfo, warning);
			invoice.JZ_WeightUQ = Core.Constants.Weight.Grams;
			AssertHasWarningContaining(invoice.JZ_NetWeightInfo, warning);
			invoice.JZ_Weight = 10001m;
			AssertNoWarning(invoice.JZ_NetWeightInfo, warning);
		}
	}
}
