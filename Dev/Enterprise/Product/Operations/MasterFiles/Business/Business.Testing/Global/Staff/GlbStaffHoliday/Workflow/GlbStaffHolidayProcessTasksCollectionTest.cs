using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffHolidayProcessTasksCollection))]
	public class GlbStaffHolidayProcessTasksCollectionTest : ProcessTaskCollectionTest<GlbStaffHolidayProcessTasksCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(GlbStaffHolidayProcessTask), collection.AddNew().GetType());
		}

		protected override GlbStaffHolidayProcessTasksCollection GetCollectionToTestCore() => (GlbStaffHolidayProcessTasksCollection)((IWorkflowProvider)Factory.NewWithValidTestData<GlbStaffHoliday>()).WorkflowItems;
	}
}
