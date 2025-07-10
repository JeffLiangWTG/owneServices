using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyAllocationMutexTest : TestCaseWithFactory
	{
		public void TestGetFriendlyMessage_NotLocked()
		{
			var voyage = Factory.New<JobVoyage>();
			var mutex = new AgencyAllocationMutex(voyage);
			var lockInfo = mutex.GetLockInfo();
			var (caption, message, allowRelease) = mutex.GetFriendlyMessage(lockInfo);
			AssertNotNull("The friendly message must not be null", message);
			Assert("The caption must not be empty", caption.Length > 0);
			Assert("The friendly message must not be empty", message.Length > 0);
			Assert("AllowRelease is false because nothing to be unlocked", !allowRelease);
		}

		[TestDate(2024, 1, 1)]
		public void TestGetFriendlyMessage_Locked()
		{
			var voyage = Factory.New<JobVoyage>();

			AssertNotNull(Env.CurrentUser);

			var mutex = new AgencyAllocationMutex(voyage);
			var mutex2 = new AgencyAllocationMutex(voyage);
			mutex2.Lock();

			Assert("Precondition: the first mutex must not be locked", !mutex.HasLock);
			Assert("Precondition: the second mutex must be locked", mutex2.HasLock);

			try
			{
				var info = mutex.GetLockInfo();

				GlbStaff.CurrentUser.GS_IsController = true;
				var (caption, message, allowRelease) = mutex.GetFriendlyMessage(info);
				AssertEquals($"User {info.UserWithLock.GS_LoginName} has this voyage locked since {info.LockStartTime}. Do you want to release the existing lock?", message);
				Assert(allowRelease);

				GlbStaff.CurrentUser.GS_IsController = false;

				(caption, message, allowRelease) = mutex.GetFriendlyMessage(info);
				AssertEquals($"User {info.UserWithLock.GS_LoginName} has this voyage locked since {info.LockStartTime}. Try again later.", message);
				Assert(!allowRelease);

				TestDateAttribute.AddSeconds(5 * 60 + 1);
				(caption, message, allowRelease) = mutex.GetFriendlyMessage(info);
				AssertEquals($"User {info.UserWithLock.GS_LoginName} has this voyage locked since {info.LockStartTime}. Please contact a controller to release the existing lock.", message);
				Assert(!allowRelease);

				mutex.ReleaseLocks(info);
				mutex.Lock();
				Assert("The first mutex must be locked", mutex.HasLock);
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}

				if (mutex2.HasLock)
				{
					mutex2.Unlock();
				}
			}
		}
	}
}
