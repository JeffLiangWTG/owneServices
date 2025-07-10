using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class LearningCentreCampaignFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddCampaignFilters(result);
			return result;
		}

		void AddCampaignFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(FilterDescription.CampaignID, GlbCompanyCampaignSchema.G0_CampaignID).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobSkillExamCampaignFilter|CampaignID", FilterDescription.CampaignID);
			filters.AddFiltersForTranslatableText(FilterDescription.CampaignName, GlbCompanyCampaignSchema.G0_CampaignName, typeof(GlbCompanyCampaign), ResString.GetMultilingualString("Recruiter|HRJobSkillExamCampaignFilter|CampaignName", FilterDescription.CampaignName));
			ModuleNkFilter campaignManagerFilter = filters.AddNkFilter(FilterDescription.CampaignManager, GlbCompanyCampaignSchema.G0_GS_NKCampaignManager, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			campaignManagerFilter.IsPublishedOnWeb = false;
			campaignManagerFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobSkillExamCampaignFilter|CampaignManager", FilterDescription.CampaignManager);
			ModuleNkFilter campaignCoordinatorFilter = filters.AddNkFilter(FilterDescription.CampaignCoordinator, GlbCompanyCampaignSchema.G0_GS_NKCampaignCoordinator, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			campaignCoordinatorFilter.IsPublishedOnWeb = false;
			campaignCoordinatorFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobSkillExamCampaignFilter|CampaignCoordinator", FilterDescription.CampaignCoordinator);
			var stageFilter = filters.AddTextFilter(FilterDescription.Stage, GlbCompanyCampaignSchema.G0_Stage, OrganisationsDataRegistry.Instance.CampaignStageList.Value);
			stageFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobSkillExamCampaignFilter|Stage", FilterDescription.Stage);
			stageFilter.IsPublishedOnWeb = false;
			filters.AddFiltersForTranslatableText(FilterDescription.Comment, GlbCompanyCampaignSchema.G0_CampaignComment, typeof(GlbCompanyCampaign), ResString.GetMultilingualString("Recruiter|HRJobSkillExamCampaignFilter|Comment", FilterDescription.Comment));
			filters.AddTextFilter(FilterDescription.Type, GlbCompanyCampaignSchema.G0_Type, new LearningCentreTestTypes()).MultilingualDescription = ResString.GetMultilingualString("Recruiter|LearningCentreCampaignFilterBusinessObject|Type", FilterDescription.Type);
		}

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string Type = "Type";
			public const string Comment = "Comment";
			public const string Stage = "Stage";
			public const string CampaignCoordinator = "Campaign Coordinator";
			public const string CampaignManager = "Campaign Manager";
			public const string CampaignName = "Campaign Name";
			public const string CampaignID = "Campaign ID";

			#endregion
		}

		#endregion
	}
}
