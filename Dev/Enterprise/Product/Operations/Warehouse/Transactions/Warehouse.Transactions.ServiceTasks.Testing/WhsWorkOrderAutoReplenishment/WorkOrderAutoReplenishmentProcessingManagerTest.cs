using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class WorkOrderAutoReplenishmentProcessingManagerTest : WhsTestCaseWithFactory
	{
		#region TestCreateWorkOrdersForReplenishment_WithoutProductParam

		public void TestCreateWorkOrdersForReplenishment_WithoutProductParam()
		{
			var client = Helper.CreateClient("1", "1");
			var part = CreateBOM(client, "Part");
			var whs = Helper.CreateWarehouse("1");
			Factory.Save();

			var product = WhsProduct.GetWhsProduct(part);
			AssertEquals("Precondition: Product not bind to warehouse.", 0, product.ParamsByWhsAndClient.Count);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(client, whs));
			AssertEquals("Product not bind to warehouse, should not create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			var productParam = Helper.CreateProductParamsByWhsAndClient(part, client, whs, 5m, 50m, 1m, "");
			Factory.Save();

			AssertEquals("Product has Param", 1, product.ParamsByWhsAndClient.Count);

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000001: was created successfully for Client 1, Warehouse 1.
Products and Units For Replenishment: PART : 50.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(client, whs);
			AssertWorkOrder(workOrder.Lines.Single(), client, whs, part, 50m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_NonBOMProduct

		public void TestCreateWorkOrdersForReplenishment_NonBOMProduct()
		{
			var client = Helper.CreateClient("1", "1");
			var part = Helper.CreateProduct(client, "Part");
			var whs = Helper.CreateWarehouse("1");

			part.OP_KitIsAutoReplenished = true;
			Helper.CreateProductParamsByWhsAndClient(part, client, whs, 5m, 50m, 1m, "");
			Factory.Save();

			AssertEquals("Precondition: Non BOM Product.", 0, part.BillOfMaterials.Count);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(client, whs));
			AssertEquals("Non BOM Product, should not create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			var subBOM = Helper.CreateProduct(client, "Sub1");
			Helper.CreateProductBOM(part, subBOM);
			Factory.Save();

			AssertEquals("Product is BOM", true, part.BillOfMaterials.Count > 0);

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000001: was created successfully for Client 1, Warehouse 1.
Products and Units For Replenishment: PART : 50.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(client, whs);
			AssertWorkOrder(workOrder.Lines.Single(), client, whs, part, 50m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_UntickAutoReplenish

		public void TestCreateWorkOrdersForReplenishment_UntickAutoReplenish()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			var productParam = Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 5m, 50m, 1m, "");
			Factory.Save();

			AssertEquals("Precondition: Auto replenish of Product is un-tick", false, bike.OP_KitIsAutoReplenished);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(data.Org1, data.Whs1));
			AssertEquals("Untick auto replenish, should not create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			bike.OP_KitIsAutoReplenished = true;
			Factory.Save();

			AssertEquals("Auto replenish of Product has ticked", true, bike.OP_KitIsAutoReplenished);

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000002: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 50.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrder.Lines.Single(), data.Org1, data.Whs1, bike, 50m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_ReplenishmentMinimumAndEconomicQuantityAreAllZero

		public void TestCreateWorkOrdersForReplenishment_ReplenishmentMinimumAndEconomicQuantityAreAllZero()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			var productParam = Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 0m, 0m);
			Factory.Save();

			AssertEquals("Precondition: Auto replenish ticked.", true, bike.OP_KitIsAutoReplenished);
			AssertEquals("Precondition: Replenishment Minimum should be 0m.", 0m, productParam.W3_ReplenishmentMinimum);
			AssertEquals("Precondition: Economic Quantity should be 0m.", 0m, productParam.W3_EconomicQuantity);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(data.Org1, data.Whs1));
			AssertEquals("When Replenishment Minimum and Economic Quantity = 0m, no need create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			productParam.W3_EconomicQuantity = 50m;
			productParam.W3_ReplenishmentMultiple = 1m;
			Factory.Save();

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000002: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 50.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrder.Lines.Single(), data.Org1, data.Whs1, bike, 50m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_ReplenishmentMinimumIsZeroAndEconomicQuantiyGreaterThanZero

		#region TestCreateWorkOrdersForReplenishment_ReplenishmentMinimumIsZeroAndEconomicQuantiyGreaterThanZero_WithoutStockAndUnfinalisedWorkOrder

		public void TestCreateWorkOrdersForReplenishment_ReplenishmentMinimumIsZeroAndEconomicQuantiyGreaterThanZero_WithoutStockAndUnfinalisedWorkOrder()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			var productParam = Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 0m, 50m, 1m, "");
			Factory.Save();

			AssertEquals("Precondition: Auto replenish ticked.", true, bike.OP_KitIsAutoReplenished);
			AssertEquals("Precondition: Replenishment Minimum should be 0m.", 0m, productParam.W3_ReplenishmentMinimum);
			AssertEquals("Precondition: Economic Minimum should be greater than 0m.", 50m, productParam.W3_EconomicQuantity);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000002: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 50.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrder.Lines.Single(), data.Org1, data.Whs1, bike, 50m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_ReplenishmentMinimumIsZeroAndEconomicQuantiyGreaterThanZero_ExistStock

		public void TestCreateWorkOrdersForReplenishment_ReplenishmentMinimumIsZeroAndEconomicQuantiyGreaterThanZero_ExistStock()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			var productParam = Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 0m, 50m, 1m, "");

			data.CreateProductInInventory("Bike", bike, 10m);

			Factory.Save();

			AssertEquals("Precondition: Auto replenish ticked.", true, bike.OP_KitIsAutoReplenished);
			AssertEquals("Precondition: Replenishment Minimum should be 0m.", 0m, productParam.W3_ReplenishmentMinimum);
			AssertEquals("Precondition: Economic Quantity should be greater than 0m.", 50m, productParam.W3_EconomicQuantity);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(data.Org1, data.Whs1));
			AssertEquals("When Replenishment Minimum = 0 and Economic Quantity > 0m and exist stock, no need create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			// remove bikes from stock
			var inv = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, bike.PK)).Single();
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			var adjustmentLine = adjustment.CreateDocketLineFromInventory(inv);
			adjustmentLine.WE_ReasonCode = AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment;
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);
			AssertEquals("Precondition", 0m, inv.WI_TotalUnits);
			Factory.Save();

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000003: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 50.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrder.Lines.Single(), data.Org1, data.Whs1, bike, 50m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_ReplenishmentMinimumIsZeroAndEconomicQuantiyGreaterThanZero_ExistUnfinalisedWorkOrderWithASSType

		public void TestCreateWorkOrdersForReplenishment_ReplenishmentMinimumIsZeroAndEconomicQuantiyGreaterThanZero_ExistUnfinalisedWorkOrderWithASSType()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			var productParam = Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 0m, 50m, 1m, "");

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, bike, 10m);

			Factory.Save();

			AssertEquals("Precondition: Auto replenish ticked.", true, bike.OP_KitIsAutoReplenished);
			AssertEquals("Precondition: Replenishment Minimum should be 0m.", 0m, productParam.W3_ReplenishmentMinimum);
			AssertEquals("Precondition: Economic Quantity should be greater than 0m.", 50m, productParam.W3_EconomicQuantity);
			AssertEquals("Precondition: Work Order type", WorkOrderType.Codes.Assemble, workOrder.WD_DocketSubType);
			AssertEquals("Precondition: Work Order should not be finalised", false, workOrder.IsFinalised);
			AssertEquals("Precondition: Units on WorkOrder", 10m, workOrderLine.WE_TransactionQuantity);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(data.Org1, data.Whs1, workOrder));
			AssertEquals("When Replenishment Minimum = 0 and Economic Quantity > 0m and exist un-finalised work order with ASS type, no need create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			workOrderLine.WE_TransactionQuantity = 0m;
			Factory.Save();

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000003: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 50.000", Logger.ToString().Trim());

			var newWorkOrder = GetWorkOrder(data.Org1, data.Whs1, workOrder);
			AssertWorkOrder(newWorkOrder.Lines.Single(), data.Org1, data.Whs1, bike, 50m);
		}

		#endregion

		#endregion

		#region TestCreateWorkOrdersForReplenishment_GreaterThanOrEqualReplenishmentMinimum

		#region TestCreateWorkOrdersForReplenishment_GreaterThanOrEqualReplenishmentMinimum_OnlyStock

		public void TestCreateWorkOrdersForReplenishment_GreaterThanOrEqualReplenishmentMinimum_OnlyStock()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			var productParam = Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 5m, 20m, 1m, "");

			data.CreateProductInInventory("Bike", bike, 10m);
			Factory.Save();

			AssertEquals("Precondition: Replenishment Minimum", 5m, productParam.W3_ReplenishmentMinimum);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created.", GetWorkOrder(data.Org1, data.Whs1));
			AssertEquals("When Stock > Replenishment Minimum, no need create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			productParam.W3_ReplenishmentMinimum = 10m;
			Factory.Save();

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(data.Org1, data.Whs1));
			AssertEquals("When Stock == Replenishment Minimum, no need create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			productParam.W3_ReplenishmentMinimum = 15m;
			Factory.Save();

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000002: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 10.000", Logger.ToString().Trim());

			var newWorkOrder = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(newWorkOrder.Lines.Single(), data.Org1, data.Whs1, bike, 10m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_GreaterThanOrEqualReplenishmentMinimum_StockPlusUnfinalisedWorkOrdersWithASSType

		public void TestCreateWorkOrdersForReplenishment_GreaterThanOrEqualReplenishmentMinimum_StockPlusUnfinalisedWorkOrdersWithASSType()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			var productParam = Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 15m, 30m, 1m, "");

			data.CreateProductInInventory("Bike", bike, 10m);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, bike, 10m);

			Factory.Save();

			AssertEquals("Precondition: Replenishment Minimum", 15m, productParam.W3_ReplenishmentMinimum);
			AssertEquals("Precondition: Work Order type", WorkOrderType.Codes.Assemble, workOrder.WD_DocketSubType);
			AssertEquals("Precondition: Work Order should not be finalised", false, workOrder.IsFinalised);
			AssertEquals("Precondition: Units on WorkOrder", 10m, workOrderLine.WE_TransactionQuantity);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created.", GetWorkOrder(data.Org1, data.Whs1, workOrder));
			AssertEquals("When Stock + UnfinalisedWorkOrder(Type = Assemble).Units > Replenishment Minimum, no need create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			productParam.W3_ReplenishmentMinimum = 20m;
			Factory.Save();

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(data.Org1, data.Whs1, workOrder));
			AssertEquals("When Stock + UnfinalisedWorkOrder(Type = Assemble).Units == Replenishment Minimum, no need create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			productParam.W3_ReplenishmentMinimum = 25m;
			Factory.Save();

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000003: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 10.000", Logger.ToString().Trim());

			var newWorkOrder = GetWorkOrder(data.Org1, data.Whs1, workOrder);
			AssertWorkOrder(newWorkOrder.Lines.Single(), data.Org1, data.Whs1, bike, 10m);
		}

		#endregion

		#endregion

		#region TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum

		public void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_NoStock()
		{
			TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimumCore(LessThanReplenishmentMinimumFlag.NoStock);
		}

		public void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_OnlyStock()
		{
			TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimumCore(LessThanReplenishmentMinimumFlag.OnlyStock);
		}

		public void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_StockPlusUnfinalisedWorkOrdersWithASSType()
		{
			TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimumCore(LessThanReplenishmentMinimumFlag.StockPlusUnfinalisedWorkOrderWithASS);
		}

		void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimumCore(LessThanReplenishmentMinimumFlag flag)
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 20m, 30m, 5m, "");

			var expectedQuantity = 0m;
			var expectedWorkOrderID = "W00000002";
			WhsWorkOrder workOrder = null;
			switch (flag)
			{
				case LessThanReplenishmentMinimumFlag.NoStock:
					expectedQuantity = 30.000m;
					break;
				case LessThanReplenishmentMinimumFlag.OnlyStock:
					data.CreateProductInInventory("Bike", bike, 10m);
					expectedQuantity = 20.000m;
					break;
				case LessThanReplenishmentMinimumFlag.StockPlusUnfinalisedWorkOrderWithASS:
					data.CreateProductInInventory("Bike", bike, 10m);
					workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
					Helper.CreateWhsWorkOrderLine(workOrder, bike, 5m);
					expectedQuantity = 15.000m;
					expectedWorkOrderID = "W00000003";
					break;
			}

			Factory.Save();

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals($@"Information|Work Order {expectedWorkOrderID}: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : {expectedQuantity}", Logger.ToString().Trim());

			var newWorkOrder = GetWorkOrder(data.Org1, data.Whs1, workOrder);
			AssertWorkOrder(newWorkOrder.Lines.Single(), data.Org1, data.Whs1, bike, expectedQuantity);
		}

		enum LessThanReplenishmentMinimumFlag
		{
			NoStock,
			OnlyStock,
			StockPlusUnfinalisedWorkOrderWithASS,
		}

		#region TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_ReplenishmentMultipleIsZero

		public void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_ReplenishmentMultipleIsZero()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			var productParam = Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 20m, 30m, "");

			data.CreateProductInInventory("Bike", bike, 11m);

			Factory.Save();

			AssertEquals("Precondition: Replenishment Multiple", 0m, productParam.W3_ReplenishmentMultiple);

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals("ReplenishmentMultiple is 0, should not create work order for replenishment.",
				"Information|Did not find any Products that needed replenishing.", Logger.ToString().Trim());

			productParam.W3_ReplenishmentMultiple = 1m;
			Factory.Save();

			Logger.ClearLog();
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000002: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 19.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrder.Lines.Single(), data.Org1, data.Whs1, bike, 19m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_RoundByReplenishmentMultiple

		public void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_RoundByReplenishmentMultiple()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 5m, 21m, 4m, "");

			Factory.Save();

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000002: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 24.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrder.Lines.Single(), data.Org1, data.Whs1, bike, 24m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_ClientHasMultipleProducts

		public void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_ClientHasMultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bom1 = CreateBOM(data.Org1, "BOM1");
			Helper.CreateProductParamsByWhsAndClient(bom1, data.Org1, data.Whs1, 5m, 30m, 10m, "");

			var bom2 = CreateBOM(data.Org1, "BOM2");
			Helper.CreateProductParamsByWhsAndClient(bom2, data.Org1, data.Whs1, 10m, 50m, 10m, "");

			Factory.Save();

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000001: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: BOM1 : 30.000
Work Order W00000002: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: BOM2 : 50.000", Logger.ToString().Trim());

			var workOrder1 = GetWorkOrder(data.Org1, data.Whs1, null, bom1);
			AssertWorkOrder(workOrder1.Lines.Single(), data.Org1, data.Whs1, bom1, 30m);

			var workOrder2 = GetWorkOrder(data.Org1, data.Whs1, workOrder1);
			AssertWorkOrder(workOrder2.Lines.Single(), data.Org1, data.Whs1, bom2, 50m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_ClientWithDifferentWarehouses

		public void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_ClientWithDifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newWhs = Helper.CreateWarehouse("2");

			var bom1 = CreateBOM(data.Org1, "BOM1");
			Helper.CreateProductParamsByWhsAndClient(bom1, data.Org1, data.Whs1, 5m, 30m, 10m, "");
			Helper.CreateProductParamsByWhsAndClient(bom1, data.Org1, newWhs, 10m, 50m, 10m, "");

			Factory.Save();

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals("Service Task Log", @"Information|Work Order W00000001: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: BOM1 : 30.000
Work Order W00000002: was created successfully for Client 111, Warehouse 2.
Products and Units For Replenishment: BOM1 : 50.000", Logger.ToString().Trim());

			var workOrderForBOM1 = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrderForBOM1.Lines.Single(), data.Org1, data.Whs1, bom1, 30m);

			var workOrderForBOM2 = GetWorkOrder(data.Org1, newWhs);
			AssertWorkOrder(workOrderForBOM2.Lines.Single(), data.Org1, newWhs, bom1, 50m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_MultipleClientsWithSameWarehouse

		public void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_MultipleClientsWithSameWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newClient = Helper.CreateClient("2");

			var bom1 = CreateBOM(data.Org1, "BOM1");
			Helper.CreateProductParamsByWhsAndClient(bom1, data.Org1, data.Whs1, 5m, 30m, 10m, "");

			Helper.CreateProductClientRelationShip(newClient, bom1);
			Helper.CreateProductParamsByWhsAndClient(bom1, newClient, data.Whs1, 10m, 50m, 10m, "");

			Factory.Save();

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals("Service Task Log", @"Information|Work Order W00000001: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: BOM1 : 30.000
Work Order W00000002: was created successfully for Client 2, Warehouse 1.
Products and Units For Replenishment: BOM1 : 50.000", Logger.ToString().Trim());

			var workOrderForBOM1 = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrderForBOM1.Lines.Single(), data.Org1, data.Whs1, bom1, 30m);

			var workOrderForBOM2 = GetWorkOrder(newClient, data.Whs1);
			AssertWorkOrder(workOrderForBOM2.Lines.Single(), newClient, data.Whs1, bom1, 50m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_MultipleClientsWithDifferentWarehouses

		public void TestCreateWorkOrdersForReplenishment_LessThanReplenishmentMinimum_MultipleClientsWithDifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newWhs = Helper.CreateWarehouse("2");
			var newClient = Helper.CreateClient("2");

			var bom1 = CreateBOM(data.Org1, "BOM1");
			Helper.CreateProductParamsByWhsAndClient(bom1, data.Org1, data.Whs1, 5m, 30m, 10m, "");

			Helper.CreateProductClientRelationShip(newClient, bom1);
			Helper.CreateProductParamsByWhsAndClient(bom1, newClient, newWhs, 10m, 50m, 10m, "");

			Factory.Save();

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals("Service Task Log", @"Information|Work Order W00000001: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: BOM1 : 30.000
Work Order W00000002: was created successfully for Client 2, Warehouse 2.
Products and Units For Replenishment: BOM1 : 50.000", Logger.ToString().Trim());

			var workOrderForBOM1 = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrderForBOM1.Lines.Single(), data.Org1, data.Whs1, bom1, 30m);

			var workOrderForBOM2 = GetWorkOrder(newClient, newWhs);
			AssertWorkOrder(workOrderForBOM2.Lines.Single(), newClient, newWhs, bom1, 50m);
		}

		#endregion

		#endregion

		#region TestCreateWorkOrdersForReplenishment_LogWhenCreatedWorkOrderHasErrors

		public void TestCreateWorkOrdersForReplenishment_LogWhenCreatedWorkOrderHasErrors()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 20m, 30m, 5m, "");

			Factory.Save();

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.IsAddErrorForTest = true;
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(data.Org1, data.Whs1));
			AssertEquals(@"Error|Creating a Work Order for Client: 111, Warehouse: 1 failed.
Validation errors: Error - Warehouse Work Order: Test Error1.", Logger.ToString().Trim());

			Logger.ClearLog();
			processingManager.IsAddErrorForTest = false;
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000002: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 30.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrder.Lines.Single(), data.Org1, data.Whs1, bike, 30m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_LogWhenSaveError

		public void TestCreateWorkOrdersForReplenishment_LogWhenSaveError()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var bike = data.BOM.Bike;
			bike.OP_KitIsAutoReplenished = true;
			Helper.CreateProductParamsByWhsAndClient(bike, data.Org1, data.Whs1, 20m, 30m, 5m, "");

			Factory.Save();

			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
			processingManager.IsAddFactorySavingErrorForTest = true;
			processingManager.CreateWorkOrderForReplenishment();

			AssertNull("No WorkOrder created", GetWorkOrder(data.Org1, data.Whs1));
			AssertEquals(@"Error|Creating a Work Order for Client: , Warehouse: 1 failed.
Save errors: Save Error", Logger.ToString().Trim());

			Logger.ClearLog();
			processingManager.IsAddFactorySavingErrorForTest = false;
			processingManager.CreateWorkOrderForReplenishment();

			AssertEquals(@"Information|Work Order W00000003: was created successfully for Client 111, Warehouse 1.
Products and Units For Replenishment: MOTORBIKE : 30.000", Logger.ToString().Trim());

			var workOrder = GetWorkOrder(data.Org1, data.Whs1);
			AssertWorkOrder(workOrder.Lines.Single(), data.Org1, data.Whs1, bike, 30m);
		}

		#endregion

		#region TestCreateWorkOrdersForReplenishment_SetsUserContext

		public void TestCreateWorkOrdersForReplenishment_SetsUserContext()
		{
			var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = Helper.CreateClient("CL1");
			var part1 = CreateBOM(client1, "PROD1");
			var client2 = Helper.CreateClient("CL2");
			var part2 = CreateBOM(client2, "PROD2");
			var client3 = Helper.CreateClient("CL3");
			var part3 = CreateBOM(client3, "PROD3");
			Factory.Save();

			var product1 = WhsProduct.GetWhsProduct(part1);
			Helper.CreateProductParamsByWhsAndClient(part1, client1, warehouse1, 5m, 50m, 1m, "");
			AssertEquals("Product has Param", 1, product1.ParamsByWhsAndClient.Count);

			var product2 = WhsProduct.GetWhsProduct(part2);
			Helper.CreateProductParamsByWhsAndClient(part2, client2, warehouse2, 5m, 50m, 1m, "");
			AssertEquals("Product has Param", 1, product2.ParamsByWhsAndClient.Count);

			var product3 = WhsProduct.GetWhsProduct(part3);
			Helper.CreateProductParamsByWhsAndClient(part3, client3, warehouse1, 5m, 50m, 1m, "");
			AssertEquals("Product has Param", 1, product3.ParamsByWhsAndClient.Count);
			Factory.Save();

			var contextChangeCount = 0;
			var branchContexts = new HashSet<ZGuid>();
			var testBranchCode = Env.CurrentBranch.Code;

			try
			{
				Env.Instance.UserContextChanged += OnContextChanged;
				var processingManager = new WorkOrderAutoReplenishmentProcessingManager(Factory, Logger);
				processingManager.CreateWorkOrderForReplenishment();

				var workOrder1 = GetWorkOrder(client1, warehouse1);
				AssertWorkOrder(workOrder1.Lines.Single(), client1, warehouse1, part1, 50m);

				var workOrder2 = GetWorkOrder(client2, warehouse2);
				AssertWorkOrder(workOrder2.Lines.Single(), client2, warehouse2, part2, 50m);

				var workOrder3 = GetWorkOrder(client3, warehouse1);
				AssertWorkOrder(workOrder3.Lines.Single(), client3, warehouse1, part3, 50m);

				AssertEquals("Changed context twice.", 2, contextChangeCount);
				AssertContainsExactElementsInAnyOrder("Changed to 2 warehouse branch contexts.", new[] { warehouse1.WW_GB_RelatedCompanyBranch, warehouse2.WW_GB_RelatedCompanyBranch }, branchContexts);
			}
			finally
			{
				Env.Instance.UserContextChanged -= OnContextChanged;
			}

			void OnContextChanged(object sender, IUserContextChangingEventArgs e)
			{
				if (e.NewUserContext.Branch.Code != testBranchCode) // do not count if it's just reverting to the previous context on temp context dispose
				{
					contextChangeCount++;
					branchContexts.Add(e.NewUserContext.Branch.PK);
				}
			}
		}

		#endregion

		#region Implementation

		OrgSupplierPart CreateBOM(OrgHeader client, string code)
		{
			var bom = Helper.CreateProduct(client, code);
			bom.OP_KitIsAutoReplenished = true;

			var subBOM = Helper.CreateProduct(client, "Sub" + code);
			Helper.CreateProductBOM(bom, subBOM);

			return bom;
		}

		void AssertWorkOrder(WhsDocketLine workOrderLine, OrgHeader expectedClient, WhsWarehouse expectedWarehouse, OrgSupplierPart expectedProduct, decimal expectedQuantity)
		{
			AssertNotNull(workOrderLine);
			AssertEquals(expectedClient, workOrderLine.Docket.Client);
			AssertEquals(expectedWarehouse, workOrderLine.Docket.Warehouse);
			AssertEquals(expectedProduct, workOrderLine.Product.Parent);
			AssertEquals(expectedQuantity, workOrderLine.WE_TransactionQuantity);
		}

		WhsWorkOrder GetWorkOrder(OrgHeader client, WhsWarehouse warehouse, WhsWorkOrder excludeWorkOrderPK = null, OrgSupplierPart part = null)
		{
			var workOrderQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			workOrderQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, client.PK);
			workOrderQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehouse.PK);
			workOrderQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder);
			workOrderQuery.AddToFilter(WhsDocketSchema.WD_DocketSubType, WorkOrderType.Codes.Assemble);

			if (excludeWorkOrderPK != null)
			{
				workOrderQuery.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, excludeWorkOrderPK.PK);
			}
			if (part != null)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				subQuery.AddToFilter(WhsDocketLineSchema.WE_OP, part.PK);

				workOrderQuery.AddSubQuery(subQuery, JoinCondition.And);
			}

			var workOrders = Factory.Load<WhsWorkOrder>(workOrderQuery);
			AssertEquals(true, workOrders.Length <= 1);

			return workOrders.SingleOrDefault();
		}

		TestServiceLogger Logger
		{
			get { return logger ?? (logger = new TestServiceLogger()); }
		}
		TestServiceLogger logger;

		#endregion
	}
}
