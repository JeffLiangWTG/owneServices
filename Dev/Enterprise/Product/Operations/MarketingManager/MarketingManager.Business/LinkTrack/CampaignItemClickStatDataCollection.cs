using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignItemClickStatDataCollection : NonPersistentBusinessObjectCollection<CampaignItemClickStatData>
	{
		public CampaignItemClickStatDataCollection(GlbCompanyCampaignItem campaignItem)
			: base(campaignItem.Factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CampaignItemClickStatData(Factory, "", "");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
