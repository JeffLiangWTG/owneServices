

using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.GUI.Test
{
	[TestedType(typeof(GteVehicleMovementFilterBusinessObject))]
	public class GteVehicleMovementFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		readonly ZDateTimeOffset EntryTime = ZDateTimeOffset.Now.AddDays(-5);
		readonly ZDateTimeOffset ExitTime = ZDateTimeOffset.Now;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GteVehicleMovementFilterBusinessObject();
		}

		public void TestVehicleMovementFiltersExist()
		{
			var filters = GetNewFilterStripBusinessObject();

			CombineAssertions(() =>
			{
				AssertFilter("Vehicle Registration", FilterCategories.NumbersAndReferences, filters[GteVehicleMovementFilterBusinessObject.Schema.VehicleRegistration]);
				AssertFilter("Vehicle Type", FilterCategories.ModesAndTypes, filters[GteVehicleMovementFilterBusinessObject.Schema.VehicleType]);
				AssertFilter("Booking Number", FilterCategories.NumbersAndReferences, filters[GteVehicleMovementFilterBusinessObject.Schema.BookingNumber]);

				AssertFilter("Gate-in Number", FilterCategories.NumbersAndReferences, filters[GteVehicleMovementFilterBusinessObject.Schema.GateInNumber]);
				AssertFilter("Entry Time", FilterCategories.Dates, filters[GteVehicleMovementFilterBusinessObject.Schema.EntryTime]);
				AssertFilter("Entry Driver", FilterCategories.TextSearch, filters[GteVehicleMovementFilterBusinessObject.Schema.EntryDriver]);
				AssertFilter("Entry License", FilterCategories.TextSearch, filters[GteVehicleMovementFilterBusinessObject.Schema.EntryLicense]);

				AssertFilter("Gate-out Number", FilterCategories.NumbersAndReferences, filters[GteVehicleMovementFilterBusinessObject.Schema.GateOutNumber]);
				AssertFilter("Exit Time", FilterCategories.Dates, filters[GteVehicleMovementFilterBusinessObject.Schema.ExitTime]);
				AssertFilter("Exit Driver", FilterCategories.TextSearch, filters[GteVehicleMovementFilterBusinessObject.Schema.ExitDriver]);
				AssertFilter("Exit License", FilterCategories.TextSearch, filters[GteVehicleMovementFilterBusinessObject.Schema.ExitLicense]);
			});
		}

		[RequiresSTA]
		void AssertFilter(string expectedLabel, FilterCategory expectedCategory, ModuleFilter moduleFilter)
		{
			AssertNotNull($"{expectedLabel} should not be null", moduleFilter);
			AssertEquals($"{expectedLabel} has the correct category", expectedCategory, moduleFilter.Category);
		}

		#region Vehicle Registration Number

		public void TestFilterVehicleRegistration()
		{
			var vehicleMovement = CreateVehicleMovement();
			var vehicleMovement2 = CreateVehicleMovement(vehicleRego: "REG-2");

			Factory.Save();
			Asserter.AddToScope(vehicleMovement, vehicleMovement2);

			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[GteVehicleMovementFilterBusinessObject.Schema.VehicleRegistration]);
			var filterModuleNumber = (ModuleNumberFilter)filters[GteVehicleMovementFilterBusinessObject.Schema.VehicleRegistration];
			filterModuleNumber.IsActive = true;

			filterModuleNumber.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filterModuleNumber.Property = "REG-1";

			Asserter.AssertMatches("Should have found a matched vehicle registration number in Vehicle Movement", filterModuleNumber, vehicleMovement);

			filterModuleNumber.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filterModuleNumber.Property = "REG";

			Asserter.AssertMatches("Should have returned multiple records of vehicle registration number in Vehicle Movement", filterModuleNumber, vehicleMovement, vehicleMovement2);
		}

		#endregion

		#region Booking Number

		public void TestFilterBookingNumber()
		{
			var vehicleMovements = Enumerable.Range(1, 3)
				.Select(i => CreateVehicleMovementWithBooking($"GB{i:00}"))
				.ToArray();

			Factory.Save();
			Asserter.AddToScope(vehicleMovements);

			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[GteVehicleMovementFilterBusinessObject.Schema.BookingNumber]);
			var filterModuleNumber = (ModuleNumberFilter)filters[GteVehicleMovementFilterBusinessObject.Schema.BookingNumber];
			filterModuleNumber.IsActive = true;

			filterModuleNumber.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filterModuleNumber.Property = "GB02";
			Asserter.AssertMatches("Should have found a matched booking number in Vehicle Movement", filterModuleNumber, vehicleMovements[1]);

			filterModuleNumber.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filterModuleNumber.Property = "GB";
			Asserter.AssertMatches("Should have returned all records of booking number in Vehicle Movement", filterModuleNumber, vehicleMovements);

			filterModuleNumber.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filterModuleNumber.Property = "0";
			Asserter.AssertMatches("Should have returned all records of booking number in Vehicle Movement", filterModuleNumber, vehicleMovements);

			filterModuleNumber.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filterModuleNumber.Property = "3";
			Asserter.AssertMatches("Should have found a matched booking number in Vehicle Movement", filterModuleNumber, vehicleMovements[2]);

			filterModuleNumber.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filterModuleNumber.Property = "GB01";
			Asserter.AssertMatches("Should have found a matched booking number in Vehicle Movement", filterModuleNumber, vehicleMovements[0]);
		}

		#endregion

		#region Vehicle Type
		public void TestFilterVehicleType()
		{
			var vehicleMovement = CreateVehicleMovement(vehicleType: "20GP");
			var vehicleMovement2 = CreateVehicleMovement(vehicleRego: "REG-2", vehicleType: "40GP");

			Factory.Save();
			Asserter.AddToScope(vehicleMovement, vehicleMovement2);

			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[GteVehicleMovementFilterBusinessObject.Schema.VehicleType]);
			var filter = (ModuleGuidFilter)filters[GteVehicleMovementFilterBusinessObject.Schema.VehicleType];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filter.Property = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
			Asserter.AssertMatches("Should have found a matched vehicle type in Vehicle Movement", filters.Filter, vehicleMovement);
		}

		#endregion

		#region Gate Action Number

		public void TestFilterVehicleGateActionNumber()
		{
			var vehicleMovement = CreateVehicleMovement(gateInNumber: "GIN111", gateOutNumber: "GO111");
			var vehicleMovement2 = CreateVehicleMovement(vehicleRego: "REG-2", gateInNumber: "GIN222", gateOutNumber: "GO222");

			Factory.Save();
			Asserter.AddToScope(vehicleMovement, vehicleMovement2);

			AssertVehicleEntryNumberFilters(vehicleMovement, vehicleMovement2, GteVehicleMovementFilterBusinessObject.Schema.GateInNumber, "GIN111", "GIN", "Gate In Number");
			AssertVehicleEntryNumberFilters(vehicleMovement, vehicleMovement2, GteVehicleMovementFilterBusinessObject.Schema.GateOutNumber, "GO111", "GO", "Gate Out Number");
		}

		#endregion

		#region Vehicle Driver Details

		public void TestFilterVehicleDriverDetails()
		{
			var vehicleMovement = CreateVehicleMovement(driverNameIn: "DriverIn1", driverLicenseIn: "LicenseIn1", driverNameOut: "DriverOut1", driverLicenseOut: "LicenseOut1");
			var vehicleMovement2 = CreateVehicleMovement(vehicleRego: "REG-2", driverNameIn: "DriverIn2", driverLicenseIn: "LicenseIn2", driverNameOut: "DriverOut2", driverLicenseOut: "LicenseOut2");

			Factory.Save();
			Asserter.AddToScope(vehicleMovement, vehicleMovement2);

			AssertVehicleEntryTextFilters(vehicleMovement, vehicleMovement2, GteVehicleMovementFilterBusinessObject.Schema.EntryDriver, "DriverIn1", "DriverIn", "Entry Driver");
			AssertVehicleEntryTextFilters(vehicleMovement, vehicleMovement2, GteVehicleMovementFilterBusinessObject.Schema.EntryLicense, "LicenseIn1", "LicenseIn", "Entry License");
			AssertVehicleEntryTextFilters(vehicleMovement, vehicleMovement2, GteVehicleMovementFilterBusinessObject.Schema.ExitDriver, "DriverOut1", "DriverOut", "Exit Driver");
			AssertVehicleEntryTextFilters(vehicleMovement, vehicleMovement2, GteVehicleMovementFilterBusinessObject.Schema.ExitLicense, "LicenseOut1", "LicenseOut", "Exit License");
		}

		#endregion

		#region Vehicle Entry Time

		public void TestFilterVehicleEntryTime()
		{
			var vehicleMovement = CreateVehicleMovement(gateInNumber: "GIN111", gateOutNumber: "GO111", entryTime: EntryTime, exitTime: ExitTime);
			var vehicleMovement2 = CreateVehicleMovement(vehicleRego: "REG-2", gateInNumber: "GIN222", gateOutNumber: "GO222", entryTime: EntryTime, exitTime: ExitTime);

			Factory.Save();
			Asserter.AddToScope(vehicleMovement, vehicleMovement2);

			AssertVehicleEntryDateFilters(vehicleMovement, vehicleMovement2, GteVehicleMovementFilterBusinessObject.Schema.EntryTime, EntryTime, "Entry Time");
			AssertVehicleEntryDateFilters(vehicleMovement, vehicleMovement2, GteVehicleMovementFilterBusinessObject.Schema.ExitTime, ExitTime, "Exit Time");
		}

		#endregion

		protected GteVehicleMovement CreateVehicleMovement(string vehicleRego = "REG-1", string vehicleType = "RTRK", string gateInNumber = "", string gateOutNumber = "", string driverNameIn = "",
															string driverLicenseIn = "", string driverNameOut = "", string driverLicenseOut = "", ZDateTimeOffset? entryTime = null, ZDateTimeOffset? exitTime = null)
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, vehicleType);
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = vehicleRego;
			vehicleMovement.GVM_RC_VehicleType = refContainer.PK;

			var lane = Factory.NewWithValidTestData<GteLane>();

			var vehicleGateInEntry = Factory.New<GteVehicleEntry>();
			vehicleGateInEntry.GVE_IsIncoming = true;
			vehicleGateInEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleGateInEntry.GVE_GateActionNumber = gateInNumber;
			vehicleGateInEntry.GVE_DriverName = driverNameIn;
			vehicleGateInEntry.GVE_DriverLicenseNumber = driverLicenseIn;
			vehicleGateInEntry.GVE_EntryTime = entryTime ?? ZDateTimeOffset.Now;
			vehicleGateInEntry.GVE_GLN_Lane = lane.PK;

			var vehicleGateOutEntry = Factory.New<GteVehicleEntry>();
			vehicleGateOutEntry.GVE_IsIncoming = false;
			vehicleGateOutEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleGateOutEntry.GVE_GateActionNumber = gateOutNumber;
			vehicleGateOutEntry.GVE_DriverName = driverNameOut;
			vehicleGateOutEntry.GVE_DriverLicenseNumber = driverLicenseOut;
			vehicleGateOutEntry.GVE_EntryTime = exitTime ?? ZDateTimeOffset.Now;
			vehicleGateOutEntry.GVE_GLN_Lane = lane.PK;

			return vehicleMovement;
		}

		protected GteVehicleMovement CreateVehicleMovementWithBooking(string bookingRef)
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = bookingRef;

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			booking.GateMovementBookings.Add(gateMovementBooking);

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GateMovements.Add(gateMovement);

			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_IsIncoming = true;

			return vehicleMovement;
		}

		public void AssertVehicleEntryNumberFilters(GteVehicleMovement vm1, GteVehicleMovement vm2, string filterObject, string filterExact, string filterContains, string expectedLabel)
		{
			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[filterObject]);
			var filter = (ModuleNumberFilter)filters[filterObject];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filter.Property = filterExact;

			Asserter.AssertMatches($"Should have a matched {expectedLabel} in Vehicle Movement", filter, vm1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.Property = filterContains;

			Asserter.AssertMatches($"Should have multiple matched {expectedLabel} in Vehicle Movement", filter, vm1, vm2);
		}

		public void AssertVehicleEntryTextFilters(GteVehicleMovement vm1, GteVehicleMovement vm2, string filterObject, string filterExact, string filterContains, string expectedLabel)
		{
			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[filterObject]);
			var filter = (ModuleTextFilter)filters[filterObject];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = filterExact;

			Asserter.AssertMatches($"Should have a matched {expectedLabel} in Vehicle Movement", filter, vm1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.Property = filterContains;

			Asserter.AssertMatches($"Should have multiple matched {expectedLabel} in Vehicle Movement", filter, vm1, vm2);
		}

		public void AssertVehicleEntryDateFilters(GteVehicleMovement vm1, GteVehicleMovement vm2, string filterObject, ZDateTimeOffset vehicleEntryDate, string expectedLabel)
		{
			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[filterObject]);
			var filter = (ModuleDateTimeOffsetFilter)filters[filterObject];
			filter.IsActive = true;

			filter.PropertySearch = ModuleDateTimeOffsetFilter.SpecifiedDateRange;
			filter.Property1 = vehicleEntryDate.ToZDateTime();
			filter.Property2 = vehicleEntryDate.ToZDateTime().AddDays(1);

			Asserter.AssertMatches($"Should have multiple matched {expectedLabel} in Vehicle Movement", filter, vm1, vm2);
		}

		protected FilterStripAsserter<GteVehicleMovement> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<GteVehicleMovement>(Factory, vhm => vhm.PK.ToString())); }
		}

		FilterStripAsserter<GteVehicleMovement> asserter;
	}
}
