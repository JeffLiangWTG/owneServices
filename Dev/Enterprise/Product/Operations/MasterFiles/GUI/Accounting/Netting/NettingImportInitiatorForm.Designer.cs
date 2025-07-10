namespace Enterprise.MasterFiles.GUI.Accounting.Netting
{
	partial class NettingImportInitiatorForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PaymentStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ledgerDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.importEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.importStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.nettingCentreDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.importNettingCentreTransactionsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.saveButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.activityTrailTextBox = new CargoWise.Windows.UI.KRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainPanel.SuspendLayout();
			this.PaymentStatus.SuspendLayout();
			this.ledgerDropEdit1.SuspendLayout();
			this.importEndDateEdit.SuspendLayout();
			this.importStartDateEdit.SuspendLayout();
			this.nettingCentreDropEdit.SuspendLayout();
			this.saveButtonsPanel.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 469, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.Accounting.Netting.NettingImportInitiator);
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.PaymentStatus);
			this.mainPanel.Controls.Add(this.ledgerDropEdit1);
			this.mainPanel.Controls.Add(this.importEndDateEdit);
			this.mainPanel.Controls.Add(this.importStartDateEdit);
			this.mainPanel.Controls.Add(this.nettingCentreDropEdit);
			this.mainPanel.Controls.Add(this.importNettingCentreTransactionsCheckBox);
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 161, true);
			this.mainPanel.TabIndex = 1;
			// 
			// PaymentStatus
			// 
			this.PaymentStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentStatus, "SettlementStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingImportInitiator)(null)).SettlementStatus)));
			this.PaymentStatus.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8f4438fa-fe1d-43d3-80dd-8d6bf6f73c2b", "Payment Status");
			this.PaymentStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 55, true);
			this.PaymentStatus.Name = "PaymentStatus";
			this.PaymentStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 17, true);
			this.PaymentStatus.TabIndex = 7;
			// 
			// ledgerDropEdit1
			// 
			this.ledgerDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ledgerDropEdit1, "Ledger");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingImportInitiator)(null)).Ledger)));
			this.ledgerDropEdit1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a6c4c5c4-055c-43b2-a90b-a2e23d0c0f1c", "Ledger");
			this.ledgerDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 34, true);
			this.ledgerDropEdit1.Name = "ledgerDropEdit1";
			this.ledgerDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 17, true);
			this.ledgerDropEdit1.TabIndex = 6;
			// 
			// importEndDateEdit
			// 
			this.importEndDateEdit.AllowDrop = true;
			this.importEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.importEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.importEndDateEdit, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingImportInitiator)(null)).EndDate)));
			this.importEndDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2792eb24-33db-4a7d-9ab7-745385080305", "Import End Date");
			this.importEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 98, true);
			this.importEndDateEdit.Name = "importEndDateEdit";
			this.importEndDateEdit.TabIndex = 9;
			// 
			// importStartDateEdit
			// 
			this.importStartDateEdit.AllowDrop = true;
			this.importStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.importStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.importStartDateEdit, "StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingImportInitiator)(null)).StartDate)));
			this.importStartDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c1a1c1c3-cfa3-412e-b1a0-108ef9145da6", "Import Start Date");
			this.importStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 77, true);
			this.importStartDateEdit.Name = "importStartDateEdit";
			this.importStartDateEdit.TabIndex = 8;
			// 
			// nettingCentreDropEdit
			// 
			this.nettingCentreDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.nettingCentreDropEdit, "NettingSystemCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingImportInitiator)(null)).NettingSystemCode)));
			this.nettingCentreDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("99fd459d-9fc8-426b-9755-fe2ae3e06aa9", "Netting System");
			this.nettingCentreDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 13, true);
			this.nettingCentreDropEdit.Name = "nettingCentreDropEdit";
			this.nettingCentreDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 17, true);
			this.nettingCentreDropEdit.TabIndex = 5;
			// 
			// importNettingCentreTransactionsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.importNettingCentreTransactionsCheckBox, "IncludeTransactionsForNettingCentre");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingImportInitiator)(null)).IncludeTransactionsForNettingCentre)));
			this.importNettingCentreTransactionsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dce3cb11-ef2e-4d35-a998-1b5a73dfcf73", "Is Netting Center part of Netting Group");
			this.importNettingCentreTransactionsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.importNettingCentreTransactionsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 125, true);
			this.importNettingCentreTransactionsCheckBox.Name = "importNettingCentreTransactionsCheckBox";
			this.importNettingCentreTransactionsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 24, true);
			this.importNettingCentreTransactionsCheckBox.TabIndex = 10;
			// 
			// saveButtonsPanel
			// 
			this.saveButtonsPanel.Controls.Add(this.ButtonsUserControl);
			this.saveButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.saveButtonsPanel.Name = "saveButtonsPanel";
			this.saveButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 37, true);
			this.saveButtonsPanel.TabIndex = 10;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 12, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 25, true);
			this.ButtonsUserControl.TabIndex = 0;
			// 
			// activityTrailTextBox
			// 
			this.activityTrailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 179, true);
			this.activityTrailTextBox.MaxLength = 10000000;
			this.activityTrailTextBox.Name = "activityTrailTextBox";
			this.activityTrailTextBox.ReadOnly = true;
			this.activityTrailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 243, true);
			this.activityTrailTextBox.TabIndex = 9;
			this.activityTrailTextBox.Text = "";
			// 
			// NettingImportInitiatorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("A66953D4-BA40-49F3-AF2F-D40B63438156", "Queue Transactions For Netting");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 493, true);
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.activityTrailTextBox);
			this.Controls.Add(this.saveButtonsPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.Accounting.Netting.NettingImportInitiator);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 530, true);
			this.Name = "NettingImportInitiatorForm";
			this.Text = "QueueTransactionsForm";
			this.Controls.SetChildIndex(this.saveButtonsPanel, 0);
			this.Controls.SetChildIndex(this.activityTrailTextBox, 0);
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.PaymentStatus.ResumeLayout(true);
			this.PaymentStatus.PerformLayout();
			this.ledgerDropEdit1.ResumeLayout(true);
			this.ledgerDropEdit1.PerformLayout();
			this.importEndDateEdit.ResumeLayout(true);
			this.importEndDateEdit.PerformLayout();
			this.importStartDateEdit.ResumeLayout(true);
			this.importStartDateEdit.PerformLayout();
			this.nettingCentreDropEdit.ResumeLayout(true);
			this.nettingCentreDropEdit.PerformLayout();
			this.saveButtonsPanel.ResumeLayout(false);
			this.saveButtonsPanel.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel mainPanel;
		private ZArchitecture.GUI.ZPanel saveButtonsPanel;
		private ZArchitecture.GUI.ZDateEdit importStartDateEdit;
		protected Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private ZArchitecture.GUI.ZDropEdit nettingCentreDropEdit;
		private ZArchitecture.GUI.ZDateEdit importEndDateEdit;
		private ZArchitecture.GUI.ZCheckBox importNettingCentreTransactionsCheckBox;
		private ZArchitecture.GUI.ZDropEdit ledgerDropEdit1;
		private ZArchitecture.GUI.ZDropEdit PaymentStatus;
		private CargoWise.Windows.UI.KRichTextBox activityTrailTextBox;
	}
}
