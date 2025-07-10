using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationAttemptFilterProvider : FilterStripBusinessObject
	{
		#region Query Delegates

		public ZQuery GetStatusQuery(ZString status)
		{
			var query = new ZDBOnlyQuery(typeof(GlbAccreditationAttempt));
			var now = ZDateTime.Now;

			if (status.EqualsIgnoringCase(CommencedStatus))
			{
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_CompletionDate, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, GlbAccreditationAttemptSchema.HAA_ExpiryDate, SQLComparisonOperator.GreaterThan, now);
				query.AddToFilter(JoinCondition.And, GlbAccreditationAttemptSchema.HAA_CompletionDueDate, SQLComparisonOperator.GreaterThan, now);
			}
			else if (status.EqualsIgnoringCase(FailedToCompleteStatus))
			{
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_CompletionDate, SQLComparisonOperator.Equal, null);
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_CompletionDueDate, SQLComparisonOperator.LessThanOrEqualTo, now);
			}
			else if (status.EqualsIgnoringCase(CompletedStatus))
			{
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_CompletionDate, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(JoinCondition.And, GlbAccreditationAttemptSchema.HAA_ExpiryDate, SQLComparisonOperator.GreaterThan, now);
			}
			else if (status.EqualsIgnoringCase(ExpiredStatus))
			{
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_CompletionDate, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_ExpiryDate, SQLComparisonOperator.LessThanOrEqualTo, now);
			}

			return query;
		}

		public GetTextQueryWithOperator GetEmailQuery()
		{
			return delegate(SQLComparisonOperator comparisonOperator, ZString email)
			{
				var applicantSubQuery = new ZDBOnlySubQuery(typeof(HRJobApplicant), HRJobApplicantSchema.HA_PER);
				applicantSubQuery.AddToFilter(HRJobApplicantSchema.HA_EmailAddress, comparisonOperator, email);
				var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
				contactSubQuery.AddToFilter(OrgContactSchema.OC_Email, comparisonOperator, email);
				var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_PER);
				staffSubQuery.AddToFilter(GlbStaffSchema.GS_EmailAddress, comparisonOperator, email);

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbAccreditationAttempt));
				var personSubQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
				personSubQuery.AddToFilter(GlbPersonSchema.PER_EmailAddress, comparisonOperator, email);

				query.AddSubQuery(GlbAccreditationAttemptSchema.HAA_PER, personSubQuery, JoinCondition.Or);
				query.AddSubQuery(GlbAccreditationAttemptSchema.HAA_PER, applicantSubQuery, JoinCondition.Or);
				query.AddSubQuery(GlbAccreditationAttemptSchema.HAA_PER, contactSubQuery, JoinCondition.Or);
				query.AddSubQuery(GlbAccreditationAttemptSchema.HAA_PER, staffSubQuery, JoinCondition.Or);

				return query;
			};
		}

		public GetTextQueryWithOperator GetOrganizationQuery()
		{
			return delegate(SQLComparisonOperator comparisonOperator, ZString value)
			{
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, comparisonOperator, value);

				var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				orgSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);

				var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
				contactSubQuery.AddSubQuery(OrgContactSchema.OC_OA_OrgAddress, orgAddressSubQuery, JoinCondition.Or);
				contactSubQuery.AddSubQuery(OrgContactSchema.OC_OH, orgSubQuery, JoinCondition.Or);

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbAccreditationAttempt));
				query.AddSubQuery(GlbAccreditationAttemptSchema.HAA_PER, contactSubQuery, JoinCondition.And);

				return query;
			};
		}

		public ZQuery GetLocationQuery(ZString value)
		{
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, value);

			var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, value);

			var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
			contactSubQuery.AddSubQuery(OrgContactSchema.OC_OA_OrgAddress, orgAddressSubQuery, JoinCondition.Or);
			contactSubQuery.AddSubQuery(OrgContactSchema.OC_OH, orgSubQuery, JoinCondition.Or);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbAccreditationAttempt));
			query.AddSubQuery(GlbAccreditationAttemptSchema.HAA_PER, contactSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Module Filter Sub Groups

		public class PersonSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbAccreditationAttempt));
				var subQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(GlbAccreditationAttemptSchema.HAA_PER, subQuery, JoinCondition.And);

				return query;
			}
		}

		public class AccreditationSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbAccreditationAttempt));
				var subQuery = new ZDBOnlySubQuery(typeof(GlbAccreditation), GlbAccreditationSchema.PK);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(GlbAccreditationAttemptSchema.HAA_HAC, subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string Person = "Person";
			public const string PersonFullName = "Person Full Name";
			public const string EmailAddress = "Person Related Email Address";
			public const string RelatedOrganizationName = "Related Organization Name";
			public const string WorkingLocation = "Working Location";
			public const string AccreditationCode = "Accreditation Code";
			public const string HighestLevelReachedByProgram = "Highest Level Reached By Program";
			public const string Status = "Status";
			public const string CommencementDate = "Commencement Date";
			public const string CompletionDueDate = "Completion Due Date";
			public const string CompletionDate = "Completion Date";
			public const string ExpiryDate = "Expiry Date";
			public const string WebPublished = "Web Published";
			public const string IsRefresher = "Is Refresher";
			public const string CertificateCode = "Certificate Code";

			#endregion
		}

		#endregion

		#region Lookups

		#region Status

		public CodeDescriptionPairList StatusList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();

				result.AddPair(CommencedStatus, Res.GetString("GlbAccreditationAttemptBusinessObject|Status|Commenced", "Show Commenced Only"));
				result.AddPair(CompletedStatus, Res.GetString("GlbPersonFilterBusinessObject|Status|Completed", "Show Completed Only"));
				result.AddPair(ExpiredStatus, Res.GetString("GlbPersonFilterBusinessObject|Status|Expired", "Show Expired Only"));
				result.AddPair(FailedToCompleteStatus, Res.GetString("GlbPersonFilterBusinessObject|Status|Failed", "Show Failed To Complete Only"));
				return result;
			}
		}

		protected string CommencedStatus = Res.GetString("8A7A738A-7E31-40AB-B488-6C07FF6B8FAA", "Commenced");
		protected string CompletedStatus = Res.GetString("081A1C27-86BF-4C0E-8853-01CBC9F934BB", "Completed");
		protected string ExpiredStatus = Res.GetString("8C34A94D-A913-45EC-8C1F-5F4030A92ECC", "Expired");
		protected string FailedToCompleteStatus = Res.GetString("206AA473-AB96-42F1-96DE-6327207C8C11", "Failed To Complete");

		#endregion

		#region Persons

		public GlbPersonCollection Persons
		{
			get
			{
				if (fPersons == null)
				{
					fPersons = new GlbPersonCollection(Factory);
				}
				return fPersons;
			}
		}
		GlbPersonCollection fPersons;

		#endregion

		#region Accreditation Requirements

		public GlbAccreditationCollection Accreditations
		{
			get
			{
				if (fAccreditations == null)
				{
					fAccreditations = new GlbAccreditationCollection(Factory);
				}
				return fAccreditations;
			}
		}
		GlbAccreditationCollection fAccreditations;

		#endregion

		public GlbAccreditationGroupCollection AccreditationGroups
		{
			get
			{
				if (fAccreditationGroups == null)
				{
					fAccreditationGroups = new GlbAccreditationGroupCollection(Factory);
				}
				return fAccreditationGroups;
			}
		}

		GlbAccreditationGroupCollection fAccreditationGroups;

		#region Locations

		public LocationCollection Locations
		{
			get
			{
				if (fLocations == null)
				{
					fLocations = new LocationCollection(Factory, false);
				}
				return fLocations;
			}
		}
		LocationCollection fLocations;

		#endregion

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}
	}
}
