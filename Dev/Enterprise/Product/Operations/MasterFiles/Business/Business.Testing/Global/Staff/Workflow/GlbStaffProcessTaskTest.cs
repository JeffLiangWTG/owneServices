using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffProcessTask))]
	sealed class GlbStaffProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			GlbStaff glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaffProcessTask processTask = ((GlbStaffProcessTasksCollection)glbStaff.WorkflowItems).AddNew();
			AssertEquals(glbStaff, processTask.Parent);
			AssertEquals(ControllerIDs.GlbStaff, processTask.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<GlbStaff>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
