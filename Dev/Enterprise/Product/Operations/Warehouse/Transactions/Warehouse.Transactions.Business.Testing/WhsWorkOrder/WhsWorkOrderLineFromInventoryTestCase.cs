using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Common.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsWorkOrderLineFromInventoryTestCase : DocketLineFromInventoryHelperTest<WhsWorkOrder, WhsWorkOrderLine>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PickableDocketLineFromInventoryHelper<WhsWorkOrderLine>(new TestNotificationBuffer(), null));
		}

		#endregion

		#region TestHasProductUnitsOrAttribsChangedIsSet

		public void TestHasProductUnitsOrAttribsChangedIsSet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "O1");

			var inventory = receive.Lines[0].Inventory[0];

			var docketLineFromInventoryHelper = GetInventoryHelper(order);
			var hasProductUnitsOrAttribsBeenSet = false;
			var orderLine1 = order.Lines.AddNew();
			orderLine1.WE_OPInfo.ValueChanged += (sender, e) => hasProductUnitsOrAttribsBeenSet |= orderLine1.Shortfall.HasProductUnitsOrAttribsChanged;
			orderLine1.WE_TransactionQuantityInfo.ValueChanged += (sender, e) => hasProductUnitsOrAttribsBeenSet |= orderLine1.Shortfall.HasProductUnitsOrAttribsChanged;
			docketLineFromInventoryHelper.SetDocketLineFromInventory(orderLine1, inventory.InDocketLine, ExcludeFromCopy.None);
			AssertEquals("While setting orderline fields, shortfall property should have been false.", false, hasProductUnitsOrAttribsBeenSet);
			AssertEquals("After setting, all fields, it should be set to true.", true, orderLine1.Shortfall.HasProductUnitsOrAttribsChanged);

			WhsPickableDocketLine orderLine2 = null;

			using (order.ShortfallManager.DeferMarkingLinesAsShortfallPropertiesChanged())
			{
				orderLine2 = (WhsPickableDocketLine)docketLineFromInventoryHelper.CreateDocketLineFromInventory(order.Lines, inventory);
				AssertEquals(false, orderLine2.Shortfall.HasProductUnitsOrAttribsChanged);
			}

			AssertEquals(true, orderLine2.Shortfall.HasProductUnitsOrAttribsChanged);
		}

		#endregion

		protected override bool CanBeCustomsJob => false;

		protected override WhsWorkOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			workOrder.WD_WW_Whs = warehouse.PK;
			workOrder.WD_OH_Client = client.PK;

			return workOrder;
		}

		protected override WhsWorkOrderLine GetNewDocketLine(WhsWorkOrder docket, OrgSupplierPart part, WhsLocation location)
		{
			var workOrderLine = Helper.CreateWhsWorkOrderLine(docket, part, 10m);
			workOrderLine.RunPreSaveValidation(); // to commit inventory

			return workOrderLine;
		}

		protected override void AssertLocations(WhsDocketLine line, WhsInventoryView inventory)
		{
			// Locations not used by Order
		}

		protected override DocketLineFromInventoryHelper<WhsWorkOrderLine> GetInventoryHelper(WhsDocket docket)
		{
			return new PickableDocketLineFromInventoryHelper<WhsWorkOrderLine>(docket?.NotificationSubscriber, (WhsPickableDocket)docket);
		}
	}
}
