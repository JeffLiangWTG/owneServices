using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(ZForm))]
	sealed class TaskWithDetailsAndFilterControlTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFiltering()
		{
			using (ZForm form = new ZForm(Dummy))
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				TaskWithDetailsAndFilterControl control = AddAndReturnNewControl(form);
				form.Show();

				ProcessTask task = control.TasksView.TasksView.AddNew();
				task.P9_Status = "ABC";

				control.TasksView.P9_Status = "X";
				control.FilterButton.PerformClick();
				AssertEquals("Task should be shown.", false, control.TasksView.TasksView.Contains(task));

				control.TasksView.P9_Status = "ABC";
				control.FilterButton.PerformClick();
				AssertEquals("Task should not be shown.", true, control.TasksView.TasksView.Contains(task));

				control.ClearButton.PerformClick();
				AssertEquals("Filters should be cleared.", "", control.TasksView.P9_Status);
			}
		}

		[RequiresSTA]
		public void TestBindToFilter()
		{
			using (TaskWithDetailsAndFilterControl control = new TaskWithDetailsAndFilterControl())
			{
				AssertEquals("StatusDropEdit.BindTo", "P9_Status", control.StatusDropEdit.BindTo);
				AssertEquals("TypeDropEdit.BindTo", "P9_Type", control.TypeDropEdit.BindTo);
				AssertEquals("StaffFindbox.BindTo", "P9_GS_NKAssignedStaffMember", control.StaffFindbox.BindTo);
				AssertEquals("GroupFindbox.BindTo", "P9_GG_AssignedGroup", control.GroupFindbox.BindTo);

				control.BindToFilter = "Apple";
				AssertEquals("StatusDropEdit.BindTo", "Apple.P9_Status", control.StatusDropEdit.BindTo);
				AssertEquals("TypeDropEdit.BindTo", "Apple.P9_Type", control.TypeDropEdit.BindTo);
				AssertEquals("StaffFindbox.BindTo", "Apple.P9_GS_NKAssignedStaffMember", control.StaffFindbox.BindTo);
				AssertEquals("GroupFindbox.BindTo", "Apple.P9_GG_AssignedGroup", control.GroupFindbox.BindTo);

				control.BindToFilter = "Orange";
				AssertEquals("StatusDropEdit.BindTo", "Orange.P9_Status", control.StatusDropEdit.BindTo);
				AssertEquals("TypeDropEdit.BindTo", "Orange.P9_Type", control.TypeDropEdit.BindTo);
				AssertEquals("StaffFindbox.BindTo", "Orange.P9_GS_NKAssignedStaffMember", control.StaffFindbox.BindTo);
				AssertEquals("GroupFindbox.BindTo", "Orange.P9_GG_AssignedGroup", control.GroupFindbox.BindTo);
			}
		}

		[RequiresSTA]
		public void TestControlsCharacterCasingShouldBeUpperCase()
		{
			using (var control = new TaskWithDetailsAndFilterControl())
			{
				AssertEquals(CharacterCasing.Upper, control.StatusDropEdit.CharacterCasing);
				AssertEquals(CharacterCasing.Upper, control.TypeDropEdit.CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestTasksGrid()
		{
			using (TaskWithDetailsAndFilterControl control = new TaskWithDetailsAndFilterControl())
			{
				AssertEquals("TasksGrid", control.TasksControl.TasksControl.TasksGrid, control.TasksGrid);
			}
		}

		[RequiresSTA]
		public void TestNavigateToWorkflowItem()
		{
			using (ZForm form = new ZForm(Dummy))
			using (TaskWithDetailsAndFilterControl control = new TaskWithDetailsAndFilterControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(control);

				form.Show();
				Application.DoEvents();
				control.SetDataBinding(Dummy.WorkflowItems.Exceptions, "");

				ProcessTask task1 = Dummy.WorkflowItems.Tasks.AddNew();
				ProcessTask task2 = Dummy.WorkflowItems.Tasks.AddNew();

				control.TasksGrid.List.Add(task1);
				control.TasksGrid.List.Add(task2);

				control.NavigateToWorkflowItem(task2);
				Assert(control.TasksGrid.ListManager.GetCurrent() == task2);

				control.NavigateToWorkflowItem(task1);
				Assert(control.TasksGrid.ListManager.GetCurrent() == task1);
			}
		}

		[RequiresSTA]
		public void TestDataBinding_WhenBMSEnabled_ShouldSetDefaultWorkflowForTasksWithoutWorkflow()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(Dummy, Factory);
			var defaultWorkflow = jobHeader.ProcessHeaders[0];
			var taskWithWorkflow = Dummy.WorkflowItems.AddNew();
			var someWorkflowPK = ZGuid.NewZGuid();
			taskWithWorkflow.P9_FH_ProcessHeader = someWorkflowPK;
			var taskWithoutWorkflow1 = Dummy.WorkflowItems.AddNew();
			taskWithoutWorkflow1.P9_FH_ProcessHeader = ZGuid.Empty;
			var taskWithoutWorkflow2 = Dummy.WorkflowItems.AddNew();
			taskWithoutWorkflow2.P9_FH_ProcessHeader = ZGuid.Empty;

			using (var control = new TaskWithDetailsAndFilterControl())
			{
				control.SetDataBinding(Dummy, "");
			}

			AssertEquals(someWorkflowPK, taskWithWorkflow.P9_FH_ProcessHeader);
			AssertEquals(defaultWorkflow.PK, taskWithoutWorkflow1.P9_FH_ProcessHeader);
			AssertEquals(defaultWorkflow.PK, taskWithoutWorkflow2.P9_FH_ProcessHeader);

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;

			taskWithoutWorkflow1.P9_FH_ProcessHeader = ZGuid.Empty;
			taskWithoutWorkflow2.P9_FH_ProcessHeader = ZGuid.Empty;

			using (var control = new TaskWithDetailsAndFilterControl())
			{
				control.SetDataBinding(Dummy, "");
			}

			AssertEquals(someWorkflowPK, taskWithWorkflow.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, taskWithoutWorkflow1.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, taskWithoutWorkflow2.P9_FH_ProcessHeader);
		}

		[RequiresSTA]
		public void TestTextBoxAndTaskGridAreProvidedForITaskDetailsControl()
		{
			using (var control = new TaskWithDetailsAndFilterControl())
			{
				Assert(control is ITaskDetailsControl);

				if (control is ITaskDetailsControl taskDetailsControl)
				{
					AssertNotNull(taskDetailsControl.NotesRichTextBox);
					AssertNotNull(taskDetailsControl.TasksGrid);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ZForm result = new ZForm(Dummy);
			result.CaptionRenderingEnabled = true;
			AddAndReturnNewControl(result);
			result.ControllerID = Enterprise.ZArchitecture.Modules.Testing.DummyControllerIDs.Dummy;
			return result;
		}

		TaskWithDetailsAndFilterControl AddAndReturnNewControl(ZForm form)
		{
			TaskWithDetailsAndFilterControl control = new TaskWithDetailsAndFilterControl();

			control.BindToFilter = "";
			control.BindToGrid = "TasksView";
			control.SetDataBinding(Dummy, "");
			form.Height = control.Height + 100;
			form.Width = control.Width + 100;
			form.Controls.Add(control);

			return control;
		}

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;

		#endregion
	}
}
