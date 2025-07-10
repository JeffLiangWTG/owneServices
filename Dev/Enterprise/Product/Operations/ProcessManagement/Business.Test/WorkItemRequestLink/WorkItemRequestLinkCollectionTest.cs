using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemRequestLinkCollection))]
	class WorkItemRequestLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<WorkItemRequestLinkCollection>
	{
		protected override WorkItemRequestLinkCollection GetCollectionToTest()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			return new WorkItemRequestLinkCollection(workItem);
		}
	}
}
