using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ProcessTaskTemplateForm : ZTemplateForm, IFormInteractionMode
	{
		/// <summary>
		/// Empty constructor needed for form designer
		/// </summary>
		public ProcessTaskTemplateForm()
		{
			InitializeComponent();
		}

		public ProcessTaskTemplateForm(ProcessTaskTemplate template)
		{
			InitializeComponent();

			effectiveDateEdit.Visible = Registry.Business.WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.Value;
			endDateEdit.Visible = Registry.Business.WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.Value;

			TasksTabControl.SelectedIndexChanged += TasksSelectedIndexChanged;
			MilestoneTemplateHintCaptionLabel.Visible = false;
			MilestoneTemplateHintCaptionLabel.ForeColor = Color.DarkGreen;
			processTaskTemplateDescriptionLabel.Text = Res.GetString("ProcessTaskTemplateForm|Description", "The below settings allow you to specify what fields you wish to display on the allowed tabs on the job. You can also choose their placement and ordering. In addition, you can choose which tabs you want displayed by default on the form for new job registrations.");

			universalTriggersControl = ObjectFactory.Get<ZUserControl>("TemplateTriggersUserControl");
			universalTriggersControl.Name = "TemplateTriggersUserControl";
			universalTriggersControl.Size = WorkflowTriggerSplitPanel.Size;
			universalTriggersControl.Location = WorkflowTriggerSplitPanel.Location;
			universalTriggersControl.Anchor = WorkflowTriggerSplitPanel.Anchor;

			universalTriggersControl.SetDataBinding(template.TemplateTriggers, string.Empty);

			WorkflowTriggersPanel.Controls.Add(universalTriggersControl);

			SetDataBinding(template, "");

			PlugIns.Add(ControllerIDs.Audit);

			if (!ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				WorkflowsTabPage.Dispose();
				WorkflowsNotAvailableLabel.Dispose();

				CompletionStatementsTabPage.Dispose();
				CompletionStatementsNotAvailableLabel.Dispose();

				ReleaseGroupRulesTabPage.Dispose();
				ReleaseGroupRulesNotAvailableLabel.Dispose();
			}
			else
			{
				var workflowControl = ObjectFactory.Get<ZUserControl>(nameof(IProcessTaskTemplateWorkflowControl));
				SetupControlWithinTabPage(WorkflowsTabPage, workflowControl, DataSource, string.Empty, "ProcessTaskTemplateWorkflowControl");
				WorkflowsTabPage.Controls.Add(WorkflowsNotAvailableLabel);

				var completionStatementsControl = ObjectFactory.Get<ZUserControl>("CompletionStatementsControl");
				SetupControlWithinTabPage(CompletionStatementsTabPage, completionStatementsControl, DataSource, "CompletionStatementTasks", "CompletionStatementsControl");
				CompletionStatementsTabPage.Controls.Add(CompletionStatementsNotAvailableLabel);

				if (WorkflowDataRegistry.Instance.EnableTemplateReleaseGroupRules.Value)
				{
					var releaseGroupRulesControl = ObjectFactory.Get<ZUserControl>("ProcessTemplateReleaseGroupRulesControl");
					SetupControlWithinTabPage(ReleaseGroupRulesTabPage, releaseGroupRulesControl, DataSource, string.Empty, "ReleaseGroupRulesControl");
					ReleaseGroupRulesTabPage.Controls.Add(ReleaseGroupRulesNotAvailableLabel);
					ReleaseGroupRulesNotAvailableLabel.AllowOverlap(ReleaseGroupRulesTabPage.Controls.Find("ReleaseGroupRulesControl", true).FirstOrDefault());
				}
				else
				{
					ReleaseGroupRulesTabPage.Dispose();
				}

				SetCoverLabelVisibility(WorkflowsNotAvailableLabel, template, TemplateEntityType.Workflows);
				SetCoverLabelVisibility(ReleaseGroupRulesNotAvailableLabel, template, TemplateEntityType.ReleaseGroupRules);
				SetCoverLabelVisibility(CompletionStatementsNotAvailableLabel, template, TemplateEntityType.CompletionStatements);

				WorkflowsNotAvailableLabel.AllowOverlap(WorkflowsTabPage.Controls.Find("ProcessTaskTemplateWorkflowControl", true).FirstOrDefault());
				CompletionStatementsNotAvailableLabel.AllowOverlap(CompletionStatementsTabPage.Controls.Find("CompletionStatementsControl", true).FirstOrDefault());
			}

			if (template.P0_IsUniversal)
			{
				TasksTabControl.SelectedTab = WorkflowTriggersTabPage;
			}

			UpdateHintCaptionVisibility();
			SetupAllowOverlap();
		}

		void SetupAllowOverlap()
		{
			CustomizedFieldsNotAvailableLabel.AllowOverlap(customFieldGroupBox);
			ScreenLayoutNotAvailableLabel.AllowOverlap(ScreenLayout_IsFallback);
			ScreenLayoutNotAvailableLabel.AllowOverlap(ScreenLayoutContainer);
			TasksNotAvailableLabel.AllowOverlap(tasksFallbackMethod);
			TasksNotAvailableLabel.AllowOverlap(tasksControl);
			MilestonesNotAvailableLabel.AllowOverlap(milestonesFallbackMethod);
			MilestonesNotAvailableLabel.AllowOverlap(splitContainer4);
			MilestonesPanel.AllowOverlap(splitContainer4);
			WorkflowTriggersNotAvailableLabel.AllowOverlap(WorkflowTriggerSplitPanel);
			WorkflowTriggersNotAvailableLabel.AllowOverlap(triggersFallbackMethod);
			WorkflowTriggersPanel.AllowOverlap(triggersFallbackMethod);
			ValidationToolNotAvailableLabel.AllowOverlap(ValidationToolUserControl);
			ValidationToolNotAvailableLabel.AllowOverlap(validationFallbackMethod);
		}

		static void SetupControlWithinTabPage(ZTabPage hostTab, ZUserControl control, object dataSource, string dataMemberName, string controlName)
		{
			control.AllowDrop = true;
			control.SetDataBinding(dataSource, dataMemberName);
			control.Dock = DockStyle.Fill;
			control.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			control.Name = controlName;
			control.TabIndex = 0;

			hostTab.Controls.Add(control);
		}

		void TasksSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateHintCaptionVisibility();
		}

		void UpdateHintCaptionVisibility()
		{
			MilestoneTemplateHintCaptionLabel.Visible = TasksTabControl.SelectedTab == MilestonesTabPage || (TasksTabControl.SelectedTab == WorkflowTriggersTabPage && (DataSource?.P0_IsUniversal ?? false));
		}

		readonly ZUserControl universalTriggersControl;

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				foreach (var tuple in GetControlsAndDataMembersRequiringCaptionBinding())
				{
					tuple.Item1.GetExtension<LabelCaptionRenderer>().DataBindings.RemoveBinding((NoResString)"Caption");
				}

				DataSource.GlobalTemplateInfo.ValueChanged -= SetControlVisibility;
				DataSource.P0_ProcessTypeInfo.ValueChanged -= SetControlVisibility;
				DataSource.P0_IsUniversalInfo.ValueChanged -= SetControlVisibility;
				DataSource.P0_IsPartialTemplateInfo.ValueChanged -= SetControlVisibility;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				foreach (var tuple in GetControlsAndDataMembersRequiringCaptionBinding())
				{
					tuple.Item1.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", DataSource, tuple.Item2, true));
				}

				DataSource.GlobalTemplateInfo.ValueChanged += SetControlVisibility;
				DataSource.P0_ProcessTypeInfo.ValueChanged += SetControlVisibility;
				DataSource.P0_IsUniversalInfo.ValueChanged += SetControlVisibility;
				DataSource.P0_IsPartialTemplateInfo.ValueChanged += SetControlVisibility;

				SetControlVisibility();
			}
		}

		IEnumerable<Tuple<Control, string>> GetControlsAndDataMembersRequiringCaptionBinding()
		{
			yield return Tuple.Create<Control, string>(ClientGuidFindBox, "ClientName");
			yield return Tuple.Create<Control, string>(SubType1DropEdit, "SubType1Label");
			yield return Tuple.Create<Control, string>(SubType2DropEdit, "SubType2Label");
			yield return Tuple.Create<Control, string>(SubType3DropEdit, "SubType3Label");
			yield return Tuple.Create<Control, string>(SubType4DropEdit, "SubType4Label");
			yield return Tuple.Create<Control, string>(SubType5DropEdit, "SubType5Label");
			yield return Tuple.Create<Control, string>(SubType1CodeFindBox, "SubType1Label");
			yield return Tuple.Create<Control, string>(SubType2CodeFindBox, "SubType2Label");
			yield return Tuple.Create<Control, string>(SubType3CodeFindBox, "SubType3Label");
			yield return Tuple.Create<Control, string>(SubType4CodeFindBox, "SubType4Label");
			yield return Tuple.Create<Control, string>(SubType5CodeFindBox, "SubType5Label");
			yield return Tuple.Create<Control, string>(Location1CodeFindBox, "Port1Name");
			yield return Tuple.Create<Control, string>(Location2CodeFindBox, "Port2Name");
			yield return Tuple.Create<Control, string>(WarehouseGuidFindBox, "WarehouseName");
		}

		void SetControlVisibility(object sender, EventArgs e)
		{
			SetControlVisibility();
		}

		void SetControlVisibility()
		{
			var template = DataSource;

			SubType1DropEdit.Visible = template.SubType1Exists && template.Lookups.List1 is ICodeDescriptionPairList;
			SubType2DropEdit.Visible = template.SubType2Exists && template.Lookups.List2 is ICodeDescriptionPairList;
			SubType3DropEdit.Visible = template.SubType3Exists && template.Lookups.List3 is ICodeDescriptionPairList;
			SubType4DropEdit.Visible = template.SubType4Exists && template.Lookups.List4 is ICodeDescriptionPairList;
			SubType5DropEdit.Visible = template.SubType5Exists && template.Lookups.List5 is ICodeDescriptionPairList;

			SubType1CodeFindBox.Visible = template.SubType1Exists && template.Lookups.List1 is IActiveBusinessObjectCollection;
			SubType2CodeFindBox.Visible = template.SubType2Exists && template.Lookups.List2 is IActiveBusinessObjectCollection;
			SubType3CodeFindBox.Visible = template.SubType3Exists && template.Lookups.List3 is IActiveBusinessObjectCollection;
			SubType4CodeFindBox.Visible = template.SubType4Exists && template.Lookups.List4 is IActiveBusinessObjectCollection;
			SubType5CodeFindBox.Visible = template.SubType5Exists && template.Lookups.List5 is IActiveBusinessObjectCollection;

			Location1CodeFindBox.Visible = template.LoadPortExists;
			Location2CodeFindBox.Visible = template.DischargePortExists;

			ClientGuidFindBox.Visible = template.ClientExists;
			WarehouseGuidFindBox.Visible = template.WarehouseExists;

			BranchGuidFindBox.Visible = template.BranchExists;
			DepartmentGuidFindBox.Visible = template.DepartmentExists;

			SetCoverLabelVisibility(WorkflowsNotAvailableLabel, template, TemplateEntityType.Workflows);
			SetCoverLabelVisibility(TasksNotAvailableLabel, template, TemplateEntityType.Tasks);
			SetCoverLabelVisibility(CompletionStatementsNotAvailableLabel, template, TemplateEntityType.CompletionStatements);
			SetCoverLabelVisibility(MilestonesNotAvailableLabel, template, TemplateEntityType.Milestones);
			SetCoverLabelVisibility(WorkflowTriggersNotAvailableLabel, template, TemplateEntityType.Triggers);
			SetCoverLabelVisibility(ValidationToolNotAvailableLabel, template, TemplateEntityType.ValidationTool);
			SetCoverLabelVisibility(ScreenLayoutNotAvailableLabel, template, TemplateEntityType.ScreenLayout);
			SetCoverLabelVisibility(CustomizedFieldsNotAvailableLabel, template, TemplateEntityType.CustomFields);
			SetCoverLabelVisibility(ReleaseGroupRulesNotAvailableLabel, template, TemplateEntityType.ReleaseGroupRules);

			SetTabPagesVisibility(template);

			var isUniversalTemplate = template.P0_IsUniversal && template.SupportsTemplateEntityType(TemplateEntityType.Triggers).IsSupported;

			universalTriggersControl.Visible = isUniversalTemplate;
			WorkflowTriggerSplitPanel.Visible = !isUniversalTemplate;
		}

		static void SetCoverLabelVisibility(ZLabel label, ProcessTaskTemplate template, TemplateEntityType entityType)
		{
			var supportToken = template.SupportsTemplateEntityType(entityType);
			var labelVisible = !supportToken.IsSupported;

			if (labelVisible)
			{
				label.Text = supportToken.NotSupportedReason;
			}

			label.AutoSize = false;
			label.Font = OFont.GetFontBold();
			label.Dock = DockStyle.Fill;
			label.Visible = labelVisible;

			label.BringToFront();
		}

		void SetTabPagesVisibility(ProcessTaskTemplate template)
		{
			FindTabPageAndSetVisibility("TasksTabPage", true);
			FindTabPageAndSetVisibility("FormCustomizationTabPage", true);
			FindTabPageAndSetVisibility("CustomizedFieldsTabPage", true);
			FindTabPageAndSetVisibility("MilestonesTabPage", true);
			FindTabPageAndSetVisibility("WorkflowTriggersTabPage", true);
			FindTabPageAndSetVisibility("ValidationToolTabPage", false);

			switch (template.P0_ProcessType)
			{
				case WorkflowDescriptors.WhsTransferWorkflowDescriptorCode:
					FindTabPageAndSetVisibility("FormCustomizationTabPage", false);
					FindTabPageAndSetVisibility("CustomizedFieldsTabPage", false);
					break;

				case WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor:
					FindTabPageAndSetVisibility("ArrivalNotificationTabPage", true);
					FindTabPageAndSetVisibility("UnloadingRemarksTabPage", true);

					break;

				case WorkflowDescriptors.OrgPartRelationWorkflowDescriptorCode:
					FindTabPageAndSetVisibility("TasksTabPage", false);
					FindTabPageAndSetVisibility("FormCustomizationTabPage", false);
					FindTabPageAndSetVisibility("CustomizedFieldsTabPage", true);
					FindTabPageAndSetVisibility("MilestonesTabPage", false);
					FindTabPageAndSetVisibility("WorkflowTriggersTabPage", false);
					break;

				default:
					break;
			}

			if (DataSource?.WorkflowDescriptor?.ValidationToolSettings.SupportsValidationRules ?? false)
			{
				FindTabPageAndSetVisibility("ValidationToolTabPage", true);
			}
		}

		void FindTabPageAndSetVisibility(ZString tabPageName, ZBool visible)
		{
			var tabPage = TasksTabControl.AllTabPages.Cast<ZTabPage>().FirstOrDefault(p => p.Name == tabPageName);
			if (tabPage != null)
			{
				SetTabPageVisibility(tabPage, visible);
			}
		}

		void SetTabPageVisibility(ZTabPage tabPage, ZBool visible)
		{
			if (tabPage.TabVisible != visible && (!tabPage.IsDisposed || tabPage.IsAccessible))
			{
				tabPage.TabVisible = visible;
			}
		}

		new ProcessTaskTemplate DataSource
		{
			get { return (ProcessTaskTemplate)base.DataSource; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				UnsubscribeHandlers();
			}
			base.Dispose(isNotFinalizing);
		}

		void UnsubscribeHandlers()
		{
			if (TasksTabControl != null)
			{
				TasksTabControl.SelectedIndexChanged -= TasksSelectedIndexChanged;
			}

			if (DataSource != null)
			{
				DataSource.GlobalTemplateInfo.ValueChanged -= SetControlVisibility;
				DataSource.P0_ProcessTypeInfo.ValueChanged -= SetControlVisibility;
				DataSource.P0_IsUniversalInfo.ValueChanged -= SetControlVisibility;
				DataSource.P0_IsPartialTemplateInfo.ValueChanged -= SetControlVisibility;
			}
		}

		#endregion

		#region Implementation

		protected override bool SupportsEDocs => false;

		//ShowAuditTab should always be set to false because adding the tab in the base class causes null error. Audit tab plugin is added in the constructor of this form manually
		protected override bool ShowAuditTab => false;

		public bool IsShowingAuditTab => ShowAuditTab;

		internal protected new ProcessTaskTemplate BusinessEntity
		{
			get { return (ProcessTaskTemplate)base.DataSource; }
		}

		protected override void PopulateDevTools(List<IDevTool> tools)
		{
			base.PopulateDevTools(tools);
			tools.Add(new FormStateXmlExplorerTool());
		}

		internal void ResetScreenLayoutButton_Click(object sender, EventArgs e)
		{
			if (DataSource.P0_IsSystem)
			{
				Globals.Message.Show(Res.GetString("f5eebe2c-f945-42bb-9b83-0ee4554c8ce6", "You cannot Customize Screens for System Defined Workflow Templates."));
			}
			else
			{
				DialogResult result = Globals.Message.Show(Res.GetString("3f90dfe0-7a17-4c74-a1b6-1511575d21bf", "Are you sure you want to reset the Field and Tab settings for this screen?"), Res.GetString("843c2a7a-658b-4c1b-9467-fcc3ff085aab", "Screen Layout"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					DataSource.FormCustomisationSettings.Reset();
				}
			}
		}

		internal protected new ContinueWithSave ValidateAndSave()
		{
			var template = DataSource;

			if (template.ShouldWarnAboutNonSharedTasks && template.HasNonSharedTasks)
			{
				var warningMessage = Res.GetString("20de64c4-7d3c-4a71-9afc-eec1de9085de", "There are tasks which are not marked as 'shared'. This template is marked as 'global' so will be used in all companies, but these tasks will only be available in the company in which the template is applied.");
				var dialogResult = Globals.Message.ShowConfirmationWithNotifications(
					new ConfirmationDialogDescriptor(
						Res.GetString("5f63696b-4429-4a11-bc91-550d6add93ea", "Save Workflow Template"),
						Res.GetString("0babb17f-fe2a-44ad-833d-0f42ea4a0e4c", "Please review the following notifications before this template can be saved:"), new[] { new ConfirmationNotification(NotificationTypes.Warning, warningMessage) }));

				if (dialogResult != ZDialogResult.OK)
				{
					return ContinueWithSave.No;
				}
			}

			var result = base.ValidateAndSave();

			if (result == ContinueWithSave.Yes)
			{
				var infoMessage = Res.GetString("76525C4F-9EA6-4521-85BE-2F56CF1A2402", "Template changes can take up to {0} minutes to propagate to all users in the system. If you wish to test any changes made immediately please restart your application.", ProcessTaskTemplate.Loader.CacheDurationInMins);
				Globals.Message.ShowInformation(infoMessage);
			}

			return result;
		}

		#endregion

		public bool IsModal()
		{
			return false;
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			var template = DataSource;
			if (template.WorkflowDescriptor != null)
			{
				var caption = Res.GetString("03173025-4DB0-4540-9B50-0B94FF3AAE9B", "Please Confirm");
				var confirmation = Res.GetString("338A849A-ADC3-4DA1-81C0-C5D0CA4FE780", "Yes");
				string message = Res.GetString("B55F839D-A3AB-4885-B6BC-1CE87570254A", "are not supported by this process type.\r\nThey will be deleted from this template.\r\nAre you sure you want to proceed with this change?");
				bool removeMilestones = false;
				bool removeTriggers = false;
				bool removeTasks = false;

				if (!template.SupportsTemplateEntityType(TemplateEntityType.Triggers).IsSupported && (template.WorkflowItems.Triggers.Count > 0))
				{
					message = ((NoResString)"Triggers ") + message;
					removeTriggers = true;
				}
				if (!template.SupportsTemplateEntityType(TemplateEntityType.Tasks).IsSupported && (template.WorkflowItems.Tasks.Count > 0))
				{
					message = ((NoResString)"Tasks ") + message;
					removeTasks = true;
				}
				if (!template.SupportsTemplateEntityType(TemplateEntityType.Milestones).IsSupported && (template.WorkflowItems.Milestones.Count > 0))
				{
					message = ((NoResString)"Milestones ") + message;
					removeMilestones = true;
				}
				if (removeMilestones || removeTasks || removeTriggers)
				{
					if (Globals.Message.ShowConfirmation(message, caption, confirmation, ZMessageBoxIcon.Warning) == ZDialogResult.OK)
					{
						if (removeMilestones)
						{
							template.WorkflowItems.Milestones.RemoveAndDeleteAll();
						}
						if (removeTriggers)
						{
							template.WorkflowItems.Triggers.RemoveAndDeleteAll();
						}
						if (removeTasks)
						{
							template.WorkflowItems.Tasks.RemoveAndDeleteAll();
						}
					}
					else
					{
						result = ContinueWithSave.No;
					}
				}
			}
			return result;
		}
	}
}
