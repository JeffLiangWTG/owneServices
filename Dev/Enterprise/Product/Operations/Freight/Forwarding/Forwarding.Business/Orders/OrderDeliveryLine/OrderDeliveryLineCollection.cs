using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderDeliveryLineCollection : NonPersistentBusinessObjectCollection<OrderDeliveryLine>
	{
		public OrderDeliveryLineCollection(JobShipmentPreplanning preAdvice)
			: base(preAdvice.Factory)
		{
			this.PreAdvice = preAdvice;
		}

		public readonly JobShipmentPreplanning PreAdvice;

		#region Loading

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrderDeliveryLine(PreAdvice);
		}

		public override void Load()
		{
			ZQuery query = new ZQuery();

			List<ZGuid> orderPKs = new List<ZGuid>();
			foreach (ZBoolDescriptionPair pair in PreAdvice.OrderNumberList)
			{
				if (pair.Value)
				{
					orderPKs.Add(pair.PK);
				}
			}

			if (orderPKs.Count > 0)
			{
				query.AddToFilter(JobOrderLineSchema.JO_JD, orderPKs.ToArray());
				query.AddToFilter(JobOrderLineSchema.JO_LineStatus, SQLComparisonOperator.NotEqual, Core.Constants.OrderStatus.Cancelled);
				OrderLine[] lines = Factory.Load<OrderLine>(query);

				foreach (OrderLine line in lines)
				{
					OrderDeliveryLine deliveryLine = new OrderDeliveryLine(PreAdvice);
					deliveryLine.OrderNumber = line.Order.JD_OrderNumber;
					deliveryLine.OrderNumberSplit = line.Order.JD_OrderNumberSplit;
					deliveryLine.OrderLineNumber = line.JO_LineNo.ToString();

					Add(deliveryLine);
				}
			}
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (Count > 0)
			{
				OrderDeliveryLine lastLine = this[Count - 1];
				OrderDeliveryLine newLine = (OrderDeliveryLine)child;

				newLine.OrderNumber = lastLine.OrderNumber;

				int orderLineNumber;
				if (!int.TryParse(lastLine.OrderLineNumber, out orderLineNumber))
				{
					orderLineNumber = 0;
				}
				newLine.OrderLineNumber = (orderLineNumber + 1).ToString(CultureInfo.InvariantCulture);

				if (lastLine.Line != null && newLine.Line != null)
				{
					if (newLine.Line.JO_ContainerNumber.IsEmpty)
					{
						newLine.Line.JO_ContainerNumber = lastLine.Line.JO_ContainerNumber;
					}

					if (lastLine.Line.JO_ContainerPackingOrder > 0)
					{
						newLine.Line.JO_ContainerPackingOrder = lastLine.Line.JO_ContainerPackingOrder + 1;
					}

					if (lastLine.Line.JO_CommercialInvoiceNo != lastLine.Order.JD_InvoiceNumber)
					{
						newLine.Line.JO_CommercialInvoiceNo = lastLine.Line.JO_CommercialInvoiceNo;
					}

					if (lastLine.Line.JO_RN_NKCountryOfOrigin != lastLine.Order.JD_RN_NKCountryOfSupply)
					{
						newLine.Line.JO_RN_NKCountryOfOrigin = lastLine.Line.JO_RN_NKCountryOfOrigin;
					}
				}
			}
		}

		#endregion

		#region Sort

		public void SortByOrderNumberAndLineNumber()
		{
			PropertyDescriptor orderNumberPropertyDesc = ((PropertyDescriptorCollection)ZCustomTypeDescriptor.GetProperties(typeof(OrderDeliveryLine)))["OrderNumber"];
			PropertyDescriptor orderLineNumberPropertyDesc = ((PropertyDescriptorCollection)ZCustomTypeDescriptor.GetProperties(typeof(OrderDeliveryLine)))["OrderLineNumber"];
			ListSortDescription sort1 = new ListSortDescription(orderNumberPropertyDesc, ListSortDirection.Ascending);
			ListSortDescription sort2 = new ListSortDescription(orderLineNumberPropertyDesc, ListSortDirection.Ascending);
			ListSortDescription[] sorts = new ListSortDescription[] { sort2, sort1 };

			ListSortDescriptionCollection sortsColl = new ListSortDescriptionCollection(sorts);
			((IBindingListView)this).ApplySort(sortsColl);
		}

		#endregion
	}
}
