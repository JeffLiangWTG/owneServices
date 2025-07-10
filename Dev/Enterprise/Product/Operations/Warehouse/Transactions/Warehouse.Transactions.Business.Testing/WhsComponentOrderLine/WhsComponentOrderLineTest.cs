using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsComponentOrderLineTest<TDocketLine, TDocket> : WhsPickableDocketLineTest<TDocketLine, TDocket>
		where TDocketLine : WhsComponentOrderLine
		where TDocket : WhsComponentOrder
	{
		#region Unused Tests

		public override void TestOnDeleteByInventory()
		{
			Assert("Suppress Exception - Component Order lines don't use Inventory", true);
		}

		// Base Test doesn't setup properly for Component Orders.
		protected override bool UsesBondedEntryKey => false;

		protected override void TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccessCore()
		{
			Assert("Calculation in one DB hit for all lines is not yet supported by this Docket type.", true);
		}

		protected override void TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccess_OnlyCalledWhenLineInDBCore()
		{
			Assert("Calculation in one DB hit for all lines is not yet supported by this Docket type.", true);
		}

		#endregion

		#region Customs

		protected override void SetDocketToCustomsTransaction(TDocket docket)
		{
			Helper.CreateArea(docket.Warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			docket.WD_IsInwardsProcessingJob = true;
		}

		#endregion

		#region TestIsUpdateForOrderPickingStatusAllowed

		protected override void TestIsUpdateForOrderPickingStatusAllowedCore()
		{
			var workOrderLine = GetNewBusinessObject();
			AssertEquals("Unfinalised WorkOrderLine true", true, workOrderLine.IsUpdateForOrderPickingStatusAllowed());

			workOrderLine.WE_DocketLineStatus = DocketStatus.Codes.Finalised;
			workOrderLine.WE_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Finalised WorkOrderLine false", false, workOrderLine.IsUpdateForOrderPickingStatusAllowed());
		}

		#endregion

		#region TestCloneLineNoAndSubLineNo

		protected override void TestCloneLineNoAndSubLineNo(WhsDocketLine docketLine)
		{
			// don't call base as LineNo is not cloned for work order lines.
			var docket = docketLine.Docket;
			docketLine.WE_LineNo = 4;
			docketLine.WE_SubLineNo = 3;

			var clone1 = (WhsDocketLine)docketLine.Clone();
			AssertEquals("Original SubLineNo should not change", (short)3, docketLine.WE_SubLineNo);
			AssertEquals("Cloned SubLineNo should not be copied", (short)0, clone1.WE_SubLineNo);
			AssertEquals("Cloned LineNo should not be copied", (short)0, clone1.WE_LineNo);
		}

		#endregion

		#region TestCanCrossDockInventory

		protected override bool ExpectedCanCrossDockInventory => false;

		#endregion

		#region TestWE_CrossDockQuantity

		protected override ZDecimal ExpectedCrossDockQuantity => ZDecimal.Zero;

		#endregion

		#region TestIsComponentLineOnSalesOrder

		protected override bool IsChildOrderLineAComponentLineOnSalesOrder => false;

		#endregion

		#region TestDelete_DoesNotRecalculateTotalWeightAndVolumeCore

		protected override sealed void TestDelete_DoesNotRecalculateTotalWeightAndVolumeCore()
		{
			TestDelete_DoesNotRecalculateTotalWeightAndVolume(type: WorkOrderType.Codes.Assemble);
		}

		public void TestDelete_DoesNotRecalculateTotalWeightAndVolume_Disassembly()
		{
			TestDelete_DoesNotRecalculateTotalWeightAndVolume(type: WorkOrderType.Codes.Disassemble);
		}

		protected abstract void TestDelete_DoesNotRecalculateTotalWeightAndVolume(ZString type);

		#endregion

		#region TestWE_WD_UpdatingTotalsCore

		protected override sealed void TestWE_WD_UpdatingTotalsCore()
		{
			TestWE_WD_UpdatingTotals(type: WorkOrderType.Codes.Assemble);
		}

		public void TestWE_WD_UpdatingTotals_Disassembly()
		{
			TestWE_WD_UpdatingTotals(type: WorkOrderType.Codes.Disassemble);
		}

		protected abstract void TestWE_WD_UpdatingTotals(ZString type);

		#endregion

		#region TestWE_TransactionQuantity_UpdatesTotalWeightAndVolumeCore

		protected override sealed void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolumeCore()
		{
			TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume(type: WorkOrderType.Codes.Assemble);
		}

		public void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_Disassembly()
		{
			TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume(type: WorkOrderType.Codes.Disassemble);
		}

		protected abstract void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume(ZString type);

		#endregion
	}
}
