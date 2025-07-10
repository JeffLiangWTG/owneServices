using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BulkExchangeRateUpdater))]
	sealed class BulkExchangeRateUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		BulkExchangeRateUpdater TestUpdater;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkExchangeRateUpdater(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestUpdater = (BulkExchangeRateUpdater)GetNewBusinessObject();
		}

		#endregion

		[TestDate(2004, 4, 4)]
		public void TestPostingToDatabase()
		{
			ExchangeRateWrapper wrapper1 = TestUpdater.ExchangeRateWrappers.AddNew();

			//ZQuery Filter = new ZQuery();
			//Filter.MaximumRows = 2;
			//RefCurrency [] Currencies = (RefCurrency[]) Factory.Load(typeof(RefCurrency), Filter);
			//Wrapper1.Currency = Currencies[0];

			ZString expectedCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			wrapper1.CurrencyNK = expectedCurrency;

			wrapper1.BuyStartDate = new ZDateTime(2004, 1, 1);
			wrapper1.BuyExpiryDate = new ZDateTime(2004, 2, 2);
			wrapper1.BuyRate = new ZDecimal(1.2);

			wrapper1.SellStartDate = new ZDateTime(2004, 3, 3);
			wrapper1.SellExpiryDate = new ZDateTime(2004, 4, 4);
			wrapper1.SellRate = new ZDecimal(3.4);

			ZGuid buyExRateBizOPK = wrapper1.BuyExchangeRateBizO.PK;
			ZGuid sellExRateBizOPK = wrapper1.SellExchangeRateBizO.PK;

			Factory.Save();

			var originalBuyExRateBizO = Factory.Load<RefExchangeRate>(buyExRateBizOPK);
			var originalSellExRateBizO = Factory.Load<RefExchangeRate>(sellExRateBizOPK);

			AssertEquals(expectedCurrency, originalBuyExRateBizO.RE_RX_NKExCurrency);
			AssertEquals(new ZDateTime(2004, 1, 1), originalBuyExRateBizO.RE_StartDate);
			AssertEquals(new ZDateTime(2004, 2, 2, 23, 59, 0), originalBuyExRateBizO.RE_ExpiryDate);
			AssertEquals(new ZDecimal(1.2), originalBuyExRateBizO.RE_SellRate);
		}

		[TestDate(2004, 4, 4)]
		public void TestLogsAreAddedToCurrency()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			var exchangeRateWraper = TestUpdater.ExchangeRateWrappers.AddNew();
			var oldLogsCountForBuyExchangeRate = exchangeRateWraper.BuyExchangeRateBizO.Logs.DatabaseCount;
			var oldLogsCountForSellExchangeRate = exchangeRateWraper.SellExchangeRateBizO.Logs.DatabaseCount;

			ZString currencyCode = currency.RX_Code;
			exchangeRateWraper.CurrencyNK = currencyCode;
			exchangeRateWraper.BuyStartDate = new ZDateTime(2004, 1, 1);
			exchangeRateWraper.BuyExpiryDate = new ZDateTime(2004, 2, 2);
			exchangeRateWraper.BuyRate = new ZDecimal(1.2);
			exchangeRateWraper.SellStartDate = new ZDateTime(2004, 3, 3);
			exchangeRateWraper.SellExpiryDate = new ZDateTime(2004, 4, 4);
			exchangeRateWraper.SellRate = new ZDecimal(3.4);
			Factory.Save();

			var newLogsCountForBuyExchangeRate = exchangeRateWraper.BuyExchangeRateBizO.Logs.DatabaseCount;
			var newLogsCountForSellExchangeRate = exchangeRateWraper.SellExchangeRateBizO.Logs.DatabaseCount;

			AssertEquals("There should be 1 log created for the buy exchange rate.", 1, newLogsCountForBuyExchangeRate - oldLogsCountForBuyExchangeRate);
			AssertEquals("There should be 1 log created for the sell exchange rate.", 1, newLogsCountForSellExchangeRate - oldLogsCountForSellExchangeRate);
		}

		[TestDate(2004, 4, 4)]
		public void TestValidationChecksNonSavedExchangeRates()
		{
			var testCurrency = Factory.NewWithValidTestData<RefCurrency>();
			Factory.Save();

			ExchangeRateWrapper wrapper1 = TestUpdater.ExchangeRateWrappers.AddNew();
			wrapper1.CurrencyNK = testCurrency.RX_Code;
			wrapper1.BuyStartDate = new ZDateTime(2004, 4, 4);
			wrapper1.BuyRate = new ZDecimal(0.2);

			Assert("The bulk updater should have no errors because none of the wrapper objects should have objects (W1).\n" +
				String.Join("\n", TestUpdater.Notifications.Select(x => x.Type == NotificationType.Error).Take(5)),
				!TestUpdater.HasErrors);

			ExchangeRateWrapper wrapper2 = TestUpdater.ExchangeRateWrappers.AddNew();
			wrapper2.CurrencyNK = testCurrency.RX_Code;
			wrapper2.BuyRate = new ZDecimal(0.5);

			Assert("The bulk updater should have no errors because none of the wrapper objects should have objects (W2).\n" +
				String.Join("\n", TestUpdater.Notifications.Select(x => x.Type == NotificationType.Error).Take(5)),
				!TestUpdater.HasErrors);

			wrapper1.BuyExpiryDate = new ZDateTime(2004, 4, 5);
			Assert("Wrapper1 should have errors since the expiry date overlaps with Wrapper2",
				wrapper1.HasErrors);
		}

		public void TestExchangeRateDecimalPlaces()
		{
			ZBool originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;

			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				AssertEquals("ExchangeRate decimal places should be 6", 6, TestUpdater.ExchangeRateDecimalPlaces);

				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
				AssertEquals("ExchangeRate decimal places should be 6", 6, TestUpdater.ExchangeRateDecimalPlaces);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
			}
		}
	}
}
