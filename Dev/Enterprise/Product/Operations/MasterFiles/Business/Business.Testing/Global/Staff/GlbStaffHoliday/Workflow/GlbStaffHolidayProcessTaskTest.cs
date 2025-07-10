using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffHolidayProcessTask))]
	public class GlbStaffHolidayProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var glbStaffHoliday = Factory.NewWithValidTestData<GlbStaffHoliday>();
			var processTask = glbStaffHoliday.WorkflowItems.AddNew();
			AssertEquals(glbStaffHoliday, processTask.Parent);
			AssertEquals(ControllerIDs.GlbStaffHoliday, processTask.ParentControllerID);
		}

		public void TestGlbStaffHolidayProcessTaskReadOnly()
		{
			var glbStaffHoliday = Factory.NewWithValidTestData<GlbStaffHoliday>();
			var processTask = glbStaffHoliday.WorkflowItems.AddNew();
			processTask.ReadOnly = true;
			AssertEquals(true, processTask.ReadOnly);
			AssertEquals(false, processTask.P9_GS_NKAssignedStaffMemberInfo.ReadOnly);
			AssertEquals(true, processTask.P9_StatusInfo.ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<GlbStaffHoliday>().WorkflowItems.AddNew();

		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks() => AssertEquals((GetNewBusinessObject() as ProcessTask).ParentControllerID, ControllerIDs.GlbStaffHoliday);

		#endregion
	}
}
