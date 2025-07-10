using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.Workflow.Business.Test
{
	class ContainmentBarrierTaskHelperTest : WorkflowTestCase
	{
		public void TestGetContainmentBarriersPrerequisiteTasks_StandaloneTask()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals(0, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task).Count());
		}

		public void TestGetContainmentBarriersPrerequisiteTasks_TaskWithNoWorkflow()
		{
			var job = (BusinessObject)Factory.New<IWorkItem>();
			var task1 = BMTestHelper.CreateTask(job);
			var task2 = BMTestHelper.CreateTask(job);
			var task3 = BMTestHelper.CreateTask(job);

			AssertEquals(0, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task1).Count());
			AssertContainsExactElementsInAnyOrder(new[] { task1 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task2));
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task3));
		}

		public void TestGetContainmentBarriersPrerequisiteTasks_TaskInAnotherNonLinkedWorkflow()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMTestHelper.CreateWorkflow(BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false), "workflow3");

			var task1 = BMTestHelper.CreateTask(workflow1, sequence: 1);
			var task2 = BMTestHelper.CreateTask(workflow2, sequence: 2);
			var task3 = BMTestHelper.CreateTask(workflow3, sequence: 3);

			AssertEquals(0, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task1).Count());
			AssertEquals(0, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task2).Count());
			AssertEquals(0, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task3).Count());
		}

		public void TestGetContainmentBarriersPrerequisiteTasks_TaskInLinkedWorkflow_WithPostrequisites()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMTestHelper.CreateWorkflow(BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false), "workflow3");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			var task1 = BMTestHelper.CreateTask(workflow1, sequence: 1);
			var task2 = BMTestHelper.CreateTask(workflow2, sequence: 2);
			var task3 = BMTestHelper.CreateTask(workflow3, sequence: 3);

			AssertEquals(0, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task1).Count());
			AssertContainsExactElementsInAnyOrder(new[] { task1 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task2));
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task3));

			AssertContainsExactElementsInAnyOrder(new[] { task1 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task2, checkWithinJobOnly: true));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<IProcessTask>(), ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task3, checkWithinJobOnly: true));
		}

		public void TestGetContainmentBarriersPrerequisiteTasks_TasksInMultiplePrereqChains()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMTestHelper.CreateWorkflow(jobHeader, "workflow4");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow4);
			workflow1.GetOrCreateDependencyLink(workflow3);
			workflow3.GetOrCreateDependencyLink(workflow4);

			var task1 = BMTestHelper.CreateTask(workflow1, sequence: 1);
			var task2 = BMTestHelper.CreateTask(workflow2, sequence: 2);
			var task3 = BMTestHelper.CreateTask(workflow3, sequence: 3);
			var task4 = BMTestHelper.CreateTask(workflow4, sequence: 4);

			AssertEquals(0, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task1).Count());
			AssertContainsExactElementsInAnyOrder(new[] { task1 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task2));
			AssertContainsExactElementsInAnyOrder(new[] { task1 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task3));
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2, task3 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task4));
		}

		public void TestGetContainmentBarriersPrerequisiteTasks_TasksInChildWorkflow()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMTestHelper.CreateWorkflow(jobHeader, "workflow2");

			workflow2.GetOrCreateLinkToParent(workflow1);

			var task1 = BMTestHelper.CreateTask(workflow1, sequence: 1);
			var task2 = BMTestHelper.CreateTask(workflow2, sequence: 2);
			var task3 = BMTestHelper.CreateTask(workflow1, sequence: 3);
			var task4 = BMTestHelper.CreateTask(workflow1, sequence: 4);

			AssertContainsExactElementsInAnyOrder(new[] { task2 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task1));
			AssertEquals(0, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task2).Count());
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task3));
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2, task3 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task4));
		}

		public void TestGetContainmentBarriersPrerequisiteTasks_TasksInPrerequisiteChildWorkflow()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMTestHelper.CreateWorkflow(jobHeader, "workflow3");

			workflow2.GetOrCreateLinkToParent(workflow1);
			workflow1.GetOrCreateDependencyLink(workflow3);

			var task1 = BMTestHelper.CreateTask(workflow1, sequence: 1);
			var task2 = BMTestHelper.CreateTask(workflow2, sequence: 2);
			var task3 = BMTestHelper.CreateTask(workflow1, sequence: 3);
			var task4 = BMTestHelper.CreateTask(workflow3, sequence: 4);

			AssertContainsExactElementsInAnyOrder(new[] { task2 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task1));
			AssertEquals(0, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task2).Count());
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task3));
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2, task3 }, ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(task4));
		}
	}
}
