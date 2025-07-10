using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsCheckStockOnHandIsBalancedStrategyTest : WhsTestCaseWithFactory
	{
		#region TestWhsCheckStockOnHandIsBalancedStrategy_WhsDocketLine

		#region TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ReceiveLine

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ReceiveLine>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not be defer trigger if Receive Line has full SOH.", false,
				strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_StockOnHand = 5m;
			AssertEquals("Should defer if SOH is less than transaction quantity.", true, strategy.ShouldDeferTrigger(receiveLine));
		}

		#endregion

		#region TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_CancelledReceiveLine

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_CancelledReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			var receiveLine = receive.Lines.Single();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ReceiveLine>() as
					IDeferTriggerConditionStrategy;

			receiveLine.WE_StockOnHand = 5m;
			AssertEquals("Should defer if SOH is less than transaction quantity.", true, strategy.ShouldDeferTrigger(receiveLine));

			receive.CancelReactivateDocket();
			AssertEquals("Precondition: Receive is cancelled.", true, receive.IsCancelled);
			AssertEquals("Precondition: ReceiveLine is cancelled.", "CAN", receiveLine.WE_DocketLineStatus);
			AssertEquals("Precondition: ReceiveLine has correct transaction quantity.", 10m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: ReceiveLine has correct SOH.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Should not defer if receive line is cancelled.", false, strategy.ShouldDeferTrigger(receiveLine));
		}

		#endregion

		#region TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_ReceiveLine

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_ReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_ReceiveLine>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_ClientOrderedUnits = 15m;
			AssertEquals("Should not defer if relevant property changes.", false, strategy.ShouldDeferTrigger(receiveLine));
			receiveLine.WE_ClientOrderedUnits = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_TransactionQuantity = 15m;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(receiveLine));
			receiveLine.WE_TransactionQuantity = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_StockOnHand = 15m;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(receiveLine));
			receiveLine.WE_StockOnHand = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(receiveLine));

			var finalisedDate = receiveLine.WE_FinalisedDate;
			receiveLine.WE_FinalisedDate = ZDateTimeOffset.Today.AddDays(-10);
			AssertEquals("Should defer if relevant property changes.", false, strategy.ShouldDeferTrigger(receiveLine));
			receiveLine.WE_FinalisedDate = finalisedDate; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(receiveLine));
			receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_IsOriginalInventory = false;
			AssertEquals("Should not defer trigger if there are no relevant changes for the line.", false,
				strategy.ShouldDeferTrigger(receiveLine));
		}

		#endregion

		#region TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_AdjustmentLine

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_AdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_AdjustmentLine>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(adjustmentLine));

			adjustmentLine.WE_ClientOrderedUnits = 15m;
			AssertEquals("Should not defer if relevant property changes.", false, strategy.ShouldDeferTrigger(adjustmentLine));
			adjustmentLine.WE_ClientOrderedUnits = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(adjustmentLine));

			adjustmentLine.WE_TransactionQuantity = 15m;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(adjustmentLine));
			adjustmentLine.WE_TransactionQuantity = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(adjustmentLine));

			adjustmentLine.WE_StockOnHand = 15m;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(adjustmentLine));
			adjustmentLine.WE_StockOnHand = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(adjustmentLine));

			var finalisedDate = adjustmentLine.WE_FinalisedDate;
			adjustmentLine.WE_FinalisedDate = ZDateTimeOffset.Today.AddDays(-10);
			AssertEquals("Should defer if relevant property changes.", false, strategy.ShouldDeferTrigger(adjustmentLine));
			adjustmentLine.WE_FinalisedDate = finalisedDate; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(adjustmentLine));

			adjustmentLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(adjustmentLine));
			adjustmentLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(adjustmentLine));

			adjustmentLine.WE_IsOriginalInventory = false;
			AssertEquals("Should not defer trigger if there are no relevant changes for the line.", false,
				strategy.ShouldDeferTrigger(adjustmentLine));
		}

		#endregion

		#region TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, "", data.Whs1.DefaultLocation, "", GlbStaff.CurrentUser);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_ClientOrderedUnits = 15m;
			AssertEquals("Should not defer if relevant property changes.", false, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_ClientOrderedUnits = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_TransactionQuantity = 15m;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_TransactionQuantity = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_StockOnHand = 15m;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_StockOnHand = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			var finalisedDate = transferLine.WE_FinalisedDate;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.Today.AddDays(-10);
			AssertEquals("Should defer if relevant property changes.", false, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_FinalisedDate = finalisedDate; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.HeldForTransfer;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_IsOriginalInventory = false;
			AssertEquals("Should not defer trigger if there are no relevant changes for the line.", false,
				strategy.ShouldDeferTrigger(transferLine));
		}

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_PickedTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, "", data.Whs1.DefaultLocation, "", GlbStaff.CurrentUser);
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_ClientOrderedUnits = 15m;
			AssertEquals("Should not defer if relevant property changes.", false, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_ClientOrderedUnits = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_TransactionQuantity = 15m;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_TransactionQuantity = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_StockOnHand = 15m;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_StockOnHand = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_FinalisedDate = ZDateTimeOffset.Today.AddDays(-10);
			AssertEquals("Should defer if relevant property changes.", false, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_FinalisedDate = ZDateTimeOffset.Empty; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(transferLine));
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.HeldForTransfer; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_IsOriginalInventory = false;
			AssertEquals("Should not defer trigger if there are no relevant changes for the line.", false,
				strategy.ShouldDeferTrigger(transferLine));
		}

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_EnteredTransferLineIgnored()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer Line is committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_TransactionQuantity = 15m;
			AssertEquals("Should not defer trigger for entered transfer line.", false, strategy.ShouldDeferTrigger(transferLine));
		}

		#endregion

		#region TestWhsCheckStockOnHandIsBalancedStrategy_ParentInventoryLineInsert

		public void TestWhsCheckStockOnHandIsBalancedStrategy_ParentInventoryLineInsert()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ForParent>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Should not defer trigger if it's not a child line.", false, strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_IsOriginalInventory = false;
			AssertEquals("Should not be defer trigger if it's not a child line.", false, strategy.ShouldDeferTrigger(receiveLine));
			receiveLine.WE_IsOriginalInventory = true; // cleanup

			receiveLine.WE_WE_ParentDocketLine = ZGuid.NewZGuid();
			AssertEquals("Should not be defer trigger if it's not a child line.", false, strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_IsOriginalInventory = false;
			AssertEquals("Should defer trigger if it's a child line.", true, strategy.ShouldDeferTrigger(receiveLine));
		}

		#endregion

		#region TestWhsCheckStockOnHandIsBalancedStrategy_ParentInventoryLineInsertUpdate

		public void TestWhsCheckStockOnHandIsBalancedStrategy_ParentInventoryLineInsertUpdate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_IsOriginalInventory = false;
			AssertEquals("Should defer if relevant property changes.", false, strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_WE_ParentDocketLine = ZGuid.NewZGuid();
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(receiveLine));
			receiveLine.WE_WE_ParentDocketLine = ZGuid.Empty; // clean up
			receiveLine.WE_IsOriginalInventory = true; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_TransactionQuantity = 15m;
			AssertEquals("Should not defer trigger if there are no relevant changes for the line.", false,
				strategy.ShouldDeferTrigger(receiveLine));
		}

		public void TestWhsCheckStockOnHandIsBalancedStrategy_ParentInventoryLineInsertUpdate_DoesNotDeferInsert()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Should not be defer trigger if it's an insert.", false, strategy.ShouldDeferTrigger(receiveLine));

			receiveLine.WE_IsOriginalInventory = false;
			receiveLine.WE_WE_ParentDocketLine = ZGuid.NewZGuid();
			AssertEquals("Should not be defer trigger if it's an insert.", false, strategy.ShouldDeferTrigger(receiveLine));
		}

		public void TestWhsCheckStockOnHandIsBalancedStrategy_ParentInventoryLineInsertUpdate_IsAlreadyChildLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			receiveLine.WE_StockOnHand = 6m;
			var childReceiveLine = (WhsReceiveLine)receiveLine.Clone();
			childReceiveLine.WE_WD = receive.PK;
			childReceiveLine.WE_TransactionQuantity = 4m;
			childReceiveLine.WE_StockOnHand = 4m;
			childReceiveLine.WE_IsOriginalInventory = false;
			childReceiveLine.WE_WE_ParentDocketLine = receiveLine.PK;
			childReceiveLine.WE_WE_OriginalDocketLineForRating = receiveLine.PK;
			childReceiveLine.WE_OriginalInventoryStatus = receiveLine.WE_OriginalInventoryStatus;
			childReceiveLine.WE_CurrentInventoryStatus = receiveLine.WE_CurrentInventoryStatus;
			childReceiveLine.WE_FinalisedDate = receiveLine.WE_FinalisedDate;
			childReceiveLine.WE_DocketLineStatus = receiveLine.WE_DocketLineStatus;
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(childReceiveLine));

			childReceiveLine.WE_IsOriginalInventory = true;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(childReceiveLine));
			childReceiveLine.WE_IsOriginalInventory = false; // clean up

			childReceiveLine.WE_WE_ParentDocketLine = ZGuid.Empty;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(childReceiveLine));
			childReceiveLine.WE_WE_ParentDocketLine = receiveLine.PK; // clean up

			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(childReceiveLine));

			childReceiveLine.WE_TransactionQuantity = 15m;
			AssertEquals("Should defer trigger if transaction quantity changes for the line.", true, strategy.ShouldDeferTrigger(childReceiveLine));
		}

		#endregion

		#region TestWhsCheckStockOnHandIsBalancedStrategy_IgnoresAdjustmentOutLines

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_IgnoresAdjustmentOutLines()
		{
			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_AdjustmentLine>() as
					IDeferTriggerConditionStrategy;
			TestWhsCheckStockOnHandIsBalancedStrategy_IgnoresAdjustmentOutLines(strategy,
				l =>
				{
					l.WE_DocketLineStatus = "FIN";
					l.WE_FinalisedDate = ZDateTimeOffset.Now;
				},
				isInsert: false);
		}

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_IgnoresAdjustmentOutLines()
		{
			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_AdjustmentLine>() as
					IDeferTriggerConditionStrategy;
			TestWhsCheckStockOnHandIsBalancedStrategy_IgnoresAdjustmentOutLines(strategy,
				l =>
				{
					l.WE_DocketLineStatus = "FIN";
					l.WE_FinalisedDate = ZDateTimeOffset.Now;
				},
				isInsert: true);
		}

		void TestWhsCheckStockOnHandIsBalancedStrategy_IgnoresAdjustmentOutLines(
			IDeferTriggerConditionStrategy strategy, Action<WhsDocketLine> modifyToTriggerDeferralInInventory, bool isInsert)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);

			if (!isInsert)
			{
				Factory.Save();
			}

			AssertEquals("Precondition - should not defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(adjustmentLine));

			modifyToTriggerDeferralInInventory(adjustmentLine);
			AssertEquals("Should defer trigger for adjustment IN lines.", true,
				strategy.ShouldDeferTrigger(adjustmentLine));

			adjustmentLine.WE_TransactionQuantity = -15m;
			AssertEquals("Should NOT defer trigger for adjustment OUT lines.", false,
				strategy.ShouldDeferTrigger(adjustmentLine));

			if (isInsert)
			{
				adjustmentLine.WE_TransactionQuantity = 15m;
				AssertEquals("Should defer trigger for adjustment IN lines.", true,
					strategy.ShouldDeferTrigger(adjustmentLine));

				adjustmentLine.WE_StockOnHand = 15m;
				AssertEquals("Should NOT defer trigger for adjustment IN lines with full SOH.", false,
					strategy.ShouldDeferTrigger(adjustmentLine));
			}
		}

		#endregion

		#region TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_TransferLine

		public void TestWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_TransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, data.Whs1.DefaultLocation, "P");
			transferLine.WE_TransactionQuantity = 10m;

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_TransferLine>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not defer trigger for entered transferLine.", false,
				strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_StockOnHand = 5m;
			AssertEquals("Should not defer for entered transferLine.", false, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_DocketLineStatus = "HFT";
			AssertEquals("Should defer for HFT transferLine.", true, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_DocketLineStatus = "FIN";
			AssertEquals("Should defer for FIN transferLine.", true, strategy.ShouldDeferTrigger(transferLine));

			transferLine.WE_StockOnHand = 10m;
			AssertEquals("Should not defer for transferLine with full SOH.", false, strategy.ShouldDeferTrigger(transferLine));
		}

		#endregion

		#region TestWhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy

		public void TestWhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var receiveLine = receive.Lines[0];
			receiveLine.WE_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			var serialNumber = Helper.CreateWhsSerialNumber(data.Org1, data.Part1, "SN1");
			Helper.CreateWhsSerialNumberPivot(receiveLine, serialNumber);
			Factory.Save();

			var cacheKey = "IWhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy_Client|" + receive.PK;
			Factory.TryGetValueFromCacheOnly(cacheKey, out OrgHeader cacheValue);
			AssertEquals("Precondition", null, cacheValue);

			var strategy = ObjectFactory.Get<IWhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy>() as IDeferTriggerConditionStrategy;

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false, strategy.ShouldDeferTrigger(receiveLine));

				receiveLine.WE_StockOnHand = 2m;
				AssertEquals("Should defer if not relevant property changes.", false, strategy.ShouldDeferTrigger(receiveLine));
				receiveLine.WE_StockOnHand = 1m; // clean up
				AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(receiveLine));

				receiveLine.WE_TransactionQuantity = 2m;
				AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(receiveLine));

				Factory.TryGetValueFromCacheOnly(cacheKey, out cacheValue);
				AssertEquals("Should cache client.", receive.Client, cacheValue);

				receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
				receiveLine.WE_FinalisedDate = ZDateTimeOffset.Empty;
				AssertEquals("Should defer is not finalised even related property changes.", false, strategy.ShouldDeferTrigger(receiveLine));
				receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised; // clean up
				receiveLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
				AssertEquals("Should still defer TransactionQuantity is changed.", true, strategy.ShouldDeferTrigger(receiveLine));

				var relation = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, "OWN");
				relation.OU_UseSerialNumber = false;
				AssertEquals("Should not defer even serial number not used.", false, strategy.ShouldDeferTrigger(receiveLine));
				relation.OU_UseSerialNumber = true; // clean up
				AssertEquals("Should still defer TransactionQuantity is changed.", true, strategy.ShouldDeferTrigger(receiveLine));

				receiveLine.WE_TransactionQuantity = 1m; // clean up
				AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(receiveLine));
			}

			receiveLine.WE_TransactionQuantity = 2m;
			AssertEquals("Should not defer if EnableSchemaRedesign is false.", false, strategy.ShouldDeferTrigger(receiveLine));
		}

		#endregion

		#endregion

		#region TestWhsCheckStockOnHandIsBalancedStrategy_WhsPickLine

		#region TestWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate

		public void TestWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, receiveLine.Location);
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var pickLine = adjustmentLine.PickLines.Single();
			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(pickLine));

			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_WE_InventoryLine] = Guid.NewGuid();
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(pickLine));
			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_WE_InventoryLine] =
				receiveLine.PK.ToGuid(); // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(pickLine));

			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_Units] = 15m;
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(pickLine));
			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_Units] = 10m; // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(pickLine));

			var pickedDate = pickLine.WZ_PickedDateTime;
			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_PickedDateTime] =
				ZDateTimeOffset.Today.AddDays(-10).ToDateTimeOffset();
			AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(pickLine));
			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_PickedDateTime] =
				pickedDate.ToDateTimeOffset(); // clean up
			AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(pickLine));

			pickLine.WZ_WE_TransactionLine = ZGuid.NewZGuid();
			AssertEquals("Should not defer trigger if there are no relevant changes for the line.", false,
				strategy.ShouldDeferTrigger(pickLine));
		}

		public void TestWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate_IgnoresZeroPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pickLine = Helper.CreateReservePickLine(orderLine, receiveLine.Inventory[0], 10m);
			pickLine.WZ_Units = 0m;
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(pickLine));

			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_PickedDateTime] =
				ZDateTimeOffset.Today.ToDateTimeOffset();
			AssertEquals("If WZ_Units is 0 and has not changed, then we should NOT defer trigger for the line.", false,
				strategy.ShouldDeferTrigger(pickLine));

			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_Units] = 15m;
			AssertEquals("If WZ_Units is > 0 or changed to / from 0, then we should defer trigger for the line.", true,
				strategy.ShouldDeferTrigger(pickLine));
		}

		public void TestWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate_IgnoresNotPickedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pickLine = Helper.CreateReservePickLine(orderLine, receiveLine.Inventory[0], 10m);
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate>() as
					IDeferTriggerConditionStrategy;
			AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false,
				strategy.ShouldDeferTrigger(pickLine));

			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_Units] = 15m;
			AssertEquals(
				"If pick line is not picked and has not been unpicked, then we should NOT defer trigger for the line.",
				false, strategy.ShouldDeferTrigger(pickLine));

			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_PickedDateTime] =
				ZDateTimeOffset.Today.ToDateTimeOffset();
			AssertEquals(
				"If pick line is picked, or has changed to / from picked, then we should defer trigger for the line.",
				true, strategy.ShouldDeferTrigger(pickLine));
		}

		#endregion

		#endregion
	}
}
