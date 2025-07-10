using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignBudgetItem))]
	sealed class GlbCompanyCampaignBudgetItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCurrency()
		{
			var iNR = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "INR"));
			iNR.ExchangeRates.DeleteAll();
			RefExchangeRate rate = iNR.ExchangeRates.AddNew();
			rate.RE_ExRateType = "BUY";
			rate.RE_SellRate = 1.74m;

			var campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaignBudgetItem item = campaign.BudgetItems.AddNew();

			item.G9_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, item.G9_ExchangeRate);

			item.G9_RX_NKCurrency = "INR";
			AssertEquals(1.74m, item.G9_ExchangeRate);

			item.G9_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, item.G9_ExchangeRate);

			item.G9_RX_NKCurrency = "XXX";
			AssertEquals(1m, item.G9_ExchangeRate);
		}

		public void TestChargeCodeDescription()
		{
			var code = Factory.New<AccChargeCode>();
			code.AC_Code = "AAA";
			code.AC_Desc = "AAA Charge Code";

			var campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaignBudgetItem item = campaign.BudgetItems.AddNew();

			item.G9_AC = code.PK;
			AssertEquals("AAA Charge Code", item.G9_ChargeDescription);

			item.G9_AC = ZGuid.Empty;
			AssertEquals("AAA Charge Code", item.G9_ChargeDescription);
		}

		public void TestLocalAmounts()
		{
			var iNR = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "INR"));
			iNR.ExchangeRates.DeleteAll();
			RefExchangeRate rate = iNR.ExchangeRates.AddNew();
			rate.RE_ExRateType = "BUY";
			rate.RE_SellRate = 30m;

			var campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaignBudgetItem item = campaign.BudgetItems.AddNew();

			item.G9_RX_NKCurrency = "INR";
			AssertEquals("Precondition", 30m, item.G9_ExchangeRate);

			item.G9_PerUnitAmount = 600m;
			item.G9_FlatAmount = 1000m;

			AssertEquals(20m, item.LocalPerUnitAmount);
			AssertEquals(33.33m, item.LocalFlatAmount);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var campaign = factory.NewWithValidTestData<GlbCompanyCampaign>();
			return campaign.BudgetItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return campaign.BudgetItems.AddNew();
		}

		#endregion
	}
}
