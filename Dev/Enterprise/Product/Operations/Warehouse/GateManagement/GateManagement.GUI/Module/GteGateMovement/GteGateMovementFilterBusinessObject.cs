using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteGateMovementFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string TransportReference = nameof(TransportReference);
			public const string UnitNumber = nameof(UnitNumber);
			public const string CargoType = nameof(CargoType);
			public const string PackageType = nameof(PackageType);
			public const string UnitType = nameof(UnitType);

			public const string GateInNumber = nameof(GateInNumber);
			public const string EntryTime = nameof(EntryTime);

			public const string MovementBookingNumber = nameof(MovementBookingNumber);
			public const string InstructionReference = nameof(InstructionReference);
			public const string BookingPartyReference = nameof(BookingPartyReference);
			public const string BookingNumber = nameof(BookingNumber);

			public const string Facility = nameof(Facility);
		}

		public override SchemaGuidColumn PKSchemaColumn => GteGateMovementSchema.PK;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddNumberAndReferenceFilters(filters);
			AddDateFilters(filters);
			AddGuidAndNKFilters(filters);

			return filters;
		}

		#region Filters

		void AddNumberAndReferenceFilters(ModuleFilterCollection filters)
		{
			var transportReferenceFilter = filters.AddNumberFilter(Schema.TransportReference, GteGateMovementSchema.GGM_TransportReference);
			transportReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|TransportReference", "Transport Reference");

			var unitNumberFilter = filters.AddNumberFilter(Schema.UnitNumber, GteGateMovementSchema.GGM_UnitNumber);
			unitNumberFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|UnitNumber", "Unit Number");

			var gateInNumberFilter = filters.AddNumberFilter(Schema.GateInNumber, GetGateInNumberQuery);
			gateInNumberFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|GateInNumber", "Gate-In Number");

			var movementBookingNumberFilter = filters.AddNumberFilter(Schema.MovementBookingNumber, GetMovementBookingNumberQuery);
			movementBookingNumberFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|MovementBookingNumber", "Movement Booking Number");

			var instructionReferenceFilter = filters.AddNumberFilter(Schema.InstructionReference, GetInstructionReferenceQuery);
			instructionReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|InstructionReference", "Instruction Reference");

			var bookingPartyReferenceFilter = filters.AddNumberFilter(Schema.BookingPartyReference, GetBookingPartyReferenceQuery);
			bookingPartyReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|BookingPartyReference", "Booking Party Reference");

			var bookingNumberFilter = filters.AddNumberFilter(Schema.BookingNumber, GetBookingNumberQuery);
			bookingNumberFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|BookingNumber", "Booking Number");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var entryTimeFilter = filters.AddDateFilter(Schema.EntryTime, GetVehicleEntryTimeQuery);
			entryTimeFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|EntryTime", "Gate-In Date");
		}

		void AddGuidAndNKFilters(ModuleFilterCollection filters)
		{
			var warehouseFilter = filters.AddGuidFilter(Schema.Facility, ModuleIDs.WhsConfigWarehouse, GetWarehouseQuery, new WhsWarehouseCollection(Factory));
			warehouseFilter.Category = FilterCategories.Organisations;
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|Facility", "Facility");

			var unitTypeFilter = filters.AddGuidFilter(Schema.UnitType, ModuleIDs.RefContainer, GteGateMovementSchema.GGM_RC_UnitType, new RefContainerCollection(Factory));
			unitTypeFilter.Category = FilterCategories.ModesAndTypes;
			unitTypeFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|UnitType", "Unit Type");

			var cargoTypeFilter = filters.AddNkFilter(Schema.CargoType, GteGateMovementSchema.GGM_RH_NKCargoType, ModuleIDs.RefCommodityCode, new RefCommodityCodeCollection(Factory));
			cargoTypeFilter.Category = FilterCategories.ModesAndTypes;
			cargoTypeFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|CargoType", "Cargo Type");

			var packageTypeFilter = filters.AddNkFilter(Schema.PackageType, GteGateMovementSchema.GGM_F3_NKPackageType, ModuleIDs.RefPackType, new RefPackTypeCollection(Factory));
			packageTypeFilter.Category = FilterCategories.ModesAndTypes;
			packageTypeFilter.MultilingualDescription = ResString.GetMultilingualString("GteGateMovement|GteGateMovementFilterBusinessObject|PackageType", "Package Type");
		}

		#endregion

		ZQuery GetMovementBookingNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var queryGateMovementBooking = new ZDBOnlySubQuery(typeof(GteGateMovementBooking), GteGateMovementBookingSchema.PK);
			queryGateMovementBooking.AddToFilter(GteGateMovementBookingSchema.GBM_MovementBookingNumber, comparisonOperator, value);

			var queryGateMovement = new ZDBOnlyQuery(typeof(GteGateMovement));
			queryGateMovement.AddSubQuery(GteGateMovementSchema.GGM_GBM_MovementBooking, queryGateMovementBooking, JoinCondition.And);

			return queryGateMovement;
		}

		ZQuery GetInstructionReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var queryGateMovementBooking = new ZDBOnlySubQuery(typeof(GteGateMovementBooking), GteGateMovementBookingSchema.PK);
			queryGateMovementBooking.AddToFilter(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, comparisonOperator, value);

			var queryGateMovement = new ZDBOnlyQuery(typeof(GteGateMovement));
			queryGateMovement.AddSubQuery(GteGateMovementSchema.GGM_GBM_MovementBooking, queryGateMovementBooking, JoinCondition.And);

			return queryGateMovement;
		}

		ZQuery GetBookingPartyReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var queryGateMovementBooking = new ZDBOnlySubQuery(typeof(GteGateMovementBooking), GteGateMovementBookingSchema.PK);
			queryGateMovementBooking.AddToFilter(GteGateMovementBookingSchema.GBM_SourceReferenceNumber, comparisonOperator, value);

			var queryGateMovement = new ZDBOnlyQuery(typeof(GteGateMovement));
			queryGateMovement.AddSubQuery(GteGateMovementSchema.GGM_GBM_MovementBooking, queryGateMovementBooking, JoinCondition.And);

			return queryGateMovement;
		}

		ZQuery GetBookingNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var queryGteBooking = new ZDBOnlySubQuery(typeof(GteBooking), GteBookingSchema.PK);
			queryGteBooking.AddToFilter(GteBookingSchema.GBK_ReferenceNumber, comparisonOperator, value);

			var queryGateMovementBooking = new ZDBOnlySubQuery(typeof(GteGateMovementBooking), GteGateMovementBookingSchema.PK);
			queryGateMovementBooking.AddSubQuery(GteGateMovementBookingSchema.GBM_GBK_Booking, queryGteBooking, JoinCondition.And);

			var queryGateMovement = new ZDBOnlyQuery(typeof(GteGateMovement));
			queryGateMovement.AddSubQuery(GteGateMovementSchema.GGM_GBM_MovementBooking, queryGateMovementBooking, JoinCondition.And);

			return queryGateMovement;
		}

		ZQuery GetGateInNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var queryVehicleEntry = new ZDBOnlySubQuery(typeof(GteVehicleEntry), GteVehicleEntrySchema.GVE_GVM_VehicleMovement);
			queryVehicleEntry.AddToFilter(GteVehicleEntrySchema.GVE_GateActionNumber, comparisonOperator, value);
			queryVehicleEntry.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, true);

			var queryVehicleMovement = new ZDBOnlySubQuery(typeof(GteVehicleMovement), GteVehicleMovementSchema.PK);
			queryVehicleMovement.AddSubQuery(GteVehicleMovementSchema.PK, queryVehicleEntry, JoinCondition.And);

			var queryGateMovement = new ZDBOnlyQuery(typeof(GteGateMovement));
			queryGateMovement.AddSubQuery(GteGateMovementSchema.GGM_GVM_VehicleMovement, queryVehicleMovement, JoinCondition.And);

			return queryGateMovement;
		}

		ZQuery GetVehicleEntryTimeQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset fromDate, ZDateTimeOffset toDate)
		{
			var queryVehicleEntry = new ZDBOnlySubQuery(typeof(GteVehicleEntry), GteVehicleEntrySchema.GVE_GVM_VehicleMovement);			
			queryVehicleEntry.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, true);
			AddDateTimeOffsetRange(queryVehicleEntry, comparisonOperator, JoinCondition.And, GteVehicleEntrySchema.GVE_EntryTime, fromDate, toDate, false, false);

			var queryVehicleMovement = new ZDBOnlySubQuery(typeof(GteVehicleMovement), GteVehicleMovementSchema.PK);
			queryVehicleMovement.AddSubQuery(GteVehicleMovementSchema.PK, queryVehicleEntry, JoinCondition.And);

			var queryGateMovement = new ZDBOnlyQuery(typeof(GteGateMovement));
			queryGateMovement.AddSubQuery(GteGateMovementSchema.GGM_GVM_VehicleMovement, queryVehicleMovement, JoinCondition.And);

			return queryGateMovement;
		}

		ZQuery GetWarehouseQuery(ZGuid value)
		{
			var queryGateMovement = new ZDBOnlyQuery(typeof(GteGateMovement));
			var queryLocation = new ZDBOnlySubQuery(typeof(WhsLocation), WhsLocationViewSchema.PK);
			var queryWarehouse = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.PK);

			queryWarehouse.AddToFilter(WhsWarehouseSchema.PK,value);

			queryLocation.AddSubQuery(WhsLocationViewSchema.WLV_WW_Whs, queryWarehouse, JoinCondition.And);
			queryGateMovement.AddSubQuery(GteGateMovementSchema.GGM_WL_Dock, queryLocation, JoinCondition.And);

			return queryGateMovement;
		}
	}
}
