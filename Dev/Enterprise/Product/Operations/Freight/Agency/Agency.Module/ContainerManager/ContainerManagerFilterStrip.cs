using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
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
	public class ContainerManagerFilterStrip : FilterStripBusinessObject
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Strip Constant")]
		public static class Descriptions
		{
			// Numbers & Refs
			public const string BillOfLading = "Bill of Lading";
			public const string ContainerNumber = "Container #";
			public const string ShipmentNumber = "Shipment #";
			public const string LastLeaseContractNo = "Last Lease Contract #";

			// Dates
			public const string MovementDate = "Movement Date";

			// Locations
			public const string DepotPort = "Depot Port";
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string LoadPort = "Load Port";
			public const string DischargePort = "Discharge Port";

			// Organisations / Staff
			public const string Owner = "Owner";
			public const string OwnerType = "Owner Type";
			public const string Depot = "Depot";

			// Modes And Types
			public const string ContainerCondition = "Container Condition";
			public const string ContainerType = "Container Type";
			public const string ContainerQuality = "Container Quality";
			public const string IsoType = "ISO Type";
			public const string MovedAsEmpty = "Moved as Empty";
			public const string MovementType = "Movement Type";

			//Text Search
			public const string VoyageVessel = "Voyage/Vessel";

			// Status And Flags
			public const string LocationCategory = "Location Category";
			public const string MovementFlags = "Movement Flags";
			public static string MovementFlags_LastMovement { get { return Res.GetString("89d4dd8c-6c6d-45f1-90d1-3c7462a41949", "Last Movement"); } }
		}

		#endregion

		#region SubGroups

		ModuleFilterSubGroup ContainerMovementsFilterProcessor => containerMovementsFilterProcessor ?? (containerMovementsFilterProcessor = new ContainerMovementProcessor());
		ContainerMovementProcessor containerMovementsFilterProcessor;

		ModuleFilterSubGroup ShipmentFilterProcessor => shipmentFilterProcessor ?? (shipmentFilterProcessor = new ShipmentSubGroup());
		ShipmentSubGroup shipmentFilterProcessor;

		ModuleFilterSubGroup RefContainerProcessor => refContainerProcessor ?? (refContainerProcessor = new RefContainerSubGroup());
		RefContainerSubGroup refContainerProcessor;

		ModuleFilterSubGroup LastContainerMovementProcessor => lastContainerMovementProcessor ?? (lastContainerMovementProcessor = new LastContainerMovementFilterSubGroup());
		ModuleFilterSubGroup lastContainerMovementProcessor;

		class ContainerMovementProcessor : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return FilterOnContainerMove(filter);
			}
		}

		class ShipmentSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery jobShipment = new ZDBOnlySubQuery(typeof(AgencyShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				jobShipment.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
				jobShipment.AddToFilter(filter);

				ZDBOnlySubQuery jobContainer = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_ContainerNum);
				jobContainer.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);
				jobContainer.AddSubQuery(jobShipment, JoinCondition.And);

				ZDBOnlyQuery refStock = new ZDBOnlyQuery(typeof(RefContainerStock));
				refStock.AddSubQuery(RefContainerStockSchema.R6_ContainerNum, jobContainer, JoinCondition.And);

				return refStock;
			}
		}

		class RefContainerSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery typeFilter = new ZDBOnlySubQuery(typeof(RefContainer), RefContainerStockSchema.R6_RC);
				typeFilter.AddToFilter(filter);

				ZDBOnlyQuery stockFilter = new ZDBOnlyQuery(typeof(RefContainerStock));
				stockFilter.AddSubQuery(typeFilter, JoinCondition.And);

				return stockFilter;
			}
		}

		class LastContainerMovementFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var sql = @"
