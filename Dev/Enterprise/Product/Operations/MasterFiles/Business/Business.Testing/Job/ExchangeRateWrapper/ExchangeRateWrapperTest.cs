using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExchangeRateWrapper))]
	public class ExchangeRateWrapperTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		ExchangeRateWrapper TestExchangeRateWrapper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExchangeRateWrapper(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestExchangeRateWrapper = (ExchangeRateWrapper)GetNewBusinessObject();
			year = ZDateTime.Now.Year - 2;
		}

		int year;

		#endregion

		#region TestCompanyFiltering

		public void TestCompanyFiltering()
		{
			var testCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			var testCompany1 = Factory.NewWithValidTestData(typeof(GlbCompany)) as GlbCompany;
			//GlbCompany TestCompany2 = Factory.NewWithValidTestData(typeof(GlbCompany)) as GlbCompany;
			var testBuyExchangeRateDiffComp = Factory.New<RefExchangeRate>();
			testBuyExchangeRateDiffComp.RE_GC = testCompany1.PK;
			testBuyExchangeRateDiffComp.RE_RX_NKExCurrency = testCurrency.RX_Code;
			testBuyExchangeRateDiffComp.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			testBuyExchangeRateDiffComp.RE_SellRate = 0.45M;
			testBuyExchangeRateDiffComp.RE_StartDate = new ZDateTime(year, 5, 1);
			testBuyExchangeRateDiffComp.RE_ExpiryDate = new ZDateTime(year, 5, 2);

			RefExchangeRate testSellExchangeRateDiffComp = Factory.New<RefExchangeRate>();
			testSellExchangeRateDiffComp.RE_GC = testCompany1.PK;
			testSellExchangeRateDiffComp.RE_RX_NKExCurrency = testCurrency.RX_Code;
			testSellExchangeRateDiffComp.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			testSellExchangeRateDiffComp.RE_SellRate = 0.67M;
			testSellExchangeRateDiffComp.RE_StartDate = new ZDateTime(year, 4, 1);
			testSellExchangeRateDiffComp.RE_ExpiryDate = new ZDateTime(year, 4, 2);

			RefExchangeRate testBuyExchangeRateCurComp = Factory.New<RefExchangeRate>();
			testBuyExchangeRateCurComp.RE_GC = GlbCompany.CurrentCompany.PK;
			testBuyExchangeRateCurComp.RE_RX_NKExCurrency = testCurrency.RX_Code;
			testBuyExchangeRateCurComp.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			testBuyExchangeRateCurComp.RE_SellRate = 0.21M;
			testBuyExchangeRateCurComp.RE_StartDate = new ZDateTime(year, 3, 1);
			testBuyExchangeRateCurComp.RE_ExpiryDate = new ZDateTime(year, 3, 2);

			RefExchangeRate testSellExchangeRateCurComp = Factory.New<RefExchangeRate>();
			testSellExchangeRateCurComp.RE_GC = GlbCompany.CurrentCompany.PK;
			testSellExchangeRateCurComp.RE_RX_NKExCurrency = testCurrency.RX_Code;
			testSellExchangeRateCurComp.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			testSellExchangeRateCurComp.RE_SellRate = 0.43M;
			testSellExchangeRateCurComp.RE_StartDate = new ZDateTime(year, 2, 1);
			testSellExchangeRateCurComp.RE_ExpiryDate = new ZDateTime(year, 2, 2);

			TestExchangeRateWrapper.CurrencyNK = testCurrency.RX_Code;

			AssertEquals("should pull out the buy start date of the current company", new ZDateTime(year, 3, 3),
				TestExchangeRateWrapper.BuyStartDate);
			AssertEquals("should pull out the buy expiry date of the current company", new ZDateTime(year, 3, 3, 23, 59, 0),
				TestExchangeRateWrapper.BuyExpiryDate);
			AssertEquals("should pull out the sell start date of the current company", new ZDateTime(year, 2, 3),
				TestExchangeRateWrapper.SellStartDate);
			AssertEquals("Should pull out the sell expiry date of the current company", new ZDateTime(year, 2, 3, 23, 59, 0),
				TestExchangeRateWrapper.SellExpiryDate);
			AssertEquals("Should pull out the buy rate of the current company", 0.21M,
				TestExchangeRateWrapper.BuyRate);
			AssertEquals("Should not pull out the sell rate of the current company", 0.43M,
				TestExchangeRateWrapper.SellRate);

			// note that the next two changes overlap the exchangerate with the 
			// ones in the other company
			TestExchangeRateWrapper.BuyExpiryDate = new ZDateTime(year, 5, 6);
			TestExchangeRateWrapper.SellExpiryDate = new ZDateTime(year, 4, 6);

			Assert("Should not be any errors since overlap is between different companies",
				!TestExchangeRateWrapper.BuyExpiryDateInfo.HasErrors());
			Assert("Should not be any errors since overlap is between different companies",
				!TestExchangeRateWrapper.SellExpiryDateInfo.HasErrors());
		}

		#endregion

		#region TestPullOutDates

		public void TestPullOutDates()
		{
			RefExchangeRate testBuyRate = Factory.New<RefExchangeRate>();
			RefExchangeRate testSellRate = Factory.New<RefExchangeRate>();
			ZString testCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			testBuyRate.RE_RX_NKExCurrency = testCurrency;
			testBuyRate.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			testBuyRate.RE_StartDate = new ZDateTime(year, 3, 3);
			testBuyRate.RE_ExpiryDate = new ZDateTime(year, 3, 4);

			testSellRate.RE_RX_NKExCurrency = testCurrency;
			testSellRate.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			testSellRate.RE_StartDate = new ZDateTime(year, 4, 4);
			testSellRate.RE_ExpiryDate = new ZDateTime(year, 4, 5);

			TestExchangeRateWrapper.CurrencyNK = testCurrency;
			AssertEquals("Should default to the Buy ExchangeRate's Expiry Date + 1",
				new ZDateTime(year, 3, 5, 0, 0, 0), TestExchangeRateWrapper.BuyStartDate);
			AssertEquals("Should default to the Buy ExchangeRate's Expiry Date + 1",
				new ZDateTime(year, 3, 5, 23, 59, 0), TestExchangeRateWrapper.BuyExpiryDate);

			AssertEquals("Should default to the Sell ExchangeRate's Expiry Date + 1",
				new ZDateTime(year, 4, 6, 0, 0, 0), TestExchangeRateWrapper.SellStartDate);
			AssertEquals("Should default to the Sell ExchangeRate's Expiry Date + 1",
				new ZDateTime(year, 4, 6, 23, 59, 0), TestExchangeRateWrapper.SellExpiryDate);
		}

		#endregion

		#region TestPullOutRates

		public void TestPullOutRates()
		{
			var differentFactoryForSaving = new BusinessObjectFactory();
			var testBuyRate = differentFactoryForSaving.New<RefExchangeRate>();
			var testSellRate = differentFactoryForSaving.New<RefExchangeRate>();
			ZString testCurrency = differentFactoryForSaving.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			testBuyRate.RE_RX_NKExCurrency = testCurrency;
			testBuyRate.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			testBuyRate.RE_StartDate = ZDateTime.Today;
			testBuyRate.RE_ExpiryDate = ZDateTime.Today;
			testBuyRate.RE_SellRate = new ZDecimal(0.45);

			testSellRate.RE_RX_NKExCurrency = testCurrency;
			testSellRate.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			testSellRate.RE_StartDate = ZDateTime.Today;
			testSellRate.RE_ExpiryDate = ZDateTime.Today;
			testSellRate.RE_SellRate = new ZDecimal(0.67);
			differentFactoryForSaving.Save();

			TestExchangeRateWrapper.CurrencyNK = testCurrency;

			AssertEquals("should pull out the buy exchange rate created by the same factory",
				0.45M, TestExchangeRateWrapper.BuyRate);
			AssertEquals("should pull out the sell exchange rate created by the same factory",
				0.67M, TestExchangeRateWrapper.SellRate);
		}

		#endregion

		#region TestSettingBuyStartDate

		[TestDate(2020, 5, 5)]
		public void TestSettingBuyStartDate()
		{
			CombineAssertions(() =>
			{
				TestExchangeRateWrapper.BuyStartDate = new ZDateTime(year, 4, 4);
				AssertEquals("Should not update BuyExpiryDate", new ZDateTime(2020, 5, 5, 23, 59, 0),
					TestExchangeRateWrapper.BuyExpiryDate);
				AssertEquals("Should update SellStartDate", new ZDateTime(year, 4, 4, 0, 0, 0),
					TestExchangeRateWrapper.SellStartDate);
				AssertEquals("Should not update SellExpiryDate", new ZDateTime(2020, 5, 5, 23, 59, 0),
					TestExchangeRateWrapper.SellExpiryDate);

				TestExchangeRateWrapper.BuyStartDate = new ZDateTime(year, 4, 6);
				AssertEquals("Should not update BuyExpiryDate", new ZDateTime(2020, 5, 5, 23, 59, 0),
					TestExchangeRateWrapper.BuyExpiryDate);
				AssertEquals("Should not update SellStartDate", new ZDateTime(year, 4, 4, 0, 0, 0),
					TestExchangeRateWrapper.SellStartDate);
				AssertEquals("Should not update SellExpiryDate", new ZDateTime(2020, 5, 5, 23, 59, 0),
					TestExchangeRateWrapper.SellExpiryDate);
			});
		}

		#endregion

		#region TestSettingBuyExpiryDate

		[TestDate(2020, 5, 5)]
		public void TestSettingBuyExpiryDate()
		{
			TestExchangeRateWrapper.BuyExpiryDate = new ZDateTime(year, 5, 5);
			AssertEquals("Should not automatically set the SellExpiryDate", new ZDateTime(2020, 5, 5, 23, 59, 0),
				TestExchangeRateWrapper.SellExpiryDate);
			TestExchangeRateWrapper.BuyExpiryDate = new ZDateTime(year, 5, 7);
			AssertEquals("Should not automatically set the SellExpiryDate", new ZDateTime(2020, 5, 5, 23, 59, 0),
				TestExchangeRateWrapper.SellExpiryDate);
		}

		#endregion

		#region TestSellExpiryDateValidation

		public void TestSellExpiryDateValidation()
		{
			RefExchangeRate exchangeRate4 = Factory.New<RefExchangeRate>();
			exchangeRate4.RE_StartDate = new ZDateTime(year, 7, 8);
			exchangeRate4.RE_ExpiryDate = new ZDateTime(year, 7, 16);
			exchangeRate4.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			exchangeRate4.RE_RX_NKExCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			Factory.Save();

			ExchangeRateWrapper testWrapper = new ExchangeRateWrapper(Factory);
			testWrapper.CurrencyNK = exchangeRate4.RE_RX_NKExCurrency;

			testWrapper.SellExpiryDate = new ZDateTime(year, 7, 12);
			Assert("SellExpiry Date overlaps with existing exchange rate and should raise an error",
				testWrapper.SellExpiryDateInfo.HasErrors());
		}

		#endregion

		#region TestBuyExpiryDateValidation

		public void TestBuyExpiryDateValidation()
		{
			RefExchangeRate exchangeRate3 = Factory.New<RefExchangeRate>();
			exchangeRate3.RE_StartDate = new ZDateTime(year, 6, 7);
			exchangeRate3.RE_ExpiryDate = new ZDateTime(year, 6, 11);
			exchangeRate3.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			exchangeRate3.RE_RX_NKExCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			Factory.Save();

			ExchangeRateWrapper testWrapper = new ExchangeRateWrapper(Factory);
			testWrapper.CurrencyNK = exchangeRate3.RE_RX_NKExCurrency;

			testWrapper.BuyExpiryDate = new ZDateTime(year, 6, 8);
			Assert("Expiry date overlaps with existing Buy ExchangeRate should cause error",
				testWrapper.BuyExpiryDateInfo.HasErrors());
			testWrapper.BuyExpiryDate = new ZDateTime(year, 6, 14);
			Assert("expiry date does not overlap with any existing Buy Exchange Rates, should not cause errors",
				!testWrapper.BuyExpiryDateInfo.HasErrors());
		}

		#endregion

		#region TestBuyStartDateValidation

		[TestDate(2020, 3, 9)]
		public void TestBuyStartDateValidation()
		{
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			exchangeRate1.RE_StartDate = new ZDateTime(year, 3, 4);
			exchangeRate1.RE_ExpiryDate = new ZDateTime(year, 3, 8);
			exchangeRate1.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			exchangeRate1.RE_RX_NKExCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			Factory.Save();

			var rateWrapper = new ExchangeRateWrapper(Factory);
			rateWrapper.CurrencyNK = exchangeRate1.RE_RX_NKExCurrency;

			CombineAssertions(() =>
			{
				if (rateWrapper.HasErrors)
				{
					AssertEquals("ExchangeRateWrapper has the following error ", string.Empty, rateWrapper.GetMessageErrors().ToMessageListString());
				}

				var expectedStartDate = new ZDateTime(year, 3, 9, 0, 0, 0);
				var expectedExpiryDate = new ZDateTime(year, 3, 9, 23, 59, 0);
				AssertEquals("Buy Start date should default to the latest expiry date for the same currency + 1",
					expectedStartDate, rateWrapper.BuyStartDate);
				AssertEquals("Buy Expiry date should default to the latest expiry date for the same currency + 1",
					expectedExpiryDate, rateWrapper.BuyExpiryDate);
				AssertEquals("Sell Start date should default to the latest expiry date for the same currency + 1",
					new ZDateTime(2018, 3, 9, 0, 0, 0), rateWrapper.SellStartDate);
				AssertEquals("Sell Expiry date should default to the latest expiry date for the same currency + 1",
					new ZDateTime(2020, 3, 9, 23, 59, 0), rateWrapper.SellExpiryDate);

				rateWrapper.BuyStartDate = new ZDateTime(year, 3, 6);
				Assert("Start date overlaps, should have error on the Buy expiry date error provider",
					rateWrapper.BuyExpiryDateInfo.HasErrors());
				Assert("Should not have error on Buy Start date error provider",
					!rateWrapper.BuyStartDateInfo.HasErrors());

				rateWrapper.BuyStartDate = new ZDateTime(year, 3, 9);
				Assert("Start date does not overlap, should not have errors",
					!rateWrapper.BuyExpiryDateInfo.HasErrors());
				Assert("Should not have error on Buy Start date error provider",
					!rateWrapper.BuyStartDateInfo.HasErrors());
			});
		}

		#endregion

		#region TestSellStartDateValidation

		public void TestSellStartDateValidation()
		{
			RefExchangeRate exchangeRate2 = Factory.New<RefExchangeRate>();
			exchangeRate2.RE_StartDate = new ZDateTime(year, 5, 6);
			exchangeRate2.RE_ExpiryDate = new ZDateTime(year, 5, 10);
			exchangeRate2.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			exchangeRate2.RE_RX_NKExCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			Factory.Save();

			ExchangeRateWrapper testWrapper = new ExchangeRateWrapper(Factory);
			testWrapper.CurrencyNK = exchangeRate2.RE_RX_NKExCurrency;

			if (testWrapper.HasErrors)
			{
				AssertEquals("ExchangeRateWrapper has the following error ", string.Empty, testWrapper.GetMessageErrors().ToMessageListString());
			}

			testWrapper.SellStartDate = new ZDateTime(year, 5, 8);
			Assert("Sell start date overlaps an existing Sell date range thus giving an error on the Sell Expiry Date error provider",
				testWrapper.SellExpiryDateInfo.HasErrors());
			Assert("Sell Start date error provider should not have errors",
				!testWrapper.SellStartDateInfo.HasErrors());

			testWrapper.SellStartDate = new ZDateTime(year, 5, 11);
			Assert("Sell Start date should be valid since does not overlap",
				!testWrapper.SellExpiryDateInfo.HasErrors());
			Assert("Sell Start date error provider should not have errors",
				!testWrapper.SellStartDateInfo.HasErrors());
		}

		#endregion

		#region TestCurrencyValidation

		public void TestCurrencyValidation()
		{
			ExchangeRateWrapper testWrapper = new ExchangeRateWrapper(Factory);
			Assert("Newly created RateWrapper should have no exceptions", !testWrapper.HasErrors);
			testWrapper.CurrencyNK = "XXX";
			Assert("Invalid guid should give exception", testWrapper.HasErrors);
		}

		#endregion

		#region TestBuyRateValidation

		public void TestBuyRateValidation()
		{
			TestExchangeRateWrapper.BuyRateInfo.ClearAllNotifications();
			TestExchangeRateWrapper.BuyRate = 0;
			Assert("Cannot post if exchange rate is 0.00", TestExchangeRateWrapper.BuyRateInfo.HasErrors());

			TestExchangeRateWrapper.BuyRateInfo.ClearAllNotifications();
			TestExchangeRateWrapper.BuyRate = -1;
			Assert("Negative exchange rate should give an error", TestExchangeRateWrapper.BuyRateInfo.HasErrors());

			TestExchangeRateWrapper.BuyRateInfo.ClearAllNotifications();
			TestExchangeRateWrapper.BuyRate = 3;
			Assert("Rate is positive should not have errors", !TestExchangeRateWrapper.BuyRateInfo.HasErrors());
		}

		#endregion

		#region TestSellRateValidation

		public void TestSellRateValidation()
		{
			TestExchangeRateWrapper.SellRateInfo.ClearAllNotifications();
			TestExchangeRateWrapper.SellRate = -1;
			Assert("Negative rate should give errors", TestExchangeRateWrapper.SellRateInfo.HasErrors());

			TestExchangeRateWrapper.SellRateInfo.ClearAllNotifications();
			TestExchangeRateWrapper.SellRate = 0;
			Assert("cannot post a zero exchange rate", TestExchangeRateWrapper.SellRateInfo.HasErrors());

			TestExchangeRateWrapper.SellRateInfo.ClearAllNotifications();
			TestExchangeRateWrapper.SellRate = 2;
			Assert("positive exchange rate should not give errors", !TestExchangeRateWrapper.SellRateInfo.HasErrors());
		}

		#endregion

		#region TestActiveCurrencyValidation

		public void TestActiveCurrencyValidation()
		{
			RefCurrency nonActiveCurrency = Factory.New<RefCurrency>();
			nonActiveCurrency.RX_IsActive = false;
			Factory.Save();
			ExchangeRateWrapper testWrapper = new ExchangeRateWrapper(Factory);
			testWrapper.CurrencyNK = nonActiveCurrency.RX_Code;
			Assert("Non active currencies should raise an error", testWrapper.CurrencyNKInfo.HasErrors());
		}

		#endregion

		#region Test Proxy Properties

		public void TestCurrencyProxyProperty()
		{
			ZString currencyCode = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			TestExchangeRateWrapper.CurrencyNK = currencyCode;
			AssertEquals("Currency of the buy proxy BizO should be set", currencyCode,
				TestExchangeRateWrapper.BuyExchangeRateBizO.RE_RX_NKExCurrency);
			AssertEquals("Currency of the sell proxy BizO should be set", currencyCode,
				TestExchangeRateWrapper.SellExchangeRateBizO.RE_RX_NKExCurrency);
			AssertEquals("The Currency_get method should work", currencyCode,
				TestExchangeRateWrapper.CurrencyNK);
		}

		public void TestBuyStartDateProxyProperty()
		{
			ZDateTime testTime = new ZDateTime(year, 4, 5);
			ZDateTime expectedTime = new ZDateTime(year, 4, 5, 0, 0, 0);
			TestExchangeRateWrapper.BuyStartDate = testTime;
			AssertEquals("BuyStartDate of the buy proxy BizO should be set", expectedTime,
				TestExchangeRateWrapper.BuyExchangeRateBizO.RE_StartDate);
			AssertEquals("The BuyStartDate_Get method should work", expectedTime,
				TestExchangeRateWrapper.BuyStartDate);
		}

		public void TestBuyExpiryDateProxyProperty()
		{
			ZDateTime testTime = new ZDateTime(year, 4, 5);
			ZDateTime expectedTime = new ZDateTime(year, 4, 5, 23, 59, 0);
			TestExchangeRateWrapper.BuyExpiryDate = testTime;
			AssertEquals("BuyExpiryDate of the buy proxy BizO should be set", expectedTime,
				TestExchangeRateWrapper.BuyExchangeRateBizO.RE_ExpiryDate);
			AssertEquals("The BuyExpiryDate_Get method should work", expectedTime,
				TestExchangeRateWrapper.BuyExpiryDate);
		}

		public void TestBuyRateProxyProperty()
		{
			ZDecimal testRate = new ZDecimal(3.456);
			TestExchangeRateWrapper.BuyRate = testRate;
			AssertEquals("BuyRate of the buy proxy BizO should be set", testRate,
				TestExchangeRateWrapper.BuyExchangeRateBizO.RE_SellRate);
			AssertEquals("The BuyRate_get method should work", testRate,
				TestExchangeRateWrapper.BuyRate);
		}

		public void TestSellStartDateProxyProperty()
		{
			ZDateTime testTime = new ZDateTime(year, 6, 7);
			ZDateTime expectedTime = new ZDateTime(year, 6, 7, 0, 0, 0);
			TestExchangeRateWrapper.SellStartDate = testTime;
			AssertEquals("Start Date of the sell proxy BizO should be set", expectedTime,
				TestExchangeRateWrapper.SellExchangeRateBizO.RE_StartDate);
			AssertEquals("SellStartDate_Get method of the wrapper should also work", expectedTime,
				TestExchangeRateWrapper.SellStartDate);
		}

		public void TestSellExpiryDateProxyProperty()
		{
			ZDateTime testTime = new ZDateTime(year, 6, 7);
			ZDateTime expectedTime = new ZDateTime(year, 6, 7, 23, 59, 0);
			TestExchangeRateWrapper.SellExpiryDate = testTime;
			AssertEquals("Expiry date of the sell proxy BizO should be set", expectedTime,
				TestExchangeRateWrapper.SellExchangeRateBizO.RE_ExpiryDate);
			AssertEquals("SellExpiryDate_Get method of the wrapper should work", expectedTime,
				TestExchangeRateWrapper.SellExpiryDate);
		}

		public void TestSellRateProxyProperty()
		{
			ZDecimal testRate = new ZDecimal(8.765);
			TestExchangeRateWrapper.SellRate = testRate;
			AssertEquals("Sell rate of the Sell proxy BizO should be set", testRate,
				TestExchangeRateWrapper.SellExchangeRateBizO.RE_SellRate);
			AssertEquals("SellRate_Get method of the wrapper should work", testRate,
				TestExchangeRateWrapper.SellRate);
		}

		#endregion

		#region TestMatchingBuyExchangeRatesCollection

		[TestDate(2018, 11, 25, 21, 37, 0)]
		public void TestMatchingBuyExchangeRatesCollection()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "SOME ORG";

			var currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "###";
			var currency2 = Factory.New<RefCurrency>();
			currency2.RX_Code = "^^^";

			var exRate1 = Factory.New<RefExchangeRate>();
			exRate1.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate1.RE_StartDate = ZDateTime.Today;
			exRate1.RE_ExpiryDate = ZDateTime.Today;
			exRate1.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;

			var exRate2 = Factory.New<RefExchangeRate>();
			exRate2.RE_RX_NKExCurrency = currency2.RX_Code;
			exRate2.RE_StartDate = new ZDateTime(year, 3, 4);
			exRate2.RE_ExpiryDate = new ZDateTime(year, 4, 5);
			exRate2.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;

			var sellExRate = Factory.New<RefExchangeRate>();
			sellExRate.RE_RX_NKExCurrency = currency1.RX_Code;
			sellExRate.RE_StartDate = ZDateTime.Today;
			sellExRate.RE_ExpiryDate = ZDateTime.Today;
			sellExRate.RE_ExRateType = ExchangeRateWrapper.SellExRateType;

			var exRate3 = Factory.New<RefExchangeRate>();
			exRate3.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate3.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			exRate3.RE_StartDate = ZDateTime.Today;
			exRate3.RE_ExpiryDate = ZDateTime.Today;
			exRate3.RE_OH_Client = orgHeader.PK;

			var exRate4 = Factory.New<RefExchangeRate>();
			exRate4.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate4.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			exRate4.RE_StartDate = ZDateTime.Today;
			exRate4.RE_ExpiryDate = ZDateTime.Today;
			exRate4.RE_OH_Client = orgHeader.PK;

			var lastTwoMonth = ZDateTime.Today.AddMonths(-2);
			var exRate5 = Factory.New<RefExchangeRate>();
			exRate5.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate5.RE_StartDate = new ZDateTime(lastTwoMonth.Year, lastTwoMonth.Month, 3);
			exRate5.RE_ExpiryDate = new ZDateTime(lastTwoMonth.Year, lastTwoMonth.Month, 4);
			exRate5.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;

			var lastMonth = ZDateTime.Today.AddMonths(-1);
			var exRate6 = Factory.New<RefExchangeRate>();
			exRate6.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate6.RE_StartDate = new ZDateTime(lastMonth.Year, lastMonth.Month, 5);
			exRate6.RE_ExpiryDate = new ZDateTime(lastMonth.Year, lastMonth.Month, 6);
			exRate6.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;

			var exRate7 = Factory.New<RefExchangeRate>();
			exRate7.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate7.RE_StartDate = new ZDateTime(lastMonth.Year, lastMonth.Month, 7);
			exRate7.RE_ExpiryDate = new ZDateTime(lastMonth.Year, lastMonth.Month, 8);
			exRate7.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;

			var nextMonth = ZDateTime.Today.AddMonths(1);
			var exRate8 = Factory.New<RefExchangeRate>();
			exRate8.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate8.RE_StartDate = new ZDateTime(nextMonth.Year, nextMonth.Month, 4);
			exRate8.RE_ExpiryDate = new ZDateTime(nextMonth.Year, nextMonth.Month, 5);
			exRate8.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;

			var nextTwoMonth = ZDateTime.Today.AddMonths(2);
			var exRate9 = Factory.New<RefExchangeRate>();
			exRate9.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate9.RE_StartDate = new ZDateTime(nextTwoMonth.Year, nextTwoMonth.Month, 4);
			exRate9.RE_ExpiryDate = new ZDateTime(nextTwoMonth.Year, nextTwoMonth.Month, 5);
			exRate9.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;

			Factory.Save();

			TestExchangeRateWrapper.CurrencyNK = currency1.RX_Code;
			TestExchangeRateWrapper.BuyStartDate = ZDateTime.Today.AddDays(1);
			TestExchangeRateWrapper.BuyExpiryDate = ZDateTime.Today.AddDays(1);
			AssertEquals(4, TestExchangeRateWrapper.MatchingBuyExchangeRates.Count);
			Assert("should contain exRate1", TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate1));
			Assert("Should contain exRate6", TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate6));
			Assert("Should contain exRate7", TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate7));
			Assert("Should contain exRate8", TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate8));
			Assert("should not contain exRate2 because different currency", !TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate2));
			Assert("Should not contain sellExRate because its a sell exchange rate", !TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(sellExRate));

			var hitsBefore = Db.Connection.ExecutedCommandCount;
			TestExchangeRateWrapper.BuyExpiryDate = ZDateTime.Today.AddDays(2);
			var hitsAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals("Should not have extra db hits", 0, hitsAfter - hitsBefore);

			TestExchangeRateWrapper.CurrencyNK = currency2.RX_Code;
			Assert("MatchingBuyExchangeRates should contain TestBuyExRate2", TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate2));
			Assert("MatchingBuyExchangeRates should not contain TestBuyExRate1", !TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate1));

			Assert("Buy expiry date should not have errors", !TestExchangeRateWrapper.BuyExpiryDateInfo.HasErrors());
			TestExchangeRateWrapper.BuyStartDate = new ZDateTime(year, 4, 4);
			Assert("Buy expiry date should have errors caused by start date", TestExchangeRateWrapper.BuyExpiryDateInfo.HasErrors());
			TestExchangeRateWrapper.BuyStartDate = new ZDateTime(year, 4, 6);
			Assert("Buy expiry date should not have errors caused by start date", !TestExchangeRateWrapper.BuyExpiryDateInfo.HasErrors());

			TestExchangeRateWrapper.BuyExpiryDate = new ZDateTime(year, 6, 7);
			Assert("Buy expiry date should not have errors caused by expiry date", !TestExchangeRateWrapper.BuyExpiryDateInfo.HasErrors());
			TestExchangeRateWrapper.BuyStartDate = new ZDateTime(year, 3, 3);
			Assert("Buy expiry Date have errors caused by enclosing overlap", TestExchangeRateWrapper.BuyExpiryDateInfo.HasErrors());
		}

		[TestDate(2018, 11, 30, 21, 37, 0)]
		public void TestMatchingBuyExchangeRatesCollectionWhenBuyDatesExceedValidSmallDateRange()
		{
			var currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "###";

			var exRate1 = Factory.New<RefExchangeRate>();
			exRate1.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate1.RE_StartDate = ZDateTime.Today;
			exRate1.RE_ExpiryDate = ZDateTime.Today;
			exRate1.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;

			Factory.Save();

			TestExchangeRateWrapper.CurrencyNK = currency1.RX_Code;
			TestExchangeRateWrapper.BuyStartDate = ZDateTime.Today.AddDays(1);
			TestExchangeRateWrapper.BuyExpiryDate = ZDateTime.Today.AddDays(1);
			AssertEquals(1, TestExchangeRateWrapper.MatchingBuyExchangeRates.Count);
			Assert("should contain exRate1", TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate1));

			var maxSmallDate = ZDateTime.MaxSmallDateTimeValue; // 2079-06-06 23:59:29.000
			var minSmallDate = ZDateTime.MinSmallDateTimeValue; // 1900-01-01 00:00:00:000

			// expiryDateOfNextMonth = 1989-12-31
			TestExchangeRateWrapper.BuyStartDate = minSmallDate.AddMonths(1).AddDays(-1);
			AssertEquals(0, TestExchangeRateWrapper.MatchingBuyExchangeRates.Count);

			// expiryDateOfNextMonth = 1900-1-1
			TestExchangeRateWrapper.BuyStartDate = minSmallDate.AddMonths(1);
			AssertEquals(1, TestExchangeRateWrapper.MatchingBuyExchangeRates.Count);
			Assert("should contain exRate1", TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate1));

			// expiryDateOfNextMonth = 2079-6-30
			TestExchangeRateWrapper.BuyExpiryDate = maxSmallDate.AddDays(-36);
			AssertEquals(0, TestExchangeRateWrapper.MatchingBuyExchangeRates.Count);

			// expiryDateOfNextMonth = 2079-5-31
			TestExchangeRateWrapper.BuyExpiryDate = maxSmallDate.AddDays(-37);
			AssertEquals(1, TestExchangeRateWrapper.MatchingBuyExchangeRates.Count);

			// startDateOfLastMonth = 2018-10-1
			// expiryDateOfNextMonth = 2018-9-30
			TestExchangeRateWrapper.BuyStartDate = ZDateTime.Today;
			TestExchangeRateWrapper.BuyExpiryDate = ZDateTime.Today.AddMonths(-3);
			AssertEquals(0, TestExchangeRateWrapper.MatchingBuyExchangeRates.Count);

			// startDateOfLastMonth = 2018-11-1
			// expiryDateOfNextMonth = 2018-11-31
			TestExchangeRateWrapper.BuyStartDate = ZDateTime.Today.AddMonths(1);
			TestExchangeRateWrapper.BuyExpiryDate = ZDateTime.Today.AddMonths(-1);
			AssertEquals(1, TestExchangeRateWrapper.MatchingBuyExchangeRates.Count);
			Assert("should contain exRate2", TestExchangeRateWrapper.MatchingBuyExchangeRates.Contains(exRate1));
		}

		#endregion

		#region TestMatchingSellExchangeRatesCollection

		[TestDate(2018, 11, 25, 21, 37, 0)]
		public void TestMatchingSellExchangeRatesCollection()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "SOME ORG";

			var currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "@@@";
			var currency2 = Factory.New<RefCurrency>();
			currency2.RX_Code = ")))";

			var exRate1 = Factory.New<RefExchangeRate>();
			exRate1.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate1.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			exRate1.RE_StartDate = ZDateTime.Today;
			exRate1.RE_ExpiryDate = ZDateTime.Today;

			var exRate2 = Factory.New<RefExchangeRate>();
			exRate2.RE_RX_NKExCurrency = currency2.RX_Code;
			exRate2.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			exRate2.RE_ExpiryDate = ZDateTime.Today;
			exRate2.RE_StartDate = ZDateTime.Today;

			var exRate3 = Factory.New<RefExchangeRate>();
			exRate3.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate3.RE_ExRateType = ExchangeRateWrapper.SellExRateType;
			exRate3.RE_StartDate = ZDateTime.Today;
			exRate3.RE_ExpiryDate = ZDateTime.Today;
			exRate3.RE_OH_Client = orgHeader.PK;

			var buyExRate = Factory.New<RefExchangeRate>();
			buyExRate.RE_RX_NKExCurrency = currency1.RX_Code;
			buyExRate.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			buyExRate.RE_StartDate = ZDateTime.Today;
			buyExRate.RE_ExpiryDate = ZDateTime.Today;
			buyExRate.RE_OH_Client = orgHeader.PK;

			var lastTwoMonth = ZDateTime.Today.AddMonths(-2);
			var exRate4 = Factory.New<RefExchangeRate>();
			exRate4.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate4.RE_StartDate = new ZDateTime(lastTwoMonth.Year, lastTwoMonth.Month, 3);
			exRate4.RE_ExpiryDate = new ZDateTime(lastTwoMonth.Year, lastTwoMonth.Month, 4);
			exRate4.RE_ExRateType = ExchangeRateWrapper.SellExRateType;

			var lastMonth = ZDateTime.Today.AddMonths(-1);
			var exRate5 = Factory.New<RefExchangeRate>();
			exRate5.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate5.RE_StartDate = new ZDateTime(lastMonth.Year, lastMonth.Month, 5);
			exRate5.RE_ExpiryDate = new ZDateTime(lastMonth.Year, lastMonth.Month, 6);
			exRate5.RE_ExRateType = ExchangeRateWrapper.SellExRateType;

			var exRate6 = Factory.New<RefExchangeRate>();
			exRate6.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate6.RE_StartDate = new ZDateTime(lastMonth.Year, lastMonth.Month, 7);
			exRate6.RE_ExpiryDate = new ZDateTime(lastMonth.Year, lastMonth.Month, 8);
			exRate6.RE_ExRateType = ExchangeRateWrapper.SellExRateType;

			var nextMonth = ZDateTime.Today.AddMonths(1);
			var exRate7 = Factory.New<RefExchangeRate>();
			exRate7.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate7.RE_StartDate = new ZDateTime(nextMonth.Year, nextMonth.Month, 4);
			exRate7.RE_ExpiryDate = new ZDateTime(nextMonth.Year, nextMonth.Month, 5);
			exRate7.RE_ExRateType = ExchangeRateWrapper.SellExRateType;

			var nextTwoMonth = ZDateTime.Today.AddMonths(2);
			var exRate8 = Factory.New<RefExchangeRate>();
			exRate8.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate8.RE_StartDate = new ZDateTime(nextTwoMonth.Year, nextTwoMonth.Month, 4);
			exRate8.RE_ExpiryDate = new ZDateTime(nextTwoMonth.Year, nextTwoMonth.Month, 5);
			exRate8.RE_ExRateType = ExchangeRateWrapper.SellExRateType;

			Factory.Save();

			TestExchangeRateWrapper.CurrencyNK = currency1.RX_Code;
			TestExchangeRateWrapper.SellStartDate = ZDateTime.Today.AddDays(1);
			TestExchangeRateWrapper.SellExpiryDate = ZDateTime.Today.AddDays(1);
			AssertEquals(4, TestExchangeRateWrapper.MatchingSellExchangeRates.Count);
			Assert("MatchingSellExchangeRates should contain ExRate1", TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate1));
			Assert("MatchingSellExchangeRates should contain ExRate5", TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate5));
			Assert("MatchingSellExchangeRates should contain ExRate6", TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate6));
			Assert("MatchingSellExchangeRates should contain ExRate7", TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate7));
			Assert("MatchingSellExchangeRates should not contain ExRate2", !TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate2));

			var hitsBefore = Db.Connection.ExecutedCommandCount;
			TestExchangeRateWrapper.SellExpiryDate = ZDateTime.Today.AddDays(2);
			var hitsAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals("Shoud not have extra db hits", 0, hitsAfter - hitsBefore);

			TestExchangeRateWrapper.CurrencyNK = currency2.RX_Code;
			Assert("MatchingSellExRates should contain ExRate2", TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate2));
			Assert("MatchingSellExRates should not contain ExRate1", !TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate1));
		}

		[TestDate(2018, 11, 30, 21, 37, 0)]
		public void TestMatchingSellExchangeRatesCollectionWhenSellDatesExceedValidSmallDateRange()
		{
			var currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "###";

			var exRate1 = Factory.New<RefExchangeRate>();
			exRate1.RE_RX_NKExCurrency = currency1.RX_Code;
			exRate1.RE_StartDate = ZDateTime.Today;
			exRate1.RE_ExpiryDate = ZDateTime.Today;
			exRate1.RE_ExRateType = ExchangeRateWrapper.SellExRateType;

			Factory.Save();

			TestExchangeRateWrapper.CurrencyNK = currency1.RX_Code;
			TestExchangeRateWrapper.SellStartDate = ZDateTime.Today.AddDays(1);
			TestExchangeRateWrapper.SellExpiryDate = ZDateTime.Today.AddDays(1);
			AssertEquals(1, TestExchangeRateWrapper.MatchingSellExchangeRates.Count);
			Assert("should contain exRate1", TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate1));

			var maxSmallDate = ZDateTime.MaxSmallDateTimeValue; // 2079-06-06 23:59:29.000
			var minSmallDate = ZDateTime.MinSmallDateTimeValue; // 1900-01-01 00:00:00:000

			// expiryDateOfNextMonth = 1989-12-31
			TestExchangeRateWrapper.SellStartDate = minSmallDate.AddMonths(1).AddDays(-1);
			AssertEquals(0, TestExchangeRateWrapper.MatchingSellExchangeRates.Count);

			// expiryDateOfNextMonth = 1900-1-1
			TestExchangeRateWrapper.SellStartDate = minSmallDate.AddMonths(1);
			AssertEquals(1, TestExchangeRateWrapper.MatchingSellExchangeRates.Count);
			Assert("should contain exRate1", TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate1));

			// expiryDateOfNextMonth = 2079-6-30
			TestExchangeRateWrapper.SellExpiryDate = maxSmallDate.AddDays(-36);
			AssertEquals(0, TestExchangeRateWrapper.MatchingSellExchangeRates.Count);

			// expiryDateOfNextMonth = 2079-5-31
			TestExchangeRateWrapper.SellExpiryDate = maxSmallDate.AddDays(-37);
			AssertEquals(1, TestExchangeRateWrapper.MatchingSellExchangeRates.Count);

			// startDateOfLastMonth = 2018-10-1
			// expiryDateOfNextMonth = 2018-9-30
			TestExchangeRateWrapper.SellStartDate = ZDateTime.Today;
			TestExchangeRateWrapper.SellExpiryDate = ZDateTime.Today.AddMonths(-3);
			AssertEquals(0, TestExchangeRateWrapper.MatchingSellExchangeRates.Count);

			// startDateOfLastMonth = 2018-11-1
			// expiryDateOfNextMonth = 2018-11-31
			TestExchangeRateWrapper.SellStartDate = ZDateTime.Today.AddMonths(1);
			TestExchangeRateWrapper.SellExpiryDate = ZDateTime.Today.AddMonths(-1);
			AssertEquals(1, TestExchangeRateWrapper.MatchingSellExchangeRates.Count);
			Assert("should contain exRate2", TestExchangeRateWrapper.MatchingSellExchangeRates.Contains(exRate1));
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			TestExchangeRateWrapper.BuyStartDate = ZDateTime.Now;
			TestExchangeRateWrapper.SellStartDate = ZDateTime.Now;
			AssertEquals(false, TestExchangeRateWrapper.fBuyExchangeRateBizO.IsDeleted);
			AssertEquals(false, TestExchangeRateWrapper.fSellExchangeRateBizO.IsDeleted);

			TestExchangeRateWrapper.Delete();
			AssertEquals("when delete wrapper ,fBuyExchangeRateBizO should also be deleted", true, TestExchangeRateWrapper.fBuyExchangeRateBizO.IsDeleted);
			AssertEquals("when delete wrapper ,fBuyExchangeRateBizO should also be deleted", true, TestExchangeRateWrapper.fBuyExchangeRateBizO.IsDeleted);
		}

		#endregion

		#region TestExcessSmallDates

		public void TestValidationOfExcessValidSmallDateForBuyDates()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = new ZDateTime(year, 3, 4);
			exchangeRate.RE_ExpiryDate = new ZDateTime(year, 3, 8);
			exchangeRate.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			exchangeRate.RE_RX_NKExCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			Factory.Save();

			var wrapper = new ExchangeRateWrapper(Factory);
			wrapper.CurrencyNK = exchangeRate.RE_RX_NKExCurrency;

			var maxSmallDate = ZDateTime.MaxSmallDateTimeValue;
			var minSmallDate = ZDateTime.MinSmallDateTimeValue;

			var excessMinSmallDateError = $"The date '{minSmallDate.AddMonths(-1):dd-MMM-yyyy}' is earlier than '{minSmallDate:dd-MMM-yyyy}', the limit for this field.";
			var excessMaxSmallDateError = $"The date '{maxSmallDate.AddMonths(1):dd-MMM-yyyy}' is later than '{maxSmallDate:dd-MMM-yyyy}', the limit for this field.";
			var mandatoryEnterError = "Please enter a value.";

			wrapper.BuyStartDate = ZDateTime.Empty;
			AssertEquals(1, wrapper.BuyStartDateInfo.Notifications.Count());
			Assert(wrapper.BuyStartDateInfo.HasError(mandatoryEnterError));

			wrapper.BuyExpiryDate = ZDateTime.Empty;
			AssertEquals(1, wrapper.BuyExpiryDateInfo.Notifications.Count());
			Assert(wrapper.BuyExpiryDateInfo.HasError(mandatoryEnterError));

			wrapper.BuyStartDate = maxSmallDate.AddMonths(1);
			wrapper.BuyExpiryDate = ZDateTime.Today;
			Assert(wrapper.BuyStartDateInfo.HasError(excessMaxSmallDateError));

			wrapper.BuyStartDate = minSmallDate.AddMonths(-1);
			wrapper.BuyExpiryDate = ZDateTime.Today;
			Assert(wrapper.BuyStartDateInfo.HasError(excessMinSmallDateError));

			wrapper.BuyExpiryDate = maxSmallDate.AddMonths(1);
			wrapper.BuyStartDate = ZDateTime.Today;
			Assert(wrapper.BuyExpiryDateInfo.HasError(excessMaxSmallDateError));

			wrapper.BuyExpiryDate = minSmallDate.AddMonths(-1);
			wrapper.BuyStartDate = ZDateTime.Today;
			Assert(wrapper.BuyExpiryDateInfo.HasError(excessMinSmallDateError));

			wrapper.BuyStartDate = maxSmallDate.AddMonths(1);
			wrapper.BuyExpiryDate = minSmallDate.AddMonths(-1);
			Assert(wrapper.BuyStartDateInfo.HasError(excessMaxSmallDateError));
			Assert(wrapper.BuyExpiryDateInfo.HasError(excessMinSmallDateError));

			wrapper.BuyStartDate = minSmallDate.AddMonths(-1);
			wrapper.BuyExpiryDate = maxSmallDate.AddMonths(1);
			Assert(wrapper.BuyStartDateInfo.HasError(excessMinSmallDateError));
			Assert(wrapper.BuyExpiryDateInfo.HasError(excessMaxSmallDateError));
		}

		public void TestValidationOfExcessValidSmallDateForSellDates()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = new ZDateTime(year, 3, 4);
			exchangeRate.RE_ExpiryDate = new ZDateTime(year, 3, 8);
			exchangeRate.RE_ExRateType = ExchangeRateWrapper.BuyExRateType;
			exchangeRate.RE_RX_NKExCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			Factory.Save();

			var wrapper = new ExchangeRateWrapper(Factory);
			wrapper.CurrencyNK = exchangeRate.RE_RX_NKExCurrency;

			var maxSmallDate = ZDateTime.MaxSmallDateTimeValue;
			var minSmallDate = ZDateTime.MinSmallDateTimeValue;

			var excessMinSmallDateError = $"The date '{minSmallDate.AddMonths(-1):dd-MMM-yyyy}' is earlier than '{minSmallDate:dd-MMM-yyyy}', the limit for this field.";
			var excessMaxSmallDateError = $"The date '{maxSmallDate.AddMonths(1):dd-MMM-yyyy}' is later than '{maxSmallDate:dd-MMM-yyyy}', the limit for this field.";
			var mandatoryEnterError = "Please enter a value.";

			wrapper.SellStartDate = ZDateTime.Empty;
			AssertEquals(1, wrapper.SellStartDateInfo.Notifications.Count());
			Assert(wrapper.SellStartDateInfo.HasError(mandatoryEnterError));

			wrapper.SellExpiryDate = ZDateTime.Empty;
			AssertEquals(1, wrapper.SellExpiryDateInfo.Notifications.Count());
			Assert(wrapper.SellExpiryDateInfo.HasError(mandatoryEnterError));

			wrapper.SellStartDate = maxSmallDate.AddMonths(1);
			wrapper.SellExpiryDate = ZDateTime.Today;
			Assert(wrapper.SellStartDateInfo.HasError(excessMaxSmallDateError));

			wrapper.SellStartDate = minSmallDate.AddMonths(-1);
			wrapper.SellExpiryDate = ZDateTime.Today;
			Assert(wrapper.SellStartDateInfo.HasError(excessMinSmallDateError));

			wrapper.SellExpiryDate = maxSmallDate.AddMonths(1);
			wrapper.SellStartDate = ZDateTime.Today;
			Assert(wrapper.SellExpiryDateInfo.HasError(excessMaxSmallDateError));

			wrapper.SellExpiryDate = minSmallDate.AddMonths(-1);
			wrapper.SellStartDate = ZDateTime.Today;
			Assert(wrapper.SellExpiryDateInfo.HasError(excessMinSmallDateError));

			wrapper.SellStartDate = maxSmallDate.AddMonths(1);
			wrapper.SellExpiryDate = minSmallDate.AddMonths(-1);
			Assert(wrapper.SellStartDateInfo.HasError(excessMaxSmallDateError));
			Assert(wrapper.SellExpiryDateInfo.HasError(excessMinSmallDateError));

			wrapper.SellStartDate = minSmallDate.AddMonths(-1);
			wrapper.SellExpiryDate = maxSmallDate.AddMonths(1);
			Assert(wrapper.SellStartDateInfo.HasError(excessMinSmallDateError));
			Assert(wrapper.SellExpiryDateInfo.HasError(excessMaxSmallDateError));
		}

		#endregion
	}
}
