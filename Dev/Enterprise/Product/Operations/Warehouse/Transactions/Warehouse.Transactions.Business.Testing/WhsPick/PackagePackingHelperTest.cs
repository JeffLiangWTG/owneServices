using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PackagePackingHelperTest : WhsTestCaseWithFactory
	{
		#region TestGetPickLineFromPackage

		public void TestGetPickLineFromPackage()
		{
			#region Setup Data

			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			#region locations / areas setup

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var loc3 = data.Whs1.FindLocation("A-3");
			var loc4 = data.Whs1.FindLocation("A-4");
			var loc5 = data.Whs1.FindLocation("A-5");

			var areaA = Helper.CreateArea(data.Whs1, "Area - A");
			var areaB = Helper.CreateArea(data.Whs1, "Area - B");
			loc1.WLV_WA_PickingArea = areaA.PK;
			loc2.WLV_WA_PickingArea = areaA.PK;
			loc3.WLV_WA_PickingArea = areaA.PK;
			loc4.WLV_WA_PickingArea = areaB.PK;
			loc5.WLV_WA_PickingArea = areaB.PK;

			Factory.Save();
			loc2.WLV_PickPathSequence = 1;
			loc1.WLV_PickPathSequence = 2;
			loc3.WLV_PickPathSequence = 3;

			loc5.WLV_PickPathSequence = 4;
			loc4.WLV_PickPathSequence = 5;

			#endregion

			// so the area and location setup looks like this (increasing sequence from left to right):
			// Area - A: loc2 -> loc1 -> loc3
			// Area - B: loc5 -> loc4
			// looks confusing, but that is the way to ensure that in-memory order of objects has no effect on real sorting

			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var invAreaALoc1pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc1);
			var invAreaALoc2pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc2);
			var invAreaALoc3pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc3);
			var invAreaBLoc4pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc4);
			var invAreaBLoc5pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc5);

			var invAreaALoc1case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc1);
			var invAreaALoc2case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc2);
			var invAreaALoc3case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc3);
			var invAreaBLoc4case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc4);
			var invAreaBLoc5case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc5);

			var invAreaALoc1splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc1);
			var invAreaALoc2splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc2);
			var invAreaALoc3splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc3);
			var invAreaBLoc4splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc4);
			var invAreaBLoc5splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc5);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			#endregion

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 130);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			var allPickPackages = pick.OuterPackages;
			AssertEquals("There should be 10 label packages: 5 for pallets and 5 for cases.", 10, allPickPackages.Count);
			var packagePalletALoc1 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc1pallet.CommittedPickLines.Single().PK);
			var packagePalletALoc2 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc2pallet.CommittedPickLines.Single().PK);
			var packagePalletALoc3 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc3pallet.CommittedPickLines.Single().PK);
			var packagePalletBLoc4 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc4pallet.CommittedPickLines.Single().PK);
			var packagePalletBLoc5 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc5pallet.CommittedPickLines.Single().PK);
			var packageCaseALoc1 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc1case.CommittedPickLines.Single().PK);
			var packageCaseALoc2 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc2case.CommittedPickLines.Single().PK);
			var packageCaseALoc3 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc3case.CommittedPickLines.Single().PK);
			var packageCaseBLoc4 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc4case.CommittedPickLines.Single().PK);
			var packageCaseBLoc5 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc5case.CommittedPickLines.Single().PK);

			var packingHelper = new PackingTestHelper(Factory);
			// As I don't want to rely on cartonisation algorithm to create packages for test, I will create them manually
			var packAreaALoc1 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(packAreaALoc1, invAreaALoc1splitCase.CommittedPickLines.Single());

			var packAreaALoc2AreaBLoc4 = packingHelper.CreatePackage(order.PackageJob, 2, "UNT"); // this package with 2 picklines
			packingHelper.CreatePackageDivot(packAreaALoc2AreaBLoc4, invAreaALoc2splitCase.CommittedPickLines.Single()); // this pickline has lower sort order and should be used for package sorting
			packingHelper.CreatePackageDivot(packAreaALoc2AreaBLoc4, invAreaBLoc4splitCase.CommittedPickLines.Single());

			var packAreaALoc3 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(packAreaALoc3, invAreaALoc3splitCase.CommittedPickLines.Single());

			var packAreaBLoc5 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(packAreaBLoc5, invAreaBLoc5splitCase.CommittedPickLines.Single());

			Factory.Save();

			AssertEquals(invAreaALoc1splitCase.CommittedPickLines.Single().PK, new PackagePackingHelper(packAreaALoc1).GetPickLineFromPackage().PK);
			AssertEquals(invAreaALoc2splitCase.CommittedPickLines.Single().PK, new PackagePackingHelper(packAreaALoc2AreaBLoc4).GetPickLineFromPackage().PK);
			AssertEquals(invAreaALoc3splitCase.CommittedPickLines.Single().PK, new PackagePackingHelper(packAreaALoc3).GetPickLineFromPackage().PK);
			AssertEquals(invAreaBLoc5splitCase.CommittedPickLines.Single().PK, new PackagePackingHelper(packAreaBLoc5).GetPickLineFromPackage().PK);
		}

		public void TestTestGetPickLineFromPackage_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locA2, "");

			Factory.Save();
			locA1.WLV_PickPathSequence = 2;
			locA2.WLV_PickPathSequence = 1;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA1);
			var pickLineA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA2);

			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLineA1);
			packingHelper.CreatePackageDivot(package, pickLineA2);

			Helper.PickAndMakeInTransitTransfer(pickLineA1, ZDateTimeOffset.Now);
			Helper.PickAndMakeInTransitTransfer(pickLineA2, ZDateTimeOffset.Now);
			Factory.Save();

			var newPickLineA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLineForAvailableInventory.Location == locA1);
			var newPickLineA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLineForAvailableInventory.Location == locA2);

			AssertEquals("Should get the Pick Line with the lowest Pick Path Sequence.", newPickLineA2, new PackagePackingHelper(package).GetPickLineFromPackage());
		}

		public void TestTestGetPickLineFromPackage_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 100m, location1);
			Helper.CreateWhsReceiveInventoryLine(receive, bike, 20m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 30m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();
			AssertEquals("Should be allocated.", true, orderLine1.PickLines.Count == 2);

			var pickLine1 = orderLine1.PickLines.Single(l => l.WZ_Units == 20m);
			var pickLine2 = orderLine1.PickLines.Single(l => l.WZ_Units == 10m);

			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);

			AssertEquals("Precondition", location2.PK, pickLine1.InventoryLine.WE_WL);
			AssertEquals("Precondition", ZGuid.Empty, pickLine2.InventoryLine.WE_WL);
			AssertEquals("The Pick By BOM Inventory Line doesn't have a Location yet, the value is defaulted to 0.", pickLine2, new PackagePackingHelper(package).GetPickLineFromPackage());
		}

		#endregion

		#region TestGetDocketIDFromPackage

		public void TestGetDocketIDFromPackage()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(true);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(package, pick.GetAllPickLines().First());

			AssertEquals("W00000002", new PackagePackingHelper(package).GetDocketIDFromPackage());
			AssertEquals("", new PackagePackingHelper(Factory.New<PkgPackage>()).GetDocketIDFromPackage());
		}

		#endregion

		#region TestGetPickAreaFromPackage

		public void TestGetPackagePickArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var loc1 = data.Whs1.FindLocation("A-1");
			var areaA = Helper.CreateArea(data.Whs1, "Area - A");
			loc1.WLV_WA_PickingArea = areaA.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2, loc1, "");
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1);
			var pick1 = Helper.CreatePickNew(order1);
			var pick1Line = pick1.GetAllPickLines().Single(pl => pl.InventoryLine.Location == loc1);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pick1Line);
			Factory.Save();

			AssertEquals("Pick area shown for package.", "Area - A", new PackagePackingHelper(package1).GetPackagePickArea());
		}

		public void TestGetPackagePickAreaSplitPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingHelper = new PackingTestHelper(Factory);

			var loc1 = data.Whs1.FindLocation("A-1");
			var areaA = Helper.CreateArea(data.Whs1, "Area - A");
			loc1.WLV_WA_PickingArea = areaA.PK;
			loc1.WLV_PickPathSequence = 2;
			var loc2 = data.Whs1.FindLocation("A-2");
			var areaB = Helper.CreateArea(data.Whs1, "Area - B");
			loc2.WLV_WA_PickingArea = areaB.PK;
			loc2.WLV_PickPathSequence = 1;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2, loc1, ""); // 2 in loc 1
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2, loc2, ""); // 3 in loc 2
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 4);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == loc1);
			var pickLine2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == loc2);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Factory.Save();

			AssertEquals("First pick area shown for split case package.", "Area - B", new PackagePackingHelper(package).GetPackagePickArea());
		}

		public void TestGetPackagePickAreaAfterPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var loc1 = data.Whs1.FindLocation("A-1");
			var areaA = Helper.CreateArea(data.Whs1, "Area - A");
			loc1.WLV_WA_PickingArea = areaA.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2, loc1, "");
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1);
			var pick1 = Helper.CreatePickNew(order1);
			var pick1Line = pick1.GetAllPickLines().Single(pl => pl.InventoryLine.Location == loc1);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pick1Line);
			Factory.Save();
			pick1Line.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Pick area shown for package.", "Area - A", new PackagePackingHelper(package1).GetPackagePickArea());
		}

		#endregion

		#region TestGetPackagePickAreaNopickLines

		public void TestGetPackagePickAreaNopickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1);
			var pick = Helper.CreatePickNew(order);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			Factory.Save();

			AssertEquals("Empty string for pick with no lines.", string.Empty, new PackagePackingHelper(package).GetPackagePickArea());
		}

		#endregion

		#region TestGetPackagePickAreaNullPackage

		public void TestGetPackagePickAreaNullPackage()
		{
			AssertExceptionThrown<ArgumentNullException>("Throw argument exception when package is null", () => new PackagePackingHelper(null).GetPackagePickArea());
		}

		#endregion

		#region TestGetPackageLabelStatus

		public void TestGetPackageLabelStatus_SplitCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			#region locations / areas setup

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var loc3 = data.Whs1.FindLocation("A-3");
			var loc4 = data.Whs1.FindLocation("A-4");
			var loc5 = data.Whs1.FindLocation("A-5");

			var areaA = Helper.CreateArea(data.Whs1, "Area - A");
			var areaB = Helper.CreateArea(data.Whs1, "Area - B");
			loc1.WLV_WA_PickingArea = areaA.PK;
			loc2.WLV_WA_PickingArea = areaA.PK;
			loc3.WLV_WA_PickingArea = areaA.PK;
			loc4.WLV_WA_PickingArea = areaB.PK;
			loc5.WLV_WA_PickingArea = areaB.PK;

			Factory.Save();
			loc2.WLV_PickPathSequence = 1;
			loc1.WLV_PickPathSequence = 2;
			loc3.WLV_PickPathSequence = 3;

			loc5.WLV_PickPathSequence = 4;
			loc4.WLV_PickPathSequence = 5;

			#endregion

			// so the area and location setup looks like this (increasing sequence from left to right):
			// Area - A: loc2 -> loc1 -> loc3
			// Area - B: loc5 -> loc4
			// looks confusing, but that is the way to ensure that in-memory order of objects has no effect on real sorting

			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var invAreaALoc1pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc1);
			var invAreaALoc2pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc2);
			var invAreaALoc3pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc3);
			var invAreaBLoc4pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc4);
			var invAreaBLoc5pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc5);

			var invAreaALoc1case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc1);
			var invAreaALoc2case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc2);
			var invAreaALoc3case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc3);
			var invAreaBLoc4case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc4);
			var invAreaBLoc5case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc5);

			var invAreaALoc1splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc1);
			var invAreaALoc2splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc2);
			var invAreaALoc3splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc3);
			var invAreaBLoc4splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc4);
			var invAreaBLoc5splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc5);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 130);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			var allPickPackages = pick.OuterPackages;
			AssertEquals("There should be 10 label packages: 5 for pallets and 5 for cases.", 10, allPickPackages.Count);

			var packagePalletALoc1 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc1pallet.CommittedPickLines.Single().PK);
			var packagePalletALoc2 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc2pallet.CommittedPickLines.Single().PK);
			var packagePalletALoc3 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc3pallet.CommittedPickLines.Single().PK);
			var packagePalletBLoc4 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc4pallet.CommittedPickLines.Single().PK);
			var packagePalletBLoc5 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc5pallet.CommittedPickLines.Single().PK);
			var packageCaseALoc1 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc1case.CommittedPickLines.Single().PK);
			var packageCaseALoc2 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc2case.CommittedPickLines.Single().PK);
			var packageCaseALoc3 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc3case.CommittedPickLines.Single().PK);
			var packageCaseBLoc4 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc4case.CommittedPickLines.Single().PK);
			var packageCaseBLoc5 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc5case.CommittedPickLines.Single().PK);

			var packingHelper = new PackingTestHelper(Factory);

			// Package: packAreaALoc1
			var packAreaALoc1 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			var pickLineForInventoryAreaALoc1splitCase = invAreaALoc1splitCase.CommittedPickLines.Single();
			packingHelper.CreatePackageDivot(packAreaALoc1, pickLineForInventoryAreaALoc1splitCase);

			// Package: packAreaALoc2AreaBLoc4
			var packAreaALoc2AreaBLoc4 = packingHelper.CreatePackage(order.PackageJob, 2, "UNT"); // this package with 2 picklines
			var pickLineForInventoryAreaALoc2splitCase = invAreaALoc2splitCase.CommittedPickLines.Single();
			var pickLineForInventoryAreaBLoc4splitCase = invAreaBLoc4splitCase.CommittedPickLines.Single();
			packingHelper.CreatePackageDivot(packAreaALoc2AreaBLoc4, pickLineForInventoryAreaALoc2splitCase);
			packingHelper.CreatePackageDivot(packAreaALoc2AreaBLoc4, pickLineForInventoryAreaBLoc4splitCase);

			Factory.Save();

			var packagePackingHelperLoc1 = new PackagePackingHelper(packAreaALoc1);
			var packagePackingHelperLoc2AreaBLoc4 = new PackagePackingHelper(packAreaALoc2AreaBLoc4);
			AssertEquals("Initial status should be Not Printed.", "Not Printed", packagePackingHelperLoc1.GetPackageLabelStatus());
			AssertEquals("Initial status should be Not Printed.", "Not Printed", packagePackingHelperLoc2AreaBLoc4.GetPackageLabelStatus());

			// packAreaALoc1
			PrintPackage(packAreaALoc1);
			AssertEquals("Printed.", "Printed", packagePackingHelperLoc1.GetPackageLabelStatus());

			AssignPackagePicker(pickLineForInventoryAreaALoc1splitCase, "JSL");
			AssertEquals("In Picking.", "In Picking", packagePackingHelperLoc1.GetPackageLabelStatus());

			PickPickLine(pickLineForInventoryAreaALoc1splitCase);
			AssertEquals("Picked.", "Picked", packagePackingHelperLoc1.GetPackageLabelStatus());

			ClosePackage(packAreaALoc1);
			AssertEquals("Packed.", "Packed", packagePackingHelperLoc1.GetPackageLabelStatus());

			//packAreaALoc2AreaBLoc4
			PrintPackage(packAreaALoc2AreaBLoc4);
			AssertEquals("Printed.", "Printed", packagePackingHelperLoc2AreaBLoc4.GetPackageLabelStatus());

			AssignPackagePicker(pickLineForInventoryAreaALoc2splitCase, "MXF"); // one pick line is assigned
			AssertEquals("In Picking.", "In Picking", packagePackingHelperLoc2AreaBLoc4.GetPackageLabelStatus());

			PickPickLine(pickLineForInventoryAreaALoc2splitCase);   // one pick line is picked, but still in picking
			AssertEquals("In Picking.", "In Picking", packagePackingHelperLoc2AreaBLoc4.GetPackageLabelStatus());

			PickPickLine(pickLineForInventoryAreaBLoc4splitCase);   // all pick lines are picked
			AssertEquals("Picked.", "Picked", packagePackingHelperLoc2AreaBLoc4.GetPackageLabelStatus());

			// printing another labels, doesn't overwrite status back to Printed
			PrintPackage(packAreaALoc2AreaBLoc4);
			AssertEquals("Picked.", "Picked", packagePackingHelperLoc2AreaBLoc4.GetPackageLabelStatus());

			ClosePackage(packAreaALoc2AreaBLoc4);
			AssertEquals("Packed.", "Packed", packagePackingHelperLoc2AreaBLoc4.GetPackageLabelStatus());
		}

		public void TestGetPackageLabelStatus_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locA2, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA1);
			var pickLineA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA2);

			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLineA1);
			packingHelper.CreatePackageDivot(package, pickLineA2);

			Helper.PickAndMakeInTransitTransfer(pickLineA1, ZDateTimeOffset.Now);
			Helper.PickAndMakeInTransitTransfer(pickLineA2, ZDateTimeOffset.Now);
			Factory.Save();

			AssertEquals("Status should be picked.", "Picked", new PackagePackingHelper(package).GetPackageLabelStatus());
		}

		#region class MockDocumentEventSource

		class MockDocumentEventSource : IDocumentEvents
		{
			public event DocumentCancelEventHandler DocumentPrintRequested;
			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public event DocumentPrintedEventHandler DocumentPrinted;

			public void FireDocumentPrintRequested(DocumentCancelEventArgs e)
			{
				DocumentPrintRequested(this, e);
			}

			public void FireDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed(this, e);
			}

			public void FireDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrePrinted(this, e);
			}

			public void FireDocumentPrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrinted(this, e);
			}
		}

		public void TestGetPackageLabelStatus_ReadyToPack_PackingStation()
		{
			TestGetPackageLabelStatus_ReadyToPackCore(LocationClasses.Codes.PST);
		}

		public void TestGetPackageLabelStatus_ReadyToPack_PackingConsolidationLocation()
		{
			TestGetPackageLabelStatus_ReadyToPackCore(LocationClasses.Codes.CON);
		}

		void TestGetPackageLabelStatus_ReadyToPackCore(string locationClass)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, locationClass);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			var pickLine = pick.GetAllPickLines().First();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			var packagePackingHelper = new PackagePackingHelper(package);

			pickLine.WZ_GS_NKAssignedTo = "ABC";
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			transferLine.FinaliseDocketLine();

			AssertEquals("Package Label Status is Ready To Pack", "Ready To Pack", packagePackingHelper.GetPackageLabelStatus());
		}

		public void TestGetPackageLabelStatus()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			var pickLine = pick.GetAllPickLines().First();
			packingHelper.CreatePackageDivot(package, pickLine);
			var load = CreateLoad(data.Whs1.DefaultOutboundDockDoorLocation);
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			var packagePackingHelper = new PackagePackingHelper(package);

			// Not Printed => Printed => In Picking => Picked => Packed => Staged => Loaded => Departed

			AssertEquals("Not Printed", packagePackingHelper.GetPackageLabelStatus());

			PrintPackage(package);
			AssertEquals("Not Printed => [Printed] => In Picking => Picked => Packed => Staged => Loaded => Departed", "Printed", packagePackingHelper.GetPackageLabelStatus());

			AssignPackagePicker(pickLine, "ABC");
			AssertEquals("... => Printed => [In Picking] => Picked => ...", "In Picking", packagePackingHelper.GetPackageLabelStatus());

			PickPickLine(pickLine);
			AssertEquals("... => In Picking => [Picked] => Packed => ...", "Picked", packagePackingHelper.GetPackageLabelStatus());

			ClosePackage(package);
			AssertEquals("... => Picked => [Packed] => Staged => ...", "Packed", packagePackingHelper.GetPackageLabelStatus());

			StageInventory(pickLine);
			AssertEquals("... => Packed => [Staged] => Loaded => ...", "Staged", packagePackingHelper.GetPackageLabelStatus());

			LoadPackage(loadPkgPackagePivot);
			AssertEquals(".... => Staged => [Loaded] => Departed", "Loaded", packagePackingHelper.GetPackageLabelStatus());

			Helper.DepartPackageNow(loadPkgPackagePivot);
			AssertEquals("... => Loaded => [Departed]", "Departed", packagePackingHelper.GetPackageLabelStatus());
		}

		public void TestGetPackageLabelStatusDBHits()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			var pickLine = pick.GetAllPickLines().Single();
			packingHelper.CreatePackageDivot(package, pickLine);
			var load = CreateLoad(data.Whs1.DefaultOutboundDockDoorLocation);
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			var expectedDbHitsNoPrint = new Dictionary<string, int>()
			{
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLoadSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};
			AssertDbHits("No action", "Not Printed", expectedDbHitsNoPrint);

			var expectedDbHitsPrinted = new Dictionary<string, int>()
			{
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsLoadSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};
			PrintPackage(package);
			AssertDbHits("Not Printed => [Printed] => In Picking => Picked => Packed => Staged => Loaded => Departed", "Printed", expectedDbHitsPrinted);

			var expectedDbHitsInPicking = new Dictionary<string, int>()
			{
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLoadSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};
			AssignPackagePicker(pickLine, staff.GS_Code);
			AssertDbHits("... => Printed => [In Picking] => Picked => ...", "In Picking", expectedDbHitsInPicking);

			var expectedDbHitsPicked = new Dictionary<string, int>()
			{
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLoadSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};
			PickPickLine(pickLine);
			AssertDbHits("... => In Picking => [Picked] => Packed => ...", "Picked", expectedDbHitsPicked);

			var expectedDbHitsPacked = new Dictionary<string, int>()
			{
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLoadSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};
			ClosePackage(package);
			AssertDbHits("... => Picked => [Packed] => Staged => ...", "Packed", expectedDbHitsPacked);

			var expectedDbHitsStaged = new Dictionary<string, int>()
			{
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLoadSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};
			StageInventory(pickLine);
			AssertDbHits("... => Packed => [Staged] => Loaded => ...", "Staged", expectedDbHitsStaged);

			var expectedDbHitsLoaded = new Dictionary<string, int>()
			{
				{ WhsLoadSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
			};
			LoadPackage(loadPkgPackagePivot);
			AssertDbHits(".... => Staged => [Loaded] => Departed", "Loaded", expectedDbHitsLoaded);

			var expectedDbHitsDeparted = new Dictionary<string, int>()
			{
				{ WhsLoadSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
			};
			Helper.DepartPackageNow(loadPkgPackagePivot);
			AssertDbHits("... => Loaded => [Departed]", "Departed", expectedDbHitsDeparted);

			void AssertDbHits(string msg, string expectedStatus, Dictionary<string, int> expectedDbHits)
			{
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				var pkg = newFactory.Load<PkgPackage>(package.PK);
				var packagePackingHelper = new PackagePackingHelper(pkg);
				newFactory.ResetDatabaseLoadCount();
				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
				{
					AssertEquals(msg, expectedStatus, packagePackingHelper.GetPackageLabelStatus());
				}
			}
		}

		void PrintPackage(PkgPackage package)
		{
			var supporter = new PkgPackageDocumentSupporter(package);
			var eventSource = new MockDocumentEventSource();
			supporter.Initialise(eventSource);
			var queryDeliveryLabel = new DocumentZQuery(BusinessContext.Packing, "Delivery Label (Top Level)");
			var deliveryLabelCommand = package.Factory.LoadTop1<DocumentCommand>(queryDeliveryLabel);
			var argsDeliveryLabel = new DocumentPrintedEventArgs(DeliveryInstructionDestination.None, deliveryLabelCommand);
			eventSource.FireDocumentPrinted(argsDeliveryLabel);
		}

		static void AssignPackagePicker(WhsPickLine pickLine, ZString assignedTo) => pickLine.WZ_GS_NKAssignedTo = assignedTo;

		static void PickPickLine(WhsPickLine pickLine) => pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

		static void ClosePackage(PkgPackage package) => package.KP_ClosedTimeUtc = ZDateTime.UtcNow;

		static void StageInventory(WhsPickLine pickLine)
		{
			if (pickLine.InventoryLine is WhsTransferLine transferLine)
			{
				transferLine.FinaliseDocketLine();
			}
			else
			{
				pickLine.Inventory.WI_InventoryStatus = InventoryStatus.Codes.Staged;
			}
			AssertEquals(InventoryStatus.Codes.Staged, pickLine.Inventory.WI_InventoryStatus);
		}

		static void LoadPackage(WhsLoadPkgPackagePivot loadPkgPackagePivot)
		{
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";
		}

		#endregion

		#endregion

		#region TestGetGetUserAssignedToPackage

		public void TestGetUserAssignedToPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "ABC";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLine);

			AssertEquals("Correct user code is returned.", "ABC", new PackagePackingHelper(package).GetUserAssignedToPackage());
		}

		public void TestGetUserAssignedToPackage_NoUserAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();

			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			AssertEquals("No user code is returned.", ZString.Empty, new PackagePackingHelper(package).GetUserAssignedToPackage());
		}

		public void TestGetUserAssignedToPackage_MultipleUsers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user1 = Helper.CreateGlbStaff("ABC", "ABC");
			var user2 = Helper.CreateGlbStaff("DEF", "DEF");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 6m);
			pickLine1.WZ_GS_NKAssignedTo = user1.GS_Code;
			var pickLine2 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 4m);
			pickLine2.WZ_GS_NKAssignedTo = user2.GS_Code;

			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Factory.Save();

			AssertEquals("'Multiple' is returned for packages with multiple users assigned to.", "Multiple", new PackagePackingHelper(package).GetUserAssignedToPackage());
		}

		public void TestGetUserAssignedToPackage_MultiplePickLinesSameUser()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user = Helper.CreateGlbStaff("ABC", "ABC");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 6m);
			pickLine1.WZ_GS_NKAssignedTo = user.GS_Code;
			var pickLine2 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 4m);
			pickLine2.WZ_GS_NKAssignedTo = user.GS_Code;

			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Factory.Save();

			AssertEquals("Correct user code is returned, user assigned to pick lines.", "ABC", new PackagePackingHelper(package).GetUserAssignedToPackage());
		}

		public void TestGetUserAssignedToPackage_OnePickLineWithNoUserAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user1 = Factory.New<GlbStaff>();
			user1.GS_Code = "ABC";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 6m);
			pickLine1.WZ_GS_NKAssignedTo = user1.GS_Code;
			var pickLine2 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 4m);

			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Factory.Save();

			AssertEquals("User code from pick line with assigned user is returned.", "ABC", new PackagePackingHelper(package).GetUserAssignedToPackage());
		}

		public void TestGetUserAssignedToPackage_UserOnOriginalPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "ABC";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package, pickLine);

			AssertEquals("PickLine has no user assigned to.", ZString.Empty, pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("User code is returned, pick line from created transfer.", "E", new PackagePackingHelper(package).GetUserAssignedToPackage());
		}

		public void TestGetUserAssignedToPackage_NoPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1);
			Helper.CreatePickNew(order);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			Factory.Save();

			AssertEquals("Empty string for package with no pick lines.", string.Empty, new PackagePackingHelper(package).GetUserAssignedToPackage());
		}

		public void TestGetUserAssignedToPackage_NullPackage()
		{
			AssertExceptionThrown<ArgumentNullException>("Throw argument exception when package is null", () => new PackagePackingHelper(null).GetUserAssignedToPackage());
		}

		#endregion

		#region TestAddFetchHintsWhenDeletingPackageJobFromPickableDockets

		public void TestAddFetchHintsWhenDeletingPackageJobFromPickableDockets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 10);
			for (int i = 0; i < 10; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);
				for (int j = 0; j < 10; j++)
				{
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
				}

				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
			}

			var pickableDockets = new List<WhsPickableDocket>();
			for (int i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, 1m);
				pickableDockets.Add(order);
			}

			var pick = Factory.New<WhsPick>();
			pick.AddOrders(pickableDockets);
			AssertContainsExactElementsInAnyOrder("Precondition: Should have attached orders.", pickableDockets, pick.Orders);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var ordersInOtherFactory = otherFactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, pickableDockets.Select(o => o.PK)));
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);

			var expectedDbHits = new Dictionary<string, int>()
					{
						{ WhsPickSchema.Constants.TableName, 1 },
						{ PkgPackageJobSchema.Constants.TableName, 1 },
						{ WhsDocketSchema.Constants.TableName, 2 },
						{ WhsDocketLineSchema.Constants.TableName, 1 },
						{ WhsPickLineSchema.Constants.TableName, 1 },
						{ WhsWarehouseSchema.Constants.TableName, 1 },
						{ JobDocumentDeliverySchema.Constants.TableName, 1 },
						{ JobDocumentExclusionSchema.Constants.TableName, 1 },
						{ OrgHeaderSchema.Constants.TableName, 1 },
						{ OrgMiscServSchema.Constants.TableName, 1 },
						{ OrgSupplierPartSchema.Constants.TableName, 1 },
						{ WhsBondedWarehouseAttributeSchema.Constants.TableName, 1 },
						{ PkgPackageSchema.Constants.TableName, 1 },
						{ PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 1 },
						{ StmNoteSchema.Constants.TableName, 2 },
						{ StmDocDataOverrideSchema.Constants.TableName, 2 },
						{ StmUniversalCopySchema.Constants.TableName, 1 },
					};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			{
				pickInOtherFactory.RemoveOrders(ordersInOtherFactory);
				AssertEquals("PickInOtherFactory should NO have attached orders.", 0, pickInOtherFactory.Orders.Count);
			}
		}

		#endregion

		#region CreateLoad

		WhsLoad CreateLoad(WhsLocation dockDoorLocation)
		{
			var transportCompany = Helper.CreateClient("TC1");
			var carrierServicelevel = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(transportCompany, dockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			load.WLO_PL_NKCarrierServiceLevel = carrierServicelevel.PL_Code;

			return load;
		}

		#endregion
	}
}
