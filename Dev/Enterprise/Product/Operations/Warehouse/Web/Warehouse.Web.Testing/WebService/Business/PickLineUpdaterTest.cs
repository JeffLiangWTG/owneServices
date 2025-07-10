using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.PickByLabel.Testing;
using Enterprise.Warehouse.Web.WebService;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using PickTrolleyStatus = Enterprise.Warehouse.Transactions.TrolleyPicking.PickTrolleyStatus;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PickLineUpdaterTest : WhsTestCaseWithFactory
	{
		#region TestConfirmPickLinePickedQty_PickByLabel

		#region TestConfirmPickLinePickedQty_PickByLabel_ShortPick

		public void TestConfirmPickLinePickedQty_PickByLabel_ShortPick()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pick.GetAllPickLines().Single(o => o.SupplierPart == data.Part1);

			var package = order.PackageJob.Packages.AddNew(Core.Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			packingHelper.CreatePackageDivot(package, pickLines[1]);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Helper.Factory.Save();

			ConfirmPickLinesPickedQty(new[] { pickLine }, new PickingInfo(1, false), staff);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_PickByLabel_RCA

		public void TestConfirmPickLinePickedQty_PickByLabel_RCA()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var releaseLine = order.Lines[0].ReleaseLines[0];

			var package1 = order.PackageJob.Packages.AddNew("PLT", "123");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "XYZ");
			package1.Pack(releaseLine, 4m);
			package2.Pack(releaseLine, 4m);
			Factory.Save();

			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package2.PK);
			Helper.Factory.Save();

			order.Lines[0].ClearReleaseLines();
			var releaseCapturedInfo = new WhsReleaseCapturedInfo { Attribute1 = "BLUE", Quantity = 3m };

			var pickLines = order.Lines[0].PickLines.ToArray();
			var pickLinePKs = pickLines.Select(p => p.PK.ToGuid()).ToArray();
			PickLineUpdater.ConfirmPickLinesPickedQty(pickLines, new PickingInfo(3m, false), staff,
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("", 3m, "", new[] { releaseCapturedInfo }) }) }, null, false);
			AssertNull("Divot package has been deleted and it should remove package label from list.", WhsPickByLabelHelperForTesting.GetPackageHasActivePickByLabel(package2));
		}

		#endregion

		#region TestConfirmPickLinePickedQty_PickByLabel_FullyShortPick

		public void TestConfirmPickLinePickedQty_PickByLabel_FullyShortPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(order.PackableItemParents.Typed.Single(), 5m);
			AssertEquals("Precondition: Package is packed.", 1, package.PackedItemDivots.Count);
			Factory.Save();
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, GlbStaff.CurrentUser.GS_Code, package.PK);
			Factory.Save();

			var pickLine = order.Lines[0].PickLines.Single();
			AssertEquals("Precondition- check pack expected pickline", pickLine.PK, pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().Package.GetPickLines().Single().PK);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(0m, false), GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				null,
				false);
			GlbStaff.CurrentUser.Factory.Save();
			AssertEquals("Not error expected", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Divot should be deleted is short whole pickLine.", 0, package.PackedItemDivots.Count);
			AssertNull("Label should be deleted for empty package.", pickByLabelJob.Labels.SingleOrDefault());
		}

		#endregion

		#region TestConfirmPickLinePickedQty_PickByLabel_PartiallyShortPick

		public void TestConfirmPickLinePickedQty_PickByLabel_PartiallyShortPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(order.PackableItemParents.Typed.Single(), 5m);
			AssertEquals("Precondition: Package is packed.", 1, package.PackedItemDivots.Count);
			Factory.Save();
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, GlbStaff.CurrentUser.GS_Code, package.PK);
			Factory.Save();

			var pickLine = order.Lines[0].PickLines.Single();
			AssertEquals("Precondition- check pack expected pickline", pickLine.PK, pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().Package.GetPickLines().Single().PK);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine }, new PickingInfo(2m, false), staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				null,
				false);
			AssertEquals("Not error expected", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Divot should **not** be deleted is partially pickLine.", 1, package.PackedItemDivots.Count);
			AssertEquals("Label should **not** be deleted package.", 2m, pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().Package.GetPickLines().Single().WZ_Units);
		}

		#endregion

		public void TestConfirmPickLinePickedQty_SerialNumberAttributeValidation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 2m);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part1, 2m);

			var pick1 = Helper.CreatePickNew(order1, order2);
			var pick2 = Helper.CreatePickNew(order3);
			Helper.Factory.Save();

			orderLine1.ReleaseLines[0].PartAttribute1 = "S1";
			orderLine1.ReleaseLines[0].Quantity = 1m;
			orderLine1.ReleaseLines.AddNew("", "", "", "S2", ZDate.Empty, ZDate.Empty, 1m);

			orderLine2.ReleaseLines[0].PartAttribute1 = "S3";
			orderLine2.ReleaseLines[0].Quantity = 1m;
			orderLine2.ReleaseLines.AddNew("", "", "", "S4", ZDate.Empty, ZDate.Empty, 1m);
			Helper.Factory.Save();

			var releaseCapturedInfos = new[]
			{
				new WhsReleaseCapturedInfo { Attribute1 = "S1", Quantity = 1 },
				new WhsReleaseCapturedInfo { Attribute1 = "S5", Quantity = 1 }
			};

			Globals.IsWeb = true;
			Globals.IsUserInteractive = false;

			var pickLines = pick2.GetAllPickLines().ToArray();
			var pickLinePKs = pickLines.Select(p => p.PK.ToGuid()).ToArray();
			Assert("Environment is RF.", WhsEnvironment.IsRF);
			AssertExceptionThrown<InvalidOperationException>("Confirm pick lines should thrown an exception.",
				"Invalid release line resulted from release captured attributes update.",
				() => PickLineUpdater.ConfirmPickLinesPickedQty(
					pickLines,
					new PickingInfo(2m, false),
					staff,
					new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("UNT", 1m, "", releaseCapturedInfos) }) },
					null,
					false));
		}

		#endregion

		#region TestConfirmPickLinePickedQty_ShortPickAll

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_ShortPickAll()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			Factory.Save();
			var inventoryLinePK = pickLine.WZ_WE_InventoryLine;

			ConfirmPickLinesPickedQty(new[] { pickLine }, new PickingInfo(0, false), staff);
			var inventoryLine = Helper.Factory.Load<WhsDocketLine>(inventoryLinePK);
			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);
			AssertWhsPickShortLineExists(inventoryLine.PK, order.Lines[0].PK, 2m, staff.GS_Code, ZDateTime.UtcNow);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_ShortPickPartial

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_ShortPickPartial()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			Factory.Save();
			var inventoryLinePK = pickLine.WZ_WE_InventoryLine;

			ConfirmPickLinesPickedQty(new[] { pickLine }, new PickingInfo(1, false), staff);

			AssertEquals("Should only be 1 short lines", 1, Factory.Load<WhsPickShortLine>(new ZQuery()).Length);

			var inventoryLine = Helper.Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, inventoryLinePK));
			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 1m, order.WD_UnitsSent);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);
			AssertWhsPickShortLineExists(receive.Lines[0].PK, order.Lines[0].PK, 1m, staff.GS_Code, ZDateTime.UtcNow);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_MultiplePickLineWithSameInventoryLine_ShortPickAll

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_MultiplePickLineWithSameInventoryLine_ShortPickAll()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part2, 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 5m);
			var pick = Helper.CreatePickNew(order1, order2, order3, order4);
			Factory.Save();

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff);

			var inventoryLine1 = Helper.Factory.Load<WhsDocketLine>(receive1.Lines[0].PK);
			var inventoryLine2 = Helper.Factory.Load<WhsDocketLine>(receive2.Lines[0].PK);
			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 0m, order1.WD_UnitsSent);
			AssertEquals("Order was shorted.", 0m, order2.WD_UnitsSent);
			AssertEquals("Order was shorted.", 0m, order3.WD_UnitsSent);
			AssertEquals("Order was shorted.", 0m, order4.WD_UnitsSent);

			AssertEquals("Should only be 4 short lines", 4, Factory.Load<WhsPickShortLine>(new ZQuery()).Length);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine1.WE_CurrentInventoryStatus);
			AssertWhsPickShortLineExists(inventoryLine1.PK, order1.Lines[0].PK, 5m, staff.GS_Code, ZDateTime.UtcNow);
			AssertWhsPickShortLineExists(inventoryLine1.PK, order2.Lines[0].PK, 5m, staff.GS_Code, ZDateTime.UtcNow);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine2.WE_CurrentInventoryStatus);
			AssertWhsPickShortLineExists(inventoryLine2.PK, order3.Lines[0].PK, 5m, staff.GS_Code, ZDateTime.UtcNow);
			AssertWhsPickShortLineExists(inventoryLine2.PK, order4.Lines[0].PK, 5m, staff.GS_Code, ZDateTime.UtcNow);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_ShortPickAllInventoryWithMatchingProduct

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_ShortPickAllInventoryWithMatchingProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, palletID: "PID1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, palletID: "PID1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, palletID: "");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, loc, palletID: "");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, loc, palletID: "");
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			var pickLineInventoryPK = pick.GetAllPickLines().Single().WZ_WE_InventoryLine;

			var staff = Helper.CreateGlbStaff("GS1", "GS1");

			Factory.Save();

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff);

			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 0m, order1.WD_UnitsSent);
			AssertEquals("Should only be 1 short lines", 1, Factory.Load<WhsPickShortLine>(new ZQuery()).Length);
			AssertWhsPickShortLineExists(pickLineInventoryPK, order1.Lines[0].PK, 5m, staff.GS_Code, ZDateTime.UtcNow);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine1.WE_CurrentInventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine2.WE_CurrentInventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine3.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine3.WE_CurrentInventoryStatus);

			AssertEquals("Inventory for different product was not shorted.", string.Empty, receiveLine4.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory for different product was not held.", InventoryStatus.Codes.Available, receiveLine4.WE_CurrentInventoryStatus);

			AssertEquals("Inventory for different product was not shorted.", string.Empty, receiveLine5.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory for different product was not held.", InventoryStatus.Codes.Available, receiveLine5.WE_CurrentInventoryStatus);
		}

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_ShortPickAllInventoryWithMatchingPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, palletID: "PID1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, palletID: "PID1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, loc, palletID: "PID1");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, palletID: "PID2");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, loc, palletID: "PID2");
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, loc, palletID: "PID2");
			var receiveLine7 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, loc, palletID: "PID3");
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			var allocatedInventoryLine = order1.Lines.Single().PickLines.Single().WZ_WE_InventoryLine;

			var staff = Helper.CreateGlbStaff("GS1", "GS1");

			Factory.Save();

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff);

			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 0m, order1.WD_UnitsSent);
			AssertEquals("Should only be 1 short lines", 1, Factory.Load<WhsPickShortLine>(new ZQuery()).Length);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine1.WE_CurrentInventoryStatus);
			AssertWhsPickShortLineExists(allocatedInventoryLine, order1.Lines[0].PK, 5m, staff.GS_Code, ZDateTime.UtcNow);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine2.WE_CurrentInventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine3.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine3.WE_CurrentInventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine4.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine4.WE_CurrentInventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine5.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine5.WE_CurrentInventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine6.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine6.WE_CurrentInventoryStatus);

			AssertEquals("Inventory for different product on different pallet was not shorted.", string.Empty, receiveLine7.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory for different product on different pallet was not held.", InventoryStatus.Codes.Available, receiveLine7.WE_CurrentInventoryStatus);
		}

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_ShortPickAllInventory_IgnoresOtherWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("W2", "B", 2, 1);
			Factory.Save();

			var loc = data.Whs1.FindLocation("A-1");
			var loc2 = whs2.FindLocation("B-1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, loc, palletID: "PID1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, loc, palletID: "PID1");

			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, loc2, palletID: "PID1");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, loc2, palletID: "PID1");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, loc2, palletID: "PID1");
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			var allocatedInventoryLine = order1.Lines.Single().PickLines.Single().WZ_WE_InventoryLine;

			var staff = Helper.CreateGlbStaff("GS1", "GS1");

			Factory.Save();

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff);

			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 0m, order1.WD_UnitsSent);
			AssertEquals("Should only be 1 short lines", 1, Factory.Load<WhsPickShortLine>(new ZQuery()).Length);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine1.WE_CurrentInventoryStatus);
			AssertWhsPickShortLineExists(allocatedInventoryLine, order1.Lines[0].PK, 5m, staff.GS_Code, ZDateTime.UtcNow);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine2.WE_CurrentInventoryStatus);

			AssertEquals("Inventory for different warehouse was not shorted.", string.Empty, receiveLine3.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory for different warehouse was not held.", InventoryStatus.Codes.Available, receiveLine3.WE_CurrentInventoryStatus);

			AssertEquals("Inventory for different warehouse was not shorted.", string.Empty, receiveLine4.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory for different warehouse was not held.", InventoryStatus.Codes.Available, receiveLine4.WE_CurrentInventoryStatus);

			AssertEquals("Inventory for different warehouse was not shorted.", string.Empty, receiveLine5.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory for different warehouse was not held.", InventoryStatus.Codes.Available, receiveLine5.WE_CurrentInventoryStatus);
		}

		#region TestConfirmPickLinePickedQty_ShortPickAllInventory_ChangeInventoryHeldCodeForShortedInventoryLines

		[TestDate(2024, 7, 24, 15, 12, 22)]
		public void TestConfirmPickLinePickedQty_ShortPickAllInventory_ChangeInventoryHeldCodeForShortedInventoryLines_EdgeCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, palletID: "");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, palletID: "");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, palletID: "");
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 7m); // 5 + 7 = 12 > 10
			var pick = Helper.CreatePickNew(order1);

			var staff = Helper.CreateGlbStaff("GS1", "GS1");

			Factory.Save();
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: there should be 3 pickLines", 3, pickLines.Count());
			var inventoryLine1 = pickLines.First(pl => pl.WZ_Units == 5m && pl.WZ_WE_TransactionLine != orderLine2.PK).InventoryLine;
			var inventoryLine2 = pickLines.First(pl => pl.WZ_Units == 5m && pl.WZ_WE_TransactionLine == orderLine2.PK).InventoryLine;
			var inventoryLine3 = pickLines.First(pl => pl.WZ_Units == 2m && pl.WZ_WE_TransactionLine == orderLine2.PK).InventoryLine;
			AssertEquals("Precondition: pickLine1 and pickLine2 should share the same inventory line", inventoryLine1.PK, inventoryLine2.PK);
			AssertNotEquals("Precondition: orderLine2 should use different inventory lines", inventoryLine2.PK, inventoryLine3.PK);

			AssertNoExceptionThrown(() => ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff)); // To fix error thrown in trigger - TG_WhsInventoryHoldChangeLog_LogVersionCheck

			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 0m, order1.WD_UnitsSent);
			AssertEquals("Should be 3 short lines", 3, Factory.Load<WhsPickShortLine>(new ZQuery()).Length);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine1.WE_CurrentInventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine2.WE_CurrentInventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine3.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine3.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_ShortPickAllInventory_IgnoresStagedInventory

		[TestDate(2024, 7, 15, 10, 12, 22)]
		public void TestConfirmPickLinePickedQty_ShortPickAllInventory_IgnoresStagedInventory_WithoutPalletID()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Factory.Save();
			var serviceAreaLoc = data.Whs1.FindLocation("SERVICEROW");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 10m, serviceAreaLoc, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 10m, serviceAreaLoc, "");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Today, data.Part1, 10m, serviceAreaLoc, "");
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", ZDateTimeOffset.Today, data.Part1, 10m, serviceAreaLoc, "");
			var inventoryLine1 = receive1.Lines[0];
			var inventoryLine2 = receive2.Lines[0];
			var inventoryLine3 = receive3.Lines[0];
			var inventoryLine4 = receive4.Lines[0];
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			var transferLine = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			Factory.Save();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition: stagedInventoryLine is related to inventoryLine1.", inventoryLine1.PK, transferLine.PickLines.Single().InventoryLine.PK);
			AssertEquals("Transfer lines for VAS Orders should *not* have their Original Status changed.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Transfer lines for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on Transfers for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, transferLine.Inventory[0].WI_InventoryStatus);

			AssertNoExceptionThrown(() => Factory.Save()); // ensure data is correct

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			Factory.Save();

			AssertNoExceptionThrown(() => ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff));
			AssertEquals("Original Status for Staged TrasferLine should not be changed.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Current Status for Staged TrasferLine should still be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Current Status for Inventory on Transfers for VAS Orders should still be Staged.", InventoryStatus.Codes.Staged, transferLine.Inventory[0].WI_InventoryStatus);
			AssertEquals("Staged inventory should have the same location", serviceAreaLoc.PK, inventoryLine1.Location.PK);
			AssertEquals("Staged inventory was NOT shorted.", string.Empty, inventoryLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Staged inventory was NOT held.", InventoryStatus.Codes.Available, inventoryLine1.WE_CurrentInventoryStatus);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine2.WE_CurrentInventoryStatus);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine3.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine3.WE_CurrentInventoryStatus);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine4.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine4.WE_CurrentInventoryStatus);
		}

		[TestDate(2024, 7, 15, 10, 12, 22)]
		public void TestConfirmPickLinePickedQty_ShortPickAllInventory_IgnoresStagedInventory_WithPalletID()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Factory.Save();
			var serviceAreaLoc = data.Whs1.FindLocation("SERVICEROW");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 10m, serviceAreaLoc, "PID1");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 10m, serviceAreaLoc, "PID1");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Today, data.Part1, 10m, serviceAreaLoc, "PID1");
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", ZDateTimeOffset.Today, data.Part1, 10m, serviceAreaLoc, "PID1");
			var inventoryLine1 = receive1.Lines[0];
			var inventoryLine2 = receive2.Lines[0];
			var inventoryLine3 = receive3.Lines[0];
			var inventoryLine4 = receive4.Lines[0];
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			var transferLine = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			Factory.Save();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition: stagedInventoryLine is related to inventoryLine1.", inventoryLine1.PK, transferLine.PickLines.Single().InventoryLine.PK);
			AssertEquals("Transfer lines for VAS Orders should *not* have their Original Status changed.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Transfer lines for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on Transfers for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, transferLine.Inventory[0].WI_InventoryStatus);

			AssertNoExceptionThrown(() => Factory.Save()); // ensure data is correct

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			Factory.Save();

			AssertNoExceptionThrown(() => ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff));
			AssertEquals("Original Status for Staged TrasferLine should not be changed.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Current Status for Staged TrasferLine should still be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Current Status for Inventory on Transfers for VAS Orders should still be Staged.", InventoryStatus.Codes.Staged, transferLine.Inventory[0].WI_InventoryStatus);
			AssertEquals("Staged inventory should have the same location", serviceAreaLoc.PK, inventoryLine1.Location.PK);
			AssertEquals("Staged inventory was NOT shorted.", string.Empty, inventoryLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Staged inventory was NOT held.", InventoryStatus.Codes.Available, inventoryLine1.WE_CurrentInventoryStatus);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine2.WE_CurrentInventoryStatus);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine3.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine3.WE_CurrentInventoryStatus);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine4.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine4.WE_CurrentInventoryStatus);
		}

		public void TestConfirmPickLinePickedQty_ShortPickAllInventory_IgnoresStagedInventory_WithPalletID_EdgeCase()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc = data.Whs1.FindLocation("A-1");
			Factory.Save();
			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var product2 = Helper.CreateProduct(data.Org1, "Product2");
			Factory.Save();
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 10m, loc, "PID1");
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 10m, loc, "PID1");// different client
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Today, product2, 10m, loc, "PID1");// different product
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", ZDateTimeOffset.Today, data.Part1, 10m, loc, "PID1");

			var inventoryLine1 = receive1.Lines[0];
			var inventoryLine2 = receive2.Lines[0];
			var inventoryLine3 = receive3.Lines[0];
			var inventoryLine4 = receive4.Lines[0];
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, data.Part1, 5m);// different client
			var orderLine2 = order2.Lines[0];
			var pick2 = Helper.CreatePickNew(order2);
			var stagedInventoryLine1 = Helper.PickAndMakeInTransitTransfer(order2.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);
			var ddlTransfer1 = pick2.Transfers.Single();
			ddlTransfer1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition: stagedInventoryLine1 is related to inventoryLine2.", inventoryLine2.PK, stagedInventoryLine1.PickLines.Single().InventoryLine.PK);
			AssertEquals("Precondition: Should be staged.", InventoryStatus.Codes.Staged, stagedInventoryLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be available.", InventoryStatus.Codes.Available, stagedInventoryLine1.WE_OriginalInventoryStatus);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, product2, 5m);// different product
			var orderLine3 = order3.Lines[0];
			var pick3 = Helper.CreatePickNew(order3);
			var stagedInventoryLine2 = Helper.PickAndMakeInTransitTransfer(order3.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);

			var ddlTransfer2 = pick3.Transfers.Single();
			ddlTransfer2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition: stagedInventoryLine2 is related to inventoryLine3.", inventoryLine3.PK, stagedInventoryLine2.PickLines.Single().InventoryLine.PK);
			AssertEquals("Precondition: Should be staged.", InventoryStatus.Codes.Staged, stagedInventoryLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be available.", InventoryStatus.Codes.Available, stagedInventoryLine2.WE_OriginalInventoryStatus);

			Factory.Save();
			AssertNoExceptionThrown(() => ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff));
			AssertEquals("Original Status for Staged TrasferLine1 should not be changed.", InventoryStatus.Codes.Available, stagedInventoryLine1.WE_OriginalInventoryStatus);
			AssertEquals("Current Status for Staged TrasferLine1 should still be Staged.", InventoryStatus.Codes.Staged, stagedInventoryLine1.WE_CurrentInventoryStatus);
			AssertEquals("Current Status for Inventory on Transfer1 for VAS Orders should still be Staged.", InventoryStatus.Codes.Staged, stagedInventoryLine1.Inventory[0].WI_InventoryStatus);
			AssertEquals("Original Status for Staged TrasferLine2 should not be changed.", InventoryStatus.Codes.Available, stagedInventoryLine2.WE_OriginalInventoryStatus);
			AssertEquals("Current Status for Staged TrasferLine2 should still be Staged.", InventoryStatus.Codes.Staged, stagedInventoryLine2.WE_CurrentInventoryStatus);
			AssertEquals("Current Status for Inventory on Transfer2 for VAS Orders should still be Staged.", InventoryStatus.Codes.Staged, stagedInventoryLine2.Inventory[0].WI_InventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine1.WE_CurrentInventoryStatus);
			AssertEquals("Staged inventory was NOT shorted.", string.Empty, inventoryLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Staged inventory was NOT held.", InventoryStatus.Codes.Available, inventoryLine2.WE_CurrentInventoryStatus);
			AssertEquals("Staged inventory was NOT shorted.", string.Empty, inventoryLine3.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Staged inventory was NOT held.", InventoryStatus.Codes.Available, inventoryLine3.WE_CurrentInventoryStatus);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine4.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine4.WE_CurrentInventoryStatus);
		}

		#endregion

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_ShortPickAllInventoryFromPalletWithMatchingProduct_IgnoresAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var now = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, loc, "", now.AddDays(5), now.AddDays(-1), "A1", "B1", "C1", "");
			receiveLine1.WE_SerialNumber = "S1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, loc, "", now.AddDays(5), now.AddDays(-1), "A2", "B2", "C2", "");
			receiveLine2.WE_SerialNumber = "S2";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, loc, "", now.AddDays(5), now.AddDays(-1), "A3", "B3", "C3", "");
			receiveLine3.WE_SerialNumber = "S3";
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, loc, "", now.AddDays(5), now.AddDays(-1), "A4", "B4", "C4", "");
			receiveLine4.WE_SerialNumber = "S4";
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, loc, "", now.AddDays(5), now.AddDays(-1), "A5", "B5", "C5", "");
			receiveLine5.WE_SerialNumber = "S5";
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			var allocatedInventoryLine = order.Lines.Single().PickLines.Single().WZ_WE_InventoryLine;

			Factory.Save();

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff);

			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Should only be 1 short lines", 1, Factory.Load<WhsPickShortLine>(new ZQuery()).Length);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine1.WE_CurrentInventoryStatus);
			AssertWhsPickShortLineExists(allocatedInventoryLine, order.Lines[0].PK, 1m, staff.GS_Code, ZDateTime.UtcNow);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine2.WE_CurrentInventoryStatus);

			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, receiveLine3.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, receiveLine3.WE_CurrentInventoryStatus);

			AssertEquals("Inventory for different product was not shorted.", string.Empty, receiveLine4.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory for different product was not held.", InventoryStatus.Codes.Available, receiveLine4.WE_CurrentInventoryStatus);

			AssertEquals("Inventory for different product was not shorted.", string.Empty, receiveLine5.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory for different product was not held.", InventoryStatus.Codes.Available, receiveLine5.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_MultiplePickLineWithSameInventoryLine_ShortPickPartial

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_MultiplePickLineWithSameInventoryLine_ShortPickPartial()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 2m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order1, order2, order3, order4);
			Factory.Save();

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(5, false), staff);

			var inventoryLines = Helper.Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, receive.Lines[0].PK));
			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order1 fully picked.", 4m, order1.WD_UnitsSent);
			AssertEquals("Order2 was shorted.", 1m, order2.WD_UnitsSent);
			AssertEquals("Order3 was shorted.", 0m, order3.WD_UnitsSent);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLines[0].WE_CurrentInventoryStatus);
			AssertEquals("2 Held Inventory created, 1 from reduce pickline and 1 from delete picklines.", 2, inventoryLines.Length);
			AssertEquals("Order4 was shorted.", 0m, order4.WD_UnitsSent);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLines[0].WE_WHC_NKCurrentInventoryHeldCode);

			AssertEquals("Should only be 3 short lines", 3, Factory.Load<WhsPickShortLine>(new ZQuery()).Length);
			AssertWhsPickShortLineExists(receive.Lines[0].PK, order2.Lines[0].PK, 2m, staff.GS_Code, ZDateTime.UtcNow);
			AssertWhsPickShortLineExists(receive.Lines[0].PK, order3.Lines[0].PK, 2m, staff.GS_Code, ZDateTime.UtcNow);
			AssertWhsPickShortLineExists(receive.Lines[0].PK, order4.Lines[0].PK, 1m, staff.GS_Code, ZDateTime.UtcNow);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_CreateCycleCountOnShort

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_CreateCycleCountOnShort_ParamOn()
		{
			TestConfirmPickLinePickedQty_CreateCycleCountOnShort_Core(paramOn: true);
		}

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_CreateCycleCountOnShort_ParamOff()
		{
			TestConfirmPickLinePickedQty_CreateCycleCountOnShort_Core(paramOn: false);
		}

		void TestConfirmPickLinePickedQty_CreateCycleCountOnShort_Core(bool paramOn)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_CycleCountOnShort = paramOn;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 2m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order1, order2, order3, order4);
			Factory.Save();

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(5, false), staff);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventoryLines = newFactory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, receive.Lines[0].PK));
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLines[0].WE_WHC_NKCurrentInventoryHeldCode);

			AssertEquals("Should only be 3 short lines", 3, Factory.Load<WhsPickShortLine>(new ZQuery()).Length);
			AssertWhsPickShortLineExists(receive.Lines[0].PK, order2.Lines[0].PK, 2m, staff.GS_Code, ZDateTime.UtcNow);
			AssertWhsPickShortLineExists(receive.Lines[0].PK, order3.Lines[0].PK, 2m, staff.GS_Code, ZDateTime.UtcNow);
			AssertWhsPickShortLineExists(receive.Lines[0].PK, order4.Lines[0].PK, 1m, staff.GS_Code, ZDateTime.UtcNow);

			var createCycleCounts = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			if (paramOn)
			{
				AssertEquals("Only 1 task should be created", 1, createCycleCounts.Length);

				var cycleCountTask = createCycleCounts[0];
				AssertEquals("1 Cycle Count should be created to the specified location", location.PK, cycleCountTask.WCL_WL_Location);
				AssertEquals("1 Cycle Count should be created to the specified granularity", CycleCountGranularity.Codes.ProductWithAttributes, cycleCountTask.WCL_Granularity);
				AssertEquals("1 Cycle Count should be created to the specified priority", (byte)1, cycleCountTask.WCL_Priority);
			}
			else
			{
				AssertEquals("No tasks should be created", 0, createCycleCounts.Length);
			}
		}

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_CreateCycleCountOnShort_MultipleLocations()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_CycleCountOnShort = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, location1);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(5, false), staff);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventoryLines = newFactory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, inv.WI_WE_InDocketLine));
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLines[0].WE_WHC_NKCurrentInventoryHeldCode);
			AssertWhsPickShortLineExists(inv.WI_WE_InDocketLine, order2.Lines[0].PK, 1m, staff.GS_Code, ZDateTime.UtcNow);

			var createCycleCounts = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Only 1 task should be created", 1, createCycleCounts.Length);

			var cycleCountTask = createCycleCounts[0];
			AssertEquals("1 Cycle Count should be created to the specified location", location2.PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("1 Cycle Count should be created to the specified granularity", CycleCountGranularity.Codes.ProductWithAttributes, cycleCountTask.WCL_Granularity);
			AssertEquals("1 Cycle Count should be created to the specified priority", (byte)1, cycleCountTask.WCL_Priority);
		}

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_CreateCycleCountOnShort_ParamsDifferentForMultipleClients_ParamOn()
		{
			TestConfirmPickLinePickedQty_CreateCycleCountOnShort_ParamsDifferentForMultipleClientsCore(paramOn: true);
		}

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_CreateCycleCountOnShort_ParamsDifferentForMultipleClients_ParamOff()
		{
			TestConfirmPickLinePickedQty_CreateCycleCountOnShort_ParamsDifferentForMultipleClientsCore(paramOn: false);
		}

		void TestConfirmPickLinePickedQty_CreateCycleCountOnShort_ParamsDifferentForMultipleClientsCore(bool paramOn)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_CycleCountOnShort = true;

			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams2.WPP_CycleCountOnShort = paramOn;

			var location = data.Whs1.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 4m, location);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 2m, location);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O3", data.Part1, 2m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(5m, false), staff);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventoryLines = newFactory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, inv.WI_WE_InDocketLine));
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLines[0].WE_WHC_NKCurrentInventoryHeldCode);
			AssertWhsPickShortLineExists(inv.WI_WE_InDocketLine, order2.Lines[0].PK, 1m, staff.GS_Code, ZDateTime.UtcNow);

			var createCycleCounts = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Correct number of tasks should be created", paramOn ? 1 : 0, createCycleCounts.Length);

			if (paramOn)
			{
				var cycleCountTask = createCycleCounts[0];
				AssertEquals("1 Cycle Count should be created to the specified location", location.PK, cycleCountTask.WCL_WL_Location);
				AssertEquals("1 Cycle Count should be created to the specified granularity", CycleCountGranularity.Codes.ProductWithAttributes, cycleCountTask.WCL_Granularity);
				AssertEquals("1 Cycle Count should be created to the specified priority", (byte)1, cycleCountTask.WCL_Priority);
			}
		}

		[TestDate(2023, 5, 1, 8, 12, 22)]
		public void TestConfirmPickLinePickedQty_CreateCycleCountOnShort_ParamsDifferentForMultipleWarehouses()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 2, 1);
			Factory.Save();

			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_CycleCountOnShort = false;

			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = whs2.PK;
			pickParams2.WPP_CycleCountOnShort = true;

			var locationWhs1 = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 4m, locationWhs1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var locationWhs2 = whs2.FindLocation("A-2");
			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2");
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 4m, locationWhs2);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs2, "O2", data.Part1, 4m);
			var pick2 = Helper.CreatePickNew(order2);
			Factory.Save();

			ConfirmPickLinesPickedQty(pick1.GetAllPickLines().ToArray(), new PickingInfo(2, false), staff);
			ConfirmPickLinesPickedQty(pick2.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			AssertEquals("Should only be 2 short lines", 2, newFactory.Load<WhsPickShortLine>(new ZQuery()).Length);

			var inventory1Lines = newFactory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, inv1.WI_WE_InDocketLine));
			AssertEquals("Inventory1 was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventory1Lines[0].WE_WHC_NKCurrentInventoryHeldCode);
			AssertWhsPickShortLineExists(inv1.WI_WE_InDocketLine, order1.Lines[0].PK, 2m, staff.GS_Code, ZDateTime.UtcNow);

			var inventory2Lines = newFactory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.PK, inv2.WI_WE_InDocketLine));
			AssertEquals("Inventory2 was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventory2Lines[0].WE_WHC_NKCurrentInventoryHeldCode);
			AssertWhsPickShortLineExists(inv2.WI_WE_InDocketLine, order2.Lines[0].PK, 4m, staff.GS_Code, ZDateTime.UtcNow);

			var createCycleCounts = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Only 1 task should be created", 1, createCycleCounts.Length);

			var cycleCountTask = createCycleCounts[0];
			AssertEquals("1 Cycle Count should be created to the specified location", locationWhs2.PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("1 Cycle Count should be created to the specified granularity", CycleCountGranularity.Codes.ProductWithAttributes, cycleCountTask.WCL_Granularity);
			AssertEquals("1 Cycle Count should be created to the specified priority", (byte)1, cycleCountTask.WCL_Priority);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_SuspendsPercentageCompleteRecalculationUntilAllPicklinesAreUpdated

		public void TestConfirmPickLinePickedQty_SuspendsPercentageCompleteRecalculationUntilAllPicklinesAreUpdated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "ABC", false, true);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locationA2, "DEF", false, true);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var percentageCompleteValueUpdated = 0;
			pick.WP_PercentageCompleteInfo.ValueChanged += (s, e) => { percentageCompleteValueUpdated++; };
			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(20m, false), staff);

			AssertEquals("WP_PercentageComple is updated once only.", 1, percentageCompleteValueUpdated);
			AssertEquals("WP_PercentageComple is correct.", new ZByte(100), pick.WP_PercentageComplete);
		}

		#endregion

		#region SerialNumber

		public void TestConfirmPickLinePickedQty_WhenPickingMoreThan50ProductWithSerialNumberAttributeInOneOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 51m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 51m);
			var pick = Helper.CreatePickNew(order1);

			Factory.Save();

			var pickLines = pick.GetAllPickLines().ToArray();
			var transactionLinePks = pickLines.Select(pickLine => pickLine.WZ_WE_TransactionLine).ToArray();
			var persistedPickLines = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, transactionLinePks));

			var pickLinePks = pickLines.Select(pickLine => pickLine.PK.ToGuid()).ToArray();
			AssertEquals("Precondition: Expecting 1 pick lines", 1, persistedPickLines.Length);

			var releaseCapturedInfo = Enumerable
				.Range(1, 51)
				.Select(serial => new WhsReleaseCapturedInfo { SerialNumber = serial.ToString(), Quantity = 1 })
				.ToArray();

			PickLineUpdater.ConfirmPickLinesPickedQty(
				pickLines,
				new PickingInfo(51m, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePks, new[] { new PickedPackTypeInfo("UNT", 1m, "", releaseCapturedInfo) }) },
				null,
				false);
			staff.Factory.Save();
			var assertionFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var resultPickLines = assertionFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, transactionLinePks));

			AssertEquals("Expecting 51 pick lines", 51, resultPickLines.Length);
			AssertEquals("Expecting all pick lines to have 'Original Picked Inventory Set' value", true, resultPickLines.All(pickLine => pickLine.IsPickedFromPutawayLocation));
			AssertEquals("Expecting all pick lines to have In-Transit Transfer Line.", true, resultPickLines.All(pickLine => ((WhsTransferLine)pickLine.InventoryLine).IsPicked));
			AssertEquals("Expecting correct total picked quantity from all pick lines", 51m, resultPickLines.Sum(pickLine => pickLine.WZ_Units));
		}

		public void TestPackPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var wrongOperator = Factory.NewWithValidTestData<GlbStaff>();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "RED", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BLUE", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX", "ABC");
			Factory.Save();

			orderLine.ClearReleaseLines();
			AssertExceptionThrown<ArgumentNullException>(() => PickLineUpdater.ConfirmPickLinesPickedQty(
				Array.Empty<WhsPickLine>(),
				new PickingInfo(),
				null,
				Array.Empty<PickLinesToPickedPackTypeInfo>(),
				package,
				false));

			var packTypeInfo1 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN1", 1) });
			var packTypeInfo2 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN2", 1) });
			var packTypeInfo3 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN3", 1) });

			var packTypeInfo6 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN6", 1) });
			var packTypeInfo7 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN7", 1) });
			var packTypeInfo8 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN8", 1) });
			var packTypeInfo9 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN9", 1) });
			var packTypeInfo10 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN10", 1) });

			var pickLine1 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "RED");
			var pickLine2 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "BLUE");
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1 },
				new PickingInfo(3m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) },
				package,
				false);

			orderLine.ClearReleaseLines();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine2 },
				new PickingInfo(5m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, new[] { packTypeInfo6, packTypeInfo7, packTypeInfo8, packTypeInfo9, packTypeInfo10 }) },
				package,
				false);

			package.PackedItems.RemoveAll();
			package.PackedItemDivots.Add(package.PackedItemDivots[0]); // refresh packedItems non-persistent collection on package
			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN1");
			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN2");
			var releaseLine3 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN3");
			var releaseLine4 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN6");
			var releaseLine5 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN7");
			var releaseLine6 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN8");
			var releaseLine7 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN9");
			var releaseLine8 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN10");
			AssertContainsExactElementsInAnyOrder(
				new[] { releaseLine1, releaseLine2, releaseLine3, releaseLine4, releaseLine5, releaseLine6, releaseLine7, releaseLine8 },
				package.PackedItems.Typed.Select(p => p.PackableItemParent));

			AssertEquals("We have 8 pick lines now.", 8, orderLine.PickLines.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "SN1", "SN2", "SN3", "SN6", "SN7", "SN8", "SN9", "SN10" }, orderLine.PickLines.Select(l => l.WZ_ReleaseCapturedSerialNumber));
			AssertEquals("All Release Captured Attributes should have quantity of 1 and no attribute values for non-release captured Attributes.", true,
				orderLine.PickLines.All(l => l.WZ_ReleaseCapturedPartAttrib1 == "" && l.WZ_ReleaseCapturedPartAttrib2 == "" && l.WZ_ReleaseCapturedPartAttrib3 == "" && l.WZ_Units == 1m));
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, package.PackedItems.Typed.Select(p => p.PackedItems.Single()));

			AssertEquals("All Serial's Numbers are packed with Quantity 1.", 8, package.PackedItemDivots.Count(p => p.KI_PackedQty == 1m));
			AssertEquals("Should have Packed the correct Release Lines,", 3, new[] { releaseLine1, releaseLine2, releaseLine3 }.Count(r => r.PartAttribute2 == "RED"));
			AssertEquals("Should have Packed the correct Release Lines,", 5,
				new[] { releaseLine4, releaseLine5, releaseLine6, releaseLine7, releaseLine8 }.Count(r => r.PartAttribute2 == "BLUE"));
		}

		public void TestPackPickLines_ClosePackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var wrongOperator = Factory.NewWithValidTestData<GlbStaff>();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "RED", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BLUE", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX", "ABC");
			Factory.Save();

			orderLine.ClearReleaseLines();
			var packTypeInfo1 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN1", 1) });
			var packTypeInfo2 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN2", 1) });
			var packTypeInfo3 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN3", 1) });

			var packTypeInfo6 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN6", 1) });
			var packTypeInfo7 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN7", 1) });
			var packTypeInfo8 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN8", 1) });
			var packTypeInfo9 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN9", 1) });
			var packTypeInfo10 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN10", 1) });

			var pickLine1 = orderLine.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "RED");
			var pickLine2 = orderLine.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "BLUE");
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1 },
				new PickingInfo(3m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) },
				package,
				false);

			orderLine.ClearReleaseLines();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine2 },
				new PickingInfo(5m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, new[] { packTypeInfo6, packTypeInfo7, packTypeInfo8, packTypeInfo9, packTypeInfo10 }) },
				package,
				false);

			var response = new WebServiceResponse();
			PickLineUpdater.ClosePackage(package, response);

			package.PackedItems.RemoveAll();
			package.PackedItemDivots.Add(package.PackedItemDivots[0]); // refresh packedItems non-persistent collection on package
			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN1");
			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN2");
			var releaseLine3 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN3");
			var releaseLine4 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN6");
			var releaseLine5 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN7");
			var releaseLine6 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN8");
			var releaseLine7 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN9");
			var releaseLine8 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN10");
			AssertContainsExactElementsInAnyOrder(
				new[] { releaseLine1, releaseLine2, releaseLine3, releaseLine4, releaseLine5, releaseLine6, releaseLine7, releaseLine8 },
				package.PackedItems.Typed.Select(p => p.PackableItemParent));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "SN1", "SN2", "SN3" },
				orderLine.PickLines.Where(l => l.InventoryLine.WE_PartAttrib2 == "RED").Select(l => l.WZ_ReleaseCapturedSerialNumber));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "SN6", "SN7", "SN8", "SN9", "SN10" },
				orderLine.PickLines.Where(l => l.InventoryLine.WE_PartAttrib2 == "BLUE").Select(l => l.WZ_ReleaseCapturedSerialNumber));
			AssertEquals("All Release Captured Attributes should have quantity of 1 and no attribute values for non-release captured Attributes.", true,
				orderLine.PickLines.All(l => l.WZ_ReleaseCapturedPartAttrib1 == "" && l.WZ_ReleaseCapturedPartAttrib2 == "" && l.WZ_ReleaseCapturedPartAttrib3 == "" && l.WZ_Units == 1m));
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, package.PackedItems.Typed.Select(p => p.PackedItems.Single()));

			AssertEquals("All Serial's Numbers are packed with Quantity 1.", 8, package.PackedItemDivots.Count(p => p.KI_PackedQty == 1m));
			AssertEquals("Should have Packed the correct Release Lines,", 3, new[] { releaseLine1, releaseLine2, releaseLine3 }.Count(r => r.PartAttribute2 == "RED"));
			AssertEquals("Should have Packed the correct Release Lines,", 5,
				new[] { releaseLine4, releaseLine5, releaseLine6, releaseLine7, releaseLine8 }.Count(r => r.PartAttribute2 == "BLUE"));
			AssertEquals("Package should be closed.", true, package.KP_ClosedTimeUtc.IsValid);
		}

		public void TestPackPickLines_ClosePackage_ZeroUnitsPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var wrongOperator = Factory.NewWithValidTestData<GlbStaff>();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX", "ABC");
			Factory.Save();

			orderLine.ClearReleaseLines();
			var packTypeInfo1 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN1", 1) });
			var packTypeInfo2 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN2", 1) });
			var packTypeInfo3 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN3", 1) });

			var packTypeInfo6 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN6", 1) });
			var packTypeInfo7 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN7", 1) });
			var packTypeInfo8 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN8", 1) });
			var packTypeInfo9 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN9", 1) });
			var packTypeInfo10 = new PickedPackTypeInfo("UNT", 1m, "", new[] { new WhsReleaseCapturedInfo("", "", "", "SN10", 1) });

			var pickLine1 = orderLine.PickLines[0];
			var pickLine2 = orderLine.PickLines[1];
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1, pickLine2 },
				new PickingInfo(8m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3, packTypeInfo6, packTypeInfo7, packTypeInfo8, packTypeInfo9, packTypeInfo10 }) },
				package,
				false);

			orderLine.ClearReleaseLines();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1 },
				new PickingInfo(0m, false, true),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3, packTypeInfo6, packTypeInfo7, packTypeInfo8, packTypeInfo9, packTypeInfo10 }) },
				package,
				false);

			var response = new WebServiceResponse();
			PickLineUpdater.ClosePackage(package, response);

			package.PackedItems.RemoveAll();
			package.PackedItemDivots.Add(package.PackedItemDivots[0]); // refresh packedItems non-persistent collection on package
			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN1");
			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN2");
			var releaseLine3 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN3");
			var releaseLine4 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN6");
			var releaseLine5 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN7");
			var releaseLine6 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN8");
			var releaseLine7 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN9");
			var releaseLine8 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN10");
			AssertContainsExactElementsInAnyOrder(
				new[] { releaseLine1, releaseLine2, releaseLine3, releaseLine4, releaseLine5, releaseLine6, releaseLine7, releaseLine8 },
				package.PackedItems.Typed.Select(p => p.PackableItemParent));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "SN1", "SN2", "SN3", "SN6", "SN7", "SN8", "SN9", "SN10" }, orderLine.PickLines.Select(l => l.WZ_ReleaseCapturedSerialNumber));
			AssertEquals("All Release Captured Attributes should have quantity of 1 and no attribute values for non-release captured Attributes.", true,
				orderLine.PickLines.All(l => l.WZ_ReleaseCapturedPartAttrib1 == "" && l.WZ_ReleaseCapturedPartAttrib2 == "" && l.WZ_ReleaseCapturedPartAttrib3 == "" && l.WZ_Units == 1m));
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, package.PackedItems.Typed.Select(p => p.PackedItems.Single()));

			AssertEquals("All Serial's Numbers are packed with Quantity 1.", 8, package.PackedItemDivots.Count(p => p.KI_PackedQty == 1m));
			AssertEquals("Package should be closed.", true, package.KP_ClosedTimeUtc.IsValid);
		}

		public void TestPackPickLines_WithAlreadyCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");

			var pickLine = pick.GetAllPickLines().Single();
			var orderLine = order.Lines[0];
			AssertEquals("Precondition: Should have 1 release Line.", 1, orderLine.ReleaseLines.Count);

			var releaseCapturedInfos1 = new[] { new WhsReleaseCapturedInfo { SerialNumber = "SN1", Quantity = 1m }, new WhsReleaseCapturedInfo { SerialNumber = "SN2", Quantity = 1m } };
			orderLine.ClearReleaseLines();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine }, new PickingInfo(2m, false, true),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 2m, "", releaseCapturedInfos1) }) },
				package,
				false);
			AssertEquals("Items are packed.", 2, package.PackedItemDivots.Count);
			AssertEquals("Should have split the release line twice.", 3, orderLine.ReleaseLines.Count);
			AssertNoExceptionThrown(() => Factory.Save());

			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN1");
			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN2");
			AssertEquals("Released Quantity is correct.", 1m, releaseLine1.Quantity);
			AssertEquals("Released Quantity is correct.", 1m, releaseLine2.Quantity);

			AssertEquals("We have 3 Pick Lines now.", 3, orderLine.PickLines.Count);
			AssertEquals("Only two Release Captured Attribs are stored.", 2, orderLine.PickLines.Count(l => l.HasReleaseCapturedAttribs));

			var pickLine1 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedSerialNumber == "SN1");
			var pickLine2 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedSerialNumber == "SN2");
			AssertEquals("Released Quantity is correct.", 1m, pickLine1.WZ_Units);
			AssertEquals("Released Quantity is correct.", 1m, pickLine2.WZ_Units);

			var packedItem1 = package.PackedItems.Typed.Single(p => p.PackableItemParent == releaseLine1);
			var packedItem2 = package.PackedItems.Typed.Single(p => p.PackableItemParent == releaseLine2);
			AssertEquals("Packed Item Quantity is correct.", 1m, packedItem1.PackedQty);
			AssertEquals("Packed Item Quantity is correct.", 1m, packedItem2.PackedQty);
			AssertEquals("Packed Item is correct.", pickLine1, packedItem1.PackedItems.Single());
			AssertEquals("Packed Item is correct.", pickLine2, packedItem2.PackedItems.Single());

			var releaseCapturedInfos2 = new[] { new WhsReleaseCapturedInfo { SerialNumber = "SN3", Quantity = 1 } };
			orderLine.ClearReleaseLines();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(1m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 1m, "", releaseCapturedInfos2) }) },
				package,
				false);
			AssertEquals("Items are packed.", 3, package.PackedItemDivots.Count);
			AssertEquals("Release Lines should remain split.", 3, orderLine.ReleaseLines.Count);

			releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN1");
			releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN2");
			var releaseLine3 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN3");
			AssertEquals("Released Quantity is correct.", 1m, releaseLine1.Quantity);
			AssertEquals("Released Quantity is correct.", 1m, releaseLine2.Quantity);
			AssertEquals("Released Quantity is correct.", 1m, releaseLine3.Quantity);

			AssertEquals("Only three Release Captured Attribs are stored.", 3, orderLine.PickLines.Count(l => l.HasReleaseCapturedAttribs));

			pickLine1 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedSerialNumber == "SN1");
			pickLine2 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedSerialNumber == "SN2");
			var pickLine3 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedSerialNumber == "SN3");

			package.PackedItems.RemoveAll();
			package.PackedItemDivots.Add(package.PackedItemDivots[0]); // refresh packed items.
			packedItem1 = package.PackedItems.Typed.Single(p => p.PackableItemParent == releaseLine1);
			packedItem2 = package.PackedItems.Typed.Single(p => p.PackableItemParent == releaseLine2);
			var packedItem3 = package.PackedItems.Typed.Single(p => p.PackableItemParent == releaseLine3);
			AssertEquals("Packed Item Quantity is correct.", 1m, packedItem1.PackedQty);
			AssertEquals("Packed Item Quantity is correct.", 1m, packedItem2.PackedQty);
			AssertEquals("Packed Item Quantity is correct.", 1m, packedItem3.PackedQty);
			AssertEquals("Packed Item Quantity is correct.", pickLine1, packedItem1.PackedItems.Single());
			AssertEquals("Packed Item Quantity is correct.", pickLine2, packedItem2.PackedItems.Single());
			AssertEquals("Packed Item Quantity is correct.", pickLine3, packedItem3.PackedItems.Single());

			AssertEquals("Released Quantity is correct.", 1m, pickLine1.WZ_Units);
			AssertEquals("Released Quantity is correct.", 1m, pickLine2.WZ_Units);
			AssertEquals("Released Quantity is correct.", 1m, pickLine3.WZ_Units);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestPackPickLineForAttributeNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN2", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN4", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertNotEquals("Precondition: Release Line has Serial Number.", "", releaseLine.SerialNumber);

			var pickLine = orderLine.PickLines.Single(p => p.InventoryLine.WE_SerialNumber == releaseLine.SerialNumber);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			AssertExceptionThrown<ArgumentNullException>(() => PickLineUpdater.PackPickLineForAttributeNeutral(null, releaseLine, package));
			AssertExceptionThrown<ArgumentNullException>(() => PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, null, package));
			AssertExceptionThrown<ArgumentNullException>(() => PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, releaseLine, null));

			pickLine.WZ_Units = 2m;
			AssertExceptionThrown(typeof(InvalidOperationException), "Must pack Serial Numbers in Units of 1.",
				() => PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, releaseLine, package));
			pickLine.WZ_Units = 1m; // clean-up

			PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, releaseLine, package);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, package.PackedItems.Typed.Select(d => d.PackableItemParent));
			AssertEquals("Should have correct Packed Item.", pickLine, package.PackedItems[0].PackedItems.Single());
			AssertEquals("Only 1 should be packed.", 1m, package.PackedItems[0].PackedQty);

			AssertExceptionThrown(typeof(InvalidOperationException), "Pick Line to Pack for Attribute Neutral should be unpacked.",
				() => PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, releaseLine, package));
		}

		public void TestPackPickLineForAttributeNeutral_ReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN2", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN4", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertNotEquals("Precondition: Release Line has Serial Number.", "", releaseLine.SerialNumber);

			releaseLine.PartAttribute2 = "RED"; // simulate web service release capturing the Serial number.
			var pickLine = orderLine.PickLines.Single(p => p.InventoryLine.WE_SerialNumber == releaseLine.SerialNumber);

			AssertEquals("Precondition: Release Captured correctly.", 1, orderLine.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED" && l.WZ_Units == 1m));

			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			AssertExceptionThrown<ArgumentNullException>(() => PickLineUpdater.PackPickLineForAttributeNeutral(null, releaseLine, package));
			AssertExceptionThrown<ArgumentNullException>(() => PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, null, package));
			AssertExceptionThrown<ArgumentNullException>(() => PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, releaseLine, null));

			pickLine.WZ_Units = 2m;
			AssertExceptionThrown(typeof(InvalidOperationException), "Must pack Serial Numbers in Units of 1.",
				() => PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, releaseLine, package));
			pickLine.WZ_Units = 1m; // clean-up

			PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, releaseLine, package);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, package.PackedItems.Typed.Select(d => d.PackableItemParent));

			AssertEquals("Should have correct Packed Item.", pickLine, package.PackedItems[0].PackedItems.Single());

			AssertEquals("Only 1 should be packed.", 1m, package.PackedItems[0].PackedQty);

			AssertExceptionThrown(typeof(InvalidOperationException), "Pick Line to Pack for Attribute Neutral should be unpacked.",
				() => PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, releaseLine, package));
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestConfirmPickLinePickedQty_ReturnShortedOrderLines

		public void TestConfirmPickLinePickedQty_ReturnShortedOrderLines()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var shortedOrderLinePKs = ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(0, false), staff);
			AssertEquals("order1 was shorted.", 0m, order1.WD_UnitsSent);
			AssertEquals("order2 was shorted.", 0m, order2.WD_UnitsSent);
			AssertEquals("Both OrderLines were shorted.", 2, shortedOrderLinePKs.Count());
			AssertContainsExactElementsInAnyOrder("OrderLine PK should match", new Guid[] { order1.Lines[0].PK.ToGuid(), order2.Lines[0].PK.ToGuid() }, shortedOrderLinePKs);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_ReturnShortedOrderLines_Partial

		public void TestConfirmPickLinePickedQty_ReturnShortedOrderLines_Partial()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var shortedOrderLinePKs = ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(4, false), staff);
			AssertEquals("order1 was all picked.", 3m, order1.WD_UnitsSent);
			AssertEquals("order2 was partial shorted.", 1m, order2.WD_UnitsSent);
			AssertEquals("order2 were shorted.", 1, shortedOrderLinePKs.Count());
			AssertContainsExactElementsInAnyOrder("OrderLine PK should match", new Guid[] { order2.Lines[0].PK.ToGuid() }, shortedOrderLinePKs);
		}

		#endregion

		#region TestConfirmPickLinePickedQty_ReturnEmptyIfAllPicked

		public void TestConfirmPickLinePickedQty_ReturnEmptyIfAllPicked()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var shortedOrderLinePKs = ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(5, false), staff);
			AssertEquals("order1 was fully picked.", 3m, order1.WD_UnitsSent);
			AssertEquals("order2 was fully shorted.", 2m, order2.WD_UnitsSent);
			AssertEquals("no orderline was shorted.", 0, shortedOrderLinePKs.Count());
		}

		#endregion

		#region TestConfirmPickLinePickedQty

		public void TestConfirmPickLinePickedQty_BOM()
		{
			var orderLine = SetupBOMData();
			var oper1 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var order = orderLine.Order;
			var pick = order.Pick;
			var bomComponentProduct1 = orderLine.SupplierPart.BillOfMaterials.Single(bom => bom.Component.OP_PartNum == "BOM1").Component;
			var bomComponentProduct2 = orderLine.SupplierPart.BillOfMaterials.Single(bom => bom.Component.OP_PartNum == "BOM2").Component;
			var orderedInventoryBomComponentProduct1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().First(oi => oi.SupplierPart.PK == bomComponentProduct1.PK);
			var orderedInventoryBomComponentProduct2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().First(oi => oi.SupplierPart.PK == bomComponentProduct2.PK);

			AssertEquals("PickLineQuantity should be 18", 18m, orderedInventoryBomComponentProduct1.PickLineQuantity);
			AssertEquals("PickLineQuantity should be 6", 6m, orderedInventoryBomComponentProduct2.PickLineQuantity);

			AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(orderLine, 11m, 6m);

			ConfirmPickLinesPickedQty(orderedInventoryBomComponentProduct1.AvailableInventories[0].PickLines.ToArray(), new PickingInfo(15m, false), oper1);
			AssertPickLines(orderedInventoryBomComponentProduct1, 15m);
			AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(orderLine, 10m, 5m);

			ConfirmPickLinesPickedQty(orderedInventoryBomComponentProduct2.AvailableInventories[0].PickLines.ToArray(), new PickingInfo(5m, false), oper1);
			AssertPickLines(orderedInventoryBomComponentProduct2, 5m);
			AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(orderLine, 10m, 5m);

			orderLine.ClearReleaseLines();
			order.Pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			order.Pick.FinalisePick();
			AssertEquals("Should be finalised", true, order.Pick.IsFinalised);
		}

		public void TestConfirmPickLinePickedQty_BOM_ShortingComponentsFully()
		{
			var orderLine = SetupBOMData();
			var oper1 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var order = orderLine.Order;
			var pick = order.Pick;
			var bomComponentProduct1 = orderLine.SupplierPart.BillOfMaterials.Single(bom => bom.Component.OP_PartNum == "BOM1").Component;
			var bomComponentProduct2 = orderLine.SupplierPart.BillOfMaterials.Single(bom => bom.Component.OP_PartNum == "BOM2").Component;
			var orderedInventoryBomComponentProduct1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().First(oi => oi.SupplierPart.PK == bomComponentProduct1.PK);
			var orderedInventoryBomComponentProduct2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().First(oi => oi.SupplierPart.PK == bomComponentProduct2.PK);

			AssertEquals("PickLineQuantity should be 18", 18m, orderedInventoryBomComponentProduct1.PickLineQuantity);
			AssertEquals("PickLineQuantity should be 6", 6m, orderedInventoryBomComponentProduct2.PickLineQuantity);
			AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(orderLine, 11m, 6m);

			ConfirmPickLinesPickedQty(orderedInventoryBomComponentProduct1.AvailableInventories[0].PickLines.ToArray(), new PickingInfo(0m, false), oper1);
			AssertPickLines(orderedInventoryBomComponentProduct1, 0m);
			AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(orderLine, 5m, 0m);

			ConfirmPickLinesPickedQty(orderedInventoryBomComponentProduct2.AvailableInventories[0].PickLines.ToArray(), new PickingInfo(0m, false), oper1);
			AssertPickLines(orderedInventoryBomComponentProduct2, 0m);
			AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(orderLine, 5m, 0m);

			order.Pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			order.Pick.FinalisePick();
			AssertEquals("Should be finalised", true, order.Pick.IsFinalised);
		}

		WhsOrderLine SetupBOMData()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 5);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 3m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 1m, Constants.PkgUnit.Unit);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, mainProduct, AttributeNumber.One, true);

			var inventoryLocation = data.Whs1.FindLocation("A-1-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 2m, data.Whs1.FindLocation("A-1-1"), ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 2m, data.Whs1.FindLocation("A-1-2"), ZDate.Empty, ZDate.Empty, "PA2", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 1m, data.Whs1.FindLocation("A-1-3"), ZDate.Empty, ZDate.Empty, "PA3", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, data.Whs1.FindLocation("A-2-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, data.Whs1.FindLocation("A-2-2"));

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 11m);
			Factory.Save();

			Helper.CreatePickNew(order);
			orderLine.ClearReleaseLines();

			return orderLine;
		}

		public void TestConfirmPickLinePickedQty_WhenPickingPicklineWithRcaAndMultipleOrders()
		{
			// Arrange.

			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, true);

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);

			orderLine1.ReleaseLines[0].PartAttribute1 = "RED";
			orderLine2.ReleaseLines[0].PartAttribute1 = "GREEN";

			Factory.Save();

			var pickLines = pick.GetAllPickLines().ToArray();
			var transactionLinePks = pickLines.Select(pickLine => pickLine.WZ_WE_TransactionLine).ToArray();
			var pickLinePks = pickLines.Select(pickLine => pickLine.PK).ToArray();

			var persistedPickLines = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, transactionLinePks));
			AssertEquals("Precondition: Expecting 2 pick lines", 2, persistedPickLines.Length);

			// Act.

			ConfirmPickLinesPickedQty(pickLines, new PickingInfo(8m, false), staff);

			// Assert.

			var resultPickLines = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, transactionLinePks));

			AssertEquals("Expecting 2 pick lines", 2, resultPickLines.Length);
			AssertEquals("Expecting all pick lines to have 'Original Picked Inventory Line' value", true, resultPickLines.All(pickLine => pickLine.IsPickedFromPutawayLocation));
			AssertEquals("Expecting correct picked quantity", 8m, resultPickLines.Sum(pickLine => pickLine.WZ_Units));
		}

		void AssertPickLines(WhsPickOrderedInventory orderedInventoryBomComponentProduct, ZDecimal pickLineQuantity)
		{
			if (pickLineQuantity != 0)
			{
				AssertEquals("PickLine units should be " + pickLineQuantity.ToString(), pickLineQuantity, orderedInventoryBomComponentProduct.AvailableInventories[0].PickLines.ElementAt(0).WZ_Units);
			}
			AssertEquals("PickLineQuantity should be " + pickLineQuantity.ToString(), pickLineQuantity, orderedInventoryBomComponentProduct.PickLineQuantity);
		}

		void AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(WhsOrderLine orderLine, ZDecimal sumOfUnitsMet, ZDecimal totalPickLineQuantityFromComponents)
		{
			AssertEquals("SumOfUnitsMet should be " + sumOfUnitsMet.ToString(), sumOfUnitsMet, orderLine.SumOfUnitsMet);
			AssertEquals("TotalPickLineQuantityFromComponents be " + totalPickLineQuantityFromComponents.ToString(), totalPickLineQuantityFromComponents, orderLine.TotalPickLineQuantityFromComponents);
		}

		public void TestConfirmPickLinePickedQty_BOM_WithReleaseCapture()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 5, 5);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductUnit(bomComponentProduct1, Constants.PkgUnit.Unit, Constants.PkgUnit.Bottle, 2m);

			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 3m, Constants.PkgUnit.Bottle);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductUnit(bomComponentProduct2, Constants.PkgUnit.Unit, Constants.PkgUnit.Bottle, 3m);
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 1m, Constants.PkgUnit.Bottle);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, bomComponentProduct1, AttributeNumber.One, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 2m, data.Whs1.FindLocation("A-1-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 2m, data.Whs1.FindLocation("A-1-2"));
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 1m, data.Whs1.FindLocation("A-1-3"));
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 38m, data.Whs1.FindLocation("A-2-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, data.Whs1.FindLocation("A-2-2"));

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 11m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			orderLine.ClearReleaseLines();
			var orderedInventoryBomComponentProduct1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(oi => oi.SupplierPart.PK == bomComponentProduct1.PK);
			var orderedInventoryBomComponentProduct2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(oi => oi.SupplierPart.PK == bomComponentProduct2.PK);

			AssertEquals("PickLineQuantity should be 36", 36m, orderedInventoryBomComponentProduct1.PickLineQuantity);
			AssertEquals("PickLineQuantity should be 18", 18m, orderedInventoryBomComponentProduct2.PickLineQuantity);

			AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(orderLine, 11m, 6m);

			var releaseCaptureInfo = new WhsReleaseCapturedInfo { Attribute1 = "RED", Quantity = 36m };
			var pickLines = orderedInventoryBomComponentProduct1.AvailableInventories[0].PickLines.ToArray();
			var pickLinePKs = pickLines.Select(pickLine => pickLine.PK.ToGuid()).ToArray();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				pickLines,
				new PickingInfo(36m, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("UNT", 36m, "", new[] { releaseCaptureInfo }) }) },
				null,
				false);

			AssertPickLines(orderedInventoryBomComponentProduct1, 36m);
			AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(orderLine, 11m, 6m);

			ConfirmPickLinesPickedQty(
				orderedInventoryBomComponentProduct2.AvailableInventories[0].PickLines.ToArray(),
				new PickingInfo(18m, false),
				staff);

			AssertPickLines(orderedInventoryBomComponentProduct2, 18m);
			AssertSumOfUnitsMetAndTotalPickLineQuantityFromComponents(orderLine, 11m, 6m);

			AssertEquals("Only 1 Release Line to represent both Full Kits and Kits built from Components.", 1, orderLine.ReleaseLines.Count);
			AssertEquals("Correct quantity Released.", 11m, orderLine.ReleaseLines[0].Quantity);

			order.Pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			order.Pick.FinalisePick();
			AssertEquals("Should be finalised", true, order.Pick.IsFinalised);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		[TestDate(2007, 12, 13)]
		public void TestConfirmPickLinePickedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10;
			pick.OrderedInventories[1].AvailableInventories[0].PickLineQuantity = 10;
			Factory.Save();

			var oper1 = Factory.NewWithValidTestData<GlbStaff>();
			var oper2 = Factory.NewWithValidTestData<GlbStaff>();

			var orderedInventory1 = pick.OrderedInventories[0];
			var orderedInventory2 = pick.OrderedInventories[1];
			AssertEquals(10m, orderedInventory1.PickLineQuantity);
			AssertEquals(10m, orderedInventory2.PickLineQuantity);

			var availableInventory1 = orderedInventory1.AvailableInventories[0];
			ConfirmPickLinesPickedQty(availableInventory1.PickLines.ToArray(), new PickingInfo(5m, false), oper1);

			var pickLine1 = availableInventory1.PickLines.ElementAt(0);
			AssertEquals(5m, pickLine1.WZ_Units);
			AssertEquals(5m, orderedInventory1.PickLineQuantity);
			AssertEquals("Inventory should be Picked.", true, pickLine1.IsPickedFromPutawayLocation);
			AssertEquals("In-Transit PickLine should not have user Assigned.", "", pickLine1.WZ_GS_NKAssignedTo);
			AssertEquals("Short Picking Inventory should hold all available units for matching product.", 0m, pickLine1.InventoryLineForAvailableInventory.WE_StockOnHand);

			var shortedInventoryLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, pickLine1.InventoryLineForAvailableInventory.PK));
			AssertEquals("Should have one shorted inventory line.", InventoryStatus.Codes.Held, shortedInventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have one shorted inventory line.", InventoryHoldCodes.Codes.ShortPicked, shortedInventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Shorted stock should be 95.", 95m, shortedInventoryLine.WE_StockOnHand);
			AssertWhsPickShortLineExists(receiveLine1.PK, orderedInventory1.Owners[0].PK, 5m, oper1.GS_Code, ZDateTime.UtcNow);

			var pickDetailsSplit1 = availableInventory1.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single();
			AssertEquals(oper1.PK, pickDetailsSplit1.AssignedToPK);
			AssertEquals(ZDateTimeOffset.Now, pickDetailsSplit1.PickedDate);

			var availableInventory2 = orderedInventory2.AvailableInventories[0];
			ConfirmPickLinesPickedQty(availableInventory2.PickLines.ToArray(), new PickingInfo(10m, false), oper2);

			var pickLine2 = availableInventory2.PickLines.ElementAt(0);
			AssertEquals(10m, pickLine2.WZ_Units);
			AssertEquals(10m, orderedInventory2.PickLineQuantity);
			AssertEquals("Inventory should be Picked.", true, pickLine2.IsPickedFromPutawayLocation);
			AssertEquals("In-Transit PickLine should not have user Assigned.", "", pickLine2.WZ_GS_NKAssignedTo);
			AssertEquals("Picking Inventory should reduce Total Units.", 90m, pickLine2.InventoryLineForAvailableInventory.WE_StockOnHand);

			var pickDetailsSplit2 = availableInventory2.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single();
			AssertEquals(oper2.PK, pickDetailsSplit2.AssignedToPK);
			AssertEquals(ZDateTimeOffset.Now, pickDetailsSplit2.PickedDate);
		}

		[TestDate(2007, 12, 13)]
		public void TestConfirmPickLinePickedQty_Errors()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10;
			pick.OrderedInventories[1].AvailableInventories[0].PickLineQuantity = 10;
			Factory.Save();

			var oper1 = Factory.NewWithValidTestData<GlbStaff>();
			var oper2 = Factory.NewWithValidTestData<GlbStaff>();

			var orderedInventory1 = pick.OrderedInventories[0];
			var orderedInventory2 = pick.OrderedInventories[1];
			AssertEquals(10m, orderedInventory1.PickLineQuantity);
			AssertEquals(10m, orderedInventory2.PickLineQuantity);

			orderedInventory1.AvailableInventories[0].PickLines.ForEach(pl => pl.WZ_GS_NKAssignedTo = oper1.GS_Code);
			AssertExceptionThrown(typeof(InvalidOperationException), "Unable to update pick line. Pick line is assigned to another operator.",
				() => ConfirmPickLinesPickedQty(orderedInventory1.AvailableInventories[0].PickLines.ToArray(), new PickingInfo(5m, false), oper2));

			AssertExceptionThrown(typeof(ArgumentException), "Confirm Qty is greater than Requested Qty.",
				() => ConfirmPickLinesPickedQty(orderedInventory1.AvailableInventories[0].PickLines.ToArray(), new PickingInfo(15m, false), oper1));
		}

		#endregion

		#region TestConfirmPickLinePickedQtyForVerifiedEmpty

		public void TestConfirmPickLinePickedQtyForVerifiedEmpty_Yes()
		{
			// setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			// setup receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var lines = pick.OrderedInventories[0].AvailableInventories[0].PickLines.ToArray();

			ConfirmPickLinesPickedQty(lines, new PickingInfo(5m, false), staff);
			Assert(lines.All(pl => pl.WZ_VerifiedEmpty == "Y"));
		}

		public void TestConfirmPickLinePickedQtyForVerifiedEmpty_No()
		{
			// setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			// setup receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var lines = pick.OrderedInventories[0].AvailableInventories[0].PickLines.ToArray();

			ConfirmPickLinesPickedQty(lines, new PickingInfo(5m, true), staff);
			Assert(lines.All(pl => pl.WZ_VerifiedEmpty == "N"));
		}

		#endregion

		#region TestConfirmPickLinePickedQty_AlreadyPicked

		public void TestConfirmPickLinePickedQty_AlreadyPicked()
		{
			TestConfirmPickLinePickedQty_AlreadyPicked_Core(usingInTransitTransfer: false);
		}

		public void TestConfirmPickLinePickedQty_AlreadyPicked_InTransit()
		{
			TestConfirmPickLinePickedQty_AlreadyPicked_Core(usingInTransitTransfer: true);
		}

		void TestConfirmPickLinePickedQty_AlreadyPicked_Core(bool usingInTransitTransfer)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.GetAllPickLines().ForEach(pl => pl.WZ_GS_NKAssignedTo = "ST1");

			var pickLine = orderLine1.PickLines.Single();
			if (usingInTransitTransfer)
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				Factory.Save();
			}
			else
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
				{
					Factory.Save();
					AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, orderLine1.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
				}
			}

			AssertExceptionThrown(typeof(ArgumentException), "Confirm Qty is greater than Requested Qty.",
				() => ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(10m, false), staff));
			AssertEquals("Should *not* have picked the unpicked pickline.", ZDateTimeOffset.Empty, orderLine2.PickLines[0].WZ_PickedDateTime);
		}

		public void TestConfirmPickLinePickedQty_PartiallyPicked_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);

			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_GS_NKAssignedTo = "ST1");

			var pickLine = orderLine1.PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(4m, false), staff);
			AssertEquals("Should have successfully Picked Pick Line.", true, orderLine2.PickLines.Single().IsPickedFromPutawayLocation);
		}

		#endregion

		#region TestClosePackage

		public void TestClosePackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			var response = new WebServiceResponse();
			PickLineUpdater.ClosePackage(package, response);
			AssertEquals("Package should be close", true, package.KP_ClosedTimeUtc.IsValid);
			AssertEquals(ErrorTypes.None, response.Error);
			Assert(response.ErrorMessage.IsNullOrEmpty());
		}

		public void TestClosePackage_NullPackage()
		{
			var response = new WebServiceResponse();
			AssertNoExceptionThrown(() => PickLineUpdater.ClosePackage(null, response));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("package cannot be null.", response.ErrorMessage);
		}

		public void TestClosePackage_ClosedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			var response = new WebServiceResponse();
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertNoExceptionThrown(() => PickLineUpdater.ClosePackage(package, response));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package with ID 'ABC' is already closed.", response.ErrorMessage);
		}

		#endregion

		#region TestClosePackageWithPrinter

		public void TestClosePackageWithPrinter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			var response = new WebServiceResponse();
			PickLineUpdater.ClosePackageWithPrinter(package, "PRINTER", response);
			AssertEquals("Package should be close.", true, package.KP_ClosedTimeUtc.IsValid);
			AssertEquals("PRINTER", package.PrinterUsedToPrintLabel);
			AssertEquals(ErrorTypes.None, response.Error);
			Assert(response.ErrorMessage.IsNullOrEmpty());
		}

		public void TestClosePackageWithPrinter_NoPrinterName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			var response = new WebServiceResponse();
			PickLineUpdater.ClosePackageWithPrinter(package, "", response);
			AssertEquals("Package should be close, although Printer Name is empty.", true, package.KP_ClosedTimeUtc.IsValid);
			AssertEquals("", package.PrinterUsedToPrintLabel);
			AssertEquals(ErrorTypes.None, response.Error);
			Assert(response.ErrorMessage.IsNullOrEmpty());
		}

		public void TestClosePackageWithPrinter_NullPackage()
		{
			var response = new WebServiceResponse();
			AssertNoExceptionThrown(() => PickLineUpdater.ClosePackageWithPrinter(null, "printer", response));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("package cannot be null.", response.ErrorMessage);
		}

		public void TestClosePackageWithPrinter_ClosedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			var response = new WebServiceResponse();
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertNoExceptionThrown(() => PickLineUpdater.ClosePackageWithPrinter(package, "printer", response));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package with ID 'ABC' is already closed.", response.ErrorMessage);
		}

		#endregion

		#region TestPackPickLines_ManyPickLines

		public void TestPackPickLines_ManyPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("BOX", "ABC");
			Factory.Save();

			var allPickLines = pick.GetAllPickLines().ToArray();
			var pickLinePKs = allPickLines.Select(p => p.PK.ToGuid()).ToArray();
			var releaseCapturedInfos1 = new[] { new WhsReleaseCapturedInfo("BLUE", "", "", "", 6), new WhsReleaseCapturedInfo("RED", "", "", "", 5) };
			orderLine1.ClearReleaseLines();
			orderLine2.ClearReleaseLines();

			PickLineUpdater.ConfirmPickLinesPickedQty(
				allPickLines,
				new PickingInfo(11, false, true),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("UNT", 11m, "", releaseCapturedInfos1) }) },
				package,
				false);

			var releaseLinesForOrderLine1 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().ToArray();
			Assert("Release lines quantity must not exceed order line quantity.", releaseLinesForOrderLine1.Sum(rl => rl.Quantity) <= orderLine1.WE_TransactionQuantity);
			Assert("Packed quantity must not exceed order line quantity.", releaseLinesForOrderLine1.Sum(rl => rl.GetPackedQty()) <= orderLine1.WE_TransactionQuantity);

			var releaseLinesForOrderLine2 = orderLine2.ReleaseLines.Cast<WhsReleaseLine>().ToArray();
			Assert("Release lines quantity must not exceed order line quantity.", releaseLinesForOrderLine2.Sum(rl => rl.Quantity) <= orderLine2.WE_TransactionQuantity);
			Assert("Packed quantity must not exceed order line quantity.", releaseLinesForOrderLine2.Sum(rl => rl.GetPackedQty()) <= orderLine2.WE_TransactionQuantity);

			var allReleaseLines = releaseLinesForOrderLine1.Concat(releaseLinesForOrderLine2).ToArray();
			AssertEquals("Should be 6 BLUE items release captured.", 6m, allReleaseLines.Where(rl => rl.PartAttribute1 == "BLUE").Sum(rl => rl.Quantity));
			AssertEquals("Should be 5 RED items release captured.", 5m, allReleaseLines.Where(rl => rl.PartAttribute1 == "RED").Sum(rl => rl.Quantity));
			AssertEquals("Should be 4 items without release captured items.", 4m, allReleaseLines.Where(rl => rl.PartAttribute1.IsEmpty).Sum(rl => rl.Quantity));

			var releaseCapturedAttributesForOrderLine1 = orderLine1.PickLines.ToArray();
			AssertEquals("Release Lines and Release Captured Attribute Quantities for the same Order Line should be the same.",
				releaseLinesForOrderLine1.Where(rl => !rl.PartAttribute1.IsEmpty).Sum(rl => rl.Quantity), releaseCapturedAttributesForOrderLine1.Sum(l => l.WZ_Units));

			var releaseCapturedAttributesForOrderLine2 = orderLine2.PickLines.ToArray();
			AssertEquals("Release Lines and Release Captured Attribute Quantities for the same Order Line should be the same.",
				releaseLinesForOrderLine2.Where(rl => !rl.PartAttribute1.IsEmpty).Sum(rl => rl.Quantity), releaseCapturedAttributesForOrderLine2.Where(l => !l.WZ_ReleaseCapturedPartAttrib1.IsEmpty).Sum(l => l.WZ_Units));

			var allReleaseCapturedAttributes = releaseCapturedAttributesForOrderLine1.Concat(releaseCapturedAttributesForOrderLine2).ToArray();
			AssertEquals("Should be 6 BLUE items release captured.", 6m, allReleaseCapturedAttributes.Where(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE").Sum(l => l.WZ_Units));
			AssertEquals("Should be 5 RED items release captured.", 5m, allReleaseCapturedAttributes.Where(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED").Sum(l => l.WZ_Units));
			AssertEquals("Should be 11 items release captured.", 11m, allReleaseCapturedAttributes.Where(l => !l.WZ_ReleaseCapturedPartAttrib1.IsEmpty).Sum(l => l.WZ_Units));
			AssertEquals("No Pick Line should be over released.", true, orderLine1.PickLines.Concat(orderLine2.PickLines).All(p => p.UnreleaseCapturedQty >= 0));
			AssertContainsExactElementsInAnyOrder(allReleaseCapturedAttributes.Where(l => !l.WZ_ReleaseCapturedPartAttrib1.IsEmpty), package.PackedItemDivots.Select(d => d.PackedItem));

			AssertEquals("Package should have correct Packed Qty.", 11m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertNoExceptionThrown(() => Factory.Save());

			// now pack the remaining 4 items
			orderLine1.ClearReleaseLines();
			orderLine2.ClearReleaseLines();
			var releaseCapturedInfos2 = new[] { new WhsReleaseCapturedInfo("RED", "", "", "", 2), new WhsReleaseCapturedInfo("GREEN", "", "", "", 2) };
			PickLineUpdater.ConfirmPickLinesPickedQty(
				allPickLines,
				new PickingInfo(4m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("UNT", 4m, "", releaseCapturedInfos2) }) },
				package,
				false);

			releaseLinesForOrderLine1 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().ToArray();
			Assert("Release lines quantity must not exceed order line quantity.", releaseLinesForOrderLine1.Sum(rl => rl.Quantity) <= orderLine1.WE_TransactionQuantity);
			Assert("Packed quantity must not exceed order line quantity.", releaseLinesForOrderLine1.Sum(rl => rl.GetPackedQty()) <= orderLine1.WE_TransactionQuantity);

			releaseLinesForOrderLine2 = orderLine2.ReleaseLines.Cast<WhsReleaseLine>().ToArray();
			Assert("Release lines quantity must not exceed order line quantity.", releaseLinesForOrderLine2.Sum(rl => rl.Quantity) <= orderLine2.WE_TransactionQuantity);
			Assert("Packed quantity must not exceed order line quantity.", releaseLinesForOrderLine2.Sum(rl => rl.GetPackedQty()) <= orderLine2.WE_TransactionQuantity);

			allReleaseLines = releaseLinesForOrderLine1.Concat(releaseLinesForOrderLine2).ToArray();

			AssertEquals("Should be 6 BLUE items release captured.", 6m, allReleaseLines.Where(rl => rl.PartAttribute1 == "BLUE").Sum(rl => rl.Quantity));
			AssertEquals("Should be 5+2 RED items release captured.", 7m, allReleaseLines.Where(rl => rl.PartAttribute1 == "RED").Sum(rl => rl.Quantity));
			AssertEquals("Should be 2 GREEN items release captured.", 2m, allReleaseLines.Where(rl => rl.PartAttribute1 == "GREEN").Sum(rl => rl.Quantity));
			AssertEquals("There should be no empty Release Lines.", 0m, allReleaseLines.Where(rl => rl.PartAttribute1.IsEmpty).Sum(rl => rl.Quantity));

			releaseCapturedAttributesForOrderLine1 = orderLine1.PickLines.ToArray();
			AssertEquals("Release Lines and Release Captured Attribute Quantities for the same Order Line should be the same.",
				releaseLinesForOrderLine1.Sum(rl => rl.Quantity), releaseCapturedAttributesForOrderLine1.Sum(l => l.WZ_Units));

			releaseCapturedAttributesForOrderLine2 = orderLine2.PickLines.ToArray();
			AssertEquals("Release Lines and Release Captured Attribute Quantities for the same Order Line should be the same.",
				releaseLinesForOrderLine2.Sum(rl => rl.Quantity), releaseCapturedAttributesForOrderLine2.Sum(l => l.WZ_Units));

			allReleaseCapturedAttributes = releaseCapturedAttributesForOrderLine1.Concat(releaseCapturedAttributesForOrderLine2).ToArray();
			AssertEquals("Should be 6 BLUE items release captured.", 6m, allReleaseCapturedAttributes.Where(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE").Sum(l => l.WZ_Units));
			AssertEquals("Should be 5+2 RED items release captured.", 7m, allReleaseCapturedAttributes.Where(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED").Sum(l => l.WZ_Units));
			AssertEquals("Should be 2 GREEN items release captured.", 2m, allReleaseCapturedAttributes.Where(l => l.WZ_ReleaseCapturedPartAttrib1 == "GREEN").Sum(l => l.WZ_Units));
			AssertEquals("Should be 15 items release captured.", 15m, allReleaseCapturedAttributes.Sum(l => l.WZ_Units));
			AssertEquals("No Pick Line should be over released.", true, orderLine1.PickLines.Concat(orderLine2.PickLines).All(p => p.UnreleaseCapturedQty >= 0));
			AssertContainsExactElementsInAnyOrder(allReleaseCapturedAttributes, package.PackedItemDivots.Select(d => d.PackedItem));

			AssertEquals("Package should have correct Packed Qty.", 15m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestPackPickLines_DoesNotChangeExistingPackedItemsOnPackage

		public void TestPackPickLines_DoesNotChangeExistingPackedItemsOnPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(order.PackableItemParents.Typed.Single(), 2m);
			AssertEquals("Precondition: Package is packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 2 units.", 2m, package.PackedItemDivots[0].KI_PackedQty);
			Factory.Save();

			var pickLine1 = order.Lines[0].PickLines.Single(pl => pl.WZ_Units == 2m);
			var pickLine2 = order.Lines[0].PickLines.Single(pl => pl.WZ_Units == 3m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine2 },
				new PickingInfo(3m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				package,
				false);
			AssertEquals("One new Packed Item Divot added.", 2, package.PackedItemDivots.Count);
			AssertEquals("Total Packed Qty is increased.", 5m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1, pickLine2 }, package.PackedItemDivots.Select(d => d.PackedItem));
		}

		#endregion

		#region TestPackPickLines_ChangesExistingPacking

		public void TestPackPickLines_ChangesExistingPacking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(order.PackableItemParents.Typed.Single(), 2m);
			AssertEquals("Precondition: Package is packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 2 units.", 2m, package.PackedItemDivots[0].KI_PackedQty);
			Factory.Save();

			var pickLine1 = order.Lines[0].PickLines.Single(pl => pl.WZ_Units == 2m);
			var pickLine2 = order.Lines[0].PickLines.Single(pl => pl.WZ_Units == 3m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1, pickLine2 },
				new PickingInfo(3m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				package,
				false);
			GlbStaff.CurrentUser.Factory.Save();
			AssertEquals("Should only have one new Packed Item Divot.", 1, package.PackedItemDivots.Count);
			AssertEquals("Total Packed Qty is increased.", 3m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, package.PackedItemDivots.Select(d => d.PackedItem));
		}

		public void TestPackPickLines_ChangesExistingPacking_PackingWithTheSamePickLineDoesNotThrowException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(order.PackableItemParents.Typed.Single(), 2m);
			AssertEquals("Precondition: Package is packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 2 units.", 2m, package.PackedItemDivots[0].KI_PackedQty);
			Factory.Save();

			var pickLine1 = order.Lines[0].PickLines.Single(pl => pl.WZ_Units == 2m);
			var pickLine2 = order.Lines[0].PickLines.Single(pl => pl.WZ_Units == 3m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1, pickLine2 },
				new PickingInfo(5m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				package,
				false);
			AssertEquals("No silent developer exception is reported", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Should have 2 Packed Item Divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Total Packed Qty is increased.", 5m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1, pickLine2 }, package.PackedItemDivots.Select(d => d.PackedItem));
		}

		#endregion

		#region TestPackPickLines_ChangesExistingPacking_WhenSplitting

		public void TestPackPickLines_ChangesExistingPacking_WhenSplitting()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(order.PackableItemParents.Typed.Single(), 2m);
			AssertEquals("Precondition: Package is packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 2 units.", 2m, package.PackedItemDivots[0].KI_PackedQty);
			Factory.Save();

			var pickLine1 = order.Lines[0].PickLines.Single(pl => pl.WZ_Units == 2m);
			var pickLine2 = order.Lines[0].PickLines.Single(pl => pl.WZ_Units == 3m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1, pickLine2 },
				new PickingInfo(4m, false, true),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				package,
				false);

			AssertEquals("One new Packed Item Divot added.", 2, package.PackedItemDivots.Count);
			AssertEquals("Total Packed Qty is increased.", 4m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2, order.Lines[0].PickLines.Single(pl => pl != pickLine1 && pl != pickLine2) }, package.PackedItemDivots.Select(d => d.PackedItem));
		}

		#endregion

		#region TestPickSuspendPickLine

		public void TestPickSuspendPickLine()
		{
			// setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			// setup receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].Allocate = true;
			var line = pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0);

			ConfirmPickLinesPickedQty(new[] { line }, new PickingInfo(2m, false, true), staff);
			AssertEquals("Should create new line.", 2, pick.GetAllPickLines().Count());

			var pickLine_picked = pick.GetAllPickLines().Single(l => l.IsPickedFromPutawayLocation);
			var pickLine_unpicked = pick.GetAllPickLines().Single(l => !l.IsPickedFromPutawayLocation);
			AssertEquals(2m, pickLine_picked.WZ_Units);
			AssertEquals(1m, pickLine_unpicked.WZ_Units);
		}

		#endregion

		#region TestConfirmPickLinesPickedQty_PickSuspendPickLine_IsPicking

		public void TestConfirmPickLinesPickedQty_PickSuspendPickLine_IsPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("G1", "U1");

			// setup receive, order and pick
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive", data.Part1, 3m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order", data.Part1, 3m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var pickline = pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0);
			pickline.WZ_GS_NKAssignedTo = "ABC";
			pickline.WZ_IsPicking = true;

			ConfirmPickLinesPickedQty(new[] { pickline }, new PickingInfo(2m, false, true), staff);
			AssertEquals("Should create new line.", 2, pick.GetAllPickLines().Count());

			var pickLine_picked = pick.GetAllPickLines().Single(l => l.IsPickedFromPutawayLocation && !l.WZ_IsPicking);
			var pickLine_unpicked = pick.GetAllPickLines().Single(l => !l.IsPickedFromPutawayLocation && l.WZ_IsPicking);
			AssertEquals(2m, pickLine_picked.WZ_Units);
			AssertEquals(1m, pickLine_unpicked.WZ_Units);
		}

		public void TestPickSuspendPickLine_ReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition: There should be 1 release line.", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Before specify the release attribute only one pick line to pick 8 items", 1, pick.GetAllPickLines().Count());

			orderLine1.ClearReleaseLines();
			var pickLine = pick.GetAllPickLines().Single();
			var releaseCapturedInfo = new[]
				{
					new WhsReleaseCapturedInfo("BLUE", "", "", "", 6),
					new WhsReleaseCapturedInfo("RED", "", "", "", 2)
				};

			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(8m, false, true),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 8m, "", releaseCapturedInfo) }) },
				null,
				false);
			var releaseLines = orderLine1.ReleaseLines.ToArray<WhsReleaseLine>();
			AssertEquals("It should split pick line to 3 lines one for 6 Red, one for 2 Blue and one for 2 remaining items", 3, releaseLines.Length);
			AssertNotNull(releaseLines.Single(a => a.PartAttribute1 == "BLUE" && a.Quantity == 6));
			AssertNotNull(releaseLines.Single(a => a.PartAttribute1 == "RED" && a.Quantity == 2));
			AssertNotNull(releaseLines.Single(a => a.PartAttribute1 == "" && a.Quantity == 2));
			AssertEquals("We have 3 PickLines", 3, pick.GetAllPickLines().Count());
		}

		#endregion

		#region TestPackPickLines_WhenFullyPacked

		public void TestPackPickLines_WhenFullyPacked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(order.PackableItemParents.Typed.Single(), 5m);
			AssertEquals("Precondition: Package is packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 5 units.", 5m, package.PackedItemDivots[0].KI_PackedQty);
			Factory.Save();

			var pickLine = pick.GetAllPickLines().Single();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(1m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				package,
				false);
			AssertEquals("PickLine is not over packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("PickLine is not over packed.", 1m, package.PackedItemDivots[0].KI_PackedQty);
		}

		#endregion

		#region TestPackPickLines_PackedInAnotherPackage

		public void TestPackPickLines_PackedInAnotherPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("PLT", "123");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "XYZ");
			package1.Pack(order.PackableItemParents.Typed.Single(), 5m);
			AssertEquals("Precondition: Package is packed.", 1, package1.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 5 units.", 5m, package1.PackedItemDivots[0].KI_PackedQty);
			Factory.Save();

			var pickLine = pick.GetAllPickLines().Single();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine }, new PickingInfo(1m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				package2,
				false);
			AssertEquals("Package Divot is removed.", 0, package1.PackedItemDivots.Count);
			AssertEquals("Other Package is packed.", 1, package2.PackedItemDivots.Count);
			AssertEquals("Pickline is not over packed.", 1m, package2.PackedItemDivots[0].KI_PackedQty);
		}

		#endregion

		#region TestPackPickLines_PrePacked

		public void TestPackPickLines_PrePacked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(order.PackableItemParents.Typed.Single(), 5m);
			AssertEquals("Precondition: Package is packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 5 units.", 5m, package.PackedItemDivots[0].KI_PackedQty);
			Factory.Save();

			var pickLine = pick.GetAllPickLines().Single();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(1m, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				null,
				false);
			AssertEquals("Package is not over packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("Package is not over packed.", 1m, package.PackedItemDivots[0].KI_PackedQty);
		}

		#endregion

		#region TestPackPickLines_PrePacked_NonPickedPickLines

		public void TestPackPickLines_PrePacked_NonPickedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(order.PackableItemParents.Typed.Single(), 2m);
			AssertEquals("Precondition: Package is packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 2 units.", 2m, package.PackedItemDivots[0].KI_PackedQty);

			var pickLine1 = order.Lines[0].PickLines.Single(pl => pl.IsUnpacked(Factory));
			var pickLine2 = order.Lines[0].PickLines.Single(pl => !pl.IsUnpacked(Factory));
			AssertEquals("Precondition: Package is packed with 2 units.", pickLine2, package.PackedItemDivots[0].PackedItem);
			Factory.Save();

			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1, pickLine2 },
				new PickingInfo(3m, false, true),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				null,
				false);
			AssertEquals("Package is not over packed.", 1, package.PackedItemDivots.Count);
			AssertEquals("Pick Line is not over packed.", 2m, package.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("Correct Pick Line is packed.", pickLine2, package.PackedItemDivots[0].PackedItem);
		}

		#endregion

		#region TestPackPickLines_PrePacked_PreReleaseCaptured

		public void TestPackPickLines_PrePacked_PreReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine1 = order.Lines[0].ReleaseLines[0];
			releaseLine1.Quantity = 3m;
			releaseLine1.PartAttribute1 = "RED";

			var releaseLine2 = order.Lines[0].ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "BLUE";
			releaseLine2.Quantity = 2m;

			var package1 = order.PackageJob.Packages.AddNew("PLT", "123");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "XYZ");
			package1.Pack(releaseLine1, 3m);
			package2.Pack(releaseLine2, 2m);
			AssertEquals("Precondition: Package is packed.", 1, package1.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed.", 1, package2.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 3 units.", 3m, package1.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("Precondition: Package is packed with 2 units.", 2m, package2.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("Precondition: 2 picklines.", 2, order.Lines[0].PickLines.Count);

			Factory.Save();

			var releaseCapturedInfos = new[] { new WhsReleaseCapturedInfo { Attribute1 = "GREEN", Quantity = 3m }, new WhsReleaseCapturedInfo { Attribute1 = "YELLOW", Quantity = 2m } };
			order.Lines[0].ClearReleaseLines();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				pick.GetAllPickLines().ToArray(),
				new PickingInfo(5m, false),
				Factory.NewWithValidTestData<GlbStaff>(),
				new[] { new PickLinesToPickedPackTypeInfo(pick.GetAllPickLines().Select(l => l.PK.ToGuid()).ToArray(), new[] { new PickedPackTypeInfo("UNT", 5m, "", releaseCapturedInfos) }) },
				null,
				true);
			AssertEquals("Package is not over packed.", 1, package1.PackedItemDivots.Count);
			AssertEquals("Package is not over packed.", 1, package2.PackedItemDivots.Count);
			AssertEquals("Package is not over packed.", 3m, package1.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("Package is not over packed.", 2m, package2.PackedItemDivots[0].KI_PackedQty);
		}

		#endregion

		#region TestPackPickLines_PrePacked_ReleaseCaptureOnRF

		public void TestPackPickLines_PrePacked_ReleaseCaptureOnRF()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = order.Lines[0].ReleaseLines[0];

			var package1 = order.PackageJob.Packages.AddNew("PLT", "123");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "XYZ");
			package1.Pack(releaseLine, 3m);
			package2.Pack(releaseLine, 2m);
			AssertEquals("Precondition: Package is packed.", 1, package1.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed.", 1, package2.PackedItemDivots.Count);
			AssertEquals("Precondition: Package is packed with 3 units.", 3m, package1.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("Precondition: Package is packed with 2 units.", 2m, package2.PackedItemDivots[0].KI_PackedQty);
			Factory.Save();

			order.Lines[0].ClearReleaseLines();
			var releaseCapturedInfos = new[]
			{
				new WhsReleaseCapturedInfo { Attribute1 = "SN1", Quantity = 1m },
				new WhsReleaseCapturedInfo { Attribute1 = "SN2", Quantity = 1m },
				new WhsReleaseCapturedInfo { Attribute1 = "SN3", Quantity = 1m },
				new WhsReleaseCapturedInfo { Attribute1 = "SN4", Quantity = 1m },
				new WhsReleaseCapturedInfo { Attribute1 = "SN5", Quantity = 1m },
			};

			var pickLines = order.Lines[0].PickLines.ToArray();
			var pickLinePKs = pickLines.Select(p => p.PK.ToGuid()).ToArray();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				pickLines,
				new PickingInfo(5m, false),
				Factory.NewWithValidTestData<GlbStaff>(),
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("UNT", 5m, "", releaseCapturedInfos) }) },
				null,
				false);
			AssertEquals("Package is not over packed.", 3m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Package is not over packed.", 3m, package1.PackedItemDivots.Select(d => d.PackedItem).Sum(d => d.Quantity));
			AssertEquals("Package is not over packed.", 2m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Package is not over packed.", 2m, package2.PackedItemDivots.Select(d => d.PackedItem).Sum(d => d.Quantity));
			AssertEquals("Release Captured Attribs have correct sum.", 5m, order.Lines[0].PickLines.Where(l => l.HasReleaseCapturedAttribs).Sum(l => l.WZ_Units));
		}

		#endregion

		#region TestPackPickLines_QuantityToPackIsTooBig

		public void TestPackPickLines_QuantityToPackIsTooBig()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 0m;
			data.Part1.OP_Cubic = 0m;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 999999.001m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 999999.001m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = order.Lines[0].ReleaseLines[0];

			var package1 = order.PackageJob.Packages.AddNew("PLT", "123");
			Factory.Save();

			var pickLines = order.Lines[0].PickLines.ToArray();

			AssertExceptionThrown(typeof(ArgumentException), "Packing failed, cannot pack more than 999999 units.",
				() => PickLineUpdater.ConfirmPickLinesPickedQty(
				pickLines,
				new PickingInfo(999999.001m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLines[0].PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				package1,
				false));
			AssertEquals("Nothing packed.", 0m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
		}

		#endregion

		#region TestPackPickLines_QuantityToPack

		public void TestPackPickLines_QuantityToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 0m;
			data.Part1.OP_Cubic = 0m;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 999999m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 999999m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = order.Lines[0].ReleaseLines[0];

			var package1 = order.PackageJob.Packages.AddNew("PLT", "123");
			Factory.Save();

			var pickLines = order.Lines[0].PickLines.ToArray();

			AssertNoExceptionThrown(() => PickLineUpdater.ConfirmPickLinesPickedQty(
				pickLines,
				new PickingInfo(999999m, false),
				GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLines[0].PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				package1,
				false));
			AssertEquals("Packed successfully.", 999999m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
		}

		#endregion

		#region TestPackPickLines_QuantityToPack_MultipleOrdersAndPackages

		public void TestPackPickLines_QuantityToPack_MultipleOrdersAndPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var picker = Helper.CreateGlbStaff("PPP", "Pete");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();
			var pickLine3 = pickLine2.Split(9m);

			var releaseLine1 = order1.Lines[0].ReleaseLines[0];
			var releaseLine2 = order2.Lines[0].ReleaseLines[0];

			var package1 = order1.PackageJob.Packages.AddNew("PLT", "123");
			package1.Pack(pickLine1, releaseLine1);
			var package2 = order2.PackageJob.Packages.AddNew("PLT", "XYZ");
			package2.Pack(pickLine2, releaseLine2);
			Factory.Save();

			AssertEquals(5m, pickLine1.WZ_Units);
			AssertEquals(11m, pickLine2.WZ_Units);
			AssertEquals(9m, pickLine3.WZ_Units);

			// picked 20m so that pickLine2 & pickLine3 will be fullfilled, while pickLine1 will be deleted.
			AssertNoExceptionThrown(() => PickLineUpdater.ConfirmPickLinesPickedQty(
				new WhsPickLine[] { pickLine1, pickLine2, pickLine3 },
				new PickingInfo(20m, false),
				picker,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid(), pickLine3.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				null,
				false));

			AssertNoExceptionThrown(Factory.Save);

			AssertEquals(true, pickLine1.IsDeleted);
			AssertEquals(11m, pickLine2.WZ_Units);
			AssertEquals(true, pickLine2.IsPickedFromPutawayLocation);
			AssertEquals(9m, pickLine3.WZ_Units);
			AssertEquals(true, pickLine3.IsPickedFromPutawayLocation);

			AssertEquals("Was removed by shorting.", 0, package1.PackedItemDivots.Count);

			AssertEquals(1, package2.PackedItemDivots.Count);
			AssertEquals(pickLine2.PK, package2.PackedItemDivots[0].KI_ParentID);
			AssertEquals(11m, package2.PackedItemDivots[0].KI_PackedQty);
		}

		#endregion

		#region TestPackPickLines_QuantityToPack_MultipleOrdersAndPackages_PickByBOM

		public void TestPackPickLines_QuantityToPack_MultipleOrdersAndPackages_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var picker = Helper.CreateGlbStaff("PPP", "Pete");
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 200m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var kitOrder1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 5m);
			var kitOrder2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT2", bike, 20m);
			var pick = Helper.CreatePickNew(kitOrder1, kitOrder2);

			var kitOrderLine1 = (WhsOrderLine)kitOrder1.Lines.Single(l => l.WE_OP == bike.PK);
			var wheelOrderLine1 = (WhsOrderLine)kitOrder1.Lines.Single(l => l.WE_OP == wheel.PK);
			var kitOrderLine2 = (WhsOrderLine)kitOrder2.Lines.Single(l => l.WE_OP == bike.PK);
			var wheelOrderLine2 = (WhsOrderLine)kitOrder2.Lines.Single(l => l.WE_OP == wheel.PK);

			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var kitPickLine2 = kitOrderLine2.PickLines.Single();
			var kitPickLine3 = kitPickLine2.Split(9m);

			var wheelPickLine1 = wheelOrderLine1.PickLines.Single();
			var wheelPickLine2 = wheelOrderLine2.PickLines.Single();

			Factory.Save();

			var releaseLine1 = kitOrderLine1.ReleaseLines[0];
			var releaseLine2 = kitOrderLine2.ReleaseLines[0];

			var package1 = kitOrder1.PackageJob.Packages.AddNew("PLT", "123");
			package1.Pack(kitPickLine1, releaseLine1);
			var package2 = kitOrder2.PackageJob.Packages.AddNew("PLT", "XYZ");
			package2.Pack(kitPickLine2, releaseLine2);
			Factory.Save();

			AssertEquals(5m, kitPickLine1.WZ_Units);
			AssertEquals(11m, kitPickLine2.WZ_Units);
			AssertEquals(9m, kitPickLine3.WZ_Units);

			// picked 38m wheels so that part of kitPickLine2 & kitPickLine3 will be fullfilled, while kitPickLine1 will be deleted.
			AssertNoExceptionThrown(() => PickLineUpdater.ConfirmPickLinesPickedQty(
				new WhsPickLine[] { wheelPickLine1, wheelPickLine2 },
				new PickingInfo(38m, false),
				picker,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { wheelPickLine1.PK.ToGuid(), wheelPickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				null,
				false));

			AssertNoExceptionThrown(Factory.Save);

			AssertEquals("All components on kitOrder1 are shorted, so as the kits.", true, kitPickLine1.IsDeleted);
			AssertEquals("When there are multiple Kit Pick Lines, we keep the first and delete others.", true, kitPickLine2.IsDeleted || kitPickLine3.IsDeleted);
			AssertEquals("Although we deleted 1 Kit Pick Line, a new one will be split from the remaining one from repacking.", 2, kitOrderLine2.PickLines.Count);

			kitPickLine2 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 11m);
			kitPickLine3 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 8m);

			AssertEquals("Kit Pick Lines won't be picked until putaway.", false, kitPickLine2.IsPickedFromPutawayLocation);
			AssertEquals("Kit Pick Lines won't be picked until putaway.", false, kitPickLine3.IsPickedFromPutawayLocation);

			AssertEquals("Was removed by shorting.", 0, package1.PackedItemDivots.Count);

			AssertEquals(1, package2.PackedItemDivots.Count);
			AssertEquals(kitPickLine2.PK, package2.PackedItemDivots[0].KI_ParentID);
			AssertEquals(11m, package2.PackedItemDivots[0].KI_PackedQty);
		}

		#endregion

		#region TestShortpickPickLine

		public void TestShortpickPickLine()
		{
			// setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			// setup receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: There should be 0 units sent in the order.", 0m, order.WD_UnitsSent);

			pick.OrderedInventories[0].AvailableInventories[0].Allocate = true;
			AssertEquals("Precondition: There should be 3 units sent in the order.", 3m, order.WD_UnitsSent);
			var line = pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0);

			var poke = ((WhsOrderLine)line.DocketLine).ReleaseLines;
			var releaseLine = ((WhsOrderLine)line.DocketLine).ReleaseLines;
			AssertEquals("Precondition: There should be one attribute met line.", 1, releaseLine.Count);
			Factory.Save();

			ConfirmPickLinesPickedQty(new[] { line }, new PickingInfo(2m, false), staff);
			AssertEquals("There should be two attribute met lines.", 1, releaseLine.Count);
			((WhsOrderLine)line.DocketLine).ReleaseLines.Cast<WhsReleaseLine>().Single(a => a.PK == releaseLine[0].PK && a.Quantity == 2);

			AssertEquals("There should be 3 units required in the docket.", 3m, ((WhsOrderLine)line.DocketLine).Docket.WD_TotalUnitsFromLines);
			AssertEquals("There should be 2 units sent in the docket.", 2m, ((WhsOrderLine)line.DocketLine).Docket.WD_UnitsSent);
		}

		#endregion

		#region TestShortPickPickLine_MultipleAttributeMetLines

		public void TestShortPickPickLine_MultipleAttributeMetLines()
		{
			// setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("G1", "U1");

			// setup receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			var inventoryForPart1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventoryForPart2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order1OrderLine1ForPart1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order1OrderLine2ForPart1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			var order2OrderLine1ForPart1 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var order2OrderLine2ForPart2 = Helper.CreateWhsOrderLine(order2, data.Part2, 2m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = Helper.CreateWhsPickLine(order1OrderLine1ForPart1, inventoryForPart1, 6m);
			ReleaseLinesOnPickManager.NotifyChange(pickLine1.Factory, pickLine1, 6);
			var pickLine2 = Helper.CreateWhsPickLine(order1OrderLine2ForPart1, inventoryForPart1, 1m);
			ReleaseLinesOnPickManager.NotifyChange(pickLine2.Factory, pickLine2, 1);
			var pickLine3 = Helper.CreateWhsPickLine(order2OrderLine1ForPart1, inventoryForPart1, 3m);
			ReleaseLinesOnPickManager.NotifyChange(pickLine3.Factory, pickLine3, 3);

			AssertEquals("There should be 6 units in pickline 1.", 6m, pickLine1.WZ_Units);
			AssertEquals("There should be 1 units in pickline 2.", 1m, pickLine2.WZ_Units);
			AssertEquals("There should be 3 units in pickline 3.", 3m, pickLine3.WZ_Units);
			AssertEquals("There should be 7 units sent in order 1.", 7m, order1.WD_UnitsSent);

			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(i => i.SupplierPart == data.Part1);
			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(i => i.SupplierPart == data.Part2);
			var availableInventory1 = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var availableInventory2 = orderedInventory2.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

			AssertEquals("Precondition: There should be 0 picklines in orderline 2.", 0, order2OrderLine2ForPart2.PickLines.Count);
			availableInventory2.Allocate = true;
			AssertEquals("There should now be 1 pickline allocated to orderline 2.", 1, order2OrderLine2ForPart2.PickLines.Count);

			var pickLine4 = order2OrderLine2ForPart2.PickLines[0];
			AssertEquals("There should be 2 units in pickline 4.", 2m, pickLine4.WZ_Units);
			AssertEquals("There should be 5 units sent in order 2.", 5m, order2.WD_UnitsSent);

			orderedInventory1.ClearPickLinesCache();
			Factory.Save();

			AssertEquals("Precondition", 10m, orderedInventory1.PickLineQuantity);

			AssertEquals("Precondition", 2m, orderedInventory2.PickLineQuantity);

			ConfirmPickLinesPickedQty(new[] { pickLine1 }, new PickingInfo(1m, false), staff);
			ConfirmPickLinesPickedQty(new[] { pickLine2 }, new PickingInfo(0m, false), staff);
			ConfirmPickLinesPickedQty(new[] { pickLine3 }, new PickingInfo(0m, false), staff);
			ConfirmPickLinesPickedQty(new[] { pickLine4 }, new PickingInfo(2m, false), staff);

			AssertEquals("Quantity picked should be 1 since pick lines are short picked.", 1m, orderedInventory1.PickLineQuantity);

			AssertEquals("For part2 Quantity picked should not be changed.", 2m, orderedInventory2.PickLineQuantity);

			AssertEquals("There should be 1 units sent in order 1.", 1m, order1.WD_UnitsSent);
			AssertEquals("There should be 2 units sent in order 2.", 2m, order2.WD_UnitsSent);
		}

		#endregion

		#region TestShortPickPickLine_ReducesAppropriateReleaseLines

		public void TestShortPickPickLine_ReducesAppropriateReleaseLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m).WI_PartAttrib2 = "RED";
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m).WI_PartAttrib2 = "BLUE";
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m).WI_PartAttrib2 = "BLACK";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var pick = Helper.CreatePickNew(false, false, order);

			var pickLine1 = pick.GetAllPickLines().Single(l => l.InventoryLine.WE_PartAttrib2 == "RED");
			var pickLine2 = pick.GetAllPickLines().Single(l => l.InventoryLine.WE_PartAttrib2 == "BLUE");
			var pickLine3 = pick.GetAllPickLines().Single(l => l.InventoryLine.WE_PartAttrib2 == "BLACK");

			var releaseLines = order.Lines[0].ReleaseLines.ToArray<WhsReleaseLine>();
			AssertEquals("Precondition: There should be 3 release lines.", 3, releaseLines.Length);
			AssertEquals(1, releaseLines.Count(a => a.PartAttribute2 == "RED"));
			AssertEquals(1, releaseLines.Count(a => a.PartAttribute2 == "BLUE"));
			AssertEquals(1, releaseLines.Count(a => a.PartAttribute2 == "BLACK"));
			AssertEquals("Precondition: There should be 6 units sent in the order.", 6m, order.WD_UnitsSent);

			// confirm 1st line
			order.Lines[0].ClearReleaseLines();
			ConfirmPickLinesPickedQty(new[] { pickLine1 }, new PickingInfo(2m, false), staff);
			Assert("First pickline should be picked", pickLine1.IsPickedFromPutawayLocation);
			Assert("Second pickline should not be picked", !pickLine2.IsPickedFromPutawayLocation);
			Assert("Third pickline should not be picked", !pickLine3.IsPickedFromPutawayLocation);

			releaseLines = order.Lines[0].ReleaseLines.ToArray<WhsReleaseLine>();
			AssertEquals("There are still 3 release lines.", 3, releaseLines.Length);
			AssertEquals("There are still 6 units sent in the order.", 6m, order.WD_UnitsSent);

			//short pick 2nd line by one unit
			order.Lines[0].ClearReleaseLines();
			ConfirmPickLinesPickedQty(new[] { pickLine2 }, new PickingInfo(1m, false), staff);
			Assert("First pickline should be picked", pickLine1.IsPickedFromPutawayLocation);
			Assert("Second pickline should be picked", pickLine2.IsPickedFromPutawayLocation);
			Assert("Third pickline should not be picked", !pickLine3.IsPickedFromPutawayLocation);

			releaseLines = order.Lines[0].ReleaseLines.ToArray<WhsReleaseLine>();
			AssertEquals("There should be 3 attribute met lines", 3, releaseLines.Length);
			AssertEquals("Release line for RED should stay as it was already confirmed", 1, releaseLines.Count(a => a.PartAttribute2 == "RED" && a.Quantity == 2));
			AssertEquals("Release line for BLUE should stay with updated quantity", 1, releaseLines.Count(a => a.PartAttribute2 == "BLUE" && a.Quantity == 1));
			AssertEquals("Release line for BLACK should stay as it was not YET confirmed", 1, releaseLines.Count(a => a.PartAttribute2 == "BLACK" && a.Quantity == 2));
			AssertEquals("There are now 5 units sent in the order.", 5m, order.WD_UnitsSent);

			//short pick 3rd line completely
			order.Lines[0].ClearReleaseLines();
			ConfirmPickLinesPickedQty(new[] { pickLine3 }, new PickingInfo(0m, false), staff);
			Assert("First pickline should be picked", pickLine1.IsPickedFromPutawayLocation);
			Assert("Second pickline should be picked", pickLine2.IsPickedFromPutawayLocation);
			Assert("Third pick line should be deleted because of short pick", pickLine3.IsDeleted);

			releaseLines = order.Lines[0].ReleaseLines.ToArray<WhsReleaseLine>();
			AssertEquals("There should be 2 attribute met lines", 2, releaseLines.Length);
			AssertEquals("Release line for RED should stay as it was already confirmed", 1, releaseLines.Count(a => a.PartAttribute2 == "RED" && a.Quantity == 2));
			AssertEquals("Release line for BLUE should stay with 1 unit as it was already confirmed", 1, releaseLines.Count(a => a.PartAttribute2 == "BLUE" && a.Quantity == 1));
			AssertEquals("Release line for BLACK should be deleted as it was short picked completely", 0, releaseLines.Count(a => a.PartAttribute2 == "BLACK"));

			AssertEquals("There are now 3 units sent in the order.", 3m, order.WD_UnitsSent);
		}

		#endregion

		#region TestShortPick_WillNotReduceReleaseLinesWithReleaseCapturedInfo

		public void TestShortPick_WillNotReduceReleaseLinesWithReleaseCapturedInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(false, false, order);
			AssertEquals("Precondition: There are 4 units sent in the order.", 4m, order.WD_UnitsSent);

			var releaseCaptureAttribute = new WhsReleaseCapturedInfo { Attribute2 = "RED", Quantity = 2 };

			orderLine.ClearReleaseLines();
			var picklines = pick.GetAllPickLines().ToArray();
			var pickLine1 = picklines[0];
			var pickLine2 = picklines[1];
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1 },
				new PickingInfo(2m, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 2, "", new[] { releaseCaptureAttribute }) }) },
				null,
				true);
			Assert("First pickline should be picked.", pickLine1.IsPickedFromPutawayLocation);
			Assert("Second pickline should not be picked.", !pickLine2.IsPickedFromPutawayLocation);
			AssertEquals("There are still 4 units sent in the order.", 4m, order.WD_UnitsSent);

			var releaseLines = orderLine.ReleaseLines.ToArray<WhsReleaseLine>();
			AssertEquals("There should be 2 release lines.", 2, releaseLines.Length);
			AssertEquals("One release line with captured attribute.", 1, releaseLines.Count(a => a.PartAttribute2 == "RED" && a.Quantity == 2));
			AssertEquals("Another release line with no attributes.", 1, releaseLines.Count(a => a.PartAttribute2 == "" && a.Quantity == 2));

			AssertEquals("2 PickLines", 2, orderLine.PickLines.Count);
			AssertEquals("1 red pickline", 2m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED").WZ_Units);
			AssertEquals("1 empty pickline", 2m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "").WZ_Units);

			orderLine.ClearReleaseLines();
			ConfirmPickLinesPickedQty(new[] { pickLine2 }, new PickingInfo(0m, false), staff);
			Assert("First pickline should be picked.", pickLine1.IsPickedFromPutawayLocation);
			Assert("Second pickline should be deleted.", pickLine2.IsDeleted);
			AssertEquals("There are now 2 units sent in the order.", 2m, order.WD_UnitsSent);

			releaseLines = orderLine.ReleaseLines.ToArray<WhsReleaseLine>();
			AssertEquals("There should be 1 release line.", 1, releaseLines.Length);
			AssertEquals("Release line with release captured info should stay as it was already confirmed.", 1, releaseLines.Count(a => a.PartAttribute2 == "RED" && a.Quantity == 2));
			AssertEquals("1 PickLine.", 1, orderLine.PickLines.Count);
			AssertEquals("1 red pickline", 2m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED").WZ_Units);
		}

		#endregion

		#region TestShortPickPickLine_PickByBOM_WithPrePackedLine

		public void TestShortPickPickLine_PickByBOM_WithPrePackedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var packingHelper = new PackingTestHelper(Factory);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 10m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 10m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();
			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = Helper.CreatePickNew(kitOrder);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var wheelPickLine1 = pick.GetAllPickLines().Single(l => l.WZ_Units == 10m && l.WZ_WE_InventoryLine == receiveLine1.PK);
			var wheelPickLine2 = pick.GetAllPickLines().Single(l => l.WZ_Units == 10m && l.WZ_WE_InventoryLine == receiveLine2.PK);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, kitPickLine1);

			ConfirmPickLinesPickedQty(new[] { wheelPickLine1 }, new PickingInfo(2m, false), staff);
			AssertNoExceptionThrown(Factory.Save);

			AssertEquals(1, kitOrderLine1.PickLines.Count);
			AssertEquals(6m, kitPickLine1.WZ_Units);

			AssertEquals("1 line packed.", 1, pkg1.PackedItemDivots.Count);
			var divot = pkg1.PackedItemDivots[0];
			AssertEquals("1 line packed.", kitPickLine1.PK, divot.KI_ParentID);
			AssertEquals("1 line packed.", 6m, divot.KI_PackedQty);
		}

		public void TestShortPickPickLine_PickByBOM_WithPrePackedLine_MultipleComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var packingHelper = new PackingTestHelper(Factory);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 10m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, frame, 4m, location2);
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, frame, 1m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();
			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 5m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = Helper.CreatePickNew(kitOrder);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var wheelPickLine = pick.GetAllPickLines().Single(l => l.WZ_Units == 10m && l.WZ_WE_InventoryLine == receiveLine1.PK);
			var framePickLine1 = pick.GetAllPickLines().Single(l => l.WZ_Units == 4m && l.WZ_WE_InventoryLine == receiveLine2.PK);
			var framePickLine2 = pick.GetAllPickLines().Single(l => l.WZ_Units == 1m && l.WZ_WE_InventoryLine == receiveLine3.PK);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, kitPickLine1);

			ConfirmPickLinesPickedQty(new[] { wheelPickLine }, new PickingInfo(10m, false), staff);
			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(4m, false), staff);
			AssertNoExceptionThrown(Factory.Save);

			AssertEquals(1, kitOrderLine1.PickLines.Count);
			AssertEquals(4m, kitPickLine1.WZ_Units);

			AssertEquals("1 line packed.", 1, pkg1.PackedItemDivots.Count);
			var divot = pkg1.PackedItemDivots[0];
			AssertEquals("1 line packed.", kitPickLine1.PK, divot.KI_ParentID);
			AssertEquals("1 line packed.", 4m, divot.KI_PackedQty);
		}

		#endregion

		#region TestConfirmPickLinePickedQtyShortPick

		public void TestConfirmPickLinePickedQtyShortPick()
		{
			AssertConfirmPickLinePickedQtyShortPick(false);
		}

		public void TestConfirmPickLinePickedQtyShortPick_WithUOMPackTypes(bool uomPackType)
		{
			AssertConfirmPickLinePickedQtyShortPick(true);
		}

		void AssertConfirmPickLinePickedQtyShortPick(bool uomPackType)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = uomPackType;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part2, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("At this point PickByUOM enabled as it is taken from warehouse settings", uomPackType, pick.IsPickByUOMEnabled);
			AssertPickLineForQty(pick, data.Part1, staff, 0m, true);
			AssertPickLineForQty(pick, data.Part2, staff, 1m, false);
		}

		void AssertPickLineForQty(WhsPick pick, OrgSupplierPart part, GlbStaff staff, decimal confirmedQty, bool isDeleted)
		{
			var avlInvLineForPickQtyGreaterThanZero = (WhsPickAvailableInventory)pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == part).AvailableInventories.Single();
			var pickLineforQtyGreaterThanZero = avlInvLineForPickQtyGreaterThanZero.PickLines.Single();
			ConfirmPickLinesPickedQty(new[] { pickLineforQtyGreaterThanZero }, new PickingInfo(confirmedQty, false), staff);
			AssertEquals(isDeleted, pickLineforQtyGreaterThanZero.IsDeleted);
		}

		#endregion

		#region TestConfirmPickLinesPickedQty_ManyPickLines

		public void TestConfirmPickLinesPickedQty_ManyPickLines()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 11m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(l => l.WZ_Units == 9);
			var pickLine2 = pick.GetAllPickLines().Single(l => l.WZ_Units == 2);

			ConfirmPickLinesPickedQty(pick.GetAllPickLines().ToArray(), new PickingInfo(6m, false), staff);

			AssertEquals("First pickline quantity should be updated.", 6m, pickLine1.WZ_Units);
			Assert("Second pickline should be deleted.", pickLine2.IsDeleted);
		}

		#endregion

		#region TestConfirmPickLinesPickedQty_ManyLinesAndReleaseCapture

		public void TestConfirmPickLinesPickedQty_ManyLinesAndReleaseCapture()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals(15m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			orderLine1.ClearReleaseLines();
			orderLine2.ClearReleaseLines();

			var releaseCapturedInfos = new[]
			{
				new WhsReleaseCapturedInfo { Attribute2 = "BLUE", Quantity = 5 },
				new WhsReleaseCapturedInfo { Attribute2 = "RED", Quantity = 6 }
			};
			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLinePKs = pickLines.Select(p => p.PK.ToGuid()).ToArray();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				pickLines,
				new PickingInfo(11m, false), staff,
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("UNT", 11m, "", releaseCapturedInfos) }) },
				null,
				true);

			AssertEquals("New sum of picklines must be 11.", 11m, new[] { orderLine1, orderLine2 }.SelectMany(l => l.PickLines).Sum(l => l.WZ_Units));

			var orderLine1ReleasedSum = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Sum(rl => rl.Quantity);
			var orderLine2ReleasedSum = orderLine2.ReleaseLines.Cast<WhsReleaseLine>().Sum(rl => rl.Quantity);

			var orderLine1ReleaseCapturedSum = orderLine1.PickLines.Sum(l => l.WZ_Units);
			var orderLine2ReleaseCapturedSum = orderLine2.PickLines.Sum(l => l.WZ_Units);
			AssertEquals("Release units sum quantity should match sum of pickline quantity.", 11m, orderLine1ReleasedSum + orderLine2ReleasedSum);
			AssertEquals(true, orderLine1ReleasedSum <= orderLine1.WE_TransactionQuantity);
			AssertEquals(true, orderLine2ReleasedSum <= orderLine2.WE_TransactionQuantity);
			AssertEquals("Release Lines and Release Captured quantities should match for the same OrderLine.", orderLine1ReleasedSum, orderLine1ReleaseCapturedSum);
			AssertEquals("Release Lines and Release Captured quantities should match for the same OrderLine.", orderLine2ReleasedSum, orderLine2ReleaseCapturedSum);

			var allReleaseLines = new[] { orderLine1, orderLine2 }.SelectMany(ol => ol.ReleaseLines.Cast<WhsReleaseLine>()).ToArray();
			AssertEquals(5m, allReleaseLines.Where(rl => rl.PartAttribute2 == "BLUE").Sum(rl => rl.Quantity));
			AssertEquals(6m, allReleaseLines.Where(rl => rl.PartAttribute2 == "RED").Sum(rl => rl.Quantity));

			var allPickLines = new[] { orderLine1, orderLine2 }.SelectMany(ol => ol.PickLines).ToArray();
			AssertEquals(5m, allPickLines.Where(l => l.WZ_ReleaseCapturedPartAttrib2 == "BLUE").Sum(l => l.WZ_Units));
			AssertEquals(6m, allPickLines.Where(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED").Sum(l => l.WZ_Units));
		}

		#endregion

		#region TestConfirmPickLinesPickedQty_NotBlowsUpForWorkOrderReleaseCapturedComponents

		public void TestConfirmPickLinesPickedQty_NotBlowsUpForWorkOrderReleaseCapturedComponents()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Colour");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 3m);
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition", 6m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			var releaseCapturedInfos = new[]
			{
				new WhsReleaseCapturedInfo { Attribute2 = "BLUE", Quantity = 4 },
				new WhsReleaseCapturedInfo { Attribute2 = "RED", Quantity = 2 }
			};

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLinePKs = pickLines.Select(p => p.PK.ToGuid()).ToArray();
			AssertNoExceptionThrown(() => PickLineUpdater.ConfirmPickLinesPickedQty(
				pickLines,
				new PickingInfo(6m, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("UNT", 6m, "", releaseCapturedInfos) }) },
				null,
				false));
		}

		#endregion

		#region TestInvalidReleaseCapturedInfosAreNotAllowed

		public void TestInvalidReleaseCapturedInfosAreNotAllowed()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var pickLine = orderline.PickLines[0];

			var releaseInfo1 = new WhsReleaseCapturedInfo("BLUE", "", "", "", 5);
			var releaseInfo2 = new WhsReleaseCapturedInfo("RED", "", "", "", 1);
			AssertExceptionThrown<ArgumentException>("Exception should be thrown when confirmed qty (8) is not equal to release captured info (5+1)",
				() => PickLineUpdater.ConfirmPickLinesPickedQty(
					new[] { pickLine },
					new PickingInfo(8, false),
					staff,
					new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 6m, "", new[] { releaseInfo1, releaseInfo2 }) }) },
					null,
					false));
		}

		#endregion

		#region TestReleaseCaptureSplittingReleaseLines

		public void TestReleaseCaptureSplittingReleaseLines()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			AssertEquals("Precondition: PartAttrib1 is release captured", true, product.IsPartAttribReleaseCaptured(data.Org1, 1));
			AssertEquals("Precondition: PartAttrib2 is NOT release captured", false, product.IsPartAttribReleaseCaptured(data.Org1, 2));
			AssertEquals("Precondition: PartAttrib3 is NOT release captured", false, product.IsPartAttribReleaseCaptured(data.Org1, 3));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines[0];
			AssertEquals(1, orderLine.ReleaseLines.Count);
			AssertEquals(10m, orderLine.ReleaseLines[0].Quantity);
			AssertEquals(false, pickLine.HasReleaseCapturedAttribs);

			orderLine.ClearReleaseLines();
			var releaseInfo1 = new WhsReleaseCapturedInfo("BLUE", "", "", "", 5);
			var releaseInfo2 = new WhsReleaseCapturedInfo("RED", "", "", "", 3);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(8, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 5m, "", new[] { releaseInfo1, releaseInfo2 }) }) },
				null,
				true);
			AssertEquals(2, orderLine.ReleaseLines.Count);

			var releaseLines = orderLine.ReleaseLines.ToArray<WhsReleaseLine>();
			var attBlue = releaseLines.Single(a => a.PartAttribute1 == "BLUE");
			AssertEquals(5m, attBlue.Quantity);

			var attRed = releaseLines.Single(a => a.PartAttribute1 == "RED");
			AssertEquals(3m, attRed.Quantity);

			AssertEquals(2, orderLine.PickLines.Count);

			var releasedBlue = orderLine.PickLines.Single(a => a.WZ_ReleaseCapturedPartAttrib1 == "BLUE");
			AssertEquals(5m, releasedBlue.WZ_Units);

			var releasedRed = orderLine.PickLines.Single(a => a.WZ_ReleaseCapturedPartAttrib1 == "RED");
			AssertEquals(3m, releasedRed.WZ_Units);

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestReleaseCaptureSplittingReleaseLinesDiffLocations

		public void TestReleaseCaptureSplittingReleaseLinesDiffLocations()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			AssertEquals("Precondition: PartAttrib1 is release captured", true, product.IsPartAttribReleaseCaptured(data.Org1, 1));
			AssertEquals("Precondition: PartAttrib2 is NOT release captured", false, product.IsPartAttribReleaseCaptured(data.Org1, 2));
			AssertEquals("Precondition: PartAttrib3 is NOT release captured", false, product.IsPartAttribReleaseCaptured(data.Org1, 3));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.FindLocation("A-2"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderline.PickLines[0];
			var pickLine2 = orderline.PickLines[1];
			AssertEquals(1, orderline.ReleaseLines.Count);
			AssertEquals(20m, orderline.ReleaseLines[0].Quantity);
			AssertEquals(false, pickLine1.HasReleaseCapturedAttribs);
			AssertEquals(false, pickLine2.HasReleaseCapturedAttribs);

			orderline.ClearReleaseLines();
			var releaseInfo1 = new WhsReleaseCapturedInfo("BLUE", "", "", "", 4);
			var releaseInfo2 = new WhsReleaseCapturedInfo("RED", "", "", "", 3);
			var releaseInfo3 = new WhsReleaseCapturedInfo("BLUE", "", "", "", 5);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1 },
				new PickingInfo(7, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 7m, "", new[] { releaseInfo1, releaseInfo2 }) }) },
				null,
				true);

			orderline.ClearReleaseLines();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine2 },
				new PickingInfo(5, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 5m, "", new[] { releaseInfo3 }) }) },
				null,
				true);
			AssertEquals(2, orderline.ReleaseLines.Count);

			var releaseLines = orderline.ReleaseLines.ToArray<WhsReleaseLine>();
			var attBlue = releaseLines.Single(a => a.PartAttribute1 == "BLUE");
			AssertEquals("The two Blue Release Captured infos should have their quantity combined.", 4m + 5m, attBlue.Quantity);

			var attRed = releaseLines.Single(a => a.PartAttribute1 == "RED");
			AssertEquals(3m, attRed.Quantity);

			AssertEquals(3, orderline.PickLines.Count);
			AssertEquals(2, orderline.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE"));
			AssertEquals(1, orderline.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && l.WZ_Units == 4m));
			AssertEquals(1, orderline.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && l.WZ_Units == 5m));
			AssertEquals(1, orderline.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED" && l.WZ_Units == 3m));

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestReleaseCaptureSplittingReleaseLinesDiffLocationsAndManyReleaseLines

		public void TestReleaseCaptureSplittingReleaseLinesDiffLocationsAndManyReleaseLines()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			AssertEquals("Precondition: PartAttrib1 is release captured", true, product.IsPartAttribReleaseCaptured(data.Org1, 1));
			AssertEquals("Precondition: PartAttrib2 is NOT release captured", false, product.IsPartAttribReleaseCaptured(data.Org1, 2));
			AssertEquals("Precondition: PartAttrib3 is NOT release captured", false, product.IsPartAttribReleaseCaptured(data.Org1, 3));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m, data.Whs1.FindLocation("A-2"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 11m);
			Helper.CreatePickNew(order);
			AssertEquals(1, orderLine.ReleaseLines.Count);
			AssertEquals(11m, orderLine.ReleaseLines[0].Quantity);

			var pickLine = orderLine.PickLines.First(p => p.WZ_Units == 5m);
			AssertEquals(false, pickLine.HasReleaseCapturedAttribs);

			orderLine.ClearReleaseLines();
			var releaseInfo1 = new WhsReleaseCapturedInfo("BLUE", "", "", "", 4);
			var releaseInfo2 = new WhsReleaseCapturedInfo("RED", "", "", "", 1);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(5m, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 5m, "", new[] { releaseInfo1, releaseInfo2 }) }) },
				null,
				true);

			var releaseLines = orderLine.ReleaseLines.ToArray<WhsReleaseLine>();
			var attBlue = releaseLines.Single(a => a.PartAttribute1 == "BLUE");
			AssertEquals(4m, attBlue.Quantity);

			var attRed = releaseLines.Single(a => a.PartAttribute1 == "RED");
			AssertEquals(1m, attRed.Quantity);
			AssertEquals(6m, releaseLines.Where(a => a != attBlue && a != attRed).Sum(a => a.Quantity));
			AssertEquals(11m, orderLine.SumOfUnitsMet);
			AssertEquals(3, orderLine.PickLines.Count);
			AssertEquals(1, orderLine.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && l.WZ_Units == 4m));
			AssertEquals(1, orderLine.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED" && l.WZ_Units == 1m));
			AssertEquals(1, orderLine.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1 == "" && l.WZ_Units == 6m));

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestReleaseLinesCreatedFromReleaseCapturedInfoWhenThereAreNoExistingReleaseLines

		public void TestReleaseLinesCreatedFromReleaseCapturedInfoWhenThereAreNoExistingReleaseLines()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			AssertEquals("Precondition: PartAttrib1 is release captured", true, product.IsPartAttribReleaseCaptured(data.Org1, 1));
			AssertEquals("Precondition: PartAttrib1 is NOT release captured", false, product.IsPartAttribReleaseCaptured(data.Org1, 2));
			AssertEquals("Precondition: PartAttrib1 is NOT release captured", false, product.IsPartAttribReleaseCaptured(data.Org1, 3));
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			AssertEquals(0, orderLine.PickLines.Count(l => l.HasReleaseCapturedAttribs));

			orderLine.ClearReleaseLines();
			var releaseInfo1 = new WhsReleaseCapturedInfo("BLUE", "", "", "", 5);
			var releaseInfo2 = new WhsReleaseCapturedInfo("RED", "", "", "", 3);
			var pickLines = orderLine.PickLines.ToArray();
			var pickLinePKs = pickLines.Select(p => p.PK.ToGuid()).ToArray();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				orderLine.PickLines.ToArray(),
				new PickingInfo(8, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("UNT", 8m, "", new[] { releaseInfo1, releaseInfo2 }) }) },
				null,
				true);
			AssertEquals(2, orderLine.ReleaseLines.Count);

			var releaseLines = orderLine.ReleaseLines.ToArray<WhsReleaseLine>();
			var attBlue = releaseLines.Single(a => a.PartAttribute1 == "BLUE");
			AssertEquals(5m, attBlue.Quantity);

			var attRed = releaseLines.Single(a => a.PartAttribute1 == "RED");
			AssertEquals(3m, attRed.Quantity);
			AssertEquals(5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE").WZ_Units);
			AssertEquals("2 Red PickLines.", 2, orderLine.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertNotNull("1 Red Line with 2m.", orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED" && l.WZ_Units == 2m));
			AssertNotNull("1 Red Line with 1m.", orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED" && l.WZ_Units == 1m));
			AssertEquals("No Pick Lines should be Over released.", true, orderLine.PickLines.All(p => p.UnreleaseCapturedQty >= 0));
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestReleaseCaptureWithAlreadyCapturedAttribs

		public void TestReleaseCaptureWithAlreadyCapturedAttribs()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var pickLine = orderLine.PickLines[0];
			AssertEquals(1, orderLine.ReleaseLines.Count);
			AssertEquals(10m, orderLine.ReleaseLines[0].Quantity);
			AssertEquals(false, pickLine.HasReleaseCapturedAttribs);

			orderLine.ClearReleaseLines();
			var releaseInfo1 = new WhsReleaseCapturedInfo("BLUE", "", "", "", 5);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(5m, false, true),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 5m, "", new[] { releaseInfo1 }) }) },
				package,
				false);
			AssertEquals(2, orderLine.ReleaseLines.Count);

			var packedPickLine = orderLine.PickLines.Single(pl => pl != pickLine);

			AssertEquals(2, orderLine.PickLines.Count);
			AssertEquals(5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE").WZ_Units);
			AssertEquals(5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "").WZ_Units);

			var releaseLines = orderLine.ReleaseLines.ToArray<WhsReleaseLine>();
			var attBlue = releaseLines.Single(a => a.PartAttribute1 == "BLUE");
			AssertEquals(5m, attBlue.Quantity);
			AssertEquals(5m, releaseLines.Single(a => a.PartAttribute1 == "").Quantity);
			Factory.Save();

			releaseInfo1.IsCaptured = true;
			releaseInfo1.Quantity = 5;

			orderLine.ClearReleaseLines();
			var releaseInfo2 = new WhsReleaseCapturedInfo("RED", "", "", "", 3);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { packedPickLine, pickLine },
				new PickingInfo(3, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { packedPickLine.PK.ToGuid(), pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 5m, "", new[] { releaseInfo2 }) }) },
				package,
				false);
			AssertEquals(2, orderLine.ReleaseLines.Count);

			releaseLines = orderLine.ReleaseLines.ToArray<WhsReleaseLine>();
			attBlue = releaseLines.Single(a => a.PartAttribute1 == "BLUE");
			AssertEquals(5m, attBlue.Quantity);

			var attRed = releaseLines.Single(a => a.PartAttribute1 == "RED");
			AssertEquals(3m, attRed.Quantity);
			AssertEquals(0, releaseLines.Count(a => a.PartAttribute1 == ""));
			AssertEquals(2, orderLine.PickLines.Count);
			var blueLine = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE");
			var redLine = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED");
			AssertEquals(5m, blueLine.WZ_Units);
			AssertEquals(3m, redLine.WZ_Units);
			AssertEquals("Package is packed with Release Captured Attribs.", 2, package.PackedItemDivots.Count);
			AssertEquals("Package is packed with Release Captured Attribs.", 1,
				package.PackedItemDivots.Count(d => d.PackedItem == blueLine && d.KI_PackedQty == 5m));
			AssertEquals("Package is packed with Release Captured Attribs.", 1,
				package.PackedItemDivots.Count(d => d.PackedItem == redLine && d.KI_PackedQty == 3m));

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestReleaseCaptureWithAlreadyCapturedAttribs_SomeAttribsEnteredInEnterprise

		public void TestReleaseCaptureWithAlreadyCapturedAttribs_SomeAttribsEnteredInEnterprise()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines[0];
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "GREEN";

			var oldPickLines = orderLine.PickLines.ToArray();

			AssertEquals(1, orderLine.PickLines.Count);
			AssertEquals("Precondition: Release Captured Attribute is GREEN.", "GREEN", orderLine.PickLines.Single(l => l.WZ_Units == 10m).WZ_ReleaseCapturedPartAttrib1);

			orderLine.ClearReleaseLines(); // clear caches

			Factory.Save();

			var releaseInfo = new WhsReleaseCapturedInfo("BLUE", "", "", "", 10m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(10m, false), staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 10m, "", new[] { releaseInfo }) }) },
				null,
				false);

			AssertEquals("PickLines not deleted.", true, oldPickLines.All(l => !l.IsDeleted));
			AssertEquals("One PickLine has 10m Blue.", "BLUE", orderLine.PickLines.Single(l => l.WZ_Units == 10m).WZ_ReleaseCapturedPartAttrib1);
			AssertNoExceptionThrown(() => Factory.Save());

			var poke = orderLine.ReleaseLines.Count;
			orderLine.ReleaseLines.RunPreSaveValidation();
			AssertNoRowErrors(orderLine);
		}

		#endregion

		#region TestReleaseCaptureWithAlreadyCapturedAttribs_SomeAttribsEnteredInEnterprise_AttributeValueMatches

		public void TestReleaseCaptureWithAlreadyCapturedAttribs_SomeAttribsEnteredInEnterprise_AttributeValueMatches()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines[0];
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "BLUE";
			releaseLine.Quantity = 4m;

			var oldPickLines = orderLine.PickLines.ToArray();
			AssertEquals(2, orderLine.PickLines.Count);
			AssertEquals("Precondition: Release Captured Attribute is BLUE.", "BLUE", orderLine.PickLines.Single(l => l.WZ_Units == 4m).WZ_ReleaseCapturedPartAttrib1);

			orderLine.ClearReleaseLines(); // clear caches

			Factory.Save();

			var releaseInfo = new WhsReleaseCapturedInfo("BLUE", "", "", "", 10m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				orderLine.PickLines.ToArray(),
				new PickingInfo(10m, false), staff,
				new[] { new PickLinesToPickedPackTypeInfo(orderLine.PickLines.Select(l => l.PK.ToGuid()).ToArray(), new[] { new PickedPackTypeInfo("UNT", 10m, "", new[] { releaseInfo }) }) },
				null,
				true);

			AssertEquals("PickLines not deleted.", true, oldPickLines.All(l => !l.IsDeleted));
			AssertEquals("One PickLine has 4m Blue.", "BLUE", orderLine.PickLines.Single(l => l.WZ_Units == 4m).WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("One PickLine has 6m Blue.", "BLUE", orderLine.PickLines.Single(l => l.WZ_Units == 6m).WZ_ReleaseCapturedPartAttrib1);

			AssertNoExceptionThrown(() => Factory.Save());

			var poke = orderLine.ReleaseLines.Count;
			orderLine.ReleaseLines.RunPreSaveValidation();
			AssertNoRowErrors(orderLine);
		}

		#endregion

		#region TestReleaseCaptureWithAlreadyCapturedAttribs_SomeAttribsEnteredInEnterprise_ShortPick

		public void TestReleaseCaptureWithAlreadyCapturedAttribs_SomeAttribsEnteredInEnterprise_ShortPick()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines[0];
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "GREEN";

			AssertEquals(1, orderLine.PickLines.Count);
			AssertEquals("One PickLine has 10m Green.", "GREEN", orderLine.PickLines.Single(l => l.WZ_Units == 10m).WZ_ReleaseCapturedPartAttrib1);

			orderLine.ClearReleaseLines(); // clear caches
			var oldPickLines = orderLine.PickLines;

			Factory.Save();

			var releaseInfo = new WhsReleaseCapturedInfo("BLUE", "", "", "", 8m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(8m, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 8m, "", new[] { releaseInfo }) }) },
				null,
				false);
			AssertEquals(8m, pickLine.WZ_Units);
			AssertEquals("PickLines not deleted.", true, oldPickLines.All(l => !l.IsDeleted));
			AssertEquals("1 Pick Line.", 1, orderLine.PickLines.Count);
			AssertEquals("One PickLine has 8m Blue.", "BLUE", orderLine.PickLines.Single(l => l.WZ_Units == 8m).WZ_ReleaseCapturedPartAttrib1);

			AssertNoExceptionThrown(() => Factory.Save());

			var poke = orderLine.ReleaseLines.Count;
			orderLine.ReleaseLines.RunPreSaveValidation();
			AssertNoRowErrors(orderLine);
		}

		#endregion

		#region TestReleaseCaptureWithNoCapturedAttribs_AttribsEnteredInRF_ShortPickSplitPickLine

		public void TestReleaseCaptureWithNoCapturedAttribs_AttribsEnteredInRF_ShortPickSplitPickLine()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines[0];

			AssertEquals("No captured attributes yet.", false, pickLine.HasReleaseCapturedAttribs);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);

			// create Tote (for Order)
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);

			// create Slot and Assign Tote
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, 5);
			packingHelper.CreatePackageDivot(packageTote, pickLine);
			Factory.Save();

			AssertEquals("Precondition: Release Captured Attribute is not entered.", false, pickLine.HasReleaseCapturedAttribs);
			AssertEquals("Precondition: There should be 1 packed item divot.", 1, packageTote.PackedItemDivots.Count);
			AssertEquals("Precondition: There should be 1 pickline.", 1, pick.GetAllPickLines().Count());
			Factory.Save();

			var releaseInfo1 = new WhsReleaseCapturedInfo("BLUE", "", "", "", 8m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(8m, false, true),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 8m, "", new[] { releaseInfo1 }) }) },
				null,
				false);

			AssertEquals("2 PickLines.", 2, pick.GetAllPickLines().Count());
			AssertEquals("One PickLine has 8m Blue.", "BLUE", orderLine.PickLines.Single(l => l.WZ_Units == 8m).WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("One PickLine has 2m empty.", "", orderLine.PickLines.Single(l => l.WZ_Units == 2m).WZ_ReleaseCapturedPartAttrib1);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("There should be 2 packed item divots.", 2, packageTote.PackedItemDivots.Count);
			AssertEquals("There should be 2 picklines.", 2, pick.GetAllPickLines().Count());

			var releaseInfo2 = new WhsReleaseCapturedInfo("RED", "", "", "", 1m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine },
				new PickingInfo(1m, false, true),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 1m, "", new[] { releaseInfo2 }) }) },
				null,
				false);

			AssertEquals("3 PickLines.", 3, pick.GetAllPickLines().Count());
			AssertEquals("One PickLine has 8m Blue.", "BLUE", orderLine.PickLines.Single(l => l.WZ_Units == 8m).WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("One PickLine has 1m Red.", 1m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED").WZ_Units);
			AssertEquals("One PickLine has 1m empty.", 1m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "").WZ_Units);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("There should be 3 packed item divots.", 3, packageTote.PackedItemDivots.Count);
			AssertEquals("There should be 3 picklines.", 3, pick.GetAllPickLines().Count());
		}

		#endregion

		#region TestReleaseCaptureWithAlreadyCapturedAttribs_SomeAttribsEnteredInEnterprise_ShortPick_ManyPickLines

		public void TestReleaseCaptureWithAlreadyCapturedAttribs_SomeAttribsEnteredInEnterprise_ShortPick_ManyPickLines()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine.PickLines.Single(p => p.WZ_Units == 10m);
			var pickLine2 = orderLine.PickLines.Single(p => p.WZ_Units == 5m);
			var releaseLine1 = orderLine.ReleaseLines[0];
			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine1.Quantity = 10m;
			releaseLine2.Quantity = 5m;
			releaseLine1.PartAttribute1 = "GREEN";
			releaseLine2.PartAttribute1 = "YELLOW";

			AssertEquals("Precondition: Release Captured Qty is correct.", 10m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "GREEN").WZ_Units);
			AssertEquals("Precondition: Release Captured Qty is correct.", 5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "YELLOW").WZ_Units);

			orderLine.ClearReleaseLines(); // clear caches

			Factory.Save();

			var releaseInfo = new WhsReleaseCapturedInfo("BLUE", "", "", "", 8m);
			PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1, pickLine2 },
				new PickingInfo(8m, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 8m, "", new[] { releaseInfo }) }) },
				null,
				false);
			AssertEquals(8m, orderLine.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("1 PickLine remaining.", 1, orderLine.PickLines.Count);
			AssertEquals("New Release Captured Attribs should be stored.", 8m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE").WZ_Units);
			AssertNoExceptionThrown(() => Factory.Save());

			var poke = orderLine.ReleaseLines.Count;
			orderLine.ReleaseLines.RunPreSaveValidation();
			AssertNoRowErrors(orderLine);
		}

		#endregion

		#region TestRelease CapturedAttributesShouldKeepInformattionAfterSplitingPickLine

		public void TestRealseLineShouldKeepInformattionAfterSplitingPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 100);
			Helper.CreateProductUnit(data.Part1, "BOX", "PLT", 4);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 236m);
			// as PickByUOM is on - 3 picklines will be created
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: PickLines are split up by UOM Type.", 3, orderLine.PickLines.Count);

			var releaseCaptureInfo = new WhsReleaseCapturedInfo { Attribute2 = "RED", Quantity = 180 };

			var pickLines = orderLine.PickLines.ToArray();
			var pickLinePKs = pickLines.Select(p => p.PK.ToGuid()).ToArray();
			PickLineUpdater.ConfirmPickLinesPickedQty(
				pickLines,
				new PickingInfo(180m, false),
				staff,
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("UNT", 180m, "", new[] { releaseCaptureInfo }) }) },
				null,
				false);

			AssertEquals("Should have release Captured 180 Red Units.", 180m, orderLine.PickLines.Where(l => l.HasReleaseCapturedAttribs).Sum(l => l.WZ_Units));
			AssertEquals("Should have release Captured 180 Red Units.", true, orderLine.PickLines.Where(l => l.HasReleaseCapturedAttribs).All(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED"));
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasNotShorted_FullyShortingTheSecond

		public void TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasNotShorted_FullyShortingTheSecond()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(0, false), staff);

			AssertEquals("Fully shorted.", true, framePickLine.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, orderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, orderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery).Length);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasNotShorted_PartiallyShortingTheSecond

		public void TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasNotShorted_PartiallyShortingTheSecond()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(3, false), staff);

			AssertEquals("Partially shorted.", 3m, framePickLine.WZ_Units);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			var reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 3m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 3m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 3m);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasFullyShorted_FullyShortingTheSecond

		public void TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasFullyShorted_FullyShortingTheSecond()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { wheelPickLine }, new PickingInfo(0, false), staff);
			AssertEquals("Fully shorted.", true, wheelPickLine.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(0, false), staff);
			AssertEquals("Fully shorted.", true, framePickLine.IsDeleted);
			newFactory = new BusinessObjectFactory();
			reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line remains deleted.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Pick Line remains deleted.", 0, reloadedOrderLine.PickLines.Count);
			linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links remains deleted.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery).Length);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasFullyShorted_PartillyShortingTheSecond

		public void TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasFullyShorted_PartillyShortingTheSecond()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { wheelPickLine }, new PickingInfo(0, false), staff);
			AssertEquals("Fully shorted.", true, wheelPickLine.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(3, false), staff);
			AssertEquals("Partially shorted.", 3m, framePickLine.WZ_Units);
			newFactory = new BusinessObjectFactory();
			reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line remains deleted.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Pick Line remains deleted.", 0, reloadedOrderLine.PickLines.Count);
			linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links remains deleted.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery).Length);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasPartiallyShorted_PartillyShortingTheSecond_ReduceMoreKitQty

		public void TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasPartiallyShorted_PartillyShortingTheSecond_ReduceMoreKitQty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { wheelPickLine }, new PickingInfo(9, false), staff);
			AssertEquals("Partially shorted.", 9m, wheelPickLine.WZ_Units);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			var reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 4m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 4m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 8m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 4m);

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(3, false), staff);
			AssertEquals("Partially shorted.", 3m, framePickLine.WZ_Units);
			newFactory = new BusinessObjectFactory();
			reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 3m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 3m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 3m);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasPartiallyShorted_PartillyShortingTheSecond

		public void TestRecreatePickByBOMReceiveLine_Shorting_SingleKitLine_TwoComponents_TheFirstWasPartiallyShorted_PartillyShortingTheSecond()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { wheelPickLine }, new PickingInfo(4, false), staff);
			AssertEquals("Partially shorted.", 4m, wheelPickLine.WZ_Units);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			var reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 2m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 2m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 4m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 2m);

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(3, false), staff);
			AssertEquals("Partially shorted, but more than we need because the lack of wheels.", 3m, framePickLine.WZ_Units);
			newFactory = new BusinessObjectFactory();
			reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 2m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 2m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 4m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 2m);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnSameOrder_FullyShorted

		public void TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnSameOrder_FullyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			var createdReceiveLine2 = createdReceive.Lines[1];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(0, false), staff);

			AssertEquals("Fully shorted.", true, framePickLine1.IsDeleted);
			AssertEquals("Fully shorted.", true, framePickLine2.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine1.PickLines.Count);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine2.PickLines.Count);
			var linkQuery1 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine1.ChildComponentLines.Select(l => l.PK).ToArray());
			var linkQuery2 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine2.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery1).Length);
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery2).Length);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnSameOrder_PartillyShorted

		public void TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnSameOrder_PartillyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			var createdReceiveLine2 = createdReceive.Lines[1];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(2, false), staff);

			var newFactory = new BusinessObjectFactory();
			AssertEquals("One is fully shorted and one is not.", true, framePickLine1.IsDeleted || framePickLine2.IsDeleted);
			var remainingPickLine = framePickLine1.IsDeleted ? framePickLine2 : framePickLine1;
			var relevantKitOrderLine = newFactory.Load<WhsOrderLine>(remainingPickLine.DocketLine.WE_WE_ParentDocketLine);
			var relevantWheelOrderLine = (WhsOrderLine)relevantKitOrderLine.ChildComponentLines.Single(c => c.WE_OP == wheel.PK);
			var relevantFrameOrderLine = (WhsOrderLine)relevantKitOrderLine.ChildComponentLines.Single(c => c.WE_OP == frame.PK);
			var reloadedReceives = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var reloadedReceive = reloadedReceives[0];
			AssertEquals("One Receive Line deleted.", 1, reloadedReceive.Lines.Count);
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == relevantWheelOrderLine.PK);
			var reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == relevantFrameOrderLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 2m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, relevantKitOrderLine, 2m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, relevantWheelOrderLine, 4m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, relevantFrameOrderLine, 2m);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnDifferentOrders_FullyShorted

		public void TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnDifferentOrders_FullyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 5m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order1);
			var createdReceiveLine1 = createdReceive.Lines[0];
			var createdReceiveLine2 = createdReceive.Lines[1];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(0, false), staff);

			AssertEquals("Fully shorted.", true, framePickLine1.IsDeleted);
			AssertEquals("Fully shorted.", true, framePickLine2.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine1.PickLines.Count);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine2.PickLines.Count);
			var linkQuery1 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine1.ChildComponentLines.Select(l => l.PK).ToArray());
			var linkQuery2 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine2.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery1).Length);
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery2).Length);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnDifferentOrders_PartillyShorted

		public void TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnDifferentOrders_PartillyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 5m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order1);
			var createdReceiveLine1 = createdReceive.Lines[0];
			var createdReceiveLine2 = createdReceive.Lines[1];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(2, false), staff);

			AssertEquals("One is fully shorted and one is not.", true, framePickLine1.IsDeleted || framePickLine2.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var remainingPickLine = framePickLine1.IsDeleted ? framePickLine2 : framePickLine1;
			var relevantKitOrderLine = newFactory.Load<WhsOrderLine>(remainingPickLine.DocketLine.WE_WE_ParentDocketLine);
			var relevantWheelOrderLine = (WhsOrderLine)relevantKitOrderLine.ChildComponentLines.Single(c => c.WE_OP == wheel.PK);
			var relevantFrameOrderLine = (WhsOrderLine)relevantKitOrderLine.ChildComponentLines.Single(c => c.WE_OP == frame.PK);
			var reloadedReceives = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var reloadedReceive = reloadedReceives[0];
			AssertEquals("One Receive Line deleted.", 1, reloadedReceive.Lines.Count);
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == relevantWheelOrderLine.PK);
			var reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == relevantFrameOrderLine.PK);
			AssertCreatedReceive(reloadedReceive, order1);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 2m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, relevantKitOrderLine, 2m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, relevantWheelOrderLine, 4m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, relevantFrameOrderLine, 2m);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnDifferentClients_FullyShorted

		public void TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnDifferentClients_FullyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var org2 = Helper.CreateClient("XY9");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			Helper.CreateProductClientRelationShip(org2, bike);
			Helper.CreateProductClientRelationShip(org2, wheel);
			Helper.CreateProductClientRelationShip(org2, frame);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive2, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 5m);
			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 5m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceives = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertEquals("2 Receives for 2 Clients.", 2, createdReceives.Length);
			var createdReceive1 = createdReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
			var createdReceive2 = createdReceives.Single(d => d.WD_OH_Client == org2.PK);
			AssertCreatedReceive(createdReceive1, order1);
			AssertCreatedReceive(createdReceive2, order2);
			var createdReceiveLine1 = createdReceive1.Lines[0];
			var createdReceiveLine2 = createdReceive2.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(0, false), staff);

			AssertEquals("Fully shorted.", true, framePickLine1.IsDeleted);
			AssertEquals("Fully shorted.", true, framePickLine2.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceives = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertEquals("2 Receives for 2 Clients.", 2, reloadedReceives.Length);
			var reloadedReceive1 = createdReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
			var reloadedReceive2 = createdReceives.Single(d => d.WD_OH_Client == org2.PK);
			var reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive1.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive2.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine1.PickLines.Count);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine2.PickLines.Count);
			var linkQuery1 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine1.ChildComponentLines.Select(l => l.PK).ToArray());
			var linkQuery2 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine2.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery1).Length);
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery2).Length);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnDifferentClients_PartillyShorted

		public void TestRecreatePickByBOMReceiveLine_Shorting_MultipleKitLinesOnDifferentClients_PartillyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var org2 = Helper.CreateClient("XY9");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			Helper.CreateProductClientRelationShip(org2, bike);
			Helper.CreateProductClientRelationShip(org2, wheel);
			Helper.CreateProductClientRelationShip(org2, frame);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive2, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 5m);
			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 5m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceives = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertEquals("2 Receives for 2 Clients.", 2, createdReceives.Length);
			var createdReceive1 = createdReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
			var createdReceive2 = createdReceives.Single(d => d.WD_OH_Client == org2.PK);
			AssertCreatedReceive(createdReceive1, order1);
			AssertCreatedReceive(createdReceive2, order2);
			var createdReceiveLine1 = createdReceive1.Lines[0];
			var createdReceiveLine2 = createdReceive2.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(2, false), staff);

			AssertEquals("One is fully shorted and one is not.", true, framePickLine1.IsDeleted || framePickLine2.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var remainingPickLine = framePickLine1.IsDeleted ? framePickLine2 : framePickLine1;
			var relevantKitOrderLine = newFactory.Load<WhsOrderLine>(remainingPickLine.DocketLine.WE_WE_ParentDocketLine);
			var relevantWheelOrderLine = (WhsOrderLine)relevantKitOrderLine.ChildComponentLines.Single(c => c.WE_OP == wheel.PK);
			var relevantFrameOrderLine = (WhsOrderLine)relevantKitOrderLine.ChildComponentLines.Single(c => c.WE_OP == frame.PK);
			var reloadedReceives = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertEquals("2 Receives for 2 Clients.", 2, reloadedReceives.Length);
			var reloadedReceive1 = createdReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
			var reloadedReceive2 = createdReceives.Single(d => d.WD_OH_Client == org2.PK);
			var reloadedReceiveLine = (WhsReceiveLine)(reloadedReceive1.Lines.Count > 0 ? reloadedReceive1.Lines.Single() : reloadedReceive2.Lines.Single());
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == relevantWheelOrderLine.PK);
			var reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == relevantFrameOrderLine.PK);
			AssertCreatedReceive(reloadedReceive1, order1);
			AssertCreatedReceive(reloadedReceive2, order2);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 2m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, relevantKitOrderLine, 2m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, relevantWheelOrderLine, 4m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, relevantFrameOrderLine, 2m);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_ReassignPackTypeToPickLines

		public void TestRecreatePickByBOMReceiveLine_Shorting_ReassignPackTypeToPickLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 100m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 50m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single();
			AssertEquals("Should be allocated.", 2, wheelComponentLine.PickLines.Count);
			var wheelPickLine1 = wheelComponentLine.PickLines.Single(l => l.WZ_Units == 96m);
			var wheelPickLine2 = wheelComponentLine.PickLines.Single(l => l.WZ_Units == 4m);
			AssertEquals("CTN", wheelPickLine1.WZ_F3_NKAllocatedPackType);
			AssertEquals("UNT", wheelPickLine2.WZ_F3_NKAllocatedPackType);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 50m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 2, createdPickLines.Length);
			var relatedPickLine1 = createdPickLines.Single(l => l.WZ_Units == 48m);
			var relatedPickLine2 = createdPickLines.Single(l => l.WZ_Units == 2m);
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine, 48m, "CTN");
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine1, orderLine, 2m, "UNT");
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 100m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 100m);

			var newFactory0 = new BusinessObjectFactory() { RefreshEnabled = false };
			wheelPickLine1 = newFactory0.Load<WhsPickLine>(wheelPickLine1.PK);
			wheelPickLine2 = newFactory0.Load<WhsPickLine>(wheelPickLine2.PK);
			staff = newFactory0.Load<GlbStaff>(staff.PK);
			ConfirmPickLinesPickedQty(new[] { wheelPickLine1, wheelPickLine2 }, new PickingInfo(50m, false), staff);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			createdReceiveLine1 = reloadedReceive.Lines[0];
			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Receive Line's quantity reduced.", 1, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 25m);
			AssertEquals(2, orderLine.PickLines.Count);
			createdPickLines = orderLine.PickLines.ToArray();
			relatedPickLine1 = createdPickLines.Single(l => l.WZ_Units == 24m);
			relatedPickLine2 = createdPickLines.Single(l => l.WZ_Units == 1m);
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine, 24m, "CTN");
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine1, orderLine, 1m, "UNT");
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Shorting_ReassignPackTypeToPickLines_TwoOrders

		public void TestRecreatePickByBOMReceiveLine_Shorting_ReassignPackTypeToPickLines_TwoOrders()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 200m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 20m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 40m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock();
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 1, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single();
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single();
			AssertEquals("Should be allocated.", 2, wheelComponentLine1.PickLines.Count);
			AssertEquals("Should be allocated.", 2, wheelComponentLine2.PickLines.Count);
			var wheelPickLine1 = wheelComponentLine1.PickLines.Single(l => l.WZ_Units == 36m);
			var wheelPickLine2 = wheelComponentLine1.PickLines.Single(l => l.WZ_Units == 4m);
			var wheelPickLine3 = wheelComponentLine2.PickLines.Single(l => l.WZ_Units == 72m);
			var wheelPickLine4 = wheelComponentLine2.PickLines.Single(l => l.WZ_Units == 8m);
			AssertEquals("CTN", wheelPickLine1.WZ_F3_NKAllocatedPackType);
			AssertEquals("UNT", wheelPickLine2.WZ_F3_NKAllocatedPackType);
			AssertEquals("CTN", wheelPickLine3.WZ_F3_NKAllocatedPackType);
			AssertEquals("UNT", wheelPickLine4.WZ_F3_NKAllocatedPackType);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order1);
			var createdReceiveLine1 = (WhsReceiveLine)createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 20m);
			var createdReceiveLine2 = (WhsReceiveLine)createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 40m);
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 20m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 40m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 2, createdPickLines1.Length);
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 2, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1.Single(l => l.WZ_Units == 12m);
			var relatedPickLine2 = createdPickLines1.Single(l => l.WZ_Units == 8m);
			var relatedPickLine3 = createdPickLines2.Single(l => l.WZ_Units == 36m);
			var relatedPickLine4 = createdPickLines2.Single(l => l.WZ_Units == 4m);
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 12m, "CTN");
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine1, orderLine1, 8m, "UNT");
			AssertCreatedPickLine(relatedPickLine3, createdReceiveLine2, orderLine2, 36m, "CTN");
			AssertCreatedPickLine(relatedPickLine4, createdReceiveLine2, orderLine2, 4m, "UNT");

			var newFactory0 = new BusinessObjectFactory() { RefreshEnabled = false };
			wheelPickLine1 = newFactory0.Load<WhsPickLine>(wheelPickLine1.PK);
			wheelPickLine2 = newFactory0.Load<WhsPickLine>(wheelPickLine2.PK);
			wheelPickLine3 = newFactory0.Load<WhsPickLine>(wheelPickLine3.PK);
			wheelPickLine4 = newFactory0.Load<WhsPickLine>(wheelPickLine4.PK);
			staff = newFactory0.Load<GlbStaff>(staff.PK);
			ConfirmPickLinesPickedQty(new[] { wheelPickLine1, wheelPickLine2, wheelPickLine3, wheelPickLine4 }, new PickingInfo(90m, false), staff);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			createdReceiveLine1 = (WhsReceiveLine)reloadedReceive.Lines.Single(l => l.WE_TransactionQuantity == 9m);
			createdReceiveLine2 = (WhsReceiveLine)reloadedReceive.Lines.Single(l => l.WE_TransactionQuantity == 36m);
			orderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			orderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 9m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 36m);
			AssertEquals(1, orderLine1.PickLines.Count);
			AssertEquals(1, orderLine2.PickLines.Count);
			relatedPickLine1 = orderLine1.PickLines.Single();
			relatedPickLine3 = orderLine2.PickLines.Single();
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 9m, "UNT");
			AssertCreatedPickLine(relatedPickLine3, createdReceiveLine2, orderLine2, 36m, "CTN");
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_SingleKitLine_FullyAllocated

		public void TestRecreatePickByBOMReceiveLine_Reallocation_SingleKitLine_FullyAllocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			// create new inventories for reallocation
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 10m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive2, frame, 10m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(0, false), staff);

			AssertEquals("Precondition: Fully shorted.", true, framePickLine.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Precondition: Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Precondition: Pick Line deleted due to Short Picking.", 0, reloadedOrderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("Precondition: BOM Inventory Links deleted due to Short Picking.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLineInAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
				var notifications = new NotificationBuffer();
				var allocationResult = pickInAnotherFactory.AutoAllocateItems(pickInAnotherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray(), notifications);
				AssertEquals("Rellocated successfully.", AllocationResult.AllocatedStock, allocationResult);

				var orderLinesToReallocate = new WhsOrderLine[] { orderLineInAnotherFactory };
				PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
				newFactory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
				reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
				AssertCreatedReceive(reloadedReceive, order);
				var reloadedReceiveLine1 = reloadedReceive.Lines[0];
				AssertCreatedReceiveLine(reloadedReceiveLine1, bike, 5m);
				var reloadedPickLines = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine1.PK));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines.Length);
				var reloadedPickLine = reloadedPickLines[0];
				AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine1, orderLine, 5m);
				AssertEquals("Links created.", 2, reloadedReceiveLine1.BOMComponentLinks.Count());
				var reloadedWheelLink = reloadedReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
				var reloadedFrameLink = reloadedReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
				AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine1, (WhsOrderLine)reloadedOrderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
				AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine1, (WhsOrderLine)reloadedOrderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			}
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_SingleKitLine_PartiallyAllocated

		public void TestRecreatePickByBOMReceiveLine_Reallocation_SingleKitLine_PartiallyAllocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			// create new inventories for reallocation
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 4m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive2, frame, 1m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(0, false), staff);

			AssertEquals("Precondition: Fully shorted.", true, framePickLine.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Precondition: Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Precondition: Pick Line deleted due to Short Picking.", 0, reloadedOrderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("Precondition: BOM Inventory Links deleted due to Short Picking.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLineInAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
				var notifications = new NotificationBuffer();
				var allocationResult = pickInAnotherFactory.AutoAllocateItems(pickInAnotherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray(), notifications);
				AssertEquals("Rellocated successfully.", AllocationResult.AllocatedStock, allocationResult);

				var orderLinesToReallocate = new WhsOrderLine[] { orderLineInAnotherFactory };
				PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
				newFactory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
				reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
				AssertCreatedReceive(reloadedReceive, order);
				var reloadedReceiveLine1 = reloadedReceive.Lines[0];
				AssertCreatedReceiveLine(reloadedReceiveLine1, bike, 1m);
				var reloadedPickLines = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine1.PK));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines.Length);
				var reloadedPickLine = reloadedPickLines[0];
				AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine1, orderLine, 1m);
				AssertEquals("Links created.", 2, reloadedReceiveLine1.BOMComponentLinks.Count());
				var reloadedWheelLink = reloadedReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 2m);
				var reloadedFrameLink = reloadedReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 1m);
				AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine1, (WhsOrderLine)reloadedOrderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 2m);
				AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine1, (WhsOrderLine)reloadedOrderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 1m);
			}
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_SingleKitLine_NothingAllocated

		public void TestRecreatePickByBOMReceiveLine_Reallocation_SingleKitLine_NothingAllocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(0, false), staff);

			AssertEquals("Precondition: Fully shorted.", true, framePickLine.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Precondition: Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Precondition: Pick Line deleted due to Short Picking.", 0, reloadedOrderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("Precondition: BOM Inventory Links deleted due to Short Picking.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
			var orderLineInAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);

			var orderLinesToReallocate = new WhsOrderLine[] { orderLineInAnotherFactory };
			PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			reloadedReceive = newFactory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Nothing reallocated, no new Receive Line.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Nothing reallocated, no new Pick Line.", 0, reloadedOrderLine.PickLines.Count);
			linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("Nothing reallocated, no new BOM Inventory Links.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_SingleKitLine_HasInventoryForKit

		public void TestRecreatePickByBOMReceiveLine_Reallocation_SingleKitLine_HasInventoryForKit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 8m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 4m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, bike, 1m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 8 wheels.", 8m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 4 frames.", 4m, framePickLine.WZ_Units);
			AssertEquals("Precondition: 1 PickLine can be picked directly, 1 from Components.", 2, orderLine.PickLines.Count);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 4m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 4m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 8m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 4m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 8m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 4m);

			// create new inventories for reallocation
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 10m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive2, frame, 10m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			ConfirmPickLinesPickedQty(new[] { framePickLine }, new PickingInfo(1, false), staff);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			var reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 1m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 1m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 2m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 1m);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLineInAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
				var notifications = new NotificationBuffer();
				var allocationResult = pickInAnotherFactory.AutoAllocateItems(pickInAnotherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray(), notifications);
				AssertEquals("Rellocated successfully.", AllocationResult.AllocatedStock, allocationResult);

				var orderLinesToReallocate = new WhsOrderLine[] { orderLineInAnotherFactory };
				PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
				newFactory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
				var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
				AssertCreatedReceive(reloadedReceive, order);
				var reloadedReceiveLine1 = reloadedReceive.Lines[0];
				AssertCreatedReceiveLine(reloadedReceiveLine1, bike, 4m);
				var reloadedPickLines = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine1.PK));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines.Length);
				reloadedPickLine = reloadedPickLines[0];
				AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine1, orderLine, 4m);
				AssertEquals("Links created.", 2, reloadedReceiveLine1.BOMComponentLinks.Count());
				reloadedWheelLink = reloadedReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 8m);
				reloadedFrameLink = reloadedReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 4m);
				AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine1, (WhsOrderLine)reloadedOrderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 8m);
				AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine1, (WhsOrderLine)reloadedOrderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 4m);
			}
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_MultipleKitLinesOnDifferentOrders_FullyAllocated

		public void TestRecreatePickByBOMReceiveLine_Reallocation_MultipleKitLinesOnDifferentOrders_FullyAllocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 5m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order1);
			var createdReceiveLine1 = createdReceive.Lines[0];
			var createdReceiveLine2 = createdReceive.Lines[1];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			// create new inventories for reallocation
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 10m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive2, frame, 10m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(0, false), staff);
			Helper.Factory.Save();

			AssertEquals("Fully shorted.", true, framePickLine1.IsDeleted);
			AssertEquals("Fully shorted.", true, framePickLine2.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine1.PickLines.Count);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine2.PickLines.Count);
			var linkQuery1 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine1.ChildComponentLines.Select(l => l.PK).ToArray());
			var linkQuery2 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine2.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery1).Length);
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery2).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLine1InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				var orderLine2InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				var notifications = new NotificationBuffer();
				var allocationResult = pickInAnotherFactory.AutoAllocateItems(pickInAnotherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray(), notifications);
				AssertEquals("Rellocated successfully.", AllocationResult.AllocatedStock, allocationResult);

				var orderLinesToReallocate = new WhsOrderLine[] { orderLine1InAnotherFactory, orderLine2InAnotherFactory };
				PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
				newFactory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
				reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				AssertCreatedReceive(reloadedReceive, order1);
				AssertEquals("2 Receive Lines.", 2, reloadedReceive.Lines.Count);
				var links1 = newFactory.Load<WhsBOMInventoryPivot>(linkQuery1);
				var links2 = newFactory.Load<WhsBOMInventoryPivot>(linkQuery2);
				AssertEquals("Links created.", 2, links1.Length);
				AssertEquals("Links created.", 2, links2.Length);
				var reloadedWheelLink1 = links1.Single(l => l.WIP_ComponentQuantity == 10m);
				var reloadedFrameLink1 = links1.Single(l => l.WIP_ComponentQuantity == 5m);
				var reloadedWheelLink2 = links2.Single(l => l.WIP_ComponentQuantity == 10m);
				var reloadedFrameLink2 = links2.Single(l => l.WIP_ComponentQuantity == 5m);
				AssertEquals(wheelComponentLine1.PK, reloadedWheelLink1.WIP_WE_ComponentLine);
				AssertEquals(frameComponentLine1.PK, reloadedFrameLink1.WIP_WE_ComponentLine);
				AssertEquals(wheelComponentLine2.PK, reloadedWheelLink2.WIP_WE_ComponentLine);
				AssertEquals(frameComponentLine2.PK, reloadedFrameLink2.WIP_WE_ComponentLine);
				AssertCreatedReceiveLine((WhsReceiveLine)reloadedWheelLink1.InventoryLine, bike, 5m);
				AssertCreatedReceiveLine((WhsReceiveLine)reloadedWheelLink2.InventoryLine, bike, 5m);
				var reloadedPickLines1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedWheelLink1.WIP_WE_InventoryLine));
				var reloadedPickLines2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedWheelLink2.WIP_WE_InventoryLine));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines1.Length);
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines2.Length);
				var reloadedPickLine1 = reloadedPickLines1[0];
				var reloadedPickLine2 = reloadedPickLines2[0];
				AssertCreatedPickLine(reloadedPickLine1, (WhsReceiveLine)reloadedWheelLink1.InventoryLine, orderLine1, 5m);
				AssertCreatedPickLine(reloadedPickLine2, (WhsReceiveLine)reloadedWheelLink2.InventoryLine, orderLine2, 5m);
			}
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_MultipleKitLinesOnDifferentOrders_PartillyAllocated

		public void TestRecreatePickByBOMReceiveLine_Reallocation_MultipleKitLinesOnDifferentOrders_PartillyAllocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 5m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order1);
			var createdReceiveLine1 = createdReceive.Lines[0];
			var createdReceiveLine2 = createdReceive.Lines[1];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			// create new inventories for reallocation
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 18m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive2, frame, 9m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(0, false), staff);
			Helper.Factory.Save();

			AssertEquals("Fully shorted.", true, framePickLine1.IsDeleted);
			AssertEquals("Fully shorted.", true, framePickLine2.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine1.PickLines.Count);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine2.PickLines.Count);
			var linkQuery1 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine1.ChildComponentLines.Select(l => l.PK).ToArray());
			var linkQuery2 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine2.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery1).Length);
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery2).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLine1InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				var orderLine2InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				var notifications = new NotificationBuffer();
				var allocationResult = pickInAnotherFactory.AutoAllocateItems(pickInAnotherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray(), notifications);
				AssertEquals("Rellocated successfully.", AllocationResult.AllocatedStock, allocationResult);

				var orderLinesToReallocate = new WhsOrderLine[] { orderLine1InAnotherFactory, orderLine2InAnotherFactory };
				PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
				newFactory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
				reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				AssertCreatedReceive(reloadedReceive, order1);
				AssertEquals("2 Receive Lines.", 2, reloadedReceive.Lines.Count);
				var links1 = newFactory.Load<WhsBOMInventoryPivot>(linkQuery1);
				var links2 = newFactory.Load<WhsBOMInventoryPivot>(linkQuery2);
				AssertEquals("Links created.", 2, links1.Length);
				AssertEquals("Links created.", 2, links2.Length);
				var reloadedWheelLink1 = links1.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine1.PK);
				var reloadedFrameLink1 = links1.Single(l => l.WIP_WE_ComponentLine == frameComponentLine1.PK);
				var reloadedWheelLink2 = links2.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine2.PK);
				var reloadedFrameLink2 = links2.Single(l => l.WIP_WE_ComponentLine == frameComponentLine2.PK);
				AssertEquals(18m, reloadedWheelLink1.WIP_ComponentQuantity + reloadedWheelLink2.WIP_ComponentQuantity);
				AssertEquals(9m, reloadedFrameLink1.WIP_ComponentQuantity + reloadedFrameLink2.WIP_ComponentQuantity);
				var reloadedReceiveLine1 = (WhsReceiveLine)reloadedWheelLink1.InventoryLine;
				var reloadedReceiveLine2 = (WhsReceiveLine)reloadedWheelLink2.InventoryLine;
				var receiveLine1Qty = reloadedReceiveLine1.WE_TransactionQuantity;
				var receiveLine2Qty = reloadedReceiveLine2.WE_TransactionQuantity;
				AssertEquals(true, receiveLine1Qty == 4m || receiveLine1Qty == 5m);
				AssertEquals(9m, receiveLine1Qty + receiveLine2Qty);
				AssertCreatedReceiveLine((WhsReceiveLine)reloadedWheelLink1.InventoryLine, bike, receiveLine1Qty);
				AssertCreatedReceiveLine((WhsReceiveLine)reloadedWheelLink2.InventoryLine, bike, receiveLine2Qty);
				var reloadedPickLines1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedWheelLink1.WIP_WE_InventoryLine));
				var reloadedPickLines2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedWheelLink2.WIP_WE_InventoryLine));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines1.Length);
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines2.Length);
				var reloadedPickLine1 = reloadedPickLines1[0];
				var reloadedPickLine2 = reloadedPickLines2[0];
				AssertCreatedPickLine(reloadedPickLine1, (WhsReceiveLine)reloadedWheelLink1.InventoryLine, orderLine1, receiveLine1Qty);
				AssertCreatedPickLine(reloadedPickLine2, (WhsReceiveLine)reloadedWheelLink2.InventoryLine, orderLine2, receiveLine2Qty);
			}
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_MultipleKitLinesOnDifferentCliens_FullyAllocated

		public void TestRecreatePickByBOMReceiveLine_Reallocation_MultipleKitLinesOnDifferentClients_FullyAllocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var org2 = Helper.CreateClient("XY9");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			Helper.CreateProductClientRelationShip(org2, bike);
			Helper.CreateProductClientRelationShip(org2, wheel);
			Helper.CreateProductClientRelationShip(org2, frame);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive2, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 5m);
			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 5m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceives = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceive1 = createdReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
			var createdReceive2 = createdReceives.Single(d => d.WD_OH_Client == org2.PK);
			AssertEquals(2, createdReceives.Length);
			AssertCreatedReceive(createdReceive1, order1);
			AssertCreatedReceive(createdReceive2, order2);
			var createdReceiveLine1 = createdReceive1.Lines[0];
			var createdReceiveLine2 = createdReceive2.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			// create new inventories for reallocation
			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveInventoryLine(receive3, wheel, 10m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive3, frame, 10m, data.Whs1.FindLocation("A-2"));
			receive3.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive3);

			var receive4 = Helper.CreateWhsReceive(org2, data.Whs1, "R4");
			Helper.CreateWhsReceiveInventoryLine(receive4, wheel, 10m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive4, frame, 10m, data.Whs1.FindLocation("A-2"));
			receive4.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive4);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(0, false), staff);
			Helper.Factory.Save();

			AssertEquals("Fully shorted.", true, framePickLine1.IsDeleted);
			AssertEquals("Fully shorted.", true, framePickLine2.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceives = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var reloadedReceive1 = reloadedReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
			var reloadedReceive2 = reloadedReceives.Single(d => d.WD_OH_Client == org2.PK);
			var reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive1.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive2.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine1.PickLines.Count);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine2.PickLines.Count);
			var linkQuery1 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine1.ChildComponentLines.Select(l => l.PK).ToArray());
			var linkQuery2 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine2.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery1).Length);
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery2).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLine1InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				var orderLine2InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				var notifications = new NotificationBuffer();
				var allocationResult = pickInAnotherFactory.AutoAllocateItems(pickInAnotherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray(), notifications);
				AssertEquals("Rellocated successfully.", AllocationResult.AllocatedStock, allocationResult);

				var orderLinesToReallocate = new WhsOrderLine[] { orderLine1InAnotherFactory, orderLine2InAnotherFactory };
				PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
				newFactory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedReceives = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
				reloadedReceive1 = reloadedReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
				reloadedReceive2 = reloadedReceives.Single(d => d.WD_OH_Client == org2.PK);
				reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				AssertCreatedReceive(reloadedReceive1, order1);
				AssertCreatedReceive(reloadedReceive2, order2);
				AssertEquals("1 Receive Line for each Receive.", 1, reloadedReceive1.Lines.Count);
				AssertEquals("1 Receive Line for each Receive.", 1, reloadedReceive2.Lines.Count);
				var links1 = newFactory.Load<WhsBOMInventoryPivot>(linkQuery1);
				var links2 = newFactory.Load<WhsBOMInventoryPivot>(linkQuery2);
				AssertEquals("Links created.", 2, links1.Length);
				AssertEquals("Links created.", 2, links2.Length);
				var reloadedWheelLink1 = links1.Single(l => l.WIP_ComponentQuantity == 10m);
				var reloadedFrameLink1 = links1.Single(l => l.WIP_ComponentQuantity == 5m);
				var reloadedWheelLink2 = links2.Single(l => l.WIP_ComponentQuantity == 10m);
				var reloadedFrameLink2 = links2.Single(l => l.WIP_ComponentQuantity == 5m);
				AssertEquals(wheelComponentLine1.PK, reloadedWheelLink1.WIP_WE_ComponentLine);
				AssertEquals(frameComponentLine1.PK, reloadedFrameLink1.WIP_WE_ComponentLine);
				AssertEquals(wheelComponentLine2.PK, reloadedWheelLink2.WIP_WE_ComponentLine);
				AssertEquals(frameComponentLine2.PK, reloadedFrameLink2.WIP_WE_ComponentLine);
				AssertCreatedReceiveLine(reloadedReceive1.Lines[0], bike, 5m);
				AssertCreatedReceiveLine(reloadedReceive2.Lines[0], bike, 5m);
				var reloadedPickLines1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedWheelLink1.WIP_WE_InventoryLine));
				var reloadedPickLines2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedWheelLink2.WIP_WE_InventoryLine));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines1.Length);
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines2.Length);
				var reloadedPickLine1 = reloadedPickLines1[0];
				var reloadedPickLine2 = reloadedPickLines2[0];
				AssertCreatedPickLine(reloadedPickLine1, reloadedReceive1.Lines[0], orderLine1, 5m);
				AssertCreatedPickLine(reloadedPickLine2, reloadedReceive2.Lines[0], orderLine2, 5m);
			}
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_MultipleKitLinesOnDifferentClients_PartillyAllocated

		public void TestRecreatePickByBOMReceiveLine_Reallocation_MultipleKitLinesOnDifferentClients_PartillyAllocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var org2 = Helper.CreateClient("XY9");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			Helper.CreateProductClientRelationShip(org2, bike);
			Helper.CreateProductClientRelationShip(org2, wheel);
			Helper.CreateProductClientRelationShip(org2, frame);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 20m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive2, frame, 20m, data.Whs1.FindLocation("A-1"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 5m);
			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 5m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.First().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine1.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine2.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine2.WZ_Units);

			var createdReceives = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceive1 = createdReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
			var createdReceive2 = createdReceives.Single(d => d.WD_OH_Client == org2.PK);
			AssertEquals(2, createdReceives.Length);
			AssertCreatedReceive(createdReceive1, order1);
			AssertCreatedReceive(createdReceive2, order2);
			var createdReceiveLine1 = createdReceive1.Lines[0];
			var createdReceiveLine2 = createdReceive2.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 5m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 5m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Links created.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			var wheelLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			var wheelLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink2 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink1, createdReceiveLine1, (WhsOrderLine)orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);
			AssertCreatedLink(wheelLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink2, createdReceiveLine2, (WhsOrderLine)orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK), 5m);

			// create new inventories for reallocation
			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveInventoryLine(receive3, wheel, 4m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive3, frame, 1m, data.Whs1.FindLocation("A-2"));
			receive3.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive3);

			var receive4 = Helper.CreateWhsReceive(org2, data.Whs1, "R4");
			Helper.CreateWhsReceiveInventoryLine(receive4, wheel, 7m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive4, frame, 3m, data.Whs1.FindLocation("A-2"));
			receive4.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive4);

			ConfirmPickLinesPickedQty(new[] { framePickLine1, framePickLine2 }, new PickingInfo(0, false), staff);
			Helper.Factory.Save();

			AssertEquals("Fully shorted.", true, framePickLine1.IsDeleted);
			AssertEquals("Fully shorted.", true, framePickLine2.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			var reloadedReceives = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var reloadedReceive1 = reloadedReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
			var reloadedReceive2 = reloadedReceives.Single(d => d.WD_OH_Client == org2.PK);
			var reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive1.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive2.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine1.PickLines.Count);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine2.PickLines.Count);
			var linkQuery1 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine1.ChildComponentLines.Select(l => l.PK).ToArray());
			var linkQuery2 = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine2.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery1).Length);
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery2).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLine1InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				var orderLine2InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				var notifications = new NotificationBuffer();
				var allocationResult = pickInAnotherFactory.AutoAllocateItems(pickInAnotherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray(), notifications);
				AssertEquals("Rellocated successfully but not fully.", AllocationResult.AllocatedStock, allocationResult);

				var orderLinesToReallocate = new WhsOrderLine[] { orderLine1InAnotherFactory, orderLine2InAnotherFactory };
				PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
				newFactory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedReceives = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
				reloadedReceive1 = reloadedReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
				reloadedReceive2 = reloadedReceives.Single(d => d.WD_OH_Client == org2.PK);
				reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				AssertCreatedReceive(reloadedReceive1, order1);
				AssertCreatedReceive(reloadedReceive2, order2);
				AssertEquals("1 Receive Line for each Receive.", 1, reloadedReceive1.Lines.Count);
				AssertEquals("1 Receive Line for each Receive.", 1, reloadedReceive2.Lines.Count);
				var links1 = newFactory.Load<WhsBOMInventoryPivot>(linkQuery1);
				var links2 = newFactory.Load<WhsBOMInventoryPivot>(linkQuery2);
				AssertEquals("Links created.", 2, links1.Length);
				AssertEquals("Links created.", 2, links2.Length);
				var reloadedWheelLink1 = links1.Single(l => l.WIP_ComponentQuantity == 2m);
				var reloadedFrameLink1 = links1.Single(l => l.WIP_ComponentQuantity == 1m);
				var reloadedWheelLink2 = links2.Single(l => l.WIP_ComponentQuantity == 6m);
				var reloadedFrameLink2 = links2.Single(l => l.WIP_ComponentQuantity == 3m);
				AssertEquals(wheelComponentLine1.PK, reloadedWheelLink1.WIP_WE_ComponentLine);
				AssertEquals(frameComponentLine1.PK, reloadedFrameLink1.WIP_WE_ComponentLine);
				AssertEquals(wheelComponentLine2.PK, reloadedWheelLink2.WIP_WE_ComponentLine);
				AssertEquals(frameComponentLine2.PK, reloadedFrameLink2.WIP_WE_ComponentLine);
				AssertCreatedReceiveLine(reloadedReceive1.Lines[0], bike, 1m);
				AssertCreatedReceiveLine(reloadedReceive2.Lines[0], bike, 3m);
				var reloadedPickLines1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedWheelLink1.WIP_WE_InventoryLine));
				var reloadedPickLines2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedWheelLink2.WIP_WE_InventoryLine));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines1.Length);
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, reloadedPickLines2.Length);
				var reloadedPickLine1 = reloadedPickLines1[0];
				var reloadedPickLine2 = reloadedPickLines2[0];
				AssertCreatedPickLine(reloadedPickLine1, reloadedReceive1.Lines[0], orderLine1, 1m);
				AssertCreatedPickLine(reloadedPickLine2, reloadedReceive2.Lines[0], orderLine2, 3m);
			}
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_ReassignPackTypeToPickLines

		public void TestRecreatePickByBOMReceiveLine_Reallocation_ReassignPackTypeToPickLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 100m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 20m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single();
			AssertEquals("Should be allocated.", 2, wheelComponentLine.PickLines.Count);
			var wheelPickLine1 = wheelComponentLine.PickLines.Single(l => l.WZ_Units == 36m);
			var wheelPickLine2 = wheelComponentLine.PickLines.Single(l => l.WZ_Units == 4m);
			AssertEquals("CTN", wheelPickLine1.WZ_F3_NKAllocatedPackType);
			AssertEquals("UNT", wheelPickLine2.WZ_F3_NKAllocatedPackType);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 20m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 2, createdPickLines.Length);
			var relatedPickLine1 = createdPickLines.Single(l => l.WZ_Units == 12m);
			var relatedPickLine2 = createdPickLines.Single(l => l.WZ_Units == 8m);
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine, 12m, "CTN");
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine1, orderLine, 8m, "UNT");
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 40m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 40m);

			// create new inventories for reallocation
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 30m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var newFactory0 = new BusinessObjectFactory() { RefreshEnabled = false };
			wheelPickLine1 = newFactory0.Load<WhsPickLine>(wheelPickLine1.PK);
			wheelPickLine2 = newFactory0.Load<WhsPickLine>(wheelPickLine2.PK);
			staff = newFactory0.Load<GlbStaff>(staff.PK);
			ConfirmPickLinesPickedQty(new[] { wheelPickLine1, wheelPickLine2 }, new PickingInfo(0, false), staff);
			AssertEquals("Precondition: Fully shorted.", true, wheelPickLine1.IsDeleted);
			AssertEquals("Precondition: Fully shorted.", true, wheelPickLine2.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Precondition: Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Precondition: Pick Line deleted due to Short Picking.", 0, reloadedOrderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, reloadedOrderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("Precondition: BOM Inventory Links deleted due to Short Picking.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLineInAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
				var notifications = new NotificationBuffer();
				var allocationResult = pickInAnotherFactory.AutoAllocateItems(pickInAnotherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray(), notifications);
				AssertEquals("Rellocated successfully.", AllocationResult.AllocatedStock, allocationResult);

				var orderLinesToReallocate = new WhsOrderLine[] { orderLineInAnotherFactory };
				PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
				newFactory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
				reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
				AssertCreatedReceive(reloadedReceive, order);
				var reloadedReceiveLine1 = reloadedReceive.Lines[0];
				AssertCreatedReceiveLine(reloadedReceiveLine1, bike, 15m);
				var reloadedPickLines = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine1.PK));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 2, reloadedPickLines.Length);
				relatedPickLine1 = reloadedPickLines.Single(l => l.WZ_Units == 12m);
				relatedPickLine2 = reloadedPickLines.Single(l => l.WZ_Units == 3m);
				AssertCreatedPickLine(relatedPickLine1, reloadedReceiveLine1, orderLine, 12m, "CTN");
				AssertCreatedPickLine(relatedPickLine2, reloadedReceiveLine1, orderLine, 3m, "UNT");
				AssertEquals("Links created.", 1, reloadedReceiveLine1.BOMComponentLinks.Count());
				var reloadedWheelLink = reloadedReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 30m);
				AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine1, (WhsOrderLine)reloadedOrderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK), 30m);
			}
		}

		#endregion

		#region TestRecreatePickByBOMReceiveLine_Reallocation_ReassignPackTypeToPickLines_TwoOrders

		public void TestRecreatePickByBOMReceiveLine_Reallocation_ReassignPackTypeToPickLines_TwoOrders()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 200m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 20m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 40m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock();
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 1, orderLine2.ChildComponentLines.Count);
			var wheelComponentLine1 = orderLine1.ChildComponentLines.Single();
			var wheelComponentLine2 = orderLine2.ChildComponentLines.Single();
			AssertEquals("Should be allocated.", 2, wheelComponentLine1.PickLines.Count);
			AssertEquals("Should be allocated.", 2, wheelComponentLine2.PickLines.Count);
			var wheelPickLine1 = wheelComponentLine1.PickLines.Single(l => l.WZ_Units == 36m);
			var wheelPickLine2 = wheelComponentLine1.PickLines.Single(l => l.WZ_Units == 4m);
			var wheelPickLine3 = wheelComponentLine2.PickLines.Single(l => l.WZ_Units == 72m);
			var wheelPickLine4 = wheelComponentLine2.PickLines.Single(l => l.WZ_Units == 8m);
			AssertEquals("CTN", wheelPickLine1.WZ_F3_NKAllocatedPackType);
			AssertEquals("UNT", wheelPickLine2.WZ_F3_NKAllocatedPackType);
			AssertEquals("CTN", wheelPickLine3.WZ_F3_NKAllocatedPackType);
			AssertEquals("UNT", wheelPickLine4.WZ_F3_NKAllocatedPackType);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order1);
			var createdReceiveLine1 = (WhsReceiveLine)createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 20m);
			var createdReceiveLine2 = (WhsReceiveLine)createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 40m);
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 20m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 40m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 2, createdPickLines1.Length);
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 2, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1.Single(l => l.WZ_Units == 12m);
			var relatedPickLine2 = createdPickLines1.Single(l => l.WZ_Units == 8m);
			var relatedPickLine3 = createdPickLines2.Single(l => l.WZ_Units == 36m);
			var relatedPickLine4 = createdPickLines2.Single(l => l.WZ_Units == 4m);
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 12m, "CTN");
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine1, orderLine1, 8m, "UNT");
			AssertCreatedPickLine(relatedPickLine3, createdReceiveLine2, orderLine2, 36m, "CTN");
			AssertCreatedPickLine(relatedPickLine4, createdReceiveLine2, orderLine2, 4m, "UNT");

			var newFactory0 = new BusinessObjectFactory() { RefreshEnabled = false };
			wheelPickLine1 = newFactory0.Load<WhsPickLine>(wheelPickLine1.PK);
			wheelPickLine2 = newFactory0.Load<WhsPickLine>(wheelPickLine2.PK);
			wheelPickLine3 = newFactory0.Load<WhsPickLine>(wheelPickLine3.PK);
			wheelPickLine4 = newFactory0.Load<WhsPickLine>(wheelPickLine4.PK);
			staff = newFactory0.Load<GlbStaff>(staff.PK);
			ConfirmPickLinesPickedQty(new[] { wheelPickLine1, wheelPickLine2, wheelPickLine3, wheelPickLine4 }, new PickingInfo(50m, false), staff);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			createdReceiveLine2 = (WhsReceiveLine)reloadedReceive.Lines.Single();
			orderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			orderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 25m);
			AssertEquals(0, orderLine1.PickLines.Count);
			AssertEquals(2, orderLine2.PickLines.Count);
			relatedPickLine3 = orderLine2.PickLines.Single(l => l.WZ_Units == 24m);
			relatedPickLine4 = orderLine2.PickLines.Single(l => l.WZ_Units == 1m);
			AssertCreatedPickLine(relatedPickLine3, createdReceiveLine2, orderLine2, 24m, "CTN");
			AssertCreatedPickLine(relatedPickLine4, createdReceiveLine2, orderLine2, 1m, "UNT");

			// create new inventories for reallocation
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 200m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var pickInAnotherFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLine1InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				var orderLine2InAnotherFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				var notifications = new NotificationBuffer();
				var allocationResult = pickInAnotherFactory.AutoAllocateItems(pickInAnotherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray(), notifications);
				AssertEquals("Rellocated successfully.", AllocationResult.AllocatedStock, allocationResult);

				var orderLinesToReallocate = new WhsOrderLine[] { orderLine1InAnotherFactory, orderLine2InAnotherFactory };
				PickLineUpdater.RecreatePickByBOMReceiveLine(pickInAnotherFactory, orderLinesToReallocate, LazyAllReleaseLinesForTest(orderLinesToReallocate), isShorting: false);
				newFactory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
				var reloadedOrderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				var reloadedOrderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				AssertCreatedReceive(reloadedReceive, order1);
				var reloadedReceiveLine1 = (WhsReceiveLine)reloadedReceive.Lines.Single(l => l.WE_TransactionQuantity == 20m);
				var reloadedReceiveLine2 = (WhsReceiveLine)reloadedReceive.Lines.Single(l => l.WE_TransactionQuantity == 40m);
				AssertCreatedReceiveLine(reloadedReceiveLine1, bike, 20m);
				AssertCreatedReceiveLine(reloadedReceiveLine2, bike, 40m);
				var reloadedPickLines1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine1.PK));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 2, reloadedPickLines1.Length);
				var reloadedPickLines2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine2.PK));
				AssertEquals("Should be committed to relevant Order Line through PickLine.", 2, reloadedPickLines2.Length);
				relatedPickLine1 = reloadedPickLines1.Single(l => l.WZ_Units == 12m);
				relatedPickLine2 = reloadedPickLines1.Single(l => l.WZ_Units == 8m);
				relatedPickLine3 = reloadedPickLines2.Single(l => l.WZ_Units == 36m);
				relatedPickLine4 = reloadedPickLines2.Single(l => l.WZ_Units == 4m);
				AssertCreatedPickLine(relatedPickLine1, reloadedReceiveLine1, orderLine1, 12m, "CTN");
				AssertCreatedPickLine(relatedPickLine2, reloadedReceiveLine1, orderLine1, 8m, "UNT");
				AssertCreatedPickLine(relatedPickLine3, reloadedReceiveLine2, orderLine2, 36m, "CTN");
				AssertCreatedPickLine(relatedPickLine4, reloadedReceiveLine2, orderLine2, 4m, "UNT");
			}
		}

		#endregion

		#region TestConfirmPickLinesPickedQty_AutoCreatePackage

		public void TestConfirmPickLinesPickedQty_AutoCreatePackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var packTypeInfo1 = new PickedPackTypeInfo("BOX", 2m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo2 = new PickedPackTypeInfo("BAG", 3m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo3 = new PickedPackTypeInfo("PLT", 4m, "", Array.Empty<WhsReleaseCapturedInfo>());

			var packInfo = new[] { new PickLinesToPickedPackTypeInfo(order.Lines[0].PickLines.Select(l => l.PK.ToGuid()).ToArray(), new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) };
			PickLineUpdater.ConfirmPickLinesPickedQty(order.Lines[0].PickLines.ToArray(), new PickingInfo(9m, false), staff, packInfo, null, createPackagesForPickedPacks: true);

			AssertEquals("Should have created package.", 3, order.PackageJob.Packages.Count);
			var package1 = order.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			var package2 = order.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			var package3 = order.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed);
			AssertEquals("Should have packed package.", 2m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have packed package.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have packed package.", 4m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));
		}

		#endregion

		#region TestCreateAndPackIntoPickedPackages

		public void TestCreateAndPackIntoPickedPackages_ThrowExceptionForGetOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew("PLT", "123");
			package1.Pack(order1.PackableItemParents.Typed.Single(), 5m);
			AssertEquals("Precondition: Package is packed.", 1, package1.PackedItemDivots.Count);
			Factory.Save();
			var pickByLabelJob1 = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order1.Pick.DockDoorLocation.PK, GlbStaff.CurrentUser.GS_Code, package1.PK);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew("PLT", "456");
			package2.Pack(order2.PackableItemParents.Typed.Single(), 5m);
			AssertEquals("Precondition: Package is packed.", 1, package2.PackedItemDivots.Count);
			Factory.Save();
			var pickByLabelJob2 = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order2.Pick.DockDoorLocation.PK, GlbStaff.CurrentUser.GS_Code, package2.PK);
			Factory.Save();

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();

			AssertExceptionThrown<ArgumentException>("Should throw exception when can't GetOrderLine for picked pickLine", "Should have a matching OrderLine", () => PickLineUpdater.ConfirmPickLinesPickedQty(
				new[] { pickLine1 },
				new PickingInfo(2m, false), GlbStaff.CurrentUser,
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				null,
				true));
		}

		#endregion

		#region AssertReceiveCreatedFromPickByBOM

		void AssertCreatedReceive(WhsReceive createdReceive, WhsOrder order)
		{
			AssertNotNull("Should created a new Receive.", createdReceive);
			AssertEquals(order.WD_WW_Whs, createdReceive.WD_WW_Whs);
			AssertEquals(order.WD_OH_Client, createdReceive.WD_OH_Client);
			AssertEquals((byte)0, createdReceive.WD_ExternalReferenceSplit);
			AssertEquals(0m, createdReceive.WD_TotalUnits);
			AssertEquals(0, createdReceive.WD_PackagesSent);
			AssertEquals((short)0, createdReceive.WD_TotalPallets);

			AssertNull("Should not create Billing Job for this Receive.", createdReceive.JobHeader);
		}

		void AssertCreatedReceiveLine(WhsReceiveLine createdReceiveLine1, OrgSupplierPart kit, decimal stockOnHand)
		{
			AssertEquals(kit.PK, createdReceiveLine1.WE_OP);
			AssertEquals(stockOnHand, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals(stockOnHand, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals(ZDateTimeOffset.Empty, createdReceiveLine1.WE_AdjustmentArrivalDate);
		}

		void AssertCreatedPickLine(WhsPickLine pickLine, WhsReceiveLine receiveLine, WhsOrderLine orderLine, ZDecimal units, string packType = "")
		{
			AssertEquals(orderLine.PK, pickLine.WZ_WE_TransactionLine);
			AssertEquals(receiveLine.PK, pickLine.WZ_WE_InventoryLine);
			AssertEquals(units, pickLine.WZ_Units);
			AssertEquals(receiveLine.WE_F3_NKPackType, pickLine.WZ_UnitsUQ);
			AssertEquals(0m, pickLine.WZ_OriginalReservedQty);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedPartAttrib2);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedPartAttrib3);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedSerialNumber);
			AssertEquals(packType, pickLine.WZ_F3_NKAllocatedPackType);
		}

		void AssertCreatedLink(WhsBOMInventoryPivot link, WhsReceiveLine receiveLine, WhsOrderLine orderLine, decimal componentQuantity)
		{
			AssertEquals(orderLine.PK, link.WIP_WE_ComponentLine);
			AssertEquals(receiveLine.PK, link.WIP_WE_InventoryLine);
			AssertEquals(componentQuantity, link.WIP_ComponentQuantity);
		}

		#endregion

		#region AssertWhsPickShortLineExists

		void AssertWhsPickShortLineExists(
			ZGuid inventoryLinePK,
			ZGuid transactionLinePK,
			ZDecimal units,
			ZString shortedBy,
			ZDateTime shortedTime)
		{
			var query = new ZQuery(WhsPickShortLineSchema.WZS_WE_InventoryLine, inventoryLinePK);
			query.AddToFilter(WhsPickShortLineSchema.WZS_WE_TransactionLine, transactionLinePK);
			query.AddToFilter(WhsPickShortLineSchema.WZS_ShortUnits, units);
			query.AddToFilter(WhsPickShortLineSchema.WZS_GS_NKShortedBy, shortedBy);
			query.AddToFilter(WhsPickShortLineSchema.WZS_ShortedDateTimeUtc, shortedTime);

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertEquals("Pick Short Line should exist", 1, factory.Load<WhsPickShortLine>(query)?.Length ?? 0);
		}

		#endregion

		#region ConfirmPickLinesPickedQty

		static IEnumerable<Guid> ConfirmPickLinesPickedQty(WhsPickLine[] lines, PickingInfo pickingInfo, GlbStaff picker)
		{
			var shortedOrderLinePKs = PickLineUpdater.ConfirmPickLinesPickedQty(lines, pickingInfo, picker, Array.Empty<PickLinesToPickedPackTypeInfo>(), null, createPackagesForPickedPacks: false);
			picker.Factory.Save();
			return shortedOrderLinePKs;
		}

		#endregion

		#region LazyAllReleaseLinesForTest

		Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> LazyAllReleaseLinesForTest(WhsOrderLine[] kitOrderLines)
			=> new Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>>(() => PackageHelper.GetReleaseLinesByKey(kitOrderLines));

		#endregion
	}
}
