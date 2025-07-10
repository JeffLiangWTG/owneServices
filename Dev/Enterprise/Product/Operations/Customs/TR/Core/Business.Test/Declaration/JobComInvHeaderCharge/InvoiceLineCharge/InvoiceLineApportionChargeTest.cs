using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineApportionCharge))]
	class InvoiceLineApportionChargeTest : EU.Business.Declaration.Testing.InvoiceLineApportionChargeTest
	{
		public void TestJ7_RX_NKCurrencyWhenLTC()
		{
			var apportionedLTCCharge = invoiceLine.ApportionedCharges.Single(charge => charge.J7_ChargeType == TRIncotermChargeCodeList.Codes.LocalTotalCharges);
			AssertEquals("Default currency for LTC: TRY", Core.Constants.CurrencyCodes.Turkey, apportionedLTCCharge.J7_RX_NKCurrency);
		}

		public void TestValidation()
		{
			AssertType<InvoiceLineApportionChargeValidation>(invoiceLine.ApportionedCharges.First().Validation);
		}

		public void TestLookups()
		{
			AssertType<InvoiceLineApportionChargeLookups>(invoiceLine.ApportionedCharges.First().Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => invoiceLine.ApportionedCharges.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<InvoiceLineApportionCharge>();
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
