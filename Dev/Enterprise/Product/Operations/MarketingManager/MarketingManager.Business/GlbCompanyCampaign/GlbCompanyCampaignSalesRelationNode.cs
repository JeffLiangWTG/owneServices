using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSalesRelationMasterNode : GlbCompanyCampaignSalesRelationNode
	{
		public GlbCompanyCampaignSalesRelationMasterNode(GlbCompanyCampaignSalesRelationModel relationModel, GlbCompanyCampaign campaign)
			: base(relationModel, campaign)
		{
		}

		GlbCompanyCampaign GlbCompanyCampaign
		{
			get { return (GlbCompanyCampaign)base.BizObj; }
		}

		protected override IEnumerable<IRelatableActivity> LoadChildBizObjs()
		{
			EnsureListChangedEventsAdded();
			foreach (var activity in GlbCompanyCampaign.PostCampaignItemPivotCollection.Activities)
			{
				yield return activity;
			}

			foreach (var childBizObj in base.LoadChildBizObjs())
			{
				yield return childBizObj;
			}
		}

		void EnsureListChangedEventsAdded()
		{
			if (!listChangedEventsAdded)
			{
				listChangedEventsAdded = true;
				((IBindingList)GlbCompanyCampaign.PostCampaignItemPivotCollection).ListChanged += CampaignItemChildPivotCollection_ListChanged;
			}
		}
		bool listChangedEventsAdded;

		void CampaignItemChildPivotCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			NotifyChildNodesChanged();
		}
	}

	public class GlbCompanyCampaignSalesRelationNode : SalesRelationNode
	{
		public GlbCompanyCampaignSalesRelationNode(GlbCompanyCampaignSalesRelationModel relationModel, IRelatableActivity relatableActivity)
			: base(relationModel, relatableActivity)
		{
		}

		new GlbCompanyCampaignSalesRelationModel TreeModel
		{
			get { return (GlbCompanyCampaignSalesRelationModel)base.TreeModel; }
		}

		protected override IRelatableActivity LoadParentBizObj()
		{
			var parentBizObj = base.LoadParentBizObj();
			var parentBizObjAsCampaignItem = parentBizObj as GlbCompanyCampaignItem;
			if (parentBizObjAsCampaignItem != null)
			{
				var parentBizObjCampaign = parentBizObjAsCampaignItem.CompanyCampaign;
				if (parentBizObjCampaign.PK == TreeModel.Master.PK)
				{
					parentBizObj = TreeModel.Master;
				}
			}

			return parentBizObj;
		}

		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(IRelatableActivity previousParent, IRelatableActivity newParent, bool checkValid)
		{
			var previousParentAsCampaign = previousParent as GlbCompanyCampaign;
			if (previousParentAsCampaign != null && previousParentAsCampaign.PK == TreeModel.Master.PK)
			{
				var campaignItemPks = new HashSet<ZGuid>(TreeModel.Master.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Select(item => item.PK));
				foreach (var bizObjParent in BizObj.RelatedParentActivityPivotCollection.Activities)
				{
					if (campaignItemPks.Contains(bizObjParent.PK))
					{
						previousParent = bizObjParent;
					}
				}
			}

			return base.ChangeParentOnBizObj(previousParent, newParent, checkValid);
		}
	}
}
