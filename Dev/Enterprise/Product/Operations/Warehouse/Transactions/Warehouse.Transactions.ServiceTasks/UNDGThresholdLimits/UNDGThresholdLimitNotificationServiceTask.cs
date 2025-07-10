using System.Threading;
using CargoWise.Application;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Warehouse.Transactions.ServiceTasks.UNDGThresholdLimitNotificationServiceTask.Code,
	Enterprise.Warehouse.Transactions.ServiceTasks.UNDGThresholdLimitNotificationServiceTask.Description,
	Enterprise.Warehouse.Transactions.ServiceTasks.UNDGThresholdLimitNotificationServiceTask.Category,
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.UNDGThresholdLimitNotificationServiceTask),
	MinimumPeriod = "1hour",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day"
)]
namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
	public class UNDGThresholdLimitNotificationServiceTask : ServiceProviderImpl
	{
		public const string Code = "DGT";
		public const string Description = "Notify Warehouse Managers of Inventory Exceeding DG Limits";
		public const string Category = "WHS";

		public override void RunTask(CancellationToken token)
		{
			var warehouses = ObjectFactory.Get<IUNDGThresholdLimitWarehouseFinder>().LoadWarehousesWithDGLimits();
			if (warehouses.Count > 0)
			{
				var processingManager = ObjectFactory.Get<IUNDGThresholdLimitNotificationProcessor>();
				foreach (var warehouse in warehouses)
				{
					token.ThrowIfCancellationRequested();
					processingManager.NotifyWarehouseManagersOfExceededUNDGLimits(token, warehouse, ServiceLogger);
				}
			}
			else
			{
				ServiceLogger.Information(Res.GetString("792163e0-faec-4c1f-ad4f-138236b1e9c8", "There are no warehouses using UNDG Limit functionality."));
			}
		}
	}
}
