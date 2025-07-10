using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	class CompleteAndFinaliseVASOrderProcessor : IProcessor
	{
		public CompleteAndFinaliseVASOrderProcessor(WhsVASOrder vasOrder)
		{
			VASOrder = Argument.NotNull(vasOrder, "vasOrder");
		}

		readonly WhsVASOrder VASOrder;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			var successfullyCompletedVASOrder = VASOrder.WVO_WorkCompletedTimeUtc.IsValid || MarkVASOrderAsCompleted(notifications);
			if (successfullyCompletedVASOrder)
			{
				var finalisedSuccessfully = VASOrder.FinaliseVASOrder(notifications, confirmFinalise: false);
				if (finalisedSuccessfully)
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(VASOrder.Factory.Save, null);
				}
			}
		}

		bool MarkVASOrderAsCompleted(INotifications notifications)
		{
			var result = false;

			if (VASOrder.MarkVASOrderAsCompleted(notifications))
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(VASOrder.Factory.Save, null);
				result = !VASOrder.HasChanges;
			}

			return result;
		}
	}
}
