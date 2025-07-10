using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(InventoryController))]
	class InventoryControllerBasherTest : WhsControllerBaseBasherTest
	{
		public void TestGetForm_FromReceiveLine()
		{
			TestGetForm_Core(() =>
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 3000m, data.Whs1.DefaultLocation);
				Factory.Save();

				return receiveLine;
			});
		}

		public void TestGetForm_FromAdjustmentLine()
		{
			TestGetForm_Core(() =>
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1");
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "A-1", "PLT-1");
				adjustment.FinaliseDocket();
				Factory.Save();

				AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

				return adjustmentLine;
			});
		}

		public void TestGetForm_FromTransferLine()
		{
			TestGetForm_Core(() =>
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 7m, "A-1", "A-2");
				transfer.RunPreSaveValidation(); // to commit inventory
				transfer.FinaliseDocket();
				Factory.Save();

				AssertEquals("Adjustment should be finalised.", true, transfer.IsFinalised);

				return transferLine;
			});
		}

		void TestGetForm_Core(Func<WhsDocketLine> createDocketLine)
		{
			var docketLine = createDocketLine();

			AssertEquals("Precondition", false, docketLine.IsInventoryEditForm);
			AssertEquals("Precondition", false, docketLine.Inventory[0].IsInventoryEditForm);

			var controller = new InventoryController();
			using (var form = controller.ShowViewForm(docketLine))
			{
				AssertEquals(typeof(InventoryForm), form.GetType());
				var businessEntityForm = (WhsDocketLine)form.BusinessEntityForPersistingForm;
				AssertEquals(true, businessEntityForm.IsInventoryEditForm);
				AssertEquals(true, businessEntityForm.Inventory[0].IsInventoryEditForm);
			}
		}

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		public override void TestNewForm()
		{
			Assert("Cant create New Inventory through the module screen", true);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsInventory;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();
			return receive.Inventory[0];
		}

		#endregion
	}
}
