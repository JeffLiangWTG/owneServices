using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class AllocatePackageLabelsStrategyTest : WhsTestCaseWithFactory
	{
		#region RunChecksPriorToCartonisingOrPickingByLabel

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_UnsavedPick

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_UnsavedPick()
		{
			var pick = Factory.New<WhsPick>();
			pick.WP_PickOption = WhsPickOption.Codes.Auto;

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'false' as we encountered a fatal error.", false, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));

			AssertEquals("Should have shown an actual Error Message.", true, notifications.Events.Length > 0);
			AssertEquals("You must save the Pick before Allocating Package Labels.", notifications.Events[0].Message);
		}

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_UnsavedPick_SaveToFactoryFalse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var mediumCarton = Helper.CreateWhsCartonSize("MED", 15, 10, 10, 10, 30, 100, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var largeCarton = Helper.CreateWhsCartonSize("LRG", 15, 15, 15, 15, 50, 500, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			whsGroup.CartonSizes.AddRange(new[] { smallCarton, mediumCarton, largeCarton });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Grams, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			// Set up product so 15 units will fit in 2x Small cartons or 1x Medium Carton
			data.Part1.OP_CubicUQ = Constants.Volume.CubicCentimeters;
			data.Part1.OP_Depth = 0.5m;
			data.Part1.OP_Width = 0.5m;
			data.Part1.OP_Height = 0.5m;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_Weight = 1m;
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.HasChanges = true;

			Assert("Precondition: pick has changes.", pick.HasChanges);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should not return 'false' for an unsaved pick.", true, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick, saveFactory: false));
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_NoOrders

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_NoOrders()
		{
			var pick = Factory.New<WhsPick>();
			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'false' as we encountered a fatal error.", false, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));

			AssertEquals("Should have shown an actual Error Message.", true, notifications.Events.Length > 0);
			AssertEquals("No Orders to Allocate Package Labels for.", notifications.Events[0].Message);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_TaskPlanningStatus

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_TaskPlanningStatus_Ready()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'false' as we encountered a fatal error.", false, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));

			AssertEquals("Should have shown an actual Error Message.", true, notifications.Events.Length > 0);
			AssertEquals("You cannot Allocate Package Labels because the pick is Ready For Planning or Planned.", notifications.Events[0].Message);
		}

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_TaskPlanningStatus_Planned()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'false' as we encountered a fatal error.", false, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));

			AssertEquals("Should have shown an actual Error Message.", true, notifications.Events.Length > 0);
			AssertEquals("You cannot Allocate Package Labels because the pick is Ready For Planning or Planned.", notifications.Events[0].Message);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_PickByUOMNotEnabled

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_PickByUOMNotEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'false' as we encountered a fatal error.", false, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));

			AssertEquals("Should have shown an actual Error Message.", true, notifications.Events.Length > 0);
			AssertEquals("Pick By UOM must be enabled to Allocate Package Labels.", notifications.Events[0].Message);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_PickByUOMNotEnabled_WhenPickIsCreatedBeforeSettingPickByUOM

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_PickByUOMNotEnabled_WhenPickIsCreatedBeforeSettingPickByUOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'false' as we encountered a fatal error.", false, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));

			AssertEquals("Should have shown an actual Error Message.", true, notifications.Events.Length > 0);
			AssertEquals("Pick By UOM must be enabled to Allocate Package Labels. If the Pick was created prior to setting Pick By UOM, you must Reallocate Stock to the Pick.", notifications.Events[0].Message);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_NoAllocations

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_NoAllocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick);

			AssertEquals("Should have shown an actual Error Message.", true, notifications.Events.Length > 0);
			AssertEquals("No Allocations to Allocate Package Labels to.", notifications.Events[0].Message);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_SingleOrder

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_SingleOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");

			var otherRefType1 = PackingHelper.CreateRefPackType("2", "2", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds);
			var otherRefType2 = PackingHelper.CreateRefPackType("3", "3", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds);
			data.Part2.OP_StockKeepingUnit = otherRefType1.F3_Code;
			part3.OP_StockKeepingUnit = Constants.Volume.CubicMetres;
			part4.OP_StockKeepingUnit = Constants.PkgUnit.Unit;
			Factory.Save();

			part4.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(part4, otherRefType2.F3_Code, 2m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, part4, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Helper.CreateWhsOrderLine(order, part3, 5m);
			Helper.CreateWhsOrderLine(order, part4, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick);
			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have errors if Cartonising SplitCase goods.", 4, logs.Length);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No UOM Type, Order: {0} Pack Type: UNT", order.WD_DocketID), logs[0].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No UOM Type, Order: {0} Pack Type: 2", order.WD_DocketID), logs[1].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No UOM Type, Order: {0} Pack Type: M3", order.WD_DocketID), logs[2].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No UOM Type, Order: {0} Pack Type: 3", order.WD_DocketID), logs[3].DisplayEventReference);
			AssertEquals("Should *not* have detached order if no Orders could be cartonised.", 1, pick.Orders.Count);
			AssertEquals("Should have saved.", false, pick.HasChanges);

			otherRefType1.F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			otherRefType2.F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick);
			logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have only added 2 extra logs.", 4 + 2, logs.Length);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_SingleOrder_FactorySaveFails

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_SingleOrder_FactorySaveFails()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var otherRefType = PackingHelper.CreateRefPackType("2", "2", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds);
			data.Part2.OP_StockKeepingUnit = otherRefType.F3_Code;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInOtherFactory = factory2.Load<WhsPick>(pick.PK);
			pickInOtherFactory.PickPriority = 2;
			factory2.Save();

			pick.PickPriority = 3; // force concurrency error.
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertNoExceptionThrown(() => allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_MultiOrder

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_MultiOrder_WithFactorySave()
		{
			TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_MultiOrderCore(true);
		}

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_MultiOrder_NoFactorySave()
		{
			TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_MultiOrderCore(false);
		}

		void TestRunChecksPriorToCartonisingOrPickingByLabel_NoUOMTypes_MultiOrderCore(bool factorySave)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'false' as we encountered a fatal error.", false, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick, factorySave));
			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have errors if Cartonising SplitCase goods.", 2, logs.Length);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No UOM Type, Order: {0} Pack Type: UNT", order1.WD_DocketID), logs[0].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No UOM Type, Order: {0} Pack Type: UNT", order2.WD_DocketID), logs[1].DisplayEventReference);
			AssertEquals("Should *not* have detached order if no Orders could be cartonised.", 2, pick.Orders.Count);
			AssertEquals("Should have shown an actual Error Message in addition to logging if all Orders failed.", true, notifications.Events.Length > 0);
			AssertEquals("No Order(s) on the Pick are valid for Allocating Package Labels. Check the Events on the Pick for more details.", notifications.Events[0].Message);
			AssertEquals("Should have saved or not saved based on factorySave parameter.", factorySave, !pick.HasChanges);

			notifications.Clear();
			var splitCaseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			Factory.Save();

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, splitCaseRefType.F3_Code, 2m);

			pick.ClearAllocatedItems();
			pick.AutoAllocateItemsWithMock(); // Force PickByUOM to run
			AssertEquals("Precondition - PickByUOM run", true, pick.GetAllPickLines().All(pl => !pl.WZ_F3_NKAllocatedPackType.IsEmpty));
			Factory.Save(); // Save so we dont just update the existing unsaved log

			AssertEquals("Should return 'true' as we did *not* encountered a fatal error.", true, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick, factorySave));
			logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have errors if Cartonising SplitCase goods.", 2 + 1, logs.Length);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No UOM Type, Order: {0} Pack Type: UNT", order2.WD_DocketID, data.Part2.OP_PartNum), logs[2].DisplayEventReference);
			AssertContainsExactElementsInAnyOrder("Should have detached order if no Orders could be cartonised.", new[] { order1 }, pick.Orders);
			AssertEquals("Should *not* have shown an actual Error Message.", false, notifications.Events.Any(e => e.Message == "No Order(s) on the Pick are valid for Allocating Package Labels. Check the Events on the Pick for more details."));
			AssertEquals("Should have saved or not saved based on factorySave parameter.", factorySave, !pick.HasChanges);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_BOMProductPickedWithoutWorkOrder

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_BOMProductPickedWithoutWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var splitCaseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, splitCaseRefType.F3_Code);
			Helper.CreateProductBOM(bike, frame, 1m, splitCaseRefType.F3_Code);

			var pc = Helper.CreateProduct(data.Org1, "PC");
			pc.OP_IsComponentPickedOnSalesOrder = true;
			var keyboard = Helper.CreateProduct(data.Org1, "BOARD");
			var mouse = Helper.CreateProduct(data.Org1, "MOUSE");
			Helper.CreateProductBOM(pc, keyboard, 1m, splitCaseRefType.F3_Code);
			Helper.CreateProductBOM(pc, mouse, 1m, splitCaseRefType.F3_Code);

			bike.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			wheel.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			frame.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			pc.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			keyboard.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			mouse.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			data.Part2.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			Factory.Save();

			var location = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 25m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, keyboard, 50m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, mouse, 50m, location);
			receive.FinaliseDocket();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var kitOrderLine = order1.Lines[0];
			var wheelLine = Helper.CreateWhsOrderLine(order1, wheel, 1m);
			var frameLine = Helper.CreateWhsOrderLine(order1, frame, 1m);
			var pcLine = Helper.CreateWhsOrderLine(order1, pc, 25m);
			var pcLine2 = Helper.CreateWhsOrderLine(order1, pc, 14m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine.ChildComponentLines.Count > 0);

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'true' as we did *not* encountered a fatal error.", true, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));
			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));

			AssertEquals("Should have no error if Cartonising SplitCase goods.", 0, logs.Length);
			AssertEquals("Should have not shown an actual Error Message in addition to logging.", true, notifications.Events.Length == 0);
			AssertContainsExactElementsInAnyOrder("Should have not detached order.", new[] { order1, order2 }, pick.Orders);
			AssertEquals("Should have saved.", false, pick.HasChanges);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_BOMProductPickedWithoutWorkOrder_WithAllOrdersNotDetached

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_BOMProductPickedWithoutWorkOrder_WithAllOrdersNotDetached()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var splitCaseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, splitCaseRefType.F3_Code);
			Helper.CreateProductBOM(bike, frame, 1m, splitCaseRefType.F3_Code);

			var pc = Helper.CreateProduct(data.Org1, "PC");
			pc.OP_IsComponentPickedOnSalesOrder = true;
			var keyboard = Helper.CreateProduct(data.Org1, "BOARD");
			var mouse = Helper.CreateProduct(data.Org1, "MOUSE");
			Helper.CreateProductBOM(pc, keyboard, 1m, splitCaseRefType.F3_Code);
			Helper.CreateProductBOM(pc, mouse, 1m, splitCaseRefType.F3_Code);

			bike.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			wheel.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			frame.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			pc.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			keyboard.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			mouse.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			data.Part2.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			Factory.Save();

			var location = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 25m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, keyboard, 50m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, mouse, 50m, location);
			receive.FinaliseDocket();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var wheelLine = Helper.CreateWhsOrderLine(order1, wheel, 1m);
			var frameLine = Helper.CreateWhsOrderLine(order1, frame, 1m);
			var pcLine = Helper.CreateWhsOrderLine(order1, pc, 25m);
			var pcLine2 = Helper.CreateWhsOrderLine(order1, pc, 14m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", pc, 10m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var hasPickByBomProducts = pick.Orders.Cast<WhsOrder>().SelectMany(o => o.Lines).Cast<WhsPickableDocketLine>().Any(l => l.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - Picked By BOM", true, hasPickByBomProducts);

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();

			AssertEquals("We can process Pick By BOM lines.", true, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));
			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have no error if Cartonising SplitCase goods.", 0, logs.Length);
			AssertEquals("Should have not shown an actual Error Message in addition to logging.", true, notifications.Events.Length == 0);
			AssertContainsExactElementsInAnyOrder("Should have not detached order.", new[] { order1, order2 }, pick.Orders);
			AssertEquals("Should have saved.", false, pick.HasChanges);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_BOMProductPickedWithoutWorkOrder_ComponentIsNonUOMType

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_BOMProductPickedWithoutWorkOrder_ComponentIsNonUOMType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var caseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.Case);
			var nonUOMType = PackingHelper.CreateRefPackType("222", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, nonUOMType.F3_Code);
			Helper.CreateProductBOM(bike, frame, 1m, nonUOMType.F3_Code);

			bike.OP_StockKeepingUnit = caseRefType.F3_Code;
			wheel.OP_StockKeepingUnit = nonUOMType.F3_Code;
			frame.OP_StockKeepingUnit = nonUOMType.F3_Code;
			Factory.Save();

			var location = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 25m, location);
			receive.FinaliseDocket();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var pick = Helper.CreatePickNew(order1);
			Factory.Save();

			AssertEquals("Precondition - Picked By BOM", true, order1.Lines[0].ChildComponentLines.Count > 0);

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();

			var result = allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick);
			AssertEquals("Should return 'true' as we did *not* encountered a fatal error.", true, result);

			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have no error if Cartonising Case goods.", 0, logs.Length);
			AssertEquals("Should have not shown an actual Error Message in addition to logging.", true, notifications.Events.Length == 0);
			AssertContainsExactElementsInAnyOrder("Should have not detached order.", new[] { order1 }, pick.Orders);
			AssertEquals("Should have saved.", false, pick.HasChanges);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_AddErrorLogAtSameTimeForAllCases

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_AddErrorLogAtSameTimeForAllCases()
		{
			var jsl = Helper.CreateGlbStaff("JSL", "Jason");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			//No UOM Type
			var otherRefType1 = PackingHelper.CreateRefPackType("2", "2", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds);
			data.Part2.OP_StockKeepingUnit = otherRefType1.F3_Code;

			// Product has Release Captured Attributes
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var splitCaseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = splitCaseRefType.F3_Code;

			// BOM Product picked without Work Order
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, splitCaseRefType.F3_Code);
			Helper.CreateProductBOM(bike, frame, 1m, splitCaseRefType.F3_Code);

			bike.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			wheel.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			frame.OP_StockKeepingUnit = splitCaseRefType.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", wheel, 50m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", frame, 25m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", bike, 1m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 5m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var kitOrderLine = order2.Lines[0];
			var wheelLine = Helper.CreateWhsOrderLine(order2, wheel, 1m);
			var frameLine = Helper.CreateWhsOrderLine(order2, frame, 1m);

			var pick = Helper.CreatePickNew(order1, order2);

			// Product has been picked
			AssertEquals("Pre-condition:", 1, orderLine1.PickLines.Count);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today.AddHours(-1);
			pickLine1.WZ_GS_NKAssignedTo = "JSL";
			var bikePickLine = kitOrderLine.PickLines.Single(l => l.WZ_Units == 1m);
			bikePickLine.WZ_PickedDateTime = ZDateTimeOffset.Today.AddHours(-1);
			bikePickLine.WZ_GS_NKAssignedTo = "JSL";

			// Product has Release Captured Attributes
			var releaseLine = orderLine1.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";

			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine.ChildComponentLines.Count > 0);

			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick);

			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));

			AssertEquals("Should have errors if Cartonising SplitCase goods.", 4, logs.Length);
			AssertContainsExactElementsInAnyOrder("Should have errors if Cartonising SplitCase goods.", new[]
			{
						string.Format("Error Report: Cartonization, No UOM Type, Order: {0} Pack Type: 2", order1.WD_DocketID),
						string.Format("Error Report: Cartonization, Line has been Release Captured, Order: {0} Product: P1", order1.WD_DocketID),
						string.Format("Error Report: Cartonization, Line has been picked, Order: {0} Product: P1", order1.WD_DocketID),
						string.Format("Error Report: Cartonization, Line has been picked, Order: {0} Product: BIKE", order2.WD_DocketID),
					}, logs.Select(x => x.DisplayEventReference.ToString()));
			AssertEquals("Should have shown an actual Error Message in addition to logging if all Orders failed.", true, notifications.Events.Length > 0);
			AssertEquals("No Order(s) on the Pick are valid for Allocating Package Labels. Check the Events on the Pick for more details.", notifications.Events[0].Message);
			AssertEquals("Should have saved.", false, pick.HasChanges);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_NoOrdersHaveReleaseCapturedAttributes

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_NoOrdersHaveReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var splitCaseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = splitCaseRefType.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1);
			var releaseLine = orderLine1.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick);

			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have errors if Cartonising SplitCase goods.", 1, logs.Length);
			AssertContainsExactElementsInAnyOrder("Should have errors if Cartonising SplitCase goods.", new[]
			{
				string.Format("Error Report: Cartonization, Line has been Release Captured, Order: {0} Product: P1", order1.WD_DocketID) ,
			}, logs.Select(x => x.DisplayEventReference.ToString()));

			AssertEquals("Should have shown an actual Error Message in addition to logging if all Orders failed.", true, notifications.Events.Length > 0);
			AssertEquals("No Order(s) on the Pick are valid for Allocating Package Labels. Check the Events on the Pick for more details.", notifications.Events[0].Message);
			AssertEquals("Should have saved.", false, pick.HasChanges);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_NoOrdersThatHaveBeenPicked

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_NoOrdersThatHaveBeenPicked()
		{
			var jsl = Helper.CreateGlbStaff("JSL", "Jason");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var splitCaseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = splitCaseRefType.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Pre-condition:", 1, orderLine1.PickLines.Count);
			var pickLine = orderLine1.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today.AddHours(-1);
			pickLine.WZ_GS_NKAssignedTo = "JSL";

			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick);

			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have errors if Cartonising SplitCase goods.", 1, logs.Length);
			AssertContainsExactElementsInAnyOrder("Should have errors if Cartonising SplitCase goods.", new[]
			{
				string.Format("Error Report: Cartonization, Line has been picked, Order: {0} Product: P1", order1.WD_DocketID) ,
			}, logs.Select(x => x.DisplayEventReference.ToString()));
			AssertEquals("Should have shown an actual Error Message in addition to logging if all Orders failed.", true, notifications.Events.Length > 0);
			AssertEquals("No Order(s) on the Pick are valid for Allocating Package Labels. Check the Events on the Pick for more details.", notifications.Events[0].Message);
			AssertEquals("Should have saved.", false, pick.HasChanges);
		}

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_NoOrdersThatHaveBeenPicked_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var splitCaseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = splitCaseRefType.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition:", 1, orderLine.PickLines.Count);
			var pickLine = orderLine.PickLines[0];
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick);

			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have errors if Cartonising SplitCase goods.", 1, logs.Length);
			AssertContainsExactElementsInAnyOrder("Should have errors if Cartonising SplitCase goods.", new[]
			{
				string.Format("Error Report: Cartonization, Line has been picked, Order: {0} Product: P1", order.WD_DocketID) ,
			}, logs.Select(x => x.DisplayEventReference.ToString()));
			AssertEquals("Should have shown an actual Error Message in addition to logging if all Orders failed.", true, notifications.Events.Length > 0);
			AssertEquals("No Order(s) on the Pick are valid for Allocating Package Labels. Check the Events on the Pick for more details.", notifications.Events[0].Message);
			AssertEquals("Should have saved.", false, pick.HasChanges);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_PickAwaitingReplenishment

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_PickAwaitingReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var mediumCarton = Helper.CreateWhsCartonSize("MED", 15, 10, 10, 10, 30, 100, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var largeCarton = Helper.CreateWhsCartonSize("LRG", 15, 15, 15, 15, 50, 500, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			whsGroup.CartonSizes.AddRange(new[] { smallCarton, mediumCarton, largeCarton });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Grams, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;
			data.Part2.OP_StockKeepingUnit = refType1.F3_Code;

			// Set up product so 15 units will fit in 2x Small cartons or 1x Medium Carton
			data.Part2.OP_CubicUQ = Constants.Volume.CubicCentimeters;
			data.Part2.OP_Depth = 0.5m;
			data.Part2.OP_Width = 0.5m;
			data.Part2.OP_Height = 0.5m;
			data.Part2.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part2.OP_Weight = 1m; // This is the important field. 15 * 10 > (20 - 10)
			data.Part2.OP_WeightUQ = Constants.Weight.Grams;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today.AddDays(-1), data.Part1, 3m,
				pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today.AddDays(-4), data.Part1, 3m,
				bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 15m);
			Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Factory.Save();
			AssertEquals(true, pick.WP_IsAwaitingReplenishment);

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'false' as we encountered a fatal error.", false, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));

			AssertEquals("Should have shown an actual Error Message.", true, notifications.Events.Length > 0);
			AssertEquals("You cannot Allocate Package Labels if the pick is awaiting replenishment.", notifications.Events[0].Message);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_FulfillmentRuleNotMet

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_FulfillmentRuleNotMet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var mediumCarton = Helper.CreateWhsCartonSize("MED", 15, 10, 10, 10, 30, 100, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var largeCarton = Helper.CreateWhsCartonSize("LRG", 15, 15, 15, 15, 50, 500, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			whsGroup.CartonSizes.AddRange(new[] { smallCarton, mediumCarton, largeCarton });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Grams, UOMPackTypesList.Codes.SplitCase);
			data.Part2.OP_StockKeepingUnit = refType1.F3_Code;

			// Set up product so 15 units will fit in 2x Small cartons or 1x Medium Carton
			data.Part2.OP_CubicUQ = Constants.Volume.CubicCentimeters;
			data.Part2.OP_Depth = 0.5m;
			data.Part2.OP_Width = 0.5m;
			data.Part2.OP_Height = 0.5m;
			data.Part2.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part2.OP_Weight = 1m; // This is the important field. 15 * 10 > (20 - 10)
			data.Part2.OP_WeightUQ = Constants.Weight.Grams;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 15m);
			Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition", false, pick.IsFulfillmentRulesMet());
			AssertEquals("Precondition", PickStatus.Codes.Building, pick.WP_PickStatus);

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'false' as we encountered a fatal error.", false, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));

			AssertEquals("Should have shown an actual Error Message.", true, notifications.Events.Length > 0);
			AssertEquals("You cannot Allocate Package Labels until Fulfillment Rules are met or overridden.",
				notifications.Events[0].Message);
		}

		#endregion

		#region TestRunChecksPriorToCartonisingOrPickingByLabel_FulfillmentRuleNotMet_RuleOverriden

		public void TestRunChecksPriorToCartonisingOrPickingByLabel_FulfillmentRuleNotMet_RuleOverriden()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var mediumCarton = Helper.CreateWhsCartonSize("MED", 15, 10, 10, 10, 30, 100, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var largeCarton = Helper.CreateWhsCartonSize("LRG", 15, 15, 15, 15, 50, 500, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			whsGroup.CartonSizes.AddRange(new[] { smallCarton, mediumCarton, largeCarton });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Grams, UOMPackTypesList.Codes.SplitCase);
			data.Part2.OP_StockKeepingUnit = refType1.F3_Code;

			// Set up product so 15 units will fit in 2x Small cartons or 1x Medium Carton
			data.Part2.OP_CubicUQ = Constants.Volume.CubicCentimeters;
			data.Part2.OP_Depth = 0.5m;
			data.Part2.OP_Width = 0.5m;
			data.Part2.OP_Height = 0.5m;
			data.Part2.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part2.OP_Weight = 1m; // This is the important field. 15 * 10 > (20 - 10)
			data.Part2.OP_WeightUQ = Constants.Weight.Grams;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 15m);
			Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition", false, pick.IsFulfillmentRulesMet());
			AssertEquals("Precondition", PickStatus.Codes.Building, pick.WP_PickStatus);

			order.OverrideFulfillmentRuleAndCreateLog(Notify);
			Factory.Save();

			AssertEquals("Fulfillment rule overriden.", PickStatus.Codes.Created, pick.WP_PickStatus);
			Factory.Save();

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should return 'true', no errors are reported.", true, allocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(pick));
			AssertEquals("Should not have shown an actual Error Message.", false, notifications.Events.Length > 0);
		}

		#endregion

		#endregion

		#region Cartonise Split Cases

		#region TestCartoniseSplitCases_EndToEnd

		public void TestCartoniseSplitCases_EndToEnd()
		{
			// The purpose of this test is NOT to test permutations of inputs to the algorithm
			// It is just a cursory test to ensure the algorithm works when not mocked out
			// Comprehensive tests can be found alongside the algorithm.
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var mediumCarton = Helper.CreateWhsCartonSize("MED", 15, 10, 10, 10, 30, 100, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			var largeCarton = Helper.CreateWhsCartonSize("LRG", 15, 15, 15, 15, 50, 500, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			whsGroup.CartonSizes.AddRange(new[] { smallCarton, mediumCarton, largeCarton });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Grams, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			// Set up product so 15 units will fit in 2x Small cartons or 1x Medium Carton
			data.Part1.OP_CubicUQ = Constants.Volume.CubicCentimeters;
			data.Part1.OP_Depth = 0.5m;
			data.Part1.OP_Width = 0.5m;
			data.Part1.OP_Height = 0.5m;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_Weight = 1m; // This is the important field. 15 * 10 > (20 - 10)
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertEquals("Order should still be attached to pick", true, pick.Orders.Contains(order));
			var packageJob = order.PackageJob;
			AssertEquals("Should have created package.", 1, packageJob.Packages.Count);

			AssertEquals("Should have generated IDs.", true, packageJob.Packages.All(p => p.KP_PackageID.StartsWith(string.Format("{0}-00", packageJob.ParentJob.JobNo))));
			AssertEquals("Should not show any messages.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertCreatedPackages(mediumCarton, whsGroup, Constants.PkgUnit.Carton, packageJob.Packages);

			// Modify part config and retry
			data.Part1.OP_Weight = 0.25m;
			packageJob.Packages[0].Delete();
			AssertEquals("Precondition.", 0, packageJob.Packages.Count);

			AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals("Order should still be attached to pick", true, pick.Orders.Contains(order));
			AssertEquals("Should have created package.", 1, packageJob.Packages.Count);

			AssertEquals("Should have generated IDs.", true, packageJob.Packages.All(p => p.KP_PackageID.StartsWith(string.Format("{0}-00", packageJob.ParentJob.JobNo))));
			AssertEquals("Should not show any messages.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertCreatedPackages(smallCarton, whsGroup, Constants.PkgUnit.Carton, packageJob.Packages);
			AssertPickLineAssignedToPackage(packageJob.Packages.Single().PackedItemDivots.Single(), pick.GetAllPickLines().Single());
		}

		[NUnit.Framework.GuiTest]
		public void TestCartoniseSplitCases_EndToEnd_MinimiseVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			whsGroup.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;
			var smallCarton = Helper.CreateWhsCartonSize("SML", 41m, 28m, 19.5m, 0.3m, 19m, 999999, new ZByte(99), Constants.Length.Centimetres, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			var mediumCarton = Helper.CreateWhsCartonSize("MED", 41m, 28m, 28m, 0.4m, 20m, 999999, new ZByte(99), Constants.Length.Centimetres, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			whsGroup.CartonSizes.AddRange(new[] { smallCarton, mediumCarton });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			PackingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			PackingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Case, UOMPackTypesList.Codes.Case);
			PackingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Case, 4m);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Case, Constants.PkgUnit.Pallet, 32m);
			data.Part1.OP_Depth = 34m;
			data.Part1.OP_Width = 27.5m;
			data.Part1.OP_Height = 11m;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			Helper.SetProductWeightAndVolume(data.Part1, 4.1m, Constants.Weight.Kilograms, 0.01m, Constants.Volume.CubicMetres);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 128m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 4m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_ForceSplitCaseUOMTypeAllocation = true;
			pick.AutoAllocateItems();
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageJob = order.PackageJob;
			AssertContainsExactElementsInAnyOrder(new[] { "1 - SML", "1 - SML" }, packageJob.Packages.Select(p => p.CartonGroupAndSize));
		}

		#endregion

		#region TestCartoniseSplitCases_SimpleCase

		public void TestCartoniseSplitCases_SimpleCase()
		{
			TestCartoniseSplitCases_SimpleCase(useTwoCartonTypes: false);
		}

		public void TestCartoniseSplitCases_SimpleCase_SSCC()
		{
			TestCartoniseSplitCases_SimpleCase(useTwoCartonTypes: false, useSSCC: true);
		}

		public void TestCartoniseSplitCases_SimpleCase_WithTwoCartonTypes()
		{
			TestCartoniseSplitCases_SimpleCase(useTwoCartonTypes: true);
		}

		void TestCartoniseSplitCases_SimpleCase(bool useTwoCartonTypes, bool useSSCC = false)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			if (useSSCC)
			{
				data.Whs1.WW_UseGS1PrefixFallback = true;
				data.Whs1.WarehouseAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1111111");
			}

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize1 = Helper.CreateWhsCartonSize("WH", 1, 2, 4, 8, 16, 32, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize1);

			var whsSize2 = Helper.CreateWhsCartonSize("WH2", 3, 5, 7, 9, 11, 13, new ZByte(80), Constants.Length.Feet, Constants.Weight.Kilograms);
			if (useTwoCartonTypes)
			{
				whsGroup.CartonSizes.Add(whsSize2);
			}

			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			var refType2 = PackingHelper.CreateRefPackType("2", "2", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Case);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;
			data.Part2.OP_StockKeepingUnit = refType2.F3_Code;
			part3.OP_StockKeepingUnit = refType1.F3_Code;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Helper.CreateWhsOrderLine(order, part3, 5m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 3, pick.OrderedInventories.Count);

			// Set up mock which packs the single part 1 pack line into
			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);
			var part3OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == part3.PK);
			var pickLineForPart1 = part1OrderedInventory.AvailableInventories[0].PickLines.Single();
			var pickLineForPart3 = part3OrderedInventory.AvailableInventories[0].PickLines.Single();
			var cartonisationResult1 = new CartonisationAlgorithmResult(whsSize1.PK);
			cartonisationResult1.Items.Add(new CartonisationItem(pickLineForPart1.PK, 5));

			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			pickLineForPart1.WZ_GS_NKAssignedTo = pickerAAA.GS_Code;

			if (!useTwoCartonTypes)
			{
				cartonisationResult1.Items.Add(new CartonisationItem(pickLineForPart3.PK, 5));
			}

			var cartonisationResult2 = new CartonisationAlgorithmResult(whsSize2.PK);
			cartonisationResult2.Items.Add(new CartonisationItem(pickLineForPart3.PK, 5));

			IEnumerable<CartonisationAlgorithmResult> method(IEnumerable<ICartonisableItem> arg1, IEnumerable<ICartonDefinition> arg2)
			{
				// Assert Input
				AssertContainsExactElementsInAnyOrder("Should have passed in pick lines for Part1 + Part3.", new[] { pickLineForPart1, pickLineForPart3 }, arg1);
				AssertContainsExactElementsInAnyOrder("Should have passed in CartonSizes.", whsGroup.CartonGroupSizeLinks, arg2);

				// Provide output, used as input to code which is tested in below assertions
				return useTwoCartonTypes ? new[] { cartonisationResult1, cartonisationResult2 } : new[] { cartonisationResult1 };
			}

			var mock = new Mock<ICartonisation>();
			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
					It.IsAny<IEnumerable<ICartonDefinition>>()))
				.Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>(method);

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Should have saved.", false, pick.HasChanges);
				mock.VerifyAll();
			}

			AssertEquals("Order should still be attached to pick", true, pick.Orders.Contains(order));
			var packageJob = order.PackageJob;
			AssertEquals("Should have created package(s).", useTwoCartonTypes ? 2 : 1, packageJob.Packages.Count);

			var packageIDShouldStartWith = useSSCC ? "01111111" : string.Format("{0}-00", packageJob.ParentJob.JobNo);
			AssertEquals("Should have generated IDs.", true, packageJob.Packages.All(p => p.KP_PackageID.StartsWith(packageIDShouldStartWith)));
			AssertEquals("Should not show any messages.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals("Should have cleared Picked By", string.Empty, pickLineForPart1.WZ_GS_NKAssignedTo);

			if (!useTwoCartonTypes)
			{
				AssertCreatedPackages(whsSize1, whsGroup, Constants.PkgUnit.Carton, packageJob.Packages);
				AssertEquals(1, packageJob.Packages.Count);
				AssertEquals("Packed Items", 2, packageJob.Packages.Single().PackedItemDivots.Count);
				AssertEquals("ParentTableCode", true, packageJob.Packages.Single().PackedItemDivots.All(divot => divot.KI_ParentTableCode == WhsPickLineSchema.Constants.Prefix));
				AssertEquals("PackedQty", true, packageJob.Packages.Single().PackedItemDivots.All(divot => divot.KI_PackedQty == 5m));
				AssertEquals("ParentID", true, packageJob.Packages.Single().PackedItemDivots.All(divot => pick.GetAllPickLines().Select(pl => pl.PK).Contains(divot.KI_ParentID)));
			}
			else
			{
				var carton1Packages = packageJob.Packages.Where(p => p.KP_DimensionUQ == Constants.Length.Metres);
				var carton2Packages = packageJob.Packages.Where(p => p.KP_DimensionUQ == Constants.Length.Feet);

				AssertEquals(1, carton1Packages.Count());
				AssertEquals(1, carton2Packages.Count());

				AssertCreatedPackages(whsSize1, whsGroup, Constants.PkgUnit.Carton, carton1Packages);
				AssertCreatedPackages(whsSize2, whsGroup, Constants.PkgUnit.Carton, carton2Packages);
				AssertPickLineAssignedToPackage(carton1Packages.Single().PackedItemDivots.Single(), pickLineForPart1);
				AssertPickLineAssignedToPackage(carton2Packages.Single().PackedItemDivots.Single(), pickLineForPart3);
			}
		}

		#endregion

		#region TestCartoniseSplitCases_RunOrderByOrder

		public void TestCartoniseSplitCases_RunOrderByOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var carrier = Helper.CreateClient("CAR");

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);

			var whsGroup = Helper.CreateWhsCartonGroup("4", "WhsOrg");
			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var carrierSize = Helper.CreateWhsCartonSize("CA", 1, 2, 4, 8, 16, 32, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);
			var whsSize = Helper.CreateWhsCartonSize("WH", 3, 5, 7, 9, 11, 13, new ZByte(80), Constants.Length.Feet, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			carrierGroup.CartonSizes.Add(carrierSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;
			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;

			order2.TransportCoNameOrPK = carrier.PK.ToString();
			Factory.Save();

			var order1Hits = 0;
			var order2Hits = 0;

			var mock = new Mock<ICartonisation>();

			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
					It.IsAny<IEnumerable<ICartonDefinition>>()))
				.Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>(
					(arg1, arg2) =>
					{
						var pickLines = arg1.Cast<WhsPickLine>();
						var cartonSizes = arg2.Cast<WhsCartonGroupSizeLink>().Select(l => l.CartonSize);
						AssertEquals(1, pickLines.Count());

						var order = pickLines.First().DocketLine.Docket;
						if (order == order1)
						{
							AssertEquals(whsSize, cartonSizes.Single());
							order1Hits++;
						}
						else if (order == order2)
						{
							AssertEquals(carrierSize, cartonSizes.Single());
							order2Hits++;
						}
						else
						{
							Fail("Other order passed in?");
						}

						var cartonisationResult = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
						cartonisationResult.Items.Add(new CartonisationItem(pickLines.Single().PK, 5));
						return new[] { cartonisationResult };
					});

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Should have saved.", false, pick.HasChanges);
				mock.VerifyAll();
			}

			AssertEquals(1, order1Hits);
			AssertEquals(1, order2Hits);

			AssertContainsExactElementsInAnyOrder("Orders should still be attached to pick", new[] { order1, order2 }, pick.Orders);

			var packageJob1 = order1.PackageJob;
			var packageJob2 = order2.PackageJob;
			AssertEquals("Should have created package for Order1.", 1, packageJob1.Packages.Count);
			AssertEquals("Should have created package for Order2.", 1, packageJob2.Packages.Count);
			AssertEquals("Should have generated IDs for Order1.", true, packageJob1.Packages.All(p => p.KP_PackageID.StartsWith(string.Format("{0}-00", packageJob1.ParentJob.JobNo))));
			AssertEquals("Should have generated IDs for Order2.", true, packageJob2.Packages.All(p => p.KP_PackageID.StartsWith(string.Format("{0}-00", packageJob2.ParentJob.JobNo))));

			AssertCreatedPackages(whsSize, whsGroup, Constants.PkgUnit.Carton, packageJob1.Packages);
			AssertCreatedPackages(carrierSize, carrierGroup, Constants.PkgUnit.Carton, packageJob2.Packages);

			AssertPickLineAssignedToPackage(packageJob1.Packages.Single().PackedItemDivots.Single(), order1.Lines[0].PickLines[0]);
			AssertPickLineAssignedToPackage(packageJob2.Packages.Single().PackedItemDivots.Single(), order2.Lines[0].PickLines[0]);
		}

		#endregion

		#region TestCartonizeSplitCases_SplitsBy

		void TestCartonizeSplitCases_SplitsByCore(bool hasParam, bool splitByArea, bool splitByProduct, bool splitByCategory, bool isCategoryEmpty, int expectedPackages)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2, 2);

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");

			Factory.Save();

			var area2 = Helper.CreateArea(data.Whs1, "B");
			var area3 = Helper.CreateArea(data.Whs1, "C");
			var area4 = Helper.CreateArea(data.Whs1, "D");

			var location1 = data.Whs1.FindLocation("B-1-1-1");
			var location2 = data.Whs1.FindLocation("B-1-1-2");
			var location3 = data.Whs1.FindLocation("B-1-2-1");
			var location4 = data.Whs1.FindLocation("B-1-2-2");
			var location5 = data.Whs1.FindLocation("B-2-1-1");
			var location6 = data.Whs1.FindLocation("B-2-1-2");
			var location7 = data.Whs1.FindLocation("B-2-2-1");
			var location8 = data.Whs1.FindLocation("B-2-2-2");
			location3.WLV_WA_PickingArea = area2.PK;
			location4.WLV_WA_PickingArea = area2.PK;
			location5.WLV_WA_PickingArea = area3.PK;
			location6.WLV_WA_PickingArea = area3.PK;
			location7.WLV_WA_PickingArea = area4.PK;
			location8.WLV_WA_PickingArea = area4.PK;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;
			data.Part2.OP_StockKeepingUnit = refType1.F3_Code;
			part3.OP_StockKeepingUnit = refType1.F3_Code;
			part4.OP_StockKeepingUnit = refType1.F3_Code;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1");

			foreach (var product in new[] { data.Part1, data.Part2, part3, part4 })
			{
				foreach (var location in new[] { location1, location2, location3, location4, location5, location6, location7, location8 })
				{
					Helper.CreateWhsReceiveInventoryLine(receive, product, 5m, location);
				}
			}

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "W");
			var partGroup = Helper.CreateWhsCartonGroup("2", "P");
			var whsSize = Helper.CreateWhsCartonSize("WH", 3, 5, 7, 9, 11, 9999, new ZByte(80), Constants.Length.Feet, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			partGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var relation1 = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var category1 = Factory.New<OrgPartCategory>();
			category1.OPC_CategoryCode = "ABC";
			category1.OPC_CategoryDescription = "ABC";
			relation1.OU_WCG_CartonGroup = partGroup.PK;
			relation1.OU_OPC_Category = category1.PK;

			var relation2 = data.Part2.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var category2 = Factory.New<OrgPartCategory>();
			category2.OPC_CategoryCode = "BCD";
			category2.OPC_CategoryDescription = "BCD";
			relation2.OU_WCG_CartonGroup = partGroup.PK;
			relation2.OU_OPC_Category = category2.PK;

			var relation3 = part3.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation3.OU_WCG_CartonGroup = partGroup.PK;
			relation3.OU_OPC_Category = category2.PK;

			var relation4 = part4.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation4.OU_WCG_CartonGroup = partGroup.PK;
			relation4.OU_OPC_Category = category1.PK;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 40m);
			Helper.CreateWhsOrderLine(order, data.Part2, 40m);
			Helper.CreateWhsOrderLine(order, part3, 40m);
			Helper.CreateWhsOrderLine(order, part4, 40m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 32, pick.GetAllPickLines().Count());

			if (isCategoryEmpty)
			{
				relation1.OU_OPC_Category = ZGuid.Empty;
				relation2.OU_OPC_Category = ZGuid.Empty;
				relation3.OU_OPC_Category = ZGuid.Empty;
				relation4.OU_OPC_Category = ZGuid.Empty;
			}

			if (hasParam)
			{
				var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
				pickParams.WPP_WW_Warehouse = data.Whs1.PK;
				pickParams.WPP_CartoniseByArea = splitByArea;
				pickParams.WPP_CartonizeByProduct = splitByProduct;
				pickParams.WPP_CartonizeByProductCategory = splitByCategory;
			}

			AssertEquals("Preconditon: No Packages.", 0, order.PackageJob.Packages.Count);

			var mock = new Mock<ICartonisation>();
			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
					It.IsAny<IEnumerable<ICartonDefinition>>()))
				.Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>((arg1, arg2) =>
				{
					var cartonSizes = arg2;
					var pickLines = arg1.Cast<WhsPickLine>();
					AssertEquals("Split by Area is " + splitByArea + ", pick lines for multiple areas " + (splitByArea ? "shouldn't" : "should") + " have been passed in.", splitByArea ? 1 : 4, pickLines.Select(pl => pl.InventoryLine.LocationArea).Distinct().Count());
					AssertEquals("Split by Prodcut is " + splitByProduct + ", pick lines for mutiple products " + ((splitByProduct || (splitByCategory && isCategoryEmpty)) ? "shouldn't" : "should") + " have been passed in.", (splitByProduct || (splitByCategory && isCategoryEmpty)) ? 1 : splitByCategory ? 2 : 4, pickLines.Select(pl => pl.InventoryLine.Product).Distinct().Count());
					AssertEquals("Split by Category is " + splitByCategory + ", pick lines for mutiple category " + ((splitByProduct || splitByCategory) ? "shouldn't" : "should") + " have been passed in.", (splitByProduct || splitByCategory) ? 1 : 2, pickLines.Select(pl => pl.InventoryLine.SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).Category).Distinct().Count());

					var cartonizationResult = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
					foreach (var pickLine in pickLines)
					{
						cartonizationResult.Items.Add(new CartonisationItem(pickLine.PK, (int)pickLine.WZ_Units));
					}
					return new[] { cartonizationResult };
				});

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				mock.Verify(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
					It.IsAny<IEnumerable<ICartonDefinition>>()), Times.Exactly(expectedPackages));
			}

			AssertEquals("Should have created " + expectedPackages + " packages.", expectedPackages, order.PackageJob.Packages.Count);
		}

		#region TestCartonizeSplitCases_SplitsByDefault(Area on, Product off, Category off)

		public void TestCartonizeSplitCases_SplitsByDefault()
		{
			TestCartonizeSplitCases_SplitsByCore(hasParam: false, splitByArea: true, splitByProduct: false, splitByCategory: false, isCategoryEmpty: false, expectedPackages: 4);
		}

		#endregion

		#region TestCartonizeSplitCases_SplitsByAreaOffProductOffCategoryOff(Area off, Product off, Category off)

		public void TestCartonizeSplitCases_SplitsByAreaOffProductOffCategoryOff()
		{
			TestCartonizeSplitCases_SplitsByCore(hasParam: true, splitByArea: false, splitByProduct: false, splitByCategory: false, isCategoryEmpty: false, expectedPackages: 1);
		}

		#endregion

		#region TestCartonizeSplitCases_SplitsByAreaOnProductOnCategoryOff(Area on, Product on, Category off)

		public void TestCartonizeSplitCases_SplitsByAreaOnProductOnCategoryOff()
		{
			TestCartonizeSplitCases_SplitsByCore(hasParam: true, splitByArea: true, splitByProduct: true, splitByCategory: false, isCategoryEmpty: false, expectedPackages: 16);
		}

		#endregion

		#region TestCartonizeSplitCases_SplitsByAreaOffProductOnCategoryOff(Area off, Product on, Category off)

		public void TestCartonizeSplitCases_SplitsByAreaOffProductOnCategoryOff()
		{
			TestCartonizeSplitCases_SplitsByCore(hasParam: true, splitByArea: false, splitByProduct: true, splitByCategory: false, isCategoryEmpty: false, expectedPackages: 4);
		}

		#endregion

		#region TestCartonizeSplitCases_SplitsByAreaOffProductOffCategoryOn(Area off, Product off, Category on)

		public void TestCartonizeSplitCases_SplitsByAreaOffProductOffCategoryOn()
		{
			TestCartonizeSplitCases_SplitsByCore(hasParam: true, splitByArea: false, splitByProduct: false, splitByCategory: true, isCategoryEmpty: false, expectedPackages: 2);
		}

		#endregion

		#region TestCartonizeSplitCases_SplitsByAreaOnProductOffCategoryOn(Area on, Product off, Category on)

		public void TestCartonizeSplitCases_SplitsByAreaOnProductOffCategoryOn()
		{
			TestCartonizeSplitCases_SplitsByCore(hasParam: true, splitByArea: true, splitByProduct: false, splitByCategory: true, isCategoryEmpty: false, expectedPackages: 8);
		}

		#endregion

		#region TestCartonizeSplitCases_SplitsByAreaOnProductOffCategoryOnWithNullCategory(Area on, Product off, Category on, OU_OPC_Category is null)

		public void TestCartonizeSplitCases_SplitsByAreaOnProductOffCategoryOnWithNullCategory()
		{
			TestCartonizeSplitCases_SplitsByCore(hasParam: true, splitByArea: true, splitByProduct: false, splitByCategory: true, isCategoryEmpty: true, expectedPackages: 16);
		}

		#endregion

		#region TestCartonizeSplitCases_SplitsByAreaOffProductOffCategoryOnWithNullCategory(Area off, Product off, Category on, OU_OPC_Category is null)

		public void TestCartonizeSplitCases_SplitsByAreaOffProductOffCategoryOnWithNullCategory()
		{
			TestCartonizeSplitCases_SplitsByCore(hasParam: true, splitByArea: false, splitByProduct: false, splitByCategory: true, isCategoryEmpty: true, expectedPackages: 4);
		}

		#endregion

		#endregion

		#region TestCartoniseSplitCases_SplitPickLine

		public void TestCartoniseSplitCases_SplitPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 10m, pick.GetAllPickLines().Single().WZ_Units);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH");
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var mock = new Mock<ICartonisation>();

			IEnumerable<CartonisationAlgorithmResult> method(IEnumerable<ICartonisableItem> arg1, IEnumerable<ICartonDefinition> arg2)
			{
				var pickLines = arg1.Cast<WhsPickLine>();
				var cartonSizes = arg2;
				AssertEquals("Precondition", 1, pickLines.Count());
				AssertEquals("Precondition", 1, cartonSizes.Count());

				var cartonisationResult1 = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
				cartonisationResult1.Items.Add(new CartonisationItem(pickLines.Single().PK, 4));

				var cartonisationResult2 = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
				cartonisationResult2.Items.Add(new CartonisationItem(pickLines.Single().PK, 4));

				var cartonisationResult3 = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
				cartonisationResult3.Items.Add(new CartonisationItem(pickLines.Single().PK, 2));
				return new[] { cartonisationResult1, cartonisationResult2, cartonisationResult3 };
			}

			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
				It.IsAny<IEnumerable<ICartonDefinition>>())).Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>(method);

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Should have saved.", false, pick.HasChanges);
				mock.VerifyAll();
			}

			AssertEquals("Order should still be attached to pick", true, pick.Orders.Contains(order));
			AssertEquals("Should have split pick lines.", 3, pick.GetAllPickLines().Count());
			AssertEquals("Should have split pick lines.", 2, pick.GetAllPickLines().Count(pl => pl.WZ_WE_TransactionLine == order.Lines[0].PK && pl.WZ_Units == 4m));
			AssertEquals("Should have split pick lines.", 1, pick.GetAllPickLines().Count(pl => pl.WZ_WE_TransactionLine == order.Lines[0].PK && pl.WZ_Units == 2m));
			AssertEquals("AvailableInventory cache should have been updated.", 3, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());

			var packageJob = order.PackageJob;
			AssertEquals("Should have created package for Order1.", 3, packageJob.Packages.Count);
			AssertEquals("Should have generated IDs for Order1.", true, packageJob.Packages.All(p => p.KP_PackageID.StartsWith(string.Format("{0}-00", packageJob.ParentJob.JobNo))));
			AssertCreatedPackages(whsSize, whsGroup, Constants.PkgUnit.Carton, packageJob.Packages);
			AssertContainsExactElementsInAnyOrder("Each pick line is assigned to its own package", pick.GetAllPickLines().Select(pl => pl.PK), packageJob.Packages.Select(p => p.PackedItemDivots.Single().KI_ParentID));
		}

		public void TestCartoniseSplitCases_SplitPickLine_PickByBOM()
		{
			TestCartoniseSplitCases_SplitPickLine_PickByBOM_Core(useNewFactory: false);
		}

		public void TestCartoniseSplitCases_SplitPickLine_PickByBOM_NewFactory()
		{
			TestCartoniseSplitCases_SplitPickLine_PickByBOM_Core(useNewFactory: true);
		}

		void TestCartoniseSplitCases_SplitPickLine_PickByBOM_Core(bool useNewFactory)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			bike.OP_StockKeepingUnit = refType1.F3_Code;
			wheel.OP_StockKeepingUnit = refType1.F3_Code;
			Helper.CreateProductBOM(bike, wheel, 2m, refType1.F3_Code);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 10m);
			var pick = Helper.CreatePickNew(order);
			var componentOrderLine = orderLine1.ChildComponentLines.Single();

			var pickLine = orderLine1.PickLines.Single();
			var wheelPickLine = componentOrderLine.PickLines.Single();
			AssertNotNull("Precondition", pickLine.SupplierPart);
			AssertEquals("1", pickLine.WZ_F3_NKAllocatedPackType);
			AssertEquals("1", wheelPickLine.WZ_F3_NKAllocatedPackType);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH");
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;
			Factory.Save();

			var mock = new Mock<ICartonisation>();

			IEnumerable<CartonisationAlgorithmResult> method(IEnumerable<ICartonisableItem> arg1, IEnumerable<ICartonDefinition> arg2)
			{
				var pickLines = arg1.Cast<WhsPickLine>();
				var cartonSizes = arg2;
				AssertEquals("Precondition", 1, pickLines.Count());
				AssertEquals("Precondition", 1, cartonSizes.Count());

				var cartonisationResult1 = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
				cartonisationResult1.Items.Add(new CartonisationItem(pickLines.Single().PK, 4));

				var cartonisationResult2 = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
				cartonisationResult2.Items.Add(new CartonisationItem(pickLines.Single().PK, 4));

				var cartonisationResult3 = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
				cartonisationResult3.Items.Add(new CartonisationItem(pickLines.Single().PK, 2));
				return new[] { cartonisationResult1, cartonisationResult2, cartonisationResult3 };
			}

			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
				It.IsAny<IEnumerable<ICartonDefinition>>())).Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>(method);

			using (ObjectFactory.Substitute(mock.Object))
			{
				var factory = useNewFactory ? new BusinessObjectFactory() : Factory;
				if (useNewFactory)
				{
					pick = factory.Load<WhsPick>(pick.PK);
				}
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				AssertNoExceptionThrown(() => factory.Save());
				AssertEquals("Should have saved.", false, pick.HasChanges);
				mock.VerifyAll();
			}

			AssertEquals("Order should still be attached to pick", true, pick.Orders.Contains(order));

			var kitOrderLine = order.ParentLines.Single();
			AssertEquals("Bikes Cartonised.", 3, kitOrderLine.PickLines.Count);
			AssertEquals("Should have split pick lines.", 2, kitOrderLine.PickLines.Count(pl => pl.WZ_WE_TransactionLine == kitOrderLine.PK && pl.WZ_Units == 4m));
			AssertEquals("Should have split pick lines.", 1, kitOrderLine.PickLines.Count(pl => pl.WZ_WE_TransactionLine == kitOrderLine.PK && pl.WZ_Units == 2m));

			AssertEquals("ComponnetLines remains the same.", "1", componentOrderLine.PickLines.Single().WZ_F3_NKAllocatedPackType);
			AssertEquals("1", pickLine.WZ_F3_NKAllocatedPackType);
			AssertEquals("Wheels not Cartonised.", 1, componentOrderLine.PickLines.Count);

			var packageJob = order.PackageJob;
			AssertEquals("Should have created package for Order1.", 3, packageJob.Packages.Count);
			AssertEquals("Should have generated IDs for Order1.", true, packageJob.Packages.All(p => p.KP_PackageID.StartsWith(string.Format("{0}-00", packageJob.ParentJob.JobNo))));
			AssertCreatedPackages(whsSize, whsGroup, Constants.PkgUnit.Carton, packageJob.Packages);
			AssertContainsExactElementsInAnyOrder("Each pick line is assigned to its own package", kitOrderLine.PickLines.Select(pl => pl.PK), packageJob.Packages.Select(p => p.PackedItemDivots.Single().KI_ParentID));
		}

		#endregion

		#region TestCartoniseSplitCases_ItemOnOrderDoesntFit

		public void TestCartoniseSplitCases_ItemOnOrderDoesntFit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;
			data.Part2.OP_StockKeepingUnit = refType1.F3_Code;
			part3.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "3", part3, 5m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 5m);
			Helper.CreateWhsOrderLine(order1, part3, 5m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH", 3, 5, 7, 9, 11, 13, new ZByte(80), Constants.Length.Feet, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;
			Factory.Save();

			var order1Hits = 0;
			var order2Hits = 0;

			var mock = new Mock<ICartonisation>();

			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
				It.IsAny<IEnumerable<ICartonDefinition>>())).Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>(
				(arg1, arg2) =>
				{
					var pickLines = arg1.Cast<WhsPickLine>();
					var cartonSizes = arg2;
					AssertEquals("Precondition", whsSize, ((WhsCartonGroupSizeLink)cartonSizes.Single()).CartonSize);

					var order = pickLines.First().DocketLine.Docket;
					if (order == order1)
					{
						AssertEquals("Precondition", 3, pickLines.Count());
						order1Hits++;
					}
					else if (order == order2)
					{
						AssertEquals("Precondition", 1, pickLines.Count());
						order2Hits++;
					}
					else
					{
						Fail("Other order passed in?");
					}

					var result = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
					result.Items.Add(new CartonisationItem(pickLines.First().PK, 5));
					// Order 1, Line 2 & 3 do not get cartonised

					return new[] { result };
				});

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				AssertEquals("Should have saved.", false, pick.HasChanges);
				mock.VerifyAll();
			}

			AssertEquals("Precondition", 1, order1Hits);
			AssertEquals("Precondition", 1, order2Hits);

			AssertContainsExactElementsInAnyOrder("Order1 should be detached from pick", new[] { order2 }, pick.Orders);

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Shouldn't have added Errors as we can progress by detaching orders.", false,
				notifications.Events.Any(e => e.Message == "No Order(s) could be Cartonized. Check the Events on the Pick for more details."));

			AssertEquals("Should have added an ErrorReport event.", 2, logs.Length);
			AssertEquals("Should have errors if Cartonising SplitCase goods.",
				string.Format("Error Report: Cartonization, Failed To Cartonize, Order: {0} Product: {1}", order1.WD_DocketID, data.Part2.OP_PartNum), logs[0].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.",
				string.Format("Error Report: Cartonization, Failed To Cartonize, Order: {0} Product: {1}", order1.WD_DocketID, part3.OP_PartNum), logs[1].DisplayEventReference);

			var packageJobOrder2 = order2.PackageJob;
			AssertEquals("Should have created package for Order2.", 1, packageJobOrder2.Packages.Count);
			AssertEquals("Should have generated IDs for Order2.", true, packageJobOrder2.Packages.All(p => p.KP_PackageID.StartsWith(string.Format("{0}-00", packageJobOrder2.ParentJob.JobNo))));

			AssertCreatedPackages(whsSize, whsGroup, Constants.PkgUnit.Carton, packageJobOrder2.Packages);
			AssertPickLineAssignedToPackage(packageJobOrder2.Packages.Single().PackedItemDivots.Single(), order2.Lines[0].PickLines[0]);
		}

		#endregion

		#region TestCartoniseSplitCases_ItemOnOrderDoesntFit_NoOrdersCanBeCartonised

		public void TestCartoniseSplitCases_ItemOnOrderDoesntFit_NoOrdersCanBeCartonised_FactorySave()
		{
			TestCartoniseSplitCases_ItemOnOrderDoesntFit_NoOrdersCanBeCartonised_Core(true);
		}

		public void TestCartoniseSplitCases_ItemOnOrderDoesntFit_NoOrdersCanBeCartonised_NoFactorySave()
		{
			TestCartoniseSplitCases_ItemOnOrderDoesntFit_NoOrdersCanBeCartonised_Core(false);
		}

		void TestCartoniseSplitCases_ItemOnOrderDoesntFit_NoOrdersCanBeCartonised_Core(bool factorySave)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;
			data.Part2.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 5m);
			var pick = Helper.CreatePickNew(order1, order2);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH", 3, 5, 7, 9, 11, 13, new ZByte(80), Constants.Length.Feet, Constants.Weight.Pounds);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;
			Factory.Save();

			var mock = new Mock<ICartonisation>();

			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
				It.IsAny<IEnumerable<ICartonDefinition>>())).Returns(() => Enumerable.Empty<ICartonWithItems>());

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should have returned errors as nothing could be cartonised.", CartonisationResult.Error, allocatePackageLabelsStrategy.CartoniseSplitCases(pick, factorySave));
				AssertEquals("Should have or not have saved based on factorySave parameter.", !factorySave, pick.HasChanges);
				mock.VerifyAll();
			}

			AssertContainsExactElementsInAnyOrder("No orders should be detached from pick.", new[] { order1, order2 }, pick.Orders);

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have added an ErrorReport event.", 2, logs.Length);
			AssertEquals("Should have errors if Cartonising SplitCase goods.",
				string.Format("Error Report: Cartonization, Failed To Cartonize, Order: {0} Product: {1}", order1.WD_DocketID, data.Part1.OP_PartNum), logs[0].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.",
				string.Format("Error Report: Cartonization, Failed To Cartonize, Order: {0} Product: {1}", order2.WD_DocketID, data.Part2.OP_PartNum), logs[1].DisplayEventReference);

			AssertEquals("Should have shown an actual Error Message in addition to logging if all Orders failed.", true, notifications.Events.Length > 0);
			AssertEquals("No Order(s) could be Cartonized. Check the Events on the Pick for more details.", notifications.Events[0].Message);
		}

		#endregion

		#region TestCartoniseSplitCases_ItemOnOrderDoesntFit_NoCartonGroup_EndToEnd

		public void TestCartoniseSplitCases_ItemOnOrderDoesntFit_NoCartonGroup_EndToEnd()
		{
			// If no orders can be cartonised, we shouldn't remove all orders and lock down the pick etc. unneccesarily
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should have an error.", CartonisationResult.Error, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			AssertEquals("Should not have cartonised.", 0, order1.PackageJob.Packages.Count);
			AssertEquals("Should not have cartonised.", 0, order2.PackageJob.Packages.Count);
			AssertContainsExactElementsInAnyOrder("Orders should still be attached to pick", new[] { order1, order2 }, pick.Orders);
			AssertEquals("No Order(s) could be Cartonized. Check the Events on the Pick for more details.", notifications.Events[0].Message);
			AssertEquals("Should have saved.", false, pick.HasChanges);
		}

		#endregion

		#region TestCartoniseSplitCases_ItemOnOrderDoesntFit_CartonGroupWithNoSizes_EndToEnd

		public void TestCartoniseSplitCases_ItemOnOrderDoesntFit_CartonGroupWithNoSizes_EndToEnd()
		{
			// If no orders can be cartonised, we shouldn't remove all orders and lock down the pick etc. unneccesarily
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg"); // No Sizes
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;
			Factory.Save();

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should have an error.", CartonisationResult.Error, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			AssertEquals("Should not have cartonised.", 0, order.PackageJob.Packages.Count);
			AssertContainsExactElementsInAnyOrder("Orders should still be attached to pick", new[] { order }, pick.Orders);
			AssertEquals("No Order(s) could be Cartonized. Check the Events on the Pick for more details.", notifications.Events[0].Message);
			AssertEquals("Should have saved.", false, pick.HasChanges);
		}

		#endregion

		#region TestCartoniseSplitCases_NoAssociatedCartonGroups

		public void TestCartoniseSplitCases_NoAssociatedCartonGroups()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var splitCaseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);
			var caseRefType = PackingHelper.CreateRefPackType("2", "2", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.Case);
			data.Part1.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			data.Part2.OP_StockKeepingUnit = caseRefType.F3_Code;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			var notifications = ((NotificationBuffer)pick.NotificationSubscriber);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should have an error.", CartonisationResult.Error, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have errors if Cartonising SplitCase goods.", 1, logs.Length);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No Carton Group, Order: {0}", order1.WD_DocketID), logs[0].DisplayEventReference);
			AssertEquals("Should *not* have detached order if no Orders could be cartonised.", 1, pick.Orders.Count);
			AssertEquals("Should have shown an actual Error Message in addition to logging if all Orders failed.", true, notifications.Events.Length > 0);
			AssertEquals("No Order(s) could be Cartonized. Check the Events on the Pick for more details.", notifications.Events[0].Message);
			AssertEquals("Should have saved.", false, pick.HasChanges);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1, 5m);
			var order_WithNoSplitCaseStock = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part2, 5m);
			pick.Orders.AddRange(new[] { order2, order_WithNoSplitCaseStock });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			Assert("Precondition", order1.CartonGroup.OrgCartonGroupPK.IsEmpty);
			Assert("Precondition", order2.CartonGroup.OrgCartonGroupPK.IsEmpty);
			Assert("Precondition", order_WithNoSplitCaseStock.CartonGroup.OrgCartonGroupPK.IsEmpty);

			notifications.Clear();
			AssertEquals("Should have return 'NothingToCartonise' as there were no fatal errors.", CartonisationResult.NothingToCartonise, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have errors if Cartonising SplitCase goods.", 1 + 2, logs.Length);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No Carton Group, Order: {0}", order1.WD_DocketID), logs[0].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No Carton Group, Order: {0}", order1.WD_DocketID), logs[1].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No Carton Group, Order: {0}", order2.WD_DocketID), logs[2].DisplayEventReference);
			AssertContainsExactElementsInAnyOrder("Should have detached orders that could not be cartonised.", new[] { order_WithNoSplitCaseStock }, pick.Orders);
			AssertEquals("Should *not* have shown an actual Error Message if we can continue (by detaching orders).", false,
				notifications.Events.Any(e => e.Message == "No Order(s) could be Cartonized. Check the Events on the Pick for more details."));
			AssertEquals("Should have saved.", false, pick.HasChanges);

			var group = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var size1 = Helper.CreateWhsCartonSize("WH", 1, 2, 4, 8, 16, 32, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);
			group.CartonSizes.Add(size1);
			var consignee = Helper.CreateClient("CNE");
			consignee.MiscServ.OM_WCG_CartonGroup = group.PK;
			order2.ConsigneeNameOrPK = consignee.PK.ToString();

			Assert("Precondition", order1.CartonGroup.OrgCartonGroupPK.IsEmpty);
			Assert("Precondition", !order2.CartonGroup.OrgCartonGroupPK.IsEmpty);
			Assert("Precondition", order_WithNoSplitCaseStock.CartonGroup.OrgCartonGroupPK.IsEmpty);
			pick.Orders.AddRange(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			AssertEquals("Should have saved.", false, pick.HasChanges);
			logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have errors if Cartonising SplitCase goods.", 1 + 2 + 1, logs.Length);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No Carton Group, Order: {0}", order1.WD_DocketID), logs[0].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No Carton Group, Order: {0}", order1.WD_DocketID), logs[1].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No Carton Group, Order: {0}", order2.WD_DocketID), logs[2].DisplayEventReference);
			AssertEquals("Should have errors if Cartonising SplitCase goods.", string.Format("Error Report: Cartonization, No Carton Group, Order: {0}", order1.WD_DocketID), logs[3].DisplayEventReference);
			AssertContainsExactElementsInAnyOrder("Order2 should still be attached to the pick.", new[] { order2, order_WithNoSplitCaseStock }, pick.Orders);

			order1.ConsigneeNameOrPK = consignee.PK.ToString();
			Assert("Precondition", !order1.CartonGroup.OrgCartonGroupPK.IsEmpty);
			Assert("Precondition", !order2.CartonGroup.OrgCartonGroupPK.IsEmpty);
			Assert("Precondition", order_WithNoSplitCaseStock.CartonGroup.OrgCartonGroupPK.IsEmpty);

			CleanUp(pick);
			pick.Orders.Add(order1);
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should have no errors.", 1 + 2 + 1, logs.Length);
			AssertContainsExactElementsInAnyOrder("Should have detached orders that could not be cartonised.", new[] { order1, order2, order_WithNoSplitCaseStock }, pick.Orders);
		}

		#endregion

		#region TestCartoniseSplitCases_NonSplitCaseOrder

		public void TestCartoniseSplitCases_NonSplitCaseOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType = PackingHelper.CreateRefPackType("2", "2", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH");
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;
			Factory.Save();

			var mock = new Mock<ICartonisation>();
			mock.Verify(
				m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
					It.IsAny<IEnumerable<ICartonDefinition>>()), Times.Never);

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should return 'NothingToCartonise' as nothing was able to be cartonised and no packages were created (neither success or error).",
					CartonisationResult.NothingToCartonise, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				mock.VerifyAll();
			}

			var packageJob1 = order1.PackageJob;
			AssertNotNull("Should have package jobs as the orders are picked.", packageJob1);
			AssertEquals("Should *not* have created package for Order1.", 0, packageJob1.Packages.Count);

			AssertEquals("Order1 should still be attached to pick.", true, pick.Orders.Contains(order1));

			var notificationBuffer = ((NotificationBuffer)pick.NotificationSubscriber);
			AssertEquals("Should *not* have shown Info.", false, notificationBuffer.AsString.Contains("Error while Cartonising"));
		}

		#endregion

		#region TestCartoniseSplitCases_NonSplitCaseOrderIsntRemoved

		public void TestCartoniseSplitCases_NonSplitCaseOrderIsntRemoved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			var refType2 = PackingHelper.CreateRefPackType("2", "2", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			data.Part2.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part2, refType2.F3_Code, 2m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", refType2.F3_Code, order2.Lines[0].PickLines[0].WZ_F3_NKAllocatedPackType);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH");
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;
			Factory.Save();

			var mock = new Mock<ICartonisation>();
			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
				It.IsAny<IEnumerable<ICartonDefinition>>())).Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>(
				(arg1, arg2) =>
				{
					var pickLines = arg1.Cast<WhsPickLine>();
					var cartonSizes = arg2;
					AssertEquals("Precondition", 1, pickLines.Count());
					AssertEquals("Precondition", 1, cartonSizes.Count());

					var cartonisationResult = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
					cartonisationResult.Items.Add(new CartonisationItem(pickLines.Single().PK, 10));
					return new[] { cartonisationResult };
				});

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should succeed as there were no fatal errors.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Should have saved.", false, pick.HasChanges);
				mock.VerifyAll();
			}

			var packageJob1 = order1.PackageJob;
			var packageJob2 = order2.PackageJob;
			AssertNotNull("Should have package jobs as the orders are picked.", packageJob1);
			AssertNotNull("Should have package jobs as the orders are picked.", packageJob2);
			AssertEquals("Should have created package for Order1.", 1, packageJob1.Packages.Count);
			AssertEquals("Should not have cartonised Order2.", 0, packageJob2.Packages.Count);

			AssertEquals("Order1 should still be attached to pick.", true, pick.Orders.Contains(order1));
			AssertEquals("Order2 should still be attached to pick.", true, pick.Orders.Contains(order2));
			AssertEquals("Should not have reverted PickByUOM on the non Split Case order.", refType2.F3_Code, order2.Lines[0].PickLines[0].WZ_F3_NKAllocatedPackType);

			var notificationBuffer = ((NotificationBuffer)pick.NotificationSubscriber);
			AssertEquals("Should *not* have shown Info.", false, notificationBuffer.AsString.Contains("Error while Cartonising"));
		}

		#endregion

		#region TestCartoniseSplitCases_CartonGroups

		public void TestCartoniseSplitCases_CartonGroups()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var carrier = Helper.CreateClient("CAR");
			var consignee = Helper.CreateClient("CNE");

			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var cneGroup = Helper.CreateWhsCartonGroup("2", "Consignee");
			var clientGroup = Helper.CreateWhsCartonGroup("3", "WhsClient");
			var whsGroup = Helper.CreateWhsCartonGroup("4", "WhsOrg");
			var productGroup = Helper.CreateWhsCartonGroup("5", "Product");

			var carrierSize1 = Helper.CreateWhsCartonSize("CA1");
			var carrierSize2 = Helper.CreateWhsCartonSize("CA2");
			var cneSize1 = Helper.CreateWhsCartonSize("CNE1");
			var cneSize2 = Helper.CreateWhsCartonSize("CNE2");
			var clientSize1 = Helper.CreateWhsCartonSize("CL1");
			var clientSize2 = Helper.CreateWhsCartonSize("CL2");
			var clientSize3 = Helper.CreateWhsCartonSize("CL3");
			var whsSize = Helper.CreateWhsCartonSize("WH");
			var productSize = Helper.CreateWhsCartonSize("PRO");

			carrierGroup.CartonSizes.Add(carrierSize1);
			carrierGroup.CartonSizes.Add(carrierSize2);
			cneGroup.CartonSizes.Add(cneSize1);
			cneGroup.CartonSizes.Add(cneSize2);
			clientGroup.CartonSizes.Add(clientSize1);
			clientGroup.CartonSizes.Add(clientSize2);
			clientGroup.CartonSizes.Add(clientSize3);
			whsGroup.CartonSizes.Add(whsSize);
			productGroup.CartonSizes.Add(productSize);

			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;
			consignee.MiscServ.OM_WCG_CartonGroup = cneGroup.PK;
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var productMock = new Mock<ICartonisation>();
			var whsMock = new Mock<ICartonisation>();
			var cneMock = new Mock<ICartonisation>();
			var clientMock = new Mock<ICartonisation>();
			var carrierMock = new Mock<ICartonisation>();
			ExpectCartonsPassedInToCartoniseItems(productMock, productGroup.CartonGroupSizeLinks);
			ExpectCartonsPassedInToCartoniseItems(whsMock, whsGroup.CartonGroupSizeLinks);
			ExpectCartonsPassedInToCartoniseItems(cneMock, cneGroup.CartonGroupSizeLinks);
			ExpectCartonsPassedInToCartoniseItems(clientMock, clientGroup.CartonGroupSizeLinks);
			ExpectCartonsPassedInToCartoniseItems(carrierMock, carrierGroup.CartonGroupSizeLinks);

			var pick = Helper.CreatePickNew(order);
			SetCartonisationMockAndRunAssertions(whsMock, pick);

			data.Org1.MiscServ.OM_WCG_CartonGroup = clientGroup.PK;
			SetCartonisationMockAndRunAssertions(clientMock, pick);

			order.ConsigneeNameOrPK = consignee.PK.ToString();
			SetCartonisationMockAndRunAssertions(cneMock, pick);

			order.TransportCoNameOrPK = carrier.PK.ToString();
			SetCartonisationMockAndRunAssertions(carrierMock, pick);
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup = productGroup.PK;
			SetCartonisationMockAndRunAssertions(productMock, pick);
		}

		#endregion

		#region TestCartoniseSplitCases_CartonGroups_WorkWithPriority

		public void TestCartoniseSplitCases_CartonGroups_WorkWithPriority()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var carrier = Helper.CreateClient("CAR");
			var consignee = Helper.CreateClient("CNE");

			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var cneGroup = Helper.CreateWhsCartonGroup("2", "Consignee");
			var clientGroup = Helper.CreateWhsCartonGroup("3", "WhsClient");
			var whsGroup = Helper.CreateWhsCartonGroup("4", "WhsOrg");
			var productGroup = Helper.CreateWhsCartonGroup("5", "Product");

			var carrierSize1 = Helper.CreateWhsCartonSize("CA1");
			var carrierSize2 = Helper.CreateWhsCartonSize("CA2");
			var cneSize1 = Helper.CreateWhsCartonSize("CNE1");
			var cneSize2 = Helper.CreateWhsCartonSize("CNE2");
			var clientSize1 = Helper.CreateWhsCartonSize("CL1");
			var clientSize2 = Helper.CreateWhsCartonSize("CL2");
			var clientSize3 = Helper.CreateWhsCartonSize("CL3");
			var whsSize = Helper.CreateWhsCartonSize("WH");
			var productSize = Helper.CreateWhsCartonSize("PRO");

			carrierGroup.CartonSizes.Add(carrierSize1);
			carrierGroup.CartonSizes.Add(carrierSize2);
			cneGroup.CartonSizes.Add(cneSize1);
			cneGroup.CartonSizes.Add(cneSize2);
			clientGroup.CartonSizes.Add(clientSize1);
			clientGroup.CartonSizes.Add(clientSize2);
			clientGroup.CartonSizes.Add(clientSize3);
			whsGroup.CartonSizes.Add(whsSize);
			productGroup.CartonSizes.Add(productSize);

			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;
			consignee.MiscServ.OM_WCG_CartonGroup = cneGroup.PK;
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var productMock = new Mock<ICartonisation>();
			var whsMock = new Mock<ICartonisation>();
			var cneMock = new Mock<ICartonisation>();
			var clientMock = new Mock<ICartonisation>();
			var carrierMock = new Mock<ICartonisation>();

			// Setup all CartonGroup sources
			var pick = Helper.CreatePickNew(order);
			data.Org1.MiscServ.OM_WCG_CartonGroup = clientGroup.PK;
			order.ConsigneeNameOrPK = consignee.PK.ToString();
			order.TransportCoNameOrPK = carrier.PK.ToString();
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup = productGroup.PK;

			// Client has the highest Priority
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 5,
					Carrier = 3,
					Consignee = 2,
					Client = 1,
					Warehouse = 4
				}))
			{
				ExpectCartonsPassedInToCartoniseItems(clientMock, clientGroup.CartonGroupSizeLinks);
				SetCartonisationMockAndRunAssertions(clientMock, pick);
			}

			// Carrier has the highest Priority
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 5,
					Carrier = 1,
					Consignee = 2,
					Client = 3,
					Warehouse = 4
				}))
			{
				ExpectCartonsPassedInToCartoniseItems(carrierMock, carrierGroup.CartonGroupSizeLinks);
				SetCartonisationMockAndRunAssertions(carrierMock, pick);
			}

			// Warehouse has the highest Priority
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 5,
					Carrier = 4,
					Consignee = 2,
					Client = 3,
					Warehouse = 1
				}))
			{
				ExpectCartonsPassedInToCartoniseItems(whsMock, whsGroup.CartonGroupSizeLinks);
				SetCartonisationMockAndRunAssertions(whsMock, pick);
			}

			// Consignee has the highest Priority
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 2,
					Carrier = 3,
					Consignee = 1,
					Client = 5,
					Warehouse = 4
				}))
			{
				ExpectCartonsPassedInToCartoniseItems(cneMock, cneGroup.CartonGroupSizeLinks);
				SetCartonisationMockAndRunAssertions(cneMock, pick);
			}

			// Product has the highest Priority
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 1,
					Carrier = 3,
					Consignee = 2,
					Client = 5,
					Warehouse = 4
				}))
			{
				ExpectCartonsPassedInToCartoniseItems(productMock, productGroup.CartonGroupSizeLinks);
				SetCartonisationMockAndRunAssertions(productMock, pick);
			}
		}

		#endregion

		#region TestCartoniseSplitCases_CartonGroups_WorkWithFallback

		public void TestCartoniseSplitCases_CartonGroups_WorkWithFallback()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var carrier = Helper.CreateClient("CAR");
			var consignee = Helper.CreateClient("CNE");

			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var cneGroup = Helper.CreateWhsCartonGroup("2", "Consignee");
			var clientGroup = Helper.CreateWhsCartonGroup("3", "WhsClient");
			var productGroup = Helper.CreateWhsCartonGroup("4", "Product");

			var carrierSize1 = Helper.CreateWhsCartonSize("CA1");
			var carrierSize2 = Helper.CreateWhsCartonSize("CA2");
			var cneSize1 = Helper.CreateWhsCartonSize("CNE1");
			var cneSize2 = Helper.CreateWhsCartonSize("CNE2");
			var clientSize1 = Helper.CreateWhsCartonSize("CL1");
			var clientSize2 = Helper.CreateWhsCartonSize("CL2");
			var clientSize3 = Helper.CreateWhsCartonSize("CL3");
			var productSize = Helper.CreateWhsCartonSize("PRO");

			carrierGroup.CartonSizes.Add(carrierSize1);
			carrierGroup.CartonSizes.Add(carrierSize2);
			cneGroup.CartonSizes.Add(cneSize1);
			cneGroup.CartonSizes.Add(cneSize2);
			clientGroup.CartonSizes.Add(clientSize1);
			clientGroup.CartonSizes.Add(clientSize2);
			clientGroup.CartonSizes.Add(clientSize3);
			productGroup.CartonSizes.Add(productSize);

			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;
			consignee.MiscServ.OM_WCG_CartonGroup = cneGroup.PK;

			var carrierMock = new Mock<ICartonisation>();

			// Setup all CartonGroup sources
			var pick = Helper.CreatePickNew(order);
			data.Org1.MiscServ.OM_WCG_CartonGroup = clientGroup.PK;
			order.ConsigneeNameOrPK = consignee.PK.ToString();
			order.TransportCoNameOrPK = carrier.PK.ToString();
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup = productGroup.PK;

			// Client(unused), Consignee(unused), priority: Warehouse(no CartonGroup) -> *Carrier* -> Product
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 3,
					Carrier = 2,
					Consignee = 0,
					Client = 0,
					Warehouse = 1
				}))
			{
				ExpectCartonsPassedInToCartoniseItems(carrierMock, carrierGroup.CartonGroupSizeLinks);
				SetCartonisationMockAndRunAssertions(carrierMock, pick);
			}
		}

		#endregion

		#region TestCartoniseSplitCases_CartonGroups_OverridenOnPart

		public void TestCartoniseSplitCases_CartonGroups_OverridenOnPart()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;
			data.Part2.OP_StockKeepingUnit = refType1.F3_Code;

			var client2 = Helper.CreateClient("2");
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(client2.PK, OrgPartRelation.RelationshipTypes.Owner);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part2, 5m);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "3", data.Part1, 5m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);

			var group1 = Helper.CreateWhsCartonGroup("1", "1");
			var group2 = Helper.CreateWhsCartonGroup("2", "2");
			var size1 = Helper.CreateWhsCartonSize("1");
			var size2 = Helper.CreateWhsCartonSize("2");
			group1.CartonSizes.Add(size1);
			group2.CartonSizes.Add(size2);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = group1.PK;
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup = group2.PK;

			var sizesPassedIn = new List<WhsCartonSize>();

			var mock = new Mock<ICartonisation>();
			mock
				.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
					It.IsAny<IEnumerable<ICartonDefinition>>()))
				.Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>((arg1, arg2) =>
				{
					sizesPassedIn.Add(((WhsCartonGroupSizeLink)arg2.Single()).CartonSize); // Should have passed in a single carton size on both passes.
					return Enumerable.Empty<ICartonWithItems>();
				});

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				allocatePackageLabelsStrategy.CartoniseSplitCases(pick);
				AssertEquals("Should have saved.", false, pick.HasChanges);
				mock.Verify();
			}

			AssertContainsExactElementsInAnyOrder("Only one of the 3 pick lines (the one for client2 + Part1) has an overriden carton group.", new[] { size1, size1, size2 }, sizesPassedIn);
		}

		#endregion

		#region TestCartoniseSplitCases_PickByUOM_WithConversions

		public void TestCartoniseSplitCases_PickByUOM_WithConversions()
		{
			// PickByUOM Cases:
			//1. Parts SKU is Split Case, conversions to Case. Should avoid, handled by pick by label.
			//2. Parts SKU is Split Case, conversions to Pallet. Should avoid, handled by pick by label.
			//3. Parts SKU is Split Case, conversions to other Split Case. Should still use - but remove the allocated type. Will be handled by Pick By Biggest Pack Type on RF
			//4. Parts SKU is Pallet, conversions to Split Case. Handled by Cartonisation.
			//5. Parts SKU is Case, conversions to Split Case. Handled by Cartonisation.
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var splitCaseType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			var otherSplitCaseType = PackingHelper.CreateRefPackType("2", "2", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			var palletType = PackingHelper.CreateRefPackType("3", "3", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			var caseType = PackingHelper.CreateRefPackType("4", "4", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Case);

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var part5 = Helper.CreateProduct(data.Org1, "P5");

			data.Part1.OP_StockKeepingUnit = splitCaseType.F3_Code;
			data.Part2.OP_StockKeepingUnit = splitCaseType.F3_Code;
			part3.OP_StockKeepingUnit = splitCaseType.F3_Code;
			part4.OP_StockKeepingUnit = palletType.F3_Code;
			part5.OP_StockKeepingUnit = caseType.F3_Code;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			part3.PartUnits.RemoveAndDeleteAll();
			part4.PartUnits.RemoveAndDeleteAll();
			part5.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, caseType.F3_Code, 2m);
			Helper.CreateProductUnit(data.Part2, palletType.F3_Code, 2m);
			Helper.CreateProductUnit(part3, otherSplitCaseType.F3_Code, 2m);
			Helper.CreateProductUnit(part4, splitCaseType.F3_Code, 2m);
			Helper.CreateProductUnit(part5, splitCaseType.F3_Code, 2m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 9m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 9m);
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 9m);
			Helper.CreateWhsReceiveInventoryLine(receive, part4, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, part5, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9m);
			Helper.CreateWhsOrderLine(order, data.Part2, 9m);
			Helper.CreateWhsOrderLine(order, part3, 9m);
			Helper.CreateWhsOrderLine(order, part4, 10m);
			Helper.CreateWhsOrderLine(order, part5, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 8, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 5, pick.GetAllPickLines().Count(pl => pl.AllocatedPackType == splitCaseType));
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count(pl => pl.AllocatedPackType == otherSplitCaseType));
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count(pl => pl.AllocatedPackType == palletType));
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count(pl => pl.AllocatedPackType == caseType));

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH");
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var mock = new Mock<ICartonisation>();
			mock
				.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
					It.IsAny<IEnumerable<ICartonDefinition>>()))
				.Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>((arg1, arg2) =>
				{
					var cartonSizes = arg2;
					var pickLines = arg1.Cast<WhsPickLine>();
					AssertEquals("Precondition", 1, cartonSizes.Count());

					AssertEquals("Precondition", 6, pickLines.Count());
					AssertContainsExactElementsInAnyOrder("Precondition", new[] { splitCaseType, otherSplitCaseType }, pickLines.Select(pl => pl.AllocatedPackType).Distinct());

					var cartonisationResult = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
					foreach (var pickLine in pickLines)
					{
						cartonisationResult.Items.Add(new CartonisationItem(pickLine.PK, (int)pickLine.WZ_Units));
					}

					return new[] { cartonisationResult };
				});

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should succeed as there were no fatal errors.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Should have saved.", false, pick.HasChanges);
				mock.Verify();
			}

			AssertEquals("Order should still be attached to pick", true, pick.Orders.Contains(order));
			AssertEquals("Should still have all 8 Pick Lines on order.", 8, pick.GetAllPickLines().Count());
			AssertEquals("Should not have touched the Pallet or Case Lines.", 1, pick.GetAllPickLines().Count(pl => pl.Product.Parent == part4));
			AssertEquals("Should not have touched the Pallet or Case Lines.", 1, pick.GetAllPickLines().Count(pl => pl.Product.Parent == part5));

			AssertEquals("Should have removed the allocated type with Other (not SKU) Split Case Type.", false, pick.GetAllPickLines().Any(pl => pl.WZ_F3_NKAllocatedPackType == otherSplitCaseType.F3_Code));
			AssertEquals("Should have refreshed UOM Grid.", false,
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.SelectMany(orderedInv => orderedInv.AvailableInventories).Cast<WhsPickAvailableInventory>()
				.Any(availInv => availInv.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitByUOM>()
					.Any(uom => uom.PackQuantityUQ == otherSplitCaseType.F3_Code)));

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { splitCaseType, palletType, caseType }, pick.GetAllPickLines().Select(pl => pl.AllocatedPackType).Distinct());

			AssertEquals("Shouldn't have added Errors.", false, ((NotificationBuffer)pick.NotificationSubscriber).Events.Any(e => e.Message == "No Order(s) could be Cartonized. Check the Events on the Pick for more details."));
		}
		#endregion

		#region TestCartoniseSplitCases_FactorySaveFails

		public void TestCartoniseSplitCases_FactorySaveFails()
		{
			// If no orders can be cartonised, we shouldn't remove all orders and lock down the pick etc. unneccesarily
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInOtherFactory = factory2.Load<WhsPick>(pick.PK);
			pickInOtherFactory.PickPriority = 2;
			factory2.Save();

			pick.PickPriority = 3; // force concurrency error.
			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertNoExceptionThrown(() => allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
		}

		#endregion

		public void TestCartoniseSplitCases_SerialNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType = PackingHelper.CreateRefPackType("2", "2", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (int i = 0; i < 10; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN" + i, "");
			}
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			AssertEquals("Precondition", 10, pick.GetAllPickLines().Count());

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH");
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var mock = new Mock<ICartonisation>();

			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
					It.IsAny<IEnumerable<ICartonDefinition>>()))
				.Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>((arg1, arg2) =>
				{
					var pickLines = arg1.Cast<WhsPickLine>();
					var cartonSizes = arg2;
					AssertEquals("Precondition", 10, pickLines.Count());
					AssertEquals("Precondition", 1, cartonSizes.Count());

					var cartonisationResult = new CartonisationAlgorithmResult(cartonSizes.Single().PK);
					foreach (var pickLine in pickLines)
					{
						cartonisationResult.Items.Add(new CartonisationItem(pickLine.PK, 1));
					}
					return new[] { cartonisationResult };
				});

			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Should have saved.", false, pick.HasChanges);
				mock.VerifyAll();
			}

			var packageJob = order.PackageJob;
			AssertEquals("Should have created package for Order1.", 1, packageJob.Packages.Count);
			var package = packageJob.Packages[0];
			AssertEquals("Should have generated IDs for Order1.", true, package.KP_PackageID.StartsWith(string.Format("{0}-00", packageJob.ParentJob.JobNo)));
			AssertCreatedPackages(whsSize, whsGroup, Constants.PkgUnit.Carton, packageJob.Packages);
			AssertContainsExactElementsInAnyOrder("Each pick line is assigned to its own package", pick.GetAllPickLines().Select(pl => pl.PK), package.PackedItemDivots.Select(p => p.KI_ParentID));
		}

		public void TestCartoniseSplitCases_OrderWithNoPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var cartonSize = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			whsGroup.CartonSizes.Add(cartonSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Grams, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			data.Part1.OP_CubicUQ = Constants.Volume.CubicCentimeters;
			data.Part1.OP_Depth = 0.5m;
			data.Part1.OP_Width = 0.5m;
			data.Part1.OP_Height = 0.5m;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_Weight = 0.25m;
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var poke = pickInNewFactory.Orders.Cast<WhsOrder>().SelectMany(o => o.Lines).Cast<WhsOrderLine>().SelectMany(l => l.PickLines).ToArray();

			pick.Orders.Remove(order);
			Factory.Save();
			AssertNull("Precondition: Order has no package job.", order.PackageJob);

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			var isCartonised = CartonisationResult.Error;
			AssertNoExceptionThrown(() => isCartonised = allocatePackageLabelsStrategy.CartoniseSplitCases(pickInNewFactory));
			AssertEquals("Should have dome nothing.", CartonisationResult.NothingToCartonise, isCartonised);
			AssertNoExceptionThrown(() => newFactory.Save());
			AssertEquals("Should have saved.", false, pickInNewFactory.HasChanges);
		}

		public void TestCartoniseSplitCases_MultipleOrders_OrderWithNoPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var cartonSize = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			whsGroup.CartonSizes.Add(cartonSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Grams, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			data.Part1.OP_CubicUQ = Constants.Volume.CubicCentimeters;
			data.Part1.OP_Depth = 0.5m;
			data.Part1.OP_Width = 0.5m;
			data.Part1.OP_Height = 0.5m;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_Weight = 0.25m;
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var poke = pickInNewFactory.Orders.Cast<WhsOrder>().SelectMany(o => o.Lines).Cast<WhsOrderLine>().SelectMany(l => l.PickLines).ToArray();

			pick.Orders.Remove(order2);
			Factory.Save();
			AssertNull("Precondition: Order2 has no package job.", order2.PackageJob);

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			var isCartonised = CartonisationResult.NothingToCartonise;
			AssertNoExceptionThrown(() => isCartonised = allocatePackageLabelsStrategy.CartoniseSplitCases(pickInNewFactory));
			AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, isCartonised);
			AssertNoExceptionThrown(() => newFactory.Save());
			AssertEquals("Should have saved.", false, pickInNewFactory.HasChanges);

			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var packageJob = order1InNewFactory.PackageJob;
			AssertEquals("Should have created package.", 1, packageJob.Packages.Count);
			AssertEquals("Created packages only from the valid order.", 1, pick.OuterPackages.Count);
		}

		#region TestCartoniseSplitCases_NotDefaultPackTypeOfCartonSize

		public void TestCartoniseSplitCases_NotDefaultPackTypeOfCartonSize()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var cartonSize = Helper.CreateWhsCartonSize("LRG", 15, 15, 15, 15, 50, 500, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams);
			cartonSize.WCS_F3_NKPackType = Constants.PkgUnit.Box;
			whsGroup.CartonSizes.Add(cartonSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Grams, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			// Set up product so 15 units will fit in 2x Small cartons or 1x Medium Carton
			data.Part1.OP_CubicUQ = Constants.Volume.CubicCentimeters;
			data.Part1.OP_Depth = 0.5m;
			data.Part1.OP_Width = 0.5m;
			data.Part1.OP_Height = 0.5m;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_Weight = 1m; // This is the important field. 15 * 10 > (20 - 10)
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			var packageJob = order.PackageJob;
			AssertEquals("Should have created package.", 1, packageJob.Packages.Count);

			var package = packageJob.Packages[0];
			AssertEquals(package.KP_F3_NKPackType, Constants.PkgUnit.Box);
			AssertCreatedPackages(cartonSize, whsGroup, cartonSize.WCS_F3_NKPackType, packageJob.Packages);
		}

		#endregion

		#endregion

		#region Pick By Label

		#region TestPickByLabel

		public void TestPickCasesByLabel()
		{
			TestPickByLabel(UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick), SSCCOptions.NoWarehousePrefix);
		}

		public void TestPickCasesByLabel_SSCC()
		{
			TestPickByLabel(UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick), SSCCOptions.WarehousePrefixWithOptionTicked);
		}

		public void TestPickCasesByLabel_SSCC_NoWarehousePrefixFallback()
		{
			TestPickByLabel(UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick), SSCCOptions.WarehousePrefixWithOptionUnticked);
		}

		public void TestPickPalletsByLabel()
		{
			TestPickByLabel(UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick), SSCCOptions.NoWarehousePrefix);
		}

		public void TestPickPalletsByLabel_SSCC()
		{
			TestPickByLabel(UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick), SSCCOptions.WarehousePrefixWithOptionTicked);
		}

		public void TestPickPalletsByLabel_SSCC_NoWarehousePrefixFallback()
		{
			TestPickByLabel(UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick), SSCCOptions.WarehousePrefixWithOptionUnticked);
		}

		enum SSCCOptions
		{
			NoWarehousePrefix,
			WarehousePrefixWithOptionUnticked,
			WarehousePrefixWithOptionTicked,
		}

		void TestPickByLabel(string uomType, string otherUomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel, SSCCOptions ssccOptions)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			var refType2 = PackingHelper.CreateRefPackType("2", "2", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Kilograms, otherUomType);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;
			data.Part2.OP_StockKeepingUnit = refType2.F3_Code;

			if (ssccOptions != SSCCOptions.NoWarehousePrefix)
			{
				data.Whs1.WarehouseAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1111111");

				if (ssccOptions == SSCCOptions.WarehousePrefixWithOptionTicked)
				{
					data.Whs1.WW_UseGS1PrefixFallback = true;
				}
				else
				{
					AssertEquals("Precondition - make sure we supress the message.", false, data.Whs1.WW_UseGS1PrefixFallback);
				}
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			Helper.CreateWhsOrderLine(order, data.Part2, 8m);

			var pick = Helper.CreatePickNew(order);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should *not* have succeeded.", false, pickXByLabel(allocatePackageLabelsStrategy, pick));

			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition", 2, pick.OrderedInventories.Count);

			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);
			var part2OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part2.PK);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", 1, part2OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories[0].PickLines.Count());
			AssertEquals("Precondition", 1, part2OrderedInventory.AvailableInventories[0].PickLines.Count());

			var pickLine = part1OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0);
			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			pickLine.WZ_GS_NKAssignedTo = pickerAAA.GS_Code;

			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertEquals("Did not touch Part 2 lines.", 1, part2OrderedInventory.AvailableInventories.Count);
			AssertEquals("Did not touch Part 2 lines.", 1, part2OrderedInventory.AvailableInventories[0].PickLines.Count());
			AssertEquals("Did not touch Part 2 lines.", 8m, part2OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0).WZ_Units);

			AssertEquals(1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Should have split the pick lines for Part 1.", 5, part1OrderedInventory.AvailableInventories[0].PickLines.Count());
			AssertEquals("Should have cleared Picked By", true, pick.GetAllPickLines().All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty));

			var packageJob = ((WhsOrder)pick.Orders[0]).PackageJob;
			AssertEquals("Should have created packages for Part1.", 5, packageJob.Packages.Count);

			var packageIDShouldStartWith = ssccOptions == SSCCOptions.WarehousePrefixWithOptionTicked ? "01111111" : string.Format("{0}-00", packageJob.ParentJob.JobNo);
			AssertEquals("Should have generated IDs.", true, packageJob.Packages.All(p => p.KP_PackageID.StartsWith(packageIDShouldStartWith)));
			AssertEquals("Should not show any messages.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertCreatedPackages(refType1, packageJob.Packages, part1OrderedInventory.AvailableInventories[0].PickLines);
		}

		#endregion

		#region TestPickByLabel_MultipleOrders

		public void TestPickCasesByLabel_MultipleOrders()
		{
			TestPickByLabel_MultipleOrders(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_MultipleOrders()
		{
			TestPickByLabel_MultipleOrders(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_MultipleOrders(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);
			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should have succeeded", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			var packageJob1 = ((WhsOrder)pick.Orders[0]).PackageJob;
			var packageJob2 = ((WhsOrder)pick.Orders[1]).PackageJob;
			AssertEquals("Should have created Packages.", 5, packageJob1.Packages.Count);
			AssertEquals("Should have created Packages.", 5, packageJob2.Packages.Count);

			AssertCreatedPackages(refType1, order1.PackageJob.Packages);
			AssertCreatedPackages(refType1, order2.PackageJob.Packages);
		}

		#endregion

		#region TestPickByLabel_DoesntTryToRepackPickLines

		public void TestPickCasesByLabel_DoesntTryToRepackPickLines()
		{
			TestPickByLabel_DoesntTryToRepackPickLines(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_DoesntTryToRepackPickLines()
		{
			TestPickByLabel_DoesntTryToRepackPickLines(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_DoesntTryToRepackPickLines(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			// Cartonisation may result in split pick lines, so we revert AllocatedPackTypes to the SKU.
			// With a strange setup, this may mean these picklines can be found by PickByLabel, so we must filter out pick lines that are already packed
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());
			var pickLine = pick.GetAllPickLines().Single();

			var package = order.PackageJob.Packages.AddNew();
			var divot = package.PackedItemDivots.AddNew();
			divot.KI_ParentTableCode = WhsPickLineSchema.Constants.Prefix;
			divot.KI_ParentID = pickLine.PK;
			divot.KI_PackedQty = pickLine.WZ_Units;
			AssertEquals("Precondition", true, order.PackageJob.IsPacked(pickLine));

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should *not* have succeeded.", false, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertEquals("Should not have split pick lines", 1, pick.GetAllPickLines().Count());
			AssertEquals("Should be the same pickline from the start.", pickLine, pick.GetAllPickLines().Single());
			AssertEquals("Should not have created new packages.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Should be the same package from the start.", package, order.PackageJob.Packages[0]);
		}

		#endregion

		#region TestPickByLabel_WithPackTypeEqualToStockKeepingUnit

		[Flags]
		enum TestMeasuresToChange
		{
			Height = 1,
			Width = 2,
			Depth = 4,
			Weight = 8
		}

		public void TestPickFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit_All() =>
			TestPickCasesFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Height | TestMeasuresToChange.Depth | TestMeasuresToChange.Width | TestMeasuresToChange.Weight);

		public void TestPickFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit_Height() => TestPickCasesFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Height);

		public void TestPickFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit_Width() => TestPickCasesFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Width);

		public void TestPickFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit_Depth() => TestPickCasesFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Depth);

		public void TestPickFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit_Weight() => TestPickCasesFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Weight);

		void TestPickCasesFromCasesByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange toChange) =>
			TestPickCasesByLabel_WithPackTypeEqualToStockKeepingUnit(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick), toChange);

		public void TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit_All() =>
			TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Height | TestMeasuresToChange.Depth | TestMeasuresToChange.Width | TestMeasuresToChange.Weight);

		public void TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit_Height() => TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Height);

		public void TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit_Width() => TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Width);

		public void TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit_Depth() => TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Depth);

		public void TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit_Weight() => TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange.Weight);

		void TestPickCasesFromPalletByLabel_WithPackTypeEqualToStockKeepingUnit(TestMeasuresToChange toChange) =>
			TestPickCasesByLabel_WithPackTypeEqualToStockKeepingUnit(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick), toChange);

		void TestPickCasesByLabel_WithPackTypeEqualToStockKeepingUnit(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel, TestMeasuresToChange toChange)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType = PackingHelper.CreateRefPackType("1", "1", 0m, 0m, 0m, Constants.Length.Metres, 0m, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_CubicUQ = Constants.Volume.CubicFeet;
			data.Part1.OP_MeasureUQ = Constants.Length.Feet;
			data.Part1.OP_WeightUQ = Constants.Weight.Pounds;
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			var height = toChange.HasFlag(TestMeasuresToChange.Height) ? 5m : 0m;
			var depth = toChange.HasFlag(TestMeasuresToChange.Depth) ? 9m : 0m;
			var width = toChange.HasFlag(TestMeasuresToChange.Width) ? 12m : 0m;
			var weight = toChange.HasFlag(TestMeasuresToChange.Weight) ? 6m : 0m;
			data.Part1.OP_Height = height;
			data.Part1.OP_Depth = depth;
			data.Part1.OP_Width = width;
			data.Part1.OP_Weight = weight;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 1m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition", 1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			var packageJob = order1.PackageJob;

			var packages = order1.PackageJob.Packages;
			AssertEquals("Precondition: Created 1 package.", 1, packages.Count);

			var package = packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Volume UQ must be taken from product.", Constants.Volume.CubicFeet, package.KP_VolumeUQ);
				AssertEquals("Dimension UQ must be taken from product.", Constants.Length.Feet, package.KP_DimensionUQ);
				AssertEquals("Weight UQ must be taken from product.", Constants.Weight.Pounds, package.KP_WeightUQ);
				AssertEquals("Height must be taken from product.", height, package.KP_Height);
				AssertEquals("Depth (Length) must be taken from product.", depth, package.KP_Length);
				AssertEquals("Width must be taken from product.", width, package.KP_Width);
				AssertEquals("Weight must be taken from	product.", weight, package.KP_Weight);
				AssertEquals("If Weight must is taken from product, Tare Weight should be 0.", 0m, package.KP_TareWeight);
				AssertEquals("Volume must be taken from	product.", height * width * depth, package.KP_Volume);
			});
		}

		#endregion

		#region TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoWeightUnit

		public void TestPickCasesByLabel_WithPackTypeEqualToStockKeepingUnit_NoWeightUnit()
		{
			TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoWeightUnit(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_WithPackTypeEqualToStockKeepingUnit_NoWeightUnit()
		{
			TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoWeightUnit(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoWeightUnit(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			data.Part1.OP_CubicUQ = Constants.Volume.CubicDecimetres;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_WeightUQ = "";
			data.Part1.OP_Cubic = 2m;
			data.Part1.OP_Height = 6m;
			data.Part1.OP_Width = 7m;
			data.Part1.OP_Depth = 8m;
			data.Part1.OP_Weight = 11m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should *not* have succeeded.", false, pickXByLabel(allocatePackageLabelsStrategy, pick));

			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);

			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories[0].PickLines.Count());

			var pickLine = part1OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0);
			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			pickLine.WZ_GS_NKAssignedTo = pickerAAA.GS_Code;

			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertEquals(1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Should have cleared Picked By", true, pick.GetAllPickLines().All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty));

			var packageJob = ((WhsOrder)pick.Orders[0]).PackageJob;
			AssertEquals("Should have created a package.", 1, packageJob.Packages.Count);
			AssertCreatedPackages(refType1, packageJob.Packages, part1OrderedInventory.AvailableInventories[0].PickLines);

			var package = packageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Valid volume UQ should be set.", Constants.Volume.CubicDecimetres, package.KP_VolumeUQ);
				AssertEquals("Valid dimension UQ should be set.", Constants.Length.Centimetres, package.KP_DimensionUQ);
				AssertEquals("Weight UQ should remain as the RefPackType default.", Constants.Weight.Kilograms, package.KP_WeightUQ);
				AssertEquals("Should have set Volume.", 2m, package.KP_Volume);
				AssertEquals("Should have set Height.", 6m, package.KP_Height);
				AssertEquals("Should have set Width.", 7m, package.KP_Width);
				AssertEquals("Should have set Length.", 8m, package.KP_Length);
				AssertEquals("Should *not* have set Weight.", 0m, package.KP_Weight);
				AssertEquals("Should *not* have Tare Weight.", 0m, package.KP_TareWeight);
			});
		}

		#endregion

		#region TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoVolumeUnit

		public void TestPickCasesByLabel_WithPackTypeEqualToStockKeepingUnit_NoVolumeUnit()
		{
			TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoVolumeUnit(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_WithPackTypeEqualToStockKeepingUnit_NoVolumeUnit()
		{
			TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoVolumeUnit(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoVolumeUnit(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			data.Part1.OP_CubicUQ = "";
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;
			data.Part1.OP_Cubic = 2m;
			data.Part1.OP_Height = 6m;
			data.Part1.OP_Width = 7m;
			data.Part1.OP_Depth = 8m;
			data.Part1.OP_Weight = 11m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should *not* have succeeded.", false, pickXByLabel(allocatePackageLabelsStrategy, pick));

			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);

			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories[0].PickLines.Count());

			var pickLine = part1OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0);
			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			pickLine.WZ_GS_NKAssignedTo = pickerAAA.GS_Code;

			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertEquals(1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Should have cleared Picked By", true, pick.GetAllPickLines().All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty));

			var packageJob = ((WhsOrder)pick.Orders[0]).PackageJob;
			AssertEquals("Should have created a package.", 1, packageJob.Packages.Count);
			AssertCreatedPackages(refType1, packageJob.Packages, part1OrderedInventory.AvailableInventories[0].PickLines);

			var package = packageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Volume UQ should remain as the default.", Constants.Volume.CubicMetres, package.KP_VolumeUQ);
				AssertEquals("Valid dimension UQ should be set.", Constants.Length.Centimetres, package.KP_DimensionUQ);
				AssertEquals("Weight UQ should be set.", Constants.Weight.Grams, package.KP_WeightUQ);
				AssertEquals("Should *not* have set Volume.", 0m, package.KP_Volume);
				AssertEquals("Should have set Height.", 6m, package.KP_Height);
				AssertEquals("Should have set Width.", 7m, package.KP_Width);
				AssertEquals("Should have set Length.", 8m, package.KP_Length);
				AssertEquals("Should have set Weight.", 11m, package.KP_Weight);
				AssertEquals("Should have no Tare Weight.", 0m, package.KP_TareWeight);
			});
		}

		#endregion

		#region TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoMeasureUnit

		public void TestPickCasesByLabel_WithPackTypeEqualToStockKeepingUnit_NoMeasureUnit()
		{
			TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoMeasureUnit(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_WithPackTypeEqualToStockKeepingUnit_NoMeasureUnit()
		{
			TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoMeasureUnit(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_NoMeasureUnit(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			data.Part1.OP_CubicUQ = Constants.Volume.CubicDecimetres;
			data.Part1.OP_MeasureUQ = "";
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;
			data.Part1.OP_Cubic = 2m;
			data.Part1.OP_Height = 6m;
			data.Part1.OP_Width = 7m;
			data.Part1.OP_Depth = 8m;
			data.Part1.OP_Weight = 11m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should *not* have succeeded.", false, pickXByLabel(allocatePackageLabelsStrategy, pick));

			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);

			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories[0].PickLines.Count());

			var pickLine = part1OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0);
			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			pickLine.WZ_GS_NKAssignedTo = pickerAAA.GS_Code;

			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertEquals(1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Should have cleared Picked By", true, pick.GetAllPickLines().All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty));

			var packageJob = ((WhsOrder)pick.Orders[0]).PackageJob;
			AssertEquals("Should have created a package.", 1, packageJob.Packages.Count);
			AssertCreatedPackages(refType1, packageJob.Packages, part1OrderedInventory.AvailableInventories[0].PickLines);

			var package = packageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Valid Volume UQ should be set.", Constants.Volume.CubicDecimetres, package.KP_VolumeUQ);
				AssertEquals("Dimension UQ should be set to the default.", Constants.Length.Metres, package.KP_DimensionUQ);
				AssertEquals("Valid Weight UQ should be set.", Constants.Weight.Grams, package.KP_WeightUQ);
				AssertEquals("Should have set Volume.", 2m, package.KP_Volume);
				AssertEquals("Should *not* have set Height.", 0m, package.KP_Height);
				AssertEquals("Should *not* have set Width.", 0m, package.KP_Width);
				AssertEquals("Should *not* have set Length.", 0m, package.KP_Length);
				AssertEquals("Should have set Weight.", 11m, package.KP_Weight);
				AssertEquals("Should have no Tare Weight.", 0m, package.KP_TareWeight);
			});
		}

		#endregion

		#region TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_WithOneToOneUnitConversion

		public void TestPickCasesByLabel_WithPackTypeEqualToStockKeepingUnit_WithOneToOneUnitConversion()
		{
			TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_WithOneToOneUnitConversion(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_WithPackTypeEqualToStockKeepingUnit_WithOneToOneUnitConversion()
		{
			TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_WithOneToOneUnitConversion(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_WithPackTypeEqualToStockKeepingUnit_WithOneToOneUnitConversion(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			data.Part1.OP_CubicUQ = Constants.Volume.CubicDecimetres;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;
			data.Part1.OP_Cubic = 2m;
			data.Part1.OP_Height = 6m;
			data.Part1.OP_Width = 7m;
			data.Part1.OP_Depth = 8m;
			data.Part1.OP_Weight = 11m;

			// creating the 1 to 1 Unit Conversion should be ignored as validation forces its weight to be 0 or the same weight as the product.
			// this is just to ensure that the code will use the Product details and that the unit Conversion & ref pack type are ignored.
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, refType1.F3_Code, 1m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should *not* have succeeded.", false, pickXByLabel(allocatePackageLabelsStrategy, pick));

			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);

			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories[0].PickLines.Count());

			var pickLine = part1OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0);
			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			pickLine.WZ_GS_NKAssignedTo = pickerAAA.GS_Code;

			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertEquals(1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Should have cleared Picked By", true, pick.GetAllPickLines().All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty));

			var packageJob = ((WhsOrder)pick.Orders[0]).PackageJob;
			AssertEquals("Should have created a package.", 1, packageJob.Packages.Count);
			AssertCreatedPackages(refType1, packageJob.Packages, part1OrderedInventory.AvailableInventories[0].PickLines);

			var package = packageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Valid Volume UQ should be set.", Constants.Volume.CubicDecimetres, package.KP_VolumeUQ);
				AssertEquals("Dimension UQ should be set to the default.", Constants.Length.Centimetres, package.KP_DimensionUQ);
				AssertEquals("Valid Weight UQ should be set.", Constants.Weight.Grams, package.KP_WeightUQ);
				AssertEquals("Should have set Volume.", 2m, package.KP_Volume);
				AssertEquals("Should have set Height.", 6m, package.KP_Height);
				AssertEquals("Should have set Width.", 7m, package.KP_Width);
				AssertEquals("Should have set Length.", 8m, package.KP_Length);
				AssertEquals("Should have set Weight.", 11m, package.KP_Weight);
				AssertEquals("Should have no Tare Weight.", 0m, package.KP_TareWeight);
			});
		}

		#endregion

		#region TestPickCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit

		public void TestPickCasesFromCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit()
		{
			TestPickCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickCasesFromPalletByLabel_WithPackTypeNotEqualToStockKeepingUnit()
		{
			TestPickCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType = PackingHelper.CreateRefPackType("1", "1", 0m, 0m, 0m, Constants.Length.Metres, 0m, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_CubicUQ = Constants.Volume.CubicFeet;
			data.Part1.OP_MeasureUQ = Constants.Length.Feet;
			data.Part1.OP_WeightUQ = Constants.Weight.Pounds;
			data.Part1.OP_Height = 5m;
			data.Part1.OP_Depth = 9m;
			data.Part1.OP_Width = 12m;
			data.Part1.OP_Weight = 6m;

			var partUnit1 = Helper.CreateProductUnit(data.Part1, refType.F3_Code, 3m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition", 1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Height should be 0.", 0m, partUnit1.OF_Height);
				AssertEquals("Precondition: Depth should be 0.", 0m, partUnit1.OF_Depth);
				AssertEquals("Precondition: Width should be 0.", 0m, partUnit1.OF_Width);
				AssertEquals("Precondition: Weight should be 0.", 0m, partUnit1.OF_Weight);
				AssertEquals("Precondition: Cubic should be 0.", 0m, partUnit1.OF_Cubic);
			});

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			var packageJob = order1.PackageJob;

			var packages = order1.PackageJob.Packages;
			AssertEquals("Precondition: Created 1 package.", 1, packages.Count);

			var package = packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Volume UQ must be taken from conversion units.", Constants.Volume.CubicFeet, package.KP_VolumeUQ);
				AssertEquals("Dimension UQ must be taken from conversion units.", Constants.Length.Feet, package.KP_DimensionUQ);
				AssertEquals("Weight UQ must be taken from conversion units.", Constants.Weight.Kilograms, package.KP_WeightUQ);
				AssertEquals("Height must be taken from conversion units.", 0m, package.KP_Height);
				AssertEquals("Depth (Length) must be taken from conversion units.", 0m, package.KP_Length);
				AssertEquals("Width must be taken from conversion units.", 0m, package.KP_Width);
				AssertEquals("Weight must be taken from conversion units.", 8.165m, package.KP_Weight);
				AssertEquals("No Weight on the Ref Pack Type.", 0m, package.KP_TareWeight);
				AssertEquals("Volume must be taken from conversion units.", 0m, package.KP_Volume);
			});
		}

		#endregion

		#region TestPickCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit

		public void TestPickCasesFromCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit_AndTareWeightIsSet()
		{
			TestPickCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit_AndTareWeightIsSet(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickCasesFromPalletByLabel_WithPackTypeNotEqualToStockKeepingUnit_AndTareWeightIsSet()
		{
			TestPickCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit_AndTareWeightIsSet(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickCasesByLabel_WithPackTypeNotEqualToStockKeepingUnit_AndTareWeightIsSet(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType = PackingHelper.CreateRefPackType("1", "1", 0m, 0m, 0m, Constants.Length.Metres, 2m, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_CubicUQ = Constants.Volume.CubicFeet;
			data.Part1.OP_MeasureUQ = Constants.Length.Feet;
			data.Part1.OP_WeightUQ = Constants.Weight.Pounds;
			data.Part1.OP_Height = 5m;
			data.Part1.OP_Depth = 9m;
			data.Part1.OP_Width = 12m;
			data.Part1.OP_Weight = 6m;

			var partUnit1 = Helper.CreateProductUnit(data.Part1, refType.F3_Code, 3m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition", 1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Height should be 0.", 0m, partUnit1.OF_Height);
				AssertEquals("Precondition: Depth should be 0.", 0m, partUnit1.OF_Depth);
				AssertEquals("Precondition: Width should be 0.", 0m, partUnit1.OF_Width);
				AssertEquals("Precondition: Weight should be 0.", 0m, partUnit1.OF_Weight);
				AssertEquals("Precondition: Cubic should be 0.", 0m, partUnit1.OF_Cubic);
			});

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			var packageJob = order1.PackageJob;

			var packages = order1.PackageJob.Packages;
			AssertEquals("Precondition: Created 1 package.", 1, packages.Count);

			var package = packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Volume UQ must be taken from conversion units.", Constants.Volume.CubicFeet, package.KP_VolumeUQ);
				AssertEquals("Dimension UQ must be taken from conversion units.", Constants.Length.Feet, package.KP_DimensionUQ);
				AssertEquals("Weight UQ must be taken from conversion units.", Constants.Weight.Kilograms, package.KP_WeightUQ);
				AssertEquals("Height must be taken from conversion units.", 0m, package.KP_Height);
				AssertEquals("Depth (Length) must be taken from conversion units.", 0m, package.KP_Length);
				AssertEquals("Width must be taken from conversion units.", 0m, package.KP_Width);
				AssertEquals("Weight must be taken from conversion units.", 10.165m, package.KP_Weight);
				AssertEquals("Tare Weight was from the Ref Pack Type.", 2m, package.KP_TareWeight);
				AssertEquals("Volume must be taken from conversion units.", 0m, package.KP_Volume);
			});
		}

		#endregion

		#region TestPickByLabel_WithInvalidConversions_NoWeightUnit

		public void TestPickCasesByLabel_WithInvalidConversions_NoWeightUnit()
		{
			TestPickByLabel_WithInvalidConversions_NoWeightUnit(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_WithInvalidConversions_NoWeightUnit()
		{
			TestPickByLabel_WithInvalidConversions_NoWeightUnit(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_WithInvalidConversions_NoWeightUnit(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var partUnit1 = Helper.CreateProductUnit(data.Part1, refType1.F3_Code, 5m);
			data.Part1.OP_CubicUQ = Constants.Volume.CubicDecimetres;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_WeightUQ = "";
			partUnit1.OF_Cubic = 2m;
			partUnit1.OF_Height = 6m;
			partUnit1.OF_Width = 7m;
			partUnit1.OF_Depth = 8m;
			partUnit1.OF_Weight = 11m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should *not* have succeeded.", false, pickXByLabel(allocatePackageLabelsStrategy, pick));

			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);

			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories[0].PickLines.Count());

			var pickLine = part1OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0);
			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			pickLine.WZ_GS_NKAssignedTo = pickerAAA.GS_Code;

			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertEquals(1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Should have cleared Picked By", true, pick.GetAllPickLines().All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty));

			var packageJob = ((WhsOrder)pick.Orders[0]).PackageJob;
			AssertEquals("Should have created a package.", 1, packageJob.Packages.Count);
			AssertCreatedPackages(refType1, packageJob.Packages, part1OrderedInventory.AvailableInventories[0].PickLines);

			var package = packageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Valid volume UQ should be set.", Constants.Volume.CubicDecimetres, package.KP_VolumeUQ);
				AssertEquals("Valid dimension UQ should be set.", Constants.Length.Centimetres, package.KP_DimensionUQ);
				AssertEquals("Weight UQ should remain as the RefPackType default.", Constants.Weight.Kilograms, package.KP_WeightUQ);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Volume);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Height);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Width);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Length);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Weight);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_TareWeight);
			});
		}

		#endregion

		#region TestPickByLabel_WithInvalidConversions_NoVolumeUnit

		public void TestPickCasesByLabel_WithInvalidConversions_NoVolumeUnit()
		{
			TestPickByLabel_WithInvalidConversions_NoVolumeUnit(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_WithInvalidConversions_NoVolumeUnit()
		{
			TestPickByLabel_WithInvalidConversions_NoVolumeUnit(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_WithInvalidConversions_NoVolumeUnit(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var partUnit1 = Helper.CreateProductUnit(data.Part1, refType1.F3_Code, 5m);
			data.Part1.OP_CubicUQ = "";
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;
			partUnit1.OF_Cubic = 2m;
			partUnit1.OF_Height = 6m;
			partUnit1.OF_Width = 7m;
			partUnit1.OF_Depth = 8m;
			partUnit1.OF_Weight = 11m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should *not* have succeeded.", false, pickXByLabel(allocatePackageLabelsStrategy, pick));

			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);

			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories[0].PickLines.Count());

			var pickLine = part1OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0);
			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			pickLine.WZ_GS_NKAssignedTo = pickerAAA.GS_Code;

			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertEquals(1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Should have cleared Picked By", true, pick.GetAllPickLines().All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty));

			var packageJob = ((WhsOrder)pick.Orders[0]).PackageJob;
			AssertEquals("Should have created a package.", 1, packageJob.Packages.Count);
			AssertCreatedPackages(refType1, packageJob.Packages, part1OrderedInventory.AvailableInventories[0].PickLines);

			var package = packageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Volume UQ should remain as the default.", Constants.Volume.CubicMetres, package.KP_VolumeUQ);
				AssertEquals("Valid dimension UQ should be set.", Constants.Length.Centimetres, package.KP_DimensionUQ);
				AssertEquals("Weight UQ doesn't get set when there are invalid conversions, this is existing behaviour.", Constants.Weight.Kilograms, package.KP_WeightUQ);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Volume);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Height);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Width);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Length);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Weight);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_TareWeight);
			});
		}

		#endregion

		#region TestPickByLabel_WithConversions_NoDimensionUnit

		public void TestPickCasesByLabel_WithInvalidConversions_NoDimensionUnit()
		{
			TestPickByLabel_WithInvalidConversions_NoDimensionUnit(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_WithInvalidConversions_NoDimensionUnit()
		{
			TestPickByLabel_WithInvalidConversions_NoDimensionUnit(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_WithInvalidConversions_NoDimensionUnit(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var partUnit1 = Helper.CreateProductUnit(data.Part1, refType1.F3_Code, 5m);
			data.Part1.OP_CubicUQ = Constants.Volume.CubicDecimetres;
			data.Part1.OP_MeasureUQ = "";
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;
			partUnit1.OF_Cubic = 2m;
			partUnit1.OF_Height = 6m;
			partUnit1.OF_Width = 7m;
			partUnit1.OF_Depth = 8m;
			partUnit1.OF_Weight = 11m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			AssertEquals("Should *not* have succeeded.", false, pickXByLabel(allocatePackageLabelsStrategy, pick));

			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);

			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", 1, part1OrderedInventory.AvailableInventories[0].PickLines.Count());

			var pickLine = part1OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0);
			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			pickLine.WZ_GS_NKAssignedTo = pickerAAA.GS_Code;

			AssertEquals("Should have succeeded.", true, pickXByLabel(allocatePackageLabelsStrategy, pick));
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertEquals(1, part1OrderedInventory.AvailableInventories.Count);
			AssertEquals("Should have cleared Picked By", true, pick.GetAllPickLines().All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty));

			var packageJob = ((WhsOrder)pick.Orders[0]).PackageJob;
			AssertEquals("Should have created a package.", 1, packageJob.Packages.Count);
			AssertCreatedPackages(refType1, packageJob.Packages, part1OrderedInventory.AvailableInventories[0].PickLines);

			var package = packageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Valid volume UQ should be set.", Constants.Volume.CubicDecimetres, package.KP_VolumeUQ);
				AssertEquals("Dimension UQ should remain as the RefPackType default.", Constants.Length.Metres, package.KP_DimensionUQ);
				AssertEquals("Weight UQ doesn't get set when there are invalid conversions, this is existing behaviour.", Constants.Weight.Kilograms, package.KP_WeightUQ);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Volume);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Height);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Width);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Length);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_Weight);
				AssertEquals("Invalid conversions will not be applied.", 0m, package.KP_TareWeight);
			});
		}

		#endregion

		#region TestPickByLabel_WithConversions_MultiOrders

		public void TestPickByCase_WithConversions_MultiOrders()
		{
			TestPickByLabel_MultiOrders(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickByPallet_WithConversions_MultiOrders()
		{
			TestPickByLabel_MultiOrders(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_MultiOrders(string uomType, Action<IAllocatePackageLabelsStrategy, WhsPick> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType = PackingHelper.CreateRefPackType("3", "3", 5m, 9m, 12m, Constants.Length.Metres, 6m, Constants.Weight.Hectograms, uomType);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();

			var partUnit1 = Helper.CreateProductUnit(data.Part1, refType.F3_Code, 10m);
			partUnit1.OF_Cubic = 2m;
			data.Part1.OP_CubicUQ = Constants.Volume.CubicDecimetres;
			data.Part1.OP_MeasureUQ = Constants.Length.Centimetres;
			data.Part1.OP_WeightUQ = Constants.Weight.Grams;
			data.Part1.OP_Weight = 13m;
			partUnit1.OF_Height = 6m;
			partUnit1.OF_Width = 7m;
			partUnit1.OF_Depth = 8m;
			partUnit1.OF_Weight = 11m;

			Helper.CreateProductUnit(data.Part2, refType.F3_Code, 10m);
			data.Part2.OP_CubicUQ = Constants.Volume.CubicCentimeters;
			data.Part2.OP_MeasureUQ = Constants.Length.Millimetres;
			data.Part2.OP_WeightUQ = Constants.Weight.Milligrams;
			data.Part2.OP_Weight = 14m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part2, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition", 2, pick.OrderedInventories.Count);
			AssertEquals("Precondition", 1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
			AssertEquals("Precondition", 1, pick.OrderedInventories[1].AvailableInventories[0].PickLines.Count());

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[1].AvailableInventories[0];
			AssertEquals("Precondition", 1, availableInventory1.PickLines.Count());
			AssertEquals("Precondition", 1, availableInventory2.PickLines.Count());
			AssertEquals("Precondition", 10m, availableInventory1.PickLines.ElementAt(0).WZ_Units);
			AssertEquals("Precondition", 10m, availableInventory2.PickLines.ElementAt(0).WZ_Units);

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			pickXByLabel(allocatePackageLabelsStrategy, pick);
			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals("Should have created 1x Package.", 1, order1.PackageJob.Packages.Count);
			AssertEquals("Should have created 1x Package.", 1, order2.PackageJob.Packages.Count);
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertCreatedPackages(refType, order1.PackageJob.Packages, availableInventory1.PickLines);
			AssertCreatedPackages(refType, order2.PackageJob.Packages, availableInventory2.PickLines);
			AssertEquals(10m, order1.PackageJob.Packages.Sum(p => p.PackedItemDivots.Sum(pi => pi.KI_PackedQty)));
			AssertEquals(10m, order2.PackageJob.Packages.Sum(p => p.PackedItemDivots.Sum(pi => pi.KI_PackedQty)));
			AssertContainsExactElementsInAnyOrder(availableInventory1.PickLines, order1.PackageJob.Packages.SelectMany(p => p.PackedItemDivots).Select(p => p.PackedItem));
			AssertContainsExactElementsInAnyOrder(availableInventory2.PickLines, order2.PackageJob.Packages.SelectMany(p => p.PackedItemDivots).Select(p => p.PackedItem));

			foreach (var packageForOrder1 in order1.PackageJob.Packages)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Volume UQ must be taken from unit conversions", Constants.Volume.CubicDecimetres, packageForOrder1.KP_VolumeUQ);
					AssertEquals("Dimension UQ must be taken from unit conversions", Constants.Length.Centimetres, packageForOrder1.KP_DimensionUQ);
					AssertEquals("Weight UQ must be taken from unit conversions", Constants.Weight.Grams, packageForOrder1.KP_WeightUQ);
					AssertEquals("Volume must be taken from unit conversions", 2m, packageForOrder1.KP_Volume);
					AssertEquals("Height must be taken from unit conversions", 6m, packageForOrder1.KP_Height);
					AssertEquals("Width must be taken from unit conversions", 7m, packageForOrder1.KP_Width);
					AssertEquals("Depth (Length) must be taken from unit conversions", 8m, packageForOrder1.KP_Length);
					AssertEquals("Weight must be taken from unit conversions", 11m, packageForOrder1.KP_Weight);
					AssertEquals("Tare Weight must be 0.", 0m, packageForOrder1.KP_TareWeight);
				});
			}

			foreach (var packageForOrder2 in order2.PackageJob.Packages)
			{
				CombineAssertions(() =>
				{
					// we have a unit conversion but without any measures
					AssertEquals("Volume UQ must be taken from product definition", data.Part2.OP_CubicUQ, packageForOrder2.KP_VolumeUQ);
					AssertEquals("Dimension UQ must be taken from product definition", data.Part2.OP_MeasureUQ, packageForOrder2.KP_DimensionUQ);
					AssertEquals("Weight UQ must be taken from ref pack type, as unit conversion has no weight defined", Constants.Weight.Hectograms, packageForOrder2.KP_WeightUQ);
					AssertEquals("Volume must be zero as there are no unit conversion.", 0m, packageForOrder2.KP_Volume);
					AssertEquals("Height must be zero as there are no unit conversion.", 0m, packageForOrder2.KP_Height);
					AssertEquals("Width must be zero as there are no unit conversion.", 0m, packageForOrder2.KP_Width);
					AssertEquals("Depth (Length) must be zero as there are no unit conversion.", 0m, packageForOrder2.KP_Length);
					AssertEquals("Weight must be taken from ref pack type & product, as there are no unit conversion. 6 Hectograms from ref pack type + 14 * 10 Milligrams from product definition", 6.001m, packageForOrder2.KP_Weight);
					AssertEquals("Tare Weight should be same as Pack Type.", 6m, packageForOrder2.KP_TareWeight);
				});
			}

			order1.PackageJob.Packages.DeleteAll();
			AssertEquals("Precondition", 0, order1.PackageJob.Packages.Count);

			var partUnit2 = Helper.CreateProductUnit(data.Part1, refType.F3_Code, refType.F3_Code, 1m);
			AssertEquals("Precondition", true, partUnit2.HasErrors);

			pickXByLabel(allocatePackageLabelsStrategy, pick);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have created 1x Package.", 1, order1.PackageJob.Packages.Count);

			// no measures from unit conversion must be taken as there are errros on PartUnits for refType.F3_Code
			foreach (var packageForOrder1 in order1.PackageJob.Packages)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Volume UQ must be taken from product definition", data.Part1.OP_CubicUQ, packageForOrder1.KP_VolumeUQ);
					AssertEquals("Dimension UQ must be taken from product definition", data.Part1.OP_MeasureUQ, packageForOrder1.KP_DimensionUQ);
					AssertEquals("Weight UQ must be taken from ref pack type, because there are errors in unit conversion.", Constants.Weight.Hectograms, packageForOrder1.KP_WeightUQ);
					AssertEquals("Volume must be zero because there are errors in unit conversion.", 0m, packageForOrder1.KP_Volume);
					AssertEquals("Height must be zero because there are errors in unit conversion.", 0m, packageForOrder1.KP_Height);
					AssertEquals("Width must be zero because there are errors in unit conversion.", 0m, packageForOrder1.KP_Width);
					AssertEquals("Depth (Length) must be zero because there are errors in unit conversion.", 0m, packageForOrder1.KP_Length);
					// important difference with case when there are no unit conversions
					AssertEquals("Weight must be zero because there are errors in unit conversion.", 0m, packageForOrder1.KP_Weight);
					AssertEquals("Tare Weight must be zero because there are errors in unit conversion.", 0m, packageForOrder1.KP_TareWeight);
				});
			}
		}

		#endregion

		#region TestPickByLabel_WithConversions_SplitCaseSKU

		public void TestPickByLabel_WithConversions_SplitCaseSKU()
		{
			// SKU has Split Case type - but will be picked by bigger pack types using PickByLabel. Split into two pick lines by PickByUOM. One has Case UOM, other has Pallet UOM.
			// 14 units to pick, pallet has 5 and case has 2. Require 2x of both types.
			TestPickByLabel_WithConversions(useSplitCase: true);
		}

		public void TestPickByLabel_WithConversions_CaseSKU()
		{
			// SKU has Case type - but PickByUOM results in some Pallet Pick Lines and larger Cases than the SKU.
			// 14 units to pick, pallet has 5 and case has 2. Require 2x of both types.
			TestPickByLabel_WithConversions(useSplitCase: false);
		}

		void TestPickByLabel_WithConversions(bool useSplitCase)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var caseRefType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Case);
			var palletRefType = PackingHelper.CreateRefPackType("2", "2", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			var splitCaseRefType = PackingHelper.CreateRefPackType("3", "3", 5m, 9m, 12m, Constants.Length.Metres, 6, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			var smallerCaseRefType = PackingHelper.CreateRefPackType("4", "4", 5m, 9m, 12m, Constants.Length.Metres, 6, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Case);

			data.Part1.OP_StockKeepingUnit = useSplitCase ? splitCaseRefType.F3_Code : smallerCaseRefType.F3_Code;
			data.Part2.OP_StockKeepingUnit = splitCaseRefType.F3_Code;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, caseRefType.F3_Code, 2m);
			Helper.CreateProductUnit(data.Part1, palletRefType.F3_Code, 5m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part2, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 14m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.OrderedInventories.Count);
			AssertEquals("Precondition", 1, pick.OrderedInventories[0].AvailableInventories.Count);
			AssertEquals("Precondition", 1, pick.OrderedInventories[1].AvailableInventories.Count);

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[1].AvailableInventories[0];
			AssertEquals("Precondition", 2, availableInventory1.PickLines.Count());
			AssertEquals("Precondition", 1, availableInventory2.PickLines.Count());
			AssertEquals("Precondition", true, availableInventory1.PickLines.Any(pl => pl.WZ_Units == 4 && pl.WZ_F3_NKAllocatedPackType == caseRefType.F3_Code));
			AssertEquals("Precondition", true, availableInventory1.PickLines.Any(pl => pl.WZ_Units == 10 && pl.WZ_F3_NKAllocatedPackType == palletRefType.F3_Code));
			AssertEquals("Precondition", splitCaseRefType.F3_Code, availableInventory2.QuantityUQ);
			AssertEquals("Precondition", splitCaseRefType.F3_Code, availableInventory2.PickLines.ElementAt(0).WZ_F3_NKAllocatedPackType);  // because Part2 has no unit conversions

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();

			// Pick Cases
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize1 = Helper.CreateWhsCartonSize("WH", 10, 20, 40, 80, 160, 320, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize1);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			AssertEquals("Should have succeeded.", CartonisationResult.Cartonised, allocatePackageLabelsStrategy.CartoniseSplitCases(pick));
			var packageJob = ((WhsOrder)pick.Orders[0]).PackageJob;
			AssertEquals("Precondition - CartoniseSplitCases should cartonise the one SplitCase line.", 1, packageJob.Packages.Count);
			AssertCreatedPackages(whsSize1, whsGroup, Constants.PkgUnit.Carton, packageJob.Packages);
			var splitCasePackage = packageJob.Packages.Single();

			allocatePackageLabelsStrategy.PickCasesByLabel(pick);
			AssertEquals("Should have created Packages for the 2x Case Lines (Ignored split case + pallet).", 3, packageJob.Packages.Count);
			AssertCreatedPackages(caseRefType, packageJob.Packages.Except(splitCasePackage));

			AssertEquals("Should have split PickLines.", 3, availableInventory1.PickLines.Count());
			AssertEquals(2, availableInventory1.PickLines.Count(pl => pl.WZ_Units == 2 && pl.WZ_F3_NKAllocatedPackType == caseRefType.F3_Code));

			// Pick Pallets
			allocatePackageLabelsStrategy.PickPalletsByLabel(pick);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have created Packages for the 2x Pallet Lines (Ignored split case + case).", 5, packageJob.Packages.Count);
			AssertEquals("Should have saved.", false, pick.HasChanges);

			var casePackages = packageJob.Packages.Where(pkg => pkg.KP_F3_NKPackType == "1").ToArray();
			var palletPackages = packageJob.Packages.Where(pkg => pkg.KP_F3_NKPackType == "2").ToArray();
			AssertEquals(2, casePackages.Length);
			AssertEquals(2, palletPackages.Length);

			AssertEquals("Should have split PickLines.", 4, availableInventory1.PickLines.Count());
			AssertEquals(2, availableInventory1.PickLines.Count(pl => pl.WZ_Units == 2 && pl.WZ_F3_NKAllocatedPackType == caseRefType.F3_Code));
			AssertEquals(2, availableInventory1.PickLines.Count(pl => pl.WZ_Units == 5 && pl.WZ_F3_NKAllocatedPackType == palletRefType.F3_Code));

			AssertCreatedPackages(caseRefType, casePackages, availableInventory1.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == caseRefType.F3_Code));
			AssertCreatedPackages(palletRefType, palletPackages, availableInventory1.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == palletRefType.F3_Code));
		}

		#endregion

		#region TestPickByLabel_WithConversions_SplitCaseSKU_EdgeCases

		public void TestPickCasesByLabel_WithConversions_EdgeCases()
		{
			TestPickByLabel_WithConversions_EdgeCases(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickPalletsByLabel_WithConversions_EdgeCases()
		{
			TestPickByLabel_WithConversions_EdgeCases(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_WithConversions_EdgeCases(string uomType, Action<IAllocatePackageLabelsStrategy, WhsPick> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var refType = PackingHelper.CreateRefPackType("3", "3", 5m, 9m, 12m, Constants.Length.Metres, 6, Constants.Weight.Kilograms, uomType);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, refType.F3_Code, 10m);
			Helper.CreateProductUnit(data.Part2, refType.F3_Code, 10m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part2, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 30m);
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition", 2, pick.OrderedInventories.Count);
			AssertEquals("Precondition", 1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
			AssertEquals("Precondition", 1, pick.OrderedInventories[1].AvailableInventories[0].PickLines.Count());

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[1].AvailableInventories[0];
			AssertEquals("Precondition", 1, availableInventory1.PickLines.Count());
			AssertEquals("Precondition", 1, availableInventory2.PickLines.Count());
			AssertEquals("Precondition", 20m, availableInventory1.PickLines.ElementAt(0).WZ_Units);
			AssertEquals("Precondition", 30m, availableInventory2.PickLines.ElementAt(0).WZ_Units);

			availableInventory1.PickLines.ElementAt(0).Split(8m);
			availableInventory1.PickLines.ElementAt(0).Split(5m);
			availableInventory2.PickLines.ElementAt(0).Split(16m);
			AssertEquals("Precondition", 3, availableInventory1.PickLines.Count()); // 8 + 5 + 7 units
			AssertEquals("Precondition", 2, availableInventory2.PickLines.Count()); // 16 + 14 units

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			pickXByLabel(allocatePackageLabelsStrategy, pick);

			AssertEquals("Should have created 2x Packages.", 2, order1.PackageJob.Packages.Count);
			AssertEquals("Should have created 3x Packages.", 3, order2.PackageJob.Packages.Count);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			AssertCreatedPackages(refType, order1.PackageJob.Packages, availableInventory1.PickLines);
			AssertCreatedPackages(refType, order2.PackageJob.Packages, availableInventory2.PickLines);
			AssertEquals(20m, order1.PackageJob.Packages.Sum(p => p.PackedItemDivots.Sum(pi => pi.KI_PackedQty)));
			AssertEquals(30m, order2.PackageJob.Packages.Sum(p => p.PackedItemDivots.Sum(pi => pi.KI_PackedQty)));
			AssertContainsExactElementsInAnyOrder(availableInventory1.PickLines, order1.PackageJob.Packages.SelectMany(p => p.PackedItemDivots).Select(p => p.PackedItem));
			AssertContainsExactElementsInAnyOrder(availableInventory2.PickLines, order2.PackageJob.Packages.SelectMany(p => p.PackedItemDivots).Select(p => p.PackedItem));
		}

		#endregion

		#region TestPickByLabel_PickByUOM_WithAggregatedLines

		public void TestPickByLabel_PickByUOM_WithAggregatedLines_Case()
		{
			TestPickByLabel_PickByUOM_WithAggregatedLines(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickByLabel_PickByUOM_WithAggregatedLines_Pallet()
		{
			TestPickByLabel_PickByUOM_WithAggregatedLines(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_PickByUOM_WithAggregatedLines(string uomType, Action<AllocatePackageLabelsStrategy, WhsPick> pickByX)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var skuRefType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			var mediumRefType = PackingHelper.CreateRefPackType("2", "2", 4m, 8m, 16m, Constants.Length.Metres, 6, Constants.Weight.Kilograms, uomType);
			var bigRefType = PackingHelper.CreateRefPackType("3", "3", 8m, 16m, 32m, Constants.Length.Metres, 12, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_StockKeepingUnit = skuRefType.F3_Code;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, mediumRefType.F3_Code, 3m);
			Helper.CreateProductUnit(data.Part1, bigRefType.F3_Code, 5m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				while (pickLine.WZ_Units > 1m)
				{
					pickLine.Split(1m);
				}
			}

			AssertEquals("Precondition", 8, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 8m, pick.GetAllPickLines().Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition", true, pick.GetAllPickLines()
				.All(pl => pl.WZ_F3_NKAllocatedPackType == mediumRefType.F3_Code || pl.WZ_F3_NKAllocatedPackType == bigRefType.F3_Code));

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			pickByX(allocatePackageLabelsStrategy, pick);
			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals(2, order.PackageJob.Packages.Count);
			AssertEquals("Should have saved.", false, pick.HasChanges);
			AssertCreatedPackages(mediumRefType, order.PackageJob.Packages.Where(p => p.KP_F3_NKPackType == mediumRefType.F3_Code),
				pick.GetAllPickLines().Where(pl => pl.WZ_F3_NKAllocatedPackType == mediumRefType.F3_Code));

			AssertCreatedPackages(bigRefType, order.PackageJob.Packages.Where(p => p.KP_F3_NKPackType == bigRefType.F3_Code),
				pick.GetAllPickLines().Where(pl => pl.WZ_F3_NKAllocatedPackType == bigRefType.F3_Code));
		}

		#endregion

		#region TestPickByLabel_SerialNeutral

		public void TestPickByLabel_SerialNeutral_Case()
		{
			TestPickByLabel_SerialNeutral(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		public void TestPickByLabel_SerialNeutral_Pallet()
		{
			TestPickByLabel_SerialNeutral(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		void TestPickByLabel_SerialNeutral(string uomType, Action<AllocatePackageLabelsStrategy, WhsPick> pickByX)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var skuRefType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			var mediumRefType = PackingHelper.CreateRefPackType("2", "2", 4m, 8m, 16m, Constants.Length.Metres, 6, Constants.Weight.Kilograms, uomType);
			var bigRefType = PackingHelper.CreateRefPackType("3", "3", 8m, 16m, 32m, Constants.Length.Metres, 12, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_StockKeepingUnit = skuRefType.F3_Code;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, mediumRefType.F3_Code, 3m);
			Helper.CreateProductUnit(data.Part1, bigRefType.F3_Code, 5m);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (int i = 0; i < 10; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN" + i, "");
			}
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition", 8, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 8m, pick.GetAllPickLines().Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition", true, pick.GetAllPickLines()
				.All(pl => pl.WZ_F3_NKAllocatedPackType == mediumRefType.F3_Code || pl.WZ_F3_NKAllocatedPackType == bigRefType.F3_Code));

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			pickByX(allocatePackageLabelsStrategy, pick);
			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals(2, order.PackageJob.Packages.Count);
			AssertEquals("Should have saved.", false, pick.HasChanges);
			AssertCreatedPackages(mediumRefType, order.PackageJob.Packages.Where(p => p.KP_F3_NKPackType == mediumRefType.F3_Code),
				pick.GetAllPickLines().Where(pl => pl.WZ_F3_NKAllocatedPackType == mediumRefType.F3_Code));

			AssertCreatedPackages(bigRefType, order.PackageJob.Packages.Where(p => p.KP_F3_NKPackType == bigRefType.F3_Code),
				pick.GetAllPickLines().Where(pl => pl.WZ_F3_NKAllocatedPackType == bigRefType.F3_Code));
		}

		#endregion

		#region TestPickByLabel_OrderWithNoPackageJob

		public void TestPickByLabel_OrderWithNoPackageJob_PickPalletByLabel()
		{
			TestPickByLabel_OrderWithNoPackageJobCore(UOMPackTypesList.Codes.Pallet, (allocateStategy, pick) => allocateStategy.PickPalletsByLabel(pick));
		}

		public void TestPickByLabel_OrderWithNoPackageJob_PickCasesByLabel()
		{
			TestPickByLabel_OrderWithNoPackageJobCore(UOMPackTypesList.Codes.Case, (allocateStategy, pick) => allocateStategy.PickCasesByLabel(pick));
		}

		void TestPickByLabel_OrderWithNoPackageJobCore(string uomType, Func<IAllocatePackageLabelsStrategy, WhsPick, bool> pickXByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			data.Part1.OP_StockKeepingUnit = refType1.F3_Code;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);
			var part1OrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part1.PK);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var poke = pickInNewFactory.Orders.Cast<WhsOrder>().SelectMany(o => o.Lines).Cast<WhsOrderLine>().SelectMany(l => l.PickLines).ToArray();

			pick.Orders.Remove(order2);
			Factory.Save();
			AssertNull("Precondition: Order2 has no package job.", order2.PackageJob);

			var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
			var isPickByLabelSuccessful = false;
			AssertNoExceptionThrown(() => isPickByLabelSuccessful = pickXByLabel(allocatePackageLabelsStrategy, pickInNewFactory));
			AssertEquals("Should have succeeded", true, isPickByLabelSuccessful);
			AssertNoExceptionThrown(() => newFactory.Save());
			AssertEquals("Should have saved.", false, pick.HasChanges);

			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var packageJob1 = order1InNewFactory.PackageJob;
			AssertEquals("Should have created Packages.", 5, packageJob1.Packages.Count);
			AssertEquals("Created packages only from the valid order.", 5, pickInNewFactory.OuterPackages.Count);
			AssertCreatedPackages(refType1, order1InNewFactory.PackageJob.Packages);
		}

		#endregion

		#endregion

		#region Implementation

		static void AssertPickLineAssignedToPackage(PkgPackageItemDivot divot, WhsPickLine pickLine)
		{
			AssertEquals("ParentTableCode", WhsPickLineSchema.Constants.Prefix, divot.KI_ParentTableCode);
			AssertEquals("PackedQty", pickLine.WZ_Units, divot.KI_PackedQty);
			AssertEquals("ParentID", pickLine.PK, divot.KI_ParentID);
		}

		static void AssertCreatedPackages(RefPackType refType, IEnumerable<PkgPackage> packages, IEnumerable<WhsPickLine> pickLines = null)
		{
			CombineAssertions("Should have defaulted details from RefPackType.",
				() =>
				{
					AssertEquals("PackType", true, packages.All(p => p.KP_F3_NKPackType == refType.F3_Code));
					AssertEquals("ParentType", true, packages.All(p => p.PackedItemDivots.All(pid => pid.KI_ParentTableCode == WhsPickLineSchema.Constants.Prefix)));
					AssertEquals("PackedQty", true, packages.All(p => p.PackedItemDivots.All(pid => pid.KI_PackedQty == ((WhsPickLine)pid.PackedItem).WZ_Units)));

					if (pickLines != null)
					{
						var pickLinePKs = pickLines.Select(pl => pl.PK);
						AssertContainsExactElementsInAnyOrder("ParentID", pickLinePKs, packages.SelectMany(p => p.PackedItemDivots).Select(pid => pid.KI_ParentID));
					}
				});
		}

		static void AssertCreatedPackages(WhsCartonSize cartonSize, WhsCartonGroup cartonGroup, string pkgType, IEnumerable<PkgPackage> packages)
		{
			AssertCreatedPackages(cartonSize, pkgType, packages);
			AssertEquals("Should have populated carton group and size.", true, packages.All(p => p.CartonGroupAndSize == string.Format("{0} - {1}", cartonGroup.WCG_Code, cartonSize.WCS_Code)));
		}

		static void AssertCreatedPackages(IPackageTemplate template, string pkgType, IEnumerable<PkgPackage> packages)
		{
			CombineAssertions("Should have defaulted details from the template.",
				() =>
				{
					AssertEquals("PackType", true, packages.All(p => p.KP_F3_NKPackType == pkgType));
					AssertEquals("Height", true, packages.All(p => p.KP_Height == template.Height));
					AssertEquals("Length", true, packages.All(p => p.KP_Length == template.Length));
					AssertEquals("Width", true, packages.All(p => p.KP_Width == template.Width));

					AssertEquals("Weight", true, packages.All(p => (p.KP_Weight - p.PackedItems.Typed.Sum(pi => pi.PackableItemParent.WeightPerUnit * pi.PackedQty)) == template.TareWeight));
					AssertEquals("TareWeight", true, packages.All(p => p.KP_TareWeight == template.TareWeight));
					AssertEquals("Dimension UQ", true, packages.All(p => p.KP_DimensionUQ == template.DimensionUQ));
					AssertEquals("Weight UQ", true, packages.All(p => p.KP_WeightUQ == template.WeightUQ));
				});
		}

		static void CleanUp(WhsPick pick)
		{
			pick.WP_IsCartonised = true;
			pick.CancelPackageLabelAllocations();
			AssertEquals("Precondition: Package Label Allocations were reversed.", false, pick.WP_IsCartonised);
		}

		static void ExpectCartonsPassedInToCartoniseItems(Mock<ICartonisation> mock, IEnumerable<ICartonDefinition> cartonSizes)
		{
			mock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(),
					It.IsAny<IEnumerable<ICartonDefinition>>()))
				.Returns<IEnumerable<ICartonisableItem>, IEnumerable<ICartonDefinition>>((arg1, arg2) =>
				{
					AssertContainsExactElementsInAnyOrder("Should pass in Cartons.", cartonSizes, arg2);
					return Enumerable.Empty<ICartonWithItems>();
				});
		}

		static void SetCartonisationMockAndRunAssertions(Mock<ICartonisation> mock, WhsPick pick)
		{
			using (ObjectFactory.Substitute(mock.Object))
			{
				var allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy();
				allocatePackageLabelsStrategy.CartoniseSplitCases(pick);
				mock.VerifyAll();
			}
		}

		#region PackingHelper

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#region CartonisationResult

		class CartonisationAlgorithmResult : ICartonWithItems
		{
			public CartonisationAlgorithmResult(ZGuid cartonPK)
			{
				this.cartonPK = cartonPK.ToGuid();
			}
			readonly Guid cartonPK;

			public Guid CartonPK
			{
				get { return cartonPK; }
			}

			public List<IContentResult> Items
			{
				get { return items ?? (items = new List<IContentResult>()); }
			}
			List<IContentResult> items;

			IEnumerable<IContentResult> ICartonWithItems.Items
			{
				get { return items; }
			}
		}

		#endregion

		#region CartonisationItem

		class CartonisationItem : IContentResult
		{
			public CartonisationItem(ZGuid itemPK, int quantity)
			{
				this.itemPK = itemPK.ToGuid();
				this.quantity = quantity;
			}
			readonly Guid itemPK;
			readonly int quantity;

			public Guid CartonisableItemPK
			{
				get { return itemPK; }
			}

			public decimal Quantity
			{
				get { return quantity; }
			}
		}

		#endregion

		#endregion
	}
}
