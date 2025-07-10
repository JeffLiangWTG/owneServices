using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAdjustmentLineTriggerTestCase : WhsDocketLineTriggerTestCase
	{
		protected override WhsDocketLine GetNewDocketLineForNewNonFinalisedDocket(TestDataSimpleEnvironment data)
		{
			var docket = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			return Helper.CreateWhsAdjustmentLine(docket, data.Part1, 1, data.Whs1.FindLocation("A-1"));
		}

		protected override WhsDocketLine GetNewDocketLineForNewFinalisedDocket(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product, ZDecimal qty)
		{
			var docket = Helper.CreateWhsAdjustment(client, whs);
			var docketLine = Helper.CreateWhsAdjustmentLine(docket, product, 1, whs.FindLocation("A-1"));
			docketLine.WE_TransactionQuantity = qty;
			docket.FinaliseDocketWithoutUserConfirmation();
			return docketLine;
		}

		protected override WhsDocketLine GetNewDocketLineForDocket(WhsDocket docket, OrgSupplierPart product)
		{
			return Helper.CreateWhsAdjustmentLine((WhsAdjustment)docket, product, 1, docket.Warehouse.FindLocation("A-1"));
		}

		protected override bool DoesCreateNewStock
		{
			get { return true; }
		}

		protected override void OverFlowLocationCapacityTriggerTestForSetWE_TransactionQuantityCore(WhsDocketLine docketLine, ZDecimal targetQty)
		{
			docketLine.SuspendValidation();
			base.OverFlowLocationCapacityTriggerTestForSetWE_TransactionQuantityCore(docketLine, targetQty);
			docketLine.Docket.FinaliseDocket(); // Pending adjustments are ignored for the trigger, finalised adjustment must be used to test the trigger.
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docketLine.Docket);
		}

		protected override void OverFlowTriggerTestForSetWE_WLCore(WhsDocketLine docketLine, WhsTransferLine pendingTransferLine)
		{
			docketLine.SuspendValidation();
			base.OverFlowTriggerTestForSetWE_WLCore(docketLine, pendingTransferLine);
			docketLine.Docket.FinaliseDocket(); // Pending adjustments are ignored for the trigger, finalised adjustment must be used to test the trigger.
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docketLine.Docket);
		}

		#region TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrect

		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert()
		{
			TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrectCore((adjustmentLine) =>
			{
				Helper.CreateWhsAdjustmentLine(adjustmentLine.Docket, adjustmentLine.SupplierPart, -10m, adjustmentLine.Location);
			});
		}

		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Update()
		{
			TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrectCore((adjustmentLine) =>
			{
				adjustmentLine.WE_TransactionQuantity = -13m;
			});
		}

		void TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrectCore(Action<WhsAdjustmentLine> setDataForTriggerToFail)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, receive.Lines[0].Location);

			adjustmentLine.RunPreSaveValidation(); // to commit inventory and create pickline
			Factory.Save();

			// dodgy insert / modify adjustment line to ensure trigger fails
			setDataForTriggerToFail(adjustmentLine);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID}", true), "When trying to over-pick adjustment line the trigger should fail.");
		}

		#endregion
	}
}
