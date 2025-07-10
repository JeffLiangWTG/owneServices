using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Module
{
	public partial class CountryStatesGlbHolidayFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTranslatableTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("d2aa2322-e140-4244-bea7-65c5873eb795", "Holiday Name");
			zTranslatableTextBoxColumnStyleInfo1.ColumnName = "GHC_HolidayName";
			zTranslatableTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);

			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("eda73570-b7f4-4739-95cb-7f190ed7765d", "Date");
			zTextBoxColumnStyleInfo1.ColumnName = "GHC_Date";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("430028d5-f8e0-45fc-b353-0c39d29a107a", "Country");
			zTextBoxColumnStyleInfo2.ColumnName = "GHC_Countrycode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);

			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("c48286da-72c9-4edc-b248-50ec59242863", "Applicability");
			zTextBoxColumnStyleInfo3.ColumnName = "GHC_CountryStatesApplicability";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);

			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("25c84faf-93e5-4a4b-abd7-7c7348f4e131", "Is Recurring");
			zCheckBoxColumnStyleInfo1.ColumnName = "GHC_Recurring";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);

			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("22d73f35-1d62-49a1-8fb6-e8b7b43ceb9b", "Is Working Day");
			zCheckBoxColumnStyleInfo2.ColumnName = "GHC_IsWorkingDay";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo);
			// 
			// CountryStatesGlbHolidayFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "CountryStatesGlbHolidayFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
