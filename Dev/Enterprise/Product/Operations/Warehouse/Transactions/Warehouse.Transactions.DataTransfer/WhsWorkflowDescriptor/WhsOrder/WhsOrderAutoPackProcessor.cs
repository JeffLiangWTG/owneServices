using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsOrderAutoPackProcessor : IProcessor
	{
		public WhsOrderAutoPackProcessor(IBusiness order)
		{
			this.order = order as WhsOrder;
		}
		readonly WhsOrder order;

		void IProcessor.Process(INotifications notifications, CancellationToken unused)
		{
			if (order != null)
			{
				try
				{
					order.AutoPackAndPrintAllLabelsWithNoFactorySave(notifications, WarehouseDataRegistry.Instance.DisablePrintingLabelsOnAutoPack.Value);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notifications.AddError(ex.Message);
				}
			}
		}
	}
}
