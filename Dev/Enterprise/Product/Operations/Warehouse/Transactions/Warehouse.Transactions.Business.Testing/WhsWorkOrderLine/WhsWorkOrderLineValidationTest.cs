using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWorkOrderLineValidationTest : WhsComponentOrderLineValidationTest<WhsWorkOrderLine, WhsWorkOrder>
	{
		#region TestValidateWE_OP

		public void TestValidateWE_OP()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var workOrder = Factory.New<WhsWorkOrder>();
			var line = workOrder.Lines.AddNew();
			AssertNoErrors("Precondition", line.WE_OPInfo);

			line.WE_OP = data.Part1.PK;
			AssertHasErrors(line.WE_OPInfo);

			line.ReadOnly = true;
			line.Validation.ValidateWE_OP();
			AssertHasError("WorkOrderLines that are readonly but not finalised will still show errors.", line.WE_OPInfo, "This product does not have components.");

			using (line.SuspendValidationTesting())
			{
				line.ClearAllNotifications();
			}

			// ensure no errors if the line is finalised
			var pick = Factory.New<WhsPick>();
			workOrder.WD_WP = pick.PK;
			line.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			line.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			line.Validation.ValidateWE_OP();
			AssertNoErrors("WorkOrderLines that are readonly and finalised should never have errors.", line.WE_OPInfo);
		}

		#endregion

		#region TestValidateWE_OP_ForDisassembly

		public void TestValidateWE_OP_ForDisassembly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.BOM.BikeEngine.OP_CanDisassembleKit = true;
			data.BOM.BikeWheel.OP_CanDisassembleKit = false;
			Factory.Save();

			// create a work order for disassembly
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine line = workOrder.Lines.AddNew();
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			line.WE_OP = data.BOM.BikeEngine.PK;
			AssertEquals("Engine can be disassembled - should not be in error.", false, line.WE_OPInfo.HasErrors());

			line.WE_OP = data.BOM.BikeWheel.PK;
			AssertEquals("Wheels cannot be disassembled - should be in error.", true, line.WE_OPInfo.HasErrors());

			// ensure changes to WD_DocketSubType updates the validation of the products
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			AssertEquals("Wheels cannot be disassembled but the WorkOrder type is Assembly - should not be in error.", false, line.WE_OPInfo.HasErrors());

			// ensure no errors if the line is readonly
			line.ReadOnly = true;
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			AssertNoErrors("WorkOrderLines that are readonly should never have errors (they are probably expanded component lines).", line.WE_OPInfo);
		}

		#endregion

		#region TestValidateWE_OP_WhenProductIsMadeBOMAfterBeingAddedToWorkOrder

		public void TestValidateWE_OP_WhenProductIsMadeBOMAfterBeingAddedToWorkOrder()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create a work order to build 1 x Tyre (will fail validation -- not a BOM product)
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.WheelTyre, 1m);
			AssertNoError(line.WE_OPInfo, "No component lines exist. The product may have been recently edited -- try re-adding this line.");

			// make the tyre a BOM product
			Helper.CreateProductBOM(data.BOM.WheelTyre, data.BOM.WheelRim, 1, "UNT");
			Factory.Save();

			// revalidate and ensure we check for child component lines
			line.Validation.ValidateWE_OP();
			AssertNoErrors("Component count validation should only run on ValidateAll().", line.WE_OPInfo);
			line.Validation.ValidateAll();
			AssertHasError(line.WE_OPInfo, "No component lines exist. The product may have been recently edited -- try re-adding this line.");
		}

		#endregion

		#region TestValidateWE_OP_WhenProductWithComponentQuantityZero

		public void TestValidateWE_OP_WhenProductWithComponentQuantityZero()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.WheelTyre, 1m);
			Helper.CreateProductBOM(data.BOM.WheelTyre, data.BOM.WheelRim, 1m, "UNT");
			Factory.Save();

			line.Validation.ValidateWE_OP();
			AssertNoError(line.WE_OPInfo, "This product has a component with an invalid quantity.");

			Helper.CreateProductBOM(data.BOM.WheelTyre, data.BOM.WheelRim, 0m, "UNT");
			Factory.Save();

			line.Validation.ValidateWE_OP();
			AssertHasError(line.WE_OPInfo, "This product has a component with an invalid quantity.");
		}

		public void TestValidateWE_OP_WhenProductWithComponentQuantityZero_VirtualWhs()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.Whs1.WW_IsVirtualWarehouse = true;

			var rim = Helper.CreateProductBOM(data.BOM.WheelTyre, data.BOM.WheelRim, 0m, "UNT");
			rim.OE_ExcludeForVirtualWarehouse = true;

			var workOrder1 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "W1");
			var line1 = Helper.CreateWhsWorkOrderLine(workOrder1, data.BOM.WheelTyre, 1m);
			Factory.Save();

			line1.Validation.ValidateWE_OP();
			AssertNoError(line1.WE_OPInfo, "This product has a component with an invalid quantity.");

			data.Whs1.WW_IsVirtualWarehouse = false;
			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "W2");
			var line2 = Helper.CreateWhsWorkOrderLine(workOrder2, data.BOM.WheelTyre, 1m);
			Factory.Save();

			line2.Validation.ValidateWE_OP();
			AssertHasError(line2.WE_OPInfo, "This product has a component with an invalid quantity.");
		}

		#endregion

		#region TestValidateWE_OP_ComponentWithNoUnitConversion

		public void TestValidateWE_OP_ComponentWithNoUnitConversion()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			data.CreateProductInInventory("20 Wheels", data.BOM.BikeWheel, 20m);
			data.CreateProductInInventory("10 Engines", data.BOM.BikeEngine, 10m);
			data.CreateProductInInventory("10 Polish", data.BOM.Polish, 10m);
			Factory.Save();

			// Create a WorkOrder
			var bikeWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var bikeWorkOrderLine = Helper.CreateWhsWorkOrderLine(bikeWorkOrder, data.BOM.Bike, 10m);
			Factory.Save();

			var expectedError = "Cannot convert component stock keeping unit SHT to UNT. Please add unit conversion for component ENGINE or change stock keeping unit.";

			bikeWorkOrderLine.Validation.ValidateWE_OP();
			AssertEquals("Precondition", false, bikeWorkOrderLine.WE_OPInfo.HasErrors());
			AssertNoError("Precondition", bikeWorkOrderLine.WE_OPInfo, expectedError);

			data.BOM.BikeEngine.OP_StockKeepingUnit = Constants.PkgUnit.Sheet;  // invalid - no unit conversion SHT to UNT 

			bikeWorkOrderLine.Validation.ValidateWE_OP();
			AssertHasError(bikeWorkOrderLine.WE_OPInfo, expectedError);

			data.BOM.BikeWheel.OP_StockKeepingUnit = Constants.PkgUnit.Bundle;  // invalid - no unit conversion BND to UNT 

			expectedError = "Cannot convert component stock keeping unit SHT to UNT. Please add unit conversion for component ENGINE or change stock keeping unit.\nCannot convert component stock keeping unit BND to UNT. Please add unit conversion for component WHEEL or change stock keeping unit.";

			bikeWorkOrderLine.Validation.ValidateWE_OP();
			AssertEquals(expectedError, bikeWorkOrderLine.WE_OPInfo.Notifications.ToMessageListString());
		}

		#endregion

		#region TestValidateWE_OP_AfterPickingAndRemovingBOM

		public void TestValidateWE_OP_AfterPickingAndRemovingBOM()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			// create work order and pick
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 2m);
			Factory.Save();
			var pick = Helper.CreatePickNew(workOrder);
			AssertNotNull("Precondition", pick);
			AssertEquals("Precondition", true, workOrder.IsAttachedToPickButNotFinalised);

			// remove bom
			bomProduct.BillOfMaterials.DeleteAll();
			Factory.Save();

			workOrder.Lines[0].Validation.ValidateWE_OP();
			AssertHasError(workOrder.Lines[0].WE_OPInfo, "This product does not have components.");
		}

		#endregion

		#region TestValidateWE_OP_AfterFinalisedAndRemovingBOM

		public void TestValidateWE_OP_AfterFinalisedAndRemovingBOM()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			// create work order and pick
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 2m);
			Factory.Save();
			var pick = Helper.CreatePickNew(workOrder);
			AssertNotNull("Precondition", pick);
			AssertEquals("Precondition", true, workOrder.IsAttachedToPickButNotFinalised);
			workOrder.FinaliseDocket();
			AssertIsFinalisedPrecondition(workOrder);

			// remove bom
			bomProduct.BillOfMaterials.DeleteAll();
			Factory.Save();

			workOrder.Lines[0].Validation.ValidateWE_OP();

			AssertNoErrors(workOrder.Lines[0].WE_OPInfo);
		}

		#endregion

		#region TestValidateWE_ShortfallQuantityCached

		#region TestValidateWE_ShortfallQuantityCached

		protected override void TestValidateWE_ShortfallQuantityCachedCore()
		{
			// this is a Before Pick test.

			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// work order to build 10 bikes.
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			AssertShortfall(bikeLine, "More components are required to build 10 Motorbike(s).");
			AssertShortfall(bikeWheelLine, "There are not enough Wheel(s) in stock to build the required number of Motorbike(s).");
			AssertShortfall(bikeEngineLine, "There are not enough Engine(s) in stock to build the required number of Motorbike(s).");
			AssertNoWarnings(wheelTyreLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(wheelRimLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(engineBlockLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(enginePistonLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonHeadLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonCrankLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonRingLine.WE_ShortfallQuantityCachedInfo);

			// receive enough stock to fulfill shortfalls
			data.CreateBOMComponentsInInventory();
			Factory.Save();

			foreach (WhsWorkOrderLine line in workOrder.AllLines)
			{
				line.Validation.ValidateWE_ShortfallQuantityCached();
			}

			AssertNoWarnings(bikeLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(bikeWheelLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(bikeEngineLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(wheelTyreLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(wheelRimLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(engineBlockLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(enginePistonLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonHeadLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonCrankLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonRingLine.WE_ShortfallQuantityCachedInfo);
		}

		#endregion

		#region TestValidateWE_ShortfallQuantityCached_ForDisassembly

		public void TestValidateWE_ShortfallQuantityCached_ForDisassembly()
		{
			// this is a Before Pick test.

			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.CreateProductInInventory("Bikes1", data.BOM.Bike, 5m);

			// work order to disassemble 10 bikes.
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			AssertShortfall(bikeLine, "More components are required to disassemble 10 Motorbike(s).");
			AssertNoWarnings(bikeWheelLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(bikeEngineLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(wheelTyreLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(wheelRimLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(engineBlockLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(enginePistonLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonHeadLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonCrankLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonRingLine.WE_ShortfallQuantityCachedInfo);

			// receive enough stock to fulfill shortfalls
			data.CreateProductInInventory("Bikes2", data.BOM.Bike, 5m);
			Factory.Save();

			foreach (WhsWorkOrderLine line in workOrder.AllLines)
			{
				line.Validation.ValidateWE_ShortfallQuantityCached();
			}

			AssertNoWarnings(bikeLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(bikeWheelLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(bikeEngineLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(wheelTyreLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(wheelRimLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(engineBlockLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(enginePistonLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonHeadLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonCrankLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(pistonRingLine.WE_ShortfallQuantityCachedInfo);
		}

		#endregion

		#region TestValidateWE_ShortfallQuantityCached_AfterPickingAndRemovingBOM

		public void TestValidateWE_ShortfallQuantityCached_AfterPickingAndRemovingBOM()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			// create work order and pick
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 2m);
			Factory.Save();
			var pick = Helper.CreatePickNew(workOrder);
			AssertNotNull("Precondition", pick);
			AssertEquals("Precondition", true, workOrder.IsAttachedToPickButNotFinalised);

			// remove bom
			bomProduct.BillOfMaterials.DeleteAll();
			Factory.Save();

			AssertNoExceptionThrown(() => workOrder.Lines[0].Validation.ValidateWE_ShortfallQuantityCached());

			AssertHasWarning(workOrder.Lines[0].WE_ShortfallQuantityCachedInfo, "More components are required to build 2 P1(s).");
		}

		#endregion

		void AssertShortfall(WhsWorkOrderLine line, ZString message)
		{
			line.Validation.ValidateWE_ShortfallQuantityCached();
			AssertHasWarning(line.WE_ShortfallQuantityCachedInfo, message);
		}

		#endregion

		#region TestValidateWE_TransactionQuantity

		public void TestValidateWE_TransactionQuantity_Assembly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.Serial, true);

			// work order to assemble 10 bikes.
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			Factory.Save();
			AssertNoError("WorkOrderLines which qty > 1 and SerialNumber is not null show error.", bikeLine.WE_TransactionQuantityInfo, "Must always be 1 or less for serial number controlled products");

			bikeLine.WE_SerialNumber = "Serial0001";
			Factory.Save();
			AssertHasError("WorkOrderLines which qty > 1 and SerialNumber is not null show error.", bikeLine.WE_TransactionQuantityInfo, "Must always be 1 or less for serial number controlled products");

			bikeLine.WE_TransactionQuantity = 1;
			Factory.Save();
			AssertNoError("WorkOrderLines which qty > 1 and SerialNumber is not null show error.", bikeLine.WE_TransactionQuantityInfo, "Must always be 1 or less for serial number controlled products");
		}

		public void TestValidateWE_TransactionQuantity_Disassembly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", new TestNotificationBuffer());
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.Bike, 1m, data.Whs1.DefaultLocation);
			receiveLine.WI_SerialNumber = "Serial0001";
			receive.FinaliseDocket();
			Factory.Save();

			// work order to disassemble 1 bikes with serial number
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			Factory.Save();
			AssertNoError("WorkOrderLines which qty > 1 and SerialNumber is not null show error.", bikeLine.WE_TransactionQuantityInfo, "Must always be 1 or less for serial number controlled products");

			bikeLine.WE_SerialNumber = "Serial0001";
			Factory.Save();
			AssertHasError("WorkOrderLines which qty > 1 and SerialNumber is not null show error.", bikeLine.WE_TransactionQuantityInfo, "Must always be 1 or less for serial number controlled products");

			bikeLine.WE_TransactionQuantity = 1;
			Factory.Save();
			AssertNoError("WorkOrderLines which qty > 1 and SerialNumber is not null show error.", bikeLine.WE_TransactionQuantityInfo, "Must always be 1 or less for serial number controlled products");
		}

		#endregion

		#region TestValidateWE_SerialNumber_Disassembly

		public void TestValidateWE_SerialNumber_Disassembly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", new TestNotificationBuffer());
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.Bike, 1m, data.Whs1.DefaultLocation);
			receiveLine.WI_SerialNumber = "Serial0001";
			receive.FinaliseDocket();
			Factory.Save();

			// work order to disassemble 1 bikes with serial number
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 1m);
			bikeLine.WE_SerialNumber = "Serial0001";
			Factory.Save();
			AssertNoWarnings(bikeLine.WE_ShortfallQuantityCachedInfo);

			bikeLine.WE_SerialNumber = "Serial0002";
			Factory.Save();
			AssertShortfall(bikeLine, "More components are required to disassemble 1 Motorbike(s).");
		}

		#endregion

		#region TestValidateAll

		protected override ZString ExpectedShortfallWarning1 => "More components are required to build 110 P1(s).";
		protected override ZString ExpectedShortfallWarning2 => "More components are required to build 110 P1(s).";

		#endregion

		#region TestValidateWE_TransactionQuantity

		public void TestValidateWE_TransactionQuantity()
		{
			// create bom product
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);
			Factory.Save();

			// create work order, pick and then change pack type of a component product
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, WorkOrderType.Codes.Assemble);
			var line = workOrder.Lines.AddNew();
			line.WE_OP = bomProduct.PK;

			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			Factory.Save();
			line.WE_TransactionQuantity = 0;
			line.Validation.ValidateAll();
			AssertNoRowError(line, "Component product definition has been changed. Please delete this line and add it again.");

			line.WE_TransactionQuantity = 1;
			line.Validation.ValidateAll();
			AssertHasRowError(line, "Component product definition has been changed. Please delete this line and add it again.");
		}

		#endregion

		#region TestCheckWE_PartAttrib_JulianBatchNumber

		protected override void TestCheckWE_PartAttrib_JulianBatchNumber_AdditionalSetup(TestDataSimpleEnvironment data)
		{
			Helper.CreateProductBOM(data.Part1, data.Part2);
			base.TestCheckWE_PartAttrib_JulianBatchNumber_AdditionalSetup(data);
		}

		protected override bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo)
		{
			return !partAttributeInfo.Value.IsEmpty;
		}

		#endregion

		#region TestValidateWE_OP_SecondaryPart

		public void TestValidateWE_OP_SecondaryPart()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.BOM.BikeEngine.OP_CanDisassembleKit = true;
			data.BOM.BikeWheel.OP_CanDisassembleKit = true;
			Factory.Save();

			// create a work order for disassembly
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var line = workOrder.Lines.AddNew();
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			line.WE_OP = data.BOM.BikeEngine.PK;
			AssertNoErrors("Should be able to add When is not Secondary Product.", line.WE_OPInfo);

			Helper.CreateSecondaryProduct(data.Part1, data.BOM.BikeWheel, 4m);
			line.WE_OP = data.BOM.BikeWheel.PK;
			AssertHasError("Should not be able to add Secondary Product.", line.WE_OPInfo, "Cannot select Secondary Product for disassembly work order.");
		}

		#endregion

		#region Implementation

		protected override FinalisableDocketHelper<WhsWorkOrder> GetNewDocketHelper()
		{
			return new FinalisableWorkOrderHelper(Factory);
		}

		#endregion
	}
}
