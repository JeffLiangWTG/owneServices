using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class SentCampaignItemsRelatedChildActivityPivotCollection : ActiveBusinessObjectCollection<ViewRelatedActivityPivot>
	{
		#region Constructor

		public SentCampaignItemsRelatedChildActivityPivotCollection(GlbCompanyCampaign campaign)
			: base(campaign.Factory, GetCampaignItemRelatableChildrenQuery())
		{
			this.campaign = campaign;

			SetupAdditionalFilter();
		}

		static ZQuery GetCampaignItemRelatableChildrenQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityTableCode, GlbCompanyCampaignItemSchema.Constants.Prefix);
			return result;
		}

		#endregion

		#region Properties

		readonly GlbCompanyCampaign campaign;

		public IEnumerable<IRelatableActivity> Activities
		{
			get { return this.Select(x => x.ChildActivity).Where(x => x != null); }
		}

		#endregion

		#region AdditionalFilter

		void SetupAdditionalFilter()
		{
			RefreshAdditionalFilter();
			((IBindingList)campaign.CampaignsItemsSent).ListChanged += CampaignItemPostPivotCollection_ListChanged;
		}

		void CampaignItemPostPivotCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			RefreshAdditionalFilter();
		}

		void RefreshAdditionalFilter()
		{
			var query = new ZQuery() { AllowTableValuedParameters = true };
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, campaign.CampaignsItemsSent.Select(x => x.PK));

			AdditionalFilter = query;
		}

		#endregion

		#region Allowed Actions

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}
}
