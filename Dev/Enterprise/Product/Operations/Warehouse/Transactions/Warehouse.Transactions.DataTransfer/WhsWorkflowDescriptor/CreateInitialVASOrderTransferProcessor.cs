using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	class CreateInitialVASOrderTransferProcessor : IProcessor
	{
		public CreateInitialVASOrderTransferProcessor(WhsVASOrder vasOrder)
		{
			VASOrder = Argument.NotNull(vasOrder, "vasOrder");
		}

		readonly WhsVASOrder VASOrder;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			VASOrder.GetOrCreateInitialTransfer(notifications);
		}
	}
}