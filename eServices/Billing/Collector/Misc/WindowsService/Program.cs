using System.ServiceProcess;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using Microsoft.Extensions.Logging;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService
{
	static class Program
	{
		static void Main(string[] args)
		{
			ServiceBase.Run(new BillingService(typeof (Program).Assembly, "plugins_settings.xml", "plugins_state.xml", new LoggerFactory().AddLog4Net()));
		}
	}
} 
