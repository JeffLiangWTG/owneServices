using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public interface IAttachOrders : IShipmentWithDocsAndCartage
	{
		OrderCollection AttachedOrders { get; }
		OrderCollection PossibleOrdersForAttachment_List { get; }

		void SetDefaultsOnOrder(Order newOrder);
		void OnOrderAttached(Order attachedOrder);
	}
}
