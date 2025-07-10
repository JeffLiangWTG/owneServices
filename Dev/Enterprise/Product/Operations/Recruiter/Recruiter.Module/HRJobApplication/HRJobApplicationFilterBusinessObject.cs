using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class HRJobApplicationFilterBusinessObject : FilterStripBusinessObject
	{
		public HRJobApplicationFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			var submissionDateFilter = filters.AddDateFilter(HRJobApplicationFilterProvider.FilterDescription.SubmissionDate, HRJobApplicationSchema.HP_SubmissionTimeUtc, true);
			submissionDateFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|SubmissionDate", "Submission Date");

			AddRelatedItemFilters(filters);
			AddTextFilters(filters);
			AddFlagFilters(filters);
			AddReferringPartyFilters(filters);
			return filters;
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var documentsFilter = filters.AddFlagsFilter(HRJobApplicationFilterProvider.FilterDescription.HasApplicationDocuments,
				new[] { Res.GetString("Recruiter|HRJobApplication|HasApplicationDocuments", "Has Application Documents (Job Application Document)") },
				new GetFlagsQuery[] { HasDocumentsQuery });

			documentsFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|HasApplicationDocuments", "Has Application Documents (Job Application Document)");
		}

		ZQuery HasDocumentsQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplication));
			var subQuery = new ZDBOnlySubQuery(typeof(HRJobApplicationDocument), HRJobApplicationDocumentSchema.HPD_HP, !value);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var applicantNameFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.ApplicantFullName, GetApplicantNameFilter);
			applicantNameFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ApplicantFullName", "Applicant Full Name (Job Applicant)");
			applicantNameFilter.SubGroup = ApplicantSubGroup;

			var applicantEmailFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.ApplicantEmail, HRJobApplicantSchema.HA_EmailAddress);
			applicantEmailFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ApplicantEmail", "Applicant Email (Job Applicant)");
			applicantEmailFilter.SubGroup = ApplicantSubGroup;

			var applicantMobilePhoneFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.ApplicantMobilePhone, GetApplicantMobilePhoneFilter);
			applicantMobilePhoneFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ApplicantMobilePhone", "Applicant Mobile Phone (Job Applicant)");
			applicantMobilePhoneFilter.SubGroup = ApplicantSubGroup;

			var adTitleFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.AdTitle, HRRecruitmentJobCampaignSchema.HV_AdTitle);
			adTitleFilter.IsBlankFilterQueryDelegate = GetAdTitleIsBlankQuery;
			adTitleFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|AdTitle", "Ad Title (Job Openings)");
			adTitleFilter.SubGroup = JobOpeningSubGroup;

			var jobTitleFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.JobTitle, HRJobRoleSchema.HJ_JobTitle);
			jobTitleFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|JobTitle", "Job Title (Job Role)");
			jobTitleFilter.SubGroup = JobRoleSubGroup;

			var contactNameFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.ContactName, OrgContactSchema.OC_ContactName);
			contactNameFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ContactName", "Contact Name (Contact)");
			contactNameFilter.SubGroup = ContactSubGroup;

			var statusFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.CurrentStatus, HRJobApplicationSchema.HP_CurrentStatus, RecruiterDataRegistry.Instance.ApplicationStatuses.Value.AsCodeDescriptionPairList);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|CurrentStatus", "Status");

			var overallRatingFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.OverallRating, GetRatingFilter, GetRatingList);
			overallRatingFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|OverallRating", "Overall Rating");
			overallRatingFilter.ComparisonOperator_List.Clear();
			overallRatingFilter.ComparisonOperator_List.AddRange(GetJobApplicationCommonComparisonOperatorList());
			overallRatingFilter.ComparisonOperator_List.DefaultCode = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;

			var resumeKeywordsFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.ResumeKeywords, GetResumeKeywordsQuery);
			resumeKeywordsFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ResumeKeywords", HRJobApplicationFilterProvider.FilterDescription.ResumeKeywords); // Filter name
			resumeKeywordsFilter.Category = FilterCategories.TextSearch;
			resumeKeywordsFilter.ComparisonOperator_List.Clear();
			resumeKeywordsFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
		}

		ZQuery GetAdTitleIsBlankQuery()
		{
			var nullHP_HVResult = new ZQuery(HRJobApplicationSchema.HP_HV, null);
			var nullHV_HJResult = new ZDBOnlyQuery(typeof(HRJobApplication));
			var nullHP_AdTitleQuery = new ZQuery(HRRecruitmentJobCampaignSchema.HV_AdTitle, string.Empty);
			var subQuery = new ZDBOnlySubQuery(typeof(HRRecruitmentJobCampaign), HRJobApplicationSchema.HP_HV);
			subQuery.AddToFilter(nullHP_AdTitleQuery);
			nullHV_HJResult.AddSubQuery(subQuery, JoinCondition.And);
			return nullHP_HVResult.AddToFilter(nullHV_HJResult, JoinCondition.Or);
		}

		ZQuery GetApplicantNameFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplicant));
			var personQuery = new ZDBOnlySubQuery(typeof(GlbPerson), HRJobApplicantSchema.HA_PER);
			personQuery.AddToFilter(GlbPersonSchema.PER_FullName, comparisonOperator, value);

			query.AddSubQuery(personQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetApplicantMobilePhoneFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplicant));
			var personQuery = new ZDBOnlySubQuery(typeof(GlbPerson), HRJobApplicantSchema.HA_PER);
			personQuery.AddToFilter(GlbPersonSchema.PER_MobilePhone, comparisonOperator, value);

			query.AddSubQuery(personQuery, JoinCondition.And);
			return query;
		}

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

		public static CodeDescriptionPairList GetRatingList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("-1", ResString.GetMultilingualString("034168ea-6752-40cf-bd2f-ba3beec1e127", "Unrated"));
			list.AddPair("1", ResString.GetMultilingualString("fbab3aca-032a-45a6-9159-e1f945518026", "Suitable"));
			list.AddPair("2", ResString.GetMultilingualString("bb2ff11a-3363-4577-914f-b5c475a22f55", "Potential"));
			list.AddPair("3", ResString.GetMultilingualString("3ab20396-ada1-4e26-b479-9fab10b6d720", "Unsuitable"));

			return list;
		}

		public static CodeDescriptionPairList GetJobApplicationCommonComparisonOperatorList()
		{
			var list = new CodeDescriptionPairList();
			list.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			list.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			list.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			list.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));

			return list;
		}

		#region ResumeKeywords

		ZQuery GetResumeKeywordsQuery(SQLComparisonOperator comparisonOperator, ZString keywords)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplication));
			var applicationDocSubQuery = HRJobApplicationDocumentFilterHelper.GetResumeKeywordsSubQuery(keywords);
			query.AddSubQuery(applicationDocSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var applicantFilter = filters.AddGuidFilter(HRJobApplicationFilterProvider.FilterDescription.Applicant, ModuleIDs.HRJobApplicant, HRJobApplicantSchema.PK, JobApplicationFilterProvider.Applicants);
			applicantFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|Applicant", "Applicant");
			applicantFilter.SubGroup = ApplicantSubGroup;

			var jobRoleFilter = filters.AddGuidFilter(HRJobApplicationFilterProvider.FilterDescription.JobRole, ModuleIDs.HRJobRole, HRJobRoleSchema.PK, JobApplicationFilterProvider.JobRoles);
			jobRoleFilter.IsBlankFilterQueryDelegate = GetJobRoleIsBlankQuery;
			jobRoleFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|JobRole", "Job Role");
			jobRoleFilter.SubGroup = JobRoleSubGroup;

			var locationFilter = filters.AddGuidFilter(HRJobApplicationFilterProvider.FilterDescription.OfficeLocation, ModuleIDs.Organisation, OrgHeaderSchema.PK, JobApplicationFilterProvider.Locations);
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|OfficeLocation", "Office Location");
			locationFilter.SubGroup = LocationSubGroup;

			var contactFilter = filters.AddGuidFilter(HRJobApplicationFilterProvider.FilterDescription.Contact, ModuleIDs.OrgContacts, OrgContactSchema.PK, JobApplicationFilterProvider.Contacts);
			contactFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|Contact", "Contact");
			contactFilter.SubGroup = ContactSubGroup;

			var responsibleStaffFilterApplication = filters.AddNkFilter("Responsible Recruiter (Job Application)", GetResponsibleStaffFilterApplication, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			responsibleStaffFilterApplication.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ResponsibleRecruiter(JobApplication)", "Responsible Recruiter (Job Application)");

			var responsibleStaffFilterOpenings = filters.AddNkFilter(HRJobApplicationFilterProvider.FilterDescription.RecruitmentCoordinator, HRRecruitmentJobCampaignSchema.HV_GS_NKControlledBy, ModuleIDs.GlbStaff, JobApplicationFilterProvider.Coordinators);
			responsibleStaffFilterOpenings.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ResponsibleRecruiter(JobOpenings)", "Responsible Recruiter (Job Openings)");
			responsibleStaffFilterOpenings.SubGroup = CoordinatorSubGroup;
		}

		ZQuery GetJobRoleIsBlankQuery()
		{
			var nullHP_HVResult = new ZQuery(HRJobApplicationSchema.HP_HV, null);
			var nullHV_HJResult = new ZDBOnlyQuery(typeof(HRJobApplication));
			var nullHP_HJQuery = new ZQuery(HRRecruitmentJobCampaignSchema.HV_HJ_JobRole, null);
			var subQuery = new ZDBOnlySubQuery(typeof(HRRecruitmentJobCampaign), HRJobApplicationSchema.HP_HV);
			subQuery.AddToFilter(nullHP_HJQuery);
			nullHV_HJResult.AddSubQuery(subQuery, JoinCondition.And);
			return nullHP_HVResult.AddToFilter(nullHV_HJResult, JoinCondition.Or);
		}

		void AddReferringPartyFilters(ModuleFilterCollection filters)
		{
			var sourceFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.Source, HRJobApplicationSchema.HP_SourceType, RecruiterDataRegistry.Instance.ReferringSourcesTypes.Value);
			sourceFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|Source", "Source");
			sourceFilter.Category = ReferringPartyFilterCategory;

			var sourceDetailsFilter = filters.AddTextFilter(HRJobApplicationFilterProvider.FilterDescription.SourceDetails, HRJobApplicationSchema.HP_SourceDetails);
			sourceDetailsFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|SourceDetails", "Source Details");
			sourceDetailsFilter.Category = ReferringPartyFilterCategory;

			var referringOrganisationFilter = filters.AddGuidFilter(HRJobApplicationFilterProvider.FilterDescription.ReferringOrganisation, ModuleIDs.Organisation, HRJobApplicationSchema.HP_OH_ReferringOrganisation, JobApplicationFilterProvider.ReferringOrganisations);
			referringOrganisationFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ReferringOrganisation", "Referring Organization");
			referringOrganisationFilter.Category = ReferringPartyFilterCategory;

			var referringPersonFilter = filters.AddGuidFilter(HRJobApplicationFilterProvider.FilterDescription.ReferringPerson, ModuleIDs.GlbPerson, HRJobApplicationSchema.HP_PER_ReferringPerson, JobApplicationFilterProvider.ReferringPersons);
			referringPersonFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ReferringPerson", "Referring Person");
			referringPersonFilter.Category = ReferringPartyFilterCategory;

			var referringStaffFilter = filters.AddNkFilter(HRJobApplicationFilterProvider.FilterDescription.ReferringStaff, GetReferringStaffQuery, ModuleIDs.GlbStaff, JobApplicationFilterProvider.ReferringStaffs);
			referringStaffFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplication|ReferringStaff", "Referring Staff");
			referringStaffFilter.Category = ReferringPartyFilterCategory;
			referringStaffFilter.MaxLength = GlbStaffSchema.GS_Code.MaxLength;
		}

		ZQuery GetReferringStaffQuery(SQLComparisonOperator comparisonOperator, ZString staffCode)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplication));
			query.AddToFilter(HRJobApplicationSchema.HP_SourceType, ReferringSourcesTypes.Codes.StaffReferral);
			var subQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_PER, HRJobApplicationSchema.HP_PER_ReferringPerson);
			subQuery.AddToFilter(GlbStaffSchema.GS_Code, comparisonOperator, staffCode);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetResponsibleStaffFilterApplication(SQLComparisonOperator comparisonOperator, ZString staffCode)
			=> new ZQuery(HRJobApplicationSchema.HP_GS_NKAssignedTo, comparisonOperator, staffCode);

		HRJobApplicationFilterProvider JobApplicationFilterProvider
		{
			get { return jobApplicationFilterProvider ?? (jobApplicationFilterProvider = new HRJobApplicationFilterProvider()); }
		}
		HRJobApplicationFilterProvider jobApplicationFilterProvider;

		#region Sub Groups
		HRJobApplicationFilterProvider.ApplicantSubGroup ApplicantSubGroup
		{
			get { return applicantSubGroup ?? (applicantSubGroup = new HRJobApplicationFilterProvider.ApplicantSubGroup()); }
		}
		HRJobApplicationFilterProvider.ApplicantSubGroup applicantSubGroup;

		HRJobApplicationFilterProvider.JobOpeningSubGroup JobOpeningSubGroup
		{
			get { return jobOpeningSubGroup ?? (jobOpeningSubGroup = new HRJobApplicationFilterProvider.JobOpeningSubGroup()); }
		}
		HRJobApplicationFilterProvider.JobOpeningSubGroup jobOpeningSubGroup;

		HRJobApplicationFilterProvider.JobRoleSubGroup JobRoleSubGroup
		{
			get { return jobRoleSubGroup ?? (jobRoleSubGroup = new HRJobApplicationFilterProvider.JobRoleSubGroup(JobOpeningSubGroup)); }
		}
		HRJobApplicationFilterProvider.JobRoleSubGroup jobRoleSubGroup;

		HRJobApplicationFilterProvider.LocationSubGroup LocationSubGroup
		{
			get { return locationSubGroup ?? (locationSubGroup = new HRJobApplicationFilterProvider.LocationSubGroup(JobOpeningSubGroup)); }
		}
		HRJobApplicationFilterProvider.LocationSubGroup locationSubGroup;

		HRJobApplicationFilterProvider.ContactSubGroup ContactSubGroup
		{
			get { return contactSubGroup ?? (contactSubGroup = new HRJobApplicationFilterProvider.ContactSubGroup(JobOpeningSubGroup)); }
		}
		HRJobApplicationFilterProvider.ContactSubGroup contactSubGroup;

		HRJobApplicationFilterProvider.CoordinatorSubGroup CoordinatorSubGroup
		{
			get { return coordinatorSubGroup ?? (coordinatorSubGroup = new HRJobApplicationFilterProvider.CoordinatorSubGroup(JobOpeningSubGroup)); }
		}
		HRJobApplicationFilterProvider.CoordinatorSubGroup coordinatorSubGroup;

		#endregion

		readonly FilterCategory ReferringPartyFilterCategory = new FilterCategory(ResString.GetMultilingualString("Recruiter|HRJobApplicationFilter|ReferringParty", "Referring Party"));

		#endregion
	}
}
