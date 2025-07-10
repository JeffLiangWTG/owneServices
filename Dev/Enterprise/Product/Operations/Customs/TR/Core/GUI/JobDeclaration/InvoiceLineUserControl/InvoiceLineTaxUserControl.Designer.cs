namespace Enterprise.Customs.TR.GUI
{
	partial class InvoiceLineTaxUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            this.InvoiceLineTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.InvoiceLineTaxGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.InvoiceLineTaxGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InvoiceLineTaxGrid)).BeginInit();
            this.InvoiceLineTaxGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine);
            // 
            // InvoiceLineTaxGroupBox
            // 
            this.InvoiceLineTaxGroupBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("F2F57714-8C37-4F6A-B8C1-239567B4D74C", "Duty And Tax");
            this.InvoiceLineTaxGroupBox.Controls.Add(this.InvoiceLineTaxGrid);
            this.InvoiceLineTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.InvoiceLineTaxGroupBox, true);
            this.InvoiceLineTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.InvoiceLineTaxGroupBox.Name = "InvoiceLineTaxGroupBox";
            this.InvoiceLineTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 314, true);
            this.InvoiceLineTaxGroupBox.TabIndex = 5;
            this.InvoiceLineTaxGroupBox.TabStop = false;
            // 
            // InvoiceLineTaxGrid
            // 
            this.InvoiceLineTaxGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.InvoiceLineTaxGrid, "FilteredInvoiceLines.Taxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).NationalType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_RateOverrideReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_MethodOfPayment)));
			this.InvoiceLineTaxGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("CC543375-069F-4605-997D-36DA48317529", "Type");
            zDropEditColumnStyleInfo1.ColumnName = "JLT_Type";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.TR.GUI.Res.GetData("F6126BC3-F1E6-43F0-A7CC-EB391A456473", "Tariff Type");
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("279866AF-0A48-4610-8957-96E1C7F6FB78", "Duty Type");
			zDropEditColumnStyleInfo5.ColumnName = "NationalType";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.GroupName = Enterprise.Customs.TR.GUI.Res.GetData("14DA03D4-CD8F-4EF0-BE86-5A9F2D4C99A6", "National Type");
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("F0FDD311-3A02-4BAB-98EB-453BB4D65A3C", "Description");
            zTextBoxColumnStyleInfo1.ColumnName = "JLT_TypeDescription";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.TR.GUI.Res.GetData("F6126BC3-F1E6-43F0-A7CC-EB391A456473", "Tariff Type");
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("8DB3C00C-5C3F-4B69-92AB-FB06F8A13B33", "Action");
            zDropEditColumnStyleInfo2.ColumnName = "JLT_RateOverrideReasonCode";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("8686A197-08DA-4F77-9B07-4E6A0BEDBC59", "Base Amount");
            zCalcEditColumnStyleInfo1.ColumnName = "JLT_BaseValue";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("1A0651D4-A794-4D8E-98E4-6331F525331F", "Method of Calculation");
            zDropEditColumnStyleInfo3.ColumnName = "JLT_MethodOfCalculation";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("FBFE1742-D425-4849-963E-403A91436131", "Tax Rate");
            zCalcEditColumnStyleInfo2.ColumnName = "JLT_Rate";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("A73BF6BC-015F-4AE1-B1E9-B0FBE67D55CD", "Total Amount");
            zCalcEditColumnStyleInfo3.ColumnName = "JLT_Amount";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("5EC9E52B-D26F-40F8-BF04-03203DB235C9", "Method of Payment");
            zDropEditColumnStyleInfo4.ColumnName = "JLT_MethodOfPayment";
            zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.InvoiceLineTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InvoiceLineTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.InvoiceLineTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.InvoiceLineTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.InvoiceLineTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.InvoiceLineTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.InvoiceLineTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.InvoiceLineTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.InvoiceLineTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
            this.InvoiceLineTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InvoiceLineTaxGrid.GridId = "65dfe8f0-5f77-4f2b-92c9-13c660e1fa51";
            this.InvoiceLineTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.InvoiceLineTaxGrid.LayoutKey = "InvoiceLineTaxGrid";
            this.InvoiceLineTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
            this.InvoiceLineTaxGrid.Name = "InvoiceLineTaxGrid";
            this.InvoiceLineTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 299, true);
            this.InvoiceLineTaxGrid.TabIndex = 0;
            // 
            // InvoiceLineTaxUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.InvoiceLineTaxGroupBox);
            this.Name = "InvoiceLineTaxUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 314, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.InvoiceLineTaxGroupBox.ResumeLayout(false);
            this.InvoiceLineTaxGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InvoiceLineTaxGrid)).EndInit();
            this.InvoiceLineTaxGrid.ResumeLayout(false);
            this.InvoiceLineTaxGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox InvoiceLineTaxGroupBox;
		public ZArchitecture.ZGrid InvoiceLineTaxGrid;
	}
}
