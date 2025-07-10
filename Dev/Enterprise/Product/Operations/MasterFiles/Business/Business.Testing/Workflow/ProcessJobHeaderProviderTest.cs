using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ProcessJobHeaderProviderTest : TestCaseWithFactory
	{
		public void TestGetForParent_BMSEnabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = (BMSystem)helper.CreateSystem(Factory, "DUM");

			var job = Factory.New<DummyWithWorkflow>();
			AssertNotNull(ProcessJobHeaderProvider.GetForParent(job, Factory));
		}

		public void TestGetForParent_BMSDisabled()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			TestCaseHelper.ClearTable(ProcessHeaderSchema.Constants.TableName);

			var job = (IWorkflowProvider)Factory.New<DummyWithWorkflow>();
			var task = job.WorkflowItems.AddNew();
			AssertNull(task.ProcessHeader);
			AssertNull(ProcessJobHeaderProvider.GetForParent(job, Factory));
			Factory.Save();

			AssertEquals(0, Factory.GetDatabaseCount(typeof(ProcessHeader)));
		}

		public void TestGetForParent_BMSEnabled_RelatedWorkflowTypeDisabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = (BMSystem)helper.CreateSystemAndRelatedWorkflowType(Factory, "DUM", isActive: false);

			var job = Factory.New<DummyWithWorkflow>();
			AssertNull(ProcessJobHeaderProvider.GetForParent(job, Factory));
		}

		public void TestGetWorkflowsForProcessTaskCollection_ShouldNotThrowException()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = (BMSystem)helper.CreateSystem(Factory, "DUM");

			var job = (IWorkflowProvider)Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			var defaultWorkflow = job.Workflows[0];
			var workflow1 = helper.CreateWorkflow(jobHeader, "WorkFlow Item 1");
			var workflow2 = helper.CreateWorkflow(jobHeader, "WorkFlow Item 2");

			var task0 = helper.CreateTask(workflow1, "GNA", 60, "UDF", "ASN", 1, "My task 0");
			var task1 = helper.CreateTask(workflow1, "GNA", 60, "UDF", "ASN", 2, "My task 1");
			var task2 = helper.CreateTask(workflow1, "GNA", 60, "UDF", "ASN", 3, "My task 2");
			var task3 = helper.CreateTask(workflow2, "GNA", 60, "UDF", "ASN", 1, "My task 3");

			Factory.Save();

			var workflows = ProcessJobHeaderProvider.GetWorkflowsForProcessTaskCollection(job.WorkflowItems, Factory);

			AssertContainsExactElementsInAnyOrder(new[] { defaultWorkflow, workflow1, workflow2 }, workflows);
		}
	}
}
