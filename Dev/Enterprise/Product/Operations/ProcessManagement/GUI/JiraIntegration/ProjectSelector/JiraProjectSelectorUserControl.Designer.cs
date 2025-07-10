using CargoWiseOne.ResourceStrings;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.ProcessManagement.GUI
{
	partial class JiraProjectSelectorUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ProjectKeysGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ImportAllProjectsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CancelImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BeginImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssueJQLTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportInProgressIssuesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ImportUnStartedIssuesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ImportCompletedIssuesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ImportIssueAttachmentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JiraSystemCodeDropList = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LoginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.projectSelectionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.queryOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectKeysGrid)).BeginInit();
			this.ProjectKeysGrid.SuspendLayout();
			this.JiraSystemCodeDropList.SuspendLayout();
			this.LoginGroupBox.SuspendLayout();
			this.projectSelectionGroupBox.SuspendLayout();
			this.queryOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.ProjectsToImportViewModel);
			// 
			// ProjectKeysGrid
			// 
			this.ProjectKeysGrid.AllowNavigation = false;
			this.ProjectKeysGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProjectKeysGrid, "SpecificProjectsToImport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).SpecificProjectsToImport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.ProjectKeyViewModel)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).SpecificProjectsToImport)).SyncRoot)).ProjectKey)));
			this.ProjectKeysGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ProjectKey";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.ProjectKeysGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProjectKeysGrid.GridId = "a77e2982-7947-4b2b-91b9-4e9bcf42977e";
			this.ProjectKeysGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProjectKeysGrid.LayoutKey = "ProjectKeysGrid";
			this.ProjectKeysGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 70, true);
			this.ProjectKeysGrid.Name = "ProjectKeysGrid";
			this.ProjectKeysGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 190, true);
			this.ProjectKeysGrid.TabIndex = 4;
			// 
			// ImportAllProjectsCheckBox
			// 
			this.ImportAllProjectsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ImportAllProjectsCheckBox, "ShouldImportAllProjects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).ShouldImportAllProjects)));
			this.ImportAllProjectsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImportAllProjectsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 19, true);
			this.ImportAllProjectsCheckBox.Name = "ImportAllProjectsCheckBox";
			this.ImportAllProjectsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.ImportAllProjectsCheckBox.TabIndex = 3;
			this.ImportAllProjectsCheckBox.UseVisualStyleBackColor = true;
			// 
			// CancelImportButton
			// 
			this.CancelImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelImportButton.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("26031808-9c25-49d5-8ba8-130ca90113ad", "Cancel");
			this.CancelImportButton.IsCaptionOverridden = false;
			this.CancelImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 521, true);
			this.CancelImportButton.Name = "CancelImportButton";
			this.CancelImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 22, true);
			this.CancelImportButton.TabIndex = 12;
			this.CancelImportButton.ToolTipCaption = null;
			this.CancelImportButton.UseVisualStyleBackColor = true;
			// 
			// BeginImportButton
			// 
			this.BeginImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BeginImportButton.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("5eff3788-8f9e-4484-be4a-784579789fb0", "Begin Import");
			this.BeginImportButton.IsCaptionOverridden = false;
			this.BeginImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 521, true);
			this.BeginImportButton.Name = "BeginImportButton";
			this.BeginImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BeginImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 22, true);
			this.BeginImportButton.TabIndex = 11;
			this.BeginImportButton.ToolTipCaption = null;
			this.BeginImportButton.UseVisualStyleBackColor = true;
			this.BeginImportButton.Click += new System.EventHandler(this.BeginImportButton_Click);
			// 
			// UserNameTextBox
			// 
			this.UserNameTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UserNameTextBox, "JiraUserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).JiraUserName)));
			this.UserNameTextBox.CaptionResourceString = null;
			this.UserNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 50, true);
			this.UserNameTextBox.Name = "UserNameTextBox";
			this.UserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 18, true);
			this.UserNameTextBox.TabIndex = 1;
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "JiraAuthToken");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).JiraAuthToken)));
			this.PasswordTextBox.CaptionResourceString = null;
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 78, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '•';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 18, true);
			this.PasswordTextBox.TabIndex = 2;
			// 
			// IssueJQLTextBox
			// 
			this.IssueJQLTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IssueJQLTextBox, "IssueJiraQueryLanguageStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).IssueJiraQueryLanguageStatement)));
			this.IssueJQLTextBox.CaptionResourceString = null;
			this.IssueJQLTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IssueJQLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 91, true);
			this.IssueJQLTextBox.Name = "IssueJQLTextBox";
			this.IssueJQLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 18, true);
			this.IssueJQLTextBox.TabIndex = 9;
			// 
			// ImportInProgressIssuesCheckBox
			// 
			this.ImportInProgressIssuesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ImportInProgressIssuesCheckBox, "ShouldImportInProgressIssues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).ShouldImportInProgressIssues)));
			this.ImportInProgressIssuesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImportInProgressIssuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 19, true);
			this.ImportInProgressIssuesCheckBox.Name = "ImportInProgressIssuesCheckBox";
			this.ImportInProgressIssuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.ImportInProgressIssuesCheckBox.TabIndex = 5;
			this.ImportInProgressIssuesCheckBox.UseVisualStyleBackColor = true;
			// 
			// ImportUnStartedIssuesCheckBox
			// 
			this.ImportUnStartedIssuesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ImportUnStartedIssuesCheckBox, "ShouldImportUnStartedIssues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).ShouldImportUnStartedIssues)));
			this.ImportUnStartedIssuesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImportUnStartedIssuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 42, true);
			this.ImportUnStartedIssuesCheckBox.Name = "ImportUnStartedIssuesCheckBox";
			this.ImportUnStartedIssuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.ImportUnStartedIssuesCheckBox.TabIndex = 6;
			this.ImportUnStartedIssuesCheckBox.UseVisualStyleBackColor = true;
			// 
			// ImportCompletedIssuesCheckBox
			// 
			this.ImportCompletedIssuesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ImportCompletedIssuesCheckBox, "ShouldImportCompletedIssues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).ShouldImportCompletedIssues)));
			this.ImportCompletedIssuesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImportCompletedIssuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 65, true);
			this.ImportCompletedIssuesCheckBox.Name = "ImportCompletedIssuesCheckBox";
			this.ImportCompletedIssuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.ImportCompletedIssuesCheckBox.TabIndex = 7;
			this.ImportCompletedIssuesCheckBox.UseVisualStyleBackColor = true;
			// 
			// ImportIssueAttachmentsCheckBox
			// 
			this.ImportIssueAttachmentsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ImportIssueAttachmentsCheckBox, "ShouldImportIssueAttachments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).ShouldImportIssueAttachments)));
			this.ImportIssueAttachmentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImportIssueAttachmentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(237, 19, true);
			this.ImportIssueAttachmentsCheckBox.Name = "ImportIssueAttachmentsCheckBox";
			this.ImportIssueAttachmentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.ImportIssueAttachmentsCheckBox.TabIndex = 8;
			this.ImportIssueAttachmentsCheckBox.UseVisualStyleBackColor = true;
			// 
			// JiraSystemCodeDropList
			// 
			this.JiraSystemCodeDropList.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JiraSystemCodeDropList, "JiraSystemCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.ProjectsToImportViewModel)(null)).JiraSystemCode)));
			this.JiraSystemCodeDropList.CaptionResourceString = Res.GetData("743F98A1-6CE1-4FF8-8380-98F31B444B23", "Jira URL", "The Jira URL, defined in {0} used to access the Jira REST APIs.").Format(ProcessManagementRegistry.Instance.JiraSiteUrls.Inner.Location);
			this.JiraSystemCodeDropList.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 21, true);
			this.JiraSystemCodeDropList.Name = "JiraSystemCodeDropList";
			this.JiraSystemCodeDropList.PreBoundMaxLength = 3;
			this.JiraSystemCodeDropList.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 18, true);
			this.JiraSystemCodeDropList.TabIndex = 0;
			// 
			// LoginGroupBox
			// 
			this.LoginGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("fc5fff8f-56c9-4ad6-b00d-6ff856b2f272", "Login Details");
			this.LoginGroupBox.Controls.Add(this.JiraSystemCodeDropList);
			this.LoginGroupBox.Controls.Add(this.UserNameTextBox);
			this.LoginGroupBox.Controls.Add(this.PasswordTextBox);
			this.LoginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LoginGroupBox.Name = "LoginGroupBox";
			this.LoginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 109, true);
			this.LoginGroupBox.TabIndex = 13;
			this.LoginGroupBox.TabStop = false;
			// 
			// projectSelectionGroupBox
			// 
			this.projectSelectionGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("0e07b4f6-dd98-48d9-bd58-3f90c2d39bbe", "Projects");
			this.projectSelectionGroupBox.Controls.Add(this.ProjectKeysGrid);
			this.projectSelectionGroupBox.Controls.Add(this.MessageLabel);
			this.projectSelectionGroupBox.Controls.Add(this.ImportAllProjectsCheckBox);
			this.projectSelectionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 118, true);
			this.projectSelectionGroupBox.Name = "projectSelectionGroupBox";
			this.projectSelectionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 266, true);
			this.projectSelectionGroupBox.TabIndex = 14;
			this.projectSelectionGroupBox.TabStop = false;
			// 
			// MessageLabel
			// 
			this.MessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageLabel.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("65b781ce-0ccc-4e48-a502-5c8cf769156f", "Or, list specific projects to import:");
			this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageLabel, false);
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 39, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 28, true);
			this.MessageLabel.TabIndex = 2;
			// 
			// queryOptionsGroupBox
			// 
			this.queryOptionsGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("2970364a-938c-4eec-a2ec-780fde95cc3a", "Query Options");
			this.queryOptionsGroupBox.Controls.Add(this.ImportInProgressIssuesCheckBox);
			this.queryOptionsGroupBox.Controls.Add(this.IssueJQLTextBox);
			this.queryOptionsGroupBox.Controls.Add(this.ImportUnStartedIssuesCheckBox);
			this.queryOptionsGroupBox.Controls.Add(this.ImportIssueAttachmentsCheckBox);
			this.queryOptionsGroupBox.Controls.Add(this.ImportCompletedIssuesCheckBox);
			this.queryOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 390, true);
			this.queryOptionsGroupBox.Name = "queryOptionsGroupBox";
			this.queryOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 123, true);
			this.queryOptionsGroupBox.TabIndex = 15;
			this.queryOptionsGroupBox.TabStop = false;
			// 
			// JiraProjectSelectorUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.queryOptionsGroupBox);
			this.Controls.Add(this.projectSelectionGroupBox);
			this.Controls.Add(this.LoginGroupBox);
			this.Controls.Add(this.BeginImportButton);
			this.Controls.Add(this.CancelImportButton);
			this.Name = "JiraProjectSelectorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 549, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectKeysGrid)).EndInit();
			this.ProjectKeysGrid.ResumeLayout(false);
			this.ProjectKeysGrid.PerformLayout();
			this.JiraSystemCodeDropList.ResumeLayout(true);
			this.JiraSystemCodeDropList.PerformLayout();
			this.LoginGroupBox.ResumeLayout(false);
			this.LoginGroupBox.PerformLayout();
			this.projectSelectionGroupBox.ResumeLayout(false);
			this.projectSelectionGroupBox.PerformLayout();
			this.queryOptionsGroupBox.ResumeLayout(false);
			this.queryOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZGrid ProjectKeysGrid;
		private ZArchitecture.GUI.ZCheckBox ImportAllProjectsCheckBox;
		internal ZArchitecture.GUI.ZButton CancelImportButton;
		private ZArchitecture.GUI.ZButton BeginImportButton;
		private ZArchitecture.ZTextBox UserNameTextBox;
		private ZArchitecture.ZTextBox PasswordTextBox;
		private ZArchitecture.ZTextBox IssueJQLTextBox;
		private ZArchitecture.GUI.ZCheckBox ImportInProgressIssuesCheckBox;
		private ZArchitecture.GUI.ZCheckBox ImportUnStartedIssuesCheckBox;
		private ZArchitecture.GUI.ZCheckBox ImportCompletedIssuesCheckBox;
		private ZArchitecture.GUI.ZCheckBox ImportIssueAttachmentsCheckBox;
		private ZArchitecture.GUI.ZDropEdit JiraSystemCodeDropList;
		private ZArchitecture.GUI.ZGroupBox LoginGroupBox;
		private ZArchitecture.GUI.ZGroupBox projectSelectionGroupBox;
		private ZArchitecture.GUI.ZGroupBox queryOptionsGroupBox;
		protected ZArchitecture.ZLabel MessageLabel;
	}
}
