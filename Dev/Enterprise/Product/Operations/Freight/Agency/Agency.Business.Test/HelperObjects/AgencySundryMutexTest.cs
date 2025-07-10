using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencySundryMutexTest : TestCaseWithFactory
	{
		public void TestGetFriendlyMessage_NotLocked()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AgencySundryMutex mutex = AgencySundryMutex.New(org);
			string message = mutex.GetFriendlyMessage();
			AssertNotNull("The friendly message must not be null", message);
			Assert("The friendly message must not be empty", message.Length > 0);
		}

		public void TestGetFriendlyMessage_Locked()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AgencySundryMutex mutex = AgencySundryMutex.New(org);
			AgencySundryMutex mutex2 = AgencySundryMutex.New(org);
			mutex2.Lock();
			Assert("Precondition: the first mutex must not be locked", !mutex.HasLock);
			Assert("Precondition: the second mutex must be locked", mutex2.HasLock);
			string message = null;
			try
			{
				message = mutex.GetFriendlyMessage();
			}
			finally
			{
				mutex2.Unlock();
			}

			AssertNotNull("The friendly message must not be null", message);
			Assert("The friendly message must not be empty", message.Length > 0);
		}
	}
}
