using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MilestoneCollectionView))]
	sealed class MilestoneCollectionViewTest : MilestoneOrTriggerCollectionViewTest<MilestoneCollectionView>
	{
		#region TriggerTaskMilestoneCompletion

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_DbHits()
		{
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CompletionMilestoneOutcomeModes.Codes.Cancel);
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsTestHelper.EnableBMSInRegistry();
			bmsTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";

			var jobHeader = bmsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var job = jobHeader.Parent as IWorkflowProvider;
			var workflows = new List<IProcessHeader>(9);
			var tasks = new List<ProcessTask>(18);

			var milestone1 = job.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "I can fly";
			milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestone1PK = milestone1.PK;

			var milestone2 = job.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "So high";
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestone2PK = milestone2.PK;

			var milestone3 = job.WorkflowItems.Milestones.AddNew();
			milestone3.P9_Description = "In the sky";
			milestone3.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestone3PK = milestone3.PK;

			for (int i = 0; i < 3; i++)
			{
				var workflowForMilestone1 = bmsTestHelper.CreateWorkflow(jobHeader, "workflow" + i + "ForMilestone1");
				MilestoneCompletionHelper.SetCompletionMilestone(workflowForMilestone1, milestone1);
				workflows.Add(workflowForMilestone1);

				var workflowForMilestone2 = bmsTestHelper.CreateWorkflow(jobHeader, "workflow" + i + "ForMilestone2");
				MilestoneCompletionHelper.SetCompletionMilestone(workflowForMilestone2, milestone2);
				workflows.Add(workflowForMilestone2);

				var workflowWithoutMilestone = bmsTestHelper.CreateWorkflow(jobHeader, "workflowWithoutMilestone" + i);
				workflows.Add(workflowWithoutMilestone);

				for (int y = 0; y < 3; y++)
				{
					var task = bmsTestHelper.CreateTask(workflowWithoutMilestone, staffCode: staff.GS_Code, description: "Task" + y + "workflowWithoutMilestone" + i) as ProcessTask;
					MilestoneCompletionHelper.SetCompletionMilestone(task, milestone3);
					tasks.Add(task);

					var taskInWorkflowForMilestone2 = bmsTestHelper.CreateTask(workflowForMilestone2, staffCode: staff.GS_Code, description: "Task" + y + "InWorkflowForMilestone2" + i) as ProcessTask;
					MilestoneCompletionHelper.SetCompletionMilestone(task, milestone2);
					tasks.Add(taskInWorkflowForMilestone2);

					var taskInWorkflowForMilestoneNotAssigned = bmsTestHelper.CreateTask(workflowForMilestone2, description: "Task" + y + "InWorkflowForMilestone2NotAssigned" + i) as ProcessTask;
					MilestoneCompletionHelper.SetCompletionMilestone(task, milestone2);
					tasks.Add(taskInWorkflowForMilestoneNotAssigned);
				}
			}

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var expectedCompletionMilestoneHits = new Dictionary<string, int>
				{
					{ JobHeaderSchema.Constants.TableName, 1 },
					{ ProcessHeaderSchema.Constants.TableName, 0 },
					{ ProcessTasksSchema.Constants.TableName, 6 },
					{ StmALogSchema.Constants.TableName, 3 },
					{ StmDocDataOverrideSchema.Constants.TableName, 0 },
					{ StmNoteSchema.Constants.TableName, 0 },
					{ StmUniversalCopySchema.Constants.TableName, 0 }
				};

				Factory.ResetDatabaseLoadCount();
				job.Logs.AddNew(Events.CustomisableEvent01);
				AssertDbHits("Setting Milestone P9_ActualDate and actually closing the tasks.", expectedCompletionMilestoneHits, Factory, ignoreHitsFromTablesCachedInUberFactory: true);
			}

			var expectedFactorySaveHits = new Dictionary<string, int>
			{
				{ JobHeaderSchema.Constants.TableName, 0 },
				{ ProcessHeaderSchema.Constants.TableName, 0 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 6 },
			};

			Factory.ResetDatabaseLoadCount();
			Factory.Save();
			AssertDbHits("Saving the factory later.", expectedFactorySaveHits, Factory, ignoreHitsFromTablesCachedInUberFactory: true);

			foreach (var task in tasks)
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
				AssertContains("This task was completed automatically by the", task.P9_NotesAsString);
			}
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldCompleteOpenedAssignedAndSuspendedWorkflowsTasks_WhenMilestoneIsReachedAndOutcomeIsClose()
		{
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CompletionMilestoneOutcomeModes.Codes.Close);
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";

			var jobHeader = bmsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowWithOpenTask = bmsTestHelper.CreateWorkflow(jobHeader, "workflowWithOpenTask");
			var workflowWithAssignedTask = bmsTestHelper.CreateWorkflow(jobHeader, "workflowWithAssignedTask");
			var workflowWithSuspendedTask = bmsTestHelper.CreateWorkflow(jobHeader, "workflowWithSuspendedTask");
			var workflowWithClosedTask = bmsTestHelper.CreateWorkflow(jobHeader, "workflowWithClosedTask");
			var workflowWithCancelledTask = bmsTestHelper.CreateWorkflow(jobHeader, "workflowWithCancelledTask");

			bmsTestHelper.CreateTask(workflowWithOpenTask, staffCode: staff.GS_Code);
			bmsTestHelper.CreateTask(workflowWithAssignedTask, staffCode: staff.GS_Code);
			bmsTestHelper.CreateTask(workflowWithSuspendedTask, staffCode: staff.GS_Code);
			bmsTestHelper.CreateTask(workflowWithClosedTask, staffCode: staff.GS_Code);
			bmsTestHelper.CreateTask(workflowWithCancelledTask, staffCode: staff.GS_Code);

			var job = jobHeader.Parent as IWorkflowProvider;
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "I Can See";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestonePK = milestone.PK;
			var openTask = workflowWithOpenTask.Tasks.Single() as ProcessTask;
			var assignedTask = workflowWithAssignedTask.Tasks.Single() as ProcessTask;
			assignedTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var suspendedTask = workflowWithSuspendedTask.Tasks.Single() as ProcessTask;
			var closedTask = workflowWithClosedTask.Tasks.Single() as ProcessTask;
			var cancelledTask = workflowWithCancelledTask.Tasks.Single() as ProcessTask;
			var taskType = openTask.P9_Type;

			openTask.P9_Type = taskType;
			assignedTask.P9_Type = taskType;
			suspendedTask.P9_Type = taskType;
			closedTask.P9_Type = taskType;
			cancelledTask.P9_Type = taskType;

			MilestoneCompletionHelper.SetCompletionMilestone(workflowWithOpenTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(workflowWithAssignedTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(workflowWithSuspendedTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(workflowWithClosedTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(workflowWithCancelledTask, milestone);

			openTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			assignedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			suspendedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			closedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			cancelledTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			CombineAssertions("Preconditon: tasks have correct status and are valid", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Open, openTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assignedTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, suspendedTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, closedTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, cancelledTask.P9_Status);
				AssertNoErrors(openTask);
				AssertNoErrors(assignedTask);
				AssertNoErrors(suspendedTask);
				AssertNoErrors(closedTask);
				AssertNoErrors(cancelledTask);
			});

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2030, 08, 20, 15, 10, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent01);
			}

			AssertEquals("Milestone was not reached, factory was not saved", ProcessTask.NextToBeCompletedStatusCode, milestone.P9_Status);

			CombineAssertions("tasks that should never change status or notes", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, closedTask.P9_Status);
				AssertEquals("No notes", ZString.Empty, closedTask.P9_NotesAsString);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, cancelledTask.P9_Status);
				AssertEquals("No notes", ZString.Empty, cancelledTask.P9_NotesAsString);
			});

			CombineAssertions($"tasks that should change status to close and notes", () =>
			{
				var expectedNotes = $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF";
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, openTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, openTask.P9_Notes);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, assignedTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, assignedTask.P9_Notes);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, suspendedTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, suspendedTask.P9_Notes);
			});

			Factory.Save();
			AssertEquals("Milestone was reached", ProcessTask.LastCompletedStatusCode, milestone.P9_Status);
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldCloseWorkkflow_WhenCompletingLastTaskAndMilestoneIsReached()
		{
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CompletionMilestoneOutcomeModes.Codes.Close);
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";

			var jobHeader = bmsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = bmsTestHelper.CreateWorkflow(jobHeader, "workflow");

			var task1 = bmsTestHelper.CreateTask(workflow, staffCode: staff.GS_Code, sequence: 1) as ProcessTask;
			var task2 = bmsTestHelper.CreateTask(workflow, staffCode: staff.GS_Code, sequence: 1) as ProcessTask;

			var job = jobHeader.Parent as IWorkflowProvider;
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "I Can See";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestonePK = milestone.PK;

			MilestoneCompletionHelper.SetCompletionMilestone(task1, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(task2, milestone);

			CombineAssertions("Preconditon: tasks and workflow have correct status and are valid", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
				AssertNoErrors(task1);
				AssertNoErrors(task2);

				AssertEquals("OPN", workflow.FH_Status);
				AssertEquals("OPN", jobHeader.FH_Status);
			});

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2030, 08, 20, 15, 10, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent01);
			}

			AssertEquals("Milestone was not reached, factory was not saved", ProcessTask.NextToBeCompletedStatusCode, milestone.P9_Status);

			CombineAssertions($"tasks that should change status to close and set notes", () =>
			{
				var expectedNotes = $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF";
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, task1.P9_Notes);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, task2.P9_Notes);

				AssertEquals("Workflow should be closed", "CLS", workflow.FH_Status);
				AssertEquals("Job Header should be closed", "CLS", jobHeader.FH_Status);
			});

			Factory.Save();
			AssertEquals("Milestone was reached", ProcessTask.LastCompletedStatusCode, milestone.P9_Status);
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		[TestUtcOffset(-10, 0, 0)]
		public void TestTriggerTaskMilestoneCompletion_HasCorrectUTCTimeInTaskNotes_WhenMilestoneOutcomeIsClose()
		{
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CompletionMilestoneOutcomeModes.Codes.Close);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var milestonePK = milestone.PK;
			milestone.P9_Description = "My Kilometrestone";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;

			var assignedTask = job.WorkflowItems.Tasks.AddNew();
			assignedTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var taskType = assignedTask.Lookups.Types[0].Code;

			assignedTask.P9_Type = taskType;

			MilestoneCompletionHelper.SetCompletionMilestone(assignedTask, milestone);

			assignedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2030, 08, 20, 15, 10, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent01);
			}

			CombineAssertions("Tasks should change to closed status and have the expected UTC time in notes.", () =>
			{
				var expectedNotes = $"This task was completed automatically by the My Kilometrestone milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF";
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, assignedTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, assignedTask.P9_Notes);
			});
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldCancelOnlyOpenedAssignedAndSuspendedTasks_WhenMilestoneIsReachedAndOutcomeIsCancel()
		{
			AssertOnlyOpenedAssignedAndSuspendedTasksCompleteWhenMilestoneIsReached(ProcessTaskStatusCodeList.Codes.Cancelled, CompletionMilestoneOutcomeModes.Codes.Cancel);
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldCloseOnlyOpenedAssignedAndSuspendedTasks_WhenMilestoneIsReachedAndOutcomeIsClose()
		{
			AssertOnlyOpenedAssignedAndSuspendedTasksCompleteWhenMilestoneIsReached(ProcessTaskStatusCodeList.Codes.Closed, CompletionMilestoneOutcomeModes.Codes.Close);
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldCloseOnlyOpenedAssignedAndSuspendedTasks_WhenMilestoneIsReachedAndOutcomeIsAttemptToClose()
		{
			AssertOnlyOpenedAssignedAndSuspendedTasksCompleteWhenMilestoneIsReached(ProcessTaskStatusCodeList.Codes.Closed, CompletionMilestoneOutcomeModes.Codes.AttemptToClose);
		}

		void AssertOnlyOpenedAssignedAndSuspendedTasksCompleteWhenMilestoneIsReached(string expectedTasksStatus, string completionMilestoneOutcome)
		{
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, completionMilestoneOutcome);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "I Can See";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestonePK = milestone.PK;
			var openTask = job.WorkflowItems.Tasks.AddNew();
			var assignedTask = job.WorkflowItems.Tasks.AddNew();
			assignedTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var suspendedTask = job.WorkflowItems.Tasks.AddNew();
			var closedTask = job.WorkflowItems.Tasks.AddNew();
			var cancelledTask = job.WorkflowItems.Tasks.AddNew();
			var taskType = openTask.Lookups.Types[0].Code;

			openTask.P9_Type = taskType;
			assignedTask.P9_Type = taskType;
			suspendedTask.P9_Type = taskType;
			closedTask.P9_Type = taskType;
			cancelledTask.P9_Type = taskType;

			MilestoneCompletionHelper.SetCompletionMilestone(openTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(assignedTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(suspendedTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(closedTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(cancelledTask, milestone);

			openTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			assignedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			suspendedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			closedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			cancelledTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			CombineAssertions("Preconditon: tasks have correct status and are valid", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Open, openTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assignedTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, suspendedTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, closedTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, cancelledTask.P9_Status);
				AssertNoErrors(openTask);
				AssertNoErrors(assignedTask);
				AssertNoErrors(suspendedTask);
				AssertNoErrors(closedTask);
				AssertNoErrors(cancelledTask);
			});

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2030, 08, 20, 15, 10, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent01);
			}

			AssertEquals("Milestone was not reached, factory was not saved", ProcessTask.NextToBeCompletedStatusCode, milestone.P9_Status);

			CombineAssertions("tasks that should never change status or notes", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, closedTask.P9_Status);
				AssertEquals("No notes", ZString.Empty, closedTask.P9_NotesAsString);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, cancelledTask.P9_Status);
				AssertEquals("No notes", ZString.Empty, cancelledTask.P9_NotesAsString);
			});

			CombineAssertions($"tasks that should change status to {expectedTasksStatus} and notes", () =>
			{
				var expectedNotes = $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF";
				AssertEquals(expectedTasksStatus, openTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, openTask.P9_Notes);
				AssertEquals(expectedTasksStatus, assignedTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, assignedTask.P9_Notes);
				AssertEquals(expectedTasksStatus, suspendedTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, suspendedTask.P9_Notes);
			});

			Factory.Save();
			AssertEquals("Milestone was reached", ProcessTask.LastCompletedStatusCode, milestone.P9_Status);
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldNotCancelTaskWithErrors_WhenMilestoneIsReachedAndOutcomeIsCancel()
		{
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CompletionMilestoneOutcomeModes.Codes.Cancel);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "I Can See";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestonePK = milestone.PK;

			var validTask = job.WorkflowItems.Tasks.AddNew();
			var invalidTask = job.WorkflowItems.Tasks.AddNew();
			var taskType = validTask.Lookups.Types[0].Code;

			MilestoneCompletionHelper.SetCompletionMilestone(validTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(invalidTask, milestone);

			validTask.P9_Type = taskType;
			invalidTask.P9_Type = "NOP";

			CombineAssertions("Preconditon", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, validTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, invalidTask.P9_Status);
				AssertNoErrors("Task is valid", validTask);
				AssertHasErrors("Task is invalid", invalidTask.P9_TypeInfo);
			});

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2030, 08, 20, 15, 10, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent01);
			}

			AssertEquals("Milestone was not reached, factory was not saved", ProcessTask.NextToBeCompletedStatusCode, milestone.P9_Status);

			CombineAssertions($"Only valid task should be cancel", () =>
			{
				var expectedNotes = $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF";

				AssertNoErrors("Task should still be valid", validTask);
				AssertHasErrors("Task should still be invalid", invalidTask.P9_TypeInfo);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, validTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, validTask.P9_Notes);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, invalidTask.P9_Status);
				AssertEquals("No notes", ZString.Empty, invalidTask.P9_NotesAsString);
			});
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldNotCloseTaskWithErrors_WhenMilestoneIsReachedAndOutcomeIsClose()
		{
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CompletionMilestoneOutcomeModes.Codes.Close);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "I Can See";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestonePK = milestone.PK;

			var validTask = job.WorkflowItems.Tasks.AddNew();
			var invalidTask = job.WorkflowItems.Tasks.AddNew();
			var taskType = validTask.Lookups.Types[0].Code;

			MilestoneCompletionHelper.SetCompletionMilestone(validTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(invalidTask, milestone);

			validTask.P9_Type = taskType;
			invalidTask.P9_Type = "NOP";

			CombineAssertions("Preconditon", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, validTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, invalidTask.P9_Status);
				AssertNoErrors("Task is valid", validTask);
				AssertHasErrors("Task is invalid", invalidTask.P9_TypeInfo);
			});

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2030, 08, 20, 15, 10, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent01);
			}

			AssertEquals("Milestone was not reached, factory was not saved", ProcessTask.NextToBeCompletedStatusCode, milestone.P9_Status);

			CombineAssertions($"Only valid task should close", () =>
			{
				var expectedNotes = $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF";

				AssertNoErrors("Task should still be valid", validTask);
				AssertHasErrors("Task should still be invalid", invalidTask.P9_TypeInfo);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, validTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, validTask.P9_Notes);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, invalidTask.P9_Status);
				AssertEquals("No notes", ZString.Empty, invalidTask.P9_NotesAsString);
			});
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldNotCancelTaskWithErrorAfterTryToComplente_WhenMilestoneIsReachedAndOutcomeIsAttemptToClose()
		{
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CompletionMilestoneOutcomeModes.Codes.Close);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "I Can See";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestonePK = milestone.PK;

			var validTask = job.WorkflowItems.Tasks.AddNew();
			var invalidTask = job.WorkflowItems.Tasks.AddNew();
			var taskType = validTask.Lookups.Types[0].Code;

			MilestoneCompletionHelper.SetCompletionMilestone(validTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(invalidTask, milestone);

			validTask.P9_Type = taskType;
			invalidTask.P9_Type = "NOP";

			CombineAssertions("Preconditon", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, validTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, invalidTask.P9_Status);
				AssertNoErrors("Task is valid", validTask);
				AssertHasErrors("Task is invalid", invalidTask.P9_TypeInfo);
			});

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2030, 08, 20, 15, 10, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent01);
			}

			AssertEquals("Milestone was not reached, factory was not saved", ProcessTask.NextToBeCompletedStatusCode, milestone.P9_Status);

			CombineAssertions($"Only valid task should close", () =>
			{
				var expectedNotes = $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF";

				AssertNoErrors("Task should still be valid", validTask);
				AssertHasErrors("Task should still be invalid", invalidTask.P9_TypeInfo);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, validTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, validTask.P9_Notes);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, invalidTask.P9_Status);
				AssertEquals("No notes", ZString.Empty, invalidTask.P9_NotesAsString);
			});
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldNotCompleteContainmentBarrierTask_WhenMilestoneIsReached()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CompletionMilestoneOutcomeModes.Codes.Close);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			var jobLevelWorkflow = bmsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = bmsTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");
			var job = jobLevelWorkflow.Parent as IWorkflowProvider;
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "I Can See";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestonePK = milestone.PK;

			var normalTask = job.WorkflowItems.Tasks.AddNew();
			normalTask.P9_Type = normalTask.Lookups.Types[0].Code;
			var containmentBarrierTask = job.WorkflowItems.Tasks.AddNew();
			containmentBarrierTask.P9_Type = "CB";

			MilestoneCompletionHelper.SetCompletionMilestone(normalTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(containmentBarrierTask, milestone);

			CombineAssertions("Preconditon", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, normalTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, containmentBarrierTask.P9_Status);
				AssertNoErrors("Task is valid", normalTask);
				AssertNoErrors("Task is valid", containmentBarrierTask);
			});

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2030, 08, 20, 15, 10, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent01);
			}

			AssertEquals("Milestone was not reached, factory was not saved", ProcessTask.NextToBeCompletedStatusCode, milestone.P9_Status);

			CombineAssertions($"Only normal Task task should close", () =>
			{
				var expectedNotes = $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF";

				AssertNoErrors("Task should still be valid", normalTask);
				AssertNoErrors("Task should still invalid", containmentBarrierTask);

				AssertEquals("Normal task should close", ProcessTaskStatusCodeList.Codes.Closed, normalTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has completion infos", expectedNotes, normalTask.P9_Notes);

				AssertEquals("Containment Barrier Task should not close", ProcessTaskStatusCodeList.Codes.Assigned, containmentBarrierTask.P9_Status);
				AssertEquals("No notes", ZString.Empty, containmentBarrierTask.P9_NotesAsString);
			});
		}

		[TestDate(2022, 06, 17, 1, 30, 45)]
		public void TestTriggerTaskMilestoneCompletion_ShouldCancelTaskWithError_WhenMilestoneIsReachedAndOutcomeIsAttemptToClose()
		{
			WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CompletionMilestoneOutcomeModes.Codes.AttemptToClose);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "I Can See";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			var milestonePK = milestone.PK;

			var validTask = job.WorkflowItems.Tasks.AddNew();
			var invalidTaskWhenClose = job.WorkflowItems.Tasks.AddNew();
			var taskType = validTask.Lookups.Types[0];
			taskType.IsRequireActualDuration = true;
			var lastValidTask = job.WorkflowItems.Tasks.AddNew();

			var taskTypeCode = validTask.Lookups.Types[0].Code;

			MilestoneCompletionHelper.SetCompletionMilestone(validTask, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(invalidTaskWhenClose, milestone);
			MilestoneCompletionHelper.SetCompletionMilestone(lastValidTask, milestone);

			validTask.P9_Type = taskTypeCode;
			validTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			validTask.P9_ActualDuration = TestDateAttribute.Date.AddHours(1);
			validTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			validTask.P9_Sequence = 1;

			invalidTaskWhenClose.P9_Type = taskTypeCode;
			invalidTaskWhenClose.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			invalidTaskWhenClose.P9_Sequence = 2;

			lastValidTask.P9_Type = taskTypeCode;
			lastValidTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			lastValidTask.P9_ActualDuration = TestDateAttribute.Date.AddHours(1);
			lastValidTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			lastValidTask.P9_Sequence = 3;

			CombineAssertions("Preconditon", () =>
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, validTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, invalidTaskWhenClose.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, lastValidTask.P9_Status);
				AssertNoErrors("Task is valid", validTask);
				AssertHasErrors("Task is invalid because need duration to close", invalidTaskWhenClose.P9_ActualDurationInfo);
			});

			//changing task to Assigned to try to make CompletionMilestone try to close it.
			invalidTaskWhenClose.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2030, 08, 20, 15, 10, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent01);
			}

			AssertEquals("Milestone was not reached, factory was not saved", ProcessTask.NextToBeCompletedStatusCode, milestone.P9_Status);

			CombineAssertions($"Valid tasks should be closed. Invalid task if should be canceled", () =>
			{
				AssertNoErrors("Task should still be valid", validTask);
				AssertNoErrors("Task should be valid, it is ok to cancel without actual duration", invalidTaskWhenClose);
				AssertNoErrors("Task should still be valid", lastValidTask);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, validTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Task notes has correct completion infos", $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF", validTask.P9_Notes);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, invalidTaskWhenClose.P9_Status);
				MasterFilesTestHelper.AssertRtfText("task notes has correct completion infos", $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF, due to validation problems this task was canceled", invalidTaskWhenClose.P9_Notes);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, lastValidTask.P9_Status);
				MasterFilesTestHelper.AssertRtfText("Last task notes has correct completion infos", $"This task was completed automatically by the I Can See milestone at 20-Aug-30 15:10:05 (UTC) based on the {Events.CustomisableEvent01Code} event caused by STF", validTask.P9_Notes);
			});
		}

		public void TestMilestoneCompletion_WhenClosedTasksHaveCompletionEvents_ShouldFireThoseEvents()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobLevelWorkflow = helper.GetJobHeaderForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobLevelWorkflow, "Workflow");
			var task1 = (ProcessTask)helper.CreateTask(workflow);
			var task2 = (ProcessTask)helper.CreateTask(workflow);
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "Z00";
			milestone.P9_Description = "I am milsey";
			MilestoneCompletionHelper.SetCompletionMilestone(task1, milestone);

			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(2, task2.P9_Sequence);

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);

			var eventsFromTaskClosing = task2.Logs.Find(log => log.SL_SE_NKEvent == Events.StartabilityChangedCode);
			AssertContainsExactElementsInAnyOrder("No event should be added yet because the factory isn't saved.", Array.Empty<string>(), eventsFromTaskClosing.Select(x => x.SL_Reference));

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			eventsFromTaskClosing = task2.Logs.Find(log => log.SL_SE_NKEvent == Events.StartabilityChangedCode);
			AssertContainsExactElementsInAnyOrder("The task completion event should have been added.", new[] { "|SRT=Y" }, eventsFromTaskClosing.Select(x => x.SL_Reference));
		}

		#endregion

		public new void TestAllowSort()
		{
			Assert(((IBindingList)Collection).SupportsSorting);
		}

		public override void TestIsThisPartOfTheCollection()
		{
			ProcessTask task = Collection.AddNew();
			task.IsMilestone = false;
			task.IsException = false;

			ProcessTask milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.IsException = false;

			ProcessTask exception = Collection.AddNew();
			exception.IsMilestone = false;
			exception.IsException = true;

			AssertEquals("The item that returns true on IsTypeMatch should be included", 1, Collection.Count);
			AssertEquals("The item that returns true on IsTypeMatch should be included", milestone, Collection[0]);
		}

		public void TestSetCollectionRelationships()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			AssertEquals("IsMilestone", true, milestone.IsMilestone);
			AssertEquals("P9_ParentID NOT attached to job", Dummy.PK, milestone.P9_ParentID);
		}

		#region Milestones Concurrency

		[ExpectNoExceptions]
		public void TestSortCollectionDoesNotCrashSortingMilestones()
		{
			SetMilestoneStatusWithSideEffect(d => d.WorkflowItems.Milestones.Sort("P9_Sequence"));
		}

		[ExpectNoExceptions]
		public void TestSortCollectionDoesNotCrashRebuildingMilestones()
		{
			SetMilestoneStatusWithSideEffect(d => d.WorkflowItems.Milestones.Rebuild());
		}

		[ExpectNoExceptions]
		public void TestRecursivelySetStatus()
		{
			SetMilestoneStatusWithSideEffect(d => d.WorkflowItems.Milestones.SetMilestoneStatuses());
		}

		void SetMilestoneStatusWithSideEffect(Action<DummyWithWorkflow> actoManacto)
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var milestone1 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Sequence = 1;
			var milestone2 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Sequence = 3;
			var milestone3 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone3.P9_Sequence = 2;

			milestone1.P9_StatusInfo.ValueChanged += (s, e) => actoManacto(dummy1);
			dummy1.WorkflowItems.Milestones.SetMilestoneStatuses();
		}

		[ExpectNoExceptions]
		public void TestIsValidColumnConcurrencyMerge()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var milestone1 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Sequence = 1;
			((ILightValidationInternals)milestone1).IsValid = true;
			milestone1.P9_ActualDateInternal = new ZDateTimeOffset(ZDateTime.Now.AddDays(-1));
			milestone1.P9_Status = "LST";
			var milestone2 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Sequence = 2;
			((ILightValidationInternals)milestone2).IsValid = true;
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			milestone1 = factory1.Load<ProcessTask>(milestone1.PK);
			milestone2 = factory1.Load<ProcessTask>(milestone2.PK);
			milestone2.P9_ActualDateInternal = new ZDateTimeOffset(ZDateTime.Now);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var milestone1Reload = factory2.Load<ProcessTask>(milestone1.PK);
			((ILightValidationInternals)milestone1Reload).IsValid = false;
			factory2.Save();
			ZExceptionReporting.ProcessWithSaveExceptionHandling(factory1.Save, null, true);

			AssertEquals("The value of P9_IsValid should be merged", false, ((ILightValidationInternals)milestone1).IsValid);
		}

		public void TestDateIsNotReverted()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyWithWorkflow dummy1 = factory1.New<DummyWithWorkflow>();

			ProcessTask milestone1 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;
			milestone1.SetMilestoneActualDateForTest(ZDateTime.Now);
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyWithWorkflow dummy2 = factory2.Load<DummyWithWorkflow>(dummy1.PK);
			AssertNotEquals(ZDateTime.Empty, dummy2.WorkflowItems.Milestones[Events.AddedARecordToTheSystem].P9_ActualDateInfo.OriginalValue);
			AssertNotEquals(ZDateTime.Empty, dummy2.WorkflowItems.Milestones[Events.AddedARecordToTheSystem].P9_ActualDate);

			milestone1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			factory1.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyWithWorkflow dummy3 = factory3.Load<DummyWithWorkflow>(dummy1.PK);
			AssertEquals(ZDateTime.Empty, dummy3.WorkflowItems.Milestones[Events.AddedARecordToTheSystem].P9_ActualDateInfo.OriginalValue);
			AssertEquals(ZDateTime.Empty, dummy3.WorkflowItems.Milestones[Events.AddedARecordToTheSystem].P9_ActualDate);

			dummy2.WorkflowItems.Milestones[Events.AddedARecordToTheSystem].P9_Description = "zz";
			factory2.Save();

			BusinessObjectFactory factory4 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyWithWorkflow dummy4 = factory4.Load<DummyWithWorkflow>(dummy1.PK);
			AssertNotEquals(ZDateTime.Empty, dummy4.WorkflowItems.Milestones[Events.AddedARecordToTheSystem].P9_ActualDateInfo.OriginalValue);
			AssertNotEquals(ZDateTime.Empty, dummy4.WorkflowItems.Milestones[Events.AddedARecordToTheSystem].P9_ActualDate);
		}

		[ExpectNoExceptions]
		public void TestMilestonesConcurency()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyWithWorkflow dummy1 = factory1.New<DummyWithWorkflow>();

			ProcessTask milestone1 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;

			ProcessTask milestone2 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			ProcessTask milestone3 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			milestone1.SetMilestoneActualDateForTest(ZDateTime.Now);

			dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			factory1.Save();
			AssertEquals(1, dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);

			BusinessObjectFactory factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyWithWorkflow dummy2 = factory2.Load<DummyWithWorkflow>(dummy1.PK);

			dummy1.WorkflowItems.Milestones[Events.Departure].SetMilestoneActualDateForTest(new ZDateTime(2005, 1, 2));

			dummy2.WorkflowItems.Milestones[Events.Arrival].SetMilestoneActualDateForTest(new ZDateTime(2005, 1, 3));

			dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			factory1.Save();
			AssertEquals(1, dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);
			AssertEquals(0, dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);

			AssertEquals(new ZDateTime(2005, 1, 2), dummy1.WorkflowItems.Milestones[Events.Departure].P9_ActualDate);
			AssertEquals("CLS", dummy1.WorkflowItems.Milestones[Events.Departure].P9_Status);
			AssertEquals(ZDateTime.Empty, dummy1.WorkflowItems.Milestones[Events.Arrival].P9_ActualDate);
			AssertEquals("NXT", dummy1.WorkflowItems.Milestones[Events.Arrival].P9_Status);

			dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			factory2.Save();
			AssertEquals(0, dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);
			AssertEquals(1, dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);

			AssertEquals(new ZDateTime(2005, 1, 2), dummy2.WorkflowItems.Milestones[Events.Departure].P9_ActualDate);
			AssertEquals("CLS", dummy2.WorkflowItems.Milestones[Events.Departure].P9_Status);
			AssertEquals(new ZDateTime(2005, 1, 3), dummy2.WorkflowItems.Milestones[Events.Arrival].P9_ActualDate);
			AssertEquals("CLS", dummy2.WorkflowItems.Milestones[Events.Arrival].P9_Status);
		}

		public void TestMilestonesConcurencyOtherWay()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyWithWorkflow dummy1 = factory1.New<DummyWithWorkflow>();

			ProcessTask milestone1 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;

			ProcessTask milestone2 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			ProcessTask milestone3 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			milestone1.SetMilestoneActualDateForTest(ZDateTime.Now);

			dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			factory1.Save();
			AssertEquals(1, dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);

			BusinessObjectFactory factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyWithWorkflow dummy2 = factory2.Load<DummyWithWorkflow>(dummy1.PK);

			dummy1.WorkflowItems.Milestones[Events.Departure].SetMilestoneActualDateForTest(new ZDateTime(2005, 1, 2));

			dummy2.WorkflowItems.Milestones[Events.Arrival].SetMilestoneActualDateForTest(new ZDateTime(2005, 1, 3));

			dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			factory2.Save();
			AssertEquals(0, dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);
			AssertEquals(1, dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);

			AssertEquals(ZDateTime.Empty, dummy2.WorkflowItems.Milestones[Events.Departure].P9_ActualDate);
			AssertEquals("NXT", dummy2.WorkflowItems.Milestones[Events.Departure].P9_Status);
			AssertEquals(new ZDateTime(2005, 1, 3), dummy2.WorkflowItems.Milestones[Events.Arrival].P9_ActualDate);
			AssertEquals("CLS", dummy2.WorkflowItems.Milestones[Events.Arrival].P9_Status);

			dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			factory1.Save();
			AssertEquals(1, dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);
			AssertEquals(0, dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);

			AssertEquals(new ZDateTime(2005, 1, 2), dummy1.WorkflowItems.Milestones[Events.Departure].P9_ActualDate);
			AssertEquals("CLS", dummy1.WorkflowItems.Milestones[Events.Departure].P9_Status);
			AssertEquals(new ZDateTime(2005, 1, 3), dummy1.WorkflowItems.Milestones[Events.Arrival].P9_ActualDate);
			AssertEquals("CLS", dummy1.WorkflowItems.Milestones[Events.Arrival].P9_Status);
		}

		public void TestSetMilestoneStatusesIsCalledForAllCollections()
		{
			DummyWithWorkflow dummy1 = Factory.New<DummyWithWorkflow>();

			ProcessTask milestone1 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;

			ProcessTask milestone2 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			ProcessTask milestone3 = dummy1.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			milestone1.SetMilestoneActualDateForTest(ZDateTime.Now);

			DummyWithWorkflow dummy2 = Factory.New<DummyWithWorkflow>();

			ProcessTask milestone4 = dummy2.WorkflowItems.Milestones.AddNew();
			milestone4.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;

			ProcessTask milestone5 = dummy2.WorkflowItems.Milestones.AddNew();
			milestone5.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			ProcessTask milestone6 = dummy2.WorkflowItems.Milestones.AddNew();
			milestone6.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			milestone4.SetMilestoneActualDateForTest(ZDateTime.Now);

			dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug = 0;
			Factory.Save();
			AssertEquals(1, dummy1.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);
			AssertEquals(1, dummy2.WorkflowItems.Milestones.SetMilestoneStatusesAndRefreshCountForDebug);
		}

		#endregion

		#region TestDoNotUpdateEventOnRefresh

		public void TestDoNotUpdateEventOnRefresh()
		{
			ZDateTime time1 = new ZDateTime(2011, 5, 27, 13, 47, 12);
			ZDateTime time2 = new ZDateTime(2011, 5, 27, 13, 48, 13);

			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Qwerty";
			milestone.TriggerConditions.TriggerEventCode = AutoEvents.CustomsReadyToPayCode;
			milestone.SetMilestoneActualDateForTest(time1);
			Factory.Save();

			ProcessTask milestoneInOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTask>(milestone.PK);
			milestoneInOtherFactory.P9_Description = "Zxcvbnm";
			milestoneInOtherFactory.Factory.Save();

			StmALog log = Dummy.Logs.MostRecentLogByEventTime(AutoEvents.CustomsReadyToPay);
			AssertNotNull(log);
			AssertEquals(time1, log.SL_EventTime);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "Abcd";
			}
			milestone.SetMilestoneActualDateForTest(time2);

			StmALog log1 = Dummy.Logs.MostRecentLogByEventTime(AutoEvents.CustomsReadyToPay);
			AssertNotEquals(log.PK, log1.PK);
			AssertEquals("Should update log", time2, log1.SL_EventTime);
			AssertEquals("Should update log", "", log1.SL_Reference);

			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_Reference = "Abcd";
			}

			AssertEquals("Qwerty", milestone.P9_Description);

			Factory.Save();

			AssertEquals("Should reload db values", "Zxcvbnm", milestone.P9_Description);
			AssertEquals("Should keep edited date", time2, milestone.P9_ActualDate);

			StmALog log2 = Dummy.Logs.MostRecentLogByEventTime(AutoEvents.CustomsReadyToPay);
			AssertNotEquals(log.PK, log2.PK);
			AssertEquals(log1.PK, log2.PK);
			AssertEquals("Should not update log", "Abcd", log2.SL_Reference);
		}

		#endregion

		public void TestSetMilestoneStatusesWithoutListChanged()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			dummy.WorkflowItems.MilestonesIncludingRelated.AddNew();

			var bindingList = dummy.WorkflowItems.Milestones as IBindingList;
			var listChangedEventFiredCount = 0;
			bindingList.ListChanged += (sender, e) =>
			{
				listChangedEventFiredCount++;
			};
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			AssertGreaterThan(listChangedEventFiredCount, 0);
		}

		public void TestSetMilestoneStatusesWithoutCollectionToFilterListChanged()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			dummy.WorkflowItems.MilestonesIncludingRelated.AddNew();

			var bindingList = dummy.WorkflowItems.Milestones.CollectionToFilter as IBindingList;
			var listChangedEventFiredCount = 0;
			bindingList.ListChanged += (sender, e) =>
			{
				listChangedEventFiredCount++;
			};
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			AssertGreaterThan(listChangedEventFiredCount, 0);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ProcessTask result = Factory.New<ProcessTask>();
			result.IsMilestone = true;
			return result;
		}

		protected override WorkflowItemCollectionView GetNewCollectionView(ProcessTaskCollection collection)
		{
			return new MilestoneCollectionView(collection);
		}

		#endregion
	}
}