E9_PK IN
(
	SELECT E9_PK
	FROM
	(
		SELECT
			E9_PK,
			ROW_NUMBER() OVER(PARTITION BY E9_R6 ORDER BY E9_MovementDate DESC) AS RowNumber
		FROM dbo.JobContainerMove
		WHERE E9_MovementDate IS NOT NULL
	) AS LastMovement
	WHERE RowNumber = 1
)"; // Part of SQL expression

				var lastMovementFilter = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.E9_R6);
				lastMovementFilter.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());
				lastMovementFilter.AddToFilter(filter);

				var stockFilter = new ZDBOnlyQuery(typeof(RefContainerStock));
				stockFilter.AddSubQuery(lastMovementFilter, JoinCondition.And);
				return stockFilter;
			}
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleNumberFilter(Descriptions.ContainerNumber, RefContainerStockSchema.R6_ContainerNum) { MultilingualDescription = ResString.GetMultilingualString("baeb7685-d2a5-4b07-a6a2-d5aa937a296c", "Container #") };
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberAndRefFilters(filters);
			AddModeAndTypeFilters(filters);
			AddDateTimeFilters(filters);
			AddLocationFilters(filters);
			AddOrganisationFilters(filters);
			AddVoyageVesselFilters(filters);
			AddStatusAndFlagsFilters(filters);
			return filters;
		}

		#region AddNumberAndRefFilters

		void AddNumberAndRefFilters(ModuleFilterCollection filters)
		{
			var billOfLadingFilter = filters.AddNumberFilter(Descriptions.BillOfLading, JobShipmentSchema.JS_HouseBill);
			billOfLadingFilter.MultilingualDescription = ResString.GetMultilingualString("064bac10-0b2c-490d-b567-e1cfed8ff4dc", "Bill of Lading");
			billOfLadingFilter.SubGroup = ShipmentFilterProcessor;

			var shipmentNumberFilter = filters.AddFountainFilter(Descriptions.ShipmentNumber, JobShipmentSchema.JS_UniqueConsignRef, "V");
			shipmentNumberFilter.MultilingualDescription = ResString.GetMultilingualString("0d0ca14d-c15a-4337-9db3-d7c523a1a4a9", "Shipment #");
			shipmentNumberFilter.SubGroup = ShipmentFilterProcessor;

			var leaseNumberFilter = filters.AddNumberFilter(Descriptions.LastLeaseContractNo, JobContainerMoveSchema.E9_LeaseNumber);
			leaseNumberFilter.MultilingualDescription = ResString.GetMultilingualString("3f9c95a5-4a8f-43d1-ad7a-9d1bcefd5576", "Last Lease Contract #");
			leaseNumberFilter.SubGroup = LastContainerMovementProcessor;
		}

		#endregion

		#region AddDateTimeFilters

		void AddDateTimeFilters(ModuleFilterCollection filters)
		{
			ModuleDateFilter filter = filters.AddDateFilter(Descriptions.MovementDate, JobContainerMoveSchema.E9_MovementDate);
			filter.MultilingualDescription = ResString.GetMultilingualString("4609c22d-5e64-4550-b716-e93323453744", "Movement Date");
			filter.SubGroup = ContainerMovementsFilterProcessor;
		}

		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var depotPortFilter = filters.AddNkFilter(ContainerMoveFilterStrip.Descriptions.DepotPort, GetDepotPortFilter, ModuleIDs.Location, new LocationCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(OrgAddressSchema.OA_RL_NKRelatedPortCode);
			depotPortFilter.MultilingualDescription = ResString.GetMultilingualString("bf108697-5274-43ff-81fd-0f268b520e0f", "Depot Port");
			depotPortFilter.Category = FilterCategories.Locations;
			depotPortFilter.SubGroup = ContainerMovementsFilterProcessor;

			var originFilter = filters.AddNkFilter(ContainerMoveFilterStrip.Descriptions.Origin, GetOriginFilter, ModuleIDs.Location, new LocationCollection(Factory));
			originFilter.MultilingualDescription = ResString.GetMultilingualString("7EC678EE-256A-4228-B247-36717E38D440", "Origin");
			originFilter.Category = FilterCategories.Locations;

			var destinationFilter = filters.AddNkFilter(ContainerMoveFilterStrip.Descriptions.Destination, GetDestinationFilter, ModuleIDs.Location, new LocationCollection(Factory));
			destinationFilter.MultilingualDescription = ResString.GetMultilingualString("72b58130-cd75-4ada-a20b-4ea1c8e857cf", "Destination");
			destinationFilter.Category = FilterCategories.Locations;

			var loadPortFilter = filters.AddNkFilter(ContainerMoveFilterStrip.Descriptions.LoadPort, GetLoadPortFilter, ModuleIDs.Location, new LocationCollection(Factory));
			loadPortFilter.MultilingualDescription = ResString.GetMultilingualString("93ae1272-8ed5-4846-b6c7-2bff62a370c3", "Load Port");
			loadPortFilter.Category = FilterCategories.Locations;

			var dischargePortFilter = filters.AddNkFilter(ContainerMoveFilterStrip.Descriptions.DischargePort, GetDischargePortFilter, ModuleIDs.Location, new LocationCollection(Factory));
			dischargePortFilter.MultilingualDescription = ResString.GetMultilingualString("ecb35cb6-9810-4024-9688-c311070782fe", "Discharge Port");
			dischargePortFilter.Category = FilterCategories.Locations;
		}

		ZQuery GetDepotPortFilter(ZString location)
		{
			ZDBOnlySubQuery headerFilter = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			headerFilter.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, location);

			ZDBOnlyQuery addressFilterFallBack = new ZDBOnlyQuery(typeof(OrgAddress));
			addressFilterFallBack.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, "");
			addressFilterFallBack.AddSubQuery(headerFilter, JoinCondition.And);

			ZDBOnlySubQuery addressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobContainerMoveSchema.E9_OA_Depot);
			addressFilter.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, location);
			addressFilter.AddToFilter(addressFilterFallBack, JoinCondition.Or);

			ZDBOnlyQuery movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
			movementFilter.AddSubQuery(addressFilter, JoinCondition.And);

			return movementFilter;
		}

		ZQuery GetOriginFilter(ZString location)
		{
			var containerNumberFilter = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			containerNumberFilter.AddToFilter(JobContainerSchema.JC_ContainerNum, SQLComparisonOperator.Equal, RefContainerStockSchema.R6_ContainerNum);

			var shipmentFilter = new ZDBOnlySubQuery(typeof(AgencyShipment), JobShipmentSchema.JS_JX);
			shipmentFilter.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, location);
			shipmentFilter.AddSubQuery(containerNumberFilter, JoinCondition.And);

			var sailingFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.JX_JA);
			sailingFilter.AddSubQuery(shipmentFilter, JoinCondition.And);

			var voyOriginFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.JA_JV);
			voyOriginFilter.AddSubQuery(sailingFilter, JoinCondition.And);

			var movementFilter = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.E9_R6);
			movementFilter.AddSubQuery(JobContainerMoveSchema.E9_JV, voyOriginFilter, JoinCondition.And);

			var containerFilter = new ZDBOnlyQuery(typeof(RefContainerStock));
			containerFilter.AddSubQuery(movementFilter, JoinCondition.And);

			return containerFilter;
		}

		ZQuery GetDestinationFilter(ZString location)
		{
			var containerNumberFilter = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			containerNumberFilter.AddToFilter(JobContainerSchema.JC_ContainerNum, SQLComparisonOperator.Equal, RefContainerStockSchema.R6_ContainerNum);

			var shipmentFilter = new ZDBOnlySubQuery(typeof(AgencyShipment), JobShipmentSchema.JS_JX);
			shipmentFilter.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, location);
			shipmentFilter.AddSubQuery(containerNumberFilter, JoinCondition.And);

			var sailingFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.JX_JB);
			sailingFilter.AddSubQuery(shipmentFilter, JoinCondition.And);

			var voyDestinationFilter = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.JB_JV);
			voyDestinationFilter.AddSubQuery(sailingFilter, JoinCondition.And);

			var movementFilter = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.E9_R6);
			movementFilter.AddSubQuery(JobContainerMoveSchema.E9_JV, voyDestinationFilter, JoinCondition.And);

			var containerFilter = new ZDBOnlyQuery(typeof(RefContainerStock));
			containerFilter.AddSubQuery(movementFilter, JoinCondition.And);

			return containerFilter;
		}

		ZQuery GetLoadPortFilter(ZString location)
		{
			var voyOriginFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.JA_JV);
			voyOriginFilter.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, location);

			var movementFilter = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.E9_R6);
			movementFilter.AddSubQuery(JobContainerMoveSchema.E9_JV, voyOriginFilter, JoinCondition.And);

			var containerNumFilter = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_ContainerNum);

			var containerFilter = new ZDBOnlyQuery(typeof(RefContainerStock));
			containerFilter.AddSubQuery(movementFilter, JoinCondition.And);
			containerFilter.AddSubQuery(RefContainerStockSchema.R6_ContainerNum, containerNumFilter, JoinCondition.And);

			return containerFilter;
		}

		ZQuery GetDischargePortFilter(ZString location)
		{
			var voyDestinationFilter = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.JB_JV);
			voyDestinationFilter.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, location);

			var movementFilter = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.E9_R6);
			movementFilter.AddSubQuery(JobContainerMoveSchema.E9_JV, voyDestinationFilter, JoinCondition.And);

			var containerNumFilter = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_ContainerNum);

			var containerFilter = new ZDBOnlyQuery(typeof(RefContainerStock));
			containerFilter.AddSubQuery(movementFilter, JoinCondition.And);
			containerFilter.AddSubQuery(RefContainerStockSchema.R6_ContainerNum, containerNumFilter, JoinCondition.And);

			return containerFilter;
		}

		#endregion

		#region AddOrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			OrganisationsFindBoxCollection orgs = new OrganisationsFindBoxCollection(Factory);

			ModuleFilter filter = filters.AddGuidFilter(Descriptions.Owner, ModuleIDs.Organisation, RefContainerStockSchema.R6_OH_Owner, orgs);
			filter.MultilingualDescription = ResString.GetMultilingualString("3e76f34f-7f66-40e1-9437-ddc8e80d2030", "Owner");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter(Descriptions.OwnerType, RefContainerStockSchema.R6_OwnerType, new ContainerOwnershipList());
			filter.MultilingualDescription = ResString.GetMultilingualString("26f6624e-9318-4032-a9d6-3df5b25d8e81", "Owner Type");
			filter.Category = FilterCategories.Organisations;

			ModuleGuidFilter depotFilter = filters.AddGuidFilter(Descriptions.Depot, ModuleIDs.Organisation, GetDepotFilter, orgs);
			depotFilter.MultilingualDescription = ResString.GetMultilingualString("6dd54bd4-339e-4dfe-8665-41744cc1a5ed", "Depot");
			depotFilter.Category = FilterCategories.Organisations;
			depotFilter.SubGroup = ContainerMovementsFilterProcessor;
		}

		ZQuery GetDepotFilter(ZGuid depot)
		{
			ZDBOnlySubQuery address = new ZDBOnlySubQuery(typeof(OrgAddress), JobContainerMoveSchema.E9_OA_Depot);
			address.AddToFilter(OrgAddressSchema.OA_OH, depot);

			ZDBOnlyQuery movement = new ZDBOnlyQuery(typeof(ContainerMovement));
			movement.AddSubQuery(address, JoinCondition.And);

			return movement;
		}

		#endregion

		#region AddVoyageVesselFilters

		void AddVoyageVesselFilters(ModuleFilterCollection filters)
		{
			var voyageVesselFilter = new VoyageVesselModuleFilter(Descriptions.VoyageVessel, GetVoyageVesselFilter, new RefVesselCollection(Factory))
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			voyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("c7ba649f-adf8-4f31-a931-f70f37744025", "Voyage/Vessel");
			voyageVesselFilter.Category = FilterCategories.NumbersAndReferences;
			voyageVesselFilter.SubGroup = ContainerMovementsFilterProcessor;
			filters.AddCustomFilter(voyageVesselFilter);
		}

		ZQuery GetVoyageVesselFilter(SQLComparisonOperator opp, ZString voyage, ZString vessel, ZBool includeArchived)
		{
			var query = VoyageVesselModuleFilterHelper.GetBasicVoyageVesselQuery(opp, voyage, vessel, includeArchived, JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);

			return FromVoyageFilter(query);
		}

		#endregion

		#region AddModeAndTypeFilters

		void AddModeAndTypeFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddGuidFilter(Descriptions.ContainerType, ModuleIDs.RefContainer, RefContainerStockSchema.R6_RC, new RefContainerCollection(Factory, Core.Constants.TransportModes.Sea));
			filter.MultilingualDescription = ResString.GetMultilingualString("ba7bf7d0-d471-4f71-850c-251622e92838", "Container Type");
			filter.Category = FilterCategories.ModesAndTypes;

			filter = filters.AddTextFilter(Descriptions.IsoType, RefContainerSchema.RC_ISOType);
			filter.SubGroup = RefContainerProcessor;
			filter.MultilingualDescription = ResString.GetMultilingualString("af4b8008-44ff-4c18-9d47-a3427ca52494", "ISO Type");
			filter.Category = FilterCategories.ModesAndTypes;

			ModuleTextFilter containerConditionFilter = filters.AddTextFilter(Descriptions.ContainerCondition, JobContainerMoveSchema.E9_ContainerCondition, AgencyRegistry.Instance.ContainerDamageCodes.Value);
			containerConditionFilter.MultilingualDescription = ResString.GetMultilingualString("46d90560-a231-41ff-be65-2a628bef1d8b", "Container Condition");
			containerConditionFilter.Category = FilterCategories.ModesAndTypes;
			containerConditionFilter.SubGroup = ContainerMovementsFilterProcessor;

			ModuleTextFilter containerQualityFilter = filters.AddTextFilter(Descriptions.ContainerQuality, JobContainerMoveSchema.E9_ContainerQuality, AgencyRegistry.Instance.ContainerCleanCodes.Value);
			containerQualityFilter.MultilingualDescription = ResString.GetMultilingualString("184bf96e-2975-443a-b838-fd98ebbadf91", "Container Quality");
			containerQualityFilter.Category = FilterCategories.ModesAndTypes;
			containerQualityFilter.SubGroup = ContainerMovementsFilterProcessor;

			ModuleTextFilter movementTypeFilter = filters.AddTextFilter(Descriptions.MovementType, JobContainerMoveSchema.E9_MovementType, Factory.GetCachedValue<ContainerMovementTypes>());
			movementTypeFilter.MultilingualDescription = ResString.GetMultilingualString("d5ca87ff-3224-4a9d-b63c-5e220885a330", "Movement Type");
			movementTypeFilter.Category = FilterCategories.ModesAndTypes;
			movementTypeFilter.SubGroup = ContainerMovementsFilterProcessor;

			ModuleTextFilter containerEmptyFilter = filters.AddTextFilter(Descriptions.MovedAsEmpty, GetMovedAsEmptyFilter, new MovedAsEmptyFilter());
			containerEmptyFilter.MultilingualDescription = ResString.GetMultilingualString("b8955b03-b8fd-4b09-b0ce-1b177a85e941", "Moved as Empty");
			containerEmptyFilter.Category = FilterCategories.ModesAndTypes;
			containerEmptyFilter.SubGroup = ContainerMovementsFilterProcessor;
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

		#region AddStatusAndFlagsFilters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter locationCategoryFilter = filters.AddTextFilter(Descriptions.LocationCategory, LocationCategoryFilter, Factory.GetCachedValue<ContainerLocationCategoryList>());
			locationCategoryFilter.MultilingualDescription = ResString.GetMultilingualString("9b75e2c7-a950-4758-abde-e4e2b570b9b6", "Location Category");
			locationCategoryFilter.Category = FilterCategories.StatusAndFlags;

			ModuleFlagsFilter movementFlagsFilter = filters.AddFlagsFilter(Descriptions.MovementFlags,
				new string[] { Descriptions.MovementFlags_LastMovement },
				new GetFlagsQuery[] { GetLastMovementFilter });
			movementFlagsFilter.MultilingualDescription = ResString.GetMultilingualString("7c5a8b53-27d9-44f1-b193-19249ae73db0", "Movement Flags");
			movementFlagsFilter.SubGroup = ContainerMovementsFilterProcessor;
		}

		ZQuery GetLastMovementFilter(ZBool value)
		{
			const string sqlFormat =
				"{1} in " +
				"(" +
					"select {1} " +
					"from {0} " +
					"join " +
					"(" +
						"select {2} StockPK, max({3}) LastMoveDate " +
						"from {0} " +
						"where {3} is not null " +
						"group by {2} " +
					") LastContainerMove on {2} = StockPK and {3} = LastMoveDate " +
				")" +
				"";

			if (value)
			{
				string sql = string.Format(CultureInfo.InvariantCulture, sqlFormat,
					JobContainerMoveSchema.Constants.TableName,
					JobContainerMoveSchema.Constants.PK,
					JobContainerMoveSchema.Constants.E9_R6,
					JobContainerMoveSchema.Constants.E9_MovementDate
				);

				ZDBOnlyQuery lastMovementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
				lastMovementFilter.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());

				return lastMovementFilter;
			}
			else
			{
				return new ZQuery();
			}
		}

		ZQuery LocationCategoryFilter(ZString code)
		{
			if (code.EqualsIgnoringCase(ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment))
			{
				string sqlFormat =
	$@"{RefContainerStockSchema.Constants.PK} IN
			(SELECT ContainerPK
			FROM
			(Select 
					LastMovements.ContainerPK,
					LastMovements.DepotPort,
					BookedContainers.BookingPK
			From
					(
						Select 
								{JobContainerMoveSchema.Constants.E9_R6} AS ContainerPK,
								{JobContainerMoveSchema.Constants.E9_JV} AS VoyagePK,
								{RefContainerStockSchema.Constants.R6_ContainerNum} AS ContainerNumber,
								Depot.DepotPort
						From
								{RefContainerStockSchema.Constants.SqlSchemaName}.{RefContainerStockSchema.Constants.TableName}
									INNER JOIN
								{JobContainerMoveSchema.Constants.SqlSchemaName}.{JobContainerMoveSchema.Constants.TableName}
									ON {RefContainerStockSchema.Constants.PK} = {JobContainerMoveSchema.Constants.E9_R6}
									INNER JOIN
								(
									SELECT
										{JobContainerMoveSchema.Constants.E9_R6} AS StockPK, 
										MAX({JobContainerMoveSchema.Constants.E9_MovementDate}) AS LastMoveDate 
									FROM {JobContainerMoveSchema.Constants.SqlSchemaName}.{JobContainerMoveSchema.Constants.TableName} 
									WHERE {JobContainerMoveSchema.Constants.E9_MovementDate} IS NOT NULL 
									Group BY {JobContainerMoveSchema.Constants.E9_R6}
								) LastContainerMove 
									ON
										{JobContainerMoveSchema.Constants.E9_R6} = StockPK
											AND
										{JobContainerMoveSchema.Constants.E9_MovementDate} = LastMoveDate
											AND
										{JobContainerMoveSchema.Constants.E9_MovementType} = 'DIS'
									INNER JOIN
								(
									SELECT 
										{OrgAddressSchema.Constants.PK} AS DepotAddress,
										CASE WHEN {OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode} = ''
										THEN {OrgHeaderSchema.Constants.OH_RL_NKClosestPort} 
										ELSE {OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode}
										END AS DepotPort
									FROM
										{OrgAddressSchema.Constants.SqlSchemaName}.{OrgAddressSchema.Constants.TableName}
												INNER JOIN
										{OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName}
												ON {OrgAddressSchema.Constants.OA_OH} = {OrgHeaderSchema.Constants.PK}
								) Depot
									ON {JobContainerMoveSchema.Constants.E9_OA_Depot} = Depot.DepotAddress
					) LastMovements
				INNER JOIN
					(
							SELECT 
									{JobContainerSchema.Constants.JC_ContainerNum} AS ContainerNumber,
									{JobVoyageSchema.Constants.PK} AS VoyagePK,
									{JobShipmentSchema.Constants.PK} AS BookingPK
							FROM
									{JobContainerSchema.Constants.SqlSchemaName}.{JobContainerSchema.Constants.TableName}
										INNER JOIN
									{JobShipmentSchema.Constants.SqlSchemaName}.{JobShipmentSchema.Constants.TableName}
										On {JobShipmentSchema.Constants.PK} = {JobContainerSchema.Constants.JC_JS_FCLBookingOnlyLink}
										INNER JOIN
									{JobSailingSchema.Constants.SqlSchemaName}.{JobSailingSchema.Constants.TableName}
										On {JobSailingSchema.Constants.PK} = {JobShipmentSchema.Constants.JS_JX}
										INNER JOIN
									{JobVoyOriginSchema.Constants.SqlSchemaName}.{JobVoyOriginSchema.Constants.TableName}
										On {JobVoyOriginSchema.Constants.PK} = {JobSailingSchema.Constants.JX_JA}
										INNER JOIN
									{JobVoyageSchema.Constants.SqlSchemaName}.{JobVoyageSchema.Constants.TableName}
										On {JobVoyageSchema.Constants.PK} = {JobVoyOriginSchema.Constants.JA_JV}
							WHERE {JobContainerSchema.Constants.JC_Purpose} = 'REL'  AND {JobShipmentSchema.Constants.JS_IsCancelled} = 0 AND {JobVoyageSchema.Constants.JV_IsActive} = 1
					) BookedContainers
				ON
					LastMovements.ContainerNumber = BookedContainers.ContainerNumber
						AND
					LastMovements.VoyagePK = BookedContainers.VoyagePK) DischargedBookedContainers
			INNER JOIN
				{JobConsolTransportSchema.Constants.SqlSchemaName}.{JobConsolTransportSchema.Constants.TableName}
					ON
						DischargedBookedContainers.BookingPK = {JobConsolTransportSchema.Constants.JW_ParentGUID}
							AND
						{JobConsolTransportSchema.Constants.JW_TransportMode} = 'SEA'
							AND
						DischargedBookedContainers.DepotPort = {JobConsolTransportSchema.Constants.JW_RL_NKLoadPort}
			Group By ContainerPK)";

				var containers = new ZDBOnlyQuery(typeof(RefContainerStock));
				containers.AddFilterAndZSQLParameterCollection(sqlFormat, new ZSqlParameterCollection());
				return containers;
			}

			var movementFilter = new ZQuery();
			movementFilter.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.GetMovementCodesLeadingToLocation(code));
			movementFilter.AddToFilter(GetLastMovementFilter(true));
			return FilterOnContainerMove(movementFilter);
		}

		#endregion

		#region Implementation

		static ZQuery FromVoyageFilter(ZQuery voyageFilter)
		{
			ZDBOnlySubQuery jobVoyage = new ZDBOnlySubQuery(typeof(JobVoyage), JobContainerMoveSchema.E9_JV);
			jobVoyage.AddToFilter(voyageFilter);

			ZDBOnlyQuery jobContainerMovement = new ZDBOnlyQuery(typeof(ContainerMovement));
			jobContainerMovement.AddSubQuery(jobVoyage, JoinCondition.And);

			return jobContainerMovement;
		}

		static ZQuery FilterOnContainerMove(ZQuery movementFilter)
		{
			ZDBOnlySubQuery movement = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.E9_R6);
			movement.AddToFilter(movementFilter);

			ZDBOnlyQuery container = new ZDBOnlyQuery(typeof(RefContainerStock));
			container.AddSubQuery(movement, JoinCondition.And);

			return container;
		}

		public class MovedAsEmptyFilter : CodeDescriptionPairList
		{
			public const string IsEmpty = "EMP";
			public const string IsNotEmpty = "NOT";

			public MovedAsEmptyFilter()
			{
				AddPair(IsEmpty, ResString.GetMultilingualString("dcab10a5-bd7e-4772-82a8-b9421d5c175f", "Empty"));
				AddPair(IsNotEmpty, ResString.GetMultilingualString("4721410c-f14f-4b9a-909b-c24b6222bfcc", "Not Empty"));
			}
		}

		#endregion
	}
}


