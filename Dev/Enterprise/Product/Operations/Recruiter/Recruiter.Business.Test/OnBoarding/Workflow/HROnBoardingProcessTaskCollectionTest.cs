using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HROnBoardingProcessTaskCollection))]
	sealed class HROnBoardingProcessTaskCollectionTest : ProcessTaskCollectionTest<HROnBoardingProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(HROnBoardingProcessTask), collection.AddNew().GetType());
		}

		#region Implementation

		protected override HROnBoardingProcessTaskCollection GetCollectionToTestCore()
		{
			return Factory.NewWithValidTestData<HROnBoarding>().WorkflowItems;
		}

		#endregion
	}
}
