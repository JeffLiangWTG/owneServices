using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignBudgetItemValidation : AutoGlbCompanyCampaignBudgetItemValidation
	{
		public GlbCompanyCampaignBudgetItemValidation(AutoGlbCompanyCampaignBudgetItem parent) : base(parent)
		{
		}

		#region G9_ExchangeRate

		protected override void CheckG9_ExchangeRate()
		{
			base.CheckG9_ExchangeRate();
			if (Parent.G9_ExchangeRate < 0)
			{
				Parent.G9_ExchangeRateInfo.AddError(Res.GetString("bed0e631-000f-485d-888e-efb76b154f53", "Exchange Rate must be non-negative."));
			}
		}

		#endregion

		#region G9_FlatAmount

		protected override void CheckG9_FlatAmount()
		{
			base.CheckG9_FlatAmount();
			if (Parent.G9_FlatAmount < 0)
			{
				Parent.G9_FlatAmountInfo.AddError(Res.GetString("2eeeac6d-3c70-41c6-a183-f829652a5b6d", "Flat amount must be non-negative."));
			}
		}

		#endregion

		#region G9_PerUnitAmount

		protected override void CheckG9_PerUnitAmount()
		{
			base.CheckG9_PerUnitAmount();
			if (Parent.G9_PerUnitAmount < 0)
			{
				Parent.G9_PerUnitAmountInfo.AddError(Res.GetString("1d6e8dfe-1d48-4e68-835f-fc7be0b1ae9a", "Per Unit amount must be non-negative."));
			}
		}

		#endregion

		#region G9_RX_NKCurrency

		protected override void CheckG9_RX_NKCurrency()
		{
			base.CheckG9_RX_NKCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.G9_RX_NKCurrencyInfo, Parent.Lookups.Currencies);
		}

		#endregion
	}
}
