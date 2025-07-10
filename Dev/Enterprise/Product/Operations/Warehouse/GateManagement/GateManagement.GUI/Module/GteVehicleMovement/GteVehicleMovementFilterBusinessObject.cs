
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteVehicleMovementFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string VehicleRegistration = nameof(VehicleRegistration);
			public const string VehicleType = nameof(VehicleType);
			public const string BookingNumber = nameof(BookingNumber);

			public const string GateInNumber = nameof(GateInNumber);
			public const string EntryTime = nameof(EntryTime);
			public const string EntryDriver = nameof(EntryDriver);
			public const string EntryLicense = nameof(EntryLicense);

			public const string GateOutNumber = nameof(GateOutNumber);
			public const string ExitTime = nameof(ExitTime);
			public const string ExitDriver = nameof(ExitDriver);
			public const string ExitLicense = nameof(ExitLicense);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddTextFilters(filters);
			AddDateFilters(filters);

			return filters;
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var vehicleRegistrationFilter = filters.AddNumberFilter(Schema.VehicleRegistration, GteVehicleMovementSchema.GVM_VehicleRegistration);
			vehicleRegistrationFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|VehicleRegistration", "Vehicle Registration");

			var gateInNumberFilter = filters.AddNumberFilter(Schema.GateInNumber, GetGateInNumberQuery);
			gateInNumberFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|GateInNumber", "Gate-in Number");
			gateInNumberFilter.SubGroup = new VehicleEntrySubGroup();

			var gateOutNumberFilter = filters.AddNumberFilter(Schema.GateOutNumber, GetGateOutNumberQuery);
			gateOutNumberFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|GateOutNumber", "Gate-out Number");
			gateOutNumberFilter.SubGroup = new VehicleEntrySubGroup();

			var bookingNumberFilter = filters.AddNumberFilter(Schema.BookingNumber, GteBookingSchema.GBK_ReferenceNumber);
			bookingNumberFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|BookingNumber", "Booking Number");
			bookingNumberFilter.SubGroup = new BookingNumberSubGroup();
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var entryDriverFilter = filters.AddTextFilter(Schema.EntryDriver, GetEntryDriverNameQuery);
			entryDriverFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|EntryDriver", "Entry Driver");
			entryDriverFilter.SubGroup = new VehicleEntrySubGroup();

			var exitDriverFilter = filters.AddTextFilter(Schema.ExitDriver, GetExitDriverNameQuery);
			exitDriverFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|ExitDriver", "Exit Driver");
			exitDriverFilter.SubGroup = new VehicleEntrySubGroup();

			var entryLicenseFilter = filters.AddTextFilter(Schema.EntryLicense, GetEntryDriverLicenceQuery);
			entryLicenseFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|EntryLicense", "Entry License");
			entryLicenseFilter.SubGroup = new VehicleEntrySubGroup();

			var exitLicenseFilter = filters.AddTextFilter(Schema.ExitLicense, GetExitDriverLicenceQuery);
			exitLicenseFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|ExitLicense", "Exit License");
			exitLicenseFilter.SubGroup = new VehicleEntrySubGroup();

			var vehicleTypeFilter = filters.AddGuidFilter(Schema.VehicleType, ModuleIDs.RefContainer, (pk) => new ZQuery(RefContainerSchema.PK, pk), ContainerTypeList);
			vehicleTypeFilter.Category = FilterCategories.ModesAndTypes;
			vehicleTypeFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|VehicleType", "Vehicle Type");
			vehicleTypeFilter.SubGroup = VehicleTypeFilterProcessor;
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var entryTimeFilter = filters.AddDateFilter(Schema.EntryTime, GetVehicleEntryTimeQuery);
			entryTimeFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|EntryTime", "Entry Time");
			entryTimeFilter.SubGroup = new VehicleEntrySubGroup();

			var exitTimeFilter = filters.AddDateFilter(Schema.ExitTime, GetVehicleExitTimeQuery);
			exitTimeFilter.MultilingualDescription = ResString.GetMultilingualString("GteVehicleMovement|GteVehicleMovementFilterBusinessObject|ExitTime", "Exit Time");
			exitTimeFilter.SubGroup = new VehicleEntrySubGroup();
		}

		ZQuery GetGateInNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(GteVehicleEntry));
			query.AddToFilter(GteVehicleEntrySchema.GVE_GateActionNumber, comparisonOperator, value);
			query.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, true);
			return query;
		}

		ZQuery GetGateOutNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(GteVehicleEntry));
			query.AddToFilter(GteVehicleEntrySchema.GVE_GateActionNumber, comparisonOperator, value);
			query.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, false);
			return query;
		}

		ZQuery GetEntryDriverNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(GteVehicleEntry));
			query.AddToFilter(GteVehicleEntrySchema.GVE_DriverName, comparisonOperator, value);
			query.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, true);
			return query;
		}

		ZQuery GetExitDriverNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(GteVehicleEntry));
			query.AddToFilter(GteVehicleEntrySchema.GVE_DriverName, comparisonOperator, value);
			query.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, false);
			return query;
		}

		ZQuery GetEntryDriverLicenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(GteVehicleEntry));
			query.AddToFilter(GteVehicleEntrySchema.GVE_DriverLicenseNumber, comparisonOperator, value);
			query.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, true);
			return query;
		}

		ZQuery GetExitDriverLicenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(GteVehicleEntry));
			query.AddToFilter(GteVehicleEntrySchema.GVE_DriverLicenseNumber, comparisonOperator, value);
			query.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, false);
			return query;
		}

		ZQuery GetVehicleEntryTimeQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset fromDate, ZDateTimeOffset toDate)
		{
			var query = new ZDBOnlyQuery(typeof(GteVehicleEntry));
			query.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, true);
			AddDateTimeOffsetRange(query, comparisonOperator, JoinCondition.And, GteVehicleEntrySchema.GVE_EntryTime, fromDate, toDate, false, false);
			return query;
		}

		ZQuery GetVehicleExitTimeQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset fromDate, ZDateTimeOffset toDate)
		{
			var query = new ZDBOnlyQuery(typeof(GteVehicleEntry));
			query.AddToFilter(GteVehicleEntrySchema.GVE_IsIncoming, false);
			AddDateTimeOffsetRange(query, comparisonOperator, JoinCondition.And, GteVehicleEntrySchema.GVE_EntryTime, fromDate, toDate, false, false);
			return query;
		}

		class VehicleEntrySubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GteVehicleMovement));
				var subQuery = new ZDBOnlySubQuery(typeof(GteVehicleEntry), GteVehicleEntrySchema.GVE_GVM_VehicleMovement);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(GteVehicleMovementSchema.PK, subQuery, JoinCondition.And);
				return query;
			}
		}

		class BookingNumberSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var vehicleMovementQuery = new ZDBOnlyQuery(typeof(GteVehicleMovement));

				var bookingSubQuery = new ZDBOnlySubQuery(typeof(GteBooking), GteBookingSchema.PK);
				bookingSubQuery.AddToFilter(filter);

				var movementBookingSubQuery = new ZDBOnlySubQuery(typeof(GteGateMovementBooking), GteGateMovementBookingSchema.PK);
				movementBookingSubQuery.AddSubQuery(GteGateMovementBookingSchema.GBM_GBK_Booking, bookingSubQuery, JoinCondition.And);

				var gateMovementSubQuery = new ZDBOnlySubQuery(typeof(GteGateMovement), GteGateMovementSchema.GGM_GVM_VehicleMovement);
				gateMovementSubQuery.AddSubQuery(GteGateMovementSchema.GGM_GBM_MovementBooking, movementBookingSubQuery, JoinCondition.And);

				vehicleMovementQuery.AddSubQuery(GteVehicleMovementSchema.PK, gateMovementSubQuery, JoinCondition.And);

				return vehicleMovementQuery;
			}
		}

		RefContainerCollection ContainerTypeList => containerTypeList ?? (containerTypeList = new RefContainerCollection(Factory));

		RefContainerCollection containerTypeList;

		ModuleFilterSubGroup VehicleTypeFilterProcessor => vehicleTypeFilterProcessor ?? (vehicleTypeFilterProcessor = new VehicleTypeSubGroup());

		VehicleTypeSubGroup vehicleTypeFilterProcessor;

		class VehicleTypeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GteVehicleMovement));
				var subQuery = new ZDBOnlySubQuery(typeof(RefContainer), RefContainerSchema.PK);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(GteVehicleMovementSchema.GVM_RC_VehicleType, subQuery, JoinCondition.And);
				return query;
			}
		}
	}
}
