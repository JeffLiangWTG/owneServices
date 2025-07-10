using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module.ReferenceFiles.TransitTimeServiceLevelCombination;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	partial class TransitTimeServiceLevelCombinationFilterControl
	{
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo originzoneCodeTextBoxStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo originOwnerTextBoxStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo destinationZoneCodeTextboxStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo destinationOwnerTextBoxStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo transitTimeTextBoxStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			SetupServiceLevelColumns();
			originzoneCodeTextBoxStyleInfo.ColumnName = "OriginZoneCode";
			originzoneCodeTextBoxStyleInfo.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("45a5abd8-7757-49f4-9d61-993d3a38b286", "Origin Zone");
			originzoneCodeTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			originOwnerTextBoxStyleInfo.ColumnName = "OriginZoneOwner";
			originOwnerTextBoxStyleInfo.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("c1ee8461-1444-4fd9-80b9-37595cd56030", "Origin Org.", "Origin Owner/Carrier", "");
			originOwnerTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			destinationZoneCodeTextboxStyleInfo.ColumnName = "DestinationZoneCode";
			destinationZoneCodeTextboxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			destinationZoneCodeTextboxStyleInfo.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("4605276b-aa41-4a77-a976-5c26e1f641e9", "Destination Zone");
			destinationOwnerTextBoxStyleInfo.ColumnName = "DestinationZoneOwner";
			destinationOwnerTextBoxStyleInfo.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("7213dd49-8e5d-46c4-9c5a-182c1c85cabd", "Dest. Org.", "Destination Owner/Carrier", "");
			destinationOwnerTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			transitTimeTextBoxStyleInfo.ColumnName = "TransitTimeFormatted";
			transitTimeTextBoxStyleInfo.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("3fe12440-88bb-4c62-ad87-671199103e4e", "Transit Time");
			transitTimeTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.ColumnName = "TSC_Mode";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("637ece89-02a4-4661-b0bf-96e6e01f09d7", "Mode");
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(originzoneCodeTextBoxStyleInfo);
			this.grid.ColumnStyles.Add(originOwnerTextBoxStyleInfo);
			this.grid.ColumnStyles.Add(destinationZoneCodeTextboxStyleInfo);
			this.grid.ColumnStyles.Add(destinationOwnerTextBoxStyleInfo);
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				this.grid.ColumnStyles.Add(transitTimeTextBoxStyleInfo);
			}
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 413, true);
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 28, true);
			// 
			// TransitTimeServiceLevelCombinationFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "TransitTimeServiceLevelCombinationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
