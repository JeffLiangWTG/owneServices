using System;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[GuiTest]
	class ContainmentBarrierViewModelValidationTest : WorkflowTestCase
	{
		#region Resource Under Review Validation

		public void TestResourceShouldNotReviewOwnWork()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "task1", staffCode: "R01", sequence: 10, createStaffIfNotExist: true, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task1 = workflow.Tasks.First();

			var task2 = BMTestHelper.CreateTask(workflow, staffCode: "R01", taskType: "QCB", sequence: 3, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			var viewModel = GetViewModel(task2, ProcessTaskStatusCodeList.Codes.Closed);

			viewModel.ResourceUnderReviewNK = "R01";
			var warningMessages = viewModel.GetWarnings().GetUniqueMessageList();
			CombineAssertions("WHEN resource review own work THEN show warning", () =>
			{
				AssertEquals(1, warningMessages.Length);
				AssertEquals("Warning - ResourceUnderReviewNK: Resource should not review own work.", warningMessages?.FirstOrDefault());
			});
		}

		[TestDate(2014, 8, 20)]
		public void TestGivenInvalidResourceUnderReview_WhenCancel_ShouldNotError()
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "task1", staffCode: "R01", sequence: 10, createStaffIfNotExist: true);
			var task1 = workflow.Tasks.First();
			((ProcessTask)task1).P9_ActualDuration = TestDateAttribute.Date.AddHours(5);

			var task2 = BMTestHelper.CreateTask(workflow, staffCode: "R02", taskType: "QCB", sequence: 3, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			var viewModel = GetViewModel(task2, ProcessTaskStatusCodeList.Codes.Closed);

			AssertEquals("User not setting 'User Under Review'", ZString.Empty, viewModel.ResourceUnderReviewNK);
			viewModel.Response = ContainmentBarrierResponses.Canceled;
			viewModel.CommitResponse();

			var errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals("GIVEN user not setting 'User Under Review' WHEN cancel THEN should not error", 0, errorMessages.Length);

			viewModel.ResourceUnderReviewNK = "???";
			viewModel.Response = ContainmentBarrierResponses.Canceled;
			errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals("GIVEN user setting invalid 'User Under Review' WHEN cancel THEN should not error", 0, errorMessages.Length);
		}

		[TestDate(2014, 8, 20)]
		public void TestResourceUnderReviewValidation_RequireResourceUnderReviewRegistryIsTrue()
		{
			var viewModel = SetupResourceUnderReviewValidation(requireResourceUnderReviewRegistry: true);

			AssertEquals("GIVEN setting'Require User Under Review' registry = true", true, WorkflowDataRegistry.Instance.RequireResourceUnderReview.Value);
			viewModel.ResourceUnderReviewNK = ZString.Empty;
			AssertEquals("GIVEN 'Resource Under review' is not set", ZString.Empty, viewModel.ResourceUnderReviewNK);
			var errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals(1, errorMessages.Length);
			AssertEquals("Error - ResourceUnderReviewNK: Please enter an User whose work is under review.", errorMessages?.FirstOrDefault());
		}

		[TestDate(2014, 8, 20)]
		public void TestResourceUnderReviewValidation_RequireResourceUnderReviewRegistryIsFalse()
		{
			var viewModel = SetupResourceUnderReviewValidation(requireResourceUnderReviewRegistry: false);

			AssertEquals("GIVEN setting'Require User Under Review' registry = false", false, WorkflowDataRegistry.Instance.RequireResourceUnderReview.Value);
			viewModel.ResourceUnderReviewNK = ZString.Empty;
			AssertEquals("GIVEN not setting 'Resource Under review'", ZString.Empty, viewModel.ResourceUnderReviewNK);
			var errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals(0, errorMessages.Length);
		}

		ContainmentBarrierViewModel SetupResourceUnderReviewValidation(bool requireResourceUnderReviewRegistry)
		{
			BMTestHelper.EnableBMSInRegistry();
			SetAsQCBTaskType("QCB", "ORG");

			WorkflowDataRegistry.Instance.RequireResourceUnderReview.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, requireResourceUnderReviewRegistry);

			var workflow = BMTestHelper.CreateWorkflowAndTask(Factory, "workflow", description: "task1", staffCode: "R01", sequence: 1, createStaffIfNotExist: true);
			var task1 = workflow.Tasks.First();
			((ProcessTask)task1).P9_ActualDuration = TestDateAttribute.Date.AddHours(5);

			var task2 = BMTestHelper.CreateTask(workflow, staffCode: "R02", taskType: "QCB", sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, createStaffIfNotExist: true);
			var viewModel = GetViewModel(task2, ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			viewModel.IterateFromTaskPK = task1.PK;

			var staffR01 = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "R01");
			viewModel.ResourceUnderReviewNK = "R01";
			AssertNull("GIVEN 'User Under Review' is not in the list", viewModel.Lookups.ResourceUnderReviewList.FirstOrDefault(staff => staff.PK == staffR01.PK));
			var errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals(1, errorMessages.Length);
			AssertEquals("Error - ResourceUnderReviewNK: Enter a valid User whose work is under review.", errorMessages?.FirstOrDefault());

			var staffR02 = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "R02");
			viewModel.ResourceUnderReviewNK = "R02";
			AssertNotNull("GIVEN 'User Under Review' is in the list", viewModel.Lookups.ResourceUnderReviewList.FirstOrDefault(staff => staff.PK == staffR02.PK));
			errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals(0, errorMessages.Length);

			return viewModel;
		}

		#endregion

		public void TestIterateFromTaskPK()
		{
			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMTestHelper.CreateTask(workflow, taskType: "QCB");
			task.P9_Sequence = 20;
			task.P9_Description = "QCB task";

			var viewModel = GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed);

			viewModel.Validation.ValidateAll();
			AssertNoErrors(viewModel.IterateFromTaskPKInfo);
			AssertEquals(true, viewModel.IterateFromTaskPKInfo.ReadOnly);

			viewModel.Response = ContainmentBarrierResponses.Canceled;
			AssertNoErrors(viewModel.IterateFromTaskPKInfo);
			AssertEquals(true, viewModel.IterateFromTaskPKInfo.ReadOnly);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;
			AssertHasError(viewModel.IterateFromTaskPKInfo, "Please enter a Task.");
			AssertEquals(false, viewModel.IterateFromTaskPKInfo.ReadOnly);

			var afterQCBTask = BMTestHelper.CreateTask(workflow);
			afterQCBTask.P9_Sequence = 30;
			afterQCBTask.P9_Description = "afterQCBTask";

			viewModel.IterateFromTaskPK = afterQCBTask.PK;
			AssertHasError(viewModel.IterateFromTaskPKInfo, "Please select a valid Iterate From Task.");

			var iterateFromTask = BMTestHelper.CreateTask(workflow);
			iterateFromTask.P9_Sequence = 10;
			iterateFromTask.P9_Description = "iterateFromTask";

			viewModel.IterateFromTaskPK = iterateFromTask.PK;
			AssertNoErrors(viewModel.IterateFromTaskPKInfo);

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var taskList = viewModel.JobTasksWithWorkflowFiltering as IterateFromProcessTaskCollectionView;
			taskList.SelectedWorkflow = workflow1;
			viewModel.IterateFromWorkflowPK = workflow1.PK;
			AssertHasError(viewModel.IterateFromWorkflowPKInfo, "Please select a valid Iterate From Workflow.");
		}

		public void TestIterateReasonValidationNoWarning()
		{
			AssertIterateReasonValidation(IterationReasonValidationList.Codes.None, 0, 0);
		}

		public void TestIterateReasonValidationWarning()
		{
			AssertIterateReasonValidation(IterationReasonValidationList.Codes.Warning, 1, 0);
		}

		public void TestIterateReasonValidationError()
		{
			AssertIterateReasonValidation(IterationReasonValidationList.Codes.Error, 0, 1);
		}

		void AssertIterateReasonValidation(ZString validation, int numOfWarnings, int numOfErrors)
		{
			var jobType = "WKI";

			var iterationReasonsColl = WorkflowDataRegistry.Instance.IterationReasons.Value;

			var iterationReason = iterationReasonsColl
				.OfType<CategorisedWorkflowIterationReasons>()
				.FirstOrDefault(x => x.Code == jobType);

			if (iterationReason == null)
			{
				iterationReason = iterationReasonsColl.AddNew();
				iterationReason.Code = jobType;
			}
			iterationReason.IterationReasons.RemoveAndDeleteAll();

			var iterationReason1 = iterationReason.IterationReasons.AddNew();
			iterationReason1.Code = "RS1";
			iterationReason1.Description = (NoResString)"Reason 1";

			var iterationReason2 = iterationReason.IterationReasons.AddNew();
			iterationReason2.Code = "RS2";
			iterationReason2.Description = (NoResString)"Reason 2";

			iterationReason.IterationReasonValidation = validation;

			WorkflowDataRegistry.Instance.IterationReasons.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, iterationReasonsColl);

			var viewModel = CreateContaineBarrierViewModel(jobType);

			AssertEquals(2, viewModel.Lookups.IterationReasonsRegistryLists.Count);
			AssertEquals("RS1", viewModel.Lookups.IterationReasonsRegistryLists[0].Code);
			AssertEquals("Reason 1", viewModel.Lookups.IterationReasonsRegistryLists[0].Description);
			AssertEquals("RS2", viewModel.Lookups.IterationReasonsRegistryLists[1].Code);
			AssertEquals("Reason 2", viewModel.Lookups.IterationReasonsRegistryLists[1].Description);

			viewModel.Response = ContainmentBarrierResponses.IterationRequired;

			// Empty Reason
			var errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals(ZGuid.Empty, viewModel.IterateReasonPK);
			AssertEquals(numOfErrors, errorMessages.Length);
			if (numOfErrors > 0)
			{
				AssertEquals(1, errorMessages.Length);
				AssertEquals("Error - IterateReasonPK: Please enter a Reason.", errorMessages[0]);
			}
			var warningMessages = viewModel.GetWarnings().GetUniqueMessageList();
			AssertEquals(numOfWarnings, warningMessages.Length);
			if (numOfWarnings > 0)
			{
				AssertEquals(1, warningMessages.Length);
				AssertEquals("Warning - IterateReasonPK: You have not entered a Reason.", warningMessages[0]);
			}

			// Invalid Reason
			viewModel.IterateReasonPK = ZGuid.NewZGuid();
			errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals(1, errorMessages.Length);
			AssertEquals("Error - IterateReasonPK: Please select a valid iteration reason.", errorMessages[0]);
			warningMessages = viewModel.GetWarnings().GetUniqueMessageList();
			AssertEquals(0, warningMessages.Length);

			// Valid Reason
			viewModel.IterateReasonPK = viewModel.Lookups.IterationReasonsRegistryLists[0].PK;
			errorMessages = viewModel.GetErrors().GetUniqueMessageList();
			AssertEquals(0, errorMessages.Length);
			warningMessages = viewModel.GetWarnings().GetUniqueMessageList();
			AssertEquals(0, warningMessages.Length);
		}

		ContainmentBarrierViewModel CreateContaineBarrierViewModel(string jobType)
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
			var viewModel = GetViewModel(task, ProcessTaskStatusCodeList.Codes.Closed);
			return viewModel;
		}

		protected override void SetUp()
		{
			base.SetUp();

			EnableBufferManagement();
			SetAsQCBTaskType("QCB", "WKI");
		}

		public ContainmentBarrierViewModel GetViewModel(IProcessTask task, string status)
		{
			var viewModel = new ContainmentBarrierViewModel(task, status, deselectCancelledTasksFromIteration: true);
			DisposableLeakListener.Instance.UnRegisterDisposable(viewModel);
			return viewModel;
		}
	}
}
