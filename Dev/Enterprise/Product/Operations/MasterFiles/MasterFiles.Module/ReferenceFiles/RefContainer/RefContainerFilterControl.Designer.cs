namespace Enterprise.MasterFiles.Module
{
	public partial class RefContainerFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|1a568f24-7809-479b-b777-3f0e4db1a2b5", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "RC_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|bd3b4d44-82e0-464f-b920-99a295b07989", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "RC_Code";
			zTextBoxColumnStyleInfo2.ColumnName = "RC_DescriptionMultilingual";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|889a54a9-7f03-4446-9347-97b7b5951204", "Mode");
			zTextBoxColumnStyleInfo3.ColumnName = "RC_ShippingMode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|4495a923-981d-4437-b04c-d262ef5fa5cc", "Container Type");
			zTextBoxColumnStyleInfo4.ColumnName = "RC_ContainerType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|122ae08f-0bed-446b-b7f3-21edb4158564", "IATA Class");
			zTextBoxColumnStyleInfo5.ColumnName = "RC_IATARateClass";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|9c26f137-f92a-49a0-b0ce-a781c3f64545", "Height");
			zCalcEditColumnStyleInfo1.ColumnName = "RC_Height";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|fbf0ea4a-acdb-40eb-b900-da6fc82c804f", "Length");
			zCalcEditColumnStyleInfo2.ColumnName = "RC_Length";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|9df69f8b-b731-4416-a271-a255b8fdfab2", "Width");
			zCalcEditColumnStyleInfo3.ColumnName = "RC_Width";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "RC_GrossWeight";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "RC_TareWeight";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|b4727fe4-d3c6-4b1d-b146-df98c7f12c46", "Capacity (M3)");
			zCalcEditColumnStyleInfo6.ColumnName = "RC_CubicCapacity";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|2e286edb-852e-4de7-a156-b8254bdf6028", "ISO");
			zCheckBoxColumnStyleInfo2.ColumnName = "RC_IsIso";
			zCheckBoxColumnStyleInfo2.GroupName = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|747e35c8-fbd7-4860-8698-4ac884c70e7a", "ISO Details");
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.ColumnName = "RC_ISOType";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|747e35c8-fbd7-4860-8698-4ac884c70e7a", "ISO Details");
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|61df3ac4-9820-4369-be3c-d32af42e4161", "TEU");
			zCalcEditColumnStyleInfo7.ColumnName = "RC_TEU";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|e88af7b7-c7f1-476a-823f-02e7211dcdd3", "ISO Size", "The official size based on the ISO Type specified.");
			zTextBoxColumnStyleInfo7.ColumnName = "ISOType+Dimensions";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|747e35c8-fbd7-4860-8698-4ac884c70e7a", "ISO Details");
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|246a7be0-175d-4f09-9a78-e5423c79367b", "ISO Description", "The official description based on the ISO Type specified.");
			zTextBoxColumnStyleInfo8.ColumnName = "ISOType+Description";
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.MasterFiles.Module.Res.GetData("RefContainerFilterControl|747e35c8-fbd7-4860-8698-4ac884c70e7a", "ISO Details");
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 376, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefContainer);
			// 
			// RefContainerFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RefContainerFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 528, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
