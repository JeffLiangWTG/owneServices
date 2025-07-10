using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemProcessTaskCollection))]
	class WorkItemProcessTaskCollectionTest : ProcessTaskCollectionTest<WorkItemProcessTaskCollection>
	{
		protected override WorkItemProcessTaskCollection GetCollectionToTestCore()
		{
			WorkItem workItem = Factory.New<WorkItem>();
			return new WorkItemProcessTaskCollection(workItem);
		}
	}
}
