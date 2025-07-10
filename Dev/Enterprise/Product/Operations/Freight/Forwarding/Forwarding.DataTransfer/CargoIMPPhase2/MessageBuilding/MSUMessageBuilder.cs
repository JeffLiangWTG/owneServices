using CargoWise.Types;
using Enterprise.Edifact.Auto;
using Enterprise.Freight.Forwarding.Registry;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class MSUMessageBuilder : MessageBuilder
	{
		public ZString MilestoneStatusCode { get; set; }
		public ZDateTime MilestoneStatusDate { get; set; }

		protected override SegmentGroup CreateMessage()
		{
			MSUMessage message = new MSUMessage();
			message.MSU[0].MessageVersion = MessageVersion;
			FillMessageDetail(message.MSG[0]);
			FillShipmentDetail(message.SHD[0]);
			FillStatusData(message.STS[0]);
			return message;
		}

		void FillShipmentDetail(MSUSHDSegment sHDSegment)
		{
			ShipmentInformationProvider provider = new ShipmentInformationProvider(Shipment);
			sHDSegment.Forwarder = ConvertAndCheckValue(provider.Forwarder, true, DataTypeDefinitions.Parties_ForwarderId);
			sHDSegment.HouseBillDate = ConvertAndCheckValue(provider.HouseBillDate, true, DataTypeDefinitions.Date);
			sHDSegment.HouseBillId = ConvertAndCheckValue(provider.HouseBillId, true, DataTypeDefinitions.ShipmentIds_ShipmentId);
			sHDSegment.RoutingIdentifier = "";
			sHDSegment.Pieces = ConvertAndCheckValue(provider.Pieces, true, DataTypeDefinitions.Quantities_NumberOfPieces);

			bool weightMandatory = false;
			switch (MilestoneStatusCode)
			{
				case CargoIMPPhase2MSUEventCodeList.Codes.DEW:
				case CargoIMPPhase2MSUEventCodeList.Codes.REW:
					weightMandatory = true;
					break;
			}

			sHDSegment.Weight = ConvertAndCheckValue(provider.Weight, weightMandatory, DataTypeDefinitions.Quantities_Weight);

			bool weightUnitMandatory = weightMandatory | !string.IsNullOrEmpty(sHDSegment.Weight);
			sHDSegment.WeightUnit = ConvertAndCheckValue(provider.WeightUnit, weightUnitMandatory, DataTypeDefinitions.WeightUnitConverter);

			sHDSegment.Volume = ConvertAndCheckValue(provider.Volume, false, DataTypeDefinitions.Quantities_Volume);
			if (!string.IsNullOrEmpty(sHDSegment.Volume))
			{
				sHDSegment.VolumeUnit = ConvertAndCheckValue(provider.VolumeUnit, true, DataTypeDefinitions.VolumeUnitConverter);
			}
		}

		void FillStatusData(STSSegment sTSSegment)
		{
			sTSSegment.MilestoneStatusCode = ConvertAndCheckValue(
					new InformationResult<ZString>(MilestoneStatusCode,
							Res.GetString("b06ef21f-705d-47ff-bffe-4b1debe08d15", "Milestone Status Code")),
					true, DataTypeDefinitions.MilestoneStatusCodeConverter);
			sTSSegment.TimeIndicator = TimeIndicatorList.Local;
			sTSSegment.DateAndTime = ConvertAndCheckValue(new InformationResult<ZDateTime>(MilestoneStatusDate, Res.GetString("beb5c3f7-9345-4324-9470-2841cf8449b1", "Milestone Actual Date")), true, DataTypeDefinitions.DateTime);
			FillStatusPartiesAndLocation(sTSSegment);
		}

		void FillStatusPartiesAndLocation(STSSegment sTSSegment)
		{
			ShipmentInformationProvider provider = new ShipmentInformationProvider(Shipment);
			bool isLocationMandatory = false;
			InformationResult<ZString> location = InformationResult<ZString>.Empty;
			InformationResult<ZString> pickupDeliveryParty = InformationResult<ZString>.Empty;
			InformationResult<ZString> customer = InformationResult<ZString>.Empty;
			switch (MilestoneStatusCode)
			{
				case CargoIMPPhase2MSUEventCodeList.Codes.REW:
					location = provider.ExportWarehouse;
					customer = provider.ExportWarehouseCustomer;
					pickupDeliveryParty = provider.ExportWarehouseDeliveryParty;
					isLocationMandatory = true;
					break;

				case CargoIMPPhase2MSUEventCodeList.Codes.DEW:
					location = provider.ExportWarehouse;
					customer = provider.ExportWarehouseCustomer;
					pickupDeliveryParty = provider.ExportWarehousePickupParty;
					isLocationMandatory = true;
					break;

				case CargoIMPPhase2MSUEventCodeList.Codes.DOC:
					location = provider.ExportingCarriersTerminal;
					customer = provider.ExportingCarriersTerminalCustomer;
					pickupDeliveryParty = provider.ExportingCarriersTerminalDeliveryParty;
					isLocationMandatory = true;
					break;

				case CargoIMPPhase2MSUEventCodeList.Codes.RIW:
					location = provider.ImportWarehouse;
					customer = provider.ImportWarehouseCustomer;
					pickupDeliveryParty = provider.ImportWarehouseDeliveryParty;
					isLocationMandatory = true;
					break;

				case CargoIMPPhase2MSUEventCodeList.Codes.OFD:
					location = provider.ImportWarehouse;
					customer = provider.ImportWarehouseCustomer;
					pickupDeliveryParty = provider.ImportWarehousePickupParty;
					isLocationMandatory = true;
					break;

				case CargoIMPPhase2MSUEventCodeList.Codes.POD:
					location = provider.DeliverTo;
					customer = provider.DeliverToCustomer;
					pickupDeliveryParty = provider.DeliverToDeliveryParty;
					isLocationMandatory = false;
					break;
			}

			sTSSegment.StatusLocation = ConvertAndCheckValue(location, isLocationMandatory, DataTypeDefinitions.Locations_LocationId);
			sTSSegment.PickUpDeliveryCustomerId = ConvertAndCheckValue(customer, false, DataTypeDefinitions.Parties_InterestedPartyId);
			sTSSegment.PickUpDeliveryPartyId = ConvertAndCheckValue(pickupDeliveryParty, false, DataTypeDefinitions.Parties_InterestedPartyId);
		}
	}
}
