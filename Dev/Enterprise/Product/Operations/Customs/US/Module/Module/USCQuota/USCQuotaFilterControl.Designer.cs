using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.Module
{
	partial class USCQuotaFilterControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Tariff/Category/Visa No";
			zTextBoxColumnStyleInfo1.ColumnName = "UT_Code";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Caption = "Country of Origin";
			zTextBoxColumnStyleInfo2.ColumnName = "UT_UC_NKOriginCountry";
			zTextBoxColumnStyleInfo3.Caption = "Period Process Date Indicator";
			zTextBoxColumnStyleInfo3.ColumnName = "PeriodProcessDateIndicatorDesc";
			zDateEditColumnStyleInfo1.Caption = "Begin Date";
			zDateEditColumnStyleInfo1.ColumnName = "UT_BeginDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "UT_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo4.Caption = "First Namesake";
			zTextBoxColumnStyleInfo4.ColumnName = "UT_FirstNamesake";
			zTextBoxColumnStyleInfo5.Caption = "Second Namesake";
			zTextBoxColumnStyleInfo5.ColumnName = "UT_SecondNamesake";
			zTextBoxColumnStyleInfo6.Caption = "Second Tariff No";
			zTextBoxColumnStyleInfo6.ColumnName = "UT_SecondTariffNo";
			zCheckBoxColumnStyleInfo1.Caption = "Held";
			zCheckBoxColumnStyleInfo1.ColumnName = "UT_IsHeld";
			zDateEditColumnStyleInfo3.Caption = "Last Trasaction Date";
			zDateEditColumnStyleInfo3.ColumnName = "UT_LastTrasactionDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Caption = "Last Update Date";
			zDateEditColumnStyleInfo4.ColumnName = "UT_LastUpdateDate";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Quota Limit";
			zCalcEditColumnStyleInfo1.ColumnName = "UT_QuotaLimit";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCQuoteFilter|7365dd9a-7441-486f-973a-c4a7fb058487", "Quota Qty");
			zTextBoxColumnStyleInfo7.Caption = "UQ";
			zTextBoxColumnStyleInfo7.ColumnName = "UT_QuotaUQ";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCQuoteFilter|7365dd9a-7441-486f-973a-c4a7fb058487", "Quota Qty");
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Threshold Qty";
			zCalcEditColumnStyleInfo2.ColumnName = "UT_ThresholdQty";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Qty To Date";
			zCalcEditColumnStyleInfo3.ColumnName = "UT_QtyToDate";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zTextBoxColumnStyleInfo8.Caption = "Quota Limit Type";
			zTextBoxColumnStyleInfo8.ColumnName = "QuotaLimitTypeDesc";
			zTextBoxColumnStyleInfo9.Caption = "Quota Period";
			zTextBoxColumnStyleInfo9.ColumnName = "UT_QuotaPeriod";
			zTextBoxColumnStyleInfo10.Caption = "Quota Status";
			zTextBoxColumnStyleInfo10.ColumnName = "QuotaStatusDesc";
			zTextBoxColumnStyleInfo11.Caption = "Quota Type";
			zTextBoxColumnStyleInfo11.ColumnName = "QuotaTypeDesc";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Textile Conversion Factor";
			zCalcEditColumnStyleInfo4.ColumnName = "UT_TextileConversionFactor";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.USCQuota);
			// 
			// USCQuotaFilterControl
			// 
			this.Name = "USCQuotaFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
