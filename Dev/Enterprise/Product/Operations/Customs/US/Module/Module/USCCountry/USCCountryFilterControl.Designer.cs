using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module
{
	partial class USCCountryFilterControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "ISO Country Code";
			zTextBoxColumnStyleInfo1.ColumnName = USCCountry.Schema.UC_Code;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.Caption = "Name";
			zTextBoxColumnStyleInfo2.ColumnName = "UC_Name";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.Caption = "Currency Code";
			zTextBoxColumnStyleInfo3.ColumnName = "UC_CurrencyCode";
			zTextBoxColumnStyleInfo4.Caption = "Currency Name";
			zTextBoxColumnStyleInfo4.ColumnName = "UC_CurrencyName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.Caption = "Drawback Eligibility";
			zCheckBoxColumnStyleInfo1.ColumnName = "UC_DrawbackEligibility";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo2.Caption = "Lesser Developed Country";
			zCheckBoxColumnStyleInfo2.ColumnName = "UC_LesserDevelopedCountry";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo3.Caption = "Restriction Indicator";
			zCheckBoxColumnStyleInfo3.ColumnName = "UC_RestrictionIndicator";
			zCheckBoxColumnStyleInfo3.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|932a3334-bd87-4bb5-bafe-087196cb25c5", "Restriction Indicator");
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.Caption = "Restriction Indicator Begin Date";
			zDateEditColumnStyleInfo1.ColumnName = "UC_RestrictionIndicatorBeginDate";
			zDateEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|932a3334-bd87-4bb5-bafe-087196cb25c5", "Restriction Indicator");
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo2.Caption = "Restriction Indicator End Date";
			zDateEditColumnStyleInfo2.ColumnName = "UC_RestrictionIndicatorEndDate";
			zDateEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|932a3334-bd87-4bb5-bafe-087196cb25c5", "Restriction Indicator");
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo5.Caption = "Shedule C Country Code";
			zTextBoxColumnStyleInfo5.ColumnName = "UC_SheduleCCountryCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo6.Caption = "Special Trade Programs Indicator";
			zTextBoxColumnStyleInfo6.ColumnName = "UC_SpecialTradeProgramsIndicator";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|1f6e57f2-cadd-4330-860f-9dfd9a85e213", "Special Trade Indicator");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo3.Caption = "Special Trade Programs Begin Date";
			zDateEditColumnStyleInfo3.ColumnName = "UC_SpecialTradeProgramsBeginDate";
			zDateEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|1f6e57f2-cadd-4330-860f-9dfd9a85e213", "Special Trade Indicator");
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo4.Caption = "Special Trade Programs End Date";
			zDateEditColumnStyleInfo4.ColumnName = "UC_SpecialTradeProgramsEndDate";
			zDateEditColumnStyleInfo4.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|1f6e57f2-cadd-4330-860f-9dfd9a85e213", "Special Trade Indicator");
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo7.Caption = "SPI Code";
			zTextBoxColumnStyleInfo7.ColumnName = "UC_SPICode";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|da4ca273-1ce9-40d0-8ffa-e432e9d79d8c", "SPI Code");
			zDateEditColumnStyleInfo5.Caption = "SPI Begin Date";
			zDateEditColumnStyleInfo5.ColumnName = "UC_SPIBeginDate";
			zDateEditColumnStyleInfo5.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|da4ca273-1ce9-40d0-8ffa-e432e9d79d8c", "SPI Code");
			zDateEditColumnStyleInfo6.Caption = "SPI End Date";
			zDateEditColumnStyleInfo6.ColumnName = "UC_SPIEndDate";
			zDateEditColumnStyleInfo6.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|da4ca273-1ce9-40d0-8ffa-e432e9d79d8c", "SPI Code");
			zTextBoxColumnStyleInfo8.Caption = "Rate Indicator";
			zTextBoxColumnStyleInfo8.ColumnName = "UC_RateColumnIndicator";
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|72e26e0a-e578-461c-9759-b859ea2482d3", "Rate Indicator");
			zDateEditColumnStyleInfo7.Caption = "Rate Begin Date";
			zDateEditColumnStyleInfo7.ColumnName = "UC_RateColumnBeginDate";
			zDateEditColumnStyleInfo7.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|72e26e0a-e578-461c-9759-b859ea2482d3", "Rate Indicator");
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo8.Caption = "Rate End Date";
			zDateEditColumnStyleInfo8.ColumnName = "UC_RateColumnEndDate";
			zDateEditColumnStyleInfo8.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|72e26e0a-e578-461c-9759-b859ea2482d3", "Rate Indicator");
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo4.Caption = "GSP Indicator";
			zCheckBoxColumnStyleInfo4.ColumnName = "UC_GSPIndicator";
			zCheckBoxColumnStyleInfo4.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|6350d908-6103-4660-addb-dde13c741b15", "GSP Indicator");
			zDateEditColumnStyleInfo9.Caption = "GSP Begin Date";
			zDateEditColumnStyleInfo9.ColumnName = "UC_GSPBeginDate";
			zDateEditColumnStyleInfo9.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|6350d908-6103-4660-addb-dde13c741b15", "GSP Indicator");
			zDateEditColumnStyleInfo10.Caption = "GSP End Date";
			zDateEditColumnStyleInfo10.ColumnName = "UC_GSPEndDate";
			zDateEditColumnStyleInfo10.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|6350d908-6103-4660-addb-dde13c741b15", "GSP Indicator");
			zTextBoxColumnStyleInfo9.Caption = "Miscellaneous SPI Indicator";
			zTextBoxColumnStyleInfo9.ColumnName = "UC_MiscellaneousSPIIndicator";
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|4df226a9-cb02-4516-983f-0b605c1827d2", "Miscellaneous SPI Indicator");
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo11.Caption = "Miscellaneous SPI Begin Date";
			zDateEditColumnStyleInfo11.ColumnName = "UC_MiscellaneousSPIBeginDate";
			zDateEditColumnStyleInfo11.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|4df226a9-cb02-4516-983f-0b605c1827d2", "Miscellaneous SPI Indicator");
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo12.Caption = "Miscellaneous SPI End Date";
			zDateEditColumnStyleInfo12.ColumnName = "UC_MiscellaneousSPIEndDate";
			zDateEditColumnStyleInfo12.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCCountryFilter|4df226a9-cb02-4516-983f-0b605c1827d2", "Miscellaneous SPI Indicator");
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(USCCountry);
			// 
			// USCCountryFilterControl
			// 
			this.Name = "USCCountryFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
