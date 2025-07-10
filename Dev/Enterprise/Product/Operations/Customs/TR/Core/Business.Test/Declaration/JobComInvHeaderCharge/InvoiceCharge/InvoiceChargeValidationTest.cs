using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceChargeValidation))]
	class InvoiceChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJ7_RX_NKCurrency()
		{
			var currencyMustBeMessage = "Currency of Local charges must be TRY.";
			var charge = invoiceHeader.Charges.AddNew();
			var targetInfo = charge.J7_RX_NKCurrencyInfo;

			charge.J7_ChargeType = TRIncotermChargeCodeList.Codes.OFT;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertNoMessageError("No check for non-LTC charges.", targetInfo, currencyMustBeMessage);

			charge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge.Validation.ValidateJ7_RX_NKCurrency();
			AssertHasMessageError("Check for LEC charges.", targetInfo, currencyMustBeMessage);

			charge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LocalTotalCharges;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge.Validation.ValidateJ7_RX_NKCurrency();
			AssertHasMessageError("Check for LTC charges.", targetInfo, currencyMustBeMessage);

			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;
			charge.Validation.ValidateJ7_RX_NKCurrency();
			AssertNoMessageError("Check for LTC charges(validation passes).", targetInfo, currencyMustBeMessage);
		}

		public void TestCheckJ7_Amount_LineLTCSum_ShouldEqualTo_InvoiceLTC()
		{
			invoiceHeader.JZ_InvoiceAmount = 3600;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var invoiceCharge = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.LocalTotalCharges);
			invoiceCharge.J7_Amount = 360;
			invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1200;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2400;

			jobDeclaration.ResumeApportionment();

			invoiceLine1.ApportionedCharges.Single(charge => charge.J7_ChargeType == TRIncotermChargeCodeList.Codes.LocalTotalCharges).J7_Amount = 121;
			invoiceCharge.Validation.ValidateJ7_Amount();
			var targetInfo = invoiceCharge.J7_AmountInfo;

			AssertHasMessageErrorContaining(
				"Inv. LTC Charge should equal to Sum of lines LTC Charges.",
				targetInfo,
				"Sum of LTC charges in the Invoice Lines should be equal to the amount of LTC charge in the Invoice Header."
			);

			invoiceCharge.J7_Amount = 361;
			AssertNoMessageErrorContaining(
				"Inv. LTC Charge should equal to Sum of lines LTC Charges(validation pass).",
				targetInfo,
				"Sum of LTC charges in the Invoice Lines should be equal to the amount of LTC charge in the Invoice Header."
			);
		}

		public void TestCheckJ7_Amount_LineTFCSum_ShouldEqualTo_InvoiceTFC()
		{
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.Turkey, 1, ZDateTime.Today, Factory, ExchangeRateType.Customs);
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 19.33, ZDateTime.Today, Factory, ExchangeRateType.Customs);
			jobDeclaration.Company.GC_IsReciprocal = true;
			Factory.Save();

			invoiceHeader.JZ_InvoiceAmount = 3600;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var invoiceCharge = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.TotalForeignCharges);
			invoiceCharge.J7_Amount = 360;
			invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1200;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2400;

			jobDeclaration.ResumeApportionment();

			var invoiceLineCharge1 = invoiceLine1.ApportionedCharges.Single(charge => charge.J7_ChargeType == TRIncotermChargeCodeList.Codes.TotalForeignCharges);
			invoiceLineCharge1.J7_Amount = 121;
			var invoiceLineCharge2 = invoiceLine2.ApportionedCharges.Single(charge => charge.J7_ChargeType == TRIncotermChargeCodeList.Codes.TotalForeignCharges);
			invoiceLineCharge2.J7_Amount = 240;

			invoiceCharge.Validation.ValidateJ7_Amount();
			var targetInfo = invoiceCharge.J7_AmountInfo;
			AssertHasMessageErrorContaining(
				"Inv. TFC Charge should equal to Sum of lines TFC Charges.",
				targetInfo,
				"Sum of TFC charges in the Invoice Lines should be equal to the amount of TFC charge in the Invoice Header."
			);

			invoiceLine1.Charges.AddNew(TRIncotermChargeCodeList.Codes.TotalForeignCharges).J7_Amount = 1;

			invoiceCharge.Validation.ValidateJ7_Amount();
			AssertHasMessageErrorContaining(
				"Inv. TFC Charge should equal Sum of lines TFC Charges(also including non-apportioned charges).",
				targetInfo,
				"Sum of TFC charges in the Invoice Lines should be equal to the amount of TFC charge in the Invoice Header."
			);

			invoiceCharge.J7_Amount = 362;
			AssertNoMessageErrorContaining(
				"Inv. TFC Charge should equal Sum of lines TFC Charges(also including non-apportioned charges, validation pass).",
				targetInfo,
				"Sum of TFC charges in the Invoice Lines should be equal to the amount of TFC charge in the Invoice Header."
			);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			invoiceHeader = jobDeclaration.Invoices.AddNew();
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceHeader invoiceHeader;
	}
}
