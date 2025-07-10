using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DummyShipmentAnnouncer : IShipmentAnnouncer
	{
		public void Announce(BusinessObject shipment)
		{
		}

		public bool IsNotificationRequiredIfDeliveryAddressChangedByFreight(BusinessObject shipment)
		{
			return true;
		}

		public string AdditionalNotificationText
		{
			get
			{
				return "Additional Text.";
			}
		}
	}
}
