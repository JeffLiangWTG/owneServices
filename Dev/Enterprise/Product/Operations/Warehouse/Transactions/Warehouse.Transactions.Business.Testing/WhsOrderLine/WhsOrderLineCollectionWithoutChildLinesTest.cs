using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderLineCollectionWithoutChildLines))]
	class WhsOrderLineCollectionWithoutChildLinesTest : WhsOrderLineCollectionTest<WhsOrderLineCollectionWithoutChildLines>
	{
		#region TestOrderLineCollectionForGUI

		public void TestOrderLineCollectionForGUI()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct1 = Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 3m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			var bomPartForMainProduct2 = Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 9m, Constants.PkgUnit.Unit);
			Factory.Save();

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			AssertEquals("1 order should be added to Pick", 1, pick.Orders.Count);
			AssertEquals("0 additional lines should be added to order; component lines created after allocation", 1, pick.Orders[0].Lines.Count);

			var orderCollectionForGUI = new WhsOrderLineCollectionWithoutChildLines(order);
			AssertEquals("The collection should contain 1 order", 1, orderCollectionForGUI.Count);

			var orderCollection = new WhsOrderLineCollection(order);
			AssertEquals("The collection should contain 1 order", 1, orderCollection.Count);
		}

		#endregion

		#region Implementation

		protected override WhsOrderLineCollectionWithoutChildLines GetCollectionToTest()
		{
			return new WhsOrderLineCollectionWithoutChildLines(Factory.NewWithValidTestData<WhsOrder>());
		}

		#endregion
	}
}
