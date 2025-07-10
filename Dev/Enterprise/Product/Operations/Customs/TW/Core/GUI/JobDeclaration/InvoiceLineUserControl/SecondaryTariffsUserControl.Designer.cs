namespace Enterprise.Customs.TW.GUI
{
	partial class SecondaryTariffsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.secondaryTariffsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SecondaryTariffsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.secondaryTariffsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecondaryTariffsGrid)).BeginInit();
			this.SecondaryTariffsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobComInvoiceLine);
			// 
			// secondaryTariffsGroupBox
			// 
			this.secondaryTariffsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("916B315C-64A2-4855-A4C3-3EF66814178F", "Taxes");
			this.secondaryTariffsGroupBox.Controls.Add(this.SecondaryTariffsGrid);
			this.secondaryTariffsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.secondaryTariffsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.secondaryTariffsGroupBox.Name = "secondaryTariffsGroupBox";
			this.secondaryTariffsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 131, true);
			this.secondaryTariffsGroupBox.TabIndex = 0;
			this.secondaryTariffsGroupBox.TabStop = false;
			// 
			// SecondaryTariffsGrid
			// 
			this.SecondaryTariffsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SecondaryTariffsGrid, "Taxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)).SyncRoot)).JLT_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)).SyncRoot)).JLT_TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)).SyncRoot)).JLT_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)).SyncRoot)).JLT_TariffDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)).SyncRoot)).JLT_BaseQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)).SyncRoot)).JLT_BaseQuantityUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)).SyncRoot)).FormattedTariffRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)).SyncRoot)).JLT_MethodOfPayment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(null)).Taxes)).SyncRoot)).PaymentMethodDescription)));
			this.SecondaryTariffsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "JLT_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "JLT_TypeDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(226);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JLT_Tariff";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "JLT_TariffDesc";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JLT_BaseQuantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "JLT_BaseQuantityUQ";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.ColumnName = "FormattedTariffRate";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.ColumnName = "JLT_MethodOfPayment";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("b70d35ec-9768-4ee4-aeaa-73b5ad86ebbc", "Payment Method");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo5.ColumnName = "PaymentMethodDescription";
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("b70d35ec-9768-4ee4-aeaa-73b5ad86ebbc", "Payment Method");
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.SecondaryTariffsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SecondaryTariffsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecondaryTariffsGrid.GridId = "005DBC4D-C80B-4CF1-8F2B-60B8B05777B0";
			this.SecondaryTariffsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SecondaryTariffsGrid.LayoutKey = "zGrid1";
			this.SecondaryTariffsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SecondaryTariffsGrid.Name = "SecondaryTariffsGrid";
			this.SecondaryTariffsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 112, true);
			this.SecondaryTariffsGrid.TabIndex = 0;
			// 
			// SecondaryTariffsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.secondaryTariffsGroupBox);
			this.Name = "SecondaryTariffsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 131, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.secondaryTariffsGroupBox.ResumeLayout(false);
			this.secondaryTariffsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecondaryTariffsGrid)).EndInit();
			this.SecondaryTariffsGrid.ResumeLayout(false);
			this.SecondaryTariffsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox secondaryTariffsGroupBox;
		public ZArchitecture.ZGrid SecondaryTariffsGrid;
	}
}
