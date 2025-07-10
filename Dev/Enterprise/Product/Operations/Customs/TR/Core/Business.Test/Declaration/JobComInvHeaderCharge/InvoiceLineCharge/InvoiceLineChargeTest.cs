using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	sealed class InvoiceLineChargeTest : EU.Business.Declaration.Testing.InvoiceLineChargeTest
	{
		public new void TestValidation()
		{
			var charge = invoiceLine.Charges.AddNew();
			AssertType<InvoiceLineChargeValidation>(charge.Validation);
		}

		public void TeestLookups()
		{
			var charge = invoiceLine.Charges.AddNew();
			AssertType<InvoiceLineChargeLookups>(charge.Lookups);
		}

		public void TestGetCurrencyFromInvoiceChargeWithSameChargeType()
		{
			var invoiceChargeCOM = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.COM);
			invoiceChargeCOM.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLineChargeCOM = invoiceLine.Charges.AddNew();
			invoiceLineChargeCOM.J7_ChargeType = TRIncotermChargeCodeList.Codes.COM;

			var invoiceLineChargeDEM = invoiceLine.Charges.AddNew();
			invoiceLineChargeDEM.J7_ChargeType = TRIncotermChargeCodeList.Codes.DEM;

			CombineAssertions("Charge Type", () =>
			{
				AssertEquals("COM has Invoice Charge | Currency Code", Core.Constants.CurrencyCodes.UnitedStates, invoiceLineChargeCOM.J7_RX_NKCurrency);
				AssertEquals("DEM hasn't Invoice Charge | Currency Code", ZString.Empty, invoiceLineChargeDEM.J7_RX_NKCurrency);
			});
		}

		public void TestJ7_ChargeType_DefaultCurrency_LocalCharge()
		{
			var invoiceLineChargeLBC = invoiceLine.Charges.AddNew();
			invoiceLineChargeLBC.J7_ChargeType = TRIncotermChargeCodeList.Codes.LBC;

			var invoiceLineChargeLSC = invoiceLine.Charges.AddNew();
			invoiceLineChargeLSC.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoiceLineChargeLSC.J7_ChargeType = TRIncotermChargeCodeList.Codes.LSC;

			var invoiceLineChargeXXX = invoiceLine.Charges.AddNew();
			invoiceLineChargeXXX.J7_ChargeType = "XXX";

			CombineAssertions("Charge Type", () =>
			{
				AssertEquals("LocalCharge and J7_RX_NKCurrency is Default TRY", Core.Constants.CurrencyCodes.Turkey, invoiceLineChargeLBC.J7_RX_NKCurrency);
				AssertEquals("LocalCharge and J7_RX_NKCurrency isn't empty", Core.Constants.CurrencyCodes.EuropeanUnion, invoiceLineChargeLSC.J7_RX_NKCurrency);

				AssertEquals("Not LocalCharge", ZString.Empty, invoiceLineChargeXXX.J7_RX_NKCurrency);
				invoiceLineChargeXXX.J7_ChargeType = TRIncotermChargeCodeList.Codes.LBC;
				AssertEquals("LocalCharge and J7_RX_NKCurrency is Default TRY", Core.Constants.CurrencyCodes.Turkey, invoiceLineChargeXXX.J7_RX_NKCurrency);
			});
		}

		public void TestJ7_ChargeType_DefaultCurrency_ForeignCharge()
		{
			var invoiceLineCharge1 = invoiceLine.Charges.AddNew();
			invoiceLineCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoiceLineCharge1.J7_ChargeType = TRIncotermChargeCodeList.Codes.COM;
			var currencyFromInvoiceChargeWithSameChargeType = invoiceLineCharge1.GetCurrencyFromInvoiceChargeWithSameChargeType();

			CombineAssertions("GetCurrencyFromInvoiceChargeWithSameChargeType", () =>
			{
				AssertEquals("currencyFromInvoiceChargeWithSameChargeType is empty", ZString.Empty, currencyFromInvoiceChargeWithSameChargeType);
				AssertEquals("J7_RX_NKCurrency isn't empty", Core.Constants.CurrencyCodes.EuropeanUnion, invoiceLineCharge1.J7_RX_NKCurrency);
			});

			var invoiceCharge = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.COM);
			invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLineCharge2 = invoiceLine.Charges.AddNew();
			invoiceLineCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoiceLineCharge2.J7_ChargeType = TRIncotermChargeCodeList.Codes.COM;
			currencyFromInvoiceChargeWithSameChargeType = invoiceLineCharge2.GetCurrencyFromInvoiceChargeWithSameChargeType();

			CombineAssertions("GetCurrencyFromInvoiceChargeWithSameChargeType", () =>
			{
				AssertNotEquals("currencyFromInvoiceChargeWithSameChargeType isn't empty", ZString.Empty, currencyFromInvoiceChargeWithSameChargeType);
				AssertEquals("J7_RX_NKCurrency is not different", invoiceCharge.J7_RX_NKCurrency, invoiceLineCharge2.J7_RX_NKCurrency);
			});
		}

		public void TestJ7_RX_NKCurrency_ReadOnly()
		{
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.COM;
			invoiceLineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var targetInfo = invoiceLineCharge.J7_RX_NKCurrencyInfo;

			CombineAssertions(() =>
			{
				invoiceLineCharge.J7_Percentage = 0.25m;
				AssertEquals("base.GetJ7_RX_NKCurrency_ReadOnly() is true", true, targetInfo.ReadOnly);

				invoiceLineCharge.J7_Percentage = ZDecimal.Zero;
				AssertEquals("base.GetJ7_RX_NKCurrency_ReadOnly() is false", false, targetInfo.ReadOnly);

				var invoiceCharge = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.COM);
				invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("J7_RX_NKCurrency isn't empty and J7_ChargeType is COM and J7_RX_NKCurrency is same as GetCurrencyFromInvoiceChargeWithSameChargeType()", true, targetInfo.ReadOnly);

				invoiceLineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals("J7_RX_NKCurrency isn't empty and J7_ChargeType is COM and J7_RX_NKCurrency is different to GetCurrencyFromInvoiceChargeWithSameChargeType()", false, targetInfo.ReadOnly);

				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LDC;
				invoiceLineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("J7_RX_NKCurrency isn't empty and J7_ChargeType isn't foreign and J7_RX_NKCurrency is same as GetCurrencyFromInvoiceChargeWithSameChargeType()", false, targetInfo.ReadOnly);

				invoiceCharge.J7_RX_NKCurrency = ZString.Empty;
				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.COM;
				invoiceLineCharge.J7_RX_NKCurrency = ZString.Empty;
				AssertEquals("J7_RX_NKCurrency is empty and J7_ChargeType is COM and J7_RX_NKCurrency is same as GetCurrencyFromInvoiceChargeWithSameChargeType()", false, targetInfo.ReadOnly);
			});
		}

		public void TestExplanationReadOnlyForDifferentChargeTypes()
		{
			var invoiceLineCharge = invoiceLine.Charges.AddNew();

			CombineAssertions("Explanation Read-Only status for different Charge Types", () =>
			{
				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.COM;
				AssertEquals(invoiceLineCharge.ExplanationInfo.ReadOnly, true);

				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.OTH;
				AssertEquals(invoiceLineCharge.ExplanationInfo.ReadOnly, false);

				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LOT;
				AssertEquals(invoiceLineCharge.ExplanationInfo.ReadOnly, false);

				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LDC;
				AssertEquals(invoiceLineCharge.ExplanationInfo.ReadOnly, true);
			});
		}

		public void TestExplanationMaxLengthExceedAndAttributeSet()
		{
			var invoiceLineCharge = invoiceLine.Charges.AddNew();

			var maxLength = 100;
			var additionalMaxLengthExplanation = new ZString(new string('A', maxLength));

			invoiceLineCharge.Explanation = additionalMaxLengthExplanation;

			AssertEquals("The Explanation field should not exceed the maximum length of 100 characters", additionalMaxLengthExplanation, invoiceLineCharge.Explanation);

			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(InvoiceLineCharge), nameof(InvoiceLineCharge.Explanation), false, attr => attr.MaxLength == 100);
		}

		public void TestClearExplanationIfRequired()
		{
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			invoiceLineCharge.Explanation = "Test Explanation";

			CombineAssertions("Clear Explanation If Required", () =>
			{
				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.OTH;
				AssertEquals(invoiceLineCharge.Explanation, "Test Explanation");

				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.COM;
				AssertEquals(invoiceLineCharge.Explanation, ZString.Empty);

				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LOT;
				invoiceLineCharge.Explanation = "Test Explanation";
				AssertEquals(invoiceLineCharge.Explanation, "Test Explanation");

				invoiceLineCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LDC;
				AssertEquals(invoiceLineCharge.Explanation, ZString.Empty);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => invoiceLine.Charges.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			var jobDeclaration = Factory.New<JobDeclaration>();
			invoiceHeader = jobDeclaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		}
		JobComInvoiceHeader invoiceHeader;
		new JobComInvoiceLine invoiceLine;
	}
}
