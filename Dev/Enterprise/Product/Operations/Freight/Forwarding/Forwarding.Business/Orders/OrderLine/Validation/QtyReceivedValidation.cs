using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class QtyReceivedEqualsQtyOrderedValidation : ValidationProvider
	{
		public QtyReceivedEqualsQtyOrderedValidation(OrderLine orderLine) : base(orderLine)
		{
			this.OrderLine = orderLine;
		}

		public void CheckQtyReceivedEqualsQtyOrdered()
		{
			if (!OrderLine.JO_Quantity.IsEmpty && !OrderLine.JO_QtyReceived.IsEmpty)
			{
				decimal qtyOrdered = OrderLine.JO_Quantity;
				decimal qtyReceived = OrderLine.JO_QtyReceived;

				if (qtyReceived > 0) // no need to validate if order has not yet been fullfilled
				{
					if (qtyReceived > qtyOrdered)
					{
						string message = Res.GetString("041e4971-7ea7-4e0b-b17c-42f06f0900c2", "The {0} is greater than the {1}.", OrderLine.JO_QtyReceivedInfo.Description, OrderLine.JO_QuantityInfo.Description);
						OrderLine.JO_QtyReceivedInfo.AddWarning(message);
					}
					else if (qtyReceived < qtyOrdered)
					{
						string message = Res.GetString("c40e4e41-9ae1-4140-86e0-4318fe95804c", "The {0} is less than the {1}.", OrderLine.JO_QtyReceivedInfo.Description, OrderLine.JO_QuantityInfo.Description);
						OrderLine.JO_QtyReceivedInfo.AddWarning(message);
					}
				}
			}
		}

		#region Implementation

		protected OrderLine OrderLine;

		#endregion
	}
}
