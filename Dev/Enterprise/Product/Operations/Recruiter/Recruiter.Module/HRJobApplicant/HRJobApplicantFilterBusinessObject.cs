using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class HRJobApplicantFilterBusinessObject : FilterStripBusinessObject
	{
		public HRJobApplicantFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var certificateSubGroup = new HRJobApplicantFilterProvider.CertificateSubGroup();
			AddTextFilters(filters, certificateSubGroup);
			AddRelatedItemFilters(filters);
			AddFlagsFilters(filters);
			AddDateFilters(filters, certificateSubGroup);
			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters, HRJobApplicantFilterProvider.CertificateSubGroup certificateSubGroup)
		{
			var applicationSubGroup = new HRJobApplicantFilterProvider.ApplicationSubGroup();

			if (Env.Security.HRJobApplicantViewName.IsAllowed)
			{
				filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.FullName, GetFullNameQuery).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|FullName", HRJobApplicantFilterProvider.FilterDescription.FullName);
			}

			if (Env.Security.HRJobApplicantViewUserAddress.IsAllowed)
			{
				filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.City, GetCityQuery).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|City", HRJobApplicantFilterProvider.FilterDescription.City);
				filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.State, GetStateQuery).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|State", HRJobApplicantFilterProvider.FilterDescription.State);
			}

			if (Env.Security.HRJobApplicantViewName.IsAllowed)
			{
				var column = new SchemaStringColumn(GlbPersonSchema.Instance, "PER_FullNameAI", -1, SqlDbType.NVarChar, "", false, 256, false, false, ""); // Name of a column
				var filter = filters.AddTextFilter("Full Name (Accents Excluded)", column);
				var subGroup = new HRJobApplicantFilterProvider.GlbPersonSubGroup();
				filter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|FullNameAccentsExcluded", "Full Name (Accents Excluded)");
				filter.SubGroup = subGroup;
			}

			filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.Email, HRJobApplicantSchema.HA_EmailAddress).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|Email", HRJobApplicantFilterProvider.FilterDescription.Email);

			if (Env.Security.HRJobApplicantViewHomePhone.IsAllowed)
			{
				filters.AddTextFilter(FilterDescription.HomePhone, GetHomePhoneQuery).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|HomePhone", FilterDescription.HomePhone);
			}

			if (Env.Security.HRJobApplicantViewMobilePhone.IsAllowed)
			{
				filters.AddTextFilter(FilterDescription.MobilePhone, GetMobilePhoneQuery).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|MobilePhone", FilterDescription.MobilePhone);
			}

			var applicationStatusFilter = filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.ApplicationStatus, HRJobApplicationSchema.HP_CurrentStatus, JobApplicantFilterProvider.ApplicationStatuses);
			applicationStatusFilter.MaxLength = HRJobApplicationSchema.HP_CurrentStatus.MaxLength;
			applicationStatusFilter.MultilingualDescription = ResString.GetMultilingualString("d2976066-c715-4755-967a-5dd8ad6a8f9b", "Application Status (Job Application)");
			applicationStatusFilter.SubGroup = applicationSubGroup;

			var workPermitStatusFilter = filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.WorkPermitStatus, HRJobApplicantSchema.HA_WorkPermitStatus, JobApplicantFilterProvider.WorkPermitStatuses);
			workPermitStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|WorkPermitStatus", HRJobApplicantFilterProvider.FilterDescription.WorkPermitStatus);
			workPermitStatusFilter.Category = FilterCategories.StatusAndFlags;
			var availabilityFilter = filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.Availability, HRJobApplicantSchema.HA_Availability, JobApplicantFilterProvider.Availabilities);
			availabilityFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|Availability", HRJobApplicantFilterProvider.FilterDescription.Availability);
			availabilityFilter.Category = FilterCategories.StatusAndFlags;

			var certificateCodeFilter = filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.CertificateType, GenRegCertAccredMaintListSchema.XZ_Type, JobApplicantFilterProvider.Certificates);
			certificateCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|Certificate Type", HRJobApplicantFilterProvider.FilterDescription.CertificateType); // Filter name
			certificateCodeFilter.Category = FilterCategories.TextSearch;
			certificateCodeFilter.SubGroup = certificateSubGroup;
			var certificateRefNumberFilter = filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.CertificateNumber, GenRegCertAccredMaintListSchema.XZ_RefNumber);
			certificateRefNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|Certificate Number", HRJobApplicantFilterProvider.FilterDescription.CertificateNumber); // Filter name
			certificateRefNumberFilter.Category = FilterCategories.TextSearch;
			certificateRefNumberFilter.SubGroup = certificateSubGroup;

			var resumeKeywordsFilter = filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.ResumeKeywords, GetResumeKeywordsQuery);
			resumeKeywordsFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|ResumeKeywords", HRJobApplicantFilterProvider.FilterDescription.ResumeKeywords); // Filter name
			resumeKeywordsFilter.Category = FilterCategories.TextSearch;
			resumeKeywordsFilter.ComparisonOperator_List.Clear();
			resumeKeywordsFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
		}

		ZQuery GetFullNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPersonColumnFilter(comparisonOperator, value, GlbPersonSchema.PER_FullName);
		}

		ZQuery GetCityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPersonColumnFilter(comparisonOperator, value, GlbPersonSchema.PER_City);
		}

		ZQuery GetStateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPersonColumnFilter(comparisonOperator, value, GlbPersonSchema.PER_State);
		}

		ZQuery GetHomePhoneQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPersonColumnFilter(comparisonOperator, value, GlbPersonSchema.PER_HomePhone);
		}

		ZQuery GetMobilePhoneQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPersonColumnFilter(comparisonOperator, value, GlbPersonSchema.PER_MobilePhone);
		}

		ZQuery GetCountryQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPersonColumnFilter(comparisonOperator, value, GlbPersonSchema.PER_RN_NKCountry);
		}

		static ZQuery GetPersonColumnFilter(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn column)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplicant));
			var personQuery = new ZDBOnlySubQuery(typeof(GlbPerson), HRJobApplicantSchema.HA_PER);
			personQuery.AddToFilter(column, comparisonOperator, value);

			query.AddSubQuery(personQuery, JoinCondition.And);
			return query;
		}

		#region ResumeKeywords

		ZQuery GetResumeKeywordsQuery(SQLComparisonOperator comparisonOperator, ZString keywords)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplicant));
			var applicationDocSubQuery = HRJobApplicationDocumentFilterHelper.GetResumeKeywordsSubQuery(keywords);
			var applicationSubQuery = new ZDBOnlySubQuery(typeof(HRJobApplication), HRJobApplicationSchema.HP_HA);
			applicationSubQuery.AddSubQuery(applicationDocSubQuery, JoinCondition.And);
			query.AddSubQuery(applicationSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var applicationSubGroup = new HRJobApplicantFilterProvider.ApplicationSubGroup();
			var jobCampaignSubGroup = new HRJobApplicantFilterProvider.JobCampaignSubGroup(applicationSubGroup);
			var jobRoleSubGroup = new HRJobApplicantFilterProvider.JobRoleSubGroup(jobCampaignSubGroup);

			if (Env.Security.HRJobApplicantViewNationality.IsAllowed)
			{
				filters.AddTextFilter(HRJobApplicantFilterProvider.FilterDescription.Country, GetCountryQuery, JobApplicantFilterProvider.Countries).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|Country", "Country/Region");
			}

			var jobRoleFilter = filters.AddGuidFilter(HRJobApplicantFilterProvider.FilterDescription.JobRole, ModuleIDs.HRJobRole, HRJobRoleSchema.PK, JobApplicantFilterProvider.JobRoles);
			jobRoleFilter.IsPublishedOnWeb = false;
			jobRoleFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|JobRole", HRJobApplicantFilterProvider.FilterDescription.JobRole);
			jobRoleFilter.SubGroup = jobRoleSubGroup;
			jobRoleFilter.IsBlankFilterQueryDelegate = GetJobRoleIsBlankQuery;

			var campaignDetailsFilter = filters.AddGuidFilter(HRJobApplicantFilterProvider.FilterDescription.JobOpenings, ModuleIDs.HRJobOpenings, HRRecruitmentJobCampaignSchema.PK, JobApplicantFilterProvider.Campaigns);
			campaignDetailsFilter.IsPublishedOnWeb = false;
			campaignDetailsFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|JobOpenings", HRJobApplicantFilterProvider.FilterDescription.JobOpenings);
			campaignDetailsFilter.SubGroup = jobCampaignSubGroup;
		}

		ZQuery GetJobRoleIsBlankQuery()
		{
			var campaignQuery = new ZDBOnlyQuery(typeof(HRRecruitmentJobCampaign));
			campaignQuery.AddSubQuery(new ZDBOnlySubQuery(typeof(HRJobRole), HRRecruitmentJobCampaignSchema.HV_HJ_JobRole), JoinCondition.And);
			var applicationQuery = new ZDBOnlyQuery(typeof(HRJobApplication));
			var campaignSubQuery = new ZDBOnlySubQuery(typeof(HRRecruitmentJobCampaign), HRJobApplicationSchema.HP_HV);
			campaignSubQuery.AddToFilter(campaignQuery);
			applicationQuery.AddSubQuery(campaignSubQuery, JoinCondition.And);

			var applicantQuery = new ZDBOnlyQuery(typeof(HRJobApplicant));
			var applicationSubQuery = new ZDBOnlySubQuery(typeof(HRJobApplication), HRJobApplicationSchema.HP_HA, true);
			applicationSubQuery.AddToFilter(applicationQuery, JoinCondition.And);
			applicantQuery.AddSubQuery(applicationSubQuery, JoinCondition.And);

			return applicantQuery;
		}

		#endregion

		#region Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter(HRJobApplicantFilterProvider.FilterDescription.HasApplied, new string[] { Res.GetString("56f2127d-c09f-4911-a56c-d8a9f630378c", "Has Applied") }, new GetFlagsQuery[] { JobApplicantFilterProvider.GetHasAppliedFlagQuery }).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|HasApplied", "Has Applied (Job Application)");
			filters.AddFlagsFilter(HRJobApplicantFilterProvider.FilterDescription.RegistrationMethod, new string[] { Res.GetString("f3530483-4633-4635-9d9a-1cfacf6d0e90", "Registered via Web"), Res.GetString("f2f747c9-231d-4c3a-8cf5-c3c84e57c4bf", "Registered manually") }, new GetFlagsQuery[] { JobApplicantFilterProvider.GetRegistrationMethodDelegate(true), JobApplicantFilterProvider.GetRegistrationMethodDelegate(false) }, JoinCondition.Or).
				MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobApplicantFilter|RegistrationMethod", HRJobApplicantFilterProvider.FilterDescription.RegistrationMethod);

			var learningCentreUserFilter = filters.AddFlagsFilter(HRJobApplicantFilterProvider.FilterDescription.ApplicantType, new string[] { Res.GetString("3ccc8070-5400-4f55-a7b9-eb724f511cb4", "Learning Center User") }, new GetFlagsQuery[] { JobApplicantFilterProvider.IncludeLearningCentreUserQuery });
			learningCentreUserFilter.MultilingualDescription = ResString.GetMultilingualString("780b71c0-b128-49f4-9d62-090b82faa39a", HRJobApplicantFilterProvider.FilterDescription.ApplicantType);
		}

		void AddDateFilters(ModuleFilterCollection filters, HRJobApplicantFilterProvider.CertificateSubGroup certificateSubGroup)
		{
			filters.AddDateFilter(HRJobApplicantFilterProvider.FilterDescription.SubmissionTime, JobApplicantFilterProvider.SubmissionTimeFilterQuery, true, false).MultilingualDescription = ResString.GetMultilingualString("cc84a6f1-ca36-4e24-b8ba-73a99ad7fa12", "Submission Time (Job Application)");

			var certificateIssueDateFilter = filters.AddDateFilter(HRJobApplicantFilterProvider.FilterDescription.CertificateIssueDate, GenRegCertAccredMaintListSchema.XZ_IssueDate);
			certificateIssueDateFilter.MultilingualDescription = ResString.GetMultilingualString("65BC9727-3AB8-45DA-A533-F5DE6FD9FD2D", HRJobApplicantFilterProvider.FilterDescription.CertificateIssueDate);
			certificateIssueDateFilter.SubGroup = certificateSubGroup;

			var certificateExpiryDateFilter = filters.AddDateFilter(HRJobApplicantFilterProvider.FilterDescription.CertificateExpiryDate, GenRegCertAccredMaintListSchema.XZ_ExpiryOrDueDate);
			certificateExpiryDateFilter.MultilingualDescription = ResString.GetMultilingualString("cc84a6f1-ca36-4e24-b8ba-73a99ad7fa11", HRJobApplicantFilterProvider.FilterDescription.CertificateExpiryDate);
			certificateExpiryDateFilter.SubGroup = certificateSubGroup;
		}

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string CertificateExpiryDate = "Certificate Expiry Date";
			public const string CertificateIssueDate = "Certificate Issue Date";
			public const string SubmissionTime = "Submission Time";
			public const string RegistrationMethod = "Registration Method";
			public const string HasApplied = "Has Applied";
			public const string CampaignDetails = "Campaign Details";
			public const string JobRole = "Job Role";
			public const string JobSkill = "Job Skill";
			public const string Country = "Country";
			public const string CertificateNumber = "Certificate Number";
			public const string CertificateType = "Certificate Type";
			public const string Availability = "Availability";
			public const string WorkPermitStatus = "Work Permit Status";
			public const string ApplicationStatus = "Application Status";
			public const string MobilePhone = "Mobile Phone";
			public const string HomePhone = "Home Phone";

			#endregion
		}

		#endregion

		#endregion

		#endregion

		public HRJobApplicantFilterProvider JobApplicantFilterProvider
		{
			get { return jobApplicantFilterProvider ?? (jobApplicantFilterProvider = new HRJobApplicantFilterProvider()); }
		}
		HRJobApplicantFilterProvider jobApplicantFilterProvider;
	}
}
