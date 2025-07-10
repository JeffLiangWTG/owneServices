namespace Enterprise.MasterFiles.Module
{
	partial class StaffReportingManagerRoleFilterControl
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
			this.ManagerFindBox = new ZArchitecture.GUI.ZGuidFindBox();
			this.ReportingRoleDropEdit = new ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.StaffReportingManagerRoleModuleFilter);
			// 
			// ManagerFindBox
			// 
			this.BindingSource.SetBindingMember(this.ManagerFindBox, "Manager");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.StaffReportingManagerRoleModuleFilter)(null)).Manager)));
			this.ManagerFindBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("StaffReportingManagerRoleFilterControl|390ec28a-1ca9-4d70-84b2-38efc17a84e7", "Manager");
			this.ManagerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 1, true);
			this.ManagerFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.ModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff));
			this.ManagerFindBox.Name = "ManagerFindBox";
			this.ManagerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.ManagerFindBox.TabIndex = 3;
			// 
			// ReportingRoleDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ReportingRoleDropEdit, "ReportingRole");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.StaffReportingManagerRoleModuleFilter)(null)).ReportingRole)));
			this.ReportingRoleDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("StaffReportingManagerRoleFilterControl|f0050be8-7d16-42f6-99fa-2853a6c4774f", "Role");
			this.ReportingRoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 23, true);
			this.ReportingRoleDropEdit.Name = "ReportingRoleDropEdit";
			this.ReportingRoleDropEdit.PreBoundMaxLength = 2;
			this.ReportingRoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.ReportingRoleDropEdit.TabIndex = 4;
			// 
			// StaffReportingManagerRoleFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManagerFindBox);
			this.Controls.Add(this.ReportingRoleDropEdit);
			this.Name = "StaffReportingManagerRoleFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 43, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ManagerFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ReportingRoleDropEdit;
	}
}
