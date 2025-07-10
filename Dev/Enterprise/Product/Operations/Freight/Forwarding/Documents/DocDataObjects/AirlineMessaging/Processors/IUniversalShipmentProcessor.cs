
namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging.Processors
{
	interface IUniversalShipmentProcessor
	{
		void Process(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment);
	}
}
