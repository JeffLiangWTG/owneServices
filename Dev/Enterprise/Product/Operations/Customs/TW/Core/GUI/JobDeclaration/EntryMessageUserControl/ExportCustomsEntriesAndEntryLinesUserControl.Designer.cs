namespace Enterprise.Customs.TW.GUI
{
	partial class ExportCustomsEntriesAndEntryLinesUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TotalInvoiceAmountInInvoiceCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.BusinessTaxBaseAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCashTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxPanel.SuspendLayout();
			this.DeclarationCalculationsGroupBox.SuspendLayout();
			this.DeclarationCalculationsPanel.SuspendLayout();
			this.FeesGroupBoxPanel.SuspendLayout();
			this.TotalCustomsValueInLocalCurrencyControl.SuspendLayout();
			this.TotalCustomsValueInInvoiceCurrencyControl.SuspendLayout();
			this.TotalInternationalFreightAmountInInvoiceCurrencyControl.SuspendLayout();
			this.TotalInternationalInsuranceAmountInInvoiceCurrencyControl.SuspendLayout();
			this.TotalAdditionsInInvoiceCurrencyControl.SuspendLayout();
			this.TotalDeductionsInInvoiceCurrencyControl.SuspendLayout();
			this.EntriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).BeginInit();
			this.EntriesBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).BeginInit();
			this.MainHorizontalSplitContainer.Panel1.SuspendLayout();
			this.MainHorizontalSplitContainer.Panel2.SuspendLayout();
			this.MainHorizontalSplitContainer.SuspendLayout();
			this.EntryLinesMessagesTabControl.SuspendLayout();
			this.EntryLinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
			this.EntryLineGrid.SuspendLayout();
			this.MessageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
			this.TopVerticalSplitContainer.Panel1.SuspendLayout();
			this.TopVerticalSplitContainer.Panel2.SuspendLayout();
			this.TopVerticalSplitContainer.SuspendLayout();
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TotalInvoiceAmountInInvoiceCurrencyControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// TaxPanel
			// 
			this.TaxPanel.Controls.Add(this.BusinessTaxBaseAmountCalcEdit);
			this.TaxPanel.Controls.Add(this.TotalCashTaxAmountCalcEdit);
			this.TaxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 238, true);
			this.TaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 55, true);
			// 
			// DeclarationCalculationsGroupBox
			// 
			this.DeclarationCalculationsGroupBox.Controls.Add(this.TotalInvoiceAmountInInvoiceCurrencyControl);
			this.DeclarationCalculationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 235, true);
			this.DeclarationCalculationsGroupBox.Controls.SetChildIndex(this.TotalDeductionsInInvoiceCurrencyControl, 0);
			this.DeclarationCalculationsGroupBox.Controls.SetChildIndex(this.TotalAdditionsInInvoiceCurrencyControl, 0);
			this.DeclarationCalculationsGroupBox.Controls.SetChildIndex(this.TotalInternationalInsuranceAmountInInvoiceCurrencyControl, 0);
			this.DeclarationCalculationsGroupBox.Controls.SetChildIndex(this.TotalInternationalFreightAmountInInvoiceCurrencyControl, 0);
			this.DeclarationCalculationsGroupBox.Controls.SetChildIndex(this.TotalCustomsValueInInvoiceCurrencyControl, 0);
			this.DeclarationCalculationsGroupBox.Controls.SetChildIndex(this.TotalCustomsValueInLocalCurrencyControl, 0);
			this.DeclarationCalculationsGroupBox.Controls.SetChildIndex(this.TotalInvoiceAmountInInvoiceCurrencyControl, 0);
			// 
			// DeclarationCalculationsPanel
			// 
			this.DeclarationCalculationsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 241, true);
			this.DeclarationCalculationsPanel.Visible = true;
			// 
			// FeesGroupBoxPanel
			// 
			this.FeesGroupBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(787, 0, true);
			this.FeesGroupBoxPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 307, true);
			this.FeesGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 307, true);
			// 
			// TotalCustomsValueInLocalCurrencyControl
			// 
			this.TotalCustomsValueInLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 180, true);
			// 
			// TotalCustomsValueInInvoiceCurrencyControl
			// 
			this.TotalCustomsValueInInvoiceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 154, true);
			// 
			// TotalInternationalFreightAmountInInvoiceCurrencyControl
			// 
			this.TotalInternationalFreightAmountInInvoiceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 50, true);
			// 
			// TotalInternationalInsuranceAmountInInvoiceCurrencyControl
			// 
			this.TotalInternationalInsuranceAmountInInvoiceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 76, true);
			// 
			// TotalAdditionsInInvoiceCurrencyControl
			// 
			this.TotalAdditionsInInvoiceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 102, true);
			// 
			// TotalDeductionsInInvoiceCurrencyControl
			// 
			this.TotalDeductionsInInvoiceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 128, true);
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 9, true);
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 0, true);
			// 
			// MainHorizontalSplitContainer
			// 
			this.MainHorizontalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 551, true);
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 307, true);
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 280, true);
			// 
			// EntryLineGrid
			// 
			zTextBoxColumnStyleInfo2.ColumnName = "CL_BondedGoodsCode";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 0, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, -15, true);
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 117, true);
			// 
			// MessageTabPage
			// 
			this.MessageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 280, true);
			// 
			// TopVerticalSplitContainer
			// 
			this.TopVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 241, true);
			this.TopVerticalSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(852);
			this.TopVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(852);
			// 
			// BaseMessageUserControl
			// 
			this.BaseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 274, true);
			// 
			// TotalInvoiceAmountInInvoiceCurrencyControl
			// 
			this.TotalInvoiceAmountInInvoiceCurrencyControl.AllowDrop = true;
			this.TotalInvoiceAmountInInvoiceCurrencyControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalInvoiceAmountInInvoiceCurrencyControl.BindToAmount = "CustomsEntryHeaders.CH_TotalEXPDisbursedAmountInInvoiceCurrency";
			this.TotalInvoiceAmountInInvoiceCurrencyControl.BindToList = "CustomsEntryHeaders.Lookups+CurrencyList";
			this.TotalInvoiceAmountInInvoiceCurrencyControl.BindToUnit = "CustomsEntryHeaders.FirstInvoiceCurrencyCode";
			this.TotalInvoiceAmountInInvoiceCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TotalInvoiceAmountInInvoiceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 25, true);
			this.TotalInvoiceAmountInInvoiceCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.TotalInvoiceAmountInInvoiceCurrencyControl.Name = "TotalInvoiceAmountInInvoiceCurrencyControl";
			this.TotalInvoiceAmountInInvoiceCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TotalInvoiceAmountInInvoiceCurrencyControl.TabIndex = 0;
			// 
			// BusinessTaxBaseAmountCalcEdit
			// 
			this.BusinessTaxBaseAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BusinessTaxBaseAmountCalcEdit, "CustomsEntryHeaders.BusinessTaxBaseAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).BusinessTaxBaseAmount)));
			this.BusinessTaxBaseAmountCalcEdit.DecimalPlaces = 0;
			this.BusinessTaxBaseAmountCalcEdit.Decimals = 0;
			this.BusinessTaxBaseAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 4, true);
			this.BusinessTaxBaseAmountCalcEdit.Name = "BusinessTaxBaseAmountCalcEdit";
			this.BusinessTaxBaseAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.BusinessTaxBaseAmountCalcEdit.TabIndex = 5;
			this.BusinessTaxBaseAmountCalcEdit.Text = "0";
			this.BusinessTaxBaseAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.BusinessTaxBaseAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalCashTaxAmountCalcEdit
			// 
			this.TotalCashTaxAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalCashTaxAmountCalcEdit, "CustomsEntryHeaders.TotalCashTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalCashTaxAmount)));
			this.TotalCashTaxAmountCalcEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("b7f1f439-39fe-4d78-aa12-4e3cc34170fa", "Total Tax Amount", "The total amount of the duties, taxes and fees.");
			this.TotalCashTaxAmountCalcEdit.DecimalPlaces = 0;
			this.TotalCashTaxAmountCalcEdit.Decimals = 0;
			this.TotalCashTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 30, true);
			this.TotalCashTaxAmountCalcEdit.Name = "TotalCashTaxAmountCalcEdit";
			this.TotalCashTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalCashTaxAmountCalcEdit.TabIndex = 6;
			this.TotalCashTaxAmountCalcEdit.Text = "0";
			this.TotalCashTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCashTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// ExportCustomsEntriesAndEntryLinesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ExportCustomsEntriesAndEntryLinesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 551, true);
			this.TaxPanel.ResumeLayout(false);
			this.TaxPanel.PerformLayout();
			this.DeclarationCalculationsGroupBox.ResumeLayout(false);
			this.DeclarationCalculationsGroupBox.PerformLayout();
			this.DeclarationCalculationsPanel.ResumeLayout(false);
			this.DeclarationCalculationsPanel.PerformLayout();
			this.FeesGroupBoxPanel.ResumeLayout(false);
			this.FeesGroupBoxPanel.PerformLayout();
			this.TotalCustomsValueInLocalCurrencyControl.ResumeLayout(true);
			this.TotalCustomsValueInLocalCurrencyControl.PerformLayout();
			this.TotalCustomsValueInInvoiceCurrencyControl.ResumeLayout(true);
			this.TotalCustomsValueInInvoiceCurrencyControl.PerformLayout();
			this.TotalInternationalFreightAmountInInvoiceCurrencyControl.ResumeLayout(true);
			this.TotalInternationalFreightAmountInInvoiceCurrencyControl.PerformLayout();
			this.TotalInternationalInsuranceAmountInInvoiceCurrencyControl.ResumeLayout(true);
			this.TotalInternationalInsuranceAmountInInvoiceCurrencyControl.PerformLayout();
			this.TotalAdditionsInInvoiceCurrencyControl.ResumeLayout(true);
			this.TotalAdditionsInInvoiceCurrencyControl.PerformLayout();
			this.TotalDeductionsInInvoiceCurrencyControl.ResumeLayout(true);
			this.TotalDeductionsInInvoiceCurrencyControl.PerformLayout();
			this.EntriesGroupBox.ResumeLayout(false);
			this.EntriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).EndInit();
			this.EntriesBoundGrid.ResumeLayout(false);
			this.EntriesBoundGrid.PerformLayout();
			this.MainHorizontalSplitContainer.Panel1.ResumeLayout(false);
			this.MainHorizontalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).EndInit();
			this.MainHorizontalSplitContainer.ResumeLayout(false);
			this.MainHorizontalSplitContainer.PerformLayout();
			this.EntryLinesMessagesTabControl.ResumeLayout(false);
			this.EntryLinesMessagesTabControl.PerformLayout();
			this.EntryLinesTabPage.ResumeLayout(false);
			this.EntryLinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
			this.EntryLineGrid.ResumeLayout(false);
			this.EntryLineGrid.PerformLayout();
			this.MessageTabPage.ResumeLayout(false);
			this.MessageTabPage.PerformLayout();
			this.TopVerticalSplitContainer.Panel1.ResumeLayout(false);
			this.TopVerticalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
			this.TopVerticalSplitContainer.ResumeLayout(false);
			this.TopVerticalSplitContainer.PerformLayout();
			this.BaseMessageUserControl.ResumeLayout(true);
			this.BaseMessageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TotalInvoiceAmountInInvoiceCurrencyControl.ResumeLayout(true);
			this.TotalInvoiceAmountInInvoiceCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		Customs.GUI.ConvertToLocalCurrencyControl TotalInvoiceAmountInInvoiceCurrencyControl;
		private ZArchitecture.ZCalcEdit BusinessTaxBaseAmountCalcEdit;
		private ZArchitecture.ZCalcEdit TotalCashTaxAmountCalcEdit;
	}
}
