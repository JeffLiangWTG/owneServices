using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLineDeliverContainerFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrderLineDeliverContainerFetchStrategy(OrderLineDeliverContainer container)
			: base(container)
		{
		}

		OrderLineDeliverContainer Container => BusinessObject as OrderLineDeliverContainer;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			var orderLineDeliveryRequired = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(OrderLineDeliverContainer.J5_J4_RL_NKDestinationPort):
					case nameof(OrderLineDeliverContainer.J5_JO_Partno):
					case nameof(OrderLineDeliverContainer.J5_JD_OrderNumber):
						orderLineDeliveryRequired = true;
						break;
				}
			}

			if (orderLineDeliveryRequired)
			{
				Factory.AddFetchHint(typeof(OrderLineDelivery), Container.J5_J4);
			}

			base.FetchForViewCore(columns);
		}
	}
}
