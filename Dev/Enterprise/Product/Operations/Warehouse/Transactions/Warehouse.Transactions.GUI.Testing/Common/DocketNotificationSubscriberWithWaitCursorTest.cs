using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	internal class DocketNotificationSubscriberWithWaitCursorTest : WhsTestCaseWithFactory
	{
		public void TestDocketNotificationSubscriberWithWaitCursor()
		{
			WhsDocket docket = Factory.New<WhsReceive>();
			INotifications subscriber = new TestNotificationBuffer();
			Cursor.Current = Cursors.Arrow;

			using (DocketNotificationSubscriberWithWaitCursor cursor = new DocketNotificationSubscriberWithWaitCursor(docket, subscriber))
			{
				AssertEquals(Cursors.WaitCursor, Cursor.Current);
				AssertEquals(subscriber, docket.NotificationManager.Peek);
				AssertNotEquals(subscriber, docket.NotificationManager.LastPopped);
			}

			AssertEquals(Cursors.Arrow, Cursor.Current);
			AssertEquals(subscriber, docket.NotificationManager.LastPopped);
		}

		[ExpectNoExceptions]
		public void TestHandlesNullArguments()
		{
			Cursor.Current = Cursors.Arrow;
			using (DocketNotificationSubscriberWithWaitCursor cursor = new DocketNotificationSubscriberWithWaitCursor(null, null))
			{
				AssertEquals(Cursors.WaitCursor, Cursor.Current);
			}
			AssertEquals(Cursors.Arrow, Cursor.Current);
		}
	}
}
