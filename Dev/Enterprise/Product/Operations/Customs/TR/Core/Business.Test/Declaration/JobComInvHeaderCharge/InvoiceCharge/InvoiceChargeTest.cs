using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	public class InvoiceChargeTest : EU.Business.Declaration.Testing.InvoiceChargeTest
	{
		public void TestLookups()
		{
			var charge = invoice.Charges.AddNew();
			AssertType<InvoiceChargeLookups>(charge.Lookups);
		}

		public void TestValidation()
		{
			AssertType<InvoiceChargeValidation>(Factory.New<InvoiceCharge>().Validation);
		}

		public override void TestIsIncludedInLinesReadOnly()
		{
			invoice.JZ_IncoTerm = "FOB";

			var internationalFreightCharge = invoice.Charges.AddNew(TRIncotermChargeCodeList.Codes.OFT);
			AssertEquals("International Freight Charge ITOT should be read only", true, internationalFreightCharge.J7_IsIncludedInITOTInfo.ReadOnly);

			var internationalInsuranceCharge = invoice.Charges.AddNew(TRIncotermChargeCodeList.Codes.ONS);
			AssertEquals("International Freight Charge ITOT should be read only", true, internationalInsuranceCharge.J7_IsIncludedInITOTInfo.ReadOnly);
		}

		public override void TestJ7_Calc_IsIncludedInInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			invoice.JZ_IncoTerm = GetIncotermToTestIsIncludedInInvoice();

			var chargeLTC = invoice.Charges.AddNew(TRIncotermChargeCodeList.Codes.LocalTotalCharges);
			AssertEquals("IsIncludedInInvoiceAmount read only for LTC.", true, chargeLTC.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			var chargeTFC = invoice.Charges.AddNew(TRIncotermChargeCodeList.Codes.TotalForeignCharges);
			AssertEquals("IsIncludedInInvoiceAmount read only for TFC.", true, chargeTFC.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			var chargeOFT = invoice.Charges.AddNew(TRIncotermChargeCodeList.Codes.INT);
			AssertEquals("IsIncludedInInvoiceAmount returns base for other charges.", false, chargeOFT.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
		}

		public void TestJ7_ChargeType_SetCurrency_LocalCharge()
		{
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = string.Empty;
			AssertEquals("Not set J7_RX_NKCurrency when empty J7_ChargeType.", string.Empty, charge.J7_RX_NKCurrency);

			charge.J7_ChargeType = "XXX";
			AssertEquals("Not set J7_RX_NKCurrency when invalid J7_ChargeType.", string.Empty, charge.J7_RX_NKCurrency);

			charge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LocalCultureCharge;
			AssertEquals("Set J7_RX_NKCurrency when J7_ChargeType being foreign Charge.", Core.Constants.CurrencyCodes.Turkey, charge.J7_RX_NKCurrency);
		}

		public void TestJ7_ChargeType_SetCurrency_ForeignCharge()
		{
			var charge = invoice.Charges.AddNew(TRIncotermChargeCodeList.Codes.ROY);
			AssertEquals("Not set J7_RX_NKCurrency when InvoiceHeader has no Currency.", string.Empty, charge.J7_RX_NKCurrency);

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			charge.J7_ChargeType = string.Empty;
			AssertEquals("Not set J7_RX_NKCurrency when empty J7_ChargeType.", string.Empty, charge.J7_RX_NKCurrency);
			charge.J7_ChargeType = "XXX";
			AssertEquals("Not set J7_RX_NKCurrency when invalid J7_ChargeType.", string.Empty, charge.J7_RX_NKCurrency);

			charge.J7_ChargeType = TRIncotermChargeCodeList.Codes.ROY;
			AssertEquals("Set J7_RX_NKCurrency when J7_ChargeType being foreign Charge.", Core.Constants.CurrencyCodes.UnitedStates, charge.J7_RX_NKCurrency);
		}

		public void TestCaptionsTR()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(InvoiceCharge), nameof(InvoiceCharge.ChargeCodeDescription), false,
				x => x.Caption == "Description" && x.ShortCaption == "Desc."
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(InvoiceCharge), nameof(InvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount), false,
				x => x.Caption == "Included in Invoice Amount" && x.ShortCaption == "In Invoice Amt"
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(InvoiceCharge), nameof(InvoiceCharge.J7_IsStatisticalValueApplicable), false,
				x => x.Caption == "Included In FOB" && x.ShortCaption == "In FOB"
			);
		}

		public void GetJ7_RX_NKCurrency_ReadOnly()
		{
			var charge = invoice.Charges.AddNew();
			var targetInfo = charge.J7_RX_NKCurrencyInfo;

			charge.J7_Percentage = 20;
			AssertEquals("Base: J7_RX_NKCurrency read only when J7_Percentage has value.", true, targetInfo.ReadOnly);
			charge.J7_Percentage = 0;
			AssertEquals("Base: J7_RX_NKCurrency writable when J7_Percentage empty.", false, targetInfo.ReadOnly);

			charge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LocalCultureCharge;
			AssertEquals("TR: J7_RX_NKCurrency read only when is local charge.", true, targetInfo.ReadOnly);

			charge.J7_ChargeType = TRIncotermChargeCodeList.Codes.ROY;
			AssertEquals("TR: J7_RX_NKCurrency writable when not local charge.", false, targetInfo.ReadOnly);
		}

		protected override void SetupAllTestObjects()
		{
			testDec = Factory.New<JobDeclaration>();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			base.invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testCharge = base.invoice.Charges.AddNew();
		}

		new JobComInvoiceHeader invoice => (JobComInvoiceHeader)base.invoice;
	}
}

