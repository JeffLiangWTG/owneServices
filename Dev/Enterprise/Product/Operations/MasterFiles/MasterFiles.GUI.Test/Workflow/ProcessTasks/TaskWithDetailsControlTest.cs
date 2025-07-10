using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	sealed class TaskWithDetailsControlTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestBindTo_ShouldFindControlsWithinGroupBoxes()
		{
			using (var control = new TaskWithDetailsControl())
			{
				var startingBindTo = control.TasksControl.BindTo;
				control.BindTo = "TasksView";
				AssertEquals("Should set same BindTo as control", "TasksView", control.TasksControl.BindTo);

				var childControls = new IBindTo[]
				{
						control.NotesTextBox, control.GroupFindBox, control.CapabilityFindBox, control.ReminderCheckBox, control.SequenceCalcEdit,
						control.IDTextBox, control.DescriptionTextBox, control.ProcessHeaderDropEdit, control.StaffFindBox, control.TaskTypeDropEdit,
						control.StatusDropEdit, control.EstimateVariationFactorCalcEdit, control.ExtraResourcesGrid, control.SimpleActDurTimeEditEx,
						control.SimpleCapabilityGuidFindBox, control.SimpleDescriptionTextBox, control.SimpleEstVariationCalcEdit,
						control.SimpleGroupGuidFindBox, control.SimpleHighEstTimeEditEx, control.SimpleEstTimeEditEx,
						control.SimpleNotesRichTextBox, control.SimpleStaffCodeFindBox, control.SimpleStatusDropEdit,
						control.SimpleTypeDropEdit, control.SimpleWorkflowGuidDropEdit
				};

				foreach (IBindTo childControl in childControls)
				{
					AssertStartsWith("Child control BindTo should bet set", "TasksView.", childControl.BindTo);
				}
			}
		}

		[RequiresSTA]
		public void TestResize_ShouldExpandWorkflowDropdownsUpToFullWidth()
		{
			using (var form = new ZForm { Width = 1280, Height = 768 })
			using (var control = new TaskWithDetailsControl { Width = 100, Height = 100 })
			{
				form.Controls.Add(control);
				form.Show();

				control.DetailsTabControl.SelectedTab = control.SimpleViewTabPage;
				AssertNotEquals(400, control.SimpleWorkflowGuidDropEdit.Width);

				control.DetailsTabControl.SelectedTab = control.AdvancedViewTabPage;
				AssertNotEquals(400, control.ProcessHeaderDropEdit.Width);

				control.Size = new System.Drawing.Size(1280, 768);

				control.DetailsTabControl.SelectedTab = control.SimpleViewTabPage;
				AssertEquals(400, control.SimpleWorkflowGuidDropEdit.Width);
				control.DetailsTabControl.SelectedTab = control.AdvancedViewTabPage;
				AssertEquals(400, control.ProcessHeaderDropEdit.Width);
			}
		}

		[RequiresSTA]
		public void TestControlsCharacterCasingShouldBeUpperCase()
		{
			using (var control = new TaskWithDetailsControl())
			{
				AssertEquals(CharacterCasing.Upper, control.StatusDropEdit.CharacterCasing);
				AssertEquals(CharacterCasing.Upper, control.TaskTypeDropEdit.CharacterCasing);
				AssertEquals(CharacterCasing.Upper, control.SimpleStatusDropEdit.CharacterCasing);
				AssertEquals(CharacterCasing.Upper, control.SimpleTypeDropEdit.CharacterCasing);
			}
		}
	}
}
