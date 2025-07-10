using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.Module
{
	partial class USCVisaFilterControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
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
			zTextBoxColumnStyleInfo1.Caption = "Textile Category No";
			zTextBoxColumnStyleInfo1.ColumnName = "UO_TextileCategoryNo";
			zTextBoxColumnStyleInfo2.Caption = "Country of Origin";
			zTextBoxColumnStyleInfo2.ColumnName = "UO_UC_NKOriginCountry";
			zDateEditColumnStyleInfo1.Caption = "Begin Date";
			zDateEditColumnStyleInfo1.ColumnName = "UO_BeginDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "UO_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCheckBoxColumnStyleInfo1.Caption = "ELVIS";
			zCheckBoxColumnStyleInfo1.ColumnName = "UO_IsELVIS";
			zCheckBoxColumnStyleInfo2.Caption = "Is Exception To Visa Req";
			zCheckBoxColumnStyleInfo2.ColumnName = "UO_IsExceptionToVisaReq";
			zCheckBoxColumnStyleInfo3.Caption = "Is Part Category";
			zCheckBoxColumnStyleInfo3.ColumnName = "UO_IsPartCategory";
			zCheckBoxColumnStyleInfo4.Caption = "Is Special Program";
			zCheckBoxColumnStyleInfo4.ColumnName = "UO_IsSpecialProgram";
			zCheckBoxColumnStyleInfo5.Caption = "Is Stardard Visa Format";
			zCheckBoxColumnStyleInfo5.ColumnName = "UO_IsStardardVisaFormat";
			zCheckBoxColumnStyleInfo6.Caption = "Is Visa Exempt For Forklore";
			zCheckBoxColumnStyleInfo6.ColumnName = "UO_IsVisaExemptForForklore";
			zCheckBoxColumnStyleInfo7.Caption = "Is Visa Exempt For Sample";
			zCheckBoxColumnStyleInfo7.ColumnName = "UO_IsVisaExemptForSample";
			zCheckBoxColumnStyleInfo8.Caption = "Is Visa Quantity Indicated";
			zCheckBoxColumnStyleInfo8.ColumnName = "UO_IsVisaQuantityIndicated";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
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
			this.BindingSource.DataSourceType = typeof(Business.USCVisa);
			// 
			// USCVisaFilterControl
			// 
			this.Name = "USCVisaFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
