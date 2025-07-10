using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public abstract class ReceiveActionProcessorTest : TestCaseWithFactory
	{
		#region TestReceiveActionProcessor_NullReceive

		public void TestReceiveActionProcessor_NullReceive()
		{
			AssertExceptionThrown<ArgumentNullException>(() => GetProcessor(null));
		}

		#endregion

		#region TestProcess

		public void TestProcess_NullNotifications()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var processor = GetProcessor(receive);
			AssertExceptionThrown<ArgumentNullException>(() => processor.Process(null));
		}

		public void TestProcess_UnableToCheckReceiveNotifications()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.NotificationManager.Push(new DummyNotifications());
			AssertNull("Precondition", receive.NotificationSubscriber as NotificationBuffer);

			var processor = GetProcessor(receive);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);

			AssertEquals("Notification buffer should have warning when processor is unable to check receive's NotificationSubscriber.", NotificationType.Warning, notifications.Events.Single().Type);
			AssertEquals($"Unable to {ExpectedActionName} for receive {receive.WD_DocketID}.", notifications.Events.Single().Message);
		}

		public void TestProcess_AutoPalletiseFailure_HasError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "PLT", 5);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, data.Whs1.DefaultLocation);
			Factory.Save();

			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var processor = GetProcessor(receive);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);

			AssertEquals("Processor should not perform action if receive is finalised", 1, receive.Lines.Count);
			AssertEquals("Notification buffer should have error when receive is finalized.", NotificationType.Error, notifications.Events.Single().Type);
			AssertEquals($"Unable to {ExpectedActionName} for receive {receive.WD_DocketID} because it is finalized or canceled, or at least one of its lines has a putaway transfer.", notifications.Events.Single().Message);
		}

		public void TestProcess()
		{
			TestProcessCore();
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper
		{
			get => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		}

		WhsTestHelperFunctions helper;

		protected abstract ReceiveActionProcessor GetProcessor(WhsReceive receive);

		protected abstract void TestProcessCore();

		protected abstract ZString ExpectedActionName { get; }

		#endregion

		#region DummyNotifications

		class DummyNotifications : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}

		#endregion
	}
}
