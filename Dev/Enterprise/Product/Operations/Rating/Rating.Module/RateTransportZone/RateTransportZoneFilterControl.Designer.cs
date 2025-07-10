namespace Enterprise.Rating.Module
{
	public partial class RateTransportZoneFilterControl
	{
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zoneHubColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			//
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("9e7234b8-d594-477c-8cf2-a80cae5c7eb4", "Zone Owner Code", "Zone Owner Code", "");
			zTextBoxColumnStyleInfo5.ColumnName = "TransportProvider+RelatedPartyCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("aff92e32-35e3-4f8d-bb50-952801a990f6", "Zone Owner");
			zTextBoxColumnStyleInfo1.ColumnName = "TransportProvider+RelatedPartyFullName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("5181cd2b-0932-417a-9571-bf3ac9d63c5d", "Name", "Zone Name", "");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "TZ_ZoneName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("1f36bb71-7f10-45b7-a15e-620d9fb3b68e", "Country/Region");
			zTextBoxColumnStyleInfo3.ColumnName = "TransportProvider+CountryCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("137221df-938f-45b9-ba9d-eb9a876e90dc", "Type", "Zone Type", "");
			zTextBoxColumnStyleInfo4.ColumnName = "TransportProvider+TP_ZoneType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zoneHubColumnStyleInfo.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("56a8546b-0fb9-4779-9075-78e6eb5f739c", "Zone Hub");
			zoneHubColumnStyleInfo.ColumnName = "TransportProvider+ZoneHubLocation+R9_InternationalName";
			zoneHubColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zoneHubColumnStyleInfo);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 413, true);
			this.grid.TabIndex = 7;
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(646, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.RateTransportZone);
			// 
			// RateTransportZoneFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RateTransportZoneFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
