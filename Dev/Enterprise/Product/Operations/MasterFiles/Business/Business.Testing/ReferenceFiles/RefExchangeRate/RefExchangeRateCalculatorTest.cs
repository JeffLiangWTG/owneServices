using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefExchangeRateCalculator))]
	sealed class RefExchangeRateCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Calculator;
		}

		public void TestProperties()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AssertEquals("KRW", Calculator.QuoteCurrency);
				AssertEquals(100m, Calculator.QuoteCurrencyValue);
				AssertEquals("INR", Calculator.BaseCurrency);
				AssertEquals(300m, Calculator.BaseCurrencyValue);
			}
		}

		public void TestCalculateExchangeRate()
		{
			Calculator.QuoteCurrencyValue = 200m;
			Calculator.BaseCurrencyValue = 290m;
			Calculator.CalculateExchangeRate(1);
			CombineAssertions(() =>
			{
				AssertEquals(1.5m, ExchangeRate.RE_SellRate);
				Calculator.QuoteCurrencyValue = 100m;
				Calculator.CalculateExchangeRate(1);
				AssertEquals(2.9m, ExchangeRate.RE_SellRate);
				Calculator.QuoteCurrencyValue = 0m;
				Calculator.CalculateExchangeRate(1);
				AssertEquals(2.9m, ExchangeRate.RE_SellRate);
			});
		}

		RefExchangeRateCalculator Calculator
		{
			get
			{
				if (calculator == null)
				{
					var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
					helper.CreateNewOrGetExistingDataGrouping("IN", "India");
					helper.CreateNewOrGetExistingCusCodeType("SDCUR", "IN Customs Standard Currency List", "IN");
					var codeList2PK = helper.CreateNewOrGetExistingCusCodeList("IN", "SDCUR", "KRW", "South Korean Won", new ZDateTime(2019, 1, 1), new ZDateTime(2079, 6, 6)).PK;
					Factory.Save();
					helper.CreateNewOrGetExistingCusCodeListAttribute(codeList2PK, "Multiplier", "100");
					Factory.Save();

					ExchangeRate.RE_SellRate = 3m;
					ExchangeRate.RE_RX_NKExCurrency = "KRW";
					calculator = new RefExchangeRateCalculator(exchangeRate);
				}

				return calculator;
			}
		}
		RefExchangeRateCalculator calculator;

		RefExchangeRate ExchangeRate => exchangeRate ?? (exchangeRate = RefExchangeRate.New(Factory));
		RefExchangeRate exchangeRate;
	}
}
