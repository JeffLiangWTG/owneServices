using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLinesTotalByProductCollection : NonPersistentBusinessObjectCollection<OrderLinesTotalByProduct>
	{
		public OrderLinesTotalByProductCollection(Order order, BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(order, "order");
			this.order = order;
			this.hookedOrderLines = new List<OrderLine>();
			this.isRefreshRequired = true;
			HookOrderEvents();
			SetReadOnlyIncludingChildren(true);
		}
		readonly Order order;
		readonly List<OrderLine> hookedOrderLines;
		bool isRefreshRequired;

		void HookOrderEvents()
		{
			order.OrderLines.CountChanged += OrderLines_CountChanged;
			HookOrderLinesEvents();
		}

		void HookOrderLinesEvents()
		{
			foreach (var orderLine in order.OrderLines)
			{
				orderLine.JO_PartnoInfo.ValueChanged += OrderLine_ValueChanged;
				orderLine.JO_QuantityInfo.ValueChanged += OrderLine_ValueChanged;
				orderLine.JO_QtyInvoicedInfo.ValueChanged += OrderLine_ValueChanged;
				orderLine.JO_QtyReceivedInfo.ValueChanged += OrderLine_ValueChanged;
				this.hookedOrderLines.Add(orderLine);
			}
		}

		void UnHookOrderLinesEvents()
		{
			foreach (var orderLine in this.hookedOrderLines)
			{
				orderLine.JO_PartnoInfo.ValueChanged -= OrderLine_ValueChanged;
				orderLine.JO_QuantityInfo.ValueChanged -= OrderLine_ValueChanged;
				orderLine.JO_QtyInvoicedInfo.ValueChanged -= OrderLine_ValueChanged;
				orderLine.JO_QtyReceivedInfo.ValueChanged -= OrderLine_ValueChanged;
			}

			this.hookedOrderLines.Clear();
		}

		void OrderLine_ValueChanged(object sender, System.EventArgs e)
		{
			this.isRefreshRequired = true;
		}

		void OrderLines_CountChanged(object sender, System.EventArgs e)
		{
			this.isRefreshRequired = true;
			UnHookOrderLinesEvents();
			if (!this.order.IsDeleting)
			{
				HookOrderLinesEvents();
			}
		}

		public void Populate()
		{
			if (this.isRefreshRequired)
			{
				RemoveAndDeleteAll();
				Dictionary<ZString, List<OrderLine>> orderLines = new Dictionary<ZString, List<OrderLine>>();
				foreach (OrderLine orderLine in this.order.OrderLines)
				{
					List<OrderLine> currentList = null;
					ZString key = orderLine.JO_Partno;
					if (!orderLines.ContainsKey(key))
					{
						currentList = new List<OrderLine>();
						orderLines.Add(key, currentList);
					}
					else
					{
						currentList = orderLines[key];
					}

					currentList.Add(orderLine);
				}

				foreach (var pair in orderLines)
				{
					var total = AddNew();
					total.Product = pair.Key;
					total.OrderLines = pair.Value;
				}

				this.isRefreshRequired = false;
			}
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrderLinesTotalByProduct(Factory);
		}

		#endregion
	}
}
