using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignBudgetItemCollection : DependentBusinessObjectCollection<GlbCompanyCampaignBudgetItem, GlbCompanyCampaign>
	{
		public GlbCompanyCampaignBudgetItemCollection(GlbCompanyCampaign campaign) : base(campaign)
		{
		}

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((GlbCompanyCampaignBudgetItem)child).G9_RX_NKCurrency = Master.G0_RX_NKCampaignCurrency;
		}

		#endregion
	}
}
