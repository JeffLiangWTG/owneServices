using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface IShipmentAnnouncer
	{
		void Announce(BusinessObject shipment);
		bool IsNotificationRequiredIfDeliveryAddressChangedByFreight(BusinessObject shipment);
		string AdditionalNotificationText { get; }
	}
}
