using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsOrderLineCollectionTest<T> : WhsPickableDocketLineCollectionTestCase<T> where T : WhsOrderLineCollection
	{
		#region TestAllowNewForPickedOrder

		public void TestAllowNewForPickedOrder()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals(true, ((IBindingList)order.Lines).AllowNew);
		}

		public void TestAllowNewForPickedOrder_PreventOrderLinesUpdateWhenOrderIsPickingRegistryEnabled()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			using (WarehouseDataRegistry.Instance.PreventOrderLinesUpdateWhenOrderIsInPicking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				Helper.CreatePickNew(order);
				AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals(false, ((IBindingList)order.Lines).AllowNew);
			}
		}

		#endregion

		#region TestAllowNew_TaskPlanningStatus

		public void TestAllowNew_TaskPlanningStatus_ReadyForPlanning()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals(false, ((IBindingList)order.Lines).AllowNew);
		}

		public void TestAllowNew_TaskPlanningStatus_NotReadyForPlanning()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals(true, ((IBindingList)order.Lines).AllowNew);
		}

		public void TestAllowNew_TaskPlanningStatus_Planned()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertEquals(false, ((IBindingList)order.Lines).AllowNew);
		}

		public void TestAllowNew_TaskPlanningStatus_Empty()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals(true, ((IBindingList)order.Lines).AllowNew);
		}

		#endregion

		#region TestDoesNotAllowNewFor LOA/LDG/DEP

		public void TestDoesNotAllowNewForPickedOrder_WhenOrderStatusIsLOA()
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
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals(WhsOrderStatus.Codes.Loaded, order.WarehouseOrderStatus);
			AssertEquals(false, ((IBindingList)order.Lines).AllowNew);
		}

		public void TestDoesNotAllowNewForPickedOrder_WhenOrderStatusIsLDG()
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
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			pick.FinaliseAllOrders();
			Factory.Save();

			AssertEquals(WhsOrderStatus.Codes.Loading, order.WarehouseOrderStatus);
			AssertEquals(false, ((IBindingList)order.Lines).AllowNew);
		}

		public void TestDoesNotAllowNewForPickedOrder_WhenOrderStatusIsDEP()
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
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			pick.FinaliseAllOrders();
			Factory.Save();

			AssertEquals("Pick is not finalised.", false, pick.IsFinalised);
			AssertEquals(WhsOrderStatus.Codes.Departed, order.WarehouseOrderStatus);
			AssertEquals(false, ((IBindingList)order.Lines).AllowNew);
		}

		#endregion

		#region TestAllowRemoveForPickedOrder

		protected override bool ExpectedAllowAddOrRemoveForPickedOrder => true;

		public void TestAllowRemoveForPickedOrder()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals(true, ((IBindingList)order.Lines).AllowRemove);
		}

		public void TestAllowRemoveForPickedOrder_PreventOrderLinesUpdateWhenOrderIsPickingRegistryEnabled()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			using (WarehouseDataRegistry.Instance.PreventOrderLinesUpdateWhenOrderIsInPicking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				Helper.CreatePickNew(order);
				AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals(false, ((IBindingList)order.Lines).AllowRemove);
			}
		}

		#endregion

		#region TestAllowNew_MultiOrderPick

		public void TestAllowNew_MultiOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var orderLineCollection = ((IBindingList)order.Lines);
			AssertEquals("Precondition - order does not have pick", null, orderLine.Order.Pick);
			AssertEquals("Order does not have pick.", true, orderLineCollection.AllowNew);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - order has pick", pick, orderLine.Order.Pick);
			AssertEquals("Precondition - Order has pick but is not multi order pick.", false, orderLine.Order.Pick.IsMultiOrderPick);
			AssertEquals("Order has pick but is not multi order pick.", true, orderLineCollection.AllowNew);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			pick.Orders.Add(order2);
			AssertEquals("Precondition - pick is multi order.", true, orderLine.Order.Pick.IsMultiOrderPick);
			AssertEquals("order attached to multi-order pick .", false, orderLineCollection.AllowNew);
		}

		#endregion

		#region TestDoesNotAllowNewForPickedFTZCustomsOrder

		public void TestDoesNotAllowNewForPickedFTZCustomsOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			AssertEquals("Precondition: Before pick is created, we can add more Order Lines.", true, ((IBindingList)order.Lines).AllowNew);

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				Helper.CreatePickNew(order);
			}

			AssertEquals("After pick is created for an FTZ Warehouse, we should not be able to create new Order Lines.", false, ((IBindingList)order.Lines).AllowNew);
		}

		#endregion

		#region TestOnAdd_SetDefaultForCustoms

		public void TestOnAdd_SetDefaultForCustoms()
		{
			var order = Factory.New<WhsOrder>();
			var whs = Helper.CreateWarehouse("whs1");
			order.WD_WW_Whs = whs.PK;
			order.WD_DocketSubType = OrderType.Codes.Customs;

			var orderline1 = order.Lines.AddNew();
			AssertEquals("Should set default for Customs jobs.", WhsBondedWarehouseAttributeOutwardType.Codes.CNN, orderline1.CustomsData.WB_OutwardType);

			orderline1.CustomsData.WB_OutwardType = WhsBondedWarehouseAttributeOutwardType.Codes.EXS;

			var orderline2 = order.Lines.AddNew();
			AssertEquals("Should not change the existing value.", WhsBondedWarehouseAttributeOutwardType.Codes.EXS, orderline1.CustomsData.WB_OutwardType);
			AssertEquals("Should set default for Customs jobs.", WhsBondedWarehouseAttributeOutwardType.Codes.CNN, orderline2.CustomsData.WB_OutwardType);

			order.WD_DocketSubType = OrderType.Codes.Order;
			var orderline3 = order.Lines.AddNew();
			AssertEquals("Should not set default", string.Empty, orderline3.CustomsData.WB_OutwardType);
			AssertEquals("Keep data as is.", WhsBondedWarehouseAttributeOutwardType.Codes.EXS, orderline1.CustomsData.WB_OutwardType);
			AssertEquals("Keep data as is.", WhsBondedWarehouseAttributeOutwardType.Codes.CNN, orderline2.CustomsData.WB_OutwardType);
		}

		#endregion

		#region TestSortingByStagingLocationBOM

		protected override void TestSortingByStagingLocationBOMCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);

			var productParams1 = orderLine1.Product.ParamsByWhsAndClient.AddNew();
			productParams1.W3_OH = data.Org1.PK;
			productParams1.W3_WW = data.Whs1.PK;
			productParams1.W3_WL_StagingLocationBOM = data.Whs1.FindLocation("A-2").PK;

			var productParams2 = orderLine2.Product.ParamsByWhsAndClient.AddNew();
			productParams2.W3_OH = data.Org1.PK;
			productParams2.W3_WW = data.Whs1.PK;
			productParams2.W3_WL_StagingLocationBOM = data.Whs1.FindLocation("A-10").PK;
			Factory.Save();

			order.Lines.ApplySort(WhsPickableDocketLineCollection.StagingLocationBOMPropertyName, ListSortDirection.Ascending);
			AssertEquals("A-2", order.Lines[0].StagingLocationBOM.ToLocationString());
			AssertEquals("A-10", order.Lines[1].StagingLocationBOM.ToLocationString());

			order.Lines.ApplySort(WhsPickableDocketLineCollection.StagingLocationBOMPropertyName, ListSortDirection.Descending);
			AssertEquals("A-10", order.Lines[0].StagingLocationBOM.ToLocationString());
			AssertEquals("A-2", order.Lines[1].StagingLocationBOM.ToLocationString());
		}

		#endregion
	}
}
