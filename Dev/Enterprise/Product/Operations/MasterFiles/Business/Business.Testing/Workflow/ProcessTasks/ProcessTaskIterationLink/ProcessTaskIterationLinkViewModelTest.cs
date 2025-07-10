using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskIterationLinkViewModel))]
	class ProcessTaskIterationLinkViewModelTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2017, 1, 1, 9, 0, 0)]
		public void TestFields()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FRO";
			staff.GS_FullName = "Frodo";

			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = testHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = testHelper.CreateWorkflow(jobHeader, "WorkflowX");
			var task = testHelper.CreateTask(workflow, description: "TaskX", taskType: "INV") as ProcessTask;

			MasterFilesTestHelper.AddIterationReasonToRegistry(task.Parent.WorkflowType, "RS1", "Test Reason 1");

			var link = Factory.New<IProcessTaskIterationLink>();
			link.P9I_P9_ContainmentBarrierTask = task.PK;
			link.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
			link.P9I_GS_NKResourceUnderReview = "FRO";
			link.P9I_IterationReason = "RS1";

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			var linkViewModel = new ProcessTaskIterationLinkViewModel(link);

			CombineAssertions(() =>
			{
				AssertEquals("ResourceUnderReviewNK", staff.GS_Code, linkViewModel.ResourceUnderReviewNK);
				AssertEquals("ResourceUnderReviewFullName", staff.GS_FullName, linkViewModel.ResourceUnderReviewFullName);
				AssertEquals("OutcomeDescription", "Iteration Created", linkViewModel.OutcomeDescription);
				AssertEquals("IterationReasonDescription", "Test Reason 1", linkViewModel.IterationReasonDescription);
				AssertEquals("P9I_SystemCreateTimeUtc", new ZDateTime(2017, 1, 1, 9, 0, 0), linkViewModel.P9I_SystemCreateTimeUtc);
				AssertEquals("SystemCreateTime", Env.Time.GetLocalTimeFromUtc(new DateTime(2017, 1, 1, 9, 0, 0)), linkViewModel.SystemCreateTime);
				AssertEquals("P9I_SystemCreateUser", "E", linkViewModel.P9I_SystemCreateUser);
				AssertEquals("P9I_SystemLastEditTimeUtc", new ZDateTime(2017, 1, 1, 9, 0, 0), linkViewModel.P9I_SystemLastEditTimeUtc);
				AssertEquals("SystemLastEditTime", Env.Time.GetLocalTimeFromUtc(new DateTime(2017, 1, 1, 9, 0, 0)), linkViewModel.SystemLastEditTime);
				AssertEquals("P9I_SystemLastEditUser", "E", linkViewModel.P9I_SystemLastEditUser);
			});
		}

		public void TestOutcomeDescription()
		{
			var task = Factory.New<ProcessTask>();

			var link = Factory.New<IProcessTaskIterationLink>();
			link.P9I_P9_ContainmentBarrierTask = task.PK;
			link.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;

			link.P9I_Outcome = IterationLinkOutcomeList.Codes.Passed;
			var linkViewModel = new ProcessTaskIterationLinkViewModel(link);
			AssertEquals("Passed", linkViewModel.OutcomeDescription);

			link.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
			linkViewModel = new ProcessTaskIterationLinkViewModel(link);
			AssertEquals("Iteration Created", linkViewModel.OutcomeDescription);

			link.P9I_Outcome = IterationLinkOutcomeList.Codes.Deferred;
			linkViewModel = new ProcessTaskIterationLinkViewModel(link);
			AssertEquals("Deferred to another user", linkViewModel.OutcomeDescription);
		}

		public void TestIterationReason_ReadOnly()
		{
			var task = Factory.New<ProcessTask>();

			var link = Factory.New<IProcessTaskIterationLink>();
			link.P9I_P9_ContainmentBarrierTask = task.PK;
			link.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;

			link.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
			var linkViewModel = new ProcessTaskIterationLinkViewModel(link);
			AssertEquals("WHEN the outcome is 'iteration required' THEN iteration reason should be editable", false, linkViewModel.IterationReasonInfo.ReadOnly);

			link.P9I_Outcome = IterationLinkOutcomeList.Codes.Deferred;
			linkViewModel = new ProcessTaskIterationLinkViewModel(link);
			AssertEquals("WHEN the outcome is 'deferred' THEN iteration reason should be readonly", true, linkViewModel.IterationReasonInfo.ReadOnly);

			link.P9I_Outcome = IterationLinkOutcomeList.Codes.Passed;
			linkViewModel = new ProcessTaskIterationLinkViewModel(link);
			AssertEquals("WHEN the outcome is 'passed' THEN iteration reason should be readonly", true, linkViewModel.IterationReasonInfo.ReadOnly);
		}

		#region Setting

		public void TestSetting_ResourceUnderReview()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FRO";

			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = testHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = testHelper.CreateWorkflow(jobHeader, "WorkflowX");
			var task1 = testHelper.CreateTask(workflow, description: "Task1", taskType: "INV", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, staffCode: staff.GS_Code) as ProcessTask;
			var task2 = testHelper.CreateTask(workflow, description: "Task2", taskType: "INV") as ProcessTask;

			var link = Factory.New<IProcessTaskIterationLink>();
			link.P9I_P9_ContainmentBarrierTask = task2.PK;
			link.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;

			Factory.Save();

			var linkViewModel = new ProcessTaskIterationLinkViewModel(link);

			AssertNullOrEmpty("Precondition", link.P9I_GS_NKResourceUnderReview);
			AssertNullOrEmpty("Precondition", linkViewModel.ResourceUnderReviewNK);

			linkViewModel.ResourceUnderReviewNK = staff.GS_Code;

			Factory.Save();

			AssertEquals("WHEN setting staff should not return error", false, linkViewModel.ResourceUnderReviewNKInfo.HasErrors());
			AssertEquals("Updating ViewModel.ResourceUnderReviewNK should update the underlying table", linkViewModel.ResourceUnderReviewNK, link.P9I_GS_NKResourceUnderReview);
		}

		public void TestSetting_IterationReason()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = testHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = testHelper.CreateWorkflow(jobHeader, "WorkflowX");
			var task = testHelper.CreateTask(workflow, description: "TaskX", taskType: "INV") as ProcessTask;

			MasterFilesTestHelper.AddIterationReasonToRegistry(task.Parent.WorkflowType, "RS1", "Test Reason 1");

			var link = Factory.New<IProcessTaskIterationLink>();
			link.P9I_P9_ContainmentBarrierTask = task.PK;
			link.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;

			Factory.Save();

			var linkViewModel = new ProcessTaskIterationLinkViewModel(link);

			AssertNullOrEmpty("Precondition", link.P9I_IterationReason);
			AssertNullOrEmpty("Precondition", linkViewModel.IterationReason);

			linkViewModel.IterationReason = "RS1";

			Factory.Save();

			AssertEquals("WHEN setting iteration-reason should not return error", false, linkViewModel.IterationReasonInfo.HasErrors());
			AssertEquals("Updating ViewModel.IterationReasion should update the underlying table", linkViewModel.IterationReason, link.P9I_IterationReason);
		}

		#endregion

		#region Validation

		#region Resource Not In List

		[TestDate(2014, 8, 20)]
		public void TestResourceNotInList_WhenModifiedShowError()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FRO";

			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = testHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = testHelper.CreateWorkflow(jobHeader, "WorkflowX");
			var task1 = testHelper.CreateTask(workflow, description: "Task1", taskType: "INV", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, staffCode: staff.GS_Code) as ProcessTask;
			var task2 = testHelper.CreateTask(workflow, description: "Task2", taskType: "INV") as ProcessTask;

			var link = Factory.New<IProcessTaskIterationLink>();
			link.P9I_P9_ContainmentBarrierTask = task2.PK;
			link.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			var linkViewModel = new ProcessTaskIterationLinkViewModel(link);
			linkViewModel.ResourceUnderReviewNK = staff.GS_Code;

			CombineAssertions("GIVEN staff's task is closed WHEN showing ProcessTaskIterationLinkViewModel", () =>
			{
				AssertEquals("THEN should show error", true, linkViewModel.ResourceUnderReviewNKInfo.HasErrors());
				AssertEquals("THEN should not show warning", false, linkViewModel.ResourceUnderReviewNKInfo.HasWarnings());

				var errorMessages = linkViewModel.GetErrors().GetUniqueMessageList();
				AssertEquals("THEN should show warning message", "Error - ResourceUnderReviewNK: Enter a valid User Under Review.", errorMessages?.FirstOrDefault());
			});
		}

		[TestDate(2014, 8, 20)]
		public void TestResourceNotInList_WhenNotModifiedShowWarning()
		{
			var processTaskIterationLink = SetupTestResourceNotInList(resourceUnderReviewNK: "FRO");

			var processTaskIterationLinkViewModel = new ProcessTaskIterationLinkViewModel(processTaskIterationLink);

			CombineAssertions("GIVEN staff's task is closed WHEN showing ProcessTaskIterationLinkViewModel", () =>
			{
				AssertEquals("THEN should not show error", false, processTaskIterationLinkViewModel.ResourceUnderReviewNKInfo.HasErrors());
				AssertEquals("THEN should show warning", true, processTaskIterationLinkViewModel.ResourceUnderReviewNKInfo.HasWarnings());

				var warningMessages = processTaskIterationLinkViewModel.GetWarnings().GetUniqueMessageList();
				AssertEquals("THEN should show warning message", "Warning - ResourceUnderReviewNK: You have not entered a valid code.", warningMessages?.FirstOrDefault());
			});
		}

		IProcessTaskIterationLink SetupTestResourceNotInList(string resourceUnderReviewNK = null)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FRO";

			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = testHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = testHelper.CreateWorkflow(jobHeader, "WorkflowX");
			var task1 = testHelper.CreateTask(workflow, description: "Task1", taskType: "INV", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, staffCode: staff.GS_Code) as ProcessTask;
			var task2 = testHelper.CreateTask(workflow, description: "Task2", taskType: "INV") as ProcessTask;

			var link = Factory.New<IProcessTaskIterationLink>();
			link.P9I_P9_ContainmentBarrierTask = task2.PK;
			link.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;

			if (resourceUnderReviewNK != null)
			{
				link.P9I_GS_NKResourceUnderReview = staff.GS_Code;
			}

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();
			return link;
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var job = MasterFilesTestHelper.CreateWorkflowProvider<SalesEnquiry>(Factory);
			var task1 = MasterFilesTestHelper.CreateTask(job);
			var task2 = MasterFilesTestHelper.CreateTask(job);

			var iterationLink = MasterFilesTestHelper.CreateIterationLink(task2, task1);

			return new ProcessTaskIterationLinkViewModel(iterationLink);
		}

		#endregion
	}
}
