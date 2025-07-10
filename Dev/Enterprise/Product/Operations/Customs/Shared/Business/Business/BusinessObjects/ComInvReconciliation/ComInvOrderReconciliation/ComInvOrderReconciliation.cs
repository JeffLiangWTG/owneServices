using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class ComInvOrderReconciliation : Order
	{
		public ComInvOrderReconciliation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			QuantityType = ComInvReconciliationQuantityType.OrderQuantity;
		}

		[ChildEditable(true)]
		public new ComInvOrderLineReconciliationCollection OrderLines
		{
			get { return (ComInvOrderLineReconciliationCollection)base.OrderLines; }
		}

		public new ComInvOrderReconciliation SplitOrder(CreateOrderType splitType)
		{
			return (ComInvOrderReconciliation)base.SplitOrder(splitType);
		}

		#region BusinessObject Overrides

		protected override OrderLineCollection GetOrderLinesCore()
		{
			return new ComInvOrderLineReconciliationCollection(this);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			ComInvOrderReconciliation result = (ComInvOrderReconciliation)base.CloneInternal(args);
			result.QuantityType = QuantityType;
			return result;
		}

		#endregion

		#region InvoiceNumber

		public ZString InvoiceNumber
		{
			get { return JD_InvoiceNumber.IsEmpty ? FormattedOrderNumber() : JD_InvoiceNumber; }
		}

		ZString FormattedOrderNumber()
		{
			return JD_OrderNumberAndSplit.Replace(" ", "").SubstringSafe(0, JobComInvoiceHeaderSchema.JZ_InvoiceNumber.MaxLength);
		}

		#endregion

		#region InvoiceDate

		public ZDateTime InvoiceDate
		{
			get { return JD_InvoiceDate.IsEmpty ? ZDateTime.Now.Date : JD_InvoiceDate; }
		}

		#endregion

		#region QuantityType

		public ComInvReconciliationQuantityType QuantityType
		{
			get { return fQuantityType; }
			set
			{
				if (fQuantityType != value)
				{
					fQuantityType = value;
					foreach (ComInvOrderLineReconciliation orderLine in OrderLines)
					{
						orderLine.QuantityType = value;
					}
				}
			}
		}

		ComInvReconciliationQuantityType fQuantityType;

		#endregion
	}
}
