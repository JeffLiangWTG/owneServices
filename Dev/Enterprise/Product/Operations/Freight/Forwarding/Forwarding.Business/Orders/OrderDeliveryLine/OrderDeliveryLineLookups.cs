using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderDeliveryLineLookups : ZLookups
	{
		public OrderDeliveryLineLookups(OrderDeliveryLine parent)
			: base(parent)
		{
		}

		public new OrderDeliveryLine Parent
		{
			get { return (OrderDeliveryLine)base.Parent; }
		}

		#region Order Numbers

		public CodeDescriptionPairList OrderNumbers
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				foreach (Order order in Parent.PreAdvice.Orders)
				{
					list.AddPair(order.JD_OrderNumber);
				}
				return list;
			}
		}

		#endregion

		#region Order Number and Splits

		public CodeDescriptionPairList OrderNumberAndSplits
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				foreach (Order order in Parent.PreAdvice.Orders)
				{
					list.AddPair(order.JD_OrderNumberAndSplit);
				}

				return list;
			}
		}

		#endregion

		#region Order Line Numbers

		public CodeDescriptionPairList OrderLineNumbers
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				if (Parent.Order != null)
				{
					foreach (OrderLine line in Parent.Order.OrderLines)
					{
						list.AddPair(line.JO_LineNo.ToString());
					}
				}
				return list;
			}
		}

		#endregion
	}
}
