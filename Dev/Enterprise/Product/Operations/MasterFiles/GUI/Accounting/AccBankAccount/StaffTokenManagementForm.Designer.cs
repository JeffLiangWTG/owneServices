using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class StaffTokenManagementForm
	{
		ZPanel bottomPanel;
		ZArchitecture.ZGrid staffTokenGrid;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.saveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AuthorizeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.staffTokenGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LearnMoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DisclaimerMessage = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.staffTokenGrid)).BeginInit();
			this.staffTokenGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 495, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccBankAccount);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.RefreshButton);
			this.bottomPanel.Controls.Add(this.saveButton);
			this.bottomPanel.Controls.Add(this.AuthorizeButton);
			this.bottomPanel.Controls.Add(this.CloseButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 519, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 41, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// RefreshButton
			// 
			this.RefreshButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e5a0bf6a-691d-4dc1-b0a3-ed6ea0eb5227", "Refresh");
			this.RefreshButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.RefreshButton.IsCaptionOverridden = false;
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 0, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 41, true);
			this.RefreshButton.TabIndex = 1;
			this.RefreshButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RefreshButton.ToolTipCaption = null;
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// saveButton
			// 
			this.saveButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d982d07b-d6a0-4ab0-a2f5-6e3f4d73b2e6", "Save");
			this.saveButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.saveButton.IsCaptionOverridden = false;
			this.saveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(739, 0, true);
			this.saveButton.Name = "saveButton";
			this.saveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.saveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 41, true);
			this.saveButton.TabIndex = 2;
			this.saveButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.saveButton.ToolTipCaption = null;
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// AuthorizeButton
			// 
			this.AuthorizeButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7873fbc6-fe29-48c6-a0d4-2d3ae480f431", "Authorize");
			this.AuthorizeButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AuthorizeButton.IsCaptionOverridden = false;
			this.AuthorizeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(822, 0, true);
			this.AuthorizeButton.Name = "AuthorizeButton";
			this.AuthorizeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AuthorizeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 41, true);
			this.AuthorizeButton.TabIndex = 3;
			this.AuthorizeButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AuthorizeButton.ToolTipCaption = null;
			this.AuthorizeButton.UseVisualStyleBackColor = true;
			this.AuthorizeButton.Click += new System.EventHandler(this.AuthorizeButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2c4b7d30-82a6-42a5-8df8-31782eaa437c", "Close");
			this.CloseButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(897, 0, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 41, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// staffTokenGrid
			// 
			this.staffTokenGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.staffTokenGrid, "EPaymentStaffTokenCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).EPaymentStaffTokenCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEPaymentStaffToken)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).EPaymentStaffTokenCollection)).SyncRoot)).TK_GS_NKStaffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEPaymentStaffToken)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).EPaymentStaffTokenCollection)).SyncRoot)).StaffCode.GS_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEPaymentStaffToken)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).EPaymentStaffTokenCollection)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.AccEPaymentStaffToken)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).EPaymentStaffTokenCollection)).SyncRoot)).TK_ExpiryUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.AccEPaymentStaffToken)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).EPaymentStaffTokenCollection)).SyncRoot)).TK_RequestedUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEPaymentStaffToken)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).EPaymentStaffTokenCollection)).SyncRoot)).TK_ErrorDescription)));
			this.staffTokenGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "TK_GS_NKStaffCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "StaffCode+GS_FullName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Status";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "TK_ExpiryUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "TK_RequestedUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "TK_ErrorDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.staffTokenGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.staffTokenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.staffTokenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.staffTokenGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.staffTokenGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.staffTokenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.staffTokenGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.staffTokenGrid.GridId = "AF0B57D2-B1E1-4E04-9CB4-2524E2DCF45A";
			this.staffTokenGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.staffTokenGrid.LayoutKey = "zGrid1";
			this.staffTokenGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 97, true);
			this.staffTokenGrid.Name = "staffTokenGrid";
			this.staffTokenGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 398, true);
			this.staffTokenGrid.TabIndex = 4;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.LearnMoreButton);
			this.zPanel1.Controls.Add(this.DisclaimerMessage);
			this.zPanel1.Controls.Add(this.staffTokenGrid);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 495, true);
			this.zPanel1.TabIndex = 3;
			// 
			// LearnMoreButton
			// 
			this.LearnMoreButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0bfdf9f9-53f0-458b-b9e5-b768f1425003", "Learn More");
			this.LearnMoreButton.IsCaptionOverridden = false;
			this.LearnMoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(885, 54, true);
			this.LearnMoreButton.Name = "LearnMoreButton";
			this.LearnMoreButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LearnMoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.LearnMoreButton.TabIndex = 2;
			this.LearnMoreButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.LearnMoreButton.ToolTipCaption = null;
			this.LearnMoreButton.UseVisualStyleBackColor = true;
			this.LearnMoreButton.Click += new System.EventHandler(this.LearnMoreButton_Click);
			// 
			// DisclaimerMessage
			// 
			this.DisclaimerMessage.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DisclaimerMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.DisclaimerMessage.Name = "DisclaimerMessage";
			this.DisclaimerMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 68, true);
			this.DisclaimerMessage.TabIndex = 3;
			this.DisclaimerMessage.Text = "zLabel1";
			// 
			// StaffTokenManagementForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fca7830e-4472-4208-bf5c-91d3b280c65e", "Manage E-Payment Account Users");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 560, true);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccBankAccount);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 599, true);
			this.Name = "StaffTokenManagementForm";
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.staffTokenGrid)).EndInit();
			this.staffTokenGrid.ResumeLayout(false);
			this.staffTokenGrid.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZButton AuthorizeButton;
		private ZButton CloseButton;
		private ZButton saveButton;
		private ZButton RefreshButton;
		private ZPanel zPanel1;
		private ZLabel DisclaimerMessage;
		private ZButton LearnMoreButton;
	}
}
