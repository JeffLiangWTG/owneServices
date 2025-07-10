using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveLineValidationTest : WhsDocketLineValidationTestCase<WhsReceiveLine, WhsReceive>
	{
		#region TestCheckWE_TransactionQuantity

		#region TestCheckWE_TransactionQuantity

		protected override void TestCheckWE_TransactionQuantityCore()
		{
			base.TestCheckWE_TransactionQuantityCore();

			var expectedErrorMessage = "Quantity must be greater than or equal to zero. Otherwise delete this row";
			var whs = Helper.CreateWarehouse("WHS");
			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 1);
			var client = Helper.CreateClient("ORG1", "ORG1");
			var product = Helper.CreateProduct(client, "P1");
			var receive = Helper.CreateWhsReceive(client, whs);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, product, 0);

			TestMinDecimal(receiveLine.WE_TransactionQuantityInfo, ErrorCheckType.HasErrors, 0);
			receiveLine.WE_TransactionQuantity = -1m;
			AssertHasError(receiveLine.WE_TransactionQuantityInfo, expectedErrorMessage);
			receiveLine.WE_TransactionQuantity = 0m;
			AssertHasWarning(receiveLine.WE_TransactionQuantityInfo, "Quantity is equal to zero. Please remove the line unless recording an under.");
			receiveLine.WE_TransactionQuantity = 1m;
			AssertNoWarnings(receiveLine.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_LessThanReservedQuantity

		public void TestCheckWE_TransactionQuantity_LessThanReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			receiveLine.WE_ClientOrderedUnits = 10m;
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50);
			Helper.CreateReservePickLine(orderLine, inventory, 50);

			AssertEquals("Pre-condition", 1, receive.Inventory.Count);
			AssertEquals("Pre-condition", 50m, receive.Inventory[0].WI_CrossDockQuantity);

			receiveLine.WE_TransactionQuantity = 20m;
			AssertHasError(receiveLine.WE_TransactionQuantityInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);

			receiveLine.WE_TransactionQuantity = 50m;
			AssertNoErrors(receiveLine.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_LessThanReservedQuantity_ClientOrderedUnitsAsReservedQuantity

		public void TestCheckWE_TransactionQuantity_LessThanReservedQuantity_ClientOrderedUnitsAsReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			receiveLine.WE_ClientOrderedUnits = 50m;
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50);
			Helper.CreateReservePickLine(orderLine, inventory, 50);

			AssertEquals("Pre-condition", 1, receive.Inventory.Count);
			AssertEquals("Pre-condition", 50m, receive.Inventory[0].WI_CrossDockQuantity);

			receiveLine.WE_TransactionQuantity = 10m;
			AssertNoErrors(receiveLine.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_LessThanReservedQuantity_ErrorClearedWhenClientOrderedUnitsBecomeReservedQuantity

		public void TestCheckWE_TransactionQuantity_LessThanReservedQuantity_ErrorClearedWhenClientOrderedUnitsBecomeReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			receiveLine.WE_ClientOrderedUnits = 10m;
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50);
			Helper.CreateReservePickLine(orderLine, inventory, 50);

			AssertEquals("Pre-condition", 1, receive.Inventory.Count);
			AssertEquals("Pre-condition", 50m, receive.Inventory[0].WI_CrossDockQuantity);

			receiveLine.WE_TransactionQuantity = 20m;
			AssertHasError(receiveLine.WE_TransactionQuantityInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
			AssertNoErrors(receiveLine.WE_ClientOrderedUnitsInfo);

			receiveLine.WE_ClientOrderedUnits = 50m;
			AssertNoErrors(receiveLine.WE_TransactionQuantityInfo);
			AssertNoErrors(receiveLine.WE_ClientOrderedUnitsInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_LessThanReservedQuantity_EqualToClientOrderedUnits

		public void TestCheckWE_TransactionQuantity_LessThanReservedQuantity_EqualToClientOrderedUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			receiveLine.WE_ClientOrderedUnits = 10m;
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50);
			Helper.CreateReservePickLine(orderLine, inventory, 50);

			AssertEquals("Pre-condition", 1, receive.Inventory.Count);
			AssertEquals("Pre-condition", 50m, receive.Inventory[0].WI_CrossDockQuantity);

			receiveLine.WE_TransactionQuantity = 10m;
			AssertHasError(receiveLine.WE_ClientOrderedUnitsInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
			AssertHasError(receiveLine.WE_TransactionQuantityInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
		}

		#endregion

		#region TestCheckWE_TransactionQuantitySerialNumberQtyCheck

		public void TestCheckWE_TransactionQuantitySerialNumberQtyCheck()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithoutError = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, null, ZDate.Empty, ZDate.Empty, "", "", "", "").InDocketLine;
			receiveLineWithoutError.WE_SerialNumber = "SN01";
			var receiveLineWithoutError2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2, null, ZDate.Empty, ZDate.Empty, "", "", "", "").InDocketLine;
			var receiveLineWithError = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2, null, ZDate.Empty, ZDate.Empty, "", "", "", "").InDocketLine;
			receiveLineWithError.WE_SerialNumber = "SN02";

			receive.RunPreSaveValidation();

			AssertNoErrors(receiveLineWithoutError.WE_TransactionQuantityInfo);
			AssertNoErrors(receiveLineWithoutError2.WE_TransactionQuantityInfo);
			AssertHasError(receiveLineWithError.WE_TransactionQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		public void TestCheckWE_TransactionQuantitySerialNumberQtyCheck_PivotSerialNumber()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1);
				receiveLine1.SerialNumbers.AddNew().SerialNumberValue = "SN01";
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2);
				var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2);
				receiveLine3.SerialNumbers.AddNew().SerialNumberValue = "SN02";

				receive.RunPreSaveValidation();

				AssertNoErrors(receiveLine1.WE_TransactionQuantityInfo);
				AssertHasError(receiveLine2.WE_TransactionQuantityInfo,
					"The number of entered serial numbers 0 does not match the required quantity 2. Please ensure the serial numbers match the specified quantity.");
				AssertHasError(receiveLine3.WE_TransactionQuantityInfo,
					"The number of entered serial numbers 1 does not match the required quantity 2. Please ensure the serial numbers match the specified quantity.");

				receiveLine3.SerialNumbers.AddNew().SerialNumberValue = "SN02";
				receive.RunPreSaveValidation();
				AssertNoErrors(receiveLine3.WE_TransactionQuantityInfo);
			}
		}

		#endregion

		public void TestCheckWE_TransactionQuantity_ReturnReceive_TotalsOverTheMax()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			var returnReceiveLine1 = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 10m);
			var returnReceiveLine2 = Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 20m);
			returnReceive.WD_WD_ParentDocket = order.PK;

			AssertNoErrors(returnReceiveLine1.WE_TransactionQuantityInfo);
			AssertNoErrors(returnReceiveLine2.WE_TransactionQuantityInfo);

			returnReceiveLine1.WE_TransactionQuantity = 15m;
			AssertNoErrors(returnReceiveLine1.WE_TransactionQuantityInfo);

			returnReceiveLine1.RunPreSaveValidation();
			AssertHasError(returnReceiveLine1.WE_TransactionQuantityInfo, "Quantity is more than the available quantity to return for the released inventory from the returned order.");

			returnReceiveLine1.WE_TransactionQuantity = 10m;
			returnReceiveLine2.WE_TransactionQuantity = 21m;
			AssertNoErrors(returnReceiveLine2.WE_TransactionQuantityInfo);

			returnReceiveLine2.RunPreSaveValidation();
			AssertHasError(returnReceiveLine2.WE_TransactionQuantityInfo, "Quantity is more than the available quantity to return for the released inventory from the returned order.");
		}

		public void TestCheckWE_TransactionQuantity_ReturnReceive_TotalsOverTheMax_WithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Factory.Save();

			var inventory = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(inventory, data.Part1.PK, 10m, data.Whs1.DefaultLocation.PK, "PLT-1", ZDate.Empty, ZDate.Empty, "A", "B", "", "", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(inventory, data.Part1.PK, 15m, data.Whs1.DefaultLocation.PK, "PLT-1", ZDate.Empty, ZDate.Empty, "C", "B", "", "", "");
			inventory.AllocateLocationsWithMock();
			inventory.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 25m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive.WD_WD_ParentDocket = order.PK;
			var returnReceiveLine1 = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "A", "B", "", "");
			var returnReceiveLine2 = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 15m, ZDate.Empty, ZDate.Empty, "C", "B", "", "");

			AssertNoErrors(returnReceiveLine1.WE_TransactionQuantityInfo);
			AssertNoErrors(returnReceiveLine2.WE_TransactionQuantityInfo);

			returnReceiveLine1.WE_TransactionQuantity = 15m;
			AssertNoErrors(returnReceiveLine1.WE_TransactionQuantityInfo);

			returnReceiveLine1.RunPreSaveValidation();
			AssertHasError(returnReceiveLine1.WE_TransactionQuantityInfo, "Quantity is more than the available quantity to return for the released inventory from the returned order.");

			returnReceiveLine2.WE_TransactionQuantity = 16m;
			AssertNoErrors(returnReceiveLine2.WE_TransactionQuantityInfo);

			returnReceiveLine2.RunPreSaveValidation();
			AssertHasError(returnReceiveLine2.WE_TransactionQuantityInfo, "Quantity is more than the available quantity to return for the released inventory from the returned order.");
		}

		public void TestCheckWE_TransactionQuantity_ReturnReceive_TotalsOverTheMax_MultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			var returnReceiveLine1 = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 10m);
			var returnReceiveLine2 = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 5m);
			returnReceive.WD_WD_ParentDocket = order.PK;

			AssertNoErrors(returnReceiveLine1.WE_TransactionQuantityInfo);
			AssertNoErrors(returnReceiveLine2.WE_TransactionQuantityInfo);

			returnReceiveLine1.WE_TransactionQuantity = 12m;
			AssertNoErrors(returnReceiveLine2.WE_TransactionQuantityInfo);

			returnReceiveLine1.RunPreSaveValidation();
			AssertHasError(returnReceiveLine1.WE_TransactionQuantityInfo, "Quantity is more than the available quantity to return for the released inventory from the returned order.");

			returnReceiveLine2.RunPreSaveValidation();
			AssertHasError(returnReceiveLine2.WE_TransactionQuantityInfo, "Quantity is more than the available quantity to return for the released inventory from the returned order.");
		}

		public void TestCheckWE_TransactionQuantity_ReturnReceive_TotalsOverTheMax_ReturningProductThatWasNotOrdered()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive.WD_WD_ParentDocket = order.PK;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 10m);
			AssertNoErrors(returnReceiveLine.WE_TransactionQuantityInfo);

			returnReceiveLine.RunPreSaveValidation();
			AssertHasError(returnReceiveLine.WE_TransactionQuantityInfo, "Quantity is more than the available quantity to return for the released inventory from the returned order.");
		}

		public void TestCheckWE_TransactionQuantity_ReturnReceive_NoOrderToReturn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "SomeRandomNumber");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			var returnReceiveLine1 = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 10m);
			var returnReceiveLine2 = Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 20m);
			AssertNull("Precondition", returnReceive.OrderToReturn);

			AssertNoErrors(returnReceiveLine1.WE_TransactionQuantityInfo);
			AssertNoErrors(returnReceiveLine2.WE_TransactionQuantityInfo);

			returnReceiveLine1.WE_TransactionQuantity = 15m;
			returnReceiveLine1.RunPreSaveValidation();
			AssertNoErrors(returnReceiveLine1.WE_TransactionQuantityInfo);

			returnReceiveLine2.WE_TransactionQuantity = 21m;
			returnReceiveLine2.RunPreSaveValidation();
			AssertNoErrors(returnReceiveLine1.WE_TransactionQuantityInfo);
		}

		public void TestCheckWE_TransactionQuantity_ReturnReceive_TotalsOverTheMax_StartedReceiving_NotInDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation);
			returnReceive.WD_WD_ParentDocket = order.PK;
			returnReceive.WD_StartedReceivingTimeUtc = DateTime.UtcNow;

			Assert("Precondition", !returnReceiveLine.IsInDatabase);
			Assert("Precondition", returnReceiveLine.WE_ClientOrderedUnitsInfo.ReadOnly);
			Assert("Precondition", !returnReceiveLine.WE_TransactionQuantityInfo.ReadOnly);

			AssertNoErrors(returnReceiveLine.WE_TransactionQuantityInfo);
			returnReceiveLine.WE_TransactionQuantity = 15m;
			returnReceiveLine.RunPreSaveValidation();
			AssertHasError(returnReceiveLine.WE_TransactionQuantityInfo, "Quantity is more than the available quantity to return for the released inventory from the returned order.");

			returnReceiveLine.WE_TransactionQuantity = 10m;
			AssertNoErrors(returnReceiveLine.WE_TransactionQuantityInfo);
		}

		public void TestCheckWE_TransactionQuantity_ReturnReceive_TotalsOverTheMax_StartedReceiving_InDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation);
			returnReceive.WD_WD_ParentDocket = order.PK;
			returnReceive.WD_StartedReceivingTimeUtc = DateTime.UtcNow;
			Factory.Save();

			Assert("Precondition", returnReceiveLine.IsInDatabase);
			Assert("Precondition", returnReceiveLine.WE_ClientOrderedUnitsInfo.ReadOnly);
			Assert("Precondition", returnReceiveLine.WE_TransactionQuantityInfo.ReadOnly);

			AssertNoErrors(returnReceiveLine.WE_TransactionQuantityInfo);
			returnReceiveLine.WE_TransactionQuantity = 15m;
			returnReceiveLine.RunPreSaveValidation();
			AssertNoErrors(returnReceiveLine.WE_TransactionQuantityInfo);
		}

		public void TestCheckWE_TransactionQuantity_ReturnReceive_TotalsOverTheMax_HasPutawayTransfer_NotInDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 10m);
			returnReceive.WD_WD_ParentDocket = order.PK;
			returnReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;

			Assert("Precondition", !returnReceiveLine.IsInDatabase);
			Assert("Precondition", returnReceiveLine.WE_ClientOrderedUnitsInfo.ReadOnly);
			Assert("Precondition", returnReceiveLine.WE_TransactionQuantityInfo.ReadOnly);

			AssertNoErrors(returnReceiveLine.WE_TransactionQuantityInfo);
			returnReceiveLine.WE_TransactionQuantity = 15m;
			returnReceiveLine.RunPreSaveValidation();
			AssertHasError(returnReceiveLine.WE_TransactionQuantityInfo, "Quantity is more than the available quantity to return for the released inventory from the returned order.");

			returnReceiveLine.WE_TransactionQuantity = 10m;
			AssertNoErrors(returnReceiveLine.WE_TransactionQuantityInfo);
		}

		public void TestCheckWE_TransactionQuantity_ReturnReceive_TotalsOverTheMax_HasPutawayTransfer_InDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 10m);
			returnReceive.WD_WD_ParentDocket = order.PK;
			Factory.Save();

			returnReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;

			Assert("Precondition", returnReceiveLine.IsInDatabase);
			Assert("Precondition", returnReceiveLine.WE_ClientOrderedUnitsInfo.ReadOnly);
			Assert("Precondition", returnReceiveLine.WE_TransactionQuantityInfo.ReadOnly);

			AssertNoErrors(returnReceiveLine.WE_TransactionQuantityInfo);
			returnReceiveLine.WE_TransactionQuantity = 15m;
			returnReceiveLine.RunPreSaveValidation();
			AssertNoErrors(returnReceiveLine.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestFinaliseDocket_CrossDockAllocations

		#region TestFinaliseDocket_CrossDockAllocations_MultipleDocketsWithSingleReceiveLines

		public void TestFinaliseDocket_CrossDockAllocations_MultipleDocketsWithSingleReceiveLines()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);

			var receive1 = Helper.CreateWhsReceive(client, whs, "receive1");
			var receive2 = Helper.CreateWhsReceive(client, whs, "receive2");
			var receive3 = Helper.CreateWhsReceive(client, whs, "receive3");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, product, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, product, 10m);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive3, product, 10m);
			receive1.AllocateLocationsWithMock();
			receive2.AllocateLocationsWithMock();
			receive3.AllocateLocationsWithMock();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, whs, product, 10m);
			order1.Lines[0].ReserveStockIfAbleTo(receive1.Inventory[0], 10m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client, whs, product, 10m);
			order2.Lines[0].ReserveStockIfAbleTo(receive2.Inventory[0], 10m);
			receiveLine2.WE_TransactionQuantity = 15m;

			var order3 = Helper.CreateWhsOrderWithOrderLine(client, whs, product, 10m);
			order3.Lines[0].ReserveStockIfAbleTo(receive3.Inventory[0], 10m);
			receiveLine3.WE_TransactionQuantity = 0m;

			CombineAssertions(delegate
			{
				AssertEquals("Precondition: Order should have 10 stock reserved", 10m, order1.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: Order should have 10 stock reserved", 10m, order2.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: Order should have 10 stock reserved", 10m, order3.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: Receive should have 10 stock expected", 10m, receiveLine1.WE_ClientOrderedUnits);
				AssertEquals("Precondition: Receive should have 10 stock expected", 10m, receiveLine2.WE_ClientOrderedUnits);
				AssertEquals("Precondition: Receive should have 10 stock expected", 10m, receiveLine3.WE_ClientOrderedUnits);
				AssertEquals("Precondition: Receive should have 10 stock received", 10m, receiveLine1.WE_TransactionQuantity);
				AssertEquals("Precondition: Receive should have 15 stock received", 15m, receiveLine2.WE_TransactionQuantity);
				AssertEquals("Precondition: Receive should have no stock received", 0m, receiveLine3.WE_TransactionQuantity);
				AssertNoErrors("Precondition: Receive should have no errors.", receive1);
				AssertNoErrors("Precondition: Receive should have no errors.", receive2);
				AssertNoErrors("Precondition: Receive should have no errors.", receive3);
			});

			receive1.FinaliseDocket();
			AssertNoError(receiveLine1.ReservedQuantityInfo, "This Receive Line has 10 units reserved, you cannot finalize while the received quantity is less than this.");

			receive2.FinaliseDocket();
			AssertNoError(receiveLine2.ReservedQuantityInfo, "This Receive Line has 10 units reserved, you cannot finalize while the received quantity is less than this.");

			receive3.RunPreSaveValidation();
			AssertNoErrorContaining(receiveLine3.ReservedQuantityInfo, "This Receive Line has 10 units reserved, you cannot finalize while the received quantity is less than this.");
			receive3.FinaliseDocket();
			AssertEquals("Receive should not finalise.", false, receive3.IsFinalised);
			AssertHasError("Lines short on stock for Cross docks should create an error.", receiveLine3.ReservedQuantityInfo, "This Receive Line has 10 units reserved, you cannot finalize while the received quantity is less than this.");
		}

		#endregion

		#region TestFinaliseDocket_CrossDockAllocations_MultipleReceiveLinesOnSingleDocket

		public void TestFinaliseDocket_CrossDockAllocations_MultipleReceiveLinesOnSingleDocket()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);

			var receive = Helper.CreateWhsReceive(client, whs, "receive");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, product, 15m);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, product, 20m);
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, product, 25m);
			receive.AllocateLocationsWithMock();

			var order1 = Helper.CreateWhsOrder(client, whs);
			var order2 = Helper.CreateWhsOrder(client, whs);
			var order3 = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order1, product, 10m).ReserveStockIfAbleTo(receive.Inventory[0], 10m);
			Helper.CreateWhsOrderLine(order2, product, 15m).ReserveStockIfAbleTo(receive.Inventory[1], 15m);
			Helper.CreateWhsOrderLine(order3, product, 20m).ReserveStockIfAbleTo(receive.Inventory[2], 20m);
			Helper.CreateWhsOrderLine(order3, product, 25m).ReserveStockIfAbleTo(receive.Inventory[3], 25m);

			receiveLine2.WE_TransactionQuantity = 5m;
			receiveLine4.WE_TransactionQuantity = 5m;

			CombineAssertions(delegate
			{
				AssertEquals("Precondition: Quantity should be reserved for line.", 10m, receive.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: Quantity should be reserved for line.", 15m, receive.Lines[1].ReservedQuantity);
				AssertEquals("Precondition: Quantity should be reserved for line.", 20m, receive.Lines[2].ReservedQuantity);
				AssertEquals("Precondition: Quantity should be reserved for line.", 25m, receive.Lines[3].ReservedQuantity);
				AssertEquals("Precondition: There should be 10 stock reserved", 10m, order1.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: There should be 15 stock reserved", 15m, order2.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: There should be 20 stock reserved", 20m, order3.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: There should be 25 stock reserved", 25m, order3.Lines[1].ReservedQuantity);
				AssertNoErrors("Precondition: Receive should have no errors.", receive);
			});

			receive.FinaliseDocket();
			CombineAssertions(delegate
			{
				AssertEquals("Receive should not finalize.", false, receive.IsFinalised);
				AssertNoErrors("Lines which fulfill Cross docks should not have an error.", receiveLine1.ReservedQuantityInfo);
				AssertHasError("Lines short on stock for Cross docks should have an error.", receiveLine2.ReservedQuantityInfo, string.Format("This Receive Line has {0} units reserved, you cannot finalize while the received quantity is less than this.", 15m));
				AssertNoErrors("Lines which fulfill Cross docks should not have an error.", receiveLine3.ReservedQuantityInfo);
				AssertHasError("Lines short on stock for Cross docks should have an error.", receiveLine4.ReservedQuantityInfo, string.Format("This Receive Line has {0} units reserved, you cannot finalize while the received quantity is less than this.", 25m));
			});
		}

		#endregion

		#region TestFinaliseDocket_CrossDockAllocations_MultipleOrderLinesOnSingleInventoryLine

		public void TestFinaliseDocket_CrossDockAllocations_MultipleOrderLinesOnSingleInventoryLine()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);

			var receive1 = Helper.CreateWhsReceive(client, whs, "receive");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, product, 20m);
			receive1.AllocateLocationsWithMock();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, whs, product, 5m);
			order1.Lines[0].ReserveStockIfAbleTo(receive1.Inventory[0], 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, whs, product, 5m);
			order2.Lines[0].ReserveStockIfAbleTo(receive1.Inventory[0], 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, whs, product, 5m);
			order3.Lines[0].ReserveStockIfAbleTo(receive1.Inventory[0], 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(client, whs, product, 5m);
			order4.Lines[0].ReserveStockIfAbleTo(receive1.Inventory[0], 5m);

			receiveLine1.WE_TransactionQuantity = 5m;

			CombineAssertions(delegate
			{
				AssertEquals("Precondition: Order should have 10 stock reserved", 5m, order1.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: Order should have 10 stock reserved", 5m, order2.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: Order should have 10 stock reserved", 5m, order3.Lines[0].ReservedQuantity);
				AssertEquals("Precondition: Order should have 10 stock reserved", 5m, order4.Lines[0].ReservedQuantity);

				AssertEquals("Precondition: Receive should have 20 stock expected", 20m, receiveLine1.WE_ClientOrderedUnits);
				AssertEquals("Precondition: Receive should have 5 stock received", 5m, receiveLine1.WE_TransactionQuantity);
				AssertNoErrors("Precondition: Receive should have no errors.", receive1);
			});

			receive1.FinaliseDocket();
			AssertEquals("Receive should not finalise.", false, receive1.IsFinalised);
			AssertHasError("Lines short on stock for Cross docks should create an error.", receiveLine1.ReservedQuantityInfo, "This Receive Line has 20 units reserved, you cannot finalize while the received quantity is less than this.");
		}

		#endregion

		#endregion

		#region Product

		#region TestCheckWE_OP

		#region TestFinaliseDocket_ClientPreventReceivingProductsWithNonAttribute

		public void TestFinaliseDocket_ClientPreventReceivingProductsWithoutAttribute()
		{
			var client = Helper.CreateClient("Client");
			client.MiscServ.OM_WhsCheckPartWeightOrDimsOnReceive = "ALL";
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(client, whs, "receive");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, product, 20m);
			receive1.AllocateLocationsWithMock();
			receiveLine1.WE_TransactionQuantity = 5m;

			receive1.FinaliseDocket();
			AssertEquals("Receive should not finalise.", false, receive1.IsFinalised);
			AssertHasError(receiveLine1.WE_OPInfo, "Product cannot be received as the Product Master is missing weight or dimensions.");

			product.OP_Width = 5m;

			receive1.FinaliseDocket();
			AssertEquals("Receive should finalise.", true, receive1.IsFinalised);
			AssertNoError(receiveLine1.WE_OPInfo, "Product cannot be received as the Product Master is missing weight or dimensions.");
		}

		#endregion

		#region TestCheckProductHasWeightDefinition

		public void TestCheckProductHasWeightDefinition()
		{
			var product = Helper.CreateProduct(Helper.CreateClient(), "P1");
			product.OP_Weight = 0m;
			DocketLine.WE_OP = product.PK;
			AssertHasWarning(DocketLine.WE_OPInfo, WhsValidationHelper.ProductHasNoWeightDefinitionError);
			product.OP_Weight = 1m;
			DocketLine.Validation.ValidateWE_OP();
			AssertNoWarning(DocketLine.WE_OPInfo, WhsValidationHelper.ProductHasNoWeightDefinitionError);
		}

		#endregion

		#region TestCheckProductHasCubicDefinition

		public void TestCheckProductHasCubicDefinition()
		{
			var product = Helper.CreateProduct(Helper.CreateClient(), "P1");
			product.OP_Cubic = 0m;
			DocketLine.WE_OP = product.PK;
			AssertHasWarning(DocketLine.WE_OPInfo, WhsValidationHelper.ProductHasNoVolumeDefinitionError);
			product.OP_Cubic = 1m;
			DocketLine.Validation.ValidateWE_OP();
			AssertNoWarning(DocketLine.WE_OPInfo, WhsValidationHelper.ProductHasNoVolumeDefinitionError);
		}

		#endregion

		#region TestCheckProductHasPalletDefinition

		public void TestCheckProductHasPalletDefinition()
		{
			var product = Helper.CreateProduct(Helper.CreateClient(), "P1");
			DocketLine.WE_OP = product.PK;
			AssertHasWarning(DocketLine.WE_OPInfo, WhsValidationHelper.ProductHasNoPalletDefinitionError);
			Helper.CreateProductUnit(product, "PLT", 1m);
			DocketLine.Validation.ValidateWE_OP();
			AssertNoWarning(DocketLine.WE_OPInfo, WhsValidationHelper.ProductHasNoPalletDefinitionError);
		}

		#endregion

		#region TestCheckProductShouldNotBeChangedIfThisInventoryIsReserved

		public void TestCheckProductShouldNotBeChangedIfThisInventoryIsReserved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			Factory.Save();
			var receiveLine = receive.Lines[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(receive.Inventory[0]));
			AssertNoErrors("Precondition", receiveLine.WE_OPInfo);

			receiveLine.WE_OP = data.Part2.PK;
			AssertHasError(receiveLine.WE_OPInfo, WhsValidationHelper.CannotChangeCrossDocketInventoryProduct);

			receiveLine.WE_OP = data.Part1.PK;
			AssertNoErrors(receiveLine.WE_OPInfo);
		}

		#endregion

		#region TestCheckWE_OP_CheckForInvalidProduct_IsNotActive

		public void TestCheckWE_OP_CheckForInvalidProduct_IsNotActive()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_IsActive = false;
			DocketLine.WE_OP = part.PK;
			AssertHasError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);

			DocketLine.WE_OP = ZGuid.Empty;
			part.OP_IsActive = true;
			DocketLine.WE_OP = part.PK;
			AssertNoError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
		}

		#endregion

		#region TestCheckWE_OP_CheckForInvalidProduct_IsInvalid

		public void TestCheckWE_OP_CheckForInvalidProduct_IsInvalid()
		{
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			DocketLine.SupplierPart.OP_PartNum = "TEST";
			DocketLine.Validation.ValidateWE_OP();
			AssertNoError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.InvalidProductErrorMessage);

			part.OP_PartNum = ProductType.Codes.Invalid;
			DocketLine.WE_OP = ZGuid.Empty;
			DocketLine.WE_OP = part.PK;
			DocketLine.Validation.ValidateWE_OP();
			AssertHasError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.InvalidProductErrorMessage);
		}

		#endregion

		#region TestCheckWE_OP_ValidationProductWarningMessage

		public void TestCheckWE_OP_ValidationProductWarningMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			Factory.Save();
			var receiveLine = receive.Lines[0];
			var inventory = receiveLine.Inventory[0];
			inventory.ValidationProductWarningMessage = "XXX";
			receiveLine.Validation.ValidateWE_OP();
			AssertHasWarning(receiveLine.WE_OPInfo, "XXX");

			inventory.ValidationProductWarningMessage = "YYY";
			receiveLine.Validation.ValidateWE_OP();
			AssertHasWarning(receiveLine.WE_OPInfo, "YYY");
		}

		#endregion

		#region TestCheckWE_OP_TempProductWarning

		public void TestCheckWE_OP_TempProductWarning()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			Factory.Save();
			var receiveLine = receive.Lines[0];
			var inventory = receiveLine.Inventory[0];

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "P1";
			inventory.WI_OP = part.PK;
			receiveLine.Validation.ValidateWE_OP();
			AssertEquals(false, receiveLine.WE_OPInfo.HasWarning(WhsValidationHelper.NoPartFoundWithThisCode));

			inventory.WI_OP_PartNum = "NewProduct";
			inventory.WI_OP = ZGuid.Invalid;
			receiveLine.Validation.ValidateWE_OP();
			AssertEquals(true, receiveLine.WE_OPInfo.HasWarning(WhsValidationHelper.NoPartFoundWithThisCode));
		}

		#endregion

		#region TestCheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed

		protected override bool ShouldRunTestCheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed
		{
			get { return false; }
		}

		#endregion

		#endregion

		#region TestCheckProductDesc

		public void TestCheckProductDesc()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(data.Receive11, data.Part1, 10m);
			Factory.Save();
			var receiveLine = data.Line111.InDocketLine;
			var receiveLine2 = inventory2.InDocketLine;

			AssertEquals(false, receiveLine.ProductDescInfo.HasWarning(WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning));
			AssertEquals(false, receiveLine.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));

			data.Part1.OP_Desc = "";
			receiveLine.ProductDesc = "";
			AssertEquals(false, receiveLine.ProductDescInfo.HasWarning(WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning));
			AssertEquals(false, receiveLine.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));

			receiveLine.WE_OP = ZGuid.Invalid;
			receiveLine.ProductDesc = "BLA"; // making sure validation will run
			receiveLine.ProductDesc = "";
			AssertEquals(false, receiveLine.ProductDescInfo.HasWarning(WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning));
			AssertEquals(false, receiveLine.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));

			receiveLine.ProductCode = "NewProduct";
			receiveLine.ProductDesc = "BLA"; // making sure validation will run
			receiveLine.ProductDesc = "";
			AssertEquals(true, receiveLine.ProductDescInfo.HasWarning(WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning));
			AssertEquals(false, receiveLine.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));

			receiveLine.ProductDesc = "NewDesc";
			AssertEquals(false, receiveLine.ProductDescInfo.HasWarning(WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning));
			AssertEquals(false, receiveLine.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));

			// Same Product same Description - No errors
			receiveLine2.WE_OP = ZGuid.Invalid;
			receiveLine2.ProductCode = "NewProduct";
			receiveLine2.ProductDesc = "NewDesc";
			AssertEquals(false, receiveLine.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));
			AssertEquals(false, receiveLine2.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));

			// Same Product different Description - All lines should have errors
			receiveLine2.ProductDesc = "NewDesc-2";
			AssertEquals(true, receiveLine2.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));
			AssertEquals(false, receiveLine.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));

			receiveLine.Validation.ValidateProductDesc();
			AssertEquals(true, receiveLine.ProductDescInfo.HasError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode));
		}

		#endregion

		public void TestCheckWE_OP_ReturnReceive_ReturningProductThatWasNotOrdered()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive.WD_WD_ParentDocket = order.PK;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 10m);
			AssertHasError(returnReceiveLine.WE_OPInfo, "Product cannot be found in the associated order to return.");
		}

		public void TestCheckWE_OP_ReturnReceive_ReturningProductThatWasNotOrdered_InDatabase_WithLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive.WD_WD_ParentDocket = order.PK;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 10m, data.Whs1.DefaultInboundDockDoorLocation);
			Factory.Save();

			Assert("Precondition", !returnReceiveLine.WE_WL.IsEmpty);
			Assert("Precondition", returnReceiveLine.IsInDatabase);

			var newFactrory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactrory.Load<WhsReceiveLine>(returnReceiveLine.PK);
			returnReceiveLine.Validation.ValidateWE_OP();
			AssertNoErrors(returnReceiveLine.WE_OPInfo);
		}

		public void TestCheckWE_OP_ReturnReceive_ReturningProductThatWasNotOrdered_NotInDatabase_WithLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive.WD_WD_ParentDocket = order.PK;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 10m, data.Whs1.DefaultInboundDockDoorLocation);

			Assert("Precondition", !returnReceiveLine.WE_WL.IsEmpty);
			Assert("Precondition", !returnReceiveLine.IsInDatabase);
			AssertHasError(returnReceiveLine.WE_OPInfo, "Product cannot be found in the associated order to return.");
		}

		public void TestCheckWE_OP_ReturnReceive_ReturningProductThatWasNotOrdered_InDatabase_HasPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive.WD_WD_ParentDocket = order.PK;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 10m, data.Whs1.DefaultInboundDockDoorLocation, "A");
			returnReceiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			Factory.Save();

			var nonDockDoorLocation = data.Whs1.FindLocation("A-3");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, nonDockDoorLocation, "A", 10m);
			transfer.RunPreSaveValidation();
			Factory.Save();

			Assert("Precondition", returnReceiveLine.HasPutawayTransfer);
			Assert("Precondition", returnReceiveLine.IsInDatabase);

			var newFactrory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactrory.Load<WhsReceiveLine>(returnReceiveLine.PK);
			returnReceiveLine.Validation.ValidateWE_OP();
			AssertNoErrors(returnReceiveLine.WE_OPInfo);
		}

		public void TestCheckWE_OP_ReturnReceive_ReturningProductThatWasNotOrdered_NotInDatabase_HasPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive.WD_WD_ParentDocket = order.PK;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 10m);
			returnReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;

			Assert("Precondition", returnReceiveLine.HasPutawayTransfer);
			Assert("Precondition", !returnReceiveLine.IsInDatabase);
			AssertHasError(returnReceiveLine.WE_OPInfo, "Product cannot be found in the associated order to return.");
		}

		public void TestCheckWE_OP_ReturnReceive_NoLinkedOrderToReturn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "SomeRandomOrderNumber");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			var returnReceiveLine = Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 10m);
			AssertNoErrors(returnReceiveLine.WE_OPInfo);

			returnReceiveLine.RunPreSaveValidation();
			AssertNoErrors(returnReceiveLine.WE_OPInfo);
		}

		#endregion

		#region TestCheckSplitQuantity

		public void TestCheckSplitQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var receiveLine = receive.Lines[0];
			receiveLine.WE_TransactionQuantity = 20m;
			receiveLine.SplitQuantity = 10m;

			AssertNoErrors(receiveLine.SplitQuantityInfo);

			receiveLine.SplitQuantity = -1m;
			AssertHasError(receiveLine.SplitQuantityInfo, "Split quantity cannot be negative.");

			receiveLine.SplitQuantity = 0m;
			AssertNoErrors(receiveLine.SplitQuantityInfo);

			receiveLine.SplitQuantity = 21m;
			AssertHasError(receiveLine.SplitQuantityInfo, "Split quantity cannot be greater than the Line Quantity.");
		}

		#endregion

		#region TestCheckWE_CurrentHoldReason

		public void TestCheckWE_CurrentHoldReason()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_CurrentHoldReason = "Because I said so!";
			AssertHasError(receiveLine.WE_CurrentHoldReasonInfo, "Hold Reason cannot be entered if the stock is not on Hold. Enter in a Hold Code or remove the Reason.");

			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			AssertNoErrors(receiveLine.WE_CurrentHoldReasonInfo);
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode

		public override void TestCheckWE_WHC_NKOriginalInventoryHeldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			Factory.Save();

			inventory1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = "";
			AssertNoErrors(inventory1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			inventory1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			AssertNoErrors(inventory1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			inventory1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = "AAA";
			AssertHasErrors(inventory1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode_AfterFinalization

		public void TestCheckWE_WHC_NKOriginalInventoryHeldCode_AfterFinalization()
		{
			var inventoryHeldCode = Factory.NewWithValidTestData<WhsInventoryHeldCode>();
			inventoryHeldCode.WHC_Code = "AAA";
			Factory.Save();

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			inventory1.OriginalInventoryHeldCode = inventoryHeldCode.WHC_Code;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			inventoryHeldCode.Delete();
			inventory1.InDocketLine.Validation.ValidateAll();
			AssertNoErrors(inventory1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld

		public override void TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld()
		{
			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Pending;
			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = string.Empty;
			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please enter a Hold Code.");

			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please do not enter a Hold Code.");
		}

		#endregion

		#region TestWE_WHC_NKCheckOriginalInventoryHeldCode_Reserved

		public void TestWE_WHC_NKCheckOriginalInventoryHeldCode_Reserved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			Factory.Save();

			var receiveLine = receive.Lines[0];
			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = "AAA";
			AssertHasErrors("Enter a valid selection.", receiveLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);
			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = "";

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(receiveLine.Inventory[0]));
			AssertNoErrors("Precondition", receiveLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertHasError(receiveLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "This Inventory is Cross Docked, you cannot set the Hold Code to Damaged.");
		}

		#endregion

		#endregion

		#region TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld

		public override void TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld()
		{
			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Pending;
			DocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);

			DocketLine.WE_WHC_NKCurrentInventoryHeldCode = string.Empty;
			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo, "Please enter a Hold Code.");

			DocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);

			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo, "Please do not enter a Hold Code.");
		}

		#endregion

		#region TestValidateWE_StockOnHand

		public void TestValidateWE_StockOnHand()
		{
			TestMinDecimal(DocketLine.WE_StockOnHandInfo, ErrorCheckType.HasErrors, 0m);
		}

		#endregion

		#region TestPartAttributes

		#region TestCheckWE_ExpiryDate_DuringFinalisation

		public void TestCheckWE_ExpiryDate_DuringFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: true, finalise: false);
			Factory.Save();

			// AttributeCallChecker -- ensure no validation is called when setting the field
			var receiveLine = receive.Lines[0];
			receiveLine.WE_StockOnHand = 0m;
			using (new PartAttributeValidationChecker.AttributeCallChecker())
			{
				receiveLine.WE_ExpiryDate = ZDate.Today;
			}

			// AttributeCallChecker - ensure no validation called when WI_TotalUnits is 0
			using (new PartAttributeValidationChecker.AttributeCallChecker(false, data.Org1, data.Part1, receiveLine.WE_ExpiryDateInfo, -1))
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_ExpiryDate = ZDate.Today.AddDays(1);
			}

			receiveLine.WE_StockOnHand = receiveLine.WE_TransactionQuantity; // need to set Total Units manualy, because otherwise it will be set only during RunPreSaveValidation.

			// AttributeCallChecker - validation called and passed
			using (new PartAttributeValidationChecker.AttributeCallChecker(data.Org1, data.Part1, receiveLine.WE_ExpiryDateInfo, -1))
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_ExpiryDate = ZDate.Today.AddDays(2);
			}

			// AttributeCallChecker - validation called and passed
			using (new PartAttributeValidationChecker.AttributeCallChecker(data.Org1, data.Part1, receiveLine.WE_ExpiryDateInfo, -1))
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_ExpiryDate = ZDate.Today.AddYears(22);
			}
		}

		#endregion

		#region TestCheckWE_ExpiryDate_WhenPickByBOM

		public void TestCheckWE_ExpiryDate_WhenPickByBOM()
		{
			AssertCheckWE_PartAttrib_WhenPickByBOM(AttributeNumber.ExpiryDate);
		}

		#endregion

		#region TestCheckWE_ExpiryDate_NotificationPeriod

		[TestDate(2015, 7, 28)]
		public void TestCheckWE_ExpiryDate_NotificationPeriod()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_ExpiryNotificationPeriod = 5;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();

			var receiveLine = inventory.InDocketLine;
			AssertNoWarnings("Precondition - ensure there are no warnings.", receiveLine.WE_ExpiryDateInfo);

			receiveLine.WE_ExpiryDate = ZDate.Today.AddDays(7);
			AssertNoWarnings("Expiry Date outside of notification period should not cause warning.", receiveLine.WE_ExpiryDateInfo);

			receiveLine.WE_ExpiryDate = ZDate.Today.AddDays(5);
			AssertHasWarnings("Border case should have warning.", receiveLine.WE_ExpiryDateInfo);

			receiveLine.WE_ExpiryDate = ZDate.Today.AddDays(3);
			AssertHasWarnings("Expiry Date within notification period, should show warning.", receiveLine.WE_ExpiryDateInfo);

			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_ExpiryDate = ZDate.Today.AddDays(2);
				AssertNoWarnings("Expiry Date within notification period, but we are finalising, should not show warning.", receiveLine.WE_ExpiryDateInfo);
			}
		}

		#endregion

		#region TestCheckWE_ExpiryDateIsValidZDateTimeRange_PastYearsBeforeWarning

		public override string ExpectedErrorExpiryDatePastYearsBeforeWarning() => $"The date '03-Jun-2018' should be in future.";

		#endregion

		#region TestCheckWE_PackingDate_DuringFinalisation

		public void TestCheckWE_PackingDate_DuringFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: true, finalise: false);
			Factory.Save();

			// AttributeCallChecker -- ensure no validation is called when setting the field
			var receiveLine = receive.Lines[0];
			receiveLine.WE_StockOnHand = 0m;
			using (new PartAttributeValidationChecker.AttributeCallChecker())
			{
				receiveLine.WE_PackingDate = ZDate.Today;
			}

			// AttributeCallChecker - ensure no validation called when WI_TotalUnits is 0
			using (new PartAttributeValidationChecker.AttributeCallChecker(false, data.Org1, data.Part1, receiveLine.WE_PackingDateInfo, -2))
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_PackingDate = ZDate.Today.AddDays(1);
			}

			receiveLine.WE_StockOnHand = receiveLine.WE_TransactionQuantity; // need to set Total Units manualy, because otherwise it will be set only during RunPreSaveValidation.

			// AttributeCallChecker - validation called and passed
			using (new PartAttributeValidationChecker.AttributeCallChecker(data.Org1, data.Part1, receiveLine.WE_PackingDateInfo, -2))
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_PackingDate = ZDate.Today.AddDays(2);
			}
		}

		#endregion

		#region TestCheckWE_PackingDate_WhenPickByBOM

		public void TestCheckWE_PackingDate_WhenPickByBOM()
		{
			AssertCheckWE_PartAttrib_WhenPickByBOM(AttributeNumber.PackingDate);
		}

		#endregion

		#region TestCheckWE_PartAttrib_DuringFinalisation

		public void TestCheckWE_PartAttrib1_DuringFinalisation()
		{
			TestCheckWE_PartAttrib_DuringFinalisation((l) => l.WE_PartAttrib1Info, 1);
		}

		public void TestCheckWE_PartAttrib2_DuringFinalisation()
		{
			TestCheckWE_PartAttrib_DuringFinalisation((l) => l.WE_PartAttrib2Info, 2);
		}

		public void TestCheckWE_PartAttrib3_DuringFinalisation()
		{
			TestCheckWE_PartAttrib_DuringFinalisation((l) => l.WE_PartAttrib3Info, 3);
		}

		void TestCheckWE_PartAttrib_DuringFinalisation(Func<WhsReceiveLine, ZPropertyInfo> getPropertyInfo, int attributeNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: true, finalise: false);
			Factory.Save();

			var receiveLine = receive.Lines[0];
			receiveLine.WE_StockOnHand = 0m;
			var info = getPropertyInfo(receiveLine);
			receiveLine.Inventory[0].SetValidationPartAttribWarningMessage(attributeNumber, "Hello");
			receiveLine[info.Name] = "PA1";
			AssertHasWarning(info, "Hello");

			// AttributeCallChecker -- ensure no validation is called when setting the field
			using (new PartAttributeValidationChecker.AttributeCallChecker())
			{
				receiveLine[info.Name] = "1";
			}

			// AttributeCallChecker - ensure no validation called when WE_StockOnHand is 0
			using (new PartAttributeValidationChecker.AttributeCallChecker(false, data.Org1, data.Part1, info, attributeNumber))
			{
				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					receiveLine[info.Name] = "2";
				}
			}

			receiveLine.WE_StockOnHand = receiveLine.WE_TransactionQuantity;

			// AttributeCallChecker - validation called and passed
			using (new PartAttributeValidationChecker.AttributeCallChecker(data.Org1, data.Part1, info, attributeNumber))
			{
				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					receiveLine[info.Name] = "3";
				}
			}

			receiveLine[info.Name] = "  PA1";
			AssertHasErrors(WhsValidationHelper.ValueHasToBeTrimmed, info);
		}

		#endregion

		#region TestCheckWE_PartAttrib_JulianBatchNumber

		protected override void AssertTestCheckWE_PartAttrib_JulianBatchNumber(TestDataSimpleEnvironment data, ZPropertyInfo info, AttributeNumber attributeNumber)
		{
			var receiveLine = (WhsReceiveLine)info.BizObj;
			receiveLine.WE_StockOnHand = 10m;
			using (new SemaphoreManager(receiveLine.Docket.FinaliseDocketSemaphore)) // attribute validation is only during finalise.
			{
				base.AssertTestCheckWE_PartAttrib_JulianBatchNumber(data, info, attributeNumber);
			}
		}

		protected override void AssertTestCheckWE_PartAttrib_JulianBatchNumber_MultiAttribute(WhsReceiveLine docketLine)
		{
			docketLine.WE_StockOnHand = 10m;
			using (new SemaphoreManager(docketLine.Docket.FinaliseDocketSemaphore)) // attribute validation is only during finalise.
			{
				base.AssertTestCheckWE_PartAttrib_JulianBatchNumber_MultiAttribute(docketLine);
			}
		}

		#region IsJulianBatchNumberFormatValidationRequired

		protected override bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo)
		{
			return true;
		}

		#endregion

		#endregion

		#region TestCheckWE_PartAttrib_SerialNumberAgainstCurrentReceipt

		void AssertSerial(ZPropertyInfo inv1attribInfo, ZPropertyInfo inv2attribInfo, bool inventoriesHaveSameProduct, string attrib2Name)
		{
			ZString value1 = "SER1";
			ZString value2 = "SER2";

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO");
			inv1attribInfo.Value = value1;
			inv2attribInfo.Value = value2;
			AssertNoErrors("By Product - But different Serial, so should always have NO errors.", inv2attribInfo);

			inv2attribInfo.Value = value1;
			if (inventoriesHaveSameProduct)
			{
				AssertHasError("By Product - Same Serial - Same Products, so should have errors.", inv2attribInfo, string.Format("{0} already used.", attrib2Name));
			}
			else
			{
				AssertNoErrors("By Product - Same Serial - Different Products, so should have NO errors.", inv2attribInfo);
			}

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI");
			inv1attribInfo.Value = value1;
			inv2attribInfo.Value = value2;
			AssertNoErrors("By Client - But different Serial, so should always have NO errors.", inv2attribInfo);

			inv2attribInfo.Value = value1;
			AssertHasError("By Cient - Same Serial - Product doesn't matter, so should have errors.", inv2attribInfo, string.Format("{0} already used.", attrib2Name));
		}

		#endregion

		#region TestCheckWE_PartAttrib_WhenPickByBOM

		public void TestCheckWE_PartAttrib1_WhenPickByBOM()
		{
			AssertCheckWE_PartAttrib_WhenPickByBOM(AttributeNumber.One);
		}

		public void TestCheckWE_PartAttrib2_WhenPickByBOM()
		{
			AssertCheckWE_PartAttrib_WhenPickByBOM(AttributeNumber.Two);
		}

		public void TestCheckWE_PartAttrib3_WhenPickByBOM()
		{
			AssertCheckWE_PartAttrib_WhenPickByBOM(AttributeNumber.Three);
		}

		void AssertCheckWE_PartAttrib_WhenPickByBOM(AttributeNumber attrib)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attrib, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attrib, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_WP_ParentPickForReceive = Factory.New<WhsPick>().PK;

			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			line.WE_WL = data.Whs1.DefaultLocation.PK;

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
		}

		#endregion

		#region Test Release Captured Attributes

		protected override bool IsReleaseCapturedValidationRequired(WhsReceiveLine line)
		{
			return true;
		}

		#endregion

		#region TestCheckWE_SerialNumber

		public void TestCheckWE_SerialNumber_AgainstCurrentReceipt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineA1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLineA2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLineB = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m);

			AssertSerial(receiveLineA1.WE_SerialNumberInfo, receiveLineA2.WE_SerialNumberInfo, true, "Serial #");
			AssertSerial(receiveLineA1.WE_SerialNumberInfo, receiveLineB.WE_SerialNumberInfo, false, "Serial #");

			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true, setReleaseCaptured: true);

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI");
			receiveLineA1.WE_SerialNumber = "SER1";
			receiveLineB.WE_SerialNumber = "SER1";
			AssertNoError(receiveLineB.WE_SerialNumberInfo, "Serial # already used.");
		}

		public void TestCheckWE_SerialNumber_CancelledReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m);
			receive1Line.WE_SerialNumber = "SER1";
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receive2Line = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m, data.Whs1.DefaultLocation);
			receive2Line.WE_SerialNumber = "SER1";
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition: Failed to finalise.", false, receive2.IsFinalised);
			AssertHasError(receive2Line.WE_SerialNumberInfo, "Serial # already used.");

			receive1.CancelReactivateDocket();
			AssertEquals("Precondition: Receive was cancelled.", true, receive1.IsCancelled);
			Factory.Save();

			receive2Line.Validation.ValidateWE_SerialNumber();
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			AssertNoError(receive2Line.WE_SerialNumberInfo, "Serial # already used.");
		}

		public void TestCheckUniqueWE_SerialNumber_LineWithNoQuantity_AfterReceiveStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine1.WE_SerialNumber = "SERIAL123";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine2.WE_SerialNumber = "SERIAL123";
			Factory.Save();
			receive.PopulateASNLines();

			AssertEquals("Precondition: receive.WD_StartedReceivingTimeUtc", true, receive.StartedReceiving);
			AssertEquals("Precondition: receiveLine1.WE_TransactionQuantity", 1m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition: receiveLine1.WE_ClientOrderedUnits", 1m, receiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Precondition: receiveLine2.WE_TransactionQuantity", 1m, receiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition: receiveLine2.WE_ClientOrderedUnits", 1m, receiveLine2.WE_ClientOrderedUnits);

			AssertNoError(receiveLine1.WE_SerialNumberInfo, "Serial # already used.");
			AssertHasError("SERIAL123 is already used in receiveLine1.", receiveLine2.WE_SerialNumberInfo, "Serial # already used.");

			receiveLine2.WE_TransactionQuantity = 0m;
			Factory.Save();
			AssertNoError("Serial number duplicate error is cleared as receiveLine2 has no more transaction quantity.", receiveLine2.WE_SerialNumberInfo, "Serial # already used.");

			receiveLine1.Validation.ValidateWE_SerialNumber();
			AssertNoError(receiveLine1.WE_SerialNumberInfo, "Serial # already used.");
		}

		public void TestCheckWE_SerialNumber_QtyGreaterThanOne_TransactionQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_SerialNumber = "Ser";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_SerialNumber = "Ser2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertHasError("Errors for serial number and 10 Qty", receiveLine1.WE_TransactionQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			receiveLine1.WE_TransactionQuantity = 1m;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertNoErrors("Precondition: No errors for serial number and 1 Qty", receiveLine1.WE_TransactionQuantityInfo);

			receiveLine1.WE_SerialNumber = "";
			receiveLine1.WE_TransactionQuantity = 10m;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertNoErrors("No errors for No serial number and 10 Qty", receiveLine1.WE_TransactionQuantityInfo);
		}

		public void TestCheckWE_SerialNumber_QtyGreaterThanOne_ClientOrderedUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_SerialNumber = "Ser";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_SerialNumber = "Ser2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertHasError("Errors for serial number and 10 Qty", receiveLine1.WE_ClientOrderedUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			receiveLine1.WE_ClientOrderedUnits = 1m;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertNoErrors("Precondition: No errors for serial number and 1 Qty", receiveLine1.WE_ClientOrderedUnitsInfo);

			receiveLine1.WE_SerialNumber = "";
			receiveLine1.WE_ClientOrderedUnits = 10m;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertNoErrors("No errors for No serial number and 10 Qty", receiveLine1.WE_ClientOrderedUnitsInfo);
		}

		protected override void TestCheckWE_SerialNumber_RequiredCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_SerialNumber = "Ser";
				AssertNoErrors("Precondition: No errors for serial number existing", receiveLine.WE_SerialNumberInfo);

				receiveLine.WE_SerialNumber = "";
				AssertHasError("Errors for missing manadatory serial number", receiveLine.WE_SerialNumberInfo, "Please enter a Serial Number.");

				receiveLine.WE_SerialNumber = "FRD";
				AssertNoErrors("No errors for another serial number", receiveLine.WE_SerialNumberInfo);
			}
		}

		protected override void TestCheckWE_SerialNumber_ReleaseCapturedCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_SerialNumber = "";
				AssertNoErrors("Precondition: No errors for no serial number", receiveLine.WE_SerialNumberInfo);

				receiveLine.WE_SerialNumber = "SER";
				AssertHasError("Errors for added value on release captured serial number", receiveLine.WE_SerialNumberInfo, "Serial Number is specified as Release Captured for this Product, no value should be entered.");

				receiveLine.WE_SerialNumber = "";
				AssertNoErrors("No errors for cleared serial number", receiveLine.WE_SerialNumberInfo);
			}
		}

		protected override void TestCheckWE_SerialNumber_TrimmedCorrectlyCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_SerialNumber = "Ser";
				AssertNoErrors("Precondition: No errors for serial number", receiveLine.WE_SerialNumberInfo);

				receiveLine.WE_SerialNumber = "   Ser";
				AssertHasError("Errors for serial number untrimmed", receiveLine.WE_SerialNumberInfo, "This value cannot begin or end with white-spaces.");

				receiveLine.WE_SerialNumber = "TGT";
				AssertNoErrors("No errors for another serial number", receiveLine.WE_SerialNumberInfo);
			}
		}

		public void TestCheckWE_SerialNumber_ValidationSerialNumberWarningMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var inv = receiveLine.Inventory[0];
			inv.ValidationSerialNumberWarningMessage = "Hello";
			receiveLine.WE_SerialNumber = "SER1";
			AssertHasWarning(receiveLine.WE_SerialNumberInfo, "Hello");
		}

		public void TestCheckWE_SerialNumber_WhenPickByBOM()
		{
			AssertCheckWE_PartAttrib_WhenPickByBOM(AttributeNumber.Serial);
		}

		#endregion

		#region TestAttributeValidation_MandatoryAttributes

		public void TestAttributeValidation_MandatoryAttributes_NoLocationAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, mandatoryAttributeType: true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);

			AssertEquals(false, receiveLine.Validation.IsAttributeValidationRequired);

			receiveLine.RunPreSaveValidation();

			AssertNoErrors(receiveLine.WE_PartAttrib1Info);
			AssertNoErrors(receiveLine.WE_PartAttrib2Info);
			AssertNoErrors(receiveLine.WE_PartAttrib3Info);
			AssertNoErrors(receiveLine.WE_ExpiryDateInfo);
			AssertNoErrors(receiveLine.WE_PackingDateInfo);
			AssertNoErrors(receiveLine.WE_SerialNumberInfo);
		}

		public void TestAttributeValidation_MandatoryAttributes_DockDoorLocationAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, mandatoryAttributeType: true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine.WE_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;

			AssertEquals(true, receiveLine.Validation.IsAttributeValidationRequired);

			receiveLine.RunPreSaveValidation();

			AssertHasError(receiveLine.WE_PartAttrib1Info, "Please enter a Part Attrib. 1.");
			AssertHasError(receiveLine.WE_PartAttrib2Info, "Please enter a Part Attrib. 2.");
			AssertHasError(receiveLine.WE_PartAttrib3Info, "Please enter a Part Attrib. 3.");
			AssertHasError(receiveLine.WE_ExpiryDateInfo, "Please enter an Expiry date.");
			AssertHasError(receiveLine.WE_PackingDateInfo, "Please enter a Packing date.");
			AssertHasError(receiveLine.WE_SerialNumberInfo, "Please enter a Serial Number.");
		}

		public void TestAttributeValidation_MandatoryAttributes_NonDockDoorLocationAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, mandatoryAttributeType: true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine.WE_WL = data.Whs1.FindLocation("A-1").PK;

			AssertEquals(false, receiveLine.Validation.IsAttributeValidationRequired);

			receiveLine.RunPreSaveValidation();

			AssertNoErrors(receiveLine.WE_PartAttrib1Info);
			AssertNoErrors(receiveLine.WE_PartAttrib2Info);
			AssertNoErrors(receiveLine.WE_PartAttrib3Info);
			AssertNoErrors(receiveLine.WE_ExpiryDateInfo);
			AssertNoErrors(receiveLine.WE_PackingDateInfo);
			AssertNoErrors(receiveLine.WE_SerialNumberInfo);
		}

		public void TestAttributeValidation_NonMandatoryAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, mandatoryAttributeType: false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, mandatoryAttributeType: false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, mandatoryAttributeType: false);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);

			AssertEquals(false, receiveLine.Validation.IsAttributeValidationRequired);

			receiveLine.RunPreSaveValidation();

			AssertNoErrors(receiveLine.WE_PartAttrib1Info);
			AssertNoErrors(receiveLine.WE_PartAttrib2Info);
			AssertNoErrors(receiveLine.WE_PartAttrib3Info);

			receiveLine.WE_WL = data.Whs1.FindLocation("A-1").PK;
			AssertEquals(false, receiveLine.Validation.IsAttributeValidationRequired);

			receiveLine.RunPreSaveValidation();

			AssertNoErrors(receiveLine.WE_PartAttrib1Info);
			AssertNoErrors(receiveLine.WE_PartAttrib2Info);
			AssertNoErrors(receiveLine.WE_PartAttrib3Info);

			receiveLine.WE_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			AssertEquals(true, receiveLine.Validation.IsAttributeValidationRequired);

			receiveLine.RunPreSaveValidation();

			AssertNoErrors(receiveLine.WE_PartAttrib1Info);
			AssertNoErrors(receiveLine.WE_PartAttrib2Info);
			AssertNoErrors(receiveLine.WE_PartAttrib3Info);
		}

		#endregion

		#endregion

		#region TestCheckWE_PalletID

		#region TestCheckWE_PalletID_DifferentLocationsSamePalletID

		public void TestCheckWE_PalletID_DifferentLocationsSamePalletID()
		{
			// locations across 2 warehouses
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs1_location1 = data.Whs1.FindLocation("A-1");
			var whs1_location2 = data.Whs1.FindLocation("A-2");
			var whs2 = Helper.CreateWarehouse("W2", "A", 2, 1);
			Factory.Save();
			var whs2_location1 = whs2.FindLocation("A-1");

			// receive into warehouse 1
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			// receive into warehouse 2
			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			Factory.Save();

			var receiveLine1 = inventory1.InDocketLine;
			var receiveLine2 = inventory2.InDocketLine;
			var receiveLine3 = inventory3.InDocketLine;

			string palletID = "XXX123";
			receiveLine1.WE_WL = whs1_location1.PK;
			receiveLine2.WE_WL = whs1_location2.PK;
			receiveLine2.WE_PalletID = palletID;
			receiveLine1.WE_PalletID = palletID;
			AssertHasErrors("PalletID is duplicated across 2 locations in the same warehouse - expect an error.", receiveLine1.WE_PalletIDInfo);

			receiveLine1.WE_PalletID = "  xxx";
			AssertHasErrors(WhsValidationHelper.ValueHasToBeTrimmed, receiveLine1.WE_PalletIDInfo);

			receiveLine1.WE_PalletID = "";
			AssertNoErrors("Shouldn't validate if empty Pallet IDs", receiveLine1.WE_PalletIDInfo);

			receiveLine1.WE_WL = ZGuid.Empty;
			receiveLine1.WE_PalletID = palletID;
			AssertNoErrors("Shouldn't validate if empty location", receiveLine1.WE_PalletIDInfo);

			receiveLine1.WE_PalletID = "";
			receiveLine1.WE_WL = whs1_location2.PK;
			receiveLine1.WE_PalletID = palletID;
			AssertNoErrors("Same locations should not cause error", receiveLine1.WE_PalletIDInfo);

			receiveLine1.WE_PalletID = "";
			receiveLine2.WE_WL = ZGuid.Empty;
			receiveLine1.WE_PalletID = palletID;
			AssertNoErrors("No error if other inventory has same Pallet ID but no location", receiveLine1.WE_PalletIDInfo);

			// same Pallet ID in different warehouses
			receiveLine1.WE_WL = whs1_location1.PK;
			receiveLine2.WE_WL = ZGuid.Empty;
			receiveLine3.WE_WL = whs2_location1.PK;
			receiveLine1.WE_PalletID = palletID;
			receiveLine2.WE_PalletID = "";
			receiveLine3.WE_PalletID = palletID;
			AssertNoErrors("PalletID is duplicated across 2 locations in *different* warehouses - expect no error.", receiveLine3.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_ValidationPalletIDWarningMessage

		public void TestCheckWE_PalletID_ValidationPalletIDWarningMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			Factory.Save();

			var inventory = receive.Inventory[0];
			var receiveLine = receive.Lines[0];
			inventory.ValidationPalletIDWarningMessage = "XXX";
			AssertHasWarning(receiveLine.WE_PalletIDInfo, "XXX");

			inventory.ValidationPalletIDWarningMessage = "";
			AssertNoWarnings(receiveLine.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_NoErrorsWhenReUsePalletID

		public void TestCheckWE_PalletID_NoErrorsWhenReUsePalletID()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			data.Line111.WI_PalletID = "PLT-1";
			data.Receive11.FinaliseDocket();
			AssertIsFinalisedPrecondition(data.Receive11);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "A-2");
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.WE_PalletID = "PLT-1";
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var helperInOtherFactory = new WhsTestHelperFunctions(otherFactory);
			var receiveInOtherFactory = helperInOtherFactory.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryInOtherFactory = helperInOtherFactory.CreateWhsReceiveInventoryLine(receiveInOtherFactory, data.Part1, 10m);
			otherFactory.Save();

			var receiveLineInOtherFactory = inventoryInOtherFactory.InDocketLine;
			receiveLineInOtherFactory.WE_WL = data.Whs1.FindLocation("A-2").PK; // A-2-1
			receiveLineInOtherFactory.WE_PalletID = "PLT-1";
			AssertNoErrors(receiveLineInOtherFactory.WE_PalletIDInfo);
		}

		#endregion

		#region TestFinaliseWhsReceiveLine_EnforcePalletID

		public void TestCheckWE_PalletID_FinaliseWhsReceiveLine_EnforcePalletIDs_PalletIDFieldEmpty()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			receiveCategories.Add("RC2", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ReceiveCategory = "RC2";

			var whsClientParams1 = Factory.New<WhsClientParameterByWarehouse>();
			whsClientParams1.WY_OH_Client = data.Org1.PK;
			whsClientParams1.WY_WW_Whs = data.Whs1.PK;
			whsClientParams1.WY_ReceiveCategory = ZString.Empty;
			whsClientParams1.WY_EnforcePalletIDEntry = true;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventory.InDocketLine;

			// Act
			receiveLine.WE_PalletID = "";
			receive.FinaliseDocket();

			// Assert has error 
			AssertHasError("Should show an error when pallet ID is empty and EnforcePalletIDs property is true.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");

			whsClientParams1.WY_EnforcePalletIDEntry = false;
			receive.FinaliseDocket();

			// Assert has no error
			AssertNoError("Should not show an error when pallet ID is empty and EnforcePalletIDs property is false.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");

			// Act
			whsClientParams1.WY_EnforcePalletIDEntry = false;
			receiveLine.WE_PalletID = "PLT-123";
			receive.FinaliseDocket();

			// Assert has no error
			AssertNoError("Should not show an error when pallet ID is not empty and EnforcePalletIDs property is false.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");
		}

		public void TestCheckWE_PalletID_FinaliseWhsReceiveLine_EnforcePalletIDs_PalletIDFieldEmpty_ZeroQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			var whsClientParams1 = Factory.New<WhsClientParameterByWarehouse>();
			whsClientParams1.WY_OH_Client = data.Org1.PK;
			whsClientParams1.WY_WW_Whs = data.Whs1.PK;
			whsClientParams1.WY_ReceiveCategory = ZString.Empty;
			whsClientParams1.WY_EnforcePalletIDEntry = true;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventory.InDocketLine;

			receiveLine.WE_PalletID = "";
			receive.FinaliseDocket();
			AssertHasError("Should show an error when pallet ID is empty and EnforcePalletIDs property is true.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");

			receiveLine.WE_TransactionQuantity = 0m;
			receive.FinaliseDocket();

			AssertNoError("Should not show an error when pallet ID is empty and EnforcePalletIDs property is true, but quantity is 0.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");
		}

		public void TestCheckWE_PalletID_FinaliseWhsReceiveLine_EnforcePalletIDs_HasPickLocation()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var whsClientParams = Factory.New<WhsClientParameterByWarehouse>();
			whsClientParams.WY_OH_Client = data.Org1.PK;
			whsClientParams.WY_WW_Whs = data.Whs1.PK;
			whsClientParams.WY_EnforcePalletIDEntry = true;
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var pickFace = product.PickFaces.AddNew();
			var pickFaceLocation = locations[0];
			pickFace.WF_WL = pickFaceLocation.PK;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventory.InDocketLine;

			// Act
			receiveLine.WE_PalletID = "";
			receive.FinaliseDocket();

			// Assert has error 
			AssertHasError("Should show an error when pallet ID is empty and EnforcePalletIDs property is true.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");

			receiveLine.WE_WL = pickFaceLocation.PK;
			receive.FinaliseDocket();

			// Assert has no error
			AssertNoError("Should not show an error when pallet ID is empty, EnforcePalletIDs property is true, and location is a pick face.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");
		}

		public void TestCheckWE_PalletID_FinaliseWhsReceiveLine_EnforcePalletIDs_ReceiveIsFinalising()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var whsClientParams = Factory.New<WhsClientParameterByWarehouse>();
			whsClientParams.WY_OH_Client = data.Org1.PK;
			whsClientParams.WY_WW_Whs = data.Whs1.PK;
			whsClientParams.WY_EnforcePalletIDEntry = true;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventory.InDocketLine;

			// Act
			receiveLine.WE_PalletID = "";
			receiveLine.Validation.ValidateWE_PalletID();

			// Assert has no error
			AssertNoError("Should not show an error when pallet ID is empty,  EnforcePalletIDs property is true, and order is not finalizing.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");

			receive.FinaliseDocket();

			AssertHasError("Should show an error when pallet ID is empty, EnforcePalletIDs property is true, and order is finalizing.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");
		}

		public void TestCheckWE_PalletID_FinaliseWhsReceiveLine_EnforcePalletIDs_ClientParamHasWarehouse()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var whsClientParams = Factory.New<WhsClientParameterByWarehouse>();
			whsClientParams.WY_OH_Client = data.Org1.PK;
			whsClientParams.WY_EnforcePalletIDEntry = true;
			whsClientParams.WY_WW_Whs = ZGuid.Empty;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventory.InDocketLine;

			// Act
			receiveLine.WE_PalletID = "";

			receive.FinaliseDocket();

			AssertHasError("Should show an error when pallet ID is empty, EnforcePalletIDs property is true, order is finalizing, and client param has empty warehouse field.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");

			whsClientParams.WY_WW_Whs = data.Whs1.PK;

			receive.FinaliseDocket();

			AssertHasError("Should show an error when pallet ID is empty, EnforcePalletIDs property is true, order is finalizing, and client param has non-empty warehouse field.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID upon finalizing when pallet ID entries are enforced.");
		}

		#endregion

		#region TestCheckWE_PalletID_NewSamePalletIDDifferentLocation_Staged

		public void TestCheckWE_PalletID_NewSamePalletIDDifferentLocation_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.FindLocation("A"), "PLT-123");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order1);

			var pickLine = order1.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_PalletID = "PLT-123";
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 45m, data.Whs1.FindLocation("A"), "PLT-123");
			AssertHasError("Should have Pallet ID error as the ID exists in the dock door.", receiveLine.WE_PalletIDInfo,
				"Another location (DOCKDOOR) was already used for the same Pallet ID. Please select another location or Pallet ID.");

			receiveLine.WE_WL = data.Whs1.WW_DefaultOutboundDockDoor;
			receiveLine.RunPreSaveValidation();
			AssertNoError("Should not have Pallet ID error.", receiveLine.WE_PalletIDInfo,
				"Another location (DOCKDOOR) was already used for the same Pallet ID. Please select another location or Pallet ID.");
		}

		#endregion

		#region TestCheckWE_PalletID_SamePalletIDCannotBeEnteredAfterCreatingPutawayTransfers

		public void TestCheckWE_PalletID_SamePalletIDCannotBeEnteredAfterCreatingPutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var pendingInv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receivedInv = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 1m);
			var puttingAwayInv = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "B", 1m);
			var putawayInv = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "C", 1m);
			var arrivedInv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var putawayWithoutDockDoorInv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, nonDockDoorLocation, "D");
			var lineInPendingStatus = (WhsReceiveLine)pendingInv.InDocketLine;
			var lineInReceivedStatus = (WhsReceiveLine)receivedInv.InDocketLine;
			var lineInArrivedStatus = (WhsReceiveLine)arrivedInv.InDocketLine;
			var lineInPutawayStatusWithoutDockDoorLocation = (WhsReceiveLine)putawayWithoutDockDoorInv.InDocketLine;
			lineInArrivedStatus.WE_OriginalInventoryStatus = InventoryStatus.Codes.Arrived;
			lineInArrivedStatus.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			lineInPutawayStatusWithoutDockDoorLocation.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPalletB = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "B", 1m);
			var transferLineForPalletC = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "C", 1m);
			transfer.RunPreSaveValidation();
			transferLineForPalletB.PickedTime = ZDateTimeOffset.Now;
			transferLineForPalletC.FinaliseDocketLine();
			Factory.Save();

			AssertIsFinalisedPrecondition(transferLineForPalletC);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, lineInPendingStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, lineInReceivedStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.PuttingAway, transferLineForPalletB.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Arrived, lineInArrivedStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, lineInPutawayStatusWithoutDockDoorLocation.WE_OriginalInventoryStatus);

			AssertPalletIDErrors(lineInPendingStatus, "A", "lineInPendingStatus-A");
			AssertPalletIDErrors(lineInPendingStatus, "B", "lineInPendingStatus-B", "Pallet ID is assigned to a putaway transfer. Use a different Pallet ID.");
			AssertPalletIDErrors(lineInPendingStatus, "b", "lineInPendingStatus-B", "Pallet ID is assigned to a putaway transfer. Use a different Pallet ID.");
			AssertPalletIDErrors(lineInPendingStatus, "C", "lineInPendingStatus-C", "Pallet ID is assigned to a putaway transfer. Use a different Pallet ID.");
			AssertPalletIDErrors(lineInPendingStatus, "D", "lineInPendingStatus-D");

			AssertPalletIDErrors(lineInReceivedStatus, "B", "lineInReceivedStatus-B", "Another location (A-1) was already used for the same Pallet ID. Please select another location or Pallet ID.");
			AssertPalletIDErrors(lineInReceivedStatus, "C", "lineInReceivedStatus-C", "Another location (A-1) was already used for the same Pallet ID. Please select another location or Pallet ID.");
			AssertPalletIDErrors(lineInReceivedStatus, "D", "lineInReceivedStatus-D", "Another location (A-1) was already used for the same Pallet ID. Please select another location or Pallet ID.");

			AssertPalletIDErrors(lineInArrivedStatus, "A", "lineInArrivedStatus-A");
			AssertPalletIDErrors(lineInArrivedStatus, "B", "lineInReceivedStatus-B", "Pallet ID is assigned to a putaway transfer. Use a different Pallet ID.");
			AssertPalletIDErrors(lineInArrivedStatus, "C", "lineInArrivedStatus-C", "Pallet ID is assigned to a putaway transfer. Use a different Pallet ID.");
			AssertPalletIDErrors(lineInArrivedStatus, "D", "lineInArrivedStatus-D");
		}

		void AssertPalletIDErrors(WhsReceiveLine receiveLine, string palletID, string identifingMessage, string exceptionMessage = "")
		{
			receiveLine.WE_PalletID = palletID;
			if (string.IsNullOrEmpty(exceptionMessage))
			{
				AssertNoErrors(identifingMessage, receiveLine.WE_PalletIDInfo);
			}
			else
			{
				AssertHasError(identifingMessage, receiveLine.WE_PalletIDInfo, exceptionMessage);
			}
		}

		#endregion

		#region TestCheckWE_PalletID_InTransitWithExistingPalletInventory

		public void TestCheckWE_PalletID_InTransitWithExistingPalletInventory()
		{
			// Objective: Verify that error is shown correctly when trying to use location + pallet ID that was already used before.
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "Another location (A-1-1) was already used for the same Pallet ID. Please select another location or Pallet ID.";

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part1, 100m, locationA, "PLT456");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			var transferLine2 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 50m, locationA, "PLT456", locationB, "PLT456", picker);
			Factory.Save();

			AssertEquals("Receive 1 should be finalized.", true, receive1.IsFinalised);
			AssertEquals("Receive 2 should be finalized.", true, receive2.IsFinalised);
			AssertNoErrors("Transfer Line 1 should not have any errors.", transferLine1.WE_PalletIDInfo);
			AssertNoErrors("Transfer Line 2 should not have any errors.", transferLine2.WE_PalletIDInfo);

			var receiveForLocationB = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receiveForLocationB, data.Part1, 45m, locationB, "PLT111");
			AssertNoErrors("Receive should not have any errors.", receiveLine.WE_PalletIDInfo);

			receiveLine.WE_PalletID = "PLT123";
			AssertNoErrors("The receipt should NOT have any errors since pallet 123 is fully picked and in transit.", receiveLine.WE_PalletIDInfo);

			receiveLine.WE_PalletID = "PLT456";
			AssertHasError("Receive should show error since pallet 456 is still being picked.", receiveLine.WE_PalletIDInfo, expectedErrorMsg);

			var transferLine3 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 50m, locationA, "PLT456", locationB, "PLT456", picker);
			transfer.FinaliseDocket();
			Factory.Save();
			AssertEquals("Transfer should be finalised", true, transfer.IsFinalised);

			receiveLine.WE_PalletID = "PLT123";
			AssertNoErrors("No error is expected since inventory is being added to pallet ID 123", receiveLine.WE_PalletIDInfo);

			receiveLine.WE_PalletID = "PLT456";
			AssertNoErrors("No error is expected since inventory is being added to pallet ID 456", receiveLine.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_InTransitWithExistingPalletInventory_WithWhsPick

		public void TestCheckWE_PalletID_InTransitWithExistingPalletInventory_WithWhsPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "Another location (A-1-1) was already used for the same Pallet ID. Please select another location or Pallet ID.";
			var expectedErrorMsgForDOC = "Another location (DOCKDOOR) was already used for the same Pallet ID. Please select another location or Pallet ID.";

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, locationA, "PLT123");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m, locationA, "PLT456");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 100m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 50m);

			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals("Should be fully committed.", 100m, orderLine1.PickLineQuantity);
			AssertEquals("Should be fully committed.", 50m, orderLine2.PickLineQuantity);

			foreach (var pickLine in pick1.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}
			Factory.Save();

			var receiveForLocationB = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receiveForLocationB, data.Part1, 45m, locationB, "PLT111");
			AssertNoErrors("Receive should not have any errors.", receiveLine.WE_PalletIDInfo);

			receiveLine.WE_PalletID = "PLT456";
			AssertHasError("Receive should show error since pallet 456 still has stock on hand.", receiveLine.WE_PalletIDInfo, expectedErrorMsg);

			receiveLine.WE_PalletID = "PLT123";
			AssertHasError("Receive should have error since Pallet123 is still in-transit", receiveLine.WE_PalletIDInfo, expectedErrorMsgForDOC);
			receiveLine.WE_PalletID = "PLT111";

			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			Factory.Save();

			receiveLine.WE_PalletID = "PLT123";
			AssertNoErrors("The receipt should NOT have any errors since pallet 123 is fully picked.", receiveLine.WE_PalletIDInfo);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 50m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Should be fully committed.", 50m, order2.Lines[0].PickLineQuantity);

			foreach (var pickLine in pick2.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}
			Factory.Save();

			receiveLine.WE_PalletID = "PLT456";
			AssertHasError("Receive should have error since Pallet123 is still in-transit", receiveLine.WE_PalletIDInfo, expectedErrorMsgForDOC);
			receiveLine.WE_PalletID = "PLT111";

			pick2.FinaliseAllOrders();
			pick2.FinalisePick();
			Factory.Save();

			receiveLine.WE_PalletID = "PLT456";
			AssertNoErrors("The receipt should NOT have any errors since pallet 456 is fully picked.", receiveLine.WE_PalletIDInfo);

			receiveLine.WE_PalletID = "PLT123";
			AssertNoErrors("The receipt should NOT have any errors since pallet 123 is fully picked.", receiveLine.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_AfterTransferIsInTransit

		public void TestCheckWE_PalletID_AfterTransferIsInTransit()
		{
			// Verify that no validation error exist in receive lines after a transfer has been set in transit for same received pallet.
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			Factory.Save();

			receive.RunPreSaveValidation();
			AssertNoErrors("Receive should not have any errors.", receive.Lines.Single().WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_NotRunWhenStockOnHandIsZero

		public void TestCheckWE_PalletID_NotRunWhenStockOnHandIsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var palletID = "PLT-123";
			var picker = Helper.CreateGlbStaff("RSL", "Russell");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, dockDoorLocation, palletID);
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, palletID, 100m);
			putawayTransferLine.RunPreSaveValidation();
			putawayTransferLine.GS_NKPickedBy = picker.GS_Code;
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;

			putawayTransfer.FinaliseDocket();
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveLineInNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine.PK);
			receiveLineInNewFactory.Validation.ValidateWE_PalletID();
			AssertEquals("Precondition: SOH should be 0", ZDecimal.Zero, receiveLine.WE_StockOnHand);
			AssertNoErrors("The receipt should NOT have any errors since pallet 123 is fully picked and there is no SOH on receiveline.", receiveLineInNewFactory.WE_PalletIDInfo);
		}

		public void TestCheckWE_PalletID_CheckPalletID_PalletIdInMultipleLocationsNotCheckedIfNoSOH()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var palletID = "PLT-123";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, dockDoorLocation, palletID);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m, nonDockDoorLocation, palletID);
			Factory.Save();

			receiveLine1.Validation.ValidateWE_PalletID();
			AssertNoErrors("The line should NOT have any errors since the other inventory with the same pallet id has no SOH.", receiveLine1.WE_PalletIDInfo);

			receiveLine2.Validation.ValidateWE_PalletID();
			AssertNoErrors("The line should NOT have any errors since this inventory has no SOH.", receiveLine1.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_ValidatePutawayIntoPickFaceLocation

		public void TestCheckWE_PalletID_ValidatePutawayIntoPickFaceLocation_WithPalletIdErrorIsShown()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var pickFace = product.PickFaces.AddNew();
			var pickFaceLocation = locations[0];
			pickFace.WF_WL = pickFaceLocation.PK;
			var normalLocation = locations[1];

			pickFaceLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = false;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			receiveLine.WE_WL = pickFaceLocation.PK;
			receiveLine.WE_PalletID = "ABC";
			AssertHasError(receiveLine.WE_PalletIDInfo, "A Pallet ID cannot be entered as the Location is a Pick Face that does not Retain Pallet IDs.");

			receiveLine.WE_WL = normalLocation.PK;
			receiveLine.WE_PalletID = "ABC";
			AssertNoErrors("When location is not pick face - we don't mind to have a pallet id", receiveLine.WE_PalletIDInfo);

			receiveLine.WE_WL = pickFaceLocation.PK;
			receiveLine.WE_PalletID = "";
			AssertNoErrors("It is OK to not to have a palletId for pick face location", receiveLine.WE_PalletIDInfo);

			pickFaceLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;

			receiveLine.WE_WL = normalLocation.PK;
			receiveLine.WE_PalletID = "ABC";
			AssertNoErrors("When location is not pick face - we don't mind to have a pallet id", receiveLine.WE_PalletIDInfo);

			receiveLine.WE_WL = pickFaceLocation.PK;
			receiveLine.WE_PalletID = "ABC";
			AssertNoErrors("When WLT_RetainPalletIDsInFixedPickFaces is set to true, we can keep Pallet ID.", receiveLine.WE_PalletIDInfo);
		}

		public void TestCheckWE_PalletID_ValidatePutawayIntoPickFaceLocation_WithPalletIdDifferentClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var differentClient = Helper.CreateClient("C2", "C2");
			Helper.CreateProductClientRelationShip(differentClient, data.Part1);
			var pickFaceLocationForDifferentClient = locations[0];
			Helper.CreateProductPickFace(data.Part1, differentClient, pickFaceLocationForDifferentClient);
			var normailLocation = locations[1];

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			receiveLine.WE_WL = pickFaceLocationForDifferentClient.PK;
			receiveLine.WE_PalletID = "ABC";
			AssertNoErrors("Location is a pickface for a different client. Therefore, it should not have any errors.", receiveLine.WE_PalletIDInfo);

			receiveLine.WE_WL = pickFaceLocationForDifferentClient.PK;
			receiveLine.WE_PalletID = "";
			AssertNoErrors(receiveLine.WE_PalletIDInfo);
		}

		public void TestCheckWE_PalletID_ValidatePutawayIntoPickFaceLocation_WithPalletIdErrorIsNotShownIfOtherErrorExists()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var pickFace = product.PickFaces.AddNew();
			var pickFaceLocation = locations[0];
			pickFace.WF_WL = pickFaceLocation.PK;
			var normalLocation = locations[1];

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			receiveLine.WE_WL = pickFaceLocation.PK;
			receiveLine.WE_PalletID = "测试区";
			AssertNoError(receiveLine.WE_PalletIDInfo, "A Pallet ID cannot be entered as the Location is a Pick Face that does not Retain Pallet IDs.");

			receiveLine.WE_WL = pickFaceLocation.PK;
			receiveLine.WE_PalletID = "GRT";
			AssertHasError(receiveLine.WE_PalletIDInfo, "A Pallet ID cannot be entered as the Location is a Pick Face that does not Retain Pallet IDs.");
		}

		#endregion

		#endregion

		#region TestCheckWE_ClientOrderedUnits

		#region TestCheckWE_ClientOrderedUnits_LessThanReservedQuantity

		public void TestCheckWE_ClientOrderedUnits_LessThanReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_ClientOrderedUnits = 50m;
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50);
			Helper.CreateReservePickLine(orderLine, inventory, 50);

			AssertEquals("Pre-condition", 1, receive.Inventory.Count);
			AssertEquals("Pre-condition", 50m, receive.Inventory[0].WI_CrossDockQuantity);

			receiveLine.WE_ClientOrderedUnits = 10m;
			AssertHasError(receiveLine.WE_ClientOrderedUnitsInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);

			receiveLine.WE_ClientOrderedUnits = 50m;
			AssertNoErrors(receiveLine.WE_ClientOrderedUnitsInfo);
		}

		#endregion

		#region TestCheckWE_ClientOrderedUnits_LessThanReservedQuantity_TransactionQuantityAsReservedQuantity

		public void TestCheckWE_ClientOrderedUnits_LessThanReservedQuantity_TransactionQuantityAsReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50);
			Helper.CreateReservePickLine(orderLine, inventory, 50);

			AssertEquals("Pre-condition", 1, receive.Inventory.Count);
			AssertEquals("Pre-condition", 50m, receive.Inventory[0].WI_CrossDockQuantity);

			receive.PopulateASNLines();
			receiveLine.WE_ClientOrderedUnits = 10m;
			receiveLine.WE_TransactionQuantity = 50m;
			AssertNoErrors(receiveLine.WE_ClientOrderedUnitsInfo);
		}

		#endregion

		#region TestCheckWE_ClientOrderedUnits_LessThanReservedQuantity_ErrorClearedWhenTransactionQuantityBecomeReservedQuantity

		public void TestCheckWE_ClientOrderedUnits_LessThanReservedQuantity_ErrorClearedWhenTransactionQuantityBecomeReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			receiveLine.WE_TransactionQuantity = 10m;
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50);
			Helper.CreateReservePickLine(orderLine, inventory, 50);

			AssertEquals("Pre-condition", 1, receive.Inventory.Count);
			AssertEquals("Pre-condition", 50m, receive.Inventory[0].WI_CrossDockQuantity);

			receiveLine.WE_ClientOrderedUnits = 20m;
			receiveLine.WE_TransactionQuantity = 10m;
			AssertHasError(receiveLine.WE_ClientOrderedUnitsInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
			AssertNoErrors(receiveLine.WE_TransactionQuantityInfo);

			receiveLine.WE_TransactionQuantity = 50m;
			AssertNoErrors(receiveLine.WE_ClientOrderedUnitsInfo);
			AssertNoErrors(receiveLine.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_ClientOrderedUnits_LessThanReservedQuantity_EqualToTransactionQuantity

		public void TestCheckWE_ClientOrderedUnits_LessThanReservedQuantity_EqualToTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m);
			receiveLine.WE_TransactionQuantity = 10m;
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50);
			Helper.CreateReservePickLine(orderLine, inventory, 50);

			AssertEquals("Pre-condition", 1, receive.Inventory.Count);
			AssertEquals("Pre-condition", 50m, receive.Inventory[0].WI_CrossDockQuantity);

			receiveLine.WE_ClientOrderedUnits = 10m;
			AssertHasError(receiveLine.WE_ClientOrderedUnitsInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
			AssertHasError(receiveLine.WE_TransactionQuantityInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
		}

		#endregion

		#region TestCheckWE_ClientOrderedUnitsSerialNumberQtyCheck

		public void TestCheckWE_ClientOrderedUnitsSerialNumberQtyCheck()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithoutError1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLineWithoutError1.WE_SerialNumber = "SN01";
			var receiveLineWithError = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			receiveLineWithError.WE_SerialNumber = "SN02";
			receiveLineWithError.WE_TransactionQuantity = 1m;
			var receiveLineWithoutError2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, null, ZDate.Empty, ZDate.Empty, "", "", "", "").InDocketLine;
			receiveLineWithoutError2.WE_TransactionQuantity = 1m;

			receive.RunPreSaveValidation();

			AssertNoErrors(receiveLineWithoutError1.WE_ClientOrderedUnitsInfo);
			AssertNoErrors(receiveLineWithoutError2.WE_ClientOrderedUnitsInfo);
			AssertHasError(receiveLineWithError.WE_ClientOrderedUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#endregion

		#region TestCheckConsigneeNameOrPK

		public void TestCheckConsigneeNameOrPK()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = (WhsReceiveLine)data.Line111.InDocketLine;

			var consignee = Helper.CreateClient("CONSIGNEE");
			consignee.OH_IsConsignee = true;

			inventory.ConsigneeNameOrPK = ZGuid.Empty.ToString();
			AssertEquals("ConsigneePK is not mandatory field, should have no errors", false, inventory.ConsigneeNameOrPKInfo.HasErrors());

			inventory.ConsigneeNameOrPK = consignee.PK.ToString();
			AssertEquals("No errors expected when correct consignee organisation selected", false, inventory.ConsigneeNameOrPKInfo.HasErrors());

			inventory.ConsigneeNameOrPK = ZGuid.Invalid.ToString();
			AssertEquals("The Organisation selected doesn't excist, should be error", true, inventory.ConsigneeNameOrPKInfo.HasErrors());

			inventory.ConsigneeDocAddress.E2_AddressOverride = true;
			inventory.ConsigneeNameOrPK = "OVERRIDEN NAME";
			inventory.ConsigneeDocAddress.E2_Address1 = "OVERRIDEN ADDRESS 1";
			inventory.ConsigneeDocAddress.E2_Postcode = "00001";
			inventory.ConsigneeDocAddress.E2_City = "OVERRIDEN CITY";
			inventory.ConsigneeDocAddress.E2_State = "OVERRIDEN STATE";
			inventory.ConsigneeDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			AssertEquals("No errors expected when consignee is overriden", false, inventory.ConsigneeNameOrPKInfo.HasErrors());
			inventory.ConsigneeDocAddress.E2_AddressOverride = false;

			Factory.Save(); // to generate DocketLine and change ConsigneeDocAddress from inventory to docketLine

			inventory.ConsigneeNameOrPK = ZGuid.Empty.ToString();
			AssertEquals("ConsigneePK is not mandatory field, should have no errors", false, inventory.ConsigneeNameOrPKInfo.HasErrors());

			inventory.ConsigneeNameOrPK = consignee.PK.ToString();
			AssertEquals("No errors expected when correct consignee organisation selected", false, inventory.ConsigneeNameOrPKInfo.HasErrors());

			inventory.ConsigneeNameOrPK = ZGuid.Invalid.ToString();
			AssertEquals("The Organisation selected doesn't excist, should be error", true, inventory.ConsigneeNameOrPKInfo.HasErrors());

			inventory.ConsigneeDocAddress.E2_AddressOverride = true;
			inventory.ConsigneeNameOrPK = "OVERRIDEN NAME";
			inventory.ConsigneeDocAddress.E2_Address1 = "OVERRIDEN ADDRESS 1";
			inventory.ConsigneeDocAddress.E2_Postcode = "00001";
			inventory.ConsigneeDocAddress.E2_City = "OVERRIDEN CITY";
			inventory.ConsigneeDocAddress.E2_State = "OVERRIDEN STATE";
			inventory.ConsigneeDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			AssertEquals("No errors expected when consignee is overriden", false, inventory.ConsigneeNameOrPKInfo.HasErrors());
		}

		public void TestCheckConsigneeNameOrPK_ConsigneeDocAddressIsDeleted()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = (WhsReceiveLine)data.Line111.InDocketLine;
			inventory.ConsigneeNameOrPK = "ORIGINAL NAME";

			inventory.ConsigneeDocAddress.E2_AddressOverride = true;
			inventory.ConsigneeDocAddress.Delete();
			inventory.ConsigneeNameOrPK = "OVERRIDEN NAME";
			AssertEquals("No errors expected when consignee is overriden", false, inventory.ConsigneeNameOrPKInfo.HasErrors());
		}

		#endregion

		#region TestCheckConsigneeNameOrPK_AlwaysShowsCorrectMessages

		public void TestCheckConsigneeNameOrPK_AlwaysShowsCorrectMessages()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var docketLine = (WhsReceiveLine)data.Line111.InDocketLine;

			OrgHeader consignee = Helper.CreateClient();
			consignee.OH_IsConsignee = true;

			docketLine.ConsigneeNameOrPK = consignee.PK.ToString();
			AssertEquals("E2_OA_AddressInfo should not have an error", false, docketLine.ConsigneeDocAddress.E2_OA_AddressInfo.HasErrors());
			AssertEquals("ConsigneeNameOrPKInfo should not have an error", false, docketLine.ConsigneeNameOrPKInfo.HasErrors());

			// Trying to reproduce what happening in GUI when 'fake' error stay bind to property.
			// For example if First entered org code was invalid, but then the org got either added to system or changed to have correct type.
			// Can't be reproduced in business, because ConsigneeNameOrPK will look for Guid value and not string.
			// When adding non existing Guid value, ConsigneeNameOrPK will be changed to have ZGuid.Invalid value ranter that the Guid value we wanted.
			docketLine.WE_OP = ZGuid.Empty; // to generate some error we can use
			docketLine.ConsigneeDocAddress.E2_OA_AddressInfo.AddAllNotificationsFrom(data.Line111.WI_OPInfo);
			docketLine.ConsigneeNameOrPKInfo.AddAllNotificationsFrom(docketLine.WE_OPInfo);
			AssertEquals("E2_OA_AddressInfo should have an error", true, docketLine.ConsigneeDocAddress.E2_OA_AddressInfo.HasErrors());
			AssertEquals("ConsigneeNameOrPKInfo should have an error", true, docketLine.ConsigneeNameOrPKInfo.HasErrors());

			// check ValidateConsigneeNameOrPK dispose of no longer correct errors
			((WhsReceiveLineValidation)docketLine.Validation).ValidateConsigneeNameOrPK();
			AssertEquals("E2_OA_AddressInfo should have been revalidated and no errors expected", false, docketLine.ConsigneeDocAddress.E2_OA_AddressInfo.HasErrors());
			AssertEquals("ConsigneeNameOrPKInfo should have been revalidated and no errors expected", false, docketLine.ConsigneeNameOrPKInfo.HasErrors());
		}

		#endregion

		#region TestCheckForPendingOrders_Performance

		public void TestCheckForPendingOrders_Performance_DBHits()
		{
			const int numDockets = 30;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Factory.Save();

			for (int i = 1; i < numDockets; i++)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R" + i, data.Part1, 10m, data.Whs1.DefaultLocation, string.Empty, false);
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (var j = 0; j < numDockets; j++)
			{
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			}

			for (var k = 0; k < numDockets * 2; k++)
			{
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{k}", data.Part1, 5m);
			}

			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			var anotherFactory = new BusinessObjectFactory();
			var receiveInAnotherFactory = anotherFactory.Load<WhsReceive>(receive.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, anotherFactory))
			using (RowFactory.SetCachedTables())
			{
				receiveInAnotherFactory.RunPreSaveValidation();
			}
		}

		public void TestCheckForPendingOrders_Performance_MultipleProductsOnReceive_DBHits()
		{
			const int numProducts = 10;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var productList = new List<OrgSupplierPart>();
			for (var n = 1; n <= numProducts; n++)
			{
				var product = Helper.CreateProduct(n.ToString(), data.Org1);
				productList.Add(product);
			}
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (var j = 0; j < numProducts; j++)
			{
				Helper.CreateWhsReceiveLine(receive, productList[j], 10m, data.Whs1.DefaultLocation);
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{j}", productList[j], 5m);
			}
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			var anotherFactory = new BusinessObjectFactory();
			var receiveInAnotherFactory = anotherFactory.Load<WhsReceive>(receive.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, anotherFactory))
			using (RowFactory.SetCachedTables())
			{
				receiveInAnotherFactory.RunPreSaveValidation();
			}
		}

		public void TestCheckForPendingOrders_Performance_MultipleProductsOnReceive_ProductWithNoPendingOrder_DBHits()
		{
			const int numProducts = 10;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var productList = new List<OrgSupplierPart>();
			for (var n = 1; n <= numProducts; n++)
			{
				var product = Helper.CreateProduct(n.ToString(), data.Org1);
				productList.Add(product);
			}
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (var j = 0; j < numProducts; j++)
			{
				Helper.CreateWhsReceiveLine(receive, productList[j], 10m, data.Whs1.DefaultLocation);
			}

			for (var k = 0; k < numProducts - 1; k++)
			{
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{k}", productList[k], 5m);
			}
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			var anotherFactory = new BusinessObjectFactory();
			var receiveInAnotherFactory = anotherFactory.Load<WhsReceive>(receive.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, anotherFactory))
			using (RowFactory.SetCachedTables())
			{
				receiveInAnotherFactory.RunPreSaveValidation();
			}
		}

		#endregion

		// Calculated properties

		#region TestCheckProductUQ

		public void TestCheckProductUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, false, false);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15);
			Factory.Save();
			var receiveLine1 = receive.Lines[0];
			var receiveLine2 = receive.Lines[1];
			var lineWithTempProduct1 = (ISupportTemporaryProduct)receiveLine1;
			var lineWithTempProduct2 = (ISupportTemporaryProduct)receiveLine2;

			AssertNoWarning(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning);
			AssertNoError(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			data.Part1.OP_StockKeepingUnit = "";
			AssertNoWarning(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning);
			AssertNoError(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine1.WE_OP = ZGuid.Empty;
			AssertNoWarning(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning);
			AssertNoError(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine1.WE_OP = ZGuid.Invalid;
			receiveLine1.ProductCode = "NewProduct";
			AssertNoWarning(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning);
			AssertNoError(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			lineWithTempProduct1.ProductUQ = "111";
			AssertHasError(lineWithTempProduct1.ProductUQInfo, "Enter a valid UQ.");

			lineWithTempProduct1.ProductUQ = "";
			AssertHasWarning(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning);
			AssertNoError(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			// Same Product same StockKeepingUnit - No errors
			receiveLine2.WE_OP = ZGuid.Invalid;
			receiveLine2.ProductCode = "NewProduct";
			AssertNoError(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);
			AssertNoError(lineWithTempProduct2.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			// Same Product different StockKeepingUnit - All lines should have errors
			lineWithTempProduct2.ProductUQ = "BOX";
			AssertHasError(lineWithTempProduct2.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);
			AssertNoError(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			((WhsReceiveLineValidation)receiveLine1.Validation).ValidateProductUQ();
			AssertHasError(lineWithTempProduct1.ProductUQInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);
		}

		#endregion

		#region TestCheckCommodityCode

		public void TestCheckCommodityCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, false, false);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15);

			var commodityCode = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCode.RH_Code = "CODE";

			Factory.Save();
			var receiveLine1 = receive.Lines[0];
			var receiveLine2 = receive.Lines[1];

			AssertEquals(false, receiveLine1.CommodityCodeInfo.HasErrors());
			AssertNoError(receiveLine1.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			data.Part1.OP_RH_NKCommodityCode = "CODE";
			AssertEquals(false, receiveLine1.CommodityCodeInfo.HasErrors());
			AssertNoError(receiveLine1.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine1.WE_OP = ZGuid.Empty;
			AssertEquals(false, receiveLine1.CommodityCodeInfo.HasErrors());
			AssertNoError(receiveLine1.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine1.WE_OP = ZGuid.Invalid;
			receiveLine1.ProductCode = "NewProduct";
			AssertEquals(false, receiveLine1.CommodityCodeInfo.HasErrors());
			AssertNoError(receiveLine1.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine1.CommodityCode = "1234";
			AssertEquals(true, receiveLine1.CommodityCodeInfo.HasErrors());
			AssertNoError(receiveLine1.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine1.CommodityCode = "CODE";
			AssertEquals(false, receiveLine1.CommodityCodeInfo.HasErrors());
			AssertNoError(receiveLine1.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			// Same Product same CommodityCode - No errors
			receiveLine2.WE_OP = ZGuid.Invalid;
			receiveLine2.ProductCode = "NewProduct";
			receiveLine2.CommodityCode = "CODE";
			AssertNoError(receiveLine1.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);
			AssertNoError(receiveLine2.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			// Same Product different CommodityCode - All lines should have errors
			receiveLine2.CommodityCode = "";
			AssertHasError(receiveLine2.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);
			AssertNoError(receiveLine1.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			((WhsReceiveLineValidation)receiveLine1.Validation).ValidateCommodityCode();
			AssertHasError(receiveLine1.CommodityCodeInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);
		}

		#endregion

		#region TestCheckWE_WL

		#region TestCheckWE_WL

		public void TestCheckWE_WL()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertNoErrors("Empty location is allowed before finalise.", receiveLine.WE_WLInfo);

			receiveLine.WE_WL = locations[0].PK;
			AssertNoErrors("Entered locations are good too.", receiveLine.WE_WLInfo);

			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_WL = ZGuid.Empty;
				AssertHasErrors("Location should have an error if empty during finalise.", receiveLine.WE_WLInfo);

				receiveLine.WE_WL = locations[0].PK;
				AssertNoErrors("Location should have no errors if location is correct during finalise.", receiveLine.WE_WLInfo);
			}
		}

		protected override bool IsLocationStringFieldUsed
		{
			get { return false; } // receive form uses WE_WL instead of LocationsString property
		}

		#endregion

		#region TestCheckWE_WL_IsInCorrectWarehouse

		public void TestCheckWE_WL_IsInCorrectWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var expectedErrorMessage = "Location does not belong to the selected Warehouse.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			receiveLine.WE_WL = whs2.DefaultLocation.PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = data.Whs1.DefaultLocation.PK;
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_NotVoidLocation

		public void TestCheckWE_WL_NotVoidLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_LocationStatus = LocationStatus.Codes.Void;
			var expectedErrorMessage = "Please enter a valid location, the Location you entered is Void.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			receiveLine.WE_WL = locations[0].PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = locations[1].PK;
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_NotPackingStationLocation

		public void TestCheckWE_WL_NotPackingStationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			var expectedErrorMessage = "Please enter a valid location, the Location you entered is a Packing Station Location.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			receiveLine.WE_WL = locations[0].PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = locations[1].PK;
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_NotPackingConsolidationLocation

		public void TestCheckWE_WL_NotPackingConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var packingConsolidationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.CON);
			var packingConsolidationLocation = locations[0];
			packingConsolidationLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;
			var expectedErrorMessage = "Please enter a valid location, the Location you entered is a Packing Consolidation Location.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			receiveLine.WE_WL = locations[0].PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = locations[1].PK;
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_NotInwardProcessingLocation

		public void TestCheckWE_WL_NotInwardProcessingLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WA_PickingArea = inwardProcessingArea.PK;
			locations[0].WLV_WA_PutawayArea = inwardProcessingArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			const string expectedErrorMessage = "Please enter a valid location, the Location you have entered is in an Inward Processing area.";
			receiveLine.WE_WL = locations[0].PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = locations[1].PK;
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_IsInwardProcessing

		public void TestCheckWE_WL_IsInwardProcessing()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[1].WLV_WA_PickingArea = inwardProcessingArea.PK;
			locations[1].WLV_WA_PutawayArea = inwardProcessingArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;

			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			receiveLine.WE_WL = ZGuid.Empty;
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			const string expectedErrorMessage = "Please enter a valid location, the Location you have entered is not in an Inward Processing area.";
			receiveLine.WE_WL = locations[0].PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = locations[1].PK;
			AssertNoErrors(receiveLine.WE_WLInfo);
		}

		#endregion

		#region TestCheckWE_WL_ForReceiptEntry

		public void TestCheckWE_WL_ForReceiptEntry()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations[0], 5m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres); // max Weight specified
			Helper.SetLocationMaxWeightAndVolume(locations[1], 0m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres); // no max values specified
			Helper.SetProductWeightAndVolume(data.Part1, 1m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres);
			Factory.Save();

			var expectedErrorMessage = "Total required Weight (10.00 KG) exceeds the maximum available Weight (5.00 KG) for this location.";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			receiveLine.WE_WL = locations[0].PK;
			AssertHasWarning(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = locations[1].PK;
			AssertNoWarning(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_FixedLocaitons

		public void TestCheckWE_WL_FixedLocaitons()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var fixLocationType = Helper.CreateLocationType("XY1", "Test1", false, 1, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("XY2", "Test2", false, 0, LocationClasses.Codes.NOR);

			var fixedLocation = data.Whs1.FindLocation("A-1");
			fixedLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var normalLocation = data.Whs1.FindLocation("A-2");
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, "A-1");
			Factory.Save();

			var expectedErrorMessage = string.Format("This location is a fixed pick face location and '{0}' is not assigned to this location.", data.Part2.OP_PartNum);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m).InDocketLine;
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = fixedLocation.PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = normalLocation.PK;
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WLForDockDoorLocations

		public void TestCheckWE_WLForDockDoorLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			receiveLine.WE_WL = dockDoorLocation.PK;
			AssertNoErrors(receiveLine.WE_WLInfo);
			receiveLine.WE_WL = ZGuid.Empty;
			AssertNoErrors(receiveLine.WE_WLInfo);

			receiveLine.WE_WL = nonDockDoorLocation.PK;
			AssertNoErrors(receiveLine.WE_WLInfo);
		}

		#endregion

		#region TestWE_WL_DynamicLocations

		public void TestWE_WL_IsNotDynamicLocationForNonDynamicProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);

			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var normalLocation = locations[0];
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var dynamicLocation = locations[1];
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2m, dynamicLocation, "").InDocketLine;
			AssertHasError(receiveLine1.WE_WLInfo, $"Cannot put product {data.Part1.OP_PartNum} in dynamic location {dynamicLocation.ToLocationString()}, as it is not a dynamic product.");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 2m, normalLocation, "").InDocketLine;
			AssertNoErrors("Can receive into non dynamic area", receiveLine2.WE_WLInfo);

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3", Notify);
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 2m, dynamicLocation, "").InDocketLine;
			AssertNoErrors("Can now receive dynamic product into dynamic area", receiveLine3.WE_WLInfo);
		}

		public void TestWE_WL_IsLocatedInCorrectDynamicAreaForDynamicProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var dynamicLocation1 = locations[0];
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;

			var dynamicLocation2 = locations[1];
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2m, dynamicLocation2, "").InDocketLine;
			AssertHasError(receiveLine1.WE_WLInfo, $"Cannot put dynamic product {data.Part1.OP_PartNum} in dynamic location {dynamicLocation2.ToLocationString()}, as the location is not within the product's designated dynamic area ({dynamicArea1.WA_Name}).");

			productParams.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 2m, dynamicLocation2, "").InDocketLine;

			AssertNoErrors("Can putaway as product is assigned to a dynamic location within the correct area", receiveLine2.WE_WLInfo);
		}

		public void TestWE_WL_CanBeNonPickfaceLocationForDynamicProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var normalLocation = locations[0];
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var dynamicLocation1 = locations[1];
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea.PK;

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2m, normalLocation, "").InDocketLine;
			AssertNoErrors("Can receive dynamic product as location is not a pick face", receiveLine1.WE_WLInfo);
		}

		#endregion

		#region TestCheckWE_WLInventoryStatusOriginalValueIsReceived

		public void TestCheckWE_WLInventoryStatusOriginalValueIsReceived()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation);
			Factory.Save();

			AssertEquals("Precondition: location is not empty", false, receiveLine.WE_WL.IsEmpty);
			AssertEquals("Precondition: Inventory status is Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);

			var newLocation = data.Whs1.FindLocation("A-1");
			receiveLine.WE_WL = newLocation.PK;
			AssertHasError(receiveLine.WE_WLInfo, "Location cannot be updated if the original value of the inventory status is Received.");
		}

		public void TestCheckWE_WLInventoryStatusOriginalValueIsReceived_NotInDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			receiveLine.WE_WL = dockDoorLocation.PK;
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;

			AssertEquals("Precondition: location is not empty", false, receiveLine.WE_WL.IsEmpty);
			AssertEquals("Precondition: Inventory status is Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);

			var newLocation = data.Whs1.FindLocation("A-1");
			receiveLine.WE_WL = newLocation.PK;
			AssertNoErrors("There is no error receive line is not yet saved in the database.", receiveLine.WE_WLInfo);
		}

		public void TestCheckWE_WLInventoryStatusOriginalValueIsReceived_UpdateWithSameValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var anotherLocation = data.Whs1.FindLocation("A-1");
			receiveLine.WE_PalletID = "ABC123";
			receiveLine.WE_WL = dockDoorLocation.PK;
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			Factory.Save();

			AssertEquals("Precondition: location is not empty", false, receiveLine.WE_WL.IsEmpty);
			AssertEquals("Precondition: Inventory status is Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);

			receiveLine.WE_WL = dockDoorLocation.PK;
			AssertNoError("There is no error since the location is the same as the original location.", receiveLine.WE_WLInfo, "Location cannot be updated if the original value of the inventory status is Received.");

			receiveLine.WE_WL = anotherLocation.PK;
			AssertHasError("DockDoor location validation is removed in new inbound dock door functionality.", receiveLine.WE_WLInfo, "Location cannot be updated if the original value of the inventory status is Received.");
		}

		#endregion

		#region TestCheckIsPalletIdAssignedOnDockDoorLocation

		public void TestCheckIsPalletIdAssignedOnDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Precondition: Pallet id is empty.", ZString.Empty, receiveLine.WE_PalletID);
			AssertNoError("Precondition: There is no error as location is not assigned.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID entered when using dock door locations.");

			receiveLine.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			AssertHasError("There is an error as pallet id is empty.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID entered when using dock door locations.");

			receiveLine.WE_PalletID = "ABCD";
			AssertNoError("There is no more error as pallet id is now assigned.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID entered when using dock door locations.");

			receiveLine.WE_PalletID = "";
			AssertHasError("There is an error as pallet id is empty.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID entered when using dock door locations.");

			receiveLine.WE_WL = data.Whs1.DefaultLocation.PK;
			AssertEquals("Precondition: location is not a dockdoor location.", false, data.Whs1.DefaultLocation.IsDockDoorLocation);
			AssertNoError("There is no error as location is not dockdoor.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID entered when using dock door locations.");
		}

		public void TestCheckIsPalletIdAssignedOnDockDoorLocation_WhenCrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationType = Helper.CreateLocationType("XDC", LocationClasses.Codes.DDL);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			crossDockLocation.WLV_WLT_LocationType = locationType.PK;
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			order.WD_WL_CrossDock = crossDockLocation.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var pickLine = order.Lines[0].ReserveStockIfAbleTo(receiveLine.Inventory[0]);
			AssertEquals("Precondition: Stock is reserved", 1m, pickLine.ReservedQuantity);
			AssertEquals("Precondition: Pallet id is empty.", ZString.Empty, receiveLine.WE_PalletID);
			AssertNoErrors("Precondition: There is no error as location is not assigned.", receiveLine.WE_PalletIDInfo);

			receiveLine.WE_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			AssertEquals("Precondition: Location is a Dock Door.", true, receiveLine.Location.IsDockDoorLocation);
			AssertHasError("There is an error as pallet id is empty.", receiveLine.WE_PalletIDInfo, "You must have a Pallet ID entered when using dock door locations.");

			receiveLine.WE_WL = crossDockLocation.PK;
			AssertEquals("Precondition: Location is a Dock Door.", true, receiveLine.Location.IsDockDoorLocation);
			AssertNoErrors("There is no more error as Location is the Cross Dock Location.", receiveLine.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_WL_ReceivedLineIsNotPutaway

		public void TestCheckWE_WL_ReceivedLineIsNotPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 12m, dockDoor, "A");

			AssertEquals("Precondition: The received receiveline is not finalised.", false, receive.IsFinalised);

			receive.FinaliseDocket();
			AssertHasError("WE_WL is a dock door location, error should show.", receiveLine.WE_WLInfo,
				"This 'Received To Dock Door' receive line is still in a dock door location.\r\n Please either change Location to a non-dock door location or utilize putaway transfer to move the stock.");

			receiveLine.WE_WL = nonDockDoor.PK;
			receive.FinaliseDocket();
			AssertNoError("WE_WL is a putaway location, error should not show.", receiveLine.WE_WLInfo,
				"This 'Received To Dock Door' receive line is still in a dock door location.\r\n Please either change Location to a non-dock door location or utilize putaway transfer to move the stock.");
		}

		public void TestCheckWE_WL_ReceivedLineIsNotPutaway_NotRunWhenStockOnHandIsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m, data.Whs1.DefaultOutboundDockDoorLocation, "A");
			Factory.Save();

			AssertEquals("Precondition: The receive is not finalised.", false, receive.IsFinalised);
			AssertEquals("Precondition: The receiveline has no stock on hand.", 0m, receiveLine.WE_StockOnHand);

			receive.FinaliseDocket();
			AssertNoErrors("Receive line has no stock on hand, error should not show.", receiveLine.WE_WLInfo);
			AssertEquals(true, receive.IsFinalised);
		}

		#endregion

		#region TestCheckWE_WL_CustomsReceipt

		public void TestCheckWE_WL_CustomsReceipt_FreeStore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var freeStoreArea = Helper.CreateArea(data.Whs1, "FREE", AreaTypes.Codes.FreeStore);
			var freeStoreLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "freestore", 1, 1).Locations[0];
			freeStoreLocation.WLV_WA_PutawayArea = freeStoreArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertNoErrors(receiveLine.WE_WLInfo);

			receiveLine.WE_WL = freeStoreLocation.PK;
			AssertHasError(receiveLine.WE_WLInfo, "A Customs Receipt can only receive into locations that are in a Bonded or Excise Area.");
		}

		public void TestCheckWE_WL_CustomsReceipt_Bonded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "bonded", 1, 1).Locations[0];
			bondedLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertNoErrors(receiveLine.WE_WLInfo);

			receiveLine.WE_WL = bondedLocation.PK;
			AssertNoErrors(receiveLine.WE_WLInfo);
		}

		public void TestCheckWE_WL_CustomsReceipt_Excise()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var exciseArea = Helper.CreateArea(data.Whs1, "EXCArea", AreaTypes.Codes.Excise);
			var exciseLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "AA", 1, 1).Locations[0];
			exciseLocation.WLV_WA_PutawayArea = exciseArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertNoErrors(receiveLine.WE_WLInfo);

			receiveLine.WE_WL = exciseLocation.PK;
			AssertNoErrors(receiveLine.WE_WLInfo);
		}

		public void TestCheckWE_WL_CustomsReceipt_DynamicPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dynamicPickFaceArea = Helper.CreateArea(data.Whs1, "DPFArea", AreaTypes.Codes.DynamicPickFace, isPutawayArea: false);
			var dynamicPickFaceLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "BB", 1, 1).Locations[0];
			dynamicPickFaceLocation.WLV_WA_PickingArea = dynamicPickFaceArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertNoErrors(receiveLine.WE_WLInfo);

			receiveLine.WE_WL = dynamicPickFaceLocation.PK;
			AssertHasError(receiveLine.WE_WLInfo, "A Customs Receipt can only receive into locations that are in a Bonded or Excise Area.");
		}

		#endregion

		#region TestCheckWE_WL_GoodsReceipt

		public void TestCheckWE_WL_GoodsReceipt_FreeStore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var freeStoreArea = Helper.CreateArea(data.Whs1, "FREE", AreaTypes.Codes.FreeStore);
			var freeStoreLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "freestore", 1, 1).Locations[0];
			freeStoreLocation.WLV_WA_PutawayArea = freeStoreArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertNoErrors(receiveLine.WE_WLInfo);
		}

		public void TestCheckWE_WL_GoodsReceipt_BondedArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "Bonded", AreaTypes.Codes.Bonded);
			var bondedAreaLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "BB", 1, 1).Locations[0];
			bondedAreaLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertNoErrors("Precondition: No errors on WE_WL at setup", receiveLine.WE_WLInfo);

			var errorMessage = "A Goods Receipt cannot receive into locations in a Bonded or Excise Area.";
			receiveLine.WE_WL = bondedAreaLocation.PK;
			AssertHasError(receiveLine.WE_WLInfo, errorMessage);

			receiveLine.WE_WL = data.Whs1.FindLocation("A-1").PK;
			AssertNoErrorContaining(receiveLine.WE_WLInfo, errorMessage);
		}

		public void TestCheckWE_WL_GoodsReceipt_Excise()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var exciseArea = Helper.CreateArea(data.Whs1, "EXCArea", AreaTypes.Codes.Excise);
			var exciseLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "AA", 1, 1).Locations[0];
			exciseLocation.WLV_WA_PutawayArea = exciseArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var errorMessage = "A Goods Receipt cannot receive into locations in a Bonded or Excise Area.";
			receiveLine.WE_WL = exciseLocation.PK;
			AssertHasError(receiveLine.WE_WLInfo, errorMessage);

			receiveLine.WE_WL = data.Whs1.FindLocation("A-1").PK;
			AssertNoErrorContaining(receiveLine.WE_WLInfo, errorMessage);
		}

		public void TestCheckWE_WL_GoodsReceipt_DynamicPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dynamicPickFaceArea = Helper.CreateArea(data.Whs1, "DPFArea", AreaTypes.Codes.DynamicPickFace, isPutawayArea: false);
			var dynamicPickFaceLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "BB", 1, 1).Locations[0];
			dynamicPickFaceLocation.WLV_WA_PickingArea = dynamicPickFaceArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertNoErrors(receiveLine.WE_WLInfo);
		}

		#endregion

		#region TestCheckWE_WL_StockOnHand_LineWithoutProductYetEnteredDoesNotFail

		public void TestCheckWE_WL_StockOnHand_LineWithoutProductYetEnteredDoesNotFail()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = receive.Lines.AddNew();
			receiveLine.WE_TransactionQuantity = 1m;

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertNoExceptionThrown(() => receiveLine.WE_WL = locationA.PK);
				AssertNoWarnings(receiveLine.WE_WLInfo);
			}
		}

		#endregion

		#region TestCheckWE_WL_PickByBOMReceives_IgnorePackingStationCheck

		public void TestCheckWE_WL_PickByBOMReceives_IgnorePackingStationCheck()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			var expectedErrorMessage = "Please enter a valid location, the Location you entered is a Packing Station Location.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			receiveLine.WE_WL = locations[0].PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receive.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();
			receiveLine.Validation.ValidateWE_WL();
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_PickByBOMReceives_IgnorePackingConsolidationLocationCheck

		public void TestCheckWE_WL_PickByBOMReceives_IgnorePackingConsolidationLocationCheck()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var packingConsolidationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.CON);
			var packingConsolidationLocation = locations[0];
			packingConsolidationLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;
			var expectedErrorMessage = "Please enter a valid location, the Location you entered is a Packing Consolidation Location.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_WLInfo);

			receiveLine.WE_WL = locations[0].PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receive.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();
			receiveLine.Validation.ValidateWE_WL();
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_PickByBOMReceives_IgnoreReceiptEntryCheck

		public void TestCheckWE_WL_PickByBOMReceives_IgnoreReceiptEntryCheck()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part2, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 3m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "3");
			Helper.CreateWhsOrderLine(order3, data.Part2, 4m);

			Factory.Save(); // for dbonly query in validation
			var warnMessage = "This product is required by the following pending order(s): 1, 2";

			receiveLine.WE_WL = data.Whs1.FindLocation("A-1-1").PK; // "A-1-1"
			AssertNoWarning("Should have no warning because order lines are for diff product", receiveLine.WE_WLInfo, warnMessage);

			orderLine1.WE_OP = data.Part1.PK;
			orderLine2.WE_OP = data.Part1.PK;
			Factory.Save(); // for dbonly query in validation

			receiveLine.Validation.ValidateWE_WL();
			AssertHasWarning(receiveLine.WE_WLInfo, warnMessage);

			receive.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();
			receiveLine.Validation.ValidateWE_WL();
			AssertNoWarning("Should have no warning because inventory is cross docked", receiveLine.WE_WLInfo, warnMessage);
		}

		#endregion

		#region TestCheckWE_WL_PickByBOMReceives_IgnorePickFaceCheck

		public void TestCheckWE_WL_PickByBOMReceives_IgnorePickFaceCheck()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var fixLocationType = Helper.CreateLocationType("XY1", "Test1", false, 1, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("XY2", "Test2", false, 0, LocationClasses.Codes.NOR);

			var fixedLocation = data.Whs1.FindLocation("A-1");
			fixedLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var normalLocation = data.Whs1.FindLocation("A-2");
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, "A-1");
			Factory.Save();

			var expectedErrorMessage = string.Format("This location is a fixed pick face location and '{0}' is not assigned to this location.", data.Part2.OP_PartNum);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m).InDocketLine;
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receiveLine.WE_WL = fixedLocation.PK;
			AssertHasError(receiveLine.WE_WLInfo, expectedErrorMessage);

			receive.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();
			receiveLine.Validation.ValidateWE_WL();
			AssertNoError(receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		// Test for location capacity (weight, volume and quanitity is in WhsValidationHelper)

		#endregion

		#region TestCheckWE_WLForInTransitInventory_StockOnHand

		public void TestCheckWE_WLForInTransitInventory_StockOnHand()
		{
			// Objective: Verify that stock on hand validation works correctly when inventory is in transit to location/pallet
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedWarningMsg = "Stock On Hand exists.\r\nClient: 111, Product: P1";

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				Helper.CreateProductUnit(data.Part1, "UNT", "PLT", 100m);   // To avoid undesired warnings related to pallet units
				Factory.Save();

				var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
				var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer1, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
				Factory.Save();

				AssertNoWarnings("Precondition: Transfer should show no warnings.", transferLine1.WE_WLInfo);

				var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part1, 100m, locationB, "PLT111", allocateLocations: true, finalise: false);
				var receiveLine = receive2.Lines.Single();
				AssertHasWarning("Receive should show warning since another pallet with that product (123) is in transit to same location.", receiveLine.WE_WLInfo, expectedWarningMsg);

				receiveLine.WE_PalletID = "PLT123";
				receiveLine.Validation.ValidateWE_WL();
				AssertNoWarnings("Receive should NOT show warning since it's assumed that we're adding inventory to PLT123 already in transit to that location.", receiveLine.WE_WLInfo);
			}
		}

		#endregion

		#region TestCheckWE_WL_TotalPallets

		public void TestCheckWE_WL_TotalPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-2");

			receive.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive.FinaliseDocket();

			var expectedErrorMessage = "Total required Pallets (2) exceeds the maximum available Pallets (1) for this location.";

			AssertHasError(receiveLine1.WE_WLInfo, expectedErrorMessage);
			AssertHasError(receiveLine2.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_EmptyPalletID

		public void TestCheckWE_WL_EmptyPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1);
			receive.FinaliseDocket();

			AssertHasError(receiveLine1.WE_WLInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
		}

		#endregion

		#region TestCheckWE_PalletID_EmptyPalletID

		public void TestCheckWE_PalletID_EmptyPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1);
			receive.FinaliseDocket();

			AssertHasError(receiveLine1.WE_PalletIDInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
		}

		#endregion

		#region TestCheckWE_WL_MixedProducts

		public void TestCheckWE_WL_MixedProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 2;
			location1.WLV_PalletStackHeight = 1;
			Helper.CreateProductUnit(data.Part1, "PLT", 500m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 500m, location1, "Pallet-2");

			receive.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive.FinaliseDocket();

			var expectedErrorMessage =
				"Only a single product can be used in locations using Pallet Space capacities.";

			AssertHasError(receiveLine1.WE_WLInfo, expectedErrorMessage);
			AssertHasError(receiveLine2.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_WL_NoCapacityConstraints

		public void TestCheckWE_WL_NoCapacityConstraints()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation("A-1-1");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 500m, location, "Pallet-2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-3");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 500m, location, "Pallet-4");
			receive.FinaliseDocket();

			AssertNoErrors(receiveLine1);
			AssertNoErrors(receiveLine2);
		}

		#endregion

		#region TestCheckWE_WL_MultiplePalletsInDbAndMemory

		public void TestCheckWE_WL_MultiplePalletsInDbAndMemory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 3;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-2");

			receive.FinaliseDocket();
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 500m, location1, "Pallet-4");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 500m, location1, "Pallet-3");

			receive2.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive2.LocationCapacityValidationManager.ClearLocationAvailableCapacityForTest();
			receive2.FinaliseDocket();

			var expectedErrorMessage = "Total required Pallets (2) exceeds the maximum available Pallets (1) for this location.";
			AssertHasError(receiveLine3.WE_WLInfo, expectedErrorMessage);
			AssertHasError(receiveLine4.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_OP_PalletConversion

		public void TestCheckWE_OP_WithoutPalletConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 2;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();

			AssertHasWarning(receiveLine1.WE_OPInfo, "Products without pallet conversions cannot be put in locations using pallet spaces.");
		}

		public void TestCheckWE_OP_WithPalletConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 2;
			location1.WLV_PalletStackHeight = 1;
			Helper.CreateProductUnit(data.Part1, "PLT", 500m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();

			AssertEquals(500m, data.Part1.OP_StockKeepingUnitPerPallet);
			AssertNoWarnings(receiveLine1.WE_OPInfo);
		}

		#endregion

		#region TestDateOutOfRange

		public void TestDateOutOfRange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			AssertNotEquals(InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals(false, receiveLine.HasPutawayTransfer);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_ExpiryDateInfo, shouldHaveError: true);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_PackingDateInfo, shouldHaveError: true);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_RequiredByDateInfo, shouldHaveError: true);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_AdjustmentArrivalDateInfo, shouldHaveError: true);
		}

		public void TestDateOutOfRange_ReceivedInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "A");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			Factory.Save();

			AssertEquals("Precondition: receive line's inventory status is RECEIVED.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Expiry date is read-only", true, receiveLine.WE_ExpiryDateInfo.ReadOnly);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_ExpiryDateInfo, shouldHaveError: false);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_PackingDateInfo, shouldHaveError: false);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_RequiredByDateInfo, shouldHaveError: false);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_AdjustmentArrivalDateInfo, shouldHaveError: false);
		}

		public void TestDateOutOfRange_HasPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "A");
			Factory.Save();

			receiveLine.WE_ExpiryDate = ZDate.Today.AddYears(1);
			AssertNoErrors(receiveLine.WE_ExpiryDateInfo);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "A", 10m);
			transferLine.WE_ExpiryDate = ZDate.Today.AddYears(1);
			transfer.RunPreSaveValidation();
			Factory.Save();

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			AssertEquals("Precondition: receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);
			AssertEquals("Precondition: Expiry date is read-only", true, receiveLine.WE_ExpiryDateInfo.ReadOnly);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_ExpiryDateInfo, shouldHaveError: false);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_PackingDateInfo, shouldHaveError: false);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_RequiredByDateInfo, shouldHaveError: false);
			CheckDateErrorsForOutOfRangeDate(receiveLine.WE_AdjustmentArrivalDateInfo, shouldHaveError: false);
		}

		void CheckDateErrorsForOutOfRangeDate(ZPropertyInfo propertyInfo, bool shouldHaveError)
		{
			AssertNoErrors(propertyInfo);
			if (propertyInfo.PropertyType == typeof(ZDateTimeOffset))
			{
				propertyInfo.Value = new ZDateTimeOffset(1950, 1, 1);
			}
			else if (propertyInfo.PropertyType == typeof(ZDateTime))
			{
				propertyInfo.Value = new ZDateTime(1950, 1, 1);
			}
			else if (propertyInfo.PropertyType == typeof(ZDateTimeOffset))
			{
				propertyInfo.Value = new ZDateTimeOffset(1950, 1, 1);
			}
			else
			{
				propertyInfo.Value = new ZDate(1950, 1, 1);
			}

			if (shouldHaveError)
			{
				AssertHasError(propertyInfo, $"The date '01-Jan-1950' is more than {ValidationLimits.PastYearsBeforeError} years old and thus is not valid.");
			}
			else
			{
				AssertNoErrors(propertyInfo);
				AssertHasWarning(propertyInfo, propertyInfo.Name == "WE_ExpiryDate" ? "The date '01-Jan-1950' should be in future." : "The date '01-Jan-1950' is more than 1 year old.");
			}
		}

		#endregion

		#region DB Hits Tests

		public void TestPreSaveValidation_DBHits()
		{
			const int numLocations = 30;
			var data = new TestDataSimpleEnvironment(Factory, numLocations, 1);
			foreach (var location in data.Whs1.Rows[0].Locations)
			{
				location.WLV_MaxQuantity = 10m;
			}
			data.Whs1.FindLocation($"A-{numLocations}").WLV_MaxQuantity = 0m; // no limits
			Factory.Save();

			for (int i = 1; i < numLocations; i++)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R" + i, data.Part1, 10m, data.Whs1.FindLocation($"A-{i}"), $"A{i}", false);
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (var j = 0; j < 15; j++)
			{
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receiveLine.WE_PalletID = $"RR{j % 5}";
			}
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			var anotherFactory = new BusinessObjectFactory();
			var receiveInAnotherFactory = anotherFactory.Load<WhsReceive>(receive.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, anotherFactory))
			{
				receiveInAnotherFactory.RunPreSaveValidationExcludingChildren();
			}
		}

		#endregion

		#region  TestPreSaveValidation_BizOLoadedCount

		public void TestPreSaveValidation_BizOLoadedCount()
		{
			const int numLocations = 30;
			var data = new TestDataSimpleEnvironment(Factory, numLocations, 1);
			foreach (var location in data.Whs1.Rows[0].Locations)
			{
				location.WLV_MaxQuantity = 10m;
			}
			data.Whs1.FindLocation($"A-{numLocations}").WLV_MaxQuantity = 0m; // no limits
			Factory.Save();

			for (int i = 1; i < numLocations; i++)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R" + i, data.Part1, 10m, data.Whs1.FindLocation($"A-{i}"), $"A{i}", false);
			}

			var receiveForTesting = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (var j = 0; j < 15; j++)
			{
				var receiveLine = Helper.CreateWhsReceiveLine(receiveForTesting, data.Part1, 10m);
				receiveLine.WE_PalletID = $"RR{j % 5}";
			}
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var receiveInAnotherFactory = anotherFactory.Load<WhsReceive>(receiveForTesting.PK);

			BusinessObjectFactory.StartLogging();
			receiveInAnotherFactory.RunPreSaveValidationExcludingChildren();
			var loadLog = BusinessObjectFactory.DebugLog;
			BusinessObjectFactory.StopLogging();

			CombineAssertions(() =>
			{
				var logCount = Regex.Matches(loadLog, Regex.Escape("WE_StockOnHand > 0 and WE_PalletID <> '' and WE_PalletID")).Count;
				AssertEquals("Should load minimum Pallets.", 0, logCount);

				var allLoadedBusinessObjects = ((IBusinessObjectFactoryInternals)anotherFactory).AllBusinessObjects;
				AssertEquals("Should not load any additional inventories.", 15, allLoadedBusinessObjects.OfType<WhsInventoryView>().Count());
				AssertEquals("Should not load any additional receive lines.", 15, allLoadedBusinessObjects.OfType<WhsDocketLine>().Count());
				AssertEquals("Should not load any additional receives.", 1, allLoadedBusinessObjects.OfType<WhsDocket>().Count());
			});
		}

		#endregion

		#region TestUNDG

		#region TestUNDG_DangerousGoodsManagementEnabled

		public void TestUNDG_DangerousGoodsManagement_NotEnabled()
		{
			TestUNDG_DangerousGoodsManagementCore(false, false);
		}

		public void TestUNDG_DangerousGoodsManagement_Enabled()
		{
			TestUNDG_DangerousGoodsManagementCore(true, true);
		}

		void TestUNDG_DangerousGoodsManagementCore(bool isDangerousGoodsManagementEnabled, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = isDangerousGoodsManagementEnabled;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, data.Whs1.DefaultLocation, "Pallet-1");
			receive.RunPreSaveValidation();

			// Assert
			if (shouldHaveError)
			{
				AssertHasError(receiveLine.WE_DocketLineStatusInfo, @"The following UNDG Limits will exceed 100% of warehouse capacity limit by saving this job:
Substance Code 'AAA'
");
			}
			else
			{
				AssertNoErrors(receiveLine.WE_DocketLineStatusInfo);
			}
		}

		#endregion

		#region TestUNDG_ProductIsNotDG

		public void TestUNDG_ProductIsNotDG()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "1234", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, data.Whs1.DefaultLocation, "Pallet-1");
			receive.RunPreSaveValidation();

			// Assert
			AssertNoErrors(receiveLine.WE_DocketLineStatusInfo);
		}

		#endregion

		#region TestUNDG_Product

		public void TestUNDG_Product_UnderLimit()
		{
			TestUNDG_ProductCore(1m, shouldHaveError: false);
		}

		public void TestUNDG_Product_OverLimit()
		{
			TestUNDG_ProductCore(2m, shouldHaveError: true);
		}

		void TestUNDG_ProductCore(decimal productUndgSubstanceWeight, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, productUndgSubstanceWeight, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, data.Whs1.DefaultLocation, "Pallet-1");
			receive.RunPreSaveValidation();

			// Assert
			if (shouldHaveError)
			{
				AssertHasError(receiveLine.WE_DocketLineStatusInfo, @"The following UNDG Limits will exceed 100% of warehouse capacity limit by saving this job:
Substance Code 'AAA'
");
			}
			else
			{
				AssertNoErrors(receiveLine.WE_DocketLineStatusInfo);
			}
		}

		public void TestUNDG_Product_UnderLimit_MixedPutawayLines()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 6m, data.Whs1.DefaultLocation, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 6m, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-2");

			receive.RunPreSaveValidation();

			// Assert
			AssertNoErrors(receiveLine1.WE_DocketLineStatusInfo);
			AssertNoErrors(receiveLine2.WE_DocketLineStatusInfo);
		}

		#endregion

		#region TestUNDG_ProductUq

		public void TestUNDG_ProductUq_UnderLimit()
		{
			TestUNDG_ProductUqCore("UNT", shouldHaveError: false);
		}

		public void TestUNDG_ProductUq_OverLimit()
		{
			TestUNDG_ProductUqCore("PLT", shouldHaveError: true);
		}

		void TestUNDG_ProductUqCore(ZString packUnitType, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);
			Helper.CreateProductUnit(data.Part1, "UNT", 1m);
			Helper.CreateProductUnit(data.Part1, "PLT", 2m);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, data.Whs1.DefaultLocation, "Pallet-1");
			receiveLine.WE_F3_NKPackType = packUnitType;
			receive.RunPreSaveValidation();

			// Assert
			if (shouldHaveError)
			{
				AssertHasError(receiveLine.WE_DocketLineStatusInfo, @"The following UNDG Limits will exceed 100% of warehouse capacity limit by saving this job:
Substance Code 'AAA'
");
			}
			else
			{
				AssertNoErrors(receiveLine.WE_DocketLineStatusInfo);
			}
		}

		#endregion

		#region TestUNDG_Quantity

		public void TestUNDG_Quantity_UnderLimit()
		{
			TestUNDG_QuantityCore(10m, shouldHaveError: false);
		}

		public void TestUNDG_Quantity_OverLimit()
		{
			TestUNDG_QuantityCore(11m, shouldHaveError: true);
		}

		void TestUNDG_QuantityCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-1");
			receive.RunPreSaveValidation();

			// Assert
			if (shouldHaveError)
			{
				AssertHasError(receiveLine.WE_DocketLineStatusInfo, @"The following UNDG Limits will exceed 100% of warehouse capacity limit by saving this job:
Substance Code 'AAA'
");
			}
			else
			{
				AssertNoErrors(receiveLine.WE_DocketLineStatusInfo);
			}
		}

		#endregion

		#region TestUNDG_Location

		public void TestUNDG_Location_Dockdoor()
		{
			TestUNDG_LocationCore("DDL", shouldHaveError: false);
		}

		public void TestUNDG_Location_NormalLocation()
		{
			TestUNDG_LocationCore("NOR", shouldHaveError: true);
		}

		void TestUNDG_LocationCore(ZString locationClass, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var location = locationClass == "DDL" ? data.Whs1.DefaultInboundDockDoorLocation : data.Whs1.DefaultLocation;
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 0m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, location, "Pallet-1");
			receive.RunPreSaveValidation();

			// Assert
			if (shouldHaveError)
			{
				AssertHasError(receiveLine.WE_DocketLineStatusInfo, @"The following UNDG Limits will exceed 100% of warehouse capacity limit by saving this job:
Substance Code 'AAA'
");
			}
			else
			{
				AssertNoErrors(receiveLine.WE_DocketLineStatusInfo);
			}
		}

		#endregion

		#region TestUNDG_Threshold

		public void TestUNDG_PutawayToLocation_UnderThreshold()
		{
			TestUNDG_PutawayToLocationCore(5m, shouldHaveWarning: false);
		}

		public void TestUNDG_PutawayToLocation_OverThreshold()
		{
			TestUNDG_PutawayToLocationCore(6m, shouldHaveWarning: true);
		}

		void TestUNDG_PutawayToLocationCore(decimal receivedQuantity, bool shouldHaveWarning)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);

			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);
			Helper.CreateProductUnit(data.Part1, "PLT", 1m);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-1");
			receive.RunPreSaveValidation();

			// Assert
			if (shouldHaveWarning)
			{
				AssertHasWarning(receiveLine.WE_DocketLineStatusInfo, @"The following UNDG Limits will exceed 50% of warehouse capacity limit by saving this job:
Substance Code 'AAA'
");
			}
			else
			{
				AssertNoWarnings(receiveLine.WE_DocketLineStatusInfo);
			}
		}

		public void TestUNDG_PutawayToLocation_OverThreshold_MultipeLimits()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", undgClass, undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);
			Helper.CreateProductUnit(data.Part1, "PLT", 1m);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit3);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 6m, data.Whs1.DefaultLocation, "Pallet-1");
			receive.RunPreSaveValidation();

			// Assert
			AssertHasWarning(receiveLine.WE_DocketLineStatusInfo, @"The following UNDG Limits will exceed 50% of warehouse capacity limit by saving this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
");
		}

		#endregion

		#region  TestUNDG_MultipleReceiveLines

		public void TestUNDG_MultipleReceiveLines()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, data.Whs1.DefaultLocation, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10, data.Whs1.DefaultLocation, "Pallet-2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, data.Whs1.DefaultLocation, "Pallet-3");
			receive.RunPreSaveValidation();

			var errorMessage = @"The following UNDG Limits will exceed 100% of warehouse capacity limit by saving this job:
