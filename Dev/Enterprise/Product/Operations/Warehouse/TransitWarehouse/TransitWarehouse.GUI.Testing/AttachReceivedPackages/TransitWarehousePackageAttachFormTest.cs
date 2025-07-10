using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Packing.Business;
using Enterprise.Packing.GUI;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(WhsItemPackageStateFilterBusinessObject.Schema))]

namespace Enterprise.Warehouse.Transit.GUI.Testing
{
	[TestedType(typeof(TransitWarehousePackageAttachForm))]
	class TransitWarehousePackageAttachFormTest : ZFormBasherTest
	{
		#region TestAttachedHandlingUnit

		public void TestAttachedHandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = handlingUnitPackage.ReceiveTransportationUnit.Warehouse.WW_OA_WarehouseAddress;
			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var outerPackagesFilter = (ModuleFlagsFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.OuterPackage];
				outerPackagesFilter.Property0 = false;
				outerPackagesFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;
				var removeButton = filterControl.RemoveToolStripButton;
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { handlingUnitPackage, childPackageState1, childPackageState2 }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectSingleElement(handlingUnitPackage);
				attachButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { childPackageState1, childPackageState2 }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(childPackageState1);
				removeButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { childPackageState1 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { childPackageState2 }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				availablePackagesGrid.Grid.SelectSingleElement(childPackageState1);
				attachButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { childPackageState1, childPackageState2 }, attachedPackagesGrid.List);
			}
		}

		#endregion

		#region TestAttachAndRemoveButton_PackagesUnAssignedThroughAdditionalReference

		public void TestAttachAndRemoveButton_PackagesUnAssignedThroughAdditionalReference()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var arrived = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, receiveTransportationUnit);
			putaway.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT", TransitWarehouseStatuses.Codes.Committed, receiveTransportationUnit);
			committed.WPS_WL_LastLocation = location.PK;
			committed.WPS_WL_ReceiveLocation = location.PK;

			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC", TransitWarehouseStatuses.Codes.Picked, receiveTransportationUnit, dispatchLoadList: dispatchLoadList);
			picked.WPS_WL_LastLocation = location.PK;
			picked.WPS_WL_ReceiveLocation = location.PK;

			var staged = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA", TransitWarehouseStatuses.Codes.Staged, receiveTransportationUnit, dispatchLoadList: dispatchLoadList);
			staged.WPS_WL_LastLocation = location.PK;
			staged.WPS_WL_ReceiveLocation = location.PK;

			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			departed.WPS_WL_LastLocation = location.PK;
			departed.WPS_WL_ReceiveLocation = location.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			freightLoaded.WPS_WL_LastLocation = location.PK;
			freightLoaded.WPS_WL_ReceiveLocation = location.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";

			var packageWithDCN = Helper.CreatePackageState(rcn, 1, "PKG", "DCN_PKG", TransitWarehouseStatuses.Codes.Arrived, receiveTransportationUnit, Helper.CreateDispatchConsignment("DSPC1", warehouse.PK));

			Factory.Save();

			AssertAttachingPackageStateInControl_PackagesUnAssignedViaAdditionalReference(arrived, new List<WhsItemPackageState> { arrived, putaway, committed, picked, staged });
			AssertAttachingPackageStateInControl_PackagesUnAssignedViaAdditionalReference(putaway, new List<WhsItemPackageState> { arrived, putaway, committed, picked, staged });
			AssertAttachingPackageStateInControl_PackagesUnAssignedViaAdditionalReference(committed, new List<WhsItemPackageState> { arrived, putaway, committed, picked, staged });
			AssertAttachingPackageStateInControl_PackagesUnAssignedViaAdditionalReference(picked, new List<WhsItemPackageState> { arrived, putaway, committed, picked, staged });
			AssertAttachingPackageStateInControl_PackagesUnAssignedViaAdditionalReference(staged, new List<WhsItemPackageState> { arrived, putaway, committed, picked, staged });
		}

		void AssertAttachingPackageStateInControl_PackagesUnAssignedViaAdditionalReference(WhsItemPackageState packageStateToTest, List<WhsItemPackageState> expectedPackageStatesInAvailableGrid)
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = packageStateToTest.ReceiveTransportationUnit.Warehouse.WW_OA_WarehouseAddress;
			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;
				var removeButton = filterControl.RemoveToolStripButton;
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(expectedPackageStatesInAvailableGrid, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectSingleElement(packageStateToTest);
				attachButton.PerformClick();
				expectedPackageStatesInAvailableGrid.Remove(packageStateToTest);
				AssertContainsExactElementsInAnyOrder(expectedPackageStatesInAvailableGrid, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new[] { packageStateToTest }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(packageStateToTest);
				removeButton.PerformClick();
				expectedPackageStatesInAvailableGrid.Add(packageStateToTest);
				AssertContainsExactElementsInAnyOrder(expectedPackageStatesInAvailableGrid, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);
			}
		}

		#endregion

		#region TestAttachAndRemoveButton_PackagesAssignedThroughUnloadedVehicleNumber

		public void TestAttachAndRemoveButton_PackagesAssignedThroughUnloadedVehicleNumber()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");

			var truckRTU = Helper.CreateReceiveTransportationUnit("TruckRTU", warehouse.PK, location.PK);
			truckRTU.WRH_VehicleReference = "Truck";

			var freightRTU = Helper.CreateReceiveTransportationUnit("FreightRTU", warehouse.PK, location.PK);
			freightRTU.WRH_VehicleReference = "Freighter";

			var rocketRTU = Helper.CreateReceiveTransportationUnit("RocketRTU", warehouse.PK, location.PK);
			rocketRTU.WRH_VehicleReference = "Challenger";

			var truckPackage = Helper.CreatePackageState(rcn, 1, "PKG", "Package from truck", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: truckRTU);
			truckPackage.WPS_WRH_TransitReceiveHeader = truckRTU.PK;

			var freightShipPackage = Helper.CreatePackageState(rcn, 1, "PKG", "Package from freight ship", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: freightRTU);
			freightShipPackage.WPS_WRH_TransitReceiveHeader = freightRTU.PK;

			var rocketPackage = Helper.CreatePackageState(rcn, 1, "PKG", "Package from rocket", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rocketRTU);
			rocketPackage.WPS_WRH_TransitReceiveHeader = rocketRTU.PK;

			parent.TransitWarehouseAddressPK = truckPackage.ReceiveTransportationUnit.Warehouse.WW_OA_WarehouseAddress;

			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;
				var removeButton = filterControl.RemoveToolStripButton;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { truckPackage, freightShipPackage, rocketPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				availablePackagesGrid.Grid.SelectSingleElement(truckPackage);
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { freightShipPackage, rocketPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { truckPackage }, attachedPackagesGrid.List);

				availablePackagesGrid.Grid.SelectSingleElement(freightShipPackage);
				attachButton.PerformClick();
				availablePackagesGrid.Grid.SelectSingleElement(rocketPackage);
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>(), availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { truckPackage, freightShipPackage, rocketPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(truckPackage);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { truckPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { freightShipPackage, rocketPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(freightShipPackage);
				removeButton.PerformClick();
				attachedPackagesGrid.SelectSingleElement(rocketPackage);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { truckPackage, freightShipPackage, rocketPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);
			}
		}

		#endregion

		#region TestAttachAndRemoveButton_PackagesAssignedThroughUnloadCompleteTime

		public void TestAttachAndRemoveButton_PackagesAssignedThroughUnloadCompleteTime()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);

			var yesterdayRTU = Helper.CreateReceiveTransportationUnit("Yesterday RTU", warehouse.PK, location.PK);
			yesterdayRTU.WRH_UnloadCompleteTime = dateTimeOffset.AddDays(-1);
			yesterdayRTU.WRH_UnloadCompleteNotYetProcessedTime = yesterdayRTU.WRH_UnloadCompleteTime;
			yesterdayRTU.WRH_GateInTime = dateTimeOffset.AddDays(-2);

			var lastWeekRTU = Helper.CreateReceiveTransportationUnit("Last week RTU", warehouse.PK, location.PK);
			lastWeekRTU.WRH_UnloadCompleteTime = dateTimeOffset.AddDays(-7);
			lastWeekRTU.WRH_UnloadCompleteNotYetProcessedTime = lastWeekRTU.WRH_UnloadCompleteTime;
			lastWeekRTU.WRH_GateInTime = dateTimeOffset.AddDays(-8);

			var tomorrowRTU = Helper.CreateReceiveTransportationUnit("Tomorrow RTU", warehouse.PK, location.PK);
			tomorrowRTU.WRH_UnloadCompleteTime = dateTimeOffset.AddDays(1);
			tomorrowRTU.WRH_UnloadCompleteNotYetProcessedTime = tomorrowRTU.WRH_UnloadCompleteTime;
			tomorrowRTU.WRH_GateInTime = dateTimeOffset;

			var yesterdayPackage = Helper.CreatePackageState(rcn, 1, "PKG", "Package from yesterday", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: yesterdayRTU);
			yesterdayPackage.WPS_WRH_TransitReceiveHeader = yesterdayRTU.PK;

			var lastWeekPackage = Helper.CreatePackageState(rcn, 1, "PKG", "Package from last week", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: lastWeekRTU);
			lastWeekPackage.WPS_WRH_TransitReceiveHeader = lastWeekRTU.PK;

			var tomorrowPackage = Helper.CreatePackageState(rcn, 1, "PKG", "Package from tomorrow", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: tomorrowRTU);
			tomorrowPackage.WPS_WRH_TransitReceiveHeader = tomorrowRTU.PK;

			parent.TransitWarehouseAddressPK = yesterdayPackage.ReceiveTransportationUnit.Warehouse.WW_OA_WarehouseAddress;

			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;
				var removeButton = filterControl.RemoveToolStripButton;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { yesterdayPackage, lastWeekPackage, tomorrowPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				availablePackagesGrid.Grid.SelectSingleElement(yesterdayPackage);
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { lastWeekPackage, tomorrowPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { yesterdayPackage }, attachedPackagesGrid.List);

				availablePackagesGrid.Grid.SelectSingleElement(lastWeekPackage);
				attachButton.PerformClick();
				availablePackagesGrid.Grid.SelectSingleElement(tomorrowPackage);
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>(), availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { yesterdayPackage, lastWeekPackage, tomorrowPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(yesterdayPackage);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { yesterdayPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { lastWeekPackage, tomorrowPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(lastWeekPackage);
				removeButton.PerformClick();
				attachedPackagesGrid.SelectSingleElement(tomorrowPackage);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { yesterdayPackage, lastWeekPackage, tomorrowPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);
			}
		}

		#endregion

		#region TestAttachAndRemoveButton_PackagesAssignedThroughPackType

		public void TestAttachAndRemoveButton_PackagesAssignedThroughPackType()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packagePackage = Helper.CreatePackageState(rcn, 1, "PKG", "Package with package", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var containerPackage = Helper.CreatePackageState(rcn, 1, "CNT", "Package with container", TransitWarehouseStatuses.Codes.Arrived, receiveTransportationUnit);
			var coilPackage = Helper.CreatePackageState(rcn, 1, "COI", "Package with coil", TransitWarehouseStatuses.Codes.Arrived, receiveTransportationUnit);
			var palletPackage = Helper.CreatePackageState(rcn, 1, "PLT", "Package with pallet", TransitWarehouseStatuses.Codes.Arrived, receiveTransportationUnit, dispatchLoadList: dispatchLoadList);

			parent.TransitWarehouseAddressPK = packagePackage.ReceiveTransportationUnit.Warehouse.WW_OA_WarehouseAddress;

			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;
				var removeButton = filterControl.RemoveToolStripButton;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { packagePackage, containerPackage, coilPackage, palletPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				availablePackagesGrid.Grid.SelectSingleElement(packagePackage);
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { containerPackage, coilPackage, palletPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { packagePackage }, attachedPackagesGrid.List);

				availablePackagesGrid.Grid.SelectSingleElement(containerPackage);
				attachButton.PerformClick();
				availablePackagesGrid.Grid.SelectSingleElement(coilPackage);
				attachButton.PerformClick();
				availablePackagesGrid.Grid.SelectSingleElement(palletPackage);
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>(), availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { packagePackage, containerPackage, coilPackage, palletPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(packagePackage);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { packagePackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { containerPackage, coilPackage, palletPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(containerPackage);
				removeButton.PerformClick();
				attachedPackagesGrid.SelectSingleElement(coilPackage);
				removeButton.PerformClick();
				attachedPackagesGrid.SelectSingleElement(palletPackage);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { packagePackage, containerPackage, coilPackage, palletPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);
			}
		}

		#endregion

		#region TestAttachAndRemoveButton_PackagesAssignedThroughAdditionalReference

		public void TestAttachAndRemoveButton_PackagesAssignedThroughAdditionalReference()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var arrived = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, receiveTransportationUnit);
			putaway.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT", TransitWarehouseStatuses.Codes.Committed, receiveTransportationUnit);
			committed.WPS_WL_LastLocation = location.PK;
			committed.WPS_WL_ReceiveLocation = location.PK;

			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC", TransitWarehouseStatuses.Codes.Picked, receiveTransportationUnit, dispatchLoadList: dispatchLoadList);
			picked.WPS_WL_LastLocation = location.PK;
			picked.WPS_WL_ReceiveLocation = location.PK;

			var staged = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA", TransitWarehouseStatuses.Codes.Staged, receiveTransportationUnit, dispatchLoadList: dispatchLoadList);
			staged.WPS_WL_LastLocation = location.PK;
			staged.WPS_WL_ReceiveLocation = location.PK;

			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			departed.WPS_WL_LastLocation = location.PK;
			departed.WPS_WL_ReceiveLocation = location.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			freightLoaded.WPS_WL_LastLocation = location.PK;
			freightLoaded.WPS_WL_ReceiveLocation = location.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";

			var packageWithDCN = Helper.CreatePackageState(rcn, 1, "PKG", "DCN_PKG", TransitWarehouseStatuses.Codes.Arrived, receiveTransportationUnit, Helper.CreateDispatchConsignment("DSPC1", warehouse.PK));

			Factory.Save();

			AssertAttachingPackageStateInControl_PackagesAssignedViaAdditionalReference(arrived, new List<WhsItemPackageState> { putaway, committed, picked, staged });
			AssertAttachingPackageStateInControl_PackagesAssignedViaAdditionalReference(putaway, new List<WhsItemPackageState> { arrived, committed, picked, staged });
			AssertAttachingPackageStateInControl_PackagesAssignedViaAdditionalReference(committed, new List<WhsItemPackageState> { arrived, putaway, picked, staged });
			AssertAttachingPackageStateInControl_PackagesAssignedViaAdditionalReference(picked, new List<WhsItemPackageState> { arrived, putaway, committed, staged });
			AssertAttachingPackageStateInControl_PackagesAssignedViaAdditionalReference(staged, new List<WhsItemPackageState> { arrived, putaway, committed, picked });

			SetPackagesWeightVolume(1, 2, new List<WhsItemPackageState> { arrived, putaway, committed, picked, staged });
			Factory.Save();
			AssertAttachingPackageStateInControl_TotalPackages_TotalWeight_TotalVolume(warehouse, "5,", "5 KG,", "10 M3", new List<WhsItemPackageState> { arrived, putaway, committed, picked, staged });
		}

		void AssertAttachingPackageStateInControl_PackagesAssignedViaAdditionalReference(WhsItemPackageState packageStateWithAdditionalReference, List<WhsItemPackageState> expectedPackageStatesInAvailableGrid)
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = packageStateWithAdditionalReference.ReceiveTransportationUnit.Warehouse.WW_OA_WarehouseAddress;
			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			packageStateWithAdditionalReference.ParentJobNumber = transitWarehouseParent.JobNumber;
			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;
				var removeButton = filterControl.RemoveToolStripButton;
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(expectedPackageStatesInAvailableGrid, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new[] { packageStateWithAdditionalReference }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(packageStateWithAdditionalReference);
				removeButton.PerformClick();
				expectedPackageStatesInAvailableGrid.Add(packageStateWithAdditionalReference); // Newly removed pacakge is now added to Available Packages grid
				AssertContainsExactElementsInAnyOrder(expectedPackageStatesInAvailableGrid, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				availablePackagesGrid.Grid.SelectSingleElement(packageStateWithAdditionalReference);
				attachButton.PerformClick();
				expectedPackageStatesInAvailableGrid.Remove(packageStateWithAdditionalReference);
				AssertContainsExactElementsInAnyOrder(expectedPackageStatesInAvailableGrid, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new[] { packageStateWithAdditionalReference }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(packageStateWithAdditionalReference);
				removeButton.PerformClick();
				expectedPackageStatesInAvailableGrid.Add(packageStateWithAdditionalReference);
				AssertContainsExactElementsInAnyOrder(expectedPackageStatesInAvailableGrid, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);
			}

			packageStateWithAdditionalReference.ParentJobNumber = "";
			Factory.Save();
		}

		void AssertAttachingPackageStateInControl_TotalPackages_TotalWeight_TotalVolume(WhsWarehouse warehouse, string expectedTotalPackages, string expectedTotalWeight, string expectedTotalVolume,
			List<WhsItemPackageState> packages)
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			var transitWarehouseParent = (ITransitWarehouseParent)parent;

			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;
				var removeButton = filterControl.RemoveToolStripButton;

				var totalPackagesControl = (ZLabel)form.Controls.Find("PacksCount", true).Single();
				var totalWeightControl = (ZLabel)form.Controls.Find("WeightUQ", true).Single();
				var totalVolumeControl = (ZLabel)form.Controls.Find("VolumeUQ", true).Single();

				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(packages, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				AssertEquals("0,", totalPackagesControl.Text);
				AssertEquals("0 KG,", totalWeightControl.Text);
				AssertEquals("0 M3", totalVolumeControl.Text);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(packages, attachedPackagesGrid.List);

				AssertEquals(expectedTotalPackages, totalPackagesControl.Text);
				AssertEquals(expectedTotalWeight, totalWeightControl.Text);
				AssertEquals(expectedTotalVolume, totalVolumeControl.Text);

				attachedPackagesGrid.SelectAllElements();
				removeButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(packages, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				AssertEquals("0,", totalPackagesControl.Text);
				AssertEquals("0 KG,", totalWeightControl.Text);
				AssertEquals("0 M3", totalVolumeControl.Text);
			}
		}

		void SetPackagesWeightVolume(decimal weight, decimal volume, List<WhsItemPackageState> packagesWithWeightAndVolume)
		{
			foreach (var package in packagesWithWeightAndVolume)
			{
				package.Package.KP_Weight = weight;
				package.Package.KP_Volume = volume;
			}
		}

		#endregion

		#region TestAttachButton_AttachingPackageIsAlreadyInAnotherParent

		public void TestAttachButton_AttachingPackageIsAlreadyInAnotherParent()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Factory.Save();

			var parent2 = Factory.New<DummyBizOWithPackLines>();
			parent2.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			parent2.JobNumber = "SHP001";

			using (var form = new TransitWarehousePackageAttachForm(parent2))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;

				var parentJobNumberFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ActiveModuleFilters.Single(f => f.Description == "ParentJobNumber");
				parentJobNumberFilter.IsActive = false;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { package1, package2 }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Cannot assign any package that has been attached to another Job or added to a Dispatch Consignment."));
			}
		}

		#endregion

		#region TestAttachButton_AttachingPackageIsInDCN

		public void TestAttachButton_AttachingPackageIsInDCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;

				var dispatchExternalReferenceFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ActiveModuleFilters.Single(f => f.Description == "DispatchExternalReference");
				dispatchExternalReferenceFilter.IsActive = false;
				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { package1, package2 }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Cannot assign any package that has been attached to another Job or added to a Dispatch Consignment."));
			}
		}

		public void TestAttachButton_AttachingPackageIsInDCN_ChildPackageOfHandlingUnitIsInDCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU001", handlingUnit, rtu);
			var innerPackageInDCN = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dispatchConsignment, dispatchUnit: dtu, dispatchLoadList: dispatchLoadList);
			innerPackageInDCN.WPS_WL_LastLocation = location.PK;
			innerPackageInDCN.WPS_WL_ReceiveLocation = location.PK;
			innerPackageInDCN.WPS_IsSecure = true;
			innerPackageInDCN.WPS_SecurityStatus = "SEC";
			var innerPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			innerPackage.WPS_WL_LastLocation = location.PK;
			innerPackage.WPS_WL_ReceiveLocation = location.PK;
			innerPackage.WPS_IsSecure = true;
			innerPackage.WPS_SecurityStatus = "SEC";

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, innerPackageInDCN, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);

				var dispatchExtFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.DispatchExternalReference];
				dispatchExtFilter.IsActive = true;
				dispatchExtFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

				var attachButton = filterControl.AssignToolStripButton;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { handlingUnitPackage, innerPackage }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Cannot assign any package that has been attached to another Job or added to a Dispatch Consignment."));
			}
		}

		#endregion

		#region TestRemoveAndAttachAlreadyAttachedPackage

		public void TestRemoveAndAttachAlreadyAttachedPackage()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var attachedPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			var attachedPackage2 = Helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			var availablePackage = Helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			Factory.Save();

			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;
				var removeButton = filterControl.RemoveToolStripButton;
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new[] { availablePackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage1, attachedPackage2 }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(attachedPackage2);
				removeButton.PerformClick();
				Assert(!attachedPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				Assert(attachedPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage2, availablePackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage1 }, attachedPackagesGrid.List);

				availablePackagesGrid.Grid.SelectSingleElement(attachedPackage2);
				attachButton.PerformClick();
				Assert(!attachedPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				Assert(!attachedPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				AssertContainsExactElementsInAnyOrder(new[] { availablePackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage1, attachedPackage2 }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				var createButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				bool isAttachedPackagesCalled = false;
				bool isRemovePackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };
				parent.RemovePackagesCalled += (p) => { isRemovePackagesCalled = true; };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createButton.PerformClick();

				Assert(isAttachedPackagesCalled);
				Assert(!isRemovePackagesCalled);
				Assert(!attachedPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				Assert(!attachedPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"You did not add all packages from the following RCNs:
EXTREF1 - RCN1

Continue anyway?"));
			}
		}

		#endregion

		#region TestAttachAndRemoveAvailablePackage

		public void TestAttachAndRemoveAvailablePackage()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var pkg1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var attachedPackage = Helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);

			Factory.Save();

			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;
				var removeButton = filterControl.RemoveToolStripButton;
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new[] { pkg1 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage }, attachedPackagesGrid.List);

				availablePackagesGrid.Grid.SelectSingleElement(pkg1);
				attachButton.PerformClick();
				AssertEquals(0, availablePackagesGrid.Grid.List.Count);
				AssertContainsExactElementsInAnyOrder(new[] { pkg1, attachedPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(pkg1);
				removeButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new[] { pkg1 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				var createButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				bool isAttachedPackagesCalled = false;
				bool isRemovePackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };
				parent.RemovePackagesCalled += (p) => { isRemovePackagesCalled = true; };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createButton.PerformClick();

				Assert(isAttachedPackagesCalled);
				Assert(!isRemovePackagesCalled);
				Assert(!pkg1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				Assert(!attachedPackage.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"You did not add all packages from the following RCNs:
EXTREF1 - RCN1

Continue anyway?"));
			}
		}

		#endregion

		#region TestCreatePackLineButton_HasRemovedPackages

		public void TestCreatePackLineButton_HasRemovedPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var standalonePackageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKGS", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			var childPackageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var outerPackagesFilter = (ModuleFlagsFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.OuterPackage];
				outerPackagesFilter.Property0 = false;
				outerPackagesFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var removeButton = filterControl.RemoveToolStripButton;

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { standalonePackageState, childPackageState1, childPackageState2 }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(standalonePackageState);
				removeButton.PerformClick();
				attachedPackagesGrid.SelectSingleElement(childPackageState1);
				removeButton.PerformClick();
				attachedPackagesGrid.SelectSingleElement(childPackageState2);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { standalonePackageState, handlingUnitPackage, childPackageState1, childPackageState2 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				var isAttachedPackagesCalled = false;
				var isRemovePackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };
				parent.RemovePackagesCalled += (p) => { isRemovePackagesCalled = true; };

				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				createPackLinesButton.PerformClick();

				Assert(isAttachedPackagesCalled);
				Assert(isRemovePackagesCalled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You have removed 3 Package(s). These Packlines will be removed from the Shipment."));
			}
		}

		#endregion

		#region TestCreatePackLineButton_HasRemovedOVPs

		public void TestCreatePackLineButton_HasRemovedOVPsAndPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var standalonePackageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKGS", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			var childPackageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var overpackPackage = Helper.CreateOverpackPackage("OVP1", receiveConsignment, receiveTransportationUnit, rcn: receiveConsignment, entryNum: parent.JobNumber);

			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", overpackPackage);

			Factory.Save();

			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var extRefFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.ReceiveExternalReference];
				extRefFilter.IsActive = true;
				extRefFilter.Property = "RC1";
				extRefFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var removeButton = filterControl.RemoveToolStripButton;

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { standalonePackageState, overpackPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(overpackPackage);
				removeButton.PerformClick();
				attachedPackagesGrid.SelectSingleElement(standalonePackageState);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { standalonePackageState, overpackPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				var isAttachedPackagesCalled = false;
				var isRemovePackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };
				parent.RemovePackagesCalled += (p) => { isRemovePackagesCalled = true; };

				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				createPackLinesButton.PerformClick();

				Assert(isAttachedPackagesCalled);
				Assert(isRemovePackagesCalled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You have removed 1 Package(s) and 1 Overpack(s). These Packlines will be removed from the Shipment.\r\nDetaching an overpack with inner packlines will remove the inner packlines from the Shipment."));
			}
		}

		public void TestCreatePackLineButton_HasRemovedOVPs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var overpackPackage = Helper.CreateOverpackPackage("OVP1", receiveConsignment, receiveTransportationUnit, rcn: receiveConsignment, entryNum: parent.JobNumber);

			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", overpackPackage);

			Factory.Save();

			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var extRefFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.ReceiveExternalReference];
				extRefFilter.IsActive = true;
				extRefFilter.Property = "RC1";
				extRefFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var removeButton = filterControl.RemoveToolStripButton;

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { overpackPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(overpackPackage);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { overpackPackage }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				var isAttachedPackagesCalled = false;
				var isRemovePackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };
				parent.RemovePackagesCalled += (p) => { isRemovePackagesCalled = true; };

				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				createPackLinesButton.PerformClick();

				Assert(isAttachedPackagesCalled);
				Assert(isRemovePackagesCalled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You have removed 1 Overpack(s). These Packlines will be removed from the Shipment.\r\nDetaching an overpack with inner packlines will remove the inner packlines from the Shipment."));
			}
		}

		#endregion

		#region TestPackageDetailsControl

		public void TestAttachedPackageDetailsDialog()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var attachedPackage = Helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);

			Factory.Save();

			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			Factory.Save();

			using (var packageDetailsDialogForm = new PackageDetailsUserControlDialogForm(attachedPackage.Package))
			{
				packageDetailsDialogForm.Show();
				var packageDetailsUserControl = (PackageDetailUserControl)packageDetailsDialogForm.Controls.Find("PackageDetailUserControl", true).Single();
				AssertEquals(true, packageDetailsUserControl.Visible);

				var packQtyCalcEdit = (ZCalcEdit)packageDetailsUserControl.Controls.Find("PackQtyCalcEdit", true).Single();
				var packageIDTextBox = (ZTextBox)packageDetailsUserControl.Controls.Find("PackageIDTextBox", true).Single();
				var transportRefTextBox = (ZTextBox)packageDetailsUserControl.Controls.Find("TransportRefTextBox", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals(true, packQtyCalcEdit.ReadOnly);
					AssertEquals(true, packageIDTextBox.ReadOnly);
					AssertEquals(true, transportRefTextBox.ReadOnly);
				});
			}
		}

		#endregion

		#region TestAvailablePackagesGrid_Has_PackageDetailsMenuItem

		public void TestAttachedPackagesGrid_Has_PackageDetailsMenuItem()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var pkg1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var attachedPackage = Helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);

			Factory.Save();

			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage }, attachedPackagesGrid.List);

				AssertNotNull(attachedPackagesGrid.ContextMenu.MenuItems.Find("View Details", true));
			}
		}

		#endregion

		#region TestFilterGrid_Has_EDocsMenuItem

		public void TestFilterGrid_Has_EDocsMenuItem()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var pkg1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var attachedPackage = Helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);

			Factory.Save();

			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			Factory.Save();

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage }, attachedPackagesGrid.List);

				AssertNotNull(attachedPackagesGrid.ContextMenu.MenuItems.Find("View eDocs", true));
			}
		}

		#endregion

		#region TestAttachButton_AttachingPackagesWithoutRCN

		public void TestAttachButton_AttachingPackagesWithoutRCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageWithRCN = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var packageWithoutRCN = Helper.CreatePackageState(receiveTransportationUnit, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived);
			Factory.Save();

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			parent.JobNumber = "SHP001";

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;

				var parentJobNumberFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ActiveModuleFilters.Single(f => f.Description == "ParentJobNumber");
				parentJobNumberFilter.IsActive = false;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { packageWithRCN, packageWithoutRCN }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Packages aren't attached to a Receive Consignment so cannot be assigned to the shipment."));
			}
		}

		#endregion

		#region TestAttachButton_AttachingHandlingUnitWithInnerPackagesWithoutRCN

		public void TestAttachButton_AttachingHandlingUnitWithInnerPackagesWithoutRCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageWithRCN = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageWithoutRCN = Helper.CreatePackageState(rtu, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageWithRCN, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageWithoutRCN, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			parent.JobNumber = "SHP001";

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;

				var parentJobNumberFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ActiveModuleFilters.Single(f => f.Description == "ParentJobNumber");
				parentJobNumberFilter.IsActive = false;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { handlingUnitPackage }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Handling Unit's Child Packages aren't attached to a Receive Consignment so cannot be assigned to the shipment."));
			}
		}

		#endregion

		#region TestAttachButton_AttachingHandlingUnitInnerPackageWithSiblingWithoutRCN

		public void TestAttachButton_AttachingHandlingUnitInnerPackageWithSiblingWithoutRCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageWithRCN = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageWithoutRCN = Helper.CreatePackageState(rtu, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageWithRCN, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageWithoutRCN, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			parent.JobNumber = "SHP001";

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var outerPackagesFilter = (ModuleFlagsFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.OuterPackage];
				outerPackagesFilter.Property0 = false;
				outerPackagesFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;

				var parentJobNumberFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ActiveModuleFilters.Single(f => f.Description == "ParentJobNumber");
				parentJobNumberFilter.IsActive = false;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState>() { packageWithRCN, packageWithoutRCN, handlingUnitPackage }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectSingleElement(packageWithRCN);
				attachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Handling Unit's Child Packages aren't attached to a Receive Consignment so cannot be assigned to the shipment."));
			}
		}

		#endregion

		#region TestAttachButton_SingleHU_AttachInnerPackagesAccordingToUserSelection

		public void TestAttachButton_SingleHU_AttachInnerPackagesAccordingToUserSelection()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);
			var innerPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "Inner1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var innerPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "Inner2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var innerPackage3 = Helper.CreatePackageState(rcn, 1, "PKG", "Inner3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, innerPackage1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, innerPackage2, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, innerPackage3, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			parent.JobNumber = "SHP001";

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var outerPackagesFilter = (ModuleFlagsFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.OuterPackage];
				outerPackagesFilter.Property0 = false;
				outerPackagesFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;

				var parentJobNumberFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ActiveModuleFilters.Single(f => f.Description == "ParentJobNumber");
				parentJobNumberFilter.IsActive = false;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { handlingUnitPackage, innerPackage1, innerPackage2, innerPackage3 }, availablePackagesGrid.Grid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				availablePackagesGrid.Grid.SelectSingleElement(innerPackage1);
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { innerPackage1, innerPackage2, innerPackage3 }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectAllElements();
				filterControl.RemoveToolStripButton.PerformClick();
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { handlingUnitPackage, innerPackage1, innerPackage2, innerPackage3 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				availablePackagesGrid.Grid.SelectSingleElement(innerPackage1);
				attachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"Some of the selected packages are already packed onto HU HU1.
Do you want to attach all other packages on this HU too?"));

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { innerPackage3, innerPackage2 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { innerPackage1 }, attachedPackagesGrid.List);
			}
		}

		#endregion

		#region TestAttachButton_MultipleHUs_AttachInnerPackagesAccordingToUserSelection

		public void TestAttachButton_MultipleHUs_AttachInnerPackagesAccordingToUserSelection()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var handlingUnit1 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage1 = Helper.CreateHandlingUnitPackage("HU1", handlingUnit1, rtu);
			var innerPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "Inner1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var innerPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "Inner2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var innerPackage3 = Helper.CreatePackageState(rcn, 1, "PKG", "Inner3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var handlingUnit2 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage2 = Helper.CreateHandlingUnitPackage("HU2", handlingUnit2, rtu);
			var innerPackage4 = Helper.CreatePackageState(rcn, 1, "PKG", "Inner4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var innerPackage5 = Helper.CreatePackageState(rcn, 1, "PKG", "Inner5", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage1, innerPackage1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage1, innerPackage2, ZDateTimeOffset.Now, "ABC", handlingUnitPackage1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage1, innerPackage3, ZDateTimeOffset.Now, "ABC", handlingUnitPackage1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage2, innerPackage4, ZDateTimeOffset.Now, "ABC", handlingUnitPackage2);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage2, innerPackage5, ZDateTimeOffset.Now, "ABC", handlingUnitPackage2);

			Factory.Save();

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			parent.JobNumber = "SHP001";

			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();

				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var outerPackagesFilter = (ModuleFlagsFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.OuterPackage];
				outerPackagesFilter.Property0 = false;
				outerPackagesFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;

				var parentJobNumberFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ActiveModuleFilters.Single(f => f.Description == "ParentJobNumber");
				parentJobNumberFilter.IsActive = false;

				availablePackagesGrid.FirePerformSearch();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { handlingUnitPackage1, handlingUnitPackage2, innerPackage1, innerPackage2, innerPackage3, innerPackage4, innerPackage5 }, availablePackagesGrid.Grid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				availablePackagesGrid.Grid.SelectAllElements(x => x.PK == innerPackage1.PK || x.PK == innerPackage4.PK);
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { innerPackage1, innerPackage2, innerPackage3, innerPackage4, innerPackage5 }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectAllElements();
				filterControl.RemoveToolStripButton.PerformClick();
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { handlingUnitPackage1, handlingUnitPackage2, innerPackage1, innerPackage2, innerPackage3, innerPackage4, innerPackage5 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				availablePackagesGrid.Grid.SelectAllElements(x => x.PK == innerPackage1.PK || x.PK == innerPackage4.PK);
				attachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"Some of the selected packages are already packed onto the following HUs:
HU1
HU2
Do you want to attach all remaining packages from these HUs as well?"));

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { innerPackage2, innerPackage3, innerPackage5 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { innerPackage1, innerPackage4 }, attachedPackagesGrid.List);
			}
		}

		#endregion

		#region TestCreatePacklineButton_AttachingPartsOfPackageFromRCN

		public void TestCreatePacklineButton_AttachingPartsOfPackageFromRCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var standalonePackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKGS", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var outerPackagesFilter = (ModuleFlagsFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.OuterPackage];
				outerPackagesFilter.Property0 = false;
				outerPackagesFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var attachButton = filterControl.AssignToolStripButton;

				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				bool isAttachedPackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };

				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { standalonePackageState, handlingUnitPackage, childPackageState1, childPackageState2 }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectSingleElement(standalonePackageState);
				attachButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { handlingUnitPackage, childPackageState1, childPackageState2 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { standalonePackageState }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				createPackLinesButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"You did not add all packages from the following RCNs:
RC1 - RC1

Continue anyway?"));

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { handlingUnitPackage, childPackageState1, childPackageState2 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { standalonePackageState }, attachedPackagesGrid.List);
				Assert(!isAttachedPackagesCalled);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createPackLinesButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"You did not add all packages from the following RCNs:
RC1 - RC1

Continue anyway?"));
				Assert(isAttachedPackagesCalled);
			}
		}

		#endregion

		#region TestCreatePacklineButton_AttachingPartsOfPackageFromRCN_WhenSearchNothing

		public void TestCreatePacklineButton_AttachingPartsOfPackageFromRCN_WhenSearchNothing()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var rcn1 = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RC3", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageStateInRCN11 = Helper.CreatePackageState(rcn1, 1, "PKG", "P11", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN12 = Helper.CreatePackageState(rcn1, 1, "PKG", "P12", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN21 = Helper.CreatePackageState(rcn2, 1, "PKG", "P21", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN22 = Helper.CreatePackageState(rcn2, 1, "PKG", "P22", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageStateInRCN31 = Helper.CreatePackageState(rcn3, 1, "PKG", "P31", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN32 = Helper.CreatePackageState(rcn3, 1, "PKG", "P32", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Factory.Save();

			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var consignorFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.ConsignorCompanyName];
				consignorFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				consignorFilter.Property = "XXX";
				consignorFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				var attachButton = filterControl.AssignToolStripButton;
				bool isAttachedPackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };

				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11, packageStateInRCN12, packageStateInRCN21, packageStateInRCN31 }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				createPackLinesButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"You did not add all packages from the following RCNs:
