namespace Enterprise.MasterFiles.Module
{
	public partial class RefCurrencyFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|601f6c24-35c5-4805-a0bb-94c537d22b2c", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "RX_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "RX_DescMultilingual";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "RX_Symbol";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|d9a95e5e-fbb8-45d2-979a-c1437e98c29f", "Unit Name");
			zTextBoxColumnStyleInfo4.ColumnName = "RX_UnitNameMultilingual";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.ColumnName = "RX_SubUnitNameMultilingual";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|80ca28eb-eca3-4c13-b4e0-ebd53591cf93", "Sub Unit Ratio");
			zCalcEditColumnStyleInfo1.ColumnName = "RX_SubUnitRatio";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|14ef407b-45a7-445f-96c6-08422c02ce2c", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "RX_IsActive";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo3.ColumnName = "RX_IsSystem";
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|9FD3AE11-30DD-4E33-B782-0D5E900CA9B0", "Is System");
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("f2bebdc0-2f9c-4228-8b9f-4b5d4b63cdeb", "Exclude from CFX", "Exclude from CFX Calculation");
			zCheckBoxColumnStyleInfo2.ColumnName = "RX_IsExcludedCFXCalculation";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|46fc1b25-78c8-445b-867a-220037647793", "Buy Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "CurrentBuyRate";
			zCalcEditColumnStyleInfo2.Decimals = 6;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|d901a7cf-f5f3-4fc4-8039-732c72cc63d2", "Current Exchange Rates");
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|f31b0829-ffa6-4cc7-87c3-18d470c23802", "Sell Rate");
			zCalcEditColumnStyleInfo3.ColumnName = "CurrentSellRate";
			zCalcEditColumnStyleInfo3.Decimals = 6;
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|d901a7cf-f5f3-4fc4-8039-732c72cc63d2", "Current Exchange Rates");
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|b25d45d9-ccf3-4975-99c4-e1dcbface9e7", "Customs Rate");
			zCalcEditColumnStyleInfo4.ColumnName = "CurrentCustomsRate";
			zCalcEditColumnStyleInfo4.Decimals = 6;
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|d901a7cf-f5f3-4fc4-8039-732c72cc63d2", "Current Exchange Rates");
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|60818f88-5f63-4d5f-bfa3-d063359ff5a6", "ISO Sub Unit Ratio");
			zCalcEditColumnStyleInfo5.ColumnName = "RX_ISOSubUnitRatio";
			zCalcEditColumnStyleInfo5.Decimals = 0;
			zCalcEditColumnStyleInfo5.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 264, true);
			this.FilteredGrid.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCurrency);
			// 
			// RefCurrencyFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RefCurrencyFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
