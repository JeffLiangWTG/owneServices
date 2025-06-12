using System;
using System.ServiceProcess;
using CargoWise.eHub.Shared.ServiceTaskHost.Core;

namespace CargoWise.eHub.Shared.ServiceTaskHost
{
	public partial class ServiceTaskHost : ServiceBase
	{
		readonly IServiceTaskRunner runner;

		public ServiceTaskHost(IServiceTaskRunner runner)
		{
			InitializeComponent();

			if (runner == null) throw new ArgumentNullException("runner");
			this.runner = runner;

			this.ServiceName = runner.ServiceTaskName;
		}

		protected override void OnStart(string[] args)
		{
			runner.Start();
		}

		protected override void OnStop()
		{
			runner.Stop();
		}
	}
}