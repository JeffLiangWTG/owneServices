namespace Enterprise.MasterFiles.GUI
{
	public partial class StaffAssignmentsControl
	{

		#region Component Designer generated code

		internal Enterprise.ZArchitecture.ZGrid StaffAssignmentsGrid;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.StaffAssignmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StaffAssignmentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// StaffAssignmentsGrid
			// 
			this.StaffAssignmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StaffAssignmentsGrid, "StaffAssignments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).StaffAssignments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgStaffAssignments)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).StaffAssignments)).SyncRoot)).O8_Role)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgStaffAssignments)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).StaffAssignments)).SyncRoot)).RoleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgStaffAssignments)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).StaffAssignments)).SyncRoot)).O8_GS_NKPersonResponsible)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgStaffAssignments)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).StaffAssignments)).SyncRoot)).ResponsiblePersonName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgStaffAssignments)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).StaffAssignments)).SyncRoot)).ResponsiblePersonLoginName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgStaffAssignments)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).StaffAssignments)).SyncRoot)).O8_Department)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgStaffAssignments)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).StaffAssignments)).SyncRoot)).O8_GC)));
			this.StaffAssignmentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "O8_Role";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StaffAssignmentsControl|d9c58cbf-549c-4918-a53a-66ec6ebf73db", "Role Description");
			zTextBoxColumnStyleInfo1.ColumnName = "RoleDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "O8_GS_NKPersonResponsible";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StaffAssignmentsControl|4a3bf23b-4c27-49c6-90f2-d966f5b4de16", "Responsible Person");
			zTextBoxColumnStyleInfo2.ColumnName = "ResponsiblePersonName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StaffAssignmentsControl|22ea3ba8-1c52-4302-b789-c4a790a1017b", "Login Name");
			zTextBoxColumnStyleInfo3.ColumnName = "ResponsiblePersonLoginName";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "O8_Department";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "O8_GC";
			zGuidFindBoxColumnStyleInfo2.ToolTip = "The company that this staff assignment relates to.";
			this.StaffAssignmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.StaffAssignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StaffAssignmentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.StaffAssignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.StaffAssignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.StaffAssignmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.StaffAssignmentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.StaffAssignmentsGrid.GridId = "dba04f8d-939d-4b03-bfd4-bea80766e863";
			this.StaffAssignmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StaffAssignmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StaffAssignmentsGrid.LayoutKey = "StaffAssignmentsGrid";
			this.StaffAssignmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StaffAssignmentsGrid.Name = "StaffAssignmentsGrid";
			this.StaffAssignmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 416, true);
			this.StaffAssignmentsGrid.TabIndex = 0;
			// 
			// StaffAssignmentsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StaffAssignmentsGrid);
			this.Name = "StaffAssignmentsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StaffAssignmentsGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
