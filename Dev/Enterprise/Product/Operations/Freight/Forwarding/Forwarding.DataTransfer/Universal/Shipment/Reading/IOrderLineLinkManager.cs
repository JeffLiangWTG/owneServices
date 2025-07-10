using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public interface IOrderLineLinkManager
	{
		void AllocatePackedItemLink(OrderLine orderLine, UniversalDataBuss.DataObjects.Universal.OrderLine universalOrderLine);
		void SetPackProductLink(PackProduct packProduct, PackedItem packedItem);
		void AllocateOrderLineLink(OrderLine orderLine, UniversalDataBuss.DataObjects.Universal.OrderLine universalOrderLine);
		void AllocatePackedItemLink(PackProduct packProduct, PackedItem packedItem);
	}
}
