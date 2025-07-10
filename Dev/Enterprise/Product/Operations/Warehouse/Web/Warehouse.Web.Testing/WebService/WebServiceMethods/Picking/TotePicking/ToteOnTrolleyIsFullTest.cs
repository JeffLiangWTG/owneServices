using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using static Enterprise.Core.Constants;
using PickTrolleyStatus = Enterprise.Warehouse.Transactions.TrolleyPicking.PickTrolleyStatus;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ToteOnTrolleyIsFullTest : WhsSecureServiceTestCase
	{
		public void TestToteOnTrolleyIsFull_Empty()
		{
			var webService = GetNewWebService();
			var response = webService.ToteOnTrolleyIsFull(Guid.Empty, "");
			AssertEquals("Should Error as no Tote given.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Trolley job was not found. Cannot set Tote as Full.", response.ErrorMessage);
		}

		public void TestToteOnTrolleyIsFull_ToteOnTrolleyBuilding()
		{
			var webService = GetNewWebService();

			var helper = new WhsTestHelperFunctions(webService.Factory);
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "");
			AssertEquals("Should Error as Tote is not on a Trolley.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("This Trolley job is in BLD state. Cannot set Tote as Full.", response.ErrorMessage);
		}

		public void TestToteOnTrolleyIsFull_NoSlots()
		{
			var webService = GetNewWebService();

			var helper = new WhsTestHelperFunctions(webService.Factory);
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "ABC");
			AssertEquals("Should Error as Package is not a Tote.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Tote was not found. Cannot set Tote as Full.", response.ErrorMessage);
		}

		public void TestToteOnTrolleyIsFull_PackageNotTote()
		{
			var webService = GetNewWebService();
			var package = webService.Factory.New<PkgPackage>();
			package.KP_PackageID = "ABC";
			package.KP_F3_NKPackType = "CTN";

			var helper = new WhsTestHelperFunctions(webService.Factory);
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			var trolleySlot = helper.CreateWhsPickTrolleySlot(trolleyJob, package, 1);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "ABC");
			AssertEquals("Should Error as Package is not a Tote.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package has a Pack Type of 'CTN'. Cannot set Tote as Full.", response.ErrorMessage);
		}

		public void TestToteOnTrolleyIsFull_ToteNothingPicked()
		{
			var webService = GetNewWebService();
			var tote = webService.Factory.New<PkgPackage>();
			tote.KP_PackageID = "ABC";
			tote.SetIsTote(true);

			var helper = new WhsTestHelperFunctions(webService.Factory);
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			var trolleySlot = helper.CreateWhsPickTrolleySlot(trolleyJob, tote, 5);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "ABC");
			AssertEquals("Should Error as Tote is not on a Trolley.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Tote does not contain any items. Cannot set Tote as Full.", response.ErrorMessage);
		}

		public void TestToteOnTrolleyIsFull_WithOrder_NothingPicked()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			// create stock
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			webService.Factory.Save();

			// order stock
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);

			// create valid pick for Tote Picking
			var pick = helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: Pick should have 2 PickLines.", 2, pickLines.Count());
			pickLines.ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition: OrderLine PickLineQuantity should total 10.", 10m, order.Lines[0].PickLineQuantity);

			// create Trolley
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			// create Tote (for Order)
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);

			// create Slot and Assign Tote
			var trolleySlot = helper.CreateWhsPickTrolleySlot(trolleyJob, packageTote, 1);
			pickLines.ForEach(l => packingHelper.CreatePackageDivot(packageTote, l));

			// confirm Release Line is packed 
			var releaseLine = (IPackableItemParent)order.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).Single();
			AssertEquals("The Tote should contain the ReleaseLine.", true, packageTote.IsPacked(releaseLine));

			// confirm Release Line has both Packable Items and they are packed
			var pickLineFor4 = pickLines.Single(l => l.WZ_Units == 4);
			var pickLineFor6 = pickLines.Single(l => l.WZ_Units == 6);
			AssertContainsExactElementsInAnyOrder(new[] { pickLineFor4, pickLineFor6 }, releaseLine.PackableItems);
			AssertEquals("The Tote should contain PickLineFor4.", true, packageTote.IsPacked(pickLineFor4));
			AssertEquals("The Tote should contain PickLineFor6.", true, packageTote.IsPacked(pickLineFor6));
			webService.Factory.Save();

			// assign 4 Units to BOB, but DON'T Pick
			pickLineFor4.WZ_GS_NKAssignedTo = "BOB";
			pickLineFor4.WZ_IsPicking = true;

			// assign 6 Units to BOB, but DON'T Pick
			pickLineFor6.WZ_GS_NKAssignedTo = "BOB";
			pickLineFor6.WZ_IsPicking = true;

			// mark Tote as Full
			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			AssertEquals("Should Error as Tote is not on a Trolley.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Tote does not contain any items. Cannot set Tote as Full.", response.ErrorMessage);
		}

		public void TestToteOnTrolleyIsFull_ToteOnTrolleyAndOrderPicked()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var bob = helper.CreateGlbStaff("BOB", "Bob");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			// create stock
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			webService.Factory.Save();

			// order stock
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);

			// create valid pick for Tote Picking
			var pick = helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: Pick should have 2 PickLines.", 2, pickLines.Count());
			pickLines.ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition: OrderLine PickLineQuantity should total 10.", 10m, order.Lines[0].PickLineQuantity);

			// create Trolley
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			// create Tote (for Order)
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			var wrongTote = packingHelper.CreatePackage(packageJob, "Tote2", 1, Constants.PkgUnit.Box);
			wrongTote.SetIsTote(true);

			// create Slot and Assign Tote
			var trolleySlot = helper.CreateWhsPickTrolleySlot(trolleyJob, packageTote, 5);
			pickLines.ForEach(l => packingHelper.CreatePackageDivot(packageTote, l));
			var wrongTrolleySlot = helper.CreateWhsPickTrolleySlot(trolleyJob, wrongTote, 1);

			// confirm Release Line is packed 
			var releaseLine = (IPackableItemParent)order.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).Single();
			AssertEquals("The Tote should contain the ReleaseLine.", true, packageTote.IsPacked(releaseLine));

			// confirm Release Line has both Packable Items and they are packed
			var pickLineFor4 = pickLines.Single(l => l.WZ_Units == 4);
			var pickLineFor6 = pickLines.Single(l => l.WZ_Units == 6);
			AssertContainsExactElementsInAnyOrder(new[] { pickLineFor4, pickLineFor6 }, releaseLine.PackableItems);
			AssertEquals("The Tote should contain PickLineFor4.", true, packageTote.IsPacked(pickLineFor4));
			AssertEquals("The Tote should contain PickLineFor6.", true, packageTote.IsPacked(pickLineFor6));
			webService.Factory.Save();

			// assign 4 Units to BOB and Pick
			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			pickLineFor4.WZ_GS_NKAssignedTo = "BOB";
			pickLineFor4.WZ_PickedDateTime = now;

			// assign 6 Units to BOB, but DON'T Pick
			pickLineFor6.WZ_GS_NKAssignedTo = "BOB";
			pickLineFor6.WZ_IsPicking = true;

			// mark Tote as Full
			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			AssertEquals("Should Not Error.", ErrorTypes.None, response.Error);
			AssertEquals("Should have no Error Message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("The Tote should contain PickLineFor4.", true, packageTote.IsPacked(pickLineFor4));

			var pickedPickLine = pickLineFor4.InventoryLine.PickLines.Single();
			AssertEquals("Picked PickLine should still be assigned to BOB.", "BOB", pickedPickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Picked PickLine should not be Picking.", false, pickedPickLine.WZ_IsPicking);
			AssertEquals("Picked PickLine should still be Picked.", now, pickedPickLine.WZ_PickedDateTime);
			AssertEquals("PickLineFor4 should not be assigned to anyone.", "", pickLineFor4.WZ_GS_NKAssignedTo);
			AssertEquals("PickLineFor4 should not be Picking.", false, pickLineFor4.WZ_IsPicking);
			AssertEquals("PickLineFor4 should not have a Picked Time.", ZDateTimeOffset.Empty, pickLineFor4.WZ_PickedDateTime);
			AssertEquals("PickLineFor4 should have IsPickedFromPutawayLocation as true.", true, pickLineFor4.IsPickedFromPutawayLocation);
			AssertEquals("The Tote should have unpacked PickLineFor6.", false, packageTote.IsPacked(pickLineFor6));
			AssertEquals("PickLineFor6 should be free for another User to Pick.", "", pickLineFor6.WZ_GS_NKAssignedTo);
			AssertEquals("PickLineFor6 should no longer be Picking.", false, pickLineFor6.WZ_IsPicking);
			AssertEquals("PickLineFor6 should still not be Picked.", ZDateTimeOffset.Empty, pickLineFor6.WZ_PickedDateTime);
		}

		public void TestToteOnTrolleyIsFull_ToteOnTrolleyAndOrderPicked_DockDoorTransferCreated()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			helper.CreateGlbStaff("BOB", "Bob");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			// create stock
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			webService.Factory.Save();

			// order stock
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);

			// create valid pick for Tote Picking
			var pick = helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: Pick should have 2 PickLines.", 2, pickLines.Length);
			pickLines.ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition: OrderLine PickLineQuantity should total 10.", 10m, order.Lines[0].PickLineQuantity);

			// create Trolley
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			// create Tote (for Order)
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			var wrongTote = packingHelper.CreatePackage(packageJob, "Tote2", 1, Constants.PkgUnit.Box);
			wrongTote.SetIsTote(true);

			// create Slot and Assign Tote
			helper.CreateWhsPickTrolleySlot(trolleyJob, packageTote, 5);
			pickLines.ForEach(l => packingHelper.CreatePackageDivot(packageTote, l));
			helper.CreateWhsPickTrolleySlot(trolleyJob, wrongTote, 1);

			// confirm Release Line is packed 
			var releaseLine = (IPackableItemParent)order.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).Single();
			AssertEquals("The Tote should contain the ReleaseLine.", true, packageTote.IsPacked(releaseLine));

			// confirm Release Line has both Packable Items and they are packed
			var pickLineFor4 = pickLines.Single(l => l.WZ_Units == 4);
			var pickLineFor6 = pickLines.Single(l => l.WZ_Units == 6);
			AssertContainsExactElementsInAnyOrder(new[] { pickLineFor4, pickLineFor6 }, releaseLine.PackableItems);
			AssertEquals("The Tote should contain PickLineFor4.", true, packageTote.IsPacked(pickLineFor4));
			AssertEquals("The Tote should contain PickLineFor6.", true, packageTote.IsPacked(pickLineFor6));
			webService.Factory.Save();

			// assign 4 Units to BOB and Pick
			var now = ZDateTimeOffset.Now;
			pickLineFor4.WZ_GS_NKAssignedTo = "BOB";
			pickLineFor4.WZ_PickedDateTime = now;
			webService.Factory.Save();

			var dockDoorTransfer = pick.Transfers.Single();
			AssertEquals("PreCondition, dock door transfer was created.", true, dockDoorTransfer != null);
			var clonedPickLine = dockDoorTransfer.Lines[0].PickLines[0];
			AssertEquals("PreCondition, pickline for transfer was created.", true, clonedPickLine != null);
			AssertEquals("PreCondition, cloned pick line is picked.", true, clonedPickLine.IsPicked);

			// mark Tote as Full
			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			AssertEquals("Response should not be Error.", ErrorTypes.None, response.Error);
			AssertEquals("Response should have no Error Message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("The Tote should contain PickLineFor4.", true, packageTote.IsPacked(pickLineFor4));
			AssertEquals("The Tote should not contain PickLineFor6 as it's removed from the package.", false, packageTote.IsPacked(pickLineFor6));
		}

		public void TestToteOnTrolleyIsFull_ToteOnTrolleyAndOrderPicked_ReleaseCapturedAttribute()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			helper.CreateGlbStaff("BOB", "Bob");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true, setReleaseCaptured: true);

			// create stock
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 8m);
			webService.Factory.Save();

			// order stock
			//var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			helper.CreateWhsOrderLine(order, data.Part1, 10);
			helper.CreateWhsOrderLine(order, data.Part2, 8);
			var pick = helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			webService.Factory.Save();

			order.Lines[0].ReleaseLines[0].PartAttribute1 = "123";

			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: Pick should have 3 PickLines.", 3, pickLines.Length);
			pickLines.ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition: OrderLine PickLineQuantity should total 18.", 18m, order.Lines.Sum(line => ((WhsOrderLine)line).PickLineQuantity));

			// create Trolley
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			// create Tote (for Order)
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);

			// create Slot and Assign Tote
			helper.CreateWhsPickTrolleySlot(trolleyJob, packageTote, 5);

			pickLines.ForEach(l => packingHelper.CreatePackageDivot(packageTote, l));
			webService.Factory.Save();

			var releaseLines = order.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).ToArray();
			var pickLineFor4 = pickLines.Single(l => l.WZ_Units == 4);
			var releaseCapturedInfoFor4 = (IPackableItem)pickLineFor4;
			var pickLineFor6 = pickLines.Single(l => l.WZ_Units == 6);
			var releaseCapturedInfoFor6 = (IPackableItem)pickLineFor6;
			var pickLineForPart2 = pickLines.Single(l => l.WZ_Units == 8);
			AssertContainsExactElementsInAnyOrder(new[] { releaseCapturedInfoFor4, releaseCapturedInfoFor6 }, ((IPackableItemParent)releaseLines[0]).PackableItems);
			AssertContainsExactElementsInAnyOrder(new[] { pickLineForPart2 }, ((IPackableItemParent)releaseLines[1]).PackableItems);
			AssertEquals("The Tote should contain releaseCapturedInfoFor4.", true, packageTote.IsPacked(releaseCapturedInfoFor4));
			AssertEquals("The Tote should contain releaseCapturedInfoFor6.", true, packageTote.IsPacked(releaseCapturedInfoFor6));
			AssertEquals("The Tote should contain releaseCapturedInfoForPart2.", true, packageTote.IsPacked(pickLineForPart2));

			// assign 4 Units to BOB and Pick
			var now = ZDateTimeOffset.Now;
			pickLineFor4.WZ_GS_NKAssignedTo = "BOB";
			pickLineFor4.WZ_PickedDateTime = now;
			webService.Factory.Save();

			var dockDoorTransfer = pick.Transfers.Single();
			AssertEquals("PreCondition, dock door transfer was created.", true, dockDoorTransfer != null);
			var clonedPickLine = dockDoorTransfer.Lines[0].PickLines[0];
			AssertEquals("PreCondition, pickline for transfer was created.", true, clonedPickLine != null);
			AssertEquals("PreCondition, cloned pick line is picked.", true, clonedPickLine.IsPicked);

			// mark Tote as Full
			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			AssertEquals("Response should not be Error.", ErrorTypes.None, response.Error);
			AssertEquals("Response should have no Error Message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("The Tote should contain PickLineFor4.", true, packageTote.IsPacked(releaseCapturedInfoFor4));
			AssertEquals("The Tote should not contain PickLineFor6.", false, packageTote.IsPacked(releaseCapturedInfoFor6));
			AssertEquals("The Tote should not contain releaseCapturedInfoForPart2.", false, packageTote.IsPacked(pickLineForPart2));
		}

		public void TestToteOnTrolleyIsFull_PickByBOM_SplitPackedKitPickLine()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			var frame = helper.CreateProduct(data.Org1, "FRAME");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 8m, location);
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 12m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 4m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 6m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			helper.Factory.Save();
			var kitOrder = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = helper.CreatePickNew(kitOrder);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine1 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 8m);
			var wheelPickLine2 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 12m);
			var framePickLine1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 4m);
			var framePickLine2 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 6m);

			packingHelper.SetRefPackTypeUOM(PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			kitPickLine1.WZ_F3_NKAllocatedPackType = PkgUnit.Unit;

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var tote1 = packingHelper.CreatePackage(pkgJob, "Tote1", 1, PkgUnit.Box);
			tote1.SetIsTote(true);
			packingHelper.CreatePackageDivot(tote1, kitPickLine1);

			helper.CreateWhsPickTrolleySlot(trolleyJob, tote1, 1);

			wheelPickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			wheelPickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			wheelPickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			framePickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			framePickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			webService.Factory.Save();

			AssertEquals("Precondition: the Tote should contain 1 divot.", 1, tote1.PackedItemDivots.Count);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", kitPickLine1.PK, tote1.PackedItemDivots[0].KI_ParentID);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", 10m, tote1.PackedItemDivots[0].KI_PackedQty);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			CombineAssertions(() =>
			{
				AssertEquals("Response should not be Error.", ErrorTypes.None, response.Error);
				AssertEquals("Response should have no Error Message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var newFactory = new BusinessObjectFactory();
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			tote1 = newFactory.Load<PkgPackage>(tote1.PK);
			AssertEquals("Kit Pick Line should be split after 'Tote is full'.", 2, kitOrderLine1.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines.Single(pl => pl.WZ_Units == 4m);
			var kitPickLine2 = kitOrderLine1.PickLines.Single(pl => pl.WZ_Units == 6m);
			AssertEquals("Kit Pick Line only get marked as picked after putaway.", false, kitPickLine1.IsPicked);
			AssertEquals(false, kitPickLine2.IsPicked);
			AssertEquals("The Tote should contain 1 divot.", 1, tote1.PackedItemDivots.Count);
			AssertEquals("The Tote should contain kitPickLine1.", kitPickLine1.PK, tote1.PackedItemDivots[0].KI_ParentID);
			AssertEquals("The Tote should contain kitPickLine1.", 4m, tote1.PackedItemDivots[0].KI_PackedQty);

			AssertEquals("Picked.", true, wheelPickLine1.IsPickedFromPutawayLocation && wheelPickLine1.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Picked.", true, framePickLine1.IsPickedFromPutawayLocation && framePickLine1.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Not Picked and unassigned.", true, !wheelPickLine2.IsPickedFromPutawayLocation && wheelPickLine2.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Not Picked and unassigned.", true, !framePickLine2.IsPickedFromPutawayLocation && framePickLine2.WZ_GS_NKAssignedTo.IsEmpty);
		}

		public void TestToteOnTrolleyIsFull_PickByBOM_RemovePackedKitPickLine()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			var frame = helper.CreateProduct(data.Org1, "FRAME");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 6m, location);
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 14m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 3m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 7m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			helper.Factory.Save();
			var kitOrder = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = helper.CreatePickNew(kitOrder);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var kitPickLine2 = kitPickLine1.Split(4m);
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine1 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 6m);
			var wheelPickLine2 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 14m);
			var framePickLine1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 3m);
			var framePickLine2 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 7m);

			packingHelper.SetRefPackTypeUOM(PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			kitPickLine1.WZ_F3_NKAllocatedPackType = PkgUnit.Unit;
			kitPickLine2.WZ_F3_NKAllocatedPackType = PkgUnit.Unit;

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var tote1 = packingHelper.CreatePackage(pkgJob, "Tote1", 1, PkgUnit.Box);
			tote1.SetIsTote(true);
			packingHelper.CreatePackageDivot(tote1, kitPickLine1);
			packingHelper.CreatePackageDivot(tote1, kitPickLine2);

			helper.CreateWhsPickTrolleySlot(trolleyJob, tote1, 1);

			wheelPickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			wheelPickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			wheelPickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			framePickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			framePickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			webService.Factory.Save();

			AssertEquals("Precondition: the Tote should contain 2 divots.", 2, tote1.PackedItemDivots.Count);
			var divot1 = tote1.PackedItemDivots.Single(di => di.KI_ParentID == kitPickLine1.PK);
			var divot2 = tote1.PackedItemDivots.Single(di => di.KI_ParentID == kitPickLine2.PK);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", 6m, divot1.KI_PackedQty);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", 4m, divot2.KI_PackedQty);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			CombineAssertions(() =>
			{
				AssertEquals("Response should not be Error.", ErrorTypes.None, response.Error);
				AssertEquals("Response should have no Error Message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var newFactory = new BusinessObjectFactory();
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			tote1 = newFactory.Load<PkgPackage>(tote1.PK);
			AssertEquals("3 Kit Pick Lines, 1 created from splitting.", 3, kitOrderLine1.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines.Single(pl => pl.WZ_Units == 3m);
			kitPickLine2 = kitOrderLine1.PickLines.Single(pl => pl.WZ_Units == 1m);
			var kitPickLine3 = kitOrderLine1.PickLines.Single(pl => pl.WZ_Units == 6m);
			AssertEquals(false, kitPickLine1.IsPicked);
			AssertEquals(false, kitPickLine2.IsPicked);
			AssertEquals(false, kitPickLine3.IsPicked);
			AssertEquals("The Tote should contain 1 divot.", 1, tote1.PackedItemDivots.Count);
			AssertEquals("The Tote should contain kitPickLine1.", kitPickLine1.PK, tote1.PackedItemDivots[0].KI_ParentID);
			AssertEquals("The Tote should contain kitPickLine1.", 3m, tote1.PackedItemDivots[0].KI_PackedQty);

			AssertEquals("Picked.", true, wheelPickLine1.IsPickedFromPutawayLocation && wheelPickLine1.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Picked.", true, framePickLine1.IsPickedFromPutawayLocation && framePickLine1.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Not Picked and unassigned.", true, !wheelPickLine2.IsPickedFromPutawayLocation && wheelPickLine2.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Not Picked and unassigned.", true, !framePickLine2.IsPickedFromPutawayLocation && framePickLine2.WZ_GS_NKAssignedTo.IsEmpty);
		}

		public void TestToteOnTrolleyIsFull_PickByBOM_MultiplePickedComponentPickLines()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			var frame = helper.CreateProduct(data.Org1, "FRAME");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 18m, location);
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 2m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 4m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 5m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 1m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			helper.Factory.Save();
			var kitOrder = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = helper.CreatePickNew(kitOrder);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine1 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 18m);
			var wheelPickLine2 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 2m);
			var framePickLine1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 4m);
			var framePickLine2 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 5m);
			var framePickLine3 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 1m);

			packingHelper.SetRefPackTypeUOM(PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			kitPickLine1.WZ_F3_NKAllocatedPackType = PkgUnit.Unit;

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var tote1 = packingHelper.CreatePackage(pkgJob, "Tote1", 1, PkgUnit.Box);
			tote1.SetIsTote(true);
			packingHelper.CreatePackageDivot(tote1, kitPickLine1);

			helper.CreateWhsPickTrolleySlot(trolleyJob, tote1, 1);

			wheelPickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			wheelPickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			wheelPickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			framePickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			framePickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			framePickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine3.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			webService.Factory.Save();

			AssertEquals("Precondition: the Tote should contain 1 divot.", 1, tote1.PackedItemDivots.Count);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", kitPickLine1.PK, tote1.PackedItemDivots[0].KI_ParentID);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", 10m, tote1.PackedItemDivots[0].KI_PackedQty);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			CombineAssertions(() =>
			{
				AssertEquals("Response should not be Error.", ErrorTypes.None, response.Error);
				AssertEquals("Response should have no Error Message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var newFactory = new BusinessObjectFactory();
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			tote1 = newFactory.Load<PkgPackage>(tote1.PK);
			AssertEquals("Kit Pick Line should be split after 'Tote is full'.", 2, kitOrderLine1.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines.Single(pl => pl.WZ_Units == 9m);
			var kitPickLine2 = kitOrderLine1.PickLines.Single(pl => pl.WZ_Units == 1m);
			AssertEquals("Kit Pick Line only get marked as picked after putaway.", false, kitPickLine1.IsPicked);
			AssertEquals(false, kitPickLine2.IsPicked);
			AssertEquals("The Tote should contain 1 divot.", 1, tote1.PackedItemDivots.Count);
			AssertEquals("The Tote should contain kitPickLine1.", kitPickLine1.PK, tote1.PackedItemDivots[0].KI_ParentID);
			AssertEquals("The Tote should contain kitPickLine1.", 9m, tote1.PackedItemDivots[0].KI_PackedQty);

			AssertEquals("Picked.", true, wheelPickLine1.IsPickedFromPutawayLocation && wheelPickLine1.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Picked.", true, framePickLine1.IsPickedFromPutawayLocation && framePickLine1.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Picked.", true, framePickLine2.IsPickedFromPutawayLocation && framePickLine2.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Not Picked and unassigned.", true, !wheelPickLine2.IsPickedFromPutawayLocation && wheelPickLine2.WZ_GS_NKAssignedTo.IsEmpty);
			AssertEquals("Not Picked and unassigned.", true, !framePickLine3.IsPickedFromPutawayLocation && framePickLine3.WZ_GS_NKAssignedTo.IsEmpty);
		}

		public void TestToteOnTrolleyIsFull_PickByBOM_NothingPicked()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			var frame = helper.CreateProduct(data.Org1, "FRAME");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			helper.Factory.Save();
			var kitOrder = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = helper.CreatePickNew(kitOrder);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 20m);
			var framePickLine = frameOrderLine.PickLines.Single(l => l.WZ_Units == 10m);

			packingHelper.SetRefPackTypeUOM(PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			kitPickLine1.WZ_F3_NKAllocatedPackType = PkgUnit.Unit;

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var tote1 = packingHelper.CreatePackage(pkgJob, "Tote1", 1, PkgUnit.Box);
			tote1.SetIsTote(true);
			packingHelper.CreatePackageDivot(tote1, kitPickLine1);

			helper.CreateWhsPickTrolleySlot(trolleyJob, tote1, 1);

			wheelPickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			framePickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			webService.Factory.Save();

			AssertEquals("Precondition: the Tote should contain 1 divot.", 1, tote1.PackedItemDivots.Count);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", kitPickLine1.PK, tote1.PackedItemDivots[0].KI_ParentID);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", 10m, tote1.PackedItemDivots[0].KI_PackedQty);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			CombineAssertions(() =>
			{
				AssertEquals("Should Error as Tote is not on a Trolley.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Tote does not contain any items. Cannot set Tote as Full.", response.ErrorMessage);
			});
		}

		public void TestToteOnTrolleyIsFull_PickByBOM_CannotBuildKitsEvenlyFromPickedComponents()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			var frame = helper.CreateProduct(data.Org1, "FRAME");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 1m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 9m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			helper.Factory.Save();
			var kitOrder = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = helper.CreatePickNew(kitOrder);
			var kitPickLine1 = kitOrderLine1.PickLines.Single(l => l.WZ_Units == 10m);
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 20m);
			var framePickLine1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 1m);
			var framePickLine2 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 9m);

			packingHelper.SetRefPackTypeUOM(PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			kitPickLine1.WZ_F3_NKAllocatedPackType = PkgUnit.Unit;

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var tote1 = packingHelper.CreatePackage(pkgJob, "Tote1", 1, PkgUnit.Box);
			tote1.SetIsTote(true);
			packingHelper.CreatePackageDivot(tote1, kitPickLine1);

			helper.CreateWhsPickTrolleySlot(trolleyJob, tote1, 1);

			// pick 1 frame and 20 wheels, no bikes could be assembled on the fly.
			wheelPickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			framePickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			webService.Factory.Save();

			AssertEquals("Precondition: the Tote should contain 1 divot.", 1, tote1.PackedItemDivots.Count);
			var divot1 = tote1.PackedItemDivots.Single(di => di.KI_ParentID == kitPickLine1.PK);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", 10m, divot1.KI_PackedQty);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			CombineAssertions(() =>
			{
				AssertEquals("Should Error as picked qty of Component are not matching Kits.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Tote contains Components that are insufficient to assemble a Kit. Cannot set Tote as Full.", response.ErrorMessage);
			});
		}

		public void TestToteOnTrolleyIsFull_PickByBOM_CannotBuildKitsEvenlyFromPickedComponents_MixedWithExistingKits()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			var frame = helper.CreateProduct(data.Org1, "FRAME");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			helper.CreateWhsReceiveInventoryLine(receive, bike, 1m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			helper.Factory.Save();
			var kitOrder = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 11m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = helper.CreatePickNew(kitOrder);
			var kitPickLine1 = kitOrderLine1.PickLines.Single(l => l.WZ_Units == 10m);
			var bikePickLine = kitOrderLine1.PickLines.Single(l => l.WZ_Units == 1m);
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 20m);
			var framePickLine = frameOrderLine.PickLines.Single(l => l.WZ_Units == 10m);

			packingHelper.SetRefPackTypeUOM(PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			kitPickLine1.WZ_F3_NKAllocatedPackType = PkgUnit.Unit;
			bikePickLine.WZ_F3_NKAllocatedPackType = PkgUnit.Unit;

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var tote1 = packingHelper.CreatePackage(pkgJob, "Tote1", 1, PkgUnit.Box);
			tote1.SetIsTote(true);
			packingHelper.CreatePackageDivot(tote1, kitPickLine1);
			packingHelper.CreatePackageDivot(tote1, bikePickLine);

			helper.CreateWhsPickTrolleySlot(trolleyJob, tote1, 1);

			// pick 1 existing bike and 20 wheels, no bikes could be assembled on the fly.
			wheelPickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			bikePickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			bikePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			webService.Factory.Save();

			AssertEquals("Precondition: the Tote should contain 1 divot.", 2, tote1.PackedItemDivots.Count);
			var divot1 = tote1.PackedItemDivots.Single(di => di.KI_ParentID == kitPickLine1.PK);
			AssertEquals("Precondition: the Tote should contain kitPickLine1.", 10m, divot1.KI_PackedQty);
			var divot2 = tote1.PackedItemDivots.Single(di => di.KI_ParentID == bikePickLine.PK);
			AssertEquals("Precondition: the Tote should contain bikePickLine.", 1m, divot2.KI_PackedQty);

			var response = webService.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), "Tote1");
			CombineAssertions(() =>
			{
				AssertEquals("Should Error as picked qty of Component are not matching Kits.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Tote contains Components that are insufficient to assemble a Kit. Cannot set Tote as Full.", response.ErrorMessage);
			});
		}
	}
}

