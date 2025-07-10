using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RefCurrencyCurrencyConverterTest : CurrencyConverterTestCase
	{
		public void TestGetRefExchangeRateObject()
		{
			var currency = Factory.New<RefCurrency>();
			var rate1 = currency.ExchangeRates.AddNew();
			rate1.RE_StartDate = new ZDateTime(2006, 5, 1);
			rate1.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate1.RE_SellRate = 0.5m;

			var rate2 = currency.ExchangeRates.AddNew();
			rate2.RE_StartDate = new ZDateTime(2006, 5, 1);
			rate2.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate2.RE_SellRate = 0.51m;

			var rate3 = currency.ExchangeRates.AddNew();
			rate3.RE_StartDate = new ZDateTime(2006, 5, 2);
			rate3.RE_ExpiryDate = new ZDateTime(2006, 5, 2);
			rate3.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate3.RE_SellRate = 0.52m;

			var rate4 = currency.ExchangeRates.AddNew();
			rate4.RE_StartDate = new ZDateTime(2006, 5, 2);
			rate4.RE_ExpiryDate = new ZDateTime(2006, 5, 2);
			rate4.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate4.RE_SellRate = 0.53m;

			var rate5 = currency.ExchangeRates.AddNew();
			rate5.RE_StartDate = new ZDateTime(1900, 1, 1);
			rate5.RE_ExpiryDate = new ZDateTime(1900, 1, 1);
			rate5.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate5.RE_SellRate = 0.54m;

			var rate6 = currency.ExchangeRates.AddNew();
			rate6.RE_StartDate = new ZDateTime(2079, 6, 6);
			rate6.RE_ExpiryDate = new ZDateTime(2079, 6, 6);
			rate6.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate6.RE_SellRate = 0.55m;

			var rate7 = currency.ExchangeRates.AddNew();
			rate7.RE_StartDate = new ZDateTime(2022, 1, 1);
			rate7.RE_ExpiryDate = new ZDateTime(2022, 1, 1);
			rate7.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate;
			rate7.RE_SellRate = 0.56m;

			CombineAssertions(() =>
			{
				var currencyConverter = new RefCurrencyCurrencyConverter(Factory, new ZDateTime(2006, 5, 1), ExchangeRateType.Customs, 0);
				AssertEquals("rate1", rate1, currencyConverter.GetExchangeRateObject(currency));

				currencyConverter = new RefCurrencyCurrencyConverter(Factory, new ZDateTime(2006, 5, 3), ExchangeRateType.Customs, 1);
				AssertEquals("rate3", rate3, currencyConverter.GetExchangeRateObject(currency));

				currencyConverter = new RefCurrencyCurrencyConverter(Factory, new ZDateTime(2006, 5, 1), ExchangeRateType.Sell, 0);
				AssertEquals("rate2", rate2, currencyConverter.GetExchangeRateObject(currency));

				currencyConverter = new RefCurrencyCurrencyConverter(Factory, new ZDateTime(1900, 1, 1), ExchangeRateType.Sell, 1);
				AssertEquals("rate5", rate5, currencyConverter.GetExchangeRateObject(currency));

				currencyConverter = new RefCurrencyCurrencyConverter(Factory, new ZDateTime(2079, 6, 6), ExchangeRateType.Sell, 1);
				AssertEquals("rate6", rate6, currencyConverter.GetExchangeRateObject(currency));

				currencyConverter = new RefCurrencyCurrencyConverter(Factory, new ZDateTime(2022, 1, 1), ExchangeRateType.CustomsMeasureEURExRate, 1);
				AssertEquals("rate7", rate7, currencyConverter.GetExchangeRateObject(currency));
			});
		}

		[ExpectNoExceptions()]
		public void TestCurrencyConverterFactoryConstructor()
		{
			CurrencyConverter.New(new BusinessObjectFactory());
		}

		[ExpectNoExceptions()]
		public void TestFullyParameterisedConstructor()
		{
			CurrencyConverter.New(new BusinessObjectFactory(), new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
		}

		public void TestGetExchangeRateWhenRateTypeIsAllAndOnlyOneRateExists()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.All, 0);
			Assert("Customs RateType is there", currencyConverter.GetExchangeRate(DecoratedCurrency) != 0);

			DecoratedExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			Assert("Sell RateType is there", currencyConverter.GetExchangeRate(DecoratedCurrency) != 0);

			DecoratedExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			Assert("Sell RateType is there", currencyConverter.GetExchangeRate(DecoratedCurrency) != 0);

			DecoratedExchangeRate.Delete();
			Assert("No exchange rate for this currency", currencyConverter.GetExchangeRate(DecoratedCurrency) == 0);
		}

		public void TestGetExchangeRateAllRateTypeWhenBuyAndSellRecordsExist()
		{
			ZDecimal buyRate = 0.90m;
			ZDecimal sellRate = 0.80m;

			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.All, 0);
			DecoratedExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			DecoratedExchangeRate.RE_SellRate = buyRate;

			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.SellRate, new ZDateTime(2003, 11, 13), new ZDateTime(2003, 11, 15), sellRate, DecoratedCurrency);

			ZQuery customsRateFilter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, DecoratedExchangeRate.RE_RX_NKExCurrency);
			customsRateFilter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			BusinessObject[] customsRate = Factory.Load(typeof(RefExchangeRate), customsRateFilter);

			Assert("PreCondition:CustomsRate shouldn't exist", customsRate.Length == 0);
			AssertEquals("Exchange Rate returned by CurrencyConverter", sellRate, currencyConverter.GetExchangeRate(DecoratedCurrency));
		}

		public void TestGetExchangeRateAllRateTypeWhenSellAndCustomsRecordsExist()
		{
			DecoratedExchangeRate.RE_SellRate = 0.90m;

			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.All, 0);
			ZDecimal sellRate = 0.80m;
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.SellRate, new ZDateTime(2003, 11, 13), new ZDateTime(2003, 11, 15), sellRate, DecoratedCurrency);

			AssertEquals("PreCondition:DecoratedExchangeRate is Customs type", Core.Constants.ExchangeRateTypes.Code.CustomsRate, DecoratedExchangeRate.RE_ExRateType);
			AssertEquals("Exchange Rate returned by CurrencyConverter", DecoratedExchangeRate.RE_SellRate, currencyConverter.GetExchangeRate(DecoratedCurrency));
		}

		public void TestConvertForeignToLocal()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money foreignAmount = new Money(1000, DecoratedCurrency);
			Money localAmount = currencyConverter.ConvertRounded(foreignAmount, LocalCurrency);
			AssertEquals("Conversion of $1000 @ .70", new ZDecimal(1428.57), localAmount.Amount);
			AssertEquals("Money Currency", LocalCurrency.RX_Code, localAmount.Currency.Code);
		}

		public void TestConvertLocalToForeign()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money localAmount = new Money(1428.57M, LocalCurrency);
			Money foreignAmount = currencyConverter.ConvertRounded(localAmount, DecoratedCurrency);
			AssertEquals("Conversion of $1428.57 @ 1 / .70", new ZDecimal(1000), foreignAmount.Amount);
			AssertEquals("Money Currency", DecoratedCurrency.RX_Code, foreignAmount.Currency.Code);
		}

		public void TestLocalToLocal()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money localAmount1 = new Money(1000, LocalCurrency);
			Money localAmount2 = currencyConverter.ConvertRounded(localAmount1, LocalCurrency);
			AssertEquals("Conversion of $1000 @ 1", localAmount1.Amount, localAmount2.Amount);
			AssertEquals("Money Currency", LocalCurrency.PK, localAmount2.Currency.PK);
		}

		public void TestForeignToForeign()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money foreignAmount1 = new Money(1000, DecoratedCurrency);
			Money foreignAmount2 = currencyConverter.ConvertRounded(foreignAmount1, DecoratedCurrency);
			AssertEquals("Conversion of $1000 @ .70 > @ 1/.70", foreignAmount1.Amount, foreignAmount2.Amount);
			AssertEquals("Money Currency", foreignAmount1.Currency, foreignAmount2.Currency);
		}

		public void TestGetExchangeRateWithInvalidDate()
		{
			CurrencyConverter converter = CurrencyConverter.New(Factory, ZDateTime.Invalid, ExchangeRateType.Customs, 0);
			AssertEquals(0m, converter.GetExchangeRate(RefCurrency.New(Factory)));
			AssertEquals(1m, converter.GetExchangeRate(LocalCurrency));
		}

		public void TestGetExchangeRateWithEmptyDate()
		{
			CurrencyConverter converter = CurrencyConverter.New(Factory, ZDateTime.Empty, ExchangeRateType.Customs, 0);
			AssertEquals(0m, converter.GetExchangeRate(RefCurrency.New(Factory)));
			AssertEquals(1m, converter.GetExchangeRate(LocalCurrency));
		}

		public void TestDatabaseHitsWhenOutOfDateExchangeRate()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "NEW";
			RefExchangeRate rate1 = currency.ExchangeRates.AddNew();
			rate1.RE_StartDate = new ZDateTime(2006, 5, 1);
			rate1.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rate1.RE_ExRateType = "CUS";
			rate1.RE_SellRate = 0.5m;

			RefExchangeRate rate2 = currency.ExchangeRates.AddNew();
			rate2.RE_StartDate = new ZDateTime(2006, 5, 1);
			rate2.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rate2.RE_ExRateType = "SEL";
			rate2.RE_SellRate = 0.51m;

			RefExchangeRate rate3 = currency.ExchangeRates.AddNew();
			rate3.RE_StartDate = new ZDateTime(2006, 5, 2);
			rate3.RE_ExpiryDate = new ZDateTime(2006, 5, 2);
			rate3.RE_ExRateType = "CUS";
			rate3.RE_SellRate = 0.52m;

			RefExchangeRate rate4 = currency.ExchangeRates.AddNew();
			rate4.RE_StartDate = new ZDateTime(2006, 5, 2);
			rate4.RE_ExpiryDate = new ZDateTime(2006, 5, 2);
			rate4.RE_ExRateType = "SEL";
			rate4.RE_SellRate = 0.53m;

			Factory.ResetDatabaseLoadCount();
			RefCurrencyCurrencyConverter currencyConverter = new RefCurrencyCurrencyConverter(Factory, new ZDateTime(2009, 11, 3), ExchangeRateType.Customs, 365 * 5);
			AssertEquals("GetRefExchangeRateObject()", rate3, currencyConverter.GetExchangeRateObject(currency));

			AssertEquals(1, Factory.GetTableHitCount("RefExchangeRate"));
		}

		public void TestGetExchangeRate_WhenLocalCurrencyIsNull()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "";

			var converter = new RefCurrencyCurrencyConverter(company, Factory);
			AssertNull("Company should have null LocalCurrency", converter.Company.LocalCurrency);
			AssertNoExceptionThrown(() => converter.GetExchangeRate(Factory.New<RefCurrency>()));
		}

		public void TestGetExchangeRate_TwoCurrencies()
		{
			RefCurrency currencyA = Factory.New<RefCurrency>();
			currencyA.RX_Code = "AAA";
			RefExchangeRate rateA = currencyA.ExchangeRates.AddNew();
			rateA.RE_StartDate = new ZDateTime(2006, 5, 1);
			rateA.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rateA.RE_ExRateType = "CUS";
			rateA.RE_SellRate = 0.5m;

			RefCurrency currencyB = Factory.New<RefCurrency>();
			currencyB.RX_Code = "BBB";
			RefExchangeRate rateB = currencyB.ExchangeRates.AddNew();
			rateB.RE_StartDate = new ZDateTime(2006, 5, 1);
			rateB.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rateB.RE_ExRateType = "CUS";
			rateB.RE_SellRate = 0.5m;

			RefCurrency currencyC = Factory.New<RefCurrency>();
			currencyC.RX_Code = "CCC";
			RefExchangeRate rateC = currencyC.ExchangeRates.AddNew();
			rateC.RE_StartDate = new ZDateTime(2006, 5, 1);
			rateC.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rateC.RE_ExRateType = "CUS";
			rateC.RE_SellRate = 0.2m;

			RefCurrencyCurrencyConverter currencyConverter = new RefCurrencyCurrencyConverter(Factory, new ZDateTime(2006, 5, 1), ExchangeRateType.Customs, 365 * 5);
			AssertEquals("GetRefExchange(CurrencyA, CurrencyB)", 1m, currencyConverter.GetExchangeRate(currencyA, currencyB));
			AssertEquals("GetRefExchange(CurrencyA, CurrencyC)", 2.5m, currencyConverter.GetExchangeRate(currencyA, currencyC));
		}

		[TestDate(2018, 11, 21)]
		public void TestGetRefExchangeRateObjectIgnoreLocalClient()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "SOME ORG";

			RefCurrency currency = Factory.New<RefCurrency>();

			RefExchangeRate rate = currency.ExchangeRates.AddNew();
			rate.RE_StartDate = new ZDateTime(2018, 11, 1);
			rate.RE_ExpiryDate = new ZDateTime(2018, 11, 30);
			rate.RE_ExRateType = "BUY";
			rate.RE_SellRate = 0.51m;
			rate.RE_OH_Client = orgHeader.PK;

			RefCurrencyCurrencyConverter currencyConverter = new RefCurrencyCurrencyConverter(Factory, ZDateTime.Today, ExchangeRateType.Buy, 7);
			AssertNotEquals("GetRefExchangeRateObject()", rate, currencyConverter.GetExchangeRateObject(currency));

			rate.RE_OH_Client = ZGuid.Empty;

			AssertEquals("GetRefExchangeRateObject()", rate, currencyConverter.GetExchangeRateObject(currency));
		}

		public void TestRateCode()
		{
			var refCurrencyCurrencyConverter = new RefCurrencyCurrencyConverterExposed(Factory);

			CombineAssertions(() =>
			{
				refCurrencyCurrencyConverter.RateType = ExchangeRateType.Buy;
				AssertEquals("BUY", ExchangeRateTypes.Code.BuyRate, refCurrencyCurrencyConverter.RateCode_Exposed);

				refCurrencyCurrencyConverter.RateType = ExchangeRateType.Sell;
				AssertEquals("SEL", ExchangeRateTypes.Code.SellRate, refCurrencyCurrencyConverter.RateCode_Exposed);

				refCurrencyCurrencyConverter.RateType = ExchangeRateType.Customs;
				AssertEquals("CUS", ExchangeRateTypes.Code.CustomsRate, refCurrencyCurrencyConverter.RateCode_Exposed);

				refCurrencyCurrencyConverter.RateType = ExchangeRateType.CustomsSecondary;
				AssertEquals("CUE", ExchangeRateTypes.Code.CustomsRateSecondary, refCurrencyCurrencyConverter.RateCode_Exposed);

				refCurrencyCurrencyConverter.RateType = ExchangeRateType.CustomsMeasureEURExRate;
				AssertEquals("CUD", ExchangeRateTypes.Code.CustomsMeasureEURExRate, refCurrencyCurrencyConverter.RateCode_Exposed);

				refCurrencyCurrencyConverter.RateType = ExchangeRateType.C99;
				AssertEquals("Default", ExchangeRateTypes.Code.BuyRate, refCurrencyCurrencyConverter.RateCode_Exposed);
			});
		}
	}

	class RefCurrencyCurrencyConverterExposed : RefCurrencyCurrencyConverter
	{
		public RefCurrencyCurrencyConverterExposed(BusinessObjectFactory factory) : base(factory)
		{
		}

		public string RateCode_Exposed => base.RateCode;
	}
}
