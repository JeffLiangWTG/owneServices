using Enterprise.Customs.GUI.Guarantees;
using Enterprise.Customs.GUI.Permits;

namespace Enterprise.Customs.GUI
{
	partial class GuaranteeForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GuaranteeForm));
			this.GuaranteeDetailsUserControl = new Enterprise.Customs.GUI.GuaranteeDetailsUserControl();
			this.GuaranteeDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GuaranteeAmountZTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GuaranteeTransactionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BalanceRulesAccessTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.BalanceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GuaranteBalanceStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BalanceRemainingZTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PercentValueCustomLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BalanceUsedZTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PendingBalanceZTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.RulesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GuaranteeUserControl = new Enterprise.Customs.GUI.Permits.CusGuaranteeRuleUserControl();
			this.AccessTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.guaranteeAccessLineControl = new Enterprise.Customs.GUI.Guarantees.GuaranteeAccessLineControl();
			this.AdditionalReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.GuaranteeMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GuaranteeMessagesUserControl = new MessagesUserControl();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NewTransactionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewTransactionAmountZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NewTransactionDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.NewTransactionTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NewTransactionCommentZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewTransactionReferenceZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewTransactionAddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel4 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GuaranteeDetailsUserControl.SuspendLayout();
			this.GuaranteeDetailsGroupBox.SuspendLayout();
			this.GuaranteeTransactionsGroupBox.SuspendLayout();
			this.BalanceRulesAccessTabControl.SuspendLayout();
			this.BalanceTabPage.SuspendLayout();
			this.GuaranteBalanceStatusGroupBox.SuspendLayout();
			this.RulesTabPage.SuspendLayout();
			this.GuaranteeUserControl.SuspendLayout();
			this.AccessTabPage.SuspendLayout();
			this.guaranteeAccessLineControl.SuspendLayout();
			this.AdditionalReferencesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.GuaranteeMessagesTabPage.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.NewTransactionGroupBox.SuspendLayout();
			this.NewTransactionDateZDateEdit.SuspendLayout();
			this.NewTransactionTypeZDropEdit.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 630, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zPanel4);
			this.MainTabPage.Controls.Add(this.zPanel3);
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 603, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 603, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 603, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseCusGuaranteeHeader);
			// 
			// GuaranteeDetailsUserControl
			// 
			this.GuaranteeDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeDetailsUserControl, ".");
			this.GuaranteeDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteeDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.GuaranteeDetailsUserControl.Name = "GuaranteeDetailsUserControl";
			this.GuaranteeDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 101, true);
			this.GuaranteeDetailsUserControl.TabIndex = 0;
			// 
			// GuaranteeDetailsGroupBox
			// 
			this.GuaranteeDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("a2ebab69-9f62-466b-b0ca-9ff5d778f517", "Guarantee Details");
			this.GuaranteeDetailsGroupBox.Controls.Add(this.GuaranteeDetailsUserControl);
			this.GuaranteeDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteeDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GuaranteeDetailsGroupBox.Name = "GuaranteeDetailsGroupBox";
			this.GuaranteeDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 120, true);
			this.GuaranteeDetailsGroupBox.TabIndex = 2;
			this.GuaranteeDetailsGroupBox.TabStop = false;
			// 
			// GuaranteeAmountZTextBox
			// 
			this.BindingSource.SetBindingMember(this.GuaranteeAmountZTextBox, "CPH_Calc_OpeningBalance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_Calc_OpeningBalance)));
			this.GuaranteeAmountZTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("56d8e6b3-2613-448f-943d-0d5f73431838", "Guarantee Amount");
			this.GuaranteeAmountZTextBox.DecimalPlaces = 2;
			this.GuaranteeAmountZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 49, true);
			this.GuaranteeAmountZTextBox.Name = "GuaranteeAmountZTextBox";
			this.GuaranteeAmountZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.GuaranteeAmountZTextBox.TabIndex = 16;
			this.GuaranteeAmountZTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GuaranteeTransactionsGroupBox
			// 
			this.GuaranteeTransactionsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("fa037ba0-db28-44cc-80ed-212bfacf9d52", "Guarantee Transactions");
			this.GuaranteeTransactionsGroupBox.Controls.Add(this.zPanel2);
			this.GuaranteeTransactionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteeTransactionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.GuaranteeTransactionsGroupBox.Name = "GuaranteeTransactionsGroupBox";
			this.GuaranteeTransactionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 201, true);
			this.GuaranteeTransactionsGroupBox.TabIndex = 24;
			this.GuaranteeTransactionsGroupBox.TabStop = false;
			// 
			// zPanel2
			// 
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 182, true);
			this.zPanel2.TabIndex = 25;
			// 
			// BalanceRulesAccessTabControl
			// 
			this.BalanceRulesAccessTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BalanceRulesAccessTabControl.Controls.Add(this.BalanceTabPage);
			this.BalanceRulesAccessTabControl.Controls.Add(this.RulesTabPage);
			this.BalanceRulesAccessTabControl.Controls.Add(this.AccessTabPage);
			this.BalanceRulesAccessTabControl.Controls.Add(this.AdditionalReferencesTabPage);
			this.BalanceRulesAccessTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BalanceRulesAccessTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BalanceRulesAccessTabControl.Name = "BalanceRulesAccessTabControl";
			this.BalanceRulesAccessTabControl.SelectedIndex = 0;
			this.BalanceRulesAccessTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 226, true);
			this.BalanceRulesAccessTabControl.TabIndex = 1;
			this.BalanceRulesAccessTabControl.TabStop = false;
			// 
			// BalanceTabPage
			// 
			this.BalanceTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("21931F13-7A4C-4ED4-BD68-AAA485F2E461", "Balance");
			this.BalanceTabPage.Controls.Add(this.GuaranteBalanceStatusGroupBox);
			this.BalanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BalanceTabPage.Name = "BalanceTabPage";
			this.BalanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 199, true);
			this.BalanceTabPage.TabIndex = 0;
			// 
			// GuaranteBalanceStatusGroupBox
			// 
			this.GuaranteBalanceStatusGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("a4b73c90-25ec-49eb-bd8c-12e79925664f", "Guarantee Balance");
			this.GuaranteBalanceStatusGroupBox.Controls.Add(this.BalanceRemainingZTextBox);
			this.GuaranteBalanceStatusGroupBox.Controls.Add(this.PercentValueCustomLabel);
			this.GuaranteBalanceStatusGroupBox.Controls.Add(this.BalanceUsedZTextBox);
			this.GuaranteBalanceStatusGroupBox.Controls.Add(this.PendingBalanceZTextBox);
			this.GuaranteBalanceStatusGroupBox.Controls.Add(this.ProgressBar);
			this.GuaranteBalanceStatusGroupBox.Controls.Add(this.GuaranteeAmountZTextBox);
			this.GuaranteBalanceStatusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteBalanceStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GuaranteBalanceStatusGroupBox.Name = "GuaranteBalanceStatusGroupBox";
			this.GuaranteBalanceStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 199, true);
			this.GuaranteBalanceStatusGroupBox.TabIndex = 15;
			this.GuaranteBalanceStatusGroupBox.TabStop = false;
			// 
			// BalanceRemainingZTextBox
			// 
			this.BindingSource.SetBindingMember(this.BalanceRemainingZTextBox, "CPH_Calc_TotalBalanceIncludingPendingDecimal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_Calc_TotalBalanceIncludingPendingDecimal)));
			this.BalanceRemainingZTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("bc5fa476-d671-4949-8be0-f4459b2db798", "Remaining Balance");
			this.BalanceRemainingZTextBox.DecimalPlaces = 2;
			this.BalanceRemainingZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 71, true);
			this.BalanceRemainingZTextBox.Name = "BalanceRemainingZTextBox";
			this.BalanceRemainingZTextBox.ReadOnly = true;
			this.BalanceRemainingZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BalanceRemainingZTextBox.TabIndex = 17;
			this.BalanceRemainingZTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// PercentValueCustomLabel
			// 
			this.PercentValueCustomLabel.AutoSize = true;
			this.PercentValueCustomLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PercentValueCustomLabel.ForeColor = System.Drawing.Color.DarkGray;
			this.PercentValueCustomLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 13, true);
			this.PercentValueCustomLabel.Name = "PercentValueCustomLabel";
			this.PercentValueCustomLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.PercentValueCustomLabel.TabIndex = 18;
			this.PercentValueCustomLabel.Visible = false;
			// 
			// BalanceUsedZTextBox
			// 
			this.BindingSource.SetBindingMember(this.BalanceUsedZTextBox, "CPH_Calc_UsedBalance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_Calc_UsedBalance)));
			this.BalanceUsedZTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("052f6864-5a28-402b-853c-31f11970d278", "Balance Used");
			this.BalanceUsedZTextBox.DecimalPlaces = 2;
			this.BalanceUsedZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 27, true);
			this.BalanceUsedZTextBox.Name = "BalanceUsedZTextBox";
			this.BalanceUsedZTextBox.ReadOnly = true;
			this.BalanceUsedZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BalanceUsedZTextBox.TabIndex = 10;
			this.BalanceUsedZTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PendingBalanceZTextBox
			// 
			this.BindingSource.SetBindingMember(this.PendingBalanceZTextBox, "CPH_Calc_PendingBalanceDecimal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_Calc_PendingBalanceDecimal)));
			this.PendingBalanceZTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8A465596-6AB5-4DFF-82AC-31D79AAE5E9E", "Pending Balance");
			this.PendingBalanceZTextBox.DecimalPlaces = 2;
			this.PendingBalanceZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 92, true);
			this.PendingBalanceZTextBox.Name = "PendingBalanceZTextBox";
			this.PendingBalanceZTextBox.ReadOnly = true;
			this.PendingBalanceZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.PendingBalanceZTextBox.TabIndex = 17;
			this.PendingBalanceZTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 27, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.ProgressBar.Step = 1;
			this.ProgressBar.TabIndex = 19;
			// 
			// RulesTabPage
			// 
			this.RulesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0CC3E425-5289-4877-AFBB-5758816512A8", "Rules");
			this.RulesTabPage.Controls.Add(this.GuaranteeUserControl);
			this.RulesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.RulesTabPage.Name = "RulesTabPage";
			this.RulesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 199, true);
			this.RulesTabPage.TabIndex = 1;
			// 
			// GuaranteeUserControl
			// 
			this.GuaranteeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeUserControl, ".");
			this.GuaranteeUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GuaranteeUserControl.Name = "GuaranteeUserControl";
			this.GuaranteeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 199, true);
			this.GuaranteeUserControl.TabIndex = 0;
			// 
			// AccessTabPage
			// 
			this.AccessTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8B79CD5A-D0FE-4BFA-9DA5-57F7AD4C7432", "Access");
			this.AccessTabPage.Controls.Add(this.guaranteeAccessLineControl);
			this.AccessTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AccessTabPage.Name = "AccessTabPage";
			this.AccessTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 199, true);
			this.AccessTabPage.TabIndex = 2;
			// 
			// guaranteeAccessLineControl
			// 
			this.guaranteeAccessLineControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.guaranteeAccessLineControl, ".");
			this.guaranteeAccessLineControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.guaranteeAccessLineControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.guaranteeAccessLineControl.Name = "guaranteeAccessLineControl";
			this.guaranteeAccessLineControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 199, true);
			this.guaranteeAccessLineControl.TabIndex = 0;
			// 
			// AdditionalReferencesTabPage
			// 
			this.AdditionalReferencesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("980d42eb-0840-4e2a-84bc-218fb3724849", "Additional References");
			this.AdditionalReferencesTabPage.Controls.Add(this.zGrid1);
			this.AdditionalReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalReferencesTabPage.Name = "AdditionalReferencesTabPage";
			this.AdditionalReferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 199, true);
			this.AdditionalReferencesTabPage.TabIndex = 3;
			this.AdditionalReferencesTabPage.UseVisualStyleBackColor = true;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "AdditionalGuaranteeReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).AdditionalGuaranteeReferences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusGuaranteeReferenceNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).AdditionalGuaranteeReferences)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusGuaranteeReferenceNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).AdditionalGuaranteeReferences)).SyncRoot)).CusGuarantee.CountrySpecificInstruction.AdditionalCustomsReferenceTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusGuaranteeReferenceNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).AdditionalGuaranteeReferences)).SyncRoot)).CY_Data)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "CusGuarantee+CountrySpecificInstruction+AdditionalCustomsReferenceTypes";
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Caption = "Number";
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "d1e60b86-84a8-4801-b1d8-0cd639df83e4";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(958, 193, true);
			this.zGrid1.TabIndex = 0;
			// 
			// GuaranteeMessagesUserControl
			// 
			this.GuaranteeMessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeMessagesUserControl, ".");
			this.GuaranteeMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteeMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GuaranteeMessagesUserControl.Name = "GuaranteeMessagesUserControl";
			this.GuaranteeMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 100, true);
			this.GuaranteeMessagesUserControl.TabIndex = 0;
			// 
			// GuaranteeMessagesTabPage
			// 
			this.GuaranteeMessagesTabPage.Controls.Add(this.GuaranteeMessagesUserControl);
			this.GuaranteeMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GuaranteeMessagesTabPage.Name = "GuaranteeMessagesTabPage";
			this.GuaranteeMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 100, true);
			this.GuaranteeMessagesTabPage.TabIndex = 0;
			this.GuaranteeMessagesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0EEBE095-73EA-44BD-9605-1DFB74E739A2", "Messages");
			this.MainTabControl.Controls.Add(this.GuaranteeMessagesTabPage);
			// 
			// zPanel1
			// 
			this.zPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel1.Controls.Add(this.GuaranteeDetailsGroupBox);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 120, true);
			this.zPanel1.TabIndex = 1;
			// 
			// NewTransactionGroupBox
			// 
			this.NewTransactionGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ED03502A-D4C5-450A-8075-43E7E8235DEF", "New transaction");
			this.NewTransactionGroupBox.Controls.Add(this.NewTransactionAmountZCalcEdit);
			this.NewTransactionGroupBox.Controls.Add(this.NewTransactionDateZDateEdit);
			this.NewTransactionGroupBox.Controls.Add(this.NewTransactionTypeZDropEdit);
			this.NewTransactionGroupBox.Controls.Add(this.NewTransactionCommentZTextBox);
			this.NewTransactionGroupBox.Controls.Add(this.NewTransactionReferenceZTextBox);
			this.NewTransactionGroupBox.Controls.Add(this.NewTransactionAddButton);
			this.NewTransactionGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.NewTransactionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NewTransactionGroupBox.Name = "NewTransactionGroupBox";
			this.NewTransactionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 50, true);
			this.NewTransactionGroupBox.TabIndex = 3;
			this.NewTransactionGroupBox.TabStop = false;
			// 
			// NewTransactionAmountZCalcEdit
			// 
			this.NewTransactionAmountZCalcEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("f502c65e-44c6-4832-aa59-4c70995d9f70", "Value");
			this.NewTransactionAmountZCalcEdit.DecimalPlaces = 2;
			this.NewTransactionAmountZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 22, true);
			this.NewTransactionAmountZCalcEdit.Name = "NewTransactionAmountZCalcEdit";
			this.NewTransactionAmountZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.NewTransactionAmountZCalcEdit.TabIndex = 21;
			this.NewTransactionAmountZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NewTransactionDateZDateEdit
			// 
			this.NewTransactionDateZDateEdit.AllowDrop = true;
			this.NewTransactionDateZDateEdit.AutoCompleteMonthThreshold = 1;
			this.NewTransactionDateZDateEdit.AutoCompleteYear = true;
			this.NewTransactionDateZDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3E002578-AC4C-425E-B96F-7AF726F88DF4", "Date");
			this.NewTransactionDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 22, true);
			this.NewTransactionDateZDateEdit.Name = "NewTransactionDateZDateEdit";
			this.NewTransactionDateZDateEdit.TabIndex = 22;
			// 
			// NewTransactionTypeZDropEdit
			// 
			this.NewTransactionTypeZDropEdit.AllowDrop = true;
			this.NewTransactionTypeZDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F000FC91-04E5-4652-8871-A13BA0C214A3", "Type");
			this.NewTransactionTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 22, true);
			this.NewTransactionTypeZDropEdit.Name = "NewTransactionTypeZDropEdit";
			this.NewTransactionTypeZDropEdit.ShouldResizeByMaxLength = true;
			this.NewTransactionTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 18, true);
			this.NewTransactionTypeZDropEdit.TabIndex = 20;
			// 
			// NewTransactionCommentZTextBox
			// 
			this.NewTransactionCommentZTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("a4193f31-3aa4-4d0f-95ec-a6a517fba80b", "Comment");
			this.NewTransactionCommentZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 22, true);
			this.NewTransactionCommentZTextBox.Name = "NewTransactionCommentZTextBox";
			this.NewTransactionCommentZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.NewTransactionCommentZTextBox.TabIndex = 23;
			// 
			// NewTransactionReferenceZTextBox
			// 
			this.NewTransactionReferenceZTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C84AD38D-F5B6-40A5-9B2D-1BB2B0145C25", "Reference");
			this.NewTransactionReferenceZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(710, 22, true);
			this.NewTransactionReferenceZTextBox.Name = "NewTransactionReferenceZTextBox";
			this.NewTransactionReferenceZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.NewTransactionReferenceZTextBox.TabIndex = 24;
			// 
			// NewTransactionAddButton
			// 
			this.NewTransactionAddButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("00E9F026-4C89-430F-A1E8-BBB01D988AC1", "Add");
			this.NewTransactionAddButton.Image = ((System.Drawing.Image)(resources.GetObject("NewTransactionAddButton.Image")));
			this.NewTransactionAddButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.NewTransactionAddButton.IsCaptionOverridden = false;
			this.NewTransactionAddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 18, true);
			this.NewTransactionAddButton.Name = "NewTransactionAddButton";
			this.NewTransactionAddButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NewTransactionAddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.NewTransactionAddButton.TabIndex = 25;
			this.NewTransactionAddButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.NewTransactionAddButton.ToolTipCaption = null;
			this.NewTransactionAddButton.Click += new System.EventHandler(this.NewTransactionAddButton_Click);
			// 
			// zPanel3
			// 
			this.zPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel3.Controls.Add(this.BalanceRulesAccessTabControl);
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 126, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 226, true);
			this.zPanel3.TabIndex = 2;
			// 
			// zPanel4
			// 
			this.zPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel4.Controls.Add(this.GuaranteeTransactionsGroupBox);
			this.zPanel4.Controls.Add(this.NewTransactionGroupBox);
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 352, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 251, true);
			this.zPanel4.TabIndex = 3;
			// 
			// GuaranteeForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 686, true);
			this.DataSourceType = typeof(Enterprise.Customs.Business.BaseCusGuaranteeHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 725, true);
			this.Name = "GuaranteeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "GuaranteeForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GuaranteeDetailsUserControl.ResumeLayout(true);
			this.GuaranteeDetailsUserControl.PerformLayout();
			this.GuaranteeDetailsGroupBox.ResumeLayout(false);
			this.GuaranteeDetailsGroupBox.PerformLayout();
			this.GuaranteeTransactionsGroupBox.ResumeLayout(false);
			this.GuaranteeTransactionsGroupBox.PerformLayout();
			this.GuaranteeMessagesUserControl.ResumeLayout(false);
			this.GuaranteeMessagesUserControl.PerformLayout();
			this.BalanceRulesAccessTabControl.ResumeLayout(false);
			this.BalanceRulesAccessTabControl.PerformLayout();
			this.BalanceTabPage.ResumeLayout(false);
			this.BalanceTabPage.PerformLayout();
			this.GuaranteBalanceStatusGroupBox.ResumeLayout(false);
			this.GuaranteBalanceStatusGroupBox.PerformLayout();
			this.RulesTabPage.ResumeLayout(false);
			this.RulesTabPage.PerformLayout();
			this.GuaranteeUserControl.ResumeLayout(true);
			this.GuaranteeUserControl.PerformLayout();
			this.AccessTabPage.ResumeLayout(false);
			this.AccessTabPage.PerformLayout();
			this.guaranteeAccessLineControl.ResumeLayout(true);
			this.guaranteeAccessLineControl.PerformLayout();
			this.AdditionalReferencesTabPage.ResumeLayout(false);
			this.AdditionalReferencesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.GuaranteeMessagesTabPage.ResumeLayout(false);
			this.GuaranteeMessagesTabPage.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.NewTransactionGroupBox.ResumeLayout(false);
			this.NewTransactionGroupBox.PerformLayout();
			this.NewTransactionDateZDateEdit.ResumeLayout(true);
			this.NewTransactionDateZDateEdit.PerformLayout();
			this.NewTransactionTypeZDropEdit.ResumeLayout(true);
			this.NewTransactionTypeZDropEdit.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected ZArchitecture.GUI.ZGroupBox GuaranteeTransactionsGroupBox;
		protected ZArchitecture.GUI.ZGroupBox GuaranteeDetailsGroupBox;
		protected ZArchitecture.GUI.ZPanel zPanel1;
		protected ZArchitecture.GUI.ZPanel zPanel2;
		protected ZArchitecture.GUI.ZPanel zPanel3;
		protected ZArchitecture.GUI.ZPanel zPanel4;
		public CargoWise.Windows.UI.KProgressBar ProgressBar;
		private ZArchitecture.ZCalcEdit GuaranteeAmountZTextBox;
		protected ZArchitecture.GUI.ZGroupBox GuaranteBalanceStatusGroupBox;
		private ZArchitecture.ZCalcEdit BalanceRemainingZTextBox;
		private ZArchitecture.ZCalcEdit PendingBalanceZTextBox;
		private ZArchitecture.ZCalcEdit BalanceUsedZTextBox;
		protected ZArchitecture.ZLabel PercentValueCustomLabel;
		public ZArchitecture.GUI.ZGroupBox NewTransactionGroupBox;
		public ZArchitecture.ZCalcEdit NewTransactionAmountZCalcEdit;
		public ZArchitecture.GUI.ZDateEdit NewTransactionDateZDateEdit;
		public ZArchitecture.GUI.ZDropEdit NewTransactionTypeZDropEdit;
		public ZArchitecture.ZTextBox NewTransactionCommentZTextBox;
		public ZArchitecture.ZTextBox NewTransactionReferenceZTextBox;
		protected ZArchitecture.GUI.ZButton NewTransactionAddButton;

		protected ZArchitecture.GUI.ZTabControl BalanceRulesAccessTabControl;
		protected ZArchitecture.GUI.ZTabPage BalanceTabPage;
		protected ZArchitecture.GUI.ZTabPage RulesTabPage;
		protected ZArchitecture.GUI.ZTabPage AccessTabPage;
		private CusGuaranteeRuleUserControl GuaranteeUserControl;
		private GuaranteeAccessLineControl guaranteeAccessLineControl;
		protected ZArchitecture.GUI.ZTabPage AdditionalReferencesTabPage;
		protected ZArchitecture.GUI.ZTabPage GuaranteeMessagesTabPage;
		protected MessagesUserControl GuaranteeMessagesUserControl;
		private ZArchitecture.ZGrid zGrid1;
		GuaranteeDetailsUserControl GuaranteeDetailsUserControl;
	}
}
