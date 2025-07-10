using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class ReleaseLineReductionManagerTest : WhsTestCaseWithFactory
	{
		#region TestReduceStocksForMultipleReleaseLines_Success

		public void TestReduceStocksForMultipleReleaseLines_Success()
		{
			var (order, pick) = CreateOrderWithSingleOrderLine();
			var result = ReleaseLineReductionManager.ReduceStock(
				new WhsReleaseLine[] { order.Lines[0].ReleaseLines[0], order.Lines[0].ReleaseLines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Result of bulk reduce stock should be success", true, result.IsSuccess);
			AssertEquals("ReleaseLines should be empty", 0, order.Lines[0].ReleaseLines.Count);

			var adjustments = order.GetRelatedAdjustmentsAndTransfers();
			AssertEquals("Should have 1 Adjustment or Transfer", 1, adjustments.Count());
			var adjustment = (WhsDocket)adjustments.Single();
			AssertEquals("Should have 2 Docket Lines", 2, adjustment.Lines.Count);
			AssertEquals("First Docket Line has 5 qty for Transfer or -5 for Adjustment",
				ReduceStockReason == ReduceStockReason.Lost ? -5m : 5m,
				adjustment.Lines[0].WE_TransactionQuantity);
			AssertEquals("Second Docket Line has 5 qty for Transfer or -5 for Adjustment",
				ReduceStockReason == ReduceStockReason.Lost ? -5m : 5m,
				adjustment.Lines.Skip(1).First().WE_TransactionQuantity);
		}

		#endregion

		#region TestReduceStocksForMultipleReleaseLines_HasReleaseCapturedAttributes_Success

		public void TestReduceStocksForMultipleReleaseLines_HasReleaseCapturedAttributes_Success()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, mandatoryAttributeType: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, use: true, setReleaseCaptured: true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", "");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", "");
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = order.Lines[0].ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			releaseLine.Quantity = 5;
			var releaseLine2 = order.Lines[0].ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "BLUE";
			releaseLine2.Quantity = 5;

			pick.GetAllPickLines().ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = "E";
				pl.WZ_PickedDateTime = ZDateTimeOffset.Today;
			});
			Factory.Save();

			var result = ReleaseLineReductionManager.ReduceStock(
				new WhsReleaseLine[] { order.Lines[0].ReleaseLines[0], order.Lines[0].ReleaseLines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Result of bulk reduce stock should be success", true, result.IsSuccess);
			AssertEquals("ReleaseLines should be empty", 0, order.Lines[0].ReleaseLines.Count);

			var adjustments = order.GetRelatedAdjustmentsAndTransfers();
			AssertEquals("Should have 1 Adjustment or Transfer", 1, adjustments.Count());
			var adjustment = (WhsDocket)adjustments.Single();
			AssertEquals("Should have 2 Docket Lines", 2, adjustment.Lines.Count);
			AssertEquals("First Docket Line has 5 qty for Transfer or -5 for Adjustment",
				ReduceStockReason == ReduceStockReason.Lost ? -5m : 5m,
				adjustment.Lines[0].WE_TransactionQuantity);
			AssertEquals("Second Docket Line has 5 qty for Transfer or -5 for Adjustment",
				ReduceStockReason == ReduceStockReason.Lost ? -5m : 5m,
				adjustment.Lines.Skip(1).First().WE_TransactionQuantity);
		}

		#endregion

		#region TestReduceStocksForMultipleReleaseLines_ShortCircuitWhen_PickHasChanges

		public void TestReduceStocksForMultipleReleaseLines_ShortCircuitWhen_PickHasChanges()
		{
			var (order, pick) = CreateOrderWithSingleOrderLine();
			pick.WP_PercentageComplete = 8;
			Assert("Precondition:", pick.HasChanges);
			var result = ReleaseLineReductionManager.ReduceStock(
				new WhsReleaseLine[] { order.Lines[0].ReleaseLines[0], order.Lines[0].ReleaseLines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be failed", false, result.IsSuccess);
			AssertEquals($"Save pick before attempting to '{ReduceStockDescription}'.", result.ErrorMessage);
			AssertEquals("ReleaseLines[0] qty should be 5.", 5m, order.Lines[0].ReleaseLines[0].Quantity);
			AssertEquals("ReleaseLines[1] qty should be 5.", 5m, order.Lines[0].ReleaseLines[1].Quantity);
		}

		#endregion

		#region TestReduceStocksForVirtualWarehouse

		public void TestReduceStocksForVirtualWarehouse()
		{
			var (order, pick) = CreateOrderWithSingleOrderLine(isVirtualWarehouse: true);
			var result = ReleaseLineReductionManager.ReduceStock(
				new WhsReleaseLine[] { order.Lines[0].ReleaseLines[0], order.Lines[0].ReleaseLines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Reduce stock should be failed", false, result.IsSuccess);
			AssertEquals($"Cannot '{ReduceStockDescription}' on a Pick in a Virtual Warehouse.", result.ErrorMessage);
			AssertEquals("ReleaseLines[0] qty should be 5.", 5m, order.Lines[0].ReleaseLines[0].Quantity);
			AssertEquals("ReleaseLines[1] qty should be 5.", 5m, order.Lines[0].ReleaseLines[1].Quantity);
		}

		#endregion

		#region TestReduceStocksForMultipleReleaseLines_BOM

		public void TestReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits_FailsWhenNotAllReleaseLinesAreKits()
		{
			var order = CreateOrderWithBOMCanBePickedOnSaleOrder();

			var orderLine = order.Lines.Cast<WhsOrderLine>().Single(l => l.IsBOMProductPickedOnSalesOrder);
			var result = ReleaseLineReductionManager.ReduceStock(orderLine.ReleaseLines.Cast<WhsReleaseLine>(), ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should fail.", false, result.IsSuccess);
			AssertEquals($"Stock must be picked (kits only) or not packed to '{ReduceStockDescription}'.", result.ErrorMessage);
			AssertEquals("ReleaseLines[0] qty should be 3.", 3m, orderLine.ReleaseLines[0].Quantity);
			AssertEquals("ReleaseLines[1] qty should be 3.", 3m, orderLine.ReleaseLines[1].Quantity);
			AssertEquals("ReleaseLines[2] qty should be 4.", 4m, orderLine.ReleaseLines[2].Quantity);
		}

		public void TestReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits_FailsWhenNotAllReleaseLinesAreKits_OrderLines()
		{
			var order = CreateOrderWithBOMCanBePickedOnSaleOrder();

			var orderLine = order.Lines.Cast<WhsOrderLine>().Single(l => l.IsBOMProductPickedOnSalesOrder);
			var result = ReleaseLineReductionManager.ReduceStock(new[] { orderLine }, ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should fail.", false, result.IsSuccess);
			AssertEquals($"Stock must be picked (kits only) or not packed to '{ReduceStockDescription}'.", result.ErrorMessage);
			AssertEquals("ReleaseLines[0] qty should be 3.", 3m, orderLine.ReleaseLines[0].Quantity);
			AssertEquals("ReleaseLines[1] qty should be 3.", 3m, orderLine.ReleaseLines[1].Quantity);
			AssertEquals("ReleaseLines[2] qty should be 4.", 4m, orderLine.ReleaseLines[2].Quantity);
		}

		public void TestReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits_SucceedsWhenSelectedReleaseLinesAreKits()
		{
			var order = CreateOrderWithBOMCanBePickedOnSaleOrder();

			var orderLine = order.Lines.Cast<WhsOrderLine>().Single(l => l.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition: 3 Release Lines.", 3, orderLine.ReleaseLines.Count);

			var kitReleaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(rl => rl.PartAttribute1 == "BLUE");
			var kitReleaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(rl => rl.PartAttribute1 == "RED");
			var result = ReleaseLineReductionManager.ReduceStock(new[] { kitReleaseLine1, kitReleaseLine2 }, ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be succeed.", true, result.IsSuccess);
			AssertEquals("Both Kit Release Lines should have been successfully reduced.", 1, orderLine.ReleaseLines.Count);
			AssertEquals("ReleaseLines[0] qty should be 4.", 4m, orderLine.ReleaseLines[0].Quantity);
			AssertEquals("Only Component Release Line should be left.", "", orderLine.ReleaseLines[0].PartAttribute1);
		}

		public void TestReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits_SucceedsForJustKitQuantity()
		{
			AssertReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits(3m, expectedSuccess: true, expectedErrorMessage: "");
		}

		public void TestReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits_SucceedsForLessThanKitQuantity()
		{
			AssertReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits(2m, expectedSuccess: true, expectedErrorMessage: "");
		}

		public void TestReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits_FailsForMoreThanKitQuantity()
		{
			AssertReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits(4m, expectedSuccess: false,
				expectedErrorMessage: "Quantity (4) cannot be greater than Quantity of Kits Picked (3).");
		}

		void AssertReduceStocksForMultipleReleaseLines_BOM_OnlyConsidersKits(decimal quantityToReduce, bool expectedSuccess, string expectedErrorMessage)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 3m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().Where(pl => !pl.IsPickByBOMKitPickLine()).ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			Factory.Save();
			AssertEquals("Precondition: 1 Release Lines.", 1, orderLine.ReleaseLines.Count);

			var result = ReleaseLineReductionManager.ReduceStock(orderLine.ReleaseLines[0], quantityToReduce, ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals($"Bulk reduce stock should have {(expectedSuccess ? "failed" : "succeeded")}.", expectedSuccess, result.IsSuccess);
			AssertEquals("Bulk reduce stock should have correct Message.", expectedErrorMessage, result.ErrorMessage);
			AssertEquals("Release Lines should remain for the components.", 1, orderLine.ReleaseLines.Count);

			if (expectedSuccess)
			{
				AssertEquals("ReleaseLines Qty should be reduced by Kits reduced.", orderLine.WE_TransactionQuantity - quantityToReduce, orderLine.ReleaseLines[0].Quantity);
			}
			else
			{
				AssertEquals("ReleaseLines Qty should be unchanged.", orderLine.WE_TransactionQuantity, orderLine.ReleaseLines[0].Quantity);
			}
		}

		#endregion

		#region TestReduceStocksForMultipleReleaseLines_ShortCircuitWhen_PickIsFinalised

		public void TestReduceStocksForMultipleReleaseLines_ShortCircuitWhen_PickIsFinalised()
		{
			var (order, pick) = CreateOrderWithSingleOrderLine();
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Assert("Precondition:", pick.IsFinalised);
			var result = ReleaseLineReductionManager.ReduceStock(
				new WhsReleaseLine[] { order.Lines[0].ReleaseLines[0], order.Lines[0].ReleaseLines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be failed", false, result.IsSuccess);
			AssertEquals($"Cannot '{ReduceStockDescription}' on a finalized Pick.", result.ErrorMessage);
			AssertEquals("ReleaseLines[0] qty should be 5.", 5m, order.Lines[0].ReleaseLines[0].Quantity);
			AssertEquals("ReleaseLines[1] qty should be 5.", 5m, order.Lines[0].ReleaseLines[1].Quantity);
		}

		#endregion

		#region TestReduceStocksForMultipleReleaseLines_ShortCircuitWhen_OneReleaseLineAllPacked

		public void TestReduceStocksForMultipleReleaseLines_ShortCircuitWhen_OneReleaseLineAllPacked()
		{
			var (order, pick) = CreateOrderWithSingleOrderLine();
			var package = order.PackageJob.Packages.AddNew("BOX");
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package.Pack(order.Lines[0].ReleaseLines[1], 5m);
			Factory.Save();

			var result = ReleaseLineReductionManager.ReduceStock(
				new WhsReleaseLine[] { order.Lines[0].ReleaseLines[0], order.Lines[0].ReleaseLines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be failed", false, result.IsSuccess);
			AssertEquals($"Stock must be picked or not packed to '{ReduceStockDescription}'.", result.ErrorMessage);
			AssertEquals("ReleaseLines[0] qty should be 5.", 5m, order.Lines[0].ReleaseLines[0].Quantity);
			AssertEquals("ReleaseLines[1] qty should be 5.", 5m, order.Lines[0].ReleaseLines[1].Quantity);
		}

		#endregion

		#region TestReduceStocksForMultipleReleaseLines_DoNotSaveWhenSomeLineHasError

		public void TestReduceStocksForMultipleReleaseLines_DoNotSaveWhenSomeLineHasError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "AAA", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "BBB", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			AssertEquals("There should be 2 Release Lines", 2, orderLine.ReleaseLines.Count);

			var secondReleaseLine = orderLine.ReleaseLines[1];
			Factory.Save();

			orderLine.PickLines.ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = "E";
				pl.WZ_PickedDateTime = ZDateTimeOffset.Today;
			});

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine.PK).PickLines[1];
			using (((IWhsPickLineInternals)pickLineInOtherFactory)
				   .TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				pickLineInOtherFactory.InventoryLine.WE_StockOnHand += pickLineInOtherFactory.WZ_Units;
				pickLineInOtherFactory.Delete();
				otherFactory.Save();
			}

			var result = ReleaseLineReductionManager.ReduceStock(
				new WhsReleaseLine[] { order.Lines[0].ReleaseLines[0], secondReleaseLine },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be failed", false, result.IsSuccess);
			AssertEquals(
				$"Another user has modified the Allocated Stock on this Order.\r\nClose and re-open the form, then attempt to '{ReduceStockDescription}' again.",
				result.ErrorMessage);

			order = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsOrder>(order.PK);
			AssertEquals("There should be 1 Release Line.", 1, order.Lines[0].ReleaseLines.Count);
			AssertEquals("ReleaseLines[0] qty should be 5.", 5m, order.Lines[0].ReleaseLines[0].Quantity);

			var adjustments = order.GetRelatedAdjustmentsAndTransfers();
			AssertEquals("Should not create Adjustment or Transfer", 0, adjustments.Count());
		}

		#endregion

		#region TestReduceStocksForOrderLines_Success

		public void TestReduceStocksForOrderLines_Success()
		{
			var (order, pick) = CreateOrderWithTwoOrderLines();
			var lines = order.Lines;
			var result = ReleaseLineReductionManager.ReduceStock(new WhsOrderLine[] { lines[0], lines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Reduce stock result should be success", true, result.IsSuccess);
			AssertEquals("ReleaseLines should be empty", 0, order.Lines[0].ReleaseLines.Count);
			AssertEquals("ReleaseLines should be empty", 0, order.Lines[1].ReleaseLines.Count);

			var adjustments = order.GetRelatedAdjustmentsAndTransfers();
			AssertEquals("Should have 1 Adjustment or Transfer", 1, adjustments.Count());
			var adjustment = (WhsDocket)adjustments.Single();
			AssertEquals("Should have 4 Docket Lines", 4, adjustment.Lines.Count);
			adjustment.Lines.ForEach(docketLine =>
			{
				AssertEquals("Each Docket Line has 5 qty for Transfer or -5 for Adjustment",
					ReduceStockReason == ReduceStockReason.Lost ? -5m : 5m, docketLine.WE_TransactionQuantity);
			});
		}

		#endregion

		#region TestReduceStocksForOrderLines_ShortCircuitWhen_NoOrderLines

		public void TestReduceStocksForOrderLines_ShortCircuitWhen_NoOrderLines()
		{
			var result = ReleaseLineReductionManager.ReduceStock(Enumerable.Empty<WhsOrderLine>(),
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Reduce stock should fail", false, result.IsSuccess);
			AssertEquals("No Order Lines were selected for reduction.", result.ErrorMessage);
		}

		#endregion

		#region TestReduceStocksForOrderLines_ShortCircuitWhen_ContainsOrderLineHasNoReleaseLine

		public void TestReduceStocksForOrderLines_ShortCircuitWhen_ContainsOrderLineHasNoReleaseLine()
		{
			var (order, pick) = CreateOrderWithTwoOrderLines();
			var lines = order.Lines;
			var result = ReleaseLineReductionManager.ReduceStock(new WhsOrderLine[] { lines[0] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Reduce stock result should be success", true, result.IsSuccess);
			AssertEquals("OrderLine[0].ReleaseLines should be empty", 0, order.Lines[0].ReleaseLines.Count);

			result = ReleaseLineReductionManager.ReduceStock(new WhsOrderLine[] { lines[0], lines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Reduce stock result should be failed", false, result.IsSuccess);
			AssertEquals("One or more Order Lines have no Release Line.", result.ErrorMessage);
			AssertEquals("ReleaseLines[0] qty should be 5.", 5m, order.Lines[1].ReleaseLines[0].Quantity);
			AssertEquals("ReleaseLines[1] qty should be 5.", 5m, order.Lines[1].ReleaseLines[1].Quantity);
		}

		#endregion

		#region TestReduceStocksForOrderLines_ShortCircuitWhen_PickHasChanges

		public void TestReduceStocksForOrderLines_ShortCircuitWhen_PickHasChanges()
		{
			var (order, pick) = CreateOrderWithTwoOrderLines();
			pick.WP_PercentageComplete = 8;
			Assert("Precondition:", pick.HasChanges);
			var lines = order.Lines;
			var result = ReleaseLineReductionManager.ReduceStock(new WhsOrderLine[] { lines[0], lines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be failed", false, result.IsSuccess);
			AssertEquals($"Save pick before attempting to '{ReduceStockDescription}'.", result.ErrorMessage);
			AssertEquals("OrderLine[0].ReleaseLines[0] qty should be 5.", 5m, lines[0].ReleaseLines[0].Quantity);
			AssertEquals("OrderLine[0].ReleaseLines[1] qty should be 5.", 5m, lines[0].ReleaseLines[1].Quantity);
			AssertEquals("OrderLine[1].ReleaseLines[0] qty should be 5.", 5m, lines[1].ReleaseLines[0].Quantity);
			AssertEquals("OrderLine[1].ReleaseLines[1] qty should be 5.", 5m, lines[1].ReleaseLines[1].Quantity);
		}

		#endregion

		#region TestReduceStocksForOrderLines_FailsIfNotEnoughKitsForPickByBOMToReduceAllOrderLines

		public void TestReduceStocksForOrderLines_FailsIfNotEnoughKitsForPickByBOMToReduceAllOrderLines()
		{
			var order = CreateOrderWithBOMCanBePickedOnSaleOrder();

			var result = ReleaseLineReductionManager.ReduceStock(new WhsOrderLine[] { order.Lines[0], order.Lines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be failed", false, result.IsSuccess);
			AssertEquals($"Stock must be picked (kits only) or not packed to '{ReduceStockDescription}'.",
				result.ErrorMessage);
			AssertEquals("OrderLine[0].ReleaseLines[0] qty should be 3.", 3m, order.Lines[0].ReleaseLines[0].Quantity);
			AssertEquals("OrderLine[0].ReleaseLines[1] qty should be 3.", 3m, order.Lines[0].ReleaseLines[1].Quantity);
			AssertEquals("OrderLine[0].ReleaseLines[2] qty should be 4.", 4m, order.Lines[0].ReleaseLines[2].Quantity);
			AssertEquals("OrderLine[1].ReleaseLines[0] qty should be 10.", 10m,
				order.Lines[1].ReleaseLines[0].Quantity);
		}

		#endregion

		#region TestReduceStocksForOrderLines_ShortCircuitWhen_PickIsFinalised

		public void TestReduceStocksForOrderLines_ShortCircuitWhen_PickIsFinalised()
		{
			var (order, pick) = CreateOrderWithTwoOrderLines();
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Assert("Precondition:", pick.IsFinalised);

			var lines = order.Lines;
			var result = ReleaseLineReductionManager.ReduceStock(new WhsOrderLine[] { lines[0], lines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be failed", false, result.IsSuccess);
			AssertEquals($"Cannot '{ReduceStockDescription}' on a finalized Pick.", result.ErrorMessage);
			AssertEquals("OrderLine[0].ReleaseLines[0] qty should be 5.", 5m, lines[0].ReleaseLines[0].Quantity);
			AssertEquals("OrderLine[0].ReleaseLines[1] qty should be 5.", 5m, lines[0].ReleaseLines[1].Quantity);
			AssertEquals("OrderLine[1].ReleaseLines[0] qty should be 5.", 5m, lines[1].ReleaseLines[0].Quantity);
			AssertEquals("OrderLine[1].ReleaseLines[1] qty should be 5.", 5m, lines[1].ReleaseLines[1].Quantity);
		}

		#endregion

		#region TestReduceStocksForOrderLines_ShortCircuitWhen_OneReleaseLineAllPacked

		public void TestReduceStocksForOrderLines_ShortCircuitWhen_OneReleaseLineAllPacked()
		{
			var (order, pick) = CreateOrderWithTwoOrderLines();
			var package = order.PackageJob.Packages.AddNew("BOX");
			var lines = order.Lines;
			package.Pack(lines[1].ReleaseLines[0], 5m);
			package.Pack(lines[1].ReleaseLines[1], 5m);
			Factory.Save();

			var result = ReleaseLineReductionManager.ReduceStock(new WhsOrderLine[] { lines[0], lines[1] },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be failed", false, result.IsSuccess);
			AssertEquals($"Stock must be picked or not packed to '{ReduceStockDescription}'.", result.ErrorMessage);
			AssertEquals("OrderLine[0].ReleaseLines[0] qty should be 5.", 5m, lines[0].ReleaseLines[0].Quantity);
			AssertEquals("OrderLine[0].ReleaseLines[1] qty should be 5.", 5m, lines[0].ReleaseLines[1].Quantity);
			AssertEquals("OrderLine[1].ReleaseLines[0] qty should be 5.", 5m, lines[1].ReleaseLines[0].Quantity);
			AssertEquals("OrderLine[1].ReleaseLines[1] qty should be 5.", 5m, lines[1].ReleaseLines[1].Quantity);
		}

		#endregion

		#region TestReduceStocksForOrderLines_DoNotSaveWhenSomeLineHasError

		public void TestReduceStocksForOrderLines_DoNotSaveWhenSomeLineHasError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "AAA", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "BBB", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Empty, ZDate.Empty, "CCC", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Empty, ZDate.Empty, "DDD", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreatePickNew(order);

			AssertEquals("There should be 2 Order Lines", 2, order.Lines.Count);
			AssertEquals("There should be 2 Release Lines in orderLine1", 2, orderLine1.ReleaseLines.Count);
			AssertEquals("There should be 2 Release Lines in orderLine2", 2, orderLine2.ReleaseLines.Count);

			Factory.Save();

			orderLine1.PickLines.ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = "E";
				pl.WZ_PickedDateTime = ZDateTimeOffset.Today;
			});
			orderLine2.PickLines.ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = "E";
				pl.WZ_PickedDateTime = ZDateTimeOffset.Today;
			});

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine2.PK).PickLines[1];
			using (((IWhsPickLineInternals)pickLineInOtherFactory)
				   .TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				pickLineInOtherFactory.InventoryLine.WE_StockOnHand += pickLineInOtherFactory.WZ_Units;
				pickLineInOtherFactory.Delete();
				otherFactory.Save();
			}

			var result = ReleaseLineReductionManager.ReduceStock(new WhsOrderLine[] { orderLine1, orderLine2 },
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason);
			AssertEquals("Bulk reduce stock should be failed", false, result.IsSuccess);
			AssertEquals(
				$"Another user has modified the Allocated Stock on this Order.\r\nClose and re-open the form, then attempt to '{ReduceStockDescription}' again.",
				result.ErrorMessage);

			var newFactory = new BusinessObjectFactory();
			orderLine1 = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			orderLine2 = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("There should be 2 Release Line in orderLine1.", 2, orderLine1.ReleaseLines.Count);
			AssertEquals("There should be 1 Release Line in orderLine2.", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("OrderLine[0].ReleaseLines[0] qty should be 5.", 5m, orderLine1.ReleaseLines[0].Quantity);
			AssertEquals("OrderLine[0].ReleaseLines[1] qty should be 5.", 5m, orderLine1.ReleaseLines[1].Quantity);
			AssertEquals("OrderLine[1].ReleaseLines[0] qty should be 5.", 5m, orderLine2.ReleaseLines[0].Quantity);

			var adjustments = order.GetRelatedAdjustmentsAndTransfers();
			AssertEquals("Should not create Adjustment or Transfer", 0, adjustments.Count());
		}

		#endregion

		(WhsOrder, WhsPick) CreateOrderWithSingleOrderLine(bool isVirtualWarehouse = false)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = isVirtualWarehouse;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty,
				ZDate.Empty, "AAA", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty,
				ZDate.Empty, "BBB", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			line1.PickLines.ForEach(pl => pl.WZ_GS_NKAssignedTo = "E");
			line1.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			Factory.Save();

			AssertEquals("order releaseline is not empty", 2, line1.ReleaseLines.Count);

			return (order, pick);
		}

		(WhsOrder, WhsPick) CreateOrderWithTwoOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty,
				ZDate.Empty, "AAA", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty,
				ZDate.Empty, "BBB", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, data.Whs1.DefaultLocation, ZDate.Empty,
				ZDate.Empty, "CCC", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, data.Whs1.DefaultLocation, ZDate.Empty,
				ZDate.Empty, "DDD", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);
			line1.PickLines.ForEach(pl => pl.WZ_GS_NKAssignedTo = "E");
			line1.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			line2.PickLines.ForEach(pl => pl.WZ_GS_NKAssignedTo = "E");
			line2.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			Factory.Save();

			return (order, pick);
		}

		WhsOrder CreateOrderWithBOMCanBePickedOnSaleOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			Helper.SetProductAttributeUse(data.Org1, mainProduct, AttributeNumber.One, true);
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 3m, data.Whs1.DefaultLocation, ZDate.Empty,
				ZDate.Empty, "RED", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 3m, data.Whs1.DefaultLocation, ZDate.Empty,
				ZDate.Empty, "BLUE", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, data.Whs1.DefaultLocation,
				ZDate.Empty, ZDate.Empty, "", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Helper.CreateWhsOrderLine(order, bomComponentProduct, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().Where(pl => !pl.IsPickByBOMKitPickLine()).ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			Factory.Save();

			return order;
		}

		#region Implementation

		protected abstract ReduceStockReason ReduceStockReason { get; }
		protected abstract string ReduceStockDescription { get; }

		#endregion
	}

	class QtyLostReleaseLineReductionManagerTest : ReleaseLineReductionManagerTest
	{
		protected override ReduceStockReason ReduceStockReason => ReduceStockReason.Lost;
		protected override string ReduceStockDescription => "Adjust Out Quantity Met";
	}

	class ReturnStockReleaseLineReductionManagerTest : ReleaseLineReductionManagerTest
	{
		protected override ReduceStockReason ReduceStockReason => ReduceStockReason.Returned;
		protected override string ReduceStockDescription => "Return Items to Stock";
	}
}
