using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class EntitySalesWrapperValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTotalRevenueCurrencyCode()
		{
			var opp = Factory.New<OrgOpportunity>();
			opp.P8_DateForExchangeRate = new ZDateTime(2002, 2, 2);
			opp.P8_RX_NKEstimatedValueCurrency = "USD";
			var sales = Factory.New<OrgSales>();
			sales.OW_IsCustomRevenue = true;
			sales.OW_RX_NKRevenueCurrency = "NZD";
			var entitySales = EntitySalesWrapper.Get(sales, opp);
			entitySales.Validation.ValidateTotalRevenueCurrencyCode();

			AssertHasWarning(entitySales.TotalRevenueCurrencyCodeInfo, "There is no exchange rate valid on 02-Feb-02 for the following currency(s): NZD, USD");
		}

		public void TestValidateTotalRevenueCurrencyCode_ForDifferentToCurrentCompany()
		{
			var anotherCompany = Factory.New<GlbCompany>();
			anotherCompany.GC_Name = "Another Company Ltd";
			anotherCompany.GC_Code = "XXX";
			var opp = Factory.New<OrgOpportunity>();
			opp.P8_DateForExchangeRate = new ZDateTime(2002, 2, 2);
			opp.P8_RX_NKEstimatedValueCurrency = "USD";
			opp.P8_GC = anotherCompany.PK;

			var sales = Factory.New<OrgSales>();
			sales.OW_IsCustomRevenue = true;
			sales.OW_RX_NKRevenueCurrency = "NZD";
			var entitySales = EntitySalesWrapper.Get(sales, opp);
			entitySales.Validation.ValidateTotalRevenueCurrencyCode();

			AssertHasWarning(entitySales.TotalRevenueCurrencyCodeInfo, "Another Company Ltd (XXX) does not have an exchange rate valid on 02-Feb-02 for the following currency(s): NZD, USD");
		}
	}
}
