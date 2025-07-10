using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccComplianceSequenceForm
	{
		#region Windows Form Designer generated code

		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private ZTextBox XD_PrefixBoundText;
		internal ZGuidFindBox PrinterFindBox;
		private ZTextBox XD_CodeBoundText;
		private ZTextBox XD_DescriptionBoundText;
		private ZTextBox StartNoTextBox;
		private ZGuidFindBox BranchBoundGuidFindBox;
		internal ZGuidFindBox MenuGuidFindBox;
		internal ZCalcEdit XD_MaxChargesPerTransactionBoundText;
		internal ZTextBox NextNoTextBox;
		internal ZDateEdit XD_ExpiryDateEdit;
		private ZTextBox EndNoTextBox;
		private ZTextBox PrintingAuthNoTextBox;
		private ZGroupBox bookDetailsGroupBox;
		private ZGroupBox numberSeriesGroupBox;
		private ZGroupBox documentGroupBox;
		private ZGroupBox printingGroupBox;
		private ZCalcEdit XD_MaxNumberDigitsTextEdit;
		private ZDropEdit XD_SequenceClassDropEdit;
		private ZTextBox DocumentTitleText;
		internal ZButton VoidingSequenceButton;
		internal ZLabel numberFullLabel;
		internal ZCheckBox IsActiveCheckBox;
		internal ZDropEdit ComplianceRollupTypeDropEdit;
		private ZDropEdit XD_Calc_AllocationLevelDropEdit1;
		private ZDateEdit XD_StartDateEdit;
		private ZGuidFindBox departmentFindBox;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		//private IContainer components;

		new void InitializeComponent()
		{
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 686, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(590);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccComplianceSequence);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 654, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.ButtonsUserControl.TabIndex = 18;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 640, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccComplianceSequenceForm|bd3c9be8-55ca-406a-8fef-7a6e8439d770", "Compliance Invoice Book");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 613, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_GB_BranchOwner)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_Calc_StartNumberString)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_SQ_DocumentPrintQueue)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_Prefix)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_SU_MenuItem)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_MaxChargesPerTransaction)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_ExpiryDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_StartDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_RollupBehaviourWhenMaxExceeded)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_Calc_EndNumberString)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_PrintingAuthorizationNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_Calc_NextNumberString)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_MaximumNumberDigits)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_SequenceClass)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_Calc_DocumentTitle)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_AllocationLevel)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_GE_Department)));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 571, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 571, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			this.zStmNoteTabPage1.RunWhenBindingOrFirstShown(new System.EventHandler(this.zStmNoteTabPage1_InitializeTab));
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 571, true);
			this.zLogsTabPage1.TabIndex = 2;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(null)).XD_NumberFormat)));
			// 
			// AccComplianceSequenceForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c23ad4cd-cafd-4029-9a26-b74fcca75845", "Compliance Invoice Book");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 710, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccComplianceSequence);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 288, true);
			this.Name = "AccComplianceSequenceForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.BranchBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.StartNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.XD_DescriptionBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.XD_CodeBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.PrinterFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.XD_PrefixBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.MenuGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.XD_MaxChargesPerTransactionBoundText = new Enterprise.ZArchitecture.ZCalcEdit();
			this.XD_ExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.XD_StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ComplianceRollupTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EndNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrintingAuthNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NextNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.numberSeriesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.documentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.printingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.bookDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.XD_MaxNumberDigitsTextEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.XD_SequenceClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocumentTitleText = new Enterprise.ZArchitecture.ZTextBox();
			this.VoidingSequenceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.numberFullLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.XD_Calc_AllocationLevelDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.departmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ComplianceNumberFormatDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MainTabPage.SuspendLayout();
			this.BranchBoundGuidFindBox.SuspendLayout();
			this.PrinterFindBox.SuspendLayout();
			this.MenuGuidFindBox.SuspendLayout();
			this.XD_ExpiryDateEdit.SuspendLayout();
			this.XD_StartDateEdit.SuspendLayout();
			this.ComplianceRollupTypeDropEdit.SuspendLayout();
			this.numberSeriesGroupBox.SuspendLayout();
			this.documentGroupBox.SuspendLayout();
			this.printingGroupBox.SuspendLayout();
			this.bookDetailsGroupBox.SuspendLayout();
			this.XD_SequenceClassDropEdit.SuspendLayout();
			this.XD_Calc_AllocationLevelDropEdit1.SuspendLayout();
			this.departmentFindBox.SuspendLayout();
			this.ComplianceNumberFormatDropEdit.SuspendLayout();
			this.MainTabPage.Controls.Add(this.bookDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.numberSeriesGroupBox);
			this.MainTabPage.Controls.Add(this.documentGroupBox);
			this.MainTabPage.Controls.Add(this.printingGroupBox);
			// 
			// BranchBoundGuidFindBox
			// 
			this.BranchBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchBoundGuidFindBox, "XD_GB_BranchOwner");
			this.BranchBoundGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("984fa1cf-b263-4f0f-b823-c1fdb68761b4", "Allocation / Printing Branch");
			this.BranchBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 126, true);
			this.BranchBoundGuidFindBox.Name = "BranchBoundGuidFindBox";
			this.BranchBoundGuidFindBox.PopupCaption = null;
			this.BranchBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.BranchBoundGuidFindBox.TabIndex = 7;
			// 
			// StartNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.StartNoTextBox, "XD_Calc_StartNumberString");
			this.StartNoTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e75dcc28-3c52-49d6-a235-af2bbb331afe", "Start number");
			this.StartNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 45, true);
			this.StartNoTextBox.Name = "StartNoTextBox";
			this.StartNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.StartNoTextBox.TabIndex = 10;
			this.StartNoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// XD_DescriptionBoundText
			// 
			this.BindingSource.SetBindingMember(this.XD_DescriptionBoundText, "XD_Description");
			this.XD_DescriptionBoundText.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4cf391d1-497f-4d36-aaf9-15a22135964a", "Description");
			this.XD_DescriptionBoundText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.XD_DescriptionBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 71, true);
			this.XD_DescriptionBoundText.Name = "XD_DescriptionBoundText";
			this.XD_DescriptionBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.XD_DescriptionBoundText.TabIndex = 5;
			// 
			// XD_CodeBoundText
			// 
			this.BindingSource.SetBindingMember(this.XD_CodeBoundText, "XD_Code");
			this.XD_CodeBoundText.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("693e827a-0856-4346-982e-2b9b7e3fd047", "Code");
			this.XD_CodeBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 45, true);
			this.XD_CodeBoundText.Name = "XD_CodeBoundText";
			this.XD_CodeBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.XD_CodeBoundText.TabIndex = 3;
			// 
			// PrinterFindBox
			// 
			this.PrinterFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrinterFindBox, "XD_SQ_DocumentPrintQueue");
			this.PrinterFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3132f055-349b-4ff4-9b25-d2edc9730236", "Printing Queue");
			this.PrinterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 19, true);
			this.PrinterFindBox.Name = "PrinterFindBox";
			this.PrinterFindBox.PopupCaption = null;
			this.PrinterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.PrinterFindBox.TabIndex = 19;
			// 
			// XD_PrefixBoundText
			// 
			this.BindingSource.SetBindingMember(this.XD_PrefixBoundText, "XD_Prefix");
			this.XD_PrefixBoundText.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fbb1874f-b142-49a5-9c2d-13a8137b5ceb", "Series Prefix 1");
			this.XD_PrefixBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.XD_PrefixBoundText.Name = "XD_PrefixBoundText";
			this.XD_PrefixBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.XD_PrefixBoundText.TabIndex = 8;
			this.XD_PrefixBoundText.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MenuGuidFindBox
			// 
			this.MenuGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MenuGuidFindBox, "XD_SU_MenuItem");
			this.MenuGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4201001c-ebb6-4005-817f-ec9be074ec40", "Document Menu");
			this.MenuGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 19, true);
			this.MenuGuidFindBox.Name = "MenuGuidFindBox";
			this.MenuGuidFindBox.PopupCaption = null;
			this.MenuGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.MenuGuidFindBox.TabIndex = 15;
			// 
			// XD_MaxChargesPerTransactionBoundText
			// 
			this.BindingSource.SetBindingMember(this.XD_MaxChargesPerTransactionBoundText, "XD_MaxChargesPerTransaction");
			this.XD_MaxChargesPerTransactionBoundText.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d3717ff4-b42e-4416-9d6f-3fe0ae765bee", "Maximum Charge Lines");
			this.XD_MaxChargesPerTransactionBoundText.DecimalPlaces = 2;
			this.XD_MaxChargesPerTransactionBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 45, true);
			this.XD_MaxChargesPerTransactionBoundText.Name = "XD_MaxChargesPerTransactionBoundText";
			this.XD_MaxChargesPerTransactionBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.XD_MaxChargesPerTransactionBoundText.TabIndex = 17;
			this.XD_MaxChargesPerTransactionBoundText.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// XD_ExpiryDateEdit
			// 
			this.XD_ExpiryDateEdit.AllowDrop = true;
			this.XD_ExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.XD_ExpiryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.XD_ExpiryDateEdit, "XD_ExpiryDate");
			this.XD_ExpiryDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("35b08ae9-36f2-4b2f-8a45-caeb9495d660", "Expiry Date");
			this.XD_ExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 175, true);
			this.XD_ExpiryDateEdit.Name = "XD_ExpiryDateEdit";
			this.XD_ExpiryDateEdit.TabIndex = 16;
			// 
			// XD_StartDateEdit
			// 
			this.XD_StartDateEdit.AllowDrop = true;
			this.XD_StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.XD_StartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.XD_StartDateEdit, "XD_StartDate");
			this.XD_StartDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("685a1cd1-8580-4bbc-bd17-aa2f3dbcd98b", "Valid From");
			this.XD_StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 149, true);
			this.XD_StartDateEdit.Name = "XD_StartDateEdit";
			this.XD_StartDateEdit.TabIndex = 15;
			// 
			// ComplianceRollupTypeDropEdit
			// 
			this.ComplianceRollupTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplianceRollupTypeDropEdit, "XD_RollupBehaviourWhenMaxExceeded");
			this.ComplianceRollupTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("39584e3e-a3f8-4a75-aee1-cb3c1aee8b05", "Document Printing Style");
			this.ComplianceRollupTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 71, true);
			this.ComplianceRollupTypeDropEdit.Name = "ComplianceRollupTypeDropEdit";
			this.ComplianceRollupTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ComplianceRollupTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.ComplianceRollupTypeDropEdit.TabIndex = 18;
			// 
			// EndNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.EndNoTextBox, "XD_Calc_EndNumberString");
			this.EndNoTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ad8765a8-a3c0-445c-9388-aa201fb44747", "End number");
			this.EndNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 71, true);
			this.EndNoTextBox.Name = "EndNoTextBox";
			this.EndNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.EndNoTextBox.TabIndex = 11;
			this.EndNoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PrintingAuthNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PrintingAuthNoTextBox, "XD_PrintingAuthorizationNumber");
			this.PrintingAuthNoTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ed380921-a907-41bf-9458-2fbe8d225249", "Printing Authorization Number");
			this.PrintingAuthNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 97, true);
			this.PrintingAuthNoTextBox.Name = "PrintingAuthNoTextBox";
			this.PrintingAuthNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 20, true);
			this.PrintingAuthNoTextBox.TabIndex = 12;
			// 
			// NextNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.NextNoTextBox, "XD_Calc_NextNumberString");
			this.NextNoTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("697713cb-4cd4-4140-8f90-5e8f5639ad2b", "Next Number");
			this.NextNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 123, true);
			this.NextNoTextBox.Name = "NextNoTextBox";
			this.NextNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.NextNoTextBox.TabIndex = 13;
			this.NextNoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// numberSeriesGroupBox
			// 
			this.numberSeriesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("422d8296-a164-4863-b359-bf44a3623609", "Number Series");
			this.numberSeriesGroupBox.Controls.Add(this.ComplianceNumberFormatDropEdit);
			this.numberSeriesGroupBox.Controls.Add(this.VoidingSequenceButton);
			this.numberSeriesGroupBox.Controls.Add(this.XD_MaxNumberDigitsTextEdit);
			this.numberSeriesGroupBox.Controls.Add(this.XD_PrefixBoundText);
			this.numberSeriesGroupBox.Controls.Add(this.NextNoTextBox);
			this.numberSeriesGroupBox.Controls.Add(this.XD_ExpiryDateEdit);
			this.numberSeriesGroupBox.Controls.Add(this.XD_StartDateEdit);
			this.numberSeriesGroupBox.Controls.Add(this.StartNoTextBox);
			this.numberSeriesGroupBox.Controls.Add(this.EndNoTextBox);
			this.numberSeriesGroupBox.Controls.Add(this.PrintingAuthNoTextBox);
			this.numberSeriesGroupBox.Controls.Add(this.numberFullLabel);
			this.numberSeriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 205, true);
			this.numberSeriesGroupBox.Name = "numberSeriesGroupBox";
			this.numberSeriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 228, true);
			this.numberSeriesGroupBox.TabIndex = 17;
			this.numberSeriesGroupBox.TabStop = false;
			// 
			// documentGroupBox
			// 
			this.documentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9864ec62-7948-49e2-b24d-f9f7102f5a7a", "Compliance Document");
			this.documentGroupBox.Controls.Add(this.MenuGuidFindBox);
			this.documentGroupBox.Controls.Add(this.ComplianceRollupTypeDropEdit);
			this.documentGroupBox.Controls.Add(this.XD_MaxChargesPerTransactionBoundText);
			this.documentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 439, true);
			this.documentGroupBox.Name = "documentGroupBox";
			this.documentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 100, true);
			this.documentGroupBox.TabIndex = 18;
			this.documentGroupBox.TabStop = false;
			// 
			// printingGroupBox
			// 
			this.printingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ca5b2062-da65-4d53-a11b-f09e9f98895a", "Compliance Printing");
			this.printingGroupBox.Controls.Add(this.PrinterFindBox);
			this.printingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 545, true);
			this.printingGroupBox.Name = "printingGroupBox";
			this.printingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 48, true);
			this.printingGroupBox.TabIndex = 19;
			this.printingGroupBox.TabStop = false;
			// 
			// bookDetailsGroupBox
			// 
			this.bookDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2f48932c-153f-4a05-bb59-436872ddc9f1", "Compliance Book Details");
			this.bookDetailsGroupBox.Controls.Add(this.departmentFindBox);
			this.bookDetailsGroupBox.Controls.Add(this.XD_Calc_AllocationLevelDropEdit1);
			this.bookDetailsGroupBox.Controls.Add(this.IsActiveCheckBox);
			this.bookDetailsGroupBox.Controls.Add(this.DocumentTitleText);
			this.bookDetailsGroupBox.Controls.Add(this.BranchBoundGuidFindBox);
			this.bookDetailsGroupBox.Controls.Add(this.XD_SequenceClassDropEdit);
			this.bookDetailsGroupBox.Controls.Add(this.XD_CodeBoundText);
			this.bookDetailsGroupBox.Controls.Add(this.XD_DescriptionBoundText);
			this.bookDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 6, true);
			this.bookDetailsGroupBox.Name = "bookDetailsGroupBox";
			this.bookDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 193, true);
			this.bookDetailsGroupBox.TabIndex = 20;
			this.bookDetailsGroupBox.TabStop = false;
			// 
			// XD_MaxNumberDigitsTextEdit
			// 
			this.BindingSource.SetBindingMember(this.XD_MaxNumberDigitsTextEdit, "XD_MaximumNumberDigits");
			this.XD_MaxNumberDigitsTextEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("703b5cad-fba9-46a0-b7a6-cb14d1cec968", "Max Number Digits");
			this.XD_MaxNumberDigitsTextEdit.DecimalPlaces = 0;
			this.XD_MaxNumberDigitsTextEdit.Decimals = 0;
			this.XD_MaxNumberDigitsTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 19, true);
			this.XD_MaxNumberDigitsTextEdit.Name = "XD_MaxNumberDigitsTextEdit";
			this.XD_MaxNumberDigitsTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.XD_MaxNumberDigitsTextEdit.TabIndex = 9;
			this.XD_MaxNumberDigitsTextEdit.Text = "0";
			this.XD_MaxNumberDigitsTextEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// XD_SequenceClassDropEdit
			// 
			this.XD_SequenceClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.XD_SequenceClassDropEdit, "XD_SequenceClass");
			this.XD_SequenceClassDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("af77307b-dd7e-436b-b0e0-a1158f6cd597", "Sub Type");
			this.XD_SequenceClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 19, true);
			this.XD_SequenceClassDropEdit.Name = "XD_SequenceClassDropEdit";
			this.XD_SequenceClassDropEdit.ShouldResizeByMaxLength = true;
			this.XD_SequenceClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.XD_SequenceClassDropEdit.TabIndex = 1;
			// 
			// DocumentTitleText
			// 
			this.BindingSource.SetBindingMember(this.DocumentTitleText, "XD_Calc_DocumentTitle");
			this.DocumentTitleText.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("501fc9e9-ef65-4b21-8e23-7aa50ab0c92e", "Document Title");
			this.DocumentTitleText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DocumentTitleText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 19, true);
			this.DocumentTitleText.Name = "DocumentTitleText";
			this.DocumentTitleText.ReadOnly = true;
			this.DocumentTitleText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.DocumentTitleText.TabIndex = 2;
			// 
			// VoidingSequenceButton
			// 
			this.VoidingSequenceButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bdd6e351-ccda-4595-a769-61951359cd74", "Voiding Sequence No.");
			this.VoidingSequenceButton.IsCaptionOverridden = false;
			this.VoidingSequenceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 146, true);
			this.VoidingSequenceButton.Name = "VoidingSequenceButton";
			this.VoidingSequenceButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.VoidingSequenceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 23, true);
			this.VoidingSequenceButton.TabIndex = 14;
			this.VoidingSequenceButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.VoidingSequenceButton.ToolTipCaption = null;
			this.VoidingSequenceButton.Click += new System.EventHandler(this.VoidingSequenceButton_Click);
			// 
			// numberFullLabel
			// 
			this.numberFullLabel.AutoSize = true;
			this.numberFullLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5e876488-4eaa-4c65-885c-15bda447cd18", "All numbers in this series have been used.");
			this.numberFullLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.numberFullLabel.ForeColor = System.Drawing.Color.Red;
			this.numberFullLabel.IsFontBold = true;
			this.numberFullLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 127, true);
			this.numberFullLabel.Name = "numberFullLabel";
			this.numberFullLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 13, true);
			this.numberFullLabel.TabIndex = 19;
			this.numberFullLabel.Visible = false;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "XD_IsActive");
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("74663a17-9f32-48ef-864f-9b30409a71dc", "Is Active");
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 45, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.IsActiveCheckBox.TabIndex = 4;
			this.IsActiveCheckBox.CheckedChanged += new System.EventHandler(this.IsActiveCheckBox_CheckedChanged);
			// 
			// XD_Calc_AllocationLevelDropEdit1
			// 
			this.XD_Calc_AllocationLevelDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.XD_Calc_AllocationLevelDropEdit1, "XD_AllocationLevel");
			this.XD_Calc_AllocationLevelDropEdit1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4352c4b1-7932-44ad-b233-4cd8a3da228e", "Allocation Level");
			this.XD_Calc_AllocationLevelDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 97, true);
			this.XD_Calc_AllocationLevelDropEdit1.Name = "XD_Calc_AllocationLevelDropEdit1";
			this.XD_Calc_AllocationLevelDropEdit1.ShouldResizeByMaxLength = true;
			this.XD_Calc_AllocationLevelDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.XD_Calc_AllocationLevelDropEdit1.TabIndex = 6;
			// 
			// departmentFindBox
			// 
			this.departmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.departmentFindBox, "XD_GE_Department");
			this.departmentFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("32373611-3bb1-4e06-8b0e-94b5a8b76212", "Allocation / Printing Department");
			this.departmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 156, true);
			this.departmentFindBox.Name = "departmentFindBox";
			this.departmentFindBox.PopupCaption = null;
			this.departmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.departmentFindBox.TabIndex = 8;
			// 
			// ComplianceNumberFormatDropEdit
			// 
			this.ComplianceNumberFormatDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplianceNumberFormatDropEdit, "XD_NumberFormat");
			this.ComplianceNumberFormatDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("70f424af-02cf-45b9-b465-4429ac9d6972", "Number Format");
			this.ComplianceNumberFormatDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 201, true);
			this.ComplianceNumberFormatDropEdit.Name = "ComplianceNumberFormatDropEdit";
			this.ComplianceNumberFormatDropEdit.ShouldResizeByMaxLength = true;
			this.ComplianceNumberFormatDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 20, true);
			this.ComplianceNumberFormatDropEdit.TabIndex = 21;
			this.MainTabPage.PerformLayout();
			this.BranchBoundGuidFindBox.ResumeLayout(true);
			this.BranchBoundGuidFindBox.PerformLayout();
			this.PrinterFindBox.ResumeLayout(true);
			this.PrinterFindBox.PerformLayout();
			this.MenuGuidFindBox.ResumeLayout(true);
			this.MenuGuidFindBox.PerformLayout();
			this.XD_ExpiryDateEdit.ResumeLayout(true);
			this.XD_ExpiryDateEdit.PerformLayout();
			this.XD_StartDateEdit.ResumeLayout(true);
			this.XD_StartDateEdit.PerformLayout();
			this.ComplianceRollupTypeDropEdit.ResumeLayout(true);
			this.ComplianceRollupTypeDropEdit.PerformLayout();
			this.numberSeriesGroupBox.ResumeLayout(false);
			this.numberSeriesGroupBox.PerformLayout();
			this.documentGroupBox.ResumeLayout(false);
			this.documentGroupBox.PerformLayout();
			this.printingGroupBox.ResumeLayout(false);
			this.printingGroupBox.PerformLayout();
			this.bookDetailsGroupBox.ResumeLayout(false);
			this.bookDetailsGroupBox.PerformLayout();
			this.XD_SequenceClassDropEdit.ResumeLayout(true);
			this.XD_SequenceClassDropEdit.PerformLayout();
			this.XD_Calc_AllocationLevelDropEdit1.ResumeLayout(true);
			this.XD_Calc_AllocationLevelDropEdit1.PerformLayout();
			this.departmentFindBox.ResumeLayout(true);
			this.departmentFindBox.PerformLayout();
			this.ComplianceNumberFormatDropEdit.ResumeLayout(true);
			this.ComplianceNumberFormatDropEdit.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private void zStmNoteTabPage1_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.zStmNoteTabPage1.SuspendLayout();
			this.zStmNoteTabPage1.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(true);
		}

		private void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}

		#endregion

	}
}
