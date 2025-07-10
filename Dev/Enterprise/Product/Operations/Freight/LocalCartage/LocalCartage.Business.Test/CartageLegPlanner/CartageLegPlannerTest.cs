using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageLegPlanner))]
	internal class CartageLegPlannerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDrivers()
		{
			var driver1 = Factory.New<GlbStaff>();
			var driver2 = Factory.New<GlbStaff>();
			driver1.GS_FullName = "Fred Flintstone";
			driver2.GS_FullName = "Barney Rubble";
			driver1.GS_LoginName = "Fred";
			driver2.GS_LoginName = "Barney";
			var driverGroup = Factory.New<GlbGroup>();
			driver1.Groups.Add(driverGroup);
			driver2.Groups.Add(driverGroup);
			Factory.Save();
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driverGroup.PK.ToGuid());
			var planner = new CartageLegPlanner(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { driver1, driver2 }, planner.Drivers);
			AssertEquals("Drivers should be cached.", planner.Drivers, planner.Drivers);
		}

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
			var planner = new CartageLegPlanner(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { vehicle1, vehicle2 }, planner.Vehicles);
			AssertEquals("Vechicles should be cached.", planner.Vehicles, planner.Vehicles);
		}

		public void TestWorkSheets()
		{
			CartageLegPlanner planner = new CartageLegPlanner(Factory);
			AssertEquals(0, planner.WorkSheets.Count);
			CommonWorkSheet ws1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet ws2 = Factory.New<CommonWorkSheet>();
			Factory.Save();
			planner.WorkSheets.AdditionalFilter = new ZQuery();
			AssertNotNull(planner.WorkSheets.FindByPK(ws1.PK));
			AssertNotNull(planner.WorkSheets.FindByPK(ws2.PK));
		}

		// workSheet1             |                 ***********|***********			current
		// workSheet2  ***********|***********                 |					current
		// workSheet3  ***********|****************************|***********			current	
		// workSheet4             |       *************        |					current
		// workSheet5             |                            |   *************	not current
		//             -----------|----------------------------|----------------->
		// 			    from (today 00:00)            to (tomorrow 00:00)
		[TestDate(2016, 9, 13)]
		public void TestWorkSheets_SingleDay()
		{
			var planner = new CartageLegPlanner(Factory);
			AssertEquals("Precondition", 0, planner.WorkSheets.Count);
			var workSheet1 = Factory.New<CommonWorkSheet>();
			workSheet1.EY_StartTime = ZDateTime.Today.AddHours(7);
			workSheet1.EY_EndTime = ZDateTime.Today.AddDays(1).AddHours(7);
			var workSheet2 = Factory.New<CommonWorkSheet>();
			workSheet2.EY_StartTime = ZDateTime.Today.AddDays(-1).AddHours(7);
			workSheet2.EY_EndTime = ZDateTime.Today.AddHours(7);
			var workSheet3 = Factory.New<CommonWorkSheet>();
			workSheet3.EY_StartTime = ZDateTime.Today.AddDays(-1).AddHours(7);
			workSheet3.EY_EndTime = ZDateTime.Today.AddDays(1).AddHours(7);
			var workSheet4 = Factory.New<CommonWorkSheet>();
			workSheet4.EY_StartTime = ZDateTime.Today.AddHours(7);
			workSheet4.EY_EndTime = ZDateTime.Today.AddHours(17);
			var workSheet5 = Factory.New<CommonWorkSheet>();
			workSheet5.EY_StartTime = ZDateTime.Today.AddDays(1).AddHours(7);
			workSheet5.EY_EndTime = ZDateTime.Today.AddDays(1).AddHours(17);
			planner.DriversWorkSheetSidePanelMode = "Run Sheets - Today";
			AssertEquals(4, planner.WorkSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { workSheet1, workSheet2, workSheet3, workSheet4 }, planner.WorkSheets);
			planner.DriversWorkSheetSidePanelMode = "Run Sheets - Tomorrow";
			AssertEquals(3, planner.WorkSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { workSheet1, workSheet3, workSheet5 }, planner.WorkSheets);
			planner.DriversWorkSheetSidePanelMode = "Run Sheets - Yesterday";
			AssertEquals(2, planner.WorkSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { workSheet2, workSheet3 }, planner.WorkSheets);
		}

		[TestDate(2009, 8, 21, 17, 51, 0)]
		public void TestDriversWorkSheetSidePanelModes()
		{
			CodeDescriptionPairList list = new CartageLegPlanner(Factory).DriversWorkSheetSidePanelModes;
			AssertEquals(11, list.Count);
			AssertEquals("Drivers", list[0].Code);
			AssertEquals("Vehicles", list[1].Code);
			AssertEquals("Run Sheets - Yesterday", list[2].Code);
			AssertEquals("Run Sheets - Today", list[3].Code);
			AssertEquals("Run Sheets - Tomorrow", list[4].Code);
			AssertEquals("Run Sheets - Sunday", list[5].Code);
			AssertEquals("Run Sheets - Monday", list[6].Code);
			AssertEquals("Run Sheets - Tuesday", list[7].Code);
			AssertEquals("Run Sheets - Wednesday", list[8].Code);
			AssertEquals("Run Sheets - Thursday", list[9].Code);
			AssertEquals("Run Sheets - Friday", list[10].Code);
		}

		public void TestCartageLegs()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonBookedCtgMove move2 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move2.CartageLegs.AddNew();
			Factory.Save();
			CartageLegPlanner planner = new CartageLegPlanner(Factory);
			AssertEquals("Should have no legs", 0, planner.CartageLegs.Count);
			planner.CartageLegs.AddRange(new[] { leg1, leg2 });
			AssertEquals("Should have 2 legs", 2, planner.CartageLegs.Count);
			Assert("Should have NO changes", !planner.HasChanges);
			leg2.JU_DeliverySignedFor = "Bob";
			Assert("Should have changes", planner.HasChanges);
		}

		public void TestSwapWith()
		{
			var planner = new CartageLegPlanner(Factory);
			planner.CartageLegs.Sort(JobContainerLegsSchema.Constants.JU_PickupTimeIn, ListSortDirection.Descending);
			planner.Drivers.ApplySort(GlbStaffSchema.Constants.GS_FullName, ListSortDirection.Ascending);
			planner.WorkSheets.ApplySort(JobCartageRunSheetSchema.Constants.EY_RunSheetNumber, ListSortDirection.Descending);
			planner.Vehicles.ApplySort(RefEquipmentSchema.Constants.RQ_ShortCode, ListSortDirection.Ascending);
			planner.DriversWorkSheetSidePanelMode = "bob";
			var newPlanner = new CartageLegPlanner(new BusinessObjectFactory());
			newPlanner.SwapWith(planner);
			AssertSort(newPlanner.CartageLegs, JobContainerLegsSchema.Constants.JU_PickupTimeIn, ListSortDirection.Descending);
			AssertSort(newPlanner.Drivers, GlbStaffSchema.Constants.GS_FullName, ListSortDirection.Ascending);
			AssertSort(newPlanner.WorkSheets, JobCartageRunSheetSchema.Constants.EY_RunSheetNumber, ListSortDirection.Descending);
			AssertSort(newPlanner.Vehicles, RefEquipmentSchema.Constants.RQ_ShortCode, ListSortDirection.Ascending);
			AssertEquals("bob", newPlanner.DriversWorkSheetSidePanelMode);
		}

		void AssertSort(IBindingList list, string propertyName, ListSortDirection direction)
		{
			AssertEquals(propertyName, list.SortProperty.Name);
			AssertEquals(direction, list.SortDirection);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CartageLegPlanner(Factory);
		}
	}
}
