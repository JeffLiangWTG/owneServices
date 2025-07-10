using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSalesRelationModel : SalesRelationModel
	{
		public GlbCompanyCampaignSalesRelationModel(GlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		public new GlbCompanyCampaign Master
		{
			get { return (GlbCompanyCampaign)base.Master; }
		}

		protected override ZNode<IRelatableActivity> CreateMasterNode()
		{
			return new GlbCompanyCampaignSalesRelationMasterNode(this, Master);
		}

		protected override ZNode<IRelatableActivity> CreateNewNodeCore(ZTreeModel<IRelatableActivity> treeModel, IRelatableActivity bizOj)
		{
			return new GlbCompanyCampaignSalesRelationNode((GlbCompanyCampaignSalesRelationModel)treeModel, bizOj);
		}
	}
}
