using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.US.GUI
{
	partial class PeriodicStatementForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.DailyStatementsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DailyStatementsModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.statementMessagesUserControl1 = new Enterprise.Customs.US.GUI.StatementMessagesUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.AccReconTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.periodicAP_ARInvoiceReconUserControl1 = new Enterprise.Customs.US.GUI.Statement.PeriodicAP_ARInvoiceReconUserControl();
			this.StatementHeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.statementHeaderDetailsUserControl1 = new Enterprise.Customs.US.GUI.StatementHeaderDetailsUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DailyStatementsGroupBox.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.AccReconTabPage.SuspendLayout();
			this.StatementHeaderGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 542, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.AccReconTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 569, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AccReconTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.DailyStatementsGroupBox);
			this.MainTabPage.Controls.Add(this.StatementHeaderGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 542, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusStatementHeader);
			// 
			// DailyStatementsGroupBox
			// 
			this.DailyStatementsGroupBox.Controls.Add(this.DailyStatementsModuleButtonGrid);
			this.DailyStatementsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DailyStatementsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 181, true);
			this.DailyStatementsGroupBox.Name = "DailyStatementsGroupBox";
			this.DailyStatementsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 361, true);
			this.DailyStatementsGroupBox.TabIndex = 2;
			this.DailyStatementsGroupBox.TabStop = false;
			this.DailyStatementsGroupBox.Text = "Daily Statements";
			// 
			// DailyStatementsModuleButtonGrid
			// 
			this.DailyStatementsModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DailyStatementsModuleButtonGrid, "DailyStatements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).DailyStatements)));
			zTextBoxColumnStyleInfo1.Caption = "Statement Number";
			zTextBoxColumnStyleInfo1.ColumnName = "B2_StatementNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.Caption = "Due Date";
			zDateEditColumnStyleInfo1.ColumnName = "B2_DueDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Caption = "Print Date";
			zDateEditColumnStyleInfo2.ColumnName = "B2_PrintDate";
			zDateEditColumnStyleInfo3.Caption = "Process Date";
			zDateEditColumnStyleInfo3.ColumnName = "B2_ProcessDate";
			zTextBoxColumnStyleInfo2.Caption = "Payment Type";
			zTextBoxColumnStyleInfo2.ColumnName = "B2_PaymentType";
			zTextBoxColumnStyleInfo3.Caption = "Entry Filer Code";
			zTextBoxColumnStyleInfo3.ColumnName = "B2_EntryFilerCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = "Entry Summary Process Port";
			zTextBoxColumnStyleInfo4.ColumnName = "B2_ProcessPort";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo5.Caption = "Client Branch Designation";
			zTextBoxColumnStyleInfo5.ColumnName = "B2_BranchDesignation";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zOrganisationFindBoxColumnStyleInfo1.Caption = "Importer";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "B2_OH_Importer";
			zDateEditColumnStyleInfo4.Caption = "Payment Authorization Date";
			zDateEditColumnStyleInfo4.ColumnName = "B2_PaymentAuthorizationDate";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.Caption = "Tax Deferred";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsTaxDeferred";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DailyStatementsModuleButtonGrid.GridId = "ce47668c-6ca4-4bf0-b80b-15ba5d04d65f";
			this.DailyStatementsModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DailyStatementsModuleButtonGrid.EditButtonText = Enterprise.Customs.US.GUI.Res.GetData("4C436979-D9BD-40AD-963E-4D8AB49CB96E", "View");
			// 
			// 
			// 
			this.DailyStatementsModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.DailyStatementsModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DailyStatementsModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.DailyStatementsModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.DailyStatementsModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DailyStatementsModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.DailyStatementsModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.DailyStatementsModuleButtonGrid.InnerGrid.Name = "Grid";
			this.DailyStatementsModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 304, true);
			this.DailyStatementsModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.DailyStatementsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DailyStatementsModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.USCustomsStatement;
			this.DailyStatementsModuleButtonGrid.Name = "DailyStatementsModuleButtonGrid";
			this.DailyStatementsModuleButtonGrid.NameOfAGridElement = Enterprise.Customs.US.GUI.Res.GetData("96293A32-B712-4C44-BE56-4B6F4BE96D93", "Daily Statement");
			this.DailyStatementsModuleButtonGrid.ReadOnly = true;
			this.DailyStatementsModuleButtonGrid.ShowAttachButton = false;
			this.DailyStatementsModuleButtonGrid.ShowDetachButton = false;
			this.DailyStatementsModuleButtonGrid.ShowNewButton = false;
			this.DailyStatementsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(918, 342, true);
			this.DailyStatementsModuleButtonGrid.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MessagesTabPage.Controls.Add(this.statementMessagesUserControl1);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 542, true);
			this.MessagesTabPage.TabIndex = 3;
			this.MessagesTabPage.Text = "Messages";
			// 
			// statementMessagesUserControl1
			// 
			this.statementMessagesUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statementMessagesUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.statementMessagesUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statementMessagesUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.statementMessagesUserControl1.Name = "statementMessagesUserControl1";
			this.statementMessagesUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(918, 536, true);
			this.statementMessagesUserControl1.DataSourceType = typeof(Enterprise.Customs.US.Business.CusStatementHeader);
			this.statementMessagesUserControl1.TabIndex = 7;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 542, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// AccReconTabPage
			// 
			this.AccReconTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.AccReconTabPage.Controls.Add(this.periodicAP_ARInvoiceReconUserControl1);
			this.AccReconTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AccReconTabPage.Name = "AccReconTabPage";
			this.AccReconTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AccReconTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 542, true);
			this.AccReconTabPage.TabIndex = 5;
			this.AccReconTabPage.Text = "Accounting Recon";
			// 
			// periodicAP_ARInvoiceReconUserControl1
			// 
			this.periodicAP_ARInvoiceReconUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.periodicAP_ARInvoiceReconUserControl1, ".");
			this.periodicAP_ARInvoiceReconUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.periodicAP_ARInvoiceReconUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.periodicAP_ARInvoiceReconUserControl1.Name = "periodicAP_ARInvoiceReconUserControl1";
			this.periodicAP_ARInvoiceReconUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(918, 536, true);
			this.periodicAP_ARInvoiceReconUserControl1.TabIndex = 0;
			// 
			// StatementHeaderGroupBox
			// 
			this.StatementHeaderGroupBox.Controls.Add(this.statementHeaderDetailsUserControl1);
			this.StatementHeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.StatementHeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatementHeaderGroupBox.Name = "StatementHeaderGroupBox";
			this.StatementHeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 181, true);
			this.StatementHeaderGroupBox.TabIndex = 1;
			this.StatementHeaderGroupBox.TabStop = false;
			this.StatementHeaderGroupBox.Text = "Details";
			// 
			// statementHeaderDetailsUserControl1
			// 
			this.statementHeaderDetailsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statementHeaderDetailsUserControl1, ".");
			this.statementHeaderDetailsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statementHeaderDetailsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.statementHeaderDetailsUserControl1.Name = "statementHeaderDetailsUserControl1";
			this.statementHeaderDetailsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(918, 162, true);
			this.statementHeaderDetailsUserControl1.TabIndex = 0;
			// 
			// PeriodicStatementForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 625, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.CusStatementHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 652, true);
			this.Name = "PeriodicStatementForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "PeriodicMonthlyStatementForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DailyStatementsGroupBox.ResumeLayout(false);
			this.MessagesTabPage.ResumeLayout(false);
			this.AccReconTabPage.ResumeLayout(false);
			this.StatementHeaderGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox DailyStatementsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid DailyStatementsModuleButtonGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private StatementMessagesUserControl statementMessagesUserControl1;
		private ZWorkflowTabPage WorkflowTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage AccReconTabPage;
		internal Enterprise.Customs.US.GUI.Statement.PeriodicAP_ARInvoiceReconUserControl periodicAP_ARInvoiceReconUserControl1;
		private ZArchitecture.GUI.ZGroupBox StatementHeaderGroupBox;
		private StatementHeaderDetailsUserControl statementHeaderDetailsUserControl1;
	}
}
