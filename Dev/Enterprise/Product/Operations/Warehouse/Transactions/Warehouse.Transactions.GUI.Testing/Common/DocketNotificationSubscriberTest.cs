using CargoWise.ComponentModel;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	internal class DocketNotificationSubscriberTest : WhsTestCaseWithFactory
	{
		public void TestDocketNotificationSubscriber()
		{
			WhsDocket docket = Factory.New<WhsReceive>();
			INotifications subscriber = new TestNotificationBuffer();

			using (DocketNotificationSubscriber s = new DocketNotificationSubscriber(docket, subscriber))
			{
				AssertEquals(subscriber, docket.NotificationManager.Peek);
				AssertNotEquals(subscriber, docket.NotificationManager.LastPopped);
			}

			AssertEquals(subscriber, docket.NotificationManager.LastPopped);
		}

		[ExpectNoExceptions()]
		public void TestDocketNotificationSubscriberHandlesNullArguments()
		{
			new DocketNotificationSubscriber(null, null);
		}
	}
}
