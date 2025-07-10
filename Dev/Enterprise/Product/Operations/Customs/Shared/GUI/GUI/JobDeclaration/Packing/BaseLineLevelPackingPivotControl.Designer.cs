using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	partial class BaseLineLevelPackingPivotControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected Enterprise.ZArchitecture.ZGrid DjcPackageInvoiceLineGrid;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.DjcPackageInvoiceLineGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DjcPackageInvoiceLineGrid)).BeginInit();
			this.DjcPackageInvoiceLineGrid.SuspendLayout();
			this.SuspendLayout();

			this.BindingSource.DataSourceType = typeof(BaseJobComInvoiceLine);

			// 
			// DjcPackageInvoiceLineGrid
			// 
			this.DjcPackageInvoiceLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DjcPackageInvoiceLineGrid, "PackagesForInvoiceLinesForBindingOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.DjcPackageInvoiceLineGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|9f452fc9-1111-4e91-b699-705b07f9f4ee", "Is For Invoice Line?");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsLinked";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.ColumnName = "PackageNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PackQty";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.DjcPackageInvoiceLineGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DjcPackageInvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DjcPackageInvoiceLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DjcPackageInvoiceLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DjcPackageInvoiceLineGrid.GridId = "1eea3a76-1111-47d0-81d8-a88a6d6b1e48";
			this.DjcPackageInvoiceLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DjcPackageInvoiceLineGrid.LayoutKey = "DjcPackageInvoiceLineGrid";
			this.DjcPackageInvoiceLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DjcPackageInvoiceLineGrid.Name = "DjcPackageInvoiceLineGrid";
			this.DjcPackageInvoiceLineGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DjcPackageInvoiceLineGrid.TabIndex = 0;
			// 
			// BaseLineLevelPackingPivotControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DjcPackageInvoiceLineGrid);
			this.Name = "BaseLineLevelPackingPivotControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DjcPackageInvoiceLineGrid)).EndInit();
			this.DjcPackageInvoiceLineGrid.ResumeLayout(false);
			this.DjcPackageInvoiceLineGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
