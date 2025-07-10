using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsRMAOrderLineValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestQuantityToReturn_ShouldNotNegative

		public void TestQuantityToReturn()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10001m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10001m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var rmaOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(receive.Lines[0], order, 10001m, 10001m);
			TestMinDecimal(rmaOrderLine.QuantityToReturnInfo, ErrorCheckType.HasErrors, 0);
		}

		#endregion

		#region TestQuantityToReturn_EqualToZero

		public void TestQuantityToReturn_EqualToZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10001m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var rmaOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(receive.Lines[0], order, 10001m, 10001m);

			AssertEquals("Precondition:", 0m, rmaOrderLine.QuantityToReturn);
			AssertHasWarning(rmaOrderLine.QuantityToReturnInfo, "Will not generate RMA Receive Line for this line when amount equal 0");
		}

		#endregion

		#region TestQuantityToReturn_MustLessThanOrEqualAvailableQuantity

		public void TestQuantityToReturn_MustLessThanOrEqualAvailableQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var rmaOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(receive.Lines[0], order, 10m, 8m);
			rmaOrderLine.QuantityToReturn = 5m;
			AssertNoErrors("Precondition: No Errors", rmaOrderLine.QuantityToReturnInfo);

			rmaOrderLine.QuantityToReturn = 10m;
			AssertHasError(rmaOrderLine.QuantityToReturnInfo, "Please enter an amount less than or equal to the Available Quantity");

			rmaOrderLine.QuantityToReturn = 8m;
			AssertNoErrors("Precondition: No Errors", rmaOrderLine.QuantityToReturnInfo);
		}

		#endregion

		#region TestValidateWhsOverride

		public void TestValidateWhsOverride()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateWarehouse("W2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var rmaOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(receive.Lines[0], order, 10m, 10m);
			rmaOrderLine.WhsOverride = ZGuid.NewZGuid();
			AssertHasError(rmaOrderLine.WhsOverrideInfo, "Enter a valid Warehouse Override.");

			rmaOrderLine.WhsOverride = data.Whs1.PK;
			AssertHasError(rmaOrderLine.WhsOverrideInfo, "Do not enter the same warehouse as on the Order. Leave this blank to generate RMA for the original Warehouse");

			rmaOrderLine.WhsOverride = warehouse.PK;
			AssertNoError(rmaOrderLine.WhsOverrideInfo, "Enter a valid Warehouse Override.");
		}

		#endregion

		#region TestAutoValidationType

		public void TestAutoValidationType()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var rmaOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(receive.Lines[0], order, 10m, 10m);
			AssertEquals(typeof(WhsRMAOrderLineValidation), rmaOrderLine.Validation.AutoValidationType);
		}

		#endregion

	}
}
