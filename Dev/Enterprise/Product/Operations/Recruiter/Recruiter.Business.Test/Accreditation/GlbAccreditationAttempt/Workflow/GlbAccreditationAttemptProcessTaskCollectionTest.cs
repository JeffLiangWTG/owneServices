using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptProcessTaskCollection))]
	sealed class GlbAccreditationAttemptProcessTaskCollectionTest : ProcessTaskCollectionTest<GlbAccreditationAttemptProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(GlbAccreditationAttemptProcessTask), collection.AddNew().GetType());
		}

		#region Implementation

		protected override GlbAccreditationAttemptProcessTaskCollection GetCollectionToTestCore()
		{
			return Factory.NewWithValidTestData<GlbAccreditationAttempt>().WorkflowItems;
		}

		#endregion
	}
}
