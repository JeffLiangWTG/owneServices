using System.Threading;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"CCV",
	"Cycle Count Variances",
	"WHS",
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.CycleCountVarianceServiceTask),
	MinimumPeriod = "5Minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("CCV",
	WhsCycleCountLocationVarianceSchema.Constants.TableName,
	new[] { WhsCycleCountLocationVarianceSchema.Constants.WCC_AuthorizedAction + "=" + CycleCountVarianceAuthorizedAction.Codes.Approved },
	null)]
[assembly: HostedServiceBusinessObjectBinding("CCV",
	WhsCycleCountLocationVarianceSchema.Constants.TableName,
	new[] { WhsCycleCountLocationVarianceSchema.Constants.WCC_AuthorizedAction + "=" + CycleCountVarianceAuthorizedAction.Codes.Rejected },
	null)]
namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class CycleCountVarianceServiceTask : ServiceProviderImpl
	{
		#region RunTask

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			var processingManager = new CycleCountVarianceProcessingManager(ServiceLogger);
			processingManager.ProcessCycleCountVariances();
		}

		#endregion
	}
}
