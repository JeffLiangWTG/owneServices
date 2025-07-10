using System;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class NotificationManagerTest : TestCase
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestPushNull()
		{
			Manager.Push(null);
		}

		public void TestPushPeekPopLastPopped()
		{
			NotificationBuffer buffer1 = new NotificationBuffer();
			Manager.Push(buffer1);
			AssertEquals("Peek = just pushed", buffer1, Manager.Peek);
			AssertEquals("Pop = just pushed", buffer1, Manager.Pop());
			AssertEquals("Last Popped", buffer1, Manager.LastPopped);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestPopThrowsWhenEmpty()
		{
			Manager.Pop();
		}

		public void TestConstructorPushsDefaultBuffer()
		{
			NotificationManager manager = new NotificationManager(); // deliberately use local manager
			NotificationBuffer buffer1 = (NotificationBuffer)manager.Peek;
			AssertNotNull("Constructor", buffer1);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Manager = new NotificationManager();
		}

		NotificationManager Manager;
	}
}
