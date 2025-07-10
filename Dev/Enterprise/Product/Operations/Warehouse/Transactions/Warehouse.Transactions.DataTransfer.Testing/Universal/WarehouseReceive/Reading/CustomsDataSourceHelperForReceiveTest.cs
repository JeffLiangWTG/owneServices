using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
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
	class CustomsDataSourceHelperForReceiveTest : CustomsDataSourceHelperTest<CustomsDataSourceHelperForReceive, WhsReceive>
	{
		#region TestAdjustingReceiveIsRejectedIfPriorOrderTookStock

		public void TestAdjustingReceiveIsRejectedIfPriorOrderTookStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");
			var bondedLocation = data.Whs1.Areas[0].PickLocations[0];

			// receive stock in
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "123-1");
			receive.Inventory[0].WI_WL = bondedLocation.PK;
			Factory.Save();

			// release some of its stock
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m, "123-1", "DummyOutward-1", "");
			Helper.CreatePickNew(true, true, order);
			Factory.Save();

			var customsHelper = GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext);

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				"Cannot Import Receipt\r\nCannot amend Receipt W00000001 as some of its stock has been released.",
				() => customsHelper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(receive));
		}

		#endregion

		#region TestAdjustingReceiveIsNotRejectedIfPriorOrderThatTookStockIsCancelled

		public void TestAdjustingReceiveIsNotRejectedIfPriorOrderThatTookStockIsCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");

			// receive stock in
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "123-1");
			Factory.Save();

			// release some of its stock
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m, "123-1", "DummyOutward-1", "");
			Helper.CreatePickNew(true, true, order);
			Factory.Save();

			new CustomsDataSourceHelperForOrder(ShipmentDataObject, ShipmentDataObject.DataContext).CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(order);
			Factory.Save();

			var customsHelper = GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext);
			AssertNoExceptionThrown(() => customsHelper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(receive));
		}

		#endregion

		#region TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_SuccessfulAmendment

		protected override void AssertCancelledDocket(WhsReceive receive, IEnumerable<WhsInventoryView> amendedInventory)
		{
			AssertEquals(0, amendedInventory.Count());

			var adjustmentQuery = new ZQuery();
			adjustmentQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Adjustment);
			adjustmentQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, "R1");
			adjustmentQuery.OrderBy = WhsDocketSchema.Constants.WD_ExternalReferenceSplit + OrderByClause.Descending;

			var adjustment = Factory.LoadTop1<WhsAdjustment>(adjustmentQuery);
			AssertEquals(true, adjustment.IsFinalised);
			AssertEquals(ZByte.Zero, adjustment.WD_ExternalReferenceSplit);
		}

		#endregion

		#region TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_FailedAmendment

		protected override void TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_FailedAmendmentCore(WhsReceive receive)
		{
			// hack to make adjustment finalise fail
			var finalisedInventory = (WhsInventoryView)receive.Inventory.Single();
			finalisedInventory.WI_TotalUnits = 5m;

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				string.Format(@"Cannot Import Receipt
Receipt could not be finalized into the Warehouse for Customs Job B123 because of the following AMENDMENT error(s):
Error - WE_TransactionQuantity: Attempted to adjust 10 Units, but only 5 Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number."),
				() => GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext).CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(receive));
		}

		#endregion

		#region TestAdjustingReceiveIsRejectedIfReceiveIsInvalidForAdjustingOut

		public void TestAdjustingReceiveIsRejectedIfReceiveIsInvalidForAdjustingOut()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");
			var bondedLocation = data.Whs1.Areas[0].PickLocations[0];

			// receive stock in
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "123-1");
			receive.Inventory[0].WI_WL = bondedLocation.PK;
			Factory.Save();
			var customsHelper = GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext);

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"Cannot Import Receipt
Only Bonded Dockets that are Finalized (in virtual warehouse or for change of ownership) can be Adjusted out.",
				() => customsHelper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(receive));
		}

		#endregion

		#region Implementation

		protected override CustomsDataSourceHelperForReceive GetNewCustomsHelper(UniversalShipment topLevelDataObject, IDataContextDataObject topLevelDataContext)
		{
			return new CustomsDataSourceHelperForReceive(topLevelDataObject, topLevelDataContext);
		}

		protected override WhsReceive GetFinalisedDocket(TestDataSimpleEnvironment data)
		{
			return Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "123");
		}

		protected override RecipientRoleType GetRecipientRoleType() => RecipientRoleType.BWI;

		protected override string ExpectedDocketType => "Receipt";

		#endregion
	}
}
