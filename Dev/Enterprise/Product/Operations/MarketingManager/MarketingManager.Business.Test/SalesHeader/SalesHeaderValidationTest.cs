using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class SalesHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTotalCurrencyCode()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			opp.P8_DateForExchangeRate = new ZDateTime(2002, 2, 2);
			opp.P8_RX_NKEstimatedValueCurrency = "USD";
			var salesHeader = new SalesHeader(org, opp, salesProduct);
			salesHeader.Validation.ValidateTotalCurrencyCode();

			AssertHasWarning(salesHeader.TotalCurrencyCodeInfo, "There is no exchange rate valid on 02-Feb-02 for the following currency(s): USD");
		}

		public void TestCheckTotalCurrencyCode_ForDifferentToCurrentCompany()
		{
			var anotherCompany = Factory.New<GlbCompany>();
			anotherCompany.GC_Name = "Another Company Ltd";
			anotherCompany.GC_Code = "XXX";
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			opp.P8_DateForExchangeRate = new ZDateTime(2002, 2, 2);
			opp.P8_RX_NKEstimatedValueCurrency = "USD";
			opp.P8_GC = anotherCompany.PK;
			var salesHeader = new SalesHeader(org, opp, salesProduct);
			salesHeader.Validation.ValidateTotalCurrencyCode();

			AssertHasWarning(salesHeader.TotalCurrencyCodeInfo, "Another Company Ltd (XXX) does not have an exchange rate valid on 02-Feb-02 for the following currency(s): USD");
		}
	}
}
