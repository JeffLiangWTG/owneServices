using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsPickCreationMutexTest : TestCaseWithFactory
	{
		#region TestGetMessage

		public void TestGetMessage()
		{
			var order = Factory.New<WhsOrder>();
			var mutex = new WhsPickCreationMutex(order);
			var mutex2 = new WhsPickCreationMutex(order);
			AssertEquals("Precondition", false, mutex.HasLock);
			AssertEquals("Precondition", false, mutex2.HasLock);

			mutex.Lock();

			try
			{
				AssertEquals("Precondition", true, mutex.HasLock);
				AssertEquals("Precondition", false, mutex2.HasLock);
				AssertEquals("Message should be returned since it is locked.", string.Format("{0} is currently in the process of creating a new Pick for this Order. Try again later.", mutex.GetLockInfo().UserWithLock.GS_FullName), mutex.GetMessage());
				AssertEquals("No message is returned since it is not locked.", string.Format("{0} is currently in the process of creating a new Pick for this Order. Try again later.", mutex2.GetLockInfo().UserWithLock.GS_FullName), mutex2.GetMessage());
			}
			finally
			{
				mutex.Unlock();
			}

			AssertEquals("No message is returned since it is not locked.", "", mutex.GetMessage());
			AssertEquals("No message is returned since it is unlocked.", "", mutex2.GetMessage());
		}

		#endregion
	}
}
