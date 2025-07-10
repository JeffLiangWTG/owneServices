namespace Enterprise.Rating.Module
{
	public partial class CompanyTariffsFilterControl
	{
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("GlobalRatesFilterControl|e43660c3-aab5-4f90-88bd-5c617d9ada86", "Level");
			zTextBoxColumnStyleInfo1.ColumnName = "TH_GlobalRateLevel";
			zTextBoxColumnStyleInfo1.ToolTip = "Company Tariff Level";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("GlobalRatesFilterControl|ce57298b-443d-4fa1-a4a9-a6644e744401", "Company Tariff Description");
			zTextBoxColumnStyleInfo2.ColumnName = "TH_GlobalRateDescriptionMultilingual";
			zTextBoxColumnStyleInfo2.ToolTip = "Company Tariff Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("7efd97e2-46ff-41ba-8ccf-042e80952bcd", "Published");
			zTextBoxColumnStyleInfo3.ColumnName = "Published";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 166, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.RatingHeader);
			// 
			// GlobalRatesFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "GlobalRatesFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
