using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public class GlbCompanyCampaignFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddCompanyFilter(filters);

			filters.AddFiltersForTranslatableText("Campaign Name", GlbCompanyCampaignSchema.G0_CampaignName, typeof(GlbCompanyCampaign), ResString.GetMultilingualString("86735319-75f5-4d14-8a6c-b2644744ffa9", "Campaign Name"));
			filters.AddTextFilter("Campaign Email Subject", GlbCompanyCampaignSchema.G0_EmailSubject).MultilingualDescription = ResString.GetMultilingualString("89021b84-7dff-44d3-9562-f3deb924c6fb", "Campaign Email Subject");
			filters.AddTextFilter("Type", GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, Lookups.CampaignTypeList).MultilingualDescription = ResString.GetMultilingualString("4990eda0-8e5c-4920-9248-cfd2974e73a8", "Type");
			filters.AddTextFilter("Stage", GlbCompanyCampaignSchema.G0_Stage, Lookups.CampaignStages).MultilingualDescription = ResString.GetMultilingualString("75195e7e-a749-4524-a60c-3925ef9ae807", "Stage");

			filters.AddDateFilter("Actual Start Date", GlbCompanyCampaignSchema.G0_ActualStartedDate).MultilingualDescription = ResString.GetMultilingualString("565a46cc-2985-431b-ae91-08d65493eba7", "Actual Start Date");
			filters.AddDateFilter("Estimated Start Date", GlbCompanyCampaignSchema.G0_EstimatedStartedDate).MultilingualDescription = ResString.GetMultilingualString("47b7fab8-2c81-446f-a784-bd0fb4650c0b", "Estimated Start Date");
			filters.AddDateFilter("Actual Complete Date", GlbCompanyCampaignSchema.G0_ActualCompletedDate).MultilingualDescription = ResString.GetMultilingualString("bafe7a4a-4ddf-408d-aa3a-e532a1ab42ea", "Actual Complete Date");
			filters.AddDateFilter("Estimated Complete Date", GlbCompanyCampaignSchema.G0_EstimatedCompletedDate).MultilingualDescription = ResString.GetMultilingualString("c2d11b1b-2ec5-4ada-aea2-cf849393c095", "Estimated Complete Date");

			filters.AddNkFilter("Campaign Manager", GlbCompanyCampaignSchema.G0_GS_NKCampaignManager, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("e22e5fcb-d8d8-4834-b1d9-7567de89e9db", "Campaign Manager");
			filters.AddNkFilter("Campaign Coordinator", GlbCompanyCampaignSchema.G0_GS_NKCampaignCoordinator, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("b5f9de74-6276-4bea-8950-8d16d689cd4e", "Campaign Coordinator");

			filters.AddTextFilter("Campaign ID", GlbCompanyCampaignSchema.G0_CampaignID).MultilingualDescription = ResString.GetMultilingualString("1d1d973a-abf9-4940-aead-e41c725fdbd5", "Campaign ID");

			AddCategoryTypeFilters(filters);
			AddWorkflowCustomFieldsFilters(filters);
			AddCRMSecurityFilters(filters);

			var isTouch = filters.AddGuidFilter("Is Touch Campaign", ModuleIDs.GlbCompanyCampaign, GlbCompanyCampaignSchema.G0_G0_Master, new GlbCompanyCollection(Factory));
			isTouch.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			isTouch.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;

			var moduleFlagsFilter = filters.AddFlagsFilter("Is Human Resources Campaign", new[] { Res.GetString("4994ae12-80b8-4b6c-bea5-48764287a48d", "Is Human Resources Campaign") }, new GetFlagsQuery[] { GetIsHumanResourcesCampaignQuery });
			moduleFlagsFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			AddJobCategoryFilter(filters);

			return filters;
		}

		protected virtual ZQuery GetIsHumanResourcesCampaignQuery(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(GlbCompanyCampaignSchema.G0_IsSalesAndMarketing, ZBool.True);
			return query;
		}

		protected virtual void AddCategoryTypeFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Category 1", GlbCompanyCampaignSchema.G0_Category, Lookups.MediaCategoryList).MultilingualDescription = OrganisationsDataRegistry.Instance.CampaignCategory1Label.Value;
			filters.AddTextFilter("Category 2", GlbCompanyCampaignSchema.G0_Type, Lookups.MediaTypesList).MultilingualDescription = OrganisationsDataRegistry.Instance.CampaignCategory2Label.Value;
		}

		protected virtual void AddWorkflowCustomFieldsFilters(ModuleFilterCollection filters)
		{
			filters.AddWorkflowCustomFieldsFilters(Factory, CRMCampaignWorkflowDescriptor.WorkflowTypeCode, typeof(GlbCompanyCampaign));
		}

		protected virtual void AddCRMSecurityFilters(ModuleFilterCollection filters)
		{
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
		}

		void AddCompanyFilter(ModuleFilterCollection filters)
		{
			ModuleGuidFilter companyFilter = filters.AddGuidFilter("Company", ModuleIDs.GlbCompany, GlbCompanyCampaignSchema.G0_GC, new GlbCompanyCollection(Factory));
			companyFilter.MultilingualDescription = ResString.GetMultilingualString("2b6a42b6-beea-4e30-aa91-766df33f643b", "Company");
			companyFilter.DefaultProperty = GlbCompany.CurrentCompany.PK;
			companyFilter.Visibility = FilterVisibility.AlwaysVisible;
			if (!Env.Security.CampaignAllowSearchOutsideLoginCompany.IsAllowed)
			{
				companyFilter.PropertyValidation = CompanyFilterValidation;
			}
		}

		void CompanyFilterValidation(ZPropertyInfo info)
		{
			if ((ZGuid)info.Value != GlbCompany.CurrentCompany.PK)
			{
				string errorMessage = Res.GetString("3be1cdf8-4684-4ffb-b2ee-53e2f7ac11d8", @"Your current security rights only allow you to view campaigns relevant to your current login company ({0}).
If you think this is incorrect, please contact your system administrator.", GlbCompany.CurrentCompany.GC_Code);
				info.AddError(errorMessage);
			}
		}

		#region Recipient Contacts Filters

		#region JobCategory

		JobCategoryModuleTextFilter jobCategoryilter;
		void AddJobCategoryFilter(ModuleFilterCollection filters)
		{
			jobCategoryilter = new JobCategoryModuleTextFilter("Job Category", GetJobCategoryQuery(), OrgContactLookups.CreateJobCategoryList);
			jobCategoryilter.MultilingualDescription = ResString.GetMultilingualString("c2d21b1b-2ec5-4ada-aea2-cf849393c095", "Job Category");
			jobCategoryilter.Category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("1d1d973a-abf9-4941-aead-e41c725fdbd5", "Recipient Contacts"));
			filters.AddFilter(jobCategoryilter);
		}

		GetTextQueryWithOperator GetJobCategoryQuery()
		{
			return delegate
			{
				var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaign));
				query.AddFilterAndZSQLParameterCollection(jobCategoryilter.ComparisonOperator == jobCategoryilter.OperatorAllMatch ? AllMatchQuery : AnyOrNoneMatchQuery, new ZSqlParameterCollection());
				return query;
			};
		}

		string AnyOrNoneMatchQuery => string.Format(CultureInfo.InvariantCulture, @"
{0}
(
	{1}
	{2}
)", JobCategoryFilterExistsOperator, innerSqlText, JobCategoryFilterCondition);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "just string")]
		string AllMatchQuery => string.Format(CultureInfo.InvariantCulture, @"
NOT EXISTS
(
	{0}
	EXCEPT
	{0}
	{1}
)
AND EXISTS
(
	{0}
)", innerSqlText, JobCategoryFilterCondition);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "just string")]
		string JobCategoryFilterCondition => string.Format(CultureInfo.InvariantCulture, "AND VCC_JobCategory = '{0}'", jobCategoryilter.Property);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "just string")]
		string JobCategoryFilterExistsOperator => jobCategoryilter.ComparisonOperator == jobCategoryilter.OperatorAnyMatch ? "EXISTS" : "NOT EXISTS";

		readonly string innerSqlText = @"
