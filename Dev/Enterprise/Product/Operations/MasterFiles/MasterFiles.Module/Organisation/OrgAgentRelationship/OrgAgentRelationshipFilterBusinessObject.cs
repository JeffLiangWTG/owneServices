using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgAgentRelationshipFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		public static class Descriptions
		{
			public const string AgencyOffice = "Agency Office";
			public const string SendingReceivingAgent = "Sending / Receiving Agent";
			public const string HeadOffice = "Head Office";
			public const string ControllingCustomer = "Controlling Customer";
			public const string ProfitShareType = "Profit Share Type";
			public const string StartDate = "Start Date";
			public const string EndDate = "End Date";
			public const string JobType = "Job Type";
			public const string FreightMode = "Freight Mode";
			public const string GatewayAgentType = "Gateway Agent Type";
			public const string ShareLosses = "Share Losses";
			public const string AgreementType = "Agreement Type";
			public const string ReceivingPortOrCountry = "Receiving Location";
			public const string SendingPortOrCountry = "Sending Location";
			public const string GatewayProfitApportionmentMethod = "Gateway Profit Apportionment Method";
			public const string OrganizationOverrideType = "Organization Override Type";
			public const string OrganizationOverride = "Organization Override";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddAgencyOfficeFilter(filters);
			AddSendingReceivingAgentFilter(filters);
			AddHeadOfficeFilter(filters);
			AddControllingCustomerFilter(filters);
			AddProfitShareTypeFilter(filters);
			AddStartDateFilter(filters);
			AddEndDateFilter(filters);
			AddJobTypeFilter(filters);
			AddFreightModeFilter(filters);
			AddGatewayAgentTypeFilter(filters);
			AddShareLossesFilter(filters);
			AddAgreementTypeFilter(filters);
			AddReceivingPortOrCountryFilter(filters);
			AddSendingPortOrCountryFilter(filters);
			AddGatewayProfitApportionmentMethodFilter(filters);
			AddOrgOverrideTypeFilter(filters);
			AddOrgOverrideFilter(filters);

			return filters;
		}

		void AddAgencyOfficeFilter(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter(Descriptions.AgencyOffice, ModuleIDs.Organisation, GetAgencyOfficeQuery, Forwarders).
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|AgencyOffice", Descriptions.AgencyOffice);
		}

		ZQuery GetAgencyOfficeQuery(ZGuid value)
		{
			var result = new ZQuery();
			if (value.IsValid)
			{
				var agencyOfficeQuery = new ZDBOnlyQuery(typeof(OrgAgentRelationship));
				agencyOfficeQuery.AddToFilter(OrgAgentRelationshipSchema.O3_ProfitShareType, OrgAgentRelationship.ProfitShareTypes.AgencyProfile);
				agencyOfficeQuery.AddToFilter(OrgAgentRelationshipSchema.O3_OH_SendingAgent, value);

				result.AddToFilter(agencyOfficeQuery);
			}
			return result;
		}

		void AddSendingReceivingAgentFilter(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter(Descriptions.SendingReceivingAgent, ModuleIDs.Organisation, OrgAgentRelationshipSchema.O3_OH_SendingAgent, Forwarders, OrgAgentRelationshipSchema.O3_OH_ReceivingAgent, Forwarders).
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|SendingReceivingAgent", Descriptions.SendingReceivingAgent);
		}

		void AddHeadOfficeFilter(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter(Descriptions.HeadOffice, ModuleIDs.Organisation, OrgAgentRelationshipSchema.O3_OH_GroupNetworkOrFranchise, Organisations).
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|HeadOffice", Descriptions.HeadOffice);
		}

		void AddControllingCustomerFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(Descriptions.ControllingCustomer, ModuleIDs.Organisation, OrgProfitShareDetailsSchema.O4_OH_ControllingAgent, Organisations);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|ControllingCustomer", Descriptions.ControllingCustomer);
			filter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddProfitShareTypeFilter(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Descriptions.ProfitShareType, OrgAgentRelationshipSchema.O3_ProfitShareType, new OrgAgentRelationshipLookups(null).ProfitShareTypeList).
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|ProfitShareType", Descriptions.ProfitShareType);
		}

		void AddStartDateFilter(ModuleFilterCollection filters)
		{
			var startDateFilter = filters.AddDateFilter(Descriptions.StartDate, OrgProfitShareDetailsSchema.O4_StartDate);
			startDateFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|StartDate", Descriptions.StartDate);
			startDateFilter.Category = FilterCategories.Dates;
			startDateFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddEndDateFilter(ModuleFilterCollection filters)
		{
			var endDateFilter = filters.AddDateFilter(Descriptions.EndDate, OrgProfitShareDetailsSchema.O4_EndDate);
			endDateFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|EndDate", Descriptions.EndDate);
			endDateFilter.Category = FilterCategories.Dates;
			endDateFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddJobTypeFilter(ModuleFilterCollection filters)
		{
			var jobTypeFilter = filters.AddTextFilter(Descriptions.JobType, OrgProfitShareDetailsSchema.O4_JobType, new JobTypesList());
			jobTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|JobType", Descriptions.JobType);
			jobTypeFilter.Category = FilterCategories.StatusAndFlags;
			jobTypeFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddFreightModeFilter(ModuleFilterCollection filters)
		{
			var freightModeFilter = filters.AddTextFilter(Descriptions.FreightMode, OrgProfitShareDetailsSchema.O4_FreightMode, new OrgProfitShareDetailsLookups.FreightModesList());
			freightModeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|FreightMode", Descriptions.FreightMode);
			freightModeFilter.Category = FilterCategories.StatusAndFlags;
			freightModeFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddGatewayAgentTypeFilter(ModuleFilterCollection filters)
		{
			var gatewayAgentTypeFilter = filters.AddTextFilter(Descriptions.GatewayAgentType, OrgProfitShareDetailsSchema.O4_GatewayAgentType, new GatewayAgentTypesList());
			gatewayAgentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|GatewayAgentType", Descriptions.GatewayAgentType);
			gatewayAgentTypeFilter.Category = FilterCategories.StatusAndFlags;
			gatewayAgentTypeFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddShareLossesFilter(ModuleFilterCollection filters)
		{
			var shareLossesFilter = filters.AddFlagFilter(Descriptions.ShareLosses, ResString.GetMultilingualString("0d3c4735-bc12-4e26-9ad4-db3ccfbb8521", Descriptions.ShareLosses), OrgProfitShareDetailsSchema.O4_ShareLosses, ModuleFilterSubGroup.Default);
			shareLossesFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|ShareLosses", Descriptions.ShareLosses);
			shareLossesFilter.Category = FilterCategories.StatusAndFlags;
			shareLossesFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddAgreementTypeFilter(ModuleFilterCollection filters)
		{
			var agreementTypeFilter = filters.AddTextFilter(Descriptions.AgreementType, OrgProfitShareDetailsSchema.O4_AgreementType, new OrgProfitShareDetailsLookups.AgreementTypesList());
			agreementTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|AgreementType", Descriptions.AgreementType);
			agreementTypeFilter.Category = FilterCategories.StatusAndFlags;
			agreementTypeFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddReceivingPortOrCountryFilter(ModuleFilterCollection filters)
		{
			var receivingPortOrCountryFilter = filters.AddNkFilter(Descriptions.ReceivingPortOrCountry, OrgProfitShareDetailsSchema.O4_ReceivingPortOrCountry, ModuleIDs.Location, new LocationCollection(Factory));
			receivingPortOrCountryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|ReceivingPortOrCountry", Descriptions.ReceivingPortOrCountry);
			receivingPortOrCountryFilter.Category = FilterCategories.Locations;
			receivingPortOrCountryFilter.SubGroup = OrgProfitShareDetailsSubGroup;
			receivingPortOrCountryFilter.SupportsFiltersMatchComparisonOperator = false;
		}

		void AddSendingPortOrCountryFilter(ModuleFilterCollection filters)
		{
			var sendingPortOrCountryFilter = filters.AddNkFilter(Descriptions.SendingPortOrCountry, OrgProfitShareDetailsSchema.O4_SendingPortOrCountry, ModuleIDs.Location, new LocationCollection(Factory));
			sendingPortOrCountryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|SendingPortOrCountry", Descriptions.SendingPortOrCountry);
			sendingPortOrCountryFilter.Category = FilterCategories.Locations;
			sendingPortOrCountryFilter.SubGroup = OrgProfitShareDetailsSubGroup;
			sendingPortOrCountryFilter.SupportsFiltersMatchComparisonOperator = false;
		}

		void AddGatewayProfitApportionmentMethodFilter(ModuleFilterCollection filters)
		{
			var gatewayProfitApportionmentMethodFilter = filters.AddTextFilter(Descriptions.GatewayProfitApportionmentMethod, OrgProfitShareDetailsSchema.O4_GatewayProfitApportionmentMethod, new GatewayProfitApportionmentMethodList());
			gatewayProfitApportionmentMethodFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|GatewayProfitApportionmentMethod", Descriptions.GatewayProfitApportionmentMethod);
			gatewayProfitApportionmentMethodFilter.Category = FilterCategories.StatusAndFlags;
			gatewayProfitApportionmentMethodFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddOrgOverrideTypeFilter(ModuleFilterCollection filters)
		{
			var orgOverrideTypeFilter = filters.AddTextFilter(Descriptions.OrganizationOverrideType, OrgProfitShareDetailsSchema.O4_OrgOverrideType, new OrgProfitShareDetailsLookups.OrgOverrideTypesList());
			orgOverrideTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|OrganizationOverrideType", Descriptions.OrganizationOverrideType);
			orgOverrideTypeFilter.Category = FilterCategories.StatusAndFlags;
			orgOverrideTypeFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		void AddOrgOverrideFilter(ModuleFilterCollection filters)
		{
			var orgOverrideFilter = filters.AddGuidFilter(Descriptions.OrganizationOverride, ModuleIDs.Organisation, OrgProfitShareDetailsSchema.O4_OH_OrgOverride, Organisations);
			orgOverrideFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAgentRelationshipFilter|OrganizationOverride", Descriptions.OrganizationOverride);
			orgOverrideFilter.Category = FilterCategories.Organisations;
			orgOverrideFilter.SubGroup = OrgProfitShareDetailsSubGroup;
		}

		#region SubGroups

		public class ProfitShareDetailsSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var profitShareDetailsSubQuery = new ZDBOnlySubQuery(typeof(OrgProfitShareDetails), OrgProfitShareDetailsSchema.O4_O3_OrgProfitShareHeader);
				profitShareDetailsSubQuery.AddToFilter(filter);

				var agentRelationshipQuery = new ZDBOnlyQuery(typeof(OrgAgentRelationship));
				agentRelationshipQuery.AddSubQuery(OrgAgentRelationshipSchema.PK, profitShareDetailsSubQuery, JoinCondition.And);

				return agentRelationshipQuery;
			}
		}

		public ProfitShareDetailsSubGroup OrgProfitShareDetailsSubGroup
		{
			get { return orgProfitShareDetailsSubGroup ?? (orgProfitShareDetailsSubGroup = new ProfitShareDetailsSubGroup()); }
		}

		ProfitShareDetailsSubGroup orgProfitShareDetailsSubGroup;

		#endregion

		#endregion

		OrgProfitShareDetailsCollection orgProfitShareDetailsCollection;
		public OrgProfitShareDetailsCollection OrgProfitShareDetailsCollection
		{
			get
			{
				if (orgProfitShareDetailsCollection == null)
				{
					orgProfitShareDetailsCollection = new OrgProfitShareDetailsCollection(Factory);
					orgProfitShareDetailsCollection.SetReadOnlyIncludingChildren(true);
				}
				return orgProfitShareDetailsCollection;
			}
		}

		public void RefreshProfitShareDetails(OrgAgentRelationship orgAgentRelationship)
		{
			OrgProfitShareDetailsCollection.RemoveAll();
			if (orgAgentRelationship != null)
			{
				OrgProfitShareDetailsCollection.AddRange(orgAgentRelationship.GenericProfitShareDetails);
				OrgProfitShareDetailsCollection.AddRange(orgAgentRelationship.ClientSpecificProfitShareDetails);
			}
		}

		#region Lookups

		public ForwarderCollection Forwarders
		{
			get { return fForwarders ?? (fForwarders = new ForwarderCollection(Factory)); }
		}

		ForwarderCollection fForwarders;

		public OrganisationsFindBoxCollection Organisations
		{
			get { return fOrganisations ?? (fOrganisations = new OrganisationsFindBoxCollection(Factory)); }
		}

		OrganisationsFindBoxCollection fOrganisations;

		#endregion
	}
}
