using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	static class ILElectronicMessageProviderFactory
	{
		public static IILElectronicMessageProvider CreateILElectronicMessageProvider(IILElectronicMessageProvider messageProvider, EnterpriseBusinessObject enterpriseBusinessObject)
		{
			switch (messageProvider)
			{
				case IDeliveryOrderProvider:
					return (enterpriseBusinessObject as ForwardingShipment).DeliveryOrderProvider;
				case IGatePassMovementProvider:
					return (enterpriseBusinessObject as IGatePassMovementProviderFactory).GatePassMovementProvider;
				default:
					return null;
			}
		}
	}
}
