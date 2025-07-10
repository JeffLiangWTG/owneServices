using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderLineLinkManager : IOrderLineLinkManager
	{
		public void AllocatePackedItemLink(OrderLine orderLine, UniversalOrderLine universalOrderLine)
		{
			if (universalOrderLine.Link.HasValue && !linkToOrderLineMap.ContainsKey(universalOrderLine.Link.Value))
			{
				linkToOrderLineMap.Add(universalOrderLine.Link.Value, orderLine.PK);
			}
		}

		public void SetPackProductLink(PackProduct packProduct, PackedItem packedItem)
		{
			if (packedItem.OrderLineLink.HasValue && linkToOrderLineMap.ContainsKey(packedItem.OrderLineLink.Value))
			{
				packProduct.D2_JO = linkToOrderLineMap[packedItem.OrderLineLink.Value];
			}
		}

		public void AllocateOrderLineLink(OrderLine orderLine, UniversalOrderLine universalOrderLine)
		{
			var link = GetLink(orderLine.PK);
			universalOrderLine.Link = link;
		}

		public void AllocatePackedItemLink(PackProduct packProduct, PackedItem packedItem)
		{
			var link = GetLink(packProduct.D2_JO);
			packedItem.OrderLineLink = link;
		}

		int GetLink(ZGuid pk)
		{
			int link;

			if (!orderLineToLinkMap.TryGetValue(pk, out link) && pk.IsValid)
			{
				link = lastLink++;
				orderLineToLinkMap.Add(pk, link);
			}

			return link;
		}

		readonly Dictionary<int, ZGuid> linkToOrderLineMap = new Dictionary<int, ZGuid>();
		readonly Dictionary<ZGuid, int> orderLineToLinkMap = new Dictionary<ZGuid, int>();
		int lastLink = 1;
	}
}
