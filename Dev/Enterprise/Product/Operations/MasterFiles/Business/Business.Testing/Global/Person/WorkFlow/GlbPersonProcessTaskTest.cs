using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPersonProcessTask))]
	class GlbPersonProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			GlbPerson glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			GlbPersonProcessTask processTask = ((GlbPersonProcessTaskCollection)glbPerson.WorkflowItems).AddNew();
			AssertEquals(glbPerson, processTask.Parent);
			AssertEquals(ControllerIDs.GlbPerson, processTask.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<GlbPerson>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
