using System;
using System.Threading;
using CargoWise.Data;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Warehouse.Transactions.ServiceTasks.VASOrderCreateInitialTransferServiceTask.Code,
	Enterprise.Warehouse.Transactions.ServiceTasks.VASOrderCreateInitialTransferServiceTask.Description,
	Enterprise.Warehouse.Transactions.ServiceTasks.VASOrderCreateInitialTransferServiceTask.Category,
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.VASOrderCreateInitialTransferServiceTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour"
	)]

[assembly: HostedServiceQueueProvider(
	Enterprise.Warehouse.Transactions.ServiceTasks.VASOrderCreateInitialTransferServiceTask.Code,
	Enterprise.Warehouse.Transactions.ServiceTasks.VASOrderCreateInitialTransferServiceTask.Description,
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.VASOrderCreateInitialTransferServiceTask.QueueProvider))]

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
	public class VASOrderCreateInitialTransferServiceTask : ServiceProviderImpl
	{
		public const string Code = "CVT";
		public const string Description = "Create Transfers for VAS Orders with no initial transfer";
		public const string Category = "WHS";

		#region QueueProvider

		public class QueueProvider : IHostedServiceQueueProvider
		{
			QueueResult IHostedServiceQueueProvider.QueueResult
			{
				get
				{
					var result = QueueResult.Error;
					Db.Connection.ExecuteReader(QueueSizeQuery, reader => result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1))));
					return result;
				}
			}

			#region QueueSizeQuery

			const string QueueSizeQuery = @"
select
	count(*), isnull(max(datediff(second, WVO_SystemLastEditTimeUtc, getutcdate())), 0)
from
	dbo.WhsVASOrder
where
	WVO_CancelledTimeUtc is null and
	WVO_WD_TransferIntoServiceArea is null";

			#endregion
		}

		#endregion

		#region RunTask

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			var processingManager = new VASOrderCreateInitialTransferManager(ServiceLogger);
			processingManager.CreateTransfersForVASOrdersWithNoInitialTransfer();
		}

		#endregion
	}
}
