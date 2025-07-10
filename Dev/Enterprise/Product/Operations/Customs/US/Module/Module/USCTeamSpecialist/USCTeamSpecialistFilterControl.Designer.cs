using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.Module
{
	partial class USCTeamSpecialistFilterControl
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
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Team";
			zTextBoxColumnStyleInfo1.ColumnName = "UJ_FieldImportSpecialistTeamNumber";
			zTextBoxColumnStyleInfo2.Caption = "Port Code";
			zTextBoxColumnStyleInfo2.ColumnName = "UJ_DistrictPortCode";
			zTextBoxColumnStyleInfo3.Caption = "Tariff From";
			zTextBoxColumnStyleInfo3.ColumnName = "UJ_TariffNumberFrom";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCTeamSpecialist|3a47614b-57cb-4a3e-aa19-7f64c8d8b3f4", "Tariff");
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = "Tariff To";
			zTextBoxColumnStyleInfo4.ColumnName = "UJ_TariffNumberTo";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCTeamSpecialist|3a47614b-57cb-4a3e-aa19-7f64c8d8b3f4", "Tariff");
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Country From";
			zTextBoxColumnStyleInfo5.ColumnName = "UJ_CountryFrom";
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCTeamSpecialist|5eebab1b-1998-4ba6-aef0-0ffa0d7e813c", "Country");
			zTextBoxColumnStyleInfo6.Caption = "Country To";
			zTextBoxColumnStyleInfo6.ColumnName = "UJ_CountryTo";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.US.Module.Res.GetData("USCTeamSpecialist|5eebab1b-1998-4ba6-aef0-0ffa0d7e813c", "Country");
			zTextBoxColumnStyleInfo7.Caption = "Importer";
			zTextBoxColumnStyleInfo7.ColumnName = "UJ_ImporterOfRecordName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.USCTeamSpecialist);
			// 
			// USCTeamSpecialistFilterControl
			// 
			this.Name = "USCTeamSpecialistFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
