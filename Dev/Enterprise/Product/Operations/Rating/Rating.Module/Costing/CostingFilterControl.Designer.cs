namespace Enterprise.Rating.Module
{
	public partial class CostingFilterControl
	{
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("CostingFilterControl|b61a9d6d-8d55-4a0a-845a-facc65e887b2", "Svc. Prov Code");
			zTextBoxColumnStyleInfo1.ColumnName = "TH_ClientCode";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("CostingFilterControl|98f6ecd5-80c2-4dcf-89df-5e44c05ed41b", "Service Provider");
			zTextBoxColumnStyleInfo2.ColumnName = "TH_ClientFullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("CostingFilterControl|edff23bd-b138-4951-9ebd-97d4badbc4f5", "UNLOCO");
			zTextBoxColumnStyleInfo3.ColumnName = "SupplierUNLOCO";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("CostingFilterControl|ee0b051e-1a80-49ce-b57a-2bce4b5e958a", "Service Provider Type");
			zTextBoxColumnStyleInfo4.ColumnName = "SupplierType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("7efd97e2-46ff-41ba-8ccf-042e80952bcd", "Published");
			zTextBoxColumnStyleInfo5.ColumnName = "Published";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 19, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 517, true);
			this.grid.TabIndex = 11;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.Costing);
			// 
			// CostingFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "CostingFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 536, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
