using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefContainerISOTypesFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZLabel labelInfo = new ZArchitecture.ZLabel();

			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			this.IsFilterVisible = false;
			//
			//InfoLabel
			//
			labelInfo.Name = "InfoLabel";
			labelInfo.CaptionResourceString = Res.GetData("RefContainerISOTypesFilterControl|InfoFind", "Press Control + Find (Ctrl+F) to search through the records.");
			labelInfo.AutoSize = true;
			labelInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
			labelInfo.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 10, 0, 5, true);

			this.Controls.Add(labelInfo);
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerISOTypesFilterControl|D0524EE8-A5D4-4D6E-8285-48C54E2A1958", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ISOCode";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefContainerISOTypesFilterControl|1738B3F6-3093-44F8-9B0C-FEE2FA257591", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);

			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ContainerISOType);
			// 
			// RefContainerISOTypesFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RefContainerISOTypesFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 528, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
