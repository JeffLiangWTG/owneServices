using Enterprise.BufferManagement.Integration;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using CampaignContactFilterCategories = Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignModuleFilterCollection : ModuleFilterCollection
	{
		public GlbCompanyCampaignModuleFilterCollection(GlbCompanyCampaign campaign)
		{
			Campaign = campaign;
		}

		internal void ResetCampaignFilterList()
		{
			ResetFilterList();
		}

		readonly GlbCompanyCampaign Campaign;

		protected override bool IsFilterAvailable(ModuleFilter filter)
		{
			bool isFilterAvailable = base.IsFilterAvailable(filter);

			if (filter.Category == CampaignContactFilterCategories.CommonTypes
				|| filter.Code == FilterStripBusinessObject.CustomSqlFilterDescription
				|| BMGlobalConstants.BufferManagementCategoryDescription.Equals(filter.Category.Description))
			{
				return isFilterAvailable;
			}

			if (Campaign != null)
			{
				if (Campaign.IsUsingCampaignTrackingDataSource) // all filters available for campaign tracking
				{
					return isFilterAvailable;
				}

				if (filter.Category == CampaignContactFilterCategories.ClientIntelligenceAndInquiries)
				{
					return isFilterAvailable;
				}

				if (filter.Category == FilterCategories.SalesRelationActivity)
				{
					return isFilterAvailable && Campaign.IsUsingClientIntelligenceDataSource;
				}

				if (filter.Category == CampaignContactFilterCategories.Inquiries)
				{
					return isFilterAvailable && Campaign.IsUsingInquiryDataSource;
				}
				else if (filter.Category != CampaignContactFilterCategories.Inquiries && filter.Category != CampaignContactFilterCategories.CampaignTracking && filter.Category != GlbCompanyCampaignItemFilterBusinessObject.LinkActivityCategories.LinkActivity)
				{
					return isFilterAvailable && Campaign.IsUsingClientIntelligenceDataSource;
				}
				else if (filter.Category == CampaignContactFilterCategories.CampaignTracking || filter.Category == GlbCompanyCampaignItemFilterBusinessObject.LinkActivityCategories.LinkActivity)
				{
					return isFilterAvailable && Campaign.IsUsingCampaignTrackingDataSource;
				}
			}

			return isFilterAvailable;
		}
	}
}
