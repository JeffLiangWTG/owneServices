using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineApportionChargeValidation))]
	class InvoiceLineApportionChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJ7_Amount_LTC()
		{
			var localEnvironmentCharge = invoiceLine.Charges.AddNew(TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge);
			localEnvironmentCharge.J7_Amount = 100;

			var apportionLTCCharge = invoiceLine.ApportionedCharges.Single(charge => charge.J7_ChargeType == TRIncotermChargeCodeList.Codes.LocalTotalCharges);
			apportionLTCCharge.Validation.ValidateJ7_Amount();
			var targetInfo = apportionLTCCharge.J7_AmountInfo;
			var localAmountMessage = "Sum of Local charges should be equal to the amount of LTC charge.";
			AssertHasMessageErrorContaining("LTC amount should equal to sum of all Local charges.", targetInfo, localAmountMessage);

			invoiceLine.Charges.AddNew(TRIncotermChargeCodeList.Codes.LocalCultureCharge).J7_Amount = 260;
			apportionLTCCharge.Validation.ValidateJ7_Amount();
			AssertNoMessageErrorContaining("LTC amount should equal to sum of all Local charges(validation pass).", targetInfo, localAmountMessage);
		}

		public void TestCheckJ7_Amount_FTC()
		{
			var royCharge = invoiceLine.Charges.AddNew(TRIncotermChargeCodeList.Codes.ROY);
			royCharge.J7_Amount = 100;

			var apportionLTCCharge = invoiceLine.ApportionedCharges.Single(charge => charge.J7_ChargeType == TRIncotermChargeCodeList.Codes.TotalForeignCharges);
			apportionLTCCharge.Validation.ValidateJ7_Amount();
			var targetInfo = apportionLTCCharge.J7_AmountInfo;
			AssertHasMessageErrorContaining("TFC amount should equal to sum of all Foreign charges.", targetInfo, "Sum of Foreign charges should be equal to the amount of TFC charge.");
		}

		public void TestCheckJ7_RX_NKCurrency_LocalChargesMustBeTRY()
		{
			var apportionLTCCharge = invoiceLine.ApportionedCharges.Single(charge => charge.J7_ChargeType == TRIncotermChargeCodeList.Codes.LocalTotalCharges);
			apportionLTCCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var targetInfo = apportionLTCCharge.J7_RX_NKCurrencyInfo;
			var localChargeMessage = "can only be TRY.";
			AssertHasMessageErrorContaining("Currency of Local charges must be TRY", targetInfo, localChargeMessage);

			apportionLTCCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;
			AssertNoMessageErrorContaining("Currency of Local charges must be TRY(validation pass)", targetInfo, localChargeMessage);

			var apportionTFCCharge = invoiceLine.ApportionedCharges.Single(charge => charge.J7_ChargeType == TRIncotermChargeCodeList.Codes.TotalForeignCharges);
			apportionLTCCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertNoMessageErrorContaining("Currency of Foreign charges can be other than TRY", apportionTFCCharge.J7_RX_NKCurrencyInfo, localChargeMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();

			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.Turkey, 1, ZDateTime.Today, Factory, ExchangeRateType.Customs);
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 19.33, ZDateTime.Today, Factory, ExchangeRateType.Customs);
			jobDeclaration.Company.GC_IsReciprocal = true;
			Factory.Save();

			invoiceHeader = jobDeclaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 3600;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var invoiceChargeLTC = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.LocalTotalCharges);
			invoiceChargeLTC.J7_Amount = 360;
			invoiceChargeLTC.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;

			var invoiceChargeTFC = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.TotalForeignCharges);
			invoiceChargeTFC.J7_Amount = 360;
			invoiceChargeTFC.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 3600;

			jobDeclaration.ResumeApportionment();
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
