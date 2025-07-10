using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignBudgetItemValidationTest : BusinessObjectValidationTestCase
	{
		public void SetUpTestItem()
		{
			Campaign = Factory.New<GlbCompanyCampaign>();
			Item = Campaign.BudgetItems.AddNew();
		}

		public void TestCurrency()
		{
			SetUpTestItem();

			Item.G9_RX_NKCurrency = "";
			AssertNoErrors(Item.G9_RX_NKCurrencyInfo);

			Item.G9_RX_NKCurrency = "ZZ1";
			AssertHasErrors(Item.G9_RX_NKCurrencyInfo);

			Item.G9_RX_NKCurrency = "USD";
			AssertNoErrors(Item.G9_RX_NKCurrencyInfo);
		}
			
		public void TestExchangeRate()
		{
			SetUpTestItem();
			Item.G9_ExchangeRate = -1;
			AssertHasErrors("Exchange rate should not be negative", Item.G9_ExchangeRateInfo);
			Item.G9_ExchangeRate = 1;
			AssertNoErrors(Item.G9_ExchangeRateInfo);
		}

		public void TestFlatAmount()
		{
			SetUpTestItem();
			Item.G9_FlatAmount = -1;
			AssertHasErrors("Flat amount should not be negative", Item.G9_FlatAmountInfo);
			Item.G9_FlatAmount = 1;
			AssertNoErrors(Item.G9_FlatAmountInfo);
		}
		public void TestPerUnitAmount()
		{
			SetUpTestItem();
			Item.G9_PerUnitAmount = -1;
			AssertHasErrors("Per unit amount should not be negative", Item.G9_PerUnitAmountInfo);
			Item.G9_PerUnitAmount = 1;
			AssertNoErrors(Item.G9_PerUnitAmountInfo);
		}

		GlbCompanyCampaign Campaign;
		GlbCompanyCampaignBudgetItem Item;
	}
}
