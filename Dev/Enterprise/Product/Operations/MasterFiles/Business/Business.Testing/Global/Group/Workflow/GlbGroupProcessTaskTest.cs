using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupProcessTask))]
	public class GlbGroupProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroupProcessTask processTask = ((GlbGroupProcessTaskCollection)group.WorkflowItems).AddNew();

			AssertEquals("Parent", group, processTask.Parent);
			AssertEquals("ParentControllerID", ControllerIDs.GlbGroup, processTask.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<GlbGroup>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
