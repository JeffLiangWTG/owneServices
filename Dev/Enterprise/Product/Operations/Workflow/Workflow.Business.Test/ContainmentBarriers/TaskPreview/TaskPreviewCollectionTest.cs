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
	[TestedType(typeof(TaskPreviewCollection))]
	class TaskPreviewCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TaskPreviewCollection>
	{
		public void TestAddNew_ShouldNotBeSupported()
		{
			var collection = GetCollectionToTest();
			AssertExceptionThrown<NotSupportedException>(() => collection.AddNew());
		}

		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, GetCollectionToTest().AllowRemove);
		}

		protected override TaskPreviewCollection GetCollectionToTest()
		{
			WorkflowTestCase.SetAsQCBTaskType("QCB", "WKI");

			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "QCB";
			var viewModel = GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed);

			return viewModel.IterationTaskPreviews;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TaskPreviewCopy(task, viewModel);
		}

		ProcessTask task;
		ContainmentBarrierViewModel viewModel;

		protected override void SetUp()
		{
			base.SetUp();

			WorkflowTestCase.EnableBufferManagement();
			WorkflowTestCase.SetAsQCBTaskType("QCB", "WKI");

			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "QCB";
			viewModel = GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed);
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
