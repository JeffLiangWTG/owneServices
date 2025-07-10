using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemProcessTask))]
	public class WorkItemProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			WorkItemProcessTask processTask = workItem.WorkflowItems.AddNew();
			AssertEquals(workItem, processTask.Parent);
			AssertEquals(ControllerIDs.WorkItem, processTask.ParentControllerID);
		}

		public void TestTypeDecider()
		{
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			WorkItemProcessTask workItemTask = workItem.WorkflowItems.AddNew();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadAsProcessTask = factory2.Load<ProcessTask>(workItemTask.PK);
			Assert("is WorkItemProcessTask", loadAsProcessTask is WorkItemProcessTask);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<WorkItem>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
