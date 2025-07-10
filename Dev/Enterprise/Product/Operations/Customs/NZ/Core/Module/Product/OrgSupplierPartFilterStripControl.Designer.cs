using CargoWise.EntityFramework;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.Module
{
	public partial class OrgSupplierPartFilterStripControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("b5c334b2-9fc3-4587-abdf-cd154f8858dc", "Export Classification Lookup Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ExportClassificationLookup";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("cd477947-81ba-47da-a706-3d3881cbc9c4", "Class. Lookup Code");
			zTextBoxColumnStyleInfo2.ColumnName = "ImportClassificationLookup";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.IsVisible = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("e7e72a3f-d3c8-4f18-b3cc-2c36af1ccc06", "Export Last Audit By");
			zTextBoxColumnStyleInfo3.ColumnName = "ExportLastAuditUser";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Customs.NZ.Module.Res.GetData("OrgSupplierPartFilter|DC8AF43C-D532-43F1-A53E-F0B2A6D91CF9", "Export Audit");
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("1af2f821-8efe-44c0-852b-1a95886da3a1", "Export Last Audit Date");
			zTextBoxColumnStyleInfo4.ColumnName = "ExportLastAuditDate";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Customs.NZ.Module.Res.GetData("OrgSupplierPartFilter|DC8AF43C-D532-43F1-A53E-F0B2A6D91CF9", "Export Audit");
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("0ecb6eb6-66b6-41fb-aa2c-ef53d559058c", "Import Last Audit By");
			zTextBoxColumnStyleInfo5.ColumnName = "ImportLastAuditUser";
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Customs.NZ.Module.Res.GetData("OrgSupplierPartFilter|BDE9DE76-FADB-4E22-9FA1-476D2ABACEC3", "Import Audit");
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("5ed93164-a7fd-4032-afef-cae2c293fcc3", "Import Last Audit Date");
			zTextBoxColumnStyleInfo6.ColumnName = "ImportLastAuditDate";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.NZ.Module.Res.GetData("OrgSupplierPartFilter|BDE9DE76-FADB-4E22-9FA1-476D2ABACEC3", "Import Audit");
			zTextBoxColumnStyleInfo6.IsVisible = false;

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
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
