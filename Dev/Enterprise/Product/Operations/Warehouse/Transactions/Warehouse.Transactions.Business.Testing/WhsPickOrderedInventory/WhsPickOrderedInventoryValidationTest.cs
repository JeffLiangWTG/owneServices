using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickOrderedInventoryValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestValidateQuantityOrdered

		public void TestValidateQuantityOrdered()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			WhsPick pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);

			line.WE_TransactionQuantity = -10; // fudge
			WhsPickOrderedInventory orderedInventory = pick.OrderedInventories[0];

			orderedInventory.Validation.ValidateQuantityOrdered();
			AssertHasError(orderedInventory.QuantityOrderedInfo, "Quantity Ordered must be greater than or equal to zero");
		}

		#endregion

		#region TestValidatePickLineQuantity

		public void TestValidatePickLineQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();

			var orderedInventory = pick.OrderedInventories[0];
			orderedInventory.AvailableInventories[0].PickLines.ElementAt(0).WZ_Units = -10m; // hacking PickedQuantity
			orderedInventory.Validation.ValidatePickLineQuantity();
			AssertHasError(orderedInventory.PickLineQuantityInfo, "Quantity Allocated must be greater than or equal to zero");

			orderedInventory.AvailableInventories[0].PickLines.ElementAt(0).WZ_Units = 20m; // hacking PickedQuantity
			orderedInventory.Validation.ValidatePickLineQuantity();
			AssertHasError(orderedInventory.PickLineQuantityInfo, "Quantity Allocated can not be greater than Units Ordered. Deallocate some stock in the Inventory grid to reduce this value.");

			orderedInventory.AvailableInventories[0].PickLines.ElementAt(0).WZ_Units = 10m; // hacking PickedQuantity
			orderedInventory.Validation.ValidatePickLineQuantity();
			AssertNoErrors(orderedInventory.PickLineQuantityInfo);

			// When pick is finalised no validation should be run.
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals(true, order.IsFinalised);
			AssertEquals(true, pick.IsFinalised);

			((IBusinessObjectInternals)orderedInventory.AvailableInventories[0].PickLines.ElementAt(0)).Row[WhsPickLineSchema.Constants.WZ_Units] = 20m; // hacking PickedQuantity
			orderedInventory.Validation.ValidatePickLineQuantity();
			AssertNoErrors(orderedInventory.PickLineQuantityInfo);
		}

		#endregion

		#region TestValidateQuantityShort

		public void TestValidateQuantityShort()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			WhsPick pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();

			WhsPickOrderedInventory orderedInventory = pick.OrderedInventories[0];
			orderedInventory.Validation.ValidateQuantityShort();
			AssertNoWarnings(orderedInventory.QuantityShortInfo);

			string errMsg = "This item is short by " + data.Product1.FormattedQtyAndUnit(1m) + ". Allocate more stock in the Inventory grid to bring this value to zero.";
			orderedInventory.AvailableInventories[0].PickLineQuantity -= 1m;
			orderedInventory.Validation.ValidateQuantityShort();
			AssertHasWarning(orderedInventory.QuantityShortInfo, errMsg);

			orderedInventory.AvailableInventories[0].PickLineQuantity += 1m;
			orderedInventory.Validation.ValidateQuantityShort();
			AssertNoWarnings(orderedInventory.QuantityShortInfo);
		}

		#endregion

		#region TestValidateQuantityShortValidationWarningMessage

		public void TestValidateQuantityShortValidationWarningMessage()
		{
			WhsPickOrderedInventory orderedInventory = GetNewBusinessObject();
			orderedInventory.ValidationQuantityShortWarningMessage = "XXX";
			AssertHasWarning(orderedInventory.QuantityShortInfo, "XXX");

			orderedInventory.ValidationQuantityShortWarningMessage = "";
			AssertNoWarnings(orderedInventory.QuantityShortInfo);
		}

		#endregion

		#region Implementation

		protected virtual WhsPickOrderedInventory GetNewBusinessObject()
		{
			return new WhsPickOrderedInventory(Factory);
		}

		#endregion
	}
}
