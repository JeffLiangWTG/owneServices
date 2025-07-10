using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class StaffCredentialsPlugInTest : TestCaseWithFactory
	{
		public void TestMutex()
		{
			var staff = Factory.New<GlbStaff>();

			using (var plugin = new StaffCredentialsPlugInForTesting(staff))
			{
				AssertEquals(MutexIDs.StaffCredentialsPlugInBeingCreated, plugin.Mutex.MutexID);
				AssertEquals(staff.PK.ToString(), plugin.Mutex.RecordIdentifier);
			}
		}
	}
}
