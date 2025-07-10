using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectProcessTask))]
	class ProjectProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			Project project = Factory.NewWithValidTestData<Project>();
			ProjectProcessTask processTask = project.WorkflowItems.AddNew();
			AssertEquals(project, processTask.Parent);
			AssertEquals(ControllerIDs.Project, processTask.ParentControllerID);
		}

		public void TestTypeDecider()
		{
			Project project = Factory.NewWithValidTestData<Project>();
			ProjectProcessTask processTask = project.WorkflowItems.AddNew();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedProcessTask = anotherFactory.Load<ProcessTask>(processTask.PK);
			Assert("is ProjectProcessTask", loadedProcessTask is ProjectProcessTask);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<Project>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