SELECT G8_G0, VCC_JobCategory FROM 
dbo.GlbCompanyCampaignItem
LEFT JOIN dbo.ViewCampaignContact ON GlbCompanyCampaignItem.G8_RecipientID = ViewCampaignContact.VCC_PK
WHERE GlbCompanyCampaignItem.G8_G0 = G0_PK";

		#endregion

		#endregion

		#endregion

		#region Lookups

		GlbCompanyCampaignLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		protected virtual GlbCompanyCampaignLookups GetNewLookups()
		{
			return new GlbCompanyCampaignLookups(Factory);
		}

		GlbCompanyCampaignLookups lookups;

		#endregion

		readonly GlbCompanyCampaignCRMSecurityProvider SecurityProvider = new GlbCompanyCampaignCRMSecurityProvider();
	}

	public class JobCategoryModuleTextFilter : ModuleTextFilter
	{
		public JobCategoryModuleTextFilter(ZString description, Delegate queryDelegate, GetList listDelegate) : base(description, queryDelegate, listDelegate)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ComparisonConstants.AllMatch,
			ComparisonConstants.AnyMatch,
			ComparisonConstants.NoneMatch
		};

		internal string OperatorAllMatch => ComparisonConstants.AllMatch;
		internal string OperatorAnyMatch => ComparisonConstants.AnyMatch;
		internal string OperatorNoneMatch => ComparisonConstants.NoneMatch;

		public override bool HasComparisonOperator => true;
	}
}
