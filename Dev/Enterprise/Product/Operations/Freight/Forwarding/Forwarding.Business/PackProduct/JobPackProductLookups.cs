using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class JobPackProductLookups : AutoJobPackProductLookups
	{
		public JobPackProductLookups(AutoJobPackProduct parent) : base(parent)
		{
		}

		#region Product Code

		public OrgSupplierPartCollection OrgSupplierPartProductCodes
		{
			get { return new OrgSupplierPartCollection(Factory); }
		}

		#endregion

		#region UnitOfQuantity

		public CodeDescriptionPairList UnitOfQuantity
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion

		#region OrderLines_List

		public OrderLineCollection OrderLines_List
		{
			get
			{
				OrderLineCollection result = new OrderLineCollection(Factory, new AdhocCollectionRelationship(typeof(OrderLine)));

				ForwardingPackLine packline = ((PackProduct)Parent).PackLine;
				if (packline != null && packline.Shipment != null)
				{
					foreach (Order order in packline.Shipment.AttachedOrders)
					{
						if (order.OrderLines.Count > 0)
						{
							result.AddRange(order.OrderLines);
						}
					}
				}

				return result;
			}
		}

		#endregion
	}
}
