using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Module
{
	public class HVLVOriginLoadListFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddNumbersAndReferencesFilters(filters);
			AddModeAndTypeFilters(filters);
			AddDateFilters(filters);
			AddOrganisationFilters(filters);
			AddStatusAndFlagsFilters(filters);

			return filters;
		}

		#region Descriptions

		static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string LoadListNo = "Load List #";
			public const string MasterBillNo = "Master Bill #";
			public const string HouseBillNo = "House Bill #";
			public const string VoyageVessel = "Voyage / Vessel";
			public const string ContainerNo = "Container #";
			public const string ItemId = "Item ID";
			public const string ConsignmentId = "Consignment ID";
			public const string ShipperReference = "Shipper Reference";

			public const string ServiceLevel = "Service Level";
			public const string TransportMode = "Transport Mode";
			public const string ContainerType = "Container Type";

			public const string ETD = "ETD";
			public const string ETA = "ETA";

			public const string OriginDepot = "Origin Depot";
			public const string DestinationDepot = "Destination Depot";
			public const string Carrier = "Carrier";
			public const string CTO = "Cargo Terminal Operator";

			public const string LoadListStatus = "Load List Status";
			public const string Incoterm = "Incoterm";
			public const string IsMasterHouse = "Is Master House";

			#endregion
		}

		#endregion

		#region Sub groups

		public ModuleFilterSubGroup ItemFilterProcessor => itemFilterProcessor ?? (itemFilterProcessor = new ItemSubGroup());
		ItemSubGroup itemFilterProcessor;

		public ModuleFilterSubGroup ConsignmentFilterProcessor => consignmentFilterProcessor ?? (consignmentFilterProcessor = new ConsignmentSubGroup(ItemFilterProcessor));
		ConsignmentSubGroup consignmentFilterProcessor;

		class ItemSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var itemSubQuery = new ZDBOnlySubQuery(typeof(HVLVItem), HVLVItemSchema.HVI_HVL_LoadList);
				itemSubQuery.AddToFilter(filter);

				var loadListQuery = new ZDBOnlyQuery(typeof(HVLVOriginLoadList));
				loadListQuery.AddSubQuery(itemSubQuery, JoinCondition.And);

				return loadListQuery;
			}
		}

		class ConsignmentSubGroup : ModuleFilterSubGroup
		{
			public ConsignmentSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var consignmentSubQuery = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVItemSchema.HVI_HVC_Consignment);
				consignmentSubQuery.AddToFilter(filter);

				var itemQuery = new ZDBOnlyQuery(typeof(HVLVItem));
				itemQuery.AddSubQuery(consignmentSubQuery, JoinCondition.And);

				return itemQuery;
			}
		}

		ZQuery CargoTerminalOperatorQuery(ZGuid value)
		{
			var ctoSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			ctoSubQuery.AddToFilter(OrgHeaderSchema.PK, value);

			var ctoAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			ctoAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_OH, ctoSubQuery, JoinCondition.And);

			var agentSubQuery = new ZDBOnlySubQuery(typeof(OrgAppointedAgentPorts), OrgAppointedAgentPortsSchema.O5_OH);
			agentSubQuery.AddToFilter(OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType, SQLComparisonOperator.Equal, HVLVOriginLoadListSchema.HVL_TransportMode);
			agentSubQuery.AddSubQuery(OrgAppointedAgentPortsSchema.O5_OA_AgentOfficeAddress, ctoAddressSubQuery, JoinCondition.And);

			var depotHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			depotHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, OrgAppointedAgentPortsSchema.O5_PortOrCountry);

			var depotAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			depotAddressSubQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, OrgAppointedAgentPortsSchema.O5_PortOrCountry);
			depotAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_OH, depotHeaderSubQuery, JoinCondition.Or);

			agentSubQuery.AddSubQuery(HVLVOriginLoadListSchema.HVL_OA_OriginDepot, depotAddressSubQuery, JoinCondition.And);

			var loadListSubQuery = new ZDBOnlyQuery(typeof(HVLVOriginLoadList));
			loadListSubQuery.AddSubQuery(HVLVOriginLoadListSchema.HVL_OH_Carrier, agentSubQuery, JoinCondition.And);

			return loadListSubQuery;
		}

		#endregion

		#region Numbers and References

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var loadListNoFilter = filters.AddTextFilter(Descriptions.LoadListNo, HVLVOriginLoadListSchema.HVL_UniqueReference);
			loadListNoFilter.MultilingualDescription = ResString.GetMultilingualString("c516204d-ecc2-4c10-a24d-03ddfa11772f", "Load List #");
			loadListNoFilter.Category = FilterCategories.NumbersAndReferences;

			var masterBillNoFilter = filters.AddTextFilter(Descriptions.MasterBillNo, HVLVOriginLoadListSchema.HVL_MasterBillNumber);
			masterBillNoFilter.MultilingualDescription = ResString.GetMultilingualString("aef3aa37-7817-4a02-a622-880dee1b6e0b", "Master Bill #");
			masterBillNoFilter.Category = FilterCategories.NumbersAndReferences;

			var houseBillNoFilter = filters.AddTextFilter(Descriptions.HouseBillNo, HVLVOriginLoadListSchema.HVL_HouseBillNumber);
			houseBillNoFilter.MultilingualDescription = ResString.GetMultilingualString("9fd683b2-75ad-4d0c-b7e1-b6d1421088b7", "House Bill #");
			houseBillNoFilter.Category = FilterCategories.NumbersAndReferences;

			var voyageVesselFilter = new VoyageVesselModuleFilter(Descriptions.VoyageVessel, VoyageVesselQuery, VesselLookup)
				.WithMaxLengthOf(HVLVOriginLoadListSchema.HVL_VoyageFlight, HVLVOriginLoadListSchema.HVL_VesselName);
			voyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("256871a5-aee1-4695-a312-87585bfb71cf", "Flight / Voyage No And Vessel");
			voyageVesselFilter.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(voyageVesselFilter);

			var containerNoFilter = filters.AddTextFilter(Descriptions.ContainerNo, HVLVOriginLoadListSchema.HVL_ContainerNumber);
			containerNoFilter.MultilingualDescription = ResString.GetMultilingualString("31fb83ef-1729-4935-9c3e-fc6601c7e3fb", "Container #");
			containerNoFilter.Category = FilterCategories.NumbersAndReferences;

			var itemIdFilter = filters.AddTextFilter(Descriptions.ItemId, HVLVItemSchema.HVI_ItemId);
			itemIdFilter.MultilingualDescription = ResString.GetMultilingualString("5f948b2c-f9e9-472d-9621-e35cafb0e0b7", "Item ID");
			itemIdFilter.Category = FilterCategories.NumbersAndReferences;
			itemIdFilter.SubGroup = ItemFilterProcessor;

			var consignmentIdFilter = filters.AddTextFilter(Descriptions.ConsignmentId, HVLVConsignmentSchema.HVC_ConsignmentId);
			consignmentIdFilter.MultilingualDescription = ResString.GetMultilingualString("039f962d-38f7-4bc4-8900-c86201c57d53", "Consignment ID");
			consignmentIdFilter.Category = FilterCategories.NumbersAndReferences;
			consignmentIdFilter.SubGroup = ConsignmentFilterProcessor;

			var shipperReferenceFilter = filters.AddTextFilter(Descriptions.ShipperReference, HVLVConsignmentSchema.HVC_ShipperReference);
			shipperReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("599ae204-b20d-49c6-9569-ed31582beaae", "Shipper Reference");
			shipperReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			shipperReferenceFilter.SubGroup = ConsignmentFilterProcessor;
		}

		ZQuery VoyageVesselQuery(SQLComparisonOperator comparison, ZString voyage, ZString vessel, ZBool includeArchived)
		{
			var query = new ZDBOnlyQuery(typeof(HVLVOriginLoadList));

			if (!voyage.IsEmpty || comparison == SpecialComparisonOperator.IsBlank || comparison == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(HVLVOriginLoadListSchema.HVL_VoyageFlight, comparison, voyage);
			}

			if (!vessel.IsEmpty || comparison == SpecialComparisonOperator.IsBlank || comparison == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(HVLVOriginLoadListSchema.HVL_VesselName, comparison, vessel);
			}

			return query;
		}

		#endregion

		#region Modes and Types

		void AddModeAndTypeFilters(ModuleFilterCollection filters)
		{
			var serviceLevelFilter = filters.AddTextFilter(Descriptions.ServiceLevel, HVLVOriginLoadListSchema.HVL_RS_NKServiceLevel);
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("e96ed9ae-2da9-4d58-a51f-c13272aa314d", "Service Level");
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;

			var transportModeFilter = filters.AddTextFilter(Descriptions.TransportMode, HVLVOriginLoadListSchema.HVL_TransportMode, TransportModeLookup);
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("d7aa9eed-eeff-40b3-9347-ccf4a06cb81e", "Transport Mode");
			transportModeFilter.Category = FilterCategories.ModesAndTypes;

			var containerTypeFilter = filters.AddGuidFilter(Descriptions.ContainerType, ModuleIDs.RefContainer, HVLVOriginLoadListSchema.HVL_RC_ContainerType, ContainerTypeLookup);
			containerTypeFilter.MultilingualDescription = ResString.GetMultilingualString("1a330d06-5984-45ab-a1a7-1ee4c215d3ca", "Container Type");
			containerTypeFilter.Category = FilterCategories.ModesAndTypes;
		}

		#endregion

		#region Dates

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var etdFilter = filters.AddDateFilter(Descriptions.ETD, HVLVOriginLoadListSchema.HVL_E_Dep);
			etdFilter.MultilingualDescription = ResString.GetMultilingualString("9f8585c8-4611-43ee-9c28-6af3bd63b99d", "ETD");
			etdFilter.Category = FilterCategories.Dates;

			var etaFilter = filters.AddDateFilter(Descriptions.ETA, HVLVOriginLoadListSchema.HVL_E_Arv);
			etaFilter.MultilingualDescription = ResString.GetMultilingualString("0b9eeb87-d3b4-47ab-b949-19bfebcaea79", "ETA");
			etaFilter.Category = FilterCategories.Dates;
		}

		#endregion

		#region Organisations and Staff

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			Func<string, SchemaGuidColumn, ModuleGuidFilter> addOrgAddressFilter = (name, orgAddressColumn) =>
			{
				GetGuidQuery queryDelegate = orgHeaderPK =>
				{
					var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), orgAddressColumn);
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgHeaderPK);

					var parentQuery = new ZDBOnlyQuery(typeof(HVLVOriginLoadList));
					parentQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

					return parentQuery;
				};

				var filter = filters.AddGuidFilter(name, ModuleIDs.Organisation, queryDelegate, OrganisationsLookup);
				filter.Category = FilterCategories.Organisations;
				return filter;
			};

			var originDepotFilter = addOrgAddressFilter(Descriptions.OriginDepot, HVLVOriginLoadListSchema.HVL_OA_OriginDepot);
			originDepotFilter.MultilingualDescription = ResString.GetMultilingualString("11724801-36b2-4131-a141-46f96ac1f3a6", "Origin Depot");

			var destinationDepotFilter = addOrgAddressFilter(Descriptions.DestinationDepot, HVLVOriginLoadListSchema.HVL_OA_DestinationDepot);
			destinationDepotFilter.MultilingualDescription = ResString.GetMultilingualString("83267694-df0a-4fcb-a32c-a3d556bb83b1", "Destination Depot");

			var carrierFilter = filters.AddGuidFilter(Descriptions.Carrier, ModuleIDs.Organisation, HVLVOriginLoadListSchema.HVL_OH_Carrier, OrganisationsLookup);
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("40041a9b-0afe-4b01-a4d1-d1288b65b585", "Carrier");
			carrierFilter.Category = FilterCategories.Organisations;

			var ctoFilter = filters.AddGuidFilter(Descriptions.CTO, ModuleIDs.Organisation, CargoTerminalOperatorQuery, OrganisationsLookup);
			ctoFilter.MultilingualDescription = ResString.GetMultilingualString("605c2978-643c-4634-8ef2-973c67add36b", "Cargo Terminal Operator");
			ctoFilter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region Status and Flags

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var loadListStatusFilter = filters.AddTextFilter(Descriptions.LoadListStatus, HVLVOriginLoadListSchema.HVL_Status, HVLStatusLookup);
			loadListStatusFilter.MultilingualDescription = ResString.GetMultilingualString("3cf85698-6cc3-4575-a9e1-abcbb712a052", "Load List Status");
			loadListStatusFilter.Category = FilterCategories.StatusAndFlags;

			var loadListINCOFilter = filters.AddTextFilter(Descriptions.Incoterm, HVLVOriginLoadListSchema.HVL_INCO, HVLIncoTermsLookup);
			loadListINCOFilter.MultilingualDescription = ResString.GetMultilingualString("ea1714cc-3632-41b5-a1de-634eab7504a2", "Incoterm");
			loadListINCOFilter.Category = FilterCategories.StatusAndFlags;

			var loadListIsMasterHouseFilter = filters.AddFlagFilter(Descriptions.IsMasterHouse,
				ResString.GetMultilingualString("329fd7d1-8df4-4de2-a7aa-91c60c65a5ff", "Is Master House"),
				HVLVOriginLoadListSchema.HVL_IsMasterHouse,
				ModuleFilterSubGroup.Default);
			loadListIsMasterHouseFilter.MultilingualDescription = ResString.GetMultilingualString("8b191e4f-d6ca-4bb2-a2ce-3b3cf503308f", "Is Master House");
			loadListIsMasterHouseFilter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		#region Lookups

		RefVesselCollection VesselLookup => vesselLookup ?? (vesselLookup = new RefVesselCollection(Factory));
		RefVesselCollection vesselLookup;

		OrgHeaderCollection OrganisationsLookup => organisationsLookup ?? (organisationsLookup = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisationsLookup;

		RefContainerCollection ContainerTypeLookup => containerTypeLookup ?? (containerTypeLookup = new RefContainerCollection(Factory));
		RefContainerCollection containerTypeLookup;

		CodeDescriptionPairList TransportModeLookup => transportModeLookup ?? (transportModeLookup = new CodeDescriptionPairList(OLookUpEditType.TransportType));
		CodeDescriptionPairList transportModeLookup;

		CodeDescriptionPairList HVLStatusLookup
		{
			get
			{
				var loadList = Factory.New<HVLVOriginLoadList>();
				return loadList.Lookups.HVL_Status_List;
			}
		}

		CodeDescriptionPairList HVLIncoTermsLookup
		{
			get
			{
				var loadlist = Factory.New<HVLVOriginLoadList>();
				return loadlist.Lookups.INCOTermsList;
			}
		}

		#endregion
	}
}
