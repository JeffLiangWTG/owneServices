namespace Enterprise.MasterFiles.GUI
{
	public partial class RefVesselForm
	{

		#region Windows Form Designer generated code

		private new void InitializeComponent()
		{
			this.rV_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.rV_LloydsNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.rV_VesselTypeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.rV_NetRegisterTonBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.rV_OHBoundFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.carrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.radioCallSignTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.rV_RGBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.rV_MalaysiaVesselIdBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RV_RN_NKCountryOfRegBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomAttribute2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomAttribute3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomAttribute1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomDecimal1CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomFlag1CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ScreeningStatus = new Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			this.ScreenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RadioCallSignOverrideLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.rV_VesselTypeBoundDropDownEdit.SuspendLayout();
			this.rV_OHBoundFindBox.SuspendLayout();
			this.rV_RGBoundGuidFindBox.SuspendLayout();
			this.RV_RN_NKCountryOfRegBoundFindBox.SuspendLayout();
			this.ScreeningStatus.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 409, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefVesselForm|a5157a5a-3ad2-4eef-8825-91b5da6a7f30", "Vessel");
			this.MainTabPage.Controls.Add(this.rV_MalaysiaVesselIdBoundTextBox);
			this.MainTabPage.Controls.Add(this.ScreenButton);
			this.MainTabPage.Controls.Add(this.ScreeningStatus);
			this.MainTabPage.Controls.Add(this.carrierCodeTextBox);
			this.MainTabPage.Controls.Add(this.radioCallSignTextBox);
			this.MainTabPage.Controls.Add(this.rV_RGBoundGuidFindBox);
			this.MainTabPage.Controls.Add(this.rV_LloydsNumberBoundTextBox);
			this.MainTabPage.Controls.Add(this.rV_VesselTypeBoundDropDownEdit);
			this.MainTabPage.Controls.Add(this.CustomFlag1CheckBox);
			this.MainTabPage.Controls.Add(this.CustomDecimal1CalcEdit);
			this.MainTabPage.Controls.Add(this.CustomAttribute1TextBox);
			this.MainTabPage.Controls.Add(this.rV_NetRegisterTonBoundCalcEdit);
			this.MainTabPage.Controls.Add(this.CustomAttribute2TextBox);
			this.MainTabPage.Controls.Add(this.CustomAttribute3TextBox);
			this.MainTabPage.Controls.Add(this.rV_OHBoundFindBox);
			this.MainTabPage.Controls.Add(this.RV_RN_NKCountryOfRegBoundFindBox);
			this.MainTabPage.Controls.Add(this.IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.rV_CodeBoundTextBox);
			this.MainTabPage.Controls.Add(this.RadioCallSignOverrideLabel);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 387, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 387, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 387, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 409, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(820);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefVessel);
			// 
			// rV_CodeBoundTextBox
			// 
			this.rV_CodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.rV_CodeBoundTextBox, "RV_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_Code)));
			this.rV_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 39, true);
			this.rV_CodeBoundTextBox.Name = "rV_CodeBoundTextBox";
			this.rV_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.rV_CodeBoundTextBox.TabIndex = 0;
			// 
			// rV_LloydsNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.rV_LloydsNumberBoundTextBox, "RV_LloydsNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_LloydsNumber)));
			this.rV_LloydsNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 61, true);
			this.rV_LloydsNumberBoundTextBox.Name = "rV_LloydsNumberBoundTextBox";
			this.rV_LloydsNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.rV_LloydsNumberBoundTextBox.TabIndex = 1;
			// 
			// rV_VesselTypeBoundDropDownEdit
			// 
			this.rV_VesselTypeBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rV_VesselTypeBoundDropDownEdit, "RV_VesselType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_VesselType)));
			this.rV_VesselTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 177, true);
			this.rV_VesselTypeBoundDropDownEdit.Name = "rV_VesselTypeBoundDropDownEdit";
			this.rV_VesselTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.rV_VesselTypeBoundDropDownEdit.TabIndex = 6;
			// 
			// rV_NetRegisterTonBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.rV_NetRegisterTonBoundCalcEdit, "RV_NetRegisterTon");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_NetRegisterTon)));
			this.rV_NetRegisterTonBoundCalcEdit.DecimalPlaces = 0;
			this.rV_NetRegisterTonBoundCalcEdit.Decimals = 0;
			this.rV_NetRegisterTonBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 153, true);
			this.rV_NetRegisterTonBoundCalcEdit.Name = "rV_NetRegisterTonBoundCalcEdit";
			this.rV_NetRegisterTonBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.rV_NetRegisterTonBoundCalcEdit.TabIndex = 5;
			this.rV_NetRegisterTonBoundCalcEdit.Text = "0";
			this.rV_NetRegisterTonBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// rV_OHBoundFindBox
			// 
			this.rV_OHBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rV_OHBoundFindBox, "RV_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_OH)));
			this.rV_OHBoundFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.rV_OHBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 107, true);
			this.rV_OHBoundFindBox.Name = "rV_OHBoundFindBox";
			this.rV_OHBoundFindBox.PopupCaption = "Select the Shipping Provider";
			this.rV_OHBoundFindBox.ShouldResize = true;
			this.rV_OHBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.rV_OHBoundFindBox.TabIndex = 3;
			// 
			// carrierCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.carrierCodeTextBox, "RV_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_CarrierCode)));
			this.carrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 245, true);
			this.carrierCodeTextBox.Name = "carrierCodeTextBox";
			this.carrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.carrierCodeTextBox.TabIndex = 9;
			// 
			// radioCallSignTextBox
			// 
			this.BindingSource.SetBindingMember(this.radioCallSignTextBox, "RV_RadioCallSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_RadioCallSign)));
			this.radioCallSignTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 223, true);
			this.radioCallSignTextBox.Name = "radioCallSignTextBox";
			this.radioCallSignTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.radioCallSignTextBox.TabIndex = 8;
			// 
			// rV_RGBoundGuidFindBox
			// 
			this.rV_RGBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rV_RGBoundGuidFindBox, "RV_RG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_RG)));
			this.rV_RGBoundGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.rV_RGBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 131, true);
			this.rV_RGBoundGuidFindBox.Name = "rV_RGBoundGuidFindBox";
			this.rV_RGBoundGuidFindBox.ShouldResize = true;
			this.rV_RGBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.rV_RGBoundGuidFindBox.TabIndex = 4;
			// 
			// rV_MalaysiaVesselIdBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.rV_MalaysiaVesselIdBoundTextBox, "RV_MalaysiaVesselId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_MalaysiaVesselId)));
			this.rV_MalaysiaVesselIdBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ed0c6339-8105-45f4-9423-1fa24c01398d", "Vessel Number", "Agent Vessel Number", "Shipping Agent Vessel Number", "Vessel Number assigned by the Shipping Agent: for Malaysian Customs - K4/K5 Manifest Message");
			this.rV_MalaysiaVesselIdBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 85, true);
			this.rV_MalaysiaVesselIdBoundTextBox.Name = "rV_MalaysiaVesselIdBoundTextBox";
			this.rV_MalaysiaVesselIdBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.rV_MalaysiaVesselIdBoundTextBox.TabIndex = 2;
			// 
			// RV_RN_NKCountryOfRegBoundFindBox
			// 
			this.RV_RN_NKCountryOfRegBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RV_RN_NKCountryOfRegBoundFindBox, "RV_RN_NKCountryOfReg");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_RN_NKCountryOfReg)));
			this.RV_RN_NKCountryOfRegBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 199, true);
			this.RV_RN_NKCountryOfRegBoundFindBox.Name = "RV_RN_NKCountryOfRegBoundFindBox";
			this.RV_RN_NKCountryOfRegBoundFindBox.PopupCaption = "Select the Shipping Provider";
			this.RV_RN_NKCountryOfRegBoundFindBox.PreBoundMaxLength = 2;
			this.RV_RN_NKCountryOfRegBoundFindBox.ShouldResize = true;
			this.RV_RN_NKCountryOfRegBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.RV_RN_NKCountryOfRegBoundFindBox.TabIndex = 7;
			// 
			// CustomAttribute2TextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomAttribute2TextBox, "RV_CustomAttrib2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_CustomAttrib2)));
			this.CustomAttribute2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 309, true);
			this.CustomAttribute2TextBox.Name = "CustomAttribute2TextBox";
			this.CustomAttribute2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.CustomAttribute2TextBox.TabIndex = 12;
			this.CustomAttribute2TextBox.Visible = false;
			// 
			// CustomAttribute3TextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomAttribute3TextBox, "RV_CustomAttrib3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_CustomAttrib3)));
			this.CustomAttribute3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 332, true);
			this.CustomAttribute3TextBox.Name = "CustomAttribute3TextBox";
			this.CustomAttribute3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.CustomAttribute3TextBox.TabIndex = 13;
			this.CustomAttribute3TextBox.Visible = false;
			// 
			// CustomAttribute1TextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomAttribute1TextBox, "RV_CustomAttrib1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_CustomAttrib1)));
			this.CustomAttribute1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 286, true);
			this.CustomAttribute1TextBox.Name = "CustomAttribute1TextBox";
			this.CustomAttribute1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.CustomAttribute1TextBox.TabIndex = 11;
			this.CustomAttribute1TextBox.Visible = false;
			// 
			// CustomDecimal1CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CustomDecimal1CalcEdit, "RV_CustomDecimal1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_CustomDecimal1)));
			this.CustomDecimal1CalcEdit.DecimalPlaces = 2;
			this.CustomDecimal1CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 355, true);
			this.CustomDecimal1CalcEdit.Name = "CustomDecimal1CalcEdit";
			this.CustomDecimal1CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.CustomDecimal1CalcEdit.TabIndex = 14;
			this.CustomDecimal1CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CustomDecimal1CalcEdit.Visible = false;
			// 
			// CustomFlag1CheckBox
			// 
			this.CustomFlag1CheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CustomFlag1CheckBox, "RV_CustomFlag1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_CustomFlag1)));
			this.CustomFlag1CheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CustomFlag1CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CustomFlag1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 378, true);
			this.CustomFlag1CheckBox.Name = "CustomFlag1CheckBox";
			this.CustomFlag1CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CustomFlag1CheckBox.TabIndex = 15;
			this.CustomFlag1CheckBox.Visible = false;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RV_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_IsActive)));
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 269, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsActiveCheckBox.TabIndex = 10;
			this.IsActiveCheckBox.CheckedChanged += new System.EventHandler(this.IsActiveCheckBox_CheckedChanged);
			// 
			// ScreeningStatus
			// 
			this.ScreeningStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScreeningStatus, "RV_ScreeningStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefVessel)(null)).RV_ScreeningStatus)));
			this.ScreeningStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 16, true);
			this.ScreeningStatus.Name = "ScreeningStatus";
			this.ScreeningStatus.PreBoundMaxLength = 3;
			this.ScreeningStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 17, true);
			this.ScreeningStatus.TabIndex = 16;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Font = new System.Drawing.Font(Enterprise.ZArchitecture.Core.OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 16, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 17, true);
			this.ScreenButton.TabIndex = 16;
			this.ScreenButton.Text = "...";
			this.ScreenButton.ToolTipCaption = null;
			this.ScreenButton.UseVisualStyleBackColor = true;
			this.ScreenButton.Click += new System.EventHandler(this.ScreenButton_Click);
			// 
			// RadioCallSignOverrideLabel
			// 
			this.RadioCallSignOverrideLabel.AutoSize = true;
			this.RadioCallSignOverrideLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RadioCallSignOverrideLabel.ForeColor = System.Drawing.Color.DarkGray;
			this.RadioCallSignOverrideLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 202, true);
			this.RadioCallSignOverrideLabel.Name = "RadioCallSignOverrideLabel";
			this.RadioCallSignOverrideLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.RadioCallSignOverrideLabel.TabIndex = 17;
			this.RadioCallSignOverrideLabel.Visible = false;
			// 
			// RefVesselForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefVesselForm|68b2d9d8-1455-42dd-b6a9-75759915f0da", "Vessel");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 465, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefVessel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 503, true);
			this.Name = "RefVesselForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.rV_VesselTypeBoundDropDownEdit.ResumeLayout(true);
			this.rV_VesselTypeBoundDropDownEdit.PerformLayout();
			this.rV_OHBoundFindBox.ResumeLayout(true);
			this.rV_OHBoundFindBox.PerformLayout();
			this.rV_RGBoundGuidFindBox.ResumeLayout(true);
			this.rV_RGBoundGuidFindBox.PerformLayout();
			this.RV_RN_NKCountryOfRegBoundFindBox.ResumeLayout(true);
			this.RV_RN_NKCountryOfRegBoundFindBox.PerformLayout();
			this.ScreeningStatus.ResumeLayout(true);
			this.ScreeningStatus.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private Enterprise.ZArchitecture.ZTextBox rV_MalaysiaVesselIdBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox carrierCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox radioCallSignTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox rV_RGBoundGuidFindBox;
		private Enterprise.ZArchitecture.ZTextBox rV_LloydsNumberBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit rV_VesselTypeBoundDropDownEdit;
		private Enterprise.ZArchitecture.ZCalcEdit rV_NetRegisterTonBoundCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox rV_OHBoundFindBox;
		private Enterprise.ZArchitecture.ZTextBox rV_CodeBoundTextBox;
		protected Enterprise.ZArchitecture.ZTextBox CustomAttribute2TextBox;
		protected Enterprise.ZArchitecture.ZTextBox CustomAttribute3TextBox;
		protected Enterprise.ZArchitecture.ZTextBox CustomAttribute1TextBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox CustomFlag1CheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox RV_RN_NKCountryOfRegBoundFindBox;
		protected Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit ScreeningStatus;
		private Enterprise.ZArchitecture.GUI.ZButton ScreenButton;
		protected Enterprise.ZArchitecture.ZCalcEdit CustomDecimal1CalcEdit;
		protected Enterprise.ZArchitecture.ZLabel RadioCallSignOverrideLabel;
	}
}
