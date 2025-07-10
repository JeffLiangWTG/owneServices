
using Enterprise.MasterFiles.GUI;
namespace Enterprise.Customs.US.GUI
{
	partial class StatementForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.StatementHeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.statementHeaderDetailsUserControl2 = new Enterprise.Customs.US.GUI.StatementHeaderDetailsUserControl();
			this.ChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid2 = new Enterprise.ZArchitecture.ZGrid();
			this.EntriesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatementLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.statementMessagesUserControl1 = new Enterprise.Customs.US.GUI.StatementMessagesUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.AccountingReconTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AP_ARInvoiceReconciliationUserControl = new Enterprise.Customs.US.GUI.AP_ARInvoiceReconciliationUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatementHeaderGroupBox.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).BeginInit();
			this.EntriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StatementLinesGrid)).BeginInit();
			this.MessagesTabPage.SuspendLayout();
			this.AccountingReconTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 571, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.AccountingReconTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 598, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AccountingReconTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.EntriesGroupBox);
			this.MainTabPage.Controls.Add(this.ChargesGroupBox);
			this.MainTabPage.Controls.Add(this.StatementHeaderGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 571, true);
			this.MainTabPage.Text = "Details";
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusStatementHeader);
			// 
			// StatementHeaderGroupBox
			// 
			this.StatementHeaderGroupBox.Controls.Add(this.statementHeaderDetailsUserControl2);
			this.StatementHeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.StatementHeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatementHeaderGroupBox.Name = "StatementHeaderGroupBox";
			this.StatementHeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 173, true);
			this.StatementHeaderGroupBox.TabIndex = 1;
			this.StatementHeaderGroupBox.TabStop = false;
			this.StatementHeaderGroupBox.Text = "Details";
			// 
			// statementHeaderDetailsUserControl2
			// 
			this.statementHeaderDetailsUserControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statementHeaderDetailsUserControl2, ".");
			this.statementHeaderDetailsUserControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statementHeaderDetailsUserControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.statementHeaderDetailsUserControl2.Name = "statementHeaderDetailsUserControl2";
			this.statementHeaderDetailsUserControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 154, true);
			this.statementHeaderDetailsUserControl2.TabIndex = 0;
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Controls.Add(this.zGrid2);
			this.ChargesGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 471, true);
			this.ChargesGroupBox.Name = "ChargesGroupBox";
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 100, true);
			this.ChargesGroupBox.TabIndex = 3;
			this.ChargesGroupBox.TabStop = false;
			this.ChargesGroupBox.Text = "Entry Charges";
			// 
			// zGrid2
			// 
			this.zGrid2.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid2, "StatementLines.Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)).SyncRoot)).ChargeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)).SyncRoot)).B4_ChargeAmount)));
			this.zGrid2.CaptionVisible = false;
			zTextBoxColumnStyleInfo11.Caption = "Charge Type";
			zTextBoxColumnStyleInfo11.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Charge Amount";
			zCalcEditColumnStyleInfo2.ColumnName = "B4_ChargeAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.zGrid2.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zGrid2.CopySelectedRowsAllowed = true;
			this.zGrid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid2.GridId = "dad4bca7-4882-48d9-b6a4-280961d02c9e";
			this.zGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid2.LayoutKey = "zGrid1";
			this.zGrid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid2.Name = "zGrid2";
			this.zGrid2.ReadOnly = true;
			this.zGrid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 81, true);
			this.zGrid2.TabIndex = 0;
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Controls.Add(this.StatementLinesGrid);
			this.EntriesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
			this.EntriesGroupBox.Name = "EntriesGroupBox";
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 298, true);
			this.EntriesGroupBox.TabIndex = 2;
			this.EntriesGroupBox.TabStop = false;
			this.EntriesGroupBox.Text = "Entries";
			// 
			// zGrid1
			// 
			this.StatementLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StatementLinesGrid, "StatementLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).DeclarationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_EntryProcessPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_EntryFilerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).EntryStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).LineStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_CustomsFeesTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_DeletedByParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).IsDeferredTaxIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).IsElectronicInvoiceRequired)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_Team)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).ReleaseStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).ReleaseStatusDescription)));
			this.StatementLinesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.Caption = "Job Number";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "DeclarationPK";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.ShowNewFormWhenEmpty = false;
			zTextBoxColumnStyleInfo1.Caption = "Entry Processing Port";
			zTextBoxColumnStyleInfo1.ColumnName = "B3_EntryProcessPort";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo2.Caption = "Filer";
			zTextBoxColumnStyleInfo2.ColumnName = "B3_EntryFilerCode";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("StatementForm|db10d265-dc19-4f9f-95c2-b18f99377294", "Entry Number");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo3.Caption = "Entry Number";
			zTextBoxColumnStyleInfo3.ColumnName = "B3_EntryNum";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("StatementForm|db10d265-dc19-4f9f-95c2-b18f99377294", "Entry Number");
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = "Entry Type";
			zTextBoxColumnStyleInfo4.ColumnName = "B3_EntryType";
			zTextBoxColumnStyleInfo5.Caption = "Entry Status";
			zTextBoxColumnStyleInfo5.ColumnName = "EntryStatusDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.Caption = "Line Status";
			zTextBoxColumnStyleInfo6.ColumnName = "LineStatusDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Customs Fees Total";
			zCalcEditColumnStyleInfo1.ColumnName = "B3_CustomsFeesTotal";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.Caption = "Deleted By Party";
			zTextBoxColumnStyleInfo7.ColumnName = "B3_DeletedByParty";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.Caption = "Tax Deferred";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsDeferredTaxIndicator";
			zCheckBoxColumnStyleInfo2.Caption = "EI Required";
			zCheckBoxColumnStyleInfo2.ColumnName = "IsElectronicInvoiceRequired";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f2b0e059-b997-4393-876a-965853e4b4e2", "FIS Team");
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "B3_Team";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("98df7503-d2d9-4a4c-9414-6a9e1e1d6b5c", "Release Status");
			zTextBoxColumnStyleInfo9.ColumnName = "ReleaseStatus";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e5deed18-7669-42c8-860c-c4268cda86f1", "Release Status Description");
			zTextBoxColumnStyleInfo10.ColumnName = "ReleaseStatusDescription";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.StatementLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.StatementLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.StatementLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.StatementLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.StatementLinesGrid.CopySelectedRowsAllowed = true;
			this.StatementLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatementLinesGrid.GridId = "30c1858f-559e-49ad-bd4f-cd12e9da61f0";
			this.StatementLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StatementLinesGrid.LayoutKey = "zGrid1";
			this.StatementLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.StatementLinesGrid.Name = "zGrid1";
			this.StatementLinesGrid.ReadOnly = true;
			this.StatementLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 279, true);
			this.StatementLinesGrid.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MessagesTabPage.Controls.Add(this.statementMessagesUserControl1);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 452, true);
			this.MessagesTabPage.TabIndex = 3;
			this.MessagesTabPage.Text = "Messages";
			// 
			// statementMessagesUserControl1
			// 
			this.statementMessagesUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statementMessagesUserControl1, ".");
			this.statementMessagesUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statementMessagesUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.statementMessagesUserControl1.Name = "statementMessagesUserControl1";
			this.statementMessagesUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 446, true);
			this.statementMessagesUserControl1.TabIndex = 7;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 452, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// AccountingReconTabPage
			// 
			this.AccountingReconTabPage.Controls.Add(this.AP_ARInvoiceReconciliationUserControl);
			this.AccountingReconTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AccountingReconTabPage.Name = "AccountingReconTabPage";
			this.AccountingReconTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AccountingReconTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 452, true);
			this.AccountingReconTabPage.TabIndex = 5;
			this.AccountingReconTabPage.Text = "Accounting Recon";
			// 
			// AP_ARInvoiceReconciliationUserControl
			// 
			this.AP_ARInvoiceReconciliationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AP_ARInvoiceReconciliationUserControl, ".");
			this.AP_ARInvoiceReconciliationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AP_ARInvoiceReconciliationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AP_ARInvoiceReconciliationUserControl.Name = "AP_ARInvoiceReconciliationUserControl";
			this.AP_ARInvoiceReconciliationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 446, true);
			this.AP_ARInvoiceReconciliationUserControl.TabIndex = 0;
			// 
			// StatementForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 654, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.CusStatementHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 692, true);
			this.Name = "StatementForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "DailyStatementForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatementHeaderGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).EndInit();
			this.EntriesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.StatementLinesGrid)).EndInit();
			this.MessagesTabPage.ResumeLayout(false);
			this.AccountingReconTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox StatementHeaderGroupBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox EntriesGroupBox;
		internal Enterprise.ZArchitecture.ZGrid StatementLinesGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ChargesGroupBox;
		private Enterprise.ZArchitecture.ZGrid zGrid2;
		private StatementMessagesUserControl statementMessagesUserControl1;
		private ZWorkflowTabPage WorkflowTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage AccountingReconTabPage;
		internal Enterprise.Customs.US.GUI.AP_ARInvoiceReconciliationUserControl AP_ARInvoiceReconciliationUserControl;
		private StatementHeaderDetailsUserControl statementHeaderDetailsUserControl2;
	}
}
