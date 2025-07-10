using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class CommercialInvoiceChargeWrapperTest : DocumentWrapperTest
	{
		InvoiceCharge GenerateInvoiceCharge()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			invoiceCharge.J7_ChargeDescription = "Other Additional Charges";
			invoiceCharge.J7_Amount = 50m;
			invoiceCharge.J7_RX_NKCurrency = "TWD";
			invoiceCharge.J7_ExchangeRate = 1m;
			return invoiceCharge;
		}

		CommercialInvoiceChargeWrapper GetInvoiceChargeWrapper(InvoiceCharge invoiceCharge, BusinessObjectFactory factory)
		{
			return CommercialInvoiceChargeWrapper.New(invoiceCharge, factory);
		}

		[TestDate(2021, 02, 26)]
		public void TestPrice()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30m, new ZDateTime(2021, 02, 26), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "FORGOTTEN";
			invoiceHeader.JZ_InvoiceAmount = 5434.44m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";

			InvoiceCharge invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_Amount = 3000m;
			invoiceCharge.J7_ChargeType = "ADD";
			invoiceCharge.J7_RX_NKCurrency = "TWD";
			var wrapper = CommercialInvoiceChargeWrapper.New(invoiceCharge, Factory);
			AssertEquals(100m, wrapper.Price.Amount);
			AssertEquals("USD", wrapper.Price.Currency.Code);
		}

		public void TestInvoiceChargeDocumentData()
		{
			var invoiceCharge = GenerateInvoiceCharge();
			var wrapper = GetInvoiceChargeWrapper(invoiceCharge, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("ADD", wrapper.ChargeType);
				AssertEquals("Other Additional Charges", wrapper.ChargeDescription);
				AssertEquals(50m, wrapper.Amount);
				AssertEquals("TWD", wrapper.Currency);
			});
		}

		public void TestChargeDescription()
		{
			var invoiceCharge = GenerateInvoiceCharge();
			var wrapper = GetInvoiceChargeWrapper(invoiceCharge, Factory);

			AssertEquals("Other Additional Charges", wrapper.ChargeDescription);

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			invoiceCharge.J7_ChargeDescription = "國際運費";
			wrapper = GetInvoiceChargeWrapper(invoiceCharge, Factory);

			AssertEquals("國際運費", wrapper.ChargeDescription);

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			invoiceCharge.J7_ChargeDescription = ZString.Empty;
			wrapper = GetInvoiceChargeWrapper(invoiceCharge, Factory);

			AssertEquals("International Insurance", wrapper.ChargeDescription);
		}
	}
}
