using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class HRJobApplicantFilterProvider : FilterStripBusinessObject
	{
		#region Filters

		#region Text

		#region Submission Time

		public ZQuery SubmissionTimeFilterQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplicant));

			var subQuery = new ZDBOnlySubQuery(typeof(HRJobApplication), HRJobApplicationSchema.HP_HA);
			if (value1.IsValid)
			{
				subQuery.AddToFilter(HRJobApplicationSchema.HP_SubmissionTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
			}
			if (value2.IsValid)
			{
				subQuery.AddToFilter(HRJobApplicationSchema.HP_SubmissionTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, value2);
			}

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#endregion

		#region Flags

		#region Has Applied

		public ZQuery GetHasAppliedFlagQuery(ZBool value)
		{
			ZQuery result;
			if (value)
			{
				result = new ZDBOnlyQuery(typeof(HRJobApplicant));
				var appSubQuery = new ZDBOnlySubQuery(typeof(HRJobApplication), HRJobApplicationSchema.HP_HA);
				((ZDBOnlyQuery)result).AddSubQuery(appSubQuery, JoinCondition.And);
			}
			else
			{
				result = new ZQuery();
			}
			return result;
		}

		#endregion

		#region Registration Method

		public GetFlagsQuery GetRegistrationMethodDelegate(bool viaWeb)
		{
			return delegate(ZBool value)
			{
				ZQuery result;
				if (value)
				{
					result = new ZDBOnlyQuery(typeof(HRJobApplicant));
					var logSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
					logSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.AddedARecordToTheSystem.Code);
					var userOperator = (viaWeb) ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
					logSubQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, userOperator, User.WebUserCode);
					((ZDBOnlyQuery)result).AddSubQuery(logSubQuery, JoinCondition.And);
				}
				else
				{
					result = new ZQuery();
				}
				return result;
			};
		}

		#endregion

		#region Applicant Type

		public ZQuery IncludeLearningCentreUserQuery(ZBool value)
		{
			return GetUserTypeQuery(value);
		}

		ZQuery GetUserTypeQuery(bool isLearningCenterUserQuery)
		{
			var result = new ZDBOnlyQuery(typeof(HRJobApplicant));
			var logSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, isLearningCenterUserQuery);
			logSubQuery.AddToFilter(HRJobApplicant.GetLearningCenterUserLogQuery());
			result.AddSubQuery(logSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#endregion

		#endregion

		#region Module Filter Sub Groups

		#region GlbPerson

		public class GlbPersonSubGroup : ModuleFilterSubGroup
		{
			public GlbPersonSubGroup()
			{ }

			public GlbPersonSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRJobApplicant));

				var subQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK, HRJobApplicantSchema.HA_PER);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		public class CertificateSubGroup : ModuleFilterSubGroup
		{
			public CertificateSubGroup()
			{ }

			public CertificateSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRJobApplicant));
				var subQuery = new ZDBOnlySubQuery(typeof(GenRegCertAccredMaintList), GenRegCertAccredMaintListSchema.XZ_ParentID);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		public class ApplicationSubGroup : ModuleFilterSubGroup
		{
			public ApplicationSubGroup()
			{ }

			public ApplicationSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRJobApplicant));
				var subQuery = new ZDBOnlySubQuery(typeof(HRJobApplication), HRJobApplicationSchema.HP_HA);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		public class JobCampaignSubGroup : ModuleFilterSubGroup
		{
			public JobCampaignSubGroup()
			{ }

			public JobCampaignSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRJobApplication));
				var subQuery = new ZDBOnlySubQuery(typeof(HRRecruitmentJobCampaign), HRJobApplicationSchema.HP_HV);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		public class JobRoleSubGroup : ModuleFilterSubGroup
		{
			public JobRoleSubGroup()
			{ }

			public JobRoleSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRRecruitmentJobCampaign));
				var subQuery = new ZDBOnlySubQuery(typeof(HRJobRole), HRRecruitmentJobCampaignSchema.HV_HJ_JobRole);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string CertificateExpiryDate = "Certificate Expiry Date";
			public const string CertificateIssueDate = "Certificate Issue Date";
			public const string SubmissionTime = "Submission Time";
			public const string ApplicantType = "Applicant Type";
			public const string RegistrationMethod = "Registration Method";
			public const string HasApplied = "Has Applied";
			public const string JobOpenings = "Job Openings";
			public const string JobRole = "Job Role";
			public const string Country = "Country";
			public const string CertificateNumber = "Certificate Number";
			public const string CertificateType = "Certificate Type";
			public const string Availability = "Availability";
			public const string WorkPermitStatus = "Work Permit Status";
			public const string ApplicationStatus = "Application Status";
			public const string Email = "Email";
			public const string State = "State";
			public const string City = "City";
			public const string FullName = "Full Name";
			public const string ResumeKeywords = "Resume Keywords";

			#endregion
		}

		#endregion

		#region Lookups

		#region Countries

		public RefCountryCollection Countries
		{
			get
			{
				if (fCountries == null)
				{
					fCountries = new RefCountryCollection(Factory);
				}

				return fCountries;
			}
		}

		RefCountryCollection fCountries;

		#endregion

		#region JobRoles

		public HRJobRoleCollection JobRoles
		{
			get
			{
				if (fJobRoles == null)
				{
					fJobRoles = new HRJobRoleCollection(Factory);
				}
				return fJobRoles;
			}
		}
		HRJobRoleCollection fJobRoles;

		#endregion

		#region Campaigns

		public HRRecruitmentJobCampaignCollection Campaigns
		{
			get
			{
				if (fCampaigns == null)
				{
					fCampaigns = new HRRecruitmentJobCampaignCollection(Factory);
				}
				return fCampaigns;
			}
		}
		HRRecruitmentJobCampaignCollection fCampaigns;

		#endregion

		#region  ApplicationStatuses

		public ReadOnlyCodeDescriptionPairList ApplicationStatuses
		{
			get { return RecruiterDataRegistry.Instance.ApplicationStatuses.Value.AsCodeDescriptionPairList(); }
		}

		#endregion

		#region Work Permit Statuses

		public ICodeDescriptionPairList WorkPermitStatuses
		{
			get { return RecruiterDataRegistry.Instance.WorkPermitStatusList.Value; }
		}

		#endregion

		#region Availabilities

		public ICodeDescriptionPairList Availabilities
		{
			get { return RecruiterDataRegistry.Instance.AvailabilityList.Value; }
		}

		#endregion

		#region Certificates

		public ICodeDescriptionPairList Certificates
		{
			get { return RecruiterDataRegistry.Instance.CertificateTypesExtra.Value.GetActiveCodeDescriptionPairList(); }
		}

		#endregion

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}
	}
}
