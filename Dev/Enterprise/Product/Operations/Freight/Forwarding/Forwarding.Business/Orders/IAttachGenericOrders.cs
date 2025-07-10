using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public interface IAttachGenericOrders : IShipmentWithDocsAndCartage
	{
		GenericOrderCollection GenericOrders { get; }
	}
}
