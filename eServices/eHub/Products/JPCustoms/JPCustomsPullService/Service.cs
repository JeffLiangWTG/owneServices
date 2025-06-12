using System.ServiceProcess;
using CargoWise.eHub.Products.JPCustoms.Common;
using CargoWise.eHub.Products.JPCustoms.PullService.Common;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.PullService
{
	public partial class Service : ServiceBase
	{
		JPCustomsPullRunner runner;

		public Service(ILog logger, IPullRunnerConfiguration pullRunnerConfiguration, IJPCustomsPullManager manager)
		{
			InitializeComponent();
			runner = new JPCustomsPullRunner(logger, pullRunnerConfiguration, manager);
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
