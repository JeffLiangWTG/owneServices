using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRHiringRequestProcessTaskCollection))]
	sealed class HRHiringRequestProcessTaskCollectionTest : ProcessTaskCollectionTest<HRHiringRequestProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(HRHiringRequestProcessTask), collection.AddNew().GetType());
		}

		#region Implementation

		protected override HRHiringRequestProcessTaskCollection GetCollectionToTestCore()
		{
			return Factory.NewWithValidTestData<HRHiringRequest>().WorkflowItems;
		}

		#endregion
	}
}