RC3 - RC3
RC2 - RC2

Continue anyway?"));

				consignorFilter.Property = "";
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN22, packageStateInRCN32 }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11, packageStateInRCN12, packageStateInRCN21, packageStateInRCN31, packageStateInRCN22, packageStateInRCN32 }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				createPackLinesButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(isAttachedPackagesCalled);
			}
		}

		#endregion

		#region TestCreatePacklineButton_AttachingPartsOfPackageFromRCN_WhenSearchNothing

		public void TestCreatePacklineButton_AttachingPartsOfPackageFromRCN_RunPreAttachingValidation()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var rcn1 = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RC3", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageStateInRCN11 = Helper.CreatePackageState(rcn1, 1, "PKG", "P11", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN12 = Helper.CreatePackageState(rcn1, 1, "PKG", "P12", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN21 = Helper.CreatePackageState(rcn2, 1, "PKG", "P21", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN22 = Helper.CreatePackageState(rcn2, 1, "PKG", "P22", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageStateInRCN31 = Helper.CreatePackageState(rcn3, 1, "PKG", "P31", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN32 = Helper.CreatePackageState(rcn3, 1, "PKG", "P32", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Factory.Save();

			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var consignorFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.ConsignorCompanyName];
				consignorFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				consignorFilter.Property = "XXX";
				consignorFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				var attachButton = filterControl.AssignToolStripButton;
				bool isAttachedPackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };

				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11, packageStateInRCN12, packageStateInRCN21, packageStateInRCN31 }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				createPackLinesButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"You did not add all packages from the following RCNs:
