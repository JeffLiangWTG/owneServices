using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemRelatedItemCollection))]
	public class WorkItemRelatedItemCollectionTest : WorkTaskRelatedItemCollectionTestCase<WorkItem>
	{
		public override void TestShouldAddToCollection()
		{
			Project related1 = Factory.NewWithValidTestData<Project>();
			Project related2 = Factory.NewWithValidTestData<Project>();
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();

			AssertEquals(0, Collection.Count);

			Collection.Add(related1);
			Collection.Add(related2);
			Collection.Add(workItem);

			AssertEquals("Should not contain workitem", 2, Collection.Count);
			Assert("Should have related 1", Collection.Contains(related1));
			Assert("Should have related 2", Collection.Contains(related2));
			Assert("Should not have workitem", !Collection.Contains(workItem));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WorkItemRelatedItemCollection(Factory.NewWithValidTestData<WorkItem>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<Project>();
		}
	}
}
