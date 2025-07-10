using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module
{
	public class ContainerMoveFilterStrip : FilterStripBusinessObject
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class Descriptions
		{
			// Numbers & Refs
			public const string ContainerNumber = "Container #";
			public const string ShipmentNumber = "Shipment #";
			public const string BillOfLading = "Bill of Lading";
			public const string DetentionJobNumber = "Detention Job #";
			public const string LeaseContractNo = "Lease Contract #";

			// Dates
			public const string MovementDate = "Movement Date";
			public const string CreatedTime = "Created Time Local";

			// Locations
			public const string DepotPort = "Depot Port";
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string LoadPort = "Load Port";
			public const string DischargePort = "Discharge Port";

			// Organisations / Staff
			public const string Client = "Client";
			public const string Depot = "Depot";
			public const string Owner = "Owner";
			public const string OwnerType = "OwnerType";
			public const string Principal = "Principal";
			public const string DetentionCompany = "Detention Company";

			// Modes And Types
			public const string ContainerCondition = "Container Condition";
			public const string ContainerQuality = "Container Quality";
			public const string ContainerType = "Container Type";
			public const string IsoType = "ISO Type";
			public const string MovedAsEmpty = "Moved as Empty";
			public const string MovementType = "Movement Type";
			public const string DetentionableType = "Triggering Detentions";

			// Status And Flags
			public const string DetentionInvoiced = "Detention Invoiced";
			public const string Detentionable = "Detentionable";

			// Text Search
			public const string VoyageVessel = "Voyage/Vessel";
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();
			AddNumberAndRefFilters(collection);
			AddModeAndTypeFilters(collection);
			AddDateTimeFilters(collection);
			AddLocationFilters(collection);
			AddOrganisationFilters(collection);
			AddStatusAndFlagsFilters(collection);
			AddVoyageVesseFilters(collection);
			return collection;
		}

		#region AddNumberAndRefFilters

		void AddNumberAndRefFilters(ModuleFilterCollection collection)
		{
			ModuleFilter filter = collection.AddNumberFilter(Descriptions.ContainerNumber, RefContainerStockSchema.R6_ContainerNum);
			filter.MultilingualDescription = ResString.GetMultilingualString("9e55b141-dd63-4a67-85c6-ce2a88450d03", "Container #");
			filter.SubGroup = ContainerStockSubGroup;

			filter = collection.AddFountainFilter(Descriptions.ShipmentNumber, JobShipmentSchema.JS_UniqueConsignRef, "V");
			filter.MultilingualDescription = ResString.GetMultilingualString("5fbe0d38-529c-47a8-b2dc-d72c2d55e722", "Shipment #");
			filter.SubGroup = ShipmentSubGroup;

			filter = collection.AddNumberFilter(Descriptions.BillOfLading, JobShipmentSchema.JS_HouseBill);
			filter.MultilingualDescription = ResString.GetMultilingualString("6f95ffcb-e6fa-4d66-a2bc-4119fe2910a0", "Bill of Lading");
			filter.SubGroup = ShipmentSubGroup;

			filter = collection.AddFountainFilter(Descriptions.DetentionJobNumber, JobContainerDetentionSchema.NC_JobNumber, "DI");
			filter.MultilingualDescription = ResString.GetMultilingualString("76f68236-02fa-43af-a59b-eaf09fad6c32", "Detention Job #");
			filter.SubGroup = DetentionSubGroup;

			filter = collection.AddNumberFilter(Descriptions.LeaseContractNo, JobContainerMoveSchema.E9_LeaseNumber);
			filter.MultilingualDescription = ResString.GetMultilingualString("ec922c4f-b42b-46bb-b577-20ebd41d5c11", "Lease Contract #");
		}

		#endregion

		#region AddModeAndTypeFilters

		void AddModeAndTypeFilters(ModuleFilterCollection collection)
		{
			ModuleFilter filter = collection.AddTextFilter(Descriptions.ContainerCondition, JobContainerMoveSchema.E9_ContainerCondition, AgencyRegistry.Instance.ContainerDamageCodes.Value);
			filter.MultilingualDescription = ResString.GetMultilingualString("8fd260ae-01d5-4b71-9889-0fdf84fd6b6c", "Container Condition");
			filter.Category = FilterCategories.ModesAndTypes;

			filter = collection.AddTextFilter(Descriptions.ContainerQuality, JobContainerMoveSchema.E9_ContainerQuality, AgencyRegistry.Instance.ContainerCleanCodes.Value);
			filter.MultilingualDescription = ResString.GetMultilingualString("60923424-8d08-46f0-be2b-2e1aa9d3a32d", "Container Quality");
			filter.Category = FilterCategories.ModesAndTypes;

			ModuleGuidFilter containerTypeFilter = collection.AddGuidFilter(Descriptions.ContainerType, ModuleIDs.RefContainer, RefContainerStockSchema.R6_RC, new RefContainerCollection(Factory));
			containerTypeFilter.MultilingualDescription = ResString.GetMultilingualString("18c145bd-0c5a-4589-ba9a-268c4b1f9b15", "Container Type");
			containerTypeFilter.Category = FilterCategories.ModesAndTypes;
			containerTypeFilter.SubGroup = ContainerStockSubGroup;

			ModuleTextFilter isoTypeFilter = collection.AddTextFilter(Descriptions.IsoType, RefContainerSchema.RC_ISOType);
			isoTypeFilter.MultilingualDescription = ResString.GetMultilingualString("c841c3d8-04e4-443c-9d53-40b99238b208", "ISO Type");
			isoTypeFilter.Category = FilterCategories.ModesAndTypes;
			isoTypeFilter.SubGroup = ContainerTypeSubGroup;

			filter = collection.AddTextFilter(Descriptions.MovedAsEmpty, GetMovedAsEmptyFilter, new ContainerManagerFilterStrip.MovedAsEmptyFilter());
			filter.MultilingualDescription = ResString.GetMultilingualString("ce527de1-dbfe-4f14-953e-b933e74771b0", "Moved as Empty");
			filter.Category = FilterCategories.ModesAndTypes;

			filter = collection.AddTextFilter(Descriptions.MovementType, JobContainerMoveSchema.E9_MovementType, Factory.GetCachedValue<ContainerMovementTypes>());
			filter.MultilingualDescription = ResString.GetMultilingualString("1ee4d70e-df61-4114-acd9-a36249dca8d1", "Movement Type");
			filter.Category = FilterCategories.ModesAndTypes;

			filter = collection.AddTextFilter(Descriptions.DetentionableType, GetDetentionableTypeFilter, new DetentionInvoiceType());
			filter.MultilingualDescription = ResString.GetMultilingualString("d52da148-158e-47bd-8549-c82756dafb25", "Triggering Detentions");
			filter.Category = FilterCategories.ModesAndTypes;
		}

		ZQuery GetDetentionableTypeFilter(ZString value)
		{
			return new ZQuery(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.GetMovementCodesForDetention(value));
		}

		ZQuery GetMovedAsEmptyFilter(ZString code)
		{
			switch (code)
			{
				case MovedAsEmptyFilter.IsEmpty:
					return new ZQuery(JobContainerMoveSchema.E9_ContainerIsEmpty, true);

				case MovedAsEmptyFilter.IsNotEmpty:
					return new ZQuery(JobContainerMoveSchema.E9_ContainerIsEmpty, false);

				default:
					return new ZQuery();
			}
		}

		#endregion

		#region AddDateTimeFilters
		static ZQuery GetCreatedTimeFilter(DateComparisonOperator operation, ZDateTime fromTime, ZDateTime toTime)
		{
			switch (operation)
			{
				case DateComparisonOperator.HasDateEntered:
					return new ZQuery();

				case DateComparisonOperator.HasNoDateEntered:
					return ZQuery.NoResultQuery;
			}

			if (fromTime.IsEmpty && toTime.IsEmpty)
			{
				return new ZQuery();
			}

			var result = new ZQuery();

			if (!fromTime.IsEmpty)
			{
				var fromTimeUTC = Env.Time.GetUtcFromLocalTime(fromTime.ToDateTime());
				result.AddToFilter(JobContainerMoveSchema.E9_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, fromTimeUTC);
			}

			if (!toTime.IsEmpty)
			{
				var toTimeUTC = Env.Time.GetUtcFromLocalTime(toTime.ToDateTime());
				result.AddToFilter(JobContainerMoveSchema.E9_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, toTimeUTC);
			}

			return result;
		}

		void AddDateTimeFilters(ModuleFilterCollection collection)
		{
			collection.AddDateFilter(Descriptions.MovementDate, JobContainerMoveSchema.E9_MovementDate).MultilingualDescription = ResString.GetMultilingualString("2def79dc-d87d-43f9-8e2c-eed1bbd5548e", "Movement Date");

			var createdTimeFilter = new ModuleDateFilter(Descriptions.CreatedTime, GetCreatedTimeFilter);
			createdTimeFilter.Category = FilterCategories.AuditInformation;
			createdTimeFilter.MultilingualDescription = ResString.GetMultilingualString("8c689f97-d145-43d7-b077-ea7dcec1e039", "Created Time");

			collection.AddFilter(createdTimeFilter);
		}
		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection collection)
		{
			ModuleFilter depotPortFilter = collection.AddNkFilter(Descriptions.DepotPort, GetDepotPortFilter, ModuleIDs.Location, new LocationCollection(Factory));
			depotPortFilter.MultilingualDescription = ResString.GetMultilingualString("f8a9b3c2-9a7e-4fd8-9cbd-992826210f67", "Depot Port");
			depotPortFilter.Category = FilterCategories.Locations;

			var originFilter = collection.AddNkFilter(Descriptions.Origin, GetOriginFilter, ModuleIDs.Location, new LocationCollection(Factory));
			originFilter.MultilingualDescription = ResString.GetMultilingualString("{52c4fea5-2ffb-4fec-b889-7429a506be73}", "Origin");
			originFilter.Category = FilterCategories.Locations;

			var destinationFilter = collection.AddNkFilter(Descriptions.Destination, GetDestinationFilter, ModuleIDs.Location, new LocationCollection(Factory));
			destinationFilter.MultilingualDescription = ResString.GetMultilingualString("{afcffb84-3f4d-44db-a95d-eeac72a72e39}", "Destination");
			destinationFilter.Category = FilterCategories.Locations;

			var loadPortFilter = collection.AddNkFilter(Descriptions.LoadPort, GetLoadPortFilter, ModuleIDs.Location, new LocationCollection(Factory));
			loadPortFilter.MultilingualDescription = ResString.GetMultilingualString("{a6220130-2642-439d-9afd-a7c91c83393d}", "Load Port");
			loadPortFilter.Category = FilterCategories.Locations;

			var dischargePortFilter = collection.AddNkFilter(Descriptions.DischargePort, GetDischargePortFilter, ModuleIDs.Location, new LocationCollection(Factory));
			dischargePortFilter.MultilingualDescription = ResString.GetMultilingualString("{1f6e9440-655a-4e06-94e6-9cb6a156064e}", "Discharge Port");
			dischargePortFilter.Category = FilterCategories.Locations;
		}

		ZQuery GetDepotPortFilter(ZString location)
		{
			ZDBOnlySubQuery headerFilter = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			headerFilter.AddToFilter(LocationHelper.GetLocationFilter(Factory, location, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader)));

			ZDBOnlyQuery addressFilterFallBack = new ZDBOnlyQuery(typeof(OrgAddress));
			addressFilterFallBack.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, "");
			addressFilterFallBack.AddSubQuery(headerFilter, JoinCondition.And);

			ZDBOnlySubQuery addressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobContainerMoveSchema.E9_OA_Depot);
			addressFilter.AddToFilter(LocationHelper.GetLocationFilter(Factory, location, OrgAddressSchema.OA_RL_NKRelatedPortCode, typeof(OrgAddress)));
			addressFilter.AddToFilter(addressFilterFallBack, JoinCondition.Or);

			ZDBOnlyQuery movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
			movementFilter.AddSubQuery(addressFilter, JoinCondition.And);

			return movementFilter;
		}

		ZQuery GetOriginFilter(ZString location)
		{
			var containerStockFilter = new ZDBOnlySubQuery(typeof(RefContainerStock), RefContainerStockSchema.R6_ContainerNum);
			containerStockFilter.AddToFilter(RefContainerStockSchema.PK, SQLComparisonOperator.Equal, JobContainerMoveSchema.E9_R6);

			var containerNumberFilter = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			containerNumberFilter.AddSubQuery(JobContainerSchema.JC_ContainerNum, containerStockFilter, JoinCondition.And);

			var shipmentFilter = new ZDBOnlySubQuery(typeof(AgencyShipment), JobShipmentSchema.JS_JX);
			shipmentFilter.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, location);
			shipmentFilter.AddSubQuery(containerNumberFilter, JoinCondition.And);

			var sailingFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.JX_JA);
			sailingFilter.AddSubQuery(shipmentFilter, JoinCondition.And);

			var voyOriginFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.JA_JV);
			voyOriginFilter.AddSubQuery(sailingFilter, JoinCondition.And);

			var movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
			movementFilter.AddSubQuery(JobContainerMoveSchema.E9_JV, voyOriginFilter, JoinCondition.And);

			return movementFilter;
		}

		ZQuery GetDestinationFilter(ZString location)
		{
			var containerStockFilter = new ZDBOnlySubQuery(typeof(RefContainerStock), RefContainerStockSchema.R6_ContainerNum);
			containerStockFilter.AddToFilter(RefContainerStockSchema.PK, SQLComparisonOperator.Equal, JobContainerMoveSchema.E9_R6);

			var containerNumberFilter = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			containerNumberFilter.AddSubQuery(JobContainerSchema.JC_ContainerNum, containerStockFilter, JoinCondition.And);

			var shipmentFilter = new ZDBOnlySubQuery(typeof(AgencyShipment), JobShipmentSchema.JS_JX);
			shipmentFilter.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, location);
			shipmentFilter.AddSubQuery(containerNumberFilter, JoinCondition.And);

			var sailingFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.JX_JB);
			sailingFilter.AddSubQuery(shipmentFilter, JoinCondition.And);

			var voyDestinationFilter = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.JB_JV);
			voyDestinationFilter.AddSubQuery(sailingFilter, JoinCondition.And);

			var movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
			movementFilter.AddSubQuery(JobContainerMoveSchema.E9_JV, voyDestinationFilter, JoinCondition.And);

			return movementFilter;
		}

		ZQuery GetLoadPortFilter(ZString location)
		{
			var voyOriginFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.JA_JV);
			voyOriginFilter.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, location);

			var movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
			movementFilter.AddSubQuery(JobContainerMoveSchema.E9_JV, voyOriginFilter, JoinCondition.And);

			return movementFilter;
		}

		ZQuery GetDischargePortFilter(ZString location)
		{
			var voyDestinationFilter = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.JB_JV);
			voyDestinationFilter.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, location);

			var movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
			movementFilter.AddSubQuery(JobContainerMoveSchema.E9_JV, voyDestinationFilter, JoinCondition.And);

			return movementFilter;
		}

		#endregion

		#region AddOrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection collection)
		{
			OrganisationsFindBoxCollection orgsLookup = new OrganisationsFindBoxCollection(Factory);

			ModuleFilter filter = collection.AddGuidFilter(Descriptions.Depot, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, new OrganisationsFindBoxCollection(Factory));
			filter.Category = FilterCategories.Organisations;
			filter.SubGroup = DepotSubGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("01228eee-dc57-475d-a2e8-3151fd57267b", "Depot");

			ModuleFilter principalFilter = collection.AddGuidFilter(Descriptions.Principal, ModuleIDs.Organisation, GetPrincipalFilter, new ShipsAgencyPrincipalCollection(Factory));
			principalFilter.MultilingualDescription = ResString.GetMultilingualString("44d69876-9c69-444f-a9f2-770a560052ce", "Principal");
			principalFilter.Category = FilterCategories.Organisations;

			ModuleFilter clientFilter = collection.AddGuidFilter(Descriptions.Client, ModuleIDs.Organisation, GetClientFilter, orgsLookup);
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("c0e72aa8-13c8-4c0a-ad85-bf413765e01d", "Client");
			clientFilter.Category = FilterCategories.Organisations;

			ModuleFilter ownerFilter = collection.AddGuidFilter(Descriptions.Owner, ModuleIDs.Organisation, RefContainerStockSchema.R6_OH_Owner, orgsLookup);
			ownerFilter.MultilingualDescription = ResString.GetMultilingualString("6da246b6-3bc8-4dd2-b5ce-5396675b3227", "Owner");
			ownerFilter.Category = FilterCategories.Organisations;
			ownerFilter.SubGroup = ContainerStockSubGroup;

			ModuleFilter ownerTypeFilter = collection.AddTextFilter(Descriptions.OwnerType, RefContainerStockSchema.R6_OwnerType, new ContainerOwnershipList());
			ownerTypeFilter.MultilingualDescription = ResString.GetMultilingualString("eba6ef61-5255-44fe-9001-d62cc950df05", "Owner Type");
			ownerTypeFilter.Category = FilterCategories.Organisations;
			ownerTypeFilter.SubGroup = ContainerStockSubGroup;

			var detentionCompanyFilter = collection.AddGuidFilter(Descriptions.DetentionCompany, ModuleIDs.GlbCompany, JobContainerDetentionSchema.NC_GC, new GlbCompanyCollection(Factory));
			detentionCompanyFilter.MultilingualDescription = ResString.GetMultilingualString("f34964d8-ca6d-456d-9b17-f5b91c8e9079", "Detention Company");
			detentionCompanyFilter.Category = FilterCategories.Organisations;
			detentionCompanyFilter.SubGroup = DetentionSubGroup;
			detentionCompanyFilter.SupportsFiltersMatchComparisonOperator = false;
		}

		static ZQuery GetPrincipalFilter(ZGuid principalPK)
		{
			var deliveryAgentFallBackQuery = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.PK);
			deliveryAgentFallBackQuery.AddToFilter(JobContainerMoveSchema.E9_OH_Principal, null);
			deliveryAgentFallBackQuery.AddToFilter(FilterOnShipment(new ZQuery(JobShipmentSchema.JS_OH_DeliveryAgent, principalPK)));

			var principalQuery = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.PK);
			principalQuery.AddToFilter(JobContainerMoveSchema.E9_OH_Principal, principalPK);
			principalQuery.AddAsUnionQuery(deliveryAgentFallBackQuery);

			var result = new ZDBOnlyQuery(typeof(ContainerMovement));
			result.AddSubQuery(principalQuery, JoinCondition.And);

			return result;
		}

		static ZQuery GetClientFilter(ZGuid clientPK)
		{
			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, clientPK);

			var jobHeaderQuery = new ZDBOnlyQuery(typeof(JobHeader));
			jobHeaderQuery.AddSubQuery(addressSubQuery, JoinCondition.And);

			var clientFallBackToJobHeaderQuery = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.PK);
			clientFallBackToJobHeaderQuery.AddToFilter(JobContainerMoveSchema.E9_OH_ResponsibleParty, null);
			clientFallBackToJobHeaderQuery.AddToFilter(FilterOnShipment(FilterOnJobHeader(jobHeaderQuery)));

			var clientQuery = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.PK);
			clientQuery.AddToFilter(JobContainerMoveSchema.E9_OH_ResponsibleParty, clientPK);
			clientQuery.AddAsUnionQuery(clientFallBackToJobHeaderQuery);

			var result = new ZDBOnlyQuery(typeof(ContainerMovement));
			result.AddSubQuery(clientQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region AddStatusAndFlagsFilters

		void AddStatusAndFlagsFilters(ModuleFilterCollection collection)
		{
			collection.AddTextFilter(Descriptions.DetentionInvoiced, GetInvoicedFilter, new InvoicedList()).MultilingualDescription = ResString.GetMultilingualString("6b2b4a12-1cd8-49d9-ac5b-df985e70ce02", "Detention Invoiced");
			collection.AddTextFilter(Descriptions.Detentionable, GetDetentionableFilter, new DetentionableList()).MultilingualDescription = ResString.GetMultilingualString("1c132531-f29e-49d5-932a-17955070c2b1", "Detentionable");
		}

		ZQuery GetInvoicedFilter(ZString status)
		{
			switch (status)
			{
				case InvoicedList.Invoiced:
					return new ZQuery(JobContainerMoveSchema.E9_NC, SQLComparisonOperator.NotEqual, null);

				case InvoicedList.NotInvoiced:
					return new ZQuery(JobContainerMoveSchema.E9_NC, null);

				default:
					return new ZQuery();
			}
		}

		ZQuery GetDetentionableFilter(ZString status)
		{
			switch (status)
			{
				case DetentionableList.Detentionable:
					return new ZQuery(JobContainerMoveSchema.E9_DetentionDays, SQLComparisonOperator.GreaterThan, ZShort.Zero);

				case DetentionableList.NotDetentionable:
					return new ZQuery(JobContainerMoveSchema.E9_DetentionDays, ZShort.Zero);

				default:
					return new ZQuery();
			}
		}

		#endregion

		#region AddVoyageVesselFilters

		void AddVoyageVesseFilters(ModuleFilterCollection filters)
		{
			var voyageVesselFilter = new VoyageVesselModuleFilter(Descriptions.VoyageVessel, GetVoyageVesselFilter, new RefVesselCollection(Factory))
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			voyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("96b13c75-8dba-4461-a538-b4171c030430", "Voyage/Vessel");
			voyageVesselFilter.Category = FilterCategories.NumbersAndReferences;
			voyageVesselFilter.SubGroup = VoyageSubGroup;
			filters.AddCustomFilter(voyageVesselFilter);
		}

		ZQuery GetVoyageVesselFilter(SQLComparisonOperator opp, ZString voyage, ZString vessel, ZBool includeArchived)
		{
			return VoyageVesselModuleFilterHelper.GetBasicVoyageVesselQuery(opp, voyage, vessel, includeArchived, JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
		}

		#endregion

		#region Filter Processors

		ModuleFilterSubGroup ShipmentSubGroup
		{
			get { return shipmentSubGroup ?? (shipmentSubGroup = new ShipmentFilterSubGroup()); }
		}
		ModuleFilterSubGroup shipmentSubGroup;

		ModuleFilterSubGroup VoyageSubGroup
		{
			get { return voyageSubGroup ?? (voyageSubGroup = new VoyageFilterSubGroup()); }
		}
		ModuleFilterSubGroup voyageSubGroup;

		ModuleFilterSubGroup ContainerStockSubGroup
		{
			get { return containerStockSubGroup ?? (containerStockSubGroup = new StockFilterSubGroup()); }
		}
		ModuleFilterSubGroup containerStockSubGroup;

		ModuleFilterSubGroup ContainerTypeSubGroup
		{
			get { return containerTypeSubGroup ?? (containerTypeSubGroup = new ContainerTypeFilterSubGroup(ContainerStockSubGroup)); }
		}
		ModuleFilterSubGroup containerTypeSubGroup;

		ModuleFilterSubGroup DetentionSubGroup
		{
			get { return detentionSubGroup ?? (detentionSubGroup = new DetentionFilterSubGroup()); }
		}
		ModuleFilterSubGroup detentionSubGroup;

		ModuleFilterSubGroup DepotSubGroup
		{
			get { return depotSubGroup ?? (depotSubGroup = new DepotFilterSubGroup()); }
		}
		ModuleFilterSubGroup depotSubGroup;

		class ShipmentFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return FilterOnShipment(filter);
			}
		}

		class VoyageFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery voyageFilter = new ZDBOnlySubQuery(typeof(JobVoyage), JobContainerMoveSchema.E9_JV);
				voyageFilter.AddToFilter(filter);

				ZDBOnlyQuery movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
				movementFilter.AddSubQuery(voyageFilter, JoinCondition.And);

				return movementFilter;
			}
		}

		class StockFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery stockFilter = new ZDBOnlySubQuery(typeof(RefContainerStock), JobContainerMoveSchema.E9_R6);
				stockFilter.AddToFilter(filter);

				ZDBOnlyQuery movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
				movementFilter.AddSubQuery(stockFilter, JoinCondition.And);

				return movementFilter;
			}
		}

		class ContainerTypeFilterSubGroup : ModuleFilterSubGroup
		{
			public ContainerTypeFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent) { }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery typeFilter = new ZDBOnlySubQuery(typeof(RefContainer), RefContainerStockSchema.R6_RC);
				typeFilter.AddToFilter(filter);

				ZDBOnlyQuery stockFilter = new ZDBOnlyQuery(typeof(RefContainerStock));
				stockFilter.AddSubQuery(typeFilter, JoinCondition.And);

				return stockFilter;
			}
		}

		class DetentionFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery detentionFilter = new ZDBOnlySubQuery(typeof(ContainerDetention), JobContainerMoveSchema.E9_NC);
				detentionFilter.AddToFilter(filter);

				ZDBOnlyQuery movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
				movementFilter.AddSubQuery(detentionFilter, JoinCondition.And);

				return movementFilter;
			}
		}

		class DepotFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var address = new ZDBOnlySubQuery(typeof(OrgAddress), JobContainerMoveSchema.E9_OA_Depot);
				address.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(ContainerMovement));
				result.AddSubQuery(address, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region Implementation

		static ZQuery FilterOnJobHeader(ZQuery headerFilter)
		{
			ZDBOnlySubQuery header = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			header.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			header.AddToFilter(headerFilter);

			ZDBOnlyQuery shipment = new ZDBOnlyQuery(typeof(AgencyShipment));
			shipment.AddSubQuery(header, JoinCondition.And);

			return shipment;
		}

		static ZQuery FilterOnShipment(ZQuery shipmentFilter)
		{
			ZDBOnlySubQuery stock = new ZDBOnlySubQuery(typeof(RefContainerStock), RefContainerStockSchema.R6_ContainerNum);
			stock.AddToFilter(RefContainerStockSchema.PK, SQLComparisonOperator.Equal, JobContainerMoveSchema.E9_R6);

			ZDBOnlySubQuery container = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			container.AddSubQuery(JobContainerSchema.JC_ContainerNum, stock, JoinCondition.And);

			ZDBOnlySubQuery shipment = new ZDBOnlySubQuery(typeof(AgencyShipment), JobShipmentSchema.JS_JX);
			shipment.AddToFilter(shipmentFilter);
			shipment.AddSubQuery(container, JoinCondition.And);

			ZDBOnlySubQuery sailing = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.JX_JA);
			sailing.AddSubQuery(shipment, JoinCondition.And);

			ZDBOnlySubQuery origins = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.JA_JV);
			origins.AddSubQuery(sailing, JoinCondition.And);

			ZDBOnlyQuery movement = new ZDBOnlyQuery(typeof(ContainerMovement));
			movement.AddSubQuery(JobContainerMoveSchema.E9_JV, origins, JoinCondition.And);

			return movement;
		}

		public class InvoicedList : CodeDescriptionPairList
		{
			public const string Invoiced = "INV";
			public const string NotInvoiced = "NIV";

			public InvoicedList()
			{
				AddPair("", ResString.GetMultilingualString("8315bb6d-9ed3-4bc4-bccc-1b253940ef4d", "All"));
				AddPair(Invoiced, ResString.GetMultilingualString("663f392c-28ad-4789-ba6f-f9c993683988", "Invoiced"));
				AddPair(NotInvoiced, ResString.GetMultilingualString("434dc7b4-40e0-483c-b7dc-6b3cd678f53d", "Not Invoiced"));
			}
		}

		public class DetentionableList : CodeDescriptionPairList
		{
			public const string Detentionable = "DET";
			public const string NotDetentionable = "NDT";

			public DetentionableList()
			{
				AddPair("", ResString.GetMultilingualString("8315bb6d-9ed3-4bc4-bccc-1b253940ef4d", "All"));
				AddPair(Detentionable, ResString.GetMultilingualString("ac1d54fe-3a25-4cac-8e8f-dd98e3fd188f", "Movements with detention days."));
				AddPair(NotDetentionable, ResString.GetMultilingualString("8cebadf0-a906-41f5-a60d-c8c99211c36b", "Movements without detention days."));
			}
		}

		public class MovedAsEmptyFilter : CodeDescriptionPairList
		{
			public const string IsEmpty = "EMP";
			public const string IsNotEmpty = "NOT";

			public MovedAsEmptyFilter()
			{
				AddPair(IsEmpty, (NoResString)"Empty"); // Filter Strip Constant
				AddPair(IsNotEmpty, (NoResString)"Not Empty"); // Filter Strip Constant
			}
		}

		#endregion
	}
}



