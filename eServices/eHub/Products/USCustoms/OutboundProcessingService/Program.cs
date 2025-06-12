using System.ServiceProcess;
using Common.Logging;

namespace CargoWise.eServices.USCustoms.OutboundProcessingService
{
	static class Program
	{
		static void Main()
		{
			var logger = LogManager.GetLogger(typeof(Program));
			var service = new OutboundProcessingService(logger);

			var servicesToRun = new ServiceBase[] { service };
			ServiceBase.Run(servicesToRun);
		}
	}
}
