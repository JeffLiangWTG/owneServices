namespace Enterprise.Customs.GUI
{
	partial class DeclarationInvoiceHeaderUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.InvoiceTabControl.SuspendLayout();
			this.ComInvoiceDetailsTabPage.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ChargesTabControl.SuspendLayout();
			this.InvoiceChargesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.ApportionedTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.BaseGroupChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// JobComInvoiceHeadersBoundGrid
			// 
			this.BindingSource.SetBindingMember(this.JobComInvoiceHeadersBoundGrid, "Invoices");
			// 
			// 
			// 
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.AllowNavigation = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CaptionVisible = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.LayoutKey = "JobComInvoiceHeadersBoundGrid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 114, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
			// 
			// IncoTermTextBox
			// 
			this.BindingSource.SetBindingMember(this.IncoTermTextBox, "Invoices.IncoTerm");
			// 
			// JZ_InvoiceCurrExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JZ_InvoiceCurrExRateCalcEdit, "Invoices.JZ_InvoiceCurrExRate");
			// 
			// GrossWeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// 
			// NetWeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// 
			// JZ_InvoiceNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JZ_InvoiceNumberBoundTextBox, "Invoices.JZ_InvoiceNumber");
			// 
			// JZ_IncoTermBoundDropDownEdit
			// 
			this.BindingSource.SetBindingMember(this.JZ_IncoTermBoundDropDownEdit, "Invoices.JZ_IncoTerm");
			// 
			// GroupInvoiceDropEdit
			// 
			this.BindingSource.SetBindingMember(this.GroupInvoiceDropEdit, "Invoices.JZ_Calc_GroupInvoice");
			// 
			// InvoiceChargesGrid
			// 
			this.BindingSource.SetBindingMember(this.InvoiceChargesGrid, "Invoices.Charges");
			// 
			// ApportionedChargesGrid
			// 
			this.BindingSource.SetBindingMember(this.ApportionedChargesGrid, "Invoices.GroupCharges");
			// 
			// JZ_InvoiceCurrLandedCostExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JZ_InvoiceCurrLandedCostExRateCalcEdit, "Invoices.JZ_InvoiceCurrLandedCostExRate");
			// 
			// BaseGroupChargesGrid
			// 
			this.BindingSource.SetBindingMember(this.BaseGroupChargesGrid, "TopGroupInvoice.Charges");
			// 
			// DeclarationInvoiceHeaderUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "DeclarationInvoiceHeaderUserControl";
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.InvoiceTabControl.ResumeLayout(false);
			this.ComInvoiceDetailsTabPage.ResumeLayout(false);
			this.ComInvoiceDetailsTabPage.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesTabControl.ResumeLayout(false);
			this.InvoiceChargesTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.ApportionedTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.BaseGroupChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
