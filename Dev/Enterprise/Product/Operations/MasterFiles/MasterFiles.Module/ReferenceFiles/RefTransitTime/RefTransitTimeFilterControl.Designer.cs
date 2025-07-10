namespace Enterprise.MasterFiles.Module
{
	partial class RefTransitTimeFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			SetupServiceLevelColumns();
			zTextBoxColumnStyleInfo2.ColumnName = "OriginZone+SelectedZone+ZoneCode";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("45a5abd8-7757-49f4-9d61-993d3a38b286", "Origin Zone");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "OriginZone+SelectedZoneRelatedOrg+OH_Code";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("c1ee8461-1444-4fd9-80b9-37595cd56030", "Origin Org.", "Origin Owner/Carrier", "");
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "DestinationZone+SelectedZone+ZoneCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("4605276b-aa41-4a77-a976-5c26e1f641e9", "Destination Zone");
			zTextBoxColumnStyleInfo5.ColumnName = "DestinationZone+SelectedZoneRelatedOrg+OH_Code";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("7213dd49-8e5d-46c4-9c5a-182c1c85cabd", "Dest. Org.", "Destination Owner/Carrier", "");
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.ColumnName = "TransitTimeFormatted";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("3fe12440-88bb-4c62-ad87-671199103e4e", "Transit Time");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.ColumnName = "RTT_Mode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			var zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			zCheckBoxColumnStyleInfo1.ColumnName = "OriginZone+IsDomestic";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("90ad0aa1-f9c6-48aa-a40f-864a7c5da984", "Origin Dom.", "Origin is Domestic", "");
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);

			var zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			zCheckBoxColumnStyleInfo2.ColumnName = "DestinationZone+IsDomestic";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("8e789ded-96ba-4609-b80c-1d0344e5d94e", "Dest. Dom.", "Destination Is Domestic", "");
			grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			if (!Enterprise.Registry.Business.FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			}
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 413, true);
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 28, true);
			// 
			// RefTransitTimeFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RefTransitTimeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
