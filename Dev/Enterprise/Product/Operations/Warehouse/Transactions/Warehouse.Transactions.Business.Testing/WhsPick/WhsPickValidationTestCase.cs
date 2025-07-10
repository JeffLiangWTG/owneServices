using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickValidationTestCase : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWP_WW_Whs

		public void TestCheckWP_WW_Whs()
		{
			var whs1 = Helper.CreateWarehouse("1");
			var whs2 = Helper.CreateWarehouse("2");
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = ZGuid.Empty;
			pick.Validation.ValidateWP_WW_Whs();
			AssertNoErrors("Empty Warehouse is allowed if no orders attached", pick.WP_WW_WhsInfo);

			pick.WP_WW_Whs = whs1.PK;
			AssertNoErrors("Should not get error if we have no orders attached", pick.WP_WW_WhsInfo);

			// longed winded to get around Pick.Orders.CountChangedEvent
			var order1 = Factory.New<WhsOrder>();
			order1.WD_WW_Whs = whs1.PK;
			order1.Lines.AddNew();
			pick.Orders.Add(order1);
			pick.WP_WW_Whs = whs2.PK;
			AssertHasError("Should get error if changing to a whs that is different to attached order", pick.WP_WW_WhsInfo, "One or more orders are attached for a different warehouse. Either change this warehouse or detach the Order(s)");

			pick.WP_WW_Whs = whs1.PK;
			AssertNoErrors("Should not get error if changing to a whs that is same as attached order", pick.WP_WW_WhsInfo);

			var order2 = Factory.New<WhsOrder>();
			order2.WD_WW_Whs = whs1.PK;
			order2.Lines.AddNew();
			pick.Orders.Add(order2);
			pick.Orders[0].WD_WW_Whs = whs2.PK;
			pick.Orders[1].WD_WW_Whs = whs2.PK;
			pick.WP_WW_Whs = whs2.PK;
			AssertNoErrors("Should not get error if changing to whs that is same as attached orders", pick.WP_WW_WhsInfo);

			pick.WP_WW_Whs = whs1.PK;
			AssertHasError("Should get error if changing to whs that is different to attached orders", pick.WP_WW_WhsInfo, "One or more orders are attached for a different warehouse. Either change this warehouse or detach the Order(s)");

			// should not get different warehouse error if warehouse is empty
			pick.WP_WW_Whs = ZGuid.Empty;
			AssertNoError(pick.WP_WW_WhsInfo, "One or more orders are attached for a different warehouse. Either change this warehouse or detach the Order(s)");
		}

		#endregion

		#region TestCheckWP_PickOption

		public void TestCheckWP_PickOption()
		{
			var pick = Helper.CreatePickNew();
			pick.WP_PickOption = "";
			pick.Validation.ValidateWP_PickOption();
			AssertNoErrors("Empty Pick Option is allowed if no orders attached", pick.WP_PickOptionInfo);

			pick.WP_PickOption = WhsPickOption.Codes.ManualWithAutoAllocate;
			AssertNoErrors("Should not get error if changing to MAN when we have no orders attached", pick.WP_PickOptionInfo);

			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			AssertNoErrors("Should not get error if changing to MAT when we have no orders attached", pick.WP_PickOptionInfo);

			var order1 = Factory.New<WhsOrder>();
			order1.WD_PickOption = WhsPickOption.Codes.Manual;
			order1.Lines.AddNew();
			pick.Orders.Add(order1);

			pick.WP_PickOption = WhsPickOption.Codes.Auto;
			AssertHasError("Should get error if changing to AUT when we have a MAN order attached", pick.WP_PickOptionInfo, "One or more orders are attached which have a different Pick Option. Either change this Pick's Option or detach the Order(s)");

			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			AssertNoErrors("Should not get error if changing to MAN when we have only a MAN order attached", pick.WP_PickOptionInfo);

			var order2 = Factory.New<WhsOrder>();
			order2.WD_PickOption = WhsPickOption.Codes.Manual;
			order2.Lines.AddNew();
			pick.Orders.Add(order2);
			pick.Orders[0].WD_PickOption = WhsPickOption.Codes.Auto;
			pick.Orders[1].WD_PickOption = WhsPickOption.Codes.Auto;
			pick.WP_PickOption = WhsPickOption.Codes.Auto;
			AssertNoErrors("Should not get error if changing to AUT when we have only AUT orders attached", pick.WP_PickOptionInfo);

			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			AssertHasError("Should get error if changing to MAN when we have atleast one AUT order attached", pick.WP_PickOptionInfo, "One or more orders are attached which have a different Pick Option. Either change this Pick's Option or detach the Order(s)");

			// should not get different pickoption error if Pickoption is empty and no orders attached
			pick.Orders.RemoveAll();
			pick.WP_PickOption = "";
			AssertNoError(pick.WP_PickOptionInfo, "One or more orders are attached which have a different Pick Option. Either change this Pick's Option or detach the Order(s)");
		}

		#endregion

		#region TestCheckWP_PickOption_OrderAlreadyAttachedInDB

		public void TestCheckWP_PickOption_OrderAlreadyAttachedInDB()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			var pick1 = Helper.CreatePickNew();
			var pick2 = Helper.CreatePickNew();
			pick1.WP_PickOption = WhsPickOption.Codes.Manual;
			pick2.WP_PickOption = WhsPickOption.Codes.Manual;

			Factory.Save();

			pick1.Orders.Add(order1);
			pick1.Orders.Add(order2);

			AssertEquals("Precondition: Pick is saved in DB.", true, pick1.IsInDatabase);
			AssertEquals("Precondition: Pick is saved in DB.", true, pick2.IsInDatabase);
			AssertEquals("Precondition: Order1 is picked", true, order1.IsAttachedToPick);
			AssertEquals("Precondition: Order2 is picked", true, order2.IsAttachedToPick);
			AssertEquals("Precondition: Order1 should have pick option 'Manual'", WhsPickOption.Codes.Manual, order1.WD_PickOption);
			AssertEquals("Precondition: Order2 should have pick option 'Manual'", WhsPickOption.Codes.Manual, order2.WD_PickOption);

			order1.WD_PickOption = WhsPickOption.Codes.Auto;
			pick1.Validation.ValidateWP_PickOption();

			AssertNotEquals("The foreign key should be different to what is in the DB as the order is not attached to the pick in the DB yet.", order1.WD_WPInfo.OriginalValue, order1.WD_WP);
			AssertHasError("Should receive an error when order pick option misaligns with pick's default option, and order is not attached to pick in DB yet. .", pick1.WP_PickOptionInfo, "One or more orders are attached which have a different Pick Option. Either change this Pick's Option or detach the Order(s)");

			Factory.Save();
			order1.WD_UnitsSent = 10;
			pick1.Validation.ValidateWP_PickOption();

			AssertEquals("The foreign key should not have changed as the pick of the order was not changed.", order1.WD_WPInfo.OriginalValue, order1.WD_WP);
			AssertNoError("Should not receive an error when changing an order property, and order is already attached to pick in DB.", pick1.WP_PickOptionInfo, "One or more orders are attached which have a different Pick Option. Either change this Pick's Option or detach the Order(s)");

			pick1.Orders.Remove(order2);
			pick2.Orders.Add(order2);
			order2.WD_PickOption = WhsPickOption.Codes.ManualWithAutoAllocate;
			pick2.Validation.ValidateWP_PickOption();

			AssertNotEquals("The foreign key should have changed as the order has been added to a new pick.", order2.WD_WPInfo.OriginalValue, order2.WD_WP);
			AssertHasError("Should receive error when order with pick option ManualWithAutoAllocate gets added to new pick with pick option Auto.", pick2.WP_PickOptionInfo, "One or more orders are attached which have a different Pick Option. Either change this Pick's Option or detach the Order(s)");

			Factory.Save();
			order1.WD_PickOption = WhsPickOption.Codes.Auto;
			pick1.Validation.ValidateWP_PickOption();

			AssertEquals("The foreign key should not have changed as the pick of the order was not changed.", order1.WD_WPInfo.OriginalValue, order1.WD_WP);
			AssertNoError("Should not receive error if order with misaligned pick option is already attached to pick in DB.", pick1.WP_PickOptionInfo, "One or more orders are attached which have a different Pick Option. Either change this Pick's Option or detach the Order(s)");
		}

		#endregion

		#region TestCheckDockDoorPK

		public void TestCheckDockDoorPK()
		{
			var ddlLocationType = Helper.CreateLocationType("XYZ", LocationClasses.Codes.DDL);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;

			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var expectedDockDoorLocationErrorMessage = "Please enter a Dock Door.";
			var expectedWarehouseErrorMessage = "This Dock Door does not belong to Warehouse on the Pick.";

			var pick = Factory.New<WhsPick>();
			AssertNoError("Precondition", pick.DockDoorPKInfo, expectedDockDoorLocationErrorMessage);
			AssertNoError("Precondition", pick.DockDoorPKInfo, expectedWarehouseErrorMessage);

			// set default dock door location
			pick.WP_WW_Whs = data.Whs1.PK;
			AssertEquals("Precondition", data.Whs1.WW_DefaultOutboundDockDoor, pick.DockDoorPK);
			AssertNoError(pick.DockDoorPKInfo, expectedDockDoorLocationErrorMessage);
			AssertNoError(pick.DockDoorPKInfo, expectedWarehouseErrorMessage);

			// set *NOT* dock door location - error
			pick.DockDoorPK = locationA2.PK;
			AssertHasError(pick.DockDoorPKInfo, expectedDockDoorLocationErrorMessage);
			AssertNoError(pick.DockDoorPKInfo, expectedWarehouseErrorMessage);

			// set different dock door location from same warehouse - no errors
			pick.DockDoorPK = locationA1.PK;
			AssertNoError(pick.DockDoorPKInfo, expectedDockDoorLocationErrorMessage);
			AssertNoError(pick.DockDoorPKInfo, expectedWarehouseErrorMessage);

			// set dock door location from different warehouse - error
			pick.DockDoorPK = whs2.WW_DefaultOutboundDockDoor;
			AssertEquals("Setting dock door location should not modify pre-set warehouse.", data.Whs1.PK, pick.WP_WW_Whs);
			AssertNoError(pick.DockDoorPKInfo, expectedDockDoorLocationErrorMessage);
			AssertHasError(pick.DockDoorPKInfo, expectedWarehouseErrorMessage);

			// removing dock door location - error (should be mandatory if any orders attached)
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			pick.Orders.Add(order);

			pick.DockDoorPK = ZGuid.Empty;
			AssertEquals("Removing dock door location should not modify pre-set warehouse.", data.Whs1.PK, pick.WP_WW_Whs);
			AssertHasErrorContaining(pick.DockDoorPKInfo, expectedDockDoorLocationErrorMessage);
			AssertNoError(pick.DockDoorPKInfo, expectedWarehouseErrorMessage);

			// change warehouse to be virtual - no error (dock door location is not mandatory for picks in virtual warehouses)
			data.Whs1.WW_IsVirtualWarehouse = true;
			pick.DockDoorPK = ZGuid.Empty;
			AssertEquals("Removing dock door location should not modify pre-set warehouse.", data.Whs1.PK, pick.WP_WW_Whs);
			AssertNoError(pick.DockDoorPKInfo, expectedDockDoorLocationErrorMessage);
			AssertNoError(pick.DockDoorPKInfo, expectedWarehouseErrorMessage);
			data.Whs1.WW_IsVirtualWarehouse = false; // clean up

			// pick for work order - no error (dock door location is not mandatory for picks for work orders)
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 10m);
			pick.Orders.Remove(order);
			pick.Orders.Remove(workOrder);

			pick.DockDoorPK = ZGuid.Empty;
			AssertEquals("Removing dock door location should not modify pre-set warehouse.", data.Whs1.PK, pick.WP_WW_Whs);
			AssertNoError(pick.DockDoorPKInfo, expectedDockDoorLocationErrorMessage);
			AssertNoError(pick.DockDoorPKInfo, expectedWarehouseErrorMessage);
		}

		public void TestCheckDockDoorPK_HasCorrectLocationStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA1.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;
			locationA2.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			AssertNoErrors(pick.DockDoorPKInfo);

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code).Where(c => c != LocationStatus.Codes.Normal))
			{
				locationA2.WLV_LocationStatus = locationStatus;
				pick.DockDoorPK = locationA2.PK;
				AssertHasError(pick.DockDoorPKInfo, "Location must have a status of NOR.");

				pick.DockDoorPK = locationA1.PK;
				AssertNoErrors(pick.DockDoorPKInfo);
			}
		}

		#endregion

		#region TestDockDoorPK_ChangeDockDoor

		public void TestDockDoorPK_ChangeDockDoor_IsPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", ResString.GetMultilingualString("Test", "Test"), false, 0, LocationClasses.Codes.DDL);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "D");
			var otherDDL = newRow.Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 2, pick.GetAllPickLines().Count());
			AssertNoErrors("Precondition.", pick.DockDoorPKInfo);

			pick.DockDoorPK = otherDDL.PK;
			AssertNoErrors("Should *not* have any errors.", pick.DockDoorPKInfo);

			pick.DockDoorPK = data.Whs1.WW_DefaultOutboundDockDoor;
			AssertNoErrors("Precondition.", pick.DockDoorPKInfo);

			var pickLine1 = pick.GetAllPickLines().First();
			pickLine1.WZ_WE_OriginalPickedInventoryLine = pickLine1.WZ_WE_InventoryLine;
			AssertEquals("Precondition.", true, pickLine1.IsPickedFromPutawayLocation);
			AssertEquals("Precondition.", false, pickLine1.IsPicked);

			pick.DockDoorPK = otherDDL.PK;
			AssertHasError("Should have an error.", pick.DockDoorPKInfo, "Cannot change Dock Door as Picking has been started or a Trolley has been created for this Pick.");

			pickLine1.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
			pick.Validation.ValidateDockDoorPK();
			AssertNoErrors("Should clear the error.", pick.DockDoorPKInfo);

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition.", true, pickLine1.IsPickedFromPutawayLocation);
			AssertEquals("Precondition.", true, pickLine1.IsPicked);
			pick.Validation.ValidateDockDoorPK();
			AssertHasError("Should have an error.", pick.DockDoorPKInfo, "Cannot change Dock Door as Picking has been started or a Trolley has been created for this Pick.");

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			pick.Validation.ValidateDockDoorPK();
			AssertNoErrors("Should clear the error.", pick.DockDoorPKInfo);
		}

		public void TestDockDoorPK_ChangeDockDoor_IsPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", ResString.GetMultilingualString("Test", "Test"), false, 0, LocationClasses.Codes.DDL);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "D");
			var otherDDL = newRow.Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 2, pick.GetAllPickLines().Count());
			AssertEquals("Should *not* be readonly.", false, pick.DockDoorPKInfo.ReadOnly);

			pick.DockDoorPK = otherDDL.PK;
			AssertNoErrors("Should *not* have any errors.", pick.DockDoorPKInfo);

			pick.DockDoorPK = data.Whs1.WW_DefaultOutboundDockDoor;
			AssertNoErrors("Precondition.", pick.DockDoorPKInfo);

			var pickLine1 = pick.GetAllPickLines().First();
			pickLine1.WZ_IsPicking = true;

			pick.DockDoorPK = otherDDL.PK;
			AssertHasError("Should have an error.", pick.DockDoorPKInfo, "Cannot change Dock Door as Picking has been started or a Trolley has been created for this Pick.");

			pickLine1.WZ_IsPicking = false;
			pick.Validation.ValidateDockDoorPK();
			AssertNoErrors("Should clear the error.", pick.DockDoorPKInfo);
		}

		public void TestDockDoorPK_ChangeDockDoor_TrolleyJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", ResString.GetMultilingualString("Test", "Test"), false, 0, LocationClasses.Codes.DDL);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "D");
			var otherDDL = newRow.Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);

			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition.", 3, pick.GetAllPickLines().Count());
			AssertEquals("Should *not* be readonly.", false, pick.DockDoorPKInfo.ReadOnly);

			pick.DockDoorPK = otherDDL.PK;
			AssertNoErrors("Should *not* have any errors.", pick.DockDoorPKInfo);

			pick.DockDoorPK = data.Whs1.WW_DefaultOutboundDockDoor;
			AssertNoErrors("Precondition.", pick.DockDoorPKInfo);

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");
			var package1 = order1.PackageJob.Packages.AddNew();
			AssertEquals("Precondition: AnyPackagesOnOrdersHasActiveTrolleyJob should be false", false, pick.AnyPackagesOnOrdersHasActiveTrolleyJob);

			pick.DockDoorPK = otherDDL.PK;
			AssertNoErrors("Should *not* have any errors.", pick.DockDoorPKInfo);

			pick.DockDoorPK = data.Whs1.WW_DefaultOutboundDockDoor;
			AssertNoErrors("Precondition.", pick.DockDoorPKInfo);

			var trolleySlot = Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);

			pick.DockDoorPK = data.Whs1.WW_DefaultOutboundDockDoor;
			AssertNoErrors("Precondition.", pick.DockDoorPKInfo);

			pick.DockDoorPK = otherDDL.PK;
			AssertHasError("Should have an error.", pick.DockDoorPKInfo, "Cannot change Dock Door as Picking has been started or a Trolley has been created for this Pick.");

			((BusinessObject)trolleySlot).Delete();
			pick.Validation.ValidateDockDoorPK();
			AssertNoErrors("Should clear the error.", pick.DockDoorPKInfo);
		}

		public void TestDockDoorPK_AllowsChangingFromEmpty()
		{
			// This should not be possible, but may happen due to bugs (this has happened)
			// The intention of the existing validation is to ensure the user can't change the dockdoor during picking, but if something goes wrong and a pick exists without a dock door, we should be able to then set one.
			// As scanning DDLs is mandatory, this can change a CR3 into a CR4 or possibly a CR5 and causes no problems that we are aware of.
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.DockDoorPK = ZGuid.Empty; // user cannot do this here

			var pickLine = pick.GetAllPickLines().First();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsPick_DockDoorLocationIsCorrect", WhsPickSchema.Constants.TableName)) // Generally, this datashape should not be possible -- but problems may exist
			{
				Factory.Save();
			}
			AssertEquals("Precondition: Empty Dockdoor", ZGuid.Empty, pick.DockDoorPK);

			pick.DockDoorPK = data.Whs1.WW_DefaultOutboundDockDoor;
			AssertNoErrors("Should allow setting a dockdoor from empty.", pick.DockDoorPKInfo);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestWP_WA_DynamicPickAreaOverride

		public void TestWP_WA_DynamicPickAreaOverride_MustBeSameWhs()
		{
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var dynamicAreaSameWhs = Helper.CreateArea(data.Whs1, "AreaSame", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocSameWhs = Factory.New<WhsLocation>();
			dynamicLocSameWhs.WLV_PutawayPathSequence = 1;
			dynamicLocSameWhs.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocSameWhs.WLV_WA_PickingArea = dynamicAreaSameWhs.PK;

			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			var area2 = Helper.CreateArea(whs2, "Area2", AreaTypes.Codes.DynamicPickFace, true, false);
			var location2 = Factory.New<WhsLocation>();
			location2.WLV_PutawayPathSequence = 1;
			location2.WLV_WA_PickingArea = area2.PK;
			location2.WLV_WLT_LocationType = dynamicLocationType.PK;

			var expectedWarehouseErrorMessage = "Enter a valid Dynamic Area Override.";

			var pick = Factory.New<WhsPick>();
			AssertNoError("Precondition", pick.WP_WA_DynamicPickAreaOverrideInfo, expectedWarehouseErrorMessage);

			pick.WP_WW_Whs = data.Whs1.PK;
			AssertEquals("Precondition", ZGuid.Empty, pick.WP_WA_DynamicPickAreaOverride);
			AssertNoError(pick.WP_WA_DynamicPickAreaOverrideInfo, expectedWarehouseErrorMessage);

			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			AssertNoError(pick.WP_WA_DynamicPickAreaOverrideInfo, expectedWarehouseErrorMessage);

			pick.WP_WA_DynamicPickAreaOverride = area2.PK;
			AssertHasError(pick.WP_WA_DynamicPickAreaOverrideInfo, expectedWarehouseErrorMessage);

			pick.WP_WA_DynamicPickAreaOverride = dynamicAreaSameWhs.PK;
			AssertNoError(pick.WP_WA_DynamicPickAreaOverrideInfo, expectedWarehouseErrorMessage);
		}

		public void TestWP_WA_DynamicPickAreaOverride_AreaMustBeDynamic()
		{
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var normalLocationType = Helper.CreateLocationType("NRD", LocationClasses.Codes.NOR);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var anotherDynamicArea = Helper.CreateArea(data.Whs1, "AreaSame", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocSameWhs = Factory.New<WhsLocation>();
			dynamicLocSameWhs.WLV_PutawayPathSequence = 1;
			dynamicLocSameWhs.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocSameWhs.WLV_WA_PickingArea = anotherDynamicArea.PK;

			var nonDynAreaSameWhs = Helper.CreateArea(data.Whs1, "Nondyn");
			var nonDynLocSameWhs = Factory.New<WhsLocation>();
			nonDynLocSameWhs.WLV_PutawayPathSequence = 1;
			nonDynLocSameWhs.WLV_WLT_LocationType = normalLocationType.PK;
			nonDynLocSameWhs.WLV_WA_PickingArea = nonDynAreaSameWhs.PK;

			var expectedDynamicAreaErrorMessage = "Enter a valid Dynamic Area Override.";

			var pick = Factory.New<WhsPick>();
			AssertNoError("Precondition", pick.WP_WA_DynamicPickAreaOverrideInfo, expectedDynamicAreaErrorMessage);

			pick.WP_WW_Whs = data.Whs1.PK;
			AssertEquals("Precondition", ZGuid.Empty, pick.WP_WA_DynamicPickAreaOverride);
			AssertNoError(pick.WP_WA_DynamicPickAreaOverrideInfo, expectedDynamicAreaErrorMessage);

			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			AssertNoError(pick.WP_WA_DynamicPickAreaOverrideInfo, expectedDynamicAreaErrorMessage);

			pick.WP_WA_DynamicPickAreaOverride = anotherDynamicArea.PK;
			AssertNoError(pick.WP_WA_DynamicPickAreaOverrideInfo, expectedDynamicAreaErrorMessage);

			pick.WP_WA_DynamicPickAreaOverride = nonDynAreaSameWhs.PK;
			AssertHasError(pick.WP_WA_DynamicPickAreaOverrideInfo, expectedDynamicAreaErrorMessage);
		}

		public void TestWP_WA_DynamicPickAreaOverride_ValidDynamicErrorShowFirst()
		{
			var normalLocationType = Helper.CreateLocationType("NRD", LocationClasses.Codes.NOR);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			var area3 = Helper.CreateArea(whs2, "Area3");
			var location3 = Factory.New<WhsLocation>();
			location3.WLV_WA_PickingArea = area3.PK;
			location3.WLV_PutawayPathSequence = 1;
			location3.WLV_WLT_LocationType = normalLocationType.PK;

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			AssertEquals("Precondition", ZGuid.Empty, pick.WP_WA_DynamicPickAreaOverride);

			pick.WP_WA_DynamicPickAreaOverride = area3.PK;
			AssertEquals("Only 1 error should be shown", 1, pick.WP_WA_DynamicPickAreaOverrideInfo.GetErrors().Count());
			AssertHasError(pick.WP_WA_DynamicPickAreaOverrideInfo, "Enter a valid Dynamic Area Override.");
		}

		public void TestCheckWP_WA_DynamicPickAreaOverride_AreaHasLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicArea = Helper.CreateDynamicPF(data.Whs1, data.Whs1.FindLocation("A-1"));
			var noLocArea = Helper.CreateArea(data.Whs1, "NOLOC", AreaTypes.Codes.DynamicPickFace, true, false);
			Factory.Save();

			var expectedNoLocAreaErrorMessage = "Assigned Dynamic Area Override should have at least 1 Pick Location.";

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			AssertNoError("Precondition", pick.WP_WA_DynamicPickAreaOverrideInfo, expectedNoLocAreaErrorMessage);

			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			AssertNoErrors(pick.WP_WA_DynamicPickAreaOverrideInfo);

			pick.WP_WA_DynamicPickAreaOverride = noLocArea.PK;
			AssertHasError(pick.WP_WA_DynamicPickAreaOverrideInfo, expectedNoLocAreaErrorMessage);
		}

		#endregion

		#region TestWP_WA_DynamicPickAreaOverride_ChangeArea

		public void TestWP_WA_DynamicPickAreaOverride_ChangeArea_IsPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var dynamicAreaSameWhs = Helper.CreateArea(data.Whs1, "AreaSame", AreaTypes.Codes.DynamicPickFace, true, false);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "T");
			var dynamicLocSameWhs = row2.Locations[0];
			dynamicLocSameWhs.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocSameWhs.WLV_WA_PickingArea = dynamicAreaSameWhs.PK;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 2, pick.GetAllPickLines().Count());
			AssertNoErrors("Precondition.", pick.WP_WA_DynamicPickAreaOverrideInfo);

			pick.WP_WA_DynamicPickAreaOverride = dynamicAreaSameWhs.PK;
			AssertNoErrors("Should *not* have any errors.", pick.WP_WA_DynamicPickAreaOverrideInfo);

			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			AssertNoErrors("Precondition.", pick.WP_WA_DynamicPickAreaOverrideInfo);
			Factory.Save();

			var pickLine1 = pick.GetAllPickLines().First();
			pickLine1.WZ_WE_OriginalPickedInventoryLine = pickLine1.WZ_WE_InventoryLine;
			AssertEquals("Precondition.", true, pickLine1.IsPickedFromPutawayLocation);
			AssertEquals("Precondition.", false, pickLine1.IsPicked);

			pick.WP_WA_DynamicPickAreaOverride = dynamicAreaSameWhs.PK;
			AssertHasError("Should have an error.", pick.WP_WA_DynamicPickAreaOverrideInfo, "Cannot change Dynamic Area Override as Picking has been started or a Trolley has been created for this Pick.");

			pickLine1.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
			pick.Validation.ValidateWP_WA_DynamicPickAreaOverride();
			AssertNoErrors("Should clear the error.", pick.WP_WA_DynamicPickAreaOverrideInfo);

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition.", true, pickLine1.IsPickedFromPutawayLocation);
			AssertEquals("Precondition.", true, pickLine1.IsPicked);
			pick.Validation.ValidateWP_WA_DynamicPickAreaOverride();
			AssertHasError("Should have an error.", pick.WP_WA_DynamicPickAreaOverrideInfo, "Cannot change Dynamic Area Override as Picking has been started or a Trolley has been created for this Pick.");

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			pick.Validation.ValidateWP_WA_DynamicPickAreaOverride();
			AssertNoErrors("Should clear the error.", pick.WP_WA_DynamicPickAreaOverrideInfo);
		}

		public void TestWP_WA_DynamicPickAreaOverride_ChangeArea_IsPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var otherDynamicArea = Helper.CreateArea(data.Whs1, "AreaSame", AreaTypes.Codes.DynamicPickFace, true, false);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "T");
			var dynamicLocSameWhs = row2.Locations[0];
			dynamicLocSameWhs.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocSameWhs.WLV_WA_PickingArea = otherDynamicArea.PK;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			AssertEquals("Precondition: Pick has no picklines.", 0, pick.GetAllPickLines().Count());

			pick.WP_WA_DynamicPickAreaOverride = otherDynamicArea.PK;
			AssertNoErrors("Should *not* have any errors.", pick.WP_WA_DynamicPickAreaOverrideInfo);

			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			AssertNoErrors("Should still *not* have any errors.", pick.WP_WA_DynamicPickAreaOverrideInfo);

			pick.PickOrdersWithAllocationMock(order);
			var pickLine1 = pick.GetAllPickLines().First();
			pickLine1.WZ_IsPicking = true;

			pick.WP_WA_DynamicPickAreaOverride = otherDynamicArea.PK;
			AssertHasError("Should have an error.", pick.WP_WA_DynamicPickAreaOverrideInfo, "Cannot change Dynamic Area Override as Picking has been started or a Trolley has been created for this Pick.");

			pickLine1.WZ_IsPicking = false;
			pick.Validation.ValidateWP_WA_DynamicPickAreaOverride();
			AssertNoErrors("Should clear the error.", pick.WP_WA_DynamicPickAreaOverrideInfo);
		}

		public void TestWP_WA_DynamicPickAreaOverride_ChangeArea_OrderWithPalletIDIsAttached()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5),
				data.Part1, 100m, data.Whs1.FindLocation("A-2"), "ABC");

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderWithPalletID = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			Helper.CreateWhsOrderLine(orderWithPalletID, data.Part1, 10m, "ABC");

			var pick = Helper.CreatePickNew(order, orderWithPalletID);
			Factory.Save();

			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			AssertHasError("Should have an error.", pick.WP_WA_DynamicPickAreaOverrideInfo, "Cannot set Dynamic Area Override as there is an Order with Pallet ID on the Line attached to this Pick.");

			pick.WP_WA_DynamicPickAreaOverride = ZGuid.Empty;
			pick.Orders.Remove(orderWithPalletID);
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			AssertNoErrors("Should *not* have any errors.", pick.WP_WA_DynamicPickAreaOverrideInfo);
		}

		#endregion

		#region TestCheckWP_IsAwaitingReplenishment

		public void TestCheckWP_IsAwaitingReplenishment_Order() => TestCheckWP_IsAwaitingReplenishment_Core(PickType.Codes.Order, isExpectingError: false);
		public void TestCheckWP_IsAwaitingReplenishment_HeldInventoryOrder_HasError() => TestCheckWP_IsAwaitingReplenishment_Core(PickType.Codes.HeldInventoryOrder, isExpectingError: true);
		public void TestCheckWP_IsAwaitingReplenishment_WorkOrder() => TestCheckWP_IsAwaitingReplenishment_Core(PickType.Codes.WorkOrder, isExpectingError: false);
		public void TestCheckWP_IsAwaitingReplenishment_DynamicWorkOrder() => TestCheckWP_IsAwaitingReplenishment_Core(PickType.Codes.DynamicWorkOrder, isExpectingError: false);

		void TestCheckWP_IsAwaitingReplenishment_Core(string pickType, bool isExpectingError)
		{
			var pick = Factory.New<WhsPick>();
			pick.WP_PickType = pickType;
			
			Factory.Save();

			AssertEquals("Precondition", false, pick.HasErrors);

			pick.WP_IsAwaitingReplenishment = true;

			if (isExpectingError)
			{
				AssertHasError("Picks with held inventory orders can't wait for replenishment.", pick.WP_IsAwaitingReplenishmentInfo, "Pick with orders containing held inventory cannot be waiting for replenishment.");
			}
			else
			{
				AssertEquals(false, pick.HasErrors);
			}
		}

		#endregion

		#region TestDockDoorLocationString

		public void TestDockDoorLocationString()
		{
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.FindLocation("A-1").WLV_WLT_LocationType = ddlLocationType.PK;
			whs2.FindLocation("B-1").WLV_WLT_LocationType = ddlLocationType.PK;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			AssertNoErrors("Precondition", pick.DockDoorLocationStringInfo);
			AssertNotNull("Precondition", data.Whs1.FindLocation("A-2"));
			AssertNotNull("Precondition", data.Whs1.DefaultInboundDockDoorLocation);
			AssertNotNull("Precondition", whs2.DefaultInboundDockDoorLocation);

			// set *NOT* dock door location - error
			pick.DockDoorLocationString = "A-2";
			AssertHasError(pick.DockDoorLocationStringInfo, "Please enter a Dock Door.");

			// set different dock door location from same warehouse - no errors
			pick.DockDoorLocationString = "A-1";
			AssertNoErrors(pick.DockDoorLocationStringInfo);

			// set dock door location from different warehouse - error
			pick.DockDoorLocationString = "B-1";
			AssertEquals("Setting dock door location should not modify pre-set warehouse.", data.Whs1.PK, pick.WP_WW_Whs);
			AssertHasError(pick.DockDoorLocationStringInfo, "Enter a valid Dock Door.");

			pick.DockDoorLocationString = "";
			AssertEquals("Removing dock door location should not modify pre-set warehouse.", data.Whs1.PK, pick.WP_WW_Whs);
			AssertHasError(pick.DockDoorLocationStringInfo, "Please enter a Dock Door.");

			// change warehouse to be virtual - no error (dock door location is not mandatory for picks in virtual warehouses)
			data.Whs1.WW_IsVirtualWarehouse = true;
			pick.DockDoorLocationString = "";
			AssertEquals("Removing dock door location should not modify pre-set warehouse.", data.Whs1.PK, pick.WP_WW_Whs);
			AssertNoErrors(pick.DockDoorLocationStringInfo);
			data.Whs1.WW_IsVirtualWarehouse = false; // clean up

			// pick for work order - no error (dock door location is not mandatory for picks for work orders)
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 10m);
			pick.Orders.Remove(order);
			pick.Orders.Remove(workOrder);

			pick.DockDoorLocationString = "";
			AssertEquals("Removing dock door location should not modify pre-set warehouse.", data.Whs1.PK, pick.WP_WW_Whs);
			AssertNoErrors(pick.DockDoorLocationStringInfo);
		}

		#endregion

		#region TestValidateAllForDBHits

		public void TestValidateAllForDBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var part3 = Helper.CreateProduct(data.Org1, "P3");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 2m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 1m, location2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 1m, location2, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m, WhsPickOption.Codes.Manual);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 1m, WhsPickOption.Codes.Manual);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", part3, 1m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order1, order2, order3);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			pickInNewFactory.AutoAllocateItemsWithMock();
			AssertEquals("WhsPickLines table is hit once for WZ_WE_TransactionLine, once for committed quantity validation in AutoAllocate Items.", 2, newFactory.GetTableHitCount(WhsPickLineSchema.Constants.TableName));

			pickInNewFactory.RunPreSaveValidation();
			// increased by A.V from 3 to 4
			AssertEquals("WhsPickLines table is hit once for run pre-save validation.", 2 + 2, newFactory.GetTableHitCount(WhsPickLineSchema.Constants.TableName));
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var validation = new TestWhsPickValidationValidation(Pick);

			var list = new string[]
			{
				WhsPickSchema.Constants.WP_WL_DockDoor,
				WhsPickSchema.Constants.WP_WA_DynamicPickAreaOverride
			};

			foreach (var propertyInfo in Pick.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsCartonGroupSizeLinkValidation

		class TestWhsPickValidationValidation : WhsPickValidation
		{
			public TestWhsPickValidationValidation(WhsPick parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Pick = Factory.New<WhsPick>();
		}
		protected WhsPick Pick;

		#endregion
	}
}
