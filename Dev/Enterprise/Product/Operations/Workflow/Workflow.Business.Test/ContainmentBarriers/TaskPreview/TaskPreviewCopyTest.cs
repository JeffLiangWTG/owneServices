using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(TaskPreviewCopy))]
	class TaskPreviewCopyTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructorWithNullArgs()
		{
			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "QCB";
			var viewModel = GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskPreviewCopy(null, viewModel));
			AssertExceptionThrown<ArgumentNullException>(() => new TaskPreviewCopy(task, null));
		}

		public void TestTaskProperties()
		{
			WorkflowTestCase.EnableBufferManagement();

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "Brigadier General Jack O'Neill";

			var jobHeader = WorkflowTestCase.BMTestHelper.CreateJobHeader<IWorkItem>(Factory);
			var workflow = WorkflowTestCase.BMTestHelper.CreateWorkflow(jobHeader, "Resignation");
			var task = (ProcessTask)WorkflowTestCase.BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 10, description: "Never Mind", taskType: "QCB");

			var copy = new TaskPreviewCopy(task, GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed));

			AssertEquals(copy.ResourceName, "Brigadier General Jack O'Neill");
			AssertEquals(copy.WorkflowName, "Resignation");
			AssertEquals(copy.TaskName, "Never Mind");
			AssertEquals(copy.Sequence, 10);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "QCB";
			var viewModel = GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed);

			return new TaskPreviewCopy(task, viewModel);
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkflowTestCase.EnableBufferManagement();
			WorkflowTestCase.SetAsQCBTaskType("QCB", "WKI");
		}

		public ContainmentBarrierViewModel GetViewModel(IProcessTask task, string status)
		{
			var viewModel = new ContainmentBarrierViewModel(task, status, deselectCancelledTasksFromIteration: true);
			if (disposables == null)
			{
				disposables = new DisposableList(10);
			}
			disposables.Add(viewModel);
			return viewModel;
		}

		protected override void TearDown()
		{
			disposables?.Dispose();
			disposables = null;
			base.TearDown();
		}

		DisposableList disposables;
	}
}
