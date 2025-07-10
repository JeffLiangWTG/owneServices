using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class DeliveryOrderProviderFactory : IDeliveryOrderProviderFactory
	{
		IDeliveryOrderProvider IDeliveryOrderProviderFactory.CreateDeliveryOrderProvider(EnterpriseBusinessObject enterpriseBusinessObject)
		{
			switch (enterpriseBusinessObject)
			{
				case ForwardingShipment forwardingShipment:
					return new ForwardingShipmentDeliveryOrderProvider(forwardingShipment);
				default:
					return null;
			}
		}
	}
}
