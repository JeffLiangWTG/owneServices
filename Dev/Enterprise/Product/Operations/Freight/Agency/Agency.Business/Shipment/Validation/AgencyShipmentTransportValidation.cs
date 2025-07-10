using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyShipmentTransportValidation : ShipmentTransportValidation
	{
		public AgencyShipmentTransportValidation(Transport transport)
			: base(transport)
		{
		}

		protected override void CheckJW_OA_ArrivalLocation()
		{
			base.CheckJW_OA_ArrivalLocation();

			var shipment = Parent.Parent as AgencyShipment;
			if (shipment != null && shipment.DeliveryRoadOrRailLeg == Parent && !Parent.JW_OA_ArrivalLocation.IsEmpty)
			{
				var docAddress = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneePickupDeliveryAddress);
				if (docAddress != null && !docAddress.E2_OA_Address.IsEmpty && docAddress.E2_OA_Address != Parent.JW_OA_ArrivalLocation)
				{
					Parent.JW_OA_ArrivalLocationInfo.AddWarning(Res.GetString("894d57e4-d1c3-476a-b0bd-a71bcec8cdea", "Address entered here does not match with delivery address on the Addresses tab. You may want to synchronize the data in order to create a correct Transport Booking."));
				}
			}
		}

		protected override void CheckJW_OA_DepartureLocation()
		{
			base.CheckJW_OA_DepartureLocation();

			var shipment = Parent.Parent as AgencyShipment;
			if (shipment != null && shipment.PickupRoadOrRailLeg == Parent && !Parent.JW_OA_DepartureLocation.IsEmpty)
			{
				var docAddress = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
				if (docAddress != null && !docAddress.E2_OA_Address.IsEmpty && docAddress.E2_OA_Address != Parent.JW_OA_DepartureLocation)
				{
					Parent.JW_OA_DepartureLocationInfo.AddWarning(Res.GetString("21b52d0f-a03f-4d50-af0b-b7e368246cab", "Address entered here does not match with pickup address on the Addresses tab. You may want to synchronize the data in order to create a correct Transport Booking."));
				}
			}
		}
	}
}


