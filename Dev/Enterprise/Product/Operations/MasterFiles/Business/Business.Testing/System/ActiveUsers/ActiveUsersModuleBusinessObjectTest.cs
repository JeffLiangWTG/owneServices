using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Semaphores.Common;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveUsersModuleBusinessObject))]
	sealed class ActiveUsersModuleBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ActiveUsersModuleBusinessObject(Factory);
		}

		public void TestActiveUsers()
		{
			ActiveUsersModuleBusinessObject activeUsersModule = new ActiveUsersModuleBusinessObject(Factory);
			Assert("At least one user should be logged in.", activeUsersModule.ActiveUsers.Count > 0);

			int currentProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
			bool currentUserAndIdFound = false;

			foreach (ActiveUser activeUser in activeUsersModule.ActiveUsers)
			{
				if (
					activeUser.AU_Initials == GlbStaff.CurrentUser.GS_Code
					&& activeUser.AU_FullName == GlbStaff.CurrentUser.GS_FullName
					&& !activeUser.AU_UTCLoginTime.IsEmpty
					&& activeUser.AU_ProcessID == currentProcessId
					&& activeUser.AU_ComputerName.Contains(System.Environment.MachineName)
					)
				{
					currentUserAndIdFound = true;
					break;
				}
			}

			Assert("Current user (" + GlbStaff.CurrentUser.GS_FullName + ") should be among the active users.", currentUserAndIdFound);
		}

		public void TestActiveUsersOrder()
		{
			var activeUsersModule = new ActiveUsersModuleBusinessObjectForTest(Factory);
			var user2Guid = Guid.NewGuid();
			activeUsersModule.AddActiveUserSession(Guid.NewGuid(), "Test1", Guid.NewGuid(), "TS1", LogonType.Contact, "PC1", 1111, DateTime.Now, "SC1");
			activeUsersModule.AddActiveUserSession(user2Guid, "Test2", Guid.NewGuid(), "TS2", LogonType.Contact, "PC2", 2222, DateTime.Now, "SC2");
			activeUsersModule.AddActiveUserSession(Guid.NewGuid(), "Test3", Guid.NewGuid(), "TS3", LogonType.Contact, "PC3", 3333, DateTime.Now, "SC3");

			AssertEquals("Should have 3 ActiveUsers", 3, activeUsersModule.ActiveUsers.Count);
			AssertEquals("", "Test1", activeUsersModule.ActiveUsers[0].AU_FullName);
			AssertEquals("", "Test2", activeUsersModule.ActiveUsers[1].AU_FullName);
			AssertEquals("", "Test3", activeUsersModule.ActiveUsers[2].AU_FullName);

			activeUsersModule.ReorderActiveUsers();
			AssertEquals("Should have 3 ActiveUsers", 3, activeUsersModule.ActiveUsers.Count);
			AssertEquals("", "Test3", activeUsersModule.ActiveUsers[0].AU_FullName);
			AssertEquals("", "Test2", activeUsersModule.ActiveUsers[1].AU_FullName);
			AssertEquals("", "Test1", activeUsersModule.ActiveUsers[2].AU_FullName);

			activeUsersModule.RemoveActiveUserSession(user2Guid);
			activeUsersModule.Refresh();
			AssertEquals("Should have 2 ActiveUsers", 2, activeUsersModule.ActiveUsers.Count);
			AssertEquals("", "Test3", activeUsersModule.ActiveUsers[0].AU_FullName);
			AssertEquals("", "Test1", activeUsersModule.ActiveUsers[1].AU_FullName);
		}
	}
}
