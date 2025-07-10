using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(ProcessTaskTemplateForm))]
	sealed class ProcessTaskTemplateFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestImportDataIndisplayFieldsGrid()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				form.TasksTabControl.SelectedTab = form.FormCustomizationTabPage;
				Application.DoEvents();
				var menu = form.displayFieldsGrid.ContextMenu.MenuItems.FindByText("&Import Data...");
				menu.PerformClick();
			}
		}
		public void TestGetShowIndex()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			using (var form = new ProcessTaskTemplateForm(template))
			{
				var completionTriggerActionsUserControlForTest = new CompletionTriggerActionsUserControl();
				var fieldNameColumnStyle = completionTriggerActionsUserControlForTest.CompletionTriggerActionGrid.GetColumnStyle("PQ_FieldName");
				form.Controls.Add(completionTriggerActionsUserControlForTest);
				form.Show();
				AssertEquals(false, ((ZMacrosFindBoxColumnStyleInfo)fieldNameColumnStyle).ShowIndex);
			}
		}
		public void TestShowAuditTabIsFalse()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			using (var form = new ProcessTaskTemplateForm(template))
			{
				AssertEquals("ShowAuditTab should always be set to false because adding the tab in the base class causes null error. Audit tab plugin is added in ProcessTaskTemplateForm manually in the constructor", false, form.IsShowingAuditTab);
			}
		}

		public void TestIncompatibleTasksAreRemovedOnSave()
		{
			AssertIncompatibleWorkflowItemsAreRemovedOnSave(false, true);
		}

		public void TestIncompatibleTriggersAndMilestonesAreRemovedOnSave()
		{
			AssertIncompatibleWorkflowItemsAreRemovedOnSave(true, false);
		}

		public void AssertIncompatibleWorkflowItemsAreRemovedOnSave(bool tasksSupported, bool eventTrackingSupported)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_SubType1 = "COL";
			template.P0_SubType3 = "USA";
			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Template milestone";
			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Template task";
			task.TriggerConditions.TriggerEventCode = "Z00";
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Template trigger";
			trigger.TriggerConditions.TriggerEventCode = "Z00";

			Factory.Save();

			AssertEquals(1, template.WorkflowItems.Milestones.Count);
			AssertEquals(1, template.WorkflowItems.Tasks.Count);
			AssertEquals(1, template.WorkflowItems.Triggers.Count);

			AssertEquals(false, milestone.IsDeleted);
			AssertEquals(false, task.IsDeleted);
			AssertEquals(false, trigger.IsDeleted);

			DummyWorkflowDescriptor.Instance.TasksSupported = true;
			DummyWorkflowDescriptor.Instance.EventTrackingSupported = true;

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				Application.DoEvents();
				form.ValidateAndSave();
			}

			AssertEquals(1, template.WorkflowItems.Milestones.Count);
			AssertEquals(1, template.WorkflowItems.Tasks.Count);
			AssertEquals(1, template.WorkflowItems.Triggers.Count);

			AssertEquals(false, milestone.IsDeleted);
			AssertEquals(false, task.IsDeleted);
			AssertEquals(false, trigger.IsDeleted);

			DummyWorkflowDescriptor.Instance.TasksSupported = tasksSupported;
			DummyWorkflowDescriptor.Instance.EventTrackingSupported = eventTrackingSupported;

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				Application.DoEvents();
				form.ValidateAndSave();
			}

			AssertEquals(eventTrackingSupported, template.WorkflowItems.Milestones.Count > 0);
			AssertEquals(tasksSupported, template.WorkflowItems.Tasks.Count > 0);
			AssertEquals(eventTrackingSupported, template.WorkflowItems.Triggers.Count > 0);

			AssertEquals(!eventTrackingSupported, milestone.IsDeleted);
			AssertEquals(!tasksSupported, task.IsDeleted);
			AssertEquals(!eventTrackingSupported, trigger.IsDeleted);
		}

		public void TestIncompatibleMileStonesAreRemovedOnSave()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_SubType1 = "COL";
			template.P0_SubType3 = "USA";
			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Template milestone";

			Factory.Save();

			AssertEquals(1, template.WorkflowItems.Milestones.Count);
			AssertEquals(false, milestone.IsDeleted);

			DummyWorkflowDescriptor.Instance.TasksSupported = true;
			DummyWorkflowDescriptor.Instance.EventTrackingSupported = true;

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				Application.DoEvents();
				form.ValidateAndSave();
			}

			AssertEquals(1, template.WorkflowItems.Milestones.Count);
			AssertEquals(false, milestone.IsDeleted);

			DummyWorkflowDescriptor.Instance.EventTrackingSupportedForUniversalTemplates = true;
			template.P0_IsUniversal = true;
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				Application.DoEvents();
				form.ValidateAndSave();
			}

			AssertEquals(0, template.WorkflowItems.Milestones.Count);
			Assert(milestone.IsDeleted);
		}

		#region Macro Expressions

		public void TestExpressionInFieldNameAndFieldValue()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				var milestoneCompletionTriggerActionsGrid = (ZGrid)form.MilestoneCompletionTriggerActionsControl.Controls.Find("CompletionTriggerActionGrid", true)[0];
				var triggerCompletionTriggerActionsGrid = (ZGrid)form.TriggerCompletionTriggerActionsControl.Controls.Find("CompletionTriggerActionGrid", true)[0];

				var milestonefieldNameColumnStyle = milestoneCompletionTriggerActionsGrid.GetColumnStyle("PQ_FieldName");
				var milestonefieldValueColumnStyle = milestoneCompletionTriggerActionsGrid.GetColumnStyle("PQ_FieldValue");

				AssertEquals("Milestone PQ_FieldName should accept equal-sign.", true, ((ZMacrosFindBoxColumnStyleInfo)milestonefieldNameColumnStyle).IsUsedForExpressions);
				AssertEquals("Milestone PQ_FieldValue should accept equal-sign.", true, ((ZMacrosFindBoxColumnStyleInfo)milestonefieldValueColumnStyle).IsUsedForExpressions);

				var triggerfieldNameColumnStyle = triggerCompletionTriggerActionsGrid.GetColumnStyle("PQ_FieldName");
				var triggerfieldValueColumnStyle = triggerCompletionTriggerActionsGrid.GetColumnStyle("PQ_FieldValue");

				AssertEquals("Trigger PQ_FieldName should accept equal-sign.", true, ((ZMacrosFindBoxColumnStyleInfo)triggerfieldNameColumnStyle).IsUsedForExpressions);
				AssertEquals("Trigger PQ_FieldValue should accept equal-sign.", true, ((ZMacrosFindBoxColumnStyleInfo)triggerfieldValueColumnStyle).IsUsedForExpressions);
			}
		}

		#endregion

		#region P9_ActualDateUpdateType

		public void TestP9_ActualDateUpdateType()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				AssertEquals(true, form.MilestonesGrid.GetColumnStyle(ProcessTasksSchema.P9_ActualDateUpdateType.Name).IsVisible);
				AssertEquals(false, form.WorkflowItemsTriggersGrid.Columns.Contains(ProcessTasksSchema.P9_ActualDateUpdateType.Name));
				var tasksControl = form.TasksPanel.Controls.Find("tasksControl", true).First() as TasksControl;
				AssertEquals(false, tasksControl.TasksGrid.Columns.Contains(ProcessTasksSchema.P9_ActualDateUpdateType.Name));
			}
		}

		#endregion

		#region TestFormCaption

		public void TestFormCaption()
		{
			AssertEquals("Workflow Template", Res.GetString("ProcessTaskTemplateForm|e17bfb57-b588-458f-a78e-ce7585b257b8", "Workflow Template"));
		}

		#endregion

		#region Control Overlap

		[TestedType(typeof(ProcessTaskTemplateForm))]
		class ControlOverlapFormBasherTest : ZFormBasherTest
		{
			protected override Form GetFormToBashCore()
			{
				return new ProcessTaskTemplateForm(template);
			}

			protected override void SetUp()
			{
				base.SetUp();
				template = Factory.New<ProcessTaskTemplate>();
			}

			ProcessTaskTemplate template;

			[RequiresSTA]
			public override void TestBashingForm()
			{
				// Ignore this test
			}

			[SnailTest]
			[StressTest]
			public void TestCriteriaDontOverlapForProcessTypeAToF()
			{
				TestCriteriaDontOverlap('A', 'F');
			}

			[SnailTest]
			[StressTest]
			public void TestCriteriaDontOverlapForProcessTypeGToM()
			{
				TestCriteriaDontOverlap('G', 'M');
			}

			[SnailTest]
			[StressTest]
			public void TestCriteriaDontOverlapForProcessTypeNToS()
			{
				TestCriteriaDontOverlap('N', 'S');
			}

			[SnailTest]
			[StressTest]
			[RequiresSTA]
			public void TestCriteriaDontOverlapForProcessTypeTToZ()
			{
				TestCriteriaDontOverlap('T', 'Z');
			}

			protected void TestCriteriaDontOverlap(char fromFirstLetter, char toFirstLetter)
			{
				var descriptorList = ObjectFactory.Get<IWorkflowDescriptorList>().Cast<CodeDescriptionPair>();
				var descriptorListSubSet = descriptorList.Where(d => d.Code[0] >= fromFirstLetter && d.Code[0] <= toFirstLetter);

				foreach (ICodeDescription processType in descriptorListSubSet)
				{
					template.P0_ProcessType = processType.Code;
					template.P0_IsUniversal = false; // Universal triggers are tested in UniversalTriggersFormBasherTest.

					using (var testForm = GetFormToBash())
					{
						testForm.Show();
						Application.DoEvents();

						BashControl(testForm);

						if (GetFailureMessages().Length > 0)
						{
							this.AddError(string.Format("The above failures occurred when running for process type <b>{0} ({1})</b>", processType.Code, processType.Description));
							ReportExceptions();
						}
					}
				}

				Assert(true); // Bash Control would have highlighted the problems.
			}
		}

		#endregion

		#region Sorting

		[RequiresSTA]
		public void TestAllTaskGridsSortable()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				Application.DoEvents();
				for (int i = 0; i < form.TasksTabControl.TabCount; i++)
				{
					var selected = form.TasksTabControl.SelectedTab;
					AssertAllTaskGridsSortalble(selected);
					form.TasksTabControl.SelectNextTabPage();
					Application.DoEvents();
				}
			}
		}

		void AssertAllTaskGridsSortalble(Control selected)
		{
			foreach (var grid in selected.FindAll<ZGrid>())
			{
				Assert(string.Format(CultureInfo.InvariantCulture, "All grids on the Workflow Template should be sortable but {0} was not", grid.Name), grid.AllowSorting);
				if (grid?.ListManager?.List is WorkflowItemCollectionView tasks)
				{
					Assert(string.Format(CultureInfo.InvariantCulture, "All collections on the Workflow Template should be sortable but {0} was not", grid.Name), ((IBindingList)tasks).SupportsSorting);
				}
			}
		}

		#endregion

		#region ControlVisibilityTest

		class ControlVisibilityTest : TestCaseWithFactory
		{
			#region TestControlVisibility

			public void TestControlVisibility()
			{
				Form.Show();
				Application.DoEvents();

				AssertSubTypeDropEditVisibility(Form.SubType1DropEdit, 1);
				AssertSubTypeDropEditVisibility(Form.SubType2DropEdit, 2);
				AssertSubTypeDropEditVisibility(Form.SubType3DropEdit, 3);
				AssertSubTypeDropEditVisibility(Form.SubType4DropEdit, 4);
				AssertSubTypeDropEditVisibility(Form.SubType5DropEdit, 5);

				AssertSubTypeCodeFindBoxVisibility(Form.SubType1CodeFindBox, 1);
				AssertSubTypeCodeFindBoxVisibility(Form.SubType2CodeFindBox, 2);
				AssertSubTypeCodeFindBoxVisibility(Form.SubType3CodeFindBox, 3);
				AssertSubTypeCodeFindBoxVisibility(Form.SubType4CodeFindBox, 4);
				AssertSubTypeCodeFindBoxVisibility(Form.SubType5CodeFindBox, 5);

				AssertControlVisibility("Port 1", Form.Location1CodeFindBox, DummyWorkflowDescriptor.Instance.Port1NeededInfo);
				AssertControlVisibility("Port 2", Form.Location2CodeFindBox, DummyWorkflowDescriptor.Instance.Port2NeededInfo);
				AssertControlVisibility("Client", Form.ClientGuidFindBox, DummyWorkflowDescriptor.Instance.ClientNeededInfo);
				AssertControlVisibility("Warehouse", Form.WarehouseGuidFindBox, DummyWorkflowDescriptor.Instance.WarehouseNeededInfo);
				AssertControlVisibility("Branch", Form.BranchGuidFindBox, DummyWorkflowDescriptor.Instance.BranchNeededInfo);
				AssertControlVisibility("Department", Form.DepartmentGuidFindBox, DummyWorkflowDescriptor.Instance.DepartmentNeededInfo);
				AssertEquals("'Milestones not available' label not visible", false, Form.MilestonesNotAvailableLabel.Visible);
				AssertEquals("'Workflow Triggers not available' label not visible", false, Form.WorkflowTriggersNotAvailableLabel.Visible);
				AssertEquals("'Tasks not available' label not visible", false, Form.TasksNotAvailableLabel.Visible);
				AssertEquals("'Screen Layout not available' label not visible", false, Form.ScreenLayoutNotAvailableLabel.Visible);
			}

			#endregion

			#region TestControlVisibility_WhenMilestonesNotAvailable

			[RequiresSTA]
			public void TestControlVisibility_WhenMilestonesNotAvailable()
			{
				DummyWorkflowDescriptor.Instance.EventTrackingSupported = false;
				Form.Show();
				Application.DoEvents();

				Form.TasksTabControl.SelectedTab = Form.MilestonesTabPage;
				AssertEquals("Milestones not available label visible when event tracking not supported by workflow type", true, Form.MilestonesNotAvailableLabel.Visible);
				Form.TasksTabControl.SelectedTab = Form.WorkflowTriggersTabPage;
				AssertEquals("Workflow Triggers not available label visible when event tracking not supported by workflow type", true, Form.WorkflowTriggersNotAvailableLabel.Visible);
			}

			#endregion

			#region TestControlVisibility_WhenTaskNotAvailable

			[RequiresSTA]
			public void TestControlVisibility_WhenTaskNotAvailable()
			{
				DummyWorkflowDescriptor.Instance.TasksSupported = false;
				Form.Show();
				Application.DoEvents();

				Form.TasksTabControl.SelectedTab = Form.TasksTabPage;
				AssertEquals("Tasks not available label visible when tasks not supported by workflow type", true, Form.TasksNotAvailableLabel.Visible);
			}

			#endregion

			#region TestControlVisibility_WhenCustomFieldsNotAvailable

			public void TestControlVisibility_WhenCustomFieldsNotAvailable() => TestControlVisibility_CustomFields(customFieldsSupported: false);

			public void TestControlVisibility_WhenCustomFieldsAvailable() => TestControlVisibility_CustomFields(customFieldsSupported: true);

			void TestControlVisibility_CustomFields(bool customFieldsSupported)
			{
				DummyWorkflowDescriptor.Instance.CustomFieldsSupported = customFieldsSupported;
				Form.Show();
				Application.DoEvents();

				Form.TasksTabControl.SelectedTab = Form.CustomizedFieldsTabPage;
				AssertEquals(!customFieldsSupported, Form.CustomizedFieldsNotAvailableLabel.Visible);
			}

			#endregion

			#region ValidationToolNotAvailableLabel

			public void TestControlVisibility_ValidationToolNotAvailableLabel() => CombineAssertions(() =>
			{
				var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
				{
					ValidationRulesSupported = true
				};
				DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
				Form.Show();
				Application.DoEvents();

				validationToolSettings.IsValidationRulesAvailableForGlobalTemplatesForTest = false;
				template.GlobalTemplate = true;
				Form.TasksTabControl.SelectedTab = Form.ValidationToolTabPage;
				AssertEquals("Not available for global template", true, Form.ValidationToolNotAvailableLabel.Visible);

				validationToolSettings.IsValidationRulesAvailableForGlobalTemplatesForTest = true;
				template.GlobalTemplate = true;
				Form.TasksTabControl.SelectedTab = Form.ValidationToolTabPage;
				AssertEquals("Available for global template", false, Form.ValidationToolNotAvailableLabel.Visible);
			});

			#endregion

			#region TestControlVisibility_WhenScreenLayoutNotAvailable

			[RequiresSTA]
			public void TestControlVisibility_WhenScreenLayoutNotAvailable()
			{
				DummyWorkflowDescriptor.Instance.ScreenLayoutSupported = false;
				Form.Show();
				Application.DoEvents();

				Form.TasksTabControl.SelectedTab = Form.FormCustomizationTabPage;
				AssertEquals("Screen Layout not available label visible when screen layout not supported by workflow type", true, Form.ScreenLayoutNotAvailableLabel.Visible);
			}

			#endregion

			#region TestControlVisibility_MilestoneTemplateHintCaptionLabel

			public void TestControlVisibility_MilestoneTemplateHintCaptionLabel()
			{
				template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
				DummyWorkflowDescriptor.Instance.MilestoneTemplateHintCaptionOverride = "I'm Mr Meeseeks look at me!";

				Form.Show();
				Application.DoEvents();

				AssertEquals("MilestoneTemplateHintCaptionLabel not visible for Tasks tab", false, Form.MilestoneTemplateHintCaptionLabel.Visible);

				Form.TasksTabControl.SelectedTab = Form.MilestonesTabPage;
				AssertEquals("MilestoneTemplateHintCaptionLabel visible for Milestones tab", true, Form.MilestoneTemplateHintCaptionLabel.Visible);
				AssertEquals("I'm Mr Meeseeks look at me!", Form.MilestoneTemplateHintCaptionLabel.Text);

				Form.TasksTabControl.SelectedTab = Form.WorkflowTriggersTabPage;
				AssertEquals("MilestoneTemplateHintCaptionLabel not visible for Triggers tab", false, Form.MilestoneTemplateHintCaptionLabel.Visible);
			}

			#endregion

			#region TestControlVisibility_TabPagesVisiblity

			public void TestControlVisibility_TabPagesVisiblity()
			{
				Form.Show();

				AssertTabPageVisiblity("TasksTabPage", true);
				AssertTabPageVisiblity("FormCustomizationTabPage", true);
				AssertTabPageVisiblity("CustomizedFieldsTabPage", true);
				AssertTabPageVisiblity("MilestonesTabPage", true);
				AssertTabPageVisiblity("WorkflowTriggersTabPage", true);
				AssertTabPageVisiblity("WorkflowsTabPage", false);

				template.P0_ProcessType = WorkflowDescriptors.WhsTransferWorkflowDescriptorCode;
				AssertTabPageVisiblity("FormCustomizationTabPage", false);
				AssertTabPageVisiblity("CustomizedFieldsTabPage", false);
				AssertTabPageVisiblity("WorkflowsTabPage", false);
				AssertTabPageVisiblity("TasksTabPage", true);
				AssertTabPageVisiblity("MilestonesTabPage", true);
				AssertTabPageVisiblity("WorkflowTriggersTabPage", true);

				template.P0_ProcessType = "";
				AssertTabPageVisiblity("TasksTabPage", true);
				AssertTabPageVisiblity("FormCustomizationTabPage", true);
				AssertTabPageVisiblity("CustomizedFieldsTabPage", true);
				AssertTabPageVisiblity("MilestonesTabPage", true);
				AssertTabPageVisiblity("WorkflowTriggersTabPage", true);
				AssertTabPageVisiblity("WorkflowsTabPage", false);
			}

			void AssertTabPageVisiblity(ZString tabPageName, ZBool visible)
			{
				var tabPage = Form.TasksTabControl.AllTabPages.Cast<ZTabPage>().FirstOrDefault(p => p.Name == tabPageName);
				AssertEquals(visible, tabPage != null && tabPage.TabVisible);
			}

			#endregion

			#region TestControlVisibility_TabPageVisibility_OrgProductRelation

			public void TestControlVisibility_TabPageVisibility_OrgProductRelation()
			{
				Form.Show();

				AssertTabPageVisiblity("TasksTabPage", true);
				AssertTabPageVisiblity("FormCustomizationTabPage", true);
				AssertTabPageVisiblity("CustomizedFieldsTabPage", true);
				AssertTabPageVisiblity("MilestonesTabPage", true);
				AssertTabPageVisiblity("WorkflowTriggersTabPage", true);
				AssertTabPageVisiblity("WorkflowsTabPage", false);

				template.P0_ProcessType = WorkflowDescriptors.OrgPartRelationWorkflowDescriptorCode;
				AssertTabPageVisiblity("FormCustomizationTabPage", false);
				AssertTabPageVisiblity("CustomizedFieldsTabPage", true);
				AssertTabPageVisiblity("WorkflowsTabPage", false);
				AssertTabPageVisiblity("TasksTabPage", false);
				AssertTabPageVisiblity("MilestonesTabPage", false);
				AssertTabPageVisiblity("WorkflowTriggersTabPage", false);

				template.P0_ProcessType = "";
				AssertTabPageVisiblity("TasksTabPage", true);
				AssertTabPageVisiblity("FormCustomizationTabPage", true);
				AssertTabPageVisiblity("CustomizedFieldsTabPage", true);
				AssertTabPageVisiblity("MilestonesTabPage", true);
				AssertTabPageVisiblity("WorkflowTriggersTabPage", true);
				AssertTabPageVisiblity("WorkflowsTabPage", false);
			}

			#endregion

			#region TestControlVisibility_TabPageVisibility_ValidationToolTabPage

			public void TestControlVisibility_TabPageVisibility_ValidationToolTabPage()
			{
				var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
				{
					ValidationRulesSupported = false
				};
				DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
				AssertEquals("WorkflowDescriptor.SupportsValidationRules set to false", false, template.WorkflowDescriptor.ValidationToolSettings.SupportsValidationRules);
				Form.Show();
				AssertTabPageVisiblity("ValidationToolTabPage", false);

				validationToolSettings.ValidationRulesSupported = true;
				template.P0_ProcessTypeInfo.RefreshBinding();
				AssertEquals("WorkflowDescriptor.SupportsValidationRules set to true", true, template.WorkflowDescriptor.ValidationToolSettings.SupportsValidationRules);
				AssertTabPageVisiblity("ValidationToolTabPage", true);
			}

			#endregion

			#region Implementation

			void AssertSubTypeDropEditVisibility(ZDropEdit dropEdit, int subTypeIndex)
			{
				DummyWorkflowDescriptor.Instance.SubTypes.Clear();
				template.P0_ProcessTypeInfo.RefreshBinding();
				AssertEquals("Sub Type " + subTypeIndex + ": ZDropEdit not visible", false, dropEdit.Visible);

				for (int i = 1; i <= subTypeIndex; i++)
				{
					DummyWorkflowDescriptor.Instance.SubTypes.Add(new ProcessTemplateSubType("SubType" + i, new CodeDescriptionPairList()));
				}
				template.P0_ProcessTypeInfo.RefreshBinding();
				AssertEquals("Sub Type " + subTypeIndex + ": ZDropEdit not visible", true, dropEdit.Visible);
			}

			void AssertSubTypeCodeFindBoxVisibility(ZCodeFindBox codeFindBox, int subTypeIndex)
			{
				DummyWorkflowDescriptor.Instance.SubTypes.Clear();
				template.P0_ProcessTypeInfo.RefreshBinding();
				AssertEquals("Sub Type " + subTypeIndex + ": ZCodeFindBox not visible", false, codeFindBox.Visible);

				for (int i = 1; i <= subTypeIndex; i++)
				{
					DummyWorkflowDescriptor.Instance.SubTypes.Add(new ProcessTemplateSubType("SubType" + i, new ActiveBusinessObjectCollection<DummyBusinessObject>(new BusinessObjectFactory())));
				}
				template.P0_ProcessTypeInfo.RefreshBinding();
				AssertEquals("Sub Type " + subTypeIndex + ": ZCodeFindBox not visible", true, codeFindBox.Visible);
			}

			void AssertControlVisibility(ZString fieldDescription, Control control, ZPropertyInfo taskTemplateVisibilityProperty)
			{
				taskTemplateVisibilityProperty.Value = ZBool.False;
				template.P0_ProcessTypeInfo.RefreshBinding();
				AssertEquals(fieldDescription + ": " + control.GetType().Name + " not visible", false, control.Visible);

				taskTemplateVisibilityProperty.Value = ZBool.True;
				template.P0_ProcessTypeInfo.RefreshBinding();
				AssertEquals(fieldDescription + ": " + control.GetType().Name + " not visible", true, control.Visible);
			}

			protected override void SetUp()
			{
				base.SetUp();

				template = Factory.New<TestProcessTaskTemplate>();
			}

			protected override void TearDown()
			{
				base.TearDown();

				if (form != null)
				{
					form.Dispose();
					form = null;
				}
			}

			ProcessTaskTemplateForm Form
			{
				get { return form ?? (form = new ProcessTaskTemplateForm(template)); }
			}
			ProcessTaskTemplateForm form;

			ProcessTaskTemplate template;

			class TestProcessTaskTemplate : ProcessTaskTemplate
			{
				public TestProcessTaskTemplate(BusinessObjectFactory factory, DataRow row)
					: base(factory, row)
				{
				}

				public override WorkflowDescriptor WorkflowDescriptor
				{
					get { return DummyWorkflowDescriptor.Instance; }
				}
			}

			#endregion
		}

		#endregion

		#region TestInfoMessageShownOnSaved

		public void TestInfoMessageShownOnSaved()
		{
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;
			Form.Show();
			Application.DoEvents();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Form.ValidateAndSave();
			string expectedMessage = string.Format("Template changes can take up to {0} minutes to propagate to all users in the system. If you wish to test any changes made immediately please restart your application.", ProcessTaskTemplate.Loader.CacheDurationInMins);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowNotificationOnSave_WhenNonSharedTasksAttachedToGlobalTemplatesArePresent()
		{
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			ProcessTaskTemplate.P0_SubType1 = "COL";
			ProcessTaskTemplate.P0_SubType3 = "USA";
			ProcessTaskTemplate.GlobalTemplate = true;

			AssertEquals(true, ProcessTaskTemplate.WorkflowDescriptor.AreTasksCompanySpecific);

			var task1 = ProcessTaskTemplate.WorkflowItems.Tasks.AddNew();
			task1.P9_ShareTasksForAllCompanies = false;
			task1.P9_Description = "Bend";

			var task2 = ProcessTaskTemplate.WorkflowItems.Tasks.AddNew();
			task2.P9_ShareTasksForAllCompanies = true;
			task2.P9_Description = "Snap";

			Form.Show();
			Application.DoEvents();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Form.ValidateAndSave();

			AssertNotNull(UnitTestUserNotification.Instance.LastMessage.ConfirmationDialogDescriptor.ConfirmationNotifications.SingleOrDefault
				(x => x.Message == "There are tasks which are not marked as 'shared'. This template is marked as 'global' so will be used in all companies, but these tasks will only be available in the company in which the template is applied."));

			Assert("The type of notification should have been a warning. And yet!", UnitTestUserNotification.Instance.LastMessage.WasWarning);
		}

		public void TestShowNotificationOnSave_WhenNonSharedTasksAttachedToGlobalTemplatesArePresent_ForProcessTypeWithNonCompanySpecificTasks()
		{
			ProcessTaskTemplate.P0_ProcessType = "WKI";
			ProcessTaskTemplate.GlobalTemplate = true;

			AssertEquals(false, ProcessTaskTemplate.WorkflowDescriptor.AreTasksCompanySpecific);

			var task1 = ProcessTaskTemplate.WorkflowItems.Tasks.AddNew();
			task1.P9_ShareTasksForAllCompanies = false;
			task1.P9_Description = "Bend";

			var task2 = ProcessTaskTemplate.WorkflowItems.Tasks.AddNew();
			task2.P9_ShareTasksForAllCompanies = true;
			task2.P9_Description = "Snap";

			Form.Show();
			Application.DoEvents();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Form.ValidateAndSave();

			AssertNull(UnitTestUserNotification.Instance.LastMessage.ConfirmationDialogDescriptor);
		}

		#endregion

		#region TestResetScreenLayoutButton

		public void TestResetScreenLayoutButton()
		{
			using (ProcessTaskTemplateForm form = new ProcessTaskTemplateForm(ProcessTaskTemplate))
			{
				ProcessTaskTemplate.P0_IsSystem = true;
				form.ResetScreenLayoutButton_Click(null, EventArgs.Empty);
				AssertEquals("You cannot Customize Screens for System Defined Workflow Templates.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ProcessTaskTemplate.P0_IsSystem = false;
				form.ResetScreenLayoutButton_Click(null, EventArgs.Empty);
				AssertEquals("Are you sure you want to reset the Field and Tab settings for this screen?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				ProcessTaskTemplate.FormCustomisationSettings.DisplayFields.Add((NoResString)"field desc", "field name");

				form.ResetScreenLayoutButton_Click(null, EventArgs.Empty);
				AssertEquals("Are you sure you want to reset the Field and Tab settings for this screen?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestEffectiveDateEditVisibility

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "< Pending ,This Test should be fixed by the responsible team >")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1144:Do Not Use Private Test Method", Justification = "WI00649451")]
		void TestEffectiveDateEditVisibility()
		{
			using (var testForm = new ProcessTaskTemplateForm(ProcessTaskTemplate))
			{
				Assert("Should be visible when enabled", testForm.effectiveDateEdit.Visible);
				Assert("Should be visible when enabled", testForm.endDateEdit.Visible);
			}

			Enterprise.Registry.Business.WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var testForm = new ProcessTaskTemplateForm(ProcessTaskTemplate))
			{
				Assert("Should be hidden when disabled", !testForm.effectiveDateEdit.Visible);
				Assert("Should be hidden when disabled", !testForm.endDateEdit.Visible);
			}
		}

		#endregion

		#region Buffer Management

		public void TestShowForm_WhenBufferManagementEnabled_ShouldHaveExtraTabs()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				AssertEquals(false, form.WorkflowsTabPage.IsDisposed);
				AssertEquals(false, form.CompletionStatementsTabPage.IsDisposed);
			}

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				AssertEquals(true, form.WorkflowsTabPage.IsDisposed);
				AssertEquals(true, form.CompletionStatementsTabPage.IsDisposed);
			}
		}

		[RequiresSTA]
		public void TestSaveAfterUpdatingExternalProcessHeaderLinksViaDataRefresh_ShouldValidateCorrectly()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "ORG");

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template1.P0_ProcessType = template2.P0_ProcessType = "ORG";
			template2.P0_GB = GlbBranch.CurrentBranch.PK;

			var workflow1 = helper.CreateWorkflow(template1);
			var workflow2 = helper.CreateWorkflow(template2);

			Factory.Save();

			using (var form1 = (ZForm)ZControllerFactory.Create(ControllerIDs.ProcessTemplates).ShowEditForm(template1))
			using (var form2 = (ZForm)ZControllerFactory.Create(ControllerIDs.ProcessTemplates).ShowEditForm(template2))
			{
				var formTemplate1 = (ProcessTaskTemplate)form1.BusinessEntity;
				var formTemplate2 = (ProcessTaskTemplate)form2.BusinessEntity;

				helper.CreateDependencyLink(formTemplate1, workflow2, workflow1);
				AssertEquals(ContinueWithSave.Yes, form1.FireSaveButton());

				Application.DoEvents();
				formTemplate2.ProcessHeaders[0].FH_CompletionStatement += " Narp.";

				form2.FireSaveButton();

				AssertNoErrors("Validating the link published via data refresh shouldn't trigger validation errors or cause error reports. The ProcessHeaderLink.Template property should be set.", (BusinessObject)form2.BusinessEntity);
			}
		}

		public void TestValidateAllAfterUpdatingExternalProcessHeaderLinksViaDataRefresh_ShouldValidateCorrectly()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "ORG");

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template1.P0_ProcessType = template2.P0_ProcessType = "ORG";
			template2.P0_GB = GlbBranch.CurrentBranch.PK;

			var workflow1 = helper.CreateWorkflow(template1);
			var workflow2 = helper.CreateWorkflow(template2);

			Factory.Save();

			using (var form1 = (ZForm)ZControllerFactory.Create(ControllerIDs.ProcessTemplates).ShowEditForm(template1))
			using (var form2 = (ZForm)ZControllerFactory.Create(ControllerIDs.ProcessTemplates).ShowEditForm(template2))
			{
				var formTemplate1 = (ProcessTaskTemplate)form1.BusinessEntity;
				var formTemplate2 = (ProcessTaskTemplate)form2.BusinessEntity;

				helper.CreateDependencyLink(formTemplate1, workflow2, workflow1);
				AssertEquals(ContinueWithSave.Yes, form1.FireSaveButton());

				Application.DoEvents();
				formTemplate2.ProcessHeaders[0].FH_CompletionStatement += " Narp.";

				form2.FireValidateAllForTest();

				AssertNoErrors("Validating the link published via data refresh shouldn't trigger validation errors or cause error reports. The ProcessHeaderLink.Template property should be set.", (BusinessObject)form2.BusinessEntity);
			}
		}

		[RequiresSTA]
		public void TestShowForm_WhenTemplateReleaseGroupRulesEnabled_ShouldShowTab()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			WorkflowDataRegistry.Instance.EnableTemplateReleaseGroupRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode, name: "Bring me the blue pages");

			ShowFormAndAssertTabPagesShown("Tasks", "Milestones", "Triggers", "Screen Layout", "Custom Fields");

			WorkflowDataRegistry.Instance.EnableTemplateReleaseGroupRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ShowFormAndAssertTabPagesShown("Tasks", "Milestones", "Triggers", "Screen Layout", "Custom Fields");

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.EnableTemplateReleaseGroupRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ShowFormAndAssertTabPagesShown("Workflows", "Tasks", "Completion Statements", "Milestones", "Triggers", "Screen Layout", "Custom Fields");

			WorkflowDataRegistry.Instance.EnableTemplateReleaseGroupRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ShowFormAndAssertTabPagesShown("Workflows", "Tasks", "Completion Statements", "Milestones", "Triggers", "Screen Layout", "Custom Fields", "Release Group Rules");

			void ShowFormAndAssertTabPagesShown(params string[] expectedTabPageNames)
			{
				using (var form = new ProcessTaskTemplateForm(template))
				{
					form.Show();
					Application.DoEvents();

					AssertSequencesEqual(expectedTabPageNames, form.TasksTabControl.TabPages.Cast<ZTabPage>().Select(page => page.Text));
				}
			}
		}

		#endregion

		#region WorkflowLink

		public void TestForm_ShowsCorrectWorkflowLink()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				var workflowControl = form.WorkflowsTabPage.Controls.Find("ProcessTaskTemplateWorkflowControl", true).FirstOrDefault();
				var workflowsGrid = (ZGrid)workflowControl.Controls.Find("WorkflowsGrid", true).FirstOrDefault();
				var workflowLinksGrid = (ZGrid)workflowControl.Controls.Find("WorkflowLinksGrid", true).FirstOrDefault();

				var list = workflowsGrid.ListManager.List;
				workflowsGrid.ListManager.AddNew();
				workflowsGrid.ListManager.AddNew();
				workflowsGrid.ListManager.AddNew();
				workflowsGrid[0, 2] = new ZString("abc");
				workflowsGrid[1, 2] = new ZString("def");
				workflowsGrid[2, 2] = new ZString("ghi");

				var workflow2 = (IProcessHeader)workflowsGrid.ListManager.List[1];
				var workflow3 = (IProcessHeader)workflowsGrid.ListManager.List[2];

				workflowsGrid.ListManager.Position = 1;
				Application.DoEvents();

				workflowLinksGrid.ListManager.AddNew();
				workflowLinksGrid[0, 0] = workflow2.PK;
				workflowLinksGrid[0, 1] = workflow3.PK;
				Application.DoEvents();
				AssertEquals(1, workflowLinksGrid.ListManager.Count);

				workflowsGrid.ListManager.Position = 2;
				Application.DoEvents();
				AssertEquals(1, workflowLinksGrid.ListManager.Count);

				workflowsGrid.ListManager.Position = 0;
				Application.DoEvents();

				AssertEquals(1, workflowLinksGrid.ListManager.Count);
			}
		}

		#endregion

		#region Fallback Method

		public void TestTaskFallbackMethod()
		{
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;
			ProcessTaskTemplate.P0_SubType1 = "COL";
			ProcessTaskTemplate.P0_SubType3 = "USA";
			ProcessTaskTemplate.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			Factory.Save();

			Form.Show();
			Form.TasksTabPage.Show();
			Application.DoEvents();

			AssertEquals(ProcessTaskTemplate.P0_TaskFallbackMethod, Form.tasksFallbackMethod.Text);
		}

		public void TestMilestoneFallbackMethod()
		{
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;
			ProcessTaskTemplate.P0_SubType1 = "COL";
			ProcessTaskTemplate.P0_SubType3 = "USA";
			ProcessTaskTemplate.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			Form.Show();
			Form.MilestonesTabPage.Show();
			Application.DoEvents();

			AssertEquals(ProcessTaskTemplate.P0_MilestoneFallbackMethod, Form.milestonesFallbackMethod.Text);
		}

		public void TestTriggerFallbackMethod()
		{
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;
			ProcessTaskTemplate.P0_SubType1 = "COL";
			ProcessTaskTemplate.P0_SubType3 = "USA";
			ProcessTaskTemplate.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			Form.Show();
			Form.WorkflowTriggersTabPage.Show();
			Application.DoEvents();

			AssertEquals(ProcessTaskTemplate.P0_TriggerFallbackMethod, Form.triggersFallbackMethod.Text);
		}

		public void TestValidationFallbackMethod()
		{
			var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
			{
				ValidationRulesSupported = true
			};
			DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			ProcessTaskTemplate.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			Form.Show();
			Form.ValidationToolTabPage.Show();
			Application.DoEvents();

			AssertEquals(ProcessTaskTemplate.P0_ValidationFallbackMethod, Form.validationFallbackMethod.Text);
		}

		#endregion

		#region Universal Templates

		public void TestShowUniversalTemplate_ShouldSwitchToTriggersTab_AndShowHintLabel()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);

			Factory.Save();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Should start at the triggers tab since only triggers can be defined on a universal template (for now...)", form.WorkflowTriggersTabPage, form.TasksTabControl.SelectedTab);
				AssertEquals("Milestone hint label should be visible to explain how Universal Triggers work", true, form.MilestoneTemplateHintCaptionLabel.Visible);
				AssertEquals("Changes made to triggers on a Universal Template take effect immediately on all jobs of this Process Type that match any Template Conditions. Inactive triggers do not appear on jobs.", form.MilestoneTemplateHintCaptionLabel.Text);
			}

			template.P0_IsUniversal = false;

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Should start at the tasks tab for non-universal templates", form.TasksTabPage, form.TasksTabControl.SelectedTab);
			}
		}

		public void TestControlVisibility_ForUniversalTemplate()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			helper.EnableBMSInRegistry();

			ProcessTaskTemplate.P0_IsUniversal = true;
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			ProcessTaskTemplate.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;

			Factory.Save();

			Form.Show();
			Application.DoEvents();

			AssertEquals("Not available for Universal Templates", Form.WorkflowsNotAvailableLabel.Text);
			AssertEquals("Not available for Universal Templates", Form.TasksNotAvailableLabel.Text);
			AssertEquals("Not available for Universal Templates", Form.CompletionStatementsNotAvailableLabel.Text);
			AssertEquals("Not available for Universal Templates", Form.MilestonesNotAvailableLabel.Text);
			AssertEquals("Not available for Universal Templates", Form.ScreenLayoutNotAvailableLabel.Text);
			AssertEquals("Not available for Universal Templates", Form.CustomizedFieldsNotAvailableLabel.Text);
			AssertEquals("Not available for Universal Templates", Form.ReleaseGroupRulesNotAvailableLabel.Text);

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowsTabPage, Form.WorkflowsNotAvailableLabel, coverLabelShouldBeVisible: true);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.ReleaseGroupRulesTabPage, Form.ReleaseGroupRulesNotAvailableLabel, coverLabelShouldBeVisible: true);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.TasksTabPage, Form.TasksNotAvailableLabel, coverLabelShouldBeVisible: true);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CompletionStatementsTabPage, Form.CompletionStatementsNotAvailableLabel, coverLabelShouldBeVisible: true);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.MilestonesTabPage, Form.MilestonesNotAvailableLabel, coverLabelShouldBeVisible: true);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowTriggersTabPage, Form.WorkflowTriggersNotAvailableLabel, coverLabelShouldBeVisible: false);

			var universalTriggersControl = Form.WorkflowTriggersTabPage.Controls.Find("TemplateTriggersUserControl", true)[0];
			var regularTriggersControl = Form.WorkflowTriggersTabPage.Controls.Find("WorkflowTriggerSplitPanel", true)[0];

			AssertEquals(true, universalTriggersControl.Visible);
			AssertEquals(false, regularTriggersControl.Visible);

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.FormCustomizationTabPage, Form.ScreenLayoutNotAvailableLabel, coverLabelShouldBeVisible: true);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CustomizedFieldsTabPage, Form.CustomizedFieldsNotAvailableLabel, coverLabelShouldBeVisible: true);

			ProcessTaskTemplate.P0_IsUniversal = false;
			Factory.Save();
			Application.DoEvents();

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowsTabPage, Form.WorkflowsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.ReleaseGroupRulesTabPage, Form.ReleaseGroupRulesNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.TasksTabPage, Form.TasksNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CompletionStatementsTabPage, Form.CompletionStatementsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.MilestonesTabPage, Form.MilestonesNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowTriggersTabPage, Form.WorkflowTriggersNotAvailableLabel, coverLabelShouldBeVisible: false);

			AssertEquals(false, universalTriggersControl.Visible);
			AssertEquals(true, regularTriggersControl.Visible);

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.FormCustomizationTabPage, Form.ScreenLayoutNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CustomizedFieldsTabPage, Form.CustomizedFieldsNotAvailableLabel, coverLabelShouldBeVisible: false);
		}

		public void TestControlVisibility_ForUniversalTemplate_AndWorkflowTypeWhichDoesNotSupportTriggers()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			helper.EnableBMSInRegistry();

			ProcessTaskTemplate.P0_IsUniversal = true;
			ProcessTaskTemplate.P0_ProcessType = "SIM";
			ProcessTaskTemplate.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;

			Factory.Save();

			Form.Show();
			Application.DoEvents();

			AssertEquals("Not available for this Workflow Type", Form.WorkflowTriggersNotAvailableLabel.Text);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowTriggersTabPage, Form.WorkflowTriggersNotAvailableLabel, coverLabelShouldBeVisible: true);

			var universalTriggersControl = Form.WorkflowTriggersTabPage.Controls.Find("TemplateTriggersUserControl", true)[0];
			var regularTriggersControl = Form.WorkflowTriggersTabPage.Controls.Find("WorkflowTriggerSplitPanel", true)[0];

			AssertEquals(false, universalTriggersControl.Visible);
			AssertEquals(true, regularTriggersControl.Visible);

			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			Application.DoEvents();

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowTriggersTabPage, Form.WorkflowTriggersNotAvailableLabel, coverLabelShouldBeVisible: false);

			AssertEquals(true, universalTriggersControl.Visible);
			AssertEquals(false, regularTriggersControl.Visible);
		}

		public void TestControlVisibility_ForNonUniversalTemplate()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			helper.EnableBMSInRegistry();

			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			Factory.Save();

			Form.Show();
			Application.DoEvents();

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowsTabPage, Form.WorkflowsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.TasksTabPage, Form.TasksNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CompletionStatementsTabPage, Form.CompletionStatementsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.MilestonesTabPage, Form.MilestonesNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowTriggersTabPage, Form.WorkflowTriggersNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.FormCustomizationTabPage, Form.ScreenLayoutNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CustomizedFieldsTabPage, Form.CustomizedFieldsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.ReleaseGroupRulesTabPage, Form.ReleaseGroupRulesNotAvailableLabel, coverLabelShouldBeVisible: false);
		}

		public void TestControlVisibility_ForPartialTemplate()
		{
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			ProcessTaskTemplate.P0_IsPartialTemplate = true;

			Factory.Save();

			Form.Show();
			Application.DoEvents();

			AssertEquals("Not available for Partial Templates", Form.CustomizedFieldsNotAvailableLabel.Text);
			AssertEquals("Not available for Partial Templates", Form.ScreenLayoutNotAvailableLabel.Text);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CustomizedFieldsTabPage, Form.CustomizedFieldsNotAvailableLabel, coverLabelShouldBeVisible: true);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.FormCustomizationTabPage, Form.ScreenLayoutNotAvailableLabel, coverLabelShouldBeVisible: true);

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.TasksTabPage, Form.TasksNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.MilestonesTabPage, Form.MilestonesNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowTriggersTabPage, Form.WorkflowTriggersNotAvailableLabel, coverLabelShouldBeVisible: false);
		}

		[RequiresSTA]
		public void TestControlVisibility_ForProcessTypeWithoutRelatedBufferManagementSystem()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			Form.Show();
			Application.DoEvents();

			AssertEquals("There is no Buffer Management System defined for this Process Type", Form.WorkflowsNotAvailableLabel.Text);
			AssertEquals("There is no Buffer Management System defined for this Process Type", Form.ReleaseGroupRulesNotAvailableLabel.Text);

			Assert(string.IsNullOrEmpty(Form.TasksNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.CompletionStatementsNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.MilestonesNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.WorkflowTriggersNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.ScreenLayoutNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.CustomizedFieldsNotAvailableLabel.Text));

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowsTabPage, Form.WorkflowsNotAvailableLabel, coverLabelShouldBeVisible: true);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.TasksTabPage, Form.TasksNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CompletionStatementsTabPage, Form.CompletionStatementsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.MilestonesTabPage, Form.MilestonesNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowTriggersTabPage, Form.WorkflowTriggersNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.FormCustomizationTabPage, Form.ScreenLayoutNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CustomizedFieldsTabPage, Form.CustomizedFieldsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.ReleaseGroupRulesTabPage, Form.ReleaseGroupRulesNotAvailableLabel, coverLabelShouldBeVisible: true);
		}

		public void TestControlVisibility_ForProcessTypeWithRelatedBufferManagementSystem_RelatedWorkflowTypesInactive()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystemAndRelatedWorkflowType(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isActive: false);
			helper.EnableBMSInRegistry();

			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			Factory.Save();

			Form.Show();
			Application.DoEvents();

			CombineAssertions("Even though the related workflow type is inactive, we should still be able to see the Workflows tab and its contents for Task Templates", () =>
			{
				Assert(string.IsNullOrEmpty(Form.WorkflowsNotAvailableLabel.Text));
				ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowsTabPage, Form.WorkflowsNotAvailableLabel, coverLabelShouldBeVisible: false);
			});

			Assert(string.IsNullOrEmpty(Form.TasksNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.CompletionStatementsNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.MilestonesNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.WorkflowTriggersNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.ScreenLayoutNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.CustomizedFieldsNotAvailableLabel.Text));

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.TasksTabPage, Form.TasksNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CompletionStatementsTabPage, Form.CompletionStatementsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.MilestonesTabPage, Form.MilestonesNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowTriggersTabPage, Form.WorkflowTriggersNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.FormCustomizationTabPage, Form.ScreenLayoutNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CustomizedFieldsTabPage, Form.CustomizedFieldsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.ReleaseGroupRulesTabPage, Form.ReleaseGroupRulesNotAvailableLabel, coverLabelShouldBeVisible: false);
		}

		[RequiresSTA]
		public void TestControlVisibility_ForProcessTypeWithRelatedBufferManagementSystem()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			helper.EnableBMSInRegistry();

			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			Factory.Save();

			Form.Show();
			Application.DoEvents();

			Assert(string.IsNullOrEmpty(Form.WorkflowsNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.TasksNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.CompletionStatementsNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.MilestonesNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.WorkflowTriggersNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.ScreenLayoutNotAvailableLabel.Text));
			Assert(string.IsNullOrEmpty(Form.CustomizedFieldsNotAvailableLabel.Text));

			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowsTabPage, Form.WorkflowsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.TasksTabPage, Form.TasksNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CompletionStatementsTabPage, Form.CompletionStatementsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.MilestonesTabPage, Form.MilestonesNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.WorkflowTriggersTabPage, Form.WorkflowTriggersNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.FormCustomizationTabPage, Form.ScreenLayoutNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.CustomizedFieldsTabPage, Form.CustomizedFieldsNotAvailableLabel, coverLabelShouldBeVisible: false);
			ShowTabPageAndAssertCoverLabelVisibility(Form, Form.ReleaseGroupRulesTabPage, Form.ReleaseGroupRulesNotAvailableLabel, coverLabelShouldBeVisible: false);
		}

		static void ShowTabPageAndAssertCoverLabelVisibility(ProcessTaskTemplateForm form, ZTabPage tabPageToShow, ZLabel coverLabel, bool coverLabelShouldBeVisible)
		{
			form.TasksTabControl.SelectedTab = tabPageToShow;
			Application.DoEvents();
			AssertEquals(coverLabelShouldBeVisible, coverLabel.Visible);
		}

		[TestedType(typeof(ProcessTaskTemplateForm))]
		class UniversalTriggersFormBasherTest : ZFormBasherTest
		{
			public override void TestBashingForm()
			{
				// Ignore this test
			}

			protected override Form GetFormToBashCore()
			{
				var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

				var trigger = (IBaseTrigger)template.TemplateTriggers.AddNew();
				trigger.Description = "Ha HA!";
				trigger.TriggerEventCode = AutoEvents.TaskCompletedCode;

				Factory.Save();

				return new ProcessTaskTemplateForm(template);
			}

			public void TestBashUniversalTriggers()
			{
				using (var form = (ProcessTaskTemplateForm)GetFormToBash())
				{
					form.Show();
					form.TasksTabControl.SelectedTab = form.WorkflowTriggersTabPage;

					Application.DoEvents();

					BashControl(form.WorkflowTriggersPanel);
					ReportExceptions();
				}

				Assert(true);
			}
		}

		#endregion

		#region Universal Copy

		public void TestCopySchedulesMenuItems_ShouldNotBeAvailable_ForWorkflowsGrid()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var bms = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IBMSystem)));
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_FS_BufferManagementSystem = bms.PK;
			Assert("Template should be associated with a buffer management system to allow new items in the process headers collection and thus allow universal copy for process headers", template.ProcessHeaders.AllowNew);

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				var grid = (ZGrid)form.WorkflowsTabPage.Controls.Find("WorkflowsGrid", true).FirstOrDefault();
				AssertNotNull(grid);
				grid.ListManager.AddNew();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("ProcessJobHeader", grid.ListManager.List[0].GetType().Name);
					AssertEquals("ProcessHeader", grid.ListManager.List[1].GetType().Name);
				});

				AssertSchedulesMenuIsNotAvailableForGrid(grid, itemPosition: 0);
				AssertSchedulesMenuIsNotAvailableForGrid(grid, itemPosition: 1);
			}
		}

		public void TestCopySchedulesMenuItems_ShouldNotBeAvailable_ForTagsGrid()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				AssertSchedulesMenuIsNotAvailableForGrid(form, form.WorkflowsTabPage, "TagsGrid");
			}
		}

		[RequiresSTA]
		public void TestCopySchedulesMenuItems_ShouldNotBeAvailable_ForTasksGrid()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				AssertSchedulesMenuIsNotAvailableForGrid(form, form.TasksTabPage, "TasksGrid");
			}
		}

		public void TestCopySchedulesMenuItems_ShouldNotBeAvailable_ForCompletionStatementsGrid()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				AssertSchedulesMenuIsNotAvailableForGrid(form, form.CompletionStatementsTabPage, "CompletionStatementsGrid");
			}
		}

		public void TestCopySchedulesMenuItems_ShouldNotBeAvailable_ForMilestonesGrid()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				AssertSchedulesMenuIsNotAvailableForGrid(form, form.MilestonesTabPage, "MilestonesGrid");
			}
		}

		public void TestCopySchedulesMenuItems_ShouldNotBeAvailable_ForWorkflowItemsTriggersGrid()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			using (var form = new ProcessTaskTemplateForm(template))
			{
				form.Show();
				AssertSchedulesMenuIsNotAvailableForGrid(form, form.WorkflowTriggersTabPage, "WorkflowItemsTriggersGrid");
			}
		}

		void AssertSchedulesMenuIsNotAvailableForGrid(ProcessTaskTemplateForm form, ZTabPage tabPage, string gridName)
		{
			form.TasksTabControl.SelectTab(tabPage);
			var grid = (ZGrid)tabPage.Controls.Find(gridName, true).FirstOrDefault();
			AssertNotNull(grid);
			grid.ListManager.AddNew();

			AssertSchedulesMenuIsNotAvailableForGrid(grid);
		}

		void AssertSchedulesMenuIsNotAvailableForGrid(ZGrid grid, int itemPosition = 0)
		{
			grid.ListManager.Position = itemPosition;
			Application.DoEvents();
			AssertNull("Copy Schedules menu item should not be available for workflow templates", GetSchedulesMenuItem(grid));
		}

		MenuItem GetSchedulesMenuItem(ZGrid grid)
		{
			var ucMenuItem = grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);
			AssertNotNull(ucMenuItem);
			ucMenuItem.PerformSelect();

			AssertNotEquals(0, ucMenuItem.MenuItems.Count);

			return ucMenuItem.MenuItems.FindByText("Copy Schedules");
		}

		#endregion

		#region Test Screen Layout Grid

		public void TestScreenLayoutGridAllowsNoRemoves()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			using (var form = new ProcessTaskTemplateForm(template))
			{
				AssertEquals(RemoveAction.NoRemovePossible, form.displayFieldsGrid.RemoveAction);
				AssertEquals(RemoveAction.NoRemovePossible, form.displayTabsGrid.RemoveAction);
			}
		}

		#endregion

		#region Security

		public void TestTemplateObeysSecurity()
		{
			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var org = newFactory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();
			var staff = newFactory.NewWithValidTestData<GlbStaff>();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			newFactory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.WorkflowTaskTemplatesView.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesNew.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesDelete.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed = false;

				var controller = ZControllerFactory.Create(ControllerIDs.ProcessTemplates);
				using (var form = (ProcessTaskTemplateForm)controller.ShowEditForm(template))
				{
					AssertEquals(true, controller.GetCheckPointForView(template).IsAllowed);
					AssertEquals(false, controller.GetCheckPointForEdit(template).IsAllowed);

					form.Show();

					Assert("Form should be read-only", form.P0_ProcessTypeDropEdit.ReadOnly);
					Assert("Tasks should be read-only", template.WorkflowItems.Tasks.AddNew().ReadOnly);
					Assert("Milestones should be read-only", template.WorkflowItems.Milestones.AddNew().ReadOnly);
					Assert("Triggers should be read-only", template.WorkflowItems.Triggers.AddNew().ReadOnly);
				}
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.WorkflowTaskTemplatesView.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesNew.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesDelete.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed = false;

				var controller = ZControllerFactory.Create(ControllerIDs.ProcessTemplates);
				using (var form = (ProcessTaskTemplateForm)controller.ShowNewForm())
				{
					form.Show();
					var newTemplate = form.BusinessEntity;

					AssertEquals(true, controller.GetCheckPointForView(newTemplate).IsAllowed);
					AssertEquals(false, controller.GetCheckPointForEdit(newTemplate).IsAllowed);

					Assert("Form should not be read-only", !form.P0_ProcessTypeDropEdit.ReadOnly);
					Assert("Tasks should not be read-only", !newTemplate.WorkflowItems.Tasks.AddNew().ReadOnly);
					Assert("Milestones should not be read-only", !newTemplate.WorkflowItems.Milestones.AddNew().ReadOnly);
					Assert("Tasks should not be read-only", !newTemplate.WorkflowItems.Triggers.AddNew().ReadOnly);
				}
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.WorkflowTaskTemplatesView.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesNew.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesDelete.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed = true;

				var controller = ZControllerFactory.Create(ControllerIDs.ProcessTemplates);
				using (var form = (ProcessTaskTemplateForm)controller.ShowEditForm(template))
				{
					AssertEquals(true, controller.GetCheckPointForView(template).IsAllowed);
					AssertEquals(false, controller.GetCheckPointForEdit(template).IsAllowed);

					form.Show();

					Assert("Form should be read-only", form.P0_ProcessTypeDropEdit.ReadOnly);
					Assert("Tasks should be read-only", template.WorkflowItems.Tasks.AddNew().ReadOnly);
					Assert("Milestones should be read-only", template.WorkflowItems.Milestones.AddNew().ReadOnly);
					Assert("Triggers should be read-only", template.WorkflowItems.Triggers.AddNew().ReadOnly);
				}
			}
		}

		[RequiresSTA]
		public void TestInactiveTemplateObeysSecurity()
		{
			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var staff = newFactory.NewWithValidTestData<GlbStaff>();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsActive = false;
			newFactory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.WorkflowTaskTemplatesView.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesNew.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesDelete.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed = false;

				var controller = ZControllerFactory.Create(ControllerIDs.ProcessTemplates);
				using (var form = (ProcessTaskTemplateForm)controller.ShowEditForm(template))
				{
					AssertEquals(true, controller.GetCheckPointForView(template).IsAllowed);
					AssertEquals(false, controller.GetCheckPointForEdit(template).IsAllowed);

					form.Show();

					Assert("Form should be read-only", form.P0_ProcessTypeDropEdit.ReadOnly);
				}
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.WorkflowTaskTemplatesView.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesNew.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesDelete.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed = true;

				var controller = ZControllerFactory.Create(ControllerIDs.ProcessTemplates);
				using (var form = (ProcessTaskTemplateForm)controller.ShowEditForm(template))
				{
					form.Show();

					Assert("Form should not be read-only", !form.P0_ProcessTypeDropEdit.ReadOnly);
					Assert("The activate template checkbox should be read-only", template.P0_IsActiveInfo.ReadOnly);
				}
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.WorkflowTaskTemplatesView.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesNew.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesDelete.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed = true;

				var controller = ZControllerFactory.Create(ControllerIDs.ProcessTemplates);
				using (var form = (ProcessTaskTemplateForm)controller.ShowEditForm(template))
				{
					form.Show();

					Assert("Form should not be read-only", !form.P0_ProcessTypeDropEdit.ReadOnly);
					Assert("The activate template checkbox should not be read-only", !template.P0_IsActiveInfo.ReadOnly);
				}
			}
		}

		#endregion

		#region Implementation

		internal static bool ShouldAllowOverlap(Control control)
		{
			return control.Name.Contains("NotAvailableLabel");
		}

		ProcessTaskTemplateForm Form
		{
			get { return form ?? (form = new ProcessTaskTemplateForm(ProcessTaskTemplate)); }
		}
		ProcessTaskTemplateForm form;

		ProcessTaskTemplate ProcessTaskTemplate
		{
			get { return processTaskTemplate ?? (processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>()); }
		}

		ProcessTaskTemplate processTaskTemplate;

		protected override Form GetFormToBashCore()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			Factory.Save();

			return new ProcessTaskTemplateForm(template);
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
	}
}
