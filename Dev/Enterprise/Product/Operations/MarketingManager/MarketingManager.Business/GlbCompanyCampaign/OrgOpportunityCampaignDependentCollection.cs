using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class OrgOpportunityCampaignDependentCollection : ActiveBusinessObjectCollection<OrgOpportunity>
	{
		public OrgOpportunityCampaignDependentCollection(GlbCompanyCampaign campaign)
			: base(campaign.Factory, new ManyToManyRelationship(campaign, typeof(OrgOpportunity), typeof(ViewRelatedActivityPivot), new ZQuery(), ViewRelatedActivityPivotSchema.RAP_ParentActivityID, ViewRelatedActivityPivotSchema.RAP_ChildActivityID))
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
