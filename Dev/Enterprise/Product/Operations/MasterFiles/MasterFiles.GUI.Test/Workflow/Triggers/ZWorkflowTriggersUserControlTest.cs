using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TestForm))]
	sealed class ZWorkflowTriggersUserControlTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestMacroCodeFieldType()
		{
			var fieldNameColumnStyle = WorkflowTriggersUserControl.CompletionTriggerActionGrid.GetColumnStyle("PQ_FieldName");
			AssertEquals(nameof(ProcessTaskNotification.MacroCodeFieldType), ((TriggerConditionValueColumnStyleInfo)fieldNameColumnStyle).FieldTypeColumnName);

			fieldNameColumnStyle = WorkflowTriggersUserControl.CompletionTriggerActionGrid.GetColumnStyle("PQ_FieldValue");
			AssertEquals(nameof(ProcessTaskNotification.MacroCodeFieldType), ((TriggerConditionValueColumnStyleInfo)fieldNameColumnStyle).FieldTypeColumnName);
		}

		public void TestNavigateToWorkflowItem()
		{
			using (Form)
			using (var control = new TestTriggersUserControl())
			{
				Form.Controls.Add(control);
				Form.Show();
				Application.DoEvents();
				control.SetDataBinding(Dummy.WorkflowItems.TriggersIncludingRelated, "");

				var trigger1 = Dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
				var trigger2 = Dummy.WorkflowItems.TriggersIncludingRelated.AddNew();

				control.WorkflowTriggersGrid.List.Add(trigger1);
				control.WorkflowTriggersGrid.List.Add(trigger2);

				control.NavigateToWorkflowItem(trigger2);
				Assert(control.WorkflowTriggersGrid.ListManager.GetCurrent() == trigger2);

				control.NavigateToWorkflowItem(trigger1);
				Assert(control.WorkflowTriggersGrid.ListManager.GetCurrent() == trigger1);
			}
		}

		public void TestMCRHasDotDotDot()
		{
			var trigger1 = Dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
			trigger1.TriggerConditions.TriggerCondition = "MCR";
			trigger1.TriggerConditions.TriggerConditionValue = "\"1\"==\"1\"";

			Dummy.Factory.Save();
			using (Form)
			using (var control = new TestTriggersUserControl())
			{
				Form.Controls.Add(control);
				Form.Show();
				Application.DoEvents();
				control.SetDataBinding(Dummy.WorkflowItems.TriggersIncludingRelated, "");
				Application.DoEvents();

				control.NavigateToWorkflowItem(trigger1);
				var columnInfo = control.WorkflowTriggersGrid.ColumnStyles.OfType<TriggerConditionValueColumnStyleInfo>().First();
				var colCount = control.WorkflowTriggersGrid.ColumnStyles.IndexOf(columnInfo);
				control.WorkflowTriggersGrid.CurrentCell = new DataGridCell(0, colCount);
				Application.DoEvents();
				var textBox = control.WorkflowTriggersGrid.Controls.OfType<DataGridTextBox>().First(f => f.Text == trigger1.TriggerConditions.TriggerConditionValue);

				var rect = control.WorkflowTriggersGrid.GetCellBounds(0, colCount);
				control.WorkflowTriggersGrid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, rect.X + 3, rect.Y + 2, 0));
				Application.DoEvents();

				var column = control.WorkflowTriggersGrid.Columns.First(f => f.ColumnName.Contains("ConditionValue"));
				Assert(((ZMultiControlColumnStyle)column.ColumnStyle).EditControl is TriggerConditionValueControl);
			}
		}

		public void TestSettingPropertyThatAffectsWorkflowDoesNotLoadStaleData()
		{
			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TestTriggersUserControl())
			using (var customFieldsControl = new ProcessTemplateCustomFieldsControl())
			{
				var job = Factory.New<DummyWithWorkflow>();
				job.InitRelatedDummyWithTasks();

				form.Controls.Add(new ZTextBox());
				form.Controls.Add(customFieldsControl);
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.TriggersIncludingRelated, string.Empty);
				customFieldsControl.SetDataBinding(job, string.Empty);
				form.Show();

				var task = job.WorkflowItems.TriggersIncludingRelated.AddNew();
				control.WorkflowTriggersGrid.List.Add(task);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();

				((ProcessTask)control.WorkflowTriggersGrid.ListManager.GetCurrent()).P9_Status = "CLS";
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);

				job.SubType1 = "USA";
				AssertEquals("Task Status Should not revert back to Working", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			}
		}

		public void TestUniversalTriggerShouldFireOnTemplateApplication()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			universalTemplate.P0_SubType1 = "USA";

			var universalTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			universalTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			universalTrigger.Description = "Tagged";
			universalTrigger.TriggerFiredCountdown = 1;

			Factory.Save();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TestTriggersUserControl())
			using (var customFieldsControl = new ProcessTemplateCustomFieldsControl())
			{
				var job = Factory.New<DummyWithWorkflow>();
				job.InitRelatedDummyWithTasks();
				job.SubType1 = "COL";

				form.Controls.Add(new ZTextBox());
				form.Controls.Add(customFieldsControl);
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.TriggersIncludingRelated, string.Empty);
				customFieldsControl.SetDataBinding(job, string.Empty);
				form.Show();

				//Open workflow tab
				var view = job.WorkflowItems.TriggersIncludingRelated;

				job.SubType1 = "USA";
				job.Logs.AddNew(new EventValue(Events.CustomisableEvent00));

				AssertEquals("Universal Trigger should be applied when SubType1 Changed", 1, control.WorkflowTriggersGrid.List.Count);
				AssertEquals(universalTrigger.Identifier, ((ProcessTask)control.WorkflowTriggersGrid.List[0]).P9_ParentTemplateID);
				AssertEquals(new ZShort(0), job.WorkflowItems.Triggers[0].TriggerConditions.TriggerFiredCountdown);
			}
		}

		#region Macro Expressions

		[RequiresSTA]
		public void TestPQMacroTypeCode_ColumnType_IsDropdown()
		{
			var columnInfo = WorkflowTriggersUserControl.CompletionTriggerActionGrid.GetColumnStyle("PQ_MacroTypeCode");
			AssertEquals(typeof(ZDropEditColumnStyle), columnInfo.ColumnStyleType);
		}

		[RequiresSTA]
		public void TestPQFieldName_ColumnType_IsMacroColumnStyle()
		{
			var columnInfo = WorkflowTriggersUserControl.CompletionTriggerActionGrid.GetColumnStyle("PQ_FieldName");

			AssertEquals(typeof(TriggerConditionValueColumnStyle), columnInfo.ColumnStyleType);
			AssertEquals("MacroCodeFieldType", ((TriggerConditionValueColumnStyleInfo)columnInfo).FieldTypeColumnName);
		}

		[RequiresSTA]
		public void TestPQFieldValue_ColumnType_IsMacroColumnStyle()
		{
			var columnInfo = WorkflowTriggersUserControl.CompletionTriggerActionGrid.GetColumnStyle("PQ_FieldValue");

			AssertEquals(typeof(TriggerConditionValueColumnStyle), columnInfo.ColumnStyleType);
			AssertEquals("MacroCodeFieldType", ((TriggerConditionValueColumnStyleInfo)columnInfo).FieldTypeColumnName);
		}

		#endregion

		#region Handy Hints

		public void TestTooManyRelatedItems_ShowTooManyRelatedItemsHint()
		{
			WorkflowDataRegistry.Instance.RelatedWorkflowItemDisplayLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			var dummiesToCreate = 10;
			var dummies = new List<DummyWithWorkflow>(dummiesToCreate);
			for (int i = 0; i < dummiesToCreate; i++)
			{
				var dummy = Factory.New<DummyWithWorkflow>();
				dummy.GetRelatedWorkflowProviders_ForTest = () => dummies.Except(new[] { dummy });
				dummies.Add(dummy);
			}
			Factory.Save();

			var job = dummies[0];
			using (var form = new ZForm { Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new ZWorkflowTriggersUserControl())
			{
				form.Controls.Add(control);
				form.ControllerID = ControllerIDs.Organisation;
				control.SetDataBinding(job.WorkflowItems.TriggersIncludingRelated, string.Empty);
				form.Show();
				var workflowTasksControl = (IWorkflowTasksControl)control;

				Application.DoEvents();
				var splitContainer = workflowTasksControl.TasksHintSplitContainer;
				AssertEquals("Some tasks are not shown since they are specific to another company. We should show thie hint label.", false, splitContainer.Panel1Collapsed);
				var label = workflowTasksControl.TasksHintLabel;
				AssertEquals($"Related items are not shown as more than {5} related items were found. (Configurable via {WorkflowDataRegistry.Instance.RelatedWorkflowItemDisplayLimit.HumanReadableRegistryPath()})", label.Text);
			}
		}

		#endregion

		#region Link Label

		public void TestCreateWorkflowTriggersLinkLabel()
		{
			ProcessTaskTemplate processTaskTemplate = this.ProcessTaskTemplate;
			Factory.Save();

			Form.Show();
			Application.DoEvents();

			InvokeLinkLabelClick(WorkflowTriggersUserControl.CreateWorkflowTriggersLinkLabel);
			AssertEquals("Triggers should be created from the template", 1, Dummy.WorkflowItems.Triggers.Count);
			AssertEquals("TriggersIncludingRelated should be reloaded after applying template", 1, Dummy.WorkflowItems.TriggersIncludingRelated.Count);
		}

		public void TestCreateWorkflowTriggersLinkLabel_WhenTriggersAlreadyDefined()
		{
			Form.Show();
			Application.DoEvents();

			Dummy.WorkflowItems.Triggers.AddNew();
			InvokeLinkLabelClick(WorkflowTriggersUserControl.CreateWorkflowTriggersLinkLabel);
			AssertEquals("You can only create triggers from the template if you have no triggers already entered.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateWorkflowTriggersLinkLabel_WhenNoTemplateExists()
		{
			Form.Show();
			Application.DoEvents();

			Dummy.WorkflowItems.Triggers.RemoveAndDeleteAll();
			InvokeLinkLabelClick(WorkflowTriggersUserControl.CreateWorkflowTriggersLinkLabel);
			AssertEquals("No Workflow Triggers were added from templates for the specified details.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestClickLinkLabel_WhenUniversalTriggersPresent_ShouldApplyWorkflowTemplates()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);

			var standardTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			standardTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var universalTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			universalTrigger.TriggerConditions_ForBinding.TriggerEventCode = AutoEvents.TagWasAddedOrRemovedCode;
			universalTrigger.Description = "Tagged";

			var standardTrigger = standardTemplate.WorkflowItems.Triggers.AddNew();
			standardTrigger.TriggerConditions.TriggerEventCode = AutoEvents.WorkflowTransferredBetweenSystemComponentsCode;

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TestWorkflowTriggersUserControl())
			{
				form.Controls.Add(new ZTextBox());
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Triggers, string.Empty);
				form.Show();

				AssertEquals("Universal Trigger should be applied to job already", 1, job.WorkflowItems.TriggersIncludingRelated.Count);
				AssertEquals(universalTrigger.Identifier, job.WorkflowItems.TriggersIncludingRelated[0].P9_ParentTemplateID);

				InvokeLinkLabelClick(control.CreateWorkflowTriggersLinkLabel);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Standard trigger should now be applied to job since existing trigger was a universal trigger", 2, job.WorkflowItems.TriggersIncludingRelated.Count);
				AssertNotNull(job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == standardTrigger.PK));
			}
		}

		public void TestApplyWorkflowTemplates_WhenTemplateConditionsNowMatch_ShouldApplyWithoutCallingSave()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"DNW\"";

			var templateTask = template.WorkflowItems.Tasks.AddNew();

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TriggerConditions.TriggerEventCode = AutoEvents.WorkflowTransferredBetweenSystemComponentsCode;

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TestWorkflowTriggersUserControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(new ZTextBox());
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();

				AssertEquals(0, job.WorkflowItems.Triggers.Count);
				InvokeLinkLabelClick(control.CreateWorkflowTriggersLinkLabel);
				AssertEquals(0, job.WorkflowItems.Triggers.Count);

				job.Z0_Code = "DNW";

				AssertEquals(0, job.WorkflowItems.Triggers.Count);
				InvokeLinkLabelClick(control.CreateWorkflowTriggersLinkLabel);

				AssertEquals("Trigger template condition is now met, so should be applied to job", 1, job.WorkflowItems.Triggers.Count);
				AssertEquals("Clicking hyperlink on triggers tab should not apply workflow templates for milestones", 0, job.WorkflowItems.Milestones.Count);
				AssertEquals("Clicking hyperlink on triggers tab should not apply workflow templates for tasks", 0, job.WorkflowItems.Tasks.Count);
			}
		}

		#endregion

		#region Column Visibility

		public void TestPQ_EmailTextFallbackToTemplate()
		{
			Form.Show();
			Application.DoEvents();

			AssertNull(WorkflowTriggersUserControl.CompletionTriggerActionGrid.GetColumnStyle("PQ_EmailText"));
			AssertNotNull(WorkflowTriggersUserControl.CompletionTriggerActionGrid.GetColumnStyle("PQ_EmailTextFallbackToTemplate"));
		}

		public void TestTaskID_CorrectlyDisplayed()
		{
			using (Form)
			using (TestTriggersUserControl control = new TestTriggersUserControl())
			{
				Form.Controls.Add(control);
				Form.Show();
				Application.DoEvents();
				control.SetDataBinding(Dummy.WorkflowItems.TriggersIncludingRelated, "");

				AssertEquals("TaskID is visible", false, control.WorkflowTriggersGrid.GetColumnStyle("P9_TaskID").IsVisible);
			}
		}

		[RequiresSTA]
		public void TestPQ_RelatedEntity()
		{
			using (Form)
			using (TestTriggersUserControl control = new TestTriggersUserControl())
			{
				Form.Controls.Add(control);
				Form.Show();
				Application.DoEvents();
				control.SetDataBinding(Dummy.WorkflowItems.TriggersIncludingRelated, "");

				AssertEquals("PQ_RelatedEntityId is visible", true, control.CompletionTriggerActionGrid.GetColumnStyle("PQ_RelatedEntityId").IsVisible);
				AssertNull("PQ_RelatedEntityTableCode is not present", control.CompletionTriggerActionGrid.GetColumnStyle("PQ_RelatedEntityTableCode"));
			}
		}

		#endregion

		#region ReferenceCode Column Caption

		public void TestReferenceCodeColumnCaption_WhenVisible()
		{
			Dummy.WorkflowItems.SetRequiresReferenceCode(true);
			Form.Show();
			Application.DoEvents();
			AssertEquals("ReferenceCode column visible with the correct caption when SupportsReferenceCode=false", "Dummy Reference Code Caption", WorkflowTriggersUserControl.WorkflowTriggersGrid.TableStyles[0].GridColumnStyles[ProcessTask.Schema.ReferenceCode].HeaderText);
		}

		public void TestReferenceCodeColumnCaption_WhenNotVisible()
		{
			Dummy.WorkflowItems.SetRequiresReferenceCode(false);
			Form.Show();
			Application.DoEvents();
			AssertNull("ReferenceCode column not visible when SupportsReferenceCode=false", WorkflowTriggersUserControl.WorkflowTriggersGrid.TableStyles[0].GridColumnStyles[ProcessTask.Schema.ReferenceCode]);
		}

		#endregion

		#region Workflow Diagnostic

		public void TestWorkflowDiagnosticMenuItem_Added_OnJobForm()
		{
			var job = Factory.New<DummyWithWorkflow>();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TestWorkflowTriggersUserControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();

				var milestonesGrid = control.WorkflowTriggersGrid;

				milestonesGrid.ContextMenu.ShowPopupMenu();

				var workflowDiagnosticMenuItem = milestonesGrid.ContextMenu.MenuItems.FindByText("Workflow Diagnostic");
				Assert("GIVEN job form with milestone user-control, WHEN showing context-menu SHOULD show Workflow Diagnostic", workflowDiagnosticMenuItem.Visible);
			}
		}

		public void TestWorkflowDiagnosticMenuItem_NotAdded_OnTemplateForm()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateMilestone.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"DNW\"";

			Factory.Save();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TestWorkflowTriggersUserControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				control.SetDataBinding(template.WorkflowItems.Milestones, string.Empty);
				form.Show();

				var milestonesGrid = control.WorkflowTriggersGrid;

				milestonesGrid.ContextMenu.ShowPopupMenu();

				var workflowDiagnosticMenuItem = milestonesGrid.ContextMenu.MenuItems.FindByText("Workflow Diagnostic");
				AssertEquals("GIVEN template form with milestone user-control, WHEN showing context-menu SHOULD not show Workflow Diagnostic", false, workflowDiagnosticMenuItem.Visible);
			}
		}

		#endregion

		#region Show Matching Template Form

		public void TestShowMatchingTemplateForm()
		{
			using (var control = new ZWorkflowTriggersUserControl())
			{
				AssertNotNull(control.WorkflowTriggersGrid.ContextMenu.MenuItems.Cast<MenuItem>().First(n => n.Text == WorkflowDiagnosticContextMenuManager.WorkflowTemplateMatchCaption));
			}
		}

		#endregion

		#region Show Validate Communication Modes Menu Item

		public void TestShowValidateCommunicationModesMenuItem()
		{
			using (var control = new ZWorkflowTriggersUserControl())
			{
				AssertNotNull(control.WorkflowTriggersGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(n => n.Text == "Validate Communication Modes"));
			}
		}

		#endregion

		#region Test Classes

		class TestWorkflowTriggersUserControl : ZWorkflowTriggersUserControl
		{
			public new LinkLabel CreateWorkflowTriggersLinkLabel
			{
				get { return base.CreateWorkflowTriggersLinkLabel; }
			}

			public new ZGrid WorkflowTriggersGrid
			{
				get { return base.WorkflowTriggersGrid; }
			}
		}

		class TestForm : ZChildForm
		{
			public TestForm(WorkflowTriggerCollectionView triggers)
				: base(triggers)
			{
			}

			public TestWorkflowTriggersUserControl WorkflowTriggersUserControl
			{
				get
				{
					if (workflowTriggersUserControl == null)
					{
						workflowTriggersUserControl = new TestWorkflowTriggersUserControl();
					}
					return workflowTriggersUserControl;
				}
			}
			TestWorkflowTriggersUserControl workflowTriggersUserControl;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				var control = new TextBox { Name = "No bash" };
				Controls.Add(control);
				Controls.Add(WorkflowTriggersUserControl);
				WorkflowTriggersUserControl.Top = control.Height;

				this.CaptionRenderingEnabled = true;
			}
		}

		#endregion

		#region Implementation

		void InvokeLinkLabelClick(LinkLabel linkLabel)
		{
			linkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, linkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });
		}

		ProcessTaskTemplate ProcessTaskTemplate
		{
			get
			{
				if (processTaskTemplate == null)
				{
					processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					processTaskTemplate.P0_ProcessType = "DUM";
					ProcessTask task = processTaskTemplate.WorkflowItems.Tasks.AddNew();
					ProcessTask milestone = processTaskTemplate.WorkflowItems.Triggers.AddNew();
				}
				return processTaskTemplate;
			}
		}
		ProcessTaskTemplate processTaskTemplate;

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

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		class TestTriggersUserControl : ZWorkflowTriggersUserControl
		{
			public new LinkLabel CreateWorkflowTriggersLinkLabel
			{
				get { return base.CreateWorkflowTriggersLinkLabel; }
			}

			public new ZGrid WorkflowTriggersGrid
			{
				get { return base.WorkflowTriggersGrid; }
			}
		}

		TestForm Form
		{
			get
			{
				if (form == null)
				{
					form = new TestForm(Dummy.WorkflowItems.Triggers);
					form.ControllerID = DummyControllerIDs.Dummy;
					form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 700);
				}
				return form;
			}
		}
		TestForm form;

		TestWorkflowTriggersUserControl WorkflowTriggersUserControl
		{
			get { return Form.WorkflowTriggersUserControl; }
		}

		protected override void BashControl(Control controlToBash)
		{
			if (controlToBash.Name != "No bash")
			{
				base.BashControl(controlToBash);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return Form;
		}

		#endregion
	}
}
