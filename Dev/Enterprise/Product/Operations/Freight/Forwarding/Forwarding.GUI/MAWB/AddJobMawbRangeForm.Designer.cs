using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class AddJobMawbRangeForm : ZForm
	{
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZRadioButton CarrierMAWBRadioButton;
		private ZRadioButton NeutralMAWBRadioButton;
		private ZLabel zLabel3;
		private ZTextBox EndSuffixTextBox;
		private ZTextBox StartSuffixTextBox;
		private ZDropEdit ServiceLevelDropEdit;
		private ZLabel PortLabel;
		private ZGroupBox MasterBillNumberRangeGroupBox;
		private ZLabel ZA_AirlineCodeLabel1;
		private ZLabel ZA_AirlineCodeLabel;
		private ZLabel AirlineCodeLabel;
		ZLabel HomePortTextLabel;
		private ZGroupBox TypeGroupBox;
		private ZLabel Airline2LetterCodeLabel;
		private ZTextBox AirlinePrefixTextBox;
		ZGuidFindBox BranchFindBox;
		ZGuidFindBox CompanyFindBox;
		ZAddressControl JM_OA_FromAddressControl;
		private ZLabel zLabel5;
		ZCalcEdit MawbCountCalcEdit;

		protected new void InitializeComponent()
		{
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.CompanyFindBox = new ZGuidFindBox();
			this.BranchFindBox = new ZGuidFindBox();
			this.AirlineCodeLabel = new ZLabel();
			this.CarrierMAWBRadioButton = new ZRadioButton();
			this.NeutralMAWBRadioButton = new ZRadioButton();
			this.EndSuffixTextBox = new ZTextBox();
			this.StartSuffixTextBox = new ZTextBox();
			this.zLabel3 = new ZLabel();
			this.ServiceLevelDropEdit = new ZDropEdit();
			this.PortLabel = new ZLabel();
			this.AirlinePrefixTextBox = new ZTextBox();
			this.JM_OA_FromAddressControl = new ZAddressControl();
			this.MasterBillNumberRangeGroupBox = new ZGroupBox();
			this.MawbCountCalcEdit = new ZCalcEdit();
			this.ZA_AirlineCodeLabel1 = new ZLabel();
			this.ZA_AirlineCodeLabel = new ZLabel();
			this.zLabel5 = new ZLabel();
			this.HomePortTextLabel = new ZLabel();
			this.TypeGroupBox = new ZGroupBox();
			this.Airline2LetterCodeLabel = new ZLabel();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.CompanyFindBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.ServiceLevelDropEdit.SuspendLayout();
			this.JM_OA_FromAddressControl.SuspendLayout();
			this.MasterBillNumberRangeGroupBox.SuspendLayout();
			this.TypeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 322, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 26, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 15;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(467);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RangeJobMawb);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 292, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 25, true);
			this.PostingButtonsUserControl.TabIndex = 14;
			//
			// CompanyFindBox
			//
			this.CompanyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyFindBox, "JM_GC_Company");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((RangeJobMawb)(null)).JM_GC_Company)));
			this.CompanyFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|dccea44e-4aec-4ee8-9d9c-737b047eb060", "Company");
			this.CompanyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 8, true);
			this.CompanyFindBox.Name = "CompanyFindBox";
			this.CompanyFindBox.PreBoundMaxLength = 3;
			this.CompanyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 19, true);
			this.CompanyFindBox.TabIndex = 0;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "JM_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((RangeJobMawb)(null)).JM_GB)));
			this.BranchFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|0dd0aaaa-f19a-4f73-9e3e-41c71d3d4fd7", "Branch");
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 33, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.PreBoundMaxLength = 3;
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 19, true);
			this.BranchFindBox.TabIndex = 1;
			// 
			// AirlineCodeLabel
			// 
			this.AirlineCodeLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|c4761ed6-8117-42ba-8dac-dfa3caf4b60b", "Airline Code:");
			this.AirlineCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 101, true);
			this.AirlineCodeLabel.Name = "AirlineCodeLabel";
			this.AirlineCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 23, true);
			this.AirlineCodeLabel.TabIndex = 6;
			this.AirlineCodeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CarrierMAWBRadioButton
			// 
			this.CarrierMAWBRadioButton.AutoCheck = false;
			this.CarrierMAWBRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|93c6f5fe-45e2-4675-b8ea-15ee29bbb5d9", "Carrier MAWB");
			this.CarrierMAWBRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CarrierMAWBRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.CarrierMAWBRadioButton.Name = "CarrierMAWBRadioButton";
			this.CarrierMAWBRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
			this.CarrierMAWBRadioButton.TabIndex = 1;
			// 
			// NeutralMAWBRadioButton
			// 
			this.NeutralMAWBRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.NeutralMAWBRadioButton, "IsNeutralMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((RangeJobMawb)(null)).IsNeutralMAWB)));
			this.NeutralMAWBRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|67584549-2696-4c9a-ab0a-069d7f6d8e05", "Neutral MAWB");
			this.NeutralMAWBRadioButton.Checked = true;
			this.NeutralMAWBRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NeutralMAWBRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.NeutralMAWBRadioButton.Name = "NeutralMAWBRadioButton";
			this.NeutralMAWBRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
			this.NeutralMAWBRadioButton.TabIndex = 0;
			this.NeutralMAWBRadioButton.TabStop = true;
			// 
			// EndSuffixTextBox
			// 
			this.BindingSource.SetBindingMember(this.EndSuffixTextBox, "NumberRangeEnd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RangeJobMawb)(null)).NumberRangeEnd)));
			this.EndSuffixTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|4aeffe24-7248-4d72-89db-7d27ac9f798a", "", "Ending Air Waybill number");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EndSuffixTextBox, false);
			this.EndSuffixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(279, 48, true);
			this.EndSuffixTextBox.Name = "EndSuffixTextBox";
			this.EndSuffixTextBox.ReadOnly = true;
			this.EndSuffixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 19, true);
			this.EndSuffixTextBox.TabIndex = 7;
			this.EndSuffixTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress);
			// 
			// StartSuffixTextBox
			// 
			this.BindingSource.SetBindingMember(this.StartSuffixTextBox, "NumberRangeStart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RangeJobMawb)(null)).NumberRangeStart)));
			this.StartSuffixTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3ECD0B63-3672-4339-BF7E-7E2B0B3F9E24", "", "Starting Air Waybill number");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StartSuffixTextBox, false);
			this.StartSuffixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 48, true);
			this.StartSuffixTextBox.Name = "StartSuffixTextBox";
			this.StartSuffixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 19, true);
			this.StartSuffixTextBox.TabIndex = 4;
			this.StartSuffixTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress);
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|95c4aaba-82c3-48c6-a834-cd90f3941726", "To:");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 52, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 14, true);
			this.zLabel3.TabIndex = 5;
			// 
			// ServiceLevelDropEdit
			// 
			this.ServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelDropEdit, "JM_ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((RangeJobMawb)(null)).JM_ServiceLevel)));
			this.ServiceLevelDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|61f0d6a7-183a-4394-a92c-455e05498167", "Service Level");
			this.ServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 248, true);
			this.ServiceLevelDropEdit.Name = "ServiceLevelDropEdit";
			this.ServiceLevelDropEdit.PreBoundMaxLength = 3;
			this.ServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 19, true);
			this.ServiceLevelDropEdit.TabIndex = 13;
			// 
			// PortLabel
			// 
			this.PortLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|cbbe6bdb-0a2f-4b6f-b21f-5f23deb300fa", "Home Port:");
			this.PortLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 55, true);
			this.PortLabel.Name = "PortLabel";
			this.PortLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 23, true);
			this.PortLabel.TabIndex = 2;
			this.PortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// AirlinePrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirlinePrefixTextBox, "JM_Airline3DigitPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RangeJobMawb)(null)).JM_Airline3DigitPrefix)));
			this.AirlinePrefixTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|fdfa96aa-b0fd-4aad-906a-a23be400931a", "Airline Prefix", "3-Digit Airline Prefix.");
			this.AirlinePrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 80, true);
			this.AirlinePrefixTextBox.Name = "AirlinePrefixTextBox";
			this.AirlinePrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 19, true);
			this.AirlinePrefixTextBox.TabIndex = 5;
			// 
			// JM_OA_From
			// 
			this.JM_OA_FromAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JM_OA_FromAddressControl, "JM_OA_From");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((RangeJobMawb)(null)).JM_OA_From)));
			this.JM_OA_FromAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|40a4cba1-ca4d-48b2-b0f2-aebc0e0543bf", "Borrowed From");
			this.JM_OA_FromAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 213, true);
			this.JM_OA_FromAddressControl.Name = "JM_OA_FromAddressControl";
			this.JM_OA_FromAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 19, true);
			this.JM_OA_FromAddressControl.TabIndex = 11;
			this.JM_OA_FromAddressControl.BindToOrgList = "BorrowedFromList";
			this.JM_OA_FromAddressControl.PopupCaption = "Select Borrower Organization";
			this.JM_OA_FromAddressControl.ShowAddress = false;
			this.JM_OA_FromAddressControl.ShowOrganisationName = true;
			// 
			// MasterBillNumberRangeGroupBox
			// 
			this.MasterBillNumberRangeGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|2a56ce86-192c-4667-931e-88428b4ec425", "Master Bill Number Range");
			this.MasterBillNumberRangeGroupBox.Controls.Add(this.MawbCountCalcEdit);
			this.MasterBillNumberRangeGroupBox.Controls.Add(this.ZA_AirlineCodeLabel1);
			this.MasterBillNumberRangeGroupBox.Controls.Add(this.ZA_AirlineCodeLabel);
			this.MasterBillNumberRangeGroupBox.Controls.Add(this.zLabel3);
			this.MasterBillNumberRangeGroupBox.Controls.Add(this.StartSuffixTextBox);
			this.MasterBillNumberRangeGroupBox.Controls.Add(this.EndSuffixTextBox);
			this.MasterBillNumberRangeGroupBox.Controls.Add(this.zLabel5);
			this.MasterBillNumberRangeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 128, true);
			this.MasterBillNumberRangeGroupBox.Name = "MasterBillNumberRangeGroupBox";
			this.MasterBillNumberRangeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 80, true);
			this.MasterBillNumberRangeGroupBox.TabIndex = 9;
			this.MasterBillNumberRangeGroupBox.TabStop = false;
			// 
			// MawbCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MawbCountCalcEdit, "MawbCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((RangeJobMawb)(null)).MawbCount)));
			this.MawbCountCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|b4712fd7-86aa-4f2e-af60-6cb116deeb98", "Count", "The Number of Master Bill Numbers in this batch");
			this.MawbCountCalcEdit.DecimalPlaces = 0;
			this.MawbCountCalcEdit.Decimals = 0;
			this.MawbCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 24, true);
			this.MawbCountCalcEdit.Name = "MawbCountCalcEdit";
			this.MawbCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 19, true);
			this.MawbCountCalcEdit.TabIndex = 1;
			this.MawbCountCalcEdit.Text = "0";
			this.MawbCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZA_AirlineCodeLabel1
			// 
			this.BindingSource.SetBindingMember(this.ZA_AirlineCodeLabel1, "JM_AirlinePrefixText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RangeJobMawb)(null)).JM_AirlinePrefixText)));
			this.ZA_AirlineCodeLabel1.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ZA_AirlineCodeLabel1, false);
			this.ZA_AirlineCodeLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 50, true);
			this.ZA_AirlineCodeLabel1.Name = "ZA_AirlineCodeLabel1";
			this.ZA_AirlineCodeLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 16, true);
			this.ZA_AirlineCodeLabel1.TabIndex = 6;
			this.ZA_AirlineCodeLabel1.Text = "081";
			// 
			// ZA_AirlineCodeLabel
			// 
			this.BindingSource.SetBindingMember(this.ZA_AirlineCodeLabel, "JM_AirlinePrefixText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RangeJobMawb)(null)).JM_AirlinePrefixText)));
			this.ZA_AirlineCodeLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ZA_AirlineCodeLabel, false);
			this.ZA_AirlineCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 50, true);
			this.ZA_AirlineCodeLabel.Name = "ZA_AirlineCodeLabel";
			this.ZA_AirlineCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 16, true);
			this.ZA_AirlineCodeLabel.TabIndex = 3;
			this.ZA_AirlineCodeLabel.Text = "081";
			// 
			// zLabel5
			// 
			this.zLabel5.AutoSize = true;
			this.zLabel5.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|0fb07b2c-1713-495e-9ee3-25f8f5772719", "From:");
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 52, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 14, true);
			this.zLabel5.TabIndex = 2;
			// 
			// HomePortTextLabel
			// 
			this.HomePortTextLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HomePortTextLabel, "JM_Calc_HomePortText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RangeJobMawb)(null)).JM_Calc_HomePortText)));
			this.HomePortTextLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|edc81e77-61a3-42a5-814a-c039588148e0", "Home Port:");
			this.HomePortTextLabel.IsFontBold = true;
			this.HomePortTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 60, true);
			this.HomePortTextLabel.Name = "HomePortTextLabel";
			this.HomePortTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 14, true);
			this.HomePortTextLabel.TabIndex = 3;
			// 
			// TypeGroupBox
			// 
			this.TypeGroupBox.Controls.Add(this.CarrierMAWBRadioButton);
			this.TypeGroupBox.Controls.Add(this.NeutralMAWBRadioButton);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TypeGroupBox, false);
			this.TypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 67, true);
			this.TypeGroupBox.Name = "TypeGroupBox";
			this.TypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 54, true);
			this.TypeGroupBox.TabIndex = 8;
			this.TypeGroupBox.TabStop = false;
			// 
			// Airline2LetterCodeLabel
			// 
			this.BindingSource.SetBindingMember(this.Airline2LetterCodeLabel, "JM_Calc_Airline2LetterCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RangeJobMawb)(null)).JM_Calc_Airline2LetterCode)));
			this.Airline2LetterCodeLabel.IsFontBold = true;
			this.Airline2LetterCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 105, true);
			this.Airline2LetterCodeLabel.Name = "Airline2LetterCodeLabel";
			this.Airline2LetterCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 16, true);
			this.Airline2LetterCodeLabel.TabIndex = 7;
			this.Airline2LetterCodeLabel.Text = "Airline2LetterCode";
			// 
			// AddJobMawbRangeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AddJobMawbRangeForm|07391ba0-1ab0-4205-8adf-cb00c7dc50e2", "Air Waybill Numbers");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 347, true);
			this.Controls.Add(this.Airline2LetterCodeLabel);
			this.Controls.Add(this.TypeGroupBox);
			this.Controls.Add(this.HomePortTextLabel);
			this.Controls.Add(this.MasterBillNumberRangeGroupBox);
			this.Controls.Add(this.JM_OA_FromAddressControl);
			this.Controls.Add(this.AirlinePrefixTextBox);
			this.Controls.Add(this.PortLabel);
			this.Controls.Add(this.ServiceLevelDropEdit);
			this.Controls.Add(this.AirlineCodeLabel);
			this.Controls.Add(this.BranchFindBox);
			this.Controls.Add(this.CompanyFindBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(RangeJobMawb);
			this.DataSourceTypeName = "RangeJobMawb";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 385, true);
			this.Name = "AddJobMawbRangeForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CompanyFindBox, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.BranchFindBox, 0);
			this.Controls.SetChildIndex(this.AirlineCodeLabel, 0);
			this.Controls.SetChildIndex(this.ServiceLevelDropEdit, 0);
			this.Controls.SetChildIndex(this.PortLabel, 0);
			this.Controls.SetChildIndex(this.AirlinePrefixTextBox, 0);
			this.Controls.SetChildIndex(this.JM_OA_FromAddressControl, 0);
			this.Controls.SetChildIndex(this.MasterBillNumberRangeGroupBox, 0);
			this.Controls.SetChildIndex(this.HomePortTextLabel, 0);
			this.Controls.SetChildIndex(this.TypeGroupBox, 0);
			this.Controls.SetChildIndex(this.Airline2LetterCodeLabel, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.CompanyFindBox.ResumeLayout(true);
			this.CompanyFindBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.ServiceLevelDropEdit.ResumeLayout(true);
			this.ServiceLevelDropEdit.PerformLayout();
			this.JM_OA_FromAddressControl.ResumeLayout(true);
			this.JM_OA_FromAddressControl.PerformLayout();
			this.MasterBillNumberRangeGroupBox.ResumeLayout(false);
			this.MasterBillNumberRangeGroupBox.PerformLayout();
			this.TypeGroupBox.ResumeLayout(false);
			this.TypeGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
