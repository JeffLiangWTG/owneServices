namespace Enterprise.MasterFiles.GUI
{
	partial class GlbStaffManagerHistoryForm
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.staffManagementHistoryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.staffManagementHistoryGrid)).BeginInit();
			this.staffManagementHistoryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.staffManagementHistoryGrid);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 505, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaffManagerCollection);
			// 
			// staffManagementHistoryGrid
			// 
			this.staffManagementHistoryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.staffManagementHistoryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_ManagerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).ReportingRole.Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_GS_Manager)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).Manager.GS_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).Manager.HomeBranch.GB_BranchName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_EffectiveDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_SystemLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_SystemLastEditTimeUtc)));
			this.staffManagementHistoryGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|ReportingRoleCode", "Role");
			zDropEditColumnStyleInfo1.ColumnName = "GSM_ManagerType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|ReportingRoleDescription", "Role Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ReportingRole+Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|Manager", "Manager");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GSM_GS_Manager";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|ManagerFullName", "Manager Name");
			zTextBoxColumnStyleInfo2.ColumnName = "Manager+GS_FullName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|Branch", "Branch");
			zTextBoxColumnStyleInfo3.ColumnName = "Manager+HomeBranch+GB_BranchName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|EffectiveDate", "Effective Date");
			zDateEditColumnStyleInfo1.ColumnName = "GSM_EffectiveDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|EndDate", "End Date");
			zDateEditColumnStyleInfo2.ColumnName = "GSM_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|SystemCreateUser", "System Create User");
			zTextBoxColumnStyleInfo4.ColumnName = "GSM_SystemCreateUser";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|SystemCreateTimeUtc", "System Create Time");
			zDateEditColumnStyleInfo3.ColumnName = "GSM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|SystemLastEditUser", "System Last Edit User");
			zTextBoxColumnStyleInfo5.ColumnName = "GSM_SystemLastEditUser";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|SystemLastEditTimeUtc", "System Last Edit Time");
			zDateEditColumnStyleInfo4.ColumnName = "GSM_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.staffManagementHistoryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.staffManagementHistoryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.staffManagementHistoryGrid.GridId = "978a697c-4da0-45f6-b1b3-bd7ba2577913";
			this.staffManagementHistoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.staffManagementHistoryGrid.LayoutKey = "staffManagementHistoryGrid";
			this.staffManagementHistoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.staffManagementHistoryGrid.Name = "staffManagementHistoryGrid";
			this.staffManagementHistoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 505, true);
			this.staffManagementHistoryGrid.TabIndex = 1;
			// 
			// GlbStaffManagerHistoryForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerHistoryForm|Title", "Reporting Manager History");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 561, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaffManagerCollection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1107, 600, true);
			this.Name = "GlbStaffManagerHistoryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.staffManagementHistoryGrid)).EndInit();
			this.staffManagementHistoryGrid.ResumeLayout(false);
			this.staffManagementHistoryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid staffManagementHistoryGrid;
	}
}