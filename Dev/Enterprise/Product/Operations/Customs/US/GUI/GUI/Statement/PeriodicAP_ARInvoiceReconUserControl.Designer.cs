namespace Enterprise.Customs.US.GUI.Statement
{
	partial class PeriodicAP_ARInvoiceReconUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.FilterByPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FilterByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FilterByLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AccIntegrationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AccIntegrationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MakePaymentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PostAPInvoicesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PostARCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EntriesGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DailyStatementsModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterByPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.AccIntegrationPanel.SuspendLayout();
			this.EntriesGridPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusStatementHeader);
			// 
			// FilterByPanel
			// 
			this.FilterByPanel.Controls.Add(this.FilterByDropEdit);
			this.FilterByPanel.Controls.Add(this.FilterByLabel);
			this.FilterByPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FilterByPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterByPanel.Name = "FilterByPanel";
			this.FilterByPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 27, true);
			this.FilterByPanel.TabIndex = 3;
			// 
			// FilterByDropEdit
			// 
			this.FilterByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FilterByDropEdit, "FilterStatementLinesBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).FilterStatementLinesBy)));
			this.FilterByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 3, true);
			this.FilterByDropEdit.Name = "FilterByDropEdit";
			this.FilterByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 20, true);
			this.FilterByDropEdit.TabIndex = 3;
			// 
			// FilterByLabel
			// 
			this.FilterByLabel.AutoSize = true;
			this.FilterByLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 6, true);
			this.FilterByLabel.Name = "FilterByLabel";
			this.FilterByLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.FilterByLabel.TabIndex = 2;
			this.FilterByLabel.Text = "Filter By:";
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.AccIntegrationPanel);
			this.BottomPanel.Controls.Add(this.EntriesGridPanel);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 550, true);
			this.BottomPanel.TabIndex = 4;
			// 
			// AccIntegrationPanel
			// 
			this.AccIntegrationPanel.Controls.Add(this.AccIntegrationButton);
			this.AccIntegrationPanel.Controls.Add(this.MakePaymentCheckBox);
			this.AccIntegrationPanel.Controls.Add(this.PostAPInvoicesCheckBox);
			this.AccIntegrationPanel.Controls.Add(this.PostARCheckBox);
			this.AccIntegrationPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AccIntegrationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 513, true);
			this.AccIntegrationPanel.Name = "AccIntegrationPanel";
			this.AccIntegrationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 37, true);
			this.AccIntegrationPanel.TabIndex = 2;
			// 
			// AccIntegrationButton
			// 
			this.AccIntegrationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 7, true);
			this.AccIntegrationButton.Name = "AccIntegrationButton";
			this.AccIntegrationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.AccIntegrationButton.TabIndex = 4;
			this.AccIntegrationButton.Text = "Perform Acc Integration";
			this.AccIntegrationButton.UseVisualStyleBackColor = true;
			this.AccIntegrationButton.Click += new System.EventHandler(this.AccIntegrationButton_Click);
			// 
			// MakePaymentCheckBox
			// 
			this.MakePaymentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MakePaymentCheckBox, "MakePayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).MakePayment)));
			this.MakePaymentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MakePaymentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 10, true);
			this.MakePaymentCheckBox.Name = "MakePaymentCheckBox";
			this.MakePaymentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.MakePaymentCheckBox.TabIndex = 3;
			this.MakePaymentCheckBox.Text = "Make Payment";
			this.MakePaymentCheckBox.UseVisualStyleBackColor = true;
			// 
			// PostAPInvoicesCheckBox
			// 
			this.PostAPInvoicesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PostAPInvoicesCheckBox, "PostAPInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).PostAPInvoices)));
			this.PostAPInvoicesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PostAPInvoicesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 10, true);
			this.PostAPInvoicesCheckBox.Name = "PostAPInvoicesCheckBox";
			this.PostAPInvoicesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 17, true);
			this.PostAPInvoicesCheckBox.TabIndex = 2;
			this.PostAPInvoicesCheckBox.Text = "Post AP Invoices";
			this.PostAPInvoicesCheckBox.UseVisualStyleBackColor = true;
			// 
			// PostARCheckBox
			// 
			this.PostARCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PostARCheckBox, "PostARInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).PostARInvoices)));
			this.PostARCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PostARCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 10, true);
			this.PostARCheckBox.Name = "PostARCheckBox";
			this.PostARCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 17, true);
			this.PostARCheckBox.TabIndex = 1;
			this.PostARCheckBox.Text = "Post AR Invoices";
			this.PostARCheckBox.UseVisualStyleBackColor = true;
			// 
			// EntriesGridPanel
			// 
			this.EntriesGridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.EntriesGridPanel.Controls.Add(this.DailyStatementsModuleButtonGrid);
			this.EntriesGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntriesGridPanel.Name = "EntriesGridPanel";
			this.EntriesGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 513, true);
			this.EntriesGridPanel.TabIndex = 1;
			// 
			// DailyStatementsModuleButtonGrid
			// 
			this.DailyStatementsModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DailyStatementsModuleButtonGrid, "DailyStatementsForAccountingRecon");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).DailyStatementsForAccountingRecon)));
			zTextBoxColumnStyleInfo1.Caption = "Statement #";
			zTextBoxColumnStyleInfo1.ColumnName = "B2_StatementNumber";
			zTextBoxColumnStyleInfo2.Caption = "PUN";
			zTextBoxColumnStyleInfo2.ColumnName = "B2_AccountNo";
			zTextBoxColumnStyleInfo3.Caption = "Pay Party";
			zTextBoxColumnStyleInfo3.ColumnName = "B2_PaymentParty";
			zTextBoxColumnStyleInfo4.Caption = "Pay Status";
			zTextBoxColumnStyleInfo4.ColumnName = "B2_PaymentStatus";
			zTextBoxColumnStyleInfo5.Caption = "Pay Type";
			zTextBoxColumnStyleInfo5.ColumnName = "B2_PaymentType";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Customs Amount";
			zCalcEditColumnStyleInfo1.ColumnName = "FinalTotalAmountDue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Broker Pay Amt";
			zCalcEditColumnStyleInfo2.ColumnName = "BrokerPaymentAmount";
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "AP Posted";
			zCalcEditColumnStyleInfo3.ColumnName = "APPostedAmount";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("fcdb46cc-9b64-4272-8838-d7bf502ad58b", "AP");
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "AP Unposted";
			zCalcEditColumnStyleInfo4.ColumnName = "APUnPostedAmount";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.US.GUI.Res.GetData("fcdb46cc-9b64-4272-8838-d7bf502ad58b", "AP");
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = "AP Total";
			zCalcEditColumnStyleInfo5.ColumnName = "APTotalAmount";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Customs.US.GUI.Res.GetData("fcdb46cc-9b64-4272-8838-d7bf502ad58b", "AP");
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = "Diff";
			zCalcEditColumnStyleInfo6.ColumnName = "DifferenceBetweenAPInvoiceAndCustomsAmount";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.US.GUI.Res.GetData("fcdb46cc-9b64-4272-8838-d7bf502ad58b", "AP");
			zCheckBoxColumnStyleInfo1.Caption = "AP Fully Paid";
			zCheckBoxColumnStyleInfo1.ColumnName = "APFullyPaid";
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.Caption = "AR Posted";
			zCalcEditColumnStyleInfo7.ColumnName = "ARPostedAmount";
			zCalcEditColumnStyleInfo7.GroupName = Enterprise.Customs.US.GUI.Res.GetData("eab991fc-1756-415d-8b61-84b855a6a0b3", "AR");
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.Caption = "AR Unposted";
			zCalcEditColumnStyleInfo8.ColumnName = "ARUnPostedAmount";
			zCalcEditColumnStyleInfo8.GroupName = Enterprise.Customs.US.GUI.Res.GetData("eab991fc-1756-415d-8b61-84b855a6a0b3", "AR");
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.Caption = "AR Total";
			zCalcEditColumnStyleInfo9.ColumnName = "ARTotalAmount";
			zCalcEditColumnStyleInfo9.GroupName = Enterprise.Customs.US.GUI.Res.GetData("eab991fc-1756-415d-8b61-84b855a6a0b3", "AR");
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.Caption = "Diff";
			zCalcEditColumnStyleInfo10.ColumnName = "DifferenceBetweenARInvoiceAndCustomsAmount";
			zCalcEditColumnStyleInfo10.GroupName = Enterprise.Customs.US.GUI.Res.GetData("eab991fc-1756-415d-8b61-84b855a6a0b3", "AR");
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.DailyStatementsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.DailyStatementsModuleButtonGrid.GridId = "e7b9a2c6-bef3-4f29-98ef-fb8a057f509b";
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
			this.DailyStatementsModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 475, true);
			this.DailyStatementsModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.DailyStatementsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DailyStatementsModuleButtonGrid.Name = "DailyStatementsModuleButtonGrid";
			this.DailyStatementsModuleButtonGrid.NameOfAGridElement = Enterprise.Customs.US.GUI.Res.GetData("2E1D73CC-D02C-44BC-ABF7-B434D7225436", "Statement");
			this.DailyStatementsModuleButtonGrid.ReadOnly = true;
			this.DailyStatementsModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.DailyStatementsModuleButtonGrid.ShowAttachButton = false;
			this.DailyStatementsModuleButtonGrid.ShowDetachButton = false;
			this.DailyStatementsModuleButtonGrid.ShowNewButton = false;
			this.DailyStatementsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 513, true);
			this.DailyStatementsModuleButtonGrid.TabIndex = 0;
			// 
			// PeriodicAP_ARInvoiceReconUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.FilterByPanel);
			this.Name = "PeriodicAP_ARInvoiceReconUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 577, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterByPanel.ResumeLayout(false);
			this.FilterByPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.AccIntegrationPanel.ResumeLayout(false);
			this.AccIntegrationPanel.PerformLayout();
			this.EntriesGridPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel FilterByPanel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit FilterByDropEdit;
		private Enterprise.ZArchitecture.ZLabel FilterByLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel AccIntegrationPanel;
		internal Enterprise.ZArchitecture.GUI.ZButton AccIntegrationButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox MakePaymentCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PostAPInvoicesCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PostARCheckBox;
		private Enterprise.ZArchitecture.GUI.ZPanel EntriesGridPanel;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid DailyStatementsModuleButtonGrid;

	}
}
