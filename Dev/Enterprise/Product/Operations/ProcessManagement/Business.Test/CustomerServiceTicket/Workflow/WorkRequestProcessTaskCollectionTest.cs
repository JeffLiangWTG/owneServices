using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequestProcessTaskCollection))]
	class WorkRequestProcessTaskCollectionTest : ProcessTaskCollectionTest<WorkRequestProcessTaskCollection>
	{
		protected override WorkRequestProcessTaskCollection GetCollectionToTestCore()
		{
			return new WorkRequestProcessTaskCollection(ProcessMgmtTestHelper.CreateWorkRequest(Factory));
		}
	}
}
