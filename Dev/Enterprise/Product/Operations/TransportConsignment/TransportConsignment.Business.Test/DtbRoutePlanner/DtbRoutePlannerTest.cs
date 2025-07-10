using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbRoutePlanner))]
	sealed class DtbRoutePlannerTest : NonPersistentBusinessObjectTestCase
	{
		#region Related Entities

		#region TestAddressPoints

		public void TestAddressPoints()
		{
			var planner = new DtbRoutePlanner(Factory);
			AssertEquals(true, planner.IsRegisteredEditableChildObject(planner.AddressPoints));
			AssertEquals(0, planner.AddressPoints.Count);
			AssertEquals("AddressPoints should be cached.", planner.AddressPoints, planner.AddressPoints);
		}

		#endregion

		#region TestCarriers

		public void TestCarriers()
		{
			var carrier1 = Helper.CreateOrganisation("1");
			var carrier2 = Helper.CreateOrganisation("2");
			var otherOrg = Helper.CreateOrganisation("3");

			carrier1.OH_IsShippingProvider = true;
			carrier2.OH_IsShippingProvider = true;
			otherOrg.OH_IsShippingProvider = false;

			otherOrg.OH_IsLocalTransport = true;
			carrier1.OH_IsLocalTransport = true;
			carrier2.OH_IsLocalTransport = false;
			Factory.Save();

			var planner = new DtbRoutePlanner(Factory);
			AssertEquals(true, planner.Carriers.Contains(carrier1));
			AssertEquals(false, planner.Carriers.Contains(carrier2));
			AssertEquals(false, planner.Carriers.Contains(otherOrg));
			AssertEquals("Carriers should be cached.", planner.Carriers, planner.Carriers);
			AssertEquals(typeof(ActiveBusinessObjectCollection<OrgHeader>), planner.Carriers.GetType());
		}

		#endregion

		#region TestCustomDateOption

		public void TestCustomDateOption()
		{
			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Today;
			AssertEquals("Precondition", ZDate.Empty, planner.CustomDateOption);

			planner.CustomDateOption = ZDate.Today;
			AssertEquals(ZDate.Today, planner.CustomDateOption);

			planner.CustomDateOption = ZDate.BrettsBirthday;
			AssertEquals(ZDate.BrettsBirthday, planner.CustomDateOption);
		}

		#endregion

		#region TestDrivers

		public void TestDrivers()
		{
			var driver1 = Factory.New<GlbStaff>();
			var driver2 = Factory.New<GlbStaff>();
			var driver3 = Factory.New<GlbStaff>();
			driver1.GS_FullName = "Fred Flintstone";
			driver2.GS_FullName = "Barney Rubble";
			driver3.GS_FullName = "Wilma Flintstone";
			driver1.GS_LoginName = "Fred";
			driver2.GS_LoginName = "Barney";
			driver3.GS_LoginName = "Wilma";
			driver3.GS_IsActive = false;

			var driverGroup = Factory.New<GlbGroup>();
			driver1.Groups.Add(driverGroup);
			driver2.Groups.Add(driverGroup);
			driver3.Groups.Add(driverGroup);

			Factory.Save();

			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driverGroup.PK.ToGuid());
			var planner = new DtbRoutePlanner(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { driver1, driver2 }, planner.Drivers);
			AssertEquals("Drivers should be cached.", planner.Drivers, planner.Drivers);
		}

		#endregion

		#region TestVehicles

		public void TestVehicles()
		{
			var vehicle1 = Factory.New<RefEquipment>();
			var vehicle2 = Factory.New<RefEquipment>();
			vehicle1.RQ_ShortCode = "V1";
			vehicle2.RQ_ShortCode = "V2";
			vehicle1.RQ_IsVehicle = true;
			vehicle2.RQ_IsVehicle = true;

			var nonVehicle = Factory.New<RefEquipment>();
			nonVehicle.RQ_ShortCode = "NV";
			nonVehicle.RQ_IsVehicle = false;

			Factory.Save();

			var planner = new DtbRoutePlanner(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { vehicle1, vehicle2 }, planner.Vehicles);
			AssertEquals("Vehicles should be cached.", planner.Vehicles, planner.Vehicles);
		}

		#endregion

		#region TestRunSheets

		[TestDate(2013, 3, 8)] // a Friday
		public void TestRunSheets()
		{
			// run sheets
			var runSheetFromAMonthAgo = Helper.CreateRunSheet(ZDateTimeOffset.Today.AddMonths(-1));
			var runSheetFromTwoDaysAgo = Helper.CreateRunSheet(ZDateTimeOffset.Today.AddDays(-2));
			var runSheetForYesterday = Helper.CreateRunSheet(ZDateTimeOffset.Today.AddDays(-1));
			var runSheetForToday = Helper.CreateRunSheet(ZDateTimeOffset.Today);

			var runSheetForTomorrow1 = Helper.CreateRunSheet(ZDateTimeOffset.Today.AddDays(1));
			var runSheetForTomorrow2 = Helper.CreateRunSheet(ZDateTimeOffset.Today.AddDays(1));
			var runSheetForWednesday = Helper.CreateRunSheet(ZDateTimeOffset.Today.AddDays(5));

			// view modes
			var planner = new DtbRoutePlanner(Factory);

			planner.CurrentDay = RunSheetDay.Yesterday;
			AssertContainsExactElementsInAnyOrder(new[] { runSheetForYesterday }, planner.RunSheetsFilteredForBinding);

			planner.CurrentDay = RunSheetDay.Today;
			AssertContainsExactElementsInAnyOrder(new[] { runSheetForToday }, planner.RunSheetsFilteredForBinding);

			planner.CurrentDay = RunSheetDay.Tomorrow;
			AssertContainsExactElementsInAnyOrder(new[] { runSheetForTomorrow1, runSheetForTomorrow2 }, planner.RunSheetsFilteredForBinding);

			planner.CurrentDay = RunSheetDay.Tuesday;
			AssertEquals("ViewMode is Tuesday, should not return any RunSheets.", 0, planner.RunSheetsFilteredForBinding.Count);

			planner.CurrentDay = RunSheetDay.Wednesday;
			AssertContainsExactElementsInAnyOrder(new[] { runSheetForWednesday }, planner.RunSheetsFilteredForBinding);

			planner.CustomDateOption = ZDate.Today.AddMonths(-1);
			planner.CurrentDay = RunSheetDay.CustomDate;
			AssertContainsExactElementsInAnyOrder(new[] { runSheetFromAMonthAgo }, planner.RunSheetsFilteredForBinding);

			planner.CustomDateOption = ZDate.Today.AddDays(-2);
			AssertContainsExactElementsInAnyOrder(new[] { runSheetFromTwoDaysAgo }, planner.RunSheetsFilteredForBinding);
		}

		#endregion

		#endregion

		#region TestCreateRunSheetForDateInPastHasNoValidationErrors

		public void TestCreateRunSheetForDateInPastHasNoValidationErrors()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Yesterday;

			planner.AssignConfirmationsToRunSheet(Helper.CreateVehicle("TRK"), null, consignment.PickupInstruction.PickupConfirmation);
			AssertNoErrors("Creating a runsheet with dates in the past should not cause validation errors.", planner.RunSheetsFilteredForBinding.First());
		}

		#endregion

		#region TestCreateRunSheetForDateInFutureHasNoValidationErrors

		public void TestCreateRunSheetForDateInFutureHasNoValidationErrors()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Tomorrow;

			planner.AssignConfirmationsToRunSheet(Helper.CreateVehicle("TRK"), null, consignment.PickupInstruction.PickupConfirmation);
			AssertNoErrors("Creating a runsheet with dates in the future should not cause validation errors.", planner.RunSheetsFilteredForBinding.First());
		}

		#endregion

		#region TestCreateNewRunSheet_InCustomDateMode

		[TestUtcOffset(10, 0, 0)]
		public void TestCreateNewRunSheet_InCustomDateMode()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.CustomDate;
			planner.CustomDateOption = ZDate.BrettsBirthday;

			var truck = Helper.CreateVehicle("TRK");
			planner.AssignConfirmationsToRunSheet(truck, null, consignment.PickupInstruction.PickupConfirmation);

			var brettsBirthday = new ZDateTimeOffset(ZDate.BrettsBirthday.ToZDateTime());
			var runSheet = planner.RunSheetsFilteredForBinding.First();
			AssertEquals(brettsBirthday, runSheet.KG_StartTime);
			AssertEquals(brettsBirthday.EndOfDay(), runSheet.KG_EndTime);
		}

		#endregion

		#region TestDataRefreshInvalidatedRunSheetCache

		public void TestDataRefreshInvalidatedRunSheetCache()
		{
			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Tomorrow;
			var runSheet = Helper.CreateRunSheet(ZDateTimeOffset.Today.AddDays(1));

			var newFactory = new BusinessObjectFactory();
			var newPlanner = new DtbRoutePlanner(newFactory);
			newPlanner.CurrentDay = RunSheetDay.Tomorrow;

			AssertEquals(0, newPlanner.RunSheetsFilteredForBinding.Count);
			Factory.Save();
			AssertEquals(1, newPlanner.RunSheetsFilteredForBinding.Count);
		}

		#endregion

		#region TestCopyTransientProperties

		[TestDate(2013, 9, 30)]
		public void TestCopyTransientProperties()
		{
			var runSheetPK = ZGuid.NewZGuid();
			var driverPK = ZGuid.NewZGuid();
			var vehiclePK = ZGuid.NewZGuid();
			var carrierPK = ZGuid.NewZGuid();

			var driver = Helper.CreateDriver("OB", "Ooga Booga");
			var runSheetWithNoDriver = Helper.CreateRunSheet();
			var runSheetForDriver = Helper.CreateRunSheet(driver);

			Factory.Save();

			// create planner and set all state
			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Today;
			((IBindingList)planner.AddressPoints).ApplySort(DtbAddressPoint.GetProperties(typeof(DtbAddressPoint)).Find("AddressPK", false), ListSortDirection.Descending);
			((IBindingList)planner.Carriers).ApplySort(OrgHeader.GetProperties(typeof(OrgHeader)).Find("OH_Code", false), ListSortDirection.Ascending);
			planner.Drivers.ApplySort(GlbStaffSchema.Constants.GS_FullName, ListSortDirection.Ascending);
			planner.RunSheetsFilteredForBinding.ApplySort(DtbConsignmentRunSheetSchema.Constants.KG_GS_NKTruckDriver, ListSortDirection.Descending);
			planner.Vehicles.ApplySort(RefEquipmentSchema.Constants.RQ_ShortCode, ListSortDirection.Ascending);
			planner.SetLastSelectedEntities(runSheetPK, carrierPK, driverPK, vehiclePK);
			planner.CustomDateOption = ZDate.BrettsBirthday;

			// precondition -- ensure the driver filter is constraining the RunSheet collection
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheetWithNoDriver, runSheetForDriver }, planner.RunSheetsFilteredForBinding);
			planner.FilterRunSheetsByDriver(driver);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheetForDriver }, planner.RunSheetsFilteredForBinding);

			// ensure the new planner retains all state
			var newPlanner = new DtbRoutePlanner(new BusinessObjectFactory());
			planner.CopyTransientProperties(newPlanner);
			AssertSort(newPlanner.AddressPoints, "AddressPK", ListSortDirection.Descending);
			AssertSort(newPlanner.Carriers, "OH_Code", ListSortDirection.Ascending);
			AssertSort(newPlanner.Drivers, GlbStaffSchema.Constants.GS_FullName, ListSortDirection.Ascending);
			AssertSort(newPlanner.RunSheetsFilteredForBinding, DtbConsignmentRunSheetSchema.Constants.KG_GS_NKTruckDriver, ListSortDirection.Descending);
			AssertSort(newPlanner.Vehicles, RefEquipmentSchema.Constants.RQ_ShortCode, ListSortDirection.Ascending);
			AssertEquals(1, newPlanner.RunSheetsFilteredForBinding.Count);
			AssertEquals(driver.GS_Code, newPlanner.RunSheetsFilteredForBinding[0].KG_GS_NKTruckDriver);
			AssertEquals(runSheetPK, newPlanner.LastSelectedEntities.RunSheetPK);
			AssertEquals(carrierPK, newPlanner.LastSelectedEntities.CarrierPK);
			AssertEquals(driverPK, newPlanner.LastSelectedEntities.DriverPK);
			AssertEquals(vehiclePK, newPlanner.LastSelectedEntities.VehiclePK);
			AssertEquals(ZDate.BrettsBirthday, newPlanner.CustomDateOption);
		}

		void AssertSort(IBindingList list, string propertyName, ListSortDirection direction)
		{
			AssertEquals(propertyName, list.SortProperty.Name);
			AssertEquals(direction, list.SortDirection);
		}

		#endregion

		#region TestCopyTransientProperties_WorksWithCustomDate

		public void TestCopyTransientProperties_WorksWithCustomDate()
		{
			var runSheet = Helper.CreateRunSheet(new ZDateTimeOffset(ZDateTime.BrettsBirthday, DateTimeKind.Local));
			Factory.Save();

			var planner1 = new DtbRoutePlanner(Factory);
			planner1.CurrentDay = RunSheetDay.CustomDate;
			planner1.CustomDateOption = ZDate.BrettsBirthday;
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheet }, planner1.RunSheetsFilteredForBinding);

			var planner2 = new DtbRoutePlanner(Factory);
			planner1.CopyTransientProperties(planner2);
			AssertContainsExactElementsInAnyOrder(new[] { runSheet }, planner2.RunSheetsFilteredForBinding);
		}

		#endregion

		#region TestFilterRunSheetsByCarrier

		[TestDate(2013, 9, 30)]
		public void TestFilterRunSheetsByCarrier()
		{
			var carrier = Helper.CreateOrganisation("123");
			carrier.OH_IsLocalTransport = true;
			carrier.OH_IsShippingProvider = true;
			var runSheet = Helper.CreateRunSheet();
			var runSheetWithCarrier = Helper.CreateRunSheet(carrier);
			Factory.Save();

			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Today;
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheet, runSheetWithCarrier }, planner.RunSheetsFilteredForBinding);

			planner.FilterRunSheetsByCarrier(carrier);
			AssertContainsExactElementsInAnyOrder(new[] { runSheetWithCarrier }, planner.RunSheetsFilteredForBinding);

			planner.FilterRunSheetsByCarrier(null);
			AssertEquals(0, planner.RunSheetsFilteredForBinding.Count);
		}

		#endregion

		#region TestFilterRunSheetsByChildFilters

		[TestDate(2013, 9, 30)]
		public void TestFilterRunSheetsByChildFilters()
		{
			var carrier = Helper.CreateOrganisation("123");
			carrier.OH_IsLocalTransport = true;
			carrier.OH_IsShippingProvider = true;
			var runSheet = Helper.CreateRunSheet();
			var runSheetWithCarrier = Helper.CreateRunSheet(carrier);
			Factory.Save();

			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Today;
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheet, runSheetWithCarrier }, planner.RunSheetsFilteredForBinding);

			var query = new ZQuery(DtbConsignmentRunSheetSchema.KG_OH_TransportCo, SQLComparisonOperator.Equal, carrier.PK);
			planner.FilterRunSheetsByChildFilters(query);
			AssertContainsExactElementsInAnyOrder(new[] { runSheetWithCarrier }, planner.RunSheetsFilteredForBinding);

			planner.FilterRunSheetsByChildFilters(null);
			AssertEquals(2, planner.RunSheetsFilteredForBinding.Count);
		}

		#endregion

		#region TestFilterRunSheetsByDriver

		[TestDate(2013, 9, 30)]
		public void TestFilterRunSheetsByDriver()
		{
			var driver = Helper.CreateDriver("OB", "Ooga Booga");
			var runSheet = Helper.CreateRunSheet();
			var runSheetWithDriver = Helper.CreateRunSheet(driver);
			Factory.Save();

			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Today;
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheet, runSheetWithDriver }, planner.RunSheetsFilteredForBinding);

			planner.FilterRunSheetsByDriver(driver);
			AssertContainsExactElementsInAnyOrder(new[] { runSheetWithDriver }, planner.RunSheetsFilteredForBinding);

			planner.FilterRunSheetsByDriver(null);
			AssertEquals(0, planner.RunSheetsFilteredForBinding.Count);
		}

		#endregion

		#region TestFilterRunSheetsByVehicle

		[TestDate(2013, 9, 30)]
		public void TestFilterRunSheetsByVehicle()
		{
			var vehicle = Helper.CreateVehicle("V8");
			var runSheet = Helper.CreateRunSheet();
			var runSheetWithVehicle = Helper.CreateRunSheet(vehicle);
			Factory.Save();

			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Today;
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheet, runSheetWithVehicle }, planner.RunSheetsFilteredForBinding);

			planner.FilterRunSheetsByVehicle(vehicle);
			AssertContainsExactElementsInAnyOrder(new[] { runSheetWithVehicle }, planner.RunSheetsFilteredForBinding);

			planner.FilterRunSheetsByVehicle(null);
			AssertEquals(0, planner.RunSheetsFilteredForBinding.Count);
		}

		#endregion

		#region TestClearRunSheetsDriverAndVehicleFilter

		[TestDate(2013, 9, 30)]
		public void TestClearRunSheetsCarrierDriverAndVehicleFilter()
		{
			var driver = Helper.CreateDriver("OB", "Ooga Booga");
			var vehicle = Helper.CreateVehicle("V8");
			var runSheet = Helper.CreateRunSheet();
			var carrier = Helper.CreateOrganisation("123");
			carrier.OH_IsShippingProvider = true;
			var runSheetWithDriver = Helper.CreateRunSheet(driver);
			var runSheetWithVehicle = Helper.CreateRunSheet(vehicle);
			var runSheetWithCarrier = Helper.CreateRunSheet(carrier);
			Factory.Save();

			var planner = new DtbRoutePlanner(Factory);
			planner.CurrentDay = RunSheetDay.Today;

			planner.FilterRunSheetsByDriver(driver);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheetWithDriver }, planner.RunSheetsFilteredForBinding);
			planner.ClearRunSheetsCarrierDriverAndVehicleFilter();
			AssertContainsExactElementsInAnyOrder(new[] { runSheet, runSheetWithDriver, runSheetWithVehicle, runSheetWithCarrier }, planner.RunSheetsFilteredForBinding);

			planner.FilterRunSheetsByVehicle(vehicle);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheetWithVehicle }, planner.RunSheetsFilteredForBinding);
			planner.ClearRunSheetsCarrierDriverAndVehicleFilter();
			AssertContainsExactElementsInAnyOrder(new[] { runSheet, runSheetWithDriver, runSheetWithVehicle, runSheetWithCarrier }, planner.RunSheetsFilteredForBinding);

			planner.FilterRunSheetsByCarrier(carrier);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { runSheetWithCarrier }, planner.RunSheetsFilteredForBinding);
			planner.ClearRunSheetsCarrierDriverAndVehicleFilter();
			AssertContainsExactElementsInAnyOrder(new[] { runSheet, runSheetWithDriver, runSheetWithVehicle, runSheetWithCarrier }, planner.RunSheetsFilteredForBinding);
		}

		#endregion

		#region TestLastSelectedEntities / SetLastSelectedEntities

		public void TestLastSelectedEntities()
		{
			var planner = new DtbRoutePlanner(Factory);
			AssertEquals(ZGuid.Empty, planner.LastSelectedEntities.RunSheetPK);
			AssertEquals(ZGuid.Empty, planner.LastSelectedEntities.CarrierPK);
			AssertEquals(ZGuid.Empty, planner.LastSelectedEntities.DriverPK);
			AssertEquals(ZGuid.Empty, planner.LastSelectedEntities.VehiclePK);

			var runSheetPK = ZGuid.NewZGuid();
			var carrierPK = ZGuid.NewZGuid();
			var driverPK = ZGuid.NewZGuid();
			var vehiclePK = ZGuid.NewZGuid();
			planner.SetLastSelectedEntities(runSheetPK, carrierPK, driverPK, vehiclePK);
			AssertEquals(runSheetPK, planner.LastSelectedEntities.RunSheetPK);
			AssertEquals(carrierPK, planner.LastSelectedEntities.CarrierPK);
			AssertEquals(driverPK, planner.LastSelectedEntities.DriverPK);
			AssertEquals(vehiclePK, planner.LastSelectedEntities.VehiclePK);
		}

		#endregion

		#region Implementation

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
