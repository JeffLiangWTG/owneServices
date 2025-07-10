using Enterprise.Edifact.Auto;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class RMXMessageBuilder : MessageBuilder
	{
		protected override SegmentGroup CreateMessage()
		{
			RMXMessage message = new RMXMessage();
			message.RMX[0].MessageVersion = "2";
			FillMessageDetail(message.MSG[0]);
			FillShipmentDetail(message.SHD[0]);
			return message;
		}

		void FillShipmentDetail(RMXSHDSegment sHDSegment)
		{
			ShipmentInformationProvider provider = new ShipmentInformationProvider(Shipment);
			sHDSegment.Forwarder = ConvertAndCheckValue(provider.Forwarder, true, DataTypeDefinitions.Parties_ForwarderId);
			sHDSegment.HouseBillDate = ConvertAndCheckValue(provider.HouseBillDate, true, DataTypeDefinitions.Date);
			sHDSegment.HouseBillId = ConvertAndCheckValue(provider.HouseBillId, true, DataTypeDefinitions.ShipmentIds_ShipmentId);
		}
	}
}
