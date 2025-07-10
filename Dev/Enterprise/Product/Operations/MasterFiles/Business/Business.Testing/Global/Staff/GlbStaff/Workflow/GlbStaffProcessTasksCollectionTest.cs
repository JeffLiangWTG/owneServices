using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffProcessTasksCollection))]
	sealed class GlbStaffProcessTasksCollectionTest : ProcessTaskCollectionTest<GlbStaffProcessTasksCollection>
	{
		public void TestAddNewProcessTask()
		{
			GlbStaffProcessTasksCollection collection = GetCollectionToTestCore();
			AssertEquals(typeof(GlbStaffProcessTask), collection.AddNew().GetType());
		}

		#region Implementation

		protected override GlbStaffProcessTasksCollection GetCollectionToTestCore()
		{
			return (GlbStaffProcessTasksCollection)Factory.NewWithValidTestData<GlbStaff>().WorkflowItems;
		}

		#endregion
	}
}
