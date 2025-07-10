using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Module
{
	public class CarrierContractFilterStripBusinessObject : FilterStripBusinessObject
	{
		public CarrierContractFilterStripBusinessObject()
			: base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.CarrierContracts.Name;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var contractNumberFilter = filters.AddTextFilter(CarrierContractFilterConstants.ContractNumber, RatingContractSchema.RCT_ContractNumber);
			contractNumberFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|ContractNumber", "Contract #");
			contractNumberFilter.IsExclusiveHelper = true;

			filters.AddTextFilter(CarrierContractFilterConstants.TransportMode, RatingContractSchema.RCT_TransportMode)
				.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|TransportMode", "Transport Mode");
			filters.AddDateFilter(CarrierContractFilterConstants.StartDate, RatingContractSchema.RCT_StartDate)
				.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|StartDate", "Start Date");
			filters.AddDateFilter(CarrierContractFilterConstants.ExpiryDate, RatingContractSchema.RCT_EndDate)
				.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|ExpiryDate", "Expiry Date");
			filters.AddTextFilter(CarrierContractFilterConstants.Description, RatingContractSchema.RCT_Description)
				.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|Description", "Description");
			filters.AddTextFilter(CarrierContractFilterConstants.ContainerType, RatingContractSchema.RCT_ContainerType, ContainerTypes)
				.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|ContainerType", "Container Type");

			var serviceProviderFilter = new ModuleGuidFilterForOrg(CarrierContractFilterConstants.ServiceProvider, ModuleIDs.Organisation, RatingContractSchema.RCT_OH, OrgHeaderNamedAccounts);
			serviceProviderFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|ServiceProvider", "Service Provider");
			filters.AddFilter(serviceProviderFilter);

			var relatedAllocationsFilter = new RelatedAllocationsOfContractFilter(CarrierContractFilterConstants.AllocationRoutes, Factory);
			relatedAllocationsFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|RelatedAllocationRoutes", "Allocation Routes");
			filters.AddFilter(relatedAllocationsFilter);

			var relatedNamedAccountsFilter = new NamedAccountsOfContractFilter(CarrierContractFilterConstants.NamedAccountClients, OrgHeaderNamedAccounts, typeof(RatingContract), RatingContractSchema.Constants.Prefix, RatingContractSchema.Constants.PK);
			relatedNamedAccountsFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|RelatedNamedAccounts", "Named Account Clients");
			filters.AddFilter(relatedNamedAccountsFilter);

			var contractOwnerFilter = new ModuleNkFilter(CarrierContractFilterConstants.ContractOwner, RatingContractSchema.RCT_GS_NKContractOwner, ModuleIDs.GlbStaff, GlbStaffCollection);
			contractOwnerFilter.Category = FilterCategories.Organisations;
			contractOwnerFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|ContractOwner", "Contract Owner");
			filters.AddFilter(contractOwnerFilter);

			AddFlagFilters(filters);

			return filters;
		}

		protected override ReadOnlyCollection<ModuleFilter> GetAlwaysVisibleModuleFiltersCore()
		{
			var result = new List<ModuleFilter>();
			foreach (var filter in this)
			{
				if (filter.Visibility == FilterVisibility.AlwaysVisible)
				{
					filter.Visibility = FilterVisibility.Visible;
					result.Add(filter);
				}
			}
			return result.AsReadOnly();
		}

		CodeDescriptionPairList ContainerTypes => containerTypes ??= new RefContainerLookups(null).ContainerTypes;
		CodeDescriptionPairList containerTypes;

		OrgHeaderCollection OrgHeaderNamedAccounts => orgHeaderNamedAccounts ??= new OrgHeaderCollection(Factory);
		OrgHeaderCollection orgHeaderNamedAccounts;

		GlbStaffCollection GlbStaffCollection => glbStaffCollection ??= new GlbStaffCollection(Factory);
		GlbStaffCollection glbStaffCollection;

		#region Add Flag Filters

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var allowHazardousCommoditiesFilter = filters.AddFlagsFilter(
				CarrierContractFilterConstants.AllowHazardousCommodities,
				new[] { Res.GetString("ef2d88ef-57eb-70a8-464a-c872a5562c9a", "Allow Hazardous Commodities") },
				new GetFlagsQuery[] { GetAllowHazardousQuery });
			allowHazardousCommoditiesFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|AllowHazardousCommodities", CarrierContractFilterConstants.AllowHazardousCommodities);

			var hasAllocationRoutesFilter = filters.AddFlagsFilter(
				CarrierContractFilterConstants.HasAllocationRoutes,
				new[] { Res.GetString("105ec426-9639-e388-4478-5109a65ee0e2", "Has Allocation Routes") },
				new GetFlagsQuery[] { GetHasAllocationRoutesQuery });
			hasAllocationRoutesFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContract|CarrierContractFilter|HasAllocationRoutes", CarrierContractFilterConstants.HasAllocationRoutes);
		}

		ZQuery GetAllowHazardousQuery(ZBool allowHazardous)
			=> new ZDBOnlyQuery(typeof(RatingContract)).AddToFilter(RatingContractSchema.RCT_AllowHazardousCommodities, allowHazardous);

		ZQuery GetHasAllocationRoutesQuery(ZBool hasAllocationRoutes)
		{
			var relatedRoutesSubQuery = new ZDBOnlySubQuery(typeof(RatingContractAllocationLine), RatingContractAllocationLineSchema.RCA_RCT_RatingContract, !hasAllocationRoutes);
			var contractsQuery = new ZDBOnlyQuery(typeof(RatingContract));
			contractsQuery.AddSubQuery(relatedRoutesSubQuery, JoinCondition.And);
			return contractsQuery;
		}

		#endregion

		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;
				filter.AddToFilter(new ZQuery(RatingContractSchema.RCT_ContractType, Core.Constants.RatingContractTypes.Provider));
				filter.AddToFilter(new ZQuery(RatingContractSchema.RCT_IsActive, true));

				return filter;
			}
		}
	}
}
