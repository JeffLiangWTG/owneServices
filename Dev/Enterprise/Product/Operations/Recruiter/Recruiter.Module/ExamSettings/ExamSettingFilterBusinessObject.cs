using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class ExamSettingsFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilter(filters);
			AddNumberFilters(filters);
			AddCampaignFilter(filters);

			return filters;
		}

		FilterCategory LearningCenterCategory
		{
			get { return learningCenterCategory ?? (learningCenterCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("EDAD04C4-F863-4DF7-B674-4043359DD45C", "Learning Center"))); }
		}
		FilterCategory learningCenterCategory;

		void AddCampaignFilter(ModuleFilterCollection filters)
		{
			var moduleFilter = new ExamsModuleFilter("Learning Center", new LearningCentreCampaignCollection(Factory));
			moduleFilter.MultilingualDescription = ResString.GetMultilingualString("C40B73F2-1708-4F1C-9AC6-8E6CA8F9B1F3", "Learning Center");
			moduleFilter.Category = LearningCenterCategory;
			filters.AddFilter(moduleFilter);
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter("Number of questions asked", ExamSettingSchema.EXS_MaximumAskedQuestionsPerExam)
				.MultilingualDescription = ResString.GetMultilingualString("Recruiter|ExamSettingsFilter|QuestionsAsked", "Number of questions asked");

			filters.AddNumberRangeFilter("Exam expiry (mins)", ExamSettingSchema.EXS_ExamExpiryTimeInMinutes)
				.MultilingualDescription = ResString.GetMultilingualString("Recruiter|ExamSettingsFilter|ExamExpiry", "Exam expiry (mins.)");

			filters.AddNumberRangeFilter("Results expiry (hours)", ExamSettingSchema.EXS_TestResultsExpireAfterHours)
				.MultilingualDescription = ResString.GetMultilingualString("Recruiter|ExamSettingsFilter|ResultExpiry", "Results expiry (hours)");
		}

		void AddFlagFilter(ModuleFilterCollection filters)
		{
			var iscredited = filters.AddFlagsFilter("Is Default", new[] { Res.GetString("Recruiter|ExamSetting|IsDefault", "Is Default") },
				new[] { ExamSettingSchema.EXS_IsDefault });
			iscredited.Category = FilterCategories.StatusAndFlags;
			iscredited.MultilingualDescription = ResString.GetMultilingualString("Recruiter|ExamSettingsFilter|IsDefault", "Is Default");
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Version", ExamSettingSchema.EXS_ExamVersion, RecruiterDataRegistry.Instance.JobSkillTestVersionList.Value)
				.MultilingualDescription = ResString.GetMultilingualString("Recruiter|ExamSettingsFilter|Version", "Version");

			filters.AddTextFilter("Description", ExamSettingSchema.EXS_Description)
				.MultilingualDescription = ResString.GetMultilingualString("Recruiter|ExamSettingsFilter|Description", "Description");

			filters.AddTextFilter("Code", ExamSettingSchema.EXS_Code)
				.MultilingualDescription = ResString.GetMultilingualString("Recruiter|ExamSettingsFilter|Code", "Code");

			filters.AddTextFilter("Campaign Name", CampaignNameQuery)
				.MultilingualDescription = ResString.GetMultilingualString("Recruiter|ExamSettingsFilter|CampaignName", "Campaign Name");
		}

		ZQuery CampaignNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(ExamSetting));
			var subQuery = new ZDBOnlySubQuery(typeof(LearningCentreCampaign), ExamSettingSchema.EXS_G0);
			subQuery.AddToFilter(GlbCompanyCampaignSchema.G0_CampaignName, comparisonOperator, value);

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}
	}
}
