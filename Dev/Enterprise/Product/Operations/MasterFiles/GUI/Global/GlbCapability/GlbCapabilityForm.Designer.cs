using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbCapabilityForm : ZTemplateForm
	{
		Enterprise.ZArchitecture.ZTextBox CapabilityRequirementsTextBox;
		Enterprise.ZArchitecture.ZTextBox CodeTextBox;
		Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		ZPanel zPanel1;
		ZPanel InstructionsPanel;
		ZPanel DetailsPanel;
		ZCheckBox IsActiveCheckBox;
		internal ZTabPage MembersTabPage;
		ZTabPage AssignmentTabPage;
		internal ZModuleButtonGridWithBlankDetachMessage CapabilityMembersGrid;
		ZArchitecture.ZGrid ReleaseGroupsGrid;
		ZArchitecture.ZLabel releaseGroupSpecificConfigurationHintLabel;
		ZCheckBox AutoAssignTasksCheckBox;
		ZTimeEditEx AutoAssignTaskAgeTimeEditBox;
		ZDropEdit CapacityScope;
		ZGroupBox DefaultGroupBox;
		ZGroupBox ReleaseGroupsGroupBox;
		ZLabel ReleaseGroupsInaccessibleLabel;
		ResourceStringData DefaultDetachMessage;

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CapabilityRequirementsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InstructionsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CapacityScope = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.releaseGroupSpecificConfigurationHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AutoAssignTasksCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutoAssignTaskAgeTimeEditBox = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AssignmentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MembersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CapabilityMembersGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGridWithBlankDetachMessage();
			this.ReleaseGroupsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DefaultGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReleaseGroupsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReleaseGroupsInaccessibleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.InstructionsPanel.SuspendLayout();
			this.DetailsPanel.SuspendLayout();
			this.CapacityScope.SuspendLayout();
			this.DefaultGroupBox.SuspendLayout();
			this.ReleaseGroupsGroupBox.SuspendLayout();
			this.ReleaseGroupsInaccessibleLabel.SuspendLayout();
			this.AssignmentTabPage.SuspendLayout();
			this.MembersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CapabilityMembersGrid.InnerGrid)).BeginInit();
			this.CapabilityMembersGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGroupsGrid)).BeginInit();
			this.ReleaseGroupsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.AssignmentTabPage);
			this.MainTabControl.Controls.Add(this.MembersTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 251, true);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MembersTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AssignmentTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			this.MainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 229, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 229, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 229, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 251, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(345);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(345);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbCapability);
			// 
			// CapabilityRequirementsTextBox
			// 
			this.CapabilityRequirementsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CapabilityRequirementsTextBox, "G4_MembershipRequirements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).G4_MembershipRequirements)));
			this.CapabilityRequirementsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CapabilityRequirementsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.CapabilityRequirementsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 21, true);
			this.CapabilityRequirementsTextBox.Multiline = true;
			this.CapabilityRequirementsTextBox.Name = "CapabilityRequirementsTextBox";
			this.CapabilityRequirementsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 137, true);
			this.CapabilityRequirementsTextBox.TabIndex = 3;
			// 
			// CodeTextBox
			// 
			this.CodeTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.CodeTextBox, "G4_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).G4_Code)));
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 3, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 17, true);
			this.CodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "G4_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).G4_Description)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 28, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 17, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.InstructionsPanel);
			this.zPanel1.Controls.Add(this.DetailsPanel);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 219, true);
			this.zPanel1.TabIndex = 4;
			// 
			// InstructionsPanel
			// 
			this.InstructionsPanel.Controls.Add(this.CapabilityRequirementsTextBox);
			this.InstructionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InstructionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 58, true);
			this.InstructionsPanel.Name = "InstructionsPanel";
			this.InstructionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 141, true);
			this.InstructionsPanel.TabIndex = 5;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.Controls.Add(this.CapacityScope);
			this.DetailsPanel.Controls.Add(this.IsActiveCheckBox);
			this.DetailsPanel.Controls.Add(this.CodeTextBox);
			this.DetailsPanel.Controls.Add(this.DescriptionTextBox);
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 58, true);
			this.DetailsPanel.TabIndex = 4;
			// 
			// CapacityScope
			// 
			this.CapacityScope.AllowDrop = true;
			this.CapacityScope.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CapacityScope, "G4_CapacityScope");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).G4_CapacityScope)));
			this.CapacityScope.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a6067db2-f150-4a08-9a1a-42d623c8f9e8", "Scope");
			this.CapacityScope.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 3, true);
			this.CapacityScope.Name = "CapacityScope";
			this.CapacityScope.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 17, true);
			this.CapacityScope.TabIndex = 7;
			// 
			// AutoAssignTasksCheckBox
			// 
			this.AutoAssignTasksCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoAssignTasksCheckBox, "G4_AllowTaskAutoAssignment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).G4_AllowTaskAutoAssignment)));
			this.AutoAssignTasksCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoAssignTasksCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 23, true);
			this.AutoAssignTasksCheckBox.Name = "AutoAssignTasksCheckBox";
			this.AutoAssignTasksCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.AutoAssignTasksCheckBox.TabIndex = 6;
			this.AutoAssignTasksCheckBox.UseVisualStyleBackColor = true;
			// 
			// AutoAssignTaskAgeTimeEditBox
			// 
			this.AutoAssignTaskAgeTimeEditBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AutoAssignTaskAgeTimeEditBox, "G4_AutoAssignTasksAge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).G4_AutoAssignTasksAge)));
			this.AutoAssignTaskAgeTimeEditBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 21, true);
			this.AutoAssignTaskAgeTimeEditBox.Name = "AutoAssignTaskAgeTimeEditBox";
			this.AutoAssignTaskAgeTimeEditBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 17, true);
			this.AutoAssignTaskAgeTimeEditBox.TabIndex = 7;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "G4_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).G4_IsActive)));
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 5, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsActiveCheckBox.TabIndex = 5;
			this.IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// AssignmentTabPage
			// 
			this.AssignmentTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c549bb0f-e4d1-4715-9d54-c548fc50cfb3", "Task Auto Assignment");
			this.AssignmentTabPage.Controls.Add(this.DefaultGroupBox);
			this.AssignmentTabPage.Controls.Add(this.ReleaseGroupsGroupBox);
			this.AssignmentTabPage.Controls.Add(this.ReleaseGroupsInaccessibleLabel);
			this.AssignmentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.AssignmentTabPage.Name = "AssignmentTabPage";
			this.AssignmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 229, true);
			this.AssignmentTabPage.TabIndex = 3;
			// 
			// DefaultGroupBox
			//
			this.DefaultGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d3d5b760-34e4-4f1b-82a2-b1e6d07ff0e6", "Global Defaults");
			this.DefaultGroupBox.Controls.Add(this.AutoAssignTasksCheckBox);
			this.DefaultGroupBox.Controls.Add(this.AutoAssignTaskAgeTimeEditBox);
			this.DefaultGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.DefaultGroupBox.Name = "DefaultGroupBox";
			this.DefaultGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 55, true);
			this.DefaultGroupBox.TabIndex = 6;
			//
			// ReleaseGroupsGroupBox
			//
			this.ReleaseGroupsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) |
			(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right))));
			this.ReleaseGroupsGroupBox.Controls.Add(this.releaseGroupSpecificConfigurationHintLabel);
			this.ReleaseGroupsGroupBox.Controls.Add(this.ReleaseGroupsGrid);
			this.ReleaseGroupsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f0f58c54-3ed3-4f6c-a6a2-b03675eaa878", "Release Group Specific Configuration");
			this.ReleaseGroupsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 69, true);
			this.ReleaseGroupsGroupBox.Name = "ReleaseGroupSpecificConfigurationGroupBox";
			this.ReleaseGroupsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 400, true);
			this.ReleaseGroupsGroupBox.TabIndex = 7;
			//
			// ReleaseGroupsInaccessibleLabel
			//
			this.ReleaseGroupsInaccessibleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) |
			(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right))));
			this.ReleaseGroupsInaccessibleLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0B73A79F-9CCA-4B94-8E90-2AC29BA94D4C", "To enable Release Group Specific Configuration, set the Details tab -> Scope field to GRP");
			this.ReleaseGroupsInaccessibleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 69, true);
			this.ReleaseGroupsInaccessibleLabel.Name = "ReleaseGroupsInaccessibleLabel";
			this.ReleaseGroupsInaccessibleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 150, true);
			this.ReleaseGroupsInaccessibleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ReleaseGroupsInaccessibleLabel.TabIndex = 7;
			this.ReleaseGroupsInaccessibleLabel.Visible = false;
			//
			// ReleaseGroupSpecificConfigurationHintLabel
			//
			this.releaseGroupSpecificConfigurationHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.releaseGroupSpecificConfigurationHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
			this.releaseGroupSpecificConfigurationHintLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("A65076D7-DCE8-4E3E-9933-A19913C33AF4", "Use the grid below to create group-specific configurations that override the global defaults above.");
			this.releaseGroupSpecificConfigurationHintLabel.Name = "releaseGroupSpecificConfigurationHintLabel";
			this.releaseGroupSpecificConfigurationHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 20, true);
			//this.releaseGroupSpecificConfigurationHintLabel.BackColor = System.Drawing.Color.LightPink;
			this.releaseGroupSpecificConfigurationHintLabel.TabIndex = 8;
			//
			// ReleaseGroupsGrid
			//
			this.ReleaseGroupsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ReleaseGroupsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReleaseGroupsGrid, "ReleaseGroupPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).ReleaseGroupPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCapabilityGroupPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).ReleaseGroupPivots)).SyncRoot)).GGC_AllowTaskAutoAssignment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbCapabilityGroupPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).ReleaseGroupPivots)).SyncRoot)).GGC_AutoAssignTasksAge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCapabilityGroupPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).ReleaseGroupPivots)).SyncRoot)).GroupName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.GlbCapabilityGroupPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).ReleaseGroupPivots)).SyncRoot)).GGC_CapabilityStartableWorkflowLimit)));
			zCodeFindBoxColumnStyleInfo1.ColumnName = "GGC_GG_Group";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "GroupName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "GGC_AllowTaskAutoAssignment";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.ColumnName = "GGC_AutoAssignTasksAge";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "GGC_CapabilityStartableWorkflowLimit";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.ReleaseGroupsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ReleaseGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ReleaseGroupsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ReleaseGroupsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ReleaseGroupsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ReleaseGroupsGrid.GridId = "c753dd37-7cdb-4768-aba7-9e09f305c21a";
			this.ReleaseGroupsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseGroupsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.ReleaseGroupsGrid.Name = "ReleaseGroupsGrid";
			this.ReleaseGroupsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 360, true);
			this.ReleaseGroupsGrid.TabIndex = 2;
			// 
			// MembersTabPage
			// 
			this.MembersTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b3c918b3-ba3f-470f-9afc-113014d8de0b", "Resources with Capability");
			this.MembersTabPage.Controls.Add(this.CapabilityMembersGrid);
			this.MembersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MembersTabPage.Name = "MembersTabPage";
			this.MembersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 229, true);
			this.MembersTabPage.TabIndex = 4;
			// 
			// CapabilityMembersGrid
			// 
			this.CapabilityMembersGrid.AllowDrop = true;
			this.CapabilityMembersGrid.AttachButtonText = Enterprise.MasterFiles.GUI.Res.GetData("d9a3c3fd-7e67-4922-a83a-ce6dc9198482", "Add");
			this.BindingSource.SetBindingMember(this.CapabilityMembersGrid, "ResourcesWithCapability");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).ResourcesWithCapability)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCapability)(null)).Lookups.AllResources)));
			this.CapabilityMembersGrid.BindToFindBoxList = "Lookups+AllResources";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "GS_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo2.ColumnName = "GS_FullName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zDropEditColumnStyleInfo1.ColumnName = "CapabilityPivot+SkillLevel";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.ColumnName = "CapabilityPivot+SkillLevelDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zDateEditColumnStyleInfo1.ColumnName = "CapabilityPivot+G5_DateExperienceGained";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "GS_IsActive";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffFilterControl|0d3075e1-3435-4396-a4c9-2b383cbb136d", "Work Status");
			zTextBoxColumnStyleInfo4.ColumnName = "WorkStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8a4eab5a-62c2-42db-8862-64d570341d79", "Home Branch");
			zTextBoxColumnStyleInfo5.ColumnName = "HomeBranch+GB_Code";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1ad8c8d6-5cef-4617-b2a6-2fed84049234", "Home Department");
			zTextBoxColumnStyleInfo6.ColumnName = "HomeDepartment+GE_Code";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			this.CapabilityMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CapabilityMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CapabilityMembersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CapabilityMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CapabilityMembersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CapabilityMembersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CapabilityMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CapabilityMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CapabilityMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CapabilityMembersGrid.DetachButtonText = Enterprise.MasterFiles.GUI.Res.GetData("ebcf6674-21c2-4333-87cc-88eee8d2112a", "Remove");
			DefaultDetachMessage = Enterprise.MasterFiles.GUI.Res.GetData("D42A8F2A-656A-4770-A97E-6CECBF2C65C2", "Are you sure you want to remove the selected staff members from this Capability?");
			this.CapabilityMembersGrid.DetachMessage = DefaultDetachMessage;
			this.CapabilityMembersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CapabilityMembersGrid.GridId = "7d8f5ae2-c0c1-4188-8692-affaa0dde697";
			this.CapabilityMembersGrid.InnerGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(StaffGrid_MouseDoubleClick);
			// 
			// 
			// 
			this.CapabilityMembersGrid.InnerGrid.AllowNavigation = false;
			this.CapabilityMembersGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CapabilityMembersGrid.InnerGrid.CaptionVisible = false;
			this.CapabilityMembersGrid.InnerGrid.DisableImportDataMenuItem = true;
			this.CapabilityMembersGrid.InnerGrid.GridId = null;
			this.CapabilityMembersGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CapabilityMembersGrid.InnerGrid.LayoutKey = "Grid";
			this.CapabilityMembersGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.CapabilityMembersGrid.InnerGrid.Name = "Grid";
			this.CapabilityMembersGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.CapabilityMembersGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 193, true);
			this.CapabilityMembersGrid.InnerGrid.TabIndex = 0;
			this.CapabilityMembersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CapabilityMembersGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.CapabilityMembersGrid.Name = "CapabilityMembersGrid";
			this.CapabilityMembersGrid.ReadOnly = false;
			this.CapabilityMembersGrid.ShowEditButton = false;
			this.CapabilityMembersGrid.ShowNewButton = false;
			this.CapabilityMembersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 229, true);
			this.CapabilityMembersGrid.TabIndex = 9;
			// 
			// GlbCapabilityForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCapabilityForm|b6221484-3535-4f8d-8d92-aa31e07fdbf2", "Resource Capability");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 307, true);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbCapability);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.GlbCapability";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 340, true);
			this.Name = "GlbCapabilityForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.InstructionsPanel.ResumeLayout(false);
			this.InstructionsPanel.PerformLayout();
			this.DetailsPanel.ResumeLayout(false);
			this.DetailsPanel.PerformLayout();
			this.CapacityScope.ResumeLayout(true);
			this.CapacityScope.PerformLayout();
			this.DefaultGroupBox.ResumeLayout(false);
			this.DefaultGroupBox.PerformLayout();
			this.ReleaseGroupsGroupBox.ResumeLayout(false);
			this.ReleaseGroupsGroupBox.PerformLayout();
			this.ReleaseGroupsInaccessibleLabel.ResumeLayout(false);
			this.ReleaseGroupsInaccessibleLabel.PerformLayout();
			this.AssignmentTabPage.ResumeLayout(false);
			this.AssignmentTabPage.PerformLayout();
			this.MembersTabPage.ResumeLayout(false);
			this.MembersTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CapabilityMembersGrid.InnerGrid)).EndInit();
			this.CapabilityMembersGrid.ResumeLayout(true);
			this.CapabilityMembersGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGroupsGrid)).EndInit();
			this.ReleaseGroupsGrid.ResumeLayout(false);
			this.ReleaseGroupsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
