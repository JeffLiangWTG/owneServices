using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseOnlyInvoiceLineChargeTest : TestCaseWithFactory
	{
		public void TestMoneyInInvoiceCurrency()
		{
			RefCurrency foreignCurrency = Factory.New<RefCurrency>();
			foreignCurrency.RX_Code = "XXX";

			RefExchangeRate rate = foreignCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = new ZDateTime(2005, 1, 1);
			rate.RE_ExpiryDate = new ZDateTime(2005, 1, 1);
			rate.RE_SellRate = 0.5m;

			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.DateOfValuation).Returns(new ZDateTime(2005, 1, 1));
			BaseJobDeclaration declaration = mockDeclaration.Object;

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			BaseJobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 25000m;
			BaseInvoiceLineCharge charge = invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 5000m, "XXX");
			AssertEquals("Money in invoice currency", 10000m, charge.MoneyInInvoiceCurrency.Amount);
		}
	}
}
