using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Module
{
	public partial class OrgSupplierPartFilterStripControl : Customs.Module.OrgSupplierPartFilterStripControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.Module.Res.GetData("OrgSupplierPartFilterStripControl|D78938A8-4DA8-45DD-846E-0F8C307313B0", "Export Classification Lookup Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ExportClassificationLookup";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.Module.Res.GetData("OrgSupplierPartFilterStripControl|0E073D9E-69A9-4427-A1DA-49F623DA21A1", "Class. Lookup Code");
			zTextBoxColumnStyleInfo2.ColumnName = "ImportClassificationLookup";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.IsVisible = true;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Customs.Business.OrgSupplierPart);
			// 
			// OrgSupplierPartFilterStripControl
			// 
			this.Name = "OrgSupplierPartFilterStripControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
