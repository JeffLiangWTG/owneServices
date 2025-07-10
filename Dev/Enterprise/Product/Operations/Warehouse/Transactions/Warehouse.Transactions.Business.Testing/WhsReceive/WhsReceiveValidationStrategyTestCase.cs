using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveValidationStrategyTestCase : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => GetNewStrategy(null));
		}

		#endregion

		#region TestSerialNumberIsUnique

		public void TestSerialNumberIsUnique()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory1.WI_SerialNumber = "SN1";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Factory.Save();

			var deviceStrategy = GetNewStrategy(receive);
			inventory2.WI_SerialNumber = "SN1";
			AssertEquals("This SN should not be unique.", false, deviceStrategy.CheckSerialNumberIsUnique(receive.Client, inventory2, true));

			inventory2.WI_SerialNumber = "SN2";
			AssertEquals("This SN should be unique.", true, deviceStrategy.CheckSerialNumberIsUnique(receive.Client, inventory2, true));

			Factory.Save();

			// check that works on DB level as well as in memory.
			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
			var inventory2InOtherFactory = otherFactory.Load<WhsInventoryView>(inventory2.PK);
			var deviceStrategyInOtherFactory = GetNewStrategy(receiveInOtherFactory);

			inventory2InOtherFactory.WI_SerialNumber = "SN1";
			AssertEquals("This SN should not be unique.", false, deviceStrategyInOtherFactory.CheckSerialNumberIsUnique(receiveInOtherFactory.Client, inventory2InOtherFactory, true));

			inventory2InOtherFactory.WI_SerialNumber = "SN2";
			AssertEquals("This SN should be unique.", true, deviceStrategyInOtherFactory.CheckSerialNumberIsUnique(receiveInOtherFactory.Client, inventory2InOtherFactory, true));
		}

		public void TestSerialNumberIsUnique_NoStockOnHandInDB_AfterReceiveStockIsReleased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m);
			inventory1.WI_SerialNumber = "SN1";
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive2.PK);
			var inventory2InOtherFactory = otherFactory.Load<WhsInventoryView>(inventory2.PK);
			var deviceStrategyInOtherFactory = GetNewStrategy(receiveInOtherFactory);
			inventory2InOtherFactory.WI_SerialNumber = "SN1";
			AssertEquals("This SN should be unique.", true, deviceStrategyInOtherFactory.CheckSerialNumberIsUnique(receiveInOtherFactory.Client, inventory2InOtherFactory, true));
		}

		public void TestSerialNumberIsUnique_NoTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory1.WI_SerialNumber = "SN1";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Factory.Save();

			receive.PopulateASNLines();
			Factory.Save();
			AssertEquals(true, receive.StartedReceiving);

			var deviceStrategy = GetNewStrategy(receive);
			inventory2.WI_SerialNumber = "SN1";
			AssertEquals("This SN should not be unique.", false, deviceStrategy.CheckSerialNumberIsUnique(receive.Client, inventory2, true));

			inventory2.WI_InDocketLineUnits = 0m;
			AssertEquals("This SN is not unique but it should not be included in the SN check as inventory has no quantity after receiving.",
				true, deviceStrategy.CheckSerialNumberIsUnique(receive.Client, inventory2, true));

			// check that works on DB level as well as in memory.
			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
			var inventory2InOtherFactory = otherFactory.Load<WhsInventoryView>(inventory2.PK);
			var deviceStrategyInOtherFactory = GetNewStrategy(receiveInOtherFactory);

			inventory2InOtherFactory.WI_SerialNumber = "SN1";
			AssertEquals("This SN should not be unique.", false, deviceStrategyInOtherFactory.CheckSerialNumberIsUnique(receiveInOtherFactory.Client, inventory2InOtherFactory, true));

			inventory2InOtherFactory.WI_InDocketLineUnits = 0m;
			AssertEquals("This SN is not unique but it should not be included in the SN check as inventory has no quantity after receiving.",
				true, deviceStrategyInOtherFactory.CheckSerialNumberIsUnique(receiveInOtherFactory.Client, inventory2InOtherFactory, true));
		}

		public void TestSerialNumberIsUnique_WithPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultOutboundDockDoorLocation, "B");
			inventory.WI_SerialNumber = "SN1";
			Factory.Save();

			var transferForPallet = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPallet.WD_IsPutawayTransfer = true;
			var transferLineForPallet = (WhsTransferLine)transferForPallet.CreateDocketLineFromInventory(inventory);
			transferForPallet.RunPreSaveValidation();
			transferLineForPallet.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("WI_TotalUnits", 0m, inventory.WI_TotalUnits);
			AssertEquals("SN1", inventory.WI_SerialNumber);

			var putawayInventory = transferLineForPallet.Inventory[0];
			AssertEquals("WI_TotalUnits", 1m, putawayInventory.WI_TotalUnits);
			AssertEquals("SN1", putawayInventory.WI_SerialNumber);

			var deviceStrategy = GetNewStrategy(receive);
			AssertEquals("This SN should be unique.", true, deviceStrategy.CheckSerialNumberIsUnique(receive.Client, inventory, true));
		}

		public void TestSerialNumberIsUnique_WithCancelledReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m);
			receive1Line.WE_SerialNumber = "SER1";
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receive2Line = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m);
			receive2Line.WE_SerialNumber = "SER1";

			var deviceStrategy = GetNewStrategy(receive2);
			AssertEquals("This SN should not be unique.", false, deviceStrategy.CheckSerialNumberIsUnique(receive2.Client, receive2Line.Inventory[0], true));

			receive1.CancelReactivateDocket();
			AssertEquals("Precondition: Receive was cancelled.", true, receive1.IsCancelled);
			Factory.Save();
			AssertEquals("This SN should be unique.", true, deviceStrategy.CheckSerialNumberIsUnique(receive2.Client, receive2Line.Inventory[0], true));
		}

		#endregion

		#region TestGenerateSerialNumbers_NoUniqueSerialNumberChecksDuringGeneration

		public void TestGenerateSerialNumbers_NoUniqueSerialNumberChecksDuringGeneration()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_SerialNumber = "SN001";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine2.WE_SerialNumber = "SN005";

			receive.AllocateLocationsWithMock();

			receive.GenerateSerialNumbers(new[] { receiveLine1 });
			AssertEquals("Should be 11 lines", 11, receive.Lines.Count);
			foreach (var line in receive.Lines)
			{
				AssertEquals("SerialNumber has no errors.", false, line.WE_SerialNumberInfo.HasErrors());
			}
		}

		#endregion

		#region Implementations

		protected virtual WhsReceiveValidationStrategy GetNewStrategy(WhsReceive receive)
		{
			return new WhsReceiveValidationStrategy(receive);
		}

		#endregion
	}
}
