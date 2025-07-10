namespace Enterprise.MasterFiles.GUI
{
  partial class RefDomesticCartageZoneForm
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
		this.F1_CityTownTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.F1_RW_NKStateCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.F1_RL_NKLocoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.F1_ZoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.F1_AirportCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.F1_DistanceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.F1_DistanceUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.F1_AirportPostcodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.F1_CityTownPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.F1_PortCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.F1_IsBeyondCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.MainTabControl.SuspendLayout();
		this.MainTabPage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// MainTabControl
		// 
		this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 201, true);
		// 
		// MainTabPage
		// 
		this.MainTabPage.Controls.Add(this.F1_IsBeyondCheckBox);
		this.MainTabPage.Controls.Add(this.F1_PortCodeTextBox);
		this.MainTabPage.Controls.Add(this.F1_CityTownPostCodeTextBox);
		this.MainTabPage.Controls.Add(this.F1_AirportPostcodeTextBox);
		this.MainTabPage.Controls.Add(this.F1_DistanceUQTextBox);
		this.MainTabPage.Controls.Add(this.F1_DistanceCalcEdit);
		this.MainTabPage.Controls.Add(this.F1_AirportCityTextBox);
		this.MainTabPage.Controls.Add(this.F1_ZoneTextBox);
		this.MainTabPage.Controls.Add(this.F1_RL_NKLocoCodeFindBox);
		this.MainTabPage.Controls.Add(this.F1_RW_NKStateCodeFindBox);
		this.MainTabPage.Controls.Add(this.F1_CityTownTextBox);
		this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 174, true);
		// 
		// MainStatusBar
		// 
		this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 24, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefDomesticCartageZone);
		// 
		// F1_CityTownTextBox
		// 
		this.BindingSource.SetBindingMember(this.F1_CityTownTextBox, "F1_CityTown");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_CityTown)));
		this.F1_CityTownTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 13, true);
		this.F1_CityTownTextBox.Name = "F1_CityTownTextBox";
		this.F1_CityTownTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
		this.F1_CityTownTextBox.TabIndex = 0;
		// 
		// F1_RW_NKStateCodeFindBox
		// 
		this.BindingSource.SetBindingMember(this.F1_RW_NKStateCodeFindBox, "F1_RW_NKState");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_RW_NKState)));
		this.F1_RW_NKStateCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 39, true);
		this.F1_RW_NKStateCodeFindBox.Name = "F1_RW_NKStateCodeFindBox";
		this.F1_RW_NKStateCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
		this.F1_RW_NKStateCodeFindBox.TabIndex = 2;
		// 
		// F1_RL_NKLocoCodeFindBox
		// 
		this.BindingSource.SetBindingMember(this.F1_RL_NKLocoCodeFindBox, "F1_RL_NKLoco");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_RL_NKLoco)));
		this.F1_RL_NKLocoCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 65, true);
		this.F1_RL_NKLocoCodeFindBox.Name = "F1_RL_NKLocoCodeFindBox";
		this.F1_RL_NKLocoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
		this.F1_RL_NKLocoCodeFindBox.TabIndex = 3;
		// 
		// F1_ZoneTextBox
		// 
		this.BindingSource.SetBindingMember(this.F1_ZoneTextBox, "F1_Zone");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_Zone)));
		this.F1_ZoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 91, true);
		this.F1_ZoneTextBox.Name = "F1_ZoneTextBox";
		this.F1_ZoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
		this.F1_ZoneTextBox.TabIndex = 5;
		// 
		// F1_AirportCityTextBox
		// 
		this.BindingSource.SetBindingMember(this.F1_AirportCityTextBox, "F1_AirportCity");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_AirportCity)));
		this.F1_AirportCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 117, true);
		this.F1_AirportCityTextBox.Name = "F1_AirportCityTextBox";
		this.F1_AirportCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
		this.F1_AirportCityTextBox.TabIndex = 7;
		// 
		// F1_DistanceCalcEdit
		// 
		this.BindingSource.SetBindingMember(this.F1_DistanceCalcEdit, "F1_Distance");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_Distance)));
		this.F1_DistanceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 143, true);
		this.F1_DistanceCalcEdit.Name = "F1_DistanceCalcEdit";
		this.F1_DistanceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
		this.F1_DistanceCalcEdit.TabIndex = 9;
		this.F1_DistanceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// F1_DistanceUQTextBox
		// 
		this.F1_DistanceUQTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.F1_DistanceUQTextBox, "F1_DistanceUQ");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_DistanceUQ)));
		this.F1_DistanceUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 143, true);
		this.F1_DistanceUQTextBox.Name = "F1_DistanceUQTextBox";
		this.F1_DistanceUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
		this.F1_DistanceUQTextBox.TabIndex = 10;
		// 
		// F1_AirportPostcodeTextBox
		// 
		this.F1_AirportPostcodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.F1_AirportPostcodeTextBox, "F1_AirportPostcode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_AirportPostcode)));
		this.F1_AirportPostcodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 117, true);
		this.F1_AirportPostcodeTextBox.Name = "F1_AirportPostcodeTextBox";
		this.F1_AirportPostcodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
		this.F1_AirportPostcodeTextBox.TabIndex = 8;
		// 
		// F1_CityTownPostCodeTextBox
		// 
		this.BindingSource.SetBindingMember(this.F1_CityTownPostCodeTextBox, "F1_CityTownPostCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_CityTownPostCode)));
		this.F1_CityTownPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 39, true);
		this.F1_CityTownPostCodeTextBox.Name = "F1_CityTownPostCodeTextBox";
		this.F1_CityTownPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
		this.F1_CityTownPostCodeTextBox.TabIndex = 1;
		// 
		// F1_PortCodeTextBox
		// 
		this.F1_PortCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.F1_PortCodeTextBox, "F1_PortCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_PortCode)));
		this.F1_PortCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 65, true);
		this.F1_PortCodeTextBox.Name = "F1_PortCodeTextBox";
		this.F1_PortCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
		this.F1_PortCodeTextBox.TabIndex = 4;
		// 
		// F1_IsBeyondCheckBox
		// 
		this.F1_IsBeyondCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
		this.F1_IsBeyondCheckBox.AutoSize = true;
		this.BindingSource.SetBindingMember(this.F1_IsBeyondCheckBox, "F1_IsBeyond");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDomesticCartageZone)(null)).F1_IsBeyond)));
		this.F1_IsBeyondCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.F1_IsBeyondCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 93, true);
		this.F1_IsBeyondCheckBox.Name = "F1_IsBeyondCheckBox";
		this.F1_IsBeyondCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 17, true);
		this.F1_IsBeyondCheckBox.TabIndex = 6;
		this.F1_IsBeyondCheckBox.UseVisualStyleBackColor = true;
		// 
		// RefDomesticCartageZoneForm
		// 
		this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 257, true);
		this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDomesticCartageZoneForm|ef5ae63d-0076-40d0-9b45-7ed3d5ad9720", "Port Transport Zone", "Port Transport Zone (ACI)", "US/Canada ACI Port Transport Zone.");
		this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefDomesticCartageZone);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.Name = "RefDomesticCartageZoneForm";
		this.ShouldSerializeTabPageMethods = false;
		this.Text = "";
		this.MainTabControl.ResumeLayout(false);
		this.MainTabPage.ResumeLayout(false);
		this.MainTabPage.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);

    }

    #endregion

    private Enterprise.ZArchitecture.GUI.ZCodeFindBox F1_RW_NKStateCodeFindBox;
    private Enterprise.ZArchitecture.ZTextBox F1_CityTownTextBox;
    private Enterprise.ZArchitecture.ZTextBox F1_DistanceUQTextBox;
    private Enterprise.ZArchitecture.ZCalcEdit F1_DistanceCalcEdit;
    private Enterprise.ZArchitecture.ZTextBox F1_AirportCityTextBox;
    private Enterprise.ZArchitecture.ZTextBox F1_ZoneTextBox;
    private Enterprise.ZArchitecture.GUI.ZCodeFindBox F1_RL_NKLocoCodeFindBox;
    private Enterprise.ZArchitecture.ZTextBox F1_CityTownPostCodeTextBox;
    private Enterprise.ZArchitecture.ZTextBox F1_AirportPostcodeTextBox;
    private Enterprise.ZArchitecture.ZTextBox F1_PortCodeTextBox;
    private Enterprise.ZArchitecture.GUI.ZCheckBox F1_IsBeyondCheckBox;
  }
}
