using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PickedStockAdjustersFactoryTest : WhsTestCaseWithFactory
	{
		public void TestGetNewRecorderThrowsExceptionWhenWarehouseNotSpecified()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PickedStockAdjustersFactory().GetNewAdjuster(null, OrgHeader.DefaultOrg, ReduceStockReason.Lost));
			AssertExceptionThrown<ArgumentNullException>(() => new PickedStockAdjustersFactory().GetNewAdjuster(null, OrgHeader.DefaultOrg, ReduceStockReason.Returned));
		}

		public void TestGetNewRecorderThrowsExceptionWhenClientNotSpecified()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PickedStockAdjustersFactory().GetNewAdjuster(Factory.New<WhsWarehouse>(), null, ReduceStockReason.Lost));
			AssertExceptionThrown<ArgumentNullException>(() => new PickedStockAdjustersFactory().GetNewAdjuster(Factory.New<WhsWarehouse>(), null, ReduceStockReason.Returned));
		}

		public void TestGetNewAdjuster_ReturnsAnAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var adjuster = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);
			AssertEquals("Currently we always create an Adjustment to record lost stock.", typeof(WhsAdjustment), adjuster.GetType());
			var adjustment = (WhsAdjustment)adjuster;

			CombineAssertions(() =>
			{
				AssertEquals("Should have correct warehouse populated.", data.Whs1, adjustment.Warehouse);
				AssertEquals("Should have correct client populated.", data.Org1, adjustment.Client);
				AssertEquals(AdjustmentType.Codes.Adjustment, adjustment.WD_DocketSubType);
			});
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestGetNewAdjuster_ReturnsATransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var adjuster = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Returned);
			AssertEquals("Currently we always create a Transfer to return stock.", typeof(WhsTransfer), adjuster.GetType());
			var transfer = (WhsTransfer)adjuster;

			CombineAssertions(() =>
			{
				AssertEquals("Should have correct warehouse populated.", data.Whs1, transfer.Warehouse);
				AssertEquals("Should have correct client populated.", data.Org1, transfer.Client);
				AssertEquals(TransferType.Codes.Internal, transfer.WD_DocketSubType);
			});
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestGetAdjusterSecurityCheckpoint_WhenReduceStockReason_Lost()
		{
			var checkpoint = new PickedStockAdjustersFactory().GetAdjusterSecurityCheckpoint(ReduceStockReason.Lost);
			AssertEquals(Env.Security.WhsReleaseAdjustOutQtyMet, checkpoint);
		}

		public void TestGetAdjusterSecurityCheckpoint_WhenReduceStockReason_Returned()
		{
			var checkpoint = new PickedStockAdjustersFactory().GetAdjusterSecurityCheckpoint(ReduceStockReason.Returned);
			AssertEquals(Env.Security.WhsReleaseReturnItemsToStock, checkpoint);
		}

		public void TestFunctionDescription()
		{
			AssertEquals("Adjust Out Quantity Met", new PickedStockAdjustersFactory().GetDescription(ReduceStockReason.Lost));
			AssertEquals("Return Items to Stock", new PickedStockAdjustersFactory().GetDescription(ReduceStockReason.Returned));
		}
	}
}
