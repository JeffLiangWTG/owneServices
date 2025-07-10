namespace Enterprise.MasterFiles.Module
{
	public partial class StaffSecurityFilterControl
	{
		Enterprise.MasterFiles.GUI.SecurityFindBox SecurityFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox zGuidFindBox1;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox zGuidFindBox3;

		void InitializeComponent()
		{
			this.SecurityFindBox = new Enterprise.MasterFiles.GUI.SecurityFindBox();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGuidFindBox3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.StaffSecurityModuleFilter);
			// 
			// SecurityFindBox
			// 
			this.BindingSource.SetBindingMember(this.SecurityFindBox, "SecurityFilterContainer.LookupKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Modules.CheckpointLookupKey)(((Enterprise.MasterFiles.Module.StaffSecurityModuleFilter)(null)).SecurityFilterContainer.LookupKey)));
			this.SecurityFindBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("StaffSecurityFilterControl|6f63bd15-d293-4799-a3ab-1f257291f322", "Right");
			this.SecurityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 0, true);
			this.SecurityFindBox.Name = "SecurityFindBox";
			this.SecurityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 20, true);
			this.SecurityFindBox.TabIndex = 0;
			// 
			// zGuidFindBox1
			// 
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "Property1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.StaffSecurityModuleFilter)(null)).Property1)));
			this.zGuidFindBox1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("StaffSecurityFilterControl|b446c5d7-7437-4e4a-bc2c-b2439e2d72c9", "Branch");
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 23, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PreBoundMaxLength = 3;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.zGuidFindBox1.TabIndex = 4;
			// 
			// zGuidFindBox3
			// 
			this.BindingSource.SetBindingMember(this.zGuidFindBox3, "Property2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.StaffSecurityModuleFilter)(null)).Property2)));
			this.zGuidFindBox3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("StaffSecurityFilterControl|7ae03a41-aaa5-4c16-9128-73056260813f", "Dept");
			this.zGuidFindBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 23, true);
			this.zGuidFindBox3.Name = "zGuidFindBox3";
			this.zGuidFindBox3.PreBoundMaxLength = 3;
			this.zGuidFindBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.zGuidFindBox3.TabIndex = 7;
			// 
			// StaffSecurityFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGuidFindBox3);
			this.Controls.Add(this.zGuidFindBox1);
			this.Controls.Add(this.SecurityFindBox);
			this.Name = "StaffSecurityFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 46, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
