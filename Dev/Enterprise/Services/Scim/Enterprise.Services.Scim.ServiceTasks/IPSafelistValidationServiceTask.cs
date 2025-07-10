using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(IPSafelistValidationServiceTask.Code, "Update SCIM IP Safelist Task", IPSafelistValidationServiceTask.Code,
   typeof(IPSafelistValidationServiceTask),
   MinimumPeriod = "6day",
   MaximumPeriod = "8day",
   DefaultScheduleRunEvery = "7day",
   CanRunInAnyBranch = true,
   ActiveByDefault = true
)]

namespace Enterprise.Services.Scim.Business
{
	public class IPSafelistValidationServiceTask : ServiceProviderImpl
	{
		readonly IIPSafelistDBStore DbStore;

		public const string Code = "SCI";

		public IPSafelistValidationServiceTask()
			: this(new IPSafelistDBStore(new IPSafelistHelper()))
		{
		}

		public IPSafelistValidationServiceTask(IIPSafelistDBStore dbStore)
		{
			DbStore = dbStore;
		}

		public override void RunTask(CancellationToken token)
		{
			try
			{
				if (SystemDataRegistry.Instance.EnableScimService.Value)
				{
					DbStore.UpdateSafelistToDB();
				}
				else
				{
					DisableServiceTask();
					ServiceLogger.Log(LogType.Debug, "SCIM service is not enabled");
				}
			}
			catch (ArgumentNullException ex)
			{
				ErrorReporter.ReportOnce("URL request didn't return any IP addresses", ex);
			}
			catch (InvalidOperationException ex)
			{
				ErrorReporter.ReportOnce("IP addresses are in a wrong format", ex);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error getting IP addresses from the safelist URL", ex);
			}
		}

		void DisableServiceTask()
		{
			ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskIsActive(Code, isActive: false);
		}
	}
}
