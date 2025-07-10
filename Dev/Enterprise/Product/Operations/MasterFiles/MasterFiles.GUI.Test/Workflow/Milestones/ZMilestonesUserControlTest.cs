using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
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
	[TestedType(typeof(ZChildForm))]
	sealed class ZMilestonesUserControlTest : ZMilestonesOrExceptionsUserControlTest
	{
		public void TestShowIndex()
		{
			using (var zMilestonesUserControlForTest = new ZMilestonesUserControl())
			{
				var fieldNameColumnStyle = zMilestonesUserControlForTest.TriggersGrid.GetColumnStyle("PQ_FieldName");
				AssertEquals(false, ((ZMacrosFindBoxColumnStyleInfo)fieldNameColumnStyle).ShowIndex);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestAddingOnCellChange()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "m1";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			Factory.Save();

			using (var form = new ZForm())
			{
				form.SetReadOnly(true);
				var grid = new ZGrid();
				grid.ReadOnly = true;
				var col = new ZTextBoxColumnStyleInfo();
				col.ColumnName = "PQ_EmailText";
				grid.ColumnStyles.Add(col);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.SetDataBinding(milestone, "ProcessTaskNotifications");
				grid.SetBindingMember(".");
				var count = 0;
				bool recur = false;
				grid.CurrentCellChanged += (s, e) =>
				{
					var lastRecur = recur;
					if (count < 10 && !recur)
					{
						recur = true;
						count++;
						var notification = Factory.New<ProcessTaskNotification>();
						notification.PQ_P9 = milestone.PK;
					}
					recur = lastRecur;
				};

				form.Show();
				Application.DoEvents();
				grid.Controls.Cast<Control>().First().Select();
				Application.DoEvents();
			}
		}

		[ExpectNoExceptions]
		public void TestAddItemOnInit()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "m1";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			Factory.Save();

			using (var form = new ZForm())
			{
				form.SetReadOnly(true);
				var grid = new ZGrid();
				grid.ReadOnly = true;
				var col = new ZTextBoxColumnStyleInfo();
				col.ColumnName = "PQ_EmailText";
				grid.ColumnStyles.Add(col);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.SetDataBinding(milestone, "ProcessTaskNotifications");
				grid.SetBindingMember(".");
				var count = 0;
				bool recur = false;

				((ZBindingContext)grid.BindingContext).CollectionChanged += (s, e) =>
				{
					var lastRecur = recur;
					if (count < 10 && !recur)
					{
						recur = true;
						count++;
						var notification = Factory.New<ProcessTaskNotification>();
						notification.PQ_P9 = milestone.PK;
					}
					recur = lastRecur;
				};
				form.Show();
				Application.DoEvents();
				grid.Controls.Cast<Control>().First().Select();
				Application.DoEvents();
			}
		}

		#region Exception Alarm

		public void TestAlarmNotificationIcon()
		{
			Form.Show();
			Application.DoEvents();

			ProcessTask milestone = Dummy.WorkflowItems.MilestonesIncludingRelated.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Now.AddDays(-1));

			byte[] alarmBellIcon = ToByteArray(Icons.GetIcon(IconTypes.RedAlarmBell).ToBitmap());

			ProcessTask exception = milestone.CreateMilestoneException();
			Factory.Save();
			AssertEquals("Active exception icon shown at left of grid row", alarmBellIcon, ToByteArray(GetRowNotificationImageFromListIndex(MilestonesUserControl.MilestoneGrid, 0)));

			exception.IsExceptionActioned = true;
			AssertNotEquals("Active exception icon goes away when exception is resolved", alarmBellIcon, ToByteArray(GetRowNotificationImageFromListIndex(MilestonesUserControl.MilestoneGrid, 0)));
		}

		#endregion

		#region Link Label

		public void TestCreateMilestonesLinkLabel()
		{
			ProcessTaskTemplate processTaskTemplate = this.WorkflowTemplate;
			Factory.Save();

			Form.Show();
			Application.DoEvents();

			InvokeLinkLabelClick(MilestonesUserControl.CreateMilestonesLinkLabel);
			AssertEquals("Milestones should be created from the template", 1, Dummy.WorkflowItems.Milestones.Count);
		}

		[RequiresSTA]
		public void TestCreateMilestonesLinkLabel_WhenMilestonesAlreadyDefined()
		{
			Form.Show();
			Application.DoEvents();

			Dummy.WorkflowItems.Milestones.AddNew();
			InvokeLinkLabelClick(MilestonesUserControl.CreateMilestonesLinkLabel);
			AssertEquals("You can only create milestones from the template if you have no milestones already entered.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateMilestonesLinkLabel_WhenNoTemplateExists()
		{
			Form.Show();
			Application.DoEvents();

			Dummy.WorkflowItems.Milestones.RemoveAndDeleteAll();
			InvokeLinkLabelClick(MilestonesUserControl.CreateMilestonesLinkLabel);
			AssertEquals("No Milestones were added from templates for the specified details.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestApplyWorkflowTemplates_WhenTemplateConditionsNowMatch_ShouldApplyWithoutCallingSave()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateMilestone.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"DNW\"";

			var templateTask = template.WorkflowItems.Tasks.AddNew();

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.WorkflowTransferredBetweenSystemComponentsCode;

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TestMilestonesUserControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(new ZTextBox());
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();

				AssertEquals(0, job.WorkflowItems.Count);
				InvokeLinkLabelClick(control.CreateMilestonesLinkLabel);
				AssertEquals(0, job.WorkflowItems.Count);

				job.Z0_Code = "DNW";

				AssertEquals(0, job.WorkflowItems.Count);
				InvokeLinkLabelClick(control.CreateMilestonesLinkLabel);
				AssertEquals(1, job.WorkflowItems.Count);

				AssertEquals("Milestone template condition is now met, so should be applied to job", 1, job.WorkflowItems.Milestones.Count);
				AssertEquals("Clicking hyperlink on milestones tab should not apply workflow templates for triggers", 0, job.WorkflowItems.Triggers.Count);
				AssertEquals("Clicking hyperlink on milestones tab should not apply workflow templates for tasks", 0, job.WorkflowItems.Tasks.Count);
			}
		}

		#endregion

		#region Workflow Diagnostic

		[RequiresSTA]
		public void TestWorkflowDiagnosticMenuItem_Added_OnJobForm()
		{
			var job = Factory.New<DummyWithWorkflow>();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TestMilestonesUserControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				control.SetDataBinding(job.WorkflowItems.Tasks, string.Empty);
				form.Show();

				var milestonesGrid = control.MilestoneGrid;

				milestonesGrid.ContextMenu.ShowPopupMenu();

				var workflowDiagnosticMenuItem = milestonesGrid.ContextMenu.MenuItems.FindByText("Workflow Diagnostic");
				Assert("GIVEN job form with milestone user-control, WHEN showing context-menu SHOULD show Workflow Diagnostic", workflowDiagnosticMenuItem.Visible);
			}
		}

		[RequiresSTA]
		public void TestWorkflowDiagnosticMenuItem_NotAdded_OnTemplateForm()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateMilestone.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"DNW\"";

			Factory.Save();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new TestMilestonesUserControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				control.SetDataBinding(template.WorkflowItems.Milestones, string.Empty);
				form.Show();

				var milestonesGrid = control.MilestoneGrid;

				AssertEquals("PRE: The item in the grid should be the one from before", templateMilestone, milestonesGrid.ListManager.GetCurrent());

				milestonesGrid.ContextMenu.ShowPopupMenu();

				var workflowDiagnosticMenuItem = milestonesGrid.ContextMenu.MenuItems.FindByText("Workflow Diagnostic");
				AssertEquals("GIVEN template form with milestone user-control, WHEN showing context-menu SHOULD not show Workflow Diagnostic", false, workflowDiagnosticMenuItem.Visible);
			}
		}

		#endregion

		#region Show Matching Template Form

		public void TestShowMatchingTemplateForm()
		{
			using (var control = new ZMilestonesUserControl())
			{
				AssertNotNull(control.MilestoneGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(n => n.Text == WorkflowDiagnosticContextMenuManager.WorkflowTemplateMatchCaption));
			}
		}

		#endregion

		#region NavigateToWorkflowItem

		public void TestNavigateToWorkflowItem()
		{
			using (Form)
			using (TestMilestonesUserControl control = new TestMilestonesUserControl())
			{
				Form.Controls.Add(control);
				Form.Show();
				Application.DoEvents();
				control.SetDataBinding(Dummy.WorkflowItems.MilestonesIncludingRelated, "");

				ProcessTask milestone1 = Dummy.WorkflowItems.MilestonesIncludingRelated.AddNew();
				ProcessTask milestone2 = Dummy.WorkflowItems.MilestonesIncludingRelated.AddNew();

				control.MilestoneGrid.List.Add(milestone1);
				control.MilestoneGrid.List.Add(milestone2);

				control.NavigateToWorkflowItem(milestone2);
				Assert(control.MilestoneGrid.ListManager.GetCurrent() == milestone2);

				control.NavigateToWorkflowItem(milestone1);
				Assert(control.MilestoneGrid.ListManager.GetCurrent() == milestone1);
			}
		}

		#endregion

		#region Column Visibility

		public void TestPQ_EmailTextFallbackToTemplate()
		{
			using (Form)
			using (TestMilestonesUserControl control = new TestMilestonesUserControl())
			{
				Form.Controls.Add(control);
				Form.Show();
				Application.DoEvents();
				control.SetDataBinding(Dummy.WorkflowItems.MilestonesIncludingRelated, "");

				AssertNull(control.TriggersGrid.GetColumnStyle("PQ_EmailText"));
				AssertNotNull(control.TriggersGrid.GetColumnStyle("PQ_EmailTextFallbackToTemplate"));
			}
		}

		[RequiresSTA]
		public void TestTaskID_CorrectlyDisplayed()
		{
			using (Form)
			using (TestMilestonesUserControl control = new TestMilestonesUserControl())
			{
				Form.Controls.Add(control);
				Form.Show();
				Application.DoEvents();
				control.SetDataBinding(Dummy.WorkflowItems.MilestonesIncludingRelated, "");

				AssertEquals("TaskID is visible", false, control.MilestoneGrid.GetColumnStyle("P9_TaskID").IsVisible);
			}
		}

		public void TestP9_AcualDateUpdateType()
		{
			using (Form)
			using (TestMilestonesUserControl control = new TestMilestonesUserControl())
			{
				Form.Controls.Add(control);
				Form.Show();
				Application.DoEvents();
				control.SetDataBinding(Dummy.WorkflowItems.MilestonesIncludingRelated, "");

				var column = control.MilestoneGrid.GetColumnStyle(ProcessTasksSchema.P9_ActualDateUpdateType.Name);
				AssertEquals("P9_ActualDateUpdateType is visible", true, column.IsVisible);
			}
		}

		#endregion

		#region Macro Expressions

		public void TestExpressionInFieldNameAndFieldValue()
		{
			var fieldNameColumnStyle = MilestonesUserControl.TriggersGrid.GetColumnStyle("PQ_FieldName");
			var fieldValueColumnStyle = MilestonesUserControl.TriggersGrid.GetColumnStyle("PQ_FieldValue");

			AssertEquals("PQ_FieldName should accept equal-sign.", true, ((ZMacrosFindBoxColumnStyleInfo)fieldNameColumnStyle).IsUsedForExpressions);
			AssertEquals("PQ_FieldValue should accept equal-sign.", true, ((ZMacrosFindBoxColumnStyleInfo)fieldValueColumnStyle).IsUsedForExpressions);
		}

		#endregion

		#region ReferenceCode Column Caption

		public void TestReferenceCodeColumnCaption_WhenVisible()
		{
			Dummy.WorkflowItems.SetRequiresReferenceCode(true);
			Form.Show();
			Application.DoEvents();
			AssertEquals("ReferenceCode column visible with the correct caption when SupportsReferenceCode=false", "Dummy Reference Code Caption", MilestonesUserControl.MilestoneGrid.TableStyles[0].GridColumnStyles[ProcessTask.Schema.ReferenceCode].HeaderText);
		}

		public void TestReferenceCodeColumnCaption_WhenNotVisible()
		{
			Dummy.WorkflowItems.SetRequiresReferenceCode(false);
			Form.Show();
			Application.DoEvents();
			AssertNull("ReferenceCode column not visible when SupportsReferenceCode=false", MilestonesUserControl.MilestoneGrid.TableStyles[0].GridColumnStyles[ProcessTask.Schema.ReferenceCode]);
		}

		#endregion

		#region Grid Cell Info

		[TestDate(2020, 02, 12, 11, 00, 00)]
		public void TestMilestonesGrid_ShouldAlwaysShowNegativeEstimateDateDifference()
		{
			const string theImportantField = "P9_EstimatedDefaultTimeDelta";

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			CreateMilestoneWithATimeDelta(template, new ZDateTime(2020, 1, 1).AddHours(-10));
			CreateMilestoneWithATimeDelta(template, new ZDateTime(2020, 1, 1));
			CreateMilestoneWithATimeDelta(template, new ZDateTime(2020, 1, 1).AddHours(10));

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = new Size(1000, 1000) })
			using (var control = new TestMilestonesUserControl(columnToForceShow: theImportantField) { Dock = DockStyle.Fill, Size = new Size(1000, 1000) })
			{
				form.Controls.Add(control);
				control.SetDataBinding(template.WorkflowItems.Milestones, string.Empty);

				form.Show();
				Application.DoEvents();

				AssertDeltaColumnContent(control.MilestoneGrid, 1, theImportantField, "000:00");
				AssertDeltaColumnContent(control.MilestoneGrid, 0, theImportantField, "-010:00");
				AssertDeltaColumnContent(control.MilestoneGrid, 2, theImportantField, "010:00");
			}

			void AssertDeltaColumnContent(ZGrid grid, int rowToCheck, string columnNameToCheck, string expectedTimeString)
			{
				var bestBoyColumn = grid.Columns.ToList().IndexOf(grid.Columns.First(col => col.ColumnName.Equals(columnNameToCheck)));
				grid.CurrentCell = new DataGridCell(rowToCheck, bestBoyColumn);

				var deltaColumnCell = grid.Controls.ToList<Control>().First(ctrl => ctrl.Name.Equals(columnNameToCheck));
				AssertEquals("Without editing Edit mode, value should be what we expect", expectedTimeString, deltaColumnCell.Text);
				Application.DoEvents();
				AssertEquals("After entering Edit mode, value shouldn't change", expectedTimeString, deltaColumnCell.Text);
			}
		}

		void CreateMilestoneWithATimeDelta(ProcessTaskTemplate templateToUse, ZDateTime timeDelta)
		{
			var newMilestone = templateToUse.WorkflowItems.Milestones.AddNew();
			newMilestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			newMilestone.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"DNW\"";
			newMilestone.P9_EstimatedDefaultTimeDelta = timeDelta;
		}

		#endregion

		#region Test Classes

		class TestMilestonesUserControl : ZMilestonesUserControl
		{
			public new LinkLabel CreateMilestonesLinkLabel
			{
				get { return base.CreateMilestonesLinkLabel; }
			}

			public new ZGrid MilestoneGrid
			{
				get { return base.MilestoneGrid; }
			}

			public TestMilestonesUserControl(string columnToForceShow = "")
			{
				if (!string.IsNullOrEmpty(columnToForceShow))
				{
					foreach (var style in MilestoneGrid.ColumnStyles)
					{
						if (style is ZGridColumnInfo columnStyle && columnStyle.ColumnName == columnToForceShow)
						{
							columnStyle.IsVisible = true;
						}
					}

					foreach (var column in MilestoneGrid.Columns)
					{
						column.IsVisible = column.ColumnName.Equals(columnToForceShow);
					}
				}
			}
		}

		class TestForm : ZChildForm
		{
			public TestForm(MilestoneCollectionIncludingRelatedView milestones)
				: base(milestones)
			{
			}

			public TestMilestonesUserControl MilestonesUserControl
			{
				get
				{
					if (milestonesUserControl == null)
					{
						milestonesUserControl = new TestMilestonesUserControl();
					}
					return milestonesUserControl;
				}
			}
			TestMilestonesUserControl milestonesUserControl;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Controls.Add(MilestonesUserControl);
			}
		}

		#endregion

		#region Implementation

		void InvokeLinkLabelClick(LinkLabel linkLabel)
		{
			linkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, linkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });
		}

		Image GetRowNotificationImageFromListIndex(ZGrid grid, int rowIndex)
		{
			return (Image)typeof(ZGrid).InvokeMember("GetRowNotificationImageFromListIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { rowIndex });
		}

		ProcessTaskTemplate WorkflowTemplate
		{
			get
			{
				if (workflowTemplate == null)
				{
					workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					workflowTemplate.P0_ProcessType = "DUM";
					ProcessTask task = workflowTemplate.WorkflowItems.Tasks.AddNew();
					ProcessTask milestone = workflowTemplate.WorkflowItems.Milestones.AddNew();
				}
				return workflowTemplate;
			}
		}
		ProcessTaskTemplate workflowTemplate;

		TestForm Form
		{
			get
			{
				if (form == null)
				{
					form = new TestForm(Dummy.WorkflowItems.MilestonesIncludingRelated);
					form.ControllerID = DummyControllerIDs.Dummy;
				}
				return form;
			}
		}
		TestForm form;

		TestMilestonesUserControl MilestonesUserControl
		{
			get { return Form.MilestonesUserControl; }
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion

		#region Support

		static byte[] ToByteArray(Image bmp)
		{
			if (bmp == null)
			{
				return System.Array.Empty<byte>();
			}

			using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
			{
				bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
				return stream.ToArray();
			}
		}

		#endregion

		#region ZMilestonesOrExceptionsUserControlTest Overrides

		protected override WorkflowItemCollectionIncludingRelatedView GetCollectionIncludingRelatedView(ProcessTaskCollection collection)
		{
			return collection.MilestonesIncludingRelated;
		}

		protected override WorkflowItemCollectionView GetCollectionView(ProcessTaskCollection collection)
		{
			return collection.Milestones;
		}

		protected override ZMilestonesOrExceptionsUserControl GetNewUserControl()
		{
			return new ZMilestonesUserControl();
		}

		#endregion

		#region Show Validate Communication Modes Menu Item

		public void TestShowValidateCommunicationModesMenuItem()
		{
			using (var control = new ZMilestonesUserControl())
			{
				AssertNotNull(control.MilestoneGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(n => n.Text == "Validate Communication Modes"));
			}
		}

		#endregion
	}
}
