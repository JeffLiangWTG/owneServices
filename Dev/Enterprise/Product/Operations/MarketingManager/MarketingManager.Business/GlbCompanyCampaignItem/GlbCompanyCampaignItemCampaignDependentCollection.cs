using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignItemCampaignDependentCollection : DependentBusinessObjectCollection<GlbCompanyCampaignItem, GlbCompanyCampaign>
	{
		public GlbCompanyCampaignItemCampaignDependentCollection(GlbCompanyCampaign parent) : base(parent, parent.Factory, true)
		{
		}

		public GlbCompanyCampaignItemCampaignDependentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public GlbCompanyCampaignItem FindOrCreateNewIfNotExist(GlbCompanyCampaignItem.RecipientInfo recipientInfo)
			=> FindByRecipientPK(recipientInfo.recipientPK)
				?? AddNew(recipientInfo);

		public GlbCompanyCampaignItem AddNew(GlbCompanyCampaignItem.RecipientInfo recipientInfo)
		{
			GlbCompanyCampaignItem result = AddNew();
			result.G8_RecipientID = recipientInfo.recipientPK;
			result.G8_RecipientTableCode = recipientInfo.recipientTableCode;
			return result;
		}

		public GlbCompanyCampaignItem FindByRecipientPK(ZGuid recipientPK)
		{
			foreach (GlbCompanyCampaignItem campaignItem in this)
			{
				if (campaignItem.G8_RecipientID == recipientPK)
				{
					return campaignItem;
				}
			}

			return null;
		}
	}
}
