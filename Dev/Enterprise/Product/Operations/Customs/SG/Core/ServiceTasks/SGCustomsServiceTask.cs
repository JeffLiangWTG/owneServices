using System.Threading;
using Enterprise.Customs.SG.V4.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.SG.V4.Business.SGCustomsTaskRunner.SGCustomsServiceCode,
	Enterprise.Customs.SG.V4.Business.SGCustomsTaskRunner.SGCustomsServiceName,
	"SCM",
	typeof(Enterprise.Customs.SG.V4.ServiceTasks.SGCustomsServiceTask),
	MinimumPeriod = "1minute",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Singapore,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.SG.V4.ServiceTasks
{
	sealed class SGCustomsServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		[HostedServiceRequirements]
		public static string[] CheckServiceTaskEnvironmentIsValid() => SgCustomsServiceTaskInitialisationEnvironmentCheck.Validate();

		protected override void RunTaskCore(CancellationToken token)
		{
			new SGCustomsTaskRunner(ServiceLogger).Run(token);
		}
	}
}
