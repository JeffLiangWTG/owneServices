using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignBudgetItemCollection))]
	sealed class GlbCompanyCampaignBudgetItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultValues()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();

			campaign.G0_RX_NKCampaignCurrency = "INR";
			GlbCompanyCampaignBudgetItem item = campaign.BudgetItems.AddNew();
			AssertEquals("INR", item.G9_RX_NKCurrency);

			campaign.G0_RX_NKCampaignCurrency = "USD";
			GlbCompanyCampaignBudgetItem item2 = campaign.BudgetItems.AddNew();
			AssertEquals("INR", item.G9_RX_NKCurrency);
			AssertEquals("USD", item2.G9_RX_NKCurrency);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			return campaign.BudgetItems;
		}
	}
}
