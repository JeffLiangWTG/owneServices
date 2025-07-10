using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLinesTotalByProduct : AutoOrderLinesTotalByProduct
	{
		public OrderLinesTotalByProduct(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		internal List<OrderLine> OrderLines
		{
			get { return this.orderLines ?? (this.orderLines = new List<OrderLine>()); }
			set { this.orderLines = value; }
		}
		List<OrderLine> orderLines;

		protected override ZString GetProductDescription()
		{
			ZString result = ZString.Empty;

			if (OrderLines.Count > 0)
			{
				OrgSupplierPart product = OrderLines[0].Product;

				if (product != null)
				{
					result = product.OP_Desc;
				}
				else
				{
					result = OrderLines[0].JO_Description;
				}
			}

			return result;
		}

		public ZInt InnerPacks
		{
			get
			{
				if (!this.innerPacks.HasValue)
				{
					this.innerPacks = (ZInt)OrderLines.Sum(x => x.JO_InnerPacks);
				}

				return this.innerPacks.Value;
			}
		}
		ZInt? innerPacks;

		public ZInt OuterPacks
		{
			get
			{
				if (!this.outerPacks.HasValue)
				{
					this.outerPacks = (ZInt)OrderLines.Sum(x => x.JO_OuterPacks);
				}

				return this.outerPacks.Value;
			}
		}
		ZInt? outerPacks;

		public override ZDecimal Quantity
		{
			get
			{
				if (!this.quantity.HasValue)
				{
					this.quantity = OrderLines.Sum(x => x.JO_Quantity);
				}

				return this.quantity.Value;
			}
		}
		ZDecimal? quantity;

		public override ZDecimal QuantityInvoiced
		{
			get
			{
				if (!this.quantityInvoiced.HasValue)
				{
					this.quantityInvoiced = OrderLines.Sum(x => x.JO_QtyInvoiced);
				}

				return this.quantityInvoiced.Value;
			}
		}
		ZDecimal? quantityInvoiced;

		public override ZDecimal QuantityReceived
		{
			get
			{
				if (!this.quantityReceived.HasValue)
				{
					this.quantityReceived = OrderLines.Sum(x => x.JO_QtyReceived);
				}

				return this.quantityReceived.Value;
			}
		}
		ZDecimal? quantityReceived;

		public override ZDecimal QuantityRemaining
		{
			get
			{
				if (!quantityRemaining.HasValue)
				{
					this.quantityRemaining = OrderLines.Sum(x => x.JO_QuantityRemaining);
				}

				return this.quantityRemaining.Value;
			}
		}
		ZDecimal? quantityRemaining;
	}
}
