using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccChequeBookForm
	{
		#region Windows Form Designer generated code

		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.ZTextBox AK_CodeBoundText;
		private Enterprise.ZArchitecture.ZTextBox AK_DescriptionBoundText;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox BankAccountBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsAutoPrintChequeCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.ZTextBox StartNoTextBox;
		private Enterprise.ZArchitecture.ZTextBox LastNoTextBox;
		private Enterprise.ZArchitecture.ZTextBox CurrentNoTextBox;
		private ZGroupBox ChequePrintingGroupBox;
		private ZGuidFindBox PrinterFindBox;
		private ZCheckBox IsActiveCheckBox;
		private ZGroupBox ChequeBookDetailsGroupBox;
		private IContainer components;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 350, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(590);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccChequeBook);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 318, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.ButtonsUserControl.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 310, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChequeBookForm|2d5330fd-3732-4090-b71e-5325b9053877", "Check Book");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 283, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_Desc)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_AB)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_GB)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_AutoPrintCheque)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_Calc_CurrentNoString)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_Calc_LastNoString)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_Calc_StartNoString)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChequeBook)(null)).AK_SQ)));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 283, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 283, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// AccChequeBookForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 374, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChequeBookForm|70a60243-2088-4249-a253-2e7b46958b04", "Check Book");
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccChequeBook);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 288, true);
			this.Name = "AccChequeBookForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.AK_CodeBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.AK_DescriptionBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.BankAccountBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BranchBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.IsAutoPrintChequeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChequeBookDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CurrentNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LastNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StartNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChequePrintingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PrinterFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MainTabPage.SuspendLayout();
			this.ChequeBookDetailsGroupBox.SuspendLayout();
			this.ChequePrintingGroupBox.SuspendLayout();
			this.MainTabPage.Controls.Add(this.ChequeBookDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.ChequePrintingGroupBox);
			// 
			// AK_CodeBoundText
			// 
			this.BindingSource.SetBindingMember(this.AK_CodeBoundText, "AK_Code");
			this.AK_CodeBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 16);
			this.AK_CodeBoundText.Name = "AK_CodeBoundText";
			this.AK_CodeBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20);
			this.AK_CodeBoundText.TabIndex = 1;
			// 
			// AK_DescriptionBoundText
			// 
			this.BindingSource.SetBindingMember(this.AK_DescriptionBoundText, "AK_Desc");
			this.AK_DescriptionBoundText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AK_DescriptionBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 41);
			this.AK_DescriptionBoundText.Name = "AK_DescriptionBoundText";
			this.AK_DescriptionBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20);
			this.AK_DescriptionBoundText.TabIndex = 4;
			// 
			// BankAccountBoundGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BankAccountBoundGuidFindBox, "AK_AB");
			this.BankAccountBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 65);
			this.BankAccountBoundGuidFindBox.Name = "BankAccountBoundGuidFindBox";
			this.BankAccountBoundGuidFindBox.PopupCaption = null;
			this.BankAccountBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20);
			this.BankAccountBoundGuidFindBox.TabIndex = 6;
			// 
			// BranchBoundGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BranchBoundGuidFindBox, "AK_GB");
			this.BranchBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 89);
			this.BranchBoundGuidFindBox.Name = "BranchBoundGuidFindBox";
			this.BranchBoundGuidFindBox.PopupCaption = null;
			this.BranchBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20);
			this.BranchBoundGuidFindBox.TabIndex = 8;
			// 
			// IsAutoPrintChequeCheckBox
			// 
			this.IsAutoPrintChequeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsAutoPrintChequeCheckBox, "AK_AutoPrintCheque");
			this.IsAutoPrintChequeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsAutoPrintChequeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 17);
			this.IsAutoPrintChequeCheckBox.Name = "IsAutoPrintChequeCheckBox";
			this.IsAutoPrintChequeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 17);
			this.IsAutoPrintChequeCheckBox.TabIndex = 0;
			// 
			// ChequeBookDetailsGroupBox
			// 
			this.ChequeBookDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChequeBookForm|e4319e42-4aa8-408a-a3c0-2ec06939f847", "Check Book Details");
			this.ChequeBookDetailsGroupBox.Controls.Add(this.IsActiveCheckBox);
			this.ChequeBookDetailsGroupBox.Controls.Add(this.AK_CodeBoundText);
			this.ChequeBookDetailsGroupBox.Controls.Add(this.CurrentNoTextBox);
			this.ChequeBookDetailsGroupBox.Controls.Add(this.AK_DescriptionBoundText);
			this.ChequeBookDetailsGroupBox.Controls.Add(this.LastNoTextBox);
			this.ChequeBookDetailsGroupBox.Controls.Add(this.BankAccountBoundGuidFindBox);
			this.ChequeBookDetailsGroupBox.Controls.Add(this.StartNoTextBox);
			this.ChequeBookDetailsGroupBox.Controls.Add(this.BranchBoundGuidFindBox);
			this.ChequeBookDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3);
			this.ChequeBookDetailsGroupBox.Name = "ChequeBookDetailsGroupBox";
			this.ChequeBookDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 190);
			this.ChequeBookDetailsGroupBox.TabIndex = 0;
			this.ChequeBookDetailsGroupBox.TabStop = false;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "AK_IsActive");
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(213, 15);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 24);
			this.IsActiveCheckBox.TabIndex = 2;
			// 
			// CurrentNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrentNoTextBox, "AK_Calc_CurrentNoString");
			this.CurrentNoTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChequeBookForm|8d39f303-cce9-496f-964b-af59691f490e", "Current No.", "Current Check Number", "The Current (Next) Check Number. This number must not be less than the start number or greater than the last number.");
			this.CurrentNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 161);
			this.CurrentNoTextBox.Name = "CurrentNoTextBox";
			this.CurrentNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20);
			this.CurrentNoTextBox.TabIndex = 14;
			this.CurrentNoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LastNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.LastNoTextBox, "AK_Calc_LastNoString");
			this.LastNoTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChequeBookForm|151f368d-9361-499a-9e51-240147f5ed50", "Last No.", "Last Check Number", "Last Number for the Check Book.");
			this.LastNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 137);
			this.LastNoTextBox.Name = "LastNoTextBox";
			this.LastNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20);
			this.LastNoTextBox.TabIndex = 12;
			this.LastNoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StartNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.StartNoTextBox, "AK_Calc_StartNoString");
			this.StartNoTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChequeBookForm|ec401c6b-3cfc-4923-8cec-768517ca5b23", "Start No.", "Start Check Number", "Start Number for the Check Book.");
			this.StartNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 113);
			this.StartNoTextBox.Name = "StartNoTextBox";
			this.StartNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20);
			this.StartNoTextBox.TabIndex = 10;
			this.StartNoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChequePrintingGroupBox
			// 
			this.ChequePrintingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChequeBookForm|c64bbd96-d7c7-4804-beba-92549c02fd84", "Check Printing");
			this.ChequePrintingGroupBox.Controls.Add(this.PrinterFindBox);
			this.ChequePrintingGroupBox.Controls.Add(this.IsAutoPrintChequeCheckBox);
			this.ChequePrintingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 199);
			this.ChequePrintingGroupBox.Name = "ChequePrintingGroupBox";
			this.ChequePrintingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 73);
			this.ChequePrintingGroupBox.TabIndex = 1;
			this.ChequePrintingGroupBox.TabStop = false;
			// 
			// PrinterFindBox
			// 
			this.BindingSource.SetBindingMember(this.PrinterFindBox, "AK_SQ");
			this.PrinterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 42);
			this.PrinterFindBox.Name = "PrinterFindBox";
			this.PrinterFindBox.PopupCaption = null;
			this.PrinterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20);
			this.PrinterFindBox.TabIndex = 2;
			this.ChequeBookDetailsGroupBox.ResumeLayout(false);
			this.ChequeBookDetailsGroupBox.PerformLayout();
			this.ChequePrintingGroupBox.ResumeLayout(false);
			this.ChequePrintingGroupBox.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		#endregion

	}
}
