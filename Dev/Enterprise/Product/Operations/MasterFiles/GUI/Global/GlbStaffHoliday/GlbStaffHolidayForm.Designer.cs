using System;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbStaffHolidayForm
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected sealed override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.StaffHolidayTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.tabSecurityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zTextBoxFullName = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBoxDays = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEditStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDateEditEndDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEditStartDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDropEditType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StaffHolidayTabControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.zDropEditStatus.SuspendLayout();
			this.zDateEditEndDate.SuspendLayout();
			this.zDateEditStartDate.SuspendLayout();
			this.zDropEditType.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 684, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(703);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaffHoliday);
			// 
			// StaffHolidayTabControl
			// 
			this.StaffHolidayTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.StaffHolidayTabControl.Controls.Add(this.WorkflowTabPage);
			this.StaffHolidayTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.StaffHolidayTabControl.Controls.Add(this.zLogsTabPage1);
			this.StaffHolidayTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StaffHolidayTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.StaffHolidayTabControl.Name = "StaffHolidayTabControl";
			this.StaffHolidayTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 560, true);
			this.StaffHolidayTabControl.TabIndex = 1;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.AutoScroll = true;
			this.WorkflowTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffHoliday|cbb889d3-3e4a-47fa-bafc-643a83b46db1", "Workflow & Tracking");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 394, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 394, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 533, true);
			this.zLogsTabPage1.TabIndex = 3;
			// 
			// tabSecurityLabel
			// 
			this.tabSecurityLabel.AutoSize = true;
			this.tabSecurityLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffHoliday|767a7f81-438f-466b-97d9-e8422a520b8d", "You do not have security rights to edit this tab page.");
			this.tabSecurityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.tabSecurityLabel.ForeColor = System.Drawing.Color.Red;
			this.tabSecurityLabel.IsFontBold = true;
			this.tabSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(569, 14, true);
			this.tabSecurityLabel.Name = "tabSecurityLabel";
			this.tabSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 13, true);
			this.tabSecurityLabel.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 648, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 36, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 8, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.zTextBoxFullName);
			this.TopPanel.Controls.Add(this.zTextBoxDays);
			this.TopPanel.Controls.Add(this.zDropEditStatus);
			this.TopPanel.Controls.Add(this.zDateEditEndDate);
			this.TopPanel.Controls.Add(this.zDateEditStartDate);
			this.TopPanel.Controls.Add(this.zDropEditType);
			this.TopPanel.Controls.Add(this.tabSecurityLabel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 88, true);
			this.TopPanel.TabIndex = 0;
			// 
			// zTextBoxFullName
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxFullName, "Staff.GS_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(null)).Staff.GS_FullName)));
			this.zTextBoxFullName.CaptionResourceString = null;
			this.zTextBoxFullName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 59, true);
			this.zTextBoxFullName.Name = "zTextBoxFullName";
			this.zTextBoxFullName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zTextBoxFullName.TabIndex = 3;
			// 
			// zTextBoxDays
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxDays, "GA_DaysLeaveTaken");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(null)).GA_DaysLeaveTaken)));
			this.zTextBoxDays.CaptionResourceString = null;
			this.zTextBoxDays.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 59, true);
			this.zTextBoxDays.Name = "zTextBoxDays";
			this.zTextBoxDays.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zTextBoxDays.TabIndex = 6;
			// 
			// zDropEditStatus
			// 
			this.zDropEditStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditStatus, "GA_ApprovalStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(null)).GA_ApprovalStatus)));
			this.zDropEditStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 33, true);
			this.zDropEditStatus.Name = "zDropEditStatus";
			this.zDropEditStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zDropEditStatus.TabIndex = 2;
			// 
			// zDateEditEndDate
			// 
			this.zDateEditEndDate.AllowDrop = true;
			this.zDateEditEndDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEditEndDate, "GA_EndTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(null)).GA_EndTime)));
			this.zDateEditEndDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 33, true);
			this.zDateEditEndDate.Name = "zDateEditEndDate";
			this.zDateEditEndDate.TabIndex = 5;
			// 
			// zDateEditStartDate
			// 
			this.zDateEditStartDate.AllowDrop = true;
			this.zDateEditStartDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEditStartDate, "GA_StartTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(null)).GA_StartTime)));
			this.zDateEditStartDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 7, true);
			this.zDateEditStartDate.Name = "zDateEditStartDate";
			this.zDateEditStartDate.TabIndex = 4;
			// 
			// zDropEditType
			// 
			this.zDropEditType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditType, "GA_WorkHolidayType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(null)).GA_WorkHolidayType)));
			this.zDropEditType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 7, true);
			this.zDropEditType.Name = "zDropEditType";
			this.zDropEditType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.zDropEditType.TabIndex = 1;
			// 
			// GlbStaffHolidayForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 708, true);
			this.Controls.Add(this.StaffHolidayTabControl);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaffHoliday);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 538, true);
			this.Name = "GlbStaffHolidayForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.StaffHolidayTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StaffHolidayTabControl.ResumeLayout(false);
			this.StaffHolidayTabControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.zDropEditStatus.ResumeLayout(true);
			this.zDropEditStatus.PerformLayout();
			this.zDateEditEndDate.ResumeLayout(true);
			this.zDateEditEndDate.PerformLayout();
			this.zDateEditStartDate.ResumeLayout(true);
			this.zDateEditStartDate.PerformLayout();
			this.zDropEditType.ResumeLayout(true);
			this.zDropEditType.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl StaffHolidayTabControl;
		private Enterprise.ZArchitecture.ZLabel tabSecurityLabel;
		protected ZArchitecture.GUI.ZDropEdit zDropEditType;
		protected ZArchitecture.GUI.ZDateEdit zDateEditEndDate;
		protected ZArchitecture.GUI.ZDateEdit zDateEditStartDate;
		protected ZArchitecture.GUI.ZDropEdit zDropEditStatus;
		protected ZArchitecture.ZTextBox zTextBoxDays;
		protected ZArchitecture.ZTextBox zTextBoxFullName;
		private ZWorkflowTabPage WorkflowTabPage;
		private ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
	}
}
