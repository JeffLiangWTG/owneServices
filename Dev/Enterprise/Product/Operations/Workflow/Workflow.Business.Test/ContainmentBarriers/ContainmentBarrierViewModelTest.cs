using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[GuiTest]
	[TestDate(2014, 8, 20)]
	class ContainmentBarrierViewModelWithDummyDataTest : WorkflowTestCase
	{
		#region Constructor

		public void TestConstructor_ForNonQCBTask()
		{
			AssertExceptionThrown<ArgumentException>(() => GetViewModel(Factory.New<ProcessTask>(), ProcessTaskStatusCodeList.Codes.Closed));
		}

		#endregion

		#region ValidResponses

		public void TestValidResponses_StandaloneQCB()
		{
			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_MultipleQCBWithDifferentSequences()
		{
			var qcbTask2 = BMTestHelper.CreateTask(workflow, taskType: "QCB");

			qcbTask.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 11;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_FullName = "Samwise the Brave";

			var qcbTask2 = BMTestHelper.CreateTask(workflow, resource2.GS_Code, taskType: "QCB");

			qcbTask.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 10;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.DeferredToAnotherResource | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences_WhereOneIsOnAnotherWorkflow()
		{
			var workflow2 = workflow.JobHeader.ProcessHeaders.AddNew();

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_FullName = "Samwise the Brave";

			var qcbTask2 = BMTestHelper.CreateTask(workflow2, resource2.GS_Code, taskType: "QCB");

			qcbTask.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 10;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences_WhereOneIsClosed()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_FullName = "Samwise the Brave";

			var qcbTask2 = BMTestHelper.CreateTask(workflow, resource2.GS_Code, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			qcbTask.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 10;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences_InDifferentWorkflows()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_FullName = "Samwise the Brave";

			var workflow2 = BMTestHelper.CreateWorkflow(workflow.JobHeader, "workflow2");
			var qcbTask2 = BMTestHelper.CreateTask(workflow2, resource2.GS_Code, taskType: "QCB");

			qcbTask.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 10;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences_NoResourceAssigned()
		{
			var qcbTask2 = BMTestHelper.CreateTask(workflow, taskType: "QCB");

			qcbTask.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 10;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_NonQCBWithSameSequence()
		{
			var task2 = BMTestHelper.CreateTask(workflow);

			qcbTask.P9_Sequence = 10;
			task2.P9_Sequence = 10;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences_OtherQCBIsClosedAlready()
		{
			var qcbTask2 = BMTestHelper.CreateTask(workflow, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask3 = BMTestHelper.CreateTask(workflow, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);

			qcbTask.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 10;
			qcbTask3.P9_Sequence = 10;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences_OtherQCBHasIterationCreated()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var qcbTask2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask3 = BMTestHelper.CreateTask(workflow, resource.GS_Code, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			qcbTask.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 10;
			qcbTask3.P9_Sequence = 10;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.DeferredToAnotherResource | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);

			var iterationWorkflow = BMTestHelper.CreateWorkflow(jobHeader, "childWorkflow");
			iterationWorkflow.GetOrCreateLinkToParent(workflow);

			CreateQualityIteration(qcbTask2, task1);

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences_OtherQCBHasBeenPassed()
		{
			AssertValidResponses_ForAcceptIterationAlreadyCreated_ShouldConsiderOnlyLinksForIterationsCreated(ContainmentBarrierResponses.Passed, shouldIncludeAcceptIterationAlreadyCreatedResponse: false);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences_OtherQCBHasBeenDeferred()
		{
			AssertValidResponses_ForAcceptIterationAlreadyCreated_ShouldConsiderOnlyLinksForIterationsCreated(ContainmentBarrierResponses.DeferredToAnotherResource, shouldIncludeAcceptIterationAlreadyCreatedResponse: false);
		}

		public void TestValidResponses_MultipleQCBWithSameSequences_OtherQCBHasBeenIterated()
		{
			AssertValidResponses_ForAcceptIterationAlreadyCreated_ShouldConsiderOnlyLinksForIterationsCreated(ContainmentBarrierResponses.IterationRequired, shouldIncludeAcceptIterationAlreadyCreatedResponse: true);
		}

		void AssertValidResponses_ForAcceptIterationAlreadyCreated_ShouldConsiderOnlyLinksForIterationsCreated(ContainmentBarrierResponses iterationLinkOutcome, bool shouldIncludeAcceptIterationAlreadyCreatedResponse)
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var qcbTask2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			qcbTask.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 10;

			AssertEquals(ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.DeferredToAnotherResource | ContainmentBarrierResponses.Canceled, viewModel.ValidResponses);

			qcbTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			if (iterationLinkOutcome == ContainmentBarrierResponses.IterationRequired)
			{
				CreateQualityIteration(qcbTask2, task1);
			}
			else
			{
				var link = ((ProcessTask)qcbTask2).GetContainmentBarrierIterationLinks().AddNew();
				link.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
				link.P9I_GS_NKResourceUnderReview = resource.GS_Code;
				ContainmentBarrierViewModel.SetOutcome(link, iterationLinkOutcome);
			}

			var validOutcomes = ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled;

			if (shouldIncludeAcceptIterationAlreadyCreatedResponse)
			{
				validOutcomes |= ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource;
			}

			AssertEquals(validOutcomes, viewModel.ValidResponses);
		}

		#endregion

		#region Response

		public void TestIterationRequiredResponse_ShouldPickBestQCBTask_DifferentResourceOnLastTask()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var task2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 3, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals(ZGuid.Empty, viewModel.IterateFromTaskPK);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals(task3.PK, viewModel.IterateFromTaskPK);
			AssertContainsExactElementsInAnyOrder(new[] { task3, qcbTask }, viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(t => t.Task));
		}

		public void TestIterationRequiredResponse_ShouldPickBestQCBTask_NonClosedLastTask()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var task2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 3, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			AssertEquals(ZGuid.Empty, viewModel.IterateFromTaskPK);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals(task2.PK, viewModel.IterateFromTaskPK);
			AssertContainsExactElementsInAnyOrder(new[] { task2, task3, qcbTask }, viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(t => t.Task));
		}

		public void TestSortIsRespectedForIterationTaskPreviews()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var task2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 3, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			qcbTask.P9_Sequence = 4;

			viewModel.IterationTaskPreviews.Sort("Sequence", ListSortDirection.Descending);

			AssertEquals(ZGuid.Empty, viewModel.IterateFromTaskPK);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals(task2.PK, viewModel.IterateFromTaskPK);
			var tasksarray = viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(t => t.Task).ToArray();
			AssertEquals(qcbTask, tasksarray[0]);
			AssertEquals(task3, tasksarray[1]);
			AssertEquals(task2, tasksarray[2]);
		}

		public void TestIterationRequiredResponse_ShouldPickBestQCBTask_SameResourceOnLastTask()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var task2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = BMTestHelper.CreateTask(workflow, qcbTask.P9_GS_NKAssignedStaffMember, sequence: 3, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals(ZGuid.Empty, viewModel.IterateFromTaskPK);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals(task2.PK, viewModel.IterateFromTaskPK);
			AssertContainsExactElementsInAnyOrder(new[] { task2, task3, qcbTask }, viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(t => t.Task));
		}

		public void TestIterationRequiredResponse_ShouldPickBestQCBTask_SameSequenceOnLastTask()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var task2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: qcbTask.P9_Sequence, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals(ZGuid.Empty, viewModel.IterateFromTaskPK);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals(task2.PK, viewModel.IterateFromTaskPK);
			AssertContainsExactElementsInAnyOrder(new[] { task2, task3, qcbTask }, viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(t => t.Task));
		}

		public void TestIterationRequiredResponse_ShouldPickBestQCBTask_TaskOnPreviousWorkflowHasHigherSequence()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var workflow0 = BMTestHelper.CreateWorkflow(jobHeader, "workflow0");
			var task0 = BMTestHelper.CreateTask(workflow0, resource.GS_Code, sequence: 1000, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			workflow0.GetOrCreateDependencyLink(workflow);

			var task2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals(ZGuid.Empty, viewModel.IterateFromTaskPK);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals("task2 is the 'previous' task since it's in the same workflow, even though there's a task earlier in the network with a higher sequence number", task2, viewModel.IterateFromTask);
			AssertContainsExactElementsInAnyOrder(new[] { task2, qcbTask }, viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(t => t.Task));
		}

		public void TestIterationRequiredResponse_ShouldPickBestQCBTask_TaskOnCurrentWorkflowIsNotClosed()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var workflow0 = BMTestHelper.CreateWorkflow(jobHeader, "workflow0");
			var task0 = BMTestHelper.CreateTask(workflow0, resource.GS_Code, sequence: 1000, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			workflow0.GetOrCreateDependencyLink(workflow);

			var task2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 15);

			AssertEquals(ZGuid.Empty, viewModel.IterateFromTaskPK);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals("task0 is the 'previous' task since it's within the workflow network, and task in the same workflow as the QCB is still open", task0, viewModel.IterateFromTask);
			AssertContainsExactElementsInAnyOrder(new[] { task0, qcbTask }, viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(t => t.Task));
		}

		#endregion

		#region Dispose Implementation

		public void TestContainmentBarrierUnhookCollectionsOnDispose()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed, deselectCancelledTasksFromIteration: true);

			AssertEquals(true, viewModel.JobTasksWithWorkflowFiltering.IsHooked);
			AssertEquals(false, viewModel.IterationLinks.IsDeactivated);
			AssertEquals(false, viewModel.Lookups.ResourceUnderReviewList.IsDeactivated);

			viewModel.Dispose();
			AssertEquals(false, viewModel.JobTasksWithWorkflowFiltering.IsHooked);
			AssertEquals(true, viewModel.IterationLinks.IsDeactivated);
			AssertEquals(true, viewModel.Lookups.ResourceUnderReviewList.IsDeactivated);
		}

		#endregion

		#region Task Previews

		[GuiTest]
		public class TaskPreviewsTest : WorkflowTestCase
		{
			public void TestTaskPreviews_NoWorkflow()
			{
				var job = (BusinessObject)Factory.New<IWorkItem>();
				var task1 = BMTestHelper.CreateTask(job, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task2 = BMTestHelper.CreateTask(job, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = BMTestHelper.CreateTask(job, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

				AssertContainsExactElementsInAnyOrder(new[] { task1, task2, qcbTask }, GetTasksInPreview(qcbTask, task1));
			}

			public void TestTaskPreviews_SameWorkflow()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				var task1 = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task2 = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

				AssertContainsExactElementsInAnyOrder(new[] { task1, task2, qcbTask }, GetTasksInPreview(qcbTask, task1));
			}

			public void TestTaskPreviews_DifferentWorkflowWithNoLink()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow1 = jobHeader.ProcessHeaders.AddNew();
				var workflow2 = jobHeader.ProcessHeaders.AddNew();
				var task1 = BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task2 = BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

				AssertContainsExactElementsInAnyOrder(new[] { task1, qcbTask }, GetTasksInPreview(qcbTask, task1));
			}

			public void TestTaskPreviews_DifferentWorkflowWithLink()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow1 = jobHeader.ProcessHeaders.AddNew();
				var workflow2 = jobHeader.ProcessHeaders.AddNew();
				workflow1.GetOrCreateDependencyLink(workflow2);

				var task1 = BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task2 = BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

				AssertContainsExactElementsInAnyOrder(new[] { task1, task2, qcbTask }, GetTasksInPreview(qcbTask, task1));
			}

			public void TestTaskPreviews_LinearWorkflowChain()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow1 = jobHeader.ProcessHeaders.AddNew();
				var workflow2 = jobHeader.ProcessHeaders.AddNew();
				var workflow3 = jobHeader.ProcessHeaders.AddNew();
				workflow1.GetOrCreateDependencyLink(workflow2);
				workflow2.GetOrCreateDependencyLink(workflow3);

				var task1 = BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task2 = BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = BMTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

				AssertContainsExactElementsInAnyOrder(new[] { task1, task2, qcbTask }, GetTasksInPreview(qcbTask, task1));
			}

			public void TestTaskPreviews_MultipathConnectedWorkflowChain()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow1 = jobHeader.ProcessHeaders.AddNew();
				var workflow2 = jobHeader.ProcessHeaders.AddNew();
				var workflow3 = jobHeader.ProcessHeaders.AddNew();
				var workflow4 = jobHeader.ProcessHeaders.AddNew();
				workflow1.GetOrCreateDependencyLink(workflow2);
				workflow2.GetOrCreateDependencyLink(workflow4);
				workflow1.GetOrCreateDependencyLink(workflow3);
				workflow3.GetOrCreateDependencyLink(workflow4);

				var task1 = BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1);
				var task2 = BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2);
				var task3 = BMTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3);
				var qcbTask = BMTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 4, taskType: "QCB");

				AssertContainsExactElementsInAnyOrder(new[] { task1, task2, task3, qcbTask }, GetTasksInPreview(qcbTask, task1));
				AssertContainsExactElementsInAnyOrder(new[] { task2, qcbTask }, GetTasksInPreview(qcbTask, task2));
				AssertContainsExactElementsInAnyOrder(new[] { task3, qcbTask }, GetTasksInPreview(qcbTask, task3));
			}

			public void TestTaskPreviews_MultipathWorkflowChain_DisconnectedOnOnePath()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow1 = jobHeader.ProcessHeaders.AddNew();
				var workflow2 = jobHeader.ProcessHeaders.AddNew();
				var workflow3 = jobHeader.ProcessHeaders.AddNew();
				var workflow4 = jobHeader.ProcessHeaders.AddNew();
				workflow1.GetOrCreateDependencyLink(workflow2);
				workflow2.GetOrCreateDependencyLink(workflow4);
				workflow3.GetOrCreateDependencyLink(workflow4);

				var task1 = BMTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task2 = BMTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task3 = BMTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = BMTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

				AssertContainsExactElementsInAnyOrder(new[] { task1, task2, qcbTask }, GetTasksInPreview(qcbTask, task1));
			}

			public void TestTaskPreviews_IterateFromTasksInChildrenWorkflowDontShowTasksFromParentWorkflow()
			{
				var bmsTestHelper = BMTestHelper;
				var jobHeader = bmsTestHelper.CreateJobHeader<IWorkItem>(Factory, false);
				var parentWorkflow = bmsTestHelper.CreateWorkflow(jobHeader, "Parent Workflow");
				var childWorkflow = bmsTestHelper.CreateWorkflow(jobHeader, "Child Worklow");
				childWorkflow.GetOrCreateLinkToParent(parentWorkflow);

				var task1 = bmsTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task2 = bmsTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task3 = bmsTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task4 = bmsTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = bmsTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

				AssertContainsExactElementsInAnyOrder(new[] { task1, task2, qcbTask }, GetTasksInPreview(qcbTask, task1));
				AssertContainsExactElementsInAnyOrder(new[] { task2, qcbTask }, GetTasksInPreview(qcbTask, task2));
				AssertContainsExactElementsInAnyOrder(new[] { task3, task4, qcbTask }, GetTasksInPreview(qcbTask, task3));
				AssertContainsExactElementsInAnyOrder(new[] { task4, qcbTask }, GetTasksInPreview(qcbTask, task4));
			}

			public void TestFindBestIterateFromTask_ShouldntSelectTaskWithHigherSequenceThanQcbTask()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
				BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

				AssertEquals("The viewmodel selected a task but none were eligible for selection.", ZGuid.Empty, viewModel.FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeSameResourceAsQcbTask));
			}

			public void TestFindBestIterateFromTask_ShouldSelectNothingIfNoEligibleTasksWithLowerSequence()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				var task1 = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
				BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

				AssertEquals("The viewmodel did not select the task that has a lower sequence number than the qcb task.", task1.PK, viewModel.FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeDifferentResourceAsQcbTask));
			}

			public void TestFindBestIterateFromTask_ShouldSelectSameResourceTaskOnlyWhenOptionSpecified()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				var task1 = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var task2 = BMTestHelper.CreateTask(workflow, "RAN", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				var qcbTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

				var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

				var taskAssignedToSameResource = viewModel.FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeDifferentResourceAsQcbTask);
				var taskAssignedToDifferentResource = viewModel.FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeSameResourceAsQcbTask);
				var taskWhereResourceOptionNotSpecified = viewModel.FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.None);

				AssertEquals("The viewmodel did not select the task assigned to the same resource.", task1.PK, taskAssignedToSameResource);
				AssertNotEquals("The viewmodel selected the task assigned to a different resource.", task2.PK, taskAssignedToSameResource);

				AssertEquals("The viewmodel did not select the task assigned to a different resource.", task2.PK, taskAssignedToDifferentResource);
				AssertNotEquals("The viewmodel selected the task assigned to the same resource.", task1.PK, taskAssignedToDifferentResource);

				AssertEquals("The viewmodel did not select the latest task assigned to any resource even though non resource-related option was set.", task2.PK, taskWhereResourceOptionNotSpecified);
			}

			public void TestFindBestIterateFromTask_ShouldNotSelectCheckinTasksWhenOptionDisabled()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "SH0");
				var qcbTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
				var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

				AssertEquals("The viewmodel selected a task with a checkin type which is not allowed.", ZGuid.Empty, viewModel.FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeContainmentBarrierTasks));
			}

			public void TestFindBestIterateFromTask_ShouldFindTaskInEligibleTaskTypesList()
			{
				var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				var cduTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDU");
				var cdfTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDF");
				var qcbTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
				var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

				AssertEquals(cduTask.PK, viewModel.FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeContainmentBarrierTasks, new[] { "CDU" }));
				AssertEquals(cdfTask.PK, viewModel.FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeContainmentBarrierTasks, new[] { "CDF" }));
				AssertEquals(ZGuid.Empty, viewModel.FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeContainmentBarrierTasks, new[] { "UDF" }));
			}

			IEnumerable<IProcessTask> GetTasksInPreview(IProcessTask qcbTask, IProcessTask iterateFromTask)
			{
				var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
				viewModel.IterateFromTaskPK = iterateFromTask.PK;

				return viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(t => t.Task);
			}

			protected override void SetUp()
			{
				base.SetUp();

				EnableBufferManagement();
				SetAsQCBTaskType("QCB", "WKI");
				SetAsQCBTaskType("SH0", "WKI");
				disposables = new DisposableList(10);
			}

			public ContainmentBarrierViewModel GetViewModel(IProcessTask task, string status)
			{
				var viewModel = new ContainmentBarrierViewModel(task, status, deselectCancelledTasksFromIteration: true);
				disposables.Add(viewModel);
				return viewModel;
			}

			protected override void TearDown()
			{
				disposables.Dispose();
				base.TearDown();
			}

			DisposableList disposables;
		}

		public void TestCancelledTasksAreNotIncludedInContainmentBarrierIteration()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task1 = BMTestHelper.CreateTask(workflow, sequence: 1, description: "task 1", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var task2 = BMTestHelper.CreateTask(workflow, sequence: 2, description: "task 2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = BMTestHelper.CreateTask(workflow, sequence: 3, description: "task 3", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task4 = BMTestHelper.CreateTask(workflow, sequence: 4, description: "task 4", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var task5 = BMTestHelper.CreateTask(workflow, sequence: 5, description: "task 5", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task6 = BMTestHelper.CreateTask(workflow, sequence: 6, description: "task 6", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var qcbTask = BMTestHelper.CreateTask(workflow, sequence: 10, description: "task 10", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.CommitResponse();

			AssertEquals("1. Cancelled Task should be deselected: ", false, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 1).Single().IncludeInIteration);
			AssertEquals("2. Closed Task should be selected: ", true, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 2).Single().IncludeInIteration);
			AssertEquals("3. Closed Task should be selected: ", true, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 3).Single().IncludeInIteration);
			AssertEquals("4. Cancelled Task should be deselected: ", false, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 4).Single().IncludeInIteration);
			AssertEquals("5. Closed Task should be selected: ", true, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 5).Single().IncludeInIteration);
			AssertEquals("6. Cancelled Task should be deselected: ", false, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 6).Single().IncludeInIteration);
			AssertEquals("10. Last Containment Barrier Task should be selected: ", true, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 10).Single().IncludeInIteration);
		}

		public void TestMidContainmentBarrierTaskReflectsWhetherTaskIsCancelled()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task1 = BMTestHelper.CreateTask(workflow, sequence: 1, description: "task 1", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var task2 = BMTestHelper.CreateTask(workflow, sequence: 2, description: "task 2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			var task3 = BMTestHelper.CreateTask(workflow, sequence: 3, description: "task 3", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task4 = BMTestHelper.CreateTask(workflow, sequence: 4, description: "task 4", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled, taskType: "QCB");
			var task5 = BMTestHelper.CreateTask(workflow, sequence: 5, description: "task 5", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = BMTestHelper.CreateTask(workflow, sequence: 10, description: "task 10", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.CommitResponse();

			AssertEquals("1. Cancelled Task should be deselected: ", false, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 1).Single().IncludeInIteration);
			AssertEquals("2. Mid Containment Barrier, which is Closed Task should be selected: ", true, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 2).Single().IncludeInIteration);
			AssertEquals("3. Closed Task should be selected: ", true, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 3).Single().IncludeInIteration);
			AssertEquals("4. Mid Containment Barrier, which is Cancelled Task should be deselected: ", false, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 4).Single().IncludeInIteration);
			AssertEquals("5. Closed Task should be selected: ", true, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 5).Single().IncludeInIteration);
			AssertEquals("10. Last Closed Containment Barrier Task should be selected: ", true, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 10).Single().IncludeInIteration);
		}
		
		public void TestAllCancelledTasksAreDeselected()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task1 = BMTestHelper.CreateTask(workflow, sequence: 1, description: "task 1", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var task2 = BMTestHelper.CreateTask(workflow, sequence: 2, description: "task 2", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var task3 = BMTestHelper.CreateTask(workflow, sequence: 3, description: "task 3", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var qcbTask = BMTestHelper.CreateTask(workflow, sequence: 10, description: "task 10", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled, taskType: "QCB");

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.CommitResponse();

			AssertEquals("1. Cancelled Task should be deselected: ", false, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 1).Single().IncludeInIteration);
			AssertEquals("2. Cancelled Task should be deselected: ", false, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 2).Single().IncludeInIteration);
			AssertEquals("3. Cancelled Task should be deselected: ", false, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 3).Single().IncludeInIteration);
			AssertEquals("10. Last Cancelled Containment Barrier Task should be selected: ", true, viewModel.IterationTaskPreviews.Where(x => x.Sequence == 10).Single().IncludeInIteration);
		}

		#endregion

		#region CommitResponse

		public void TestCommitResponse_Passed()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");

			var qcbTask = (ProcessTask)BMTestHelper.CreateTask(workflow, taskType: "QCB");
			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			AssertRtfText(ZString.Empty, qcbTask.P9_Notes);

			viewModel.Response = ContainmentBarrierResponses.Passed;
			viewModel.CommitResponse();

			AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier Passed", qcbTask.P9_Notes);
		}

		public void TestCommitResponse_Passed_ExistingTaskNotes()
		{
			jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);
			workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");

			var qcbTaskWithRtf = (ProcessTask)BMTestHelper.CreateTask(workflow, taskType: "QCB");

			qcbTaskWithRtf.P9_NotesAsString = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Calibri;}}
{\*\generator Riched20 10.0.17134}\viewkind4\uc1 
\pard\sa200\sl276\slmult1\f0\fs22\lang9 Dem Notes...\par
}
";
			Factory.Save();

			viewModel = GetViewModel(qcbTaskWithRtf, ProcessTaskStatusCodeList.Codes.Closed);
			AssertRtfText("Dem Notes...", qcbTaskWithRtf.P9_Notes);

			viewModel.Response = ContainmentBarrierResponses.Passed;
			viewModel.CommitResponse();

			AssertRtfText("Dem Notes...\r\n\r\nFRO 20-Aug-14 00:00: Containment Barrier Passed", qcbTaskWithRtf.P9_Notes);
		}

		public void TestCommitResponse_IterationRequired()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);

			var task1 = BMTestHelper.CreateTask(jobHeader, sequence: 1, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = BMTestHelper.CreateTask(jobHeader, taskType: "QCB", sequence: 2, description: "qcbTask");

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			AssertRtfText(ZString.Empty, qcbTask.P9_Notes);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.CommitResponse();

			AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: RS1 - Reason 1", qcbTask.P9_Notes);
		}

		public void TestCommitResponse_NoteContainsTable_CorrectlyMergesRtf()
		{
			var rtfText = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fswiss\fprq2\fcharset0 Arial;}}
{\*\generator Riched20 10.0.17134}\viewkind4\uc1 
\pard\sa200\sl276\slmult1\f0\fs24 Here is my table\par
\trowd\trgaph10\trleft-10\trrh200\trbrdrl\brdrs\brdrw30 \trbrdrt\brdrs\brdrw30 \trbrdrr\brdrs\brdrw30 \trbrdrb\brdrs\brdrw30 \trpaddl10\trpaddr10\trpaddfl3\trpaddfr3
\clbrdrl\brdrw30\brdrs\clbrdrt\brdrw30\brdrs\clbrdrr\brdrw30\brdrs\clbrdrb\brdrw30\brdrs \cellx4235\clbrdrl\brdrw30\brdrs\clbrdrt\brdrw30\brdrs\clbrdrr\brdrw30\brdrs\clbrdrb\brdrw30\brdrs \cellx4944 
\pard\intbl\widctlpar\b\fs20\lang3081 Sections to Include\b0\cell\b Y / N\cell\row 
\pard\widctlpar\b0\lang1033\par
\b\lang3081 Yes, it was a table above!\par
}";
			jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);

			task1 = BMTestHelper.CreateTask(jobHeader, sequence: 1, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTaskWithRtf = (ProcessTask)BMTestHelper.CreateTask(jobHeader, taskType: "QCB", sequence: 2, description: "qcbTask");
			qcbTaskWithRtf.P9_NotesAsString = rtfText;
			Factory.Save();

			viewModel = GetViewModel(qcbTaskWithRtf, ProcessTaskStatusCodeList.Codes.Closed);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.CommitResponse();

			AssertRtfText("Here is my table\r\nSections to Include	Y / N\r\nYes, it was a table above!\r\n\r\nFRO 20-Aug-14 00:00: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: RS1 - Reason 1", qcbTaskWithRtf.P9_Notes);
		}

		public void TestCommitResponse_OverriddenQcbUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "CQD";
			staff.GS_LoginName = "come.quickly.distress";
			Factory.Save();

			viewModel.QcbCreatingUserLoginName = staff.GS_LoginName;
			AssertRtfText(ZString.Empty, qcbTask.P9_Notes);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.CommitResponse();

			AssertRtfText("The staff code was overridden, so the note should have started with that staff code.", "CQD 20-Aug-14 00:00: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: RS1 - Reason 1", qcbTask.P9_Notes);
		}

		public void TestCommitResponse_Deferred()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_FullName = "Samwise the Brave";
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			resource3.GS_FullName = "Gandalf the Grey";
			var resource4 = Factory.NewWithValidTestData<GlbStaff>();
			resource4.GS_FullName = "Aragorn";

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");

			var qcbTask1 = (ProcessTask)BMTestHelper.CreateTask(workflow, resource1.GS_Code, taskType: "QCB");
			var qcbTask2 = (ProcessTask)BMTestHelper.CreateTask(workflow, resource2.GS_Code, taskType: "QCB");
			var qcbTask3 = (ProcessTask)BMTestHelper.CreateTask(workflow, resource3.GS_Code, taskType: "QCB");
			var qcbTask4_differentSequence = (ProcessTask)BMTestHelper.CreateTask(workflow, resource4.GS_Code, taskType: "QCB");

			qcbTask1.P9_Sequence = 10;
			qcbTask2.P9_Sequence = 10;
			qcbTask3.P9_Sequence = 10;
			qcbTask4_differentSequence.P9_Sequence = 11;

			var viewModel = GetViewModel(qcbTask1, ProcessTaskStatusCodeList.Codes.Closed);
			AssertRtfText(ZString.Empty, qcbTask1.P9_Notes);

			viewModel.Response = ContainmentBarrierResponses.DeferredToAnotherResource;
			viewModel.CommitResponse();

			AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier verification was deferred to Samwise the Brave, Gandalf the Grey", qcbTask1.P9_Notes);
		}

		public void TestCommitResponse_Deferred_WhenNotValid()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");

			var qcbTask = (ProcessTask)BMTestHelper.CreateTask(workflow, taskType: "QCB");
			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			AssertRtfText(ZString.Empty, qcbTask.P9_Notes);

			AssertExceptionThrown<InvalidOperationException>(() => viewModel.Response = ContainmentBarrierResponses.DeferredToAnotherResource);

			AssertRtfText(ZString.Empty, qcbTask.P9_Notes);
		}

		public void TestCommitResponse_IterationRequired_ShouldCreateIterationAndBumpSubsequentTaskSequences()
		{
			var job = (BusinessObject)Factory.New<IWorkItem>();
			var tasks = ((IWorkflowProvider)job).WorkflowItems.Tasks.Cast<ProcessTask>();

			var task1 = BMTestHelper.CreateTask(job, sequence: 1, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMTestHelper.CreateTask(job, sequence: 2, description: "task2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = BMTestHelper.CreateTask(job, sequence: 3, description: "task3", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task4 = BMTestHelper.CreateTask(job, sequence: 4, description: "task4", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			var qcbTask = BMTestHelper.CreateTask(job, taskType: "QCB", sequence: 5, description: "qcbTask");

			var task6 = BMTestHelper.CreateTask(job, sequence: 6, description: "task6");
			var task7 = BMTestHelper.CreateTask(job, sequence: 7, description: "task7");
			var task8 = BMTestHelper.CreateTask(job, sequence: 12, description: "task8");

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task2.PK;

			AssertEquals(8, tasks.Count());
			viewModel.CommitResponse();
			AssertEquals(12, tasks.Count());

			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(2, task2.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
			AssertEquals(4, task4.P9_Sequence);
			AssertEquals(5, qcbTask.P9_Sequence);
			AssertEquals(10, task6.P9_Sequence);
			AssertEquals(11, task7.P9_Sequence);
			AssertEquals(16, task8.P9_Sequence);

			var newTask2 = tasks.Single(t => t.PK != task2.PK && t.P9_Description == "task2");
			var newTask3 = tasks.Single(t => t.PK != task3.PK && t.P9_Description == "task3");
			var newTask4 = tasks.Single(t => t.PK != task4.PK && t.P9_Description == "task4");
			var newQCBTask = tasks.Single(t => t.PK != qcbTask.PK && t.P9_Description == "qcbTask");

			AssertEquals(6, newTask2.P9_Sequence);
			AssertEquals(7, newTask3.P9_Sequence);
			AssertEquals(8, newTask4.P9_Sequence);
			AssertEquals(9, newQCBTask.P9_Sequence);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task3.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task4.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task6.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task7.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task8.P9_Status);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, newTask2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, newTask3.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, newTask4.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, newQCBTask.P9_Status);

			AssertEquals(ZDateTimeOffset.Empty, newTask2.P9_CompletedTime);
			AssertEquals(ZDateTimeOffset.Empty, newTask3.P9_CompletedTime);
			AssertEquals(ZDateTimeOffset.Empty, newTask4.P9_CompletedTime);
		}

		public void TestCommitResponse_IterationRequired_ShouldCreateIterationTaskSequences_WhenOnlyOneTaskIsSelected()
		{
			var job = (BusinessObject)Factory.New<IWorkItem>();
			var tasks = ((IWorkflowProvider)job).WorkflowItems.Tasks.Cast<ProcessTask>();
			var cdftask = BMTestHelper.CreateTask(job, taskType: "CDF", sequence: 1, description: "cdfTask", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var prvTask = BMTestHelper.CreateTask(job, taskType: "PRV", sequence: 2, description: "prvTask", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = BMTestHelper.CreateTask(job, taskType: "QCB", sequence: 2, description: "qcbTask");
			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = cdftask.PK;
			viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().First(t => t.Task.PK == prvTask.PK).IncludeInIteration = false;
			viewModel.CommitResponse();

			AssertEquals(5, tasks.Count());
			var newcdftask = tasks.Single(t => t.PK != cdftask.PK && t.P9_Description == "cdfTask");
			var newQCBTask = tasks.Single(t => t.PK != qcbTask.PK && t.P9_Description == "qcbTask");
			AssertEquals(3, newcdftask.P9_Sequence);
			AssertEquals(4, newQCBTask.P9_Sequence);
		}

		public void TestCommitResponse_IterationRequired_ShouldCreateIteration_ShouldNotBumpSequenceOfTaskInOtherWorkflow()
		{
			var bucket = BMTestHelper.CreateBucket(system);
			var buffer = BMTestHelper.CreateBuffer(system);
			BMTestHelper.LinkComponents(bucket, buffer);

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMTestHelper.CreateWorkflow(jobHeader, "workflow2");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow1.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMTestHelper.CreateTask(workflow1, sequence: 1, description: "task1");
			var task2 = BMTestHelper.CreateTask(workflow1, sequence: 2, description: "task2");
			var task3 = BMTestHelper.CreateTask(workflow1, sequence: 3, description: "task3");
			var task4 = BMTestHelper.CreateTask(workflow1, sequence: 4, description: "task4");

			var qcbTask = BMTestHelper.CreateTask(workflow1, taskType: "QCB", sequence: 5, description: "qcbTask");

			var task6 = BMTestHelper.CreateTask(workflow1, sequence: 6, description: "task6");
			var task7 = BMTestHelper.CreateTask(workflow2, sequence: 7, description: "task7");
			var task8 = BMTestHelper.CreateTask(workflow2, sequence: 8, description: "task8");

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task2.PK;

			AssertEquals(6, workflow1.Tasks.Count());
			AssertEquals(2, workflow2.Tasks.Count());
			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			AssertEquals("1", workflow1.Sequence);
			AssertEquals("2", workflow2.Sequence);

			viewModel.CommitResponse();

			AssertEquals(6, workflow1.Tasks.Count());
			AssertEquals(2, workflow2.Tasks.Count());
			AssertEquals(3, jobHeader.ProcessHeaders.Count);

			var qualityIterationWorkflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 1)");

			AssertEquals(4, qualityIterationWorkflow.Tasks.Count());
			AssertEquals(buffer.PK, qualityIterationWorkflow.FH_FC_CurrentComponent);
			AssertEquals(true, qualityIterationWorkflow.SynchroniseBufferPenetration);

			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(2, task2.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
			AssertEquals(4, task4.P9_Sequence);
			AssertEquals(5, qcbTask.P9_Sequence);
			AssertEquals(10, task6.P9_Sequence);
			AssertEquals(7, task7.P9_Sequence);
			AssertEquals(8, task8.P9_Sequence);

			var newTask2 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task2");
			var newTask3 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task3");
			var newTask4 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task4");
			var newQCBTask = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "qcbTask");

			AssertEquals(6, newTask2.P9_Sequence);
			AssertEquals(7, newTask3.P9_Sequence);
			AssertEquals(8, newTask4.P9_Sequence);
			AssertEquals(9, newQCBTask.P9_Sequence);

			AssertEquals(qualityIterationWorkflow.PK, newTask2.P9_FH_ProcessHeader);
			AssertEquals(qualityIterationWorkflow.PK, newTask3.P9_FH_ProcessHeader);
			AssertEquals(qualityIterationWorkflow.PK, newTask4.P9_FH_ProcessHeader);
			AssertEquals(qualityIterationWorkflow.PK, newQCBTask.P9_FH_ProcessHeader);

			AssertEquals("1", workflow1.Sequence);
			AssertEquals("2", workflow2.Sequence);
			AssertEquals("1.1", qualityIterationWorkflow.Sequence);
		}

		public void TestCommitResponse_NoCardNotesOrNotes()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");

			var iterateFromTask = (ProcessTask)BMTestHelper.CreateTask(workflow1, sequence: 2, description: "task to iterate from");
			var qcbTask = (ProcessTask)BMTestHelper.CreateTask(workflow1, taskType: "QCB", sequence: 5, description: "qcbTask");
			qcbTask.P9_NotesAsString = "the notes!";
			qcbTask.P9_CardNote = "the card note!";

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = iterateFromTask.PK;
			viewModel.CommitResponse();

			var newWorkflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 1)");
			var newTask = (ProcessTask)newWorkflow.Tasks.First(t => t.P9_Description == qcbTask.P9_Description);

			AssertEquals("New tasks created from a quality iteration should have an empty card note.", ZString.Empty, newTask.P9_CardNote);
			AssertEquals("New tasks created from a quality iteration should have empty notes.", ZBlob.Empty, newTask.P9_Notes);
		}

		public void TestCommitResponse_EmptyActualDuration()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");

			var task1 = BMTestHelper.CreateTask(workflow1, sequence: 1, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMTestHelper.CreateTask(workflow1, sequence: 2, description: "task2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = BMTestHelper.CreateTask(workflow1, taskType: "QCB", sequence: 5, description: "qcbTask", taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			((ProcessTask)task1).P9_ActualDuration = TestDateAttribute.Date.AddHours(1);
			((ProcessTask)task2).P9_ActualDuration = TestDateAttribute.Date.AddHours(1);
			((ProcessTask)qcbTask).P9_ActualDuration = TestDateAttribute.Date.AddMinutes(5);

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;

			viewModel.CommitResponse();

			var newWorkflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 1)");

			foreach (ProcessTask task in newWorkflow.Tasks)
			{
				AssertEquals(ZDateTime.Empty, task.P9_ActualDuration);
			}
		}

		public void TestCommitResponse_IterationRequired_ShouldPushIterationIntoBufferEvenWhenNoSecurity()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var bucket = BMTestHelper.CreateBucket(system);
			var buffer = BMTestHelper.CreateBuffer(system);
			BMTestHelper.LinkComponents(bucket, buffer);

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");

			workflow1.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMTestHelper.CreateTask(workflow1, sequence: 1, description: "task1");
			var qcbTask = BMTestHelper.CreateTask(workflow1, taskType: "QCB", sequence: 5, description: "qcbTask");

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				viewModel.CommitResponse();
			}

			var qualityIterationWorkflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 1)");
			AssertEquals(buffer.PK, qualityIterationWorkflow.FH_FC_CurrentComponent);
			AssertNoErrors(qualityIterationWorkflow.FH_FC_CurrentComponentInfo);
		}

		public void TestCommitResponse_IterationRequired_ShouldCreateIterationGenPivots()
		{
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals(task1.PK, viewModel.IterateFromTaskPK);

			var pivots = qcbTask.GetContainmentBarrierIterationLinks();
			AssertEquals(0, pivots.Count);

			viewModel.CommitResponse();

			pivots = qcbTask.GetContainmentBarrierIterationLinks();
			AssertEquals(1, pivots.Count);

			var qiWorkflow = workflow.JobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement.EndsWith("(Quality Iteration 1)"));
			var pivot = pivots.Single(p => p.P9I_LinkType == IterationLinkTypeList.Codes.QualityIterationTask);

			AssertEquals(qiWorkflow.PK, pivot.P9I_FH_IterationWorkflow);
			AssertEquals(task1.PK, pivot.P9I_P9_IterationTask);

			var newViewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			newViewModel.Response = ContainmentBarrierResponses.Passed;
		}

		public void TestCommitResponse_IterationRequired_WhenPassedQCBTasksIncluded_ShouldCreateIterationGenPivotsWithNumberOfPassedQCBTasks()
		{
			var task2 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2);
			var task3 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3);
			var task4 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 4);
			var task21 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 21);

			task2.P9_Type = "QCB";
			task3.P9_Type = "QCB";
			task4.P9_Type = "QCB";
			task21.P9_Type = "QCB";

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals(task1.PK, viewModel.IterateFromTaskPK);
			viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Single(t => t.Task.PK == task3.PK).IncludeInIteration = false;

			viewModel.CommitResponse();

			var pivots = qcbTask.GetContainmentBarrierIterationLinks();
			AssertEquals(3, pivots.Count);

			AssertEquals(viewModel.Lookups.IterationReasonsRegistryLists[0].Code, pivots[0].P9I_IterationReason);
			AssertEquals(viewModel.Lookups.IterationReasonsRegistryLists[0].Code, pivots[1].P9I_IterationReason);
			AssertEquals(viewModel.Lookups.IterationReasonsRegistryLists[0].Code, pivots[2].P9I_IterationReason);

			var iterationLink = pivots.Single(p => p.P9I_LinkType == IterationLinkTypeList.Codes.QualityIterationTask);
			AssertEquals(task1.PK, iterationLink.P9I_P9_IterationTask);

			var passedQCBPivots = pivots.Where(p => p.P9I_LinkType == IterationLinkTypeList.Codes.PassedContainmentBarrier).ToArray();
			AssertEquals(2, passedQCBPivots.Length);
			AssertCollectionContains(passedQCBPivots, p => p.P9I_P9_IterationTask == task2.PK);
			AssertCollectionContains(passedQCBPivots, p => p.P9I_P9_IterationTask == task4.PK);
		}

		public void TestCommitResponse_IterationRequired_WhenPassedAndFailedQCBTasksIncluded_ShouldCreateIterationGenPivotsWithNumberOfPassedQCBTasks()
		{
			var task2 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2);
			var task3 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3);
			var task4 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 4);
			var task21 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 21);

			task2.P9_Type = "QCB";
			task3.P9_Type = "QCB";
			task4.P9_Type = "QCB";
			task21.P9_Type = "QCB";

			CreateQualityIteration(task3, task2);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals(task1.PK, viewModel.IterateFromTaskPK);

			viewModel.CommitResponse();

			var pivots = qcbTask.GetContainmentBarrierIterationLinks();
			AssertEquals(3, pivots.Count);

			var iterateFromPivot = pivots.Single(p => p.P9I_LinkType == IterationLinkTypeList.Codes.QualityIterationTask);
			AssertEquals(qcbTask.PK, iterateFromPivot.P9I_P9_ContainmentBarrierTask);
			AssertEquals(task1.PK, iterateFromPivot.P9I_P9_IterationTask);

			var passedQCBPivots = pivots.Where(p => p.P9I_LinkType == IterationLinkTypeList.Codes.PassedContainmentBarrier).ToArray();
			AssertEquals(2, passedQCBPivots.Length);
			AssertCollectionContains(passedQCBPivots, p => p.P9I_P9_IterationTask == task2.PK);
			AssertCollectionContains(passedQCBPivots, p => p.P9I_P9_IterationTask == task4.PK);
		}

		public void TestCommitResponse_IterationRequired_SameWorkflow_WhenPreviewTasksContainmentBarriers_PassedContainmentBarrierLinksShouldHaveSequenceOne()
		{
			var task2 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2);
			var task3 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3);
			var task4 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 4);
			var task21 = BMTestHelper.CreateTask(workflow, task1.P9_GS_NKAssignedStaffMember, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 21);

			task2.P9_Type = "QCB";
			task3.P9_Type = "QCB";
			task4.P9_Type = "QCB";
			task21.P9_Type = "QCB";

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertEquals(task1.PK, viewModel.IterateFromTaskPK);
			viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Single(t => t.Task.PK == task3.PK).IncludeInIteration = false;

			viewModel.ShouldCreateWorkflowForIteration = false;
			viewModel.CommitResponse();

			var pivots = qcbTask.GetContainmentBarrierIterationLinks();
			AssertEquals(3, pivots.Count);

			var passedQCBPivots = pivots.Where(p => p.P9I_LinkType == IterationLinkTypeList.Codes.PassedContainmentBarrier).ToArray();
			AssertEquals(2, passedQCBPivots.Length);
			AssertEquals("PassedContainmentBarrier links should always have a sequence of 1.", (ZByte)1, passedQCBPivots[0].P9I_Sequence);
			AssertEquals("PassedContainmentBarrier links should always have a sequence of 1.", (ZByte)1, passedQCBPivots[1].P9I_Sequence);
		}

		public void TestCommitResponse_IterationRequired_ShouldCreateProcessTaskIterationLinkPivotRows()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");

			var task1 = BMTestHelper.CreateTask(workflow, sequence: 1, description: "task1");
			var task2 = BMTestHelper.CreateTask(workflow, sequence: 2, description: "task2");
			var task3 = BMTestHelper.CreateTask(workflow, sequence: 3, description: "task3");
			var task4 = BMTestHelper.CreateTask(workflow, sequence: 4, description: "task4");

			var qcbTask = BMTestHelper.CreateTask(workflow, taskType: "QCB", sequence: 5, description: "qcbTask");

			var task6 = BMTestHelper.CreateTask(workflow, sequence: 6, description: "task6");
			var task7 = BMTestHelper.CreateTask(workflow, sequence: 7, description: "task7");
			var task8 = BMTestHelper.CreateTask(workflow, sequence: 8, description: "task8");

			Factory.Save();

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task2.PK;

			AssertEquals(8, workflow.Tasks.Count());

			var pivots = Factory.Load<ProcessTaskIterationLinkPivot>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("There should be no pivots in the database yet. SAD!", Array.Empty<string>(), pivots.Select(x => x.Task?.P9_Description));

			viewModel.CommitResponse();

			var qualityIterationWorkflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 1)");

			AssertEquals(4, qualityIterationWorkflow.Tasks.Count());

			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(2, task2.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
			AssertEquals(4, task4.P9_Sequence);
			AssertEquals(5, qcbTask.P9_Sequence);
			AssertEquals(10, task6.P9_Sequence);
			AssertEquals(11, task7.P9_Sequence);
			AssertEquals(12, task8.P9_Sequence);

			var newTask2 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task2");
			var newTask3 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task3");
			var newTask4 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task4");
			var newQCBTask = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "qcbTask");

			AssertEquals(6, newTask2.P9_Sequence);
			AssertEquals(7, newTask3.P9_Sequence);
			AssertEquals(8, newTask4.P9_Sequence);
			AssertEquals(9, newQCBTask.P9_Sequence);

			newTask2.P9_Description += " (new)";
			newTask3.P9_Description += " (new)";
			newTask4.P9_Description += " (new)";
			newQCBTask.P9_Description += " (new)";

			pivots = Factory.Load<ProcessTaskIterationLinkPivot>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Pivots should have been added for the iteration tasks only. SAD!", new[] { "task2 (new)", "task3 (new)", "task4 (new)", "qcbTask (new)" }, pivots.Select(x => x.Task?.P9_Description.ToString()));

			var iterationLink = Factory.Load<ProcessTaskIterationLink>(new ZQuery(ProcessTaskIterationLinkSchema.P9I_P9_ContainmentBarrierTask, qcbTask.PK)).Single();

			CombineAssertions("The pivots should have the expected properties. SAD!", () =>
			{
				foreach (var pivot in pivots)
				{
					AssertEquals(nameof(pivot.P9P_P9I_Iteration), iterationLink.PK, pivot.P9P_P9I_Iteration);
					AssertEquals(nameof(pivot.P9P_ParentTableCode), newQCBTask.P9_ParentTableCode, pivot.P9P_ParentTableCode);
					AssertEquals(nameof(pivot.P9P_ParentId), jobHeader.FH_ParentId, pivot.P9P_ParentId);
				}
			});
		}

		public void TestCommitResponse_IterationRequired_WithCopiedTasksFromPreviousIteration_ShouldNotAttemptToCreateDuplicatePivotRows()
		{
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = true;

			viewModel.CommitResponse();
			Factory.Save();

			var qualityIterationWorkflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow (Quality Iteration 1)");
			var newQbcTask = qualityIterationWorkflow.Tasks.Single(x => x.P9_Type == "QCB");
			var newIterateFromTask = qualityIterationWorkflow.Tasks.Single(x => x.P9_Type == task1.P9_Type);

			viewModel = GetViewModel(newQbcTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = newIterateFromTask.PK;
			viewModel.ShouldCreateWorkflowForIteration = true;

			viewModel.CommitResponse();

			AssertNoExceptionThrown("Creating a nested QI should only create pivots for the cloned tasks once, because doing so twice will violate the unique index. SAD!", Factory.Save);
		}

		public void TestCommitResponse_IterationRequired_WhenShouldCreateWorkflowForIterationIsFalse_ShouldUseOriginalWorkflowForIteration()
		{
			var workflows = jobHeader.ProcessHeaders.Cast<IProcessHeader>();
			AssertContainsExactElementsInAnyOrder(new[] { "workflow" }, workflows.Select(x => x.FH_CompletionStatement));
			AssertEquals(2, workflow.Tasks.Count());

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			workflows = jobHeader.ProcessHeaders.Cast<IProcessHeader>();
			AssertContainsExactElementsInAnyOrder("ShouldCreateWorkflowForIteration is false, so no new workflows should have been created. SAD!", new[] { "workflow" }, workflows.Select(x => x.FH_CompletionStatement));

			AssertEquals(4, workflow.Tasks.Count());

			var iterationLink = Factory.Load<ProcessTaskIterationLink>(new ZQuery()).Single();
			AssertEquals(workflow.PK, iterationLink.IterationWorkflow.PK);
		}

		public void TestCommitResponse_IterationRequired_AsNestedIteration_WhenShouldCreateWorkflowForIterationIsFalse_ShouldBumpIterationSequenceNumber()
		{
			qcbTask.P9_Sequence = 2;

			var workflows = jobHeader.ProcessHeaders.Cast<IProcessHeader>();
			AssertContainsExactElementsInAnyOrder(new[] { "workflow" }, workflows.Select(x => x.FH_CompletionStatement));
			AssertEquals(2, workflow.Tasks.Count());

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			workflows = jobHeader.ProcessHeaders.Cast<IProcessHeader>();
			AssertContainsExactElementsInAnyOrder("ShouldCreateWorkflowForIteration is false, so no new workflows should have been created. SAD!", new[] { "workflow" }, workflows.Select(x => x.FH_CompletionStatement));

			AssertEquals(4, workflow.Tasks.Count());

			var iterationLink = Factory.Load<ProcessTaskIterationLink>(new ZQuery()).Single();
			AssertEquals(workflow.PK, iterationLink.IterationWorkflow.PK);
			AssertEquals((byte)1, iterationLink.P9I_Sequence);

			var qcbTaskForIteration1 = workflow.Tasks.Single(x => x.P9_Sequence == 4);

			viewModel = GetViewModel(qcbTaskForIteration1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = workflow.Tasks.Single(x => x.P9_Sequence == 3).PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			AssertEquals(6, workflow.Tasks.Count());

			iterationLink = GetIterationWhichIsNot(iterationLink);
			AssertEquals(workflow.PK, iterationLink.IterationWorkflow.PK);
			AssertEquals("The link sequence should have been incremented because it's the second iteration for the same workflow. SAD!", (byte)2, iterationLink.P9I_Sequence);
		}

		public void TestCommitResponse_IterationRequired_AsNestedIterationOf255thIteration_ShouldNotBumpIterationSequenceNumber_SoThatItDoesNotOverflow()
		{
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var iterationLink = Factory.Load<ProcessTaskIterationLink>(new ZQuery()).Single();
			iterationLink.P9I_Sequence = byte.MaxValue;

			Factory.Save();

			var qcbTaskForIteration1 = workflow.Tasks.MaxBy(x => x.P9_Sequence);

			viewModel = GetViewModel(qcbTaskForIteration1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			iterationLink = GetIterationWhichIsNot(iterationLink);
			AssertEquals("In this unlikely scenario, we've reached the end of possible sequence numbers, so we should just repeat the highest number forever.", byte.MaxValue, iterationLink.P9I_Sequence);

			AssertNoExceptionThrown("We haven't allowed the sequence field to overflow, so there should be no problem saving to the database. SAD!", Factory.Save);
		}

		public void TestCommitResponse_IterationRequired_AsNestedIteration_WhenShouldCreateWorkflowForIterationIsTrue_ShouldAlwaysMakeIterationSequenceOne()
		{
			qcbTask.P9_Sequence = 2;

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var iterationLink1 = Factory.Load<ProcessTaskIterationLink>(new ZQuery()).Single();
			AssertEquals((byte)1, iterationLink1.P9I_Sequence);

			var qcbTaskForIteration1 = workflow.Tasks.Single(x => x.P9_Sequence == 4);

			viewModel = GetViewModel(qcbTaskForIteration1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = workflow.Tasks.Single(x => x.P9_Sequence == 3).PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var iterationLink2 = GetIterationWhichIsNot(iterationLink1);
			AssertEquals((byte)2, iterationLink2.P9I_Sequence);

			var qcbTaskForIteration2 = workflow.Tasks.Single(x => x.P9_Sequence == 6);

			viewModel = GetViewModel(qcbTaskForIteration2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = workflow.Tasks.Single(x => x.P9_Sequence == 5).PK;
			viewModel.ShouldCreateWorkflowForIteration = true;

			viewModel.CommitResponse();

			var iterationLink3 = GetIterationWhichIsNot(iterationLink1, iterationLink2);
			AssertEquals("Since this iteration was created with a new workflow, it should have a sequence of 1. SAD!", (byte)1, iterationLink3.P9I_Sequence);

			var iteration3Workflow = iterationLink3.IterationWorkflow;
			var qcbTaskForIteration3 = iteration3Workflow.Tasks.Single(x => x.P9_Sequence == 8);

			viewModel = GetViewModel(qcbTaskForIteration3, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = iteration3Workflow.Tasks.Single(x => x.P9_Sequence == 7).PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var iterationLink4 = GetIterationWhichIsNot(iterationLink1, iterationLink2, iterationLink3);
			AssertEquals("Since this iteration didn't create a new workflow, it should increment from its parent iteration. SAD!. SAD!", (byte)2, iterationLink4.P9I_Sequence);
		}

		public void TestCommitResponse_SameWorkflow_WhenPreviewTasksContainAContainmentBarrierTask_ShouldNotSkipIterationSequenceNumbers()
		{
			var otherCbTask = BMTestHelper.CreateTask(workflow, taskType: "QCB", sequence: 10);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var iterationLink = Factory.Load<ProcessTaskIterationLink>(new ZQuery(ProcessTaskIterationLinkSchema.P9I_LinkType, IterationLinkTypeList.Codes.QualityIterationTask)).Single();
			AssertEquals((ZByte)1, iterationLink.P9I_Sequence);

			Factory.Save();

			var qcbTaskForIteration1 = workflow.Tasks.MaxBy(x => x.P9_Sequence);

			viewModel = GetViewModel(qcbTaskForIteration1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var pivots = qcbTask.GetContainmentBarrierIterationLinks();
			AssertEquals(2, pivots.Count);

			var passedPivot = pivots.SingleOrDefault(p => p.P9I_LinkType == IterationLinkTypeList.Codes.PassedContainmentBarrier);
			AssertNotNull("Since there was a CB task in the iterated tasks, there should be a PassedContainmentBarrier link.", passedPivot);

			iterationLink = GetIterationWhichIsNot(iterationLink);
			AssertEquals("The new iteration should not skip sequence numbers even though there are PassedContainmentBarrier links in the workflow.", (ZByte)2, iterationLink.P9I_Sequence);
		}

		public void TestCommitResponse_IterationRequired_ThenIterateAgainOnIterationCB_ProducingNewWorklows()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");

			var task1 = BMTestHelper.CreateTask(workflow, sequence: 1, description: "task1");
			var task2 = BMTestHelper.CreateTask(workflow, sequence: 3, description: "task2");
			var review1 = BMTestHelper.CreateTask(workflow, taskType: "QCB", sequence: 5, description: "review1");
			var task3 = BMTestHelper.CreateTask(workflow, sequence: 7, description: "task3");
			var task4 = BMTestHelper.CreateTask(workflow, sequence: 9, description: "task4");
			var review2 = BMTestHelper.CreateTask(workflow, taskType: "QCB", sequence: 11, description: "review2");
			var task5 = BMTestHelper.CreateTask(workflow, sequence: 13, description: "task5");

			Factory.Save();

			var viewModel = GetViewModel(review1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			AssertEquals(7, workflow.Tasks.Count());

			viewModel.CommitResponse();
			var qualityIterationWorkflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 1)");
			AssertEquals(3, qualityIterationWorkflow.Tasks.Count());

			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(3, task2.P9_Sequence);
			AssertEquals(5, review1.P9_Sequence);
			AssertEquals(10, task3.P9_Sequence);
			AssertEquals(12, task4.P9_Sequence);
			AssertEquals(14, review2.P9_Sequence);
			AssertEquals(16, task5.P9_Sequence);

			var task1_iter1 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task1");
			var task2_iter1 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task2");
			var review1_iter1 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "review1");

			AssertEquals(6, task1_iter1.P9_Sequence);
			AssertEquals(7, task2_iter1.P9_Sequence);
			AssertEquals(8, review1_iter1.P9_Sequence);

			viewModel = GetViewModel(review1_iter1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1_iter1.PK;

			viewModel.CommitResponse();
			var qualityIterationWorkflow2 = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 2)");
			AssertEquals(3, qualityIterationWorkflow2.Tasks.Count());

			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(3, task2.P9_Sequence);
			AssertEquals(5, review1.P9_Sequence);
			AssertEquals(6, task1_iter1.P9_Sequence);
			AssertEquals(7, task2_iter1.P9_Sequence);
			AssertEquals(8, review1_iter1.P9_Sequence);
			AssertEquals(13, task3.P9_Sequence);
			AssertEquals(15, task4.P9_Sequence);
			AssertEquals(17, review2.P9_Sequence);
			AssertEquals(19, task5.P9_Sequence);

			var task1_iter2 = qualityIterationWorkflow2.Tasks.Single(t => t.P9_Description == "task1");
			var task2_iter2 = qualityIterationWorkflow2.Tasks.Single(t => t.P9_Description == "task2");
			var review1_iter2 = qualityIterationWorkflow2.Tasks.Single(t => t.P9_Description == "review1");

			AssertEquals(9, task1_iter2.P9_Sequence);
			AssertEquals(10, task2_iter2.P9_Sequence);
			AssertEquals(11, review1_iter2.P9_Sequence);
		}

		public void TestCommitResponse_IterationRequired_ThenIterateAgainOnIterationCB_NotProducingNewWorklows()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");

			var task1 = BMTestHelper.CreateTask(workflow, sequence: 1, description: "task1");
			var task2 = BMTestHelper.CreateTask(workflow, sequence: 3, description: "task2");
			var review1 = BMTestHelper.CreateTask(workflow, taskType: "QCB", sequence: 5, description: "review1");
			var task3 = BMTestHelper.CreateTask(workflow, sequence: 7, description: "task3");
			var task4 = BMTestHelper.CreateTask(workflow, sequence: 9, description: "task4");
			var review2 = BMTestHelper.CreateTask(workflow, taskType: "QCB", sequence: 11, description: "review2");
			var task5 = BMTestHelper.CreateTask(workflow, sequence: 13, description: "task5");

			Factory.Save();

			var viewModel = GetViewModel(review1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			AssertEquals(7, workflow.Tasks.Count());

			viewModel.CommitResponse();
			AssertEquals(10, workflow.Tasks.Count());

			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(3, task2.P9_Sequence);
			AssertEquals(5, review1.P9_Sequence);
			AssertEquals(10, task3.P9_Sequence);
			AssertEquals(12, task4.P9_Sequence);
			AssertEquals(14, review2.P9_Sequence);
			AssertEquals(16, task5.P9_Sequence);

			var task1_iter1 = workflow.Tasks.Single(t => t.P9_Description == "task1" && (t as ProcessTask).Iteration == "1");
			var task2_iter1 = workflow.Tasks.Single(t => t.P9_Description == "task2" && (t as ProcessTask).Iteration == "1");
			var review1_iter1 = workflow.Tasks.Single(t => t.P9_Description == "review1" && (t as ProcessTask).Iteration == "1");

			AssertEquals(6, task1_iter1.P9_Sequence);
			AssertEquals(7, task2_iter1.P9_Sequence);
			AssertEquals(8, review1_iter1.P9_Sequence);

			viewModel = GetViewModel(review1_iter1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1_iter1.PK;

			viewModel.CommitResponse();
			AssertEquals(13, workflow.Tasks.Count());

			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(3, task2.P9_Sequence);
			AssertEquals(5, review1.P9_Sequence);
			AssertEquals(6, task1_iter1.P9_Sequence);
			AssertEquals(7, task2_iter1.P9_Sequence);
			AssertEquals(8, review1_iter1.P9_Sequence);
			AssertEquals(13, task3.P9_Sequence);
			AssertEquals(15, task4.P9_Sequence);
			AssertEquals(17, review2.P9_Sequence);
			AssertEquals(19, task5.P9_Sequence);

			var task1_iter2 = workflow.Tasks.Single(t => t.P9_Description == "task1" && (t as ProcessTask).Iteration == "2");
			var task2_iter2 = workflow.Tasks.Single(t => t.P9_Description == "task2" && (t as ProcessTask).Iteration == "2");
			var review1_iter2 = workflow.Tasks.Single(t => t.P9_Description == "review1" && (t as ProcessTask).Iteration == "2");

			AssertEquals(9, task1_iter2.P9_Sequence);
			AssertEquals(10, task2_iter2.P9_Sequence);
			AssertEquals(11, review1_iter2.P9_Sequence);
		}

		public void TestCommitResponse_AcceptQualityIteration()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "Elijah Wood";

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var qcbTask2 = BMTestHelper.CreateTask(workflow, resource.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB", sequence: qcbTask.P9_Sequence);
			CreateQualityIteration(qcbTask2, task1);

			viewModel.Response = ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource;
			AssertEquals(ZGuid.Empty, viewModel.IterateFromTaskPK);
			viewModel.CommitResponse();

			AssertRtfText("FRO 20-Aug-14 00:00: Accepted Quality Iteration created by Elijah Wood", ((ProcessTask)qcbTask).P9_Notes);
		}

		public void TestCommitResponse_SortTasksAfterQualityIterationIsCreated()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var bindingListView = workflow.TaskCollectionIncludingChildWorkflowTasksBindingListView;

			AssertEquals("Ensure that tasks grid is not sorted", 0, bindingListView.SortDescriptions.Count);

			BMTestHelper.CreateTask(workflow, sequence: 7, description: "task3");
			var task1 = BMTestHelper.CreateTask(workflow, sequence: 1, description: "task1");
			BMTestHelper.CreateTask(workflow, sequence: 13, description: "task5");
			BMTestHelper.CreateTask(workflow, sequence: 3, description: "task2");
			var review1 = BMTestHelper.CreateTask(workflow, sequence: 5, taskType: "QCB", description: "review1");
			BMTestHelper.CreateTask(workflow, sequence: 9, description: "task4");
			BMTestHelper.CreateTask(workflow, sequence: 11, taskType: "QCB", description: "review2");
			var viewModel = GetViewModel(review1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;
			viewModel.CommitResponse();

			AssertArrayEqualsByElements(
				"Tasks grid should be sorted by sequence ascending order after QI is created",
				workflow.Tasks.OrderBy(t => t.P9_Sequence).ToArray(),
				bindingListView.Cast<IProcessTask>().ToArray());

			var properties = TypeDescriptor.GetProperties(typeof(IProcessTask));
			var sorts = new ListSortDescriptionCollection(new ListSortDescription[]
			{
				new ListSortDescription(properties[ProcessTasksSchema.P9_Sequence.Name], ListSortDirection.Descending)
			});
			bindingListView.ApplySort(sorts);

			AssertArrayEqualsByElements(
				"Ensure that tasks grid is sorted by sequence descending order",
				workflow.Tasks.OrderByDescending(t => t.P9_Sequence).ToArray(),
				bindingListView.Cast<IProcessTask>().ToArray());

			var task1_iter1 = workflow.Tasks.Single(t => t.P9_Description == "task1" && (t as ProcessTask).Iteration == "1");
			var review1_iter1 = workflow.Tasks.Single(t => t.P9_Description == "review1" && (t as ProcessTask).Iteration == "1");

			viewModel = GetViewModel(review1_iter1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1_iter1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;
			viewModel.CommitResponse();

			AssertArrayEqualsByElements(
				"Tasks grid should be sorted by existing order (sequence descending) after QI is created",
				workflow.Tasks.OrderByDescending(t => t.P9_Sequence).ToArray(),
				bindingListView.Cast<IProcessTask>().ToArray());
		}

		public void TestCommitResponse_ShouldNotErrorReport_WhenSwitchingFromCWWebUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "CQD";
			staff.GS_LoginName = "come.quickly.distress";

			Factory.Save();

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");

			var qcbTask = (ProcessTask)BMTestHelper.CreateTask(workflow, taskType: "QCB");
			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.QcbCreatingUserLoginName = staff.GS_LoginName;
			AssertRtfText(ZString.Empty, qcbTask.P9_Notes);

			viewModel.Response = ContainmentBarrierResponses.Passed;

			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWWeb", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				viewModel.CommitResponse();
			}

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		#endregion

		#region ShouldCreateWorkflowForIteration

		public void TestShouldCreateWorkflowForIteration_WhenRelevantRegistryItemEnabled_ShouldDefaultToTrue()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals(true, viewModel.ShouldCreateWorkflowForIteration);
		}

		public void TestShouldCreateWorkflowForIteration_WhenRelevantRegistryItemDisabled_ShouldDefaultToFalse()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals(false, viewModel.ShouldCreateWorkflowForIteration);
		}

		public void TestShouldCreateWorkflowForIteration_WhenRegistryItemSetToTrue_ShouldNotBeReadOnly()
		{
			WorkflowDataRegistry.Instance.AllowUsersToChangeIterationWorkflowCreationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals(false, viewModel.ShouldCreateWorkflowForIterationInfo.ReadOnly);
		}

		public void TestShouldCreateWorkflowForIteration_WhenRegistryItemSetToFalse_ShouldBeReadOnly()
		{
			WorkflowDataRegistry.Instance.AllowUsersToChangeIterationWorkflowCreationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals(true, viewModel.ShouldCreateWorkflowForIterationInfo.ReadOnly);
		}

		#endregion

		#region Parent Iteration

		public void TestCommitResponse_CreateNestedQualityIteration_ShouldPopulateP9I_P9I_ParentIteration_SameWorkflow()
		{
			qcbTask.P9_Sequence = 2;

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var iterationLink1 = Factory.Load<ProcessTaskIterationLink>(new ZQuery()).Single();
			AssertEquals("The top-level iteration should not have a parent iteration. SAD!", ZGuid.Empty, iterationLink1.P9I_P9I_ParentIteration);

			var qcbTaskForIteration1 = workflow.Tasks.Single(x => x.P9_Sequence == 4);

			viewModel = GetViewModel(qcbTaskForIteration1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = workflow.Tasks.Single(x => x.P9_Sequence == 3).PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var iterationLink2 = GetIterationWhichIsNot(iterationLink1);
			AssertEquals("The iteration is nested, so its parent should be recorded. SAD!", iterationLink1.PK, iterationLink2.P9I_P9I_ParentIteration);

			var qcbTaskForIteration2 = workflow.Tasks.Single(x => x.P9_Sequence == 6);

			viewModel = GetViewModel(qcbTaskForIteration2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = workflow.Tasks.Single(x => x.P9_Sequence == 5).PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var iterationLink3 = GetIterationWhichIsNot(iterationLink1, iterationLink2);
			AssertEquals("The parent iteration should be the immediate parent, not the top-level iteration. SAD!", iterationLink2.PK, iterationLink3.P9I_P9I_ParentIteration);

			var newTopLevelTask = BMTestHelper.CreateTask(workflow, taskType: "QCB", sequence: 20);
			viewModel = GetViewModel(newTopLevelTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = workflow.Tasks.Single(x => x.P9_Sequence == 1).PK;
			viewModel.ShouldCreateWorkflowForIteration = false;

			viewModel.CommitResponse();

			var iterationLink4 = GetIterationWhichIsNot(iterationLink1, iterationLink2, iterationLink3);
			AssertEquals("This is another top-level iteration, so it should have no parent. SAD!", ZGuid.Empty, iterationLink4.P9I_P9I_ParentIteration);
		}

		public void TestCommitResponse_CreateNestedQualityIteration_ShouldPopulateP9I_P9I_ParentIteration_NewWorkflows()
		{
			qcbTask.P9_Sequence = 2;

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.ShouldCreateWorkflowForIteration = true;

			viewModel.CommitResponse();

			var iterationLink1 = Factory.Load<ProcessTaskIterationLink>(new ZQuery()).Single();
			AssertEquals("The top-level iteration should not have a parent iteration. SAD!", ZGuid.Empty, iterationLink1.P9I_P9I_ParentIteration);

			var iteration1Workflow = iterationLink1.IterationWorkflow;
			var qcbTaskForIteration1 = iteration1Workflow.Tasks.Single(x => x.P9_Sequence == 4);

			viewModel = GetViewModel(qcbTaskForIteration1, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = iteration1Workflow.Tasks.Single(x => x.P9_Sequence == 3).PK;
			viewModel.ShouldCreateWorkflowForIteration = true;

			viewModel.CommitResponse();

			var iterationLink2 = GetIterationWhichIsNot(iterationLink1);
			AssertEquals("The iteration is nested, so its parent should be recorded. SAD!", iterationLink1.PK, iterationLink2.P9I_P9I_ParentIteration);

			var iteration2Workflow = iterationLink2.IterationWorkflow;
			var qcbTaskForIteration2 = iteration2Workflow.Tasks.Single(x => x.P9_Sequence == 6);

			viewModel = GetViewModel(qcbTaskForIteration2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = iteration2Workflow.Tasks.Single(x => x.P9_Sequence == 5).PK;
			viewModel.ShouldCreateWorkflowForIteration = true;

			viewModel.CommitResponse();

			var iterationLink3 = GetIterationWhichIsNot(iterationLink1, iterationLink2);
			AssertEquals("The parent iteration should be the immediate parent, not the top-level iteration. SAD!", iterationLink2.PK, iterationLink3.P9I_P9I_ParentIteration);

			var newTopLevelTask = (ProcessTask)BMTestHelper.CreateTask(iteration2Workflow, taskType: "QCB", sequence: 20);
			newTopLevelTask.Iteration = ZString.Empty;

			viewModel = GetViewModel(newTopLevelTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = iteration2Workflow.Tasks.Single(x => x.P9_Sequence == 5).PK;
			viewModel.ShouldCreateWorkflowForIteration = true;

			viewModel.CommitResponse();

			var iterationLink4 = GetIterationWhichIsNot(iterationLink1, iterationLink2, iterationLink3);
			AssertEquals("This is another top-level iteration (even though its task was in a workflow that was created as part of a previous iteration), so it should have no parent. SAD!", ZGuid.Empty, iterationLink4.P9I_P9I_ParentIteration);
		}

		#endregion

		#region Deleting Task

		public void TestDeleteTaskWithQCBGenPivots_ShouldAlsoDeletePivots()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");

			var iterateFromTask = BMTestHelper.CreateTask(workflow);
			var qcbTask = BMTestHelper.CreateTask(workflow, taskType: "QCB");

			CreateQualityIteration(qcbTask, iterateFromTask);
			var pivot = Factory.LoadTop1<ProcessTaskIterationLink>(new ZQuery());

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedQCBTask = newFactory.Load<ProcessTask>(qcbTask.PK);
			loadedQCBTask.Delete();

			AssertEquals(false, pivot.IsDeleted);

			newFactory.Save();

			AssertEquals(true, pivot.IsDeleted);
		}

		#endregion

		#region Create Iteration

		[ExpectNoExceptions]
		public void TestCreateThenDeleteIterationAndCreateAnother_ShouldNotCauseDBConstraintViolations()
		{
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateReasonPK = viewModel.Lookups.IterationReasonsRegistryLists.First().PK;
			viewModel.CommitResponse();
			Factory.Save();

			var qualityIterationWorkflow = workflow.JobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement.EndsWith("(Quality Iteration 1)"));
			qualityIterationWorkflow.Delete();
			Factory.Save();

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.CommitResponse();
			Factory.Save();
		}

		public void TestQualityIterationOnWorkflowWithIdenticalSequenceNumbers_ShouldPropagateIdenticalNumbersToIteration()
		{
			var invTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "INV", sequence: 1);
			var cdfTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDF", sequence: 2);
			var astTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "AST", sequence: 2);
			var prvTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "PRV", sequence: 3);
			var cduTask = BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDU", sequence: 3);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = invTask.PK;

			var tasks = viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(t => t.Task);

			var loadedCdfTask = tasks.First(t => t.P9_Type == "CDF");
			var loadedAstTask = tasks.First(t => t.P9_Type == "AST");
			var loadedPrvTask = tasks.First(t => t.P9_Type == "PRV");
			var loadedCduTask = tasks.First(t => t.P9_Type == "CDU");

			AssertEquals(loadedCdfTask.P9_Sequence, loadedAstTask.P9_Sequence);
			AssertEquals(loadedPrvTask.P9_Sequence, loadedCduTask.P9_Sequence);
		}

		public void TestClonedTaskIsNotATemplateTask()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);

			var task1 = (ProcessTask)BMTestHelper.CreateTask(jobHeader, sequence: 1, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			task1.P9_ParentTemplateID = ZGuid.NewZGuid();
			var qcbTask = BMTestHelper.CreateTask(jobHeader, taskType: "QCB", sequence: 2, description: "qcbTask");

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			AssertRtfText(ZString.Empty, qcbTask.P9_Notes);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;
			viewModel.CommitResponse();
			var clonedTask = ((IWorkflowProvider)jobHeader.Parent).WorkflowItems.Tasks.OfType<ProcessTask>().Single(s => s.P9_Description == task1.P9_Description && s.PK != task1.PK);
			AssertEquals(ZGuid.Empty, clonedTask.P9_ParentTemplateID);
		}

		public void TestIteration_SourceRowsAreDeleted()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);
			var qcbTask = BMTestHelper.CreateTask(jobHeader, taskType: "QCB", sequence: 2, description: "qcbTask");

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			AssertRtfText(ZString.Empty, qcbTask.P9_Notes);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = ZGuid.NewZGuid();
			viewModel.IterateReasonPK = ZGuid.NewZGuid();
			viewModel.CommitResponse();
			AssertEquals(1, ((IWorkflowProvider)jobHeader.Parent).WorkflowItems.Tasks.Count);
		}

		#endregion

		#region Implementation

		public ContainmentBarrierViewModel GetViewModel(IProcessTask task, string status)
		{
			var viewModel = new ContainmentBarrierViewModel(task, status, deselectCancelledTasksFromIteration: true);
			disposables.Add(viewModel);
			return viewModel;
		}

		ProcessTaskIterationLink GetIterationWhichIsNot(params ProcessTaskIterationLink[] linksToExclude)
		{
			var query = new ZQuery(ProcessTaskIterationLinkSchema.PK, SQLComparisonOperator.NotEqual, linksToExclude.Select(x => x.PK));
			query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_Outcome, IterationLinkOutcomeList.Codes.IterationRequired);
			query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_LinkType, IterationLinkTypeList.Codes.QualityIterationTask);

			var links = Factory.Load<ProcessTaskIterationLink>(query);

			return links.Single(); // we want this to fail if there's not exactly 1 result so we can fix it.
		}

		protected override void SetUp()
		{
			base.SetUp();
			disposables = new DisposableList(10);

			EnableBufferManagement();
			SetAsQCBTaskType("QCB", "WKI");
			AddIterationReasonToRegistry("WKI", "RS1", "Reason 1");
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			system = BMTestHelper.CreateSystem(Factory, "WKI");
			jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_Code = "FRO";
			task1 = BMTestHelper.CreateTask(workflow, resource.GS_Code, sequence: 1);
			qcbTask = BMTestHelper.CreateTask(workflow, taskType: "QCB", sequence: 20);

			viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			disposables.Add(Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposables.Dispose();
			ClearIterationReasonsFromRegistry("WKI");
		}

		IBMSystem system;
		IProcessJobHeader jobHeader;
		IProcessHeader workflow;
		IProcessTask task1, qcbTask;
		ContainmentBarrierViewModel viewModel;

		DisposableList disposables;

		#endregion
	}

	[GuiTest]
	class ContainmentBarrierViewModelTest : WorkflowTestCase
	{
		#region Resource Under Review Lists 

		// preWorkflow11 -> preWorkflow1 -> currentWorkflow
		//									preWorkflow2 -> currentWorkflow
		public void TestResourceUnderReviewLists_ShouldListResourceOnClosedTasksOnPrereqs()
		{
			var currentWorkflow = SetupTestResourceUnderReviewLists();

			var preWorkflow1 = BMTestHelper.CreateWorkflowAndTask(Factory, "preWorkflow1", description: "preTask1a", staffCode: "PR1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preWorkflow1, description: "preTask1b", staffCode: "PR2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preWorkflow1.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow1, currentWorkflow);

			var preWorkflow11 = BMTestHelper.CreateWorkflowAndTask(Factory, "preWorkflow11", description: "preTask11a", staffCode: "PR3", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preWorkflow11, description: "preTask11b", staffCode: "PR4", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preWorkflow11.GetOrCreateDependencyLink(preWorkflow1);
			BMTestHelper.AssertIsPrerequisite(preWorkflow11, preWorkflow1);

			var preWorkflow2 = BMTestHelper.CreateWorkflowAndTask(Factory, "preWorkflow2", description: "preTask2a", staffCode: "PR5", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preWorkflow2, description: "preTask2b", staffCode: "PR6", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preWorkflow2.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow2, currentWorkflow);

			var qcbTask = currentWorkflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			AssertResourceUnderReviewLists(qcbTask, new string[] { "PR2", "PR4", "PR6" });
		}

		// currentWorkflow -> postWorkflow1 -> postWorkflow11
		// currentWorkflow -> postWorkflow2 									
		public void TestResourceUnderReviewLists_ShouldNotListResourceOnClosedTasksOnPostreqs()
		{
			var currentWorkflow = SetupTestResourceUnderReviewLists();

			var postWorkflow1 = BMTestHelper.CreateWorkflowAndTask(Factory, "postWorkflow1", description: "postTask1a", staffCode: "PO1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(postWorkflow1, description: "postTask1b", staffCode: "PO2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			currentWorkflow.GetOrCreateDependencyLink(postWorkflow1);
			BMTestHelper.AssertIsPrerequisite(currentWorkflow, postWorkflow1);

			var postWorkflow11 = BMTestHelper.CreateWorkflowAndTask(Factory, "postWorkflow11", description: "postTask11a", staffCode: "PO3", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(postWorkflow11, description: "postTask11b", staffCode: "PO4", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			postWorkflow1.GetOrCreateDependencyLink(postWorkflow11);
			BMTestHelper.AssertIsPrerequisite(postWorkflow1, postWorkflow11);

			var postWorkflow2 = BMTestHelper.CreateWorkflowAndTask(Factory, "postWorkflow2", description: "postTask2a", staffCode: "PO5", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(postWorkflow2, description: "postTask2b", staffCode: "PO6", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			currentWorkflow.GetOrCreateDependencyLink(postWorkflow2);
			BMTestHelper.AssertIsPrerequisite(currentWorkflow, postWorkflow2);

			var qcbTask = currentWorkflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			AssertResourceUnderReviewLists(qcbTask, Array.Empty<string>());
		}

		//	parentWorkflow11
		//			|
		//	parentWorkflow1				parentWorkflow2
		//			|												|
		//	currentWorkflow				currentWorkflow
		public void TestResourceUnderReviewLists_ShouldListResourceOnClosedTasksOnParents_SameJob()
		{
			var currentWorkflow = SetupTestResourceUnderReviewLists();

			var jobHeader = currentWorkflow.JobHeader;
			var parentWorkflow1 = BMTestHelper.CreateWorkflow(jobHeader, "parentWorkflow1");
			BMTestHelper.CreateTask(parentWorkflow1, description: "parentTask1a", staffCode: "PA1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(parentWorkflow1, description: "parentTask1b", staffCode: "PA2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow1);
			BMTestHelper.AssertIsParent(currentWorkflow, parentWorkflow1);

			var parentWorkflow11 = BMTestHelper.CreateWorkflow(jobHeader, "parentWorkflow11");
			BMTestHelper.CreateTask(parentWorkflow11, description: "parentTask11a", staffCode: "PA3", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(parentWorkflow11, description: "parentTask11b", staffCode: "PA4", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			parentWorkflow1.GetOrCreateLinkToParent(parentWorkflow11);
			BMTestHelper.AssertIsParent(parentWorkflow1, parentWorkflow11);

			var parentWorkflow2 = BMTestHelper.CreateWorkflow(jobHeader, "parentWorkflow2");
			BMTestHelper.CreateTask(parentWorkflow2, description: "parentTask2a", staffCode: "PA5", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(parentWorkflow2, description: "parentTask2b", staffCode: "PA6", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow2);
			BMTestHelper.AssertIsParent(currentWorkflow, parentWorkflow2);

			var qcbTask = currentWorkflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			AssertResourceUnderReviewLists(qcbTask, new[] { "PA2", "PA4", "PA6" });
		}

		//	parentWorkflow11
		//			|
		//	parentWorkflow1				parentWorkflow2
		//			|												|
		//	currentWorkflow				currentWorkflow
		public void TestResourceUnderReviewLists_ShouldNotListResourceOnClosedTasksOnParents_DifferentJobs()
		{
			var currentWorkflow = SetupTestResourceUnderReviewLists();

			var parentWorkflow1 = BMTestHelper.CreateWorkflowAndTask(Factory, "parentWorkflow1", description: "parentTask1a", staffCode: "PA1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(parentWorkflow1, description: "parentTask1b", staffCode: "PA2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow1);
			BMTestHelper.AssertIsParent(currentWorkflow, parentWorkflow1);

			var parentWorkflow11 = BMTestHelper.CreateWorkflowAndTask(Factory, "parentWorkflow11", description: "parentTask11a", staffCode: "PA3", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(parentWorkflow11, description: "parentTask11b", staffCode: "PA4", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			parentWorkflow1.GetOrCreateLinkToParent(parentWorkflow11);
			BMTestHelper.AssertIsParent(parentWorkflow1, parentWorkflow11);

			var parentWorkflow2 = BMTestHelper.CreateWorkflowAndTask(Factory, "parentWorkflow2", description: "parentTask2a", staffCode: "PA5", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(parentWorkflow2, description: "parentTask2b", staffCode: "PA6", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow2);
			BMTestHelper.AssertIsParent(currentWorkflow, parentWorkflow2);

			var qcbTask = currentWorkflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			AssertResourceUnderReviewLists(qcbTask, Array.Empty<string>());
		}

		public void TestResourceUnderReviewLists_ShouldListResourceOnClosedTasksOnCurrentWorkflow()
		{
			var currentWorkflow = SetupTestResourceUnderReviewLists();

			BMTestHelper.CreateTask(currentWorkflow, staffCode: "CR2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, createStaffIfNotExist: true);
			var currentQCBTask = BMTestHelper.CreateTask(currentWorkflow, staffCode: "CR3", sequence: 3, createStaffIfNotExist: true);

			var qcbTask = currentWorkflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			AssertResourceUnderReviewLists(qcbTask, new string[] { "CR2" });
		}

		IProcessHeader SetupTestResourceUnderReviewLists()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var currentWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "currentWorkflow", description: "qcbTask", staffCode: "CR1", sequence: 100, createStaffIfNotExist: true);
			var qcbTask = currentWorkflow.Tasks.First();
			qcbTask.P9_Type = "QCB";

			return currentWorkflow;
		}

		void AssertResourceUnderReviewLists(ProcessTask qcbTask, IEnumerable<string> expectedResourcesCode)
		{
			var containmentBarrierViewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			containmentBarrierViewModel.Response = ContainmentBarrierResponses.IterationRequired;
			containmentBarrierViewModel.IterateFromTaskPK = qcbTask.PK;

			AssertContainsExactElementsInAnyOrder("GIVEN pre/post/parent/child workflows, WHEN getting ResourceUnderReviewList THEN should return resource that is assigned to closed tasks for pre and parent workflow",
				expectedResourcesCode,
				containmentBarrierViewModel.Lookups.ResourceUnderReviewList.Select(resource => resource.GS_Code));
		}

		//prerequisiteWorkflow - currentWorkflow
		//	|
		//childWorkflow

		public void TestResourceUnderReviewLists_OnePrerequisiteOneChild_ShouldListStaffWorkingOnTasksInPrerequisiteAndChild()
		{
			var currentWorkflow = SetupTestResourceUnderReviewLists();
			var preWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "preWorkflow", description: "preTask1a", staffCode: "PR1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preWorkflow, description: "preTask1b", staffCode: "PR2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preWorkflow.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow, currentWorkflow);

			var preChildWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "preChildWorkflow", description: "childTask1a", staffCode: "CH1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preChildWorkflow, description: "childTask1b", staffCode: "CH2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preChildWorkflow.GetOrCreateLinkToParent(preWorkflow);
			BMTestHelper.AssertIsParent(preChildWorkflow, preWorkflow);

			var qcbTask = currentWorkflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			AssertResourceUnderReviewLists(qcbTask, new string[] { "PR2", "CH2" });
		}

		//prerequisiteWorkflow - currentWorkflow
		//	|
		//childWorkflow
		//	|
		//childWorkflow
		public void TestResourceUnderReviewLists_OnePrerequisiteTwoChildren_ShouldListStaffWorkingOnTasksInPrerequisiteAndChildren()
		{
			var currentWorkflow = SetupTestResourceUnderReviewLists();
			var preWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "preWorkflow", description: "preTask1a", staffCode: "PR1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preWorkflow, description: "preTask1b", staffCode: "PR2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preWorkflow.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow, currentWorkflow);

			var preChildWorkflow1 = BMTestHelper.CreateWorkflowAndTask(Factory, "preChildWorkflow1", description: "childTask1a", staffCode: "CH1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preChildWorkflow1, description: "childTask1b", staffCode: "CH2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preChildWorkflow1.GetOrCreateLinkToParent(preWorkflow);
			BMTestHelper.AssertIsParent(preChildWorkflow1, preWorkflow);

			var preChildWorkflow11 = BMTestHelper.CreateWorkflowAndTask(Factory, "preChildWorkflow11", description: "childTask11a", staffCode: "CH3", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preChildWorkflow11, description: "childTask1b", staffCode: "CH4", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preChildWorkflow11.GetOrCreateLinkToParent(preWorkflow);
			BMTestHelper.AssertIsParent(preChildWorkflow11, preWorkflow);

			var qcbTask = currentWorkflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			AssertResourceUnderReviewLists(qcbTask, new string[] { "PR2", "CH2", "CH4" });
		}

		//prerequisiteWorkflow 
		//	|
		//childWorkflow
		//						- currentWorkflow
		//prerequisiteWorkflow 
		//	|
		//childWorkflow
		public void TestResourceUnderReviewLists_TwoPrerequisitesTwoChildren_ShouldListStaffWorkingOnTasksInPrerequisitesAndEachChild()
		{
			var currentWorkflow = SetupTestResourceUnderReviewLists();
			var preWorkflow1 = BMTestHelper.CreateWorkflowAndTask(Factory, "preWorkflow1", description: "preTask1a", staffCode: "PR1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preWorkflow1, description: "preTask1b", staffCode: "PR2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preWorkflow1.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow1, currentWorkflow);

			var preChildWorkflow1 = BMTestHelper.CreateWorkflowAndTask(Factory, "preChildWorkflow1", description: "childTask1a", staffCode: "CH1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preChildWorkflow1, description: "childTask1b", staffCode: "CH2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preChildWorkflow1.GetOrCreateLinkToParent(preWorkflow1);
			BMTestHelper.AssertIsParent(preChildWorkflow1, preWorkflow1);

			var preWorkflow2 = BMTestHelper.CreateWorkflowAndTask(Factory, "preWorkflow2", description: "preTask2a", staffCode: "PR3", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preWorkflow2, description: "preTask2b", staffCode: "PR4", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preWorkflow2.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow2, currentWorkflow);

			var preChildWorkflow2 = BMTestHelper.CreateWorkflowAndTask(Factory, "preChildWorkflow2", description: "childTask2a", staffCode: "CH3", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(preChildWorkflow1, description: "childTask2b", staffCode: "CH4", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			preChildWorkflow2.GetOrCreateLinkToParent(preWorkflow2);
			BMTestHelper.AssertIsParent(preChildWorkflow2, preWorkflow2);

			var qcbTask = currentWorkflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			AssertResourceUnderReviewLists(qcbTask, new string[] { "PR2", "PR4", "CH2", "CH4" });
		}

		//currentWorkflow
		//		|
		//childWorkflow
		//		|
		//childWorkflow
		public void TestResourceUnderReviewLists_NoPrerequisitesTwoChildren_ShouldListStaffOfChildren()
		{
			var currentWorkflow = SetupTestResourceUnderReviewLists();

			var childWorkflow1 = BMTestHelper.CreateWorkflowAndTask(Factory, "childWorkflow1", description: "childTask1a", staffCode: "CH1", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(childWorkflow1, description: "childTask1b", staffCode: "CH2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			childWorkflow1.GetOrCreateLinkToParent(currentWorkflow);
			BMTestHelper.AssertIsParent(childWorkflow1, currentWorkflow);

			var childWorkflow11 = BMTestHelper.CreateWorkflowAndTask(Factory, "childWorkflow11", description: "childTask11a", staffCode: "CH3", createStaffIfNotExist: true);
			BMTestHelper.CreateTask(childWorkflow11, description: "childTask11b", staffCode: "CH4", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			childWorkflow11.GetOrCreateLinkToParent(childWorkflow1);
			BMTestHelper.AssertIsParent(childWorkflow11, childWorkflow1);

			var qcbTask = currentWorkflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			AssertResourceUnderReviewLists(qcbTask, new string[] { "CH2", "CH4" });
		}

		#endregion

		#region Pre-fill Resource Under Review

		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_ShouldPickUserWithLongestDurationFromMultipleTasks()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "qcbTask", staffCode: "S00", sequence: 4, taskType: "QCB", createStaffIfNotExist: true);
			var qcbTask = workflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;

			var staff1Task1 = BMTestHelper.CreateTask(workflow, staffCode: "S01", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1, description: "Closed Task1 - User S01", createStaffIfNotExist: true) as ProcessTask;
			staff1Task1.P9_ActualDuration = TestDateAttribute.Date.AddHours(3);

			var staff2Task1 = BMTestHelper.CreateTask(workflow, staffCode: "S02", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, description: "Closed Task1 - User S02", createStaffIfNotExist: true) as ProcessTask;
			staff2Task1.P9_ActualDuration = TestDateAttribute.Date.AddHours(2);

			var staff2Task2 = BMTestHelper.CreateTask(workflow, staffCode: "S02", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, description: "Closed Task2 - User S02", createStaffIfNotExist: true) as ProcessTask;
			staff2Task2.P9_ActualDuration = TestDateAttribute.Date.AddHours(2);

			Assert("GIVEN combined tasks duration for staff2 > staff1", staff2Task1.P9_ActualDuration.AddHours(TaskDurationCalculator.GetHoursFromDuration(staff2Task2.P9_ActualDuration)) > staff1Task1.P9_ActualDuration);

			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;

			AssertEquals($"GIVEN combined tasks duration for staff2 > staff1, WHEN getting ResourceUnderReviewList THEN should return staff2",
				"S02",
				viewModel.ResourceUnderReviewNK);
		}

		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_ShouldNotPickCancelledTask()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "qcbTask", staffCode: "S01", sequence: 4, taskType: "QCB", createStaffIfNotExist: true);
			var qcbTask = workflow.Tasks.First(task => task.P9_Description == "qcbTask") as ProcessTask;
			qcbTask.P9_ActualDuration = TestDateAttribute.Date.AddHours(1);

			var cancelledTask = BMTestHelper.CreateTask(workflow, staffCode: "S02", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled, sequence: 1, description: "Cancelled Task", createStaffIfNotExist: true) as ProcessTask;
			cancelledTask.P9_ActualDuration = TestDateAttribute.Date.AddHours(20);

			var closedTask = BMTestHelper.CreateTask(workflow, staffCode: "S03", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, description: "Closed Task", createStaffIfNotExist: true) as ProcessTask;
			closedTask.P9_ActualDuration = TestDateAttribute.Date.AddHours(1);

			Assert("GIVEN cancelledTask duration > closedTask duration", cancelledTask.P9_ActualDuration > closedTask.P9_ActualDuration);

			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;

			AssertEquals($"GIVEN cancelledTask duration > closedTask duration, WHEN calculating prefill resource-under-review, it should not pick cancelled task - even it has longer duration than closed task",
				"S03",
				viewModel.ResourceUnderReviewNK);
		}

		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_SameWorkflow()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var currentWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "currentWorkflow", description: "qcbTask", staffCode: "R0D", sequence: 4, taskType: "QCB", createStaffIfNotExist: true);
			CreateTasksForPrefillResourceUnderReviewTest(currentWorkflow, taskCode: "R0");

			// Should not default to user RX9 in this task because it has higher sequence than qcbTask
			var closedTaskZ = BMTestHelper.CreateTask(currentWorkflow, staffCode: "R0Z", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 9, description: $"Task R0Z", createStaffIfNotExist: true) as ProcessTask;
			closedTaskZ.P9_ActualDuration = TestDateAttribute.Date.AddHours(10);

			var message = "Not R0A because the task is open, Not R0C because the duration is less than R0B";
			AssertPrefillResourceUnderReview(message, currentWorkflow, expectedPrefillResourceUnderReview: "R0B");
		}

		// preWorkflow1 -> currentWorkflow
		// preWorkflow2 -> currentWorkflow
		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_SimplePrerequisites()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var currentWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "currentWorkflow", description: "qcbTask", staffCode: "R0A", sequence: 1, taskType: "QCB", createStaffIfNotExist: true);

			var preWorkflow1 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "preWorkflow1");
			preWorkflow1.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow1, currentWorkflow);
			CreateTasksForPrefillResourceUnderReviewTest(preWorkflow1, taskCode: "R1");

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(10);

			var preWorkflow2 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "preWorkflow2");
			preWorkflow2.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow2, currentWorkflow);
			CreateTasksForPrefillResourceUnderReviewTest(preWorkflow2, taskCode: "R2");

			var message = "Not preWorkflow1 because it is older than preWorkflow2";
			AssertPrefillResourceUnderReview(message, currentWorkflow, expectedPrefillResourceUnderReview: "R2B");
		}

		//									preWorkflow2 -> preWorkflow1 -> currentWorkflow
		//									preWorkflow3 -> preWorkflow1
		//	preWorkflow4 -> preWorkflow3
		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_ComplexPrerequisites()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var currentWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "currentWorkflow", description: "qcbTask", staffCode: "R0A", sequence: 1, taskType: "QCB", createStaffIfNotExist: true);

			var preWorkflow1 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "preWorkflow1");
			preWorkflow1.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow1, currentWorkflow);

			var preWorkflow2 = BMTestHelper.CreateWorkflow(Factory, "preWorkflow2");
			preWorkflow2.GetOrCreateDependencyLink(preWorkflow1);
			BMTestHelper.AssertIsPrerequisite(preWorkflow2, preWorkflow1);
			CreateTasksForPrefillResourceUnderReviewTest(preWorkflow2, taskCode: "R2");

			var preWorkflow3 = BMTestHelper.CreateWorkflow(Factory, "preWorkflow3");
			preWorkflow3.GetOrCreateDependencyLink(preWorkflow1);
			BMTestHelper.AssertIsPrerequisite(preWorkflow3, preWorkflow1);
			CreateTasksForPrefillResourceUnderReviewTest(preWorkflow3, taskCode: "R3");

			var preWorkflow4 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "preWorkflow4");
			preWorkflow4.GetOrCreateDependencyLink(preWorkflow3);
			BMTestHelper.AssertIsPrerequisite(preWorkflow4, preWorkflow3);
			CreateTasksForPrefillResourceUnderReviewTest(preWorkflow4, taskCode: "R4");

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(10);

			preWorkflow1.FH_CompletionStatement = $"{preWorkflow1.FH_CompletionStatement} - updated";
			preWorkflow3.FH_CompletionStatement = $"{preWorkflow3.FH_CompletionStatement} - updated";
			preWorkflow4.FH_CompletionStatement = $"{preWorkflow4.FH_CompletionStatement} - updated";

			Factory.Save();

			var message = "Not preWorkflow1 because it has no tasks, Not preWorkflow4 because preWorkflow3 closer prereq, Not preWorkflow2 because its last update is older than preWorkflow3";
			AssertPrefillResourceUnderReview(message, currentWorkflow, expectedPrefillResourceUnderReview: "R3B");
		}

		//	parentWorkflow2
		//			|
		//	parentWorkflow1
		//			|
		//	currentWorkflw
		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_SimpleParents()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var currentWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "currentWorkflow", description: "qcbTask", staffCode: "R0A", sequence: 1, taskType: "QCB", createStaffIfNotExist: true);

			var parentWorkflow1 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "parentWorkflow1");
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow1);
			BMTestHelper.AssertIsParent(currentWorkflow, parentWorkflow1);
			CreateTasksForPrefillResourceUnderReviewTest(parentWorkflow1, taskCode: "R1");

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(10);

			var parentWorkflow2 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "parentWorkflow2");
			parentWorkflow1.GetOrCreateLinkToParent(parentWorkflow2);
			BMTestHelper.AssertIsParent(parentWorkflow1, parentWorkflow2);
			CreateTasksForPrefillResourceUnderReviewTest(parentWorkflow2, taskCode: "R2");

			var message = "parentWorkflow1 because has closer relationship than parentWorkflow2";
			AssertPrefillResourceUnderReview(message, currentWorkflow, expectedPrefillResourceUnderReview: "R1B");
		}

		//										parentWorkflow4
		//												|
		//	parentWorkflow2		parentWorkflow3
		//			|									|
		//	parentWorkflow1		parentWorkflow1
		//			|
		//	currentWorkflow
		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_ComplexParents()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var currentWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "currentWorkflow", description: "qcbTask", staffCode: "R0A", sequence: 1, taskType: "QCB", createStaffIfNotExist: true);

			var parentWorkflow1 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "parentWorkflow1");
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow1);
			BMTestHelper.AssertIsParent(currentWorkflow, parentWorkflow1);

			var parentWorkflow2 = BMTestHelper.CreateWorkflow(Factory, "parentWorkflow2");
			parentWorkflow1.GetOrCreateLinkToParent(parentWorkflow2);
			BMTestHelper.AssertIsParent(parentWorkflow1, parentWorkflow2);
			CreateTasksForPrefillResourceUnderReviewTest(parentWorkflow2, taskCode: "R2");

			var parentWorkflow3 = BMTestHelper.CreateWorkflow(Factory, "parentWorkflow3");
			parentWorkflow1.GetOrCreateLinkToParent(parentWorkflow3);
			BMTestHelper.AssertIsParent(parentWorkflow1, parentWorkflow3);
			CreateTasksForPrefillResourceUnderReviewTest(parentWorkflow3, taskCode: "R3");

			var parentWorkflow4 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "parentWorkflow4");
			parentWorkflow3.GetOrCreateLinkToParent(parentWorkflow4);
			BMTestHelper.AssertIsParent(parentWorkflow3, parentWorkflow4);
			CreateTasksForPrefillResourceUnderReviewTest(parentWorkflow4, taskCode: "R4");

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(10);

			parentWorkflow1.FH_CompletionStatement = $"{parentWorkflow1.FH_CompletionStatement} - updated";
			parentWorkflow3.FH_CompletionStatement = $"{parentWorkflow3.FH_CompletionStatement} - updated";
			parentWorkflow4.FH_CompletionStatement = $"{parentWorkflow4.FH_CompletionStatement} - updated";

			Factory.Save();

			var message = "Not parentWorkflow1 because it has no tasks, Not parentWorkflow4 because preWorkflow3 closer prereq, Not parentWorkflow2 because its last update is older than parentWorkflow3";
			AssertPrefillResourceUnderReview(message, currentWorkflow, expectedPrefillResourceUnderReview: "R3B");
		}

		//										parentWorkflow1
		//												|
		//	preWorkflow1 ->		currentWorkflow
		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_PrerequisitesAndParents()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var currentWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "currentWorkflow", description: "qcbTask", staffCode: "R0A", sequence: 1, taskType: "QCB", createStaffIfNotExist: true);

			var parentWorkflow1 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "parentWorkflow1");
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow1);
			BMTestHelper.AssertIsParent(currentWorkflow, parentWorkflow1);
			CreateTasksForPrefillResourceUnderReviewTest(parentWorkflow1, taskCode: "R1");

			var preWorkflow1 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "preWorkflow1");
			preWorkflow1.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow1, currentWorkflow);
			CreateTasksForPrefillResourceUnderReviewTest(preWorkflow1, taskCode: "R1");

			var message = "Not parent because prerequisite has priority";
			AssertPrefillResourceUnderReview(message, currentWorkflow, expectedPrefillResourceUnderReview: "R1B");
		}

		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_PrerequisitesAndCurrent()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var currentWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "currentWorkflow", description: "qcbTask", staffCode: "R0A", sequence: 10, taskType: "QCB", createStaffIfNotExist: true);
			CreateTasksForPrefillResourceUnderReviewTest(currentWorkflow, taskCode: "R0");

			var preWorkflow1 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "preWorkflow1");
			preWorkflow1.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow1, currentWorkflow);
			CreateTasksForPrefillResourceUnderReviewTest(preWorkflow1, taskCode: "R1");

			var message = "Not prerequisite because current workflow has priority";
			AssertPrefillResourceUnderReview(message, currentWorkflow, expectedPrefillResourceUnderReview: "R0B");
		}

		void CreateTasksForPrefillResourceUnderReviewTest(IProcessHeader workflow, string taskCode)
		{
			var openTaskA = BMTestHelper.CreateTask(workflow, staffCode: $"{taskCode}A", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: $"Task {taskCode}A", createStaffIfNotExist: true) as ProcessTask;
			openTaskA.P9_ActualDuration = TestDateAttribute.Date.AddHours(20);

			var closedTaskB = BMTestHelper.CreateTask(workflow, staffCode: $"{taskCode}B", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, description: $"Task {taskCode}B", createStaffIfNotExist: true) as ProcessTask;
			closedTaskB.P9_ActualDuration = TestDateAttribute.Date.AddHours(5);

			var closedTaskC = BMTestHelper.CreateTask(workflow, staffCode: $"{taskCode}C", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3, description: $"Task {taskCode}C", createStaffIfNotExist: true) as ProcessTask;
			closedTaskC.P9_ActualDuration = TestDateAttribute.Date.AddHours(1);
		}

		void AssertPrefillResourceUnderReview(string message, IProcessHeader workflow, string expectedPrefillResourceUnderReview)
		{
			var qcbTask = workflow.Tasks.First(task => task.P9_Description == "qcbTask");
			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;

			AssertEquals($"GIVEN IterationRequired THEN ResourceUnderReviewNK default value should be resource who has the largest total Actual Duration recorded on closed tasks with a lower sequence number in the same workflow as the containment barrier task: {message}",
				expectedPrefillResourceUnderReview,
				viewModel.ResourceUnderReviewNK);
		}

		[TestDate(2014, 8, 20)]
		public void TestPrefillResourceUnderReview_CircularDependency()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var currentWorkflow = BMTestHelper.CreateWorkflowAndTask(Factory, "currentWorkflow", description: "qcbTask", staffCode: "R0A", sequence: 1, taskType: "QCB", createStaffIfNotExist: true);

			var parentWorkflow1 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "parentWorkflow1");
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow1);
			BMTestHelper.AssertIsParent(currentWorkflow, parentWorkflow1);
			BMTestHelper.CreateTask(parentWorkflow1, staffCode: "R1A", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "parent Task", createStaffIfNotExist: true);

			var preWorkflow1 = BMTestHelper.CreateWorkflow(Factory, completionStatement: "preWorkflow1");
			preWorkflow1.GetOrCreateDependencyLink(currentWorkflow);
			BMTestHelper.AssertIsPrerequisite(preWorkflow1, currentWorkflow);
			BMTestHelper.CreateTask(preWorkflow1, staffCode: "R1B", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "pre Task", createStaffIfNotExist: true);

			parentWorkflow1.GetOrCreateDependencyLink(preWorkflow1);
			BMTestHelper.AssertIsPrerequisite(parentWorkflow1, preWorkflow1);

			var message = "Not parent because prerequisite has priority";
			AssertPrefillResourceUnderReview(message, currentWorkflow, expectedPrefillResourceUnderReview: ZString.Empty);

			AssertEquals("No error should be reported", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestConstructor_WhenResourceUnderReviewPkSpecified_ShouldNotOverwriteResourceUnderReviewWithAlgorithmResult_PrefillEnabled()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			AssertEquals("Prefill should be enabled by default.", true, WorkflowDataRegistry.Instance.PrefillResourceUnderReview.Value);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff2.GS_Code = "ST2";
			staff3.GS_Code = "ST3";

			var workflow = BMTestHelper.CreateWorkflow(Factory, "Workflow");
			var task1 = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST3", taskType: "QCB");

			task1.P9_ActualDuration = new TimeSpan(0, 1, 0, 0);
			task2.P9_ActualDuration = new TimeSpan(0, 0, 30, 0);

			var viewModel = GetViewModelAsUserInteractive(task3, ProcessTaskStatusCodeList.Codes.Closed, "ST2");

			AssertEquals("A pk was provided for the resource under review, so it shouldn't be overridden with the algorithm, which would have otherwise chosen ST1 because they did the most work.",
				"ST2", viewModel.ResourceUnderReviewNK);
		}

		public void TestConstructor_WhenResourceUnderReviewPkSpecified_ShouldNotOverwriteResourceUnderReviewWithAlgorithmResult_PrefillDisabled()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff2.GS_Code = "ST2";
			staff3.GS_Code = "ST3";

			var workflow = BMTestHelper.CreateWorkflow(Factory, "Workflow");
			var task1 = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST3", taskType: "QCB");

			task1.P9_ActualDuration = new TimeSpan(0, 1, 0, 0);
			task2.P9_ActualDuration = new TimeSpan(0, 0, 30, 0);

			var viewModel = GetViewModelAsUserInteractive(task3, ProcessTaskStatusCodeList.Codes.Closed, "ST2");

			AssertEquals("A pk was provided for the resource under review, so it shouldn't be left blank even though prefill is disabled in the registry.",
				"ST2", viewModel.ResourceUnderReviewNK);
		}

		public void TestPrefillResourceUnderReview_WhenDisabledInRegistry_AndNotSpecifiedInConstructor_ButNotUserInteractive_ShouldPrefillAnyway()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff2.GS_Code = "ST2";
			staff3.GS_Code = "ST3";

			var workflow = BMTestHelper.CreateWorkflow(Factory, "Workflow");
			var task1 = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST3", taskType: "QCB");

			task1.P9_ActualDuration = new TimeSpan(0, 1, 0, 0);
			task2.P9_ActualDuration = new TimeSpan(0, 0, 30, 0);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var viewModel = GetViewModel(task3, ProcessTaskStatusCodeList.Codes.Closed);
				AssertEquals("Prefill is disabled in the registry, but since IsUserInteractive is false we want to prefill the value anyway.", "ST1", viewModel.ResourceUnderReviewNK);
			}
		}

		public void TestConstructor_WhenOnlyOnePossibleResourceInList_AndPrefillResourceUnderReviewDisabled_AndNotSpecifiedInConstructor_ShouldPrefillWithListResource()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";

			var workflow = BMTestHelper.CreateWorkflow(Factory, "Workflow");

			BMTestHelper.CreateTask(workflow, "ST1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1);
			var qcbTask = BMTestHelper.CreateTask(workflow, "ST2", taskType: "QCB", sequence: 2);

			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals("There's only one resource in the User Under Review list", "ST1", viewModel.Lookups.ResourceUnderReviewList.Single().GS_Code);

			AssertEquals("Prefill algorithm is disabled in the registry, but since there's only one resource in the User Under Review list and no resource specified in the constructor, that resource should be prefilled.", "ST1", viewModel.ResourceUnderReviewNK);
		}

		public void TestConstructor_WhenOnlyOnePossibleResourceInList_AndPrefillResourceUnderReviewEnabled_ButNoDefaultResourceFound_ShouldNotPrefillWithListResource()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";

			var workflow = BMTestHelper.CreateWorkflow(Factory, "Workflow");

			var qcbTask = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST1", taskType: "QCB", sequence: 1);

			// A closed task with sequence after the qcb task is not considered by the algorithm, but still shows up in the list
			BMTestHelper.CreateTask(workflow, "ST2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2);

			AssertNullOrEmpty("The prefill algorithm shouldn't find a resource", BestResourceUnderReviewProvider.FindDefaultResourceUnderReview(qcbTask));

			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals("There's only one resource in the User Under Review list", "ST2", viewModel.Lookups.ResourceUnderReviewList.Single().GS_Code);

			AssertNullOrEmpty("There's only one resource in the User Under Review list, but Prefill algorithm is enabled and could not find a resource; there should be no resource prefilled.", viewModel.ResourceUnderReviewNK);
		}

		public void TestConstructor_WhenOnlyOnePossibleResourceInList_AndPrefillResourceUnderReviewDisabledInRegistry_ButNotUserInteractive_AndNoDefaultResourceFound_ShouldNotPrefillWithListResource()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";

			var workflow = BMTestHelper.CreateWorkflow(Factory, "Workflow");

			var qcbTask = (ProcessTask)BMTestHelper.CreateTask(workflow, "ST1", taskType: "QCB", sequence: 1);

			// A closed task with sequence after the qcb task is not considered by the algorithm, but still shows up in the list
			BMTestHelper.CreateTask(workflow, "ST2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2);

			AssertNullOrEmpty("The prefill algorithm shouldn't find a resource", BestResourceUnderReviewProvider.FindDefaultResourceUnderReview(qcbTask));

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

				AssertEquals("There's only one resource in the User Under Review list", "ST2", viewModel.Lookups.ResourceUnderReviewList.Single().GS_Code);

				AssertNullOrEmpty("There's only one resource in the User Under Review list and Prefill algorithm is disabled in the registry, IsUserInteractive is false and so we use the algorithm anyway but it can't find a resource; there should be no resource prefilled.", viewModel.ResourceUnderReviewNK);
			}
		}

		public void TestConstructor_WhenOnlyOnePossibleResourceInList_AndPrefillResourceUnderReviewDisabled_AndOnlyOnePossibleResourceInList_ButResourceSpecifiedInConstructor_ShouldNotOverwriteResource()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "ST3";

			var workflow = BMTestHelper.CreateWorkflow(Factory, "Workflow");

			BMTestHelper.CreateTask(workflow, "ST1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1);
			var qcbTask = BMTestHelper.CreateTask(workflow, "ST2", taskType: "QCB", sequence: 2);

			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed, "ST3");

			AssertEquals("There's only one resource in the User Under Review list", "ST1", viewModel.Lookups.ResourceUnderReviewList.Single().GS_Code);

			AssertEquals("Prefill algorithm is disabled in the registry, and there's only one resource in the User Under Review list, but there's a resource specified in the viewmodel constructor that should not be overwritten.", "ST3", viewModel.ResourceUnderReviewNK);
		}

		public void TestConstructor_WhenMoreThanOnePossibleResourceInList_AndPrefillResourceUnderReviewDisabled_AndNotSpecifiedInConstructor_ShouldNotPrefillWithListResource()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "ST3";

			var workflow = BMTestHelper.CreateWorkflow(Factory, "Workflow");

			BMTestHelper.CreateTask(workflow, "ST1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1);
			BMTestHelper.CreateTask(workflow, "ST2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2);
			var qcbTask = BMTestHelper.CreateTask(workflow, "ST3", taskType: "QCB", sequence: 3);

			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			AssertGreaterThan("There's more than one resource in the User Under Review list", viewModel.Lookups.ResourceUnderReviewList.Count, 1);

			AssertNullOrEmpty("Prefill algorithm is disabled in the registry, and no resource specified in the constructor. There's more than one resource in the User Under Review list, so it should not be prefilled", viewModel.ResourceUnderReviewNK);
		}
		#endregion

		#region Registry

		public void TestPrefillResourceUnderReviewRegistry()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "task1", staffCode: "R01", sequence: 0, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			var task2 = BMTestHelper.CreateTask(workflow, staffCode: "R02", taskType: "QCB", sequence: 3, description: "task2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);

			AssertEquals("GIVEN 'PrefillResourceUnderReview' registry = true", true, WorkflowDataRegistry.Instance.PrefillResourceUnderReview.Value);
			var viewModel = GetViewModel(task2, ProcessTaskStatusCodeList.Codes.Closed);
			Assert("GIVEN PrefillResourceUnderReview=True THEN ResourceUnderReview should be prefilled", !viewModel.ResourceUnderReviewNK.IsEmpty);

			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("GIVEN 'PrefillResourceUnderReview' registry = false", false, WorkflowDataRegistry.Instance.PrefillResourceUnderReview.Value);
			viewModel = GetViewModel(task2, ProcessTaskStatusCodeList.Codes.Closed);
			AssertEquals("GIVEN PrefillResourceUnderReview=False THEN ResourceUnderReview should not be prefilled", ZString.Empty, viewModel.ResourceUnderReviewNK);
		}

		[TestDate(2022, 01, 04)]
		public void TestCreateIterationInNewWorkflow_ShouldBeChecked_ForContainmentBarrierIterationTypeNWF()
		{
			// Arrange
			BMTestHelper.EnableBMSInRegistry();
			CreateQCBTaskWithIterationType("QCB", "ORG", ContainmentBarrierIterationTypeList.Codes.NWF);

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "qcbTask", staffCode: "S00", sequence: 4, taskType: "QCB", createStaffIfNotExist: true);
			var qcbTask = workflow.Tasks.First(task => task.P9_Description == "qcbTask");

			// Act
			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			// Assert
			AssertEquals("Should Tick the Create Iterations In New Workflow Checkbox if Containment Barrier Iteration Type is set to NWF in the Registry", true, viewModel.ShouldCreateWorkflowForIteration);
		}

		[TestDate(2022, 01, 04)]
		public void TestCreateIterationInNewWorkflow_ShouldBeUnchecked_ForContainmentBarrierIterationTypeCWF()
		{
			BMTestHelper.EnableBMSInRegistry();
			CreateQCBTaskWithIterationType("QCB", "ORG", ContainmentBarrierIterationTypeList.Codes.CWF);

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "qcbTask", staffCode: "S00", sequence: 4, taskType: "QCB", createStaffIfNotExist: true);
			var qcbTask = workflow.Tasks.First(task => task.P9_Description == "qcbTask");

			// Act
			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			// Assert
			AssertEquals("Should not tick the Create Iterations In New Workflow Checkbox if Containment Barrier Iteration Type is set to CWF in the Registry", false, viewModel.ShouldCreateWorkflowForIteration);
		}

		[TestDate(2022, 01, 04)]
		public void TestCreateIterationInNewWorkflow_ShouldRespectGlobalRegistrySettings_ForContainmentBarrierIterationTypeGLB()
		{
			BMTestHelper.EnableBMSInRegistry();
			CreateQCBTaskWithIterationType("QCB", "ORG", ContainmentBarrierIterationTypeList.Codes.GLB);

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "qcbTask", staffCode: "S00", sequence: 4, taskType: "QCB", createStaffIfNotExist: true);
			var qcbTask = workflow.Tasks.First(task => task.P9_Description == "qcbTask");
			var registryPreference = WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.Value;

			// Act
			var viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			// Assert
			AssertEquals("Should Respect Global Registry Settings if Containment Barrier Iteration Type is set to NWF in the Registry", registryPreference, viewModel.ShouldCreateWorkflowForIteration);

			// Act - Change the registry value to whatever it was not before
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, !registryPreference);

			// Assert
			viewModel = GetViewModelAsUserInteractive(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);
			AssertEquals("Should Respect Global Registry Settings if Containment Barrier Iteration Type is set to NWF in the Registry", !registryPreference, viewModel.ShouldCreateWorkflowForIteration);
		}

		void CreateQCBTaskWithIterationType(string taskType, string workflowType, ZString containmentBarrierIterationType)
		{
			SetAsQCBTaskType(taskType, workflowType);
			var workflowTaskType = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskType(workflowType, taskType);
			workflowTaskType.ContainmentBarrierIterationType = containmentBarrierIterationType;
		}

		#endregion

		#region Process Task Iteration Link

		[TestDate(2018, 12, 12)]
		public void TestProcessTaskIterationLink_GivenContainmentBarrierForm_WhenIterated_ThenProcessTaskIterationLinkShouldBeCreated()
		{
			var result = SetupTestProcessTaskIterationLink(ContainmentBarrierResponses.IterationRequired);
			var viewModel = result.Item1;
			var workflow = result.Item2;
			var qcbTask = workflow.Tasks.First(task => task.P9_Description == "qcbTask");

			viewModel.IterateFromTaskPK = workflow.Tasks.First(task => task.P9_Description == "task1").PK;
			viewModel.CommitResponse();

			var pivots = qcbTask.GetContainmentBarrierIterationLinks();

			AssertEquals("iteration link should be created on qcbTask", 2, pivots.Count);

			var qualityIterationWorkflow = Factory.Load<IProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_CompletionStatement, "workflow (Quality Iteration 1)"))[0];

			AssertEquals(IterationLinkTypeList.Codes.QualityIterationTask, pivots[0].P9I_LinkType);
			AssertEquals(IterationLinkOutcomeList.Codes.IterationRequired, pivots[0].P9I_Outcome);
			AssertEquals("R01", pivots[0].P9I_GS_NKResourceUnderReview);
			AssertEquals(workflow.Tasks.First(task => task.P9_Description == "task1").PK, pivots[0].P9I_P9_IterationTask);
			AssertEquals(qualityIterationWorkflow.PK, pivots[0].P9I_FH_IterationWorkflow);

			AssertEquals(IterationLinkTypeList.Codes.PassedContainmentBarrier, pivots[1].P9I_LinkType);
			AssertEquals(IterationLinkOutcomeList.Codes.IterationRequired, pivots[1].P9I_Outcome);
			AssertEquals("R01", pivots[1].P9I_GS_NKResourceUnderReview);
			AssertEquals(workflow.Tasks.First(task => task.P9_Description == "qcbTaskForDeferring").PK, pivots[1].P9I_P9_IterationTask);
			AssertEquals(qualityIterationWorkflow.PK, pivots[1].P9I_FH_IterationWorkflow);
		}

		[TestDate(2014, 8, 20)]
		public void TestProcessTaskIterationLink_GivenContainmentBarrierForm_WhenDefered_ThenProcessTaskIterationLinkShouldBeCreated()
		{
			var result = SetupTestProcessTaskIterationLink(ContainmentBarrierResponses.DeferredToAnotherResource);
			var viewModel = result.Item1;
			var workflow = result.Item2;
			var qcbTask = workflow.Tasks.First(task => task.P9_Description == "qcbTask");

			viewModel.CommitResponse();

			var pivots = qcbTask.GetContainmentBarrierIterationLinks();
			AssertEquals("iteration link should be created on qcbTask", 1, pivots.Count);
			AssertEquals(IterationLinkOutcomeList.Codes.Deferred, pivots[0].P9I_Outcome);
			AssertEquals("R01", pivots[0].P9I_GS_NKResourceUnderReview);
			AssertEquals(ZGuid.Empty, pivots[0].P9I_P9_IterationTask);
			AssertEquals(ZGuid.Empty, pivots[0].P9I_FH_IterationWorkflow);
			AssertEquals(IterationLinkTypeList.Codes.QualityIterationTask, pivots[0].P9I_LinkType);
			AssertEquals(ZString.Empty, pivots[0].P9I_IterationReason);
		}

		[TestDate(2014, 8, 20)]
		public void TestProcessTaskIterationLink_GivenContainmentBarrierForm_WhenPassed_ThenProcessTaskIterationLinkShouldBeCreated()
		{
			var result = SetupTestProcessTaskIterationLink(ContainmentBarrierResponses.Passed);
			var viewModel = result.Item1;
			var workflow = result.Item2;
			var qcbTask = workflow.Tasks.First(task => task.P9_Description == "qcbTask");

			viewModel.CommitResponse();

			var pivots = qcbTask.GetContainmentBarrierIterationLinks();
			AssertEquals("iteration link should be created on qcbTask", 1, pivots.Count);
			AssertEquals(IterationLinkOutcomeList.Codes.Passed, pivots[0].P9I_Outcome);
			AssertEquals("R01", pivots[0].P9I_GS_NKResourceUnderReview);
			AssertEquals(ZGuid.Empty, pivots[0].P9I_P9_IterationTask);
			AssertEquals(ZGuid.Empty, pivots[0].P9I_FH_IterationWorkflow);
			AssertEquals(IterationLinkTypeList.Codes.QualityIterationTask, pivots[0].P9I_LinkType);
			AssertEquals(ZString.Empty, pivots[0].P9I_IterationReason);
		}

		Tuple<ContainmentBarrierViewModel, IProcessHeader> SetupTestProcessTaskIterationLink(ContainmentBarrierResponses containmentBarrierResponses)
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "task1", staffCode: "R01", sequence: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			var task1 = workflow.Tasks.First() as ProcessTask;
			if (!TestDateAttribute.IsActive)
			{
				throw new InvalidOperationException("This test method can not be used if a TestDate has not been assigned");
			}
			task1.P9_ActualDuration = TestDateAttribute.Date.AddHours(5);
			var qcbTask = BMTestHelper.CreateTask(workflow, staffCode: "R02", taskType: "QCB", sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Open, description: "qcbTask", createStaffIfNotExist: true) as ProcessTask;
			var qcbTaskForDeferring = BMTestHelper.CreateTask(workflow, staffCode: "R03", taskType: "QCB", sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Open, description: "qcbTaskForDeferring", createStaffIfNotExist: true) as ProcessTask;

			var pivots = qcbTask.GetContainmentBarrierIterationLinks();
			AssertEquals("Precondition qcbTask has no iteration links", 0, pivots.Count);

			var viewModel = GetViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed);

			viewModel.Response = containmentBarrierResponses;

			return new Tuple<ContainmentBarrierViewModel, IProcessHeader>(viewModel, workflow);
		}

		#endregion

		#region Iteration Reasons

		public void TestDefaultQualityIterationReason_OnlyOneReason()
		{
			ClearIterationReasonsFromRegistry("WKI");
			var viewModel = CreateContainmentBarrierViewModel("WKI");

			AssertEquals(1, viewModel.Lookups.IterationReasonsRegistryLists.Count);
			AssertEquals("UDF", viewModel.Lookups.IterationReasonsRegistryLists[0].Code);
			AssertEquals("Undefined - You can modify this in the System Registry, under Workflow Manager/Quality Iteration Reasons", viewModel.Lookups.IterationReasonsRegistryLists[0].Description);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;

			var errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals(0, errorMessages.Length);
			AssertEquals(viewModel.Lookups.IterationReasonsRegistryLists[0].PK, viewModel.IterateReasonPK);
		}

		public void TestDefaultQualityIterationReason_MoreReasons()
		{
			string jobType = "WKI";

			AddIterationReasonToRegistry("WKI", "RS2", "Reason 2");

			var viewModel = CreateContainmentBarrierViewModel(jobType);

			AssertEquals(2, viewModel.Lookups.IterationReasonsRegistryLists.Count);
			AssertEquals("RS1", viewModel.Lookups.IterationReasonsRegistryLists[0].Code);
			AssertEquals("Reason 1", viewModel.Lookups.IterationReasonsRegistryLists[0].Description);
			AssertEquals("RS2", viewModel.Lookups.IterationReasonsRegistryLists[1].Code);
			AssertEquals("Reason 2", viewModel.Lookups.IterationReasonsRegistryLists[1].Description);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;

			var errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals(1, errorMessages.Length);
			AssertEquals("Error - IterateReasonPK: Please enter a Reason.", errorMessages[0]);
			AssertEquals(ZGuid.Empty, viewModel.IterateReasonPK);
		}

		#endregion

		#region Edge-Case Handling

		public void TestCreateIterationOnTasksThatDoesNotBelongToCurrentCompany()
		{
			SetAsQCBTaskType("QCB", "WKI");

			var company = Factory.New<GlbCompany>();
			company.GC_Name = "Sure Locks";

			var ben = Factory.NewWithValidTestData<GlbStaff>();
			ben.GS_FullName = "Kinepict Bumbersnitch";
			var martin = Factory.NewWithValidTestData<GlbStaff>();
			martin.GS_FullName = "Martin Mertens";

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");

			BMTestHelper.CreateTaskWithCompany(workflow, martin.GS_Code, 10, sequence: 1, description: "Before anything", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "UDF", company: company);
			var task = BMTestHelper.CreateTaskWithCompany(workflow, ben.GS_Code, 10, sequence: 2, description: "Everything", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB", company: company);

			var viewModel = GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;

			AssertEquals(2, ((IWorkflowProvider)jobHeader.Parent).WorkflowItems.Count);
			AssertEquals("Tasks should be included for quality iteration", 2, viewModel.IterationTaskPreviews.Count);
		}

		public void TestConstructor_WhenTaskTypeNotContainmentBarrier_ExceptionShouldBeUseful()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_Type = "DÄV";

			Factory.Save();

			var ex = AssertExceptionThrown<ArgumentException>(() => GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed));

			AssertEquals($"Task must be a quality containment barrier. {task.HumanReadableName} (Type: DÄV, Parent Table Code: , Parent ID: 00000000-0000-0000-0000-000000000000, PK: {task.PK})", ex.Message);
		}

		#endregion

		#region Task Sequence Bumping

		public void TestCreateIteration_TasksUpParentageHierarchyShouldAlsoGetBumped()
		{
			SetAsQCBTaskType("QCB", "WKI");

			var ben = Factory.NewWithValidTestData<GlbStaff>();
			ben.GS_FullName = "Benedict Cumberbatch";
			var martin = Factory.NewWithValidTestData<GlbStaff>();
			martin.GS_FullName = "Martin Freeman";

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMTestHelper.CreateWorkflow(jobHeader, "workflow2");

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = BMTestHelper.CreateTask(workflow1, martin.GS_Code, 10, sequence: 1, description: "Before anything", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMTestHelper.CreateTask(workflow1, ben.GS_Code, 10, sequence: 2, description: "Everything", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			var task3 = BMTestHelper.CreateTask(workflow1, martin.GS_Code, 10, sequence: 3, description: "After everything");
			var unrelatedTask = BMTestHelper.CreateTask(workflow2, martin.GS_Code, 10, sequence: 5, description: "Some other time");

			// Create a Quality Iteration
			var viewModel1 = GetViewModel(task2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel1.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel1.IterateFromTaskPK = task1.PK;
			viewModel1.CommitResponse();

			AssertEquals(3, jobHeader.ProcessHeaders.Count);
			var iterationWorkflow1 = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 1)");
			AssertEquals(2, iterationWorkflow1.Tasks.Count());

			AssertEquals("'After everything' task should be bumped past the QI workflow tasks", 5, task3.P9_Sequence);
			AssertEquals("'Some other time' task should not be affected", 5, unrelatedTask.P9_Sequence);

			// Create a second Quality Iteration
			var iterationTask1_1 = iterationWorkflow1.Tasks.ElementAt(0);
			var iterationTask1_2 = iterationWorkflow1.Tasks.ElementAt(1);

			iterationTask1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var viewModel2 = GetViewModel(iterationTask1_2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel2.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel2.IterateFromTaskPK = iterationTask1_1.PK;
			viewModel2.CommitResponse();

			AssertEquals(4, jobHeader.ProcessHeaders.Count);
			var iterationWorkflow2 = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 2)");
			AssertEquals(2, iterationWorkflow2.Tasks.Count());

			AssertEquals("'After everything' task should be bumped past the nested QI workflow", 7, task3.P9_Sequence);
			AssertEquals("'Some other time' task should not be affected", 5, unrelatedTask.P9_Sequence);

			// Create a third Quality Iteration
			var iterationTask2_1 = iterationWorkflow2.Tasks.ElementAt(0);
			var iterationTask2_2 = iterationWorkflow2.Tasks.ElementAt(1);

			iterationTask2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var viewModel3 = GetViewModel(iterationTask2_2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel3.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel3.IterateFromTaskPK = iterationTask2_1.PK;
			viewModel3.CommitResponse();

			AssertEquals(5, jobHeader.ProcessHeaders.Count);
			AssertEquals("'After everything' task should be bumped past the nested QI workflow", 9, task3.P9_Sequence);
			AssertEquals("'Some other time' task should not be affected", 5, unrelatedTask.P9_Sequence);
		}

		public void TestCreateIteration_WhenPreviousIterationHasInexplicablyBeenMovedIntoTheCurrentOne_ShouldBumpSequenceOfFollowingTasksWithoutLoopingInfinitely()
		{
			SetAsQCBTaskType("QCB", "WKI");

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "Darkest Timeline");

			var task1 = BMTestHelper.CreateTask(workflow1, resource1.GS_Code, 10, sequence: 1, description: "Before everything", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMTestHelper.CreateTask(workflow1, resource2.GS_Code, 10, sequence: 2, description: "Everything", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			var finalTask = BMTestHelper.CreateTask(workflow1, resource1.GS_Code, 10, sequence: 3, description: "After everything");

			CreateQualityIteration(task2, task1);

			var workflow2 = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "Darkest Timeline (Quality Iteration 1)");
			var task3 = workflow2.Tasks.Single(t => t.P9_Description == "Before everything");
			var task4 = workflow2.Tasks.Single(t => t.P9_Description == "Everything");

			AssertEquals("Quality iteration should have pushed sequence number of subsequent task in parent workflow", 5, finalTask.P9_Sequence);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			CreateQualityIteration(task4, task3);

			var workflow3 = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "Darkest Timeline (Quality Iteration 2)");
			var task5 = workflow3.Tasks.Single(t => t.P9_Description == "Before everything");
			var task6 = workflow3.Tasks.Single(t => t.P9_Description == "Everything");

			AssertEquals("Quality iteration should have pushed sequence number of subsequent task in parent workflow, again", 7, finalTask.P9_Sequence);

			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task6.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			// Screw around with tasks.
			task3.P9_FH_ProcessHeader = workflow3.PK;
			task4.P9_FH_ProcessHeader = workflow3.PK;
			task4.P9_Sequence = 8;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			AssertNoExceptionThrown(() => CreateQualityIteration(task6, task5));

			var workflow4 = jobHeader.ProcessHeaders.Cast<IProcessHeader>().SingleOrDefault(w => w.FH_CompletionStatement == "Darkest Timeline (Quality Iteration 3)");

			AssertNotNull(workflow4);
			AssertEquals(2, workflow4.Tasks.Count());
		}

		#endregion

		#region Startability

		public void TestCreateIteration_TasksFollowingQCBShouldNotBeCurrentUntilQIComplete()
		{
			var ben = Factory.NewWithValidTestData<GlbStaff>();
			ben.GS_FullName = "Benedict Cumberbatch";
			var martin = Factory.NewWithValidTestData<GlbStaff>();
			martin.GS_FullName = "Martin Freeman";

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var mainWorkflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");

			var task1 = BMTestHelper.CreateTask(mainWorkflow, martin.GS_Code, 10, sequence: 10, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMTestHelper.CreateTask(mainWorkflow, martin.GS_Code, 10, sequence: 20, description: "task2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			var task3 = BMTestHelper.CreateTask(mainWorkflow, ben.GS_Code, 10, sequence: 30, description: "task3");
			var task4 = BMTestHelper.CreateTask(mainWorkflow, ben.GS_Code, 10, sequence: 40, description: "task4");

			SetAsQCBTaskType("QCB", "WKI");

			var viewModel1 = GetViewModel(task2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel1.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel1.IterateFromTaskPK = task1.PK;
			viewModel1.CommitResponse();

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			var iterationWorkflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow (Quality Iteration 1)");
			AssertEquals(2, iterationWorkflow.Tasks.Count());

			var iterationTask1 = iterationWorkflow.Tasks.ElementAt(0);
			var iterationTask2 = iterationWorkflow.Tasks.ElementAt(1);

			AssertEquals(true, iterationWorkflow.IsCurrent(iterationTask1));
			AssertEquals(false, iterationWorkflow.IsCurrent(iterationTask2));
			AssertEquals(false, mainWorkflow.IsCurrent(task3));
			AssertEquals(false, mainWorkflow.IsCurrent(task4));

			iterationTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(false, iterationWorkflow.IsCurrent(iterationTask1));
			AssertEquals(true, iterationWorkflow.IsCurrent(iterationTask2));
			AssertEquals(false, mainWorkflow.IsCurrent(task3));
			AssertEquals(false, mainWorkflow.IsCurrent(task4));

			iterationTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(false, iterationWorkflow.IsCurrent(iterationTask2));
			AssertEquals("Quality iteration has been closed, so current task falls back to next task in parent workflow", true, mainWorkflow.IsCurrent(task3));
			AssertEquals(false, mainWorkflow.IsCurrent(task4));

			var viewModel2 = GetViewModel(iterationTask2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel2.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel2.IterateFromTaskPK = iterationTask1.PK;
			viewModel2.CommitResponse();
			Factory.Save();

			var sectionIterationWorkflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "workflow (Quality Iteration 2)");
			AssertEquals(2, sectionIterationWorkflow.Tasks.Count());

			var iteration2Task1 = sectionIterationWorkflow.Tasks.ElementAt(0);
			var iteration2Task2 = sectionIterationWorkflow.Tasks.ElementAt(1);

			AssertEquals(true, sectionIterationWorkflow.IsCurrent(iteration2Task1));
			AssertEquals(false, sectionIterationWorkflow.IsCurrent(iteration2Task2));
			AssertEquals(false, mainWorkflow.IsCurrent(task3));
			AssertEquals(false, mainWorkflow.IsCurrent(task4));

			iteration2Task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(false, sectionIterationWorkflow.IsCurrent(iteration2Task1));
			AssertEquals(true, sectionIterationWorkflow.IsCurrent(iteration2Task2));
			AssertEquals(false, mainWorkflow.IsCurrent(task3));
			AssertEquals(false, mainWorkflow.IsCurrent(task4));

			iteration2Task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(false, sectionIterationWorkflow.IsCurrent(iteration2Task2));
			AssertEquals("All quality iterations have been closed, so current task falls back to next task in parent workflow", true, mainWorkflow.IsCurrent(task3));
			AssertEquals(false, mainWorkflow.IsCurrent(task4));
		}

		#endregion

		#region Task Filtering

		public void TestSelectWorkflow_ShouldFilterQcbTasks_ShouldNotFilterPreviewTasks()
		{
			SetAsQCBTaskType("QCB", "WKI");

			var ben = Factory.NewWithValidTestData<GlbStaff>();
			ben.GS_FullName = "Benedict Cumberbatch";

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow 1");
			var workflow2 = BMTestHelper.CreateWorkflow(jobHeader, "workflow 2");
			var workflow3 = BMTestHelper.CreateWorkflow(jobHeader, "workflow 3");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			var task1_1 = BMTestHelper.CreateTask(workflow1, ben.GS_Code, 10, sequence: 1, description: "task 1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task1_2 = BMTestHelper.CreateTask(workflow1, ben.GS_Code, 10, sequence: 2, description: "task 2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2_1 = BMTestHelper.CreateTask(workflow2, ben.GS_Code, 10, sequence: 3, description: "task 1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2_2 = BMTestHelper.CreateTask(workflow2, ben.GS_Code, 10, sequence: 4, description: "task 2", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, taskType: "QCB");
			var task3_1 = BMTestHelper.CreateTask(workflow3, ben.GS_Code, 10, sequence: 4, description: "task 1", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			var viewModel = GetViewModel(task2_2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.IterateFromTaskPK = task1_1.PK;

			AssertContainsExactElementsInAnyOrder("No workflow is selected so all tasks before the qcb task should be included to be selected as the iteration task, and yet...",
				new[] { "1  task 1  Work Item - workflow 1", "2  task 2  Work Item - workflow 1", "3  task 1  Work Item - workflow 2", }, viewModel.Lookups.ProcessTaskList.Cast<ProcessTaskFriendlyView>().Select(x => x.Description));
			AssertContainsExactElementsInAnyOrder("No workflow is selected so all tasks before the qcb task should be included in the preview tasks, and yet...",
				new[] { "workflow 1 task 1", "workflow 1 task 2", "workflow 2 task 1", "workflow 2 task 2" }, viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(x => x.WorkflowName + " " + x.TaskName));

			viewModel.IterateFromWorkflowPK = workflow1.PK;
			viewModel.IterateFromTaskPK = task1_1.PK; // This gets cleared when the workflow is selected so needs to be set again.

			AssertContainsExactElementsInAnyOrder("A workflow is selected so only tasks in that workflow should be included to be selected as the iteration task, and yet...",
				new[] { "1  task 1  Work Item - workflow 1", "2  task 2  Work Item - workflow 1" }, viewModel.Lookups.ProcessTaskList.Cast<ProcessTaskFriendlyView>().Select(x => x.Description));
			AssertContainsExactElementsInAnyOrder("A workflow is selected but all tasks before the qcb task should still be included in the preview tasks, and yet...",
				new[] { "workflow 1 task 1", "workflow 1 task 2", "workflow 2 task 1", "workflow 2 task 2" }, viewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>().Select(x => x.WorkflowName + " " + x.TaskName));
		}

		#endregion

		#region Implementation

		ContainmentBarrierViewModel CreateContainmentBarrierViewModel(string jobType)
		{
			SetAsQCBTaskType("QCB", jobType);

			var ben = Factory.NewWithValidTestData<GlbStaff>();
			ben.GS_FullName = "Benedict Cumberbatch";
			var martin = Factory.NewWithValidTestData<GlbStaff>();
			martin.GS_FullName = "Martin Freeman";

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMTestHelper.CreateWorkflow(jobHeader, "workflow2");

			workflow1.GetOrCreateDependencyLink(workflow2);

			BMTestHelper.CreateTask(workflow1, martin.GS_Code, 10, sequence: 1, description: "Before anything", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task = BMTestHelper.CreateTask(workflow1, ben.GS_Code, 10, sequence: 2, description: "Everything", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			// Create a Quality Iteration
			return GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed);
		}

		protected override void SetUp()
		{
			base.SetUp();

			EnableBufferManagement();
			ClearIterationReasonsFromRegistry("WKI");
			AddIterationReasonToRegistry("WKI", "RS1", "Reason 1");
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public ContainmentBarrierViewModel GetViewModel(IProcessTask task, string status, ZString? resourceUnderReviewNk = null)
		{
			var viewModel = new ContainmentBarrierViewModel(task, status, resourceUnderReviewNk, deselectCancelledTasksFromIteration: true);
			DisposableLeakListener.Instance.UnRegisterDisposable(viewModel);
			return viewModel;
		}

		ContainmentBarrierViewModel GetViewModelAsUserInteractive(IProcessTask task, string status, ZString? resourceUnderReviewNk = null)
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				return GetViewModel(task, status, resourceUnderReviewNk);
			}
		}

		#endregion
	}

	#region NonPersistentBusinessObjectTestCase

	[TestedType(typeof(ContainmentBarrierViewModel))]
	class ContainmentBarrierViewModelNonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			WorkflowTestCase.SetAsQCBTaskType("QCB", "WKI");
			var task = WorkflowTestCase.BMTestHelper.CreateTask((BusinessObject)Factory.New<IWorkItem>(), taskType: "QCB");

			return GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed);
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

		protected override void SetUp()
		{
			base.SetUp();

			WorkflowTestCase.EnableBufferManagement();
		}

		protected override void TearDown()
		{
			disposables?.Dispose();
			disposables = null;
			base.TearDown();
		}

		DisposableList disposables;
	}

	#endregion
}
