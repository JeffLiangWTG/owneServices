using System.Linq;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderLineCollection))]
	class WhsWorkOrderLineCollectionTest : WhsComponentOrderLineCollectionTest<WhsWorkOrderLineCollection>
	{
		#region TestFilterStrategy

		public void TestFilterStrategy()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create a work order with an empty line
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLineWithDisassemblableBOMProduct = workOrder.Lines.AddNew();

			// assigning a BOM product should auto-create component lines
			workOrderLineWithDisassemblableBOMProduct.WE_TransactionQuantity = 1;
			workOrderLineWithDisassemblableBOMProduct.WE_OP = data.BOM.Bike.PK;

			var orderLineWithNonDisassemblableBOMProduct = workOrder.Lines.AddNew();
			data.BOM.BikeEngine.OP_CanDisassembleKit = false;
			orderLineWithNonDisassemblableBOMProduct.WE_TransactionQuantity = 1;
			orderLineWithNonDisassemblableBOMProduct.WE_OP = data.BOM.BikeEngine.PK;

			var allLines = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.All);
			var disassemblyLinesForPick = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPick);
			var disassemblyLinesForPutaway = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPutaway);
			var assemblyLines = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.AssemblyLinesForPick);
			var assemblyLinesOrExpanded = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.AssemblyLinesOrExpanded);
			var assemblyLinesForReceive = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.AssemblyLinesForReceive);

			// find all elements in the "All" collection
			AssertEquals("Should be 22 work order lines - bike, enginex2, wheels, tyre, rim, blockx2, pistonx2, oilx2, headx2, crankx2, ringx2, polishx4.", 22, allLines.Count);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var engineOilLine = data.BOM.Lines.EngineOil(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			var bikePolishLine = data.BOM.Lines.BikePolish(workOrder);
			var enginePolishLine = data.BOM.Lines.EnginePolish(workOrder);
			var wheelPolishLine = data.BOM.Lines.WheelPolish(workOrder);

			var engineLine = data.BOM.Lines.BikeEngineStandAlone(workOrder);
			var engineBlockLineOnStandAloneEngine = data.BOM.Lines.EngineBlockOnStandAloneEngine(workOrder);
			var enginePistonLineOnStandAloneEngine = data.BOM.Lines.EnginePistonOnStandAloneEngine(workOrder);
			var engineOilLineOnStandAloneEngine = data.BOM.Lines.EngineOilOnStandAloneEngine(workOrder);
			var enginePolishLineOnStandAloneEngine = data.BOM.Lines.EnginePolishOnStandAloneEngine(workOrder);
			var pistonHeadLineOnStandAloneEngine = data.BOM.Lines.PistonHeadOnStandAloneEngine(workOrder);
			var pistonCrankLineOnStandAloneEngine = data.BOM.Lines.PistonCrankOnStandAloneEngine(workOrder);
			var pistonRingLineOnStandAloneEngine = data.BOM.Lines.PistonRingOnStandAloneEngine(workOrder);

			// test each active collection for content
			AssertCollectionContains(allLines, workOrderLineWithDisassemblableBOMProduct, bikeWheelLine, bikeEngineLine, wheelTyreLine, wheelRimLine, engineBlockLine, enginePistonLine, engineOilLine, pistonHeadLine, pistonCrankLine, pistonRingLine, bikePolishLine, enginePolishLine, wheelPolishLine,
				engineLine, engineBlockLineOnStandAloneEngine, engineOilLineOnStandAloneEngine, enginePistonLineOnStandAloneEngine, enginePolishLineOnStandAloneEngine, pistonCrankLineOnStandAloneEngine, pistonHeadLineOnStandAloneEngine, pistonRingLineOnStandAloneEngine);
			AssertCollectionContains(assemblyLines, bikeWheelLine, bikeEngineLine, bikePolishLine, engineBlockLineOnStandAloneEngine, enginePistonLineOnStandAloneEngine, engineOilLineOnStandAloneEngine, enginePolishLineOnStandAloneEngine);
			AssertCollectionContains(disassemblyLinesForPick, workOrderLineWithDisassemblableBOMProduct);
			AssertCollectionContains(disassemblyLinesForPutaway, bikeWheelLine, bikeEngineLine);
			AssertCollectionContains(assemblyLinesForReceive, workOrderLineWithDisassemblableBOMProduct, orderLineWithNonDisassemblableBOMProduct);

			workOrder.BOM.ExpandAllLines();
			AssertCollectionContains(assemblyLinesOrExpanded, workOrderLineWithDisassemblableBOMProduct, bikeWheelLine, bikeEngineLine, wheelTyreLine, wheelRimLine, engineBlockLine, enginePistonLine, engineOilLine, pistonHeadLine, pistonCrankLine, pistonRingLine, bikePolishLine, enginePolishLine, wheelPolishLine,
				engineLine, engineBlockLineOnStandAloneEngine, engineOilLineOnStandAloneEngine, enginePistonLineOnStandAloneEngine, enginePolishLineOnStandAloneEngine, pistonCrankLineOnStandAloneEngine, pistonHeadLineOnStandAloneEngine, pistonRingLineOnStandAloneEngine);

			workOrder.BOM.CollapseAllLines();
			AssertCollectionContains(assemblyLinesOrExpanded, workOrderLineWithDisassemblableBOMProduct, orderLineWithNonDisassemblableBOMProduct);
		}

		#endregion

		#region TestFilterStrategy_WhenProductCannotBeDisassembled

		public void TestFilterStrategy_WhenProductCannotBeDisassembled()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.BOM.BikeEngine.OP_CanDisassembleKit = true;
			data.BOM.BikeWheel.OP_CanDisassembleKit = false;

			// create a work order to disassemble 2 wheels and 1 engine
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var engineLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeEngine, 1m);
			var wheelLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeWheel, 2m);

			// the pick collection should not contain wheels as the Product cannot be disassembled
			var disassemblyLinesForPick = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPick);
			AssertEquals("Should be 1 line - Engine.", 1, disassemblyLinesForPick.Count);
			AssertCollectionContains(disassemblyLinesForPick, engineLine);

			// the putaway collection should not contain wheel components (Rims + Tyres) as the Product cannot be disassembled
			var disassemblyLinesForPutaway = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPutaway);
			AssertEquals("Should be 2 lines - Engine Block + Engine Piston.", 2, disassemblyLinesForPutaway.Count);
			disassemblyLinesForPutaway.Single(line => line.WE_OP == data.BOM.EngineBlock.PK);  //
			disassemblyLinesForPutaway.Single(line => line.WE_OP == data.BOM.EnginePiston.PK); // will throw an exception if exactly 1 result is not found

			var assemblyLinesForReceive = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.AssemblyLinesForReceive);
			AssertEquals("Should be 2 lines - Bike Engine + Bike Wheel.", 2, assemblyLinesForReceive.Count);
		}

		#endregion

		#region TestFilterStrategy_DisassemblyLinesForPutaway_ExcludesNonReusableBOM

		public void TestFilterStrategy_DisassemblyLinesForPutaway_ExcludesNonReusableBOM()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create a work order to disassemble 1 BikeEngine
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeEngine, 1m);

			// the putaway collection should not contain nonReusablePart since the component could not be reused after disassembly.
			var disassemblyLinesForPutaway = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPutaway);
			AssertEquals("Should be 2 lines - EngineBlock + EnginePistons.", 2, disassemblyLinesForPutaway.Count); // EngineOil is not in the list because it can not be reused.
			disassemblyLinesForPutaway.Single(line => line.WE_OP == data.BOM.EngineBlock.PK);
			disassemblyLinesForPutaway.Single(line => line.WE_OP == data.BOM.EnginePiston.PK);
		}

		#endregion

		#region BOA/BOD eDoc Printing Support

		public void TestResetSupplierPartDocManagerInfo()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			WhsWorkOrderLine workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);

			AssertEquals("Work Order has 1 line", 1, workOrder.Lines.Count);
			AssertNotNull("SupplierPartDocManager DocManagerCode", workOrder.Lines[0].SupplierPartDocManagerInfo);
			AssertEquals("SupplierPartDocManager DocManagerCode", "BOA", workOrder.Lines[0].SupplierPartDocManagerInfo.DocManagerCode);

			workOrder.Lines.ResetSupplierPartDocManagerInfo();
			AssertNull("SupplierPartDocManager DocManagerCode", workOrder.Lines[0].supplierPartDocManagerInfo);
			AssertNotNull("SupplierPartDocManager DocManagerCode", workOrder.Lines[0].SupplierPartDocManagerInfo);
			AssertEquals("SupplierPartDocManager DocManagerCode", "BOA", workOrder.Lines[0].SupplierPartDocManagerInfo.DocManagerCode);

			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			AssertEquals("SupplierPartDocManager DocManagerCode", "BOD", workOrder.Lines[0].SupplierPartDocManagerInfo.DocManagerCode);
		}

		#endregion

		#region Implementation

		protected override WhsWorkOrderLineCollection GetCollectionToTest()
		{
			return new WhsWorkOrderLineCollection(Factory.New<WhsWorkOrder>());
		}

		void AssertCollectionContains(WhsWorkOrderLineCollection collection, params WhsWorkOrderLine[] lines)
		{
			AssertContainsExactElementsInAnyOrder(lines, collection);
		}

		#endregion
	}
}
