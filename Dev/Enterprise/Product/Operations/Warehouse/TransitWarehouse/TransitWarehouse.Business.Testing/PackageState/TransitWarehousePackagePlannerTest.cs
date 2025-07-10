using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(TransitWarehousePackagePlanner))]
	class TransitWarehousePackagePlannerTest : NonPersistentBusinessObjectTestCase
	{
		#region TestParent

		public void TestParent()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals(parent, planner.Parent);
		}

		#endregion

		#region TestAttachedPackages

		public void TestAttachedPackages()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var arrived = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, receiveTransportationUnit, Helper.CreateDispatchConsignment("DSPC1", warehouse.PK));
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
			Factory.Save();

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals(warehouse, planner.Warehouse);
			AssertEquals("No packages are attached.", 0, new TransitWarehousePackagePlanner(Factory, parent).AttachedPackages.Count);

			Helper.CreateAdditionalReference(arrived, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { arrived }, new TransitWarehousePackagePlanner(Factory, parent).AttachedPackages);

			Helper.CreateAdditionalReference(putaway, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { putaway, arrived }, new TransitWarehousePackagePlanner(Factory, parent).AttachedPackages);

			Helper.CreateAdditionalReference(committed, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { putaway, arrived, committed }, new TransitWarehousePackagePlanner(Factory, parent).AttachedPackages);

			Helper.CreateAdditionalReference(picked, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { putaway, arrived, committed, picked }, new TransitWarehousePackagePlanner(Factory, parent).AttachedPackages);

			Helper.CreateAdditionalReference(staged, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { staged, putaway, arrived, committed, picked }, new TransitWarehousePackagePlanner(Factory, parent).AttachedPackages);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse_BranchHasBothTransitAndProductWarehouse()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TRW");
			var productWarehouseForSameAddressAndBranch = Helper.CreateWarehouse("WHS", transitWarehouse.WarehouseAddress, transitWarehouse.RelatedCompanyBranch);
			AssertEquals("Precondition", WarehouseTypes.Codes.Transit, transitWarehouse.WW_WarehouseType);
			AssertEquals("Precondition", WarehouseTypes.Codes.Product, productWarehouseForSameAddressAndBranch.WW_WarehouseType);
			parent.TransitWarehouseAddressPK = transitWarehouse.WW_OA_WarehouseAddress;

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals(transitWarehouse, planner.Warehouse);
			Assert(!planner.HasMultipleTransitWarehousesForAddress);
		}

		public void TestWarehouse_BranchHasOnlyProductWarehouse()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var productWarehouse = Helper.CreateWarehouse("WHS");
			productWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			parent.TransitWarehouseAddressPK = productWarehouse.WW_OA_WarehouseAddress;

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertNull(planner.Warehouse);
			Assert(!planner.HasMultipleTransitWarehousesForAddress);
		}

		public void TestWarehouse_NoWarehouses()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertNull(planner.Warehouse);
			Assert(!planner.HasMultipleTransitWarehousesForAddress);
		}

		public void TestWarehouse_OnlyTransitWarehouse()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TRW");
			parent.TransitWarehouseAddressPK = transitWarehouse.WW_OA_WarehouseAddress;
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals(transitWarehouse, planner.Warehouse);
			Assert(!planner.HasMultipleTransitWarehousesForAddress);
		}

		public void TestWarehouse_SameAddressInDifferentBranchesHaveTransitWarehouses()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouseForCurrentBranch = Helper.CreateTRWWarehouse("TW2");
			var transitWarehouseForDifferentBranch = Helper.CreateTRWWarehouse("TW1");
			transitWarehouseForDifferentBranch.WW_GB_RelatedCompanyBranch = Helper.CreateGlbBranch("TES").PK;
			transitWarehouseForDifferentBranch.WW_OA_WarehouseAddress = transitWarehouseForCurrentBranch.WW_OA_WarehouseAddress;
			AssertEquals("Precondition", WarehouseTypes.Codes.Transit, transitWarehouseForCurrentBranch.WW_WarehouseType);
			AssertEquals("Precondition", WarehouseTypes.Codes.Transit, transitWarehouseForDifferentBranch.WW_WarehouseType);

			parent.TransitWarehouseAddressPK = transitWarehouseForCurrentBranch.WW_OA_WarehouseAddress;
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals(transitWarehouseForDifferentBranch, planner.Warehouse);
			Assert(planner.HasMultipleTransitWarehousesForAddress);
		}

		#endregion

		#region TestPackages

		public void TestPackages()
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
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, receiveTransportationUnit, Helper.CreateDispatchConsignment("DSPC1", warehouse.PK));
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
			Factory.Save();

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertContainsExactElementsInAnyOrder("All non departed and FLO packages must be returned.", new[] { arrived, putaway, committed, picked, staged }, planner.ModuleFilterPackageCollection);

			Helper.CreateAdditionalReference(arrived, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { putaway, committed, picked, staged }, new TransitWarehousePackagePlanner(Factory, parent).ModuleFilterPackageCollection);

			Helper.CreateAdditionalReference(putaway, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { committed, picked, staged }, new TransitWarehousePackagePlanner(Factory, parent).ModuleFilterPackageCollection);

			Helper.CreateAdditionalReference(committed, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { picked, staged }, new TransitWarehousePackagePlanner(Factory, parent).ModuleFilterPackageCollection);

			Helper.CreateAdditionalReference(picked, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { staged }, new TransitWarehousePackagePlanner(Factory, parent).ModuleFilterPackageCollection);

			Helper.CreateAdditionalReference(staged, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertEquals(0, new TransitWarehousePackagePlanner(Factory, parent).ModuleFilterPackageCollection.Count);
		}

		#endregion

		#region TestAssignPackageStates

		public void TestAssignPackageStates()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var ps1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var ps2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			ps2.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;
			var ps3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			ps3.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals("Precondition", 0, planner.AttachedPackages.Count);
			AssertEquals("Precondition", false, ps1.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, ps1.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, ps1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, ps2.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", true, ps2.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, ps2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, ps3.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", true, ps3.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", true, ps3.IsRemovedFromAttachedPacklinesAndHasJobNumber);

			var isOnPackageStateCollectionChangedCalled = false;
			planner.OnPackageStateCollectionChanged += () => isOnPackageStateCollectionChangedCalled = true;
			planner.AssignPackageState(new[] { ps1, ps2, ps3 });
			AssertEquals(true, isOnPackageStateCollectionChangedCalled);
			AssertEquals(3, planner.AttachedPackages.Count);
			AssertEquals(true, ps1.IsAddedToAttachedPackageCollection);
			AssertEquals(false, ps1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, ps2.IsAddedToAttachedPackageCollection);
			AssertEquals(false, ps2.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, ps3.IsAddedToAttachedPackageCollection);
			AssertEquals(false, ps3.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps3.IsRemovedFromAttachedPacklinesAndHasJobNumber);
		}

		public void TestAssignPackageStates_HandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var childPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var handlingUnit1 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage1 = Helper.CreateHandlingUnitPackage("HU1", handlingUnit1, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage1, childPackageState1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage1, childPackageState2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage1);

			var childPackageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			childPackageState3.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;
			var handlingUnit2 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage2 = Helper.CreateHandlingUnitPackage("HU2", handlingUnit2, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage2, childPackageState3, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage2);

			var childPackageState4 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);
			var handlingUnit3 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage3 = Helper.CreateHandlingUnitPackage("HU3", handlingUnit3, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage3, childPackageState4, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage3);
			Factory.Save();

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			planner.RemovePackageStates(new[] { childPackageState4 });

			AssertEquals("Precondition", 0, planner.AttachedPackages.Count);
			AssertEquals("Precondition", false, handlingUnitPackage1.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, handlingUnitPackage1.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, handlingUnitPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, handlingUnitPackage2.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", true, handlingUnitPackage2.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, handlingUnitPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, handlingUnitPackage3.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", true, handlingUnitPackage3.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", true, handlingUnitPackage3.IsRemovedFromAttachedPacklinesAndHasJobNumber);

			planner.AssignPackageState(new[] { handlingUnitPackage1, handlingUnitPackage2, handlingUnitPackage3 });
			AssertContainsExactElementsInAnyOrder(new[] { childPackageState1, childPackageState2, childPackageState3, childPackageState4 }, planner.AttachedPackages);
			AssertEquals(true, handlingUnitPackage1.IsAddedToAttachedPackageCollection);
			AssertEquals(false, handlingUnitPackage1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, handlingUnitPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, handlingUnitPackage2.IsAddedToAttachedPackageCollection);
			AssertEquals(false, handlingUnitPackage2.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, handlingUnitPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, handlingUnitPackage3.IsAddedToAttachedPackageCollection);
			AssertEquals(false, handlingUnitPackage3.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, handlingUnitPackage3.IsRemovedFromAttachedPacklinesAndHasJobNumber);
		}

		public void TestAssignPackageStates_ChildPackage()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);
			Factory.Save();

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			planner.AssignPackageState(new[] { childPackageState1 }, true);
			AssertContainsExactElementsInAnyOrder(new[] { childPackageState1, childPackageState2 }, planner.AttachedPackages);
		}

		public void TestAssignPackageStates_Overpack()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var childPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var overpack1 = Helper.CreatePackageHandlingUnit();
			var overpackPackage1 = Helper.CreateHandlingUnitPackage("OVP1", overpack1, null, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn);
			Helper.PackPackageIntoHandlingUnit(overpackPackage1, childPackageState1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: overpackPackage1);
			Helper.PackPackageIntoHandlingUnit(overpackPackage1, childPackageState2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: overpackPackage1);

			var childPackageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var overpack2 = Helper.CreatePackageHandlingUnit();
			var overpackPackage2 = Helper.CreateHandlingUnitPackage("OVP2", overpack2, null, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn);
			overpackPackage2.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;
			Helper.PackPackageIntoHandlingUnit(overpackPackage2, childPackageState3, ZDateTimeOffset.Now, "ABC", topHandlingUnit: overpackPackage2);

			var childPackageState4 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var overpack3 = Helper.CreatePackageHandlingUnit();
			var overpackPackage3 = Helper.CreateHandlingUnitPackage("OVP3", overpack3, null, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn, entryNum: parent.JobNumber);
			Helper.PackPackageIntoHandlingUnit(overpackPackage3, childPackageState4, ZDateTimeOffset.Now, "ABC", topHandlingUnit: overpackPackage3);
			Factory.Save();

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			planner.RemovePackageStates(new[] { overpackPackage3 });

			AssertEquals("Precondition", 0, planner.AttachedPackages.Count);
			AssertEquals("Precondition", false, overpackPackage1.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage1.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, overpackPackage2.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", true, overpackPackage2.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, overpackPackage3.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", true, overpackPackage3.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", true, overpackPackage3.IsRemovedFromAttachedPacklinesAndHasJobNumber);

			planner.AssignPackageState(new[] { overpackPackage1, overpackPackage2, overpackPackage3 });
			AssertContainsExactElementsInAnyOrder(new[] { overpackPackage1, overpackPackage2, overpackPackage3 }, planner.AttachedPackages);
			AssertEquals(true, overpackPackage1.IsAddedToAttachedPackageCollection);
			AssertEquals(false, overpackPackage1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, overpackPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, overpackPackage2.IsAddedToAttachedPackageCollection);
			AssertEquals(false, overpackPackage2.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, overpackPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, overpackPackage3.IsAddedToAttachedPackageCollection);
			AssertEquals(false, overpackPackage3.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, overpackPackage3.IsRemovedFromAttachedPacklinesAndHasJobNumber);
		}

		public void TestAssignPackageStates_WithAssignAllPackageAsFalse()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveUnit: rtu);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "Child1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "Child2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage3 = Helper.CreatePackageState(rcn, 1, "PKG", "Child3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage3, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);

			var standalonePackage = Helper.CreatePackageState(rcn, 1, "PKG", "Standalone", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Factory.Save();

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals("Precondition", 0, planner.AttachedPackages.Count);

			bool collectionChangedCalled = false;
			planner.OnPackageStateCollectionChanged += () => collectionChangedCalled = true;
			planner.AssignPackageState(new[] { childPackage1, standalonePackage });
			AssertEquals(true, collectionChangedCalled);
			AssertContainsExactElementsInAnyOrder(new[] { childPackage1, standalonePackage }, planner.AttachedPackages);
			AssertEquals(false, planner.AttachedPackages.Contains(childPackage2));
			AssertEquals(false, planner.AttachedPackages.Contains(childPackage3));
			AssertEquals(true, childPackage1.IsAddedToAttachedPackageCollection);
			AssertEquals(false, childPackage1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(true, standalonePackage.IsAddedToAttachedPackageCollection);
			AssertEquals(false, standalonePackage.IsRemovedFromAttachedPackageCollection);
		}

		public void TestAssignPackageStates_WithAssignAllPackageAsTrue()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveUnit: rtu);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "Child1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "Child2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage3 = Helper.CreatePackageState(rcn, 1, "PKG", "Child3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage3, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);

			var standalonePackage = Helper.CreatePackageState(rcn, 1, "PKG", "Standalone", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Factory.Save();

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals("Precondition", 0, planner.AttachedPackages.Count);

			bool collectionChangedCalled = false;
			planner.OnPackageStateCollectionChanged += () => collectionChangedCalled = true;
			planner.AssignPackageState(new[] { childPackage1, standalonePackage }, true);
			AssertEquals(true, collectionChangedCalled);
			AssertContainsExactElementsInAnyOrder(new[] { childPackage1, standalonePackage, childPackage2, childPackage3 }, planner.AttachedPackages);
			AssertEquals(true, planner.AttachedPackages.Contains(childPackage2));
			AssertEquals(true, planner.AttachedPackages.Contains(childPackage3));
			AssertEquals(true, childPackage1.IsAddedToAttachedPackageCollection);
			AssertEquals(false, childPackage1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(true, standalonePackage.IsAddedToAttachedPackageCollection);
			AssertEquals(false, standalonePackage.IsRemovedFromAttachedPackageCollection);
		}

		public void TestAssignPackageStates_WithAssignAllPackageAsTrue_ShouldNotAssignPackagesThatAlreadyHasDCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveUnit: rtu);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "Child1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "Child2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage3 = Helper.CreatePackageState(rcn, 1, "PKG", "Child3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage3, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage);

			var standalonePackage = Helper.CreatePackageState(rcn, 1, "PKG", "Standalone", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			childPackage2.WPS_WDC_TransitDispatchConsignment = dcn.PK;

			Factory.Save();

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals("Precondition", 0, planner.AttachedPackages.Count);

			bool collectionChangedCalled = false;
			planner.OnPackageStateCollectionChanged += () => collectionChangedCalled = true;
			planner.AssignPackageState(new[] { childPackage1, standalonePackage }, true);
			AssertEquals(true, collectionChangedCalled);
			AssertContainsExactElementsInAnyOrder(new[] { childPackage1, standalonePackage, childPackage3 }, planner.AttachedPackages);
			AssertEquals(false, planner.AttachedPackages.Contains(childPackage2));
			AssertEquals(true, planner.AttachedPackages.Contains(childPackage3));
			AssertEquals(true, childPackage1.IsAddedToAttachedPackageCollection);
			AssertEquals(false, childPackage1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(true, standalonePackage.IsAddedToAttachedPackageCollection);
			AssertEquals(false, standalonePackage.IsRemovedFromAttachedPackageCollection);
		}

		#endregion

		#region TestRemovePackageStates

		public void TestRemovePackageStates()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var ps1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var ps2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			ps2.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;
			var ps3NotInRelationship = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var ps4NotInRelationshipAndHasJobNumber = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: parent.JobNumber);

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			planner.AssignPackageState(new[] { ps1, ps2 });

			AssertContainsExactElementsInAnyOrder(new[] { ps1, ps2 }, planner.AttachedPackages);
			AssertEquals("Precondition", true, ps1.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, ps1.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, ps1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", true, ps2.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, ps2.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, ps2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, ps3NotInRelationship.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, ps3NotInRelationship.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, ps3NotInRelationship.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, ps4NotInRelationshipAndHasJobNumber.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, ps4NotInRelationshipAndHasJobNumber.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, ps4NotInRelationshipAndHasJobNumber.IsRemovedFromAttachedPacklinesAndHasJobNumber);

			var isOnPackageStateCollectionChangedCalled = 0;
			planner.OnPackageStateCollectionChanged += () => isOnPackageStateCollectionChangedCalled++;
			planner.RemovePackageStates(new[] { ps1, ps3NotInRelationship, ps4NotInRelationshipAndHasJobNumber });
			AssertEquals(1, isOnPackageStateCollectionChangedCalled);
			AssertEquals(1, planner.AttachedPackages.Count);
			AssertEquals(1, planner.RemovedPackages.Count);
			AssertContainsExactElementsInAnyOrder(new[] { ps1 }, planner.RemovedPackages);
			AssertEquals(false, ps1.IsAddedToAttachedPackageCollection);
			AssertEquals(true, ps1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, ps2.IsAddedToAttachedPackageCollection);
			AssertEquals(false, ps2.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(false, ps3NotInRelationship.IsAddedToAttachedPackageCollection);
			AssertEquals(true, ps3NotInRelationship.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps3NotInRelationship.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(false, ps4NotInRelationshipAndHasJobNumber.IsAddedToAttachedPackageCollection);
			AssertEquals(true, ps4NotInRelationshipAndHasJobNumber.IsRemovedFromAttachedPackageCollection);
			AssertEquals(true, ps4NotInRelationshipAndHasJobNumber.IsRemovedFromAttachedPacklinesAndHasJobNumber);
		}

		public void TestRemovePackageStates_SelectingChildPackageDoesNotRemovesOtherInners()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var standalongPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKGA", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var handlingUnit1 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage1 = Helper.CreateHandlingUnitPackage("HU1", handlingUnit1, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage1, childPackageState1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage1, childPackageState2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackage1);
			Factory.Save();

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			planner.AssignPackageState(new[] { standalongPackageState, childPackageState1, childPackageState2 });
			AssertContainsExactElementsInAnyOrder(new[] { standalongPackageState, childPackageState1, childPackageState2 }, planner.AttachedPackages);

			var isOnPackageStateCollectionChangedCalled = 0;
			planner.OnPackageStateCollectionChanged += () => isOnPackageStateCollectionChangedCalled++;
			planner.RemovePackageStates(new[] { childPackageState1 });
			AssertEquals(1, isOnPackageStateCollectionChangedCalled);
			AssertContainsExactElementsInAnyOrder("The sibling of child packages should not be removed together", new[] { standalongPackageState, childPackageState2 }, planner.AttachedPackages);
		}

		public void TestRemovePackageStates_Overpack()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var overpack1 = Helper.CreatePackageHandlingUnit();
			var overpackPackage1 = Helper.CreateHandlingUnitPackage("OVP1", overpack1, null, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn);
			Helper.PackPackageIntoHandlingUnit(overpackPackage1, childPackageState1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: overpackPackage1);

			var childPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var overpack2 = Helper.CreatePackageHandlingUnit();
			var overpackPackage2 = Helper.CreateHandlingUnitPackage("OVP2", overpack2, null, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn);
			overpackPackage2.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;
			Helper.PackPackageIntoHandlingUnit(overpackPackage2, childPackageState2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: overpackPackage2);

			var childPackageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var overpack3 = Helper.CreatePackageHandlingUnit();
			var overpackPackage3 = Helper.CreateHandlingUnitPackage("OVP3", overpack3, null, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn);
			Helper.PackPackageIntoHandlingUnit(overpackPackage3, childPackageState3, ZDateTimeOffset.Now, "ABC", topHandlingUnit: overpackPackage3);

			var childPackageState4 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var overpack4 = Helper.CreatePackageHandlingUnit();
			var overpackPackage4 = Helper.CreateHandlingUnitPackage("OVP4", overpack4, null, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn, entryNum: parent.JobNumber);
			Helper.PackPackageIntoHandlingUnit(overpackPackage4, childPackageState4, ZDateTimeOffset.Now, "ABC", topHandlingUnit: overpackPackage4);

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			planner.AssignPackageState(new[] { overpackPackage1, overpackPackage2 });

			AssertContainsExactElementsInAnyOrder(new[] { overpackPackage1, overpackPackage2 }, planner.AttachedPackages);
			AssertEquals("Precondition", true, overpackPackage1.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage1.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", true, overpackPackage2.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage2.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, overpackPackage3.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage3.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage3.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", false, overpackPackage4.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage4.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, overpackPackage4.IsRemovedFromAttachedPacklinesAndHasJobNumber);

			var isOnPackageStateCollectionChangedCalled = 0;
			planner.OnPackageStateCollectionChanged += () => isOnPackageStateCollectionChangedCalled++;
			planner.RemovePackageStates(new[] { overpackPackage1, overpackPackage3, overpackPackage4 });
			AssertEquals(1, isOnPackageStateCollectionChangedCalled);
			AssertEquals(1, planner.AttachedPackages.Count);
			AssertEquals(1, planner.RemovedPackages.Count);
			AssertContainsExactElementsInAnyOrder(new[] { overpackPackage1 }, planner.RemovedPackages);
			AssertEquals(false, overpackPackage1.IsAddedToAttachedPackageCollection);
			AssertEquals(true, overpackPackage1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, overpackPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, overpackPackage2.IsAddedToAttachedPackageCollection);
			AssertEquals(false, overpackPackage2.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, overpackPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(false, overpackPackage3.IsAddedToAttachedPackageCollection);
			AssertEquals(true, overpackPackage3.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, overpackPackage3.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(false, overpackPackage4.IsAddedToAttachedPackageCollection);
			AssertEquals(true, overpackPackage4.IsRemovedFromAttachedPackageCollection);
			AssertEquals(true, overpackPackage4.IsRemovedFromAttachedPacklinesAndHasJobNumber);
		}

		#endregion

		#region TestRemovePacklineFromPreviousParent

		public void TestRemovePacklineFromPreviousParent()
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

			var shipment = Factory.New<ForwardingShipment>();
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

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			planner.AssignPackageState(new[] { packageStateInRCN11 });
			AssertContainsExactElementsInAnyOrder(new[] { packageStateInRCN11 }, planner.AttachedPackages);
			planner.RemovePacklineFromPreviousParent();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var packages = Factory.Load<PkgPackage>(new ZQuery());
			var package1InNewFactory = packages.FirstOrDefault(p => p.PK == package1.PK);
			AssertNull("Package 1 shall be deleted", package1InNewFactory);
			var package2InNewFactory = packages.FirstOrDefault(p => p.PK == package2.PK);
			AssertNotNull("Package 2 shall be in database", package2InNewFactory);
		}

		#endregion

		#region TestCreatePacklines

		public void TestCreatePacklines_AttachAvailablePackages()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var arrivedUnAssignedPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var arrivedUnAssignedPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { arrivedUnAssignedPackage1, arrivedUnAssignedPackage2 }, planner.ModuleFilterPackageCollection);
			AssertEquals(0, planner.AttachedPackages.Count);

			arrivedUnAssignedPackage1.AttachedPackageStateStatus = AttachedPackageStateStatus.Added;
			planner.ModuleFilterPackageCollection.RemoveFromRelationship(arrivedUnAssignedPackage1);
			planner.AttachedPackages.Add(arrivedUnAssignedPackage1);
			AssertContainsExactElementsInAnyOrder(new[] { arrivedUnAssignedPackage2 }, planner.ModuleFilterPackageCollection);
			AssertContainsExactElementsInAnyOrder(new[] { arrivedUnAssignedPackage1 }, planner.AttachedPackages);

			var expectedAttachPackagesPassedToParent = new List<ITransitPackage>();
			var expectedRemovePackagesPassedToParent = new List<ITransitPackage>();
			parent.AttachPackagesCalled += (ps) => expectedAttachPackagesPassedToParent.AddRange(ps);
			parent.RemovePackagesCalled += (ps) => expectedRemovePackagesPassedToParent.AddRange(ps);

			planner.CreatePackLines();
			AssertContainsExactElementsInAnyOrder(new[] { arrivedUnAssignedPackage1 }, expectedAttachPackagesPassedToParent);
			AssertEquals(0, expectedRemovePackagesPassedToParent.Count);

			var additionalReferenceForAttachedPackage = arrivedUnAssignedPackage1.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Single();
			AssertEquals(WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, additionalReferenceForAttachedPackage.CE_EntryType);
			AssertEquals(planner.Parent.JobNumber, additionalReferenceForAttachedPackage.CE_EntryNum);
		}

		public void TestCreatePacklines_AttachAlreadyAttachedPackage()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var alreadyAttachedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Helper.CreateAdditionalReference(alreadyAttachedPackage, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();

			AssertEquals(0, planner.ModuleFilterPackageCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { alreadyAttachedPackage }, planner.AttachedPackages);

			alreadyAttachedPackage.AttachedPackageStateStatus = AttachedPackageStateStatus.Added; // can happen if package is removed and attached again via the form
			AssertEquals(0, planner.ModuleFilterPackageCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { alreadyAttachedPackage }, planner.AttachedPackages);

			var expectedAttachPackagesPassedToParent = new List<ITransitPackage>();
			var expectedRemovePackagesPassedToParent = new List<ITransitPackage>();
			parent.AttachPackagesCalled += (ps) => expectedAttachPackagesPassedToParent.AddRange(ps);
			parent.RemovePackagesCalled += (ps) => expectedRemovePackagesPassedToParent.AddRange(ps);

			planner.CreatePackLines();
			AssertEquals(1, expectedAttachPackagesPassedToParent.Count);
			AssertEquals(0, expectedRemovePackagesPassedToParent.Count);

			var additionalReferenceForAttachedPackage = alreadyAttachedPackage.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Single();
			AssertEquals(WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, additionalReferenceForAttachedPackage.CE_EntryType);
			AssertEquals(planner.Parent.JobNumber, additionalReferenceForAttachedPackage.CE_EntryNum);
		}

		public void TestCreatePacklines_RemoveAttachedPackages()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var arrivedAssignedPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var arrivedAssignedPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Helper.CreateAdditionalReference(arrivedAssignedPackage1, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(arrivedAssignedPackage2, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();

			AssertEquals(0, planner.ModuleFilterPackageCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { arrivedAssignedPackage1, arrivedAssignedPackage2 }, planner.AttachedPackages);

			arrivedAssignedPackage1.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;
			planner.AttachedPackages.RemoveFromRelationship(arrivedAssignedPackage1);
			planner.ModuleFilterPackageCollection.Add(arrivedAssignedPackage1);

			AssertEquals(arrivedAssignedPackage1, planner.ModuleFilterPackageCollection.Single());
			AssertEquals(arrivedAssignedPackage2, planner.AttachedPackages.Single());
			AssertEquals(planner.Parent.JobNumber, arrivedAssignedPackage1.ParentJobNumber);

			var expectedAttachPackagesPassedToParent = new List<ITransitPackage>();
			var expectedRemovePackagesPassedToParent = new List<ITransitPackage>();
			parent.AttachPackagesCalled += (ps) => expectedAttachPackagesPassedToParent.AddRange(ps);
			parent.RemovePackagesCalled += (ps) => expectedRemovePackagesPassedToParent.AddRange(ps);

			planner.CreatePackLines();
			AssertEquals(1, expectedAttachPackagesPassedToParent.Count);
			AssertContainsExactElementsInAnyOrder(new[] { arrivedAssignedPackage2 }, expectedAttachPackagesPassedToParent);
			AssertContainsExactElementsInAnyOrder(new[] { arrivedAssignedPackage1 }, expectedRemovePackagesPassedToParent);
			AssertEquals(0, arrivedAssignedPackage1.AdditionalReferenceNumbers.Count);
			AssertEquals(planner.Parent.JobNumber, arrivedAssignedPackage2.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Single(a => a.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference).CE_EntryNum);
			AssertEquals(ZString.Empty, arrivedAssignedPackage1.ParentJobNumber);
		}

		public void TestAttachingPackagesWillAttachAllPackages()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Helper.CreateAdditionalReference(packageState1, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(packageState2, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();

			AssertEquals("Precondition: ", 2, planner.AttachedPackages.Count);

			packageState1.ResetPackageStateStatus();
			var expectedAttachPackagesPassedToParent = new List<ITransitPackage>();
			var expectedRemovePackagesPassedToParent = new List<ITransitPackage>();
			parent.AttachPackagesCalled += (ps) => expectedAttachPackagesPassedToParent.AddRange(ps);
			parent.RemovePackagesCalled += (ps) => expectedRemovePackagesPassedToParent.AddRange(ps);

			planner.CreatePackLines();
			AssertEquals(2, expectedAttachPackagesPassedToParent.Count);
			AssertContainsExactElementsInAnyOrder(new[] { packageState1, packageState2 }, expectedAttachPackagesPassedToParent);
		}

		public void TestCreatePacklines_AlreadyRemovedPackage()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var alreadyRemovedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { alreadyRemovedPackage }, planner.ModuleFilterPackageCollection);
			AssertEquals(0, planner.AttachedPackages.Count);

			alreadyRemovedPackage.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed; // can happen if package is attached and removed again via the form
			AssertContainsExactElementsInAnyOrder(new[] { alreadyRemovedPackage }, planner.ModuleFilterPackageCollection);
			AssertEquals(0, planner.AttachedPackages.Count);

			var expectedAttachPackagesPassedToParent = new List<ITransitPackage>();
			var expectedRemovePackagesPassedToParent = new List<ITransitPackage>();
			parent.AttachPackagesCalled += (ps) => expectedAttachPackagesPassedToParent.AddRange(ps);
			parent.RemovePackagesCalled += (ps) => expectedRemovePackagesPassedToParent.AddRange(ps);

			planner.CreatePackLines();
			AssertEquals(0, expectedAttachPackagesPassedToParent.Count);
			AssertEquals(0, expectedRemovePackagesPassedToParent.Count);
			AssertEquals(0, alreadyRemovedPackage.AdditionalReferenceNumbers.Count);
		}

		#endregion

		#region TestCancel

		public void TestCancel()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var p1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var p2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { p1, p2 }, planner.ModuleFilterPackageCollection);
			planner.ModuleFilterPackageCollection.RemoveFromRelationship(p2);
			planner.AttachedPackages.Add(p2);
			AssertEquals(p1, planner.ModuleFilterPackageCollection.Single());
			AssertEquals(p2, planner.AttachedPackages.Single());

			p1.AttachedPackageStateStatus = AttachedPackageStateStatus.Added;
			p2.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;
			AssertEquals(p1, planner.ModuleFilterPackageCollection.Single());
			AssertEquals(p2, planner.AttachedPackages.Single());

			planner.Cancel();
			AssertEquals(false, p1.IsAddedToAttachedPackageCollection);
			AssertEquals(false, p1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, p2.IsAddedToAttachedPackageCollection);
			AssertEquals(false, p2.IsRemovedFromAttachedPackageCollection);
		}

		#endregion

		#region TestResetPackageStateFlags

		public void TestResetPackageStateFlags()
		{
			var packageStateWithAllFlagsFalse = Factory.New<WhsItemPackageState>();
			var packageStateWithIsAttachedTrue = Factory.New<WhsItemPackageState>();
			var packageStateWithIsRemovedTrue = Factory.New<WhsItemPackageState>();
			packageStateWithIsAttachedTrue.AttachedPackageStateStatus = AttachedPackageStateStatus.Added;
			packageStateWithIsRemovedTrue.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;

			packageStateWithAllFlagsFalse.ResetPackageStateStatus();
			AssertEquals(false, packageStateWithAllFlagsFalse.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, packageStateWithAllFlagsFalse.IsRemovedFromAttachedPackageCollection);

			packageStateWithIsAttachedTrue.ResetPackageStateStatus();
			AssertEquals(false, packageStateWithIsAttachedTrue.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, packageStateWithIsAttachedTrue.IsRemovedFromAttachedPackageCollection);

			packageStateWithIsRemovedTrue.ResetPackageStateStatus();
			AssertEquals(false, packageStateWithIsRemovedTrue.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, packageStateWithIsRemovedTrue.IsRemovedFromAttachedPackageCollection);
		}

		#endregion

		#region TestSetParentTransportCompany

		public void TestSetParentTransportCompany_AllPackagesWithSameTranportCompany()
		{
			var transportCompany = CreateTransportCompany("TC1");
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK, vehicleRef: "V2");
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit1, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit1);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit2);
			Helper.CreateAdditionalReference(package1, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(package2, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, planner.AttachedPackages);

			planner.SetParentTranportCompany();
			AssertEquals(receiveTransportationUnit1.TransportCompany.E2_OA_Address, parent.TransportCompanyAddressPK);
		}

		public void TestSetParentTransportCompany_TransportCompanyIsOverridden()
		{
			var transportCompany = CreateTransportCompany("TC1");
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK, vehicleRef: "V2");
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit1, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var transportCompanyDocAddress = receiveTransportationUnit1.TransportCompany;
			transportCompanyDocAddress.E2_AddressOverride = true;
			transportCompanyDocAddress.CompanyName = transportCompany.OH_FullName;

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit1);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit2);
			Helper.CreateAdditionalReference(package1, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(package2, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, planner.AttachedPackages);

			planner.SetParentTranportCompany();
			AssertEquals("Should not set transport company of parent when one of AttachedPackages override the transport company", true, parent.TransportCompanyAddressPK.IsEmpty);
		}

		public void TestSetParentTransportCompany_NoTransportCompany()
		{
			var transportCompany = CreateTransportCompany("TC1");
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK, vehicleRef: "V2");
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit1);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit2);
			Helper.CreateAdditionalReference(package1, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(package2, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, planner.AttachedPackages);

			planner.SetParentTranportCompany();
			AssertEquals("Should not set transport company of parent when one AttachedPackages has no transport company", true, parent.TransportCompanyAddressPK.IsEmpty);
		}

		public void TestSetParentTransportCompany_AllPackagesWithDifferentTransportCompanies()
		{
			var transportCompany1 = CreateTransportCompany("TC1");
			var transportCompany2 = CreateTransportCompany("TC2");
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK, vehicleRef: "V2");
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit1, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany2.MainAddress);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit1);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit2);
			Helper.CreateAdditionalReference(package1, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(package2, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, planner.AttachedPackages);

			planner.SetParentTranportCompany();
			AssertEquals("Should not set transport company of parent when transport company of AttachedPackages are different", true, parent.TransportCompanyAddressPK.IsEmpty);
		}

		public void TestSetParentTransportCompany_TransportCompanyIsNotCarrier()
		{
			var transportCompany = Helper.CreateClient("TC1");
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK, vehicleRef: "V2");
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit1, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit1);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit2);
			Helper.CreateAdditionalReference(package1, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(package2, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, planner.AttachedPackages);

			planner.SetParentTranportCompany();
			AssertEquals("Should not set transport company of parent when transport company is not a carrier", true, parent.TransportCompanyAddressPK.IsEmpty);
		}

		public void TestSetParentTransportCompany_TransportCompanyIsNotLocalTransport()
		{
			var transportCompany = Helper.CreateClient("TC1");
			transportCompany.OH_IsShippingProvider = true;
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK, vehicleRef: "V2");
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit1, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit1);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit2);
			Helper.CreateAdditionalReference(package1, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(package2, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, planner.AttachedPackages);

			planner.SetParentTranportCompany();
			AssertEquals("Should not set transport company of parent when transport company is not a carrier", true, parent.TransportCompanyAddressPK.IsEmpty);
		}

		public void TestSetParentTransportCompany_NoAttachedPackages()
		{
			var transportCompany1 = CreateTransportCompany("TC1");
			var transportCompany2 = CreateTransportCompany("TC2");
			var parent = Factory.New<DummyBizOWithPackLines>();
			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK, vehicleRef: "V2");
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit1, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(receiveTransportationUnit2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany2.MainAddress);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit1);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit2);
			Factory.Save();

			AssertEquals("Precondition: No Packages attached", false, planner.AttachedPackages.Any());

			planner.SetParentTranportCompany();
			AssertEquals("Should not set transport company of parent when no AttachedPackages", true, parent.TransportCompanyAddressPK.IsEmpty);
		}

		#region TestHasSentReceiveAndDispatchInstructions

		public void TestHasSentReceiveInstructions()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.NewWithValidTestData<ForwardingShipment>();
			parent.JS_OA_ExportReceivingDepot = warehouse.WarehouseAddress.PK;
			var planner = new TransitWarehousePackagePlanner(Factory, parent);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "REF1", parentPK: parent.PK, parentCode: JobShipmentSchema.Constants.Prefix);
			var standAlonePackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Factory.Save();
			AssertEquals(true, planner.HasSentReceiveInstructions);
			AssertEquals(false, planner.HasSentDispatchInstructions);
		}

		public void TestHasSentReceiveAndDispatchInstructions_BlindPackagesAttached()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.NewWithValidTestData<ForwardingShipment>();
			parent.JS_OA_ExportReceivingDepot = warehouse.WarehouseAddress.PK;
			var planner = new TransitWarehousePackagePlanner(Factory, parent);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "REF1");
			var standAlonePackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.CreateAdditionalReference(standAlonePackageState, planner.Parent.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);

			Factory.Save();
			AssertEquals(false, planner.HasSentReceiveInstructions);
			AssertEquals(false, planner.HasSentDispatchInstructions);
		}

		public void TestHasSentDispatchInstructions()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<ForwardingShipment>();
			parent.JS_OA_ExportReceivingDepot = warehouse.WarehouseAddress.PK;

			var planner = new TransitWarehousePackagePlanner(Factory, parent);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "REF1");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, parentPK: parent.PK, parentCode: parent.TablePrefix);
			var standAlonePackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);

			Factory.Save();

			AssertEquals(true, planner.HasSentDispatchInstructions);
			AssertEquals(false, planner.HasSentReceiveInstructions);
		}

		#endregion

		OrgHeader CreateTransportCompany(string code)
		{
			var transportCompany = Helper.CreateClient(code);
			transportCompany.OH_IsShippingProvider = true;
			transportCompany.OH_IsLocalTransport = true;

			return transportCompany;
		}

		#endregion

		#region AttachPackages_InnerPackagesOfHandlingUnit

		public void TestAttachPackages_InnerPackagesOfHandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var planner = new TransitWarehousePackagePlanner(Factory, parent);

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");

			var handlingUnitPackageWithID = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackageStateWithID = Helper.CreateHandlingUnitPackage("HU1", handlingUnitPackageWithID, rtu);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageStateWithID, packageState1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackageStateWithID);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageStateWithID, packageState2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackageStateWithID);

			var standAlonePackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var handlingUnitPackageWithNoID = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackageStatWithNoID = Helper.CreateHandlingUnitPackage("", handlingUnitPackageWithNoID, rtu);
			var packageState4 = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageStatWithNoID, packageState4, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackageStatWithNoID);

			Factory.Save();

			AssertEquals("Precondition: No packages should be attached", false, planner.AttachedPackages.Any());

			var packageStateList = new List<WhsItemPackageState>()
			{
				packageState1,
				packageState2,
				standAlonePackageState,
				handlingUnitPackageStateWithID,
				packageState4
			};
			planner.AssignPackageState(packageStateList);
			AssertContainsExactElementsInAnyOrder("Only child packages with an ID should be attached to the planner", new[] { packageState1, packageState2, standAlonePackageState, packageState4 }, planner.AttachedPackages);

			Factory.Save();

			var expectedAttachPackagesPassedToParent = new List<ITransitPackage>();
			var expectedRemovePackagesPassedToParent = new List<ITransitPackage>();
			parent.AttachPackagesCalled += (ps) => expectedAttachPackagesPassedToParent.AddRange(ps);
			parent.RemovePackagesCalled += (ps) => expectedRemovePackagesPassedToParent.AddRange(ps);

			planner.CreatePackLines();
			AssertEquals(4, expectedAttachPackagesPassedToParent.Count);
			AssertEquals(0, expectedRemovePackagesPassedToParent.Count);
			AssertContainsExactElementsInAnyOrder("Only child packages with an ID should be attached ", new[] { packageState1, packageState2, standAlonePackageState, packageState4 }, expectedAttachPackagesPassedToParent);
		}

		public void TestAttachPackages_InnerPackagesOfHandlingUnit_HandlingUnitHasReference()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var planner = new TransitWarehousePackagePlanner(Factory, parent);

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");

			var standalonePackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackageStateWithID = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PkgKey2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageStateWithID, packageState1, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackageStateWithID);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageStateWithID, packageState2, ZDateTimeOffset.Now, "ABC", topHandlingUnit: handlingUnitPackageStateWithID);

			var reference = handlingUnitPackageStateWithID.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
			reference.CE_EntryNum = parent.JobNumber;

			Factory.Save();

			var packageStateList = new List<WhsItemPackageState>()
			{
				packageState1,
				packageState2,
				standalonePackageState
			};
			planner.AssignPackageState(packageStateList);
			AssertContainsExactElementsInAnyOrder("Only child packages with an ID should be attached to the planner", new[] { packageState1, packageState2, standalonePackageState }, planner.AttachedPackages);

			var expectedAttachPackagesPassedToParent = new List<ITransitPackage>();
			var expectedRemovePackagesPassedToParent = new List<ITransitPackage>();
			parent.AttachPackagesCalled += (ps) => expectedAttachPackagesPassedToParent.AddRange(ps);
			parent.RemovePackagesCalled += (ps) => expectedRemovePackagesPassedToParent.AddRange(ps);

			planner.CreatePackLines();
			AssertContainsExactElementsInAnyOrder("Packages with an ID should be attached", new[] { packageState1, packageState2, standalonePackageState }, expectedAttachPackagesPassedToParent);
			Factory.Save();

			planner.RemovePackageStates(new[] { packageState1 });
			AssertContainsExactElementsInAnyOrder("Selected packages should be removed", new[] { standalonePackageState, packageState2 }, planner.AttachedPackages);
			Factory.Save();

			planner.CreatePackLines();
			AssertContainsExactElementsInAnyOrder("Packages with an ID should be attached", new[] { packageState1 }, expectedRemovePackagesPassedToParent);
			Factory.Save();

			AssertEquals("Reference of Handling Unit should be removed", false,
				handlingUnitPackageStateWithID.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Any(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference && r.CE_EntryNum == parent.JobNumber));
		}

		#endregion

		#region TestPutBackPackageState

		public void TestPutBackPackageState()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var ps1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var ps2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			ps2.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			planner.AssignPackageState(new[] { ps1, ps2 });

			AssertContainsExactElementsInAnyOrder(new[] { ps1, ps2 }, planner.AttachedPackages);
			AssertEquals("Precondition", true, ps1.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, ps1.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, ps1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals("Precondition", true, ps2.IsAddedToAttachedPackageCollection);
			AssertEquals("Precondition", false, ps2.IsRemovedFromAttachedPackageCollection);
			AssertEquals("Precondition", false, ps2.IsRemovedFromAttachedPacklinesAndHasJobNumber);

			var isOnPackageStateCollectionChangedCalled = 0;
			planner.OnPackageStateCollectionChanged += () => isOnPackageStateCollectionChangedCalled++;
			planner.RemovePackageStates(new[] { ps1 });
			AssertEquals(1, isOnPackageStateCollectionChangedCalled);
			AssertEquals(1, planner.AttachedPackages.Count);
			AssertEquals(1, planner.RemovedPackages.Count);
			AssertContainsExactElementsInAnyOrder(new[] { ps1 }, planner.RemovedPackages);
			AssertEquals(false, ps1.IsAddedToAttachedPackageCollection);
			AssertEquals(true, ps1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, ps2.IsAddedToAttachedPackageCollection);
			AssertEquals(false, ps2.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps2.IsRemovedFromAttachedPacklinesAndHasJobNumber);

			planner.OnPackageStateCollectionChanged += () => isOnPackageStateCollectionChangedCalled++;
			planner.AssignPackageState(new[] { ps1 });
			planner.PutBackPackageState(new[] { ps1 });
			AssertEquals(3, isOnPackageStateCollectionChangedCalled);
			AssertEquals(2, planner.AttachedPackages.Count);
			AssertEquals(0, planner.RemovedPackages.Count);
			AssertEquals(true, ps1.IsAddedToAttachedPackageCollection);
			AssertEquals(false, ps1.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
			AssertEquals(true, ps2.IsAddedToAttachedPackageCollection);
			AssertEquals(false, ps2.IsRemovedFromAttachedPackageCollection);
			AssertEquals(false, ps2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = Helper.CreateTRWWarehouse().WarehouseAddress.PK;
			return new TransitWarehousePackagePlanner(Factory, parent);
		}

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
