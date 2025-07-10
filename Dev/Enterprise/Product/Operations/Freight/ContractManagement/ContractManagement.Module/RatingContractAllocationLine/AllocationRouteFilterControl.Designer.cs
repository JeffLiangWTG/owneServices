using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.Module
{
	public partial class AllocationRouteFilterControl : ZFilterStripControl<AllocationRouteModuleStrip>
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo allocationIDTextColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo startDateColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo expiryDateColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo loadPortColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo dischargePortColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo voyageNumberColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo vesselNameColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo allocatedQuantity = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo allocatedQuantityUnit = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo containerTypeColumnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo serviceStringColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo containerClassColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo namedAccountsColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo agentsColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo linkedScheduleColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo linkedScheduleETDColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo linkedScheduleETDUpdatedColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo linkedScheduleSTDColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo linkedScheduleETAColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo linkedScheduleSTAColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo gatewayConsolColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo groupageContainerModeColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo shipperOwnedContainerColumnStyle = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo allowFreightSpotRateColumnInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo tradeLaneCodeInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo tradeLaneDescriptionInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo containerWeightLimitInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo containerWeightLimitUnitInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo containerWeightLimitTypeInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo placeOfReceiptInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo placeOfDeliveryInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo priorityColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();

			allocationIDTextColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("88513d38-cbb7-a887-4464-587c3bf8f8ab", "Allocation ID");
			allocationIDTextColumnStyleInfo.ColumnName = "RCA_AllocationLineID";
			allocationIDTextColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			allocationIDTextColumnStyleInfo.IsReadOnly = true;

			startDateColumnStyleInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			startDateColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("a1342f06-d7d2-fda8-4fb1-d22d5bf5990c", "Start Date");
			startDateColumnStyleInfo.ColumnName = "RCA_StartDate";
			startDateColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			startDateColumnStyleInfo.IsReadOnly = true;

			expiryDateColumnStyleInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			expiryDateColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("b846ea1c-7fa6-a8bf-4bed-bb7721cedb8e", "Expiry Date");
			expiryDateColumnStyleInfo.ColumnName = "RCA_ExpiryDate";
			expiryDateColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			expiryDateColumnStyleInfo.IsReadOnly = true;

			loadPortColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("5b492c13-6291-1884-414c-ed4e78afa448", "Load Port");
			loadPortColumnStyleInfo.ColumnName = "RCA_Calc_LoadLocation";
			loadPortColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			loadPortColumnStyleInfo.IsReadOnly = true;

			dischargePortColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("00eebe03-d431-518a-42b0-87a4982cf5c2", "Discharge Port");
			dischargePortColumnStyleInfo.ColumnName = "RCA_Calc_DischargeLocation";
			dischargePortColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			dischargePortColumnStyleInfo.IsReadOnly = true;

			voyageNumberColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("1bc8642a-c8c6-12a3-4ab9-4fff75aa9773", "Voyage");
			voyageNumberColumnStyleInfo.ColumnName = "RCA_Calc_VoyageNumber";
			voyageNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			voyageNumberColumnStyleInfo.IsReadOnly = true;

			vesselNameColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("21c1d9c8-2860-2cb2-4c5b-adea3c58cc02", "Vessel");
			vesselNameColumnStyleInfo.ColumnName = "RCA_Calc_VesselName";
			vesselNameColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			vesselNameColumnStyleInfo.IsReadOnly = true;

			allocatedQuantity.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("1d3bf382-b749-5497-41a8-67e0692f326d", "Quantity");
			allocatedQuantity.ColumnName = "RCA_AllocatedQuantity";
			allocatedQuantity.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			allocatedQuantity.IsReadOnly = true;

			allocatedQuantityUnit.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("45509638-44cc-4ead-4faa-fb15a6436cf6", "Unit");
			allocatedQuantityUnit.ColumnName = "RCA_AllocatedUQ";
			allocatedQuantityUnit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			allocatedQuantityUnit.IsReadOnly = true;

			containerTypeColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("41c2a3bb-0ebe-ed83-4508-ef8448c8e7b7", "Container Code");
			containerTypeColumnStyleInfo.ColumnName = "RCA_RC_ContainerType";
			containerTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			containerTypeColumnStyleInfo.IsReadOnly = true;

			serviceStringColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("51d4af21-456a-d499-47cd-9ce0776380c8", "Service String");
			serviceStringColumnStyleInfo.ColumnName = "RCA_Calc_ServiceString";
			serviceStringColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			serviceStringColumnStyleInfo.IsReadOnly = true;

			containerClassColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("fff9f990-02ac-ddaf-4fcf-9192c660a92f", "Container Class");
			containerClassColumnStyleInfo.ColumnName = "RCA_StorageOrFreightRateClass";
			containerClassColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			containerClassColumnStyleInfo.IsReadOnly = true;

			namedAccountsColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("3cc5972d-6c61-3683-49ca-8458205d1c8e", "Named Account Clients");
			namedAccountsColumnStyleInfo.ColumnName = "NamedAccountsFormatted";
			namedAccountsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			namedAccountsColumnStyleInfo.IsReadOnly = true;

			agentsColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("9048899c-8cbf-cf8f-499e-3764c028fcf3", "Agents");
			agentsColumnStyleInfo.ColumnName = "AgentsFormatted";
			agentsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			agentsColumnStyleInfo.IsReadOnly = true;

			linkedScheduleColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("c56aa330-8c42-49db-8204-14cec4ba97d1", "Linked Schedule");
			linkedScheduleColumnStyleInfo.ColumnName = "LinkedSchedule";
			linkedScheduleColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleColumnStyleInfo.IsReadOnly = true;
			linkedScheduleColumnStyleInfo.GroupName = Enterprise.ContractManagement.Module.Res.GetData("d281eb4f-cfdd-4c87-841d-9b0c10cb020b", "Linked Schedule");

			linkedScheduleETDColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("fe58fa7d-ea43-45f9-840e-62de8d5987ca", "Linked Schedule ETD");
			linkedScheduleETDColumnStyleInfo.ColumnName = "LinkedScheduleETD";
			linkedScheduleETDColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleETDColumnStyleInfo.IsReadOnly = true;
			linkedScheduleETDColumnStyleInfo.GroupName = Enterprise.ContractManagement.Module.Res.GetData("d281eb4f-cfdd-4c87-841d-9b0c10cb020b", "Linked Schedule");

			linkedScheduleETDUpdatedColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("bc0690ac-7e58-44bd-81ff-11cd9142577c", "Linked Schedule ETD Updated");
			linkedScheduleETDUpdatedColumnStyleInfo.ColumnName = "LinkedScheduleETDUpdated";
			linkedScheduleETDUpdatedColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			linkedScheduleETDUpdatedColumnStyleInfo.IsReadOnly = true;
			linkedScheduleETDUpdatedColumnStyleInfo.GroupName = Enterprise.ContractManagement.Module.Res.GetData("d281eb4f-cfdd-4c87-841d-9b0c10cb020b", "Linked Schedule");

			linkedScheduleSTDColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("465561c4-bc1f-4908-b0b2-708cdbc754c8", "Linked Schedule STD");
			linkedScheduleSTDColumnStyleInfo.ColumnName = "LinkedScheduleSTD";
			linkedScheduleSTDColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleSTDColumnStyleInfo.IsReadOnly = true;
			linkedScheduleSTDColumnStyleInfo.GroupName = Enterprise.ContractManagement.Module.Res.GetData("d281eb4f-cfdd-4c87-841d-9b0c10cb020b", "Linked Schedule");

			linkedScheduleETAColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("083bcb3c-482a-47b0-806b-2229aaded89f", "Linked Schedule ETA");
			linkedScheduleETAColumnStyleInfo.ColumnName = "LinkedScheduleETA";
			linkedScheduleETAColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleETAColumnStyleInfo.IsReadOnly = true;
			linkedScheduleETAColumnStyleInfo.GroupName = Enterprise.ContractManagement.Module.Res.GetData("d281eb4f-cfdd-4c87-841d-9b0c10cb020b", "Linked Schedule");

			linkedScheduleSTAColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("b57c92c4-4ed1-4592-bd14-61b6d37c0e9f", "Linked Schedule STA");
			linkedScheduleSTAColumnStyleInfo.ColumnName = "LinkedScheduleSTA";
			linkedScheduleSTAColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleSTAColumnStyleInfo.IsReadOnly = true;
			linkedScheduleSTAColumnStyleInfo.GroupName = Enterprise.ContractManagement.Module.Res.GetData("d281eb4f-cfdd-4c87-841d-9b0c10cb020b", "Linked Schedule");

			gatewayConsolColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("26df74c9-6dc7-e89e-40ee-8008d7acde4e", "Gateway Consol");
			gatewayConsolColumnStyleInfo.ColumnName = "RCA_AllowGatewayConsolOnly";
			gatewayConsolColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			gatewayConsolColumnStyleInfo.IsReadOnly = true;

			groupageContainerModeColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("592f024f-7298-4c56-b9d7-2204626ca08c", "Groupage Container Mode");
			groupageContainerModeColumnStyleInfo.ColumnName = "RCA_AllowGroupageOnly";
			groupageContainerModeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			groupageContainerModeColumnStyleInfo.IsReadOnly = true;

			shipperOwnedContainerColumnStyle.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("22bbac59-325a-f39f-4306-26849a2681c2", "Shipper Owned Container");
			shipperOwnedContainerColumnStyle.ColumnName = "RCA_ContainerOwner";
			shipperOwnedContainerColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
			shipperOwnedContainerColumnStyle.IsReadOnly = true;

			allowFreightSpotRateColumnInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("6f85edfd-f099-33a3-4354-1879900adf0e", "Allow Freight Spot Rate");
			allowFreightSpotRateColumnInfo.ColumnName = "RCA_AllowFreightSpotRate";
			allowFreightSpotRateColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			allowFreightSpotRateColumnInfo.IsReadOnly = true;

			tradeLaneCodeInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("0af0c5a3-fafb-438a-8d90-252f57e93f29", "Trade Lane");
			tradeLaneCodeInfo.ColumnName = "RCA_Calc_TradeLaneCode";
			tradeLaneCodeInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			tradeLaneCodeInfo.IsReadOnly = true;

			tradeLaneDescriptionInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("6dc2cc68-cd47-4f10-9850-f5d8ebdcfb65", "Trade Lane Name");
			tradeLaneDescriptionInfo.ColumnName = "RCA_Calc_TradeLaneDescription";
			tradeLaneDescriptionInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			tradeLaneDescriptionInfo.IsReadOnly = true;

			containerWeightLimitInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("0704ebe2-f368-476e-8837-ee741fb28c78", "Container Weight Limit");
			containerWeightLimitInfo.ColumnName = "RCA_ContainerWeightLimit";
			containerWeightLimitInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			containerWeightLimitInfo.IsReadOnly = true;
			containerWeightLimitInfo.GroupName = Enterprise.ContractManagement.Module.Res.GetData("5c484fe7-9500-460a-a60b-2ed1e3b1b74b", "Container Weight Limit");

			containerWeightLimitUnitInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("df27ced8-ced8-49f1-83f7-03ce92007d53", "Container Weight Limit Unit");
			containerWeightLimitUnitInfo.ColumnName = "RCA_ContainerWeightLimitUQ";
			containerWeightLimitUnitInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			containerWeightLimitUnitInfo.IsReadOnly = true;
			containerWeightLimitUnitInfo.GroupName = Enterprise.ContractManagement.Module.Res.GetData("5c484fe7-9500-460a-a60b-2ed1e3b1b74b", "Container Weight Limit");

			containerWeightLimitTypeInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("91801ea4-2224-42a4-8ccb-76203c4c0f31", "Container Weight Limit Type");
			containerWeightLimitTypeInfo.ColumnName = "RCA_ContainerWeightLimitType";
			containerWeightLimitTypeInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			containerWeightLimitTypeInfo.IsReadOnly = true;
			containerWeightLimitTypeInfo.GroupName = Enterprise.ContractManagement.Module.Res.GetData("5c484fe7-9500-460a-a60b-2ed1e3b1b74b", "Container Weight Limit");

			placeOfReceiptInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("33160be3-2df4-42ab-4214-bb48d4f0ec3e", "Place of Receipt");
			placeOfReceiptInfo.ColumnName = "RCA_PlaceOfReceipt";
			placeOfReceiptInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			placeOfReceiptInfo.IsReadOnly = true;

			placeOfDeliveryInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("33160be3-2df4-42ab-4214-bb48d4f0ec3e", "Place of Delivery");
			placeOfDeliveryInfo.ColumnName = "RCA_PlaceOfDelivery";
			placeOfDeliveryInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			placeOfDeliveryInfo.IsReadOnly = true;

			priorityColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("1814bab7-b326-3e8e-417a-ac5d62adc9af", "Priority");
			priorityColumnStyleInfo.ColumnName = "RCA_Priority";
			priorityColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			priorityColumnStyleInfo.IsReadOnly = true;

			this.grid.ColumnStyles.Add(allocationIDTextColumnStyleInfo);
			this.grid.ColumnStyles.Add(startDateColumnStyleInfo);
			this.grid.ColumnStyles.Add(expiryDateColumnStyleInfo);
			this.grid.ColumnStyles.Add(loadPortColumnStyleInfo);
			this.grid.ColumnStyles.Add(dischargePortColumnStyleInfo);
			this.grid.ColumnStyles.Add(voyageNumberColumnStyleInfo);
			this.grid.ColumnStyles.Add(vesselNameColumnStyleInfo);
			this.grid.ColumnStyles.Add(containerTypeColumnStyleInfo);
			this.grid.ColumnStyles.Add(namedAccountsColumnStyleInfo);
			this.grid.ColumnStyles.Add(serviceStringColumnStyleInfo);
			this.grid.ColumnStyles.Add(linkedScheduleColumnStyleInfo);
			this.grid.ColumnStyles.Add(linkedScheduleETDColumnStyleInfo);
			this.grid.ColumnStyles.Add(linkedScheduleETDUpdatedColumnStyleInfo);
			this.grid.ColumnStyles.Add(linkedScheduleSTDColumnStyleInfo);
			this.grid.ColumnStyles.Add(linkedScheduleETAColumnStyleInfo);
			this.grid.ColumnStyles.Add(linkedScheduleSTAColumnStyleInfo);
			this.grid.ColumnStyles.Add(gatewayConsolColumnStyleInfo);
			this.grid.ColumnStyles.Add(groupageContainerModeColumnStyleInfo);
			this.grid.ColumnStyles.Add(shipperOwnedContainerColumnStyle);
			this.grid.ColumnStyles.Add(agentsColumnStyleInfo);
			this.grid.ColumnStyles.Add(tradeLaneCodeInfo);
			this.grid.ColumnStyles.Add(tradeLaneDescriptionInfo);

			if (FreightConfigurationRegistry.Instance.EnableFreightSpotRateOnAllocationRoutes.Value)
			{
				this.grid.ColumnStyles.Add(allowFreightSpotRateColumnInfo);
			}

			if (FreightConfigurationRegistry.Instance.EnablePrioritySupportOnAllocationRoutes.Value)
			{
				this.grid.ColumnStyles.Add(priorityColumnStyleInfo);
			}

			if (FreightConfigurationRegistry.Instance.EnableContainerWeightLimitSupportOnAllocationRoutes.Value)
			{
				this.grid.ColumnStyles.Add(containerWeightLimitInfo);
				this.grid.ColumnStyles.Add(containerWeightLimitUnitInfo);
				this.grid.ColumnStyles.Add(containerWeightLimitTypeInfo);
			}

			if (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.Value)
			{
				this.grid.ColumnStyles.Add(placeOfDeliveryInfo);
				this.grid.ColumnStyles.Add(placeOfReceiptInfo);
			}

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();

			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 592, true);

			// 
			// AllocationRouteFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "AllocationRouteFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 595, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
