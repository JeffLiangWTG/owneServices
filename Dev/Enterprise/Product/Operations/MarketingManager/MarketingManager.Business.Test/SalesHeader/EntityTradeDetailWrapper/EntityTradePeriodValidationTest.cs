using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class EntityTradePeriodValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPAS_RX_NKCurrency()
		{
			var opp = Factory.New<OrgOpportunity>();
			opp.P8_DateForExchangeRate = new ZDateTime(2002, 2, 2);
			opp.P8_RX_NKEstimatedValueCurrency = "USD";
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency = "NZD";
			var entityTradeDetail = EntityTradeDetailWrapper.Get(tradeDetail, opp);
			var entityTradePeriod = entityTradeDetail.CurrentProspectPeriod as EntityTradePeriod;
			entityTradePeriod.Validation.ValidatePAS_RX_NKCurrency();

			AssertHasWarning(entityTradePeriod.PAS_RX_NKCurrencyInfo, "There is no exchange rate valid on 02-Feb-02 for the following currency(s): NZD, USD");
			AssertHasWarning(entityTradeDetail.CurrencyCodeInfo, "There is no exchange rate valid on 02-Feb-02 for the following currency(s): NZD, USD");
		}

		public void TestCheckPAS_RX_NKCurrency_ForDifferentToCurrentCompany()
		{
			var anotherCompany = Factory.New<GlbCompany>();
			anotherCompany.GC_Name = "Another Company Ltd";
			anotherCompany.GC_Code = "XXX";
			var opp = Factory.New<OrgOpportunity>();
			opp.P8_DateForExchangeRate = new ZDateTime(2002, 2, 2);
			opp.P8_RX_NKEstimatedValueCurrency = "USD";
			opp.P8_GC = anotherCompany.PK;

			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency = "NZD";
			var entityTradeDetail = EntityTradeDetailWrapper.Get(tradeDetail, opp);
			var entityTradePeriod = entityTradeDetail.CurrentProspectPeriod as EntityTradePeriod;
			entityTradePeriod.Validation.ValidatePAS_RX_NKCurrency();

			AssertHasWarning(entityTradePeriod.PAS_RX_NKCurrencyInfo, "Another Company Ltd (XXX) does not have an exchange rate valid on 02-Feb-02 for the following currency(s): NZD, USD");
			AssertHasWarning(entityTradeDetail.CurrencyCodeInfo, "Another Company Ltd (XXX) does not have an exchange rate valid on 02-Feb-02 for the following currency(s): NZD, USD");
		}
	}
}
