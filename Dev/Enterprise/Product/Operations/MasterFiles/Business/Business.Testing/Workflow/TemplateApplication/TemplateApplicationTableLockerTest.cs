using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemplateApplicationTableLockerTest : TestCaseWithFactory
	{
		public void TestLockReturnTrueWhenLock()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_ParentTableCode = dummy.TablePrefix;
			task.P9_ParentID = dummy.PK;

			Factory.Save();

			var result = TemplateApplicationTableLocker.Lock(Db.Connection, ProcessTasksSchema.Instance, ProcessTasksSchema.P9_ParentID, new[] { dummy.PK });

			Assert("Should lock table", result);
		}
	}
}
