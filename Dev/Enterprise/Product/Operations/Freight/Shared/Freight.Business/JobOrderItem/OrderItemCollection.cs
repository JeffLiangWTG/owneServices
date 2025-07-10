using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class OrderItemCollection : DependentBusinessObjectCollection<OrderItem, JobDocsAndCartage>
	{
		public OrderItemCollection(JobDocsAndCartage master, BusinessObjectFactory factory)
			: base(master, factory)
		{
			Sort(OrderItem.Schema.JT_Sequence, ListSortDirection.Ascending);
		}

		public OrderItem this[string orderReference]
		{
			get { return this.Cast<OrderItem>().FirstOrDefault(order => order.JT_OrderReference == orderReference); }
		}

		public void ApplyCurrentSortOrder()
		{
			short index = 1;
			foreach (OrderItem item in this)
			{
				item.JT_Sequence = index++;
			}
		}

		#region JP_OrderItemsAsString

		public ZString AsString
		{
			get
			{
				StringBuilder result = new StringBuilder();
				if (SortInformation.PropertyName != OrderItem.Schema.JT_Sequence)
				{
					Sort(OrderItem.Schema.JT_Sequence, ListSortDirection.Ascending);
				}
				foreach (OrderItem item in this)
				{
					if (result.Length > 0)
					{
						result.Append(",");
					}

					result.Append(item.JT_OrderReference);
				}
				return result.ToString();
			}
			set
			{
				RemoveAndDeleteAll();
				string[] references = value.ToString().Replace(" ", ",").Split(',');
				int sequence = 1;
				foreach (string reference in references)
				{
					if (!string.IsNullOrEmpty(reference.Trim()))
					{
						OrderItem item = AddNew();
						item.JT_OrderReference = new ZString(reference).SubstringSafe(0, 25).Trim();
						item.JT_Sequence = (ZShort)sequence++;
					}
				}
			}
		}

		#endregion

		#region Event handling

		public event EventHandler OrderReferenceChanged;

		protected virtual void OnOrderReferenceChanged(EventArgs e)
		{
			if (OrderReferenceChanged != null)
			{
				OrderReferenceChanged(this, e);
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((OrderItem)bizOAdded).JT_OrderReferenceInfo.ValueChanged -= new EventHandler(OnOrderReferenceInfo_ValueChanged);
			((OrderItem)bizOAdded).JT_OrderReferenceInfo.ValueChanged += new EventHandler(OnOrderReferenceInfo_ValueChanged);
			OnOrderReferenceChanged(EventArgs.Empty);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			((OrderItem)bizO).JT_OrderReferenceInfo.ValueChanged -= new EventHandler(OnOrderReferenceInfo_ValueChanged);
			OnOrderReferenceChanged(EventArgs.Empty);
		}

		void OnOrderReferenceInfo_ValueChanged(object sender, EventArgs e)
		{
			OnOrderReferenceChanged(EventArgs.Empty);
		}

		#endregion
	}
}
