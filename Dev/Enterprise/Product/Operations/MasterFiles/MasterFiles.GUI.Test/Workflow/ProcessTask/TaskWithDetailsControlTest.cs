using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class TaskWithDetailsControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestConstructor_ShouldNotThrowException_WhenOpenInVisualStudio()
		{
			var registryMock = new Mock<IBMSRegistry>();
			registryMock.SetupGet(m => m.IsPlanningManagementEnabled).Throws(new Exception("Oh no!"));
			ObjectFactory.Substitute(registryMock.Object);
			DesignModeFinder.SetIsDesigningForTest(true);
			TaskWithDetailsControl control = null;

			try
			{
				AssertNoExceptionThrown(() => control = new TaskWithDetailsControl());
			}
			finally
			{
				DesignModeFinder.SetIsDesigningForTest(false);
				control?.Dispose();
				control = null;
			}
		}

		[RequiresSTA]
		public void TestChangeStatusViaTaskTabGrid_ShouldSetTaskChangeModeToTGR()
		{
			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, ProcessTaskStatusChangeModeTracker.Current);

			var workflow = Factory.New<DummyWithWorkflow>();
			var task = workflow.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			using (var form = new ZForm(workflow) { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TaskWithDetailsControl())
			{
				control.BindTo = nameof(workflow.WorkflowItems);
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var taskGrid = control.TasksControl.TasksGrid;
				var statusColumn = taskGrid.Columns.Single(c => c.ColumnName == ProcessTasksSchema.P9_Status.Name);
				var statusColumnStyle = statusColumn.ColumnStyle as ZCustomControlColumnStyle;

				taskGrid.BeginEdit(statusColumnStyle, 0);
				var zdropEdit = statusColumnStyle.EditControl as ZDropEdit;
				zdropEdit.SelectItem(ProcessTaskStatusCodeList.Codes.Working);

				AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.TasksTabGrid, ProcessTaskStatusChangeModeTracker.Current);

				taskGrid.EndEdit(statusColumn.ColumnStyle, 0, false);

				Application.DoEvents();
				Factory.Save();
			}

			var lastLogReference = task.Logs.MostRecentLogByEventTime(Events.StatusChange).SL_Reference;

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, ProcessTaskStatusChangeModeTracker.Current);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.TasksTabGrid}", lastLogReference);
		}

		[RequiresSTA]
		public void TestChangeStatusViaTaskTabGrid_WhenChangeAffectOtherTask_ShouldSetTaskChangeModeToTGR_AndOtherTaskToOTT()
		{
			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, ProcessTaskStatusChangeModeTracker.Current);

			var workflow = Factory.New<DummyWithWorkflow>();
			var otherTask = workflow.WorkflowItems.AddNew();
			otherTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			otherTask.P9_Sequence = 1;
			otherTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var taskToChange = workflow.WorkflowItems.AddNew();
			taskToChange.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			taskToChange.P9_Sequence = 2;
			taskToChange.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			using (var form = new ZForm(workflow) { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TaskWithDetailsControl())
			{
				control.BindTo = nameof(workflow.WorkflowItems);
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var taskGrid = control.TasksControl.TasksGrid;
				taskGrid.ListManager.Position = 1;
				var statusColumn = taskGrid.Columns.Single(c => c.ColumnName == ProcessTasksSchema.P9_Status.Name);
				var statusColumnStyle = statusColumn.ColumnStyle as ZCustomControlColumnStyle;

				taskGrid.BeginEdit(statusColumnStyle, 1);
				var zdropEdit = statusColumnStyle.EditControl as ZDropEdit;
				zdropEdit.SelectItem(ProcessTaskStatusCodeList.Codes.Working);

				AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.TasksTabGrid, ProcessTaskStatusChangeModeTracker.Current);

				taskGrid.EndEdit(statusColumn.ColumnStyle, 1, false);

				taskGrid.ListManager.Position = 0;

				Application.DoEvents();
				Factory.Save();
			}

			var otherTaskLastLogReference = otherTask.Logs.MostRecentLogByEventTime(Events.StatusChange).SL_Reference;
			var taskToChangeLastLogReference = taskToChange.Logs.MostRecentLogByEventTime(Events.StatusChange).SL_Reference;

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, ProcessTaskStatusChangeModeTracker.Current);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, otherTask.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, taskToChange.P9_Status);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.TasksTabGrid}", taskToChangeLastLogReference);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.OtherTask}", otherTaskLastLogReference);
		}

		[RequiresSTA]
		public void TestWhenChangeStatusViaTaskTabDetailsSimpleView_ShouldSetChangeModeToTDS()
		{
			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, ProcessTaskStatusChangeModeTracker.Current);

			var workflow = Factory.New<DummyWithWorkflow>();
			var user = Factory.New<GlbStaff>();
			var task = workflow.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskWithDetailsControl())
			{
				control.BindTo = nameof(workflow.WorkflowItems);
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var statusDropEdit = control.FindSingle<ZDropEdit>(c => c.Name == "SimpleStatusDropEdit");

				statusDropEdit.Focus();
				statusDropEdit.SelectItem(ProcessTaskStatusCodeList.Codes.Working);

				AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.TasksTabDetailsSimpleView, ProcessTaskStatusChangeModeTracker.Current);

				statusDropEdit.CommitBoundValue();
				control.TasksControl.TasksGrid.Focus();

				Application.DoEvents();
				Factory.Save();
			}

			var lastLogReference = task.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, ProcessTaskStatusChangeModeTracker.Current);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.TasksTabDetailsSimpleView}", lastLogReference);
		}

		[RequiresSTA]
		public void TestWhenChangeStatusViaTaskTabDetailsAdvancedView_ShouldSetChangeModeToTDA()
		{
			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, ProcessTaskStatusChangeModeTracker.Current);

			var workflow = Factory.New<DummyWithWorkflow>();
			var user = Factory.New<GlbStaff>();
			var task = workflow.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskWithDetailsControl())
			{
				control.BindTo = nameof(workflow.WorkflowItems);
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				control.DetailsTabControl.SelectedIndex = 1;
				var statusDropEdit = control.FindSingle<ZDropEdit>(c => c.Name == "StatusDropEdit");
				statusDropEdit.Focus();
				statusDropEdit.SelectItem(ProcessTaskStatusCodeList.Codes.Working);

				AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.TasksTabDetailsAdvancedView, ProcessTaskStatusChangeModeTracker.Current);

				statusDropEdit.CommitBoundValue();
				control.TasksControl.TasksGrid.Focus();

				Application.DoEvents();
				Factory.Save();
			}

			var lastLogReference = task.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, ProcessTaskStatusChangeModeTracker.Current);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.TasksTabDetailsAdvancedView}", lastLogReference);
		}

		[RequiresSTA]
		public void TestSpellChecker()
		{
			const string CheckSpellingMenuKey = "checkSpelling";
			var workflow = Factory.New<DummyWithWorkflow>();
			var task = workflow.WorkflowItems.AddNew();

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskWithDetailsControl())
			{
				control.BindTo = nameof(workflow.WorkflowItems);
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotNull("Spell checker should be initialized for advanced note rich text box",
						control.NotesTextBox.GetRichTextBoxForTest().ContextMenuStrip?.Items[CheckSpellingMenuKey]);

					AssertNotNull("Spell checker should be initialized for simple note rich text box",
						control.SimpleNotesRichTextBox.GetRichTextBoxForTest().ContextMenuStrip?.Items[CheckSpellingMenuKey]);
				});
			}
		}

		#region Implementation

		ProcessTaskStatusChangeModeTracker ProcessTaskStatusChangeModeTracker => ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();

		protected override void SetUp()
		{
			base.SetUp();
			ProcessTaskStatusChangeModeTracker.Clear();
		}

		#endregion
	}
}

