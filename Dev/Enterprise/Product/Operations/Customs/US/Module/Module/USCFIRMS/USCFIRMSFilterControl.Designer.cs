using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.Module
{
	partial class USCFIRMSFilterControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("edae79ea-8a79-48f9-8dc4-cc3e27e60fab", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "US_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("034cde2d-11c5-4629-911f-8f9837c0a1ef", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "US_Name";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("0c0d4a67-2b8e-4086-9d67-c9d317dcfb86", "District Port Code");
			zTextBoxColumnStyleInfo3.ColumnName = "US_DistrictPortCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("9e5bee16-3788-4f29-9d93-9b9ed9097cab", "Address");
			zTextBoxColumnStyleInfo4.ColumnName = "US_Address";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("64075429-3440-4b7e-9184-102e2c71bd9f", "City");
			zTextBoxColumnStyleInfo5.ColumnName = "US_City";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("9de973f1-129b-4529-b903-7af86472e3a8", "State");
			zTextBoxColumnStyleInfo6.ColumnName = "US_State";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("373fdf36-9f17-42bf-afd2-9ef4e87e6719", "Zip Code");
			zTextBoxColumnStyleInfo7.ColumnName = "US_ZipCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("5f85e8f8-521a-487b-920c-c458b7dccbd1", "Country");
			zTextBoxColumnStyleInfo8.ColumnName = "US_Country";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("703ebb77-2ec4-42db-b9e4-009a704c84c0", "Facility Type");
			zTextBoxColumnStyleInfo9.ColumnName = "US_FacilityType";
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCFIRMFilter|fa487133-b322-4f9d-a37d-a032ca6fbecb", "Facility Type");
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("8a9ab3b6-60a8-48f7-b963-ea99ce619833", "Facility Type Description");
			zTextBoxColumnStyleInfo10.ColumnName = "FacilityTypeDescription";
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCFIRMFilter|fa487133-b322-4f9d-a37d-a032ca6fbecb", "Facility Type");
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("1b27c995-55ab-4caf-ab5d-c16e2e787bb5", "Is Active");
			zTextBoxColumnStyleInfo11.ColumnName = "US_IsActive";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("836afe01-b8bc-42ed-a28c-c5969ee144e8", "Last Update");
			zTextBoxColumnStyleInfo12.ColumnName = "US_LastUpdate";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.USCFIRMS);
			// 
			// USCFIRMSFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "USCFIRMSFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
