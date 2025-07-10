using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	class OrderLineDeliveryFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrderLineDeliveryFetchStrategy(OrderLineDelivery delivery)
			: base(delivery)
		{
		}

		OrderLineDelivery Delivery => BusinessObject as OrderLineDelivery;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			var orderLineRequired = false;
			var orderLineDeliverContainerRequired = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(OrderLineDelivery.J4_Calc_TotalQuantityAllocated):
						orderLineDeliverContainerRequired = true;
						break;

					case nameof(OrderLineDelivery.J4_DeliverPointAddress1):
					case nameof(OrderLineDelivery.J4_DeliverPointAddress2):
						orderLineRequired = true;
						break;
				}
			}

			if (orderLineRequired)
			{
				Factory.AddFetchHint(typeof(OrderLine), Delivery.J4_JO);

				var filter = new ZQuery(OrgAddressSchema.OA_Code, SQLComparisonOperator.Equal, Delivery.J4_OA_NKDeliveryPoint);
				Factory.AddFetchHint(typeof(OrgAddress), filter);
			}

			if (orderLineDeliverContainerRequired)
			{
				Factory.AddFetchHint(JobOrderLineDeliverContainerSchema.J5_J4, Delivery.PK);
			}

			base.FetchForViewCore(columns);
		}
	}
}
