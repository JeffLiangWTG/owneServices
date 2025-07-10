using System.Collections;
using System.Collections.Generic;
using Enterprise.Customs.Common;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	class OrderLineComparer : IComparer<OrderLine>, IComparer, IComparer<IChargeApportionee>
	{
		#region IComparer<OrderLine> Members

		int IComparer<OrderLine>.Compare(OrderLine x, OrderLine y)
		{
			return CompareCore(x, y);
		}

		#endregion

		#region IComparer Members

		int IComparer.Compare(object x, object y)
		{
			return CompareCore((OrderLine)x, (OrderLine)y);
		}

		int CompareCore(OrderLine x, OrderLine y)
		{
			return x.JO_LineNoAndSplitAndSubLine.CompareTo(y.JO_LineNoAndSplitAndSubLine);
		}

		#endregion

		int IComparer<IChargeApportionee>.Compare(IChargeApportionee x, IChargeApportionee y)
		{
			return CompareCore((OrderLine)x, (OrderLine)y);
		}
	}
}
