using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffChangeRequestProcessTaskCollection))]
	class GlbStaffChangeRequestProcessTaskCollectionTest : ProcessTaskCollectionTest<GlbStaffChangeRequestProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(GlbStaffChangeRequestProcessTask), collection.AddNew().GetType());
		}

		protected override GlbStaffChangeRequestProcessTaskCollection GetCollectionToTestCore() => Factory.NewWithValidTestData<GlbStaffChangeRequest>().WorkflowItems;
	}
}
