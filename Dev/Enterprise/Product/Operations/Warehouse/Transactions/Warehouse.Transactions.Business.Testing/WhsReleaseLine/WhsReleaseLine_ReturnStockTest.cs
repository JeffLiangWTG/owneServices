using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReleaseLine_ReturnStockTest : WhsReleaseLine_ReducePickedStockTest<WhsTransfer>
	{
		#region TestReducePickedStock_Staged_EndToEnd

		public void TestReducePickedStock_Staged_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var transferLine = Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var pickLine = orderLine.PickLines.Single();
			var newPickLine = transferLine.PickLines.Single();
			AssertEquals("Precondition: PickLine is In-Transit.", true, pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid);
			AssertEquals("Precondition:", 10m, pickLine.WZ_Units);
			AssertEquals("Precondition: Stock is reduced immediately.", 0m, newPickLine.InventoryLine.WE_StockOnHand);
			Factory.Save();

			var result = ReducePickedStock(releaseLine, 10m);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true, result.IsSuccess);

			var docket = (WhsDocket)order.RelatedJobs.SingleOrDefault();
			AssertNotNull("Created adjustment/transfer must be linked to the order.", docket);
			AssertDocketCreated(docket, 1, 10m, transferLine.WE_WL_TransferFrom);

			AssertEquals("Original transfer line's original inventory status should be Available.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Original transfer line's current inventory status should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);

			var returnStockTransferLine = docket.Lines.Single();
			AssertEquals("Should be In-Transit.", InventoryStatus.Codes.InTransit, returnStockTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Should be In-Transit.", InventoryStatus.Codes.InTransit, returnStockTransferLine.WE_CurrentInventoryStatus);

			docket.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Original status should be Staged.", InventoryStatus.Codes.Staged, returnStockTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Current status should be Available.", InventoryStatus.Codes.Available, returnStockTransferLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestReducePickedStock_ReadyToPack_EndToEnd

		public void TestReducePickedStock_ReadyToPack_EndToEnd_PackingStation()
		{
			TestReducePickedStock_ReadyToPack_EndToEndCore(LocationClasses.Codes.PST);
		}

		public void TestReducePickedStock_ReadyToPack_EndToEnd_PackingConsolidation()
		{
			TestReducePickedStock_ReadyToPack_EndToEndCore(LocationClasses.Codes.CON);
		}

		void TestReducePickedStock_ReadyToPack_EndToEndCore(string packingLocationClass)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, packingLocationClass);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var transferLine = Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var pickLine = orderLine.PickLines.Single();
			var newPickLine = transferLine.PickLines.Single();
			AssertEquals("Precondition: PickLine is In-Transit.", true, pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid);
			AssertEquals("Precondition:", 10m, pickLine.WZ_Units);
			AssertEquals("Precondition: Stock is reduced immediately.", 0m, newPickLine.InventoryLine.WE_StockOnHand);
			Factory.Save();

			var result = ReducePickedStock(releaseLine, 10m);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true, result.IsSuccess);

			var docket = (WhsDocket)order.RelatedJobs.SingleOrDefault();
			AssertNotNull("Created adjustment/transfer must be linked to the order.", docket);
			AssertDocketCreated(docket, 1, 10m, transferLine.WE_WL_TransferFrom);

			AssertEquals("Original transfer line's original inventory status should be Available.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Original transfer line's current inventory status should be ReadyToPack.", InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);

			var returnStockTransferLine = docket.Lines.Single();
			AssertEquals("Should be In-Transit.", InventoryStatus.Codes.InTransit, returnStockTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Should be In-Transit.", InventoryStatus.Codes.InTransit, returnStockTransferLine.WE_CurrentInventoryStatus);

			docket.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Original status should be ReadyToPack.", InventoryStatus.Codes.ReadyToPack, returnStockTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Current status should be Available.", InventoryStatus.Codes.Available, returnStockTransferLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#region Implementation

		protected override ZQuery GetAdditionalFilterOnDocketsCreated() => new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, null);

		protected override ActionResult ReducePickedStock(WhsReleaseLine releaseLine, decimal qtyToReduce)
		{
			return ReleaseLineReductionManager.ReduceStock(releaseLine, qtyToReduce, ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
		}

		protected override ZString FunctionDescription => "Return Items to Stock";

		protected override ZString ExpectedDocketTypeCreated => DocketType.Codes.Transfer;

		protected override bool OnlyTouchesOutboundTransferLocation => false;

		protected override void AssertDocketCreated(WhsDocket docket, int expectedLineCount, decimal expectedUnitSum, ZGuid? originalLocation)
		{
			CombineAssertions(() =>
			{
				AssertEquals(DocketType.Codes.Transfer, docket.WD_DocketType);

				AssertEquals(TransferType.Codes.Internal, docket.WD_DocketSubType);
				AssertEquals($"Transfer should have {expectedLineCount} line(s).", expectedLineCount, docket.Lines.Count);
				AssertEquals($"Transfer should return {expectedUnitSum} units.", expectedUnitSum, docket.Lines.Sum(line => line.WE_TransactionQuantity));
				AssertEquals("Transfer should NOT be finalised.", false, docket.IsFinalised);
				AssertEquals("Transfer should NOT be finalised.", false, docket.Lines.All(line => line.IsFinalised));
				AssertEquals("Transfer should be Picked.", true, docket.Lines.Cast<WhsTransferLine>().All(line => line.IsPicked));
				AssertEquals("Transfer should be In-Transit.", true, docket.Lines.Cast<WhsTransferLine>().All(line => line.WE_OriginalInventoryStatus == InventoryStatus.Codes.InTransit));
				AssertEquals("Transfer should be In-Transit.", true, docket.Lines.Cast<WhsTransferLine>().All(line => line.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));

				if (originalLocation.HasValue)
				{
					AssertEquals("Transfer should be returning Stock to the Pick Location.", true, docket.Lines.Cast<WhsTransferLine>().All(line => line.WE_WL == originalLocation.Value));
				}
			});
		}

		#endregion
	}
}
