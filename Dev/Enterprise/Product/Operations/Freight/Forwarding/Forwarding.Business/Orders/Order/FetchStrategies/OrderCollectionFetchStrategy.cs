using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class OrderCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public OrderCollectionFetchStrategy(GenericOrderCollection collection) : base(collection)
		{
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			var addBuyerHints = false;
			var addSupplierHints = false;
			var addOrderLineHints = false;
			var addMilestoneHints = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(IAttachedOrder.BuyerOrgCode):
						addBuyerHints = true;
						break;

					case nameof(IAttachedOrder.SupplierOrgCode):
						addSupplierHints = true;
						break;

					case nameof(IAttachedOrder.ETA):
					case nameof(IAttachedOrder.ETD):
						addMilestoneHints = true;
						break;

					case nameof(IAttachedOrder.QuantityReceived):
					case nameof(IAttachedOrder.QuantityInvoiced):
					case nameof(IAttachedOrder.QuantityOrdered):
					case nameof(IAttachedOrder.QuantityRemaining):
					case nameof(IAttachedOrder.TotalWeight):
					case nameof(IAttachedOrder.TotalVolume):
					case nameof(IAttachedOrder.TotalPacks):
						addOrderLineHints = true;
						break;

					default:
						break;
				}
			}

			var orders = businessObjects.OfType<Order>().ToList();

			foreach (var order in orders)
			{
				if (addBuyerHints)
				{
					order.Factory.AddFetchHint(OrgAddressSchema.PK, order.JD_OA_BuyerAddress);
				}

				if (addSupplierHints)
				{
					order.Factory.AddFetchHint(OrgAddressSchema.PK, order.JD_OA_SupplierAddress);
				}

				if (addOrderLineHints)
				{
					order.Factory.AddFetchHint(JobOrderLineSchema.JO_JD, order.PK);
				}

				if (addMilestoneHints)
				{
					order.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
				}
			}
		}
	}
}
