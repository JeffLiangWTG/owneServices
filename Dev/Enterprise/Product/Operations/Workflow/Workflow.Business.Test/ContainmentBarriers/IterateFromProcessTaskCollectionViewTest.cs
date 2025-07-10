using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(IterateFromProcessTaskCollectionView))]
	[GuiTest]
	class IterateFromProcessTaskCollectionViewTest : BusinessObjectCollectionViewTestCase<IterateFromProcessTaskCollectionView>
	{
		#region Order

		public void TestOrder()
		{
			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			var task2 = job.WorkflowItems.Tasks.AddNew();
			task2.P9_Sequence = 2;
			var task1 = job.WorkflowItems.Tasks.AddNew();
			task1.P9_Sequence = 1;
			var task3 = job.WorkflowItems.Tasks.AddNew();
			task3.P9_Sequence = 3;
			task3.P9_Type = "QCB";

			var trigger = job.WorkflowItems.Triggers.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var collection = new IterateFromProcessTaskCollectionView(task3);
			AssertArrayEqualsByElements(new[] { task1, task2 }, collection.ToArray());
		}

		public void TestOrder_ShouldHaveContainmentBarrierTaskAsLastWithTasksofSameSequence()
		{
			var jobHeader = WorkflowTestCase.BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			qcbTask.P9_Sequence = 10;
			var task1 = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			task1.P9_Sequence = 10;
			task1.P9_Description = "AAA";
			var task2 = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			task2.P9_Sequence = 10;
			task2.P9_Description = "BBB";
			var task3 = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			task3.P9_Sequence = 10;
			task3.P9_Description = "CCC";
			var qcbTask1 = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			qcbTask1.P9_Sequence = 10;
			var collection = new IterateFromProcessTaskCollectionView((ProcessTask)qcbTask);

			AssertContainsExactElementsInAnyOrder(new[] { task1, task2, task3, qcbTask1 }, collection);
			AssertEquals(task1, collection[0]);
			AssertEquals(task2, collection[1]);
			AssertEquals(task3, collection[2]);
			AssertEquals(qcbTask1, collection[collection.Count - 1]);
		}

		#endregion

		#region Task Inclusion

		public void TestCollection_WhenQCBTaskIsDeleted()
		{
			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			var task = job.WorkflowItems.Tasks.AddNew();
			var qcbTask = job.WorkflowItems.Tasks.AddNew();
			qcbTask.P9_Type = "QCB";

			var collection = new IterateFromProcessTaskCollectionView(qcbTask);

			AssertEquals(1, collection.Count);

			qcbTask.Delete();
			collection.Rebuild();

			AssertEquals(0, collection.Count);
		}

		public void TestCollection_ShouldIncludeTasksOnly()
		{
			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();
			var task3 = job.WorkflowItems.Tasks.AddNew();
			task3.P9_Type = "QCB";

			var trigger = job.WorkflowItems.Triggers.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var collection = new IterateFromProcessTaskCollectionView(task3);
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2 }, collection);
		}

		public void TestCollection_ShouldIncludeCorrectWorkflows()
		{
			var jobHeader = WorkflowTestCase.BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task1 = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			task3.P9_Description = "Coding";
			var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			var collection = new IterateFromProcessTaskCollectionView((ProcessTask)qcbTask);
			AssertContainsExactElementsInAnyOrder(new[] { workflow2 }, collection.GetWorkflows());

			workflow1.GetOrCreateDependencyLink(workflow2);
			collection = new IterateFromProcessTaskCollectionView((ProcessTask)qcbTask);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2 }, collection.GetWorkflows());
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2, task3 }, collection);
			collection.SelectedWorkflow = workflow1;
			collection.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { task1 }, collection);
		}

		public void TestSortSequenceOfTask()
		{
			var jobHeader = WorkflowTestCase.BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task1 = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			task1.P9_Sequence = 100;
			task1.P9_Description = "Coding";
			var task2 = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			task2.P9_Sequence = 10;
			task2.P9_Description = "test-1";
			var task3 = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			task3.P9_Sequence = 11;
			task3.P9_Description = "test-2";
			var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			workflow1.GetOrCreateDependencyLink(workflow2);
			var collection = new IterateFromProcessTaskCollectionView((ProcessTask)qcbTask);

			AssertContainsExactElementsInAnyOrder(new[] { task1, task2, task3 }, collection);
			AssertEquals(task1, collection[0]);
			AssertEquals(task2, collection[1]);
			AssertEquals(task3, collection[2]);
		}

		public void TestCollection_ShouldIncludeTasksPreceedingQCBOnly()
		{
			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();
			var task3 = job.WorkflowItems.Tasks.AddNew();
			var task4 = job.WorkflowItems.Tasks.AddNew();

			task3.P9_Type = "QCB";

			var collection = new IterateFromProcessTaskCollectionView(task3);
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2 }, collection);
		}

		#endregion

		#region Network Task Inclusion

		public void TestTaskPreviews_NoWorkflow()
		{
			var job = (BusinessObject)Factory.New<IWorkItem>();
			var task1 = WorkflowTestCase.BMTestHelper.CreateTask(job, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = WorkflowTestCase.BMTestHelper.CreateTask(job, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask(job, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			var collection = new IterateFromProcessTaskCollectionView((ProcessTask)qcbTask);
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2, }, collection);
		}

		public void TestTaskPreviews_SameWorkflow_WithFollowingTasks()
		{
			var jobHeader = WorkflowTestCase.BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task1 = WorkflowTestCase.BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = WorkflowTestCase.BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			var task3 = WorkflowTestCase.BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			var collection = new IterateFromProcessTaskCollectionView((ProcessTask)qcbTask);
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2, }, collection);
		}

		public void TestTaskPreviews_DifferentWorkflowWithNoLink()
		{
			var jobHeader = WorkflowTestCase.BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task1 = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			var collection = new IterateFromProcessTaskCollectionView((ProcessTask)qcbTask);
			AssertContainsExactElementsInAnyOrder(new[] { task2, }, collection);
		}

		public void TestTaskPreviews_DifferentWorkflowWithLink()
		{
			var jobHeader = WorkflowTestCase.BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			var collection = new IterateFromProcessTaskCollectionView((ProcessTask)qcbTask);
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2, }, collection);
		}

		public void TestTaskPreviews_LinearWorkflowChain()
		{
			var jobHeader = WorkflowTestCase.BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			var task1 = WorkflowTestCase.BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			var task3 = WorkflowTestCase.BMTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			task3.P9_Sequence = 1;

			var collection = new IterateFromProcessTaskCollectionView((ProcessTask)qcbTask);
			AssertContainsExactElementsInAnyOrder(new[] { task1, }, collection);
		}

		#endregion

		#region GetWorkflows

		public void TestGetWorkflows_WhenTaskHasNoWorkflow_ShouldNotThrowExceptions()
		{
			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();
			task2.P9_Type = "QCB";

			var collection = new IterateFromProcessTaskCollectionView(task2);
			AssertEquals(1, collection.Count);
			AssertNull(((ProcessTask)collection.Single()).ProcessHeader);

			AssertNoExceptionThrown("Calling GetWorkflows on a collection with tasks that don't have workflows (when Buffer Management is disabled, or when the workflow type isn't associated with a Buffer Management System) should not throw exceptions. SAD!",
				() => collection.GetWorkflows());
		}

		#endregion

		#region Implementation

		protected override IterateFromProcessTaskCollectionView GetCollectionToTest()
		{
			WorkflowTestCase.SetAsQCBTaskType("QCB", "WKI");

			return new IterateFromProcessTaskCollectionView(CollectionTask);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var task = CollectionTask.Parent.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "QCB";
			task.P9_Sequence = 1;

			return task;
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkflowTestCase.EnableBufferManagement();
			WorkflowTestCase.SetAsQCBTaskType("QCB", "WKI");
		}

		ProcessTask CollectionTask
		{
			get
			{
				if (collectionTask == null)
				{
					var job = (IWorkflowProvider)Factory.New<IWorkItem>();
					collectionTask = job.WorkflowItems.Tasks.AddNew();
					collectionTask.P9_Type = "QCB";
					collectionTask.P9_Sequence = 10;

					Factory.Save();
				}

				return collectionTask;
			}
		}

		ProcessTask collectionTask;

		#endregion
	}
}
