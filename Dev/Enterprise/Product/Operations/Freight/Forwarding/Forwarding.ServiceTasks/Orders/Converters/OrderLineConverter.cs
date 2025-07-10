using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	class OrderLineConverter
	{
		public void FillPackLine(ForwardingPackLine packLine, OrderLine orderLine, decimal packQuantity)
		{
			packLine.JL_LinePrice = packLine.Shipment.CalculatePackLinePrice(orderLine.Order?.OrderCurrency, orderLine.JO_ItemPrice, packQuantity);

			var packProduct = packLine.Products.AddNew();
			packProduct.D2_JO = orderLine.PK;
			packProduct.D2_ProductQuantity = packQuantity;
			packProduct.D2_ProductUnitOfQty = orderLine.JO_F3_NKPackType;
		}
	}
}
