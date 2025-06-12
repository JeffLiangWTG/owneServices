using System.ServiceProcess;
using Common.Logging;

namespace CargoWise.eServices.Authentication.WindowsService
{
	public partial class AuthenticationService : ServiceBase
	{
		public AuthenticationService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			Logger.Info("Authentication Windows Service started...");

			AuthenticationSettings.Instance.LoadApplicationSettings(new FileSystemWatcherWrapper());
			copyFromediProdTaks = new AuthenticationServiceTask(AuthenticationSettings.Instance);
			copyFromediProdTaks.Start();
		}

		protected override void OnStop()
		{
			Logger.Info("Authentication Windows Service stopping...");

			if (copyFromediProdTaks != null)
			{
				copyFromediProdTaks.Dispose();
			}
			Logger.Info("Authentication Windows Service stopped...");
		}

		AuthenticationServiceTask copyFromediProdTaks;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthenticationService));
	}
}
