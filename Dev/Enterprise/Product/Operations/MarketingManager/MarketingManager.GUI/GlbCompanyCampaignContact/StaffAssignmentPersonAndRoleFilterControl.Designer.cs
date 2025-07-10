namespace Enterprise.MarketingManager.GUI
{
	partial class StaffAssignmentPersonAndRoleFilterControl
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
			this.StaffAssignmentPersonFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			this.StaffAssignmentRoleDropEdit = new ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.StaffAssignmentPersonAndRoleModuleFilter);
			// 
			// StaffAssignmentPersonFindBox
			// 
			this.BindingSource.SetBindingMember(this.StaffAssignmentPersonFindBox, "StaffAssignmentPerson");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.StaffAssignmentPersonAndRoleModuleFilter)(null)).StaffAssignmentPerson)));
			this.StaffAssignmentPersonFindBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("StaffAssignmentPersonAndRoleFilterControl|f815ad48-7dfd-4c5c-a255-be3f06460e50", "Person");
			this.StaffAssignmentPersonFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 1, true);
			this.StaffAssignmentPersonFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.ModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff));
			this.StaffAssignmentPersonFindBox.Name = "StaffAssignmentPersonFindBox";
			this.StaffAssignmentPersonFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.StaffAssignmentPersonFindBox.TabIndex = 3;
			// 
			// StaffAssignmentRoleDropEdit
			// 
			this.BindingSource.SetBindingMember(this.StaffAssignmentRoleDropEdit, "StaffAssignmentRole");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.GUI.StaffAssignmentPersonAndRoleModuleFilter)(null)).StaffAssignmentRole)));
			this.StaffAssignmentRoleDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("StaffAssignmentPersonAndRoleFilterControl|6c66c16f-8429-45e3-8f2e-119c80a5890a", "Role");
			this.StaffAssignmentRoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 23, true);
			this.StaffAssignmentRoleDropEdit.Name = "StaffAssignmentRoleDropEdit";
			this.StaffAssignmentRoleDropEdit.PreBoundMaxLength = 2;
			this.StaffAssignmentRoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.StaffAssignmentRoleDropEdit.TabIndex = 4;
			// 
			// StaffAssignmentPersonAndRoleFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StaffAssignmentPersonFindBox);
			this.Controls.Add(this.StaffAssignmentRoleDropEdit);
			this.Name = "StaffAssignmentPersonAndRoleFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 43, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCodeFindBox StaffAssignmentPersonFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StaffAssignmentRoleDropEdit;
	}
}
