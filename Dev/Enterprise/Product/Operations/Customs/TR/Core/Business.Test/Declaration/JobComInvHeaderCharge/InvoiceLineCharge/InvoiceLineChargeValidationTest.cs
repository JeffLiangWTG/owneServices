using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineChargeValidation))]
	sealed class InvoiceLineChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJ7_RX_NKCurrency_LocalChargesMustBeTRY()
		{
			var charge = invoiceLine.Charges.AddNew(TRIncotermChargeCodeList.Codes.LocalCultureCharge);
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var targetInfo = charge.J7_RX_NKCurrencyInfo;
			var localChargeMessage = "can only be TRY.";
			AssertHasMessageErrorContaining("Currency of Local charges must be TRY", targetInfo, localChargeMessage);

			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;
			AssertNoMessageErrorContaining("Currency of Local charges must be TRY(validation pass)", targetInfo, localChargeMessage);

			var foreighCharge = invoiceLine.Charges.AddNew(TRIncotermChargeCodeList.Codes.ROY);
			foreighCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertNoMessageErrorContaining("Currency of Foreign charges can be other than TRY", foreighCharge.J7_RX_NKCurrencyInfo, localChargeMessage);
		}

		public void TestCheckCurrencyForForeignTypeCode()
		{
			var invoiceChargeTFC = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.COM);
			invoiceChargeTFC.J7_Amount = 360;
			invoiceChargeTFC.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLineChargeCOM = invoiceLine.Charges.AddNew();
			invoiceLineChargeCOM.J7_ChargeType = TRIncotermChargeCodeList.Codes.COM;
			invoiceLineChargeCOM.J7_Amount = 360;

			AssertEquals("Currency Code", invoiceChargeTFC.J7_RX_NKCurrency, invoiceLineChargeCOM.J7_RX_NKCurrency);

			invoiceLineChargeCOM.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoiceLineChargeCOM.Validation.ValidateJ7_RX_NKCurrency();
			AssertHasMessageErrorContaining("Currency For Foreign Type Code will be same", invoiceLineChargeCOM.J7_RX_NKCurrencyInfo, "COM charges in the Invoice Lines should be equal to the currency of COM charge in the Invoice Header. Invoice Lines: EUR, Invoice Header input: USD");

			invoiceLineChargeCOM.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceLineChargeCOM.Validation.ValidateJ7_RX_NKCurrency();
			AssertNoMessageErrorContaining("Currency For Foreign Type Code will be same", invoiceLineChargeCOM.J7_RX_NKCurrencyInfo, "COM charges in the Invoice Lines should be equal to the currency of COM charge in the Invoice Header. Invoice Lines: EUR, Invoice Header input: USD");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var jobDeclaration = Factory.New<JobDeclaration>();
			invoiceHeader = jobDeclaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		}
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
