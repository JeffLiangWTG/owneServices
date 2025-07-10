using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReleaseLine_AdjustOutQuantityMetTest : WhsReleaseLine_ReducePickedStockTest<WhsAdjustment>
	{
		protected override ActionResult ReducePickedStock(WhsReleaseLine releaseLine, decimal qtyToReduce)
		{
			return ReleaseLineReductionManager.ReduceStock(releaseLine, qtyToReduce, ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Lost);
		}

		protected override ZString FunctionDescription => "Adjust Out Quantity Met";

		protected override ZString ExpectedDocketTypeCreated => DocketType.Codes.Adjustment;

		protected override bool OnlyTouchesOutboundTransferLocation => true;

		protected override void AssertDocketCreated(WhsDocket docket, int expectedLineCount, decimal expectedUnitSum, ZGuid? originalLocation)
		{
			CombineAssertions(() =>
			{
				AssertEquals(DocketType.Codes.Adjustment, docket.WD_DocketType);
				AssertEquals(AdjustmentType.Codes.Adjustment, docket.WD_DocketSubType);
				AssertEquals($"Adjustment should have a {expectedLineCount} lines.", expectedLineCount, docket.Lines.Count);
				AssertEquals("All lines must have DamagedStock reason.", expectedLineCount, docket.Lines.Count(line => line.WE_ReasonCode == AdjustmentReasonCodesCodeList.Codes.DamagedStock));
				var negativeExpectedUnitSum = -expectedUnitSum;
				AssertEquals($"Adjustment should adjust out lost stock of {negativeExpectedUnitSum} units.", negativeExpectedUnitSum, docket.Lines.Sum(line => line.WE_TransactionQuantity));
				AssertEquals("Adjustment should be finalised.", true, docket.IsFinalised);
				AssertEquals("AdjustmentLine should be finalised.", true, docket.Lines.All(line => line.IsFinalised));

				var wasPickedDirectly = docket.Lines.All(l => l.PickLines.All(pl => pl.InventoryLine.Docket.WD_WP_ParentPickForTransfer.IsEmpty));
				var expectedInventoryStatus = wasPickedDirectly ? InventoryStatus.Codes.Available : InventoryStatus.Codes.Staged;
				AssertEquals("Adjustment should have picked Staged inventory.", true, docket.Lines.Cast<WhsAdjustmentLine>().All(line => line.WE_OriginalInventoryStatus == expectedInventoryStatus));
				AssertEquals("Current status should be the same as original inventory status.", true, docket.Lines.Cast<WhsAdjustmentLine>().All(line => line.WE_CurrentInventoryStatus == expectedInventoryStatus));
			});
		}
	}
}
