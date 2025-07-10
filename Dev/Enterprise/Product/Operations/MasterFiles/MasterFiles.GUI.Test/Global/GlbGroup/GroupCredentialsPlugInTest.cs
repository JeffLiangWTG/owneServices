using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GroupCredentialsPlugInTest : TestCaseWithFactory
	{
		public void TestMutex()
		{
			var group = Factory.New<GlbGroup>();

			using (var plugin = new GroupCredentialsPlugInForTesting(group))
			{
				AssertEquals(MutexIDs.GroupCredentialsPlugInBeingCreated, plugin.Mutex.MutexID);
				AssertEquals(group.PK.ToString(), plugin.Mutex.RecordIdentifier);
			}
		}

		public void TestName()
		{
			var group = Factory.New<GlbGroup>();

			using (var plugin = new GroupCredentialsPlugInForTesting(group))
			{
				AssertEquals("Credentials", plugin.Name);
			}
		}
	}
}
