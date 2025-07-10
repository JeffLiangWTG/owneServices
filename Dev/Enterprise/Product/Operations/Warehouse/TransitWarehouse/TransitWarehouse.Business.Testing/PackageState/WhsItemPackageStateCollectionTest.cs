using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemPackageStateCollection))]
	public class WhsItemPackageStateCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemPackageStateCollection>
	{
		#region TestOnlyReturnPackagesInWarehouse

		public void TestOnlyReturnPackagesInWarehouse()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehousePackagePlanner = new TransitWarehousePackagePlanner(Factory, parent);
			var bookedPackageState = Factory.New<WhsItemPackageState>();
			var arrivedPackageState = Factory.New<WhsItemPackageState>();
			var putawayPackageState = Factory.New<WhsItemPackageState>();
			var stagedPackageState = Factory.New<WhsItemPackageState>();
			var committedToTransferPackageState = Factory.New<WhsItemPackageState>();
			var loadedPackageState = Factory.New<WhsItemPackageState>();
			var departedPackageState = Factory.New<WhsItemPackageState>();
			bookedPackageState.WPS_Status = "BKD";
			arrivedPackageState.WPS_Status = "ARV";
			putawayPackageState.WPS_Status = "PUT";
			stagedPackageState.WPS_Status = "STA";
			committedToTransferPackageState.WPS_Status = "CTT";
			loadedPackageState.WPS_Status = "FLO";
			departedPackageState.WPS_Status = "DEP";

			var packageStates = new WhsItemPackageStateCollection(transitWarehousePackagePlanner);
			AssertContainsExactElementsInAnyOrder(new[] { arrivedPackageState, putawayPackageState, stagedPackageState, committedToTransferPackageState }, packageStates);
		}

		#endregion

		#region TestAttachedPackages_HandlingUnit

		public void TestAttachedPackages_HandlingUnit()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var jobNumber = ((ITransitWarehouseParent)parent).JobNumber;
			var warehouse1 = Helper.CreateTRWWarehouse("T1");
			var warehouse2 = Helper.CreateTRWWarehouse("T2");
			parent.TransitWarehouseAddressPK = warehouse1.WarehouseAddress.PK;
			var childPackageStatesInT1 = CreateAttachedPackagesForWarhouse(jobNumber, warehouse1);
			var childPackageStatesInT2 = CreateAttachedPackagesForWarhouse(jobNumber, warehouse2);

			var planner = new TransitWarehousePackagePlanner(Factory, parent);
			AssertEquals("No packages are attached.", 0, new WhsItemPackageStateCollection(planner, jobNumber).Count);

			Factory.Save();
			AssertContainsExactElementsInAnyOrder(childPackageStatesInT1, new WhsItemPackageStateCollection(planner, jobNumber));

			parent.TransitWarehouseAddressPK = warehouse2.WarehouseAddress.PK;
			AssertContainsExactElementsInAnyOrder(childPackageStatesInT2, new WhsItemPackageStateCollection(planner, jobNumber));
		}

		IEnumerable<WhsItemPackageState> CreateAttachedPackagesForWarhouse(string jobNumber, WhsWarehouse warehouse)
		{
			var code = warehouse.WW_WarehouseCode;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment($"RCN1{code}", "STD", warehouse.PK, $"EXTREF1{code}");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit($"RTU1{code}", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList($"DLL1{code}", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment($"DCN1{code}", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit($"DTU1{code}", warehouse.PK);
			var childPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", $"PKG{code}1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: jobNumber);
			var childPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", $"PKG{code}2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: jobNumber);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage($"HU1{code}", handlingUnit, receiveTransportationUnit, entryNum: jobNumber);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			return new[] { childPackageState1, childPackageState2 };
		}

		#endregion

		#region TestAttachedPackages

		public void TestAttachedPackages()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
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
			AssertEquals("No packages are attached.", 0, new WhsItemPackageStateCollection(planner, ((ITransitWarehouseParent)parent).JobNumber).Count);

			Helper.CreateAdditionalReference(arrived, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { arrived }, new WhsItemPackageStateCollection(planner, ((ITransitWarehouseParent)parent).JobNumber));

			Helper.CreateAdditionalReference(putaway, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { putaway, arrived }, new WhsItemPackageStateCollection(planner, ((ITransitWarehouseParent)parent).JobNumber));

			Helper.CreateAdditionalReference(committed, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { putaway, arrived, committed }, new WhsItemPackageStateCollection(planner, ((ITransitWarehouseParent)parent).JobNumber));

			Helper.CreateAdditionalReference(picked, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { putaway, arrived, committed, picked }, new WhsItemPackageStateCollection(planner, ((ITransitWarehouseParent)parent).JobNumber));

			Helper.CreateAdditionalReference(staged, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { staged, putaway, arrived, committed, picked }, new WhsItemPackageStateCollection(planner, ((ITransitWarehouseParent)parent).JobNumber));
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
			AssertContainsExactElementsInAnyOrder("All non departed and FLO packages must be returned.", new[] { arrived, putaway, committed, picked, staged }, new WhsItemPackageStateCollection(planner));

			Helper.CreateAdditionalReference(arrived, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { putaway, committed, picked, staged }, new WhsItemPackageStateCollection(planner));

			Helper.CreateAdditionalReference(putaway, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { committed, picked, staged }, new WhsItemPackageStateCollection(planner));

			Helper.CreateAdditionalReference(committed, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { picked, staged }, new WhsItemPackageStateCollection(planner));

			Helper.CreateAdditionalReference(picked, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { staged }, new WhsItemPackageStateCollection(planner));

			Helper.CreateAdditionalReference(staged, ((ITransitWarehouseParent)parent).JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Factory.Save();
			AssertEquals(0, new WhsItemPackageStateCollection(planner).Count);
		}

		#endregion

		#region Implementation

		protected override WhsItemPackageStateCollection GetCollectionToTest()
		{
			var dispatchUnit = Factory.New<WhsItemDispatchTransportationUnit>();
			return new WhsItemPackageStateCollection(dispatchUnit);
		}

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}

