using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	// WhsDynamicWorkOrder Disassembly tested under WhsDynamicWorkOrderTest
	public class WorkOrderDisassemblyHelperTest : TestCaseWithFactory
	{
		#region TestDisassembleDecomposerHelper_ComponentNotReusable

		public void TestDisassembleDecomposerHelper_ComponentNotReusable_CanUseFrame_CanUseWheel()
		{
			TestDisassembleDecomposerHelper_ComponentNotReusableCore(canUseFrame: true, canUseWheel: true);
		}

		public void TestDisassembleDecomposerHelper_ComponentNotReusable_CanUseFrame_CannotUseWheel()
		{
			TestDisassembleDecomposerHelper_ComponentNotReusableCore(canUseFrame: true, canUseWheel: false);
		}

		public void TestDisassembleDecomposerHelper_ComponentNotReusable_CannotUseFrame_CanUseWheel()
		{
			TestDisassembleDecomposerHelper_ComponentNotReusableCore(canUseFrame: false, canUseWheel: true);
		}

		public void TestDisassembleDecomposerHelper_ComponentNotReusable_CannotUseFrame_CannotUseWheel()
		{
			TestDisassembleDecomposerHelper_ComponentNotReusableCore(canUseFrame: false, canUseWheel: false);
		}

		void TestDisassembleDecomposerHelper_ComponentNotReusableCore(bool canUseFrame, bool canUseWheel)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			var bomFrame = Helper.CreateProductBOM(bomBike, bikeFrame);
			bomFrame.OE_CanReuse = canUseFrame;
			var bomWheel = Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);
			bomWheel.OE_CanReuse = canUseWheel;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bikeWheel, 10m, "BEK-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bikeFrame, 10m, "BEK-2");
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", bomBike, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
			AssertEquals("Precondition: Kit has two links.", 2, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());

			Factory.Save();

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", bomBike, 5m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			disassembleWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(disassembleWorkOrder);

			var disassembleReceive = disassembleWorkOrder.Receive;
			AssertEquals("Should create only if can use.", (canUseFrame ? 1 : 0) + (canUseWheel ? 1 : 0), disassembleReceive.Lines.Count);

			// Test DisassembledInventoryCreator
			AssertExpectedInventory(disassembleReceive, bikeFrame.PK, (canUseFrame ? 5 : 0));
			AssertExpectedInventory(disassembleReceive, bikeWheel.PK, (canUseWheel ? 10 : 0));

			// Test DisassembledInventoryQuantityHelper
			AssertEquals("Should only return part are reusable.", (canUseFrame ? 5m : 0m) + (canUseWheel ? 10m : 0m), disassembleWorkOrder.GetDisassemblyQuantity());
		}

		#endregion

		#region TestDisassembleDecomposerHelper_ComponentPartiallyUseable

		public void TestDisassembleDecomposerHelper_ComponentPartiallyUseable()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var framePack = Helper.CreateProduct(data.Org1, "FramePack");
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			var bomFrame = Helper.CreateProductBOM(bomBike, bikeFrame);
			var bomFramePack = Helper.CreateProductBOM(bomBike, framePack);
			var bomWheel = Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);
			bomFramePack.OE_CanReuse = false;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bikeWheel, 10m, "BEK-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bikeFrame, 10m, "BEK-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", framePack, 10m, "BEK-3");
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", bomBike, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
			AssertEquals("Precondition: Kit has three links.", 3, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());
			Factory.Save();

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", bomBike, 5m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			disassembleWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(disassembleWorkOrder);

			var disassembleReceive = disassembleWorkOrder.Receive;
			AssertEquals("Should create only if can use.", 2, disassembleReceive.Lines.Count);

			// Test DisassembleReceiveCreator
			AssertExpectedInventory(disassembleReceive, bikeFrame.PK, 5);
			AssertExpectedInventory(disassembleReceive, framePack.PK, 0);
			AssertExpectedInventory(disassembleReceive, bikeWheel.PK, 10);

			// Test DisassembleQuantityHelper
			AssertEquals("5 Frame and 0 FramePack (not reuse) and 10 Wheel.", 15m, disassembleWorkOrder.GetDisassemblyQuantity());
		}

		#endregion

		#region TestDisassembleDecomposerHelper_DifferentDisassembleQty

		public void TestDisassembleDecomposerHelper_DifferentDisassembleQty_Less()
		{
			//Assembled 5 Bike
			TestDisassembleDecomposerHelper_DifferentDisassembleQty_Core(disassembleQty: 3, expectedDisQty: 3);
		}

		public void TestDisassembleDecomposerHelper_DifferentDisassembleQty_More()
		{
			//Assembled 5 Bike
			TestDisassembleDecomposerHelper_DifferentDisassembleQty_Core(disassembleQty: 7, expectedDisQty: 5);
		}

		void TestDisassembleDecomposerHelper_DifferentDisassembleQty_Core(decimal disassembleQty, decimal expectedDisQty)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var framePack = Helper.CreateProduct(data.Org1, "FramePack");
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);
			Helper.CreateProductBOM(bomBike, framePack, 3m, bikeWheel.OP_StockKeepingUnit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bikeWheel, 10m, "BEK-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bikeFrame, 10m, "BEK-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", framePack, 20m, "BEK-3");
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", bomBike, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
			AssertEquals("Precondition: Kit has three links.", 3, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());
			Factory.Save();

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", bomBike, disassembleQty);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(disassembleWorkOrder);

			var disassembleReceive = disassembleWorkOrder.Receive;
			AssertEquals("Should create for only all lines.", 3, disassembleReceive.Lines.Count);

			AssertExpectedInventory(disassembleReceive, bikeFrame.PK, expectedDisQty);
			AssertExpectedInventory(disassembleReceive, bikeWheel.PK, expectedDisQty * 2);
			AssertExpectedInventory(disassembleReceive, framePack.PK, expectedDisQty * 3);

			AssertEquals($"{expectedDisQty} Frame and {expectedDisQty * 3} FramePack and {expectedDisQty * 2} Wheel.", expectedDisQty * (1 + 3 + 2), disassembleWorkOrder.GetDisassemblyQuantity());
		}

		#endregion

		#region TestDisassembleDecomposerHelper_DisassembleWhenBOMChangedAfterAssemble

		public void TestDisassembleDecomposerHelper_DisassembleWhenBOMChangedAfterAssemble()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var framePack = Helper.CreateProduct(data.Org1, "FramePack");
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);
			var bomFramePack = Helper.CreateProductBOM(bomBike, framePack, 3m, bikeWheel.OP_StockKeepingUnit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bikeWheel, 20m, "BEK-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bikeFrame, 20m, "BEK-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", framePack, 30m, "BEK-3");
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", bomBike, 7m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
			AssertEquals("Precondition: Kit has three links.", 3, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());
			Factory.Save();

			bomFramePack.OE_ComponentQty = 4m; // should still use the original value 3, even if we change it to 4
			Factory.Save();

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", bomBike, 2m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(disassembleWorkOrder);

			var disassembleReceive = disassembleWorkOrder.Receive;
			AssertEquals("Should create for only all lines.", 3, disassembleReceive.Lines.Count);

			AssertExpectedInventory(disassembleReceive, bikeFrame.PK, 2m);
			AssertExpectedInventory(disassembleReceive, bikeWheel.PK, 2m * 2);
			AssertExpectedInventory(disassembleReceive, framePack.PK, 2m * 3);

			AssertEquals("2 Frame and 6 FramePack and 4 Wheel.", 2m * (1 + 3 + 2), disassembleWorkOrder.GetDisassemblyQuantity());
		}

		public void TestDisassembleDecomposerHelper_DisassemblePartiallyMultipleTimes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var framePack = Helper.CreateProduct(data.Org1, "FramePack");
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);
			var bomFramePack = Helper.CreateProductBOM(bomBike, framePack, 3m, bikeWheel.OP_StockKeepingUnit);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, framePack, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, bikeFrame, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, bikeWheel, AttributeNumber.One, true);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bikeWheel, 2m, finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bikeWheel, 2m, finalise: false);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", bikeFrame, 1m, finalise: false);
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", bikeFrame, 1m, finalise: false);
			var receive5 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", framePack, 3m, finalise: false);
			var receive6 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", framePack, 3m, finalise: false);
			receive1.Lines[0].WE_PartAttrib1 = "PA1";
			receive2.Lines[0].WE_PartAttrib1 = "PA2";
			receive3.Lines[0].WE_PartAttrib1 = "PA3";
			receive4.Lines[0].WE_PartAttrib1 = "PA4";
			receive5.Lines[0].WE_PartAttrib1 = "PA5";
			receive6.Lines[0].WE_PartAttrib1 = "PA6";
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive3.FinaliseDocketWithoutUserConfirmation();
			receive4.FinaliseDocketWithoutUserConfirmation();
			receive5.FinaliseDocketWithoutUserConfirmation();
			receive6.FinaliseDocketWithoutUserConfirmation();
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(receive1);
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(receive2);
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(receive3);
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(receive4);
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(receive5);
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(receive6);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", bomBike, 2m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Should have created 2 Lines.", 2, receive.Lines.Count);
			AssertEquals("Precondition: All Kits have three links.", true, receive.Lines.All(l => l.WE_OP == bomBike.PK && ((WhsReceiveLine)l).BOMComponentLinks.Count() == 3));
			Factory.Save();

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", bomBike, 1m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(disassembleWorkOrder);

			var disassembleReceive = disassembleWorkOrder.Receive;
			AssertEquals("Should create correct amount of lines.", 3, disassembleReceive.Lines.Count);

			var expectedInventory1 = AssertExpectedInventory(disassembleReceive, bikeFrame.PK, 1m);
			var expectedInventory2 = AssertExpectedInventory(disassembleReceive, bikeWheel.PK, 1m * 2);
			var expectedInventory3 = AssertExpectedInventory(disassembleReceive, framePack.PK, 1m * 3);
			AssertEquals("Disassembled Part Attribute should be correct.", true, expectedInventory1.WE_PartAttrib1 == "PA3" || expectedInventory1.WE_PartAttrib1 == "PA4");
			AssertEquals("Disassembled Part Attribute should be correct.", true, expectedInventory2.WE_PartAttrib1 == "PA1" || expectedInventory2.WE_PartAttrib1 == "PA2");
			AssertEquals("Disassembled Part Attribute should be correct.", true, expectedInventory3.WE_PartAttrib1 == "PA5" || expectedInventory3.WE_PartAttrib1 == "PA6");

			AssertEquals("2 Frame and 6 FramePack and 4 Wheel.", (1m + 3m + 2m), disassembleWorkOrder.GetDisassemblyQuantity());
			Factory.Save();

			var nextDisassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef3", bomBike, 1m);
			nextDisassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				nextDisassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			WhsBusinessObjectTestCase.AssertIsFinalisedPrecondition(nextDisassembleWorkOrder);

			var nextDisassembleReceive = nextDisassembleWorkOrder.Receive;
			AssertEquals("Should create correct amount of lines.", 3, nextDisassembleReceive.Lines.Count);

			var expectedInventory4 = AssertExpectedInventory(nextDisassembleReceive, bikeFrame.PK, 1m);
			var expectedInventory5 = AssertExpectedInventory(nextDisassembleReceive, bikeWheel.PK, 1m * 2);
			var expectedInventory6 = AssertExpectedInventory(nextDisassembleReceive, framePack.PK, 1m * 3);
			var expectedPartAttribForInventory4 = expectedInventory1.WE_PartAttrib1 == "PA3" ? "PA4" : "PA3";
			var expectedPartAttribForInventory5 = expectedInventory2.WE_PartAttrib1 == "PA1" ? "PA2" : "PA1";
			var expectedPartAttribForInventory6 = expectedInventory3.WE_PartAttrib1 == "PA5" ? "PA6" : "PA5";
			AssertEquals("Disassembled Part Attribute should be correct.", expectedPartAttribForInventory4, expectedInventory4.WE_PartAttrib1);
			AssertEquals("Disassembled Part Attribute should be correct.", expectedPartAttribForInventory5, expectedInventory5.WE_PartAttrib1);
			AssertEquals("Disassembled Part Attribute should be correct.", expectedPartAttribForInventory6, expectedInventory6.WE_PartAttrib1);

			AssertEquals("2 Frame and 6 FramePack and 4 Wheel.", (1m + 3m + 2m), nextDisassembleWorkOrder.GetDisassemblyQuantity());
		}

		#endregion

		#region Helper 

		WhsDocketLine AssertExpectedInventory(WhsReceive disassembleReceive, ZGuid productPK, ZDecimal expectedQuantity)
		{
			var inventory = disassembleReceive.Lines.SingleOrDefault(l => l.WE_OP == productPK);
			if (expectedQuantity > 0)
			{
				AssertEquals("Disassembled correct part amount.", expectedQuantity, inventory.WE_TransactionQuantity);
			}
			else
			{
				AssertNull("Should not create inventory in disassembly.", inventory);
			}

			return inventory;
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
