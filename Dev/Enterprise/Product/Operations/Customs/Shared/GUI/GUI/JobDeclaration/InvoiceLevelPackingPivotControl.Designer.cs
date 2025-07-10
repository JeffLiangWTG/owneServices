namespace Enterprise.Customs.GUI
{
	partial class InvoiceLevelPackingPivotControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.PackageInvoiceGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackageInvoiceGrid)).BeginInit();
			this.PackageInvoiceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// PackageInvoiceGrid
			// 
			this.PackageInvoiceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackageInvoiceGrid, "Invoices.PackagesForInvoicesForBindingOnly");
			this.PackageInvoiceGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ImportSupplierHeaderUserControl|A481599B-6DB4-425A-A88B-EB6FAD8CA8FA", "Is For Invoice Header?");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsLinked";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ImportSupplierHeaderUserControl|37E0DD02-F833-4C57-81F2-BAC06976F4D3", "Package Number");
			zTextBoxColumnStyleInfo1.ColumnName = "PackageNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ImportSupplierHeaderUserControl|F5555B88-77A2-49D0-BA92-A2F160675832", "Pack Quantity");
			zCalcEditColumnStyleInfo1.ColumnName = "PackQty";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PackageInvoiceGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PackageInvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackageInvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackageInvoiceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageInvoiceGrid.GridId = "1D0217B9-E88E-43EF-99ED-87B57166DF8A";
			this.PackageInvoiceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackageInvoiceGrid.LayoutKey = "PackageInvoiceGrid";
			this.PackageInvoiceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageInvoiceGrid.Name = "PackageInvoiceGrid";
			this.PackageInvoiceGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PackageInvoiceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.PackageInvoiceGrid.TabIndex = 1;
			// 
			// InvoiceLevelPackingPivotControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PackageInvoiceGrid);
			this.Name = "InvoiceLevelPackingPivotControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PackageInvoiceGrid)).EndInit();
			this.PackageInvoiceGrid.ResumeLayout(false);
			this.PackageInvoiceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZGrid PackageInvoiceGrid;
	}
}
