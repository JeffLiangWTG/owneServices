using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobComInvoiceHeaderTestForDocumentWrapper : BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		[TestDate(1971, 9, 18)]
		public void TestEffectiveValuationDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals(declaration.DateOfValuation, invoiceHeader.EffectiveValuationDate);
			declaration.JE_MasterBillIssuedDate = new ZDateTime(1972, 9, 18);
			AssertEquals(ZDateTime.Today.AddDays(-1), invoiceHeader.EffectiveValuationDate);
			declaration.JE_MessageType = "IMP";
			AssertEquals(new ZDateTime(1972, 9, 18), invoiceHeader.EffectiveValuationDate);
		}

		public override void TestCheckingValueOfJZ_Calc_ConversionFactor()
		{
			CombineAssertions("CIF", () =>
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				SetExchangeRate(ZDateTime.Now, ZDateTime.Now.AddDays(1), 0.5m, uSDCurrency, "CUS");

				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = invoice.InvoiceLines.AddNew();

				testInvoice.JZ_InvoiceAmount = 10000m;
				testInvoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
				testInvoice.JZ_IncoTerm = "CIF";

				testInvoice.Charges.RemoveAll();
				BaseJobComInvHeaderCharge oFT = testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
				oFT.J7_IsIncludedInITOT = true;
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);

				testDeclaration.ResumeApportionment();

				var line = testInvoice.InvoiceLines.AddNew();
				line.JI_LinePrice = 900;
				testDeclaration.ResumeApportionment();
				AssertEquals(0.88888889m, testInvoice.JZ_Calc_ConversionFactor);
			});

			CombineAssertions("FOB", () =>
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				SetExchangeRate(ZDateTime.Now, ZDateTime.Now.AddDays(1), 0.5m, uSDCurrency, "CUS");

				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = invoice.InvoiceLines.AddNew();

				testInvoice.JZ_InvoiceAmount = 10000m;
				testInvoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
				testInvoice.JZ_IncoTerm = "FOB";

				testInvoice.Charges.RemoveAll();
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);

				testDeclaration.ResumeApportionment();

				var line = testInvoice.InvoiceLines.AddNew();
				line.JI_LinePrice = 900;

				testDeclaration.ResumeApportionment();
				AssertEquals(2m, testInvoice.JZ_Calc_ConversionFactor);
			});

			CombineAssertions("FOB with INT ON Line", () =>
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				SetExchangeRate(ZDateTime.Now, ZDateTime.Now.AddDays(1), 0.5m, uSDCurrency, "CUS");

				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = invoice.InvoiceLines.AddNew();

				testInvoice.JZ_InvoiceAmount = 10000m;
				testInvoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
				testInvoice.JZ_IncoTerm = "FOB";

				testInvoice.Charges.RemoveAll();
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);

				testDeclaration.ResumeApportionment();

				var line = testInvoice.InvoiceLines.AddNew();
				line.JI_LinePrice = 2900;
				var lineINT = line.Charges.AddNew();
				lineINT.J7_ChargeType = "INT";
				lineINT.J7_Amount = 2000;

				testDeclaration.ResumeApportionment();
				AssertEquals(2m, testInvoice.JZ_Calc_ConversionFactor);
			});

			CombineAssertions("CIF IN LINE with INT", () =>
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				SetExchangeRate(ZDateTime.Now, ZDateTime.Now.AddDays(1), 0.5m, uSDCurrency, "CUS");

				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = invoice.InvoiceLines.AddNew();

				testInvoice.JZ_InvoiceAmount = 10000m;
				testInvoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
				testInvoice.JZ_IncoTerm = "CIF";

				testInvoice.Charges.RemoveAll();
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);

				testDeclaration.ResumeApportionment();

				var line = testInvoice.InvoiceLines.AddNew();
				line.JI_LinePrice = 2900;
				var lineINT = line.Charges.AddNew();
				lineINT.J7_ChargeType = "INT";
				lineINT.J7_Amount = 2000;

				testDeclaration.ResumeApportionment();
				AssertEquals(2m, testInvoice.JZ_Calc_ConversionFactor);
			});
		}

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
