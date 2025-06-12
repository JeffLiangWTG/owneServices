using System.ServiceProcess;
using Common.Logging;

namespace CargoWise.eHub.Products.NZCustoms.PullService
{
	public partial class Service : ServiceBase
	{
		NZCustomsPullRunner runner;

		public Service(IConfigurationProvider configurationProvider, INZCustomsPullManager manager, ILog logger)
		{
			InitializeComponent();
			runner = new NZCustomsPullRunner(configurationProvider, manager, logger);
		}

		protected override void OnStart(string[] args)
		{
			runner.Start();
		}

		protected override void OnStop()
		{
			runner.Stop();
		}

		protected override void OnShutdown()
		{
			if (runner != null)
			{
				runner.Dispose();
				runner = null;
			}
		}
	}
}
