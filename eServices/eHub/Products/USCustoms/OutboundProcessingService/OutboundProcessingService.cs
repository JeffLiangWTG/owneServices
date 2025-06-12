using System.ServiceProcess;
using System.Configuration;
using Common.Logging;

namespace CargoWise.eServices.USCustoms.OutboundProcessingService
{
	public partial class OutboundProcessingService : ServiceBase
	{
		public OutboundProcessingService(ILog logger)
		{
			InitializeComponent();
			this.ServiceName = ConfigurationManager.AppSettings.Get("ServiceName");
			runner = new Runner(logger);
		}

		readonly Runner runner;

		protected override void OnStart(string[] args)
		{
			Start();
		}

		void Start()
		{
			runner.StartThread();
		}

		protected override void OnStop()
		{
			runner.StopThread();
		}

		protected override void OnShutdown()
		{
			runner.StopThread();
			base.OnShutdown();
		}
	}
}
