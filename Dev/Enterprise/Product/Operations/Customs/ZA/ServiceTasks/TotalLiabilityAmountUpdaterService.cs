using System.Threading;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.ZA.ServiceTasks.TotalLiabilityAmountUpdaterService.Code,
	Enterprise.Customs.ZA.ServiceTasks.TotalLiabilityAmountUpdaterService.FriendlyName,
	Enterprise.Customs.ZA.ServiceTasks.TotalLiabilityAmountUpdaterService.Category,
	typeof(Enterprise.Customs.ZA.ServiceTasks.TotalLiabilityAmountUpdaterService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1month",
	DefaultScheduleDayOfMonth = 23
)]

namespace Enterprise.Customs.ZA.ServiceTasks
{
	public class TotalLiabilityAmountUpdaterService : ServiceProviderImpl
	{
		public const string Code = "ZLU";
		public const string Category = "WHS";
		public const string FriendlyName = "ZA Total Liability Amount Updater";

		public override void RunTask(CancellationToken token)
		{
			new TotalLiabilityAmountUpdater(ServiceLogger).Process(token);
		}
	}
}
