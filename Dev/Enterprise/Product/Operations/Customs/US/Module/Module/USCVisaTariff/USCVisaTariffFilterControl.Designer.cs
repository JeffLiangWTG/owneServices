using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.Module
{
	partial class USCVisaTariffFilterControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Tariff";
			zTextBoxColumnStyleInfo1.ColumnName = "UK_Tariff";
			zTextBoxColumnStyleInfo2.Caption = "Textile Category Number";
			zTextBoxColumnStyleInfo2.ColumnName = "Visa+UO_TextileCategoryNo";
			zTextBoxColumnStyleInfo3.Caption = "Country Of Origin";
			zTextBoxColumnStyleInfo3.ColumnName = "Visa+UO_UC_NKOriginCountry";
			zDateEditColumnStyleInfo1.Caption = "Begin Date";
			zDateEditColumnStyleInfo1.ColumnName = "Visa+UO_BeginDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "Visa+UO_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCheckBoxColumnStyleInfo1.Caption = "Is Standard Visa Format";
			zCheckBoxColumnStyleInfo1.ColumnName = "Visa+UO_IsStardardVisaFormat";
			zCheckBoxColumnStyleInfo2.Caption = "Is ELVIS";
			zCheckBoxColumnStyleInfo2.ColumnName = "Visa+UO_IsELVIS";
			zCheckBoxColumnStyleInfo3.Caption = "Is Visa Exempt For Sample";
			zCheckBoxColumnStyleInfo3.ColumnName = "Visa+UO_IsVisaExemptForSample";
			zCheckBoxColumnStyleInfo4.Caption = "Is Visa Exempt For Foklore";
			zCheckBoxColumnStyleInfo4.ColumnName = "Visa+UO_IsVisaExemptForForklore";
			zCheckBoxColumnStyleInfo5.Caption = "Is Visa Qty Indicated";
			zCheckBoxColumnStyleInfo5.ColumnName = "Visa+UO_IsVisaQuantityIndicated";
			zCheckBoxColumnStyleInfo6.Caption = "Is Exception to Visa Req";
			zCheckBoxColumnStyleInfo6.ColumnName = "Visa+UO_IsExceptionToVisaReq";
			zCheckBoxColumnStyleInfo7.Caption = "Is Part Category";
			zCheckBoxColumnStyleInfo7.ColumnName = "Visa+UO_IsPartCategory";
			zCheckBoxColumnStyleInfo8.Caption = "Is Special Program";
			zCheckBoxColumnStyleInfo8.ColumnName = "Visa+UO_IsSpecialProgram";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.USCVisaTariff);
			// 
			// USCVisaTariffFilterControl
			// 
			this.Name = "USCVisaTariffFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
