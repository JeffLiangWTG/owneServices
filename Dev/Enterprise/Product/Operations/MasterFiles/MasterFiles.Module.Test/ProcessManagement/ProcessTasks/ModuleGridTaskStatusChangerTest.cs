using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ModuleGridTaskStatusChangerTest : TestCaseWithFactory
	{
		// Called from individual modules
		public static void AssertTaskStatusChangerExistsOnFilterControl(ZFilterStripControl filterControl)
		{
			filterControl.FilteredGrid.ContextMenu.OnPopup_ForTest();

			MenuItem tasksMenuItem = null;
			MenuItem myTasksMenuItem = null;

			foreach (MenuItem item in filterControl.FilteredGrid.ContextMenu.MenuItems)
			{
				if (item.Text == "Tasks")
				{
					tasksMenuItem = item;
				}

				if (item.Text == GlbStaff.CurrentUser.GS_FullName + "'s Tasks")
				{
					myTasksMenuItem = item;
				}
			}

			AssertNotNull(tasksMenuItem);
			AssertNotNull(myTasksMenuItem);
		}

		public void TestTaskValidationWhenAttemptingToWorkOnAssignedTask()
		{
			using (ZForm form = new ZForm(DummyWithManyTasks))
			using (ZGrid grid = new ZGrid())
			{
				grid.BindTo = "DummyWithWorkflows";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				form.Show();
				grid.SetDataBinding(form.BusinessEntity, "DummyWithWorkflows");
				grid.Select(0);

				var changer = new ModuleGridTaskStatusChanger(grid);
				changer.Initialise();
				grid.ContextMenu.OnPopup_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var taskMenuItem = grid.ContextMenu.MenuItems.OfType<MenuItem>().Single(m => m.Text == "Tasks");

				var menuItem = taskMenuItem.MenuItems.OfType<MenuItem>().Single(m => m.Text == "6. Task to test working validation - Unassigned (Assigned)");
				menuItem.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();

				AssertEquals($"There are errors that need to be corrected before this {AssignedNoUserTask.HumanReadableName} can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTaskValidationWhenAttemptingToResumeSuspendedTask()
		{
			using (ZForm form = new ZForm(DummyWithManyTasks))
			using (ZGrid grid = new ZGrid())
			{
				grid.BindTo = "DummyWithWorkflows";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				form.Show();
				grid.SetDataBinding(form.BusinessEntity, "DummyWithWorkflows");
				grid.Select(0);

				var changer = new ModuleGridTaskStatusChanger(grid);
				changer.Initialise();
				grid.ContextMenu.OnPopup_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var taskMenuItem = grid.ContextMenu.MenuItems.OfType<MenuItem>().Single(m => m.Text == "Tasks");

				var menuItem = taskMenuItem.MenuItems.OfType<MenuItem>().Single(m => m.Text == "7. Task to test suspended validation - Unassigned (Suspended (Temporary Pause))");
				menuItem.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();

				AssertEquals($"There are errors that need to be corrected before this {SuspendedNoUserTask.HumanReadableName} can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTaskValidationWhenAttemptingToCloseWorkingTask()
		{
			using (ZForm form = new ZForm(DummyWithManyTasks))
			using (ZGrid grid = new ZGrid())
			{
				grid.BindTo = "DummyWithWorkflows";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				form.Show();
				grid.SetDataBinding(form.BusinessEntity, "DummyWithWorkflows");
				grid.Select(0);

				var changer = new ModuleGridTaskStatusChanger(grid);
				changer.Initialise();
				grid.ContextMenu.OnPopup_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var taskMenuItem = grid.ContextMenu.MenuItems.OfType<MenuItem>().Single(m => m.Text == "Tasks");
				var menuItem = taskMenuItem.MenuItems.OfType<MenuItem>().Single(m => m.Text == "8. Task to test closing validation - Unassigned (Working)");
				menuItem.PerformClick();
				changer.NextResultForTaskClosedFormTest = DialogResult.Yes;
				menuItem.PerformClick();

				AssertEquals($"There are errors that need to be corrected before this {ClosedNoUserTask.HumanReadableName} can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConstructorThrowsNulLRefOnNullGrid()
		{
			bool exceptionThrown = false;
			using (ZGrid grid = new ZGrid())
			{
				var changer = new ModuleGridTaskStatusChanger(grid);
				try
				{
					changer = new ModuleGridTaskStatusChanger(null);
				}
				catch (ArgumentNullException)
				{
					exceptionThrown = true;
				}
				AssertEquals(true, exceptionThrown);
			}
		}

		public void TestMenuItems_NoTasks()
		{
			DummyParent dummy = Factory.New<DummyParent>();
			dummy.DummyWithWorkflows.AddNew();

			using (ZForm form = new ZForm(dummy))
			using (ZGrid grid = new ZGrid())
			{
				grid.BindTo = "DummyWithWorkflows";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				form.Show();
				grid.SetDataBinding(dummy, "DummyWithWorkflows");
				grid.Select(0);

				new ModuleGridTaskStatusChanger(grid).Initialise();
				grid.ContextMenu.OnPopup_ForTest();

				MenuItem tasksMenuItem = null;
				MenuItem myTasksMenuItem = null;

				foreach (MenuItem item in grid.ContextMenu.MenuItems)
				{
					if (item.Text == "Tasks")
					{
						tasksMenuItem = item;
					}

					if (item.Text == GlbStaff.CurrentUser.GS_FullName + "'s Tasks")
					{
						myTasksMenuItem = item;
					}
				}

				AssertNotNull(tasksMenuItem);
				AssertNotNull(myTasksMenuItem);
				AssertEquals(1, tasksMenuItem.MenuItems.Count);
				AssertEquals("No tasks for this job", tasksMenuItem.MenuItems[0].Text);
				AssertEquals(false, tasksMenuItem.MenuItems[0].Enabled);
				AssertEquals(1, myTasksMenuItem.MenuItems.Count);
				AssertEquals("No tasks for " + GlbStaff.CurrentUser.GS_FullName + "", myTasksMenuItem.MenuItems[0].Text);
				AssertEquals(false, myTasksMenuItem.MenuItems[0].Enabled);
			}
		}

		[RequiresSTA]
		public void TestWorkflowSupportableHasTaskMenu()
		{
			using (var module = new DummyModuleSupportWorkFlowForTest())
			{
				using (var popup = module.ShowPopup())
				{
					Application.DoEvents();

					module.GridCollection.AddNew().FillWithValidTestData();
					module.GridCollection.AddNew().FillWithValidTestData();
					module.GridCollection.Factory.Save();

					using (var grid = module.DisplayGrid)
					{
						grid.Select(0);
						grid.ContextMenu.OnPopup_ForTest();

						Assert(grid.ContextMenu.MenuItems.Cast<MenuItem>().Any(m => m.Name == "ModuleGridTaskStatusChanger_MenuItemTask"));

						grid.SelectAllElements();
						grid.ContextMenu.OnPopup_ForTest();

						Assert(!grid.ContextMenu.MenuItems.Cast<MenuItem>().Any(m => m.Name == "ModuleGridTaskStatusChanger_MenuItemTask"));
					}
				}
			}
		}

		[RequiresSTA]
		public void TestWorkflowNotSupportableHasNotTaskMenu()
		{
			using (var module = new DummyModuleDoesNotSupportWorkFlowForTest())
			{
				using (var popup = module.ShowPopup())
				{
					Application.DoEvents();

					module.GridCollection.AddNew().FillWithValidTestData();
					module.GridCollection.AddNew().FillWithValidTestData();
					module.GridCollection.Factory.Save();

					using (var grid = module.DisplayGrid)
					{
						grid.Select(0);
						grid.ContextMenu.OnPopup_ForTest();

						Assert(!grid.ContextMenu.MenuItems.Cast<MenuItem>().Any(m => m.Name == "ModuleGridTaskStatusChanger_MenuItemTask"));
					}
				}
			}
		}

		public void TestChangeStatusViaTaskMenu_ShouldSetTaskStatusChangeModeToTMU()
		{
			using (ZForm form = new ZForm(DummyWithManyTasks))
			using (ZGrid grid = new ZGrid())
			{
				grid.BindTo = "DummyWithWorkflows";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				form.Show();
				grid.SetDataBinding(form.BusinessEntity, "DummyWithWorkflows");
				grid.Select(0);

				var changer = new ModuleGridTaskStatusChanger(grid);
				changer.Initialise();
				grid.ContextMenu.OnPopup_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var taskMenuItem = grid.ContextMenu.MenuItems.OfType<MenuItem>().Single(m => m.Text == "Tasks");

				var menuItem = taskMenuItem.MenuItems.OfType<MenuItem>().Single(m => m.Text.Contains(AssignedCurrentUserTask.P9_Description));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();

				var eventRef = AssignedCurrentUserTask.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, AssignedCurrentUserTask.P9_Status);
				AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu}", eventRef);

				menuItem = taskMenuItem.MenuItems.OfType<MenuItem>().Single(m => m.Text.Contains(WorkingOtherUserTask.P9_Description));
				changer.NextResultForTaskClosedFormTest = DialogResult.Yes;
				menuItem.PerformClick();

				eventRef = WorkingOtherUserTask.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, WorkingOtherUserTask.P9_Status);
				AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu}", eventRef);

				menuItem = taskMenuItem.MenuItems.OfType<MenuItem>().Single(m => m.Text.Contains(SuspendedOtherUserTask.P9_Description));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();

				eventRef = SuspendedOtherUserTask.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, SuspendedOtherUserTask.P9_Status);
				AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu}", eventRef);
			}
		}

		class DummyModuleDoesNotSupportWorkFlowForTest : DummyFilterGridModule
		{
			public override bool SupportsWorkflow => false;
		}

		class DummyModuleSupportWorkFlowForTest : DummyFilterGridModule
		{
			public override bool SupportsWorkflow => true;
		}

		public void TestMenuItems_MultipleWorkflowProviderSelectedShouldNotDisplay_TaskMenuItem()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Mr Harbourside Mansion";
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_FullName = "Lyin' Ted";

			var dummy = Factory.New<DummyParentWithMixedChildren>();
			var child1 = Factory.New<DummyWithWorkflow>();
			var child2 = Factory.New<DummyWithWorkflow>();

			child1.Z0_Description = "Sean Spicer";
			child2.Z0_Description = "Malcolm Trumble";

			var task1_1 = child1.WorkflowItems.Tasks.AddNew();
			var task1_2 = child1.WorkflowItems.Tasks.AddNew();

			var task2_1 = child2.WorkflowItems.Tasks.AddNew();
			var task2_2 = child2.WorkflowItems.Tasks.AddNew();

			task1_1.P9_Description = "Fake News";
			task1_2.P9_Description = "Real News";
			task1_1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task1_2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;

			task2_1.P9_Description = "Worst call ever";
			task2_2.P9_Description = "Dumb deal";
			task2_1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2_2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;

			dummy.DummiesWithAndOrWithoutWorkflow.Add(child1);
			dummy.DummiesWithAndOrWithoutWorkflow.Add(child2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "DummiesWithAndOrWithoutWorkflow";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();

				Application.DoEvents();

				new ModuleGridTaskStatusChanger(grid).Initialise();
				grid.SelectAllElements();
				grid.ContextMenu.OnPopup_ForTest();

				AssertNull("No Task menu when several rows are selected ", grid.ContextMenu.MenuItems.FindByText("Tasks"));

				AssertNull("No User Task menu when several rows are selected ", grid.ContextMenu.MenuItems.FindByText("Mr Harbourside Mansion's Tasks"));
			}
		}

		public void TestMenuItems_SingleNonWorkflowProviderSelected()
		{
			var dummy = Factory.New<DummyParentWithMixedChildren>();
			var child1_withWorkflow = Factory.New<DummyWithWorkflow>();
			var child2_withoutWorkflow = Factory.New<DummyBusinessObject>();

			child1_withWorkflow.Z0_Description = "Adiaga";
			child2_withoutWorkflow.Z0_Description = "Badiaga";

			var task1 = child1_withWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Adiaga 2";

			dummy.DummiesWithAndOrWithoutWorkflow.Add(child1_withWorkflow);
			dummy.DummiesWithAndOrWithoutWorkflow.Add(child2_withoutWorkflow);

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "DummiesWithAndOrWithoutWorkflow";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();

				Application.DoEvents();

				new ModuleGridTaskStatusChanger(grid).Initialise();
				grid.Select(1);
				grid.ContextMenu.OnPopup_ForTest();

				var tasksMenuItem = grid.ContextMenu.MenuItems.FindByText("Tasks");
				var currentUserTasksMenuItem = grid.ContextMenu.MenuItems.FindByText("CargoWise Support's Tasks");

				AssertEquals(1, tasksMenuItem.MenuItems.Count);
				AssertEquals(1, currentUserTasksMenuItem.MenuItems.Count);

				AssertEquals("This record does not support Workflow", tasksMenuItem.MenuItems[0].Text);
				AssertEquals(false, tasksMenuItem.MenuItems[0].Enabled);

				AssertEquals("This record does not support Workflow", currentUserTasksMenuItem.MenuItems[0].Text);
				AssertEquals(false, currentUserTasksMenuItem.MenuItems[0].Enabled);
			}
		}

		public void TestMenuItems_BothWorkflowProviderAndNonWorkflowProviderSelectedShouldNotDisplay_TaskMenuItem()
		{
			var dummy = Factory.New<DummyParentWithMixedChildren>();
			var child1_withWorkflow = Factory.New<DummyWithWorkflow>();
			var child2_withoutWorkflow = Factory.New<DummyWithHumanReadableName>();

			child1_withWorkflow.Z0_Description = "Adiaga";
			child2_withoutWorkflow.Z0_Description = "Badiaga";

			var task1 = child1_withWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Adiaga 2";

			dummy.DummiesWithAndOrWithoutWorkflow.Add(child1_withWorkflow);
			dummy.DummiesWithAndOrWithoutWorkflow.Add(child2_withoutWorkflow);

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "DummiesWithAndOrWithoutWorkflow";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();

				Application.DoEvents();

				new ModuleGridTaskStatusChanger(grid).Initialise();
				grid.SelectAllElements();
				grid.ContextMenu.OnPopup_ForTest();

				AssertNull("No Task menu when several rows are selected ", grid.ContextMenu.MenuItems.FindByText("Tasks"));

				AssertNull("No Task menu when several rows are selected ", grid.ContextMenu.MenuItems.FindByText("CargoWise Support's Tasks"));
			}
		}

		public void TestMenuItems_NotDuplicatedOnManyPopups()
		{
			DummyParent dummy = Factory.New<DummyParent>();
			dummy.DummyWithWorkflows.AddNew();

			using (ZForm form = new ZForm(dummy))
			using (ZGrid grid = new ZGrid())
			{
				grid.BindTo = "DummyWithWorkflows";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				form.Show();
				grid.SetDataBinding(dummy, "DummyWithWorkflows");
				grid.Select(0);

				new ModuleGridTaskStatusChanger(grid).Initialise();
				grid.ContextMenu.OnPopup_ForTest();
				grid.ContextMenu.OnPopup_ForTest();
				grid.ContextMenu.OnPopup_ForTest();
				grid.ContextMenu.OnPopup_ForTest();

				int tasksMenuItemCount = 0;
				int myTasksMenuItemCount = 0;
				foreach (MenuItem item in grid.ContextMenu.MenuItems)
				{
					if (item.Text == "Tasks")
					{
						tasksMenuItemCount++;
					}

					if (item.Text == GlbStaff.CurrentUser.GS_FullName + "'s Tasks")
					{
						myTasksMenuItemCount++;
					}
				}
				AssertEquals(1, tasksMenuItemCount);
				AssertEquals(1, myTasksMenuItemCount);
			}
		}

		public void TestMenuItems_CorrectlyLoaded()
		{
			using (ZForm form = new ZForm(DummyWithManyTasks))
			using (ZGrid grid = new ZGrid())
			{
				grid.BindTo = "DummyWithWorkflows";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				form.Show();
				grid.SetDataBinding(form.BusinessEntity, "DummyWithWorkflows");
				grid.Select(0);

				new ModuleGridTaskStatusChanger(grid).Initialise();
				grid.ContextMenu.OnPopup_ForTest();

				MenuItem tasksMenuItem = null;
				MenuItem myTasksMenuItem = null;

				foreach (MenuItem item in grid.ContextMenu.MenuItems)
				{
					if (item.Text == "Tasks")
					{
						tasksMenuItem = item;
					}

					if (item.Text == GlbStaff.CurrentUser.GS_FullName + "'s Tasks")
					{
						myTasksMenuItem = item;
					}
				}

				AssertNotNull(tasksMenuItem);
				AssertEquals(7, tasksMenuItem.MenuItems.Count);
				AssertEquals("1. This is task 1 - Unassigned (Open Pending Allocation)", tasksMenuItem.MenuItems[0].Text);
				AssertEquals("2. This is task 2 - " + GlbStaff.CurrentUser.GS_FullName + " (Assigned)", tasksMenuItem.MenuItems[1].Text);
				AssertEquals("4. This is task 4 - Bugger (Suspended (Temporary Pause))", tasksMenuItem.MenuItems[2].Text);
				AssertEquals("5. This is task 5 - Zubin (Working)", tasksMenuItem.MenuItems[3].Text);
				AssertEquals("6. Task to test working validation - Unassigned (Assigned)", tasksMenuItem.MenuItems[4].Text);
				AssertEquals("7. Task to test suspended validation - Unassigned (Suspended (Temporary Pause))", tasksMenuItem.MenuItems[5].Text);
				AssertEquals("8. Task to test closing validation - Unassigned (Working)", tasksMenuItem.MenuItems[6].Text);

				AssertNotNull(myTasksMenuItem);
				AssertEquals(1, myTasksMenuItem.MenuItems.Count);
				AssertEquals("2. This is task 2 - " + GlbStaff.CurrentUser.GS_FullName + " (Assigned)", myTasksMenuItem.MenuItems[0].Text);
			}
		}

		public void TestMenuItems_CorrectHandlingOfStatusWithNoValidationErrors()
		{
			using (ZForm form = new ZForm(DummyWithManyTasks))
			using (ZGrid grid = new ZGrid())
			{
				grid.BindTo = "DummyWithWorkflows";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				form.Show();
				grid.SetDataBinding(form.BusinessEntity, "DummyWithWorkflows");
				grid.Select(0);

				var changer = new ModuleGridTaskStatusChanger(grid);
				changer.Initialise();
				grid.ContextMenu.OnPopup_ForTest();

				MenuItem tasksMenuItem = null;
				MenuItem myTasksMenuItem = null;

				foreach (MenuItem item in grid.ContextMenu.MenuItems)
				{
					if (item.Text == "Tasks")
					{
						tasksMenuItem = item;
					}

					if (item.Text == GlbStaff.CurrentUser.GS_FullName + "'s Tasks")
					{
						myTasksMenuItem = item;
					}
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				// Unassigned
				tasksMenuItem.MenuItems[0].PerformClick();
				AssertEquals("To manage this task, please open the Job and assign the task from the 'Workflow' page.", UnitTestUserNotification.Instance.LastMessage.Text);

				// Assigned
				tasksMenuItem.MenuItems[1].PerformClick();
				AssertEquals("This task is assigned to " + GlbStaff.CurrentUser.GS_Code + ". Do you want to change the status to Working?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, AssignedCurrentUserTask.P9_Status);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				tasksMenuItem.MenuItems[1].PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, AssignedCurrentUserTask.P9_Status);

				// Suspended
				tasksMenuItem.MenuItems[2].PerformClick();
				AssertEquals("This task is currently suspended by " + OtherStaff2.GS_Code + ". Do you want to resume the task?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, SuspendedOtherUserTask.P9_Status);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				tasksMenuItem.MenuItems[2].PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, SuspendedOtherUserTask.P9_Status);

				// Working
				tasksMenuItem.MenuItems[3].PerformClick();
				changer.NextResultForTaskClosedFormTest = DialogResult.No;
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, WorkingOtherUserTask.P9_Status);
				changer.LastShownTaskClosedForm.Dispose();

				changer.NextResultForTaskClosedFormTest = DialogResult.Yes;
				tasksMenuItem.MenuItems[3].PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, WorkingOtherUserTask.P9_Status);
				changer.LastShownTaskClosedForm.Dispose();

				// Working for Current User
				changer.NextResultForTaskClosedFormTest = DialogResult.Yes;
				myTasksMenuItem.MenuItems[0].PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, AssignedCurrentUserTask.P9_Status);
				changer.LastShownTaskClosedForm.Dispose();
			}
		}

		public void TestCloseWorkingTaskCore()
		{
			using (ZGrid grid = new ZGrid())
			{
				var changer = new ModuleGridTaskStatusChanger(grid);
				ProcessTask task = Factory.New<ProcessTask>();

				bool taskOwnerPasswordEventIsManaged = false;
				task.P9_StatusInfo.ValueChanged += delegate
				{ taskOwnerPasswordEventIsManaged = TaskOwnerPasswordEventManagerTest.IsManaged(task); };
				changer.CloseWorkingTaskCore(task);
				Assert("Should be managed when status is being changed to Closed", taskOwnerPasswordEventIsManaged);
				Assert("Should be disposed after the property value is changed / cancelled", !TaskOwnerPasswordEventManagerTest.IsManaged(task));
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			}
		}

		public void TestMenuItems_NotNavigateItemsMoreThanOnceOnSameListOnManyPopups()
		{
			var dummy = Factory.New<DummyParent>();
			dummy.DummyWithWorkflows.AddNew();

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "DummyWithWorkflows";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("SomeProperty", 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				form.Show();
				grid.SetDataBinding(dummy, "DummyWithWorkflows");
				grid.SelectAllElements();

				var moduleGrid = new ModuleGridTaskStatusChanger(grid);
				moduleGrid.Initialise();
				grid.ContextMenu.OnPopup_ForTest();
				grid.ContextMenu.OnPopup_ForTest();
				grid.ContextMenu.OnPopup_ForTest();

				AssertEquals(1, moduleGrid.CountSetupMenuAccessForTest);
			}
		}

		#region Implementation

		DummyParent DummyWithManyTasks
		{
			get
			{
				DummyParent dummy = Factory.New<DummyParent>();
				DummyWithWorkflow workflow = dummy.DummyWithWorkflows.AddNew();

				OpenTask = workflow.WorkflowItems.AddNew();
				OpenTask.P9_Description = "This is task 1";
				OpenTask.P9_Sequence = 1;
				OpenTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

				SuspendedOtherUserTask = workflow.WorkflowItems.AddNew();
				SuspendedOtherUserTask.P9_Description = "This is task 4";
				SuspendedOtherUserTask.P9_Sequence = 4;
				SuspendedOtherUserTask.P9_GS_NKAssignedStaffMember = OtherStaff2.GS_Code;
				SuspendedOtherUserTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

				AssignedCurrentUserTask = workflow.WorkflowItems.AddNew();
				AssignedCurrentUserTask.P9_Description = "This is task 2";
				AssignedCurrentUserTask.P9_Sequence = 2;
				AssignedCurrentUserTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				AssignedCurrentUserTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

				WorkingOtherUserTask = workflow.WorkflowItems.AddNew();
				WorkingOtherUserTask.P9_Description = "This is task 5";
				WorkingOtherUserTask.P9_Sequence = 5;
				WorkingOtherUserTask.P9_GS_NKAssignedStaffMember = OtherStaff.GS_Code;
				WorkingOtherUserTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				AssignedNoUserTask = workflow.WorkflowItems.AddNew();
				AssignedNoUserTask.P9_Description = "Task to test working validation";
				AssignedNoUserTask.P9_Sequence = 6;
				AssignedNoUserTask.P9_GS_NKAssignedStaffMember = null;
				AssignedNoUserTask.P9_GG_AssignedGroup = OtherGroup.GG_GC;
				AssignedNoUserTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

				SuspendedNoUserTask = workflow.WorkflowItems.AddNew();
				SuspendedNoUserTask.P9_Description = "Task to test suspended validation";
				SuspendedNoUserTask.P9_Sequence = 7;
				SuspendedNoUserTask.P9_GS_NKAssignedStaffMember = null;
				SuspendedNoUserTask.P9_GG_AssignedGroup = OtherGroup.GG_GC;
				SuspendedNoUserTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

				ClosedNoUserTask = workflow.WorkflowItems.AddNew();
				ClosedNoUserTask.P9_Description = "Task to test closing validation";
				ClosedNoUserTask.P9_Sequence = 8;
				ClosedNoUserTask.P9_GS_NKAssignedStaffMember = null;
				ClosedNoUserTask.P9_GG_AssignedGroup = OtherGroup.GG_GC;
				ClosedNoUserTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				Factory.Save();

				return dummy;
			}
		}

		ProcessTask OpenTask;
		ProcessTask AssignedCurrentUserTask;
		ProcessTask SuspendedOtherUserTask;
		ProcessTask WorkingOtherUserTask;
		ProcessTask AssignedNoUserTask;
		ProcessTask SuspendedNoUserTask;
		ProcessTask ClosedNoUserTask;

		GlbStaff OtherStaff
		{
			get
			{
				if (fOtherStaff == null)
				{
					fOtherStaff = Factory.NewWithValidTestData<GlbStaff>();
					fOtherStaff.GS_FullName = "Zubin";

					Factory.Save();
				}

				return fOtherStaff;
			}
		}

		GlbStaff fOtherStaff;

		GlbStaff OtherStaff2
		{
			get
			{
				if (fOtherStaff2 == null)
				{
					fOtherStaff2 = Factory.NewWithValidTestData<GlbStaff>();
					fOtherStaff2.GS_FullName = "Bugger";

					Factory.Save();
				}

				return fOtherStaff2;
			}
		}

		GlbStaff fOtherStaff2;

		GlbGroup fOtherGroup;

		GlbGroup OtherGroup
		{
			get
			{
				if (fOtherGroup == null)
				{
					fOtherGroup = Factory.New<GlbGroup>();
					fOtherGroup.GG_Code = "YES";
					Factory.Save();
				}
				return fOtherGroup;
			}
		}

		#endregion
	}
}
