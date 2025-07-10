namespace Enterprise.Rating.Module
{
	 partial class UniversalCommodityCodeFilterControl
	{
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("a35ceb1a-1e63-44d6-bb4a-898411141b6c", "Universal Group");
			zTextBoxColumnStyleInfo5.ColumnName = "RH_UniversalCommodityGroup";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("3e0081d0-66c5-45d2-abee-48c5680481cd", "Universal Group Description");
			zTextBoxColumnStyleInfo6.ColumnName = "RH_UniversalCommodityGroupDescription";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("a332e33f-13c7-4860-a849-99e003847a8a", "Commodity Code");
			zTextBoxColumnStyleInfo7.ColumnName = "RH_Code";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("23b2f04c-2a4b-4f94-b626-da10157cece6", "Commodity Description");
			zTextBoxColumnStyleInfo8.ColumnName = "RH_Description";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 13, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 403, true);
			this.grid.TabIndex = 14;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(MasterFiles.Business.Rating.UniversalChargeCodeBizo);
			// 
			// UniversalCommodityCodeFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "UniversalCommodityCodeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 416, true);
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
