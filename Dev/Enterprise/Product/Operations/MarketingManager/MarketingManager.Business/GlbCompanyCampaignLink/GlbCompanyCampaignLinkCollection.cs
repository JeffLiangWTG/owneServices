using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignLinkCollection : ActiveBusinessObjectCollection<GlbCompanyCampaignLink>
	{
		public GlbCompanyCampaignLinkCollection(GlbCompanyCampaign campaign)
			: base(campaign.Factory, campaign, new ZQuery(), GlbCompanyCampaignLinkSchema.GCL_G0_Campaign)
		{
		}

		public GlbCompanyCampaignLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}

	/// <summary>
	/// A non-active collection useful for working on a copy of the active GlbCompanyCampaignLinkCollection version.
	/// Not active since adding to this copy should not add to the parent campaign.
	/// </summary>
	public class SimpleGlbCompanyCampaignLinkCollection : BusinessObjectCollection<GlbCompanyCampaignLink>
	{
		public SimpleGlbCompanyCampaignLinkCollection(GlbCompanyCampaign campaign, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Campaign = campaign;
		}

		readonly GlbCompanyCampaign Campaign;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			GlbCompanyCampaignLink link = (GlbCompanyCampaignLink)child;
			link.GCL_G0_Campaign = Campaign.PK;
			base.SetDefaultsForNewChild(link);
		}

		protected override bool AllowNewCore { get { return false; } }
		protected override bool AllowRemoveCore { get { return false; } }
		protected override CargoWise.Types.ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("d16421a7-5c95-4360-9cda-f16071a03096", "Campaign Links");
			}
		}
	}
}
