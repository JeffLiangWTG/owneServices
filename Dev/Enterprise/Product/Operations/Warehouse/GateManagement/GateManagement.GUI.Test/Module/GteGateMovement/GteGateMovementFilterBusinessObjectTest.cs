
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.GUI.Test
{
	[TestedType(typeof(GteGateMovementFilterBusinessObject))]
	public class GteGateMovementFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		readonly ZDateTimeOffset EntryTime = ZDateTimeOffset.Now;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GteGateMovementFilterBusinessObject();
		}

		public void TestGateMovementFiltersExist()
		{
			var filters = GetNewFilterStripBusinessObject();

			CombineAssertions(() =>
			{
				AssertFilter("Transport Reference", FilterCategories.NumbersAndReferences, filters[GteGateMovementFilterBusinessObject.Schema.TransportReference]);
				AssertFilter("Unit Number", FilterCategories.NumbersAndReferences, filters[GteGateMovementFilterBusinessObject.Schema.UnitNumber]);
				AssertFilter("Gate-in Number", FilterCategories.NumbersAndReferences, filters[GteGateMovementFilterBusinessObject.Schema.GateInNumber]);
				AssertFilter("Movement Booking Number", FilterCategories.NumbersAndReferences, filters[GteGateMovementFilterBusinessObject.Schema.MovementBookingNumber]);
				AssertFilter("Instruction Reference", FilterCategories.NumbersAndReferences, filters[GteGateMovementFilterBusinessObject.Schema.InstructionReference]);
				AssertFilter("Booking Party Reference", FilterCategories.NumbersAndReferences, filters[GteGateMovementFilterBusinessObject.Schema.BookingPartyReference]);
				AssertFilter("Booking Number", FilterCategories.NumbersAndReferences, filters[GteGateMovementFilterBusinessObject.Schema.BookingNumber]);

				AssertFilter("Entry Time", FilterCategories.Dates, filters[GteVehicleMovementFilterBusinessObject.Schema.EntryTime]);

				AssertFilter("Facility", FilterCategories.Organisations, filters[GteGateMovementFilterBusinessObject.Schema.Facility]);

				AssertFilter("Cargo Type", FilterCategories.ModesAndTypes, filters[GteGateMovementFilterBusinessObject.Schema.CargoType]);
				AssertFilter("Package Type", FilterCategories.ModesAndTypes, filters[GteGateMovementFilterBusinessObject.Schema.PackageType]);
				AssertFilter("Unit Type", FilterCategories.ModesAndTypes, filters[GteGateMovementFilterBusinessObject.Schema.UnitType]);
			});
		}

		[RequiresSTA]
		void AssertFilter(string expectedLabel, FilterCategory expectedCategory, ModuleFilter moduleFilter)
		{
			AssertNotNull($"{expectedLabel} should not be null", moduleFilter);
			AssertEquals($"{expectedLabel} has the correct category", expectedCategory, moduleFilter.Category);
		}

		#region Transport Reference

		public void TestFilterTransportReference()
		{
			var gateMovement1 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_TransportReference == "TREF111").FirstOrDefault();
			var gateMovement2 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_TransportReference == "TREF222").FirstOrDefault();

			Asserter.AddToScope(gateMovement1, gateMovement2);

			AssertNumberFilterFindsExactMatch(gateMovement1, GteGateMovementFilterBusinessObject.Schema.TransportReference, "TREF111");
			AssertNumberFilterFindsContainingMatches([gateMovement1, gateMovement2], GteGateMovementFilterBusinessObject.Schema.TransportReference, "TREF");
		}

		#endregion

		#region Movement Booking Number

		public void TestFilterMovementBookingNumber()
		{
			var gateMovement1 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GateMovementBooking.GBM_MovementBookingNumber == "MBN111").FirstOrDefault();
			var gateMovement2 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GateMovementBooking.GBM_MovementBookingNumber == "MBN222").FirstOrDefault();

			Asserter.AddToScope(gateMovement1, gateMovement2);

			AssertNumberFilterFindsExactMatch(gateMovement1, GteGateMovementFilterBusinessObject.Schema.MovementBookingNumber, "MBN111");
			AssertNumberFilterFindsContainingMatches([gateMovement1, gateMovement2], GteGateMovementFilterBusinessObject.Schema.MovementBookingNumber, "MBN");
		}

		#endregion

		#region Instruction Reference

		public void TestFilterInstructionReference()
		{
			var gateMovement1 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GateMovementBooking.GBM_BookingReferenceNumber == "BRN111").FirstOrDefault();
			var gateMovement2 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GateMovementBooking.GBM_BookingReferenceNumber == "BRN222").FirstOrDefault();

			Asserter.AddToScope(gateMovement1, gateMovement2);

			AssertNumberFilterFindsExactMatch(gateMovement1, GteGateMovementFilterBusinessObject.Schema.InstructionReference, "BRN111");
			AssertNumberFilterFindsContainingMatches([gateMovement1, gateMovement2], GteGateMovementFilterBusinessObject.Schema.InstructionReference, "BRN");
		}

		#endregion

		#region Booking Party Reference

		public void TestFilterBookingPartyReference()
		{
			var gateMovement1 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GateMovementBooking.GBM_SourceReferenceNumber == "SRN111").FirstOrDefault();
			var gateMovement2 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GateMovementBooking.GBM_SourceReferenceNumber == "SRN222").FirstOrDefault();

			Asserter.AddToScope(gateMovement1, gateMovement2);

			AssertNumberFilterFindsExactMatch(gateMovement1, GteGateMovementFilterBusinessObject.Schema.BookingPartyReference, "SRN111");
			AssertNumberFilterFindsContainingMatches([gateMovement1, gateMovement2], GteGateMovementFilterBusinessObject.Schema.BookingPartyReference, "SRN");
		}

		#endregion

		#region Booking Number

		public void TestFilterBookingNumber()
		{
			var gateMovement1 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GateMovementBooking.Booking.GBK_ReferenceNumber == "BK111").FirstOrDefault();
			var gateMovement2 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GateMovementBooking.Booking.GBK_ReferenceNumber == "BK222").FirstOrDefault();

			Asserter.AddToScope(gateMovement1, gateMovement2);

			AssertNumberFilterFindsExactMatch(gateMovement1, GteGateMovementFilterBusinessObject.Schema.BookingNumber, "BK111");
			AssertNumberFilterFindsContainingMatches([gateMovement1, gateMovement2], GteGateMovementFilterBusinessObject.Schema.BookingNumber, "B");
		}

		#endregion

		#region Unit Number

		public void TestFilterUnitNumber()
		{
			var gateMovement1 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_UnitNumber == "TSTCONT1").FirstOrDefault();
			var gateMovement2 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_UnitNumber == "TSTCONT2").FirstOrDefault();

			Asserter.AddToScope(gateMovement1, gateMovement2);

			AssertNumberFilterFindsExactMatch(gateMovement1, GteGateMovementFilterBusinessObject.Schema.UnitNumber, "TSTCONT1");
			AssertNumberFilterFindsContainingMatches([gateMovement1, gateMovement2], GteGateMovementFilterBusinessObject.Schema.UnitNumber, "TSTCONT");
		}

		#endregion

		#region Gate-In Number

		public void TestFilterGateInNumber()
		{
			var gateMovement1 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_TransportReference == "TREF111" && g.VehicleMovement.GateInVehicleEntry.GVE_GateActionNumber == "GIN111").FirstOrDefault();
			var gateMovement2 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_TransportReference == "TREF222" && g.VehicleMovement.GateInVehicleEntry.GVE_GateActionNumber == "GIN222").FirstOrDefault();

			Asserter.AddToScope(gateMovement1, gateMovement2);

			AssertNumberFilterFindsExactMatch(gateMovement1, GteGateMovementFilterBusinessObject.Schema.GateInNumber, "GIN111");
			AssertNumberFilterFindsContainingMatches([gateMovement1, gateMovement2], GteGateMovementFilterBusinessObject.Schema.GateInNumber, "GIN");
		}

		#endregion

		#region Gate-In Date

		public void TestFilterGateInDate()
		{
			var gateMovement1 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_TransportReference == "TREF111" && g.VehicleMovement.GateInVehicleEntry.GVE_EntryTime == EntryTime).FirstOrDefault();
			var gateMovement2 = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_TransportReference == "TREF222" && g.VehicleMovement.GateInVehicleEntry.GVE_EntryTime == EntryTime).FirstOrDefault();

			Asserter.AddToScope(gateMovement1, gateMovement2);

			AssertVehicleEntryDateFilters(gateMovement1, gateMovement2, GteGateMovementFilterBusinessObject.Schema.EntryTime, EntryTime);
		}

		#endregion

		#region Cargo Type

		public void TestFilterCargoType()
		{
			var gateMovement = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_RH_NKCargoType == "ALUM").FirstOrDefault();

			Asserter.AddToScope(gateMovement);

			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[GteGateMovementFilterBusinessObject.Schema.CargoType]);
			var filter = (ModuleNkFilter)filters[GteGateMovementFilterBusinessObject.Schema.CargoType];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
			filter.Property = "ALUM";

			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
			Asserter.AssertMatches("Should have found a matched Cargo Type in Gate Movement", filters.Filter, gateMovement);
		}

		#endregion

		#region Package Type

		public void TestFilterPackageType()
		{
			var gateMovement = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_F3_NKPackageType == "BAG").FirstOrDefault();

			Asserter.AddToScope(gateMovement);

			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[GteGateMovementFilterBusinessObject.Schema.PackageType]);
			var filter = (ModuleNkFilter)filters[GteGateMovementFilterBusinessObject.Schema.PackageType];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
			filter.Property = "BAG";

			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
			Asserter.AssertMatches("Should have found a matched Package Type in Gate Movement", filters.Filter, gateMovement);
		}

		#endregion

		#region Unit Type

		public void TestFilterUnitType()
		{
			var gateMovement = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_TransportReference == "TREF111").FirstOrDefault();

			Asserter.AddToScope(gateMovement);

			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[GteGateMovementFilterBusinessObject.Schema.UnitType]);
			var filter = (ModuleGuidFilter)filters[GteGateMovementFilterBusinessObject.Schema.UnitType];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filter.Property = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
			Asserter.AssertMatches("Should have found a matched Unit Type in Gate Movement", filters.Filter, gateMovement);
		}

		#endregion

		#region Facility

		public void TestFilterFacility()
		{
			var gateMovement = Factory.Load<GteGateMovement>(new ZQuery()).Where(g => g.GGM_TransportReference == "TREF111" && g.Dock.Warehouse.WW_WarehouseName == "Warehouse1").FirstOrDefault();
			Asserter.AddToScope(gateMovement);

			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[GteGateMovementFilterBusinessObject.Schema.Facility]);
			var filter = (ModuleGuidFilter)filters[GteGateMovementFilterBusinessObject.Schema.Facility];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filter.Property = gateMovement.Dock.Warehouse.PK;

			AssertEquals(FilterCategories.Organisations, filter.Category);
			Asserter.AssertMatches("Should have found a matched Facility in Gate Movement", filters.Filter, gateMovement);
		}

		#endregion

		void AssertNumberFilterFindsExactMatch(GteGateMovement matchedGateMovement, string filterSchema, string filterValue)
		{
			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[filterSchema]);
			var filter = (ModuleNumberFilter)filters[filterSchema];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filter.Property = filterValue;

			Asserter.AssertMatches($"Should have a matched {filterSchema} in Gate Movement", filter, matchedGateMovement);
		}

		void AssertNumberFilterFindsContainingMatches(GteGateMovement[] matchedGateMovements, string filterSchema, string filterValue)
		{
			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[filterSchema]);
			var filter = (ModuleNumberFilter)filters[filterSchema];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.Property = filterValue;

			Asserter.AssertMatches($"Should have multiple matched {filterSchema} in Gate Movement", filter, matchedGateMovements[0], matchedGateMovements[1]);
		}

		void AssertVehicleEntryDateFilters(GteGateMovement gm1, GteGateMovement gm2, string filterSchema, ZDateTimeOffset vehicleEntryDate)
		{
			var filters = GetNewFilterStripBusinessObject();

			AssertNotNull(filters.ModuleFilters[filterSchema]);
			var filter = (ModuleDateTimeOffsetFilter)filters[filterSchema];
			filter.IsActive = true;

			filter.PropertySearch = ModuleDateTimeOffsetFilter.SpecifiedDateRange;
			filter.Property1 = vehicleEntryDate.ToZDateTime();
			filter.Property2 = vehicleEntryDate.ToZDateTime().AddDays(1);

			Asserter.AssertMatches($"Should have multiple matched {filterSchema} in Gate Movement", filter, gm1, gm2);
		}

		FilterStripAsserter<GteGateMovement> Asserter
		{
			get { return asserter ??= new FilterStripAsserter<GteGateMovement>(Factory, g => g.PK.ToString()); }
		}

		FilterStripAsserter<GteGateMovement> asserter;

		protected override void SetUp()
		{
			base.SetUp();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			orgAddress.OA_Code = "BLANK SYD";

			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.Warehouse.WW_OA_WarehouseAddress = orgAddress.PK;
			dock.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			dock.Warehouse.WW_WarehouseName = "Warehouse1";
			dock.Warehouse.WW_IsActive = true;

			#region Test Data 1

			var vehicleMovement1 = Factory.NewWithValidTestData<GteVehicleMovement>();
			var vehicleGateInEntry1 = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleGateInEntry1.GVE_IsIncoming = true;
			vehicleGateInEntry1.GVE_GVM_VehicleMovement = vehicleMovement1.PK;
			vehicleGateInEntry1.GVE_GateActionNumber = "GIN111";
			vehicleGateInEntry1.GVE_EntryTime = EntryTime;
			vehicleGateInEntry1.GVE_GLN_Lane = Factory.NewWithValidTestData<GteLane>().PK;
			vehicleMovement1.VehicleEntries.Add(vehicleGateInEntry1);

			var booking1 = Factory.NewWithValidTestData<GteBooking>();
			booking1.GBK_ReferenceNumber = "BK111";

			var gateMovementBooking1 = Factory.New<GteGateMovementBooking>();
			gateMovementBooking1.GBM_GBK_Booking = booking1.PK;
			gateMovementBooking1.GBM_Source = "VBS";
			gateMovementBooking1.GBM_BookingReferenceNumber = "BRN111";
			gateMovementBooking1.GBM_SourceReferenceNumber = "SRN111";
			gateMovementBooking1.GBM_MovementBookingNumber = "MBN111";
			gateMovementBooking1.GBM_TransportReference = "TREF111";

			var gateMovement1 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement1.GGM_GVM_VehicleMovement = vehicleMovement1.PK;
			gateMovement1.GGM_GBM_MovementBooking = gateMovementBooking1.PK;
			gateMovement1.GGM_IsPickup = true;
			gateMovement1.GGM_RH_NKCargoType = "ALUM";
			gateMovement1.GGM_F3_NKPackageType = "BAG";
			gateMovement1.GGM_TransportReference = gateMovementBooking1.GBM_TransportReference;
			gateMovement1.GGM_UnitNumber = "TSTCONT1";
			gateMovement1.GGM_RC_UnitType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			gateMovement1.GGM_WL_Dock = dock.PK;

			#endregion

			#region Test Data 2

			var vehicleMovement2 = Factory.NewWithValidTestData<GteVehicleMovement>();
			var vehicleGateInEntry2 = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleGateInEntry2.GVE_IsIncoming = true;
			vehicleGateInEntry2.GVE_GVM_VehicleMovement = vehicleMovement2.PK;
			vehicleGateInEntry2.GVE_GateActionNumber = "GIN222";
			vehicleGateInEntry2.GVE_EntryTime = EntryTime;
			vehicleGateInEntry2.GVE_GLN_Lane = Factory.NewWithValidTestData<GteLane>().PK;
			vehicleMovement2.VehicleEntries.Add(vehicleGateInEntry2);

			var booking2 = Factory.NewWithValidTestData<GteBooking>();
			booking2.GBK_ReferenceNumber = "BK222";

			var gateMovementBooking2 = Factory.New<GteGateMovementBooking>();
			gateMovementBooking2.GBM_GBK_Booking = booking2.PK;
			gateMovementBooking2.GBM_Source = "VBS";
			gateMovementBooking2.GBM_BookingReferenceNumber = "BRN222";
			gateMovementBooking2.GBM_SourceReferenceNumber = "SRN222";
			gateMovementBooking2.GBM_MovementBookingNumber = "MBN222";
			gateMovementBooking2.GBM_TransportReference = "TREF222";

			var gateMovement2 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement2.GGM_GVM_VehicleMovement = vehicleMovement2.PK;
			gateMovement2.GGM_GBM_MovementBooking = gateMovementBooking2.PK;
			gateMovement2.GGM_IsPickup = true;
			gateMovement2.GGM_RH_NKCargoType = "AFLW";
			gateMovement2.GGM_F3_NKPackageType = "BOT";
			gateMovement2.GGM_TransportReference = gateMovementBooking2.GBM_TransportReference;
			gateMovement2.GGM_UnitNumber = "TSTCONT2";
			gateMovement2.GGM_RC_UnitType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			gateMovement2.GGM_WL_Dock = dock.PK;

			#endregion

			Factory.Save();
		}
	}
}
