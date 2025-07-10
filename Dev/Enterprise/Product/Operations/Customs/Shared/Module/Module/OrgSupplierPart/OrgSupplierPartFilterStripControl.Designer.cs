using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Module
{
	partial class OrgSupplierPartFilterStripControl
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();

			ZTextBoxColumnStyleInfo tariffNumberTextBox = new ZTextBoxColumnStyleInfo();
			tariffNumberTextBox.CaptionResourceString =
				Enterprise.Customs.Module.Res.GetData("9CC1B1F4-361D-4E1F-9793-476352FEBF53", "Tariff Numbers");
			tariffNumberTextBox.ColumnName = "Tariffs";
			tariffNumberTextBox.ToolTip = "Associated Tariff Numbers";
			tariffNumberTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			FilteredGrid.ColumnStyles.Add(tariffNumberTextBox);

			// 
			// OrgSupplierPartFilterStripControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "Enterprise.Customs.Business.OrgSupplierPart";
			this.Name = "OrgSupplierPartFilterStripControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
