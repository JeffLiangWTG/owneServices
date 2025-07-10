using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageCurrencyExchange))]
	sealed class LicensingMessageCurrencyExchangeTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(currencyExchange.CurrencyTypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "CurrencyTypeCode");
				NUnit.Framework.Assert.That(currencyExchange.RateNumeric, NUnit.Framework.Is.EqualTo(ZDecimal.Zero), "RateNumeric");
			});
			var invoices = declaration.Invoices;
			var invoiceHeader1 = invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader1.JZ_InvoiceCurrExRate = 2.1m;
			invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(currencyExchange.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "CurrencyTypeCode");
				NUnit.Framework.Assert.That(currencyExchange.RateNumeric, NUnit.Framework.Is.EqualTo(2.1m).Using(CustomComparers.TypeComparison), "RateNumeric");
			});
			invoices.AddNew().JZ_RX_NKInvoice_Currency = "JPY";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(currencyExchange.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison), "CurrencyTypeCode");
				NUnit.Framework.Assert.That(currencyExchange.RateNumeric, NUnit.Framework.Is.EqualTo(2.1m).Using(CustomComparers.TypeComparison), "RateNumeric");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			currencyExchange = new LicensingMessageCurrencyExchange(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		LicensingMessageCurrencyExchange currencyExchange;
	}
}
