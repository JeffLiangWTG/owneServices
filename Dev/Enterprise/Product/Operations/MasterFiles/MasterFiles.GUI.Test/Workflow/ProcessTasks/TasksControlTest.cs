using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TasksControlTest : TestCaseWithFactory
	{
		#region ReadOnly

		public void TestReadOnly()
		{
			TasksControl.SetControlReadOnly(false);
			Assert(TasksControl.TasksTemplateLinkLabel.Enabled);
			Assert(TasksControl.TasksGrid.Enabled);

			TasksControl.SetControlReadOnly(true);
			Assert(!TasksControl.TasksTemplateLinkLabel.Enabled);
			Assert("Tasks grid still enabled", TasksControl.TasksGrid.Enabled);

			TasksControl.SetControlReadOnly(false);
			Assert(TasksControl.TasksTemplateLinkLabel.Enabled);
			Assert(TasksControl.TasksGrid.Enabled);
		}

		#endregion

		#region Grid Colours

		public void TestColourDeciding()
		{
			ProcessTask onTimeTask = Factory.New<ProcessTask>();

			ProcessTask lateTask = Factory.New<ProcessTask>();
			lateTask.P9_ScheduledDateForBinding = ZDateTimeOffset.Today.AddDays(-1);

			ProcessTask waitingTask = Factory.New<ProcessTask>();
			waitingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			ColourDecidingEventArgs args = new ColourDecidingEventArgs(onTimeTask);
			TasksControl.TasksGrid_ColourDeciding(this, args);
			AssertEquals(Color.Empty, args.Colour);

			args = new ColourDecidingEventArgs(lateTask);
			TasksControl.TasksGrid_ColourDeciding(this, args);
			AssertEquals(Color.LightSalmon, args.Colour);

			args = new ColourDecidingEventArgs(waitingTask);
			TasksControl.TasksGrid_ColourDeciding(this, args);
			AssertEquals(Color.Yellow, args.Colour);
		}

		#endregion

		#region Delete

		public void TestDeleteTasks()
		{
			Form.Show();
			Form.Controls.Add(TasksControl);

			var opportunity = Factory.New<OrgOpportunity>();
			var task1 = opportunity.WorkflowItems.AddNew();
			var task2 = opportunity.WorkflowItems.AddNew();

			TasksControl.TasksGrid.BindTo = "WorkflowItems";
			TasksControl.SetDataBinding(opportunity, "");

			AssertEquals("2 Tasks", 2, TasksControl.Tasks.Count);

			opportunity.WorkflowItems.RemoveAndDelete(task2);
			AssertEquals("1 Tasks", 1, TasksControl.Tasks.Count);
		}

		#endregion

		#region Clone

		public void TestCloneTask()
		{
			Form.Show();
			Form.Controls.Add(TasksControl);
			OrgOpportunity ppp = Factory.New<OrgOpportunity>();
			ProcessTask task1 = ppp.WorkflowItems.AddNew();
			task1.TaskProperties.ActualDate = ZDateTimeOffset.Today;
			TasksControl.TasksGrid.BindTo = "WorkflowItems";
			TasksControl.SetDataBinding(ppp, "");
			AssertEquals("1 Task", 1, TasksControl.Tasks.Count);
			TasksControl.TasksGrid.Select(0);
			TasksControl.TasksGrid.ContextMenu.DoPopup();
			TasksControl.cloneTaskMenuItem.PerformClick();
			AssertEquals("2 Tasks", 2, TasksControl.Tasks.Count);
		}

		public void TestCloneMultipleTasks()
		{
			Form.Show();
			Form.Controls.Add(TasksControl);
			OrgOpportunity ppp = Factory.New<OrgOpportunity>();
			ProcessTask task1 = ppp.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			ProcessTask task2 = ppp.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Task 2";
			TasksControl.TasksGrid.BindTo = "WorkflowItems+Tasks";
			TasksControl.SetDataBinding(ppp, "");
			AssertEquals("2 Tasks", 2, TasksControl.Tasks.Count);
			TasksControl.TasksGrid.SelectAllElements();
			TasksControl.TasksGrid.ContextMenu.DoPopup();
			TasksControl.cloneTaskMenuItem.PerformClick();
			AssertEquals("4 Tasks", 4, TasksControl.Tasks.Count);
			AssertEquals("Task 1", TasksControl.Tasks[0].P9_Description);
			AssertEquals("Task 2", TasksControl.Tasks[1].P9_Description);
			AssertEquals("Task 1", TasksControl.Tasks[2].P9_Description);
			AssertEquals("Task 2", TasksControl.Tasks[3].P9_Description);
		}

		[RequiresSTA]
		public void TestCloneScoping()
		{
			// Arrange
			Form.Show();
			Form.Controls.Add(TasksControl);
			OrgOpportunity ppp = Factory.New<OrgOpportunity>();
			ProcessTask task1 = ppp.WorkflowItems.AddNew();
			TasksControl.TasksGrid.BindTo = "WorkflowItems";
			TasksControl.SetDataBinding(ppp, "");
			// Act
			TasksControl.TasksGrid.Select(0);
			TasksControl.TasksGrid.ContextMenu.DoPopup();
			TasksControl.cloneTaskMenuItem.PerformClick();
			// Assert
			AssertEquals("Scoped into the last row first column", TasksControl.TasksGrid.CurrentCell, new DataGridCell(1, 0));
		}

		public void TestCloneMultipleTasksScoping()
		{
			// Arrange
			Form.Show();
			Form.Controls.Add(TasksControl);
			OrgOpportunity ppp = Factory.New<OrgOpportunity>();

			ProcessTask task1 = ppp.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			ProcessTask task2 = ppp.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Task 2";

			TasksControl.TasksGrid.BindTo = "WorkflowItems+Tasks";
			TasksControl.SetDataBinding(ppp, "");
			// Act
			TasksControl.TasksGrid.SelectAllElements();
			TasksControl.TasksGrid.ContextMenu.DoPopup();
			TasksControl.cloneTaskMenuItem.PerformClick();
			// Assert
			AssertEquals("Scoped into the second last row first column", TasksControl.TasksGrid.CurrentCell, new DataGridCell(2, 0));
		}

		public void TestCloneIsAvailableWhenAddTasksIsAllowed()
		{
			AssertCheckpointControlsCloneMenuItem(true);
		}

		[RequiresSTA]
		public void TestCloneIsNotAvailableWhenAddTasksIsDenied()
		{
			AssertCheckpointControlsCloneMenuItem(false);
		}

		void AssertCheckpointControlsCloneMenuItem(bool isAllowed)
		{
			// Set up descriptors so the controller ids between the security checkpoint and Dummy matches up
			DummyWorkflowDescriptor.Instance.OverriddenControllerID = Dummy.ControllerID;

			SecurityCheckpoint checkpoint = GetCheckpoint(Dummy.ControllerID, SecurityCore.WorkflowAddTasksAutoGeneratedCode);
			checkpoint.IsAllowed = isAllowed;

			Form.Show();
			Form.Controls.Add(TasksControl);
			ProcessTask task1 = Dummy.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			TasksControl.TasksGrid.BindTo = "WorkflowItems+Tasks";
			TasksControl.SetDataBinding(Dummy, "");
			TasksControl.TasksGrid.ContextMenu.DoPopup();

			AssertEquals(isAllowed, TasksControl.TasksGrid.ContextMenu.MenuItems.Cast<MenuItem>().Any(mi => mi.Text == "Clone"));
		}

		SecurityCheckpoint GetCheckpoint(ControllerID controllerId, string securityCode)
		{
			var controller = ZControllerFactory.Create(controllerId);
			using (var module = ZModuleFactory.Instance.Create(controller.ModuleID))
			{
				return Env.Security.FindOrCreateWorkflowItemCheckpoint(module.SecurityCheckpoint, securityCode);
			}
		}

		#endregion

		#region Binding

		public void TestTasksGrid_BindToCollection()
		{
			Form.Show();
			Form.Controls.Add(TasksControl);

			DummyCollectionContainer container = new DummyCollectionContainer(Factory);
			TasksControl.TasksGrid.BindTo = "WorkflowItems";
			TasksControl.SetDataBinding(container, "");

			AssertEquals("Tasks", container.WorkflowItems, TasksControl.Tasks);
		}

		public void TestTasksGrid_BindToCollectionView()
		{
			Form.Show();
			Form.Controls.Add(TasksControl);

			DummyCollectionContainer container = new DummyCollectionContainer(Factory);
			TasksControl.TasksGrid.BindTo = "WorkflowItems+Tasks";
			TasksControl.SetDataBinding(container, "");

			AssertEquals("Tasks", container.WorkflowItems, TasksControl.Tasks);
		}

		#endregion

		#region Create Tasks From Template

		public void TestCreateTasksFromTemplateLinkClick()
		{
			WorkflowDataRegistry.Instance.AlwaysApplyTasksFromTemplateWhenFirstSavingJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var org = OrgHeader.New(Factory);
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = "hasdoiandiasnd asindas inas d";
			org.OH_IsSalesLead = true;

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new OrganisationFormTest.OrgFormForTest(org))
			{
				form.Show();
				form.OrgTabControl.SelectedIndex = 11;
				var salesControl = (ZUserControl)FindSubControlByName(form.OrgTabControl, "SalesControl");
				var salesTabControl = (ZTemplateTabControl)salesControl.Controls[0];
				salesTabControl.SelectedIndex = 1;
				var taskControl = GetTasksControl(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var opp = org.SalesOpportunities.AddNew();
				opp.P8_OpportunityType = "AAA";
				opp.WorkflowItems.AddNew();
				AssertEquals("1 opportunity in grid", 1, taskControl.TasksGrid.ListManager.List.Count);

				opp.WorkflowItems.AddNew();
				taskControl.TasksTemplateLinkLabel_LinkClicked(null, new LinkLabelLinkClickedEventArgs(taskControl.TasksTemplateLinkLabel.Links[0]));
				AssertEquals("You can only create tasks from the template if you have no tasks already entered.", UnitTestUserNotification.Instance.LastMessage.Text);

				opp.WorkflowItems.RemoveAndDeleteAll();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				taskControl.TasksTemplateLinkLabel_LinkClicked(null, new LinkLabelLinkClickedEventArgs(taskControl.TasksTemplateLinkLabel.Links[0]));
				AssertEquals("No Tasks were added from templates for the specified details.", UnitTestUserNotification.Instance.LastMessage.Text);

				var newFactory = new BusinessObjectFactory();
				var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = new OpportunityWorkflowDescriptor().Code;
				template.P0_SubType1 = "AAA";
				template.WorkflowItems.AddNew();
				newFactory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				taskControl.TasksTemplateLinkLabel_LinkClicked(null, new LinkLabelLinkClickedEventArgs(taskControl.TasksTemplateLinkLabel.Links[0]));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateTasksFromTemplateLinkClick_HasBrokerage()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "SHP task";

			var triggerConditions = templateTask as IBaseTrigger;
			triggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			triggerConditions.TriggerCompany = GlbCompany.CurrentCompany.PK;

			var templateConditions = new TemplateConditionsViewModel(templateTask, template);
			templateConditions.TemplateCondition1 = JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached;
			templateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateConditions.TemplateCondition2Value = "\"<_DataSource.JobDeclaration.JE_GoodsDescription>\" == \"\"";
			Factory.Save();

			WorkflowDataRegistry.Instance.AlwaysApplyTasksFromTemplateWhenFirstSavingJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var shipmentTasks = shipmentWorkflow.WorkflowItems.Tasks;

			using (var form = new ZForm(shipment))
			using (var taskControl = new TasksControl())
			{
				form.Controls.Add(taskControl);
				form.Show();
				taskControl.SetDataBinding(shipmentTasks, string.Empty);

				CombineAssertions(() =>
				{
					AssertEquals("Precondition: shipment task", 0, shipmentTasks.Count);

					taskControl.TasksTemplateLinkLabel_LinkClicked(null, null);
					AssertEquals("No Tasks were added from templates for the specified details.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("No shipment task from template", 0, shipmentTasks.Count);

					var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					declaration.JE_JS = shipment.PK;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					taskControl.TasksTemplateLinkLabel_LinkClicked(null, new LinkLabelLinkClickedEventArgs(taskControl.TasksTemplateLinkLabel.Links[0]));
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("shipment task from template", 1, shipmentTasks.Count);
				});
			}
		}

		public void TestBindingSetsConflictResolver()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template.P0_SubType1 = "AAA";

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Tarasque";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "AAA";

			using (var form = (SalesEnquiryForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowFormForNewEntity(job))
			using (var control = new TasksControl())
			{
				form.Controls.Add(control);
				AssertNotEquals(typeof(TaskStatusChangeConflictResolver), Factory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => null)?.GetType());
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();
				AssertEquals(typeof(TaskStatusChangeConflictResolver), Factory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => null)?.GetType());
			}
		}

		public void TestCreateTasksFromTemplateLinkClick_BeforeJobFirstSaved_WithManualTask_ShouldApplyTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template.P0_SubType1 = "AAA";

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Tarasque";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "AAA";

			using (var form = (SalesEnquiryForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowFormForNewEntity(job))
			using (var control = new TasksControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();

				var workflowTab = form.FindSingle<ZWorkflowTabPage>();
				((TabControl)workflowTab.Parent).SelectTab(workflowTab);

				AssertEquals("Template hasn't been applied yet", 0, job.WorkflowItems.Tasks.Count);

				var task = job.WorkflowItems.Tasks.AddNew();
				task.P9_Description = "Basilisk";

				control.TasksTemplateLinkLabel_LinkClicked(null, null);

				AssertEquals("Template has been applied, even though a 'manual' task was created.", 2, job.WorkflowItems.Tasks.Count);
				AssertEquals("Basilisk", job.WorkflowItems.Tasks[0].P9_Description);
				AssertEquals("Tarasque", job.WorkflowItems.Tasks[1].P9_Description);
			}
		}

		public void TestCreateTasksFromTemplateLinkClick_WithSubsequentFormSave_WithManualTask_ShouldNotApplyTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template.P0_SubType1 = "AAA";

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Tarasque";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();

			AssertEquals("Template hasn't been applied yet", 0, job.WorkflowItems.Tasks.Count);

			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Basilisk";

			Factory.Save();

			AssertEquals("Template hasn't been applied because the template doesn't match", 1, job.WorkflowItems.Tasks.Count);

			using (var form = (SalesEnquiryForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowFormForNewEntity(job))
			using (var control = new TasksControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();

				var workflowTab = form.FindSingle<ZWorkflowTabPage>();
				((TabControl)workflowTab.Parent).SelectTab(workflowTab);

				job.O1_EnquiryType = "AAA";

				control.TasksTemplateLinkLabel_LinkClicked(null, null);

				AssertEquals("Template hasn't been applied since the job has already been saved", 1, job.WorkflowItems.Tasks.Count);
			}
		}

		public void TestCreateTasksFromTemplateLinkClick_WhenJobFirstSaved_WithManualTaskAndRegistryDisabled_ShouldNotApplyTemplate()
		{
			WorkflowDataRegistry.Instance.AlwaysApplyTasksFromTemplateWhenFirstSavingJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template.P0_SubType1 = "AAA";

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Tarasque";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "AAA";

			using (var form = (SalesEnquiryForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowFormForNewEntity(job))
			using (var control = new TasksControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();

				var workflowTab = form.FindSingle<ZWorkflowTabPage>();
				((TabControl)workflowTab.Parent).SelectTab(workflowTab);

				AssertEquals("Template hasn't been applied yet", 0, job.WorkflowItems.Tasks.Count);

				var task = job.WorkflowItems.Tasks.AddNew();
				task.P9_Description = "Basilisk";

				control.TasksTemplateLinkLabel_LinkClicked(null, null);

				AssertEquals("Template hasn't been applied since the registry item controlling this behaviour has been disabled", 1, job.WorkflowItems.Tasks.Count);
				AssertEquals("Basilisk", job.WorkflowItems.Tasks[0].P9_Description);
			}
		}

		public void TestApplyWorkflowTemplates_WhenTemplateConditionsNowMatch_ShouldApplyWithoutCallingSave()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"DNW\"";

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.TagWasAddedOrRemovedCode;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TriggerConditions.TriggerEventCode = AutoEvents.WorkflowTransferredBetweenSystemComponentsCode;

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TasksControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(new ZTextBox());
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();

				AssertEquals(0, job.WorkflowItems.Tasks.Count);
				control.TasksTemplateLinkLabel_LinkClicked(null, null);
				AssertEquals(0, job.WorkflowItems.Tasks.Count);

				job.Z0_Code = "DNW";

				AssertEquals(0, job.WorkflowItems.Tasks.Count);
				control.TasksTemplateLinkLabel_LinkClicked(null, null);

				AssertEquals("Task template condition is now met, so should be applied to job", 1, job.WorkflowItems.Tasks.Count);
				AssertEquals("Clicking hyperlink on tasks tab should not apply workflow templates for milestones", 0, job.WorkflowItems.Milestones.Count);
				AssertEquals("Clicking hyperlink on tasks tab should not apply workflow templates for triggers", 0, job.WorkflowItems.Triggers.Count);
			}
		}

		#endregion

		public void TestMultiSortForBusinessObjectCollectionView()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			opp.P8_OpportunityType = opp.Lookups.Types[0].Code;
			opp.P8_OpportunityDescription = "Bah bah moo moo";
			opp.P8_Stage = opp.Lookups.Stages[0].Code;
			opp.P8_Status = opp.Lookups.Statuses[0].Code;

			for (int i = 1; i < 11; i++)
			{
				var task = opp.WorkflowItems.AddNew();
				task.P9_Sequence = i;
			}

			opp.WorkflowItems[0].P9_Description = "One";
			opp.WorkflowItems[1].P9_Description = "Two";
			opp.WorkflowItems[2].P9_Description = "Three";
			opp.WorkflowItems[3].P9_Description = "Four";
			opp.WorkflowItems[4].P9_Description = "Five";
			opp.WorkflowItems[5].P9_Description = "Six";
			opp.WorkflowItems[6].P9_Description = "Seven";
			opp.WorkflowItems[7].P9_Description = "Eight";
			opp.WorkflowItems[8].P9_Description = "Nine";
			opp.WorkflowItems[9].P9_Description = "Ten";

			Factory.Save();

			using (var form = new OpportunityForm(opp))
			{
				form.ControllerID = ControllerIDs.Opportunity;
				form.Show();
				form.TopLevelTabControl.SelectedIndex = 1;

				var tasksControl = GetTasksControl(form);
				var grid = tasksControl.TasksGrid;

				var testCollection1 = new ProcessTaskCollection(Factory)
					{
						opp.WorkflowItems[0],
						opp.WorkflowItems[2],
						opp.WorkflowItems[4],
						opp.WorkflowItems[6],
						opp.WorkflowItems[8],
						opp.WorkflowItems[1],
						opp.WorkflowItems[3],
						opp.WorkflowItems[5],
						opp.WorkflowItems[7],
						opp.WorkflowItems[9]
					};

				var testView1 = new ProcessTaskCollectionView(testCollection1);

				((ProcessTask)grid.ListManager.List[0]).P9_Status = "ASN";
				((ProcessTask)grid.ListManager.List[2]).P9_Status = "ASN";
				((ProcessTask)grid.ListManager.List[4]).P9_Status = "ASN";
				((ProcessTask)grid.ListManager.List[6]).P9_Status = "ASN";
				((ProcessTask)grid.ListManager.List[8]).P9_Status = "ASN";

				//click column
				var e1 = new MouseEventArgs(MouseButtons.Left, 1, ControlDpiScalingHelper.ScaleToCurrentDpiX(400), ControlDpiScalingHelper.ScaleToCurrentDpiY(10), 0);
				grid.PerformMouseDownForTest(e1, -1, 4);
				grid.PerformMouseUpForTest(e1, -1, 4);

				Assert(CompareViews((ProcessTaskCollectionView)grid.ListManager.List, testView1));

				var testCollection2 = new ProcessTaskCollection(Factory)
					{
						opp.WorkflowItems[4],
						opp.WorkflowItems[8],
						opp.WorkflowItems[0],
						opp.WorkflowItems[6],
						opp.WorkflowItems[2],
						opp.WorkflowItems[7],
						opp.WorkflowItems[3],
						opp.WorkflowItems[5],
						opp.WorkflowItems[9],
						opp.WorkflowItems[1]
					};

				var testView2 = new ProcessTaskCollectionView(testCollection2);

				grid.ShiftBypass = true;

				var e2 = new MouseEventArgs(MouseButtons.Left, 1, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(10), 0);
				grid.PerformMouseDownForTest(e2, -1, 0);
				grid.PerformMouseUpForTest(e2, -1, 0);

				Assert(CompareViews((ProcessTaskCollectionView)grid.ListManager.List, testView2));
			}
		}

		bool CompareViews(ProcessTaskCollectionView gridList, ProcessTaskCollectionView sortedList)
		{
			if (gridList.Count != sortedList.Count)
			{
				return false;
			}

			for (int i = 0; i < gridList.Count; i++)
			{
				if (gridList[i].P9_Sequence != sortedList[i].P9_Sequence
					|| gridList[i].P9_Description != sortedList[i].P9_Description
					|| gridList[i].P9_Status != sortedList[i].P9_Status)
				{
					return false;
				}
			}

			return true;
		}

		#region Show Tasks Details Form

		public void TestShowTaskDetailsForm()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			opp.P8_OpportunityType = opp.Lookups.Types[0].Code;
			opp.P8_OpportunityDescription = "Bah bah moo moo";
			opp.P8_Stage = opp.Lookups.Stages[0].Code;
			opp.P8_Status = opp.Lookups.Statuses[0].Code;

			ProcessTask task = opp.WorkflowItems.AddNew();
			task.P9_Type = task.Lookups.Types[0].Code;
			task.P9_Status = task.Lookups.Statuses[0].Code;
			task.P9_Description = "Some Task";

			using (OpportunityForm form = new OpportunityForm(opp))
			{
				form.ControllerID = ControllerIDs.Opportunity;
				form.Show();
				form.TopLevelTabControl.SelectedIndex = 1;

				TasksControl tasksControl = GetTasksControl(form);

				AssertEquals("1 task in grid", 1, tasksControl.TasksGrid.ListManager.List.Count);
				tasksControl.TasksGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				Rectangle row1Rectangle = tasksControl.TasksGrid.GetRowNotificationRectangle(0);
				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, row1Rectangle.X, row1Rectangle.Y, 0);

				tasksControl.TasksGrid_DoubleClick(this, mouseEvent);
				AssertEquals("Error about haschanges shown", "You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Form NOT Shown", tasksControl.LastController);
				AssertEquals("Not Saved", true, opp.HasChanges);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				tasksControl.TasksGrid_DoubleClick(this, mouseEvent);
				AssertEquals("Error about haschanges shown", "You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("Form Shown", tasksControl.LastController.LastShownForm);
				AssertEquals("Saved", false, opp.HasChanges);
				tasksControl.LastController.LastShownForm.Dispose();

				org.OH_FullName = "asfunasufafionfus";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Save();
				tasksControl.TasksGrid_DoubleClick(this, mouseEvent);
				AssertNotNull("Form Shown", tasksControl.LastController.LastShownForm);
				tasksControl.LastController.LastShownForm.Dispose();

				opp.WorkflowItems.RemoveAndDeleteAll();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				tasksControl.TasksGrid_DoubleClick(this, mouseEvent);
				AssertEquals("Msg shown about no item selected", "Please select a valid row.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				tasksControl.TasksGrid_DoubleClick(this, mouseEvent);
				AssertEquals("Msg shown about no item selected", "Please select a valid row.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestShowTaskDetailsForm_NewRow_NoOtherUserHasDeletedError()
		{
			var opportunity = CreateOpportunityForTaskDetailsFormTest();

			Factory.Save();

			using (var form = new OpportunityForm(opportunity))
			{
				form.ControllerID = ControllerIDs.Opportunity;
				form.Show();
				form.TopLevelTabControl.SelectedIndex = 1;

				var task = opportunity.WorkflowItems.AddNew();
				task.P9_Type = task.Lookups.Types[0].Code;
				task.P9_Status = task.Lookups.Statuses[0].Code;
				task.P9_Description = "Some Task";

				var tasksControl = GetTasksControl(form);

				AssertEquals("1 task in grid", 1, tasksControl.TasksGrid.ListManager.List.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				Rectangle row1Rectangle = tasksControl.TasksGrid.GetRowNotificationRectangle(0);
				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, row1Rectangle.X, row1Rectangle.Y, 0);
				tasksControl.TasksGrid_DoubleClick(this, mouseEvent);

				AssertEquals("Error about haschanges shown", "You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("Form Shown", tasksControl.LastController.LastShownForm);

				tasksControl.LastController.LastShownForm.Dispose();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestShowTaskDetailsForm_NewInvalidRow_NoOtherUserHasDeletedError()
		{
			var opportunity = CreateOpportunityForTaskDetailsFormTest();

			Factory.Save();

			using (var form = new OpportunityForm(opportunity))
			{
				form.ControllerID = ControllerIDs.Opportunity;
				form.Show();
				form.TopLevelTabControl.SelectedIndex = 1;

				var task = opportunity.WorkflowItems.AddNew();
				task.P9_Type = "ZZZ";

				var tasksControl = GetTasksControl(form);

				AssertEquals("1 task in grid", 1, tasksControl.TasksGrid.ListManager.List.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				Rectangle row1Rectangle = tasksControl.TasksGrid.GetRowNotificationRectangle(0);
				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, row1Rectangle.X, row1Rectangle.Y, 0);
				tasksControl.TasksGrid_DoubleClick(this, mouseEvent);

				AssertEquals("task.Type = ZZZ should cause invalidate data and cannot save", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Form NOT Shown", tasksControl.LastController);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestShowTaskDetailsForm_ExistingInvalidRow_NoOtherUserHasDeletedError()
		{
			var opportunity = CreateOpportunityForTaskDetailsFormTest();

			var task = opportunity.WorkflowItems.AddNew();
			task.P9_Type = task.Lookups.Types[0].Code;
			task.P9_Status = task.Lookups.Statuses[0].Code;
			task.P9_Description = "Some Task";

			Factory.Save();

			using (var form = new OpportunityForm(opportunity))
			{
				form.ControllerID = ControllerIDs.Opportunity;
				form.Show();
				form.TopLevelTabControl.SelectedIndex = 1;

				task.P9_Type = "ZZZ";

				var tasksControl = GetTasksControl(form);

				AssertEquals("1 task in grid", 1, tasksControl.TasksGrid.ListManager.List.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				Rectangle row1Rectangle = tasksControl.TasksGrid.GetRowNotificationRectangle(0);
				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, row1Rectangle.X, row1Rectangle.Y, 0);
				tasksControl.TasksGrid_DoubleClick(this, mouseEvent);

				AssertEquals("task.Type = ZZZ should cause invalidate data and cannot save", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Form NOT Shown", tasksControl.LastController);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestShowTaskDetailsForm_NewRow_WithoutAnyData_NoOtherUserHasDeletedError()
		{
			var opportunity = CreateOpportunityForTaskDetailsFormTest();
			Factory.Save();

			using (var form = new OpportunityForm(opportunity))
			{
				form.ControllerID = ControllerIDs.Opportunity;
				form.Show();
				form.TopLevelTabControl.SelectedIndex = 1;

				var tasksControl = GetTasksControl(form);

				var grid = tasksControl.TasksGrid;
				grid.Select();

				AssertEquals("1 task in grid", 1, tasksControl.TasksGrid.ListManager.List.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				Rectangle row1Rectangle = grid.GetRowNotificationRectangle(0);
				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, row1Rectangle.X, row1Rectangle.Y, 0);
				tasksControl.TasksGrid_DoubleClick(this, mouseEvent);

				AssertEquals("Error about HasChanges shown", "Please select a saved row to edit.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		#endregion

		#region Show Matching Template Form

		public void TestShowMatchingTemplateForm()
		{
			using (var control = new TasksControl())
			{
				AssertNotNull(control.TasksGrid.ContextMenu.MenuItems.Cast<MenuItem>().First(n => n.Text == WorkflowDiagnosticContextMenuManager.WorkflowTemplateMatchCaption));
			}
		}

		#endregion

		#region Working Task Conflict

		public void TestConflictResolution()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			opp.P8_OpportunityType = opp.Lookups.Types[0].Code;
			opp.P8_OpportunityDescription = "Bah bah moo moo";
			opp.P8_Stage = opp.Lookups.Stages[0].Code;
			opp.P8_Status = opp.Lookups.Statuses[0].Code;

			ProcessTask task = opp.WorkflowItems.Tasks.AddNew();
			task.P9_Type = task.Lookups.Types[0].Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Description = "Some Task";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			ProcessTask task1 = opp.WorkflowItems.Tasks.AddNew();
			task1.P9_Type = task.Lookups.Types[0].Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Description = "Some Task 222";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgOpportunity reloadedOpp = newFactory.Load<OrgOpportunity>(opp.PK);

			var changeTrackingResolver = new ChangeTrackingITaskStatusChangeConflictResolverProvider();

			using (OpportunityForm form = new OpportunityForm(reloadedOpp))
			{
				form.ControllerID = ControllerIDs.Opportunity;
				form.Show();

				form.TopLevelTabControl.SelectedIndex = 1;

				TasksControl tasksControl = GetTasksControl(form);
				newFactory.ClearCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver));
				newFactory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => changeTrackingResolver);

				AssertEquals("2 tasks in grid", 2, tasksControl.TasksGrid.ListManager.List.Count);

				reloadedOpp.WorkflowItems.Tasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				AssertEquals(0, changeTrackingResolver.ExistingWorkTaskExistsEventFiredCount);

				reloadedOpp.WorkflowItems.Tasks[1].P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				AssertEquals(1, changeTrackingResolver.ExistingWorkTaskExistsEventFiredCount);
			}
		}

		class ChangeTrackingITaskStatusChangeConflictResolverProvider : ITaskStatusChangeConflictResolver
		{
			public ITaskStatusChangeConflictResolver GetConflictResolver() => this;

			public ITaskStatusChangeConflictResolution GetExistingWorkTaskExists(IProcessTask task)
			{
				ExistingWorkTaskExistsEventFiredCount++;
				return TaskStatusChangeConflictResolution.Continue();
			}

			public ITaskStatusChangeConflictResolution GetExistingSuspendedTaskExists(IProcessTask task)
			{
				ExistingSuspendedTaskExistsEventFiredCount++;
				return TaskStatusChangeConflictResolution.Continue();
			}

			public TimeSpan GetWorkingDayChoice(TimeSpan withoutWorkingDays, TimeSpan withWorkingDays) => withWorkingDays;

			public int ExistingWorkTaskExistsEventFiredCount { get; private set; }
			public int ExistingSuspendedTaskExistsEventFiredCount { get; private set; }
		}

		#endregion

		#region Column Layout

		public void TestColumnLayoutForProcessTaskTemplate()
		{
			Form.Show();
			Form.Controls.Add(TasksControl);

			var businessEntity = new DummyCollectionContainer(Factory);
			var tasksGrid = TasksControl.TasksGrid;
			tasksGrid.BindTo = "TemplateWorkflowItems+Tasks";
			TasksControl.SetDataBinding(businessEntity, "");

			var columns = tasksGrid.Columns;
			CombineAssertions(() =>
			{
				AssertEquals("P9_Description visible", true, columns.Contains(ProcessTasksSchema.P9_Description.Name));
				AssertEquals("P9_Sequence visible", true, columns.Contains(ProcessTasksSchema.P9_Sequence.Name));
				AssertEquals("P9_Type visible", true, columns.Contains(ProcessTasksSchema.P9_Type.Name));
				AssertEquals("TemplateCondition1 visible", true, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.TemplateCondition1));
				AssertEquals("TemplateCondition2 visible", true, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.TemplateCondition2));
				AssertEquals("TemplateCondition2Value visible", true, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.TemplateCondition2Value));
				AssertEquals("OriginCountryCode visible", true, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.OriginCountryCode));
				AssertEquals("DestinationCountryCode visible", true, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.DestinationCountryCode));

				AssertEquals("P9_SE_NKMilestoneEvent hidden", false, columns.Contains(ProcessTasksSchema.P9_SE_NKMilestoneEvent.Name));
				AssertEquals("P9_SE_NKExceptionEvent hidden", false, columns.Contains(ProcessTasksSchema.P9_SE_NKExceptionEvent.Name));
				AssertEquals("P9_SE_NKMilestoneEventDescription hidden", false, columns.Contains(ProcessTask.Schema.P9_SE_NKMilestoneEventDescription));
				AssertEquals("P9_SE_NKExceptionEventDescription hidden", false, columns.Contains(ProcessTask.Schema.P9_SE_NKExceptionEventDescription));
				AssertEquals("ElapsedDuration hidden", false, columns.Contains("ElapsedDuration"));
				AssertEquals("SuspendedDuration hidden", false, columns.Contains("SuspendedDuration"));
				AssertEquals("HasNote hidden", false, columns.Contains("HasNote"));
				AssertEquals("P9_TaskID hidden", false, columns.Contains(ProcessTasksSchema.P9_TaskID.Name));
				AssertEquals("P9_ScheduledDate hidden", false, columns.Contains(ProcessTasksSchema.P9_ScheduledDate.Name));
				AssertEquals("P9_ActualDate hidden", false, columns.Contains(ProcessTasksSchema.P9_ActualDate.Name));
				AssertEquals("P9_ActualDuration hidden", false, columns.Contains(ProcessTasksSchema.P9_ActualDuration.Name));
				AssertEquals("P9_RecalculateScheduledDate available", true, columns.Contains(ProcessTasksSchema.P9_RecalculateScheduledDate.Name));
				AssertEquals("Iteration is not available for templates", false, columns.Contains("Iteration"));
				AssertEquals("P9_RecalculateScheduledDate hidden by default", false, columns[columns.IndexOf(c => c.ColumnName == ProcessTasksSchema.P9_RecalculateScheduledDate.Name)].IsVisible);
				AssertEquals("TimeBecameStartable is not available for templates", false, columns.Contains("TimeBecameStartable"));

				AssertEquals("TriggerContextCode visible", true, columns.Contains(nameof(ProcessTask.TriggerConditions) + "+" + nameof(TriggerConditionsViewModel.TriggerContextCode)));
				AssertEquals("TriggerCompany visible", true, columns.Contains(nameof(ProcessTask.TriggerConditions) + "+" + nameof(TriggerConditionsViewModel.TriggerCompany)));
			});
		}

		public void TestColumnLayoutForProcessTask()
		{
			Form.Show();
			Form.Controls.Add(TasksControl);

			var businessEntity = new DummyCollectionContainer(Factory);
			var tasksGrid = TasksControl.TasksGrid;
			TasksControl.TasksGrid.BindTo = "WorkflowItems+Tasks";
			TasksControl.SetDataBinding(businessEntity, "");

			var columns = tasksGrid.Columns;
			CombineAssertions(() =>
			{
				AssertEquals("P9_Description visible", true, columns.Contains(ProcessTasksSchema.P9_Description.Name));
				AssertEquals("P9_Sequence visible", true, columns.Contains(ProcessTasksSchema.P9_Sequence.Name));
				AssertEquals("P9_Type visible", true, columns.Contains(ProcessTasksSchema.P9_Type.Name));
				AssertEquals("TemplateCondition1 hidden", false, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.TemplateCondition1));
				AssertEquals("TemplateCondition2 hidden", false, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.TemplateCondition2));
				AssertEquals("TemplateCondition2Value hidden", false, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.TemplateCondition2Value));
				AssertEquals("OriginCountryCode visible", false, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.OriginCountryCode));
				AssertEquals("DestinationCountryCode visible", false, columns.Contains("TemplateConditions+" + TemplateConditionsViewModel.Schema.DestinationCountryCode));

				AssertEquals("P9_SE_NKMilestoneEvent hidden", false, columns.Contains(ProcessTasksSchema.P9_SE_NKMilestoneEvent.Name));
				AssertEquals("P9_SE_NKExceptionEvent hidden", false, columns.Contains(ProcessTasksSchema.P9_SE_NKExceptionEvent.Name));
				AssertEquals("P9_SE_NKMilestoneEventDescription hidden", false, columns.Contains(ProcessTask.Schema.P9_SE_NKMilestoneEventDescription));
				AssertEquals("P9_SE_NKExceptionEventDescription hidden", false, columns.Contains(ProcessTask.Schema.P9_SE_NKExceptionEventDescription));
				AssertEquals("ElapsedDuration visible", true, columns.Contains("ElapsedDuration"));
				AssertEquals("SuspendedDuration visible", true, columns.Contains("SuspendedDuration"));
				AssertEquals("HasNote visible", true, columns.Contains("HasNote"));
				AssertEquals("P9_TaskID visible", true, columns.Contains(ProcessTasksSchema.P9_TaskID.Name));
				AssertEquals("P9_ScheduledDate visible", true, columns.Contains("P9_ScheduledDateForBinding"));
				AssertEquals("P9_ActualDate visible", true, columns.Contains("P9_ActualDateForBinding"));
				AssertEquals("P9_ActualDuration visible", true, columns.Contains(ProcessTasksSchema.P9_ActualDuration.Name));
				AssertEquals("P9_RecalculateScheduledDate available", true, columns.Contains(ProcessTasksSchema.P9_RecalculateScheduledDate.Name));
				AssertEquals("P9_RecalculateScheduledDate hidden by default", false, columns[columns.IndexOf(c => c.ColumnName == ProcessTasksSchema.P9_RecalculateScheduledDate.Name)].IsVisible);
				AssertEquals("Iteration hidden by default", false, columns[columns.IndexOf(c => c.ColumnName == "Iteration")].IsVisible);
				AssertEquals("TimeBecameStartable not available by default", -1, columns.IndexOf(c => c.ColumnName == "TimeBecameStartable"));

				AssertEquals("TriggerContextCode visible", false, columns.Contains(nameof(ProcessTask.TriggerConditions) + "+" + nameof(TriggerConditionsViewModel.TriggerContextCode)));
				AssertEquals("TriggerCompany visible", false, columns.Contains(nameof(ProcessTask.TriggerConditions) + "+" + nameof(TriggerConditionsViewModel.TriggerCompany)));
			});
		}

		public void TestColumnLayoutForProcessTask_TimeBecameStartable_WhenBufferManagementEnabled_And_TheTypeIsInBMSystemRelatedJobTypes()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Form.Show();
			Form.Controls.Add(TasksControl);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "OPP");
			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var task1 = opportunity.WorkflowItems.AddNew();
			var task2 = opportunity.WorkflowItems.AddNew();
			TasksControl.TasksGrid.BindTo = "WorkflowItems";
			TasksControl.SetDataBinding(opportunity, "");

			var columns = TasksControl.TasksGrid.Columns;
			AssertEquals("TimeBecameStartable hidden by default when available", false, columns[columns.IndexOf(c => c.ColumnName == "TimeBecameStartable")].IsVisible);
		}

		public void TestLoadOrganisationsForm_AndNavigateToSalesOpportunitiesTabs_ShouldNotThrowException()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsSalesLead = true;

			using (var form = new ZOrganisationsForm(org))
			{
				form.Show();
				Application.DoEvents();

				form.OrganisationsTabControl.SelectedTab = form.SalesTabPage;
				Application.DoEvents();

				var salesControl = form.SalesTabPage.FindSingle<SalesUserControl>();
				var opportunityTabPage = salesControl.FindSingle<ZTabPage>("OpportunityTabPage");

				AssertNoExceptionThrown("Showing the opportunity tab shouldn't throw any exceptions.", () =>
				{
					salesControl.SalesTabControl.SelectedTab = opportunityTabPage;
					Application.DoEvents();
				});
			}
		}

		#endregion

		#region Close Task Password Entry

		[UseSnapshotProtection(true)]
		public void TestTaskOwnerPasswordRequestedEvent()
		{
			SetupDummyOrgOpportunityWorkflowTaskTypes();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			ProcessTask task = opp.WorkflowItems.AddNew();
			task.P9_Type = "RVW";
			task.P9_GS_NKAssignedStaffMember = "ZZ";

			using (OpportunityForm form = new OpportunityForm(opp))
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Show();
				form.TopLevelTabControl.SelectedIndex = 1;

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals(typeof(TaskOwnerPasswordRequestForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		void SetupDummyOrgOpportunityWorkflowTaskTypes()
		{
			CategorisedWorkflowTaskTypesCollection categorisedTaskTypesCollection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes categorisedTaskTypes = categorisedTaskTypesCollection.AddNew();

			categorisedTaskTypes.Code = OpportunityWorkflowDescriptor.WorkflowTypeCode;
			WorkflowTaskType reviewTask = categorisedTaskTypes.TaskTypes.AddNew();
			reviewTask.Code = "RVW";
			reviewTask.CanCloseTaskNotAssignedToSelf = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypesCollection);
			Env.Security.WorkflowTasksCloseTaskNotAssignedToSelf.IsAllowed = false;
		}

		#endregion

		#region Tasks from Other Companies

		public void TestTaskGrid_WhenThereAreTasksInOtherCompanies_ProcessTypeUsesCompanySpecificTasks_ShouldShowHintLabel()
		{
			var startingBranch = GlbBranch.GetCurrentBranch(Factory);
			var otherCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			var otherCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch1 = otherCompany1.Branches.AddNew();
			var otherBranch2 = otherCompany2.Branches.AddNew();

			otherBranch1.FillWithValidTestData();
			otherBranch2.FillWithValidTestData();

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.Z0_Description = "Wheremy?";

			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();
			var task3 = job.WorkflowItems.Tasks.AddNew();

			task1.P9_GC = Env.CurrentCompanyPK;
			task2.P9_GC = otherCompany1.PK;
			task3.P9_GC = otherCompany2.PK;

			Factory.Save();

			AssertEquals(true, job.IsInDatabase);
			AssertNotEquals(task1.P9_TaskID, task2.P9_TaskID);
			AssertEquals(false, DummyWorkflowDescriptor.Instance.AreTasksCompanySpecific);

			ShowTasksControlAndAssertTasksShown("Should only show all tasks in the job since we're not filtering by company", startingBranch, null, job, false, task1, task2, task3);
			ShowTasksControlAndAssertTasksShown("Should only show all tasks in the job since we're not filtering by company", otherBranch1, null, job, false, task1, task2, task3);
			ShowTasksControlAndAssertTasksShown("Should only show all tasks in the job since we're not filtering by company", otherBranch2, null, job, false, task1, task2, task3);

			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);

			ShowTasksControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list all companies which have tasks", startingBranch, new[] { otherCompany1, otherCompany2 }, job, true, task1);
			ShowTasksControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list all companies which have tasks", otherBranch1, new[] { startingBranch.Company, otherCompany2 }, job, true, task2);
			ShowTasksControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list all companies which have tasks", otherBranch2, new[] { startingBranch.Company, otherCompany1 }, job, true, task3);

			task3.P9_GC = task2.P9_GC;
			Factory.Save();

			ShowTasksControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list the single company which has a task", startingBranch, new[] { otherCompany1 }, job, true, task1);
			ShowTasksControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list the single company which has a task", otherBranch1, new[] { startingBranch.Company }, job, true, task2, task3);
			ShowTasksControlAndAssertTasksShown("Should show no tasks since all tasks are part of another company", otherBranch2, new[] { startingBranch.Company, otherCompany1 }, job, true);
		}

		[RequiresSTA]
		public void TestTaskGrid_WhenThereAreTasksFromEarlierIncarnationsOfSameJob_ProcessTypeUsesCompanySpecificTasks_ShouldNotShowHintLabel()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			company1.GC_Code = "VIC";
			company2.GC_Code = "TOR";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			company1.Branches.Add(branch1);
			company2.Branches.Add(branch2);

			var bizo = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var job = (IWorkflowProvider)bizo;
			bizo.FillWithValidTestData();

			// Save companies and branches before creating tasks because task edit permissions are tied to company + branch
			Factory.Save();

			var task1 = MasterFilesTestHelper.CreateTask(job, company: company1);
			var task2 = MasterFilesTestHelper.CreateTask(job, company: company1);
			var task3 = MasterFilesTestHelper.CreateTask(job, company: company2); // Part of a different company, but we don't care yet because it's not really part of this job as per the next line...

			task3.P9_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix; // Tasks can be present before a quote is converted into a shipment.

			AssertEquals("Pre-condition: tasks for this job type should be company-specific.", true, task1.WorkflowDescriptor.AreTasksCompanySpecific);

			Factory.Save();

			ShowTasksControlAndAssertTasksShown("Task3 belonging to the same parent GUID isn't shown because it's only part of that job in its quoted booking incarnation.", branch1, null, job, false, task1, task2);

			task3.P9_ParentTableCode = bizo.TablePrefix;
			Factory.Save();

			ShowTasksControlAndAssertTasksShown("Task3 now fully belongs to the same parent and is from a different company, so should be hidden.", branch1, new[] { company2 }, job, true, task1, task2);
		}

		static void ShowTasksControlAndAssertTasksShown(string message, GlbBranch branchToLogInto, GlbCompany[] otherCompanies, IWorkflowProvider job, bool areSomeTasksHidden, params ProcessTask[] expectedTasksToBeShown)
		{
			var factory = ((IBusiness)job).Factory.CreateNewFactory();
			job = (IWorkflowProvider)factory.Load(job.GetType(), job.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchToLogInto.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (var form = new ZForm { Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new TasksControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();

				Application.DoEvents();

				AssertContainsExactElementsInAnyOrder(message, GetTaskIDs(expectedTasksToBeShown), GetTaskIDs(control.TasksGrid.List));

				var splitContainer = control.FindSingle<KSplitContainer>(c => c.Name == "TasksHintSplitContainer");

				AssertEquals("SplitContainer is simply for hiding/showing the hint label, so shouldn't be able to adjust the slider", true, splitContainer.IsSplitterFixed);

				if (areSomeTasksHidden)
				{
					AssertEquals("Some tasks are not shown since they are specific to another company. We should show thie hint label.", false, splitContainer.Panel1Collapsed);

					var label = splitContainer.Panel1.Controls.OfType<ZLabel>().Single();

					if (otherCompanies.Length == 1)
					{
						AssertEquals($"There are tasks in this job that are shown only when logged into {otherCompanies.Single().GC_Code} company.", label.Text);
					}
					else
					{
						var list = string.Join(", ", otherCompanies.Select(c => c.GC_Code));
						AssertEquals($"There are tasks in this job that are shown only when logged into {list} companies.", label.Text);
					}
				}
				else
				{
					AssertEquals("There's no need to show the hint label since all tasks are visible and everything is fines.", true, splitContainer.Panel1Collapsed);
				}
			}
		}

		static string[] GetTaskIDs(IEnumerable tasks)
		{
			return tasks.Cast<ProcessTask>().Select(t => t.P9_TaskID.ToString()).ToArray();
		}

		#endregion

		#region Assist With This Task

		[RequiresSTA]
		public void TestAssistWithThisTaskMenuItem_OnClick()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Other user's task";
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;
			task.P9_EstDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 1, 0, 0);
			task.P9_EstimateVariationFactor = 4;
			task.P9_Sequence = 5;

			var workingTask = orgHeader.WorkflowItems.Tasks.AddNew();
			workingTask.P9_Description = "Task I'm already working on";
			workingTask.P9_Type = "INV";
			workingTask.P9_GS_NKAssignedStaffMember = currentUser;
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("A new task should be added.", 4, grid.VisibleRowCount);
				AssertEquals("A new task should be added.", 3, taskControl.Tasks.Count);

				var assistTask = (ProcessTask)taskControl.Tasks.SingleOrDefault(x => !x.IsInDatabase);
				AssertNotNull(assistTask);

				AssertEquals(orgHeader.PK, assistTask.Parent.PK);
				AssertEquals("OH", assistTask.P9_ParentTableCode);
				AssertEquals("AST", assistTask.P9_Type);
				AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
				AssertEquals(5, assistTask.P9_Sequence);
				AssertEquals(currentUser, assistTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, assistTask.P9_Status);
				AssertEquals("Assist", assistTask.P9_Description);
				AssertNull(assistTask.ProcessHeader);
				AssertEquals("We should let the user save the form when they're ready.", false, assistTask.IsInDatabase);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, workingTask.P9_Status);

				assistTask.Factory.Save();

				workingTask.Reload();
				AssertEquals("Since the current user is now assisting, their previously working task should now be suspended.", ProcessTaskStatusCodeList.Codes.Suspended, workingTask.P9_Status);
			}
		}

		[RequiresSTA]
		public void TestAddAssistanceTaskForMenuItem_StaffSubMenu_OnClick()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var anotherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Other user's task";
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;
			task.P9_EstDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 1, 0, 0);
			task.P9_EstimateVariationFactor = 4;
			task.P9_Sequence = 5;

			var workingTask = orgHeader.WorkflowItems.Tasks.AddNew();
			workingTask.P9_Description = "Task I'm already working on";
			workingTask.P9_Type = "INV";
			workingTask.P9_GS_NKAssignedStaffMember = currentUser;
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var anotherTask = orgHeader.WorkflowItems.Tasks.AddNew();
			anotherTask.P9_Description = "Another task that belongs to someone else";
			anotherTask.P9_Type = "INV";
			anotherTask.P9_GS_NKAssignedStaffMember = anotherUser;
			anotherTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForStaffSubMenu>().SingleOrDefault();
				staffSubMenu.ShowPopupMenu();

				Application.DoEvents();

				var addAssistanceMenuItem = staffSubMenu.MenuItems.OfType<AddAssistanceTaskForStaffMenuItem>().SingleOrDefault(m => m.Caption.ToString().Contains(anotherUser));
				addAssistanceMenuItem.PerformClick();

				Application.DoEvents();

				AssertEquals("A new task should be added.", 5, grid.VisibleRowCount);
				AssertEquals("A new task should be added.", 4, taskControl.Tasks.Count);

				var assistTask = (ProcessTask)taskControl.Tasks.SingleOrDefault(x => !x.IsInDatabase);
				AssertNotNull(assistTask);

				AssertEquals(orgHeader.PK, assistTask.Parent.PK);
				AssertEquals("OH", assistTask.P9_ParentTableCode);
				AssertEquals("AST", assistTask.P9_Type);
				AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
				AssertEquals(5, assistTask.P9_Sequence);
				AssertEquals(anotherUser, assistTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
				AssertEquals("Assist", assistTask.P9_Description);
				AssertNull(assistTask.ProcessHeader);
				AssertEquals("We should let the user save the form when they're ready.", false, assistTask.IsInDatabase);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, workingTask.P9_Status);

				assistTask.Factory.Save();

				workingTask.Reload();
				AssertEquals("Since the current user is not assisting, their previously working task should remain unchanged.", ProcessTaskStatusCodeList.Codes.Working, workingTask.P9_Status);
			}
		}

		[RequiresSTA]
		public void TestAddAssistanceTaskForMenuItem_StaffSubMenu_OnClick_DoNotCreateDuplicateAssistTask()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "ORG");

			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var anotherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobLevelWorkflow = helper.GetJobHeaderForParent(orgHeader, Factory);
			var workflow = jobLevelWorkflow.ProcessHeaders[0];

			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Other user's task";
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;
			task.P9_EstDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 1, 0, 0);
			task.P9_EstimateVariationFactor = 4;
			task.P9_Sequence = 5;
			task.P9_FH_ProcessHeader = workflow.PK;

			var workingTask = orgHeader.WorkflowItems.Tasks.AddNew();
			workingTask.P9_Description = "Task I'm already working on";
			workingTask.P9_Type = "INV";
			workingTask.P9_GS_NKAssignedStaffMember = currentUser;
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			workingTask.P9_FH_ProcessHeader = workflow.PK;

			var existingAssistTask = orgHeader.WorkflowItems.Tasks.AddNew();
			existingAssistTask.P9_Description = "Assist";
			existingAssistTask.P9_Type = "AST";
			existingAssistTask.P9_GS_NKAssignedStaffMember = anotherUser;
			existingAssistTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			existingAssistTask.P9_Sequence = 5;
			existingAssistTask.P9_FH_ProcessHeader = workflow.PK;

			Factory.Save();

			helper.DisableBMSInRegistry();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForStaffSubMenu>().SingleOrDefault();
				staffSubMenu.ShowPopupMenu();

				Application.DoEvents();

				var addAssistanceMenuItem = staffSubMenu.MenuItems.OfType<AddAssistanceTaskForStaffMenuItem>().SingleOrDefault(m => m.Caption.ToString().Contains(anotherUser));
				addAssistanceMenuItem.PerformClick();

				Application.DoEvents();

				AssertEquals("A new task should not be added.", 4, grid.VisibleRowCount);
				AssertEquals("A new task should not be added.", 3, taskControl.Tasks.Count);

				AssertEquals("An appropriate assistance task for this user already exists.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestAddAssistanceTaskForMenuItem_StaffSubMenu_OnClick_ShouldCreateTaskEvenWhenACapabilityAssistTaskExists()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "ORG");

			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var anotherUser = Factory.NewWithValidTestData<GlbStaff>();
			var currentUser = Env.CurrentUser.Initials;

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var pivot = anotherUser.CapabilityPivots.AddNew();
			pivot.G5_G4_Capability = capability.PK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobLevelWorkflow = helper.GetJobHeaderForParent(orgHeader, Factory);
			var workflow = jobLevelWorkflow.ProcessHeaders[0];

			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Other user's task";
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;
			task.P9_EstDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 1, 0, 0);
			task.P9_EstimateVariationFactor = 4;
			task.P9_Sequence = 5;
			task.P9_FH_ProcessHeader = workflow.PK;

			var workingTask = orgHeader.WorkflowItems.Tasks.AddNew();
			workingTask.P9_Description = "Task I'm already working on";
			workingTask.P9_Type = "INV";
			workingTask.P9_GS_NKAssignedStaffMember = currentUser;
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			workingTask.P9_FH_ProcessHeader = workflow.PK;

			var existingAssistTask = orgHeader.WorkflowItems.Tasks.AddNew();
			existingAssistTask.P9_Description = "Assist";
			existingAssistTask.P9_Type = "AST";
			existingAssistTask.P9_G4_RequiredCapability = capability.PK;
			existingAssistTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			existingAssistTask.P9_Sequence = 5;
			existingAssistTask.P9_FH_ProcessHeader = workflow.PK;

			var anotherTask = orgHeader.WorkflowItems.Tasks.AddNew();
			anotherTask.P9_Description = "Another task";
			anotherTask.P9_Type = "INV";
			anotherTask.P9_GS_NKAssignedStaffMember = anotherUser.GS_Code;
			anotherTask.P9_EstDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 1, 0, 0);
			anotherTask.P9_EstimateVariationFactor = 4;
			anotherTask.P9_Sequence = 6;
			anotherTask.P9_FH_ProcessHeader = workflow.PK;

			Factory.Save();

			helper.DisableBMSInRegistry();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForStaffSubMenu>().SingleOrDefault();
				staffSubMenu.ShowPopupMenu();

				Application.DoEvents();

				var addAssistanceMenuItem = staffSubMenu.MenuItems.OfType<AddAssistanceTaskForStaffMenuItem>().SingleOrDefault(m => m.Caption.ToString().Contains(anotherUser.GS_Code));
				addAssistanceMenuItem.PerformClick();

				Application.DoEvents();

				AssertEquals("A new task should be added.", 6, grid.VisibleRowCount);
				AssertEquals("A new task should be added.", 5, taskControl.Tasks.Count);

				var assistTask = (ProcessTask)taskControl.Tasks.SingleOrDefault(x => !x.IsInDatabase);
				AssertNotNull(assistTask);

				AssertEquals(orgHeader.PK, assistTask.Parent.PK);
				AssertEquals("OH", assistTask.P9_ParentTableCode);
				AssertEquals("AST", assistTask.P9_Type);
				AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
				AssertEquals(5, assistTask.P9_Sequence);
				AssertEquals(anotherUser.GS_Code, assistTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ZGuid.Empty, assistTask.P9_G4_RequiredCapability);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
				AssertEquals("Assist", assistTask.P9_Description);
				AssertNotNull(assistTask.ProcessHeader);
				AssertEquals("We should let the user save the form when they're ready.", false, assistTask.IsInDatabase);
			}
		}

		public void TestAddAssistanceTaskForMenuItem_CapabilitySubMenu_OnClick()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Other user's task";
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_EstDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 1, 0, 0);
			task.P9_EstimateVariationFactor = 4;
			task.P9_Sequence = 5;

			var workingTask = orgHeader.WorkflowItems.Tasks.AddNew();
			workingTask.P9_Description = "Task I'm already working on";
			workingTask.P9_Type = "INV";
			workingTask.P9_GS_NKAssignedStaffMember = currentUser;
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForCapabilitySubMenu>().SingleOrDefault();
				capabilitySubMenu.ShowPopupMenu();

				Application.DoEvents();

				var addAssistanceMenuItem = capabilitySubMenu.MenuItems.OfType<AddAssistanceTaskForCapabilityMenuItem>().SingleOrDefault();
				addAssistanceMenuItem.PerformClick();

				Application.DoEvents();

				AssertEquals("A new task should be added.", 4, grid.VisibleRowCount);
				AssertEquals("A new task should be added.", 3, taskControl.Tasks.Count);

				var assistTask = (ProcessTask)taskControl.Tasks.SingleOrDefault(x => !x.IsInDatabase);
				AssertNotNull(assistTask);

				AssertEquals(orgHeader.PK, assistTask.Parent.PK);
				AssertEquals("OH", assistTask.P9_ParentTableCode);
				AssertEquals("AST", assistTask.P9_Type);
				AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
				AssertEquals(5, assistTask.P9_Sequence);
				AssertEquals(ZString.Empty, assistTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(capability.PK, assistTask.P9_G4_RequiredCapability);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
				AssertEquals("Assist", assistTask.P9_Description);
				AssertNull(assistTask.ProcessHeader);
				AssertEquals("We should let the user save the form when they're ready.", false, assistTask.IsInDatabase);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, workingTask.P9_Status);

				assistTask.Factory.Save();

				workingTask.Reload();
				AssertEquals("Since the current user is not assisting, their previously working task should remain unchanged.", ProcessTaskStatusCodeList.Codes.Working, workingTask.P9_Status);
			}
		}

		public void TestAddAssistanceTaskForMenuItem_CapabilitySubMenu_OnClick_DoNotCreateDuplicateAssistTask()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "ORG");

			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobLevelWorkflow = helper.GetJobHeaderForParent(orgHeader, Factory);
			var workflow = jobLevelWorkflow.ProcessHeaders[0];

			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Other user's task";
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_EstDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 1, 0, 0);
			task.P9_EstimateVariationFactor = 4;
			task.P9_Sequence = 5;
			task.P9_FH_ProcessHeader = workflow.PK;

			var workingTask = orgHeader.WorkflowItems.Tasks.AddNew();
			workingTask.P9_Description = "Task I'm already working on";
			workingTask.P9_Type = "INV";
			workingTask.P9_GS_NKAssignedStaffMember = currentUser;
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			workingTask.P9_FH_ProcessHeader = workflow.PK;

			var existingAssistTask = orgHeader.WorkflowItems.Tasks.AddNew();
			existingAssistTask.P9_Description = "Assist";
			existingAssistTask.P9_Type = "AST";
			existingAssistTask.P9_G4_RequiredCapability = capability.PK;
			existingAssistTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			existingAssistTask.P9_Sequence = 5;
			existingAssistTask.P9_FH_ProcessHeader = workflow.PK;

			Factory.Save();

			helper.DisableBMSInRegistry();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForCapabilitySubMenu>().SingleOrDefault();
				capabilitySubMenu.ShowPopupMenu();

				Application.DoEvents();

				var addAssistanceMenuItem = capabilitySubMenu.MenuItems.OfType<AddAssistanceTaskForCapabilityMenuItem>().SingleOrDefault();
				addAssistanceMenuItem.PerformClick();

				Application.DoEvents();

				AssertEquals("A new task should not be added.", 4, grid.VisibleRowCount);
				AssertEquals("A new task should not be added.", 3, taskControl.Tasks.Count);

				AssertEquals("An appropriate assistance task for this capability already exists.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestAssistMenuItems_WhenWorkflowTypeNotConfiguredInRegistry_ShouldNotAppearInMenu()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", "INV", "AST");
			MasterFilesTestHelper.AddTaskTypesToRegistry("WKI", "INV", "AST");
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("WKI", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Other user's task";
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;
			task.P9_EstDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 1, 0, 0);
			task.P9_EstimateVariationFactor = 4;
			task.P9_Sequence = 5;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var hasAssistWithThisTaskMenuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().Any();
				var hasAddAssistanceTaskForMenuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().Any();
				AssertEquals("Assist With This Task isn't set up in the registry for the ORG workflow type, so this menu item shouldn't appear for this task.", false, hasAssistWithThisTaskMenuItem);
				AssertEquals("Likewise it wouldn't make sense for Add Assistance Task For to be visible if the ORG workflow type is not configured.", false, hasAddAssistanceTaskForMenuItem);
			}
		}

		public void TestAssistMenuItemsVisibility_ForTasksAssignedToTheCurrentUser()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", "INV", "AST");
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Other user's task";
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;

			var workingTask = orgHeader.WorkflowItems.Tasks.AddNew();
			workingTask.P9_Description = "Task assigned to me";
			workingTask.P9_Type = "INV";
			workingTask.P9_GS_NKAssignedStaffMember = currentUser;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workingTask))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Task assigned to me", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var hasAssistWithThisTaskMenuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().Any();
				AssertEquals("Assist With This Task shouldn't appear on tasks that are assigned to the current user, despite the often-quoted mantra of 'How can you help others if you can't even help yourself?'", false, hasAssistWithThisTaskMenuItem);
				var hasAddAssistanceTaskForMenuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().Any();
				AssertEquals("Shouldn't the Add Assistance Task For menu be visible for the current user, since they typically want to seek advice on their own task from other people?", true, hasAddAssistanceTaskForMenuItem);
			}
		}

		public void TestAssistMenuItems_ShouldNotAppearWhenMultipleRowsAreSelected()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", "INV", "AST");
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Other user's task";
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;

			var workingTask = orgHeader.WorkflowItems.Tasks.AddNew();
			workingTask.P9_Description = "Task assigned to me";
			workingTask.P9_Type = "INV";
			workingTask.P9_GS_NKAssignedStaffMember = currentUser;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				grid.SelectAllElements(x => x != null);
				AssertEquals(2, grid.SelectedRowCount);

				grid.ContextMenu.DoPopup();
				var hasAssistWithThisTaskMenuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().Any();
				AssertEquals("Assist With This Task doesn't make sense when multiple rows are selected. It says 'Assist with this TASK'... we're only allowed to have one!", false, hasAssistWithThisTaskMenuItem);
				var hasAddAssistanceTaskForMenuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().Any();
				AssertEquals("Add Assistance Task For doesn't make sense when multiple rows are selected. We only want assistance for ONE task, right?", false, hasAddAssistanceTaskForMenuItem);
			}
		}

		[RequiresSTA]
		public void TestAssistMenuItems_ShouldNotAppearWhenActualTaskIsNotSelected()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", "INV", "AST");
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var controller = ZControllerFactory.Instance.GetControllerForBizo(orgHeader);

			using (var form = (ZOrganisationsForm)controller.ShowEditForm(orgHeader))
			{
				Application.DoEvents();

				var tabPage = form.FindSingle<ZWorkflowTabPage>();
				var zTabControl = (ZTabControl)tabPage.Parent;
				zTabControl.SelectedTab = tabPage;
				Application.DoEvents();

				var taskControl = form.FindSingle<TasksControl>();
				var grid = taskControl.TasksGrid;
				AssertNull("Precondition: The new row is selected, not an actual task", grid.GetCurrent());

				grid.ContextMenu.DoPopup();
				var hasAssistWithThisTaskMenuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().Any();
				AssertEquals("Assist With This Task can obviously only be used on a real task.", false, hasAssistWithThisTaskMenuItem);
				var hasAddAssistanceTaskForMenuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().Any();
				AssertEquals("Add Assistance Task For can obviously only be used on a real task.", false, hasAddAssistanceTaskForMenuItem);
			}
		}

		#endregion

		#region Test Classes

		class DummyCollectionContainer : NonPersistentBusinessObject
		{
			public DummyCollectionContainer(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ProcessTaskCollection WorkflowItems
			{
				get
				{
					if (workflowItems == null)
					{
						workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection(this));
						RegisterEditableChildObject(workflowItems);
					}
					return workflowItems;
				}
			}

			public TemplateProcessTaskCollection TemplateWorkflowItems
			{
				get
				{
					if (templateWorkflowItems == null)
					{
						ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
						templateWorkflowItems = this.GetOrCreateProcessTaskCollectionWithKey("Template", () => new TemplateProcessTaskCollection(template));
						RegisterEditableChildObject(templateWorkflowItems);
					}
					return templateWorkflowItems;
				}
			}

			ProcessTaskCollection workflowItems;
			TemplateProcessTaskCollection templateWorkflowItems;
		}

		#endregion

		#region Implementation

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm();
				}
				return form;
			}
		}
		ZForm form;

		TasksControl TasksControl
		{
			get { return tasksControl ?? (tasksControl = new TasksControl()); }
		}
		TasksControl tasksControl;

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		TasksControl GetTasksControl(ZForm form)
		{
			return ZTestFormUtilities.GetControlsRecursively<TasksControl>(form)[0];
		}

		Control FindSubControlByName(Control parentControl, string controlName)
		{
			Control result = null;

			if (parentControl.Name == controlName)
			{
				result = parentControl;
			}
			else if (parentControl.Controls.Count > 0)
			{
				foreach (Control subControl in parentControl.Controls)
				{
					result = FindSubControlByName(subControl, controlName);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		OrgOpportunity CreateOpportunityForTaskDetailsFormTest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_OpportunityType = opportunity.Lookups.Types[0].Code;
			opportunity.P8_OpportunityDescription = "Bah bah moo moo";
			opportunity.P8_Stage = opportunity.Lookups.Stages[0].Code;
			opportunity.P8_Status = opportunity.Lookups.Statuses[0].Code;
			return opportunity;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (tasksControl != null)
			{
				tasksControl.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
