using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class GlbPersonFilterBusinessObject : FilterStripBusinessObject
	{
		public GlbPersonFilterBusinessObject()
		{
			this.QueryObjectType = typeof(GlbPerson);
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			if (!Globals.IsWeb)
			{
				AddRelatedItemFilters(filters);
			}

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			if (!Globals.IsWeb)
			{
				if (Env.Security.PersonIntelligenceViewEmail.IsAllowed)
				{
					var emailFilter = filters.AddTextFilter("Email", GetPersonalEmailQuery());
					emailFilter.MaxLength = GlbPersonSchema.PER_EmailAddress.MaxLength;
					emailFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPersonFilter|Email", "Personal Email");
				}

				var relatedEmailFilter = filters.AddTextFilter("Related Email Address", GetRelatedEmailQuery());
				relatedEmailFilter.MaxLength = GlbPersonSchema.PER_EmailAddress.MaxLength;
				relatedEmailFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPersonFilter|EmailAddress", "Related Email Address");

				filters.AddTextFilter("Friendly Name", GlbPersonSchema.PER_FriendlyName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPersonFilter|FriendlyName", "Preferred Name");

				var orgFilter = filters.AddTextFilter("Related Organization Name", GetOrganizationQuery());
				orgFilter.MaxLength = OrgAddressSchema.OA_CompanyNameOverride.MaxLength;
				orgFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPersonFilter|RelatedOrganizationName", "Related Organization Name");
			}

			filters.AddTextFilter("Full Name", GlbPersonSchema.PER_FullName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPersonFilter|FullName", "Full Name");
		}

		GetTextQueryWithOperator GetPersonalEmailQuery()
		{
			return (comparisonOperator, email) =>
			{
				var applicantSubQuery = new ZDBOnlySubQuery(typeof(HRJobApplicant), HRJobApplicantSchema.HA_PER);
				applicantSubQuery.AddToFilter(HRJobApplicantSchema.HA_EmailAddress, comparisonOperator, email);

				var personSubQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
				personSubQuery.AddToFilter(GlbPersonSchema.PER_EmailAddress, comparisonOperator, email);

				personSubQuery.AddAsUnionQuery(applicantSubQuery, true);

				var topQuery = new ZDBOnlyQuery(typeof(GlbPerson));
				topQuery.AddSubQuery(personSubQuery, JoinCondition.And);

				return topQuery;
			};
		}

		GetTextQueryWithOperator GetRelatedEmailQuery()
		{
			return (comparisonOperator, email) =>
			{
				var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
				contactSubQuery.AddToFilter(OrgContactSchema.OC_Email, comparisonOperator, email);

				var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_PER);
				staffSubQuery.AddToFilter(GlbStaffSchema.GS_EmailAddress, comparisonOperator, email);

				staffSubQuery.AddAsUnionQuery(contactSubQuery, true);

				var topQuery = new ZDBOnlyQuery(typeof(GlbPerson));
				topQuery.AddSubQuery(staffSubQuery, JoinCondition.And);

				return topQuery;
			};
		}

		GetTextQueryWithOperator GetOrganizationQuery()
		{
			return (comparisonOperator, value) =>
			{
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, comparisonOperator, value);

				var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				orgSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);

				var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
				contactSubQuery.AddSubQuery(OrgContactSchema.OC_OA_OrgAddress, orgAddressSubQuery, JoinCondition.Or);
				contactSubQuery.AddSubQuery(OrgContactSchema.OC_OH, orgSubQuery, JoinCondition.Or);

				var query = new ZDBOnlyQuery(typeof(GlbPerson));
				query.AddSubQuery(contactSubQuery, JoinCondition.And);

				return query;
			};
		}

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var orgContactSubGroup = new OrgContactSubGroup();
			var jobApplicantSubGroup = new HRJobApplicantSubGroup();
			var staffSubGroup = new GlbStaffSubGroup();

			var contextFilter = filters.AddFlagsFilter("Related Context",
				new string[]
				{
					OrgContactDescription,
					HRJobApplicantDescription,
					GlbStaffDescription
				},
				new GetFlagsQuery[]
				{
					(enabled) => !enabled ? null : orgContactSubGroup.GetSubQuery(null),
					(enabled) => !enabled ? null : jobApplicantSubGroup.GetSubQuery(null),
					(enabled) => !enabled ? null : staffSubGroup.GetSubQuery(null),
				},
				JoinCondition.And);
			contextFilter.MultilingualDescription = ResString.GetMultilingualString("32f1ec67-1eb7-4973-baf9-b439c149ff72", "Related Context");
			contextFilter.Category = FilterCategories.Other;

			if (Env.Security.OrganisationView.IsAllowed)
			{
				var contactFilter = new ModuleGuidForeignCollectionFilter("Organization Contact", ModuleIDs.OrgContacts, GlbPersonSchema.PK, OrgContactSchema.OC_PER, new OrgContactCollection(Factory), typeof(GlbPerson));
				contactFilter.MultilingualDescription = OrgContactDescription;
				filters.AddFilter(contactFilter);
			}

			if (Env.Security.HRJobApplicantView.IsAllowed)
			{
				var applicantFilter = new ModuleGuidForeignCollectionFilter("Job Applicant", ModuleIDs.HRJobApplicant, GlbPersonSchema.PK, HRJobApplicantSchema.HA_PER, (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IHRJobApplicantCollection>(), Factory), typeof(GlbPerson));
				applicantFilter.MultilingualDescription = HRJobApplicantDescription;
				filters.AddFilter(applicantFilter);
			}

			if (Env.Security.StaffView.IsAllowed)
			{
				var staffFilter = new ModuleGuidForeignCollectionFilter("Staff", ModuleIDs.GlbStaff, GlbPersonSchema.PK, GlbStaffSchema.GS_PER, new GlbStaffCollection(Factory), typeof(GlbPerson));
				staffFilter.MultilingualDescription = GlbStaffDescription;
				filters.AddFilter(staffFilter);
			}

			if (Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed)
			{
				filters.AddNkFilter("Country", GlbPersonSchema.PER_RN_NKCountry, ModuleIDs.RefCountry, new RefCountryCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPersonFilter|Country", "Country/Region");
			}

			if (Env.Security.GlbAccreditationAttemptView.IsAllowed)
			{
				var accredFilter = new ModuleGuidForeignCollectionFilter("Accreditation", ModuleIDs.GlbAccreditationAttempt, GlbPersonSchema.PK, GlbAccreditationAttemptSchema.HAA_PER, new GlbAccreditationAttemptCollection(Factory), typeof(GlbPerson));
				accredFilter.MultilingualDescription = AccreditationAttemptDescription;
				filters.AddFilter(accredFilter);
			}

			AddRelatedOrganizationFilter(filters);
		}

		void AddRelatedOrganizationFilter(ModuleFilterCollection filters)
		{
			var relatedOrganizationFilter = new ModuleGuidFilter("Related Organization", ModuleIDs.Organisation, new GetGuidQueryWithOperator(GetRelatedOrganizationQuery), Organisations);
			relatedOrganizationFilter.Category = FilterCategories.Other;
			relatedOrganizationFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPersonFilter|RelatedOrganization", "Related Organization");
			filters.AddFilter(relatedOrganizationFilter);
		}

		ZQuery GetRelatedOrganizationQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgSubQuery.AddToFilter(OrgHeaderSchema.PK, comparisonOperator, value);

			var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
			contactSubQuery.AddSubQuery(OrgContactSchema.OC_OH, orgSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(GlbPerson));
			query.AddSubQuery(contactSubQuery, JoinCondition.And);

			return query;
		}

		OrganisationsFindBoxCollection Organisations
		{
			get { return fOrganisations ?? (fOrganisations = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection fOrganisations;

		#region Related Context Filter

		public static MultilingualString OrgContactDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|OrgContactDescription", "Organization Contact"); } }
		public static MultilingualString HRJobApplicantDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|HRJobApplicantDescription", "Job Applicant"); } }
		public static MultilingualString GlbStaffDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|GlbStaffDescription", "Staff"); } }
		public static MultilingualString AccreditationAttemptDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|AccreditationDescription", "Accreditation Attempts"); } }

		#endregion

		#region Accreditation Filter

		public static FilterCategory AccreditationCategory { get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("GlbPersonFilterBusinessObject|AccreditationCategoryDescription", "Accreditation")); } }

		public static MultilingualString AccreditationCodeDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|AccreditationCodeDescription", "Code"); } }
		public static MultilingualString AccreditationStatusDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|AccreditationStatusDescription", "Status"); } }
		public static MultilingualString AccreditationCommencedDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|AccreditationCommencedDescription", "Commenced Date"); } }
		public static MultilingualString AccreditationCompletedDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|AccreditationCompletedDescription", "Completed Date"); } }
		public static MultilingualString AccreditationCompletedDueDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|AccreditationCompletedDueDescription", "Completed Due Date"); } }
		public static MultilingualString AccreditationExpiryDescription { get { return ResString.GetMultilingualString("GlbPersonFilterBusinessObject|AccreditationExpiryDescription", "Expiry Date"); } }

		public CodeDescriptionPairList AccreditationStatusList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();

				result.AddPair(Res.GetString("8A7A738A-7E31-40AB-B488-6C07FF6B8F72", "Commenced"), Res.GetString("GlbPersonFilterBusinessObject|AccreditationStatusList|Commenced", "Show Commenced Only"));
				result.AddPair(Res.GetString("081A1C27-86BF-4C0E-8853-01CBC9F93463", "Completed"), Res.GetString("GlbPersonFilterBusinessObject|AccreditationStatusList|Completed", "Show Completed Only"));
				result.AddPair(Res.GetString("8C34A94D-A913-45EC-8C1F-5F4030A92EE8", "Expired"), Res.GetString("GlbPersonFilterBusinessObject|AccreditationStatusList|Expired", "Show Expired Only"));

				return result;
			}
		}

		#endregion
		#endregion

		#region ModuleFilterSubGroups

		class OrgContactSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var dBOnlyQuery = new ZDBOnlyQuery(typeof(GlbPerson));

				var subQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
				subQuery.AddToFilter(filter);
				dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);

				return dBOnlyQuery;
			}
		}

		class HRJobApplicantSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var dBOnlyQuery = new ZDBOnlyQuery(typeof(GlbPerson));

				var subQuery = new ZDBOnlySubQuery(typeof(IHRJobApplicant), HRJobApplicantSchema.HA_PER);
				subQuery.AddToFilter(filter);
				dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);

				return dBOnlyQuery;
			}
		}

		class GlbStaffSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var dBOnlyQuery = new ZDBOnlyQuery(typeof(GlbPerson));

				var subQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_PER);
				subQuery.AddToFilter(filter);
				dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);

				return dBOnlyQuery;
			}
		}

		#endregion
	}
}
