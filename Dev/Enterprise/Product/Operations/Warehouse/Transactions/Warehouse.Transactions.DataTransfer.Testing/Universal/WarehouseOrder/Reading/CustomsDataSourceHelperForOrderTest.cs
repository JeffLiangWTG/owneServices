using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class CustomsDataSourceHelperForOrderTest : CustomsDataSourceHelperTest<CustomsDataSourceHelperForOrder, WhsOrder>
	{
		#region TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_WhenPickIsNotFinalised

		public void TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_WhenPickIsNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				"Cannot Import Order\r\nOnly finalized Orders with finalized Picks in a Virtual Warehouse can have their attached inventory amended when being Canceled out.",
				() => GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext).CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(order));
		}

		#endregion

		#region TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_SuccessfulAmendment

		protected override void AssertCancelledDocket(WhsOrder order, IEnumerable<WhsInventoryView> amendedInventory)
		{
			var inventory = amendedInventory.Single();
			AssertEquals("Order had 10 units and was cancelled, should put 10 units back into inventory.", 10m, inventory.WI_TotalUnits);
			AssertEquals("Order had 10 units and was cancelled, ensure no adjustment was created (InDocketLine would be 0 if adjusted).", 10m, inventory.WI_InDocketLineUnits);
			AssertEquals("Inventory on the ORIGINAL receipt should have been amended (as opposed to creating a new receipt).", "R1", inventory.InDocketLine.ReceiptReference);

			AssertEquals(false, order.IsFinalised);
			AssertNull(order.Pick);
			AssertEquals("Cancelled order should have all PickLines removed.", 0, order.Lines.Cast<WhsOrderLine>().SelectMany(l => l.PickLines).Count());

			var pickQuery = new ZQuery();
			pickQuery.AddToFilter(WhsPickSchema.WP_WW_Whs, order.WD_WW_Whs);

			var cancelledPick = Factory.Load<WhsPick>(pickQuery).Single();
			AssertEquals(true, cancelledPick.IsCancelled);
		}

		#endregion

		#region TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_SuccessfulAmendment_WithStockCreatedFromAdjustment

		public void TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_SuccessfulAmendment_WithStockCreatedFromAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "123");
			Factory.Save();

			// adjust in 5 more units
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5m, data.Whs1.DefaultLocation, "123", "", 0m);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			order.WD_DocketSubType = "CUS";
			order.Lines[0].WE_BondedEntryKey = "123";
			order.Lines[0].CustomsData.WB_EntryKey = "DummyOutward-1";
			Helper.CreatePickNew(true, true, order);
			AssertEquals("Precondition: All Stock was picked.", true, order.Lines[0].PickLines.All(p => p.Inventory.WI_TotalUnits == 0m));

			var customsHelper = GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext);
			var amendedInventory = customsHelper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(order);
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, adjustment }, amendedInventory.Select(i => i.Docket));

			var adjustmentInventory = amendedInventory.Single(i => i.Docket == adjustment);
			var receiveInventory = amendedInventory.Single(i => i.Docket == receive);
			AssertEquals("Inventory should be Reverted.", 10m, receiveInventory.WI_TotalUnits);
			AssertEquals("Inventory should be Reverted.", 5m, adjustmentInventory.WI_TotalUnits);
			AssertEquals("Order should be Cancelled successfully.", true, order.IsCancelled);
		}

		#endregion

		#region TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_FailedAmendment

		protected override void TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_FailedAmendmentCore(WhsOrder order)
		{
			// hack this to put back too much stock
			order.Lines[0].PickLines[0].Inventory.WI_TotalUnits = 5m;
			var helper = GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				"Cannot Import Order\r\nFailed to put back inventory when attempting to Customs-Cancel Order O1. Amendment has been rejected.",
				() => helper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(order));
		}

		#endregion

		#region TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_NotVirtualWarehouseOrImportingChangeOfInventory

		public void TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_NotVirtualWarehouseOrImportingChangeOfInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			order.Lines[0].PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;

			// Virtual Bonded Warehouse or Change of Inventory will not create In-Transit Transfers
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("Precondition", false, order.IsImportingForChangeOfInventory);
			AssertEquals("Precondition", false, data.Whs1.WW_IsVirtualWarehouse);

			var helper = GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
					"Cannot Import Order\r\nYou can only cancel a pick in a Virtual Warehouse or for Change of Inventory orders.",
					() => helper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(order));
		}

		#endregion

		#region Implementation

		protected override CustomsDataSourceHelperForOrder GetNewCustomsHelper(UniversalShipment topLevelDataObject, IDataContextDataObject topLevelDataContext)
		{
			return new CustomsDataSourceHelperForOrder(topLevelDataObject, topLevelDataContext);
		}

		protected override RecipientRoleType GetRecipientRoleType() => RecipientRoleType.BWR;

		protected override string ExpectedDocketType => "Order";

		protected override WhsOrder GetFinalisedDocket(TestDataSimpleEnvironment data)
		{
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "123");
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_DocketSubType = "CUS";
			order.Lines[0].WE_BondedEntryKey = "123";
			order.Lines[0].CustomsData.WB_EntryKey = "DummyOutward-1";
			Helper.CreatePickNew(true, true, order);

			return order;
		}

		#endregion
	}
}
