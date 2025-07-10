using System;
using CargoWise.Common;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class AirBookingRequestProcessor
	{
		public AirBookingRequestProcessor(UShipment shipment, Func<UShipment, string> send)
		{
			Send = Argument.NotNull(send, nameof(send));
			Shipment = shipment;
		}

		public readonly Func<UShipment, string> Send;
		public readonly UShipment Shipment;

		public string Process() => Send(Shipment);
	}
}
