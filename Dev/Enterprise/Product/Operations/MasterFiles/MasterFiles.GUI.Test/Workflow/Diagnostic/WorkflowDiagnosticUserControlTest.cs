using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public abstract class WorkflowDiagnosticUserControlTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFormShow_NoBinding()
		{
			using (var form = new WorkflowDiagnosticForm(null))
			{
				AssertNoExceptionThrown("Showing form", form.Show);
			}
		}

		public void TestFormShow_NoTask()
		{
			using (var form = new WorkflowDiagnosticForm(null))
			{
				var viewModel = new WorkflowDiagnosticViewModel(Dummy.WorkflowItems.TriggersIncludingRelated[0]);
				form.SetDataBinding(viewModel, "");

				AssertNoExceptionThrown("Showing form", form.Show);
			}
		}

		public void TestFormShow_NoTriggerNorMilestone()
		{
			using (var form = new WorkflowDiagnosticForm(null))
			{
				var viewModel = new WorkflowDiagnosticViewModel(Dummy.WorkflowItems.Tasks.AddNew());
				form.SetDataBinding(viewModel, "");

				AssertNoExceptionThrown("Showing form", form.Show);
			}
		}

		[RequiresSTA]
		public void TestFormShow_TriggerWithNoWTELog()
		{
			var trigger = Dummy.WorkflowItems.TriggersIncludingRelated[0];

			using (var form = new WorkflowDiagnosticForm(null))
			{
				var viewModel = new WorkflowDiagnosticViewModel(trigger);
				form.SetDataBinding(viewModel, "");

				form.Show();
				Application.DoEvents();

				CombineAssertions("GIVEN job with trigger, WHEN Workflow-Diagnostic-Form is shown, should show trigger information", () =>
				{
					AssertTriggerInformationShown(form, trigger);
				});
			}
		}

		public void TestFormShow_TriggerWithWTELog_NoJobQueue()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var trigger = Dummy.WorkflowItems.TriggersIncludingRelated[0];

			Dummy.Logs.AddNew(AutoEvents.Authorised);

			using (var form = new WorkflowDiagnosticForm(null))
			{
				var viewModel = new WorkflowDiagnosticViewModel(trigger);
				form.SetDataBinding(viewModel, "");

				form.Show();
				Application.DoEvents();

				CombineAssertions("GIVEN job with ATH trigger, WHEN ATH event raised and WTE log created, Workflow-Diagnostic-Form should show trigger, WTE log and triggering-log information", () =>
				{
					AssertTriggerInformationShown(form, trigger);
					AssertWTELogInformationShow(form, trigger);
					AssertTriggeringLogInformationShow(form, Dummy);
				});
			}
		}

		public void TestFormShow_TriggerWithWTELog_WithJobQueue()
		{
			var trigger = Dummy.WorkflowItems.TriggersIncludingRelated[0];

			var athLog = Dummy.Logs.AddNew(AutoEvents.Authorised);

			var wteLog = ((WTELogViewModel)new WorkflowDiagnosticViewModel(trigger).WTELogs.Single()).WTELog;

			var jobQueues = new StmJobQueueCollection(wteLog);
			var jobQueue = new QueuedLogForTesting(wteLog, trigger);
			jobQueues.Add(new StmJobQueueViewModel(jobQueue));

			var wteLogMock = new WTELogViewModelForTest(trigger, wteLog);
			wteLogMock.JobQueueOverride = jobQueues;

			using (var form = new WorkflowDiagnosticForm(null))
			{
				var viewModel = new WorkflowDiagnosticViewModel(trigger);

				// Replace WTELogs with our mock
				viewModel.WTELogs.RemoveAndDeleteAll();
				viewModel.WTELogs.Add(wteLogMock);

				form.SetDataBinding(viewModel, "");

				form.Show();
				Application.DoEvents();

				CombineAssertions("GIVEN job with ATH trigger, WHEN ATH event raised, WTE log created and JobQueue was created, Workflow-Diagnostic-Form should show trigger, WTE log and JobQueue", () =>
				{
					AssertTriggerInformationShown(form, trigger);
					AssertWTELogInformationShow(form, trigger);
					AssertTriggeringLogInformationShow(form, Dummy);
					AssertJobQueueInformationShow(form, jobQueue);
				});
			}
		}

		[RequiresSTA]
		public void TestFormShow_TriggerInformationCorrect()
		{
			var trigger = Dummy.WorkflowItems.TriggersIncludingRelated[0];

			using (var form = new WorkflowDiagnosticForm(null))
			{
				var viewModel = new WorkflowDiagnosticViewModel(trigger);
				form.SetDataBinding(viewModel, "");

				form.Show();
				Application.DoEvents();

				AssertTriggerInformationShown(form, trigger);
			}
		}

		[RequiresSTA]
		public void TestFormShow_NotModal()
		{
			var trigger = Dummy.WorkflowItems.TriggersIncludingRelated[0];

			using (var parentForm = new ZForm(null))
			{
				WorkflowDiagnosticUtilities.ShowWorkflowDiagnosisForm(trigger, parentForm);

				var openForms = Application.OpenForms;
				AssertEquals(openForms.Count, 1);
				var form = openForms[0];
				AssertEquals(form.GetType(), typeof(WorkflowDiagnosticForm));
				AssertEquals(form.Modal, false);
				form.Close();
			}
		}

		public void TestFormShow_ClosesWhenParentCloses()
		{
			var trigger = Dummy.WorkflowItems.TriggersIncludingRelated[0];

			using (var parentForm = new ZForm(null))
			{
				parentForm.Show();
				WorkflowDiagnosticUtilities.ShowWorkflowDiagnosisForm(trigger, parentForm);

				var openForms = Application.OpenForms;
				AssertEquals(openForms.Count, 2);

				parentForm.Close();
				AssertEquals(openForms.Count, 0);
			}
		}

		public void TestFormShow_ClosingFromDoesNotCloseParent()
		{
			var trigger = Dummy.WorkflowItems.TriggersIncludingRelated[0];

			using (var parentForm = new ZForm(null))
			{
				parentForm.Show();
				WorkflowDiagnosticUtilities.ShowWorkflowDiagnosisForm(trigger, parentForm);

				var openForms = Application.OpenForms;
				AssertEquals(openForms.Count, 2);

				var workflowDiagnosticForm = openForms.OfType<WorkflowDiagnosticForm>().First();
				workflowDiagnosticForm.Close();
				AssertEquals(openForms.Count, 1);
				AssertEquals(openForms[0], parentForm);
			}
		}

		class WTELogViewModelForTest : WTELogViewModel
		{
			public WTELogViewModelForTest(ProcessTask trigger, StmALog wteLog)
				: base(trigger, wteLog)
			{
			}

			public StmJobQueueCollection JobQueueOverride { get; set; }

			public override StmJobQueueCollection JobQueues => JobQueueOverride ?? base.JobQueues;
		}

		[RequiresSTA]
		public void TestForm_HasValidCaptions()
		{
			using (var form = new WorkflowDiagnosticForm(null))
			{
				var viewModel = new WorkflowDiagnosticViewModel(Dummy.WorkflowItems.TriggersIncludingRelated[0]);
				form.SetDataBinding(viewModel, "");

				form.Show();
				Application.DoEvents();

				CombineAssertions("WHEN workflow-diagnostic form is shown, captions should be human-readable", () =>
				{
					var descriptionTextBox = form.Controls.Find("DescriptionTextBox", true)[0] as ZTextBox;
					AssertEquals("Trigger's description should be human readable", "Description", descriptionTextBox.GetExtension<LabelCaptionRenderer>().Caption);

					var actualStartTextBox = form.Controls.Find("ActualStartTextBox", true)[0] as ZTextBox;
					AssertEquals("Trigger's actual-start should be human readable", "Actual Start Time", actualStartTextBox.GetExtension<LabelCaptionRenderer>().Caption);

					var eventTextBox = form.Controls.Find("EventTextBox", true)[0] as ZTextBox;
					AssertEquals("Trigger's event-code should be human readable", "Event Code", eventTextBox.GetExtension<LabelCaptionRenderer>().Caption);

					var triggerFieldTextBox = form.Controls.Find("TriggerFieldTextBox", true)[0] as ZTextBox;
					AssertEquals("Trigger's trigger-field should be human readable", "Trigger Field", triggerFieldTextBox.GetExtension<LabelCaptionRenderer>().Caption);

					var triggerConditionTextBox = form.Controls.Find("TriggerConditionTextBox", true)[0] as ZTextBox;
					AssertEquals("Trigger's trigger-condition should be human readable", "Trigger Condition", triggerConditionTextBox.GetExtension<LabelCaptionRenderer>().Caption);

					var triggerConditionValueTextBox = form.Controls.Find("TriggerConditionValueTextBox", true)[0] as ZTextBox;
					AssertEquals("Trigger's trigger-condition-value should be human readable", "Trigger Condition Value", triggerConditionValueTextBox.GetExtension<LabelCaptionRenderer>().Caption);

					var taskIDTextBox = form.Controls.Find("TaskIDTextBox", true)[0] as ZTextBox;
					AssertEquals("Task ID", "Task ID", taskIDTextBox.GetExtension<LabelCaptionRenderer>().Caption);

					var wteLogsGrid = form.Controls.Find("WTELogsGrid", true)[0] as ZGrid;
					var assertMessage = "WTE-logs-grid '{0}' columm-caption should be human readable";
					AssertEquals(string.Format(assertMessage, "Canceled"), "Canceled", wteLogsGrid.GetColumnCaption(StmALogSchema.SL_IsCancelled.Name));
					AssertEquals(string.Format(assertMessage, "Posted Time (UTC)"), "Posted Time (UTC)", wteLogsGrid.GetColumnCaption(StmALogSchema.SL_PostedTimeUtc.Name));
					AssertEquals(string.Format(assertMessage, "Event Time"), "Event Time", wteLogsGrid.GetColumnCaption(StmALogSchema.SL_EventTime.Name));
					AssertEquals(string.Format(assertMessage, "Event Time (UTC)"), "Event Time (UTC)", wteLogsGrid.GetColumnCaption(StmALogSchema.SL_EventTimeUtc.Name));
					AssertEquals(string.Format(assertMessage, "Event Time (Local)"), "Event Time (Local)", wteLogsGrid.GetColumnCaption("EventLocalBranchTime"));
					AssertEquals(string.Format(assertMessage, "Staff Code"), "Staff Code", wteLogsGrid.GetColumnCaption(StmALogSchema.SL_GS_NKUser.Name));
					AssertEquals(string.Format(assertMessage, "Branch"), "Branch", wteLogsGrid.GetColumnCaption(StmALogSchema.SL_GB_NKBranch.Name));
					AssertEquals(string.Format(assertMessage, "Triggered Branch"), "Triggered Branch", wteLogsGrid.GetColumnCaption("SL_TriggeredBranch"));
					AssertEquals(string.Format(assertMessage, "Department"), "Department", wteLogsGrid.GetColumnCaption(StmALogSchema.SL_GE_NKDepartment.Name));
					AssertEquals(string.Format(assertMessage, "Fire Workflow"), "Fire Workflow", wteLogsGrid.GetColumnCaption(StmALogSchema.SL_FireWorkflow.Name));
					AssertEquals(string.Format(assertMessage, "Company"), "Company", wteLogsGrid.GetColumnCaption("CompanyCode"));

					var triggeringLogGrid = form.Controls.Find("SourceLogsGrid", true)[0] as ZGrid;
					assertMessage = "Triggering-logs-grid '{0}' column-caption should be human readable";
					AssertEquals(string.Format(assertMessage, "Table"), "Table", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_Table.Name));
					AssertEquals(string.Format(assertMessage, "Canceled"), "Canceled", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_IsCancelled.Name));
					AssertEquals(string.Format(assertMessage, "Reference"), "Reference", triggeringLogGrid.GetColumnCaption("SL_ReferenceForBinding"));
					AssertEquals(string.Format(assertMessage, "Posted Time (UTC)"), "Posted Time (UTC)", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_PostedTimeUtc.Name));
					AssertEquals(string.Format(assertMessage, "Event Time"), "Event Time", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_EventTime.Name));
					AssertEquals(string.Format(assertMessage, "Event Time (UTC)"), "Event Time (UTC)", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_EventTimeUtc.Name));
					AssertEquals(string.Format(assertMessage, "Event Time (Local)"), "Event Time (Local)", triggeringLogGrid.GetColumnCaption("EventLocalBranchTime"));
					AssertEquals(string.Format(assertMessage, "Staff Code"), "Staff Code", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_GS_NKUser.Name));
					AssertEquals(string.Format(assertMessage, "Event Code"), "Event Code", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_SE_NKEvent.Name));
					AssertEquals(string.Format(assertMessage, "Branch"), "Branch", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_GB_NKBranch.Name));
					AssertEquals(string.Format(assertMessage, "Department"), "Department", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_GE_NKDepartment.Name));
					AssertEquals(string.Format(assertMessage, "Fire Workflow"), "Fire Workflow", triggeringLogGrid.GetColumnCaption(StmALogSchema.SL_FireWorkflow.Name));
					AssertEquals(string.Format(assertMessage, "Source"), "Source", triggeringLogGrid.GetColumnCaption("SL_TableFriendlyName"));

					var jobQueueGrid = form.Controls.Find("JobQueuesGrid", true)[0] as ZGrid;
					assertMessage = "Job-queue-grid '{0}' column-caption should be human readable";
					AssertEquals(string.Format(assertMessage, "Status"), "Status", jobQueueGrid.GetColumnCaption(StmJobQueueSchema.SJ_Status.Name));
					AssertEquals(string.Format(assertMessage, "Posted Time (UTC)"), "Posted Time (UTC)", jobQueueGrid.GetColumnCaption(StmJobQueueSchema.SJ_PostedTimeUtc.Name));
					AssertEquals(string.Format(assertMessage, "Event Time"), "Event Time", jobQueueGrid.GetColumnCaption(StmJobQueueSchema.SJ_EventTime.Name));
					AssertEquals(string.Format(assertMessage, "Event Time (UTC)"), "Event Time (UTC)", jobQueueGrid.GetColumnCaption(StmJobQueueSchema.SJ_EventTimeUtc.Name));
					AssertEquals(string.Format(assertMessage, "Event Time (Local)"), "Event Time (Local)", jobQueueGrid.GetColumnCaption("EventLocalBranchTime"));
					AssertEquals(string.Format(assertMessage, "User"), "User", jobQueueGrid.GetColumnCaption(StmJobQueueSchema.SJ_GS_NKUser.Name));
					AssertEquals(string.Format(assertMessage, "Is Estimat"), "Is Estimate", jobQueueGrid.GetColumnCaption(StmJobQueueSchema.SJ_IsEstimate.Name));
					AssertEquals(string.Format(assertMessage, "Is Delay Fired"), "Is Delay Fired", jobQueueGrid.GetColumnCaption(StmJobQueueSchema.SJ_IsDelayFired.Name));
				});
			}
		}

		#region Implementation

		protected virtual DummyWithWorkflow GetDummy() => Factory.New<DummyWithWorkflow>();

		protected DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = GetDummy()); }
		}
		DummyWithWorkflow dummy;

		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					var viewModel = new WorkflowDiagnosticViewModel(Dummy.WorkflowItems.TriggersIncludingRelated[0]);
					form = new WorkflowDiagnosticForm(null);
					form.SetDataBinding(viewModel, "");
				}
				return form;
			}
		}

		ZChildForm form;

		protected override Form GetFormToBashCore()
		{
			return Form;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		static void AssertTriggerInformationShown(WorkflowDiagnosticForm form, ProcessTask trigger)
		{
			var descriptionTextBox = form.Controls.Find("DescriptionTextBox", true)[0] as TextBox;
			var actualStartTextBox = form.Controls.Find("ActualStartTextBox", true)[0] as TextBox;
			var eventTextBox = form.Controls.Find("EventTextBox", true)[0] as TextBox;
			var triggerFieldTextBox = form.Controls.Find("TriggerFieldTextBox", true)[0] as TextBox;
			var triggerConditionTextBox = form.Controls.Find("TriggerConditionTextBox", true)[0] as TextBox;
			var triggerConditionValueTextBox = form.Controls.Find("TriggerConditionValueTextBox", true)[0] as TextBox;
			var taskIDTextBox = form.Controls.Find("TaskIDTextBox", true)[0] as TextBox;

			AssertEquals("Trigger's description should match form", descriptionTextBox.Text, trigger.P9_Description);

			if (new ZString(actualStartTextBox.Text).IsEmpty)
			{
				Assert("Trigger's actual-Start has not been set", trigger.P9_ActualDate.IsEmpty);
			}
			else
			{
				var expectedActualStart = (DateTimeOffset)new DateTimeOffsetConverter().ConvertFromString(actualStartTextBox.Text);
				AssertEquals("Trigger's actual-Start should match form", expectedActualStart.DateTime, trigger.P9_ActualDateForBinding.ToZDateTime().ToSmallDateTimeFloor().ToDateTime());
			}

			AssertEquals("Trigger's event should match form", eventTextBox.Text, trigger.P9_SE_NKMilestoneEvent);
			AssertEquals("Trigger's trigger-field should match form", triggerFieldTextBox.Text, trigger.P9_TriggerField);
			AssertEquals("Trigger's trigger-condition should match form", triggerConditionTextBox.Text, trigger.TriggerConditions.TriggerCondition);
			AssertEquals("Trigger's trigger-condition-value should match form", triggerConditionValueTextBox.Text, trigger.TriggerConditions.TriggerConditionValue);
			AssertEquals("Trigger's event TaskID should match form", taskIDTextBox.Text, trigger.P9_TaskID);
		}

		static void AssertWTELogInformationShow(WorkflowDiagnosticForm form, ProcessTask trigger)
		{
			var wteLogsGrid = form.Controls.Find("WTELogsGrid", true)[0] as ZGrid;
			var gridWTELog = wteLogsGrid.ListManager.GetCurrent() as WTELogViewModel;

			var wteLog = ((WTELogViewModel)new WorkflowDiagnosticViewModel(trigger).WTELogs.Single()).WTELog;

			AssertEquals("WTE log's is-estimate field should match form", gridWTELog.SL_IsEstimate, wteLog.SL_IsEstimate);
			AssertEquals("WTE log's is-cancelled field should match form", gridWTELog.SL_IsCancelled, wteLog.SL_IsCancelled);
			AssertEquals("WTE log's posted-time-UTC field should match form", gridWTELog.SL_PostedTimeUtc, wteLog.SL_PostedTimeUtc);
			AssertEquals("WTE log's event-time field should match form", gridWTELog.SL_EventTime, wteLog.SL_EventTime);
			AssertEquals("WTE log's event-time-UTC field should match form", gridWTELog.SL_EventTimeUtc, wteLog.SL_EventTimeUtc);
			AssertEquals("WTE log's event-time-Local field should match form", gridWTELog.EventLocalBranchTime, wteLog.EventLocalBranchTime);
			AssertEquals("WTE log's user field should match form", gridWTELog.SL_GS_NKUser, wteLog.SL_GS_NKUser);
			AssertEquals("WTE log's branch field should match form", gridWTELog.SL_GB_NKBranch, wteLog.SL_GB_NKBranch);
			AssertEquals("WTE log's department field should match form", gridWTELog.SL_GE_NKDepartment, wteLog.SL_GE_NKDepartment);
			AssertEquals("WTE log's fire-workflow field should match form", gridWTELog.SL_FireWorkflow, wteLog.SL_FireWorkflow);
			AssertEquals("WTE log's branch should match current branch", Env.CurrentBranch.Code, gridWTELog.SL_TriggeredBranch);
			AssertEquals("WTE log's company should match current company", Env.CurrentCompany.Code, gridWTELog.CompanyCode);
		}

		static void AssertTriggeringLogInformationShow(WorkflowDiagnosticForm form, IStmALogParent parent)
		{
			var sourceLogsGrid = form.Controls.Find("sourceLogsGrid", true)[0] as ZGrid;
			var gridSourceLog = sourceLogsGrid.ListManager.GetCurrent() as StmALog;

			var athLog = parent.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.AuthorisedCode).SingleOrDefault();

			AssertEquals("Triggering log's is-estimate field should match form", gridSourceLog.SL_Table, athLog.SL_Table);
			AssertEquals("Triggering log's is-estimate field should match form", gridSourceLog.SL_IsEstimate, athLog.SL_IsEstimate);
			AssertEquals("Triggering log's is-cancelled field should match form", gridSourceLog.SL_IsCancelled, athLog.SL_IsCancelled);
			AssertEquals("Triggering log's reference field should match form", gridSourceLog.SL_Reference, athLog.SL_Reference);
			AssertEquals("Triggering log's posted-time-UTC field should match form", gridSourceLog.SL_PostedTimeUtc, athLog.SL_PostedTimeUtc);
			AssertEquals("Triggering log's event-time field should match form", gridSourceLog.SL_EventTime, athLog.SL_EventTime);
			AssertEquals("Triggering log's event-time-UTC field should match form", gridSourceLog.SL_EventTimeUtc, athLog.SL_EventTimeUtc);
			AssertEquals("Triggering log's event-time-Local field should match form", gridSourceLog.EventLocalBranchTime, athLog.EventLocalBranchTime);
			AssertEquals("Triggering log's user field should match form", gridSourceLog.SL_GS_NKUser, athLog.SL_GS_NKUser);
			AssertEquals("Triggering log's event field should match form", gridSourceLog.SL_SE_NKEvent, athLog.SL_SE_NKEvent);
			AssertEquals("Triggering log's branch field should match form", gridSourceLog.SL_GB_NKBranch, athLog.SL_GB_NKBranch);
			AssertEquals("Triggering log's department field should match form", gridSourceLog.SL_GE_NKDepartment, athLog.SL_GE_NKDepartment);
			AssertEquals("Triggering log's fire-workflow field should match form", gridSourceLog.SL_FireWorkflow, athLog.SL_FireWorkflow);
			AssertEquals("Triggering log's source field should match form", gridSourceLog.SL_TableFriendlyName, athLog.SL_TableFriendlyName);
		}

		static void AssertJobQueueInformationShow(WorkflowDiagnosticForm form, IQueuedLog jobQueue)
		{
			var jobQueueGrid = form.Controls.Find("JobQueuesGrid", true)[0] as ZGrid;
			var gridJobQueue = jobQueueGrid.ListManager.GetCurrent() as StmJobQueueViewModel;

			AssertEquals("Job queue's status field should match form", gridJobQueue.SJ_Status, jobQueue.SJ_Status);
			AssertEquals("Job queue's posted-time-UTC field should match form", gridJobQueue.SJ_PostedTimeUtc, jobQueue.SJ_PostedTimeUtc);
			AssertEquals("Job queue's event-time field should match form", gridJobQueue.SJ_EventTime, jobQueue.SJ_EventTime);
			AssertEquals("Job queue's user field should match form", gridJobQueue.SJ_GS_NKUser, jobQueue.SJ_GS_NKUser);
			AssertEquals("Job queue's is-delay-fired field should match form", gridJobQueue.SJ_IsDelayFired, jobQueue.SJ_IsDelayFired);
		}

		#endregion
	}
}
