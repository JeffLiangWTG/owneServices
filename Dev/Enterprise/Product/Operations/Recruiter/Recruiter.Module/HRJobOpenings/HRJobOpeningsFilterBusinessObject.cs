using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class HRJobOpeningsFilterBusinessObject : FilterStripBusinessObject
	{
		public HRJobOpeningsFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddRelatedItemFilters(filters);
			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Ad Title", HRRecruitmentJobCampaignSchema.HV_AdTitle).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobOpenings|AdTitle", "Ad Title");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Ad Start Date", GetAdStartDateQuery, convertFromLocalToUTC: true, isNullable: false)
				.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobOpenings|AdStartDate", "Ad Start Date");
			filters.AddDateFilter("Ad End Date", GetAdEndDateQuery, convertFromLocalToUTC: true, isNullable: false)
				.MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobOpenings|AdEndDate", "Ad End Date");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			filters.AddNkFilter("Campaign Leader", HRRecruitmentJobCampaignSchema.HV_GS_NKControlledBy, ModuleIDs.GlbStaff, ControlledBys).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobOpenings|CampaignLeader", "Campaign Leader");
			filters.AddGuidFilter("Client", ModuleIDs.Organisation, HRRecruitmentJobCampaignSchema.HV_OH_ClientAccount, ClientAccounts).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobOpenings|Client", "Client");
			filters.AddGuidFilter("Job Role", ModuleIDs.HRJobRole, HRRecruitmentJobCampaignSchema.HV_HJ_JobRole, JobRoles).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobOpenings|JobRole", "Job Role");
			filters.AddNkFilter("Location", GetLocationQuery, ModuleIDs.Location, Locations).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobOpenings|Location", "Location");
		}

		ZQuery GetLocationQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(HRRecruitmentJobCampaign));
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), HRRecruitmentJobCampaignSchema.HV_OA_ClientAddress);
			orgAddressSubQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, value, OrgAddressSchema.OA_RL_NKRelatedPortCode, typeof(OrgAddress)));
			query.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetAdStartDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZQuery();
			if (value1.IsValid)
			{
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignStartDate, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
			}

			if (value2.IsValid)
			{
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignStartDate, SQLComparisonOperator.LessThanOrEqualTo, value2);
			}

			return query;
		}

		ZQuery GetAdEndDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZQuery();
			if (value1.IsValid)
			{
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignEndDate, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
			}

			if (value2.IsValid)
			{
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignEndDate, SQLComparisonOperator.LessThanOrEqualTo, value2);
			}

			return query;
		}

		#endregion

		#endregion

		#region Lookups

		#region ClientAccounts

		public OrganisationsFindBoxCollection ClientAccounts
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrganisationsFindBoxCollection(Factory);
				}

				return fOrganisations;
			}
		}

		OrganisationsFindBoxCollection fOrganisations;

		#endregion

		#region ControlledBys

		public virtual GlbStaffCollection ControlledBys
		{
			get
			{
				if (fControlledBys == null)
				{
					fControlledBys = new GlbStaffCollection(Factory);
				}
				return fControlledBys;
			}
		}
		GlbStaffCollection fControlledBys;

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

		#region Locations Collections

		public LocationCollection Locations
		{
			get { return new LocationCollection(Factory); }
		}

		#endregion

		#endregion
	}
}
