using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CurrencyExchangeTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCurrencyTypeCode()
		{
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			NUnit.Framework.Assert.That(currencyExchange.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("EUR").Using(CustomComparers.TypeComparison));
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			NUnit.Framework.Assert.That(currencyExchange.CurrencyTypeCode, NUnit.Framework.Is.EqualTo(Core.Constants.CurrencyCodes.UnitedStates).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRateNumeric()
		{
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice1.JZ_InvoiceCurrExRate = 35.60m;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice2.JZ_InvoiceCurrExRate = 31.37m;
			NUnit.Framework.Assert.That(currencyExchange.RateNumeric, NUnit.Framework.Is.EqualTo(35.60m).Using(CustomComparers.TypeComparison));
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_InvoiceCurrExRate = 31.37m;
			NUnit.Framework.Assert.That(currencyExchange.RateNumeric, NUnit.Framework.Is.EqualTo(31.37m).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			var entryHeader = testHelper.CreateEntryHeaderForN5203();
			invoice1 = testHelper.CreateInvoiceLineForN5203(entryHeader).InvoiceHeader;
			invoice2 = testHelper.CreateInvoiceLineForN5203(entryHeader).InvoiceHeader;
			currencyExchange = new CurrencyExchange(entryHeader);
		}

		JobComInvoiceHeader invoice1;
		JobComInvoiceHeader invoice2;
		ICurrencyExchange currencyExchange;
	}
}
