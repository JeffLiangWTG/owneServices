using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsPickInfo))]
	public class WhsPickInfoTestCase : WhsPickJobInfoTestCase<WhsPickInfo>
	{
		#region TestAdditionalConstructors

		public void TestAdditionalConstructors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100);
			Factory.Save();

			var emptypick = Helper.CreatePickNew();
			emptypick.WP_WW_Whs = data.Whs1.PK;
			var pickInfoForEmptyPick = new WhsPickInfo(emptypick, Array.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("", pickInfoForEmptyPick.Reference);
			AssertEquals(false, pickInfoForEmptyPick.IsMultiOrder);
			AssertEquals(0, pickInfoForEmptyPick.Lines.Count);
			AssertEquals(true, pickInfoForEmptyPick.IsPickByUOMTypeEnabled);

			data.Whs1.WW_IsPickByUOMEnabled = false;
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var nonEmptypick = Helper.CreatePickNew(order1);
			nonEmptypick.WP_PickNo = "TEST PICK";
			AssertEquals(1, nonEmptypick.Orders.Count);
			var pickInfoForSingleOrder = new WhsPickInfo(nonEmptypick, nonEmptypick.GetAllPickLines(), new List<string>());
			AssertEquals("TEST PICK", pickInfoForSingleOrder.Reference);
			AssertEquals(false, pickInfoForSingleOrder.IsMultiOrder);
			AssertEquals(1, pickInfoForSingleOrder.Lines.Count);
			AssertEquals(nonEmptypick.GetAllPickLines().Single().PK.ToGuid(), pickInfoForSingleOrder.Lines[0].PKs[0]);
			AssertEquals(false, pickInfoForSingleOrder.IsPickByUOMTypeEnabled);
		}

		public void TestAdditionalConstructors_MultiOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 15);
			var multiOrderPick = Helper.CreatePickNew(order1, order2);
			multiOrderPick.WP_PickNo = "TEST PICK";
			AssertEquals(2, multiOrderPick.Orders.Count);
			var pickInfoMultiOrder = new WhsPickInfo(multiOrderPick, multiOrderPick.GetAllPickLines(), new List<string>());
			AssertEquals("TEST PICK", pickInfoMultiOrder.Reference);
			AssertEquals(true, pickInfoMultiOrder.IsMultiOrder);
			AssertEquals(2, pickInfoMultiOrder.Lines.Count);
			var picklines = multiOrderPick.GetAllPickLines().ToArray();
			var line1 = picklines[0];
			var line2 = picklines[1];

			if (line1.PK == pickInfoMultiOrder.Lines[0].PKs[0])
			{
				AssertEquals(line2.PK.ToGuid(), pickInfoMultiOrder.Lines[1].PKs[0]);
			}
			else
			{
				AssertEquals(line2.PK.ToGuid(), pickInfoMultiOrder.Lines[0].PKs[0]);
				AssertEquals(line1.PK.ToGuid(), pickInfoMultiOrder.Lines[1].PKs[0]);
			}
		}

		#endregion

		#region TestConstructor_ScannedRCASerialNumbers

		public void TestConstructor_ScannedRCASerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			order.Lines[0].ReleaseLines.AddNew("", "", "", "S1", ZDate.Empty, ZDate.Empty, 1m);
			order.Lines[0].ReleaseLines.AddNew("", "", "", "S2", ZDate.Empty, ZDate.Empty, 1m);
			Factory.Save();

			var pickInfo = new WhsPickInfo(pick);
			var scannedRCASerialNumbersForPart1 = pickInfo.ScannedRCASerialNumbersPerProduct.Single(s => s.ProductPK == data.Part1.PK.ToGuid());
			AssertContainsExactElementsInAnyOrder(new[] { "S1", "S2" }, scannedRCASerialNumbersForPart1.ScannedRCASerialNumbers);
		}

		#endregion

		#region Properties

		#region TestPK

		public void TestPK()
		{
			AssertEquals(Guid.Empty, Parent.PK);

			var newGuid = Guid.NewGuid();
			Parent.PK = newGuid;
			AssertEquals(newGuid, Parent.PK);

			var data = new TestDataSimpleEnvironment(Factory);
			var pick = Helper.CreatePickNew();
			pick.WP_WW_Whs = data.Whs1.PK;
			var pickInfo = new WhsPickInfo(pick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("PK should be set to the Pick's PK.", pick.PK, pickInfo.PK);
		}

		#endregion

		#region TestPickPK

		protected override void TestPickPKsCore()
		{
			AssertContainsExactElementsInAnyOrder(Array.Empty<Guid>(), GetNewPickInfo().PickPKs);

			var data = new TestDataSimpleEnvironment(Factory);
			var pick = Helper.CreatePickNew();
			pick.WP_WW_Whs = data.Whs1.PK;

			var pickInfo = new WhsPickInfo(pick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertContainsExactElementsInAnyOrder("PickPK should be set to the Pick's PK.", new[] { pick.PK }, pickInfo.PickPKs);
		}

		#endregion

		#region TestReference

		public void TestReference()
		{
			AssertEquals("", Parent.Reference);

			Parent.Reference = "1234";
			AssertEquals("1234", Parent.Reference);

			Parent.Reference = "4321";
			AssertEquals("4321", Parent.Reference);
		}

		#endregion

		#region TestIsPickByBiggestPackTypeEnabled

		public void TestIsPickByBiggestPackTypeEnabled()
		{
			AssertEquals(false, Parent.IsPickByBiggestPackTypeEnabled);

			Parent.IsPickByBiggestPackTypeEnabled = true;
			AssertEquals(true, Parent.IsPickByBiggestPackTypeEnabled);

			Parent.IsPickByBiggestPackTypeEnabled = false;
			AssertEquals(false, Parent.IsPickByBiggestPackTypeEnabled);
		}

		#endregion

		#region TestIsPickByUOMTypeEnabled

		public void TestIsPickByUOMTypeEnabled()
		{
			AssertEquals(false, Parent.IsPickByUOMTypeEnabled);

			Parent.IsPickByUOMTypeEnabled = true;
			AssertEquals(true, Parent.IsPickByUOMTypeEnabled);

			Parent.IsPickByUOMTypeEnabled = false;
			AssertEquals(false, Parent.IsPickByUOMTypeEnabled);
		}

		#endregion

		#region TestIsPickHasBOMEnabledProduct

		public void TestIsPickHasBOMEnabledProduct()
		{
			AssertEquals(false, Parent.IsPickHasBOMEnabledProduct);

			Parent.IsPickHasBOMEnabledProduct = true;
			AssertEquals(true, Parent.IsPickHasBOMEnabledProduct);

			Parent.IsPickHasBOMEnabledProduct = false;
			AssertEquals(false, Parent.IsPickHasBOMEnabledProduct);
		}

		public void TestGetIsPickHasBOMEnabledProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var normalProduct = Helper.CreateProduct(data.Org1, "NP1");
			var bomEnabledProduct = Helper.CreateProduct(data.Org1, "BOM1");
			bomEnabledProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "COM1");
			var bomPartForMainProduct1 = Helper.CreateProductBOM(bomEnabledProduct, bomComponentProduct1, 3m, PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, bomEnabledProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, normalProduct, 5m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine11 = Helper.CreateWhsOrderLine(order1, bomEnabledProduct, 7m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine21 = Helper.CreateWhsOrderLine(order2, normalProduct, 5m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var componentOrderLine = orderLine11.ChildComponentLines.Single();
			var pickInfo1 = new WhsPickInfo(pick1, componentOrderLine.PickLines, new List<string>());
			AssertEquals(true, pickInfo1.IsPickHasBOMEnabledProduct);

			var pick2 = Helper.CreatePickNew(order2);
			var pickInfo2 = new WhsPickInfo(pick2, pick2.GetAllPickLines(), new List<string>());
			AssertEquals(false, pickInfo2.IsPickHasBOMEnabledProduct);
		}

		#endregion

		#region TestIsUsingDirectedPackingConsolidation

		public void TestIsUsingDirectedPackingConsolidation_EmptyPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var pick = Helper.CreatePickNew();
			pick.WP_WW_Whs = data.Whs1.PK;

			var pickInfo = new WhsPickInfo(pick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should *not* be using directed packing consolidation.", false, pickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_SingleOrder_UsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_SingleOrderCore(true);
		}

		public void TestIsUsingDirectedPackingConsolidation_SingleOrder_NotUsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_SingleOrderCore(false);
		}

		void TestIsUsingDirectedPackingConsolidation_SingleOrderCore(bool isUsingDirectedPackingConsolidation)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = isUsingDirectedPackingConsolidation;
			var orderPick = Helper.CreatePickNew(order);

			Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var orderPickInfo = new WhsPickInfo(orderPick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should only be using directed packing consolidation if order on pick is.",
				isUsingDirectedPackingConsolidation,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_MultiOrder_UsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_MultiOrderCore(true);
		}

		public void TestIsUsingDirectedPackingConsolidation_MultiOrder_NotUsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_MultiOrderCore(false);
		}

		void TestIsUsingDirectedPackingConsolidation_MultiOrderCore(bool isUsingDirectedPackingConsolidation)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = false;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.WD_UseDirectedPackingConsolidation = isUsingDirectedPackingConsolidation;

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			order3.WD_UseDirectedPackingConsolidation = false;
			var orderPick = Helper.CreatePickNew(order1, order2, order3);

			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);

			var package3 = order3.PackageJob.Packages.AddNew();
			package3.Pack(order3.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var orderPickInfo = new WhsPickInfo(orderPick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should only be using directed packing consolidation if any order on pick is.",
				isUsingDirectedPackingConsolidation,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		protected override void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			order.WD_UseDirectedPackingConsolidation = true;

			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderline1.ReleaseLines[0], 5m);

			Factory.Save();

			var orderPickInfo = new WhsPickInfo(pick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should NOT use directed packing consolidation if order on pick has loose inventory.",
				false,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		protected override void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryAlreadyPutawayCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			order.WD_UseDirectedPackingConsolidation = true;

			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderline1.ReleaseLines[0], 5m);

			Factory.Save();

			var pickLine = orderline2.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();

			Factory.Save();

			var orderPickInfo = new WhsPickInfo(pick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should NOT use directed packing consolidation if order on pick has loose inventory.",
				false,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_PickAndPack()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_IsPickAndPackEnabled = true;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2.PK, 5m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = new PackingTestHelper(Factory).CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var orderPickInfo = new WhsPickInfo(pick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should use directed packing consolidation if the pick is using pick and pack.",
				true,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_PickAndPack_NotUsingPackingConsolidation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_IsPickAndPackEnabled = true;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2.PK, 5m);
			order.WD_UseDirectedPackingConsolidation = false;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = new PackingTestHelper(Factory).CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var orderPickInfo = new WhsPickInfo(pick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should NOT use directed packing consolidation if order on pick is not using Packing Consolidation.",
				false,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_PickAndPack_PickHasAssignedLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_IsPickAndPackEnabled = true;

			var packingLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2.PK, 5m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			pick.WP_WL_PackingStation = packingLocation.PK;

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = new PackingTestHelper(Factory).CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var orderPickInfo = new WhsPickInfo(pick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should NOT use directed packing consolidation if the pick is assigned a Packing Location.",
				false,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_WithSomePartsAlreadyPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = false;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.WD_UseDirectedPackingConsolidation = true;

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			order3.WD_UseDirectedPackingConsolidation = true;
			var orderPick = Helper.CreatePickNew(order1, order2, order3);

			var pickLine = order1.Lines.Single().PickLines.Single();
			var transferLinePickLine1 = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLinePickLine1.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			transferLinePickLine1.FinaliseDocketLine();
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);

			var package3 = order3.PackageJob.Packages.AddNew();
			package3.Pack(order3.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var pickInfo = new WhsPickInfo(orderPick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should not be using directed packing consolidation if part of the job is already putaway.",
				false,
				pickInfo.IsUsingDirectedPackingConsolidation);
		}

		#endregion

		#region TestIsMultiOrder

		public void TestIsMultiOrder()
		{
			AssertEquals(false, Parent.IsMultiOrder);

			Parent.IsMultiOrder = true;
			AssertEquals(true, Parent.IsMultiOrder);

			Parent.IsMultiOrder = false;
			AssertEquals(false, Parent.IsMultiOrder);
		}

		#endregion

		#region TestIsWorkOrderPick

		public void TestIsWorkOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var bomPart1 = Helper.CreateProductBOM(data.Part2, componentProduct1, 1m, Constants.PkgUnit.Unit);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderPick = Helper.CreatePickNew(order);

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part2, 10m);
			var workOrderPick = Helper.CreatePickNew(workOrder);

			var orderPickInfo = new WhsPickInfo(orderPick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should *not* be a work order pick.", false, orderPickInfo.IsWorkOrderPick);

			var workOrderPickInfo = new WhsPickInfo(workOrderPick, Enumerable.Empty<WhsPickLine>(), new List<string>());
			AssertEquals("Should be a work order pick.", true, workOrderPickInfo.IsWorkOrderPick);
		}

		#endregion

		#region TestLines

		public void TestLines()
		{
			AssertNotNull(Parent.Lines);
			AssertEquals(0, Parent.Lines.Count);

			var line1 = new WhsPickLineInfo();
			var line2 = new WhsPickLineInfo();
			AssertNotEquals(line1, line2);
			Parent.Lines.Add(line1);
			Parent.Lines.Add(line2);
			AssertCollectionContains(line1, Parent.Lines);
			AssertCollectionContains(line2, Parent.Lines);

			var lines = new WhsPickLineInfoCollection();
			lines.Add(new WhsPickLineInfo());
			lines.Add(new WhsPickLineInfo());
			AssertNotEquals(Parent.Lines, lines);
		}

		#endregion

		#region TestOrders

		public new void TestOrders()
		{
			AssertNotNull(Parent.Orders);
			AssertEquals(0, Parent.Orders.Count);

			var order1 = new WhsDocketInfo();
			var order2 = new WhsDocketInfo();
			AssertNotEquals(order1, order2);
			Parent.Orders.Add(order1);
			Parent.Orders.Add(order2);
			AssertCollectionContains(order1, Parent.Orders);
			AssertCollectionContains(order2, Parent.Orders);

			var orders = new WhsDocketInfoCollection();
			orders.Add(new WhsDocketInfo());
			orders.Add(new WhsDocketInfo());
			AssertNotEquals(Parent.Orders, orders);
			Parent.Orders = orders;
			AssertEquals(Parent.Orders, orders);
		}

		#endregion

		#region TestCompletePalletPickingPallets

		public void TestCompletePalletPickingPallets()
		{
			AssertNotNull(Parent.CompletePalletPickingPallets);

			Parent.CompletePalletPickingPallets.Add("PLT-1");
			Parent.CompletePalletPickingPallets.Add("PLT-2");
			AssertCollectionContains("PLT-1", Parent.CompletePalletPickingPallets);
			AssertCollectionContains("PLT-2", Parent.CompletePalletPickingPallets);

			var completePalletPickingPallets = new List<string>();
			AssertNotEquals(Parent.CompletePalletPickingPallets, completePalletPickingPallets);

			Parent.CompletePalletPickingPallets = completePalletPickingPallets;
			AssertEquals(Parent.CompletePalletPickingPallets, completePalletPickingPallets);
		}

		#endregion

		#region TestUnitConversionsPerProduct

		public new void TestUnitConversionsPerProduct()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			// 2 lines for first product 
			helper.CreateWhsOrderLine(order, data.Part1, 5);
			helper.CreateWhsOrderLine(order, data.Part1, 7);
			// 1 line for second product
			helper.CreateWhsOrderLine(order, data.Part2, 7);
			Factory.Save();

			var pick = helper.CreatePickNew(order);
			var pickLinesCollection = new List<WhsPickLine>();
			pickLinesCollection.AddRange(pick.GetAllPickLines());
			AssertEquals("Precondition - 3 picklines should be created", 3, pickLinesCollection.Count);

			var pickInfo = new WhsPickInfo(pick, pickLinesCollection, new List<string>());
			AssertEquals("Should have 2 UnitConversionCollections. One for each distinct product", 2, pickInfo.UnitConversionsPerProduct.Count);
			AssertContainsExactElementsInAnyOrder(new[] { data.Part1.PK, data.Part2.PK }, pickInfo.UnitConversionsPerProduct.Select(u => u.ProductPK));
		}

		#endregion

		#region TestProductInfos

		public new void TestProductInfos()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			// 2 lines for first product 
			helper.CreateWhsOrderLine(order, data.Part1, 5);
			helper.CreateWhsOrderLine(order, data.Part1, 7);
			// 1 line for second product
			helper.CreateWhsOrderLine(order, data.Part2, 7);
			Factory.Save();

			var pick = helper.CreatePickNew(order);
			var pickLinesCollection = new List<WhsPickLine>();
			pickLinesCollection.AddRange(pick.GetAllPickLines());
			AssertEquals("Precondition - 3 picklines should be created", 3, pickLinesCollection.Count);

			var pickInfo = new WhsPickInfo(pick, pickLinesCollection, new List<string>());
			AssertEquals("Should have 2 ProductInfos. One for each distinct product", 2, pickInfo.ProductInfos.Count);
			AssertContainsExactElementsInAnyOrder(new[] { data.Part1.PK, data.Part2.PK }, pickInfo.ProductInfos.Select(u => u.PK));
		}

		public void TestProductInfos_BOMProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 100m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 10m);

			pick.AddOrders(new[] { order1 });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var wheelPickLine = orderLine1.ChildComponentLines.Single().PickLines[0];
			var pickLinesCollection = new List<WhsPickLine>();
			var pickInfo = new WhsPickInfo(pick, new WhsPickLine[] { wheelPickLine }, new List<string>());
			AssertEquals("Should have 2 ProductInfos, for wheel and bike.", 2, pickInfo.ProductInfos.Count);
			AssertContainsExactElementsInAnyOrder(new[] { wheel.PK, bike.PK }, pickInfo.ProductInfos.Select(u => u.PK));
		}

		#endregion

		#region TestProductPartAttributesInfos

		public new void TestProductPartAttributesInfos()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part2);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10);
			helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R3", data.Part2, 10);

			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1);
			helper.CreateWhsOrderLine(order1, data.Part1, 2);
			helper.CreateWhsOrderLine(order1, data.Part1, 2);
			helper.CreateWhsOrderLine(order1, data.Part1, 2);
			helper.CreateWhsOrderLine(order1, data.Part1, 2);
			helper.CreateWhsOrderLine(order1, data.Part1, 2);
			helper.CreateWhsOrderLine(order1, data.Part2, 10);

			var order2 = helper.CreateWhsOrder(client2, data.Whs1);
			helper.CreateWhsOrderLine(order2, data.Part2, 4);
			helper.CreateWhsOrderLine(order2, data.Part2, 6);
			Factory.Save();

			var pick = helper.CreatePickNew(order1, order2);
			var pickLinesCollection = new List<WhsPickLine>();
			pickLinesCollection.AddRange(pick.GetAllPickLines());
			AssertEquals("Precondition - 8 picklines should be created", 8, pickLinesCollection.Count);
			var pickInfo = new WhsPickInfo(pick, pickLinesCollection, new List<string>());
			AssertEquals("Should have 3 ProductPartAttributesInfos. One for each pair product+client", 3, pickInfo.ProductPartAttributesInfos.Count);
			AssertEquals(1, pickInfo.ProductPartAttributesInfos.Count(pp => pp.ClientPK == data.Org1.PK && pp.ProductPK == data.Part1.PK));
			AssertEquals(1, pickInfo.ProductPartAttributesInfos.Count(pp => pp.ClientPK == data.Org1.PK && pp.ProductPK == data.Part2.PK));
			AssertEquals(1, pickInfo.ProductPartAttributesInfos.Count(pp => pp.ClientPK == client2.PK && pp.ProductPK == data.Part2.PK));
		}

		#endregion

		#region TestIsPutawayOnly

		protected override void TestIsPutawayOnlyCore()
		{
			var pickJobInfo1 = GetNewPickInfo();
			AssertEquals("IsPutawayOnly", false, pickJobInfo1.IsPutawayOnly);

			var data = new TestDataSimpleEnvironment(Factory);
			var pick = Helper.CreatePickNew();
			pick.WP_WW_Whs = data.Whs1.PK;

			var pickJobInfo2 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("IsPutawayOnly", false, pickJobInfo1.IsPutawayOnly);

			var pickJobInfo3 = new WhsPickInfo(pick);
			AssertEquals("IsPutawayOnly", true, pickJobInfo3.IsPutawayOnly);
		}

		#endregion

		#region TestDockDoorLocation

		protected override void TestDockDoorLocationCore()
		{
			var pickJobInfo1 = GetNewPickInfo();
			AssertEquals("DockDoorLocation", string.Empty, pickJobInfo1.DockDoorLocation);

			var data = new TestDataSimpleEnvironment(Factory);
			var pickNoDockDoor = Helper.CreatePickNew();
			pickNoDockDoor.WP_WW_Whs = data.Whs1.PK;
			pickNoDockDoor.WP_WL_DockDoor = ZGuid.Empty;
			AssertEquals("Precondition", ZGuid.Empty, pickNoDockDoor.WP_WL_DockDoor);

			var pickJobInfo2 = new WhsPickInfo(pickNoDockDoor, pickNoDockDoor.GetAllPickLines(), new List<string>());
			AssertEquals("DockDoorLocation", string.Empty, pickJobInfo2.DockDoorLocation);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);

			var pickJobInfo3 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("DockDoorLocation", pick.DockDoorLocation.WLV_LocationString, pickJobInfo3.DockDoorLocation);

			pick.WP_WL_DockDoor = data.Whs1.DefaultLocation.PK;
			AssertNotEquals("Precondition", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);

			var pickJobInfo4 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("DockDoorLocation", pick.DockDoorLocation.WLV_LocationString, pickJobInfo4.DockDoorLocation);
		}

		protected override void TestDockDoorLocation_FixedWidthLocationCore()
		{
			var pickJobInfo1 = GetNewPickInfo();
			AssertEquals("DockDoorLocation", string.Empty, pickJobInfo1.DockDoorLocation);

			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var dockdoorLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, "DDL"));
			var location1 = warehouse.FindLocation("Z030201");
			location1.WLV_WLT_LocationType = dockdoorLocationType.PK;
			location1.WLV_LocationStatus = "NOR";
			var location2 = warehouse.FindLocation("Z040302");
			location2.WLV_WLT_LocationType = dockdoorLocationType.PK;
			location2.WLV_LocationStatus = "NOR";
			Factory.Save();

			warehouse.WW_DefaultInboundDockDoor = location1.PK;
			warehouse.WW_DefaultOutboundDockDoor = location1.PK;
			Factory.Save();

			var pickNoDockDoor = Helper.CreatePickNew();
			pickNoDockDoor.WP_WW_Whs = warehouse.PK;
			pickNoDockDoor.WP_WL_DockDoor = ZGuid.Empty;
			AssertEquals("Precondition", ZGuid.Empty, pickNoDockDoor.WP_WL_DockDoor);

			var pickJobInfo2 = new WhsPickInfo(pickNoDockDoor, pickNoDockDoor.GetAllPickLines(), new List<string>());
			AssertEquals("DockDoorLocation", string.Empty, pickJobInfo2.DockDoorLocation);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, "O1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", location1.PK, pick.WP_WL_DockDoor);

			var pickJobInfo3 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("DockDoorLocation", "Z030201", pickJobInfo3.DockDoorLocation);
			AssertEquals("DockDoorLocation", "Z-03-02-01", pickJobInfo3.DockDoorLocation_UserFriendly);

			pick.WP_WL_DockDoor = location2.PK;
			AssertNotEquals("Precondition", location1.PK, pick.WP_WL_DockDoor);

			var pickJobInfo4 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("DockDoorLocation", "Z040302", pickJobInfo4.DockDoorLocation);
			AssertEquals("DockDoorLocation", "Z-04-03-02", pickJobInfo4.DockDoorLocation_UserFriendly);
		}

		#endregion

		#region TestIsPackingStationAllowed

		protected override void TestIsPackingStationAllowedCore(bool hasPackingStationLocation)
		{
			var pickJobInfo1 = GetNewPickInfo();
			AssertEquals("HasPackingStation", false, pickJobInfo1.IsPackingStationAllowed);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			if (hasPackingStationLocation)
			{
				var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
				var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
				var packingLocation = newRow.Locations[0];
				packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
				Helper.Factory.Save();
			}

			var pickJobInfo2 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("HasPackingStation", hasPackingStationLocation, pickJobInfo2.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_PackingStationIsInvalidLocationStatusCore(string locationStatus)
		{
			var pickJobInfo1 = GetNewPickInfo();
			AssertEquals("HasPackingStation", false, pickJobInfo1.IsPackingStationAllowed);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			packingLocation.WLV_LocationStatus = locationStatus;
			Helper.Factory.Save();

			var pickJobInfo2 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("HasPackingStation", false, pickJobInfo2.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_PickWithAssignedPackingStationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			pick.WP_WL_PackingStation = packingLocation.PK;

			Factory.Save();

			var pickJobInfo = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("HasPackingStation", true, pickJobInfo.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_PickByBOMCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var componentOrderLine = order.Lines.Single(l => l.WE_OP == data.Part2.PK);

			var pickJobInfo = new WhsPickInfo(pick, componentOrderLine.PickLines, new List<string>());
			AssertEquals(true, pickJobInfo.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_DBHitsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var count = 10;
			var mainProducts = new List<OrgSupplierPart>();
			var subProducts = new List<OrgSupplierPart>();
			var orders = new List<WhsOrder>();

			for (var i = 0; i < count; i++)
			{
				var mainProduct = Helper.CreateProduct(data.Org1, $"P1{i}_main");
				var subProduct = Helper.CreateProduct(data.Org1, $"P1{i}_sub");
				Helper.CreateProductBOM(mainProduct, subProduct);
				mainProduct.OP_IsComponentPickedOnSalesOrder = true;

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}_main", mainProduct, 10m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}_sub", subProduct, 10m);

				mainProducts.Add(mainProduct);
				subProducts.Add(subProduct);
			}
			Factory.Save();

			for (var i = 0; i < count; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{i}", mainProducts[i], 20m);
				orders.Add(order);
			}

			var pick = Helper.CreatePickNew(orders.ToArray());
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 }
			};

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var subProductPKs = subProducts.Select(p => p.PK).ToHashSet();
			var pickLinesInNewFactory = pickInNewFactory.GetAllPickLines().Where(pl => subProductPKs.Contains(pl.DocketLine.WE_OP)).ToArray();

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var pickJobInfo = new WhsPickInfo(pickInNewFactory, pickLinesInNewFactory, new List<string>());
				AssertEquals(true, pickJobInfo.IsPackingStationAllowed);
			}
		}

		#endregion

		#region TestPSTLocationPK

		protected override void TestPackingStationPKCore()
		{
			var pickJobInfo1 = GetNewPickInfo();
			AssertEquals("No Packing Station for Default", Guid.Empty, pickJobInfo1.PackingStationPK);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			pick.WP_WL_PackingStation = packingLocation.PK;
			Helper.Factory.Save();

			var pickJobInfo2 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("Packing Station PK is Correct", packingLocation.PK.ToGuid(), pickJobInfo2.PackingStationPK);
		}

		#endregion

		#region TestAllowPickDockDoorLocationOverride

		protected override void TestAllowPickDockDoorLocationOverrideCore()
		{
			var pickJobInfo1 = GetNewPickInfo();
			AssertEquals("AllowPickDockDoorLocationOverride", false, pickJobInfo1.AllowPickDockDoorLocationOverride);

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = false;
			Factory.Save();

			var pickJobInfo2 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("AllowPickDockDoorLocationOverride", false, pickJobInfo2.AllowPickDockDoorLocationOverride);

			var pickJobInfo3 = new WhsPickInfo(pick);
			AssertEquals("AllowPickDockDoorLocationOverride", false, pickJobInfo3.AllowPickDockDoorLocationOverride);

			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			var pickJobInfo4 = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("AllowPickDockDoorLocationOverride", true, pickJobInfo4.AllowPickDockDoorLocationOverride);

			var pickJobInfo5 = new WhsPickInfo(pick);
			AssertEquals("AllowPickDockDoorLocationOverride", true, pickJobInfo5.AllowPickDockDoorLocationOverride);
		}

		protected override void TestAllowPickDockDoorLocationOverride_LinkedPicksCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingHelper = new PackingTestHelper(Factory);

			var pickPackParam1 = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);

			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part2);

			var pickPackParam2 = Helper.CreatePickPackParameter(client2, data.Whs1);
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, data.Part2, 20m);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 20m);

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			var pickJobInfo11 = new WhsPickInfo(pick1);
			AssertEquals("AllowPickDockDoorLocationOverride pick1", false, pickJobInfo11.AllowPickDockDoorLocationOverride);

			var pickJobInfo21 = new WhsPickInfo(pick2);
			AssertEquals("AllowPickDockDoorLocationOverride pick2", false, pickJobInfo21.AllowPickDockDoorLocationOverride);

			// Enable 1 param
			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			var pickJobInfo12 = new WhsPickInfo(pick1);
			AssertEquals("AllowPickDockDoorLocationOverride pick1", false, pickJobInfo12.AllowPickDockDoorLocationOverride);

			var pickJobInfo22 = new WhsPickInfo(pick2);
			AssertEquals("AllowPickDockDoorLocationOverride pick2", false, pickJobInfo22.AllowPickDockDoorLocationOverride);

			// Enable both params
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			var pickJobInfo13 = new WhsPickInfo(pick1);
			AssertEquals("AllowPickDockDoorLocationOverride pick1", true, pickJobInfo13.AllowPickDockDoorLocationOverride);

			var pickJobInfo23 = new WhsPickInfo(pick2);
			AssertEquals("AllowPickDockDoorLocationOverride pick2", true, pickJobInfo23.AllowPickDockDoorLocationOverride);

			dda.WDA_FirstPutawayToDockDoorUtc = ZDateTime.UtcNow;
			Factory.Save();

			var pickJobInfo14 = new WhsPickInfo(pick1);
			AssertEquals("AllowPickDockDoorLocationOverride pick1, false is DDA WDA_FirstPutawayToDockDoorUtc set", false, pickJobInfo14.AllowPickDockDoorLocationOverride);

			var pickJobInfo24 = new WhsPickInfo(pick2);
			AssertEquals("AllowPickDockDoorLocationOverride pick2, false is DDA WDA_FirstPutawayToDockDoorUtc set", false, pickJobInfo24.AllowPickDockDoorLocationOverride);
		}

		public void TestAllowPickDockDoorLocationOverride_StockInDDL()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingHelper = new PackingTestHelper(Factory);

			var pickPackParam1 = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var dockDoorAssignment = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			Helper.Factory.Save();

			dockDoorAssignment.WDA_FirstPutawayToDockDoorUtc = ZDateTime.UtcNow;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition: WDA_FirstPutawayToDockDoorUtc.IsValid", true, dockDoorAssignment.WDA_FirstPutawayToDockDoorUtc.IsValid);

			var pickJobInfo = new WhsPickInfo(pick);
			AssertEquals("AllowPickDockDoorLocationOverride pick is false some stock is in DDL", false, pickJobInfo.AllowPickDockDoorLocationOverride);
		}

		#endregion

		#region TestAssignedPutawayLocationCore

		protected override void TestAssignedPutawayLocation_PackingStationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", packingStationLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			pick.WP_WL_PackingStation = packingStationLocation.PK;

			Helper.Factory.Save();

			var pickJobInfo = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("Putaway Station Location String is Correct", packingStationLocation.WLV_LocationString, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", packingStationLocation.WLV_LocationString_UserFriendly, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", packingStationLocation.WLV_LocationClass, pickJobInfo.AssignedPutawayLocationClass);
		}

		protected override void TestAssignedPutawayLocation_DockDoorCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = dockdoorLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			Helper.Factory.Save();

			var pickJobInfo = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("Putaway Station Location String is Correct", dockdoorLocation.WLV_LocationString, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", dockdoorLocation.WLV_LocationString_UserFriendly, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", dockdoorLocation.WLV_LocationClass, pickJobInfo.AssignedPutawayLocationClass);
		}

		protected override void TestAssignedPutawayLocation_ConsolidationLocationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", consolidationLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			Helper.Factory.Save();

			var pickJobInfo = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("Putaway Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Location Class String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		protected override void TestAssignedPutawayLocation_NothingPutawayYetCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var pickJobInfo = new WhsPickInfo(pick, pick.GetAllPickLines(), new List<string>());
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		public void TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJob_PickByTote()
		{
			TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJobCore(isPickByTote: true);
		}

		public void TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJob_PickByCarton()
		{
			TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJobCore(isPickByTote: false);
		}

		void TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJobCore(bool isPickByTote)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var packingHelper = new PackingTestHelper(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(isPickByTote);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine1);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			Helper.Factory.Save();

			var pickJobInfo = new WhsPickInfo(pick, new[] { pickLine2 }, new List<string>());
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", isPickByTote ? LocationClasses.Codes.PST : string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		public void TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJob_NoPackingStations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingHelper = new PackingTestHelper(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine1);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			Helper.Factory.Save();

			var pickJobInfo = new WhsPickInfo(pick, new[] { pickLine2 }, new List<string>());
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		#endregion

		#endregion

		#region Implementation

		protected new WhsPickInfo Parent
		{
			get { return base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsPickInfo();
		}

		protected override WhsPickInfo GetNewPickInfo()
		{
			return new WhsPickInfo();
		}

		#endregion
	}
}
