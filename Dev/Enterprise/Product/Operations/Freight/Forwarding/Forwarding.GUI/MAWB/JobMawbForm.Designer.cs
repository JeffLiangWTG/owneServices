using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class JobMawbForm : ZTemplateForm
	{
		private ZCheckBox IsPrintedBoundCheckBox;
		private ZAddressControl JM_OA_FromAddressControl;
		private ZDropEdit ServiceLevelDropEdit;
		private ZGroupBox BorrowedOutGroupBox;
		private ZCheckBox IsBorrowedzCheckBox;
		private ZCheckBox IsCompletedzCheckBox;
		private ZDateEdit JM_ReservedUntilBoundDateEdit;
		private ZGuidFindBox JM_OH_AllocatedToBoundFindBox;
		private ZGroupBox InternalAllocationGroupBox;
		private ZGroupBox MasterBillNumberDetailsGroupBox;
		private ZArchitecture.ZLabel Airline2LetterCodeLabel;
		private ZArchitecture.ZLabel PortOfLoadingTextLabel;
		private ZArchitecture.ZLabel zLabel6;
		private ZArchitecture.ZLabel AirlineCodeLabel;
		private ZArchitecture.ZLabel zLabel1;
		private ZGuidFindBox JM_CompanyFindBox;
		private ZGuidFindBox JM_BranchFindBox;
		private ZArchitecture.ZLabel ParentJobNumberLabel;
		private ZArchitecture.ZLabel Label22;
		private ZArchitecture.ZLabel AirlinePrefixLabel;
		private ZArchitecture.ZLabel MasterBillNumberLabel;
		private ZArchitecture.ZLabel zLabel7;
		private ZArchitecture.ZLabel zLabel8;
		private ZGroupBox BorrowedInDetailsGroupBox;
		private ZCheckBox JM_IsReportedToLenderCheckBox;
		private CargoWise.Windows.UI.KPanel PanelToStopNestedGroupBoxExceptionPanel;
		private ZGroupBox AWBTypeGroupBox;
		private ZRadioButton zRadioButton2;
		private ZRadioButton NeutralMAWBRadioButton;
		private ZArchitecture.ZLabel ReferenceTypeLabel;

		new void InitializeComponent()
		{
			this.IsPrintedBoundCheckBox = new ZCheckBox();
			this.JM_OA_FromAddressControl = new ZAddressControl();
			this.ServiceLevelDropEdit = new ZDropEdit();
			this.BorrowedInDetailsGroupBox = new ZGroupBox();
			this.JM_IsReportedToLenderCheckBox = new ZCheckBox();
			this.MasterBillNumberDetailsGroupBox = new ZGroupBox();
			this.PanelToStopNestedGroupBoxExceptionPanel = new CargoWise.Windows.UI.KPanel();
			this.AWBTypeGroupBox = new ZGroupBox();
			this.zRadioButton2 = new ZRadioButton();
			this.NeutralMAWBRadioButton = new ZRadioButton();
			this.zLabel8 = new ZArchitecture.ZLabel();
			this.zLabel7 = new ZArchitecture.ZLabel();
			this.MasterBillNumberLabel = new ZArchitecture.ZLabel();
			this.AirlinePrefixLabel = new ZArchitecture.ZLabel();
			this.zLabel1 = new ZArchitecture.ZLabel();
			this.zLabel6 = new ZArchitecture.ZLabel();
			this.PortOfLoadingTextLabel = new ZArchitecture.ZLabel();
			this.AirlineCodeLabel = new ZArchitecture.ZLabel();
			this.Airline2LetterCodeLabel = new ZArchitecture.ZLabel();
			this.Label22 = new ZArchitecture.ZLabel();
			this.JM_CompanyFindBox = new ZGuidFindBox();
			this.JM_BranchFindBox = new ZGuidFindBox();
			this.InternalAllocationGroupBox = new ZGroupBox();
			this.ParentJobNumberLabel = new ZArchitecture.ZLabel();
			this.ReferenceTypeLabel = new ZArchitecture.ZLabel();
			this.BorrowedOutGroupBox = new ZGroupBox();
			this.JM_ReservedUntilBoundDateEdit = new ZDateEdit();
			this.IsCompletedzCheckBox = new ZCheckBox();
			this.JM_OH_AllocatedToBoundFindBox = new ZGuidFindBox();
			this.IsBorrowedzCheckBox = new ZCheckBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JM_OA_FromAddressControl.SuspendLayout();
			this.ServiceLevelDropEdit.SuspendLayout();
			this.BorrowedInDetailsGroupBox.SuspendLayout();
			this.MasterBillNumberDetailsGroupBox.SuspendLayout();
			this.PanelToStopNestedGroupBoxExceptionPanel.SuspendLayout();
			this.AWBTypeGroupBox.SuspendLayout();
			this.JM_CompanyFindBox.SuspendLayout();
			this.JM_BranchFindBox.SuspendLayout();
			this.InternalAllocationGroupBox.SuspendLayout();
			this.BorrowedOutGroupBox.SuspendLayout();
			this.JM_ReservedUntilBoundDateEdit.SuspendLayout();
			this.JM_OH_AllocatedToBoundFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 477, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|a5157a5a-3ad2-4eef-8825-91b5da6a7f30", "MAWB");
			this.MainTabPage.Controls.Add(this.BorrowedInDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.MasterBillNumberDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.InternalAllocationGroupBox);
			this.MainTabPage.Controls.Add(this.BorrowedOutGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(685, 454, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(685, 454, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 477, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(781);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(782);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobMawb);
			// 
			// IsPrintedBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsPrintedBoundCheckBox, "MAWB_IsPrinted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobMawb)(null)).MAWB_IsPrinted)));
			this.IsPrintedBoundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsPrintedBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPrintedBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 13, true);
			this.IsPrintedBoundCheckBox.Name = "IsPrintedBoundCheckBox";
			this.IsPrintedBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 24, true);
			this.IsPrintedBoundCheckBox.TabIndex = 2;
			this.IsPrintedBoundCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// JM_OA_FromBoundGuidFindBox
			//
			this.JM_OA_FromAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JM_OA_FromAddressControl, "JM_OA_From");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobMawb)(null)).JM_OA_From)));
			this.JM_OA_FromAddressControl.BindToOrgList = "BorrowedFromList";
			this.JM_OA_FromAddressControl.PopupCaption = "Select Borrower Organization";
			this.JM_OA_FromAddressControl.ShowAddress = false;
			this.JM_OA_FromAddressControl.ShowOrganisationName = true;
			this.JM_OA_FromAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 22, true);
			this.JM_OA_FromAddressControl.Name = "JM_OA_FromAddressControl";
			this.JM_OA_FromAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 19, true);
			this.JM_OA_FromAddressControl.TabIndex = 14;
			// 
			// ServiceLevelDropEdit
			// 
			this.ServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelDropEdit, "JM_ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobMawb)(null)).JM_ServiceLevel)));
			this.ServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 157, true);
			this.ServiceLevelDropEdit.Name = "ServiceLevelDropEdit";
			this.ServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 19, true);
			this.ServiceLevelDropEdit.TabIndex = 16;
			// 
			// BorrowedInDetailsGroupBox
			// 
			this.BorrowedInDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|036c359c-4fc5-4144-89b7-a261378f7354", "Borrowed \'In\' Details");
			this.BorrowedInDetailsGroupBox.Controls.Add(this.JM_IsReportedToLenderCheckBox);
			this.BorrowedInDetailsGroupBox.Controls.Add(this.JM_OA_FromAddressControl);
			this.BorrowedInDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 249, true);
			this.BorrowedInDetailsGroupBox.Name = "BorrowedInDetailsGroupBox";
			this.BorrowedInDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 82, true);
			this.BorrowedInDetailsGroupBox.TabIndex = 4;
			this.BorrowedInDetailsGroupBox.TabStop = false;
			// 
			// JM_IsReportedToLenderCheckBox
			// 
			this.BindingSource.SetBindingMember(this.JM_IsReportedToLenderCheckBox, "JM_IsReportedToLender");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobMawb)(null)).JM_IsReportedToLender)));
			this.JM_IsReportedToLenderCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JM_IsReportedToLenderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JM_IsReportedToLenderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 46, true);
			this.JM_IsReportedToLenderCheckBox.Name = "JM_IsReportedToLenderCheckBox";
			this.JM_IsReportedToLenderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 24, true);
			this.JM_IsReportedToLenderCheckBox.TabIndex = 15;
			this.JM_IsReportedToLenderCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// MasterBillNumberDetailsGroupBox
			// 
			this.MasterBillNumberDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|bd96b87f-e4e1-4067-85c2-1f97944b2d5d", "Master Bill Number Details");
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.PanelToStopNestedGroupBoxExceptionPanel);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.zLabel8);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.zLabel7);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.MasterBillNumberLabel);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.AirlinePrefixLabel);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.zLabel1);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.ServiceLevelDropEdit);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.zLabel6);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.PortOfLoadingTextLabel);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.AirlineCodeLabel);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.Airline2LetterCodeLabel);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.Label22);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.JM_CompanyFindBox);
			this.MasterBillNumberDetailsGroupBox.Controls.Add(this.JM_BranchFindBox);
			this.MasterBillNumberDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.MasterBillNumberDetailsGroupBox.Name = "MasterBillNumberDetailsGroupBox";
			this.MasterBillNumberDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 192, true);
			this.MasterBillNumberDetailsGroupBox.TabIndex = 0;
			this.MasterBillNumberDetailsGroupBox.TabStop = false;
			// 
			// PanelToStopNestedGroupBoxExceptionPanel
			// 
			this.PanelToStopNestedGroupBoxExceptionPanel.Controls.Add(this.AWBTypeGroupBox);
			this.PanelToStopNestedGroupBoxExceptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 78, true);
			this.PanelToStopNestedGroupBoxExceptionPanel.Name = "PanelToStopNestedGroupBoxExceptionPanel";
			this.PanelToStopNestedGroupBoxExceptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 64, true);
			this.PanelToStopNestedGroupBoxExceptionPanel.TabIndex = 17;
			// 
			// AWBTypeGroupBox
			// 
			this.AWBTypeGroupBox.Controls.Add(this.zRadioButton2);
			this.AWBTypeGroupBox.Controls.Add(this.NeutralMAWBRadioButton);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AWBTypeGroupBox, false);
			this.AWBTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AWBTypeGroupBox.Name = "AWBTypeGroupBox";
			this.AWBTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 54, true);
			this.AWBTypeGroupBox.TabIndex = 9;
			this.AWBTypeGroupBox.TabStop = false;
			// 
			// zRadioButton2
			// 
			this.zRadioButton2.AutoCheck = false;
			this.zRadioButton2.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zRadioButton2, "JM_IsPaper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobMawb)(null)).JM_IsPaper)));
			this.zRadioButton2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|36a782e9-a33c-4e2e-977f-fd858ea4c266", "Carrier MAWB");
			this.zRadioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 35, true);
			this.zRadioButton2.Name = "zRadioButton2";
			this.zRadioButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.zRadioButton2.TabIndex = 1;
			// 
			// NeutralMAWBRadioButton
			// 
			this.NeutralMAWBRadioButton.AutoCheck = false;
			this.NeutralMAWBRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NeutralMAWBRadioButton, "JM_Calc_IsNeutral");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobMawb)(null)).JM_Calc_IsNeutral)));
			this.NeutralMAWBRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|58461ba0-18b7-41be-9b6b-f8129a520481", "Neutral MAWB");
			this.NeutralMAWBRadioButton.Checked = true;
			this.NeutralMAWBRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NeutralMAWBRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 13, true);
			this.NeutralMAWBRadioButton.Name = "NeutralMAWBRadioButton";
			this.NeutralMAWBRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 17, true);
			this.NeutralMAWBRadioButton.TabIndex = 0;
			this.NeutralMAWBRadioButton.TabStop = true;
			// 
			// zLabel8
			// 
			this.zLabel8.AutoSize = true;
			this.zLabel8.IsFontBold = true;
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 134, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(11, 14, true);
			this.zLabel8.TabIndex = 11;
			this.zLabel8.Text = "-";
			// 
			// Airline prefix value label
			// 
			this.zLabel7.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel7, "JM_Airline3DigitPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobMawb)(null)).JM_Airline3DigitPrefix)));
			this.zLabel7.IsFontBold = true;
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 134, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 14, true);
			this.zLabel7.TabIndex = 10;
			this.zLabel7.Text = "081";
			// 
			// MasterBillNumberLabel
			// 
			this.MasterBillNumberLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MasterBillNumberLabel, "JM_MAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobMawb)(null)).JM_MAWB)));
			this.MasterBillNumberLabel.IsFontBold = true;
			this.MasterBillNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 135, true);
			this.MasterBillNumberLabel.Name = "MasterBillNumberLabel";
			this.MasterBillNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 14, true);
			this.MasterBillNumberLabel.TabIndex = 12;
			this.MasterBillNumberLabel.Text = "MAWB";
			// 
			// AirlinePrefixLabel
			// 
			this.AirlinePrefixLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AirlinePrefixLabel, "JM_Airline3DigitPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobMawb)(null)).JM_Airline3DigitPrefix)));
			this.AirlinePrefixLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|d3cbe035-e9b2-4f93-b7db-4a0afb455917", "Airline Prefix");
			this.AirlinePrefixLabel.IsFontBold = true;
			this.AirlinePrefixLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 90, true);
			this.AirlinePrefixLabel.Name = "AirlinePrefixLabel";
			this.AirlinePrefixLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 14, true);
			this.AirlinePrefixLabel.TabIndex = 5;
			this.AirlinePrefixLabel.Text = "AirlinePrefix";
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|0698e3fd-514e-4409-9f7b-6b9f0ebeb411", "Master Bill Number:");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 135, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 14, true);
			this.zLabel1.TabIndex = 9;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel6
			// 
			this.zLabel6.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|09e6aa07-ad50-4f30-bc04-962497702919", "Home Port:");
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 70, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 14, true);
			this.zLabel6.TabIndex = 2;
			this.zLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PortOfLoadingTextLabel
			// 
			this.PortOfLoadingTextLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingTextLabel, "JM_Calc_HomePortText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobMawb)(null)).JM_Calc_HomePortText)));
			this.PortOfLoadingTextLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|773822f2-b82d-42b1-8136-7f34b370aad9", "Port of Loading Text");
			this.PortOfLoadingTextLabel.IsFontBold = true;
			this.PortOfLoadingTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 70, true);
			this.PortOfLoadingTextLabel.Name = "PortOfLoadingTextLabel";
			this.PortOfLoadingTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.PortOfLoadingTextLabel.TabIndex = 3;
			this.PortOfLoadingTextLabel.Text = "Port of Loading Text";
			// 
			// AirlineCodeLabel
			// 
			this.AirlineCodeLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|c112cc82-5ba7-407f-b210-8f675591e464", "Airline Code:");
			this.AirlineCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 112, true);
			this.AirlineCodeLabel.Name = "AirlineCodeLabel";
			this.AirlineCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 14, true);
			this.AirlineCodeLabel.TabIndex = 6;
			this.AirlineCodeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// Airline2LetterCodeLabel
			// 
			this.Airline2LetterCodeLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.Airline2LetterCodeLabel, "JM_Calc_Airline2LetterCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobMawb)(null)).JM_Calc_Airline2LetterCode)));
			this.Airline2LetterCodeLabel.IsFontBold = true;
			this.Airline2LetterCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 112, true);
			this.Airline2LetterCodeLabel.Name = "Airline2LetterCodeLabel";
			this.Airline2LetterCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 14, true);
			this.Airline2LetterCodeLabel.TabIndex = 7;
			this.Airline2LetterCodeLabel.Text = "Airline2LetterCode";
			// 
			// Airline Prefix Label
			// 
			this.Label22.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|b8eb439c-c398-4e16-aefd-9b976b493943", "Airline Prefix:");
			this.Label22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 90, true);
			this.Label22.Name = "Label22";
			this.Label22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 14, true);
			this.Label22.TabIndex = 4;
			this.Label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// JM_CompanyFindBox
			// 
			this.JM_CompanyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JM_CompanyFindBox, "JM_GC_Company");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobMawb)(null)).JM_GC_Company)));
			this.JM_CompanyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 25, true);
			this.JM_CompanyFindBox.Name = "JM_CompanyFindBox";
			this.JM_CompanyFindBox.PreBoundMaxLength = 3;
			this.JM_CompanyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 19, true);
			this.JM_CompanyFindBox.TabIndex = 0;
			this.JM_CompanyFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|dcceafff-4aec-4eef-9d9c-737b047eb060", "Company");
			// 
			// JM_BranchFindBox
			// 
			this.JM_BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JM_BranchFindBox, "JM_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobMawb)(null)).JM_GB)));
			this.JM_BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 45, true);
			this.JM_BranchFindBox.Name = "JM_BranchFindBox";
			this.JM_BranchFindBox.PreBoundMaxLength = 3;
			this.JM_BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 19, true);
			this.JM_BranchFindBox.TabIndex = 1;
			// 
			// InternalAllocationGroupBox
			// 
			this.InternalAllocationGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|23b7b8a7-8a62-46b6-a4ce-ad738d0b2f9b", "Usage Details");
			this.InternalAllocationGroupBox.Controls.Add(this.ParentJobNumberLabel);
			this.InternalAllocationGroupBox.Controls.Add(this.IsPrintedBoundCheckBox);
			this.InternalAllocationGroupBox.Controls.Add(this.ReferenceTypeLabel);
			this.InternalAllocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 202, true);
			this.InternalAllocationGroupBox.Name = "InternalAllocationGroupBox";
			this.InternalAllocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 43, true);
			this.InternalAllocationGroupBox.TabIndex = 1;
			this.InternalAllocationGroupBox.TabStop = false;
			// 
			// ParentJobNumberLabel
			// 
			this.ParentJobNumberLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ParentJobNumberLabel, "JM_Calc_ParentJobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobMawb)(null)).JM_Calc_ParentJobNumber)));
			this.ParentJobNumberLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ParentJobNumberLabel, false);
			this.ParentJobNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 18, true);
			this.ParentJobNumberLabel.Name = "ParentJobNumberLabel";
			this.ParentJobNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 14, true);
			this.ParentJobNumberLabel.TabIndex = 1;
			this.ParentJobNumberLabel.Text = "Parent Job Number";
			// 
			// ReferenceTypeLabel
			// 
			this.BindingSource.SetBindingMember(this.ReferenceTypeLabel, "JM_Calc_UsageIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobMawb)(null)).JM_Calc_UsageIndicator)));
			this.ReferenceTypeLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|b21e1cd8-6bff-4f65-b1b9-b836a1107602", "Used by:");
			this.ReferenceTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 18, true);
			this.ReferenceTypeLabel.Name = "ReferenceTypeLabel";
			this.ReferenceTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 14, true);
			this.ReferenceTypeLabel.TabIndex = 0;
			this.ReferenceTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// BorrowedOutGroupBox
			// 
			this.BorrowedOutGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|8451e00b-4f8c-4b0b-9ec8-03a71fa37607", "Borrowed \'Out\' Details");
			this.BorrowedOutGroupBox.Controls.Add(this.JM_ReservedUntilBoundDateEdit);
			this.BorrowedOutGroupBox.Controls.Add(this.IsCompletedzCheckBox);
			this.BorrowedOutGroupBox.Controls.Add(this.JM_OH_AllocatedToBoundFindBox);
			this.BorrowedOutGroupBox.Controls.Add(this.IsBorrowedzCheckBox);
			this.BorrowedOutGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 333, true);
			this.BorrowedOutGroupBox.Name = "BorrowedOutGroupBox";
			this.BorrowedOutGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 110, true);
			this.BorrowedOutGroupBox.TabIndex = 2;
			this.BorrowedOutGroupBox.TabStop = false;
			// 
			// JM_ReservedUntilBoundDateEdit
			// 
			this.JM_ReservedUntilBoundDateEdit.AllowDrop = true;
			this.JM_ReservedUntilBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JM_ReservedUntilBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JM_ReservedUntilBoundDateEdit, "JM_ReservedUntil");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobMawb)(null)).JM_ReservedUntil)));
			this.JM_ReservedUntilBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 56, true);
			this.JM_ReservedUntilBoundDateEdit.Name = "JM_ReservedUntilBoundDateEdit";
			this.JM_ReservedUntilBoundDateEdit.TabIndex = 3;
			// 
			// IsCompletedzCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsCompletedzCheckBox, "JM_IsCompletedAWBReturned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobMawb)(null)).JM_IsCompletedAWBReturned)));
			this.IsCompletedzCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|af357824-39f1-48e1-8924-c0ad2e7bffd3", "Completed AWB Returned");
			this.IsCompletedzCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsCompletedzCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsCompletedzCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsCompletedzCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 81, true);
			this.IsCompletedzCheckBox.Name = "IsCompletedzCheckBox";
			this.IsCompletedzCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 24, true);
			this.IsCompletedzCheckBox.TabIndex = 4;
			this.IsCompletedzCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// JM_OH_AllocatedToBoundFindBox
			// 
			this.JM_OH_AllocatedToBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JM_OH_AllocatedToBoundFindBox, "JM_OH_AllocatedTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobMawb)(null)).JM_OH_AllocatedTo)));
			this.JM_OH_AllocatedToBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 24, true);
			this.JM_OH_AllocatedToBoundFindBox.Name = "JM_OH_AllocatedToBoundFindBox";
			this.JM_OH_AllocatedToBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 19, true);
			this.JM_OH_AllocatedToBoundFindBox.TabIndex = 1;
			// 
			// IsBorrowedzCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsBorrowedzCheckBox, "JM_IsBorrowedAWBInvoiced");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobMawb)(null)).JM_IsBorrowedAWBInvoiced)));
			this.IsBorrowedzCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|2af25b1b-2e9f-489f-b920-da38c5828ab2", "Borrowed AWB Invoiced");
			this.IsBorrowedzCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsBorrowedzCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsBorrowedzCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 81, true);
			this.IsBorrowedzCheckBox.Name = "IsBorrowedzCheckBox";
			this.IsBorrowedzCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 24, true);
			this.IsBorrowedzCheckBox.TabIndex = 5;
			this.IsBorrowedzCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// JobMawbForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobMawbForm|f53993cd-1a9f-44d4-a7d8-ac30dac9b7f2", "Air Waybill");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 533, true);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(JobMawb);
			this.DataSourceTypeName = "Enterprise.Freight.Business.JobMawb";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "JobMawbForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.JM_OA_FromAddressControl.ResumeLayout(true);
			this.JM_OA_FromAddressControl.PerformLayout();
			this.ServiceLevelDropEdit.ResumeLayout(true);
			this.ServiceLevelDropEdit.PerformLayout();
			this.BorrowedInDetailsGroupBox.ResumeLayout(false);
			this.BorrowedInDetailsGroupBox.PerformLayout();
			this.MasterBillNumberDetailsGroupBox.ResumeLayout(false);
			this.MasterBillNumberDetailsGroupBox.PerformLayout();
			this.PanelToStopNestedGroupBoxExceptionPanel.ResumeLayout(false);
			this.PanelToStopNestedGroupBoxExceptionPanel.PerformLayout();
			this.AWBTypeGroupBox.ResumeLayout(false);
			this.AWBTypeGroupBox.PerformLayout();
			this.JM_CompanyFindBox.ResumeLayout(true);
			this.JM_CompanyFindBox.PerformLayout();
			this.JM_BranchFindBox.ResumeLayout(true);
			this.JM_BranchFindBox.PerformLayout();
			this.InternalAllocationGroupBox.ResumeLayout(false);
			this.InternalAllocationGroupBox.PerformLayout();
			this.BorrowedOutGroupBox.ResumeLayout(false);
			this.BorrowedOutGroupBox.PerformLayout();
			this.JM_ReservedUntilBoundDateEdit.ResumeLayout(true);
			this.JM_ReservedUntilBoundDateEdit.PerformLayout();
			this.JM_OH_AllocatedToBoundFindBox.ResumeLayout(true);
			this.JM_OH_AllocatedToBoundFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
