
namespace Enterprise.MasterFiles.Module
{
	partial class OrgClientAssignedStaffFilterStrip
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
			this.ClientTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StaffRoleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AssignedStaffOperatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AssignedStaffCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DepartmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ControllingBranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClientTypeDropEdit.SuspendLayout();
			this.StaffRoleDropEdit.SuspendLayout();
			this.AssignedStaffOperatorDropEdit.SuspendLayout();
			this.AssignedStaffCodeFindBox.SuspendLayout();
			this.DepartmentDropEdit.SuspendLayout();
			this.ControllingBranchFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter);
			// 
			// ClientTypeDropEdit
			// 
			this.ClientTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientTypeDropEdit, "ClientType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter)(null)).ClientType)));
			this.ClientTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("3caf6bc8-678b-40ef-9191-c8c78afc59b4", "Client Type");
			this.ClientTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 1, true);
			this.ClientTypeDropEdit.Name = "ClientTypeDropEdit";
			this.ClientTypeDropEdit.PreBoundMaxLength = 2;
			this.ClientTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ClientTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.ClientTypeDropEdit.TabIndex = 2;
			// 
			// StaffRoleDropEdit
			// 
			this.StaffRoleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StaffRoleDropEdit, "StaffRole");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter)(null)).StaffRole)));
			this.StaffRoleDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("dcb998c6-60a8-44ce-b326-36f16a17a446", "Staff Role");
			this.StaffRoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 23, true);
			this.StaffRoleDropEdit.Name = "StaffRoleDropEdit";
			this.StaffRoleDropEdit.PreBoundMaxLength = 2;
			this.StaffRoleDropEdit.ShouldResizeByMaxLength = true;
			this.StaffRoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.StaffRoleDropEdit.TabIndex = 3;
			// 
			// AssignedStaffOperatorDropEdit
			// 
			this.AssignedStaffOperatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AssignedStaffOperatorDropEdit, "ComparisonOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter)(null)).ComparisonOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter)(null)).ComparisonOperator_List)));
			this.AssignedStaffOperatorDropEdit.BindToList = "ComparisonOperator_List";
			this.AssignedStaffOperatorDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("d04a605d-37eb-4a55-a22b-6aa5f8f69281", "Assigned Staff");
			this.AssignedStaffOperatorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.AssignedStaffOperatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 45, true);
			this.AssignedStaffOperatorDropEdit.Name = "AssignedStaffOperatorDropEdit";
			this.AssignedStaffOperatorDropEdit.ShouldResizeByMaxLength = true;
			this.AssignedStaffOperatorDropEdit.ShowDescriptionBox = false;
			this.AssignedStaffOperatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AssignedStaffOperatorDropEdit.TabIndex = 7;
			this.AssignedStaffOperatorDropEdit.Visible = false;
			// 
			// AssignedStaffCodeFindBox
			// 
			this.AssignedStaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AssignedStaffCodeFindBox, "AssignedStaff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter)(null)).AssignedStaff)));
			this.AssignedStaffCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("fdda8de7-8599-418c-b71d-84f33c8d0832", "Assigned Staff");
			this.AssignedStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 45, true);
			this.AssignedStaffCodeFindBox.Name = "AssignedStaffCodeFindBox";
			this.AssignedStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.AssignedStaffCodeFindBox.TabIndex = 4;
			// 
			// DepartmentDropEdit
			// 
			this.DepartmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartmentDropEdit, "Department");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter)(null)).Department)));
			this.DepartmentDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("ef17dbee-5620-483b-84c2-2383c6f1a058", "Department");
			this.DepartmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 67, true);
			this.DepartmentDropEdit.Name = "DepartmentDropEdit";
			this.DepartmentDropEdit.PreBoundMaxLength = 2;
			this.DepartmentDropEdit.ShouldResizeByMaxLength = true;
			this.DepartmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.DepartmentDropEdit.TabIndex = 5;
			// 
			// ControllingBranchFindBox
			// 
			this.ControllingBranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingBranchFindBox, "ControllingBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter)(null)).ControllingBranch)));
			this.ControllingBranchFindBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("d0098f27-cd54-4ee9-815b-67118decd88d", "Controlling Branch");
			this.ControllingBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 89, true);
			this.ControllingBranchFindBox.Name = "ControllingBranchFindBox";
			this.ControllingBranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.ControllingBranchFindBox.TabIndex = 6;
			// 
			// OrgClientAssignedStaffFilterStrip
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AssignedStaffOperatorDropEdit);
			this.Controls.Add(this.ControllingBranchFindBox);
			this.Controls.Add(this.DepartmentDropEdit);
			this.Controls.Add(this.AssignedStaffCodeFindBox);
			this.Controls.Add(this.StaffRoleDropEdit);
			this.Controls.Add(this.ClientTypeDropEdit);
			this.Name = "OrgClientAssignedStaffFilterStrip";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 113, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClientTypeDropEdit.ResumeLayout(true);
			this.ClientTypeDropEdit.PerformLayout();
			this.StaffRoleDropEdit.ResumeLayout(true);
			this.StaffRoleDropEdit.PerformLayout();
			this.AssignedStaffOperatorDropEdit.ResumeLayout(true);
			this.AssignedStaffOperatorDropEdit.PerformLayout();
			this.AssignedStaffCodeFindBox.ResumeLayout(true);
			this.AssignedStaffCodeFindBox.PerformLayout();
			this.DepartmentDropEdit.ResumeLayout(true);
			this.DepartmentDropEdit.PerformLayout();
			this.ControllingBranchFindBox.ResumeLayout(true);
			this.ControllingBranchFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit ClientTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit StaffRoleDropEdit;
		private ZArchitecture.GUI.ZDropEdit AssignedStaffOperatorDropEdit;
		private ZArchitecture.GUI.ZFilterCollectionFindBox AssignedStaffFilterCollectionFindBox;
		private ZArchitecture.GUI.ZCodeFindBox AssignedStaffCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit DepartmentDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox ControllingBranchFindBox;
	}
}
