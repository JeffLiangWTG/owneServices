using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickLineDeferTriggerStrategiesTest : WhsTestCaseWithFactory
	{
		#region TestWhsCheckSerialNumberMatchWithPickLineUnits_DeferTriggerStrategy

		public void TestWhsCheckSerialNumberMatchWithPickLineUnits_DeferTriggerStrategy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var receiveLine = receive.Lines[0];
			receiveLine.WE_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pickLine = Helper.CreateReservePickLine(orderLine, receiveLine.Inventory[0], 1m);
			var serialNumber = Helper.CreateWhsSerialNumber(data.Org1, data.Part1, "SN1");
			Helper.CreateWhsSerialNumberPivot(receiveLine, serialNumber);
			Factory.Save();

			var strategy = ObjectFactory.Get<IWhsCheckSerialNumberMatchWithPickLineUnits_DeferTriggerStrategy>() as IDeferTriggerConditionStrategy;

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false, strategy.ShouldDeferTrigger(pickLine));
				AssertEquals("Precondition", false, pickLine.IsPicked);
				AssertNotEquals("Precondition", "Me", pickLine.WZ_SystemLastEditUser);

				var user = pickLine.WZ_SystemLastEditUser;
				pickLine.WZ_SystemLastEditUser = "Me";
				AssertEquals("Should not defer is not relevent change.", false, strategy.ShouldDeferTrigger(pickLine));
				pickLine.WZ_SystemLastEditUser = user; // clean up
				AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(pickLine));

				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition", true, pickLine.IsPicked);
				AssertEquals("Should defer picked and relevant property changes.", true, strategy.ShouldDeferTrigger(pickLine));
			}

			AssertEquals("Should not defer if EnableSchemaRedesign is false.", false, strategy.ShouldDeferTrigger(pickLine));
		}

		#endregion

		#region TestWhsCheckTransferSerialNumberMatchWithnventory_DeferTriggerStrategy

		public void TestWhsCheckTransferSerialNumberMatchWithnventory_DeferTriggerStrategy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var receiveLine = receive.Lines[0];
			receiveLine.WE_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			transferLine.WE_SerialNumber = "SN1";
			transferLine.RunPreSaveValidation();
			var pickLine = transferLine.PickLines.Single();
			var serialNumber = Helper.CreateWhsSerialNumber(data.Org1, data.Part1, "SN1");
			Helper.CreateWhsSerialNumberPivot(receiveLine, serialNumber);
			Factory.Save();

			var strategy = ObjectFactory.Get<IWhsCheckTransferSerialNumberMatchWithnventory_DeferTriggerStrategy>() as IDeferTriggerConditionStrategy;

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false, strategy.ShouldDeferTrigger(pickLine));
				AssertEquals("Precondition", false, pickLine.IsPicked);
				AssertNotEquals("Precondition", "Me", pickLine.WZ_SystemLastEditUser);

				var user = pickLine.WZ_SystemLastEditUser;
				pickLine.WZ_SystemLastEditUser = "Me";
				AssertEquals("Should not defer is not relevent change.", false, strategy.ShouldDeferTrigger(pickLine));
				pickLine.WZ_SystemLastEditUser = user; // clean up
				AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(pickLine));

				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition", true, pickLine.IsPicked);
				AssertEquals("Should defer picked and relevant property changes.", true, strategy.ShouldDeferTrigger(pickLine));
			}

			AssertEquals("Should not defer if EnableSchemaRedesign is false.", false, strategy.ShouldDeferTrigger(pickLine));
		}

		#endregion
	}
}
