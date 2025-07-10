using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentStatusList : CodeDescriptionPairList
	{
		public AgencyShipmentStatusList(bool confirmed)
		{
			if (confirmed)
			{
				AddPair(ShipmentStatusList.Codes.ElectronicShippingInstruction, ShipmentStatusList.Descriptions.ElectronicShippingInstruction);
				AddPair(ShipmentStatusList.Codes.Confirmed, ShipmentStatusList.Descriptions.Confirmed);
				if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
				{
					AddPair(ShipmentStatusList.Codes.SIRejected, ShipmentStatusList.Descriptions.SIRejected);
				}
				AddPair(ShipmentStatusList.Codes.WebFwdInstruction, ShipmentStatusList.Descriptions.WebFwdInstruction);
			}
			else
			{
				AddPair(ShipmentStatusList.Codes.ElectronicBooking, ShipmentStatusList.Descriptions.ElectronicBooking);
				if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
				{
					AddPair(ShipmentStatusList.Codes.EBookingCancellationRequest, ShipmentStatusList.Descriptions.EBookingCancellationRequest);
				}
				AddPair(ShipmentStatusList.Codes.Booked, ShipmentStatusList.Descriptions.Booked);
				if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
				{
					AddPair(ShipmentStatusList.Codes.BookingCancelled, ShipmentStatusList.Descriptions.BookingCancelled);
					AddPair(ShipmentStatusList.Codes.BookingRejected, ShipmentStatusList.Descriptions.BookingRejected);
				}
				AddPair(ShipmentStatusList.Codes.WebBooking, ShipmentStatusList.Descriptions.WebBooking);
				AddPair(ShipmentStatusList.Codes.WaitListed, ShipmentStatusList.Descriptions.WaitListed);
			}
		}

		public AgencyShipmentStatusList(bool confirmed, bool isReceivedElectronicBooking, bool isReceivedElectronicShippingInstruction, bool isCancelled)
		{
			if (confirmed)
			{
				if (isReceivedElectronicShippingInstruction)
				{
					AddPair(ShipmentStatusList.Codes.ElectronicShippingInstruction, ShipmentStatusList.Descriptions.ElectronicShippingInstruction);
					AddPair(ShipmentStatusList.Codes.Confirmed, ShipmentStatusList.Descriptions.Confirmed);
					AddPair(ShipmentStatusList.Codes.SIRejected, ShipmentStatusList.Descriptions.SIRejected);
				}
				else
				{
					AddPair(ShipmentStatusList.Codes.ElectronicShippingInstruction, ShipmentStatusList.Descriptions.ElectronicShippingInstruction);
					AddPair(ShipmentStatusList.Codes.Confirmed, ShipmentStatusList.Descriptions.Confirmed);
					AddPair(ShipmentStatusList.Codes.SIRejected, ShipmentStatusList.Descriptions.SIRejected);
					AddPair(ShipmentStatusList.Codes.WebFwdInstruction, ShipmentStatusList.Descriptions.WebFwdInstruction);
				}
			}
			else
			{
				if (isReceivedElectronicBooking)
				{
					AddPair(ShipmentStatusList.Codes.ElectronicBooking, ShipmentStatusList.Descriptions.ElectronicBooking);
					AddPair(ShipmentStatusList.Codes.Booked, ShipmentStatusList.Descriptions.Booked);
					AddPair(ShipmentStatusList.Codes.BookingRejected, ShipmentStatusList.Descriptions.BookingRejected);
				}
				else
				{
					AddPair(ShipmentStatusList.Codes.Booked, ShipmentStatusList.Descriptions.Booked);
					AddPair(ShipmentStatusList.Codes.WaitListed, ShipmentStatusList.Descriptions.WaitListed);
					AddPair(ShipmentStatusList.Codes.WebBooking, ShipmentStatusList.Descriptions.WebBooking);
				}

				if (isCancelled)
				{
					AddPairIfNotExist(ShipmentStatusList.Codes.BookingRejected, ShipmentStatusList.Descriptions.BookingRejected);
				}
			}
		}
	}
}
