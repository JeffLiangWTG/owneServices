using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruitment.Module.CandidateManagement
{
	public class CandidateFilterStripBusinessObject : FilterStripBusinessObject
	{
		public CandidateFilterStripBusinessObject()
			: this(new BusinessObjectFactory { NameForDebugging = "CandidateFilterStripBusinessObject_Factory" })
		{
		}

		public CandidateFilterStripBusinessObject(BusinessObjectFactory factory)
			: base(factory) => LayoutContext = ModuleIDs.RecruitmentCandidateManagement.Name;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			AddTextFilters(result);
			AddRelatedItemFilters(result);

			return result;
		}

		public void InvalidateCacheQuery()
			=> ResumeKeywordsFilter.InvalidateCachedQuery();

		public bool NeedsReevaluation()
			=> ResumeKeywordsFilter.IsQueryStale;

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Candidate Name", GetNameFilter)
				.MultilingualDescription = ResString.GetMultilingualString("3935bba7-2f55-4aa5-9ede-ffbbe8a31949", "Candidate Name");

			filters.AddTextFilter("Email", GetEmailFilter)
				.MultilingualDescription = ResString.GetMultilingualString("0bf0a8c0-594e-86ba-4cd4-6bbd85cf5ccb", "Email");

			filters.AddTextFilter("City", GetCityFilter)
				.MultilingualDescription = ResString.GetMultilingualString("7e64f672-0521-4082-8617-220d4bc12dfe", "City");

			filters.AddDateFilter("Last Application", SubmissionTimeFilter, convertFromLocalToUTC: true, isNullable: false)
				.MultilingualDescription = ResString.GetMultilingualString("27932c28-c2d3-4fce-9077-b46b410c3386", "Last Application");

			var applicationRatingFilter = filters.AddTextFilter("Rating", GetRatingFilter, HRJobApplicationFilterBusinessObject.GetRatingList);
			applicationRatingFilter.MultilingualDescription = ResString.GetMultilingualString("4d2c2002-6755-42a2-bd7a-a517386954d1", "Rating");
			applicationRatingFilter.ComparisonOperator_List.Clear();
			applicationRatingFilter.ComparisonOperator_List.AddRange(HRJobApplicationFilterBusinessObject.GetJobApplicationCommonComparisonOperatorList());
			applicationRatingFilter.ComparisonOperator_List.DefaultCode = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;

			var applicationStatusFilter = filters.AddTextFilter("Application Status", GetApplicationStatusFilter, GetApplicationStatusList);
			applicationStatusFilter.MultilingualDescription = ResString.GetMultilingualString("48d8bcf0-51c1-4b77-9810-5612e61b960b", "Status");
			applicationStatusFilter.ComparisonOperator_List.Clear();
			applicationStatusFilter.ComparisonOperator_List.AddRange(HRJobApplicationFilterBusinessObject.GetJobApplicationCommonComparisonOperatorList());
			applicationStatusFilter.ComparisonOperator_List.DefaultCode = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;

			ResumeKeywordsFilter = new ModuleTextLuceneFilter("Resume Keywords", GetResumeKeywordsQuery);
			ResumeKeywordsFilter.MultilingualDescription = ResString.GetMultilingualString("11ca22ff-8f89-4d91-ba34-098a8b30923e", "Resume Keywords");
			ResumeKeywordsFilter.Category = FilterCategories.TextSearch;
			ResumeKeywordsFilter.ComparisonOperator_List.Clear();

			ResumeKeywordsFilter.ComparisonOperator_List.AddPair(
				GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.Value
				? ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith
				: ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
			filters.AddFilter(ResumeKeywordsFilter);
		}

		internal ModuleTextLuceneFilter ResumeKeywordsFilter;

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var jobRoleFilter = filters.AddGuidFilter("Selected Role", ModuleIDs.HRJobRole, GetJobRoleFilter, new HRJobRoleCollection(Factory));
			jobRoleFilter.IsPublishedOnWeb = false;
			jobRoleFilter.MultilingualDescription = ResString.GetMultilingualString("5fdb7ad6-69e1-410b-a695-3b5200d57420", "Selected Role");

			var jobCountryFilter = filters.AddNkFilter("Country", GetCountryFilter, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			jobCountryFilter.MultilingualDescription = ResString.GetMultilingualString("d2a2e326-946b-4387-a9d7-72d0a8067cfa", "Country/Region");

			var responsibleStaffFilterAppication = filters.AddNkFilter("Responsible Recruiter (Job Application)", GetResponsibleStaffFilterApplication, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			responsibleStaffFilterAppication.MultilingualDescription = ResString.GetMultilingualString("98897abc-b21b-4366-887f-ec6f7bd980d5", "Responsible Recruiter (Job Application)");
			responsibleStaffFilterAppication.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.CurrentUser;

			var responsibleStaffFilterOpenings = filters.AddNkFilter("Responsible Recruiter (Job Openings)", GetResponsibleStaffFilterOpenings, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			responsibleStaffFilterOpenings.MultilingualDescription = ResString.GetMultilingualString("cc7d2db9-52c6-4598-902b-ab174901e4e8", "Responsible Recruiter (Job Openings)");
			responsibleStaffFilterOpenings.ComparisonOperator_List.Clear();
			responsibleStaffFilterOpenings.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			responsibleStaffFilterOpenings.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.CurrentUser);
			responsibleStaffFilterOpenings.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.CurrentUser;

			var applicationFilter = new ModuleGuidFilter("Application", ModuleIDs.HRJobApplication, HRJobApplicationSchema.PK, new HRJobApplicationCollection(Factory));
			applicationFilter.MultilingualDescription = ResString.GetMultilingualString("2af36bcb-8523-43c6-989e-f1702d95395c", "Application");
			filters.AddFilter(applicationFilter);

			var applicantFilter = new ModuleGuidFilter("Applicant", ModuleIDs.HRJobApplicant, HRJobApplicationSchema.HP_HA, new HRJobApplicantCollection(Factory));
			applicantFilter.MultilingualDescription = ResString.GetMultilingualString("1e8d2d43-58ff-49ec-8c09-6e930083c929", "Applicant");
			filters.AddFilter(applicantFilter);

			var jobOpeningsFilter = new ModuleGuidFilter("Job Openings", ModuleIDs.HRJobOpenings, HRJobApplicationSchema.HP_HV, new HRRecruitmentJobCampaignCollection(Factory));
			jobOpeningsFilter.MultilingualDescription = ResString.GetMultilingualString("581ead22-11b3-435d-89d8-fe3c196db6d5", "Job Openings");
			filters.AddFilter(jobOpeningsFilter);
		}

		#region Text

		CodeDescriptionPairList GetApplicationStatusList
		{
			get => RecruiterDataRegistry.Instance.ApplicationStatuses.Value.AsCodeDescriptionPairList();
		}

		ZQuery GetApplicationStatusFilter(SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery(HRJobApplicationSchema.HP_CurrentStatus, comparisonOperator, value);

		ZQuery GetRatingFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			if (value == "-1" && comparisonOperator == SQLComparisonOperator.Equal)
			{
				query.AddToFilter(HRJobApplicationSchema.HP_ApplicationOverallRating, SQLComparisonOperator.NotEqual, new[] { "1", "2", "3" });
			}
			else if (value == "-1" && comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				query.AddToFilter(HRJobApplicationSchema.HP_ApplicationOverallRating, SQLComparisonOperator.Equal, new[] { "1", "2", "3" });
			}
			else
			{
				query.AddToFilter(HRJobApplicationSchema.HP_ApplicationOverallRating, comparisonOperator, value);
			}
			return query;
		}

		ZQuery GetNameFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = GetPersonColumnFilterSubQuery(comparisonOperator, value, GlbPersonSchema.PER_FullName);
			return AsApplicationFilter(query);
		}

		ZQuery GetCityFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = GetPersonColumnFilterSubQuery(comparisonOperator, value, GlbPersonSchema.PER_City);

			return AsApplicationFilter(query);
		}

		ZQuery GetEmailFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlySubQuery(typeof(HRJobApplicant), HRJobApplicationSchema.HP_HA);
			query.AddToFilter(HRJobApplicantSchema.HA_EmailAddress, comparisonOperator, value);

			return AsApplicationFilter(query);
		}

		static ZDBOnlySubQuery GetPersonColumnFilterSubQuery(SQLComparisonOperator comparisonOperator, ZString value, CargoWise.Schema.SchemaStringColumn column)
		{
			var query = new ZDBOnlySubQuery(typeof(HRJobApplicant), HRJobApplicationSchema.HP_HA);
			var personQuery = new ZDBOnlySubQuery(typeof(GlbPerson), HRJobApplicantSchema.HA_PER);
			personQuery.AddToFilter(column, comparisonOperator, value);

			query.AddSubQuery(personQuery, JoinCondition.And);
			return query;
		}

		ZQuery SubmissionTimeFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZQuery();
			if (value1.IsValid)
			{
				query.AddToFilter(HRJobApplicationSchema.HP_SubmissionTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
			}

			if (value2.IsValid)
			{
				query.AddToFilter(HRJobApplicationSchema.HP_SubmissionTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, value2);
			}

			return query;
		}

		internal ZQuery GetResumeKeywordsQuery(SQLComparisonOperator comparisonOperator, ZString keywords)
		{
			var applicationDocSubQuery = HRJobApplicationDocumentFilterHelper.GetResumeKeywordsSubQuery(keywords);
			var query = new ZDBOnlyQuery(typeof(HRJobApplication));
			query.AddSubQuery(applicationDocSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Related Items

		ZQuery GetJobRoleFilter(ZGuid role)
		{
			var campainSubQuery = new ZDBOnlySubQuery(typeof(HRRecruitmentJobCampaign), HRJobApplicationSchema.HP_HV);
			campainSubQuery.AddToFilter(HRRecruitmentJobCampaignSchema.HV_HJ_JobRole, role);

			var query = new ZDBOnlyQuery(typeof(HRJobApplication));
			query.AddSubQuery(campainSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetCountryFilter(ZString value)
		{
			var filter = GetPersonColumnFilterSubQuery(SQLComparisonOperator.Equal, value, GlbPersonSchema.PER_RN_NKCountry);

			return AsApplicationFilter(filter);
		}

		ZQuery GetResponsibleStaffFilterApplication(SQLComparisonOperator comparisonOperator, ZString staffCode)
			=> new ZQuery(HRJobApplicationSchema.HP_GS_NKAssignedTo, comparisonOperator, staffCode);

		ZQuery GetResponsibleStaffFilterOpenings(SQLComparisonOperator comparisonOperator, ZString staffCode)
		{
			var campainSubQuery = new ZDBOnlySubQuery(typeof(HRRecruitmentJobCampaign), HRJobApplicationSchema.HP_HV);
			campainSubQuery.AddToFilter(HRRecruitmentJobCampaignSchema.HV_GS_NKControlledBy, comparisonOperator, staffCode);

			var query = new ZDBOnlyQuery(typeof(HRJobApplication));
			query.AddSubQuery(campainSubQuery, JoinCondition.And);

			return query;
		}

		static ZDBOnlyQuery AsApplicationFilter(ZDBOnlySubQuery subQuery)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplication));
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

	}
}
