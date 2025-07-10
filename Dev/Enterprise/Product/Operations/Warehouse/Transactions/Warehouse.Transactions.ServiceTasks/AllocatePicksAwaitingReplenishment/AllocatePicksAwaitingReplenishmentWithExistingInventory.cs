using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Warehouse.Transactions.ServiceTasks.AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask.Code,
	Enterprise.Warehouse.Transactions.ServiceTasks.AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask.Description,
	Enterprise.Warehouse.Transactions.ServiceTasks.AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask.Category,
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour"
	)]

[assembly: HostedServiceQueueProvider(
	Enterprise.Warehouse.Transactions.ServiceTasks.AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask.Code,
	Enterprise.Warehouse.Transactions.ServiceTasks.AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask.Description,
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask.QueueProvider))]
[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Warehouse.Transactions.ServiceTasks.AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask.Code,
	WhsDocketSchema.Constants.TableName,
	new[] { WhsDocketSchema.Constants.WD_IsPickFaceReplenishment + "=1", WhsDocketSchema.Constants.WD_FinalisedDate + " IS NOT NULL" },
	null)]
namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
	public class AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask : ServiceProviderImpl
	{
		public const string Code = "APW";
		public const string Description = "Allocate Picks Awaiting Replenishment With Existing Inventory";
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
	count(*), isnull(max(datediff(second, WP_SystemLastEditTimeUtc, getutcdate())), 0)
from
	dbo.WhsPick
where
	WP_IsAwaitingReplenishment = 1";

			#endregion
		}

		#endregion

		#region RunTask

		public override void RunTask(CancellationToken token)
		{
			var processingManager = ObjectFactory.Get<IAllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager>();
			processingManager.AllocateAwaitingReplenishmentPicks(ServiceLogger, token);
		}

		#endregion
	}
}
