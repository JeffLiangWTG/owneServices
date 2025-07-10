namespace Enterprise.Customs.US.GUI
{
	partial class InBondNumberSettingForBranchSpecificForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CompanyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CurrentSystemSettingsGroupBox.SuspendLayout();
			this.NextNumberGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BranchGuidFindBox.SuspendLayout();
			this.CompanyGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// CurrentSystemSettingsGroupBox
			// 
			this.CurrentSystemSettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 116, true);
			this.CurrentSystemSettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 77, true);
			this.CurrentSystemSettingsGroupBox.TabIndex = 3;
			// 
			// NextNumberCalcEdit
			// 
			this.NextNumberCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a718650d-2ce8-491b-9067-352c688013f2", "Next Number");
			this.NextNumberCalcEdit.TabIndex = 0;
			// 
			// CurrentNextNumberCalcEdit
			// 
			this.CurrentNextNumberCalcEdit.TabIndex = 0;
			// 
			// AvailableNumbersCalcEdit
			// 
			this.AvailableNumbersCalcEdit.TabIndex = 1;
			// 
			// SetNextNumberButton
			// 
			this.SetNextNumberButton.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 202, true);
			this.CloseButton.TabIndex = 4;
			// 
			// NextNumberGroupBox
			// 
			this.NextNumberGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 60, true);
			this.NextNumberGroupBox.TabIndex = 2;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 231, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.InBondNumberSetting);
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "BranchPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.InBondNumberSetting)(null)).BranchPK)));
			this.BranchGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("26462e30-d9ba-4c95-9352-0861bf2de46a", "Branch");
			this.BranchGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 34, true);
			this.BranchGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.ShouldResize = true;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.BranchGuidFindBox.TabIndex = 1;
			// 
			// CompanyGuidFindBox
			// 
			this.CompanyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyGuidFindBox, "CompanyPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.InBondNumberSetting)(null)).CompanyPK)));
			this.CompanyGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cfe7e7ba-0716-47ef-af61-a7cdcecd4fe5", "Company");
			this.CompanyGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.CompanyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 6, true);
			this.CompanyGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbCompany;
			this.CompanyGuidFindBox.Name = "CompanyGuidFindBox";
			this.CompanyGuidFindBox.ShouldResize = true;
			this.CompanyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.CompanyGuidFindBox.TabIndex = 0;
			// 
			// InBondNumberSettingForBranchSpecificForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 255, true);
			this.Controls.Add(this.CompanyGuidFindBox);
			this.Controls.Add(this.BranchGuidFindBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.InBondNumberSetting);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.InBondNumberSetting";
			this.Name = "InBondNumberSettingForBranchSpecificForm";
			this.Controls.SetChildIndex(this.NextNumberGroupBox, 0);
			this.Controls.SetChildIndex(this.CurrentSystemSettingsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.Controls.SetChildIndex(this.CompanyGuidFindBox, 0);
			this.CurrentSystemSettingsGroupBox.ResumeLayout(false);
			this.CurrentSystemSettingsGroupBox.PerformLayout();
			this.NextNumberGroupBox.ResumeLayout(false);
			this.NextNumberGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.CompanyGuidFindBox.ResumeLayout(true);
			this.CompanyGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox CompanyGuidFindBox;
	}
}
