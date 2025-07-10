using Enterprise.BufferManagement.Integration;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using HRCampaignContactFilterCategories = Enterprise.Recruiter.GUI.HRGlbCompanyCampaignContactFilterBusinessObject.HRCampaignContactFilterCategories;

namespace Enterprise.Recruiter.GUI
{
	public class HRGlbCompanyCampaignModuleFilterCollection : ModuleFilterCollection
	{
		public HRGlbCompanyCampaignModuleFilterCollection(HRGlbCompanyCampaign campaign)
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

			if (filter.Category == GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.CommonTypes
					|| filter.Code == FilterStripBusinessObject.CustomSqlFilterDescription
					|| BMGlobalConstants.BufferManagementCategoryDescription.Equals(filter.Category.Description))
			{
				return isFilterAvailable;
			}

			if (Campaign != null)
			{
				if (Campaign.IsUsingCampaignTrackingDataSource)
				{
					return isFilterAvailable;
				}

				if (filter.Category == HRCampaignContactFilterCategories.Staff)
				{
					return isFilterAvailable && ((HRGlbCompanyCampaign)Campaign).IsUsingStaffDataSource;
				}

				if (filter.Category == HRCampaignContactFilterCategories.JobApplicant)
				{
					return isFilterAvailable && ((HRGlbCompanyCampaign)Campaign).IsUsingJobApplicantDataSource;
				}
			}

			return false;
		}
	}
}
