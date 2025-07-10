using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.GUI
{
	public class AllocationRouteGrid : ZGrid
	{
		public AllocationRouteGrid()
		{
			InitializeColumns();
		}

		void InitializeColumns()
		{
			ZTextBoxColumnStyleInfo allocationIDTextColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			allocationIDTextColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("fe162e1c-2b32-4d20-aa23-43dc81374c40", "Allocation ID");
			allocationIDTextColumnStyleInfo.ColumnName = "RCA_AllocationLineID";
			allocationIDTextColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			allocationIDTextColumnStyleInfo.IsReadOnly = true;

			ZDateEditColumnStyleInfo startDateColumnStyleInfo = new ZDateEditColumnStyleInfo();
			startDateColumnStyleInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			startDateColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("8fc6b117-c64d-4afe-ac3c-24e27219915c", "Start Date");
			startDateColumnStyleInfo.ColumnName = "RCA_StartDate";
			startDateColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			startDateColumnStyleInfo.IsReadOnly = true;

			ZDateEditColumnStyleInfo expiryDateColumnStyleInfo = new ZDateEditColumnStyleInfo();
			expiryDateColumnStyleInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			expiryDateColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("18445476-900b-477d-96bb-4e9d6509baa4", "Expiry Date");
			expiryDateColumnStyleInfo.ColumnName = "RCA_ExpiryDate";
			expiryDateColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			expiryDateColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo loadPortColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			loadPortColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("5ba38111-8896-44de-a961-51e2d233722e", "Load Port");
			loadPortColumnStyleInfo.ColumnName = "RCA_Calc_LoadLocation";
			loadPortColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			loadPortColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo dischargePortColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			dischargePortColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("191d634a-35db-4ef3-8cd4-e97ded8947fe", "Discharge Port");
			dischargePortColumnStyleInfo.ColumnName = "RCA_Calc_DischargeLocation";
			dischargePortColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			dischargePortColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo placeOfReceiptStyleInfo = new ZTextBoxColumnStyleInfo();
			placeOfReceiptStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("eed2d148-5290-e8bd-4f6f-836639315d53", "Place of Receipt");
			placeOfReceiptStyleInfo.ColumnName = "RCA_PlaceOfReceipt";
			placeOfReceiptStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			placeOfReceiptStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo placeOfDeliverytStyleInfo = new ZTextBoxColumnStyleInfo();
			placeOfDeliverytStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("2ec06356-5765-ab8a-4dc7-39b5188ef8ae", "Place of Delivery");
			placeOfDeliverytStyleInfo.ColumnName = "RCA_PlaceOfDelivery";
			placeOfDeliverytStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			placeOfDeliverytStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo allocatedQuantityColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			allocatedQuantityColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("ad12f9d7-d114-e1b6-4bff-12d1b3698497", "Quantity");
			allocatedQuantityColumnStyleInfo.ColumnName = "RCA_AllocatedQuantity";
			allocatedQuantityColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			allocatedQuantityColumnStyleInfo.IsReadOnly = true;
			allocatedQuantityColumnStyleInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("b347318a-210f-86b1-4b33-ebb23c02c69b", "Quantity");

			ZTextBoxColumnStyleInfo allocatedQuantityUQColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			allocatedQuantityUQColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("e50bce9c-be4d-75bd-4f93-f639aa03d4e0", "Unit");
			allocatedQuantityUQColumnStyleInfo.ColumnName = "RCA_AllocatedUQ";
			allocatedQuantityUQColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			allocatedQuantityUQColumnStyleInfo.IsReadOnly = true;
			allocatedQuantityUQColumnStyleInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("b347318a-210f-86b1-4b33-ebb23c02c69b", "Quantity");

			ZCheckBoxColumnStyleInfo allowRelatedUNLOCOsColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			allowRelatedUNLOCOsColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("8E00F0D4-0659-4584-8FE6-409E971D5283", "Allow Related UNLOCOs");
			allowRelatedUNLOCOsColumnStyleInfo.ColumnName = "RCA_AllowRelatedPorts";
			allowRelatedUNLOCOsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			allowRelatedUNLOCOsColumnStyleInfo.IsReadOnly = true;

			ZCheckBoxColumnStyleInfo bookingLimitColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			bookingLimitColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("7d41add3-aae7-04b8-4fc1-59edac6cc4c3", "Booking Limit");
			bookingLimitColumnStyleInfo.ColumnName = "RCA_HasBookingLimit";
			bookingLimitColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			bookingLimitColumnStyleInfo.IsReadOnly = true;

			ZCheckBoxColumnStyleInfo gatewayConsolColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			gatewayConsolColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("5703e979-85e0-59a9-47df-a582d69c812b", "Gateway Consol");
			gatewayConsolColumnStyleInfo.ColumnName = "RCA_AllowGatewayConsolOnly";
			gatewayConsolColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			gatewayConsolColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo shipperOwnedContainerColumnStyle = new ZTextBoxColumnStyleInfo();
			shipperOwnedContainerColumnStyle.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("92693fa1-3ccf-9d85-4028-2834ae33cfd1", "Shipper Owned Container");
			shipperOwnedContainerColumnStyle.ColumnName = "RCA_ContainerOwner";
			shipperOwnedContainerColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
			shipperOwnedContainerColumnStyle.IsReadOnly = true;

			ZCheckBoxColumnStyleInfo groupageContainerModeColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			groupageContainerModeColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("20f63333-e6a0-4a9b-96ee-0969ffdeea1c", "Groupage Container Mode");
			groupageContainerModeColumnStyleInfo.ColumnName = "RCA_AllowGroupageOnly";
			groupageContainerModeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			groupageContainerModeColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo bookingVarianceColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			bookingVarianceColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("fd122e13-f32f-d9b6-48a0-930e0ca77134", "Booking Variance");
			bookingVarianceColumnStyleInfo.ColumnName = "RCA_BookingVariance";
			bookingVarianceColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			bookingVarianceColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo voyageNumberColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			voyageNumberColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("959f2920-d3bb-4261-ae95-4f44f2308463", "Voyage");
			voyageNumberColumnStyleInfo.ColumnName = "RCA_Calc_VoyageNumber";
			voyageNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			voyageNumberColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo vesselNameColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			vesselNameColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("3a57721a-9edf-4770-9e61-86ffa95374dd", "Vessel");
			vesselNameColumnStyleInfo.ColumnName = "RCA_Calc_VesselName";
			vesselNameColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			vesselNameColumnStyleInfo.IsReadOnly = true;

			ZGuidFindBoxColumnStyleInfo containerTypeColumnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
			containerTypeColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("a2acc3ba-b640-48ba-a672-000de1fb09c1", "Container Code");
			containerTypeColumnStyleInfo.ColumnName = "RCA_RC_ContainerType";
			containerTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			containerTypeColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo serviceStringColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			serviceStringColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("aa366b17-f16c-4862-8ac9-fbf84ddf7f99", "Service String");
			serviceStringColumnStyleInfo.ColumnName = "RCA_Calc_ServiceString";
			serviceStringColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			serviceStringColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo containerClassColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			containerClassColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("7ba4c073-7099-4cd8-89f2-461bd421d6c0", "Container Class");
			containerClassColumnStyleInfo.ColumnName = "RCA_StorageOrFreightRateClass";
			containerClassColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			containerClassColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo namedAccountsColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			namedAccountsColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("ce073546-3629-4f40-97d4-cafe7e26fc8b", "Named Account Clients");
			namedAccountsColumnStyleInfo.ColumnName = "NamedAccountsFormatted";
			namedAccountsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			namedAccountsColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo agentsColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			agentsColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("4be9750e-9e5d-6dae-497f-ffba7b2dcb31", "Agents");
			agentsColumnStyleInfo.ColumnName = "AgentsFormatted";
			agentsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			agentsColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo priorityColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			priorityColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("bd04d60e-b118-b898-43cb-a9f3affee70d", "Priority");
			priorityColumnStyleInfo.ColumnName = "RCA_Priority";
			priorityColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			priorityColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo utilizationColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			utilizationColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("aa85439e-a594-f3bd-471a-88fdf322e45c", "Utilization");
			utilizationColumnStyleInfo.ColumnName = "Utilization";
			utilizationColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			utilizationColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo capacityWithVarianceColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			capacityWithVarianceColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("741c1909-2575-9692-4d98-0ec920e8c5d2", "Capacity with Variance");
			capacityWithVarianceColumnStyleInfo.ColumnName = "CapacityWithVariance";
			capacityWithVarianceColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			capacityWithVarianceColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo outstandingCommittedColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outstandingCommittedColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("3556c233-e64e-dd87-46a4-2efa87d591cf", "Outstanding Committed");
			outstandingCommittedColumnStyleInfo.ColumnName = "OutstandingCommitted";
			outstandingCommittedColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			outstandingCommittedColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo outstandingWithVarianceColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outstandingWithVarianceColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("96fd1054-44da-e0be-4806-21beb84624f5", "Outstanding with Variance");
			outstandingWithVarianceColumnStyleInfo.ColumnName = "OutstandingWithVariance";
			outstandingWithVarianceColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			outstandingWithVarianceColumnStyleInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo linkedScheduleColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			linkedScheduleColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("4b83cd5a-e810-4476-9550-a1db00f351d7", "Linked Schedule");
			linkedScheduleColumnStyleInfo.ColumnName = "LinkedSchedule";
			linkedScheduleColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleColumnStyleInfo.IsReadOnly = true;
			linkedScheduleColumnStyleInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("e0c12ac8-fcbd-405b-b186-8696547e74c1", "Linked Schedule");

			ZDateEditColumnStyleInfo linkedScheduleETDColumnStyleInfo = new ZDateEditColumnStyleInfo();
			linkedScheduleETDColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("e3a35518-8a35-47c4-b5af-228add3d6acc", "Linked Schedule ETD");
			linkedScheduleETDColumnStyleInfo.ColumnName = "LinkedScheduleETD";
			linkedScheduleETDColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleETDColumnStyleInfo.IsReadOnly = true;
			linkedScheduleETDColumnStyleInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("e0c12ac8-fcbd-405b-b186-8696547e74c1", "Linked Schedule");

			ZCheckBoxColumnStyleInfo linkedScheduleETDUpdatedColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			linkedScheduleETDUpdatedColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("2074c31b-49e3-451b-9392-de03c1bd210a", "Linked Schedule ETD Updated");
			linkedScheduleETDUpdatedColumnStyleInfo.ColumnName = "LinkedScheduleETDUpdated";
			linkedScheduleETDUpdatedColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			linkedScheduleETDUpdatedColumnStyleInfo.IsReadOnly = true;
			linkedScheduleETDUpdatedColumnStyleInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("e0c12ac8-fcbd-405b-b186-8696547e74c1", "Linked Schedule");

			ZDateEditColumnStyleInfo linkedScheduleSTDColumnStyleInfo = new ZDateEditColumnStyleInfo();
			linkedScheduleSTDColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("755c1bc9-d852-4489-8b0a-328523100344", "Linked Schedule STD");
			linkedScheduleSTDColumnStyleInfo.ColumnName = "LinkedScheduleSTD";
			linkedScheduleSTDColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleSTDColumnStyleInfo.IsReadOnly = true;
			linkedScheduleSTDColumnStyleInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("e0c12ac8-fcbd-405b-b186-8696547e74c1", "Linked Schedule");

			ZDateEditColumnStyleInfo linkedScheduleETAColumnStyleInfo = new ZDateEditColumnStyleInfo();
			linkedScheduleETAColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("1e1b3e7b-8b53-481e-868c-dcd98ecf3246", "Linked Schedule ETA");
			linkedScheduleETAColumnStyleInfo.ColumnName = "LinkedScheduleETA";
			linkedScheduleETAColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleETAColumnStyleInfo.IsReadOnly = true;
			linkedScheduleETAColumnStyleInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("e0c12ac8-fcbd-405b-b186-8696547e74c1", "Linked Schedule");

			ZDateEditColumnStyleInfo linkedScheduleSTAColumnStyleInfo = new ZDateEditColumnStyleInfo();
			linkedScheduleSTAColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("7a4c819d-73df-4f5b-a478-6243d13b4311", "Linked Schedule STA");
			linkedScheduleSTAColumnStyleInfo.ColumnName = "LinkedScheduleSTA";
			linkedScheduleSTAColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			linkedScheduleSTAColumnStyleInfo.IsReadOnly = true;
			linkedScheduleSTAColumnStyleInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("e0c12ac8-fcbd-405b-b186-8696547e74c1", "Linked Schedule");

			ZCheckBoxColumnStyleInfo allowFreightSpotRateColumnInfo = new ZCheckBoxColumnStyleInfo();
			allowFreightSpotRateColumnInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("654dd69f-ec3f-1ba1-41fc-6228e6198305", "Allow Freight Spot Rate");
			allowFreightSpotRateColumnInfo.ColumnName = "RCA_AllowFreightSpotRate";
			allowFreightSpotRateColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			allowFreightSpotRateColumnInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo tradeLaneCodeInfo = new ZTextBoxColumnStyleInfo();
			tradeLaneCodeInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("7df4dfc9-e01c-4b29-9702-2d0094804d63", "Trade Lane");
			tradeLaneCodeInfo.ColumnName = "RCA_Calc_TradeLaneCode";
			tradeLaneCodeInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			tradeLaneCodeInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo tradeLaneDescriptionInfo = new ZTextBoxColumnStyleInfo();
			tradeLaneDescriptionInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("16936d60-49ed-401b-9e40-617891573c8a", "Trade Lane Name");
			tradeLaneDescriptionInfo.ColumnName = "RCA_Calc_TradeLaneDescription";
			tradeLaneDescriptionInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			tradeLaneDescriptionInfo.IsReadOnly = true;

			ZTextBoxColumnStyleInfo containerWeightLimitInfo = new ZTextBoxColumnStyleInfo();
			containerWeightLimitInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("cb0d15be-856f-4e91-881f-10e65837cc7f", "Container Weight Limit");
			containerWeightLimitInfo.ColumnName = "RCA_ContainerWeightLimit";
			containerWeightLimitInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			containerWeightLimitInfo.IsReadOnly = true;
			containerWeightLimitInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("d0b925c1-1883-4b58-964a-e9c8deed5de7", "Container Weight Limit");

			ZTextBoxColumnStyleInfo containerWeightLimitUnitInfo = new ZTextBoxColumnStyleInfo();
			containerWeightLimitUnitInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("967ded3e-14a7-4ec5-ba99-df01c7f90160", "Container Weight Limit Unit");
			containerWeightLimitUnitInfo.ColumnName = "RCA_ContainerWeightLimitUQ";
			containerWeightLimitUnitInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			containerWeightLimitUnitInfo.IsReadOnly = true;
			containerWeightLimitUnitInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("d0b925c1-1883-4b58-964a-e9c8deed5de7", "Container Weight Limit");

			ZTextBoxColumnStyleInfo containerWeightLimitTypeInfo = new ZTextBoxColumnStyleInfo();
			containerWeightLimitTypeInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("f6cfb1f6-2d28-4f53-995d-ad598d239376", "Container Weight Limit Type");
			containerWeightLimitTypeInfo.ColumnName = "RCA_ContainerWeightLimitType";
			containerWeightLimitTypeInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			containerWeightLimitTypeInfo.IsReadOnly = true;
			containerWeightLimitTypeInfo.GroupName = Enterprise.ContractManagement.GUI.Res.GetData("d0b925c1-1883-4b58-964a-e9c8deed5de7", "Container Weight Limit");

			ColumnStyles.Add(allocationIDTextColumnStyleInfo);
			ColumnStyles.Add(startDateColumnStyleInfo);
			ColumnStyles.Add(expiryDateColumnStyleInfo);
			ColumnStyles.Add(loadPortColumnStyleInfo);
			ColumnStyles.Add(dischargePortColumnStyleInfo);
			ColumnStyles.Add(voyageNumberColumnStyleInfo);
			ColumnStyles.Add(vesselNameColumnStyleInfo);
			ColumnStyles.Add(containerTypeColumnStyleInfo);
			ColumnStyles.Add(namedAccountsColumnStyleInfo);
			ColumnStyles.Add(serviceStringColumnStyleInfo);
			ColumnStyles.Add(containerClassColumnStyleInfo);
			ColumnStyles.Add(allocatedQuantityColumnStyleInfo);
			ColumnStyles.Add(allocatedQuantityUQColumnStyleInfo);
			ColumnStyles.Add(allowRelatedUNLOCOsColumnStyleInfo);
			ColumnStyles.Add(bookingLimitColumnStyleInfo);
			ColumnStyles.Add(bookingVarianceColumnStyleInfo);
			ColumnStyles.Add(utilizationColumnStyleInfo);
			ColumnStyles.Add(capacityWithVarianceColumnStyleInfo);
			ColumnStyles.Add(outstandingCommittedColumnStyleInfo);
			ColumnStyles.Add(outstandingWithVarianceColumnStyleInfo);
			ColumnStyles.Add(linkedScheduleColumnStyleInfo);
			ColumnStyles.Add(linkedScheduleETDColumnStyleInfo);
			ColumnStyles.Add(linkedScheduleETDUpdatedColumnStyleInfo);
			ColumnStyles.Add(linkedScheduleSTDColumnStyleInfo);
			ColumnStyles.Add(linkedScheduleETAColumnStyleInfo);
			ColumnStyles.Add(linkedScheduleSTAColumnStyleInfo);
			ColumnStyles.Add(gatewayConsolColumnStyleInfo);
			ColumnStyles.Add(groupageContainerModeColumnStyleInfo);

			ColumnStyles.Add(shipperOwnedContainerColumnStyle);
			ColumnStyles.Add(agentsColumnStyleInfo);
			ColumnStyles.Add(tradeLaneCodeInfo);
			ColumnStyles.Add(tradeLaneDescriptionInfo);

			if (FreightConfigurationRegistry.Instance.EnableFreightSpotRateOnAllocationRoutes.Value)
			{
				ColumnStyles.Add(allowFreightSpotRateColumnInfo);
			}

			if (FreightConfigurationRegistry.Instance.EnablePrioritySupportOnAllocationRoutes.Value)
			{
				ColumnStyles.Add(priorityColumnStyleInfo);
			}

			if (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.Value)
			{
				ColumnStyles.Add(placeOfReceiptStyleInfo);
				ColumnStyles.Add(placeOfDeliverytStyleInfo);
			}

			if (FreightConfigurationRegistry.Instance.EnablePrioritySupportOnAllocationRoutes.Value)
			{
				ColumnStyles.Add(containerWeightLimitInfo);
				ColumnStyles.Add(containerWeightLimitUnitInfo);
				ColumnStyles.Add(containerWeightLimitTypeInfo);
			}
		}
	}
}
