using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class AdjustmentEntryFormTestWithNoTransaction : TestCase
	{
		#region TestFinaliseDocket_AdjustOut_AnotherUserHasFinalisedPick_AfterSavingAdjustment

		[UseSnapshotProtection]
		public void TestFinaliseDocket_AdjustOut_AnotherUserHasFinalisedPick_AfterSavingAdjustment()
		{
			var factory1 = new BusinessObjectFactory(Db.Connection);
			var data = new TestDataSimpleEnvironment(factory1);
			var helper = new WhsTestHelperFunctions(factory1);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "");
			factory1.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pickWithOrders = helper.CreatePickNew(order);
			factory1.Save();

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1");
			var adjustLine = helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1, data.Whs1.DefaultLocation);
			var whsNotificationSubscriberGuiHelper = new NotificationSubscriberGuiHelper();
			using (var form = new AdjustmentEntryForm(adjustment, whsNotificationSubscriberGuiHelper))
			{
				form.Show();
				form.FireSaveButton();

				var newFactory = new BusinessObjectFactory(Db.Connection) { RefreshEnabled = false };
				var pickInNewFactory = newFactory.Load<WhsPick>(pickWithOrders.PK);
				pickInNewFactory.FinaliseAllOrders();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(newFactory.Load<WhsOrder>(order.PK));

				pickInNewFactory.FinalisePick();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pickInNewFactory);
				newFactory.Save();

				var finaliseButton = GUITestHelper.FindControl<ZButton>(form.Controls, "FinaliseButton");
				finaliseButton.PerformClick();
				AssertEquals(true, adjustment.IsFinalised);
				AssertEquals(ContinueWithSave.No, form.FireSaveButton());

				var newestFactory = new BusinessObjectFactory(Db.Connection) { RefreshEnabled = false };
				AssertEquals(false, newestFactory.Load<WhsAdjustment>(adjustment.PK).IsFinalised);
				AssertEquals(9m, newestFactory.Load<WhsReceiveLine>(receive.Lines[0].PK).WE_StockOnHand);
			}
		}

		#endregion

		#region TestFinaliseDocket_AdjustOut_AnotherUserHasFinalisedPick_BeforeSavingAdjustment

		[UseSnapshotProtection]
		public void TestFinaliseDocket_AdjustOut_AnotherUserHasFinalisedPick_BeforeSavingAdjustment()
		{
			var factory1 = new BusinessObjectFactory(Db.Connection);
			var data = new TestDataSimpleEnvironment(factory1);
			var helper = new WhsTestHelperFunctions(factory1);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "");
			factory1.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pickWithOrders = helper.CreatePickNew(order);
			factory1.Save();

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1");
			var adjustLine = helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1, data.Whs1.DefaultLocation);
			var whsNotificationSubscriberGuiHelper = new NotificationSubscriberGuiHelper();
			using (var form = new AdjustmentEntryForm(adjustment, whsNotificationSubscriberGuiHelper))
			{
				form.Show();

				var newFactory = new BusinessObjectFactory(Db.Connection) { RefreshEnabled = false };
				var pickInNewFactory = newFactory.Load<WhsPick>(pickWithOrders.PK);
				pickInNewFactory.FinaliseAllOrders();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(newFactory.Load<WhsOrder>(order.PK));

				pickInNewFactory.FinalisePick();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pickInNewFactory);
				newFactory.Save();

				var finaliseButton = GUITestHelper.FindControl<ZButton>(form.Controls, "FinaliseButton");
				finaliseButton.PerformClick();
				AssertEquals(true, adjustment.IsFinalised);
				AssertEquals(ContinueWithSave.No, form.FireSaveButton());

				var newestFactory = new BusinessObjectFactory(Db.Connection) { RefreshEnabled = false };
				AssertEquals(false, newestFactory.Load<WhsAdjustment>(adjustment.PK).IsFinalised);
				AssertEquals(9m, newestFactory.Load<WhsReceiveLine>(receive.Lines[0].PK).WE_StockOnHand);
			}
		}

		#endregion

		#region TestFinaliseDocket_AdjustOut_AnotherUserAllocatedPickOnStock

		[UseSnapshotProtection]
		public void TestFinaliseDocket_AdjustOut_AnotherUserAllocatedPickOnStock()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var helper = new WhsTestHelperFunctions(factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "");
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			pickLine.WZ_Units = 5m;
			factory.Save();

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, data.Whs1.DefaultLocation);
			var whsNotificationSubscriberGuiHelper = new NotificationSubscriberGuiHelper();
			using (var form = new AdjustmentEntryForm(adjustment, whsNotificationSubscriberGuiHelper))
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				form.Show();
				var finaliseButton = GUITestHelper.FindControl<ZButton>(form.Controls, "FinaliseButton");

				// update the database directly, this is to simulate another user updating the pick units allocation 
				// on the inventory from another instance of the CW1 application
				connection2.BeginTransaction();
				CargoWise.Database.TestFramework.ObjectModel.WhsPickLine.UpdateWhere(pickLine.PK.ToGuid()).Set(l => l.WZ_Units, 6).Post(connection2);
				connection2.CommitTransaction();

				AssertNoExceptionThrown("Exception is handled.", () => finaliseButton.PerformClick());
				AssertEquals(WhsExceptionHandler.PreventOverCommitOfStockViaPickLineTriggerMsgForUser, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion
	}
}
