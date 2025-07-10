namespace Enterprise.MasterFiles.GUI
{
	public partial class WarningAcknowledgementForm
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
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.tabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.mainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLogsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 349, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 20, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(649);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRuleAck);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Visible = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 504, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.mainTabPage);
			this.tabControl.Controls.Add(this.zLogsTabPage);
			this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, -3, true);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 320, true);
			this.tabControl.TabIndex = 0;
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.warningField = new Enterprise.ZArchitecture.ZTextBox();
			this.isCancelledCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ruleName = new Enterprise.ZArchitecture.ZTextBox();
			this.createdTime = new Enterprise.ZArchitecture.ZTextBox();
			this.createdUser = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.parentCodeDescription = new Enterprise.ZArchitecture.ZTextBox();
			this.parentTableCode = new Enterprise.ZArchitecture.ZTextBox();
			this.openJob = new Enterprise.ZArchitecture.GUI.ZButton();
			this.mainTabPage.SuspendLayout();
			this.createdUser.SuspendLayout();
			this.mainTabPage.Controls.Add(this.warningField);
			this.mainTabPage.Controls.Add(this.isCancelledCheckbox);
			this.mainTabPage.Controls.Add(this.ruleName);
			this.mainTabPage.Controls.Add(this.createdTime);
			this.mainTabPage.Controls.Add(this.createdUser);
			this.mainTabPage.Controls.Add(this.parentCodeDescription);
			this.mainTabPage.Controls.Add(this.parentTableCode);
			this.mainTabPage.Controls.Add(this.openJob);
			// 
			// warningField
			// 
			this.warningField.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.warningField, "XK_Warning");
			this.warningField.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1251CE13-4ED6-4C1A-BF21-9BDB6F23E85A", "Field Name");
			this.warningField.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 41, true);
			this.warningField.Name = "warningField";
			this.warningField.ReadOnly = true;
			this.warningField.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.warningField.TabIndex = 1;
			// 
			// isCancelledCheckbox
			// 
			this.BindingSource.SetBindingMember(this.isCancelledCheckbox, "XK_IsCancelled");
			this.isCancelledCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5971AB2E-841F-4C59-BCBD-CCC02E8E5FFB", "Is Canceled");
			this.isCancelledCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isCancelledCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 14, true);
			this.isCancelledCheckbox.Name = "isCancelledCheckbox";
			this.isCancelledCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 24, true);
			this.isCancelledCheckbox.TabIndex = 0;
			// 
			// ruleName
			// 
			this.BindingSource.SetBindingMember(this.ruleName, "XK_RuleIDHumanReadableName");
			this.ruleName.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("71AA0CB9-FCFD-4C4A-AEAA-7E363513DC8C", "Rule Name");
			this.ruleName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 126, true);
			this.ruleName.Name = "ruleName";
			this.ruleName.ReadOnly = true;
			this.ruleName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.ruleName.TabIndex = 4;
			// 
			// createdTime
			// 
			this.BindingSource.SetBindingMember(this.createdTime, "XK_SystemCreateTimeUtc");
			this.createdTime.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("12AA2199-885C-45D3-9235-E0D48DC74C4D", "Created Time");
			this.createdTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 98, true);
			this.createdTime.Name = "createdTime";
			this.createdTime.ReadOnly = true;
			this.createdTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.createdTime.TabIndex = 3;
			// 
			// createdUser
			// 
			this.createdUser.AllowDrop = false;
			this.createdUser.ReadOnly = true;
			this.BindingSource.SetBindingMember(this.createdUser, "XK_SystemCreateUser");
			this.createdUser.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6164733C-DC53-411C-9CE3-2A1B35568171", "Created User");
			this.createdUser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 69, true);
			this.createdUser.Name = "createdUser";
			this.createdUser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.createdUser.TabIndex = 2;
			// 
			// parentCodeDescription
			// 
			this.BindingSource.SetBindingMember(this.parentCodeDescription, "XK_ParentIDHumanReadableName");
			this.parentCodeDescription.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C6CCD286-255E-4017-87F3-B0F42498D34F", "Parent Details");
			this.parentCodeDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 179, true);
			this.parentCodeDescription.Name = "parentCodeDescription";
			this.parentCodeDescription.ReadOnly = true;
			this.parentCodeDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.parentCodeDescription.TabIndex = 6;
			// 
			// parentTableCode
			// 
			this.BindingSource.SetBindingMember(this.parentTableCode, "XK_ParentTableCode");
			this.parentTableCode.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DD75D1F8-D246-40E4-A4B9-0FE0F827F83D", "Parent Table Code");
			this.parentTableCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 153, true);
			this.parentTableCode.Name = "parentTableCode";
			this.parentTableCode.ReadOnly = true;
			this.parentTableCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.parentTableCode.TabIndex = 5;
			// 
			// openJob
			// 
			this.openJob.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("665E5C4F-E09F-4955-B0F9-82F2C22F7D71", "Open Parent File");
			this.openJob.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 170, true);
			this.openJob.Name = "openJob";
			this.openJob.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.openJob.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 28, true);
			this.openJob.TabIndex = 7;
			this.openJob.Click += OpenJob_Click;
			this.openJob.ToolTipCaption = null;
			this.mainTabPage.PerformLayout();
			this.createdUser.ResumeLayout(true);
			this.createdUser.PerformLayout();
			this.mainTabPage.ResumeLayout(true);
			// 
			// mainTabPage
			// 
			this.mainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WarningAcknowledgementForm|D6046FC0-E822-4887-8E1F-F729B4B496DC", "Details");
			this.mainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.mainTabPage.Name = "mainTabPage";
			this.mainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 293, true);
			this.mainTabPage.TabIndex = 1;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRuleAck)(null)).XK_Warning)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRuleAck)(null)).XK_IsCancelled)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRuleAck)(null)).XK_RuleIDHumanReadableName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRuleAck)(null)).XK_SystemCreateTimeUtc)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRuleAck)(null)).XK_SystemCreateUser)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRuleAck)(null)).XK_ParentIDHumanReadableName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRuleAck)(null)).XK_ParentTableCode)));
			// 
			// zLogsTabPage
			// 
			this.zLogsTabPage.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage.Name = "zLogsTabPage";
			this.zLogsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 282, true);
			this.zLogsTabPage.TabIndex = 2;
			// 
			// WarningAcknowledgementForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WarningAcknowledgementForm|CEF7E1AC-26F7-4D0C-B52F-752155615199", "Warning Acknowledgement");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 369, true);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRuleAck);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 408, true);
			this.Name = "WarningAcknowledgementForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.tabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.tabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		public Enterprise.ZArchitecture.GUI.ZCheckBox isCancelledCheckbox;
		Enterprise.ZArchitecture.ZTextBox warningField;
		Enterprise.ZArchitecture.GUI.ZButton openJob;
		Enterprise.ZArchitecture.ZTextBox ruleName;
		Enterprise.ZArchitecture.ZTextBox parentTableCode;
		Enterprise.ZArchitecture.ZTextBox createdTime;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox createdUser;
		Enterprise.ZArchitecture.ZTextBox parentCodeDescription;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl tabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage mainTabPage;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage;
	}
}
