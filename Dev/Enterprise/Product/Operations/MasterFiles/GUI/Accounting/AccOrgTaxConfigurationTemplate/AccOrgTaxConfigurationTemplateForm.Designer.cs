namespace Enterprise.MasterFiles.GUI
{
	public partial class AccOrgTaxConfigurationTemplateForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.TaxConfigurationTemplateTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TaxConfigurationTemplateTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TaxConfigEditPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TaxConfigEditControl = new Enterprise.MasterFiles.GUI.AccOrgTaxConfigurationEditControl();
			this.TaxConfigurationTemplatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TemplateTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsPayablesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsReceivableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TemplateCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TemplateDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LinkedOrganizationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FilterLinkedOrganizationsControl = new Enterprise.MasterFiles.GUI.FilterLinkedOrganizationsControl();
			this.StmNoteTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.LogsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TaxConfigurationTemplateTabControl.SuspendLayout();
			this.TaxConfigurationTemplateTabPage.SuspendLayout();
			this.TaxConfigEditPanel.SuspendLayout();
			this.TaxConfigEditControl.SuspendLayout();
			this.TaxConfigurationTemplatePanel.SuspendLayout();
			this.TemplateTypeGroupBox.SuspendLayout();
			this.LinkedOrganizationsTabPage.SuspendLayout();
			this.FilterLinkedOrganizationsControl.SuspendLayout();
			this.StmNoteTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 589, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(737, 559, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// TaxConfigurationTemplateTabControl
			// 
			this.TaxConfigurationTemplateTabControl.Controls.Add(this.TaxConfigurationTemplateTabPage);
			this.TaxConfigurationTemplateTabControl.Controls.Add(this.LinkedOrganizationsTabPage);
			this.TaxConfigurationTemplateTabControl.Controls.Add(this.StmNoteTabPage);
			this.TaxConfigurationTemplateTabControl.Controls.Add(this.LogsTabPage);
			this.TaxConfigurationTemplateTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxConfigurationTemplateTabControl.Name = "TaxConfigurationTemplateTabControl";
			this.TaxConfigurationTemplateTabControl.SelectedIndex = 0;
			this.TaxConfigurationTemplateTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 553, true);
			this.TaxConfigurationTemplateTabControl.TabIndex = 1;
			// 
			// TaxConfigurationTemplateTabPage
			// 
			this.TaxConfigurationTemplateTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccOrgTaxConfigurationTemplateForm|a3b2570b-b7a0-4b3e-aa24-110ad3b6a9fc", "Tax Configuration Template");
			this.TaxConfigurationTemplateTabPage.Controls.Add(this.TaxConfigEditPanel);
			this.TaxConfigurationTemplateTabPage.Controls.Add(this.TaxConfigurationTemplatePanel);
			this.TaxConfigurationTemplateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TaxConfigurationTemplateTabPage.Name = "TaxConfigurationTemplateTabPage";
			this.TaxConfigurationTemplateTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TaxConfigurationTemplateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 526, true);
			this.TaxConfigurationTemplateTabPage.TabIndex = 0;
			this.TaxConfigurationTemplateTabPage.UseVisualStyleBackColor = true;
			// 
			// TaxConfigEditPanel
			// 
			this.TaxConfigEditPanel.Controls.Add(this.TaxConfigEditControl);
			this.TaxConfigEditPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxConfigEditPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 118, true);
			this.TaxConfigEditPanel.Name = "TaxConfigEditPanel";
			this.TaxConfigEditPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 405, true);
			this.TaxConfigEditPanel.TabIndex = 0;
			// 
			// TaxConfigEditControl
			// 
			this.TaxConfigEditControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxConfigEditControl, ".");
			this.TaxConfigEditControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxConfigEditControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxConfigEditControl.Name = "TaxConfigEditControl";
			this.TaxConfigEditControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 405, true);
			this.TaxConfigEditControl.TabIndex = 0;
			// 
			// TaxConfigurationTemplatePanel
			// 
			this.TaxConfigurationTemplatePanel.Controls.Add(this.TemplateTypeGroupBox);
			this.TaxConfigurationTemplatePanel.Controls.Add(this.IsActiveCheckBox);
			this.TaxConfigurationTemplatePanel.Controls.Add(this.TemplateCodeTextBox);
			this.TaxConfigurationTemplatePanel.Controls.Add(this.TemplateDescriptionTextBox);
			this.TaxConfigurationTemplatePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TaxConfigurationTemplatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TaxConfigurationTemplatePanel.Name = "TaxConfigurationTemplatePanel";
			this.TaxConfigurationTemplatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 115, true);
			this.TaxConfigurationTemplatePanel.TabIndex = 2;
			// 
			// TemplateTypeGroupBox
			// 
			this.TemplateTypeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ce229f10-1a88-4740-9449-a5d892da8bb6", "Template Type");
			this.TemplateTypeGroupBox.Controls.Add(this.IsPayablesCheckBox);
			this.TemplateTypeGroupBox.Controls.Add(this.IsReceivableCheckBox);
			this.TemplateTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(644, 22, true);
			this.TemplateTypeGroupBox.Name = "TemplateTypeGroupBox";
			this.TemplateTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 72, true);
			this.TemplateTypeGroupBox.TabIndex = 3;
			this.TemplateTypeGroupBox.TabStop = false;
			// 
			// IsPayablesCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsPayablesCheckBox, "OCT_IsPayable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).OCT_IsPayable)));
			this.IsPayablesCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6d2c9c0b-8d3a-4a45-9fcc-cec8d91a640e", "A/P - Payables Organizations");
			this.IsPayablesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPayablesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 42, true);
			this.IsPayablesCheckBox.Name = "IsPayablesCheckBox";
			this.IsPayablesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 24, true);
			this.IsPayablesCheckBox.TabIndex = 1;
			this.IsPayablesCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsReceivableCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsReceivableCheckBox, "OCT_IsReceivable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).OCT_IsReceivable)));
			this.IsReceivableCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9d2613e4-40c2-4118-a070-d07600261648", "A/R - Receivables Organizations");
			this.IsReceivableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsReceivableCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.IsReceivableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 16, true);
			this.IsReceivableCheckBox.Name = "IsReceivableCheckBox";
			this.IsReceivableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 24, true);
			this.IsReceivableCheckBox.TabIndex = 0;
			this.IsReceivableCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "OCT_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).OCT_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("13e5428f-304a-45d9-8425-70b407f1f0b1", "Is Active");
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 34, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 24, true);
			this.IsActiveCheckBox.TabIndex = 2;
			this.IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// TemplateCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TemplateCodeTextBox, "OCT_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).OCT_Code)));
			this.TemplateCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7048c0b1-65c6-4e8f-a5ad-1bbf4e3ae244", "Template Code");
			this.TemplateCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 34, true);
			this.TemplateCodeTextBox.Name = "TemplateCodeTextBox";
			this.TemplateCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.TemplateCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 20, true);
			this.TemplateCodeTextBox.TabIndex = 0;
			// 
			// TemplateDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.TemplateDescriptionTextBox, "OCT_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).OCT_Description)));
			this.TemplateDescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("db5009b0-264d-4c0e-bf21-e25fe0570574", "Template Description");
			this.TemplateDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 68, true);
			this.TemplateDescriptionTextBox.Name = "TemplateDescriptionTextBox";
			this.TemplateDescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.TemplateDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 20, true);
			this.TemplateDescriptionTextBox.TabIndex = 1;
			// 
			// LinkedOrganizationsTabPage
			// 
			this.LinkedOrganizationsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.LinkedOrganizationsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccOrgTaxConfigurationTemplateForm|4fe8aebd-f79b-4820-a86d-bf058ea3ce5e", "Linked Organizations");
			this.LinkedOrganizationsTabPage.Controls.Add(this.FilterLinkedOrganizationsControl);
			this.LinkedOrganizationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinkedOrganizationsTabPage.Name = "LinkedOrganizationsTabPage";
			this.LinkedOrganizationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LinkedOrganizationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 526, true);
			this.LinkedOrganizationsTabPage.TabIndex = 1;
			this.LinkedOrganizationsTabPage.UseVisualStyleBackColor = true;
			// 
			// FilterLinkedOrganizationsControl
			// 
			this.FilterLinkedOrganizationsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FilterLinkedOrganizationsControl, ".");
			this.FilterLinkedOrganizationsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterLinkedOrganizationsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FilterLinkedOrganizationsControl.Name = "FilterLinkedOrganizationsControl";
			this.FilterLinkedOrganizationsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(969, 544, true);
			this.FilterLinkedOrganizationsControl.TabIndex = 0;
			// 
			// StmNoteTabPage
			// 
			this.StmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StmNoteTabPage.Name = "StmNoteTabPage";
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 526, true);
			this.StmNoteTabPage.TabIndex = 2;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.ExcludeFromBindingOnSave = true;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 526, true);
			this.LogsTabPage.TabIndex = 3;
			// 
			// AccOrgTaxConfigurationTemplateForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccOrgTaxConfigurationTemplateForm|05cf772d-ac47-47e7-90c2-90d40b0a0df8", "Tax Configuration Template");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 611, true);
			this.Controls.Add(this.TaxConfigurationTemplateTabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 650, true);
			this.Name = "AccOrgTaxConfigurationTemplateForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.TaxConfigurationTemplateTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.TaxConfigurationTemplateTabControl.ResumeLayout(false);
			this.TaxConfigurationTemplateTabControl.PerformLayout();
			this.TaxConfigurationTemplateTabPage.ResumeLayout(false);
			this.TaxConfigurationTemplateTabPage.PerformLayout();
			this.TaxConfigEditPanel.ResumeLayout(false);
			this.TaxConfigEditPanel.PerformLayout();
			this.TaxConfigEditControl.ResumeLayout(true);
			this.TaxConfigEditControl.PerformLayout();
			this.TaxConfigurationTemplatePanel.ResumeLayout(false);
			this.TaxConfigurationTemplatePanel.PerformLayout();
			this.TemplateTypeGroupBox.ResumeLayout(false);
			this.TemplateTypeGroupBox.PerformLayout();
			this.LinkedOrganizationsTabPage.ResumeLayout(false);
			this.LinkedOrganizationsTabPage.PerformLayout();
			this.FilterLinkedOrganizationsControl.ResumeLayout(true);
			this.FilterLinkedOrganizationsControl.PerformLayout();
			this.StmNoteTabPage.ResumeLayout(false);
			this.StmNoteTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.MasterFiles.GUI.AccOrgTaxConfigurationEditControl TaxConfigEditControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl TaxConfigurationTemplateTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage TaxConfigurationTemplateTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage LinkedOrganizationsTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage StmNoteTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage LogsTabPage;
		private Enterprise.ZArchitecture.GUI.ZPanel TaxConfigurationTemplatePanel;
		private Enterprise.ZArchitecture.GUI.ZPanel TaxConfigEditPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox TemplateTypeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsPayablesCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsReceivableCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private ZArchitecture.ZTextBox TemplateCodeTextBox;
		private ZArchitecture.ZTextBox TemplateDescriptionTextBox;
		private FilterLinkedOrganizationsControl FilterLinkedOrganizationsControl;
	}
}
