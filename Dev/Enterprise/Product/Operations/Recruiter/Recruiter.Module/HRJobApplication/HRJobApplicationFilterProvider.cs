using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class HRJobApplicationFilterProvider : FilterStripBusinessObject
	{
		#region Module Filter Sub Groups

		public class ApplicantSubGroup : ModuleFilterSubGroup
		{
			public ApplicantSubGroup()
			{ }

			public ApplicantSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRJobApplication));
				var subQuery = new ZDBOnlySubQuery(typeof(HRJobApplicant), HRJobApplicantSchema.PK);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(HRJobApplicationSchema.HP_HA, subQuery, JoinCondition.And);

				return query;
			}
		}

		public class JobOpeningSubGroup : ModuleFilterSubGroup
		{
			public JobOpeningSubGroup()
			{ }

			public JobOpeningSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRJobApplication));
				var subQuery = new ZDBOnlySubQuery(typeof(HRRecruitmentJobCampaign), HRRecruitmentJobCampaignSchema.PK);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(HRJobApplicationSchema.HP_HV, subQuery, JoinCondition.And);

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

		public class LocationSubGroup : ModuleFilterSubGroup
		{
			public LocationSubGroup()
			{ }

			public LocationSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRRecruitmentJobCampaign));
				var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), HRRecruitmentJobCampaignSchema.HV_OH_ClientAccount);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		public class ContactSubGroup : ModuleFilterSubGroup
		{
			public ContactSubGroup()
			{ }

			public ContactSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRRecruitmentJobCampaign));
				var subQuery = new ZDBOnlySubQuery(typeof(OrgContact), HRRecruitmentJobCampaignSchema.HV_OC_ClientContact);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		public class CoordinatorSubGroup : ModuleFilterSubGroup
		{
			public CoordinatorSubGroup()
			{ }

			public CoordinatorSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRRecruitmentJobCampaign));
				var subQuery = new ZDBOnlySubQuery(typeof(GlbStaff), HRRecruitmentJobCampaignSchema.HV_GS_NKControlledBy, GlbStaffSchema.GS_Code);
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

			public const string SubmissionDate = "Submission Date";
			public const string ApplicantFullName = "Applicant Full Name";
			public const string AdTitle = "Ad Title";
			public const string JobTitle = "Job Title";
			public const string ContactName = "Contact Name";
			public const string Applicant = "Applicant";
			public const string ApplicantEmail = "Email";
			public const string ApplicantMobilePhone = "Mobile Phone";
			public const string JobRole = "Job Role";
			public const string OfficeLocation = "Office Location";
			public const string Contact = "Contact";
			public const string RecruitmentCoordinator = "Recruitment Coordinator";
			public const string CurrentStatus = "Status";
			public const string OverallRating = "Overall Rating";
			public const string HasApplicationDocuments = "Has Application Documents";
			public const string Source = "Source";
			public const string SourceDetails = "Source Details";
			public const string ReferringOrganisation = "Referring Organisation";
			public const string ReferringPerson = "Referring Person";
			public const string ReferringStaff = "Referring Staff";
			public const string ResumeKeywords = "Resume Keywords";

			#endregion
		}

		#endregion

		#region Lookups

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

		#region Applicants

		public HRJobApplicantCollection Applicants
		{
			get
			{
				if (fApplicants == null)
				{
					fApplicants = new HRJobApplicantCollection(Factory);
				}
				return fApplicants;
			}
		}
		HRJobApplicantCollection fApplicants;

		#endregion

		#region Locations

		public OrgHeaderCollection Locations
		{
			get
			{
				if (fLocations == null)
				{
					fLocations = new OrgHeaderCollection(Factory);
				}
				return fLocations;
			}
		}
		OrgHeaderCollection fLocations;

		#endregion

		#region Contacts

		public OrgContactCollection Contacts
		{
			get
			{
				if (fContacts == null)
				{
					fContacts = new OrgContactCollection(Factory);
				}
				return fContacts;
			}
		}
		OrgContactCollection fContacts;

		#endregion

		#region Coordinators

		public GlbStaffCollection Coordinators
		{
			get
			{
				if (fCoordinators == null)
				{
					fCoordinators = new GlbStaffCollection(Factory);
				}
				return fCoordinators;
			}
		}
		GlbStaffCollection fCoordinators;

		#endregion

		#region Referring Party

		public OrganisationsFindBoxCollection ReferringOrganisations => Factory.GetCachedValue("HRJobApplicationFilterProvider.ReferringOrganisations", () => new OrganisationsFindBoxCollection(Factory));

		public GlbPersonCollection ReferringPersons => Factory.GetCachedValue("HRJobApplicationFilterProvider.ReferringPersons", () => new GlbPersonCollection(Factory));

		public GlbStaffCollection ReferringStaffs => Factory.GetCachedValue("HRJobApplicationFilterProvider.ReferringStaffs", () => new GlbStaffCollection(Factory));

		#endregion

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}
	}
}
