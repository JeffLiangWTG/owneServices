using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IDeliveryOrderProviderFactory
	{
		IDeliveryOrderProvider CreateDeliveryOrderProvider(EnterpriseBusinessObject enterpriseBusinessObject);
	}
}