RC3 - RC3
RC2 - RC2

Continue anyway?"));

				consignorFilter.Property = string.Empty;
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN22, packageStateInRCN32 }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11, packageStateInRCN12, packageStateInRCN21, packageStateInRCN31, packageStateInRCN22, packageStateInRCN32 }, attachedPackagesGrid.List);

				parent.PreAttachingValidationMessage = "This is a dummy pre-attaching validation message.";

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				createPackLinesButton.PerformClick();
				AssertEquals("This is a dummy pre-attaching validation message.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Show Pre-attaching message and click 'No' to stop attaching process", !isAttachedPackagesCalled);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createPackLinesButton.PerformClick();
				AssertEquals("This is a dummy pre-attaching validation message.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Show Pre-attaching message and click 'Yes' to attach packages", isAttachedPackagesCalled);
			}
		}

		#endregion

		#region TestCreatePacklineButton_AttachingFromMoreThanFiveParents

		public void TestCreatePacklineButton_AttachingFromMoreThanFiveParents()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var rcn1 = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RC3", warehouse.PK);
			var rcn4 = Helper.CreateReceiveConsignment("RC4", warehouse.PK);
			var rcn5 = Helper.CreateReceiveConsignment("RC5", warehouse.PK);
			var rcn6 = Helper.CreateReceiveConsignment("RC6", warehouse.PK);
			Helper.CreateAdditionalReference(rcn1, "Test001", "FSH");
			Helper.CreateAdditionalReference(rcn2, "Test002", "FSH");
			Helper.CreateAdditionalReference(rcn3, "Test003", "FSH");
			Helper.CreateAdditionalReference(rcn4, "Test004", "FSH");
			Helper.CreateAdditionalReference(rcn5, "Test005", "FSH");
			Helper.CreateAdditionalReference(rcn6, "Test006", "FSH");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageStateInRCN11 = Helper.CreatePackageState(rcn1, 1, "PKG", "P11", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageStateInRCN21 = Helper.CreatePackageState(rcn2, 1, "PKG", "P21", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageStateInRCN31 = Helper.CreatePackageState(rcn3, 1, "PKG", "P31", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageStateInRCN41 = Helper.CreatePackageState(rcn4, 1, "PKG", "P41", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageStateInRCN51 = Helper.CreatePackageState(rcn5, 1, "PKG", "P51", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageStateInRCN61 = Helper.CreatePackageState(rcn6, 1, "PKG", "P61", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Factory.Save();
			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var consignorFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.ConsignorCompanyName];
				consignorFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				consignorFilter.Property = "XXX";
				consignorFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				var attachButton = filterControl.AssignToolStripButton;

				consignorFilter.Property = string.Empty;
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11, packageStateInRCN21, packageStateInRCN31, packageStateInRCN41, packageStateInRCN51, packageStateInRCN61 }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11, packageStateInRCN21, packageStateInRCN31, packageStateInRCN41, packageStateInRCN51, packageStateInRCN61 }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				createPackLinesButton.PerformClick();
				AssertEquals("Attaching packages from more than five (5) Receive Consignments (RCNs) is not supported. Please adjust the package selection to reduce the number of RCNs and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestCreatePacklineButton_RemoveFromPreviousParents

		public void TestCreatePacklineButton_RemoveFromPreviousParents()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var rcn1 = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			Helper.CreateAdditionalReference(rcn1, "Test001", "FSH");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageStateInRCN11 = Helper.CreatePackageState(rcn1, 1, "PKG", "P11", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var shipment =  Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "Test001";
			var outerLine = shipment.OuterPackLines.AddNew();
			var job = Factory.New<ForwardingPackageJob>();
			job.KJ_JobID = "Test001";
			job.KJ_ParentID = outerLine.JL_JS;
			job.KJ_ParentTableCode = "JS";
			var package1 = outerLine.PkgPackageCollection.AddNew();
			package1.KP_F3_NKPackType = "BOX";
			package1.KP_PackageID = "P11";
			package1.KP_KJ_ParentPackageJob = job.PK;
			var package2 = outerLine.PkgPackageCollection.AddNew();
			package2.KP_F3_NKPackType = "BOX";
			package2.KP_PackageID = "P12";
			package2.KP_KJ_ParentPackageJob = job.PK;

			Factory.Save();
			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var consignorFilter = (ModuleTextFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.ConsignorCompanyName];
				consignorFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				consignorFilter.Property = "XXX";
				consignorFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				var attachButton = filterControl.AssignToolStripButton;
				bool isAttachedPackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };

				consignorFilter.Property = string.Empty;
				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11 }, availablePackagesGrid.Grid.List);

				availablePackagesGrid.Grid.SelectAllElements();
				attachButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11 }, attachedPackagesGrid.List);

				parent.PreAttachingValidationMessage = "This is a dummy pre-attaching validation message.";

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createPackLinesButton.PerformClick();
				AssertEquals("This is a dummy pre-attaching validation message.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Show Pre-attaching message and click 'Yes' to attach packages", isAttachedPackagesCalled);

				Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var packages = Factory.Load<PkgPackage>(new ZQuery());
				var package1InNewFactory = packages.FirstOrDefault(p => p.PK == package1.PK);
				AssertNull("Package 1 shall be deleted", package1InNewFactory);
				var package2InNewFactory = packages.FirstOrDefault(p => p.PK == package2.PK);
				AssertNotNull("Package 2 shall be in database", package2InNewFactory);
			}
		}

		#endregion

		#region TestCreatePacklineButton_AttachingPartsOfPackageFromRCN_WhenRemoving

		public void TestCreatePacklineButton_AttachingPartsOfPackageFromRCN_WhenRemoving()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var rcn1 = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageStateInRCN11 = Helper.CreatePackageState(rcn1, 1, "PKG", "P11", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN12 = Helper.CreatePackageState(rcn1, 1, "PKG", "P12", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN21 = Helper.CreatePackageState(rcn2, 1, "PKG", "P21", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var packageStateInRCN22 = Helper.CreatePackageState(rcn2, 1, "PKG", "P22", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);

			Factory.Save();

			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageAttachForm(parent))
			{
				form.Show();
				var availablePackagesGrid = (ZFilterStripCommonControl)form.Controls.Find("LinesGrid", true).Single();
				var outerPackagesFilter = (ModuleFlagsFilter)availablePackagesGrid.FilterBusinessObject.ModuleFilters[TransitWarehousePackageAttachFilterBusinessObject.Schema.OuterPackage];
				outerPackagesFilter.Property0 = false;
				outerPackagesFilter.IsActive = true;

				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var filterControl = ((ITransitPackagePlannerFilterControl)availablePackagesGrid);
				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				var removeButton = filterControl.RemoveToolStripButton;
				bool isRemovePackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { };
				parent.RemovePackagesCalled += (p) => { isRemovePackagesCalled = true; };

				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11, packageStateInRCN12, packageStateInRCN21, packageStateInRCN22 }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(packageStateInRCN11);
				removeButton.PerformClick();
				attachedPackagesGrid.SelectSingleElement(packageStateInRCN21);
				removeButton.PerformClick();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				createPackLinesButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"You did not add all packages from the following RCNs:
RC2 - RC2
RC1 - RC1

Continue anyway?"));
				Assert(!isRemovePackagesCalled);

				availablePackagesGrid.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN11, packageStateInRCN21 }, availablePackagesGrid.Grid.List);
				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { packageStateInRCN22, packageStateInRCN12 }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(packageStateInRCN12);
				removeButton.PerformClick();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createPackLinesButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"You did not add all packages from the following RCNs:
RC2 - RC2

Continue anyway?"));
				Assert(isRemovePackagesCalled);
			}
		}

		#endregion

		#region TestRelatedItems_DoubleClickNotSupported

		public void TestRelatedItems_DoubleClickNotSupported()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Factory.Save();

			using (var form = new ShowEDocsForm(packageState))
			{
				form.Show();

				var userControl = form.Controls.Cast<Control>().OfType<ZTabControl>().Single().Controls.Cast<ZAutoSizedTabPagePlugIn>().Single().Controls.Cast<eDocsUserControl>().Single();
				var relatedParentsGrid = (ZGrid)userControl.Controls.Find("RelatedParentsGrid", true).Single();
				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, relatedParentsGrid.GetRowNotificationRectangle(0).X, relatedParentsGrid.GetRowNotificationRectangle(0).Y, 0);
				userControl.DoubleClickOn_RelatedParents(relatedParentsGrid, mouseEvent);

				AssertEquals($"Double click on Related eDocs is not supported in the Transit Packages module. Please contact {Core.Constants.ProductName} support for more information.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public override void TestBashingForm()
		{
			Assert(true);
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = Helper.CreateTRWWarehouse().WarehouseAddress.PK;
			return new TransitWarehousePackageAttachForm(parent);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
