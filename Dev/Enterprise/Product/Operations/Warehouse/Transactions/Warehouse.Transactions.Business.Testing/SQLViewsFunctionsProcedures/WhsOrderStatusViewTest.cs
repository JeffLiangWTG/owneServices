using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderStatusViewTest : WhsTestCaseWithFactory
	{
		public void TestView_NoOrders()
		{
			Helper.CreatePickNew();
			Factory.Save();

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 0, viewInfo.Length);
		}

		public void TestView_FinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 1, viewInfo.Length);
			AssertEquals("Order PK correct", order.PK, viewInfo[0]["WOS_PK"]);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Departed, viewInfo[0]["WOS_OrderStatus"]);
			AssertEquals("Docket Status correct", WhsOrderStatus.Codes.Departed, viewInfo[0]["WOS_DocketStatus"]);
		}

		public void TestView_FullyPickedInTransitOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories[0];
			var pickAvailableInventory = orderedInventory.AvailableInventories[0];
			var splitAvailableInventory = pickAvailableInventory.AvailableInventoriesSplitByPickedDetails[0];
			splitAvailableInventory.PickedDate = DateTime.Now;
			Factory.Save();

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 1, viewInfo.Length);
			AssertEquals("Order PK correct", order.PK, viewInfo[0]["WOS_PK"]);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Staged, viewInfo[0]["WOS_OrderStatus"]);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, viewInfo[0]["WOS_DocketStatus"]);
		}

		public void TestView_FullyPickedInTransitOrder_UseDirectedPackingConsolidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories[0];
			var pickAvailableInventory = orderedInventory.AvailableInventories[0];
			var splitAvailableInventory = pickAvailableInventory.AvailableInventoriesSplitByPickedDetails[0];
			splitAvailableInventory.PickedDate = DateTime.Now;
			Factory.Save();

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 1, viewInfo.Length);
			AssertEquals("Order PK correct", order.PK, viewInfo[0]["WOS_PK"]);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.ReadyToPack, viewInfo[0]["WOS_OrderStatus"]);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, viewInfo[0]["WOS_DocketStatus"]);
		}

		public void TestView_PartiallyPickedInTransitOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 50m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories[0];
			var pickAvailableInventory = orderedInventory.AvailableInventories[0];
			var splitAvailableInventory = pickAvailableInventory.AvailableInventoriesSplitByPickedDetails[0];
			splitAvailableInventory.PickedDate = DateTime.Now;
			Factory.Save();

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 1, viewInfo.Length);
			AssertEquals("Order PK correct", order.PK, viewInfo[0]["WOS_PK"]);
			AssertEquals("Order Status correct", DocketStatus.Codes.Picking, viewInfo[0]["WOS_OrderStatus"]);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, viewInfo[0]["WOS_DocketStatus"]);
		}

		public void TestView_HasUnloadedPackage_AndLoadedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;

			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKUnloadingUser = "E";
			loadPkgPackagePivot2.WLP_UnloadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Pick not finalised", false, pick.IsFinalised);
			Factory.Save();
			var orderStatusView = GetOrderStatusForOrderPK(order.PK);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loading, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_HasTwoLoads_HasUnloadedPackage_AndLoadedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck1 = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck1, startTime: DateTimeOffset.Now);
			var truck2 = Helper.CreateEquipment("T002", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck2, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load1);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;

			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load1);
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKUnloadingUser = "E";
			loadPkgPackagePivot2.WLP_UnloadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Factory.Save();

			var orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loading, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);

			var loadPkgPackagePivot3 = Helper.CreateLoadPkgPackagePivot(package2.PK, load2);
			loadPkgPackagePivot3.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot3.WLP_LoadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loaded, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Pick not finalised", false, pick.IsFinalised);

			Helper.DepartPackageNow(loadPkgPackagePivot3);
			Factory.Save();

			orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Departed, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_HasPackages_AllDeparted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Helper.DepartPackageNow(loadPkgPackagePivot2);
			Factory.Save();

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Pick not finalised", false, pick.IsFinalised);
			Factory.Save();

			var orderStatusView = GetOrderStatusForOrderPK(order.PK);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Departed, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_HasPackages_AllDeparted_HasPickLineNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 25m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("2 Pick Lines", 2, orderLine.PickLines.Count);
			orderLine.PickLines[0].WZ_PickedDateTime = DateTime.Now;
			orderLine.PickLines[0].WZ_GS_NKAssignedTo = "ABC";
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			// We cannot load packages if order is not fully picked, but for the sake of test we start loading here.
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Helper.DepartPackageNow(loadPkgPackagePivot2);
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Pick not finalised", false, pick.IsFinalised);
			Factory.Save();

			AssertEquals("Has unpicked lines.", 1, pick.GetAllPickLines().Count(l => !l.IsPicked));
			var orderStatusView = GetOrderStatusForOrderPK(order.PK);
			AssertEquals("Order Status is not Departed.", DocketStatus.Codes.Picking, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_HasPackages_SomeDeparted_AllLoaded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var departedLoad = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck);
			var otherLoad = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L4", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, departedLoad);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, otherLoad);
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Factory.Save();

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			Factory.Save();

			var orderStatusView = GetOrderStatusForOrderPK(order.PK);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loaded, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_HasPackages_PackagesLoaded_HasPickLineNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();

			// We cannot load packages if order is not fully picked, but for the sake of test we start loading here.
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Pick not finalised", false, pick.IsFinalised);
			Factory.Save();

			AssertEquals("Has unpicked lines.", 1, pick.GetAllPickLines().Count(l => !l.IsPicked));
			var orderStatusView = GetOrderStatusForOrderPK(order.PK);
			AssertEquals("Order Status is not Loaded.", DocketStatus.Codes.AttachedToPick, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.AttachedToPick, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_HasPackages_AllLoaded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			Factory.Save();

			var orderStatusView = GetOrderStatusForOrderPK(order.PK);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loaded, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_HasPackages_PartiallyLoaded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			Factory.Save();

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: loadPkgPackagePivot1 is loaded", true, loadPkgPackagePivot1.WLP_LoadedTime.IsValid);
			AssertEquals("Precondition: loadPkgPackagePivot2 is loaded", false, loadPkgPackagePivot2.WLP_LoadedTime.IsValid);
			Factory.Save();

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 1, viewInfo.Length);
			var orderStatusView = GetOrderStatusForOrderPK(order.PK);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loading, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_HasPackages_PackagesLoading_HasPickLineNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 25m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("2 Pick Lines", 2, orderLine.PickLines.Count);
			orderLine.PickLines[0].WZ_PickedDateTime = DateTime.Now;
			orderLine.PickLines[0].WZ_GS_NKAssignedTo = "ABC";
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			// We cannot load packages if order is not fully picked, but for the sake of test we start loading here.
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: loadPkgPackagePivot1 is loaded", true, loadPkgPackagePivot1.WLP_LoadedTime.IsValid);
			AssertEquals("Precondition: loadPkgPackagePivot2 is not loaded", false, loadPkgPackagePivot2.WLP_LoadedTime.IsValid);
			Factory.Save();

			AssertEquals("Has unpicked lines.", 1, pick.GetAllPickLines().Count(l => !l.IsPicked));
			var orderStatusView = GetOrderStatusForOrderPK(order.PK);
			AssertEquals("Order Status is not Loading.", DocketStatus.Codes.Picking, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		[GuiTest]
		public void TestView_NoPackages_FullyPicked()
		{
			TestView_NoPackages_FullyPickedCore(false);
		}

		[GuiTest]
		public void TestView_NoPackages_FullyPicked_UseDirectedPackingConsolidation()
		{
			TestView_NoPackages_FullyPickedCore(true);
		}

		void TestView_NoPackages_FullyPickedCore(bool useDirectedPackingConsolidation)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			order.WD_UseDirectedPackingConsolidation = useDirectedPackingConsolidation;
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Factory.Save();

			var transferLine = pick.Transfers[0].Lines[0];
			transferLine.FinaliseDocketLine();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			// So the pickline will remain picked for this test
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: Order fully picked", true, pickLine.IsPicked);
			AssertEquals("Precondition: Inventory is staged", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);

			var orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Staged, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		[GuiTest]
		public void TestView_NoPackages_PartiallyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickByAttachingOrders(order);
			Factory.Save();

			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine2.PickLines.Single();

			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: Order not finalised", false, order.IsFinalised);
			AssertEquals("Precondition: OrderLine1 picked", true, pickLine1.IsPicked);
			AssertEquals("Precondition: OrderLine2 not picked", false, pickLine2.IsPicked);
			Factory.Save();

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 1, viewInfo.Length);
			AssertEquals("Order Status correct", DocketStatus.Codes.Picking, viewInfo[0]["WOS_OrderStatus"]);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, viewInfo[0]["WOS_DocketStatus"]);
		}

		public void TestView_NoPackages_NotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Factory.Save();

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 1, viewInfo.Length);
			AssertEquals("Order Status correct", DocketStatus.Codes.Entered, viewInfo[0]["WOS_OrderStatus"]);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Entered, viewInfo[0]["WOS_DocketStatus"]);
		}

		public void TestView_ManyOrders_MultipleClients()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = Helper.CreateClient("222");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client2, data.Part2);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part2, 130m);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R7", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R8", data.Part2, 100m);
			Factory.Save();

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";

			var truck1 = Helper.CreateEquipment("T001", 25000m, Core.Constants.Weight.Kilograms, 300m, Core.Constants.Volume.CubicMetres);
			var loadDeparted = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L8", transportUnit: truck1, startTime: DateTimeOffset.Now);

			var truck2 = Helper.CreateEquipment("T002", 25000m, Core.Constants.Weight.Kilograms, 300m, Core.Constants.Volume.CubicMetres);
			var loadingLoad = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck2, startTime: DateTimeOffset.Now);
			Factory.Save();

			#region All Departed Order

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 50m);

			var pick1 = Helper.CreatePickByAttachingOrders(order1);
			Factory.Save();

			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var package1 = order1.PackageJob.Packages.AddNew();
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, loadDeparted);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Factory.Save();

			#endregion

			#region All Loaded Order

			var order2 = Helper.CreateWhsOrder(client2, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			pick1.AddOrders(new[] { order2 });
			pick1.AutoAllocateItemsWithMock();
			Factory.Save();

			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var package2 = order2.PackageJob.Packages.AddNew();
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, loadingLoad);
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			#endregion

			#region Loading Order

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part2, 30m);

			pick1.AddOrders(new[] { order3 });
			pick1.AutoAllocateItemsWithMock();
			Factory.Save();

			var pickLine3 = orderLine3.PickLines.Single();
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package3 = order3.PackageJob.Packages.AddNew();
			var package4 = order3.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot3 = Helper.CreateLoadPkgPackagePivot(package3.PK, loadingLoad);
			loadPkgPackagePivot3.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot3.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.CreateLoadPkgPackagePivot(package4.PK, loadingLoad);
			Factory.Save();

			#endregion

			#region Ready To Pack Order

			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4");
			var orderLine4 = Helper.CreateWhsOrderLine(order4, data.Part2, 30m);
			var pick4 = Helper.CreatePickNew(order4);
			var pickLine4 = orderLine4.PickLines.Single();
			pickLine4.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick4.RunPreSaveValidation();
			Factory.Save();

			var transferLine4 = pick4.Transfers[0].Lines[0];
			transferLine4.FinaliseDocketLine();

			transferLine4.WE_CurrentInventoryStatus = InventoryStatus.Codes.ReadyToPack;
			pickLine4.WZ_PickedDateTime = ZDateTimeOffset.Now;

			// So the pickline4 will remain picked for this test
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			#endregion

			#region Staged Order

			var order5 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O5");
			var orderLine5 = Helper.CreateWhsOrderLine(order5, data.Part2, 30m);
			var pick5 = Helper.CreatePickNew(order5);
			var pickLine5 = orderLine5.PickLines.Single();
			pickLine5.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick5.RunPreSaveValidation();
			Factory.Save();

			var transferLine5 = pick5.Transfers[0].Lines[0];
			transferLine5.FinaliseDocketLine();
			pickLine5.WZ_PickedDateTime = ZDateTimeOffset.Now;

			// So the pickline5 will remain picked for this test
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			#endregion

			#region Picking Order

			var order6 = Helper.CreateWhsOrder(client2, data.Whs1, "O6");
			var orderLine6 = Helper.CreateWhsOrderLine(order6, data.Part2, 30m);
			Helper.CreateWhsOrderLine(order6, data.Part2, 30m);
			Helper.CreatePickNew(order6);
			var pickLine6 = orderLine6.PickLines.Single();
			pickLine6.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			#endregion

			AssertEquals("Precondition: Pick not finalised", false, pick1.IsFinalised);

			var viewInfos = LoadDataFromView();
			AssertEquals("View count correct", 6, viewInfos.Length);

			AssertEquals("Departed Order Status correct", WhsOrderStatus.Codes.Departed, viewInfos[0]["WOS_OrderStatus"]);
			AssertEquals("Order1 Docket Status correct", DocketStatus.Codes.Picking, viewInfos[0]["WOS_DocketStatus"]);
			AssertEquals("Order1 PK correct", order1.PK, viewInfos[0]["WOS_PK"]);

			AssertEquals("Loading Order Status correct", WhsOrderStatus.Codes.Loading, viewInfos[1]["WOS_OrderStatus"]);
			AssertEquals("Order3 Docket Status correct", DocketStatus.Codes.Picking, viewInfos[1]["WOS_DocketStatus"]);
			AssertEquals("Order3 PK correct", order3.PK, viewInfos[1]["WOS_PK"]);

			AssertEquals("Loaded Order Status correct", WhsOrderStatus.Codes.Loaded, viewInfos[2]["WOS_OrderStatus"]);
			AssertEquals("Order2 Docket Status correct", DocketStatus.Codes.Picking, viewInfos[2]["WOS_DocketStatus"]);
			AssertEquals("Order2 PK correct", order2.PK, viewInfos[2]["WOS_PK"]);

			AssertEquals("Picking Order Status correct", DocketStatus.Codes.Picking, viewInfos[3]["WOS_OrderStatus"]);
			AssertEquals("Order6 Docket Status correct", DocketStatus.Codes.Picking, viewInfos[3]["WOS_DocketStatus"]);
			AssertEquals("Order6 PK correct", order6.PK, viewInfos[3]["WOS_PK"]);

			AssertEquals("Ready To Pack Order Status correct", WhsOrderStatus.Codes.ReadyToPack, viewInfos[4]["WOS_OrderStatus"]);
			AssertEquals("Order4 Docket Status correct", DocketStatus.Codes.Picking, viewInfos[4]["WOS_DocketStatus"]);
			AssertEquals("Order4 PK correct", order4.PK, viewInfos[4]["WOS_PK"]);

			AssertEquals("Staged Order Status correct", WhsOrderStatus.Codes.Staged, viewInfos[5]["WOS_OrderStatus"]);
			AssertEquals("Order5 Docket Status correct", DocketStatus.Codes.Picking, viewInfos[5]["WOS_DocketStatus"]);
			AssertEquals("Order5 PK correct", order5.PK, viewInfos[5]["WOS_PK"]);
		}

		#region TestView_PickLinesWithZeroUnits

		public void TestView_PickLinesWithZeroUnits_Picking_ZeroUnits()
		{
			TestView_PickLinesWithZeroUnits_PickingCore((line, view) => Helper.CreateWhsPickLine(line, view, 0m));
		}

		public void TestView_PickLinesWithZeroUnits_Picking_ReserveLine()
		{
			TestView_PickLinesWithZeroUnits_PickingCore((line, view) => Helper.CreateReservePickLine(line, view, 5m));
		}

		void TestView_PickLinesWithZeroUnits_PickingCore(Action<WhsOrderLine, WhsInventoryView> addPickLine)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine1.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Factory.Save();

			var transferLine = pick.Transfers[0].Lines[0];
			transferLine.FinaliseDocketLine();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			addPickLine(order.Lines[0], pickLine.Inventory);

			// So the pickline5 will remain picked for this test
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: OrderLine1 fully picked", true, pickLine.IsPicked);
			AssertEquals("Precondition: OrderLine2 NOT fully picked", false, orderLine2.PickLines.Single().IsPicked);

			var orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_PickLinesWithZeroUnits_Staged_ZeroUnits()
		{
			TestView_PickLinesWithZeroUnits_StagedReadyToPackCore(
				(line, view) => Helper.CreateWhsPickLine(line, view, 0m),
				inventoryStatus: InventoryStatus.Codes.Staged,
				expectedOrderStatus: WhsOrderStatus.Codes.Staged,
				expectedDocketStatus: DocketStatus.Codes.Picking);
		}

		public void TestView_PickLinesWithZeroUnits_Staged_ReserveLine()
		{
			TestView_PickLinesWithZeroUnits_StagedReadyToPackCore(
				(line, view) => Helper.CreateReservePickLine(line, view, 5m),
				inventoryStatus: InventoryStatus.Codes.Staged,
				expectedOrderStatus: WhsOrderStatus.Codes.Staged,
				expectedDocketStatus: DocketStatus.Codes.Picking);
		}

		public void TestView_PickLinesWithZeroUnits_ReadyToPack_ZeroUnits()
		{
			TestView_PickLinesWithZeroUnits_StagedReadyToPackCore(
				(line, view) => Helper.CreateWhsPickLine(line, view, 0m),
				inventoryStatus: InventoryStatus.Codes.ReadyToPack,
				expectedOrderStatus: WhsOrderStatus.Codes.ReadyToPack,
				expectedDocketStatus: DocketStatus.Codes.Picking);
		}

		public void TestView_PickLinesWithZeroUnits_ReadyToPack_ReserveLine()
		{
			TestView_PickLinesWithZeroUnits_StagedReadyToPackCore(
				(line, view) => Helper.CreateReservePickLine(line, view, 5m),
				inventoryStatus: InventoryStatus.Codes.ReadyToPack,
				expectedOrderStatus: WhsOrderStatus.Codes.ReadyToPack,
				expectedDocketStatus: DocketStatus.Codes.Picking);
		}

		void TestView_PickLinesWithZeroUnits_StagedReadyToPackCore(Action<WhsOrderLine, WhsInventoryView> addPickLine, string inventoryStatus, string expectedOrderStatus, string expectedDocketStatus)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			transferLine.FinaliseDocketLine();
			transferLine.WE_CurrentInventoryStatus = inventoryStatus;
			AssertEquals(
				"Precondition - should be correct.",
				inventoryStatus,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			addPickLine(order.Lines[0], pickLine.Inventory);
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator()) // So the pickline will remain picked for this test
			{
				Factory.Save();
			}

			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: Order fully picked", true, pickLine.IsPicked);

			var orderStatusView = GetOrderStatusForOrderPK(order.PK);
			AssertEquals("Order Status correct", expectedOrderStatus, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", expectedDocketStatus, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_PickLinesWithZeroUnits_Loading_ZeroUnits()
		{
			TestView_PickLinesWithZeroUnitsCore(
				(line, view) => Helper.CreateWhsPickLine(line, view, 0m),
				expectedOrderStatus: WhsOrderStatus.Codes.Loading,
				expectedDocketStatus: DocketStatus.Codes.Picking,
				isLoaded: false,
				isDeparted: false);
		}

		public void TestView_PickLinesWithZeroUnits_Loading_ReserveLine()
		{
			TestView_PickLinesWithZeroUnitsCore(
				(line, view) => Helper.CreateReservePickLine(line, view, 5m),
				expectedOrderStatus: WhsOrderStatus.Codes.Loading,
				expectedDocketStatus: DocketStatus.Codes.Picking,
				isLoaded: false,
				isDeparted: false);
		}

		public void TestView_PickLinesWithZeroUnits_Loaded_ZeroUnits()
		{
			TestView_PickLinesWithZeroUnitsCore(
				(line, view) => Helper.CreateWhsPickLine(line, view, 0m),
				expectedOrderStatus: WhsOrderStatus.Codes.Loaded,
				expectedDocketStatus: DocketStatus.Codes.Picking,
				isLoaded: true,
				isDeparted: false);
		}

		public void TestView_PickLinesWithZeroUnits_Loaded_ReserveLine()
		{
			TestView_PickLinesWithZeroUnitsCore(
				(line, view) => Helper.CreateReservePickLine(line, view, 5m),
				expectedOrderStatus: WhsOrderStatus.Codes.Loaded,
				expectedDocketStatus: DocketStatus.Codes.Picking,
				isLoaded: true,
				isDeparted: false);
		}

		public void TestView_PickLinesWithZeroUnits_Departed_ZeroUnits()
		{
			TestView_PickLinesWithZeroUnitsCore(
				(line, view) => Helper.CreateWhsPickLine(line, view, 0m),
				expectedOrderStatus: WhsOrderStatus.Codes.Departed,
				expectedDocketStatus: DocketStatus.Codes.Picking,
				isLoaded: true,
				isDeparted: true);
		}

		public void TestView_PickLinesWithZeroUnits_Departed_ReserveLine()
		{
			TestView_PickLinesWithZeroUnitsCore(
				(line, view) => Helper.CreateReservePickLine(line, view, 5m),
				expectedOrderStatus: WhsOrderStatus.Codes.Departed,
				expectedDocketStatus: DocketStatus.Codes.Picking,
				isLoaded: true,
				isDeparted: true);
		}

		void TestView_PickLinesWithZeroUnitsCore(
			Action<WhsOrderLine, WhsInventoryView> addPickLine,
			string expectedOrderStatus,
			string expectedDocketStatus,
			bool isLoaded,
			bool isDeparted)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			if (isLoaded)
			{
				loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
				loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			}
			Factory.Save();

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: loadPkgPackagePivot1 is loaded", true, loadPkgPackagePivot1.WLP_LoadedTime.IsValid);
			AssertEquals("Precondition: loadPkgPackagePivot2 is correct", isLoaded, loadPkgPackagePivot2.WLP_LoadedTime.IsValid);
			addPickLine(order.Lines[0], pickLine.Inventory);

			if (isDeparted)
			{
				Helper.DepartPackageNow(loadPkgPackagePivot1);
				Helper.DepartPackageNow(loadPkgPackagePivot2);
			}
			Factory.Save();

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 1, viewInfo.Length);
			var orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", expectedOrderStatus, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", expectedDocketStatus, orderStatusView.WOS_DocketStatus);
		}

		#endregion

		#region TestView_InventoryReadyToPack

		public void TestView_InventoryReadyToPack_All()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Factory.Save();

			var transferLine = pick.Transfers[0].Lines[0];
			transferLine.FinaliseDocketLine();
			transferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.ReadyToPack;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			// So the picklines will remain picked for this test
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: Order fully picked", true, pickLine.IsPicked);

			var orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.ReadyToPack, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_InventoryReadyToPack_SomeReadyToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part2, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderline1.PickLines.Single();
			var pickLine2 = orderline2.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Factory.Save();

			var transferLine = pick.Transfers[0].Lines[0];
			transferLine.FinaliseDocketLine();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			// So the picklines will remain picked for this test
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: OrderLine1 fully picked", true, pickLine1.IsPicked);
			AssertEquals("Precondition: OrderLine2 NOT picked", false, pickLine2.IsPicked);

			var orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_InventoryReadyToPack_SomeStaged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part2, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderline1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = orderline2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Factory.Save();

			var transferLine1 = pick.Transfers[0].Lines[0];
			transferLine1.FinaliseDocketLine();
			transferLine1.WE_CurrentInventoryStatus = InventoryStatus.Codes.ReadyToPack;

			var transferLine2 = pick.Transfers[0].Lines[1];
			transferLine2.FinaliseDocketLine();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			// So the picklines will remain picked for this test
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: OrderLine1 fully picked", true, pickLine1.IsPicked);
			AssertEquals("Precondition: OrderLine2 fully picked", true, pickLine2.IsPicked);

			var orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.ReadyToPack, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		public void TestView_InventoryReadyToPack_SomeReadyToPack_SomeInTransit()
		{
			TestView_InventoryReadyToPack_SomeReadyToPack_SomeInTransitCore(false);
		}

		public void TestView_InventoryReadyToPack_SomeReadyToPack_SomeInTransit_UseDirectedPackingConsolidation()
		{
			TestView_InventoryReadyToPack_SomeReadyToPack_SomeInTransitCore(true);
		}

		void TestView_InventoryReadyToPack_SomeReadyToPack_SomeInTransitCore(bool useDirectedPacking)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part2, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_UseDirectedPackingConsolidation = useDirectedPacking;

			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var orderline3 = Helper.CreateWhsOrderLine(order, data.Part2, 6m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderline1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = orderline2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine3 = orderline3.PickLines.Single();
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Factory.Save();

			var transferLine1 = pick.Transfers[0].Lines[0];
			transferLine1.FinaliseDocketLine();
			transferLine1.WE_CurrentInventoryStatus = InventoryStatus.Codes.ReadyToPack;

			var transferLine2 = pick.Transfers[0].Lines[1];
			transferLine2.FinaliseDocketLine();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			// So the picklines will remain picked for this test
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition: Pick not finalised", false, pick.IsFinalised);
			AssertEquals("Precondition: OrderLine1 fully picked.", true, pickLine1.IsPickedFromPutawayLocation);
			AssertEquals("Precondition: OrderLine2 fully picked.", true, pickLine2.IsPickedFromPutawayLocation);
			AssertEquals("Precondition: OrderLine3 fully picked.", true, pickLine3.IsPickedFromPutawayLocation);
			AssertNotNull("Precondition: OrderLine3 is still in transit", pickLine3.WZ_WE_OriginalPickedInventoryLine);

			var orderStatusView = GetOrderStatusForOrderPK(order.PK, true);
			AssertEquals("Order Status correct", useDirectedPacking ? WhsOrderStatus.Codes.ReadyToPack : WhsOrderStatus.Codes.Staged, orderStatusView.WOS_OrderStatus);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, orderStatusView.WOS_DocketStatus);
		}

		#endregion

		#region TestView_IgnoreNotPickedPickByBOMInventoryLines

		public void TestView_IgnoreNotPickedPickByBOMInventoryLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			var location1 = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			pick = newFactory.Load<WhsPick>(pick.PK);
			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);

			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var componentOrderLine = orderLine.ChildComponentLines.First();
			AssertEquals("Should be allocated.", 1, componentOrderLine.PickLines.Count);
			var componentPickLine1 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 10m);
			componentPickLine1.WZ_PickedDateTime = DateTimeOffset.Now;
			newFactory.Save();

			var kitReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);
			AssertEquals("Precondition: The Kit Receive Line is Pending.", InventoryStatus.Codes.Pending, kitPickLines[0].InventoryLine.WE_CurrentInventoryStatus);

			var viewInfo = LoadDataFromView();
			AssertEquals("View count correct", 1, viewInfo.Length);
			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Staged, viewInfo[0]["WOS_OrderStatus"]);
			AssertEquals("Docket Status correct", DocketStatus.Codes.Picking, viewInfo[0]["WOS_DocketStatus"]);
		}

		#endregion

		#region LoadSQLFunction

		WhsOrderStatusView[] LoadDataFromView()
		{
			var newFactory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(WhsOrderStatusView));
			query.OrderBy = WhsOrderStatusViewSchema.WOS_OrderStatus.Name;
			var result = newFactory.Load<WhsOrderStatusView>(query);
			return result;
		}

		WhsOrderStatusView GetOrderStatusForOrderPK(ZGuid orderPK, bool newFactory = false)
		{
			var query = new ZQuery(WhsOrderStatusViewSchema.PK, orderPK);
			var factory = newFactory ? new BusinessObjectFactory() : Factory;
			var whsOrderStatusView = factory.Load<WhsOrderStatusView>(query).Single();
			return whsOrderStatusView;
		}

		#endregion
	}
}
