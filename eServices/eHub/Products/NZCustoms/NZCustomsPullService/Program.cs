using System.ServiceProcess;
using Common.Logging;
using CargoWise.eHub.Products.NZCustoms.Client;

namespace CargoWise.eHub.Products.NZCustoms.PullService
{
	static class Program
	{
		static void Main()
		{
			IConfigurationProvider configurationProvider = new ConfigurationProvider();
			ILog logger = LogManager.GetLogger(typeof(Program));
			IFileManager fileManager = new FileManager(configurationProvider);
			IServiceApi serviceAPI = new ServiceApi(logger);
			INZCustomsPullManager manager = new NZCustomsPullManager(logger, configurationProvider, serviceAPI, fileManager);

			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
			{ 
				new Service(configurationProvider, manager, logger) 
			};

			logger.Info("Run NZCustomsPull as Windows Service.");
			ServiceBase.Run(ServicesToRun);
		}
	}
}
