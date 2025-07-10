using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Common.Testing
{
	[TestedType(typeof(ReviewProcessProcessTaskCollection))]
	class ReviewProcessProcessTaskCollectionTest : ProcessTaskCollectionTest<ReviewProcessProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(ReviewProcessProcessTask), collection.AddNew().GetType());
		}

		protected override ReviewProcessProcessTaskCollection GetCollectionToTestCore() => Factory.NewWithValidTestData<ReviewProcess>().WorkflowItems;
	}
}