Substance Code 'AAA'
";
			CombineAssertions(() =>
			{
				AssertHasError(receiveLine1.WE_DocketLineStatusInfo, errorMessage);
				AssertNoErrors(receiveLine2.WE_DocketLineStatusInfo);
				AssertHasError(receiveLine3.WE_DocketLineStatusInfo, errorMessage);
			});
		}

		#endregion

		#region TestUNDG_DBHits

		public void TestUNDG_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 50;

			var location = data.Whs1.Rows[0].Locations[0];
			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);
			location.WLV_WLT_LocationType = normalLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			for (var i = 1; i < 10; i++)
			{
				CreateDG(i);
			}

			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsUNDGLimitSchema.Constants.TableName, 1 }
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertNoErrors(receiveInNewFactory.WD_DocketStatusInfo);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				receiveInNewFactory.RunPreSaveValidation();

				for (var i = 0; i < 10; i++)
				{
					var line = receiveInNewFactory.Lines[i];
					AssertNoErrors(line.WE_DocketLineStatusInfo);
				}

				for (var j = 10; j < 25; j++)
				{
					var line = receiveInNewFactory.Lines[j];
					var productIndex = j / 5 + 1;
					AssertHasWarning(line.WE_DocketLineStatusInfo, @$"The following UNDG Limits will exceed 50% of warehouse capacity limit by saving this job:
Substance Code 'S00{productIndex}a'
Country Reference 'AU{productIndex}'
Class Code '{productIndex}'
");
				}

				for (var k = 25; k < 45; k++)
				{
					var line = receiveInNewFactory.Lines[k];
					var productIndex = k / 5 + 1;
					AssertHasError(line.WE_DocketLineStatusInfo, @$"The following UNDG Limits will exceed 100% of warehouse capacity limit by saving this job:
Substance Code 'S00{productIndex}a'
Country Reference 'AU{productIndex}'
Class Code '{productIndex}'
");
				}
			}

			void CreateDG(int i)
			{
				var undgUNNOCode = $"S00{i}";
				var undgVariant = "a";
				var undgCode = $"{undgUNNOCode}{undgVariant}";
				var undgStandard = "IMO";
				var undgClass = $"{i}.1D";
				var client = data.Org1;

				var substance = Helper.CreateUNDGSubstance(undgUNNOCode, undgClass, undgCode);
				var product = Helper.CreateProduct(client, $"PRD{i}");
				var dgItem = product.UNDGs.AddNew();
				dgItem.DI_DG = substance.PK;
				dgItem.DI_DGWeight = 1m;
				dgItem.DI_UnitOfWeight = Constants.Weight.Kilograms;
				dgItem.DI_DGVolume = 1m;
				dgItem.DI_UnitOfVolume = Constants.Volume.CubicMetres;

				Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 100m, totalVolumeLimit: 100m);

				var reference = Helper.CreateCountryReference(referenceCode: $"AU{i}");
				Helper.CreateUNDGCountryReferencePivot(reference.PK, undgUNNOCode, undgVariant, undgStandard);
				var undgLimitCountryReference = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 100m, totalVolumeLimit: 100m);
				warehouse.UNDGLimits.Add(undgLimitCountryReference);

				var undgLimitClass = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, $"{i}", null, totalWeightLimit: 100m, totalVolumeLimit: 100m);
				warehouse.UNDGLimits.Add(undgLimitClass);

				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
			}
		}

		#endregion

		#endregion

		#region ValidStatuses

		protected override IEnumerable<ZString> ValidStatuses => new ZString[] { "", "PFU", "FIN", "CAN" };

		#endregion

		#region GetNewDocketHelper

		protected override FinalisableDocketHelper<WhsReceive> GetNewDocketHelper()
		{
			return new FinalisableReceiveHelper(Factory);
		}

		// In Receive Check method for LocationString is overriden and empty
		protected override bool ValidateLocationString
		{
			get { return false; }
		}

		#endregion
	}
}
