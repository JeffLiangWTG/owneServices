using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class PackProduct : AutoJobPackProduct
	{
		public PackProduct(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region D2_JL

		[RelatedBusinessObject("PackLine")]
		public override ZGuid D2_JL
		{
			get { return base.D2_JL; }
			set { base.D2_JL = value; }
		}

		#endregion

		#region D2_JO

		[RelatedBusinessObject("OrderLine")]
		[List("Lookups.OrderLines_List")]
		public override ZGuid D2_JO
		{
			get { return base.D2_JO; }
			set
			{
				base.D2_JO = value;
				if (OrderLine != null)
				{
					D2_ProductCode = OrderLine.JO_Partno;
					D2_ProductQuantity = OrderLine.JO_Quantity;
					D2_ProductUnitOfQty = OrderLine.JO_F3_NKPackType;
				}
			}
		}

		#endregion

		#region D2_ProductCode

		[List("Lookups.OrgSupplierPartProductCodes")]
		public override ZString D2_ProductCode
		{
			get { return base.D2_ProductCode; }
			set
			{
				base.D2_ProductCode = value;

				if (Product != null && Product.UNDGs.Count > 0 && PackLine != null)
				{
					foreach (UNDGDataItem undg in Product.UNDGs)
					{
						if (undg.Substance != null)
						{
							var dgItems = PackLine.UNDGs.Where(x => x.DI_DG == undg.DI_DG).ToList();
							if (dgItems.Count <= 1)
							{
								var dgItem = dgItems.Count == 1 ? dgItems[0] : PackLine.UNDGs.AddNew();
								dgItem.DI_DG = undg.DI_DG;
								dgItem.DI_DGFlashPoint = undg.DI_DGFlashPoint;
								dgItem.DI_OC_DGContact = undg.DI_OC_DGContact;
								dgItem.DI_TechnicalName = undg.DI_TechnicalName;
								dgItem.DI_MPMarinePollutant = undg.DI_MPMarinePollutant;
							}
						}
					}
				}
			}
		}

		#endregion

		#region D2_ProductUnitOfQty

		[List("Lookups.UnitOfQuantity")]
		public override ZString D2_ProductUnitOfQty
		{
			get { return base.D2_ProductUnitOfQty; }
			set { base.D2_ProductUnitOfQty = value; }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public OrgSupplierPart Product
		{
			get
			{
				OrgSupplierPart result = null;

				if (PackLine != null)
				{
					result = new OrgSupplierPart.Loader(Factory, false).LoadAndReturnMatchingCount(D2_ProductCode,
						PackLine.CalcConsignee,
						PackLine.CalcConsignor,
						false, false, false,
						PackLine.Shipment?.IsExport() ?? false).BestMatchingProduct;
				}

				Order relatedOrder = null;
				if (result == null && OrderLine != null && (relatedOrder = OrderLine.Order) != null)
				{
					result = new OrgSupplierPart.Loader(Factory, false).LoadAndReturnMatchingCount(D2_ProductCode,
						relatedOrder.Buyer,
						relatedOrder.Supplier,
						false, false, false,
						relatedOrder.IsExport()).BestMatchingProduct;
				}

				return result;
			}
		}

		public ForwardingPackLine PackLine
		{
			get { return Factory.Load<ForwardingPackLine>(D2_JL); }
		}

		public OrderLine OrderLine
		{
			get { return Factory.Load<OrderLine>(D2_JO); }
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion
	}
}
