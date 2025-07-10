using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OrgTradeDetailRevenueCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2015, 5, 5)]
		public void TestGetTotalInLocalCurrency()
		{
			var org = Factory.New<OrgHeader>();

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_IsTraded = true;
			var tradeDetail1 = sales1.TradeDetails.AddNew();
			var tradePeriod1 = tradeDetail1.TradedPeriods.AddNew();
			tradePeriod1.PAS_Period = new ZDate(2015, 5, 1);
			tradePeriod1.PAS_OH_Client = org.PK;
			var tradeValue1 = tradePeriod1.TradeValues.AddNew();
			tradeValue1.PAV_GC = Env.CurrentCompanyPK;
			tradeValue1.PAV_Revenue = 100;
			tradeValue1.PAV_RX_NKCurrency = "AUD";

			var sales2 = org.SalesCollection.AddNew();
			var tradeDetail2 = sales2.TradeDetails.AddNew();
			var tradePeriod2 = tradeDetail2.TradedPeriods.AddNew();
			tradePeriod2.PAS_Period = new ZDate(2015, 5, 1);
			tradePeriod2.PAS_OH_Client = org.PK;
			var tradeValue2 = tradePeriod2.TradeValues.AddNew();
			tradeValue2.PAV_GC = Env.CurrentCompanyPK;
			tradeValue2.PAV_Revenue = 100;
			tradeValue2.PAV_RX_NKCurrency = "USD";

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2015, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2016, 1, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			var calculator = new OrgSalesRevenueCalculator(Factory, null);
			AssertEquals(100m + 100m / 2m, calculator.GetTotalInEntityCurrency(new[] { tradeValue1, tradeValue2 }, x => x.PAV_RX_NKCurrency, x => x.PAV_Revenue));
		}

		[TestDate(2015, 5, 5)]
		public void TestExchangeRate_FromLocalCurrency()
		{
			AssertEquals("Precondition", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var opp = Factory.New<OrgOpportunity>();
			opp.P8_RX_NKEstimatedValueCurrency = "USD";
			var calculator = new OrgSalesRevenueCalculator(Factory, opp);
			AssertContainsExactElementsInAnyOrder(new[] { "USD" }, calculator.GetMissingExchangeRatesToConvertTo(aud).Select(x => x.Code));
			AssertEquals(0m, calculator.GetExchangeRate(aud));

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2015, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2016, 1, 1);
			usdExRate.RE_SellRate = 2m;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), calculator.GetMissingExchangeRatesToConvertTo(aud).Select(x => x.Code));
			AssertEquals(0.5m, calculator.GetExchangeRate(aud));
		}

		[TestDate(2015, 5, 5)]
		public void TestExchangeRate_FromForeignCurrency()
		{
			AssertNotEquals("Precondition", "USD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2015, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2015, 2, 1);
			usdExRate.RE_SellRate = 2m;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			var opp = Factory.New<OrgOpportunity>();
			opp.P8_RX_NKEstimatedValueCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var calculator = new OrgSalesRevenueCalculator(Factory, opp);
			AssertContainsExactElementsInAnyOrder(new[] { "USD" }, calculator.GetMissingExchangeRatesToConvertTo(usd).Select(x => x.Code));
			AssertEquals(0m, calculator.GetExchangeRate(usd));

			usdExRate.RE_ExpiryDate = new ZDateTime(2016, 1, 1);
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), calculator.GetMissingExchangeRatesToConvertTo(usd).Select(x => x.Code));
			AssertEquals(2m, calculator.GetExchangeRate(usd));

			usdExRate.RE_ExpiryDate = new ZDateTime(2015, 4, 8);
			AssertContainsExactElementsInAnyOrder("Should fallback to most recent exchange rate up to 30 days", Enumerable.Empty<string>(), calculator.GetMissingExchangeRatesToConvertTo(usd).Select(x => x.Code));
			AssertEquals("Should fallback to most recent exchange rate up to 30 days", 2m, calculator.GetExchangeRate(usd));

			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), calculator.GetMissingExchangeRatesToConvertTo(null).Select(x => x.Code));

			opp.P8_RX_NKEstimatedValueCurrency = "XXX";
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), calculator.GetMissingExchangeRatesToConvertTo(usd).Select(x => x.Code));
		}
	}
}
