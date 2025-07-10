using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	class CreateReturnVASOrderTransferProcessor : IProcessor
	{
		public CreateReturnVASOrderTransferProcessor(WhsVASOrder vasOrder)
		{
			VASOrder = Argument.NotNull(vasOrder, "vasOrder");
		}

		readonly WhsVASOrder VASOrder;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			 VASOrder.GetOrCreateReturnTransfer(notifications);
		}
	}
}