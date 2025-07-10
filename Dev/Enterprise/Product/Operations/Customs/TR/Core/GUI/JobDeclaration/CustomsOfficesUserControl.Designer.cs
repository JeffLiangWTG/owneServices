namespace Enterprise.Customs.TR.GUI
{
	public partial class CustomsOfficesUserControl
	{
		void InitializeComponent()
		{
			this.OfficesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DischargeOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DischargePlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryCustomsFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OfficesGroupBox.SuspendLayout();
			this.CustomsOfficeFindBox.SuspendLayout();
			this.DischargeOfficeFindBox.SuspendLayout();
			this.DischargePlaceTextBox.SuspendLayout();
			this.EntryCustomsFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// OfficesGroupBox
			// 
			this.OfficesGroupBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("3E952042-7211-4F31-9E46-99C726E38114", "Customs Offices");
			this.OfficesGroupBox.Controls.Add(this.DischargePlaceTextBox);
			this.OfficesGroupBox.Controls.Add(this.CustomsOfficeFindBox);
			this.OfficesGroupBox.Controls.Add(this.DischargeOfficeFindBox);
			this.OfficesGroupBox.Controls.Add(this.EntryCustomsFindBox);
			this.OfficesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.OfficesGroupBox.Name = "OfficesGroupBox";
			this.OfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 137, true);
			this.OfficesGroupBox.TabIndex = 0;
			this.OfficesGroupBox.TabStop = false;
			// 
			// CustomsOfficeFindBox
			// 
			this.CustomsOfficeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeFindBox, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).JE_CustomsOffice)));
			this.CustomsOfficeFindBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("e5b6a381-5ab2-4360-932d-c1410abbb90d", "Customs Office");
			this.CustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 19, true);
			this.CustomsOfficeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsOfficeFindBox.Name = "CustomsOfficeFindBox";
			this.CustomsOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.CustomsOfficeFindBox.TabIndex = 0;
			// 
			// DischargeOfficeFindBox
			// 
			this.DischargeOfficeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DischargeOfficeFindBox, "DischargeOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).DischargeOffice)));
			this.DischargeOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 45, true);
			this.DischargeOfficeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.DischargeOfficeFindBox.Name = "DischargeOfficeFindBox";
			this.DischargeOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.DischargeOfficeFindBox.TabIndex = 1;
			// 
			// DischargePlaceTextBox
			// 
			this.DischargePlaceTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DischargePlaceTextBox, "DischargePlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).DischargePlace)));
			this.DischargePlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 71, true);
			this.DischargePlaceTextBox.Name = "DischargePlaceTextBox";
			this.DischargePlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.DischargePlaceTextBox.TabIndex = 2;
			// 
			// EntryCustomsFindBox
			// 
			this.EntryCustomsFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryCustomsFindBox, "EntryOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).EntryOffice)));
			this.EntryCustomsFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 97, true);
			this.EntryCustomsFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.EntryCustomsFindBox.Name = "EntryCustomsFindBox";
			this.EntryCustomsFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.EntryCustomsFindBox.TabIndex = 3;
			// 
			// CustomsOfficesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OfficesGroupBox);
			this.Name = "CustomsOfficesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 127, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OfficesGroupBox.ResumeLayout(false);
			this.OfficesGroupBox.PerformLayout();
			this.CustomsOfficeFindBox.ResumeLayout(true);
			this.CustomsOfficeFindBox.PerformLayout();
			this.DischargeOfficeFindBox.ResumeLayout(true);
			this.DischargeOfficeFindBox.PerformLayout();
			this.DischargePlaceTextBox.ResumeLayout(true);
			this.DischargePlaceTextBox.PerformLayout();
			this.EntryCustomsFindBox.ResumeLayout(true);
			this.EntryCustomsFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		ZArchitecture.GUI.ZCodeFindBox DischargeOfficeFindBox;
		ZArchitecture.GUI.ZCodeFindBox CustomsOfficeFindBox;
		ZArchitecture.GUI.ZGroupBox OfficesGroupBox;
		ZArchitecture.ZTextBox DischargePlaceTextBox;
		ZArchitecture.GUI.ZCodeFindBox EntryCustomsFindBox;
	}
}
