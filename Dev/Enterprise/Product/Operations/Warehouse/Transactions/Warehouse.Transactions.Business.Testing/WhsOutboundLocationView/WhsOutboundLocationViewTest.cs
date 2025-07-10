using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOutboundLocationView))]
	internal class WhsOutboundLocationViewTest : WhsBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;

		[DeveloperOnlyTest]
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("Cannot save the factory for a view.", true);
		}

		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete dbo.WhsOutboundLocationView", false, GetNewBusinessObject().CanDelete);
		}

		public override void TestFetchForLoad()
		{
			var client = Helper.CreateClient("cO");
			var part = Helper.CreateProduct(client, "p1");
			var warehouse = Helper.CreateWarehouse("W1", "L1");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", part, 10m);
			Factory.Save();

			for (int i = 0; i < 10; i++)
			{
				var whsOrder = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O" + i, part, 1m);
				var p = whsOrder.PK;
				Helper.CreatePickNew(whsOrder);
				Factory.Save();
				var pickLine = whsOrder.Lines[0].PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.FinaliseDocketLine();
			}
			Factory.Save();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			factory1.Load<WhsOutboundLocationView>(new ZQuery());

			var dbHits = new Dictionary<string, int>();
			dbHits.Add(WhsOutboundLocationViewSchema.Constants.TableName, 1);
			AssertDbHits(dbHits, factory1);
		}

		public void TestOutboundLocationView_JustEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var orderRef = new Guid().ToString();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertEquals("Order Status is Entered.", DocketStatus.Codes.Entered, order.WarehouseOrderStatus);

			var ordersFromView = Factory.Load<WhsOutboundLocationView>(new ZQuery());
			AssertEquals("Just entered order should not be included in WhsOutboundLocationView", 0, ordersFromView.Length);
		}

		public void TestOutboundLocationView_AttachedToPickOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Order status is AttachedToPick.", DocketStatus.Codes.AttachedToPick, order.WarehouseOrderStatus);

			var ordersFromView = Factory.Load<WhsOutboundLocationView>(new ZQuery());
			AssertEquals("AttachedToPick order should not be included in WhsOutboundLocationView", 0, ordersFromView.Length);
		}

		public void TestOutboundLocationView_StagedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Order status is staged", WhsOrderStatus.Codes.Staged, order.WarehouseOrderStatus);

			var orderFromView = Factory.Load<WhsOutboundLocationView>(new ZQuery()).Single();
			AssertEquals("Staged order should be included in WhsOutboundLocationView", order.PK, orderFromView.WOU_WD_Order);
		}

		public void TestOutboundLocationView_ReadyToPackOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Order should be Ready To Pack.", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var orderFromView = Factory.Load<WhsOutboundLocationView>(new ZQuery()).Single();
			AssertEquals("Ready to pack order should be included in WhsOutboundLocationView", order.PK, orderFromView.WOU_WD_Order);
		}

		public void TestOutboundLocationView_LoadingOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();

			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);

			Factory.Save();

			AssertEquals("Order Status is Loading.", WhsOrderStatus.Codes.Loading, order.WarehouseOrderStatus);

			var orderFromView = Factory.Load<WhsOutboundLocationView>(new ZQuery()).Single();
			AssertEquals("Loading order should be included in WhsOutboundLocationView", order.PK, orderFromView.WOU_WD_Order);
		}

		public void TestOutboundLocationView_LoadedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var s = orderLine.PickLines.Single();
			var package1 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Order Status is Loaded.", WhsOrderStatus.Codes.Loaded, order.WarehouseOrderStatus);

			var orderFromView = Factory.Load<WhsOutboundLocationView>(new ZQuery()).Single();
			AssertEquals("Loaded order should be included in WhsOutboundLocationView", order.PK, orderFromView.WOU_WD_Order);
		}

		public void TestOutboundLocationView_DepartedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);

			Factory.Save();

			AssertEquals("Order Status is Departed.", WhsOrderStatus.Codes.Departed, order.WarehouseOrderStatus);

			var ordersFromView = Factory.Load<WhsOutboundLocationView>(new ZQuery());
			AssertEquals("Departed order should not be included in WhsOutboundLocationView", 0, ordersFromView.Length);
		}

		public void TestOutboundLocationView_PutawayOrderToMultipleOutboundLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("PST", "Packing", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("CON", "Consolidate", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CON", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = consolidationLocation.PK;

			transferLine1.FinaliseDocketLine();
			transferLine2.FinaliseDocketLine();

			Factory.Save();

			var ordersFromView = Factory.Load<WhsOutboundLocationView>(new ZQuery());
			AssertEquals("There should be two entities in WhsOutboundLocationView", 2, ordersFromView.Length);
		}
	}
}
